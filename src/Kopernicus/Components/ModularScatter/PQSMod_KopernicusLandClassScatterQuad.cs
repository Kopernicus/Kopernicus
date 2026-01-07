using System;
using System.Collections.Generic;
using HarmonyLib;
using Kopernicus.Constants;
using ModularFI;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Kopernicus.Components.ModularScatter
{
    /// <summary>
    /// Per quad scatter system manager living on child objects of the Kopernicus scatter controller (ModularScatter)
    /// We inherit from the stock class to extend it, and inject/override it with harmony patches.
    /// </summary>
    public class PQSMod_KopernicusLandClassScatterQuad : PQSMod_LandClassScatterQuad
    {
        private static ProfilerMarker pmCreateQuadScatter = new ProfilerMarker("Kopernicus.CreateQuadScatter");

        private static List<CombineInstance> combineInstances = new List<CombineInstance>(200);

        /// <summary>
        /// Reference to the Kopernicus scatter controller
        /// </summary>
        public ModularScatter modularScatter;

        /// <summary>
        /// true if this quad actually has scatters (can be false when the allowedBiome feature is used)
        /// </summary>
        public bool hasScatters;

        /// <summary>
        /// Cached world position of the quad, checked against to avoid needless scatter position updates
        /// </summary>
        public Vector3 cachedQuadPosition;

        /// <summary>
        /// quad-relative position of every scatter object, only populated if necessary
        /// </summary>
        public List<Vector3> scatterLocalPositions;

        /// <summary>
        /// world position of every scatter object, only updated if necessary
        /// </summary>
        public List<Vector3> scatterWorldPositions;

        /// <summary>
        /// Main method in charge of placing individual scatters on the quad
        /// </summary>
        public void CreateScatters()
        {
            pmCreateQuadScatter.Begin();

            if (modularScatter.allowedBiomes.Count > 0)
            {
                // TODO : perf refactor, use biome index/reference instead of name
                string scatterBiome = Utility.GetBiome(modularScatter.body, quad.quadTransform.position)?.name;
                if (scatterBiome != null && !modularScatter.allowedBiomes.Contains(scatterBiome))
                {
                    pmCreateQuadScatter.End();
                    hasScatters = false;
                    return;
                }
            }

            if (modularScatter.needsScatterPositions)
            {
                modularScatter.updatingQuads.Add(this);

                if (scatterLocalPositions == null)
                {
                    scatterLocalPositions = new List<Vector3>(count);
                    scatterWorldPositions = new List<Vector3>(count);
                }
                else
                {
                    scatterLocalPositions.Clear();
                    scatterWorldPositions.Clear();
                }
            }

            if (modularScatter.heatEmitter != null)
            {
                Events.OnCalculateBackgroundRadiationTemperature.Add(OnCalculateBackgroundRadiationTemperature);
            }

            Random.InitState(seed);

            int scatterCount = 0;
            int cachedScatterGOCount = transform.childCount;
            for (int scatterLoop = 0; scatterLoop < count; scatterLoop++)
            {
                // note : doing this here is silly, it should be moved outside the loop.
                // but doing this would change the seed-based distribution, so keep it here for backward compatibility.
                if (modularScatter.useBetterDensity)
                {
                    // Generate a random number between 0 and 1. If it is above the spawn chance, abort
                    if (Random.value > modularScatter.spawnChance)
                    {
                        if (RuntimeUtility.RuntimeUtility.KopernicusConfig.UseIncorrectScatterDensityLogic)
                        {
                            break;
                        }
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
                if (modularScatter.scatter.sphere.surfaceRelativeQuads)
                    scatterUp = (scatterPos + quad.positionPlanet).normalized;
                else
                    scatterUp = scatterPos.normalized;
                float verticalOffset = modularScatter.scatter.verticalOffset;

                // apply altitude variance defined by an eventual SeaLevelScatter component
                if (modularScatter.seaLevelScatter != null)
                    verticalOffset += Random.Range(modularScatter.seaLevelScatter.AltitudeVariance[0], modularScatter.seaLevelScatter.AltitudeVariance[1]);

                scatterPos += scatterUp * verticalOffset;

                if (modularScatter.needsScatterPositions)
                    scatterLocalPositions.Add(scatterPos);

                float scatterAngle = Random.Range(modularScatter.rotation[0], modularScatter.rotation[1]);
                Quaternion scatterRot = Quaternion.AngleAxis(scatterAngle, scatterUp) * (Quaternion)quad.quadRotation;
                float scatterScale = Random.Range(modularScatter.scatter.minScale, modularScatter.scatter.maxScale);
                Vector3 scatterScaleVector = new Vector3(scatterScale, scatterScale, scatterScale);

                // Stock does the mesh combining manually.
                // We use the Unity CombineMeshes feature instead, this is easier and a little bit faster.
                combineInstances.Add(new CombineInstance()
                {
                    mesh = modularScatter.hasMultipleMeshes ? modularScatter.meshes[Random.Range(0, modularScatter.meshes.Count)] : modularScatter.baseMesh,
                    transform = Matrix4x4.TRS(scatterPos, scatterRot, scatterScaleVector)
                });
                // Some components require having a per scatter sub-object
                // We instantiate them on-demand and reuse them. They aren't destroyed, even on scene switches
                // so be extra careful not leaking references from them.
                if (modularScatter.needsPerScatterGameObject)
                {
                    GameObject scatterGO;
                    Transform scatterTransform;
                    if (scatterCount < cachedScatterGOCount)
                    {
                        scatterTransform = transform.GetChild(scatterCount);
                        scatterGO = scatterTransform.gameObject;
                        scatterGO.SetActive(true);
                    }
                    else
                    {
                        scatterGO = new GameObject(modularScatter.scatterName);
                        scatterGO.layer = GameLayers.LOCAL_SPACE;
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
                                    mc.sharedMesh = combineInstances[scatterCount].mesh;
                                else
                                    mc.sharedMesh = modularScatter.scatterColliders.CollisionMesh;
                            }
                        }

                        if (modularScatter.lightEmitter != null)
                        {
                            GameObject lightObject = Instantiate(modularScatter.lightEmitter.Prefab.gameObject, scatterTransform, false);
                            lightObject.transform.localPosition = modularScatter.lightEmitter.Offset;
                        }
                    }

                    // match the individual scatter position/rotation/scale
                    scatterTransform.localPosition = scatterPos;
                    scatterTransform.localRotation = scatterRot;
                    scatterTransform.localScale = scatterScaleVector;
                    // if there are multiple collider meshes, assign the right one if necessary
                    if (modularScatter.scatterColliders != null && modularScatter.hasMultipleColliderMeshes)
                    {
                        MeshCollider mc = scatterGO.GetComponent<MeshCollider>();
                        Mesh mesh = combineInstances[scatterCount].mesh;
                        if (mc.sharedMesh.RefNotEquals(mesh))
                            mc.sharedMesh = mesh;
                    }
                }
                scatterCount++;
            }

            // disable unused scatter gameobjects if needed
            if (modularScatter.needsPerScatterGameObject)
            {
                int scatterGoIndex = scatterCount;
                while (scatterGoIndex < cachedScatterGOCount)
                {
                    transform.GetChild(scatterGoIndex).gameObject.SetActive(false);
                    scatterGoIndex++;
                }
            }

            // populate the world position list if needed
            if (modularScatter.needsScatterPositions)
                scatterWorldPositions.AddRange(scatterLocalPositions);

            // build the rendered mesh and activate the quad object
            mesh.CombineMeshes(combineInstances.ToArray());
            combineInstances.Clear();
            obj.SetActive(true);
            cachedQuadPosition = Vector3.zero;
            hasScatters = scatterCount > 0;
            isBuilt = true;

            pmCreateQuadScatter.End();
        }

        /// <summary>
        /// Callback for the HeatEmitter component
        /// </summary>
        public void OnCalculateBackgroundRadiationTemperature(ModularFlightIntegrator mfi)
        {
            modularScatter.heatEmitter.OnCalculateBackgroundRadiationTemperature(mfi, this);
        }
    }

    /// <summary>
    /// This is where a scatter quad is being activated. We call our replacement CreateScatters() method instead of the
    /// original PQSLandControl.LandClassScatter.CreateScatterMesh(PQSMod_LandClassScatterQuad) method.
    /// </summary>
    [HarmonyPatch(typeof(PQSMod_LandClassScatterQuad), nameof(PQSMod_LandClassScatterQuad.OnQuadUpdate))]
    class PQSMod_LandClassScatterQuad_OnQuadUpdate
    {
        static bool Prefix(PQSMod_KopernicusLandClassScatterQuad __instance, PQ quad)
        {
            if (!__instance.isBuilt && __instance.isVisible && quad.sphereRoot.quadAllowBuild && quad.sphereRoot.targetSpeed < __instance.scatter.maxSpeed)
            {
                __instance.CreateScatters();
                __instance.isBuilt = true;
                __instance.obj.SetActive(value: true);
                quad.onUpdate = (PQ.QuadDelegate)Delegate.Remove(quad.onUpdate, new PQ.QuadDelegate(__instance.OnQuadUpdate));
            }
            return false;
        }
    }

    /// <summary>
    /// Called when a scatter quad becomes inactive or is recycled.
    /// Remove the HeatEmitter callback and clear positions list.
    /// </summary>
    [HarmonyPatch(typeof(PQSMod_LandClassScatterQuad), nameof(PQSMod_LandClassScatterQuad.Destroy))]
    class PQSMod_LandClassScatterQuad_Destroy
    {
        static void Prefix(PQSMod_KopernicusLandClassScatterQuad __instance)
        {
            if (__instance.isBuilt && __instance.modularScatter.needsScatterPositions)
            {
                __instance.modularScatter.updatingQuads.Remove(__instance);

                if (__instance.scatterLocalPositions != null)
                {
                    __instance.scatterLocalPositions.Clear();
                    __instance.scatterWorldPositions.Clear();
                }

                if (__instance.modularScatter.heatEmitter != null)
                    Events.OnCalculateBackgroundRadiationTemperature.Remove(__instance.OnCalculateBackgroundRadiationTemperature);
            }
        }
    }

    /// <summary>
    /// PQSLandControl.LandClassScatter.AddScatterMeshController() uses the density param to compute the
    /// LandClassScatter.scatterN value latter used to determine scatter count per quad.
    /// We want to Revert PQS.Global_ScatterFactor if we should ignore the scatter density factor set in the KSP main menu settings
    /// </summary>
    [HarmonyPatch(typeof(PQSLandControl.LandClassScatter), nameof(PQSLandControl.LandClassScatter.AddScatterMeshController))]
    class PQSLandControl_LandClassScatter_AddScatterMeshController
    {
        static void Prefix(PQSLandControl.LandClassScatter __instance, ref double density)
        {
            ModularScatter modularScatter = __instance.scatterParent.GetComponent<ModularScatter>();
            if (modularScatter.ignoreDensityGameSetting && PQS.Global_ScatterFactor > 0)
                density /= PQS.Global_ScatterFactor;
        }
    }

    /// <summary>
    /// PQSLandControl.LandClassScatter.BuildCache() is the method creating the child objects of the scatter
    /// controller (which we replaced by the object holding our top level ModularScatter component). This is
    /// where we swap the stock PQSMod_LandClassScatterQuad by PQSMod_KopernicusLandClassScatterQuad.
    /// </summary>
    [HarmonyPatch(typeof(PQSLandControl.LandClassScatter), nameof(PQSLandControl.LandClassScatter.BuildCache))]
    class PQSLandControl_LandClassScatter_BuildCache
    {
        static bool Prefix(PQSLandControl.LandClassScatter __instance, int countToAdd)
        {
            if (__instance.scatterParent.IsNullOrDestroyed())
            {
                Debug.LogError($"[KOPERNICUS] {__instance.scatterName} scatter parent gameobject isn't set");
                return false;
            }

            for (int i = 0; i < countToAdd; i++)
            {
                GameObject gameObject = new GameObject("Unass");
                gameObject.layer = GameLayers.LOCAL_SPACE;
                gameObject.transform.SetParent(__instance.scatterParent.transform, false);

                PQSMod_KopernicusLandClassScatterQuad pQSMod_LandClassScatterQuad = gameObject.AddComponent<PQSMod_KopernicusLandClassScatterQuad>();
                pQSMod_LandClassScatterQuad.obj = gameObject;
                pQSMod_LandClassScatterQuad.modularScatter = __instance.scatterParent.GetComponent<ModularScatter>();
                pQSMod_LandClassScatterQuad.mesh = new Mesh();
                pQSMod_LandClassScatterQuad.mesh.indexFormat = IndexFormat.UInt32;
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

    [HarmonyPatch(typeof(PQSLandControl.LandClassScatter), nameof(PQSLandControl.LandClassScatter.Setup))]
    class PQSLandControl_LandClassScatter_Setup
    {
        static bool Prefix(PQSLandControl.LandClassScatter __instance, PQS sphere)
        {
            __instance.sphere = sphere;
            __instance.minLevel = Mathf.Abs(__instance.maxLevelOffset) + sphere.maxLevel;
            if (!__instance.cacheCreated)
            {
                __instance.cacheAssigned = new List<PQSMod_LandClassScatterQuad>(__instance.maxCache);
                __instance.cacheAssignedCount = 0;
                __instance.cacheUnassigned = new Stack<PQSMod_LandClassScatterQuad>(__instance.maxCache);
                __instance.cacheUnassignedCount = 0;
                __instance.cacheTotalCount = 0;
                __instance.cacheCreated = true;
            }

            return false;
        }
    }
}
