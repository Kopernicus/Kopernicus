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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Kopernicus.Components.ModularComponentSystem;
using ModularFI;
using UnityEngine;

namespace Kopernicus.Components.ModularScatter
{
    /// <summary>
    /// A Scatter Component that emits heat onto the active vessel
    /// </summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class HeatEmitterComponent : IComponent<ModularScatter>
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
        /// Multiplier curve to change ambientTemp with distance
        /// </summary>
        public FloatCurve distanceCurve;

        /// <summary>
        /// Multiplier map for ambientTemp
        /// </summary>
        public MapSO heatMap;

        /// <summary>
        /// Gets executed every frame and checks if a Kerbal is within the range of the scatter object
        /// </summary>
        void IComponent<ModularScatter>.Update(ModularScatter system)
        {
            // We don't use this
        }

        void IComponent<ModularScatter>.Apply(ModularScatter system)
        {
            // We don't use this
        }

        void IComponent<ModularScatter>.PostApply(ModularScatter system)
        {
            Events.OnCalculateBackgroundRadiationTemperature.Add((mfi) => OnCalculateBackgroundRadiationTemperature(mfi, system));
        }

        void OnCalculateBackgroundRadiationTemperature(ModularFlightIntegrator flightIntegrator, ModularScatter system)
        {
            List<GameObject> scatters = system?.scatterObjects;

            if (system?.scatterObjects != null)
            {
                Vessel vessel = flightIntegrator?.Vessel;
                CelestialBody _body = system.body;

                if (_body != null && _body == vessel?.mainBody)
                {
                    for (Int32 i = scatters.Count; i > 0; i--)
                    {
                        GameObject scatter = scatters[i - 1];

                        if (scatter?.activeSelf != true)
                            continue;

                        if (!string.IsNullOrEmpty(biomeName))
                        {
                            String biome = ScienceUtil.GetExperimentBiome(_body, vessel.latitude, vessel.longitude);

                            if (biomeName != biome)
                                continue;
                        }

                        Double altitude = altitudeCurve?.Evaluate((Single)Vector3d.Distance(vessel.transform.position, _body.transform.position)) ?? 1;
                        Double latitude = latitudeCurve?.Evaluate((Single)vessel.latitude) ?? 1;
                        Double longitude = longitudeCurve?.Evaluate((Single)vessel.longitude) ?? 1;
                        Double distance = distanceCurve?.Evaluate((Single)Vector3d.Distance(vessel.transform.position, scatter.transform.position)) ?? 1;

                        Double newTemp = altitude * latitude * longitude * ambientTemp;

                        if (heatMap)
                        {
                            Double x = ((450 - vessel.longitude) % 360) / 360.0;
                            Double y = (vessel.latitude + 90) / 180.0;
                            Double m = heatMap.GetPixelFloat(x, y);
                            newTemp *= m;
                        }

                        KopernicusHeatManager.NewTemp(newTemp, sumTemp);
                    }
                }
            }
        }
    }
}
