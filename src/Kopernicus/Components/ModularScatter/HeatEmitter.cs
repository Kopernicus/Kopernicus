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
using System.Diagnostics.CodeAnalysis;
using Kopernicus.Components.ModularComponentSystem;
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
        /// How much heat should be applied at a certain distance
        /// </summary>
        public FloatCurve HeatCurve;
        
        /// <summary>
        /// The amount of heat that gets added to the active vessel every n seconds
        /// </summary>
        public Double HeatRate;

        /// <summary>
        /// How many seconds should pass between applying the heat rate to the ship.
        /// </summary>
        public Double HeatInterval = 0.05d;

        /// <summary>
        /// Gets executed every frame and checks if a Kerbal is within the range of the scatter object
        /// </summary>
        void IComponent<ModularScatter>.Update(ModularScatter system)
        {
            // Check for an active vessel
            if (!FlightGlobals.ActiveVessel || FlightGlobals.ActiveVessel.packed)
            {
                return;
            }

            foreach (GameObject scatter in system.scatterObjects)
            {
                if (!scatter.activeSelf)
                {
                    continue;
                }
            
                // Get the distance between the active vessel and ourselves
                Single distance = Vector3.Distance(scatter.transform.position, FlightGlobals.ship_position);
                Double heat = HeatRate * HeatCurve.Evaluate(distance) / HeatInterval * Time.deltaTime;
            
                // Apply the heat to all parts of the active vessel
                for (Int32 i = 0; i < FlightGlobals.ActiveVessel.Parts.Count; i++)
                {
                    FlightGlobals.ActiveVessel.Parts[i].temperature += heat;
                }
            }
        }

        void IComponent<ModularScatter>.Apply(ModularScatter system)
        {
            // We don't use this
        }

        void IComponent<ModularScatter>.PostApply(ModularScatter system)
        {
            // We don't use this
        }
    }
}