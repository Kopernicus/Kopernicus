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

using Kopernicus.Configuration;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Kopernicus
{
    // Hook the PSystemSpawn (creation of the planetary system) event in the KSP initialization lifecycle
    [KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class Injector : MonoBehaviour
    {
        // Name of the config node group which manages Kopernicus
        public const String rootNodeName = "Kopernicus";

        // Custom Assembly query since AppDomain and Assembly loader are not quite what we want in 1.1
        [Obsolete("Please use Parser.ModTypes", true)]
        public static List<Type> ModTypes
        {
            get { return Parser.ModTypes; }
        }

        // Backup of the old system prefab, in case someone deletes planet templates we need at Runtime (Kittopia)
        public static PSystem StockSystemPrefab { get; set; }

        // Awake() is the first function called in the lifecycle of a Unity3D MonoBehaviour.  In the case of KSP,
        // it happens to be called right before the game's PSystem is instantiated from PSystemManager.Instance.systemPrefab
        public void Awake()
        {
            // Abort, if KSP isn't compatible
            if (!CompatibilityChecker.IsCompatible())
            {
                String supported = CompatibilityChecker.version_major + "." + CompatibilityChecker.version_minor + "." + CompatibilityChecker.Revision;
                String current = Versioning.version_major + "." + Versioning.version_minor + "." + Versioning.Revision;
                Debug.LogWarning("[Kopernicus] Detected incompatible install.\nCurrent version of KSP: " + current + ".\nSupported version of KSP: " + supported + ".\nPlease wait, until Kopernicus gets updated to match your version of KSP.");
                Debug.Log("[Kopernicus] Aborting...");

                // Abort
                Destroy(this);
                return;
            }
            else
            {
                String kopernicusVersion = CompatibilityChecker.version_major + "." + CompatibilityChecker.version_minor + "." + CompatibilityChecker.Revision + "-" + CompatibilityChecker.Kopernicus;
                String kspVersion = Versioning.version_major + "." + Versioning.version_minor + "." + Versioning.Revision;
                Debug.Log("[Kopernicus] Running Kopernicus " + kopernicusVersion + " on KSP " + kspVersion);
            }

            // Wrap this in a try - catch block so we can display a warning if Kopernicus fails to load for some reason
            try
            {
                // We're ALIVE
                Logger.Default.SetAsActive();
                Logger.Default.Log("Injector.Awake(): Begin");

                // Parser Config
                ParserOptions.Register("Kopernicus", new ParserOptions.Data { errorCallback = e => Logger.Active.LogException(e), logCallback = s => Logger.Active.Log(s) });

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

                // Backup the old prefab
                StockSystemPrefab = PSystemManager.Instance.systemPrefab;

                // Fire Pre-Load Event
                Events.OnPreLoad.Fire();

                // Get the current time
                DateTime start = DateTime.Now;

                // Get the configNode
                ConfigNode kopernicus = GameDatabase.Instance.GetConfigs(rootNodeName)[0].config;

                // THIS IS WHERE THE MAGIC HAPPENS - OVERWRITE THE SYSTEM PREFAB SO KSP ACCEPTS OUR CUSTOM SOLAR SYSTEM AS IF IT WERE FROM SQUAD
                PSystemManager.Instance.systemPrefab = Parser.CreateObjectFromConfigNode<Loader>(kopernicus, "Kopernicus").systemPrefab;

                // Clear space center instance so it will accept nouveau Kerbin
                SpaceCenter.Instance = null;

                // Add a handler so that we can do post spawn fixups.
                PSystemManager.Instance.OnPSystemReady.Add(PostSpawnFixups);

                // Fire Post-Load Event
                Events.OnPostLoad.Fire(PSystemManager.Instance.systemPrefab);

                // Done executing the awake function
                TimeSpan duration = (DateTime.Now - start);
                Logger.Default.Log("Injector.Awake(): Completed in: " + duration.TotalMilliseconds + " ms");
                Logger.Default.Flush();
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
                SpaceCenter.Instance = PSystemManager.Instance.localBodies.First(cb => cb.isHomeWorld).GetComponentsInChildren<SpaceCenter>(true).FirstOrDefault();
                SpaceCenter.Instance.Start();

                // Fix the flight globals index of each body and patch it's SOI
                Int32 counter = 0;
                foreach (CelestialBody body in FlightGlobals.Bodies)
                {
                    // Event
                    Events.OnPreBodyFixing.Fire(body);

                    // Patch the flightGlobalsIndex
                    body.flightGlobalsIndex = counter++;

                    // Finalize the Orbit
                    if (body.Has("finalizeBody"))
                        OrbitLoader.FinalizeOrbit(body);

                    // Patch the SOI
                    if (body.Has("sphereOfInfluence"))
                        body.sphereOfInfluence = body.Get<Double>("sphereOfInfluence");

                    // Patch the Hill Sphere
                    if (body.Has("hillSphere"))
                        body.hillSphere = body.Get<Double>("hillSphere");

                    // Make the Body a barycenter
                    if (body.Has("barycenter"))
                        body.scaledBody.SetActive(false);

                    // Event
                    Events.OnPostBodyFixing.Fire(body);

                    // Log
                    Logger.Default.Log("Found Body: " + body.bodyName + ":" + body.flightGlobalsIndex + " -> SOI = " + body.sphereOfInfluence + ", Hill Sphere = " + body.hillSphere);
                }

                // Fix the maximum viewing distance of the map view camera (get the farthest away something can be from the root object)
                PSystemBody rootBody = PSystemManager.Instance.systemPrefab.rootBody;
                Double maximumDistance = 1000d; // rootBody.children.Max(b => (b.orbitDriver != null) ? b.orbitDriver.orbit.semiMajorAxis * (1 + b.orbitDriver.orbit.eccentricity) : 0);
                if (rootBody != null)
                {
                    maximumDistance = rootBody.celestialBody.Radius * 100d;
                    if (rootBody.children != null && rootBody.children.Count > 0)
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
                if (Templates.maxViewDistance >= 0)
                {
                    maximumDistance = Templates.maxViewDistance;
                    Debug.Log("Found max distance override " + maximumDistance);
                }
                else
                    Debug.Log("Found max distance " + maximumDistance);
                PlanetariumCamera.fetch.maxDistance = ((Single)maximumDistance * 3.0f) / ScaledSpace.Instance.scaleFactor;

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
            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "KopernicusFail", "Warning", "Kopernicus was not able to load the custom planetary system due to an exception in the loading process.\n" +
                "Loading your savegame is NOT recommended, because the missing planets could corrupt it and delete your progress.\n\n" +
                "Please contact the planet pack author or the Kopernicus team about the issue and send them a valid bugreport, including your KSP.log, your ModuleManager.ConfigCache file and the folder Logs/Kopernicus/ from your KSP root directory.\n\n", "OK", true, UISkinManager.GetSkin("MainMenuSkin"));
        }

        // Log the destruction of the injector
        public void OnDestroy()
        {
            Logger.Default.Log("Injector.OnDestroy(): Complete");
            Logger.Default.Flush();
        }
    }
}
