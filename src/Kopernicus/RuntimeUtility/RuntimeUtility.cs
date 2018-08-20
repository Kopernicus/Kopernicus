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
using System.IO;
using System.Linq;
using System.Reflection;
#if !KSP131
using Expansions;
#endif
using Kopernicus.OnDemand;
using KSP.UI.Screens.Settings;
using KSP.UI.Screens.Settings.Controls;
using UnityEngine;

namespace Kopernicus
{
    // Mod runtime utilitues
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RuntimeUtility : MonoBehaviour
    {
        // Variables
        public MapObject previous;

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
            GameEvents.onLevelWasLoaded.Add(FixCameras);
            GameEvents.onLevelWasLoaded.Add(delegate (GameScenes scene)
            {
                //if (MapView.fetch != null)
                //    MapView.fetch.max3DlineDrawDist = Single.MaxValue;
                if (scene == GameScenes.SPACECENTER)
                    PatchFI();
                foreach (CelestialBody body in PSystemManager.Instance.localBodies)
                {
                    GameObject star = KopernicusStar.GetNearest(body).gameObject;
                    if (body.afg != null)
                        body.afg.sunLight = star;
                    if (body.scaledBody.GetComponent<MaterialSetDirection>() != null)
                        body.scaledBody.GetComponent<MaterialSetDirection>().target = star.transform;

                    foreach (PQSMod_MaterialSetDirection msd in body.GetComponentsInChildren<PQSMod_MaterialSetDirection>(true))
                        msd.target = star.transform;

                    // Contract Weight
                    if (ContractSystem.ContractWeights != null)
                    {
                        if (body.Has("contractWeight"))
                        {
                            if (ContractSystem.ContractWeights.ContainsKey(body.name))
                            {
                                ContractSystem.ContractWeights[body.name] = body.Get<Int32>("contractWeight");
                            }
                            else
                            {
                                ContractSystem.ContractWeights.Add(body.name, body.Get<Int32>("contractWeight"));
                            }
                        }
                    }
                }
                
                foreach (TimeOfDayAnimation anim in Resources.FindObjectsOfTypeAll<TimeOfDayAnimation>())
                    anim.target = KopernicusStar.GetNearest(FlightGlobals.GetHomeBody()).gameObject.transform;
#if FALSE
                foreach (TimeOfDayAnimation anim in Resources.FindObjectsOfTypeAll<TimeOfDayAnimation>())
                {
                    anim.gameObject.AddOrGetComponent<KopernicusStarTimeOfDay>();
                }

                foreach (GalaxyCubeControl control in Resources.FindObjectsOfTypeAll<GalaxyCubeControl>())
                {
                    control.gameObject.AddOrGetComponent<KopernicusStarGalaxyCubeControl>();
                } 

                foreach (SkySphereControl control in Resources.FindObjectsOfTypeAll<SkySphereControl>())
                {
                    control.gameObject.AddOrGetComponent<KopernicusStarSkySphereControl>();
                } 
#endif
            });
            GameEvents.onProtoVesselLoad.Add(TransformBodyReferencesOnLoad);
            GameEvents.onProtoVesselSave.Add(TransformBodyReferencesOnSave);

            // Update Music Logic
            if (MusicLogic.fetch != null && FlightGlobals.fetch != null && FlightGlobals.GetHomeBody() != null)
                MusicLogic.fetch.flightMusicSpaceAltitude = FlightGlobals.GetHomeBody().atmosphereDepth;
            
            // Log
            Logger.Default.Log("[Kopernicus] RuntimeUtility Started");
            Logger.Default.Flush();
        }

        // Execute MainMenu functions
        void Start()
        {
            previous = PlanetariumCamera.fetch.initialTarget;
            PlanetariumCamera.fetch.targets
                .Where(m => m.celestialBody != null && (m.celestialBody.Has("barycenter") || !m.celestialBody.Get("selectable", true)))
                .ToList()
                .ForEach(map => PlanetariumCamera.fetch.targets.Remove(map));

            // Stars
            GameObject gob = Sun.Instance.gameObject;
            KopernicusStar star = gob.AddComponent<KopernicusStar>();
            Utility.CopyObjectFields(Sun.Instance, star, false);
            DestroyImmediate(Sun.Instance);
            Sun.Instance = star;

            // LensFlares
            gob = SunFlare.Instance.gameObject;
            KopernicusSunFlare flare = gob.AddComponent<KopernicusSunFlare>();
            gob.name = star.sun.name;
            Utility.CopyObjectFields(SunFlare.Instance, flare, false);
            DestroyImmediate(SunFlare.Instance);
            SunFlare.Instance = star.lensFlare = flare;

            // Bodies
            Dictionary<String, KeyValuePair<CelestialBody, CelestialBody>> fixes = new Dictionary<String, KeyValuePair<CelestialBody, CelestialBody>>();

            foreach (CelestialBody body in PSystemManager.Instance.localBodies)
            {
                // More stars
                if (body.flightGlobalsIndex != 0 && body.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length > 0)
                {
                    GameObject starObj = Instantiate(Sun.Instance.gameObject);
                    star = starObj.GetComponent<KopernicusStar>();
                    star.sun = body;
                    starObj.transform.parent = Sun.Instance.transform.parent;
                    starObj.name = body.name;
                    starObj.transform.localPosition = Vector3.zero;
                    starObj.transform.localRotation = Quaternion.identity;
                    starObj.transform.localScale = Vector3.one;
                    starObj.transform.position = body.position;
                    starObj.transform.rotation = body.rotation;

                    GameObject flareObj = Instantiate(SunFlare.Instance.gameObject);
                    flare = flareObj.GetComponent<KopernicusSunFlare>();
                    star.lensFlare = flare;
                    flareObj.transform.parent = SunFlare.Instance.transform.parent;
                    flareObj.name = body.name;
                    flareObj.transform.localPosition = Vector3.zero;
                    flareObj.transform.localRotation = Quaternion.identity;
                    flareObj.transform.localScale = Vector3.one;
                    flareObj.transform.position = body.position;
                    flareObj.transform.rotation = body.rotation;
                }

                // Post spawn patcher
                if (body.Has("orbitPatches"))
                {
                    ConfigNode orbitNode = body.Get<ConfigNode>("orbitPatches");
                    OrbitLoader loader = new OrbitLoader(body);
                    Parser.LoadObjectFromConfigurationNode(loader, orbitNode, "Kopernicus");
                    CelestialBody oldRef = body.referenceBody;
                    body.referenceBody.orbitingBodies.Remove(body);

                    CelestialBody newRef = PSystemManager.Instance.localBodies.FirstOrDefault(b => b.transform.name == loader.referenceBody);
                    if (newRef != null)
                    {
                        body.orbit.referenceBody = body.orbitDriver.referenceBody = newRef;
                    }
                    else
                    {
                        // Log the exception
                        Debug.Log("Exception: PostSpawnOrbit reference body for \"" + body.name + "\" could not be found. Missing body name is \"" + loader.referenceBody + "\".");

                        // Open the Warning popup
                        Injector.DisplayWarning();
                    }

                    fixes.Add(body.transform.name, new KeyValuePair<CelestialBody, CelestialBody>(oldRef, body.referenceBody));
                    body.referenceBody.orbitingBodies.Add(body);
                    body.referenceBody.orbitingBodies = body.referenceBody.orbitingBodies.OrderBy(cb => cb.orbit.semiMajorAxis).ToList();
                    body.orbit.Init();
                    body.orbitDriver.UpdateOrbit();

                    // Calculations
                    if (!body.Has("sphereOfInfluence"))
                        body.sphereOfInfluence = body.orbit.semiMajorAxis * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 0.4);
                    if (!body.Has("hillSphere"))
                        body.hillSphere = body.orbit.semiMajorAxis * (1 - body.orbit.eccentricity) * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 0.333333333333333);
                    if (body.solarRotationPeriod)
                    {
                        Double rotPeriod = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, body.transform.name).celestialBody.rotationPeriod;
                        Double num1 = Math.PI * 2 * Math.Sqrt(Math.Pow(Math.Abs(body.orbit.semiMajorAxis), 3) / body.orbit.referenceBody.gravParameter);
                        body.rotationPeriod = rotPeriod * num1 / (num1 + rotPeriod); ;
                    }
                }
            }

            // Update the order in the tracking station
            List<MapObject> trackingstation = new List<MapObject>();
            Utility.DoRecursive(PSystemManager.Instance.localBodies[0], cb => cb.orbitingBodies, cb =>
            {
                trackingstation.Add(PlanetariumCamera.fetch.targets.Find(t => t.celestialBody == cb));
            });
            PlanetariumCamera.fetch.targets.Clear();
            PlanetariumCamera.fetch.targets.AddRange(trackingstation);

            // Update the initialTarget of the tracking station
            Resources.FindObjectsOfTypeAll<PlanetariumCamera>().FirstOrDefault().initialTarget = Resources.FindObjectsOfTypeAll<ScaledMovement>().FirstOrDefault(o => o.celestialBody.isHomeWorld);

            // Undo stuff
            foreach (CelestialBody b in PSystemManager.Instance.localBodies.Where(b_ => b_.Has("orbitPatches")))
            {
                fixes[b.transform.name].Value.orbitingBodies.Remove(b);
                fixes[b.transform.name].Key.orbitingBodies.Add(b);
                fixes[b.transform.name].Key.orbitingBodies = fixes[b.transform.name].Key.orbitingBodies.OrderBy(cb => cb.orbit.semiMajorAxis).ToList();
            }

            
            #if !KSP131
            if (ExpansionsLoader.IsExpansionInstalled("MakingHistory"))
            {
                PQSCity2[] cities = FindObjectsOfType<PQSCity2>();
                foreach (String site in Templates.RemoveLaunchSites)
                {
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
                FieldInfo stockLaunchSitesField = typeof(PSystemSetup).GetField("stocklaunchsites", BindingFlags.NonPublic | BindingFlags.Instance);
                LaunchSite[] stockLaunchSites = (LaunchSite[])stockLaunchSitesField.GetValue(PSystemSetup.Instance);
                LaunchSite[] updateStockLaunchSites = stockLaunchSites.Where(site => !Templates.RemoveLaunchSites.Contains(site.name)).ToArray();
                if (stockLaunchSites.Length != updateStockLaunchSites.Length)
                {
                    stockLaunchSitesField.SetValue(PSystemSetup.Instance, updateStockLaunchSites);
                }
            }
            #endif
#if FALSE
            // AFG-Ception
            foreach (CelestialBody body in PSystemManager.Instance.localBodies)
            {
                if (body.afg == null)
                {
                    continue;
                }
                
                foreach (KopernicusStar s in KopernicusStar.Stars)
                {
                    AtmosphereFromGround afg;
                    if (s != Sun.Instance)
                    {
                        afg = Instantiate(body.afg.gameObject)
                            .GetComponent<AtmosphereFromGround>();
                        Utility.CopyObjectFields(body.afg, afg, false);
                        afg.transform.parent = body.afg.transform.parent;
                        afg.transform.localPosition = body.afg.transform.localPosition;
                        afg.transform.localScale = body.afg.transform.localScale;
                        afg.transform.localRotation = body.afg.transform.localRotation;
                        afg.gameObject.layer = body.afg.gameObject.layer;
                    }
                    else
                    {
                        afg = body.afg;
                    }

                    afg.gameObject.AddComponent<KopernicusStarAFG>().Star = s;
                }
            }
#endif
        }

        /// <summary>
        /// Fields for the orbit targeter patching
        /// </summary>
        private FieldInfo[] fields;

        /// <summary>
        /// All display names for the terrain detail
        /// </summary>
        private String[] _details;

        // Stuff
        void LateUpdate()
        {
            FixZooming();
            ApplyOrbitVisibility();
            RDFixer();
            
            // Prevent the orbit lines from flickering
            PlanetariumCamera.Camera.farClipPlane = 1e14f;

            // Remove buttons in map view for barycenters
            if (MapView.MapIsEnabled)
            {
                if (fields == null)
                {
                    FieldInfo mode_f = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.FieldType.IsEnum && f.FieldType.IsNested);
                    FieldInfo context_f = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.FieldType == typeof(MapContextMenu));
                    #if !KSP131
                    FieldInfo cast_f = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.FieldType == typeof(OrbitRendererBase.OrbitCastHit));
                    #else
                    FieldInfo cast_f = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.FieldType == typeof(OrbitRenderer.OrbitCastHit));
                    #endif
                    fields = new FieldInfo[] { mode_f, context_f, cast_f };
                }
                if (FlightGlobals.ActiveVessel != null)
                {
                    OrbitTargeter targeter = FlightGlobals.ActiveVessel.orbitTargeter;
                    if (targeter == null)
                        return;
                    Int32 mode = (Int32)fields[0].GetValue(targeter);
                    if (mode == 2)
                    {
                        #if !KSP131
                        OrbitRendererBase.OrbitCastHit cast = (OrbitRendererBase.OrbitCastHit)fields[2].GetValue(targeter);
                        #else
                        OrbitRenderer.OrbitCastHit cast = (OrbitRenderer.OrbitCastHit) fields[2].GetValue(targeter);
                        #endif
                        CelestialBody body = PSystemManager.Instance.localBodies.Find(b => b.name == cast.or?.discoveryInfo?.name?.Value);
                        if (body == null) return;
                        if (body.Has("barycenter") || !body.Get("selectable", true))
                        {
                            if (cast.driver?.Targetable == null) return;
                            MapContextMenu context = MapContextMenu.Create(body.name, new Rect(0.5f, 0.5f, 300f, 50f), cast, () =>
                            {
                                fields[0].SetValue(targeter, 0);
                                fields[1].SetValue(targeter, null);
                            }, new SetAsTarget(cast.driver.Targetable, () => FlightGlobals.fetch.VesselTarget));
                            fields[1].SetValue(targeter, context);
                        }
                    }
                }
                
                // Apply orbit icon customization
                foreach (MapNode node in Resources.FindObjectsOfTypeAll<MapNode>())
                {
                    if (node.mapObject != null && node.mapObject.celestialBody != null && node.mapObject.celestialBody.Has("iconTexture"))
                    {
                        Texture2D texture = node.mapObject.celestialBody.Get<Texture2D>("iconTexture");
                        node.SetIcon(Sprite.Create(texture,
                            new Rect(0, 0, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.Tight,
                            Vector4.zero));
                    }
                }
            }

            foreach (CelestialBody body in PSystemManager.Instance.localBodies)
            {
                if (body.afg == null) continue;
                GameObject star_ = KopernicusStar.GetNearest(body).gameObject;
                Vector3 planet2cam = body.scaledBody.transform.position - body.afg.mainCamera.transform.position;
                body.afg.lightDot = Mathf.Clamp01(Vector3.Dot(planet2cam, body.afg.mainCamera.transform.position - star_.transform.position) * body.afg.dawnFactor);
                body.afg.GetComponent<Renderer>().sharedMaterial.SetFloat("_lightDot", body.afg.lightDot);
            }
            
            // Update the names of the presets in the settings dialog
            if (HighLogic.LoadedScene == GameScenes.SETTINGS)
            {
                foreach (SettingsTerrainDetail detail in Resources.FindObjectsOfTypeAll<SettingsTerrainDetail>())
                {
                    detail.displayStringValue = true;
                    detail.stringValues = _details ?? (_details = Templates.PresetDisplayNames.ToArray());
                }
            }
        }

        // Status
        Boolean isDone = false;
        Boolean isDone2 = false;

        // Fix the Zooming-Out bug
        void FixZooming()
        {
            if (HighLogic.LoadedSceneHasPlanetarium && MapView.fetch != null && !isDone)
            {
                // Fix the bug via switching away from Home and back immideatly. 
                // TODO: Check if this still happens
                PlanetariumCamera.fetch.SetTarget(PlanetariumCamera.fetch.targets[(PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) + 1) % PlanetariumCamera.fetch.targets.Count]);
                PlanetariumCamera.fetch.SetTarget(PlanetariumCamera.fetch.targets[(PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) - 1) + (((PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) - 1) >= 0) ? 0 : PlanetariumCamera.fetch.targets.Count)]);

                // Terminate for the moment.
                isDone = true;
            }

            // Set custom Zoom-In limits
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION || MapView.MapIsEnabled)
            {
                MapObject target = PlanetariumCamera.fetch.target;
                if (target?.celestialBody != null)
                {
                    CelestialBody body = target.celestialBody;
                    if (body.Has("maxZoom"))
                        PlanetariumCamera.fetch.minDistance = body.Get<Single>("maxZoom");
                    else
                        PlanetariumCamera.fetch.minDistance = 10;
                }
            }
        }

        // Applies invisible orbits
        void ApplyOrbitVisibility()
        {
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION || (HighLogic.LoadedSceneIsFlight && MapView.MapIsEnabled))
            {
                // Loop
                foreach (CelestialBody body in PSystemManager.Instance.localBodies)
                {
                    // Check for Renderer
                    if (!body.orbitDriver)
                        continue;
                    if (!body.orbitDriver.Renderer)
                        return;

                    // Apply Orbit mode changes
                    if (body.Has("drawMode"))
                        body.orbitDriver.Renderer.drawMode = body.Get<OrbitRenderer.DrawMode>("drawMode");

                    // Apply Orbit icon changes
                    if (body.Has("drawIcons"))
                        body.orbitDriver.Renderer.drawIcons = body.Get<OrbitRenderer.DrawIcons>("drawIcons");
                }
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

        // Remove the thumbnail for Barycenters in the RD and patch name changes
        void RDFixer()
        {
            // Only run in SpaceCenter
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                // Done
                if (isDone2)
                    return;

                // Waaah
                foreach (RDArchivesController controller in Resources.FindObjectsOfTypeAll<RDArchivesController>())
                    controller.gameObject.AddOrGetComponent<RnDFixer>();
                isDone2 = true;
            }
            else
            {
                isDone2 = false;
            }
        }

        // Fix the Space Center
        void FixCameras(GameScenes scene)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER && !HighLogic.LoadedSceneIsEditor)
                return;

            // Get the parental body
            CelestialBody body = null;
            if (Planetarium.fetch != null)
                body = Planetarium.fetch.Home;
            else
                body = FlightGlobals.Bodies.Find(b => b.isHomeWorld);

            // If there's no body, exit.
            if (body == null)
            {
                Debug.Log("[Kopernicus] Couldn't find the parental body!");
                return;
            }

            // Get the KSC object
            PQSCity ksc = body.pqsController.GetComponentsInChildren<PQSCity>(true).First(m => m.name == "KSC");

            // If there's no KSC, exit.
            if (ksc == null)
            {
                Debug.Log("[Kopernicus] Couldn't find the KSC object!");
                return;
            }

            // Go throug the SpaceCenterCameras and fix them
            foreach (SpaceCenterCamera2 cam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
            {
                if (ksc.repositionToSphere || ksc.repositionToSphereSurface)
                {
                    Double normalHeight = body.pqsController.GetSurfaceHeight((Vector3d)ksc.repositionRadial.normalized) - body.Radius;
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
                        camPQS = fi;
                    else if (fi.FieldType == typeof(Transform) && transform1 == null)
                        transform1 = fi;
                    else if (fi.FieldType == typeof(Transform) && transform2 == null)
                        transform2 = fi;
                    else if (fi.FieldType == typeof(SurfaceObject))
                        surfaceObj = fi;
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
                        Debug.Log("[Kopernicus] SSC2 can't find initial transform!");
                        Transform initialTrfOrig = transform1.GetValue(cam) as Transform;
                        if (initialTrfOrig != null)
                            cam.transform.NestToParent(initialTrfOrig);
                        else
                            Debug.Log("[Kopernicus] SSC2 own initial transform null!");
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
                        Debug.Log("[Kopernicus] SSC2 cam transform null!");

                    cam.ResetCamera();

                    SurfaceObject so = surfaceObj.GetValue(cam) as SurfaceObject;
                    if (so != null)
                    {
                        so.ReturnToParent();
                        DestroyImmediate(so);
                    }
                    else
                        Debug.Log("[Kopernicus] SSC2 surfaceObject is null!");

                    surfaceObj.SetValue(cam, SurfaceObject.Create(initialTransform.gameObject, FlightGlobals.currentMainBody, 3, KFSMUpdateMode.FIXEDUPDATE));

                    Debug.Log("[Kopernicus] Fixed SpaceCenterCamera");
                }
                else
                    Debug.Log("[Kopernicus] ERROR fixing space center camera, could not find some fields");
            }
        }

        // Patch FlightIntegrator
        void PatchFI()
        {
            Events.OnRuntimeUtilityPatchFI.Fire();
            ModularFlightIntegrator.RegisterCalculateSunBodyFluxOverride(KopernicusStar.SunBodyFlux);
        }

        // Transforms body references in the savegames
        void TransformBodyReferencesOnLoad(GameEvents.FromToAction<ProtoVessel, ConfigNode> data)
        {
            // Check if the config node is null
            if (data.to == null)
                return;
            ConfigNode orbit = data.to.GetNode("ORBIT");
            String bodyIdent = orbit.GetValue("IDENT");
            CelestialBody body = PSystemManager.Instance.localBodies.FirstOrDefault(b => b.Get<String>("identifier") == bodyIdent);
            if (body == null)
                return;
            orbit.SetValue("REF", body.flightGlobalsIndex);
        }

        // Transforms body references in the savegames
        void TransformBodyReferencesOnSave(GameEvents.FromToAction<ProtoVessel, ConfigNode> data)
        {
            // Save the reference to the real body
            if (data.to == null)
                return;
            ConfigNode orbit = data.to.GetNode("ORBIT");
            CelestialBody body = PSystemManager.Instance.localBodies.FirstOrDefault(b => b.flightGlobalsIndex == data.from.orbitSnapShot.ReferenceBodyIndex);
            if (body == null)
                return;
            orbit.AddValue("IDENT", body.Get<String>("identifier"));
        }

        // Remove the Handlers
        void OnDestroy()
        {
            GameEvents.onPartUnpack.Remove(OnPartUnpack);
            GameEvents.onLevelWasLoaded.Remove(FixCameras);
            GameEvents.onGUIRnDComplexSpawn.Remove(RDFixer);
        }
    }
}
