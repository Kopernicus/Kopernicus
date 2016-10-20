/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */
 
using UnityEngine;
using Kopernicus.Components;
using System;
using System.Reflection;
using System.Linq;
using KSP.UI.Screens;
using ModularFI;

namespace Kopernicus
{
    // Mod runtime utilitues
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RuntimeUtility : MonoBehaviour
    {
        // Variables
        public MapObject previous;

        // Awake() - flag this class as don't destroy on load and register delegates
        void Awake ()
        {
            // Don't run if Kopernicus isn't compatible
            if (!CompatibilityChecker.IsCompatible())
            {
                Destroy(this);
                return;
            }

            // Make sure the runtime utility isn't killed
            DontDestroyOnLoad (this);

            // Add handlers
            GameEvents.onPartUnpack.Add(OnPartUnpack);
            GameEvents.onLevelWasLoaded.Add(FixCameras);
            GameEvents.onLevelWasLoaded.Add(delegate (GameScenes scene)
            {
                if (HighLogic.LoadedSceneHasPlanetarium && MapView.fetch != null)
                    MapView.fetch.max3DlineDrawDist = 20000f;
                if (scene == GameScenes.MAINMENU)
                    UpdateMenu();
                if (scene == GameScenes.SPACECENTER)
                    PatchFI();
                foreach (CelestialBody body in PSystemManager.Instance.localBodies)
                {
                    GameObject star_ = KopernicusStar.GetNearest(body).gameObject;
                    if (body.afg != null)
                        body.afg.sunLight = star_;
                    if (body.scaledBody.GetComponent<MaterialSetDirection>() != null)
                        body.scaledBody.GetComponent<MaterialSetDirection>().target = star_.transform;
                    foreach (PQSMod_MaterialSetDirection msd in body.GetComponentsInChildren<PQSMod_MaterialSetDirection>(true))
                        msd.target = star_.transform;
                }
                foreach (TimeOfDayAnimation anim in Resources.FindObjectsOfTypeAll<TimeOfDayAnimation>())
                    anim.target = KopernicusStar.GetNearest(FlightGlobals.GetHomeBody()).gameObject.transform;
            });

            // Update Music Logic
            if (MusicLogic.fetch != null && FlightGlobals.fetch != null && FlightGlobals.GetHomeBody() != null)
                MusicLogic.fetch.flightMusicSpaceAltitude = FlightGlobals.GetHomeBody().atmosphereDepth;

            // Log
            Logger.Default.Log ("[Kopernicus] RuntimeUtility Started");
            Logger.Default.Flush ();
        }

        // Execute MainMenu functions
        void Start()
        {
            UpdateMenu();
            previous = PlanetariumCamera.fetch.initialTarget;
            PlanetariumCamera.fetch.targets
                .Where(m => Templates.barycenters.Contains(m.GetName()) || Templates.notSelectable.Contains(m.GetName()))
                .ToList()
                .ForEach(map => PlanetariumCamera.fetch.targets.Remove(map));

            // Stars
            GameObject gob = Sun.Instance.gameObject;
            KopernicusStar star = gob.AddComponent<KopernicusStar>();
            Utility.CopyObjectFields(Sun.Instance, star, false);
            DestroyImmediate(Sun.Instance);
            Sun.Instance = star;

            // More stars
            foreach (CelestialBody body in PSystemManager.Instance.localBodies.Where(b => b.flightGlobalsIndex != 0 && b.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length > 0))
            {
                GameObject starObj = Instantiate(Sun.Instance.gameObject);
                KopernicusStar star_ = starObj.GetComponent<KopernicusStar>();
                star_.sun = body;
                starObj.transform.parent = Sun.Instance.transform.parent;
                starObj.name = body.name;
                starObj.transform.localPosition = Vector3.zero;
                starObj.transform.localRotation = Quaternion.identity;
                starObj.transform.localScale = Vector3.one;
                starObj.transform.position = body.position;
                starObj.transform.rotation = body.rotation;
            }
        }

        // Stuff
        void LateUpdate()
        {
            FixZooming();
            ApplyOrbitVisibility();
            RDFixer();

            foreach (CelestialBody body in PSystemManager.Instance.localBodies)
            {
                if (body.afg == null) continue;
                GameObject star_ = KopernicusStar.GetNearest(body).gameObject;
                Vector3 planet2cam = body.scaledBody.transform.position - body.afg.mainCamera.transform.position;
                body.afg.lightDot = Mathf.Clamp01(Vector3.Dot(planet2cam, body.afg.mainCamera.transform.position - star_.transform.position) * body.afg.dawnFactor);
                body.afg.GetComponent<Renderer>().material.SetFloat("_lightDot", body.afg.lightDot);
            }
        }

        // Status
        bool isDone = false;
        bool isDone2 = false;

        // Fix the Zooming-Out bug
        void FixZooming()
        {
            if (HighLogic.LoadedSceneHasPlanetarium && MapView.fetch != null && !isDone)
            {
                // Fix the bug via switching away from Home and back immideatly. 
                // TODO: Check if this still happend
                PlanetariumCamera.fetch.SetTarget(PlanetariumCamera.fetch.targets[(PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) + 1) % PlanetariumCamera.fetch.targets.Count]);
                PlanetariumCamera.fetch.SetTarget(PlanetariumCamera.fetch.targets[(PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) - 1) + (((PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) - 1) >= 0) ? 0 : PlanetariumCamera.fetch.targets.Count)]);

                // Terminate for the moment.
                isDone = true;
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
                    if (Templates.drawMode.ContainsKey(body.transform.name))
                        body.orbitDriver.Renderer.drawMode = Templates.drawMode[body.transform.name];

                    // Apply Orbit icon changes
                    if (Templates.drawIcons.ContainsKey(body.transform.name))
                        body.orbitDriver.Renderer.drawIcons = Templates.drawIcons[body.transform.name];
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

        // Update the menu body
        void UpdateMenu()
        {
            // Grab the main body
            CelestialBody planetCB = PSystemManager.Instance.localBodies.Find(b => b.transform.name == Templates.menuBody);
            PSystemBody planet = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, Templates.menuBody);
            if (planetCB == null || planet == null)
            {
                planet = Utility.FindHomeBody(PSystemManager.Instance.systemPrefab.rootBody);
                planetCB = PSystemManager.Instance.localBodies.Find(b => b.isHomeWorld);
            }
            if (planet == null || planetCB == null)
            {
                Debug.LogError("[Kopernicus] Could not find homeworld!");
                return;
            }

            // Get the MainMenu-Logic
            MainMenu main = FindObjectOfType<MainMenu>();
            if (main == null)
            {
                Debug.LogError("[Kopernicus] No main menu object!");
                return;
            }
            MainMenuEnvLogic logic = main.envLogic;

            // Set it to Space, because the Mun-Area won't work with sth else than Mun
            if (logic.areas.Length < 2)
            {
                Debug.LogError("[Kopernicus] Not enough bodies");
                return;
            }
            logic.areas[0].SetActive(false);
            logic.areas[1].SetActive(true);

            // Get our active Space
            GameObject space = logic.areas[1];

            // Deactivate Kerbins Transform
            Transform kerbin = space.transform.Find("Kerbin");
            if (kerbin == null)
            {
                Debug.LogError("[Kopernicus] No Kerbin transform!");
                return;
            }
            kerbin.gameObject.SetActive(false);

            // Deactivate Muns Transform
            Transform mun = space.transform.Find("MunPivot");
            if (mun == null)
            {
                Debug.LogError("[Kopernicus] No MunPivot transform!");
                return;
            }
            mun.gameObject.SetActive(false);

            // Clone the scaledVersion and attach it to the Scene
            GameObject menuPlanet = Instantiate(planet.scaledVersion) as GameObject;
            menuPlanet.transform.parent = space.transform;

            // Destroy stuff
            DestroyImmediate(menuPlanet.GetComponent<ScaledSpaceFader>());
            DestroyImmediate(menuPlanet.GetComponent<SphereCollider>());
            DestroyImmediate(menuPlanet.GetComponentInChildren<AtmosphereFromGround>());
            DestroyImmediate(menuPlanet.GetComponent<MaterialSetDirection>());

            // That sounds funny
            Rotato planetRotato = menuPlanet.AddComponent<Rotato>();
            Rotato planetRefRotato = kerbin.GetComponent<Rotato>();
            planetRotato.speed = (planetRefRotato.speed / 9284.50070356553f) * (float)planetCB.orbitDriver.orbit.orbitalSpeed; // calc.exe for the win

            // Scale the body
            menuPlanet.transform.localScale = kerbin.localScale;
            menuPlanet.transform.localPosition = kerbin.localPosition;
            menuPlanet.transform.position = kerbin.position;

            // And set it to layer 0
            menuPlanet.layer = 0;

            // Patch the material, because Mods like TextureReplacer run post spawn, and we'd overwrite their changes
            menuPlanet.GetComponent<Renderer>().sharedMaterial = planetCB.scaledBody.GetComponent<Renderer>().sharedMaterial;

            // Copy EVE 7.4 clouds / Rings
            for (int i = 0; i < planetCB.scaledBody.transform.childCount; i++)
            {
                // Just clone everything
                Transform t = planetCB.scaledBody.transform.GetChild(i);
                if (t.gameObject.GetComponent<AtmosphereFromGround>())
                    continue;
                GameObject newT = Instantiate(t.gameObject) as GameObject;
                newT.transform.parent = menuPlanet.transform;
                newT.layer = 0;
                newT.transform.localPosition = Vector3.zero;
                newT.transform.localRotation = Quaternion.identity;
                newT.transform.localScale = (float)(1008 / planetCB.Radius) * Vector3.one;
            }

            // And now, create the moons
            foreach (PSystemBody moon in planet.children)
            {
                // Grab the CeletialBody of the moon
                CelestialBody moonCB = PSystemManager.Instance.localBodies.Find(b => b.GetTransform().name == moon.name);

                // Create the Rotation-Transforms
                GameObject menuMoonPivot = new GameObject(moon.name + " Pivot");
                menuMoonPivot.gameObject.layer = 0;
                menuMoonPivot.transform.position = menuPlanet.transform.position;

                // Still funny...
                Rotato munRotato = menuMoonPivot.AddComponent<Rotato>();
                Rotato refRotato = mun.GetComponent<Rotato>();
                munRotato.speed = (refRotato.speed / 542.494239600754f) * (float)moonCB.GetOrbit().getOrbitalSpeedAtDistance(moonCB.GetOrbit().semiMajorAxis);

                // Clone the scaledVersion and attach it to the pivot
                GameObject menuMoon = Instantiate(moon.scaledVersion) as GameObject;
                menuMoon.transform.parent = menuMoonPivot.transform;

                // Move and scale the menuMoon correctly
                menuMoon.transform.localPosition = new Vector3(-5000f * (float)(moonCB.GetOrbit().semiMajorAxis / 12000000.0), 0f, 0f);
                menuMoon.transform.localScale *= 7f;

                // Destroy stuff
                DestroyImmediate(menuMoon.GetComponent<ScaledSpaceFader>());
                DestroyImmediate(menuMoon.GetComponent<SphereCollider>());
                DestroyImmediate(menuMoon.GetComponentInChildren<AtmosphereFromGround>());
                DestroyImmediate(menuMoon.GetComponent<MaterialSetDirection>());

                // More Rotato
                Rotato moonRotato = menuMoon.AddComponent<Rotato>();
                moonRotato.speed = -0.005f / (float)(moonCB.rotationPeriod / 400.0);

                // Apply orbital stuff
                menuMoon.transform.Rotate(0f, (float)moonCB.orbitDriver.orbit.LAN, 0f);
                menuMoon.transform.Rotate(0f, 0f, (float)moonCB.orbitDriver.orbit.inclination);
                menuMoon.transform.Rotate(0f, (float)moonCB.orbitDriver.orbit.argumentOfPeriapsis, 0f);

                // And set the layer to 0
                menuMoon.layer = 0;

                // Patch the material, because Mods like TextureReplacer run post spawn, and we'd overwrite their changes
                menuMoon.GetComponent<Renderer>().sharedMaterial = moonCB.scaledBody.GetComponent<Renderer>().sharedMaterial;

                // Copy EVE 7.4 clouds / Rings
                for (int i = 0; i < moonCB.scaledBody.transform.childCount; i++)
                {
                    Transform t = moonCB.scaledBody.transform.GetChild(i);
                    if (t.gameObject.GetComponent<AtmosphereFromGround>())
                        continue;
                    GameObject newT = Instantiate(t.gameObject) as GameObject;
                    newT.transform.parent = menuMoon.transform;
                    newT.layer = 0;
                    newT.transform.localPosition = Vector3.zero;
                    newT.transform.localRotation = Quaternion.identity;
                    newT.transform.localScale = (float)(1008 / moonCB.Radius) * Vector3.one;
                }
            }
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
                    double normalHeight = body.pqsController.GetSurfaceHeight((Vector3d)ksc.repositionRadial.normalized) - body.Radius;
                    if (ksc.repositionToSphereSurface)
                    {
                        normalHeight += ksc.repositionRadiusOffset;
                    }
                    cam.altitudeInitial = 0f - (float)normalHeight;
                }
                else
                {
                    cam.altitudeInitial = 0f - (float)ksc.repositionRadiusOffset;
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
                for (int i = 0; i < fields.Length; ++i)
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
            ModularFlightIntegrator.RegisterCalculateSunBodyFluxOverride(KopernicusStar.SunBodyFlux);
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

