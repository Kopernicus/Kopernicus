﻿/**
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

using Contracts;
using Expansions;
using Kopernicus.Components;
using Kopernicus.ConfigParser;
using Kopernicus.Configuration;
using Kopernicus.Constants;
using KSP.Localization;
using KSP.UI;
using KSP.UI.Screens;
using KSP.UI.Screens.Mapview;
using KSP.UI.Screens.Mapview.MapContextMenuOptions;
using KSP.UI.Screens.Settings.Controls;
using ModularFI;
using SentinelMission;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Kopernicus
{
    public class Node<T>
    {
        public T data;
        public Node<T> parent;
        public List<Node<T>> children;

        public void AddChildren(Node<T> obj)
        {
            if (obj == this)
                return;
            obj.parent = this;
            if (children == null)
                children = new List<Node<T>>();
            children.Add(obj);
        }
        public void RemoveChildren(Node<T> obj)
        {
            obj.parent = null;
            if (children == null || children.Count == 0)
            {
                children = new List<Node<T>>();
                return;
            }
            children.Remove(obj);
        }
        public void AddToParent(Node<T> parent)
        {
            if (parent == this)
                return;
            if (parent.children == null)
                parent.children = new List<Node<T>>();
            this.parent = parent;
            parent.children.Add(this);
        }
    }
}

namespace Kopernicus.RuntimeUtility
{
    // Mod runtime utilities
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RuntimeUtility : MonoBehaviour
    {
        private static int setPQSTimer = 0;
        private static int hillSphereRecomputeTimer = 0;
        public static PQSCache.PQSPreset pqsLow;
        public static PQSCache.PQSPreset pqsDefault;
        public static PQSCache.PQSPreset pqsHigh;

        private static bool shadowsFixed = false;
        //old mockbody for compat
        public static CelestialBody mockBody = null;
        //Plugin Path finding logic
        private static string pluginPath;
        public static string PluginPath
        {
            get
            {
                if (ReferenceEquals(null, pluginPath))
                {
                    string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                    UriBuilder uri = new UriBuilder(codeBase);
                    pluginPath = Uri.UnescapeDataString(uri.Path);
                    pluginPath = Path.GetDirectoryName(pluginPath);
                }
                return pluginPath;
            }
        }
        public static ConfigReader KopernicusConfig = new Kopernicus.Configuration.ConfigReader();
        // Awake() - flag this class as don't destroy on load and register delegates
        [SuppressMessage("ReSharper", "ConvertClosureToMethodGroup")]
        private void Awake()
        {
            // Don't run if Kopernicus isn't compatible
            if (!CompatibilityChecker.IsCompatible())
            {
                Destroy(this);
                return;
            }

            // Make sure the runtime utility isn't killed
            DontDestroyOnLoad(this);
            // Init the runtime logging
            new Logger("Kopernicus.Runtime", true).SetAsActive();
            // Add handlers
            GameEvents.OnMapEntered.Add(() => OnMapEntered());
            GameEvents.onLevelWasLoaded.Add(s => OnLevelLoaded(s));
            GameEvents.onProtoVesselLoad.Add(d => TransformBodyReferencesOnLoad(d));
            GameEvents.onProtoVesselSave.Add(d => TransformBodyReferencesOnSave(d));
            // Add Callback only if necessary
            if (KopernicusConfig.HandleHomeworldAtmosphericUnitDisplay)
            {
                if (FlightGlobals.GetBodyByName(RuntimeUtility.KopernicusConfig.HomeWorldName).atmospherePressureSeaLevel != 101.324996948242)
                    KbApp_PlanetParameters.CallbackAfterActivate += CallbackAfterActivate;
            }

            // Log
            Logger.Default.Log("[Kopernicus] RuntimeUtility Started");
            Logger.Default.Flush();
        }

        private bool CallbackAfterActivate(KbApp_PlanetParameters kbapp, MapObject target)
        {
            kbapp.appFrame.scrollList.Clear(true);
            kbapp.cascadingList = Object.Instantiate(kbapp.cascadingListPrefab);
            kbapp.cascadingList.Setup(kbapp.appFrame.scrollList);
            kbapp.cascadingList.transform.SetParent(base.transform, false);
            UIListItem header = kbapp.cascadingList.CreateHeader(Localizer.Format("#autoLOC_462403"), out Button button, true);
            kbapp.cascadingList.ruiList.AddCascadingItem(header, kbapp.cascadingList.CreateFooter(), kbapp.CreatePhysicalCharacteristics(), button, -1);
            header = kbapp.cascadingList.CreateHeader(Localizer.Format("#autoLOC_462406"), out button, true);
            kbapp.cascadingList.ruiList.AddCascadingItem(header, kbapp.cascadingList.CreateFooter(), CreateAtmosphericCharacteristics(target.celestialBody, kbapp.cascadingList), button, -1);

            return true;
        }

        private List<UIListItem> CreateAtmosphericCharacteristics(CelestialBody currentBody, GenericCascadingList cascadingList)
        {
            GenericCascadingList genericCascadingList = cascadingList;
            Boolean atmosphere = currentBody.atmosphere && currentBody.atmospherePressureSeaLevel > 0;

            String key = Localizer.Format("#autoLOC_462448");
            String template = atmosphere ? "#autoLOC_439855" : "#autoLOC_439856";
            UIListItem item = genericCascadingList.CreateBody(key, "<color=#b8f4d1>" + Localizer.Format((string)template) + "</color>");

            List<UIListItem> list = new List<UIListItem>();
            list.Add(item);

            if (atmosphere)
            {
                item = cascadingList.CreateBody(Localizer.Format("#autoLOC_462453"), "<color=#b8f4d1>" + KSPUtil.LocalizeNumber(currentBody.atmosphereDepth, "N0") + " " + Localizer.Format("#autoLOC_7001411") + "</color>");
                list.Add(item);
                item = cascadingList.CreateBody(Localizer.Format("#autoLOC_462456"), "<color=#b8f4d1>" + KSPUtil.LocalizeNumber(currentBody.atmospherePressureSeaLevel / 101.324996948242, "0.#####") + " " + Localizer.Format("#autoLOC_7001419") + "</color>");
                list.Add(item);
                item = cascadingList.CreateBody(Localizer.Format("#autoLOC_462459"), "<color=#b8f4d1>" + KSPUtil.LocalizeNumber(currentBody.atmosphereTemperatureSeaLevel, "0.##") + " " + Localizer.Format("#autoLOC_7001406") + "</color>");
                list.Add(item);
            }

            return list;
        }

        // Execute MainMenu functions
        private void Start()
        {
            RemoveUnselectableObjects();
            ApplyLaunchSitePatches();
            ApplyMusicAltitude();
            ApplyInitialTarget();
            ApplyOrbitPatches();
            ApplyStarPatchSun();
            ApplyFlagFixes();

            for (Int32 i = 0; i < PSystemManager.Instance.localBodies.Count; i++)
            {
                ApplyStarPatches(PSystemManager.Instance.localBodies[i]);
                if (!PSystemManager.Instance.localBodies[i].Equals(PSystemManager.Instance.systemPrefab.rootBody.celestialBody) && (KopernicusConfig.RecomputeSOIAndHillSpheres))
                {
                    try
                    {
                        PSystemManager.Instance.localBodies[i].sphereOfInfluence = PSystemManager.Instance.localBodies[i].orbit.semiMajorAxis * Math.Pow(PSystemManager.Instance.localBodies[i].Mass / PSystemManager.Instance.localBodies[i].orbit.referenceBody.Mass, 0.4);
                        PSystemManager.Instance.localBodies[i].hillSphere = PSystemManager.Instance.localBodies[i].orbit.semiMajorAxis * (1 - PSystemManager.Instance.localBodies[i].orbit.eccentricity) * Math.Pow(PSystemManager.Instance.localBodies[i].Mass / (3 * (PSystemManager.Instance.localBodies[i].orbit.referenceBody.Mass + PSystemManager.Instance.localBodies[i].Mass)), 1.0 / 3.0);
                    }
                    catch { }
                }
            }

            CalculateHomeBodySMA();
        }

        // Stuff
        private void LateUpdate()
        {
            FixZooming();
            ApplyRnDPatches();
            Force3DRendering();
            UpdatePresetNames();
            ApplyMapTargetPatches();
            FixFlickeringOrbitLines();
            ApplyOrbitIconCustomization();

            // Apply changes for all bodies
            for (Int32 i = 0; i < PSystemManager.Instance.localBodies.Count; i++)
            {
                ApplyOrbitVisibility(PSystemManager.Instance.localBodies[i]);
                AtmosphereLightPatch(PSystemManager.Instance.localBodies[i]);
            }
        }
        // Run patches every time a new scene was loaded
        private void OnLevelLoaded(GameScenes scene)
        {
            PatchFlightIntegrator();
            PatchTimeOfDayAnimation();
            StartCoroutine(CallbackUtil.DelayedCallback(3, FixFlags));
            PatchContracts();
            shadowsFixed = false;
        }

        private static void FixShadows()
        {
            try
            {
                GameObject.Destroy(DynamicShadowSettings.Instance);
                DynamicShadowSettings.Instance = null;
            }
            catch
            {
                //dont need to do this then
            }
            shadowsFixed = true;
        }

        private void Update()
        {
            if (RuntimeUtility.KopernicusConfig.PrincipiaFriendlySOIComputation)
            {
                RuntimeUtility.hillSphereRecomputeTimer++;
            }
            if (hillSphereRecomputeTimer > 8725)
            {
                hillSphereRecomputeTimer = 0;
                for (Int32 i = 0; i < PSystemManager.Instance.localBodies.Count; i++)
                {
                    if (!PSystemManager.Instance.localBodies[i].Equals(PSystemManager.Instance.systemPrefab.rootBody.celestialBody))
                    {
                        try
                        {
                            double newSOI = PSystemManager.Instance.localBodies[i].orbit.semiMajorAxis * Math.Pow(PSystemManager.Instance.localBodies[i].Mass / PSystemManager.Instance.localBodies[i].orbit.referenceBody.Mass, 0.4);
                            double newHS = PSystemManager.Instance.localBodies[i].orbit.semiMajorAxis * (1 - PSystemManager.Instance.localBodies[i].orbit.eccentricity) * Math.Pow(PSystemManager.Instance.localBodies[i].Mass / (3 * (PSystemManager.Instance.localBodies[i].orbit.referenceBody.Mass + PSystemManager.Instance.localBodies[i].Mass)), 1.0 / 3.0);
                            double newHSvsOld = PSystemManager.Instance.localBodies[i].hillSphere / newHS;
                            double newSOIvsOld = PSystemManager.Instance.localBodies[i].sphereOfInfluence / newSOI;
                            if ((newSOIvsOld > 1.05) || (newSOIvsOld < 0.95))
                            {
                                PSystemManager.Instance.localBodies[i].sphereOfInfluence = newSOI;
                            }
                            if ((newHSvsOld > 1.05) || (newHSvsOld < 0.95))
                            {
                                PSystemManager.Instance.localBodies[i].hillSphere = newHS;
                            }
                        }
                        catch { }
                    }
                }
            }
            if (HighLogic.LoadedScene.Equals(GameScenes.SETTINGS) || HighLogic.LoadedScene.Equals(GameScenes.MAINMENU))
            {
                setPQSTimer++;
                if (setPQSTimer > 120)
                {
                    setPQSTimer = 0;
                    if (!PQSCache.PresetList.preset.Equals(Kopernicus.RuntimeUtility.RuntimeUtility.KopernicusConfig.SelectedPQSQuality))
                    {
                        Kopernicus.RuntimeUtility.RuntimeUtility.KopernicusConfig.SelectedPQSQuality = PQSCache.PresetList.preset;
                        PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "Kopernicus", "Kopernicus", "You have changed the Terrain Detail setting.  Do note that can slightly change terrain altitudes, potentially affecting landed vessels!  Revert this setting back if unsure.", "OK", true, UISkinManager.GetSkin("MainMenuSkin"));
                    }
                }
            }
            if (!shadowsFixed)
            {
                FixShadows();
            }
        }

        // Transforms body references in the save games
        private static void TransformBodyReferencesOnLoad(GameEvents.FromToAction<ProtoVessel, ConfigNode> data)
        {
            // Check if the config node is null
            if (data.to == null)
            {
                return;
            }
            ConfigNode orbit = data.to.GetNode("ORBIT");
            String bodyIdent = orbit.GetValue("IDENT");
            CelestialBody body = UBI.GetBody(bodyIdent);
            if (body == null)
            {
                return;
            }
            orbit.SetValue("REF", body.flightGlobalsIndex);
        }

        // Transforms body references in the save games
        private static void TransformBodyReferencesOnSave(GameEvents.FromToAction<ProtoVessel, ConfigNode> data)
        {
            // Save the reference to the real body
            if (data.to == null)
            {
                return;
            }
            ConfigNode orbit = data.to.GetNode("ORBIT");
            CelestialBody body = PSystemManager.Instance.localBodies.FirstOrDefault(b => b.flightGlobalsIndex == data.from.orbitSnapShot.ReferenceBodyIndex);
            if (body == null)
            {
                return;
            }
            orbit.AddValue("IDENT", UBI.GetUBI(body));
        }

        // Removes unselectable map targets from the list
        private static void RemoveUnselectableObjects()
        {
            if (Templates.MapTargets == null)
            {
                Templates.MapTargets = PlanetariumCamera.fetch.targets;
            }

            PlanetariumCamera.fetch.targets = Templates.MapTargets.Where(m =>
                m.celestialBody != null && m.celestialBody.Get("selectable", true)).ToList();
        }

        // Apply the star patch to the center body
        private static void ApplyStarPatchSun()
        {
            // Sun
            GameObject gob = Sun.Instance.gameObject;
            KopernicusStar star = gob.AddComponent<KopernicusStar>();
            if (!shadowsFixed)
            {
                FixShadows();
            }
            Utility.CopyObjectFields(Sun.Instance, star, false);
            DestroyImmediate(Sun.Instance);
            Sun.Instance = star;

            KopernicusStar.CelestialBodies =
                new Dictionary<CelestialBody, KopernicusStar>
                {
                    {
                        star.sun, star
                    }
                };

            // SunFlare
            gob = SunFlare.Instance.gameObject;
            KopernicusSunFlare flare = gob.AddComponent<KopernicusSunFlare>();
            gob.name = star.sun.name;
            Utility.CopyObjectFields(SunFlare.Instance, flare, false);
            DestroyImmediate(SunFlare.Instance);
            SunFlare.Instance = star.lensFlare = flare;
        }

        [SuppressMessage("ReSharper", "RedundantCast")]
        private static void ApplyStarPatches(CelestialBody body)
        {
            if (body.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length <= 0 || body.flightGlobalsIndex == 0)
            {
                return;
            }

            GameObject starObj = UnityEngine.Object.Instantiate(Sun.Instance.gameObject, Sun.Instance.transform.parent, true);
            KopernicusStar star = starObj.GetComponent<KopernicusStar>();
            star.sun = body;
            starObj.name = body.name;
            starObj.transform.localPosition = Vector3.zero;
            starObj.transform.localRotation = Quaternion.identity;
            starObj.transform.localScale = Vector3.one;
            starObj.transform.SetPositionAndRotation(body.position, body.rotation);

            KopernicusStar.CelestialBodies.Add(star.sun, star);

            GameObject flareObj = UnityEngine.Object.Instantiate(SunFlare.Instance.gameObject, SunFlare.Instance.transform.parent, true);
            KopernicusSunFlare flare = flareObj.GetComponent<KopernicusSunFlare>();
            star.lensFlare = flare;
            flareObj.name = body.name;
            flareObj.transform.localPosition = Vector3.zero;
            flareObj.transform.localRotation = Quaternion.identity;
            flareObj.transform.localScale = Vector3.one;
            flareObj.transform.SetPositionAndRotation(body.position, body.rotation);
        }

        private static void CalculateHomeBodySMA()
        {
            CelestialBody homeBody = FlightGlobals.GetBodyByName(RuntimeUtility.KopernicusConfig.HomeWorldName);
            if (homeBody == null)
            {
                return;
            }

            while (KopernicusStar.Stars.All(s => s.sun != homeBody.referenceBody) && homeBody.referenceBody != null)
            {
                homeBody = homeBody.referenceBody;
            }

            KopernicusStar.HomeBodySMA = homeBody.orbit.semiMajorAxis;
        }

        private static void ApplyLaunchSitePatches()
        {
            if (!ExpansionsLoader.IsExpansionInstalled("MakingHistory"))
            {
                return;
            }

            PQSCity2[] cities = FindObjectsOfType<PQSCity2>();
            for (Int32 i = 0; i < Templates.RemoveLaunchSites.Count; i++)
            {
                String site = Templates.RemoveLaunchSites[i];

                // Remove the launch site from the list if it exists
                if (PSystemSetup.Instance.LaunchSites.Any(s => s.name == site))
                {
                    PSystemSetup.Instance.RemoveLaunchSite(site);
                }

                PQSCity2 city = cities.FirstOrDefault(c =>
                {
                    GameObject o;
                    return (o = c.gameObject).name == site || o.name == site + "(Clone)";
                });

                // Kill the PQSCity if it exists
                if (city != null)
                {
                    Destroy(city.gameObject);
                }
            }

            // PSystemSetup.RemoveLaunchSite does not remove launch sites from PSystemSetup.StockLaunchSites
            // Currently an ArgumentOutOfRangeException can result when they are different lists because of a bug in stock code
            try
            {
                FieldInfo stockLaunchSitesField = typeof(PSystemSetup).GetField("stocklaunchsites", BindingFlags.NonPublic | BindingFlags.Instance);
                if (stockLaunchSitesField == null)
                {
                    return;
                }
                LaunchSite[] stockLaunchSites = (LaunchSite[])stockLaunchSitesField.GetValue(PSystemSetup.Instance);
                LaunchSite[] updateStockLaunchSites = stockLaunchSites.Where(site => !Templates.RemoveLaunchSites.Contains(site.name)).ToArray();
                if (stockLaunchSites.Length != updateStockLaunchSites.Length)
                {
                    stockLaunchSitesField.SetValue(PSystemSetup.Instance, updateStockLaunchSites);
                }
            }
            catch (Exception ex)
            {
                Logger.Default.Log("Failed to remove launch sites from 'stockLaunchSites' field. Exception information follows.");
                Logger.Default.LogException(ex);
            }
        }

        // Apply the home atmosphere height as the altitude where space music starts
        private static void ApplyMusicAltitude()
        {
            // Update Music Logic
            if (MusicLogic.fetch == null)
            {
                return;
            }

            if (FlightGlobals.fetch == null)
            {
                return;
            }

            if (FlightGlobals.GetBodyByName(RuntimeUtility.KopernicusConfig.HomeWorldName) == null)
            {
                return;
            }

            MusicLogic.fetch.flightMusicSpaceAltitude = FlightGlobals.GetBodyByName(RuntimeUtility.KopernicusConfig.HomeWorldName).atmosphereDepth;
        }

        // Update the initialTarget of the tracking station
        private static void ApplyInitialTarget()
        {
            CelestialBody home = null;
            try
            {
                home = FlightGlobals.GetBodyByName(RuntimeUtility.KopernicusConfig.HomeWorldName);
            }
            catch
            {
                home = FlightGlobals.GetBodyByName("Kerbin");
            }
            ScaledMovement movement = home.scaledBody.GetComponentInChildren<ScaledMovement>();
            PlanetariumCamera.fetch.initialTarget = movement;
        }

        // Apply PostSpawnOrbit patches
        private void ApplyOrbitPatches()
        {
            // Bodies
            Dictionary<String, KeyValuePair<CelestialBody, CelestialBody>> fixes = new Dictionary<String, KeyValuePair<CelestialBody, CelestialBody>>();

            for (Int32 i = 0; i < PSystemManager.Instance.localBodies.Count; i++)
            {
                CelestialBody body = PSystemManager.Instance.localBodies[i];

                // Post spawn patcher
                if (!body.Has("orbitPatches"))
                {
                    continue;
                }

                ConfigNode orbitNode = body.Get<ConfigNode>("orbitPatches");
                OrbitLoader loader = new OrbitLoader(body);
                CelestialBody oldRef = body.referenceBody;
                Parser.LoadObjectFromConfigurationNode(loader, orbitNode, "Kopernicus");
                oldRef.orbitingBodies.Remove(body);

                if (body.referenceBody == null)
                {
                    // Log the exception
                    Debug.Log("Exception: PostSpawnOrbit reference body for \"" + body.name +
                              "\" could not be found. Missing body name is \"" + loader.ReferenceBody + "\".");

                    // Open the Warning popup
                    Injector.DisplayWarning();
                    Destroy(this);
                    return;
                }

                fixes.Add(body.transform.name,
                    new KeyValuePair<CelestialBody, CelestialBody>(oldRef, body.referenceBody));
                body.referenceBody.orbitingBodies.Add(body);
                body.referenceBody.orbitingBodies =
                    body.referenceBody.orbitingBodies.OrderBy(cb => cb.orbit.semiMajorAxis).ToList();
                body.orbit.Init();

                //body.orbitDriver.UpdateOrbit();

                // Calculations
                if (!body.Has("sphereOfInfluence"))
                {
                    body.sphereOfInfluence = body.orbit.semiMajorAxis * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 0.4);
                }

                if (!body.Has("hillSphere"))
                {
                    body.hillSphere = body.orbit.semiMajorAxis * (1 - body.orbit.eccentricity) * Math.Pow(body.Mass / (3 * (body.orbit.referenceBody.Mass + body.Mass)), 1.0 / 3.0);
                }

                if (!body.solarRotationPeriod)
                {
                    continue;
                }

                Double rotationPeriod = body.rotationPeriod;
                Double orbitalPeriod = body.orbit.period;
                body.rotationPeriod = rotationPeriod * orbitalPeriod / (orbitalPeriod + rotationPeriod);
            }

            // Update the order in the tracking station
            List<MapObject> trackingstation = new List<MapObject>();
            Utility.DoRecursive(PSystemManager.Instance.localBodies[0], cb => cb.orbitingBodies, cb =>
            {
                trackingstation.Add(PlanetariumCamera.fetch.targets.Find(t => t.celestialBody == cb));
            });
            PlanetariumCamera.fetch.targets = trackingstation.Where(m => m != null).ToList();

            // Undo stuff
            foreach (CelestialBody b in PSystemManager.Instance.localBodies.Where(b => b.Has("orbitPatches")))
            {
                String transformName = b.transform.name;
                fixes[transformName].Value.orbitingBodies.Remove(b);
                fixes[transformName].Key.orbitingBodies.Add(b);
                fixes[transformName].Key.orbitingBodies = fixes[transformName].Key.orbitingBodies
                    .OrderBy(cb => cb.orbit.semiMajorAxis).ToList();
            }
        }

        // Status
        private Boolean _fixZoomingIsDone;

        // Fix the Zooming-Out bug
        private void FixZooming()
        {
            if (HighLogic.LoadedSceneHasPlanetarium && MapView.fetch && !_fixZoomingIsDone)
            {
                // Fix the bug via switching away from Home and back immediately.
                // TODO: Check if this still happens
                PlanetariumCamera.fetch.SetTarget(PlanetariumCamera.fetch.targets[
                    (PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) + 1) %
                    PlanetariumCamera.fetch.targets.Count]);
                PlanetariumCamera.fetch.SetTarget(PlanetariumCamera.fetch.targets[
                    PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) - 1 +
                    (PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) - 1 >= 0
                        ? 0
                        : PlanetariumCamera.fetch.targets.Count)]);

                // Terminate for the moment.
                _fixZoomingIsDone = true;
            }

            // Set custom Zoom-In limits
            if (HighLogic.LoadedScene != GameScenes.TRACKSTATION && !MapView.MapIsEnabled)
            {
                return;
            }

            MapObject target = PlanetariumCamera.fetch.target;
            if (!target || !target.celestialBody)
            {
                return;
            }

            CelestialBody body = target.celestialBody;
            if (body.Has("maxZoom"))
            {
                PlanetariumCamera.fetch.minDistance = body.Get<Single>("maxZoom");
            }
            else
            {
                PlanetariumCamera.fetch.minDistance = 10;
            }
        }

        // Whether to apply RD changes next frame
        private Boolean _rdIsDone;

        // Remove the thumbnail for barycenters in the RD and patch name changes
        private void ApplyRnDPatches()
        {
            // Only run in SpaceCenter
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                // Done
                if (_rdIsDone)
                {
                    return;
                }

                // Waaah
                foreach (RDArchivesController controller in Resources.FindObjectsOfTypeAll<RDArchivesController>())
                {
                    controller.gameObject.AddOrGetComponent<RnDFixer>();
                }

                _rdIsDone = true;
            }
            else
            {
                _rdIsDone = false;
            }
        }

        // If 3D rendering of orbits is forced, enable it
        private static void Force3DRendering()
        {
            if (!Templates.Force3DOrbits)
            {
                return;
            }

            if (MapView.fetch)
            {
                MapView.fetch.max3DlineDrawDist = Single.MaxValue;
                GameSettings.MAP_MAX_ORBIT_BEFORE_FORCE2D = Int32.MaxValue;
            }
        }

        // The preset names
        private String[] _details;

        // Update the names of the presets in the settings dialog
        private void UpdatePresetNames()
        {
            if (HighLogic.LoadedScene != GameScenes.SETTINGS)
            {
                return;
            }

            foreach (SettingsTerrainDetail detail in Resources.FindObjectsOfTypeAll<SettingsTerrainDetail>())
            {
                detail.displayStringValue = true;
                detail.stringValues = _details ?? (_details = Templates.PresetDisplayNames.ToArray());
            }
        }

        // Reflection fields for orbit targeting
        private FieldInfo[] _fields;

        // Removes the target buttons for barycenters in the MapView
        [SuppressMessage("ReSharper", "AccessToStaticMemberViaDerivedType")]
        private void ApplyMapTargetPatches()
        {
            if (!MapView.MapIsEnabled)
            {
                return;
            }

            // Grab the needed fields for reflection
            if (_fields == null)
            {
                FieldInfo modeField = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(f => f.FieldType.IsEnum && f.FieldType.IsNested);
                FieldInfo contextField = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(f => f.FieldType == typeof(MapContextMenu));
                FieldInfo castField = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(f => f.FieldType == typeof(OrbitRenderer.OrbitCastHit));

                _fields = new[]
                {
                    modeField, contextField, castField
                };
            }

            // Remove buttons in map view for barycenters
            if (!FlightGlobals.ActiveVessel)
            {
                return;
            }

            OrbitTargeter targeter = FlightGlobals.ActiveVessel.orbitTargeter;
            if (!targeter)
            {
                return;
            }

            Int32 mode = (Int32)_fields[0].GetValue(targeter);
            if (mode != 2)
            {
                return;
            }
            OrbitRenderer.OrbitCastHit cast = (OrbitRenderer.OrbitCastHit)_fields[2].GetValue(targeter);

            CelestialBody body = PSystemManager.Instance.localBodies.Find(b =>
                cast.or != null && cast.or.discoveryInfo?.name != null && b.name == cast.or.discoveryInfo.name.Value);
            if (!body)
            {
                return;
            }

            if (!body.Has("barycenter") && body.Get("selectable", true))
            {
                return;
            }

            if (!cast.driver || cast.driver.Targetable == null)
            {
                return;
            }

            MapContextMenu context = MapContextMenu.Create(body.name, new Rect(0.5f, 0.5f, 300f, 50f), cast, () =>
            {
                _fields[0].SetValue(targeter, 0);
                _fields[1].SetValue(targeter, null);
            }, new SetAsTarget(cast.driver.Targetable, () => FlightGlobals.fetch.VesselTarget));
            _fields[1].SetValue(targeter, context);
        }

        private static void FixFlickeringOrbitLines()
        {
            if ((!(Versioning.version_minor < 9)) && (Versioning.version_minor < 11))
            {
                // Prevent the orbit lines from flickering in 1.9.1 and 1.10.1
                PlanetariumCamera.Camera.farClipPlane = 1e14f;
            }
        }
        // Whether to apply the customizations next frame
        private Boolean _orbitIconsReady;

        private void OnMapEntered()
        {
            _orbitIconsReady = true;
        }

        // The cache for the orbit icon objects
        private readonly Dictionary<CelestialBody, Sprite> _spriteCache = new Dictionary<CelestialBody, Sprite>();

        // Apply custom orbit icons.
        private void ApplyOrbitIconCustomization()
        {
            // Check if the patch is allowed to run
            if (!_orbitIconsReady)
            {
                return;
            }

            // Check if the UINodes are already spawned
            Boolean nodesReady = FlightGlobals.Bodies.Any(b => b.MapObject != null && b.MapObject.uiNode != null);
            if (!nodesReady)
            {
                return;
            }

            IEnumerable<CelestialBody> customOrbitalIcons = FlightGlobals.Bodies.Where(b =>
                b.MapObject != null && b.MapObject.uiNode != null && b.Has("iconTexture"));
            try
            {
                foreach (CelestialBody body in customOrbitalIcons)
                {
                    _spriteCache.TryGetValue(body, out Sprite sprite);
                    if (!sprite)
                    {
                        Texture2D texture = body.Get<Texture2D>("iconTexture");
                        sprite = Sprite.Create(
                            texture,
                            new Rect(0, 0, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f),
                            100,
                            1,
                            SpriteMeshType.Tight,
                            Vector4.zero
                        );
                        _spriteCache[body] = sprite;
                    }
                    body.MapObject.uiNode.SetIcon(sprite);
                }
            }
            catch
            {
                Debug.LogWarning("[KOPERNICUS]Unable to apply Orbit Icon customization, does path exist?");
            }
            _orbitIconsReady = false;
        }

        // Applies invisible orbits
        [SuppressMessage("ReSharper", "AccessToStaticMemberViaDerivedType")]
        private static void ApplyOrbitVisibility(CelestialBody body)
        {
            if (HighLogic.LoadedScene != GameScenes.TRACKSTATION &&
                (!HighLogic.LoadedSceneIsFlight || !MapView.MapIsEnabled))
            {
                return;
            }

            // Loop
            // Check for Renderer
            if (!body.orbitDriver)
            {
                return;
            }
            if (!body.orbitDriver.Renderer)
            {
                return;
            }

            // Apply Orbit mode changes
            if (body.Has("drawMode"))
            {
                body.orbitDriver.Renderer.drawMode = body.Get<OrbitRenderer.DrawMode>("drawMode");
            }

            // Apply Orbit icon changes
            if (body.Has("drawIcons"))
            {
                body.orbitDriver.Renderer.drawIcons = body.Get<OrbitRenderer.DrawIcons>("drawIcons");
            }
        }

        // Shader accessor for AtmosphereFromGround
        private readonly Int32 _lightDot = Shader.PropertyToID("_lightDot");

        // Use the brightest star as the AFG star
        private void AtmosphereLightPatch(CelestialBody body)
        {
            if ((!body.afg) || (!KopernicusStar.UseMultiStarLogic))
            {
                return;
            }

            GameObject star = KopernicusStar.GetBrightest(body).gameObject;
            Vector3 afgCamPosition = body.afg.mainCamera.transform.position;
            Vector3 distance = body.scaledBody.transform.position - afgCamPosition;
            body.afg.lightDot = Mathf.Clamp01(Vector3.Dot(distance, afgCamPosition - star.transform.position) * body.afg.dawnFactor);
            body.afg.GetComponent<Renderer>().sharedMaterial.SetFloat(_lightDot, body.afg.lightDot);
        }

        // Patch FlightIntegrator
        private static void PatchFlightIntegrator()
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                return;
            }
            Events.OnRuntimeUtilityPatchFI.Fire();
            ModularFlightIntegrator.RegisterCalculateSunBodyFluxOverride(KopernicusStar.SunBodyFlux);
            ModularFlightIntegrator.RegisterCalculateBackgroundRadiationTemperatureOverride(KopernicusHeatManager.RadiationTemperature);
        }

        private static void PatchContracts()
        {
            //Small Contract fixer to remove Sentinel Contracts
            if (ContractSystem.ContractTypes != null && !KopernicusConfig.UseKopernicusAsteroidSystem.ToLower().Equals("stock"))
            {
                if (ContractSystem.ContractTypes.Remove(typeof(SentinelContract)))
                {
                    Debug.Log("[Kopernicus] Due to selected asteroid spawner, SENTINEL Contracts are broken and have been scrubbed.");
                }
            }
            //Patch weights of contracts
            for (Int32 i = 0; i < PSystemManager.Instance.localBodies.Count; i++)
            {
                PatchStarReferences(PSystemManager.Instance.localBodies[i]);
                PatchContractWeight(PSystemManager.Instance.localBodies[i]);
            }
        }

        // Patch the KSC light animation
        private static void PatchTimeOfDayAnimation()
        {
            TimeOfDayAnimation[] animations = Resources.FindObjectsOfTypeAll<TimeOfDayAnimation>();
            if (KopernicusConfig.KSCLightsAlwaysOn)
            {
                for (Int32 i = 0; i < animations.Length; i++)
                {
                    animations[i].target = FlightGlobals.GetBodyByName(RuntimeUtility.KopernicusConfig.HomeWorldName).bodyTransform.parent;
                }
            }
            else
            {
                for (Int32 i = 0; i < animations.Length; i++)
                {
                    animations[i].target = KopernicusStar.GetBrightest(FlightGlobals.GetBodyByName(RuntimeUtility.KopernicusConfig.HomeWorldName)).transform;
                }
            }
        }

        // Patch various references to point to the brightest star
        private static void PatchStarReferences(CelestialBody body)
        {
            GameObject star = KopernicusStar.GetBrightest(body).gameObject;

            if (body.afg != null)
            {
                body.afg.sunLight = star;
            }
            if (body.scaledBody.GetComponents<MaterialSetDirection>() != null)
            {
                MaterialSetDirection[] MSDs = body.scaledBody.GetComponents<MaterialSetDirection>();
                foreach (MaterialSetDirection MSD in MSDs)
                {
                    MSD.target = star.transform;
                }
            }
            foreach (PQSMod_MaterialSetDirection msd in
                body.GetComponentsInChildren<PQSMod_MaterialSetDirection>(true))
            {
                msd.target = star.transform;
            }
        }

        // Contract Weight
        private static void PatchContractWeight(CelestialBody body)
        {
            if (ContractSystem.ContractWeights == null)
            {
                return;
            }

            if (!body.Has("contractWeight"))
            {
                return;
            }

            if (ContractSystem.ContractWeights.ContainsKey(body.name))
            {
                ContractSystem.ContractWeights[body.name] = body.Get<Int32>("contractWeight");
            }
            else
            {
                ContractSystem.ContractWeights.Add(body.name, body.Get<Int32>("contractWeight"));
            }
        }

        // Flag Fixer
        private void ApplyFlagFixes()
        {
            GameEvents.OnKSCFacilityUpgraded.Add(FixFlags);
            GameEvents.OnKSCStructureRepaired.Add(FixFlags);
        }

        private void FixFlags(DestructibleBuilding data)
        {
            FixFlags();
        }

        private void FixFlags(Upgradeables.UpgradeableFacility data0, int data1)
        {
            FixFlags();
        }

        private void FixFlags()
        {
            if (FlightGlobals.GetBodyByName(RuntimeUtility.KopernicusConfig.HomeWorldName) != null && FlightGlobals.GetBodyByName(RuntimeUtility.KopernicusConfig.HomeWorldName).pqsController != null)
            {
                PQSCity KSC = FlightGlobals
                    .GetBodyByName(RuntimeUtility.KopernicusConfig.HomeWorldName)
                    .pqsController
                    .GetComponentsInChildren<PQSCity>(true)?
                    .FirstOrDefault(p => p.name == "KSC");

                SkinnedMeshRenderer[] flags = KSC
                    .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .Where(smr => smr.name == "Flag")?
                    .ToArray();

                flags.ToList().ForEach(flag =>
                {
                    flag.rootBone = flag
                        .rootBone
                        .parent
                        .gameObject
                        .GetChild("bn_upper_flag_a01")
                        .transform;
                });
            }
        }

        public static void WriteConfigIfNoneExists()
        {
            if (!File.Exists(PluginPath + "/../Config/Kopernicus_Config.cfg"))
            {
                UpdateConfig();
            }
        }

        public static void UpdateConfig()
        {
            if (File.Exists(PluginPath + "/../Config/Kopernicus_Config.cfg"))
            {
                File.Delete(PluginPath + "/../Config/Kopernicus_Config.cfg");
            }
            if (!File.Exists(PluginPath + "/../Config/Kopernicus_Config.cfg"))
            {
                Debug.Log("[Kopernicus] Writing out updated Kopernicus_Config.cfg");
                using (StreamWriter configFile = new StreamWriter(PluginPath + "/../Config/Kopernicus_Config.cfg"))
                {
                    configFile.WriteLine("// Kopernicus base configuration. Provides ability to flag things and set user options. Generates at defaults for stock settings and warnings config.");
                    configFile.WriteLine("// This file is rewritten/created when the game exits. This means any manual (not in the in-game GUI) edits made to this file while playing the game will not be preserved. Edit this file only with the game exited, please.");
                    configFile.WriteLine("// Some (but not all) of these settings can also be changed through the in-game GUI. Changes through the in-game GUI will be preserved.");
                    configFile.WriteLine("Kopernicus_config");
                    configFile.WriteLine("{");
                    configFile.WriteLine("	HomeWorldName = " + KopernicusConfig.HomeWorldName + " //String with the home bodies name. Allows for directly changing the home body to a differently named body. Default is Kerbin.");
                    configFile.WriteLine("	EnforceShaders = " + KopernicusConfig.EnforceShaders.ToString() + " //Boolean. Whether or not to force the user into EnforcedShaderLevel, not allowing them to change settings.");
                    configFile.WriteLine("	WarnShaders = " + KopernicusConfig.WarnShaders.ToString() + " //Boolean. Whether or not to warn the user with a message if not using EnforcedShaderLevel.");
                    configFile.WriteLine("	EnforcedShaderLevel = " + KopernicusConfig.EnforcedShaderLevel.ToString() + " //Integer. A number defining the enforced shader level for the above parameters. 0=Low,1=Medium,2=High,3=Ultra.");
                    if ((!KopernicusConfig.UseKopernicusAsteroidSystem.ToLower().Equals("true")) && (!KopernicusConfig.UseKopernicusAsteroidSystem.ToLower().Equals("false")) && (!KopernicusConfig.UseKopernicusAsteroidSystem.ToLower().Equals("stock")))
                    {
                        configFile.WriteLine("	UseKopernicusAsteroidSystem = True //String with three valid values, True,False, and Stock. True means use the old customizable Kopernicus asteroid generator with no comet support (many packs use this so it's the default). False means don't generate anything, or wait for an external generator. Stock means use the internal games generator, which supports comets, but usually only works well in stock based systems with Dres and Kerbin present.");
                    }
                    else
                    {
                        configFile.WriteLine("	UseKopernicusAsteroidSystem = " + KopernicusConfig.UseKopernicusAsteroidSystem + " //String with three valid values, True,False, and Stock. True means use the old customizable Kopernicus asteroid generator with no comet support (many packs use this so it's the default). False means don't generate anything, or wait for an external generator. Stock means use the internal games generator, which supports comets, but usually only works well in stock based systems with Dres and Kerbin present.");
                    }
                    configFile.WriteLine("	SolarRefreshRate = " + KopernicusConfig.SolarRefreshRate.ToString() + " //Integer. A number defining the number of seconds between EC calculations when using the multistar cfg file. Can be used to finetune performance (higher runs faster). Otherwise irrelevant.");
                    configFile.WriteLine("	EnableKopernicusShadowManager = " + KopernicusConfig.EnableKopernicusShadowManager.ToString() + " //Boolean. Whether or not to run the Internal Kopernicus Shadow System. True by default, users using mods that do their own shadows (scatterer etc) may want to disable this to save a small bit of performance.");
                    configFile.WriteLine("	ShadowRangeCap = " + KopernicusConfig.ShadowRangeCap + " //Integer. A number defining the maximum distance at which shadows may be cast. Lower numbers yield less shadow cascading artifacts, but higher numbers cast shadows farther. Default at 50000 is an approximation of stock. Only works if EnableKopernicusShadowManager is true.");
                    configFile.WriteLine("	DisableMainMenuMunScene = " + KopernicusConfig.DisableMainMenuMunScene.ToString() + " //Boolean. Whether or not to disable the Mun main menu scene. Only set to false if you want that scene back.");
                    configFile.WriteLine("	KSCLightsAlwaysOn = " + KopernicusConfig.KSCLightsAlwaysOn.ToString() + " //Boolean. Whether or not to force the KSC Lights to always be on. Default false.");
                    configFile.WriteLine("	UseOriginalKSC2 = " + KopernicusConfig.UseOriginalKSC2.ToString() + " //Boolean. Whether or not to force the original, uncompacted KSC2 to load.  Will not be editable in any form by Kopernicus.");
                    configFile.WriteLine("	HandleHomeworldAtmosphericUnitDisplay = " + KopernicusConfig.HandleHomeworldAtmosphericUnitDisplay.ToString() + " //Boolean. This is for calculating 1atm unit at home world. Normally should be true, but mods like PlanetaryInfoPlus may want to set this false.");
                    configFile.WriteLine("	UseIncorrectScatterDensityLogic = " + KopernicusConfig.UseIncorrectScatterDensityLogic.ToString() + " //Boolean. This is a compatability option for old modpacks that were built with the old (wrong) density logic in mind. Turn on if scatters seem too dense. Please do not use in new releases.");
                    configFile.WriteLine("	DisableFarAwayColliders = " + KopernicusConfig.DisableFarAwayColliders.ToString() + " //Boolean. Fix a raycast physics bug occuring in large systems, notably resulting in wheels and landing legs falling through the ground.");
                    configFile.WriteLine("	EnableAtmosphericExtinction = " + KopernicusConfig.EnableAtmosphericExtinction.ToString() + " //Whether to use built-in atmospheric extinction effect of lens flares. This is somewhat expensive - O(nlog(n)) on average.");
                    configFile.WriteLine("	UseStockMohoTemplate = " + KopernicusConfig.UseStockMohoTemplate.ToString() + " //Boolean. This uses the stock Moho template with the Mohole bug/feature. Planet packs may customize this as desired. Be aware disabling this disables the Mohole.");
                    configFile.WriteLine("	UseOnDemandLoader = " + KopernicusConfig.UseOnDemandLoader.ToString() + " //Boolean. Default False. Turning this on can save ram and thus improve perforamnce situationally but will break some mods requiring long distance viewing and also increase stutter.");
                    configFile.WriteLine("	UseRealWorldDensity = " + KopernicusConfig.UseRealWorldDensity.ToString() + " //Boolean. Default False. Turning this on will calculate realistic body gravity and densities for all or Kerbolar/stock bodies based on size of said body. Don't turn this on unless you understand what it does.");
                    configFile.WriteLine("	RecomputeSOIAndHillSpheres = " + KopernicusConfig.RecomputeSOIAndHillSpheres.ToString() + " //Boolean. Default False. Turning this on will recompute hill spheres and SOIs using standard math for bodies that have been modified for density in anyway by UseRealWorldDensity. Global effect/Not affected by LimitRWDensityToStockBodies. Leave alone if you don't understand.");
                    configFile.WriteLine("	PrincipiaFriendlySOIComputation = " + KopernicusConfig.PrincipiaFriendlySOIComputation.ToString() + " //Boolean. Default False. Turning this on will recompute hill spheres and SOIs using standard math constantly, updating them when they hit a deviation of more than 5%. This has a performance penalty, and is only useful for Principia. Leave alone if you don't understand.");
                    configFile.WriteLine("	LimitRWDensityToStockBodies = " + KopernicusConfig.LimitRWDensityToStockBodies.ToString() + " //Boolean. Default True. Turning this on will limit density corrections to stock/Kerbolar bodies only. Don't mess with this unless you understand what it does.");
                    configFile.WriteLine("	UseOlderRWScalingLogic = " + KopernicusConfig.UseOlderRWScalingLogic.ToString() + " //Boolean. Default False. Turning this on will use the old gas giant rescale logic that was less scientifically correct. Don't mess with this unless you understand what it does.");
                    configFile.WriteLine("	RescaleFactor = " + KopernicusConfig.RescaleFactor.ToString() + " //Float. Default 1.0. Set this to the rescale factor of your system if using UseRealWorldDensity, otherwise ignore.");
                    configFile.WriteLine("	RealWorldSizeFactor = " + KopernicusConfig.RealWorldSizeFactor.ToString() + " //Float. Default 10.625. This is the size the density multiplier considers a 'normal' real world system. Don't change unless you know what you are doing.");
                    configFile.WriteLine("	SelectedPQSQuality = " + PQSCache.PresetList.preset);
                    configFile.WriteLine("	SettingsWindowXcoord = " + KopernicusConfig.SettingsWindowXcoord.ToString());
                    configFile.WriteLine("	SettingsWindowYcoord = " + KopernicusConfig.SettingsWindowYcoord.ToString());
                    configFile.WriteLine("}");
                    configFile.Flush();
                    configFile.Close();
                }
            }
        }
        // Remove the Handlers
        private void OnDestroy()
        {
            UpdateConfig();
            GameEvents.OnMapEntered.Remove(OnMapEntered);
            GameEvents.onLevelWasLoaded.Remove(OnLevelLoaded);
            GameEvents.onProtoVesselLoad.Remove(TransformBodyReferencesOnLoad);
            GameEvents.onProtoVesselSave.Remove(TransformBodyReferencesOnSave);
        }
    }
}
