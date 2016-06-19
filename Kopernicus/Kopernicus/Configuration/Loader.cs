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
using System.Collections.Generic;
using UnityEngine;
using Kopernicus.Configuration.Asteroids;

namespace Kopernicus
{
    namespace Configuration
    {
        // Class to manage and load configurations for Kopernicus
        public class Loader : IParserEventSubscriber
        {
            // Name of the config type which holds the body definition
            public const string bodyNodeName = "Body";

            // Name of the config type which holds the asteroid definition
            public const string asteroidNodeName = "Asteroid";

            // Currently edited body
            public static Body currentBody { get; set; }

            // The returned PSystem
            public PSystem systemPrefab { get; set; }

            // The current instance of the Loader
            public static Loader Instance { get; set; }

            // The name of the PSystem
            [ParserTarget("name")]
            public string name
            {
                get { return systemPrefab.systemName; }
                set { systemPrefab.systemName = value; }
            }

            // TimeScale for the planets
            [ParserTarget("timeScale")]
            public NumericParser<double> systemTimeScale
            {
                get { return systemPrefab.systemTimeScale; }
                set { systemPrefab.systemTimeScale = value; }
            }

            // Scale of the System
            [ParserTarget("scale")]
            public NumericParser<double> systemScale
            {
                get { return systemPrefab.systemScale; }
                set { systemPrefab.systemScale = value; }
            }

            // Global Epoch setting
            [ParserTarget("Epoch")]
            public NumericParser<double> epoch
            {
                get { return Templates.epoch; }
                set { Templates.epoch = value; }
            }

            // If the OnDemand Systems are enabled
            [ParserTarget("useOnDemand")]
            public NumericParser<bool> useOnDemand
            {
                get { return OnDemand.OnDemandStorage.useOnDemand; }
                set { OnDemand.OnDemandStorage.useOnDemand = value; }
            }

            // If the OnDemand System should load missing maps
            [ParserTarget("onDemandLoadOnMissing")]
            public NumericParser<bool> onDemandLoadOnMissing
            {
                get { return OnDemand.OnDemandStorage.onDemandLoadOnMissing; }
                set { OnDemand.OnDemandStorage.onDemandLoadOnMissing = value; }
            }

            // If the OnDemand System should write a debug message when a texture is missing
            [ParserTarget("onDemandLogOnMissing")]
            public NumericParser<bool> onDemandLogOnMissing
            {
                get { return OnDemand.OnDemandStorage.onDemandLogOnMissing; }
                set { OnDemand.OnDemandStorage.onDemandLogOnMissing = value; }
            }

            // Set this to the unload delay in seconds
            [ParserTarget("onDemandUnloadDelay")]
            public NumericParser<int> onDemandUnloadDelay
            {
                get { return OnDemand.OnDemandStorage.onDemandUnloadDelay; }
                set { OnDemand.OnDemandStorage.onDemandUnloadDelay = value; }
            }

            // The body that is displayed at main menu
            [ParserTarget("mainMenuBody")]
            public string mainMenuBody
            {
                get { return Templates.menuBody; }
                set { Templates.menuBody = value; }
            }

            // Whether the main menu body should be randomized
            public List<string> randomMainMenuBodies = new List<String>();

            // The maximum viewing distance in tracking station
            [ParserTarget("maxViewingDistance")]
            public NumericParser<double> maxViewDistance
            {
                get { return Templates.maxViewDistance; }
                set { Templates.maxViewDistance = value; }
            }

            // Fade multiplier for tracking station
            [ParserTarget("scaledSpaceFaderMult")]
            public NumericParser<double> scaledSpaceFaderMult
            {
                get { return ScaledSpaceFader.faderMult; }
                set { ScaledSpaceFader.faderMult = value; }
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
            }

            // Generates the system prefab from the configuration 
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                // Dictionary of bodies generated
                Dictionary<string, Body> bodies = new Dictionary<string, Body>();

                // Load all of the bodies
                foreach (ConfigNode bodyNode in node.GetNodes(bodyNodeName)) 
                {
                    // Create a logger for this body
                    Logger bodyLogger = new Logger(bodyNode.GetValue("name") + ".Body");
                    bodyLogger.SetAsActive();

                    // Attempt to create the body
                    try
                    {
                        currentBody = new Body();
                        Parser.LoadObjectFromConfigurationNode(currentBody, bodyNode);
                        bodies.Add(currentBody.name, currentBody);
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Loaded Body: " + currentBody.name);
                    } 
                    catch (Exception e) 
                    {
                        bodyLogger.LogException(e);
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Failed to load Body: " + bodyNode.GetValue("name"));
                    }

                    // Restore default logger
                    bodyLogger.Flush ();
                    Logger.Default.SetAsActive ();
                }

                // Load all of the asteroids                
                foreach (ConfigNode asteroidNode in node.GetNodes(asteroidNodeName))
                {
                    // Create a logger for this asteroid
                    Logger logger = new Logger(asteroidNode.GetValue("name") + ".Asteroid");
                    logger.SetAsActive();

                    // Attempt to create the Asteroid
                    try
                    {
                        Asteroid asteroid = Parser.CreateObjectFromConfigNode<Asteroid>(asteroidNode);
                        DiscoverableObjects.asteroids.Add(asteroid);
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Loaded Asteroid: " + asteroid.name);
                    }
                    catch (Exception e)
                    {
                        logger.LogException(e);
                        Logger.Default.Log("[Kopernicus]: Configuration.Loader: Failed to load Asteroid: " + asteroidNode.GetValue("name"));
                    }

                    // Restore default logger
                    logger.Flush();
                    Logger.Default.SetAsActive();
                }

                // Glue all the orbits together in the defined pattern
                foreach (KeyValuePair<string, Body> body in bodies) 
                {
                    // If this body is in orbit around another body
                    if(body.Value.orbit != null)
                    {
                        // Get the Body object for the reference body
                        Body parent = null;
                        if(!bodies.TryGetValue(body.Value.orbit.referenceBody, out parent))
                        {
                            throw new Exception("\"" + body.Value.orbit.referenceBody + "\" not found.");
                        }

                        // Setup the orbit of the body
                        parent.generatedBody.children.Add(body.Value.generatedBody);
                        body.Value.generatedBody.orbitDriver.referenceBody = parent.generatedBody.celestialBody;
                        body.Value.generatedBody.orbitDriver.orbit.referenceBody = parent.generatedBody.celestialBody;
                    }

                    // Parent the generated body to the PSystem
                    body.Value.generatedBody.transform.parent = systemPrefab.transform;
                }

                // Elect root body
                systemPrefab.rootBody = bodies.First(p => p.Value.orbit == null).Value.generatedBody;

                // Sort by distance from parent (discover how this effects local bodies)
                RecursivelySortBodies (systemPrefab.rootBody);

                // Fix doubled flightGlobals
                List<int> numbers = new List<int>() { 0 };
                int index = bodies.Sum(b => b.Value.generatedBody.flightGlobalsIndex);
                PatchFGI(ref numbers, ref index, systemPrefab.rootBody);

                // Main Menu bodies
                if (randomMainMenuBodies.Any())
                    Templates.menuBody = randomMainMenuBodies[new System.Random().Next(0, randomMainMenuBodies.Count)];
            }

            // Sort bodies by distance from parent body
            public static void RecursivelySortBodies (PSystemBody body)
            {
                body.children = body.children.OrderBy(b => b.orbitDriver.orbit.semiMajorAxis * (1 + b.orbitDriver.orbit.eccentricity)).ToList();
                foreach (PSystemBody child in body.children) 
                {
                    RecursivelySortBodies (child);
                }
            }

            // Patch the FlightGlobalsIndex of bodies
            public static void PatchFGI(ref List<int> numbers, ref int index, PSystemBody rootBody)
            {
                foreach (PSystemBody body in rootBody.children)
                {
                    if (numbers.Contains(body.flightGlobalsIndex))
                        body.flightGlobalsIndex = index++;
                    numbers.Add(body.flightGlobalsIndex);
                    PatchFGI(ref numbers, ref index, body);
                }
            }
        }
    }
}

