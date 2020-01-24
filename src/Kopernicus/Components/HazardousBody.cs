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
        /// If the ambientTemp should be added.
        /// </summary>
        public Boolean sumTemp = false;

        /// <summary>
        /// The name of the biome.
        /// </summary>
        public String biomeName;

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
        internal static double RadiationTemperature(ModularFlightIntegrator flightIntegrator, Double baseTemp)
        {
            // Stock Behaviour
            baseTemp = UtilMath.Lerp(baseTemp, PhysicsGlobals.SpaceTemperature, flightIntegrator.DensityThermalLerp);

            // Hazardous Body
            Vessel vessel = flightIntegrator?.Vessel;

            if (vessel != null)
            {
                CelestialBody _body = vessel?.mainBody;
                HazardousBody[] hazardousBodies = _body?.GetComponents<HazardousBody>();

                if (hazardousBodies?.Length > 0)
                {
                    Double addTemp = 0;

                    for (Int32 i = hazardousBodies.Length; i > 0; i--)
                    {
                        HazardousBody hazardousBody = hazardousBodies[i - 1];

                        if (!string.IsNullOrEmpty(hazardousBody.biomeName))
                        {
                            String biomeName = ScienceUtil.GetExperimentBiome(_body, vessel.latitude, vessel.longitude);

                            if (hazardousBody.biomeName != biomeName)
                                continue;
                        }

                        Double altitude = hazardousBody.altitudeCurve.Evaluate((Single)Vector3d.Distance(vessel.transform.position, _body.transform.position));
                        Double latitude = hazardousBody.latitudeCurve.Evaluate((Single)vessel.latitude);
                        Double longitude = hazardousBody.longitudeCurve.Evaluate((Single)vessel.longitude);

                        Double newTemp = altitude * latitude * longitude * hazardousBody.ambientTemp;

                        if (hazardousBody.heatMap)
                        {
                            Double x = ((450 - vessel.longitude) % 360) / 360.0;
                            Double y = (vessel.latitude + 90) / 180.0;
                            Double m = hazardousBody.heatMap.GetPixelFloat(x, y);
                            newTemp *= m;
                        }

                        if (hazardousBody.sumTemp && newTemp > 0)
                        {
                            addTemp += newTemp;
                        }
                        else if (newTemp > baseTemp)
                        {
                            baseTemp = newTemp;
                        }
                    }

                    baseTemp += addTemp;
                }
            }

            return baseTemp;
        }
    }
}
