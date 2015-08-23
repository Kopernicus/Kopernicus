/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        /**
         * Class to manage and load configurations for Kopernicus
         **/
        public class Loader
        {
            // Name of the config node group which manages Kopernicus
            private const string rootNodeName = "Kopernicus";

            // Name of the config type which holds the body definition
            private const string bodyNodeName = "Body";

            // Setup the loader
            // To get the system, do "PSystemManager.Instance.systemPrefab = (new Loader()).Generate();"
            public Loader ()
            {

            }

            /**
             * Generates the system prefab from the configuration 
             * @return System prefab object
             **/
            public PSystem Generate ()
            {
                // Dictionary of bodies generated
                Dictionary<string, Body> bodies = new Dictionary<string, Body> ();

                // Retrieve the root config node
                ConfigNode rootConfig = GameDatabase.Instance.GetConfigs (rootNodeName) [0].config;

                if (rootConfig.HasValue("Epoch"))
                    double.TryParse(rootConfig.GetValue("Epoch"), out Templates.instance.epoch);

                if(rootConfig.HasValue("useOnDemand"))
                    bool.TryParse(rootConfig.GetValue("useOnDemand"), out OnDemand.OnDemandStorage.useOnDemand);
                if (rootConfig.HasValue("onDemandLoadOnMissing"))
                    bool.TryParse(rootConfig.GetValue("onDemandLoadOnMissing"), out OnDemand.OnDemandStorage.onDemandLoadOnMissing);
                if (rootConfig.HasValue("onDemandLogOnMissing"))
                    bool.TryParse(rootConfig.GetValue("onDemandLogOnMissing"), out OnDemand.OnDemandStorage.onDemandLogOnMissing);
                if (rootConfig.HasValue("onDemandForceCollect"))
                    bool.TryParse(rootConfig.GetValue("onDemandForceCollect"), out OnDemand.OnDemandStorage.onDemandForceCollect);

                if (rootConfig.HasNode("Finalize"))
                    foreach (ConfigNode n in rootConfig.GetNode("Finalize").GetNodes(bodyNodeName))
                        if (n.HasValue("name"))
                            Templates.instance.finalizeBodies.Add(n.GetValue("name"));

                // Stage 1 - Load all of the bodies
                foreach (ConfigNode bodyNode in rootConfig.GetNodes(bodyNodeName)) 
                {
                    // Create a logger for this body
                    Logger bodyLogger = new Logger (bodyNode.GetValue ("name") + ".Body");
                    bodyLogger.SetAsActive ();

                    // Attempt to create the body
                    try
                    {
                        Body body = Parser.CreateObjectFromConfigNode<Body> (bodyNode);
                        bodies.Add (body.name, body);
                        Logger.Default.Log ("[Kopernicus]: Configuration.Loader: Loaded Body: " + body.name);
                    } 
                    catch (Exception e) 
                    {
                        bodyLogger.LogException (e);
                        Logger.Default.Log ("[Kopernicus]: Configuration.Loader: Failed to load Body: " + bodyNode.GetValue ("name"));
                    }

                    // Restore default logger
                    bodyLogger.Flush ();
                    Logger.Default.SetAsActive ();
                }

                // Stage 2 - create a new planetary system object
                GameObject gameObject = new GameObject ("Kopernicus");
                gameObject.transform.parent = Utility.Deactivator;
                PSystem system = gameObject.AddComponent<PSystem> ();
                
                // Set the planetary system defaults (pulled from PSystemManager.Instance.systemPrefab)
                system.systemName          = "Kopernicus";
                system.systemTimeScale     = 1.0; 
                system.systemScale         = 1.0;
                system.mainToolbarSelected = 2;   // initial value in stock systemPrefab. Unknown significance.

                // Stage 3 - Glue all the orbits together in the defined pattern
                foreach (KeyValuePair<string, Body> body in bodies) 
                {
                    // If this body is in orbit around another body
                    if(body.Value.referenceBody != null)
                    {
                        // Get the Body object for the reference body
                        Body parent = null;
                        if(!bodies.TryGetValue(body.Value.referenceBody, out parent))
                        {
                            throw new Exception("\"" + body.Value.referenceBody + "\" not found.");
                        }

                        // Setup the orbit of the body
                        parent.generatedBody.children.Add(body.Value.generatedBody);
                        body.Value.generatedBody.orbitDriver.referenceBody = parent.generatedBody.celestialBody;
                        body.Value.generatedBody.orbitDriver.orbit.referenceBody = parent.generatedBody.celestialBody;
                    }

                    // Parent the generated body to the PSystem
                    body.Value.generatedBody.transform.parent = system.transform;
                }

                // Stage 4 - elect root body
                system.rootBody = bodies.First(p => p.Value.referenceBody == null).Value.generatedBody;

                // Stage 4.5, get the new Menu-body / Home body
                if (rootConfig.HasValue("mainMenuBody"))
                {
                    Templates.menuBody = rootConfig.GetValue("mainMenuBody");
                }
                else
                {
                    Templates.menuBody = Utility.FindHomeBody(system.rootBody).name;
                }

                // Stage 5 - sort by distance from parent (discover how this effects local bodies)
                RecursivelySortBodies (system.rootBody);

                // Sets the SOI of the root-body to infinite
                system.rootBody.celestialBody.sphereOfInfluence = Double.PositiveInfinity;

                // Fix doubled flightGlobals
                List<int> numbers = new List<int>() { 0 };
                int index = bodies.Sum(b => b.Value.generatedBody.flightGlobalsIndex);
                foreach (PSystemBody body in system.rootBody.children)
                {
                    if (numbers.Contains(body.flightGlobalsIndex))
                        body.flightGlobalsIndex = index++;
                    numbers.Add(body.flightGlobalsIndex);
                }

                // Return the System
                return system;
            }

            // Sort bodies by distance from parent body
            private void RecursivelySortBodies (PSystemBody body)
            {
                body.children = body.children.OrderBy(b => b.orbitDriver.orbit.semiMajorAxis * (1 + b.orbitDriver.orbit.eccentricity)).ToList();
                foreach (PSystemBody child in body.children) 
                {
                    RecursivelySortBodies (child);
                }
            }
        }
    }
}

