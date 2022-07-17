using System;
using System.Collections.Generic;
using HarmonyLib;
using Kopernicus.Constants;
using ModularFI;
using Unity.Profiling;
using UnityEngine;
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
        /// Used by the HeatEmitter component to know scatter positions without having to instantiate a gameobject
        /// </summary>
        public List<Vector3> scatterPositions;

        /// <summary>
        /// Main method in charge of placing individual scatters on the quad
        /// </summary>
        public void CreateScatters()
        {
            pmCreateQuadScatter.Begin();

            if (modularScatter.allowedBiomes.Count > 0)
            {
                // TODO : perf refactor, use biome index/reference instead of name
                UnityEngine.Vector2d latLon = modularScatter.body.GetLatitudeAndLongitude(quad.transform.position);
                string scatterBiome = PQSMod_BiomeSampler.GetCachedBiome(latLon.x, latLon.y, modularScatter.body);
                if (!modularScatter.allowedBiomes.Contains(scatterBiome))
                {
                    pmCreateQuadScatter.End();
                    return;
                }
            }

            if (modularScatter.heatEmitter != null)
            {
                Events.OnCalculateBackgroundRadiationTemperature.Add(OnCalculateBackgroundRadiationTemperature);
                if (scatterPositions == null)
                {
                    scatterPositions = new List<Vector3>(modularScatter.landClassScatter.maxScatter);
                }
            }

            Random.InitState(seed);

            int scatterGOIndex = 0;
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
                combineInstances.Add(new CombineInstance()
                {
                    mesh = modularScatter.hasMultipleMeshes ? modularScatter.meshes[Random.Range(0, modularScatter.meshes.Count)] : modularScatter.baseMesh,
                    transform = Matrix4x4.TRS(scatterPos, scatterRot, scatterScaleVector)
                });

                // the ScatterColliders and LightEmitter components require having a per scatter sub-object
                // We instantiate them on-demand and reuse them. They aren't destroyed, even of scene switches
                // so be extra careful not leaking references from them.
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
                                    mc.sharedMesh = combineInstances[scatterGOIndex].mesh;
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
                        Mesh mesh = combineInstances[scatterGOIndex].mesh;
                        if (mc.sharedMesh.RefNotEquals(mesh))
                            mc.sharedMesh = mesh;
                    }
                }

                scatterGOIndex++;
            }

            // disable unused scatter gameobjects if needed
            if (modularScatter.needsPerScatterGameObject)
            {
                while (scatterGOIndex < cachedScatterGOCount)
                {
                    transform.GetChild(scatterGOIndex).gameObject.SetActive(false);
                    scatterGOIndex++;
                }
            }

            // build the rendered mesh and activate the quad object
            mesh.CombineMeshes(combineInstances.ToArray());
            combineInstances.Clear();
            obj.SetActive(true);
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
            if (__instance.isBuilt && __instance.modularScatter.heatEmitter != null)
            {
                Events.OnCalculateBackgroundRadiationTemperature.Remove(__instance.OnCalculateBackgroundRadiationTemperature);
                __instance.scatterPositions.Clear();
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
