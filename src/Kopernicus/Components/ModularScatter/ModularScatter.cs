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


// so all the features are :
// - [DONE] ScatterCollider : requires adding a GO, a meshfilter and a meshcollider
//   - only if this is enabled, add a KopernicusSurfaceObject component so the ModuleSurfaceObjectTrigger PM can find it
// - [DONE] SeaLevelScatter : just a vertical position offset, no need for a GO
// - LightEmitter : need a GO with a light on it
// - HeatEmitter : add itself to a MFI callback and add heat to the vessel

using HarmonyLib;
using Kopernicus.Components.ModularComponentSystem;
using Kopernicus.Components.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kopernicus.Constants;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Kopernicus.Components.ModularScatter
{
    [HarmonyPatch(typeof(PQSMod_LandClassScatterQuad), nameof(PQSMod_LandClassScatterQuad.OnQuadUpdate))]
    class PQSMod_LandClassScatterQuad_OnQuadUpdate
    {
        static bool Prefix(PQSMod_LandClassScatterQuad __instance, PQ quad)
        {
            if (!__instance.isBuilt && __instance.isVisible && quad.sphereRoot.quadAllowBuild && quad.sphereRoot.targetSpeed < __instance.scatter.maxSpeed)
            {
                ((PQSMod_KopernicusLandClassScatterQuad)__instance).CreateScatters(quad);
                __instance.isBuilt = true;
                __instance.obj.SetActive(value: true);
                quad.onUpdate = (PQ.QuadDelegate)Delegate.Remove(quad.onUpdate, new PQ.QuadDelegate(__instance.OnQuadUpdate));
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(PQSLandControl.LandClassScatter), "BuildCache")]
    class PQSLandControl_LandClassScatter_BuildCache
    {
        static bool Prefix(PQSLandControl.LandClassScatter __instance, int countToAdd)
        {
            if (__instance.scatterParent.IsNullOrDestroyed())
                Debug.LogError($"[KOPERNICUS] {__instance.scatterName} scatter parent gameobject isn't set");

            for (int i = 0; i < countToAdd; i++)
            {
                GameObject gameObject = new GameObject("Unass");
                gameObject.layer = GameLayers.LOCAL_SPACE;
                gameObject.transform.SetParent(__instance.scatterParent.transform, false);

                PQSMod_KopernicusLandClassScatterQuad pQSMod_LandClassScatterQuad = gameObject.AddComponent<PQSMod_KopernicusLandClassScatterQuad>();
                pQSMod_LandClassScatterQuad.obj = gameObject;
                pQSMod_LandClassScatterQuad.modularScatter = __instance.scatterParent.GetComponent<ModularScatter>();
                pQSMod_LandClassScatterQuad.mesh = new Mesh();
                pQSMod_LandClassScatterQuad.mf = gameObject.AddComponent<MeshFilter>();
                pQSMod_LandClassScatterQuad.mf.sharedMesh = pQSMod_LandClassScatterQuad.mesh;
                pQSMod_LandClassScatterQuad.mr = gameObject.AddComponent<MeshRenderer>();
                pQSMod_LandClassScatterQuad.mr.sharedMaterial = __instance.material;
                pQSMod_LandClassScatterQuad.mr.shadowCastingMode = __instance.castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
                pQSMod_LandClassScatterQuad.mr.receiveShadows = __instance.recieveShadows;
                pQSMod_LandClassScatterQuad.obj.SetActive(false);
                __instance.cacheUnassigned.Push(pQSMod_LandClassScatterQuad);
            }

            __instance.cacheTotalCount += countToAdd;
            __instance.cacheUnassignedCount += countToAdd;

            return false;
        }
    }

    public class PQSMod_KopernicusLandClassScatterQuad : PQSMod_LandClassScatterQuad
    {
        public ModularScatter modularScatter;
        public List<Vector3> scatterPositions;

        private static List<CombineInstance> combineInstances = new List<CombineInstance>(200);

        private static ProfilerMarker pmCreateQuadScatter = new ProfilerMarker("Kopernicus.CreateQuadScatter");
        public void CreateScatters(PQ quad)
        {
            pmCreateQuadScatter.Begin();

            if (modularScatter.allowedBiomes.Count > 0)
            {
                // TODO : perf refactor, use biome index/reference instead of name
                UnityEngine.Vector2d latLon = FlightGlobals.currentMainBody.GetLatitudeAndLongitude(quad.transform.position);
                string scatterBiome = PQSMod_BiomeSampler.GetCachedBiome(latLon.x, latLon.y, modularScatter.body);
                if (!modularScatter.allowedBiomes.Contains(scatterBiome))
                {
                    pmCreateQuadScatter.End();
                    return;
                }
            }

            Random.InitState(seed);

            if (modularScatter.useBetterDensity)
            {
                // TODO : perf refactor, don't use GetComponent
                Dictionary<string, double> densities = quad.GetComponent<DensityContainer>().densities;
                if (densities.TryGetValue(scatter.scatterName, out double density))
                {
                    if (modularScatter.ignoreDensityGameSetting && PQS.Global_ScatterFactor > 0)
                    {
                        density /= PQS.Global_ScatterFactor;
                    }
                    double scatterN = density * scatter.densityFactor *
                                      (quad.quadArea / quad.sphereRoot.radius / 1000.0) *
                                      scatter.maxScatter;
                    scatterN += Random.Range(modularScatter.densityVariance[0], modularScatter.densityVariance[1]);
                    count = Math.Min((int)Math.Round(scatterN), scatter.maxScatter);
                }
            }

            int scatterGOIndex = 0;
            int cachedScatterGOCount = transform.childCount;
            for (int scatterLoop = 0; scatterLoop < count; scatterLoop++)
            {
                if (modularScatter.useBetterDensity)
                {
                    // Generate a random number between 0 and 1. If it is above the spawn chance, abort
                    if (Random.value > modularScatter.spawnChance)
                    {
                        if (RuntimeUtility.RuntimeUtility.KopernicusConfig.UseIncorrectScatterDensityLogic)
                            break;

                        continue;
                    }
                }

                // stock random positioning logic
                int num2 = -1;
                int num3 = -1;
                while (num3 == num2)
                {
                    int num4 = Random.Range(1, PQS.cacheRes + 1);
                    int num5 = Random.Range(1, PQS.cacheRes + 1);
                    int x = num4 + Random.Range(-1, 1);
                    int z = num5 + Random.Range(-1, 1);
                    num3 = PQS.vi(num4, num5);
                    num2 = PQS.vi(x, z);
                }
                Vector3 scatterPos = Vector3.Lerp(quad.verts[num3], quad.verts[num2], Random.value);
                Vector3 scatterUp;
                if (modularScatter.landClassScatter.sphere.surfaceRelativeQuads)
                    scatterUp = (scatterPos + quad.positionPlanet).normalized;
                else
                    scatterUp = scatterPos.normalized;

                float verticalOffset = modularScatter.landClassScatter.verticalOffset;

                // apply altitude variance defined by an eventual SeaLevelScatter component
                if (modularScatter.seaLevelScatter != null)
                    verticalOffset += Random.Range(modularScatter.seaLevelScatter.AltitudeVariance[0], modularScatter.seaLevelScatter.AltitudeVariance[1]);

                scatterPos += scatterUp * verticalOffset;

                //if (modularScatter.heatEmitter != null)
                //    scatterPositions.Add(scatterPos);

                float scatterAngle = Random.Range(modularScatter.rotation[0], modularScatter.rotation[1]);
                Quaternion scatterRot = Quaternion.AngleAxis(scatterAngle, scatterUp) * (Quaternion)quad.quadRotation;
                float scatterScale = Random.Range(modularScatter.landClassScatter.minScale, modularScatter.landClassScatter.maxScale);
                Vector3 scatterScaleVector = new Vector3(scatterScale, scatterScale, scatterScale);

                // Stock does the mesh combining manually.
                // We use the Unity CombineMeshes feature instead, this is easier and a little bit faster.
                // TODO : verify that we aren't leaking meshes in the process.
                combineInstances.Add(new CombineInstance()
                {
                    mesh = modularScatter.hasMultipleMeshes ? modularScatter.meshes[Random.Range(0, modularScatter.meshes.Count)] : modularScatter.baseMesh,
                    transform = Matrix4x4.TRS(scatterPos, scatterRot, scatterScaleVector)
                });

                if (modularScatter.needsPerScatterGameObject)
                {
                    GameObject scatterGO;
                    Transform scatterTransform;
                    if (scatterGOIndex < cachedScatterGOCount)
                    {
                        scatterTransform = transform.GetChild(scatterGOIndex);
                        scatterGO = scatterTransform.gameObject;
                        scatterGO.SetActive(true);
                    }
                    else
                    {
                        scatterGO = new GameObject(modularScatter.scatterName);
                        scatterTransform = scatterGO.transform;
                        scatterTransform.SetParent(transform, false);

                        if (modularScatter.scatterColliders != null)
                        {
                            MeshCollider mc = scatterGO.AddComponent<MeshCollider>();
                            // if there is a single collider mesh, assign it right now and never assign
                            // it again to avoid the overhead of re-baking it every time.
                            if (!modularScatter.hasMultipleColliderMeshes)
                            {
                                if (modularScatter.scatterColliders.CollisionMesh.IsNullRef())
                                    mc.sharedMesh = combineInstances[scatterGOIndex].mesh;
                                else
                                    mc.sharedMesh = modularScatter.scatterColliders.CollisionMesh;
                            }
                        }
                    }

                    scatterTransform.localPosition = scatterPos;
                    scatterTransform.localRotation = scatterRot;
                    scatterTransform.localScale = scatterScaleVector;

                    // if there are multiple collider meshes, assign the right one if necessary
                    if (modularScatter.scatterColliders != null && modularScatter.hasMultipleColliderMeshes)
                    {
                        MeshCollider mc = scatterGO.GetComponent<MeshCollider>();
                        Mesh mesh = combineInstances[scatterGOIndex].mesh;
                        if (mc.sharedMesh.RefNotEquals(mesh))
                            mc.sharedMesh = mesh;
                    }
                }

                scatterGOIndex++;
            }

            // disable unused scatter gameobjects
            if (modularScatter.needsPerScatterGameObject)
            {
                while (scatterGOIndex < cachedScatterGOCount)
                {
                    transform.GetChild(scatterGOIndex).gameObject.SetActive(false);
                    scatterGOIndex++;
                }
            }

            mesh.CombineMeshes(combineInstances.ToArray());
            combineInstances.Clear();
            obj.SetActive(true);
            isBuilt = true;
            pmCreateQuadScatter.End();
        }
    }

    /// <summary>
    /// Component to add other Components to Scatter objects easily
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
        /// useBetterDensity : how much variation should the scatter density have?
        /// </summary>
        public List<Single> densityVariance = new List<Single> { -0.5f, 0.5f };

        /// <summary>
        /// useBetterDensity : makes the density calculation ignore the game setting for scatter density
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

        public bool needsPerScatterGameObject;

        public bool hasMultipleMeshes;

        [SerializeField]
        private List<IComponent<ModularScatter>> components;

        // ideally this should be properly refactored to not use the components
        // stuff at all but good enough for now.
        public LightEmitterComponent lightEmitter;
        public SeaLevelScatterComponent seaLevelScatter;
        public HeatEmitterComponent heatEmitter;
        public ScatterCollidersComponent scatterColliders;
        public bool hasMultipleColliderMeshes;

        public ModularScatter()
        {
            Components = new List<IComponent<ModularScatter>>();
        }

        /// <summary>
        /// Create colliders for the scatter
        /// </summary>
        private void Start()
        {
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
                    lightEmitter = (LightEmitterComponent)component;
                else if (component is SeaLevelScatterComponent)
                    seaLevelScatter = (SeaLevelScatterComponent)component;
                else if (component is HeatEmitterComponent)
                    heatEmitter = (HeatEmitterComponent)component;
                else if (component is ScatterCollidersComponent)
                {
                    scatterColliders = (ScatterCollidersComponent)component;
                    hasMultipleColliderMeshes = hasMultipleMeshes && scatterColliders.CollisionMesh.IsNullRef();
                }
            }

            needsPerScatterGameObject = lightEmitter != null || scatterColliders != null;

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
