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
using System.Reflection;
using Contracts;
using Expansions;
using Kopernicus.Components;
using Kopernicus.ConfigParser;
using Kopernicus.Configuration;
using Kopernicus.Constants;
using KSP.UI;
using KSP.UI.Screens;
using KSP.UI.Screens.Mapview;
using KSP.UI.Screens.Mapview.MapContextMenuOptions;
using KSP.UI.Screens.Settings.Controls;
using ModularFI;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;
using Object = UnityEngine.Object;

namespace Kopernicus.RuntimeUtility
{
    // Mod runtime utilities
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RuntimeUtility : MonoBehaviour
    {
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
            GameEvents.onLevelWasLoaded.Add(s => OnLevelWasLoaded(s));
            GameEvents.onProtoVesselLoad.Add(d => TransformBodyReferencesOnLoad(d));
            GameEvents.onProtoVesselSave.Add(d => TransformBodyReferencesOnSave(d));

            // Add Callback only if necessary
            if (FlightGlobals.GetHomeBody().atmospherePressureSeaLevel != 101.324996948242)
                KbApp_PlanetParameters.CallbackAfterActivate += CallbackAfterActivate;

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
            }
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
        [SuppressMessage("ReSharper", "Unity.IncorrectMethodSignature")]
        private void OnLevelWasLoaded(GameScenes scene)
        {
            PatchFlightIntegrator();
            FixCameras();
            PatchTimeOfDayAnimation();
            StartCoroutine(CallbackUtil.DelayedCallback(3, FixFlags));

            for (Int32 i = 0; i < PSystemManager.Instance.localBodies.Count; i++)
            {
                PatchStarReferences(PSystemManager.Instance.localBodies[i]);
                PatchContractWeight(PSystemManager.Instance.localBodies[i]);
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
            Utility.CopyObjectFields(Sun.Instance, star, false);
            DestroyImmediate(Sun.Instance);
            Sun.Instance = star;

            KopernicusStar.CelestialBodies =
                new Dictionary<CelestialBody, KopernicusStar>
                {
                    { star.sun, star }
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
            starObj.transform.position = body.position;
            starObj.transform.rotation = body.rotation;

            KopernicusStar.CelestialBodies.Add(star.sun, star);

            GameObject flareObj = UnityEngine.Object.Instantiate(SunFlare.Instance.gameObject, SunFlare.Instance.transform.parent, true);
            KopernicusSunFlare flare = flareObj.GetComponent<KopernicusSunFlare>();
            star.lensFlare = flare;
            flareObj.name = body.name;
            flareObj.transform.localPosition = Vector3.zero;
            flareObj.transform.localRotation = Quaternion.identity;
            flareObj.transform.localScale = Vector3.one;
            flareObj.transform.position = body.position;
            flareObj.transform.rotation = body.rotation;
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

            if (FlightGlobals.GetHomeBody() == null)
            {
                return;
            }

            MusicLogic.fetch.flightMusicSpaceAltitude = FlightGlobals.GetHomeBody().atmosphereDepth;
        }

        // Update the initialTarget of the tracking station
        private static void ApplyInitialTarget()
        {
            CelestialBody home = PSystemManager.Instance.localBodies.Find(b => b.isHomeWorld);
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
                    body.sphereOfInfluence = body.orbit.semiMajorAxis *
                                             Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 0.4);
                }
              
                if (!body.Has("hillSphere"))
                {
                    body.hillSphere = body.orbit.semiMajorAxis * (1 - body.orbit.eccentricity) *
                                      Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 0.333333333333333);
                }
              
                if (!body.solarRotationPeriod)
                {
                   continue;
                }
       
                Double rotPeriod = Utility
                    .FindBody(PSystemManager.Instance.systemPrefab.rootBody, body.transform.name).celestialBody
                    .rotationPeriod;
                Double num1 = Math.PI * 2 * Math.Sqrt(Math.Pow(Math.Abs(body.orbit.semiMajorAxis), 3) /
                                                      body.orbit.referenceBody.gravParameter);
                body.rotationPeriod = rotPeriod * num1 / (num1 + rotPeriod);
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

            MapView.fetch.max3DlineDrawDist = Single.MaxValue;
            GameSettings.MAP_MAX_ORBIT_BEFORE_FORCE2D = Int32.MaxValue;
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

                _fields = new[] {modeField, contextField, castField};
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

            Int32 mode = (Int32) _fields[0].GetValue(targeter);
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
            // Prevent the orbit lines from flickering
            PlanetariumCamera.Camera.farClipPlane = 1e14f;
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

        // Use the nearest star as the AFG star
        private void AtmosphereLightPatch(CelestialBody body)
        {
            if (!body.afg)
            {
                return;
            }

            GameObject star = KopernicusStar.GetNearest(body).gameObject;
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

        // Fix the Space Center Cameras
        private static void FixCameras()
        {
            // Only run in the space center or the editor
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER && !HighLogic.LoadedSceneIsEditor)
            {
                return;
            }

            // Get the parental body
            CelestialBody body = Planetarium.fetch != null ? Planetarium.fetch.Home : FlightGlobals.Bodies.Find(b => b.isHomeWorld);

            // If there's no body, exit.
            if (body == null)
            {
                Logger.Active.Log("[Kopernicus] Couldn't find the parental body!");
                return;
            }

            // Get the KSC object
            PQSCity ksc = body.pqsController.GetComponentsInChildren<PQSCity>(true).First(m => m.name == "KSC");

            // If there's no KSC, exit.
            if (ksc == null)
            {
                Logger.Active.Log("[Kopernicus] Couldn't find the KSC object!");
                return;
            }

            // Go through the SpaceCenterCameras and fix them
            foreach (SpaceCenterCamera2 cam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
            {
                if (ksc.repositionToSphere || ksc.repositionToSphereSurface)
                {
                    Double normalHeight = body.pqsController.GetSurfaceHeight(ksc.repositionRadial.normalized) - body.Radius;
                    if (ksc.repositionToSphereSurface)
                    {
                        normalHeight += ksc.repositionRadiusOffset;
                    }
                    cam.altitudeInitial = 0f - (Single)normalHeight;
                }
                else
                {
                    cam.altitudeInitial = 0f - (Single)ksc.repositionRadiusOffset;
                }

                // re-implement cam.Start()
                // fields
                Type camType = cam.GetType();
                FieldInfo camSphere = null;
                FieldInfo transform1 = null;
                FieldInfo transform2 = null;
                FieldInfo surfaceObj = null;

                // get fields
                FieldInfo[] fields = camType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                for (Int32 i = 0; i < fields.Length; ++i)
                {
                    FieldInfo fi = fields[i];
                    if (fi.FieldType == typeof(PQS))
                    {
                        camSphere = fi;
                    }
                    else if (fi.FieldType == typeof(Transform) && transform1 == null)
                    {
                        transform1 = fi;
                    }
                    else if (fi.FieldType == typeof(Transform) && transform2 == null)
                    {
                        transform2 = fi;
                    }
                    else if (fi.FieldType == typeof(SurfaceObject))
                    {
                        surfaceObj = fi;
                    }
                }
                if (camSphere != null && transform1 != null && transform2 != null && surfaceObj != null)
                {
                    camSphere.SetValue(cam, body.pqsController);

                    Transform initialTransform = body.pqsController.transform.Find(cam.initialPositionTransformName);
                    if (initialTransform != null)
                    {
                        transform1.SetValue(cam, initialTransform);
                        cam.transform.NestToParent(initialTransform);
                    }
                    else
                    {
                        Logger.Active.Log("[Kopernicus] SSC2 can't find initial transform!");
                        Transform initialTrfOrig = transform1.GetValue(cam) as Transform;
                        if (initialTrfOrig != null)
                        {
                            cam.transform.NestToParent(initialTrfOrig);
                        }
                        else
                        {
                            Logger.Active.Log("[Kopernicus] SSC2 own initial transform null!");
                        }
                    }
                    Transform camTransform = transform2.GetValue(cam) as Transform;
                    if (camTransform != null)
                    {
                        camTransform.NestToParent(cam.transform);
                        if (FlightCamera.fetch != null && FlightCamera.fetch.transform != null)
                        {
                            FlightCamera.fetch.transform.NestToParent(camTransform);
                        }
                        if (LocalSpace.fetch != null && LocalSpace.fetch.transform != null)
                        {
                            LocalSpace.fetch.transform.position = camTransform.position;
                        }
                    }
                    else
                    {
                        Logger.Active.Log("[Kopernicus] SSC2 cam transform null!");
                    }

                    cam.ResetCamera();

                    SurfaceObject so = surfaceObj.GetValue(cam) as SurfaceObject;
                    if (so != null)
                    {
                        so.ReturnToParent();
                        DestroyImmediate(so);
                    }
                    else
                    {
                        Logger.Active.Log("[Kopernicus] SSC2 surfaceObject is null!");
                    }

                    surfaceObj.SetValue(cam, SurfaceObject.Create(initialTransform.gameObject, FlightGlobals.currentMainBody, 3, KFSMUpdateMode.FIXEDUPDATE));
                    Logger.Active.Log("[Kopernicus] Fixed SpaceCenterCamera");
                }
                else
                {
                    Logger.Active.Log("[Kopernicus] ERROR fixing space center camera, could not find some fields");
                }
            }
        }

        // Patch the KSC light animation
        private static void PatchTimeOfDayAnimation()
        {
            TimeOfDayAnimation[] animations = Resources.FindObjectsOfTypeAll<TimeOfDayAnimation>();
            for (Int32 i = 0; i < animations.Length; i++)
            {
                animations[i].target = KopernicusStar.GetNearest(FlightGlobals.GetHomeBody()).gameObject.transform;
            }
        }

        // Patch various references to point to the nearest star
        private static void PatchStarReferences(CelestialBody body)
        {
            GameObject star = KopernicusStar.GetNearest(body).gameObject;
            if (body.afg != null)
            {
                body.afg.sunLight = star;
            }
            if (body.scaledBody.GetComponent<MaterialSetDirection>() != null)
            {
                body.scaledBody.GetComponent<MaterialSetDirection>().target = star.transform;
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
        private  void ApplyFlagFixes()
        {
            GameEvents.OnKSCFacilityUpgraded.Add(FixFlags);
            GameEvents.OnKSCStructureRepaired.Add(FixFlags);
        }

        private  void FixFlags(DestructibleBuilding data)
        {
            FixFlags();
        }

        private  void FixFlags(Upgradeables.UpgradeableFacility data0, int data1)
        {
            FixFlags();
        }

        private  void FixFlags()
        {
            PQSCity KSC = FlightGlobals.GetHomeBody()?.pqsController?.GetComponentsInChildren<PQSCity>(true)?.FirstOrDefault(p => p?.name == "KSC");
            SkinnedMeshRenderer[] flags = KSC?.GetComponentsInChildren<SkinnedMeshRenderer>(true)?.Where(smr => smr?.name == "Flag")?.ToArray();
            for (int i = 0; i < flags?.Length; i++)
            {
                flags[i].rootBone = flags[i]?.rootBone?.parent?.gameObject?.GetChild("bn_upper_flag_a01")?.transform;
            }
        }

        // Remove the Handlers
        private void OnDestroy()
        {
            GameEvents.OnMapEntered.Remove(OnMapEntered);
            GameEvents.onLevelWasLoaded.Remove(OnLevelWasLoaded);
            GameEvents.onProtoVesselLoad.Remove(TransformBodyReferencesOnLoad);
            GameEvents.onProtoVesselSave.Remove(TransformBodyReferencesOnSave);
        }
    }
}
