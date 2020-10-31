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
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class LightEmitterComponent : IComponent<ModularScatter>
    {
        /// <summary>
        /// Contains a List of lights for the scatter
        /// </summary>
        private readonly List<Light> _lights = new List<Light>();

        /// <summary>
        /// The prefab object that is instantiated to every scatter objects
        /// </summary>
        public Light Prefab;

        /// <summary>
        /// The offset of the light, relative to the center of the scatter object
        /// </summary>
        public Vector3 Offset = Vector3.zero;

        /// <summary>
        /// Gets executed every frame and checks if some of the scatters don't have
        /// </summary>
        /// <param name="system"></param>
        void IComponent<ModularScatter>.Update(ModularScatter system)
        {
            // If there's nothing to do, discard any old lights and abort
            if (system.scatterObjects.Count == 0)
            {
                if (!_lights.Any())
                {
                    return;
                }

                Debug.LogWarning("[Kopernicus] Discard old lights");
                foreach (Light light in _lights.Where(l => l))
                {
                    UnityEngine.Object.Destroy(light.gameObject);
                }

                _lights.Clear();
                return;
            }

            Boolean rebuild = false;
            if (system.scatterObjects.Count > _lights.Count)
            {
                Debug.LogWarning("[Kopernicus] Add " + (system.scatterObjects.Count - _lights.Count) +
                                 " lights");
                rebuild = true;
            }
            else if (system.scatterObjects.Count < _lights.Count)
            {
                Debug.LogWarning("[Kopernicus] Remove " + (_lights.Count - system.scatterObjects.Count) +
                                 " lights");
                rebuild = true;
            }

            if (!rebuild)
            {
                return;
            }

            for (Int32 i = 0; i < system.scatterObjects.Count; i++)
            {
                GameObject scatter = system.scatterObjects[i];

                Light light = scatter.GetComponentInChildren<Light>();
                if (light)
                {
                    continue;
                }

                GameObject lightObject = UnityEngine.Object.Instantiate(Prefab.gameObject, scatter.transform, true);
                lightObject.transform.localPosition = Offset;
                lightObject.transform.localScale = Vector3.one;
                lightObject.transform.localRotation = Quaternion.identity;
                _lights.Add(lightObject.GetComponent<Light>());
            }
        }

        /// <summary>
        /// Destroy the generated objects on a scene change so they don't appear in random positions
        /// </summary>
        private void OnGameSceneLoadRequested(GameScenes data)
        {
            foreach (Light light in _lights)
            {
                UnityEngine.Object.Destroy(light);
            }

            _lights.Clear();
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
