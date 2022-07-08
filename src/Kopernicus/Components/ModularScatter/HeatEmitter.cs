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

using Kopernicus.Components.ModularComponentSystem;
using ModularFI;
using System;
using System.Diagnostics.CodeAnalysis;
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
        public double temperature = 0;

        /// <summary>
        /// Multiplier curve to change temperature with distance from scatter
        /// </summary>
        public FloatCurve distanceCurve;

        public HeatEmitterComponent()
        {
            distanceCurve = new FloatCurve(new[] { new Keyframe(0, 1) });
        }

        public void OnCalculateBackgroundRadiationTemperature(ModularFlightIntegrator flightIntegrator, PQSMod_KopernicusLandClassScatterQuad quadController)
        {
            Vessel vessel = flightIntegrator.Vessel;
            CelestialBody body = quadController.modularScatter.body;

            if (body.RefEquals(vessel.mainBody))
                return;

            for (int i = 0; i < quadController.scatterPositions.Count; i++)
            {
                float distance = distanceCurve.Evaluate(Vector3.Distance(vessel.transform.position, quadController.scatterPositions[i]));
                KopernicusHeatManager.NewTemp(distance * temperature, false);
            }
        }

        public void Apply(ModularScatter system) => throw new NotImplementedException();

        public void PostApply(ModularScatter system) => throw new NotImplementedException();

        public void Update(ModularScatter system) => throw new NotImplementedException();
    }
}
