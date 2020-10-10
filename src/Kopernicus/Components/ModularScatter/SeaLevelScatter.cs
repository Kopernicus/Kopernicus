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
using Random = UnityEngine.Random;

namespace Kopernicus.Components.ModularScatter
{
    /// <summary>
    /// Moves the scatter objects to the sea level
    /// </summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class SeaLevelScatterComponent : IComponent<ModularScatter>
    {
        /// <summary>
        /// The GameObjects that were already moved
        /// </summary>
        private readonly List<KopernicusSurfaceObject> _moved = new List<KopernicusSurfaceObject>();

        /// <summary>
        /// Scatters will be moved up/down by a random value from this range
        /// </summary>
        public List<Single> AltitudeVariance = new List<Single> { 0f, 0f };

        /// <summary>
        /// Go through all spawned scatters, and move them so they are at sea level, and not on the terrain
        /// </summary>
        void IComponent<ModularScatter>.Update(ModularScatter system)
        {
            PQSMod_LandClassScatterQuad[] quads = system.GetComponentsInChildren<PQSMod_LandClassScatterQuad>(true);
            for (Int32 i = 0; i < quads.Length; i++)
            {
                var surfaceObjects = quads[i].obj.GetComponentsInChildren<KopernicusSurfaceObject>(true);
                // If there's nothing to do, discard any cached objects and abort
                if (surfaceObjects.Count() == 0)
                {
                    if (!_moved.Any())
                    {
                        return;
                    }

                    _moved.Clear();
                    return;
                }

                if (surfaceObjects.Count() != _moved.Count)
                {
                    _moved.Clear();
                }

                if (_moved.Count > 0)
                {
                    return;
                }

                // Init the seed
                Random.InitState(system.scatter.seed);

                // Shift every object to sea level

                KopernicusSurfaceObject scatter = surfaceObjects[i];

                Vector3 position = system.body.pqsController.transform.position;
                Vector3 direction = (scatter.transform.position - position).normalized;
                scatter.transform.position = position +
                                             direction * (Single)(system.body.Radius + system.scatter.verticalOffset +
                                                                   Random.Range(AltitudeVariance[0],
                                                                       AltitudeVariance[1]));
                _moved.Add(scatter);
            }
        }

        /// <summary>
        /// Clear the cache on a scene change
        /// </summary>
        private void OnGameSceneLoadRequested(GameScenes data)
        {
            _moved.Clear();
        }

        void IComponent<ModularScatter>.Apply(ModularScatter system)
        {
            // We don't use this
        }

        void IComponent<ModularScatter>.PostApply(ModularScatter system)
        {
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
        }
    }
}