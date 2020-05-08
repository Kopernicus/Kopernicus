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
using System.Linq;
using Kopernicus.Components.ModularComponentSystem;
using UnityEngine;

namespace Kopernicus.Components.ModularScatter
{
    /// <summary>
    /// A Scatter Component that can add colliders to a scatter object
    /// </summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class ScatterCollidersComponent : IComponent<ModularScatter>
    {
        /// <summary>
        /// How many scatters were active the last time we added colliders.
        /// This is used to avoid checking for the colliders every frame.
        /// </summary>
        private Int32 scatterCount = 0;

        /// <summary>
        /// The mesh that is used for the collider
        /// </summary>
        public Mesh CollisionMesh;

        /// <summary>
        /// Gets executed every frame and checks if a Kerbal is within the range of the scatter object
        /// </summary>
        void IComponent<ModularScatter>.Update(ModularScatter system)
        {
            if (system.scatterObjects.Count == scatterCount)
            {
                return;
            }

            scatterCount = system.scatterObjects.Count;
            for (Int32 i = 0; i < system.scatterObjects.Count; i++)
            {
                GameObject scatter = system.scatterObjects[i];
                MeshCollider collider = scatter.GetComponent<MeshCollider>();
                if (collider)
                {
                    continue;
                }

                MeshFilter filter = scatter.GetComponent<MeshFilter>();
                collider = scatter.AddComponent<MeshCollider>();
                collider.sharedMesh = CollisionMesh ? CollisionMesh : filter.sharedMesh;
                collider.enabled = true;
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
