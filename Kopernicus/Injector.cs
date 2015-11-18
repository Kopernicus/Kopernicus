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

using System;
using System.Linq;
using UnityEngine;
using Kopernicus.Configuration;

namespace Kopernicus 
{
    // Hook the PSystemSpawn (creation of the planetary system) event in the KSP initialization lifecycle
    [KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class Injector : MonoBehaviour 
    {
        // Name of the config node group which manages Kopernicus
        public const string rootNodeName = "Kopernicus";

        // Things that are used often
        public Templates templates = null;
        
        // Awake() is the first function called in the lifecycle of a Unity3D MonoBehaviour.  In the case of KSP,
        // it happens to be called right before the game's PSystem is instantiated from PSystemManager.Instance.systemPrefab
        public void Awake()
        {
            // Abort, if KSP isn't compatible
            if (!CompatibilityChecker.IsCompatible())
            {
                string supported = CompatibilityChecker.version_major + "." + CompatibilityChecker.version_minor + "." + CompatibilityChecker.Revision;
                string current = Versioning.version_major + "." + Versioning.version_minor + "." + Versioning.Revision;
                Debug.LogWarning("[Kopernicus] Detected incompatible install.\nCurrent version of KSP: " + current + ".\nSupported version of KSP: " + supported + ".\nPlease wait, until Kopernicus gets updated to match your version of KSP.");
                Debug.Log("[Kopernicus] Aborting...");

                // Abort
                Destroy(this);
                return;
            }

            // We're ALIVE
            Logger.Initialize();
            Logger.Default.SetAsActive();
            Logger.Default.Log("Injector.Awake(): Begin");

            // Yo garbage collector - we have work to do man
            DontDestroyOnLoad(this);

            // If the planetary manager does not work, well, error out
            if (PSystemManager.Instance == null) 
            {
                // Log the error
                Logger.Default.Log("Injector.Awake(): If PSystemManager.Instance is null, there is nothing to do");
                return;
            }

            // Get the current time
            DateTime start = DateTime.Now;

            // Grab templates
            templates = new Templates();

            // Get the configNode
            ConfigNode kopernicus = GameDatabase.Instance.GetConfigs(rootNodeName)[0].config;

            // THIS IS WHERE THE MAGIC HAPPENS - OVERWRITE THE SYSTEM PREFAB SO KSP ACCEPTS OUR CUSTOM SOLAR SYSTEM AS IF IT WERE FROM SQUAD
            PSystemManager.Instance.systemPrefab = Parser.CreateObjectFromConfigNode<Loader>(kopernicus).systemPrefab;

            // SEARCH FOR THE ARCHIVES CONTROLLER PREFAB AND OVERWRITE IT WITH THE CUSTOM SYSTEM
            RDArchivesController archivesController = AssetBase.RnDTechTree.GetRDScreenPrefab ().GetComponentsInChildren<RDArchivesController> (true).First ();
            archivesController.systemPrefab = PSystemManager.Instance.systemPrefab;

            // Clear space center instance so it will accept nouveau Kerbin
            SpaceCenter.Instance = null;

            // Add a handler so that we can do post spawn fixups.  
            PSystemManager.Instance.OnPSystemReady.Add(PostSpawnFixups);

            // Done executing the awake function
            TimeSpan duration = (DateTime.Now - start);
            Logger.Default.Log("Injector.Awake(): Completed in: " + duration.TotalMilliseconds + " ms");
            Logger.Default.Flush ();
        }

        // Post spawn fixups (ewwwww........)
        public void PostSpawnFixups()
        {
            Debug.Log("[Kopernicus]: Post-Spawn");

            // Fix the flight globals index of each body and patch it's SOI
            int counter = 0;
            foreach (CelestialBody body in FlightGlobals.Bodies) 
            {
                // Patch the flightGlobalsIndex
                body.flightGlobalsIndex = counter++;

                // Finalize the Orbit
                if (Templates.finalizeBodies.Contains(body.transform.name))
                    OrbitLoader.FinalizeOrbit(body);

                // Patch the SOI
                if (Templates.sphereOfInfluence.ContainsKey(body.transform.name))
                    body.sphereOfInfluence = Templates.sphereOfInfluence[body.transform.name];

                // Patch the Hill Sphere
                if (Templates.hillSphere.ContainsKey(body.transform.name))
                    body.hillSphere = Templates.hillSphere[body.transform.name];

                // Make the Body a barycenter
                if (Templates.barycenters.Contains(body.transform.name))
                    body.scaledBody.SetActive(false);

                // Apply Orbit mode changes
                if (Templates.drawMode.ContainsKey(body.transform.name))
                    body.orbitDriver.Renderer.drawMode = Templates.drawMode[body.transform.name];

                // Apply Orbit icon changes
                if (Templates.drawIcons.ContainsKey(body.transform.name))
                    body.orbitDriver.Renderer.drawIcons = Templates.drawIcons[body.transform.name];

                Logger.Default.Log ("Found Body: " + body.bodyName + ":" + body.flightGlobalsIndex + " -> SOI = " + body.sphereOfInfluence + ", Hill Sphere = " + body.hillSphere);
            }

            // Fix the maximum viewing distance of the map view camera (get the farthest away something can be from the root object)
            PSystemBody rootBody = PSystemManager.Instance.systemPrefab.rootBody;
            double maximumDistance = 1000d; // rootBody.children.Max(b => (b.orbitDriver != null) ? b.orbitDriver.orbit.semiMajorAxis * (1 + b.orbitDriver.orbit.eccentricity) : 0);
            if (rootBody != null)
            {
                maximumDistance = rootBody.celestialBody.Radius * 100d;
                if(rootBody.children != null && rootBody.children.Count > 0)
                {
                    foreach (PSystemBody body in rootBody.children)
                    {
                        if (body.orbitDriver != null)
                            maximumDistance = Math.Max(maximumDistance, body.orbitDriver.orbit.semiMajorAxis * (1d + body.orbitDriver.orbit.eccentricity));
                        else
                            Debug.Log("[Kopernicus]: Body " + body.name + " has no orbitdriver!");
                    }
                }
                else
                    Debug.Log("[Kopernicus]: Root body children null or 0");
            }
            else
                Debug.Log("[Kopernicus]: Root body null!");
            Debug.Log("Found max distance " + maximumDistance);
            PlanetariumCamera.fetch.maxDistance = ((float)maximumDistance * 3.0f) / ScaledSpace.Instance.scaleFactor;

            // Update Music Logic
            MusicLogic.fetch.flightMusicSpaceAltitude = FlightGlobals.GetHomeBody().atmosphereDepth;

            // Select the closest star to home
            StarLightSwitcher.HomeStar().SetAsActive ();

            // Flush the logger
            Logger.Default.Flush();

            // Fixups complete, time to surrender to fate
            Destroy (this);
        }

        // Log the destruction of the injector
        public void OnDestroy()
        {
            Logger.Default.Log("Injector.OnDestroy(): Complete");
            Logger.Default.Flush();
        }
    }
}

