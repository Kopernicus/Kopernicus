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
        /// The average heat rate on the surface of the body. Gets multiplied by latitude, longitude and altitude curves
        /// </summary>
        public Double heatRate;

        /// <summary>
        /// How many seconds should pass between applying the heat rate to the ship.
        /// </summary>
        public Single heatInterval = 0.05f;

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
            DontDestroyOnLoad(this);
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
        Single _heatPosition;

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

            if (_body != FlightGlobals.currentMainBody)
            {
                _body = FlightGlobals.currentMainBody;
                hazardousBody = _body.GetComponent<HazardousBody>();
            }

            if (!hazardousBody)
            {
                return;
            }

            // Decrement timer. If it becomes zero or less, it's time to apply heating.
            _heatPosition -= TimeWarp.fixedDeltaTime;
            if (_heatPosition > 0f)
            {
                return;
            }

            // We got past the counter - update time
            Double altitude =
                hazardousBody.altitudeCurve.Evaluate((Single)Vector3d.Distance(vessel.transform.position,
                    _body.transform.position));
            Double latitude = hazardousBody.latitudeCurve.Evaluate((Single)vessel.latitude);
            Double longitude = hazardousBody.longitudeCurve.Evaluate((Single)vessel.longitude);

            Double heat = altitude * latitude * longitude * hazardousBody.heatRate;
            if (hazardousBody.heatMap)
            {
                const Single FULL_CIRCLE = 1f / 360f;
                const Single HALF_CIRCLE = 1f / 180f;

                heat *= hazardousBody.heatMap.GetPixelFloat((longitude + 180) * FULL_CIRCLE, (latitude + 90) * HALF_CIRCLE);
            }

            // How many rounds of heating need to be applied before the timer is positive again (heating "debt" is paid
            // off).
            UInt32 heatingRounds = (UInt32)Math.Floor(-_heatPosition / hazardousBody.heatInterval + 1);

            foreach (Part part in vessel.Parts)
            {
                part.temperature += heat * heatingRounds;
            }

            // Increment timer so that it becomes positive.
            _heatPosition += hazardousBody.heatInterval * heatingRounds;
        }
    }
}
