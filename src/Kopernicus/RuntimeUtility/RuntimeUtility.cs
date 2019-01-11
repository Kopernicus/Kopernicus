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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using Contracts;
using Kopernicus.Components;
using Kopernicus.Configuration;
using KSP.UI.Screens;
using KSP.UI.Screens.Mapview;
using KSP.UI.Screens.Mapview.MapContextMenuOptions;
using ModularFI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
#if !KSP131
using Expansions;
#endif
using KSP.UI.Screens.Settings.Controls;
using UnityEngine;

namespace Kopernicus
{
    // Mod runtime utilitues
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    [SuppressMessage("ReSharper", "AccessToStaticMemberViaDerivedType")]
    public class RuntimeUtility : MonoBehaviour
    {
        // Awake() - flag this class as don't destroy on load and register delegates
        void Awake()
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
            new Logger("Kopernicus.Runtime").SetAsActive();

            // Add handlers
            GameEvents.onPartUnpack.Add(OnPartUnpack);
            GameEvents.OnMapEntered.Add(OnMapEntered);
            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);
            GameEvents.onProtoVesselLoad.Add(TransformBodyReferencesOnLoad);
            GameEvents.onProtoVesselSave.Add(TransformBodyReferencesOnSave);
            
            // Log
            Logger.Default.Log("[Kopernicus] RuntimeUtility Started");
            Logger.Default.Flush();
        }

        // Execute MainMenu functions
        void Start()
        {
            RemoveUnselectableObjects();
            ApplyLaunchSitePatches();
            ApplyMusicAltitude();
            ApplyInitialTarget();
            ApplyOrbitPatches();

            for (Int32 i = 0; i < PSystemManager.Instance.localBodies.Count; i++)
            {
                ApplyStarPatches(PSystemManager.Instance.localBodies[i]);
            }
        }

        // Stuff
        void LateUpdate()
        {
            FixZooming();
            ApplyRDPatches();
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
        void OnLevelWasLoaded(GameScenes scene)
        {
            PatchFI();
            FixCameras();
            PatchTimeOfDayAnimation();

            for (Int32 i = 0; i < PSystemManager.Instance.localBodies.Count; i++)
            {
                PatchStarReferences(PSystemManager.Instance.localBodies[i]);
                PatchContractWeight(PSystemManager.Instance.localBodies[i]);
            }
        }

        // Fix the buoyancy
        void OnPartUnpack(Part part)
        {
            // If there's nothing to do, abort
            if (part.partBuoyancy == null)
                return;

            // Replace PartBuoyancy with KopernicusBuoyancy
            KopernicusBuoyancy buoyancy = part.gameObject.AddComponent<KopernicusBuoyancy>();
            Utility.CopyObjectFields(part.partBuoyancy, buoyancy, false);
            part.partBuoyancy = buoyancy;
            Destroy(part.GetComponent<PartBuoyancy>());
        }

        // Transforms body references in the savegames
        void TransformBodyReferencesOnLoad(GameEvents.FromToAction<ProtoVessel, ConfigNode> data)
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

        // Transforms body references in the savegames
        void TransformBodyReferencesOnSave(GameEvents.FromToAction<ProtoVessel, ConfigNode> data)
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
        void RemoveUnselectableObjects()
        {
            PlanetariumCamera.fetch.targets
                .Where(m => m.celestialBody != null && (m.celestialBody.Has("barycenter") || !m.celestialBody.Get("selectable", true)))
                .ToList()
                .ForEach(map => PlanetariumCamera.fetch.targets.Remove(map));
        }

        [SuppressMessage("ReSharper", "RedundantCast")]
        void ApplyStarPatches(CelestialBody body)
        {
            if (body.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length <= 0)
            {
                return;
            }
            
            // Copy the Sun component and transform it into a Kopernicus one
            GameObject starObject = (GameObject)Instantiate(Sun.Instance.gameObject, Sun.Instance.transform.parent, true);
            Sun sunComponent = starObject.GetComponent<Sun>();
            KopernicusStar star = starObject.AddComponent<KopernicusStar>();
            Utility.CopyObjectFields(sunComponent, star, false);
            DestroyImmediate(sunComponent);
            
            // Configure the new star
            star.sun = body;
            starObject.name = body.name;
            starObject.transform.localPosition = Vector3.zero;
            starObject.transform.localRotation = Quaternion.identity;
            starObject.transform.localScale = Vector3.one;
            starObject.transform.position = body.position;
            starObject.transform.rotation = body.rotation;

            // Copy the SunFlare component and transform it into a Kopernicus one
            GameObject flareObj = (GameObject)Instantiate(SunFlare.Instance.gameObject, SunFlare.Instance.transform.parent, true);
            SunFlare sunFlareComponent = flareObj.GetComponent<SunFlare>();
            KopernicusSunFlare flare = flareObj.AddComponent<KopernicusSunFlare>();
            Utility.CopyObjectFields(sunFlareComponent, flare, false);
            DestroyImmediate(sunFlareComponent);
            
            // Configure the new flare
            star.lensFlare = flare;
            flareObj.name = body.name;
            flareObj.transform.localPosition = Vector3.zero;
            flareObj.transform.localRotation = Quaternion.identity;
            flareObj.transform.localScale = Vector3.one;
            flareObj.transform.position = body.position;
            flareObj.transform.rotation = body.rotation;
            
            // If we edited the center update global variables
            if (body.flightGlobalsIndex != 0)
            {
                return;
            }
            
            Sun.Instance = star;
            SunFlare.Instance = flare;
        }

        void ApplyLaunchSitePatches()
        {
            #if !KSP131
            if (!ExpansionsLoader.IsExpansionInstalled("MakingHistory"))
            {
                return;
            }
            
            PQSCity2[] cities = FindObjectsOfType<PQSCity2>();
            for (int i = 0; i < Templates.RemoveLaunchSites.Count; i++)
            {
                String site = Templates.RemoveLaunchSites[i];
                
                // Remove the launch site from the list if it exists
                if (PSystemSetup.Instance.LaunchSites.Any(s => s.name == site))
                {
                    PSystemSetup.Instance.RemoveLaunchSite(site);
                }

                PQSCity2 city = cities.FirstOrDefault(c =>
                    c.gameObject.name == site || c.gameObject.name == site + "(Clone)");

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
            #endif
        }

        // Apply the home atmosphere height as the altitude where space music starts
        void ApplyMusicAltitude()
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
        void ApplyInitialTarget()
        {
            CelestialBody home = PSystemManager.Instance.localBodies.Find(b => b.isHomeWorld);
            ScaledMovement movement = home.scaledBody.GetComponentInChildren<ScaledMovement>();
            PlanetariumCamera.fetch.initialTarget = movement;
        }
        
        // Apply PostSpawnOrbit patches
        void ApplyOrbitPatches()
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
                Parser.LoadObjectFromConfigurationNode(loader, orbitNode, "Kopernicus");
                CelestialBody oldRef = body.referenceBody;
                body.referenceBody.orbitingBodies.Remove(body);

                CelestialBody newRef = UBI.GetBody(loader.referenceBody);
                if (newRef != null)
                {
                    body.orbit.referenceBody = body.orbitDriver.referenceBody = newRef;
                }
                else
                {
                    // Log the exception
                    Debug.Log("Exception: PostSpawnOrbit reference body for \"" + body.name +
                              "\" could not be found. Missing body name is \"" + loader.referenceBody + "\".");

                    // Open the Warning popup
                    Injector.DisplayWarning();
                }

                fixes.Add(body.transform.name,
                    new KeyValuePair<CelestialBody, CelestialBody>(oldRef, body.referenceBody));
                body.referenceBody.orbitingBodies.Add(body);
                body.referenceBody.orbitingBodies =
                    body.referenceBody.orbitingBodies.OrderBy(cb => cb.orbit.semiMajorAxis).ToList();
                body.orbit.Init();
                body.orbitDriver.UpdateOrbit();

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
            PlanetariumCamera.fetch.targets.Clear();
            PlanetariumCamera.fetch.targets.AddRange(trackingstation);

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
        void FixZooming()
        {
            if (HighLogic.LoadedSceneHasPlanetarium && MapView.fetch != null && !_fixZoomingIsDone)
            {
                // Fix the bug via switching away from Home and back immideatly. 
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
            if (target == null || target.celestialBody == null)
            {
                return;
            }
            
            CelestialBody body = target.celestialBody;
            if (body.Has("maxZoom"))
                PlanetariumCamera.fetch.minDistance = body.Get<Single>("maxZoom");
            else
                PlanetariumCamera.fetch.minDistance = 10;
        }

        // Whether to apply RD changes next frame
        private Boolean _rdIsDone;
        
        // Remove the thumbnail for Barycenters in the RD and patch name changes
        void ApplyRDPatches()
        {
            // Only run in SpaceCenter
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                // Done
                if (_rdIsDone)
                    return;

                // Waaah
                foreach (RDArchivesController controller in Resources.FindObjectsOfTypeAll<RDArchivesController>())
                    controller.gameObject.AddOrGetComponent<RnDFixer>();
                _rdIsDone = true;
            }
            else
            {
                _rdIsDone = false;
            }
        }

        // If 3D rendering of orbits is forced, enable it
        void Force3DRendering()
        {
            if (!Templates.force3DOrbits)
            {
                return;
            }
            
            MapView.fetch.max3DlineDrawDist = Single.MaxValue;
            GameSettings.MAP_MAX_ORBIT_BEFORE_FORCE2D = Int32.MaxValue;
        }

        // The preset names
        private String[] _details;
        
        // Update the names of the presets in the settings dialog
        void UpdatePresetNames()
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

        // Removes the targetbuttons for barycenters in the MapView
        void ApplyMapTargetPatches()
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
            if (FlightGlobals.ActiveVessel == null)
            {
                return;
            }

            OrbitTargeter targeter = FlightGlobals.ActiveVessel.orbitTargeter;
            if (targeter == null)
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
            if (body == null)
            {
                return;
            }

            if (!body.Has("barycenter") && body.Get("selectable", true))
            {
                return;
            }

            if (cast.driver == null || cast.driver.Targetable == null)
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

        void FixFlickeringOrbitLines()
        {
            // Prevent the orbit lines from flickering
            PlanetariumCamera.Camera.farClipPlane = 1e14f;
        }

        // Whether to apply the customizations next frame
        private Boolean _orbitIconsReady;

        void OnMapEntered()
        {
            _orbitIconsReady = true;
        }
        
        // The cache for the orbit icon objects
        private readonly Dictionary<CelestialBody, Sprite> _spriteCache = new Dictionary<CelestialBody, Sprite>();

        // Apply custom orbit icons.
        void ApplyOrbitIconCustomization()
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
                if (sprite == null)
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
        void ApplyOrbitVisibility(CelestialBody body)
        {
            if (HighLogic.LoadedScene != GameScenes.TRACKSTATION &&
                (!HighLogic.LoadedSceneIsFlight || !MapView.MapIsEnabled))
            {
                return;
            }

            // Loop
            // Check for Renderer
            if (body.orbitDriver == null)
            {
                return;
            }
            if (body.orbitDriver.Renderer == null)
            {
                return;
            }

            // Apply Orbit mode changes
            if (body.Has("drawMode"))
                body.orbitDriver.Renderer.drawMode = body.Get<OrbitRenderer.DrawMode>("drawMode");

            // Apply Orbit icon changes
            if (body.Has("drawIcons"))
                body.orbitDriver.Renderer.drawIcons = body.Get<OrbitRenderer.DrawIcons>("drawIcons");
        }
        
        // Shader accessor for AtmosphereFromGround
        private readonly Int32 _lightDot = Shader.PropertyToID("_lightDot");

        // Use the nearest star as the AFG star
        void AtmosphereLightPatch(CelestialBody body)
        {
            if (body.afg == null)
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
        void PatchFI()
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                Events.OnRuntimeUtilityPatchFI.Fire();
                ModularFlightIntegrator.RegisterCalculateSunBodyFluxOverride(KopernicusStar.SunBodyFlux);
            }
        }

        // Fix the Space Center Cameras
        void FixCameras()
        {
            // Only run in the space center or the editor
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER && !HighLogic.LoadedSceneIsEditor)
            {
                return;
            }
            
            // Get the parental body
            CelestialBody body;
            if (Planetarium.fetch != null)
                body = Planetarium.fetch.Home;
            else
                body = FlightGlobals.Bodies.Find(b => b.isHomeWorld);

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

            // Go throug the SpaceCenterCameras and fix them
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
                FieldInfo camPQS = null;
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
                        camPQS = fi;
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
                if (camPQS != null && transform1 != null && transform2 != null && surfaceObj != null)
                {
                    camPQS.SetValue(cam, body.pqsController);

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
                    Logger.Active.Log("[Kopernicus] ERROR fixing space center camera, could not find some fields");
            }
        }

        // Patch the KSC light animation
        void PatchTimeOfDayAnimation()
        {
            TimeOfDayAnimation[] animations = Resources.FindObjectsOfTypeAll<TimeOfDayAnimation>();
            for (Int32 i = 0; i < animations.Length; i++)
            {
                animations[i].target = KopernicusStar.GetNearest(FlightGlobals.GetHomeBody()).gameObject.transform;
            }
        }

        // Patch various references to point to the nearest star
        void PatchStarReferences(CelestialBody body)
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
        void PatchContractWeight(CelestialBody body)
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

        // Remove the Handlers
        void OnDestroy()
        {
            GameEvents.onPartUnpack.Remove(OnPartUnpack);
            GameEvents.OnMapEntered.Remove(OnMapEntered);
            GameEvents.onLevelWasLoaded.Remove(OnLevelWasLoaded);
            GameEvents.onProtoVesselLoad.Remove(TransformBodyReferencesOnLoad);
            GameEvents.onProtoVesselSave.Remove(TransformBodyReferencesOnSave);
        }
    }
}
