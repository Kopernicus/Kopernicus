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
using Kopernicus.Components.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kopernicus.Components.ModularScatter
{
    /// <summary>
    /// Top level component that hold data for a single scatter type. It is referenced and used
    /// by the per-quad PQSMod_KopernicusLandClassScatterQuad component that actually implements
    /// our custom behaviours.
    /// </summary>
    public class ModularScatter : SerializableMonoBehaviour, IComponentSystem<ModularScatter>
    {
        /// <summary>
        /// Components that can be added to the scatter
        /// </summary>
        public List<IComponent<ModularScatter>> Components
        {
            get { return components; }
            set { components = value; }
        }

        [SerializeField]
        private List<IComponent<ModularScatter>> components;

        public string scatterName;

        /// <summary>
        /// The celestial body we are attached to
        /// </summary>
        public CelestialBody body;

        /// <summary>
        /// The mod we are attached to
        /// </summary>
        public PQSLandControl landControl;

        /// <summary>
        /// The scatter instance we are attached to
        /// </summary>
        public PQSLandControl.LandClassScatter landClassScatter;

        /// <summary>
        /// Whether to treat the density calculation as an actual floating point value
        /// </summary>
        public Boolean useBetterDensity;

        /// <summary>
        /// useBetterDensity : how large is the chance that a scatter object spawns on a quad?
        /// </summary>
        public Single spawnChance = 1f;

        /// <summary>
        /// makes the density calculation ignore the game setting for scatter density
        /// </summary>
        public Boolean ignoreDensityGameSetting;

        /// <summary>
        /// What biomes this scatter may spawn in.  Empty means all.
        /// </summary>
        public List<String> allowedBiomes = new List<String>();

        /// <summary>
        /// How much should the scatter be able to rotate
        /// </summary>
        public List<Single> rotation = new List<Single> { 0, 360f };

        /// <summary>
        /// A list of all meshes that can be used for the
        /// </summary>
        public List<Mesh> meshes = new List<Mesh>();

        /// <summary>
        /// The base mesh for the scatter.
        /// </summary>
        public Mesh baseMesh;

        /// <summary>
        /// true if each scatter requires an individual GameObject, which
        /// is required if the ScatterColliders or LightEmitter components
        /// are used.
        /// </summary>
        public bool needsPerScatterGameObject;

        /// <summary>
        /// shorthand bool to avoid having to check meshes.Count
        /// </summary>
        public bool hasMultipleMeshes;

        /// <summary>
        /// shorthand bool to avoid having to check meshes.Count
        /// </summary>
        public bool hasMultipleColliderMeshes;

        // Ideally this thing should be refactored to not use the component
        // classes, they should just be extra data in ModularScatter.
        // But good enough for now, I don't want to fight the Kopernicus parser.
        public LightEmitterComponent lightEmitter;
        public SeaLevelScatterComponent seaLevelScatter;
        public HeatEmitterComponent heatEmitter;
        public ScatterCollidersComponent scatterColliders;

        public ModularScatter()
        {
            Components = new List<IComponent<ModularScatter>>();
        }

        private void Start()
        {
            landControl = transform.parent.GetComponent<PQSLandControl>();
            body = landControl.GetComponentInParent<CelestialBody>();

            // Get the actual live LandClassScatter instance. The one we currently have 
            // is a dummy instance created by the Kopernicus deserialization process
            scatterName = landClassScatter.scatterName;
            landClassScatter = landControl.scatters.First(s => s.scatterName == scatterName);

            // force the scatter quads to be parented to us
            landClassScatter.scatterParent = gameObject;

            gameObject.name = "Scatter " + landClassScatter.scatterName;
            transform.parent = landControl.sphere.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            if (meshes.Count > 1)
            {
                hasMultipleMeshes = true;
            }
            else
            {
                hasMultipleMeshes = false;
                if (baseMesh.IsNullOrDestroyed())
                {
                    if (meshes.Count == 1)
                        baseMesh = meshes[0];
                    else
                        baseMesh = GetDefaultScatterMesh();
                }
            }

            foreach (IComponent<ModularScatter> component in components)
            {
                if (component is LightEmitterComponent)
                {
                    lightEmitter = (LightEmitterComponent)component;
                }
                else if (component is SeaLevelScatterComponent)
                {
                    seaLevelScatter = (SeaLevelScatterComponent)component;
                }
                else if (component is HeatEmitterComponent)
                {
                    heatEmitter = (HeatEmitterComponent)component;
                }
                else if (component is ScatterCollidersComponent)
                {
                    scatterColliders = (ScatterCollidersComponent)component;
                    hasMultipleColliderMeshes = hasMultipleMeshes && scatterColliders.CollisionMesh.IsNullRef();
                }
                else
                {
                    // defining a custom scatter component type in another plugin isn't supported anymore
                    Debug.LogWarning($"[KOPERNICUS] Scatter component of type {component.GetType()} is unsupported");
                }
            }

            needsPerScatterGameObject = lightEmitter != null || scatterColliders != null;
        }

        private static Mesh defaultMesh;

        /// <summary>
        /// Get the default 2D billboard mesh
        /// </summary>
        private static Mesh GetDefaultScatterMesh()
        {
            if (defaultMesh.IsNullRef())
            {
                defaultMesh = new Mesh();
                defaultMesh.vertices = new Vector3[8]
                {
                    Vector3.zero,
                    Vector3.right,
                    Vector3.up,
                    new Vector3(1f, 1f, 0f),
                    Vector3.zero,
                    Vector3.right,
                    Vector3.up,
                    new Vector3(1f, 1f, 0f)
                };

                defaultMesh.triangles = new int[12] { 0, 1, 2, 2, 1, 3, 4, 6, 5, 6, 7, 5 };

                defaultMesh.normals = new Vector3[8]
                {
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.back,
                    Vector3.back,
                    Vector3.back,
                    Vector3.back
                };
                defaultMesh.uv = new Vector2[8]
                {
                    Vector2.zero,
                    Vector2.right,
                    Vector2.up,
                    Vector2.one,
                    Vector2.zero,
                    Vector2.right,
                    Vector2.up,
                    Vector2.one
                };

                defaultMesh.RecalculateBounds();
            }

            return defaultMesh;
        }
    }
}
