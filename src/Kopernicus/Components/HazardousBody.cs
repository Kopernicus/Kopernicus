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

        private CelestialBody _body;
        
        // The current position of the timer.
        private Single _heatPosition;

        /// <summary>
        /// Get the body
        /// </summary>
        private void Start()
        {
            _body = GetComponent<CelestialBody>();
        }

        /// <summary>
        /// Update the heat
        /// </summary>
        /// <returns></returns>
        private void Update()
        {
            // Check for an active vessel
            if (!FlightGlobals.ActiveVessel || FlightGlobals.ActiveVessel.packed)
            {
                return;
            }

            // Check if the vessel is near the body
            if (FlightGlobals.currentMainBody != _body)
            {
                return;
            }

            // Simple counter
            if (_heatPosition > 0f)
            {
                _heatPosition -= Time.deltaTime;
                return;
            }

            // We got past the counter - update time
            Double altitude =
                altitudeCurve.Evaluate((Single) Vector3d.Distance(FlightGlobals.ActiveVessel.transform.position,
                    _body.transform.position));
            Double latitude = latitudeCurve.Evaluate((Single) FlightGlobals.ActiveVessel.latitude);
            Double longitude = longitudeCurve.Evaluate((Single) FlightGlobals.ActiveVessel.longitude);

            Double heat = altitude * latitude * longitude * heatRate;
            if (heatMap)
            {
                const Single FULL_CIRCLE = 1f / 360f;
                const Single HALF_CIRCLE = 1f / 180f;

                heat *= heatMap.GetPixelFloat((longitude + 180) * FULL_CIRCLE, (latitude + 90) * HALF_CIRCLE);
            }

            foreach (Part part in FlightGlobals.ActiveVessel.Parts)
            {
                part.temperature += heat;
            }

            // Reset the timer
            _heatPosition = heatInterval;
        }
    }
}
