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
using Kopernicus.Components;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Asteroids;
using Kopernicus.OnDemand;
using Kopernicus.RuntimeUtility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kopernicus.Configuration
{
    // Class to manage and load configurations for Kopernicus
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Loader : IParserEventSubscriber
    {
        // Name of the config type which holds the body definition
        public const String BODY_NODE_NAME = "Body";

        // Name of the config type which holds the asteroid definition
        public const String ASTEROID_NODE_NAME = "Asteroid";

        // Name of the config type which holds the pqs subdivision definitions
        public const String PRESET_NODE_NAME = "Preset";

        // The returned PSystem
        public PSystem SystemPrefab { get; set; }

        // The name of the PSystem
        [ParserTarget("name")]
        public String Name
        {
            get { return SystemPrefab.systemName; }
            set { SystemPrefab.systemName = value; }
        }

        // TimeScale for the planets
        [ParserTarget("timeScale")]
        public NumericParser<Double> SystemTimeScale
        {
            get { return SystemPrefab.systemTimeScale; }
            set { SystemPrefab.systemTimeScale = value; }
        }

        // Scale of the System
        [ParserTarget("scale")]
        public NumericParser<Double> SystemScale
        {
            get { return SystemPrefab.systemScale; }
            set { SystemPrefab.systemScale = value; }
        }

        // Global Epoch setting
        [ParserTarget("Epoch")]
        public NumericParser<Double> Epoch
        {
            get { return Templates.Epoch; }
            set { Templates.Epoch = value; }
        }

        // If the OnDemand Systems are enabled
        [ParserTarget("useOnDemand")]
        public NumericParser<Boolean> UseOnDemand
        {
            get { return OnDemandStorage.UseOnDemand; }
            set { OnDemandStorage.UseOnDemand = value; }
        }

        // If the OnDemand Biome Systems are enabled
        [ParserTarget("useOnDemandBiomes")]
        public NumericParser<Boolean> UseOnDemandBiomes
        {
            get { return OnDemandStorage.UseOnDemandBiomes; }
            set { OnDemandStorage.UseOnDemandBiomes = value; }
        }

        // If the OnDemand System should load missing maps
        [ParserTarget("onDemandLoadOnMissing")]
        public NumericParser<Boolean> OnDemandLoadOnMissing
        {
            get { return OnDemandStorage.OnDemandLoadOnMissing; }
            set { OnDemandStorage.OnDemandLoadOnMissing = value; }
        }

        // If the OnDemand System should write a debug message when a texture is missing
        [ParserTarget("onDemandLogOnMissing")]
        public NumericParser<Boolean> OnDemandLogOnMissing
        {
            get { return OnDemandStorage.OnDemandLogOnMissing; }
            set { OnDemandStorage.OnDemandLogOnMissing = value; }
        }

        // Set this to the unload delay in seconds
        [ParserTarget("onDemandUnloadDelay")]
        public NumericParser<Int32> OnDemandUnloadDelay
        {
            get { return OnDemandStorage.OnDemandUnloadDelay; }
            set { OnDemandStorage.OnDemandUnloadDelay = value; }
        }

        // Whether the experimental memory management in OD should be used
        [ParserTarget("useManualMemoryManagement")]
        public NumericParser<Boolean> UseManualMemoryManagement
        {
            get { return OnDemandStorage.UseManualMemoryManagement; }
            set { OnDemandStorage.UseManualMemoryManagement = value; }
        }

        // The body that is displayed at main menu
        [ParserTarget("mainMenuBody")]
        public String MainMenuBody
        {
            get { return Templates.MenuBody; }
            set { Templates.MenuBody = value; }
        }

        // The maximum viewing distance in tracking station
        [ParserTarget("maxViewingDistance")]
        public NumericParser<Double> MaxViewDistance
        {
            get { return Templates.MaxViewDistance; }
            set { Templates.MaxViewDistance = value; }
        }

        // Fade multiplier for tracking station
        [ParserTarget("scaledSpaceFaderMult")]
        public NumericParser<Double> ScaledSpaceFaderMult
        {
            get { return ScaledSpaceFader.faderMult; }
            set { ScaledSpaceFader.faderMult = value; }
        }

        // Whether to force enable 3D rendering of orbit lines
        [ParserTarget("force3DOrbits")]
        public NumericParser<Boolean> Force3DOrbits
        {
            get { return Templates.Force3DOrbits; }
            set { Templates.Force3DOrbits = value; }
        }

        // Remove Launch Sites added by DLCs
        // * Island_Airfield
        // * Woomerang_Launch_Site
        // * Woomerang_GroundObjects
        // * Desert_Launch_Site
        // * Desert_Airfield
        // * Desert_GroundObjects
        [ParserTargetCollection("self", Key = "removeLaunchSites", AllowMerge = false,
            NameSignificance = NameSignificance.Key)]
        public List<StringCollectionParser> RemoveLaunchSites
        {
            get { return new List<StringCollectionParser> { new StringCollectionParser(Templates.RemoveLaunchSites) }; }
            set { Templates.RemoveLaunchSites = value.SelectMany(v => v.Value).ToList(); }
        }

        // Create the new planetary system
        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            // If we already have a PSystem, abort
            if (SystemPrefab != null)
            {
                return;
            }

            // Create the PSystem
            GameObject gameObject = new GameObject("Kopernicus");
            gameObject.transform.parent = Utility.Deactivator;
            SystemPrefab = gameObject.AddComponent<PSystem>();

            // Set the planetary system defaults (pulled from PSystemManager.Instance.systemPrefab)
            SystemPrefab.systemName = "Kopernicus";
            SystemPrefab.systemTimeScale = 1.0;
            SystemPrefab.systemScale = 1.0;
            SystemPrefab.mainToolbarSelected = 2; // initial value in stock systemPrefab. Unknown significance.

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
            List<Body> bodies = new List<Body>();
            using (Logger logger = new Logger())
            {

                // Load all of the bodies
                foreach (ConfigNode bodyNode in node.GetNodes(BODY_NODE_NAME))
                {
                    // Grab the name of the body
                    String name = bodyNode.GetValue("name");
                    if (bodyNode.HasValue("identifier"))
                    {
                        name = bodyNode.GetValue("identifier");
                    }

                    if (seen.Contains(name))
                    {
                        Logger.Default.Log("[Kopernicus::PostApply] Skipped duplicate body " + name);
                        continue; //next body, please
                    }
                    seen.Add(name);

                    try
                    {
                        // Create a logfile for this body
                        logger.SetFilename(name + ".Body");
                        logger.SetAsActive();

                        // Attempt to create the body
                        Body currentBody = new Body();
                        Parser.SetState("Kopernicus:currentBody", () => currentBody);
                        Parser.LoadObjectFromConfigurationNode(currentBody, bodyNode, "Kopernicus"); //logs to active logger
                        bodies.Add(currentBody);
                        Events.OnLoaderLoadBody.Fire(currentBody, bodyNode);
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Loaded Body: " + currentBody.Name);
                        Parser.ClearState("Kopernicus:currentBody");

                        // Restore default logger
                        Logger.Default.SetAsActive();
                        logger.Close(); //implicit flush
                    }
                    catch (Exception e)
                    {
                        logger.LogException(e);
                        logger.Close(); //implicit flush
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Failed to load Body: " + name + ": " +
                                           e.Message);
                        throw new Exception("Failed to load Body: " + name);
                    }
                }

                seen.Clear();

                // Load all of the asteroids
                foreach (ConfigNode asteroidNode in node.GetNodes(ASTEROID_NODE_NAME))
                {
                    if (seen.Contains(asteroidNode.GetValue("name")))
                    {
                        Logger.Default.Log("[Kopernicus::PostApply] Skipped duplicate asteroid " +
                                           asteroidNode.GetValue("name"));
                        continue; //next roid, please
                    }
                    seen.Add(asteroidNode.GetValue("name"));

                    try
                    {
                        // Create a logfile for this asteroid
                        logger.SetFilename(asteroidNode.GetValue("name") + ".Asteroid");
                        logger.SetAsActive();

                        // Attempt to create the Asteroid
                        Asteroid asteroid =
                        Parser.CreateObjectFromConfigNode<Asteroid>(asteroidNode, "Kopernicus"); //logs to active logger
                        DiscoverableObjects.Asteroids.Add(asteroid);
                        Events.OnLoaderLoadAsteroid.Fire(asteroid, asteroidNode);
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Loaded Asteroid: " + asteroid.Name);

                        // Restore default logger
                        Logger.Default.SetAsActive();
                        logger.Close(); //implicit flush
                    }
                    catch (Exception e)
                    {
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Failed to load Asteroid: " +
                                           asteroidNode.GetValue("name") + ": " + e.Message);
                        logger.LogException(e);
                        logger.Close();
                        throw new Exception("Failed to load Asteroid: " + asteroidNode.GetValue("name"));
                    }

                }

                seen.Clear();
                logger.Close();

                // Load all of the PQS Presets                
                foreach (ConfigNode presetNode in node.GetNodes(PRESET_NODE_NAME))
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
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Failed to load Preset: " +
                                           presetNode.GetValue("name"));
                        throw new Exception("Failed to load Asteroid: " + presetNode.GetValue("name"));
                    }
                }

                // Register UBIs for all bodies
                CelestialBody[] localBodies = bodies.Select(b => b.GeneratedBody.celestialBody).ToArray();
                foreach (Body body in bodies)
                {
                    // Register the primary UBI
                    if (!String.IsNullOrEmpty(body.Identifier))
                    {
                        UBI.RegisterUBI(body.CelestialBody, body.Identifier, localBodies: localBodies);
                    }

                    // Register all implemented UBIs
                    foreach (String ubi in body.Implements.SelectMany(u => u.Value).Distinct())
                    {
                        if (!String.IsNullOrEmpty(ubi))
                        {
                            UBI.RegisterUBI(body.CelestialBody, ubi, true, localBodies);
                        }
                    }
                }

                // Event
                Events.OnLoaderLoadedAllBodies.Fire(this, node);

                // Glue all the orbits together in the defined pattern
                foreach (Body body in bodies)
                {
                    // If this body is in orbit around another body
                    if (body.Orbit != null)
                    {
                        // Convert a UBI reference into a normal one
                        String referenceBody = UBI.GetName(body.Orbit.ReferenceBody, localBodies);

                        // Get the Body object for the reference body
                        Body parent = bodies.Find(b => b.Name == referenceBody);
                        if (parent == null)
                        {
                            throw new Exception("Reference body for \"" + (body.Identifier ?? body.Name) +
                                                "\" could not be found. Missing body name is \"" +
                                                body.Orbit.ReferenceBody + "\".");
                        }

                        // Setup the orbit of the body
                        parent.GeneratedBody.children.Add(body.GeneratedBody);
                        body.GeneratedBody.orbitDriver.referenceBody = parent.GeneratedBody.celestialBody;
                        body.GeneratedBody.orbitDriver.orbit.referenceBody = parent.GeneratedBody.celestialBody;
                    }

                    // Parent the generated body to the PSystem
                    body.GeneratedBody.transform.parent = SystemPrefab.transform;

                    // Delete ghost space centers
                    if (!body.GeneratedBody.celestialBody.isHomeWorld && body.GeneratedBody.pqsVersion != null)
                    {
                        SpaceCenter[] centers = body.GeneratedBody.pqsVersion.GetComponentsInChildren<SpaceCenter>(true);
                        if (centers != null)
                        {
                            foreach (SpaceCenter c in centers)
                            {
                                Object.Destroy(c);
                            }
                        }
                    }

                    // Event
                    Events.OnLoaderFinalizeBody.Fire(body);
                }

                // Elect root body
                SystemPrefab.rootBody = bodies.First(p => p.Orbit == null).GeneratedBody;

                // Try to find a home world
                Body home = bodies.FirstOrDefault(p => p.GeneratedBody.celestialBody.isHomeWorld);
                if (home == null)
                {
                    throw new Exception("Homeworld body could not be found.");
                }

                // Sort by distance from parent (discover how this effects local bodies)
                Utility.DoRecursive(SystemPrefab.rootBody, body => body.children, body => body.children = body.children
                    .OrderBy(b => b.orbitDriver.orbit.semiMajorAxis * (1 + b.orbitDriver.orbit.eccentricity)).ToList());

                // Fix doubled flightGlobals
                List<Int32> numbers = new List<Int32> {0, 1};
                Int32 index = bodies.Sum(b => b.GeneratedBody.flightGlobalsIndex);
                Utility.DoRecursive(SystemPrefab.rootBody, body => body.children, body =>
                {
                    // ReSharper disable AccessToModifiedClosure
                    if (numbers.Contains(body.flightGlobalsIndex))
                    {
                        body.flightGlobalsIndex = index++;
                    }

                    if (body.celestialBody.isHomeWorld)
                    {
                        body.flightGlobalsIndex = 1; // Kerbin
                    }

                    if (body == SystemPrefab.rootBody)
                    {
                        body.flightGlobalsIndex = 0; // Sun
                    }

                    numbers.Add(body.flightGlobalsIndex);
                    // ReSharper restore AccessToModifiedClosure
                });

                // Event
                Events.OnLoaderPostApply.Fire(this, node);
            }
        }
    }
}

