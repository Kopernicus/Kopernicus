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
using System.Diagnostics.CodeAnalysis;
// ReSharper disable once RedundantUsingDirective
using System.IO;
using System.Linq;
// ReSharper disable once RedundantUsingDirective
using System.Security.Cryptography;
using Kopernicus.ConfigParser;
using Kopernicus.Configuration;
using Kopernicus.Constants;
using UnityEngine;

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
        private const String CONFIG_CHECKSUM = "73eb1037678bc520a0fe2e89768e0549b36f17a9cac7136af6e3e6b7a0ccf9b9";

        // Backup of the old system prefab, in case someone deletes planet templates we need at Runtime (Kittopia)
        public static PSystem StockSystemPrefab { get; private set; }

        // Whether the injector is currently patching the prefab
        public static Boolean IsInPrefab { get; private set; }

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
                        {ErrorCallback = e => Logger.Active.LogException(e), LogCallback = s => Logger.Active.Log(s)});

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

                // Was the system template modified?
                #if !DEBUG
                String systemCfgPath = KSPUtil.ApplicationRootPath + "GameData/Kopernicus/Config/System.cfg";
                if (File.Exists(systemCfgPath))
                {
                    Byte[] data = File.ReadAllBytes(systemCfgPath);
                    SHA256 sha256 = SHA256.Create();
                    String checksum = BitConverter.ToString(sha256.ComputeHash(data));
                    checksum = checksum.Replace("-", "");
                    checksum = checksum.ToLower();
                    if (checksum != CONFIG_CHECKSUM)
                    {
                        throw new Exception(
                            "The file 'Kopernicus/Config/System.cfg' was modified directly without ModuleManager");
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
                SpaceCenter.Instance = PSystemManager.Instance.localBodies.First(cb => cb.isHomeWorld)
                    .GetComponentsInChildren<SpaceCenter>(true).FirstOrDefault();
                if (SpaceCenter.Instance != null)
                {
                    SpaceCenter.Instance.Start();
                }

                // Fix the flight globals index of each body and patch it's SOI
                Int32 counter = 0;
                foreach (CelestialBody body in FlightGlobals.Bodies)
                {
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
                        body.scaledBody.SetActive(false);
                    }

                    // Make the bodies scaled space invisible 
                    if (body.Get("invisibleScaledSpace", false))
                    {
                        foreach (Renderer renderer in body.scaledBody.GetComponentsInChildren<Renderer>(true))
                        {
                            renderer.enabled = false;
                        }

                        foreach (ScaledSpaceFader fader in body.scaledBody.GetComponentsInChildren<ScaledSpaceFader>(
                            true))
                        {
                            fader.enabled = false;
                        }
                    }

                    // Event
                    Events.OnPostBodyFixing.Fire(body);

                    // Log
                    Logger.Default.Log("Found Body: " + body.bodyName + ":" + body.flightGlobalsIndex + " -> SOI = " +
                                       body.sphereOfInfluence + ", Hill Sphere = " + body.hillSphere);
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
                    (Single) maximumDistance * 3.0f / ScaledSpace.Instance.scaleFactor;

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
            Logger.Default.Log("Injector.OnDestroy(): Complete");
            Logger.Default.Flush();
        }
    }
}
