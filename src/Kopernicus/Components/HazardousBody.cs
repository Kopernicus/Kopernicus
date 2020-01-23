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

using ModularFI;
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
        public Double ambientTemp = 0;

        /// <summary>
        /// Multiplier curve to change ambientTemp with latitude
        /// </summary>
        public FloatCurve latitudeCurve;

        /// <summary>
        /// Multiplier curve to change ambientTemp with longitude
        /// </summary>
        public FloatCurve longitudeCurve;

        /// <summary>
        /// Multiplier curve to change ambientTemp with altitude
        /// </summary>
        public FloatCurve altitudeCurve;

        /// <summary>
        /// Multiplier map for ambientTemp
        /// </summary>
        public MapSO heatMap;

        /// <summary>
        /// Override for <see cref="FlightIntegrator.CalculateBackgroundRadiationTemperature"/>
        /// </summary>
        internal static double RadiationTemperature(ModularFlightIntegrator flightIntegrator, double baseTemp)
        {
            Vessel vessel = flightIntegrator?.Vessel;

            if (vessel != null)
            {
                CelestialBody _body = vessel?.mainBody;
                HazardousBody hazardousBody = _body?.GetComponent<HazardousBody>();

                if (hazardousBody != null)
                {
                    Double altitude = hazardousBody.altitudeCurve.Evaluate((Single)Vector3d.Distance(vessel.transform.position, _body.transform.position));
                    Double latitude = hazardousBody.latitudeCurve.Evaluate((Single)vessel.latitude);
                    Double longitude = hazardousBody.longitudeCurve.Evaluate((Single)vessel.longitude);

                    Double newTemp = altitude * latitude * longitude * hazardousBody.ambientTemp;

                    if (hazardousBody.heatMap)
                    {
                        const Single FULL_CIRCLE = 1f / 360f;
                        const Single HALF_CIRCLE = 1f / 180f;

                        newTemp *= hazardousBody.heatMap.GetPixelFloat(((465 - longitude) % 360) * FULL_CIRCLE, (latitude + 90) * HALF_CIRCLE);
                    }

                    if (newTemp > baseTemp)
                    {
                        baseTemp = newTemp;
                    }
                }
            }

            return baseTemp;
        }
    }
}
