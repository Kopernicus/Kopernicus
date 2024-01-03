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

using HarmonyLib;
using Kopernicus.ConfigParser;
using Kopernicus.Configuration;
using Kopernicus.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using UnityEngine;
using static Targeting;

namespace Kopernicus
{
    // Hook the PSystemSpawn (creation of the planetary system) event in the KSP initialization lifecycle
    [KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class Injector : MonoBehaviour
    {
        // Name of the config node group which manages Kopernicus
        private const String ROOT_NODE_NAME = "Kopernicus";

        // The checksum of the System.cfg file.
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private const String CONFIG_CHECKSUM = "cc7bda69901a41a4231c0a84696615029c4116834e0fceda70cc18d863279533";

        // Backup of the old system prefab, in case someone deletes planet templates we need at Runtime (Kittopia)
        public static PSystem StockSystemPrefab { get; private set; }

        // Whether the injector is currently patching the prefab
        public static Boolean IsInPrefab { get; private set; }

        public static MapSO moho_height;

        // Awake() is the first function called in the lifecycle of a Unity3D MonoBehaviour.  In the case of KSP,
        // it happens to be called right before the game's PSystem is instantiated from PSystemManager.Instance.systemPrefab
        public void Awake()
        {
            // Abort, if KSP isn't compatible
            if (!CompatibilityChecker.IsCompatible())
            {
                String supported = CompatibilityChecker.VERSION_MAJOR + "." + CompatibilityChecker.VERSION_MINOR + "." +
                                   CompatibilityChecker.REVISION;
                String current = Versioning.version_major + "." + Versioning.version_minor + "." + Versioning.Revision;
                Debug.LogWarning("[Kopernicus] Detected incompatible install.\nCurrent version of KSP: " + current +
                                 ".\nSupported version of KSP: " + supported +
                                 ".\nPlease wait, until Kopernicus gets updated to match your version of KSP.");
                Debug.Log("[Kopernicus] Aborting...");

                // Abort
                Destroy(this);
                return;
            }
            // Log the current version to the log
            String kopernicusVersion = CompatibilityChecker.VERSION_MAJOR + "." +
                                       CompatibilityChecker.VERSION_MINOR + "." + CompatibilityChecker.REVISION +
                                       "-" + CompatibilityChecker.KOPERNICUS;
            String kspVersion = Versioning.version_major + "." + Versioning.version_minor + "." +
                                Versioning.Revision;
            Debug.Log("[Kopernicus] Running Kopernicus " + kopernicusVersion + " on KSP " + kspVersion);
            //Harmony stuff

            Harmony harmony = new Harmony("Kopernicus");
            harmony.PatchAll();

            // Wrap this in a try - catch block so we can display a warning if Kopernicus fails to load for some reason
            try
            {
                // We're ALIVE
                IsInPrefab = true;
                Logger.Default.SetAsActive();
                Logger.Default.Log("Injector.Awake(): Begin");

                // Parser Config
                ParserOptions.Register("Kopernicus",
                    new ParserOptions.Data
                    { ErrorCallback = e => Logger.Active.LogException(e), LogCallback = s => Logger.Active.Log(s) });

                // Yo garbage collector - we have work to do man
                DontDestroyOnLoad(this);

                // If the planetary manager does not work, well, error out
                if (PSystemManager.Instance == null)
                {
                    // Log the error
                    Logger.Default.Log("Injector.Awake(): If PSystemManager.Instance is null, there is nothing to do");
                    DisplayWarning();
                    return;
                }

                if (RuntimeUtility.RuntimeUtility.KopernicusConfig.UseStockMohoTemplate)
                {
                    moho_height =
                        Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, "Moho")
                            ?.pqsVersion?.GetComponentInChildren<PQSMod_VertexHeightMap>()?.heightMap;
                }

                // Was the system template modified?
#if !DEBUG
                String systemCfgPath = KSPUtil.ApplicationRootPath + "GameData/Kopernicus/Config/System.cfg";
                if (File.Exists(systemCfgPath))
                {
                    Byte[] data = File.ReadAllBytes(systemCfgPath);
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        String checksum = BitConverter.ToString(sha256.ComputeHash(data));
                        checksum = checksum.Replace("-", "");
                        checksum = checksum.ToLower();
                        if (checksum != CONFIG_CHECKSUM)
                        {
                            throw new Exception(
                                "The file 'Kopernicus/Config/System.cfg' was modified directly without ModuleManager");
                        }
                    }
                }
#endif

                // Backup the old prefab
                StockSystemPrefab = PSystemManager.Instance.systemPrefab;

                // Fire Pre-Load Event
                Events.OnPreLoad.Fire();

                // Get the current time
                DateTime start = DateTime.Now;

                // Get the configNode
                ConfigNode kopernicus = GameDatabase.Instance.GetConfigs(ROOT_NODE_NAME)[0].config;

                // THIS IS WHERE THE MAGIC HAPPENS - OVERWRITE THE SYSTEM PREFAB SO KSP ACCEPTS OUR CUSTOM SOLAR SYSTEM AS IF IT WERE FROM SQUAD
                PSystemManager.Instance.systemPrefab =
                    Parser.CreateObjectFromConfigNode<Loader>(kopernicus, "Kopernicus").SystemPrefab;

                // Clear space center instance so it will accept nouveau Kerbin
                SpaceCenter.Instance = null;

                // Add a handler so that we can do post spawn fixups.
                PSystemManager.Instance.OnPSystemReady.Add(PostSpawnFixups);

                // Fire Post-Load Event
                Events.OnPostLoad.Fire(PSystemManager.Instance.systemPrefab);

                // Done executing the awake function
                TimeSpan duration = DateTime.Now - start;
                Logger.Default.Log("Injector.Awake(): Completed in: " + duration.TotalMilliseconds + " ms");
                Logger.Default.Flush();
                IsInPrefab = false;
            }
            catch (Exception e)
            {
                // Log the exception
                Debug.LogException(e);

                // Open the Warning popup
                DisplayWarning();
            }
        }

        // Post spawn fixups (ewwwww........)
        public void PostSpawnFixups()
        {
            // Wrap this in a try - catch block so we can display a warning if Kopernicus fails to load for some reason
            try
            {
                // Log
                Debug.Log("[Kopernicus]: Post-Spawn");
                // Fire Event
                Events.OnPreFixing.Fire();
                // Fix the SpaceCenter
                try
                {
                    SpaceCenter.Instance = PSystemManager.Instance.localBodies.First(cb => cb.name.Equals(RuntimeUtility.RuntimeUtility.KopernicusConfig.HomeWorldName)).GetComponentsInChildren<SpaceCenter>(true).FirstOrDefault();
                    CelestialBody hb = FlightGlobals.GetBodyByName(RuntimeUtility.RuntimeUtility.KopernicusConfig.HomeWorldName);
                    PSystemSetup.Instance.pqsToActivate = RuntimeUtility.RuntimeUtility.KopernicusConfig.HomeWorldName;
                    Planetarium.fetch.Home = hb;
                    foreach (PSystemSetup.SpaceCenterFacility sc in PSystemSetup.Instance.SpaceCenterFacilities)
                    {
                        sc.pqsName = RuntimeUtility.RuntimeUtility.KopernicusConfig.HomeWorldName;
                        sc.hostBody = hb;
                    }
                    foreach (LaunchSite lc in PSystemSetup.Instance.LaunchSites)
                    {
                        lc.pqsName = RuntimeUtility.RuntimeUtility.KopernicusConfig.HomeWorldName;
                    }

                    foreach (SpaceCenterCamera2 cam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
                    {
                        if (cam.pqsName.Equals("Kerbin"))
                        {
                            cam.pqsName = RuntimeUtility.RuntimeUtility.KopernicusConfig.HomeWorldName;
                        }
                    }
                }
                catch
                {
                    SpaceCenter.Instance = PSystemManager.Instance.localBodies.First(cb => cb.isHomeWorld).GetComponentsInChildren<SpaceCenter>(true).FirstOrDefault();
                }
                //Fix space center camera assignments
                if (SpaceCenter.Instance != null)
                {
                    SpaceCenter.Instance.Start();
                }
                // Fix the flight globals index of each body and patch it's SOI
                Int32 counter = 0;
                CelestialBody mockBody = null;
                foreach (CelestialBody body in FlightGlobals.Bodies)
                {
                    //Find ye old watchdog for slaying (if it exists)
                    if (body.name.Equals("KopernicusWatchdog"))
                    {
                        mockBody = body;
                    }
                    // Event
                    Events.OnPreBodyFixing.Fire(body);

                    // Patch the flightGlobalsIndex
                    body.flightGlobalsIndex = counter++;

                    // Finalize the Orbit
                    if (body.Get("finalizeBody", false))
                    {
                        OrbitLoader.FinalizeOrbit(body);
                    }

                    // Set Custom OrbitalPeriod
                    if (body.Has("customOrbitalPeriod"))
                    {
                        OrbitLoader.OrbitalPeriod(body);
                    }

                    // Patch the SOI
                    if (body.Has("sphereOfInfluence"))
                    {
                        body.sphereOfInfluence = body.Get<Double>("sphereOfInfluence");
                    }

                    // Patch the Hill Sphere
                    if (body.Has("hillSphere"))
                    {
                        body.hillSphere = body.Get<Double>("hillSphere");
                    }

                    // Make the Body a barycenter
                    if (body.Get("barycenter", false))
                    {
                        foreach (Collider collider in body.scaledBody.GetComponentsInChildren<Collider>(true))
                        {
                            collider.enabled = false;
                        }
                        body.scaledBody.SetActive(false);
                    }

                    // Make the bodies scaled space invisible 
                    if (body.Get("invisibleScaledSpace", false))
                    {
                        foreach (Renderer renderer in body.scaledBody.GetComponentsInChildren<Renderer>(true))
                        {
                            renderer.enabled = false;
                        }

                        foreach (Collider collider in body.scaledBody.GetComponentsInChildren<Collider>(true))
                        {
                            collider.enabled = false;
                        }

                        foreach (ScaledSpaceFader fader in body.scaledBody.GetComponentsInChildren<ScaledSpaceFader>(
                            true))
                        {
                            fader.enabled = false;
                        }
                    }
                    else
                    {
                        foreach (Renderer renderer in body.scaledBody.GetComponentsInChildren<Renderer>(true))
                        {
                            if (renderer.enabled)
                            {
                                foreach (Collider collider in body.scaledBody.GetComponentsInChildren<Collider>(true))
                                {
                                    collider.enabled = true;
                                }
                            }
                        }
                    }
                    if ((!body.name.Equals("Sun") && (RuntimeUtility.RuntimeUtility.KopernicusConfig.UseRealWorldDensity)))
                    {
                        if ((!Utility.IsStockBody(body)) && (RuntimeUtility.RuntimeUtility.KopernicusConfig.LimitRWDensityToStockBodies))
                        {
                            //Do Nothing
                        }
                        else
                        {
                            float realWorldSize = RuntimeUtility.RuntimeUtility.KopernicusConfig.RealWorldSizeFactor;
                            float rescaleFactor = RuntimeUtility.RuntimeUtility.KopernicusConfig.RescaleFactor;
                            float gpm;
                            if (!body.hasSolidSurface && !body.isStar) //This catches Joolian-template gas giants and applies a better mass template to them.
                            {
                                if (RuntimeUtility.RuntimeUtility.KopernicusConfig.UseOlderRWScalingLogic)
                                {
                                    gpm = (rescaleFactor / realWorldSize) * 2.5f;
                                }
                                else
                                {
                                    body.Mass = Utility.GasGiantMassFromRadius(body.Radius);
                                    Double rsq = body.Radius;
                                    rsq *= rsq;
                                    body.GeeASL = body.Mass * (6.67408E-11 / 9.80665) / rsq;
                                    body.gMagnitudeAtCenter = body.GeeASL * 9.80665 * rsq;
                                    body.gravParameter = body.gMagnitudeAtCenter;
                                    gpm = 1f;
                                }
                            }
                            else
                            {
                                gpm = rescaleFactor / realWorldSize;
                            }
                            if (gpm < 0.99f || gpm > 1.01f)
                            {
                                body.GeeASL *= gpm;
                                body.scienceValues.spaceAltitudeThreshold *= gpm;
                                if (!RuntimeUtility.RuntimeUtility.KopernicusConfig.UseOlderRWScalingLogic)
                                {
                                    body.gravParameter = body.GeeASL * 9.81 * (body.Radius * body.Radius);
                                }
                                else
                                {
                                    body.gravParameter *= gpm;
                                }
                            }
                        }
                    }
                    // Event
                    Events.OnPostBodyFixing.Fire(body);

                    // Log
                    Logger.Default.Log("Found Body: " + body.bodyName + ":" + body.flightGlobalsIndex + " -> SOI = " +
                                       body.sphereOfInfluence + ", Hill Sphere = " + body.hillSphere);
                }
                //Mark the watchdog for proper removal
                if (mockBody != null)
                {
                    try
                    {
                        FlightGlobals.Bodies.Remove(mockBody);
                        if (Kopernicus.Components.KopernicusStar.GetLocalStar(mockBody).orbitingBodies.Contains(mockBody))
                        {
                            Kopernicus.Components.KopernicusStar.GetLocalStar(mockBody).orbitingBodies.Remove(mockBody);
                        }
                        mockBody.gameObject.DestroyGameObject();
                    }
                    catch
                    {

                    }
                }

                // Fix the maximum viewing distance of the map view camera (get the farthest away something can be from the root object)
                PSystemBody rootBody = PSystemManager.Instance.systemPrefab.rootBody;
                Double maximumDistance = 1000d;
                if (rootBody != null)
                {
                    maximumDistance = rootBody.celestialBody.Radius * 100d;
                    if (rootBody.children != null && rootBody.children.Count > 0)
                    {
                        foreach (PSystemBody body in rootBody.children)
                        {
                            if (body.orbitDriver != null)
                            {
                                maximumDistance = Math.Max(maximumDistance,
                                    body.orbitDriver.orbit.semiMajorAxis * (1d + body.orbitDriver.orbit.eccentricity));
                            }
                            else
                            {
                                Debug.Log("[Kopernicus]: Body " + body.name + " has no Orbit driver!");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("[Kopernicus]: Root body children null or 0");
                    }
                }
                else
                {
                    Debug.Log("[Kopernicus]: Root body null!");
                }

                if (Templates.MaxViewDistance >= 0)
                {
                    maximumDistance = Templates.MaxViewDistance;
                    Debug.Log("Found max distance override " + maximumDistance);
                }
                else
                {
                    Debug.Log("Found max distance " + maximumDistance);
                }

                PlanetariumCamera.fetch.maxDistance =
                    (Single)maximumDistance * 3.0f / ScaledSpace.Instance.scaleFactor;

                // Call the event
                Events.OnPostFixing.Fire();

                // Flush the logger
                Logger.Default.Flush();

                // Fixups complete, time to surrender to fate
                Destroy(this);
            }
            catch (Exception e)
            {
                // Log the exception
                Debug.LogException(e);

                // Open the Warning popup
                DisplayWarning();
            }
        }

        // Displays a warning if Kopernicus failed to load for some reason
        public static void DisplayWarning()
        {
            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "KopernicusFail", "Warning",
                "Kopernicus was not able to load the custom planetary system due to an exception in the loading process.\n" +
                "Loading your saved game is NOT recommended, because the missing planets could corrupt it and delete your progress.\n\n" +
                "Please contact the planet pack author or the Kopernicus team about the issue and send them a valid bug report, including your KSP.log, your ModuleManager.ConfigCache file and the folder Logs/Kopernicus/ from your KSP root directory.\n\n",
                "OK", true, UISkinManager.GetSkin("MainMenuSkin"));
        }

        // Log the destruction of the injector
        public void OnDestroy()
        {
            PSystemManager.Instance.OnPSystemReady.Remove(PostSpawnFixups);
            Logger.Default.Log("Injector.OnDestroy(): Complete");
            Logger.Default.Flush();
        }
    }

    [HarmonyPatch(typeof(MapSO), "ConstructBilinearCoords", new Type[] { typeof(float), typeof(float) })]
    public static class MapSOPPatch_Float
    {
        private static bool Prefix(MapSO __instance, float x, float y)
        {
            if (ReferenceEquals(__instance, Injector.moho_height))
            {
                return true;
            }
            // X wraps around as it is longitude.
            x = Mathf.Abs(x - Mathf.Floor(x));
            __instance.centerX = x * __instance._width;
            __instance.minX = Mathf.FloorToInt(__instance.centerX);
            __instance.maxX = Mathf.CeilToInt(__instance.centerX);
            __instance.midX = __instance.centerX - __instance.minX;
            if (__instance.maxX == __instance._width)
                __instance.maxX = 0;

            // Y clamps as it is latitude and the poles don't wrap to each other.
            y = Mathf.Clamp(y, 0, 0.99999f);
            __instance.centerY = y * __instance._height;
            __instance.minY = Mathf.FloorToInt(__instance.centerY);
            __instance.maxY = Mathf.CeilToInt(__instance.centerY);
            __instance.midY = __instance.centerY - __instance.minY;
            if (__instance.maxY >= __instance._height)
                __instance.maxY = __instance._height - 1;

            return false;
        }
    }
    [HarmonyPatch(typeof(MapSO), "ConstructBilinearCoords", new Type[] { typeof(double), typeof(double) })]
    public static class MapSOPatch_Double
    {
        private static bool Prefix(MapSO __instance, double x, double y)
        {
            if (ReferenceEquals(__instance, Injector.moho_height))
            {
                return true;
            }
            // X wraps around as it is longitude.
            x = Math.Abs(x - Math.Floor(x));
            __instance.centerXD = x * __instance._width;
            __instance.minX = (int)Math.Floor(__instance.centerXD);
            __instance.maxX = (int)Math.Ceiling(__instance.centerXD);
            __instance.midX = (float)__instance.centerXD - __instance.minX;
            if (__instance.maxX == __instance._width)
                __instance.maxX = 0;

            // Y clamps as it is latitude and the poles don't wrap to each other.
            y = Math.Min(Math.Max(y, 0), 0.99999);
            __instance.centerYD = y * __instance._height;
            __instance.minY = (int)Math.Floor(__instance.centerYD);
            __instance.maxY = (int)Math.Ceiling(__instance.centerYD);
            __instance.midY = (float)__instance.centerYD - __instance.minY;
            if (__instance.maxY >= __instance._height)
                __instance.maxY = __instance._height - 1;

            return false;
        }
    }
    [HarmonyPatch(typeof(ROCManager), "ValidateCBBiomeCombos")]
    public static class ROCManager_ValidateCBBiomeCombos
    {
        private static Func<ROCManager, string, CelestialBody> ValidCelestialBody;
        private static Func<ROCManager, CelestialBody, string, bool> ValidCBBiome;
        static bool Prefix(ROCManager __instance)
        {
            List<ROCDefinition> rocDefinitions = __instance.rocDefinitions;
            ValidCelestialBody = AccessTools.MethodDelegate<Func<ROCManager, string, CelestialBody>>(AccessTools.Method(typeof(ROCManager), "ValidCelestialBody"));
            ValidCBBiome = AccessTools.MethodDelegate<Func<ROCManager, CelestialBody, string, bool>>(AccessTools.Method(typeof(ROCManager), "ValidCBBiome"));
            for (int num = rocDefinitions.Count - 1; num >= 0; num--)
            {
                for (int num2 = rocDefinitions[num].myCelestialBodies.Count - 1; num2 >= 0; num2--)
                {
                    CelestialBody celestialBody = ValidCelestialBody(__instance, rocDefinitions[num].myCelestialBodies[num2].name);
                    if (celestialBody.IsNullOrDestroyed())
                    {
                        Debug.LogWarningFormat("[ROCManager]: Invalid CelestialBody Name {0} on ROC Definition {1}. Removed entry.", rocDefinitions[num].myCelestialBodies[num2].name, rocDefinitions[num].type);
                        rocDefinitions[num].myCelestialBodies.RemoveAt(num2);
                        continue; // missing in stock code
                    }
                    else
                    {
                        for (int num3 = rocDefinitions[num].myCelestialBodies[num2].biomes.Count - 1; num3 >= 0; num3--)
                        {
                            if (!ValidCBBiome(__instance, celestialBody, rocDefinitions[num].myCelestialBodies[num2].biomes[num3]))
                            {
                                Debug.LogWarningFormat("[ROCManager]: Invalid Biome Name {0} for Celestial Body {1} on ROC Definition {2}. Removed entry.", rocDefinitions[num].myCelestialBodies[num2].biomes[num3], rocDefinitions[num].myCelestialBodies[num2].name, rocDefinitions[num].type);
                                rocDefinitions[num].myCelestialBodies[num2].biomes.RemoveAt(num3);
                            }
                        }
                    }
                    if (rocDefinitions[num].myCelestialBodies[num2].biomes.Count == 0) // ArgumentOutOfRangeException for myCelestialBodies[num2] when the previous if evaluate to true
                    {
                        Debug.LogWarningFormat("[ROCManager]: No Valid Biomes for Celestial Body {0} on ROC Definition {1}. Removed entry.", rocDefinitions[num].myCelestialBodies[num2].name, rocDefinitions[num].type);
                        rocDefinitions[num].myCelestialBodies.RemoveAt(num2);
                    }
                }
                if (rocDefinitions[num].myCelestialBodies.Count == 0)
                {
                    Debug.LogWarningFormat("[ROCManager]: No Valid Celestial Bodies on ROC Definition {0}. Removed entry.", rocDefinitions[num].type);
                    rocDefinitions.RemoveAt(num);
                }
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(PQSLandControl), "OnVertexBuildHeight")]
    public static class PQSLandControl_OnVertexBuildHeight
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo PQSMod_sphere_field = AccessTools.Field(typeof(PQSMod), nameof(PQSMod.sphere));
            FieldInfo PQS_sx_field = AccessTools.Field(typeof(PQS), nameof(PQS.sx));
            MethodInfo GetLongitudeFromSX_method = AccessTools.Method(typeof(PQSLandControl_OnVertexBuildHeight), nameof(GetLongitudeFromSX));

            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && ReferenceEquals(code[i].operand, PQSMod_sphere_field)
                    && code[i + 1].opcode == OpCodes.Ldfld && ReferenceEquals(code[i + 1].operand, PQS_sx_field))
                {
                    code[i + 1].opcode = OpCodes.Call;
                    code[i + 1].operand = GetLongitudeFromSX_method;
                }
            }

            return code;
        }

        /// <summary>
        /// Transform the from the sx [-0.25, 0.75] longitude range convention where [-0.25, 0] maps to [270°, 360°]
        /// and [0, 0.75] maps to [0°, 270°] into a linear [0,1] longitude range.
        /// </summary>
        public static double GetLongitudeFromSX(PQS sphere)
        {
            if (sphere.sx < 0.0)
                return sphere.sx + 1.0;
            return sphere.sx;
        }
    }

    [HarmonyPatch(typeof(SpaceCenterCamera2), "Start")]
    public static class SpaceCenterCamera2_Start
    {
        static bool Prefix(SpaceCenterCamera2 __instance)
        {
            if (__instance.pqsName.Equals("Kerbin"))
            {
                __instance.pqsName = RuntimeUtility.RuntimeUtility.KopernicusConfig.HomeWorldName;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(GameSettings), "WriteCfg")]
    public static class GameSettings_WriteCfg
    {
        static bool Prefix(GameSettings __instance)
        {
            PQSCache.PQSPreset pqspreset = new PQSCache.PQSPreset();
            pqspreset.name = "Low";
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Kerbin", 4.0, 1, 8));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("KerbinOcean", 4.0, 1, 7));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Mun", 4.0, 1, 7));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Minmus", 4.0, 1, 6));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Bop", 4.0, 1, 6));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Duna", 4.0, 1, 7));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Eve", 4.0, 1, 7));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("EveOcean", 4.0, 1, 7));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Gilly", 4.0, 1, 6));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Ike", 4.0, 1, 7));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Laythe", 4.0, 1, 7));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("LaytheOcean", 4.0, 1, 7));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Moho", 4.0, 1, 7));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Tylo", 4.0, 1, 7));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Vall", 4.0, 1, 7));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Dres", 4.0, 1, 7));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Pol", 4.0, 1, 7));
            pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Eeloo", 4.0, 1, 7));
            PQSCache.PQSPreset pqspreset2 = new PQSCache.PQSPreset();
            pqspreset2.name = "Default";
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Kerbin", 6.0, 1, 9));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("KerbinOcean", 6.0, 1, 7));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Mun", 6.0, 1, 8));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Minmus", 6.0, 1, 6));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Bop", 6.0, 1, 6));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Duna", 6.0, 1, 8));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Eve", 6.0, 1, 9));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("EveOcean", 6.0, 1, 7));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Gilly", 6.0, 1, 6));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Ike", 6.0, 1, 6));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Laythe", 6.0, 1, 9));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("LaytheOcean", 6.0, 1, 7));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Moho", 6.0, 1, 8));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Tylo", 6.0, 1, 8));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Vall", 6.0, 1, 8));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Dres", 6.0, 1, 8));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Pol", 6.0, 1, 8));
            pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Eeloo", 6.0, 1, 8));
            PQSCache.PQSPreset pqspreset3 = new PQSCache.PQSPreset();
            pqspreset3.name = "High";
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Kerbin", 8.0, 1, 10));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("KerbinOcean", 8.0, 1, 7));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Mun", 8.0, 1, 9));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Minmus", 8.0, 1, 7));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Bop", 8.0, 1, 6));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Duna", 8.0, 1, 9));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Eve", 8.0, 1, 10));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("EveOcean", 8.0, 1, 7));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Gilly", 8.0, 1, 7));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Ike", 8.0, 1, 7));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Laythe", 8.0, 1, 10));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("LaytheOcean", 8.0, 1, 7));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Moho", 8.0, 1, 9));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Tylo", 8.0, 1, 9));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Vall", 8.0, 1, 9));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Dres", 8.0, 1, 9));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Pol", 8.0, 1, 9));
            pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Eeloo", 8.0, 1, 9));
            RuntimeUtility.RuntimeUtility.pqsLow = PQSCache.PresetList.presets[0];
            RuntimeUtility.RuntimeUtility.pqsDefault = PQSCache.PresetList.presets[1];
            RuntimeUtility.RuntimeUtility.pqsHigh = PQSCache.PresetList.presets[2];
            PQSCache.PresetList.presets[0] = pqspreset;
            PQSCache.PresetList.presets[1] = pqspreset2;
            PQSCache.PresetList.presets[2] = pqspreset3;
            return true;
        }
        static void Postfix(GameSettings __instance)
        {
            PQSCache.PresetList.presets[0] = RuntimeUtility.RuntimeUtility.pqsLow;
            PQSCache.PresetList.presets[1] = RuntimeUtility.RuntimeUtility.pqsDefault;
            PQSCache.PresetList.presets[2] = RuntimeUtility.RuntimeUtility.pqsHigh;
        }
    }
    [HarmonyPatch(typeof(Sun), "LateUpdate")]
    public static class DynamicShadowSettings_OnDestroy
    {
        static bool Prefix(Sun __instance)
        {
            __instance.sunDirection = (__instance.target.position - ScaledSpace.LocalToScaledSpace(__instance.sun.position)).normalized;
            if (MapView.MapIsEnabled)
            {
                __instance.sunRotationPrecision = __instance.sunRotationPrecisionMapView;
            }
            else
            {
                __instance.sunRotationPrecision = __instance.sunRotationPrecisionDefault;
            }
            __instance.sunRotation = __instance.sunDirection;
            __instance.sunRotation.x = Math.Round(__instance.sunRotation.x, __instance.sunRotationPrecision);
            __instance.sunRotation.y = Math.Round(__instance.sunRotation.y, __instance.sunRotationPrecision);
            __instance.sunRotation.z = Math.Round(__instance.sunRotation.z, __instance.sunRotationPrecision);
            __instance.transform.forward = __instance.sunRotation;
            __instance.sunFlare.brightness = __instance.brightnessMultiplier * __instance.brightnessCurve.Evaluate((float)(1.0 / (Vector3d.Distance(__instance.target.position, ScaledSpace.LocalToScaledSpace(__instance.sun.position)) / (__instance.AU * (double)ScaledSpace.InverseScaleFactor))));
            if (__instance.useLocalSpaceSunLight)
            {
                Vector3d position = ScaledSpace.ScaledToLocalSpace(__instance.target.position);
                __instance.mainBody = FlightGlobals.currentMainBody;
                if (__instance.mainBody != null)
                {
                    if (__instance.mainBody != __instance.sun)
                    {
                        __instance.targetAltitude = FlightGlobals.getAltitudeAtPos(position, __instance.mainBody);
                        if (__instance.targetAltitude < 0.0)
                        {
                            __instance.targetAltitude = 0.0;
                        }
                        __instance.horizonAngle = Math.Acos(__instance.mainBody.Radius / (__instance.mainBody.Radius + __instance.targetAltitude));
                        __instance.horizonScalar = -Mathf.Sin((float)__instance.horizonAngle);
                        __instance.dayNightRatio = 1f - Mathf.Abs(__instance.horizonScalar);
                        __instance.fadeStartAtAlt = __instance.horizonScalar + __instance.fadeStart * __instance.dayNightRatio;
                        __instance.fadeEndAtAlt = __instance.horizonScalar - __instance.fadeEnd * __instance.dayNightRatio;
                        __instance.localTime = Vector3.Dot(-FlightGlobals.getUpAxis(position), __instance.transform.forward);
                        __instance.lgt.intensity = Mathf.Lerp(0f, __instance.scaledSunLight.intensity, Mathf.InverseLerp(__instance.fadeEndAtAlt, __instance.fadeStartAtAlt, __instance.localTime));
                        return false;
                    }
                }
                __instance.localTime = 1f;
                __instance.lgt.intensity = __instance.scaledSunLight.intensity;
            }
            return false;
        }
    }
}
