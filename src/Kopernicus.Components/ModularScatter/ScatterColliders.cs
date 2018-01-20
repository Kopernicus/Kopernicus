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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// A Scatter Component that can add colliders to a scatter object
        /// </summary>
        public class ScatterColliders : IComponent<ModularScatter>
        {
            /// <summary>
            /// Contains a List of colliders for the scatter
            /// </summary>
            public List<MeshCollider> meshColliders = new List<MeshCollider>();

            /// <summary>
            /// Gets executed every frame and checks if a Kerbal is within the range of the scatter object
            /// </summary>
            void IComponent<ModularScatter>.Update(ModularScatter system)
            {
                // If there's nothing to do, discard any old colliders and abort
                if (system.transform.childCount == 0)
                {
                    if (!meshColliders.Any()) return;
                    Debug.LogWarning("[Kopernicus] Discard old colliders");
                    foreach (MeshCollider collider in meshColliders.Where(collider => collider))
                        UnityEngine.Object.Destroy(collider);
                    meshColliders.Clear();
                    return;
                }
                Boolean rebuild = false;
                if (system.transform.childCount > meshColliders.Count)
                {
                    Debug.LogWarning("[Kopernicus] Add " + (system.transform.childCount - meshColliders.Count) + " colliders");
                    rebuild = true;
                }
                else if (system.transform.childCount < meshColliders.Count)
                {
                    Debug.LogWarning("[Kopernicus] Remove " + (meshColliders.Count - system.transform.childCount) +
                                     " colliders");
                    rebuild = true;
                }
                else if (meshColliders[0].sharedMesh != system.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh)
                {
                    Debug.LogWarning("[Kopernicus] Replacing colliders");
                    rebuild = true;
                }
                if (rebuild)
                {
                    foreach (Transform t in system.transform)
                    {
                        MeshCollider collider = t.gameObject.GetComponent<MeshCollider>();
                        if (collider) continue;
                        collider = t.gameObject.AddComponent<MeshCollider>();
                        collider.sharedMesh = t.gameObject.GetComponent<MeshFilter>().sharedMesh;
                        collider.sharedMesh.Optimize();
                        collider.enabled = true;
                        meshColliders.Add(collider);
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
}