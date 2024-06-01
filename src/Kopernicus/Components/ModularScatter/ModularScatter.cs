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
using KSP.UI;
using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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
        public PQSLandControl.LandClassScatter scatter;

        /// <summary>
        /// Whether to treat the density calculation as an actual floating point value
        /// </summary>
        public bool useBetterDensity;

        /// <summary>
        /// useBetterDensity : how large is the chance that a scatter object spawns on a quad?
        /// </summary>
        public float spawnChance = 1f;

        /// <summary>
        /// makes the density calculation ignore the game setting for scatter density
        /// </summary>
        public bool ignoreDensityGameSetting;

        /// <summary>
        /// What biomes this scatter may spawn in.  Empty means all.
        /// </summary>
        public List<String> allowedBiomes = new List<String>();

        /// <summary>
        /// Kerbal-exclusive kill Radius. Zero means none.
        /// </summary>
        public int lethalRadius = 0;

        /// <summary>
        /// Kerbal-exclusive kill Radius message on death.  Empty string means off.
        /// </summary>
        public string lethalRadiusMsg = "";

        /// <summary>
        /// Kerbal-exclusive kill Radius 200% zone warning message.  Empty string means off.
        /// </summary>
        public string lethalRadiusWarnMsg = "";

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

        /// <summary>
        /// true if each scatter requires an individual GameObject, which
        /// is required if the ScatterColliders or LightEmitter components
        /// are used.
        /// </summary>
        public bool needsPerScatterGameObject;

        /// <summary>
        /// true if some functionality require keeping track of the scatter positions
        /// Used by the HeatEmitter component and the lethalRadius option
        /// </summary>
        public bool needsScatterPositions;

        /// <summary>
        /// If scatter positions are needed, quads will register themselves in this collection
        /// in order to be updated by this ModularScatter instance.
        /// </summary>
        public HashSet<PQSMod_KopernicusLandClassScatterQuad> updatingQuads = new HashSet<PQSMod_KopernicusLandClassScatterQuad>();

        /// <summary>
        /// lethalRadius squared for fast distance comparison
        /// </summary>
        private float lethalSquareRadius;

        /// <summary>
        /// lethalRadius warning range squared for fast distance comparison
        /// </summary>
        private float lethalWarnSquareRadius;

        /// <summary>
        /// whether or not we have msg'd this kerbal already, to avoid spamming. Sets false again when out of danger.
        /// </summary>
        private static bool lethalMsgSent = false;
        private static bool lethalWarnMsgSent = false;
        private static int antiSpamCounter = 1600;

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
            scatterName = scatter.scatterName;
            scatter = landControl.scatters.First(s => s.scatterName == scatterName);

            // force the scatter quads to be parented to us
            scatter.scatterParent = gameObject;

            gameObject.name = "Scatter " + scatter.scatterName;
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

            // remove empty entries in allowedBiomes due to the kopernicus parser being dumb
            for (int i = allowedBiomes.Count; i-- > 0;)
                if (string.IsNullOrWhiteSpace(allowedBiomes[i]))
                    allowedBiomes.RemoveAt(i);

            needsPerScatterGameObject = lightEmitter != null || scatterColliders != null;
            needsScatterPositions = heatEmitter != null || lethalRadius != 0;
            lethalSquareRadius = lethalRadius * lethalRadius;
            lethalWarnSquareRadius = (lethalRadius * 2) * (lethalRadius * 2);
        }

        /// <summary>
        /// If necessary (heatEmitter or lethal scatters), on every quad that registered itself,
        /// update scatters world position and check their distance against EVA kerbals. 
        /// We do this here instead on in a per-quad fixedUpdate to save some pointless and
        /// significant overhead when those features aren't used
        /// </summary>
        private void FixedUpdate()
        {
            if (!needsScatterPositions)
                return;

            // if no active vessel, we aren't in the flight scene, no need to update either
            Vessel activeVessel = FlightGlobals.ActiveVessel;
            if (activeVessel.IsNullOrDestroyed())
                return;

            // perf optimization : if scatters aren't heat emitters, we only need to update positions if there is a kerbal on EVA
            // note : technically there could be other EVA kerbals than the active vessel and we should be checking every loaded vessel
            // but the original implementation didn't care and that's probably good enough in most cases.
            bool doLethalCheck = lethalRadius != 0 && activeVessel.isEVA;
            if (heatEmitter == null && !doLethalCheck)
                return;

            Vector3 evaKerbalPos = activeVessel.transform.position;

            foreach (PQSMod_KopernicusLandClassScatterQuad quad in updatingQuads)
            {
                if (!quad.isBuilt || !quad.hasScatters)
                    return;

                List<Vector3> scatterWorldPositions = quad.scatterWorldPositions;

                // perf optimization : only update scatters world position if the quad has moved (typically due to floating origin shifts)
                Matrix4x4 quadMatrix = quad.transform.localToWorldMatrix;
                if (quad.cachedQuadPosition.x != quadMatrix.m03 || quad.cachedQuadPosition.y != quadMatrix.m13 || quad.cachedQuadPosition.z != quadMatrix.m23)
                {
                    quad.cachedQuadPosition = new Vector3(quadMatrix.m03, quadMatrix.m13, quadMatrix.m23);
                    List<Vector3> scatterLocalPositions = quad.scatterLocalPositions;
                    for (int i = scatterLocalPositions.Count; i-- > 0;)
                        scatterWorldPositions[i] = quadMatrix.MultiplyPoint3x4(scatterLocalPositions[i]);
                }

                if (doLethalCheck)
                {
                    for (int i = scatterWorldPositions.Count; i-- > 0;)
                    {
                        if ((scatterWorldPositions[i] - evaKerbalPos).sqrMagnitude < lethalSquareRadius)
                        {
                            if ((lethalRadiusMsg.Length != 0) && (lethalMsgSent == false))
                            {
                                lethalMsgSent = true;
                                PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "Poof!", "Poof!", lethalRadiusMsg, "Poof!", true, UISkinManager.defaultSkin);
                            }
                            activeVessel.rootPart.explode();
                            break;
                        }
                        else if ((scatterWorldPositions[i] - evaKerbalPos).sqrMagnitude < lethalWarnSquareRadius)
                        {
                            if ((lethalRadiusWarnMsg.Length != 0) && (lethalWarnMsgSent == false))
                            {
                                lethalWarnMsgSent = true;
                                PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "DANGER", "DANGER", lethalRadiusWarnMsg, "UhOh!", true, UISkinManager.defaultSkin);
                            }
                        }
                    }
                }
            }
            if (antiSpamCounter < 1)
            {
                antiSpamCounter = 1600;
                lethalMsgSent = false;
                lethalWarnMsgSent = false;
            }
            else 
            {
                antiSpamCounter--;
            }
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
