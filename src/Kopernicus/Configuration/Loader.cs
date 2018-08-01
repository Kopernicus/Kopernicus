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

using Kopernicus.Components;
using Kopernicus.Configuration.Asteroids;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        // Class to manage and load configurations for Kopernicus
        [RequireConfigType(ConfigType.Node)]
        public class Loader : IParserEventSubscriber
        {
            // Name of the config type which holds the body definition
            public const String bodyNodeName = "Body";

            // Name of the config type which holds the asteroid definition
            public const String asteroidNodeName = "Asteroid";

            // Name of the config type which holds the pqs subdivision definitions
            public const String presetNodeName = "Preset";

            // Currently edited body
            public static Body currentBody { get; set; }

            // The returned PSystem
            public PSystem systemPrefab { get; set; }

            // The current instance of the Loader
            public static Loader Instance { get; set; }

            // The name of the PSystem
            [ParserTarget("name")]
            public String name
            {
                get { return systemPrefab.systemName; }
                set { systemPrefab.systemName = value; }
            }

            // TimeScale for the planets
            [ParserTarget("timeScale")]
            public NumericParser<Double> systemTimeScale
            {
                get { return systemPrefab.systemTimeScale; }
                set { systemPrefab.systemTimeScale = value; }
            }

            // Scale of the System
            [ParserTarget("scale")]
            public NumericParser<Double> systemScale
            {
                get { return systemPrefab.systemScale; }
                set { systemPrefab.systemScale = value; }
            }

            // Global Epoch setting
            [ParserTarget("Epoch")]
            public NumericParser<Double> epoch
            {
                get { return Templates.epoch; }
                set { Templates.epoch = value; }
            }

            // If the OnDemand Systems are enabled
            [ParserTarget("useOnDemand")]
            public NumericParser<Boolean> useOnDemand
            {
                get { return OnDemand.OnDemandStorage.useOnDemand; }
                set { OnDemand.OnDemandStorage.useOnDemand = value; }
            }

            // If the OnDemand System should load missing maps
            [ParserTarget("onDemandLoadOnMissing")]
            public NumericParser<Boolean> onDemandLoadOnMissing
            {
                get { return OnDemand.OnDemandStorage.onDemandLoadOnMissing; }
                set { OnDemand.OnDemandStorage.onDemandLoadOnMissing = value; }
            }

            // If the OnDemand System should write a debug message when a texture is missing
            [ParserTarget("onDemandLogOnMissing")]
            public NumericParser<Boolean> onDemandLogOnMissing
            {
                get { return OnDemand.OnDemandStorage.onDemandLogOnMissing; }
                set { OnDemand.OnDemandStorage.onDemandLogOnMissing = value; }
            }

            // Set this to the unload delay in seconds
            [ParserTarget("onDemandUnloadDelay")]
            public NumericParser<Int32> onDemandUnloadDelay
            {
                get { return OnDemand.OnDemandStorage.onDemandUnloadDelay; }
                set { OnDemand.OnDemandStorage.onDemandUnloadDelay = value; }
            }

            // Whether the experimental memory management in OD should be used
            [ParserTarget("useManualMemoryManagement")]
            public NumericParser<Boolean> useManualMemoryManagement
            {
                get { return OnDemand.OnDemandStorage.useManualMemoryManagement; }
                set { OnDemand.OnDemandStorage.useManualMemoryManagement = value; }
            }

            // The body that is displayed at main menu
            [ParserTarget("mainMenuBody")]
            public String mainMenuBody
            {
                get { return Templates.menuBody; }
                set { Templates.menuBody = value; }
            }

            // The maximum viewing distance in tracking station
            [ParserTarget("maxViewingDistance")]
            public NumericParser<Double> maxViewDistance
            {
                get { return Templates.maxViewDistance; }
                set { Templates.maxViewDistance = value; }
            }

            // Fade multiplier for tracking station
            [ParserTarget("scaledSpaceFaderMult")]
            public NumericParser<Double> scaledSpaceFaderMult
            {
                get { return ScaledSpaceFader.faderMult; }
                set { ScaledSpaceFader.faderMult = value; }
            }
            
            // Remove Launch Sites added by DLCs
            // * Island_Airfield
            // * Woomerang_Launch_Site
            // * Woomerang_GroundObjects
            // * Desert_Launch_Site
            // * Desert_Airfield
            // * Desert_GroundObjects
            [ParserTargetCollection("self", Key = "removeLaunchSites", AllowMerge = false, NameSignificance = NameSignificance.Key)]
            public List<StringCollectionParser> removeLaunchSites
            {
                get { return new List<StringCollectionParser> { new StringCollectionParser(Templates.RemoveLaunchSites)}; }
                set { Templates.RemoveLaunchSites = value.SelectMany(v => v.Value).ToList(); }
            }

            // Instance
            public Loader()
            {
                Instance = this;
            }

            // Create the new planetary system
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                // If we already have a PSystem, abort
                if (systemPrefab != null)
                    return;

                // Create the PSystem
                GameObject gameObject = new GameObject("Kopernicus");
                gameObject.transform.parent = Utility.Deactivator;
                systemPrefab = gameObject.AddComponent<PSystem>();

                // Set the planetary system defaults (pulled from PSystemManager.Instance.systemPrefab)
                systemPrefab.systemName = "Kopernicus";
                systemPrefab.systemTimeScale = 1.0;
                systemPrefab.systemScale = 1.0;
                systemPrefab.mainToolbarSelected = 2;   // initial value in stock systemPrefab. Unknown significance.

                // Load the ring shader
                ShaderLoader.LoadAssetBundle("Kopernicus/Shaders", "kopernicusshaders");

                // Event
                Events.OnLoaderApply.Fire(this, node);
            }

            // Generates the system prefab from the configuration 
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                //Set of items seen while loading, to prevent duplicate loggers being created (throws IOExceptions)
                HashSet<String> seen = new HashSet<String>();

                // Dictionary of bodies generated
                Dictionary<String, Body> bodies = new Dictionary<String, Body>();

                Logger logger = new Logger();

                // Load all of the bodies
                foreach (ConfigNode bodyNode in node.GetNodes(bodyNodeName))
                {
                    if ( seen.Contains(bodyNode.GetValue("name")) )
                    {
                        Logger.Default.Log("[Kopernicus::PostApply] Skipped duplicate body " + bodyNode.GetValue("name"));
                        continue; //next body, please
                    }
                    else
                    {
                        seen.Add(bodyNode.GetValue("name"));
                    }

                    try
                    {
                        // Create a logfile for this body
                        logger.SetFilename(bodyNode.GetValue("name") + ".Body");
                        logger.SetAsActive();

                        // Attempt to create the body
                        currentBody = new Body();
                        Parser.LoadObjectFromConfigurationNode(currentBody, bodyNode, "Kopernicus"); //logs to active logger
                        bodies.Add(currentBody.name, currentBody);
                        Events.OnLoaderLoadBody.Fire(currentBody, bodyNode);
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Loaded Body: " + currentBody.name);

                        // Restore default logger
                        Logger.Default.SetAsActive();
                        logger.Close(); //implicit flush
                    }
                    catch (Exception e)
                    {
                        logger.LogException(e);
                        logger.Close(); //implicit flush
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Failed to load Body: " + bodyNode.GetValue("name") + ": " + e.Message); 
                        throw new Exception("Failed to load Body: " + bodyNode.GetValue("name"));
                    }
                }
                seen.Clear();

                // Event
                Events.OnLoaderLoadedAllBodies.Fire(this, node);

                // Load all of the asteroids
                foreach (ConfigNode asteroidNode in node.GetNodes(asteroidNodeName))
                {
                    if (seen.Contains(asteroidNode.GetValue("name")))
                    {
                        Logger.Default.Log("[Kopernicus::PostApply] Skipped duplicate asteroid " + asteroidNode.GetValue("name"));
                        continue; //next roid, please
                    }
                    else
                    {
                        seen.Add(asteroidNode.GetValue("name"));
                    }
                    try
                    {
                        // Create a logfile for this asteroid
                        logger.SetFilename(asteroidNode.GetValue("name") + ".Asteroid");
                        logger.SetAsActive();

                        // Attempt to create the Asteroid
                        Asteroid asteroid = Parser.CreateObjectFromConfigNode<Asteroid>(asteroidNode, "Kopernicus"); //logs to active logger
                        DiscoverableObjects.asteroids.Add(asteroid);
                        Events.OnLoaderLoadAsteroid.Fire(asteroid, asteroidNode);
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Loaded Asteroid: " + asteroid.name);

                        // Restore default logger
                        Logger.Default.SetAsActive();
                        logger.Close(); //implicit flush
                    }
                    catch (Exception e)
                    {
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Failed to load Asteroid: " + asteroidNode.GetValue("name") + ": " + e.Message);
                        logger.LogException(e);
                        logger.Close();
                        throw new Exception("Failed to load Asteroid: " + asteroidNode.GetValue("name"));
                    }

                }
                seen.Clear();
                logger.Close();
                logger = null;

                // Load all of the PQS Presets                
                foreach (ConfigNode presetNode in node.GetNodes(presetNodeName))
                {
                    // Attempt to create the Preset
                    try
                    {
                        PQSCache.PQSPreset preset = new PQSCache.PQSPreset();
                        preset.Load(presetNode);
                        if (PQSCache.PresetList.presets.Any(p => p.name == preset.name))
                        {
                            Logger.Default.Log("[Kopernicus]: Configuration.Loader: Failed to load Preset: " + preset.name);
                            continue;
                        }
                        PQSCache.PresetList.presets.Add(preset);
                        
                        // Display name
                        String displayName = preset.name;
                        if (presetNode.HasValue("displayName"))
                        {
                            displayName = presetNode.GetValue("displayName");
                        }
                        Templates.PresetDisplayNames.Add(displayName);
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Loaded Preset: " + preset.name);
                    }
                    catch
                    {
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Failed to load Preset: " + presetNode.GetValue("name"));
                        throw new Exception("Failed to load Asteroid: " + presetNode.GetValue("name"));
                    }
                }

                // Glue all the orbits together in the defined pattern
                foreach (KeyValuePair<String, Body> body in bodies)
                {
                    // If this body is in orbit around another body
                    if (body.Value.orbit != null)
                    {
                        // Get the Body object for the reference body
                        if (!bodies.TryGetValue(body.Value.orbit.referenceBody, out Body parent))
                        {
                            throw new Exception("Reference body for \"" + body.Key + "\" could not be found. Missing body name is \"" + body.Value.orbit.referenceBody + "\".");
                        }

                        // Setup the orbit of the body
                        parent.generatedBody.children.Add(body.Value.generatedBody);
                        body.Value.generatedBody.orbitDriver.referenceBody = parent.generatedBody.celestialBody;
                        body.Value.generatedBody.orbitDriver.orbit.referenceBody = parent.generatedBody.celestialBody;
                    }

                    // Parent the generated body to the PSystem
                    body.Value.generatedBody.transform.parent = systemPrefab.transform;

                    // Delete ghost space centers
                    if (!body.Value.generatedBody.celestialBody.isHomeWorld && body.Value.generatedBody.pqsVersion != null)
                    {
                        SpaceCenter[] centers = body.Value.generatedBody.pqsVersion.GetComponentsInChildren<SpaceCenter>(true);
                        if (centers != null)
                        {
                            foreach (SpaceCenter c in centers)
                            {
                                UnityEngine.Object.Destroy(c);
                            }
                        }
                    }

                    // Set an unique identifier if there is noone
                    if (!body.Value.generatedBody.Has("identifier"))
                    {
                        body.Value.generatedBody.Set("identifier", body.Value.name);
                    }

                    // Event
                    Events.OnLoaderFinalizeBody.Fire(body.Value);
                }

                // Elect root body
                systemPrefab.rootBody = bodies.First(p => p.Value.orbit == null).Value.generatedBody;

                // Try to find a home world
                Body home = bodies.Values.FirstOrDefault(p => p.generatedBody.celestialBody.isHomeWorld);
                if (home == null)
                {
                    throw new Exception("Homeworld body could not be found.");
                }

                // Sort by distance from parent (discover how this effects local bodies)
                RecursivelySortBodies(systemPrefab.rootBody);
                
                // Fix doubled flightGlobals
                List<Int32> numbers = new List<Int32>() { 0, 1 };
                Int32 index = bodies.Sum(b => b.Value.generatedBody.flightGlobalsIndex);
                PatchFGI(ref numbers, ref index, systemPrefab.rootBody);

                // Check if all bodies have an unique identifier
                if (bodies.Any(b => bodies.Count(body => body.Value.identifier == b.Value.identifier) > 1))
                {
                    throw new InvalidOperationException("Found duplicated body identifiers!");
                }

                // We're done
                currentBody.generatedBody = null;

                // Event
                Events.OnLoaderPostApply.Fire(this, node);
            }

            // Sort bodies by distance from parent body
            public static void RecursivelySortBodies(PSystemBody body)
            {
                body.children = body.children.OrderBy(b => b.orbitDriver.orbit.semiMajorAxis * (1 + b.orbitDriver.orbit.eccentricity)).ToList();
                foreach (PSystemBody child in body.children)
                {
                    RecursivelySortBodies(child);
                }
            }

            // Patch the FlightGlobalsIndex of bodies
            public static void PatchFGI(ref List<Int32> numbers, ref Int32 index, PSystemBody rootBody)
            {
                foreach (PSystemBody body in rootBody.children)
                {
                    if (numbers.Contains(body.flightGlobalsIndex))
                        body.flightGlobalsIndex = index++;
                    if (body.celestialBody.isHomeWorld)
                        body.flightGlobalsIndex = 1; // Kerbin
                    numbers.Add(body.flightGlobalsIndex);
                    PatchFGI(ref numbers, ref index, body);
                }
            }
        }
    }
}

