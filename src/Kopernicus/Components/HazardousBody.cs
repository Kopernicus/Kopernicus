/**
 * Kopernicus Planetary System Modifier
 * ------------------------------------------------------------- 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 * 
 * This library is intended to be used as a plugin for Kerbal Space Program
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using UnityEngine;

namespace Kopernicus.Components
{
    /// <summary>
    /// Regional heating for planet surfaces
    /// </summary>
    public class HazardousBody : MonoBehaviour
    {
        /// <summary>
        /// The maximum temperature that will eventually be reached.
        /// </summary>
        public Double maxTemp = 0;

        /// <summary>
        /// How many seconds it'll take to get halfway to maxTemp.
        /// </summary>
        public Single lambda = 0;

        /// <summary>
        /// Controls the heat rate at a certain latitude
        /// </summary>
        public FloatCurve latitudeCurve;

        /// <summary>
        /// Controls the heat rate at a certain longitude
        /// </summary>
        public FloatCurve longitudeCurve;

        /// <summary>
        /// Controls the heat rate at a certain altitude
        /// </summary>
        public FloatCurve altitudeCurve;

        /// <summary>
        /// Controls the amount of heat that is applied on each spot of the planet
        /// </summary>
        public MapSO heatMap;
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class HazardousTracker : MonoBehaviour
    {
        void Start()
        {
            GameEvents.onVesselLoaded.Add(OnVesselLoaded);
            GameEvents.onVesselCreate.Add(OnVesselCreate);
            DontDestroyOnLoad(this);
        }

        private void OnVesselCreate(Vessel data)
        {
            data.gameObject.AddOrGetComponent<HazardousVessel>();
        }

        void OnVesselLoaded(Vessel vessel)
        {
            vessel.gameObject.AddOrGetComponent<HazardousVessel>();
        }
    }

    public class HazardousVessel : MonoBehaviour
    {
        Vessel vessel;
        CelestialBody _body;
        HazardousBody hazardousBody;

        void Start()
        {
            vessel = GetComponent<Vessel>();
        }

        /// <summary>
        /// Update the heat. Heating is physics phenomenon so do it in the physics loop.
        /// </summary>
        /// <returns></returns>
        private void FixedUpdate()
        {
            // Check if the vessel is near the body
            if (FlightGlobals.currentMainBody != vessel.mainBody)
            {
                return;
            }

            // Check if the body has changed
            if (_body != FlightGlobals.currentMainBody)
            {
                _body = FlightGlobals.currentMainBody;
                hazardousBody = _body.GetComponent<HazardousBody>();
            }

            // Check if the body is hazardous
            if (!hazardousBody)
            {
                return;
            }

            // We got past the counter - update time
            Double altitude =
                hazardousBody.altitudeCurve.Evaluate((Single)Vector3d.Distance(vessel.transform.position,
                    _body.transform.position));
            Double latitude = hazardousBody.latitudeCurve.Evaluate((Single)vessel.latitude);
            Double longitude = hazardousBody.longitudeCurve.Evaluate((Single)vessel.longitude);

            //Double heat = altitude * latitude * longitude * hazardousBody.heatRate;
            Double maxTemp = altitude * latitude * longitude * hazardousBody.maxTemp;
            if (hazardousBody.heatMap)
            {
                const Single FULL_CIRCLE = 1f / 360f;
                const Single HALF_CIRCLE = 1f / 180f;

                maxTemp *= hazardousBody.heatMap.GetPixelFloat((longitude + 180) * FULL_CIRCLE, (latitude + 90) * HALF_CIRCLE);
            }

            foreach (Part part in vessel.Parts)
            {
                if (part.temperature < maxTemp)
                {
                    part.temperature += ((maxTemp - part.temperature) * 0.69420 / hazardousBody.lambda) * TimeWarp.fixedDeltaTime;

                    if (part.temperature > maxTemp)
                        part.temperature = maxTemp;
                }
            }
        }
    }
}
