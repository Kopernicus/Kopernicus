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
using System;
using System.Collections.Generic;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class Body : IParserEventSubscriber
        {
            // Path of the ScaledSpaceCache (will eventually not matter much)
            public const String ScaledSpaceCacheDirectory = "Kopernicus/Cache";

            // Body we are trying to edit
            public PSystemBody generatedBody { get; set; }

            // The CelestialBody component of the body
            private CelestialBody _celestialBody;
            public CelestialBody celestialBody
            {
                get
                {
                    if (Injector.IsInPrefab)
                        return generatedBody?.celestialBody;
                    return _celestialBody;
                }
                set
                {
                    if (!Injector.IsInPrefab)
                        _celestialBody = value;
                }
            }

            // Name of this body
            [PreApply]
            [ParserTarget("name", Optional = false)]
            [KittopiaHideOption]
            public String name { get; set; }

            [ParserTarget("cacheFile")]
            public String cacheFile
            {
                get { return celestialBody.Get("cacheFile", ""); }
                set { celestialBody.Set("cacheFile", value); }
            }

            [ParserTarget("barycenter")]
            public NumericParser<Boolean> barycenter
            {
                get { return celestialBody.Get("barycenter", false); }
                set { celestialBody.Set("barycenter", value.Value); }
            }

            [ParserTarget("cbNameLater")]
            public String cbNameLater
            {
                get
                {
                    NameChanger changer = celestialBody.GetComponent<NameChanger>();
                    return changer ? changer.newName : "";
                }
                set
                {
                    // Change the displayName
                    celestialBody.bodyDisplayName = value;

                    // Set the NameChanger component
                    NameChanger changer = celestialBody.gameObject.AddOrGetComponent<NameChanger>();
                    changer.oldName = celestialBody.bodyName;
                    changer.newName = value;

                    // Update the name
                    if (!Injector.IsInPrefab)
                    {
                        changer.Start();
                    }
                }
            }

            // Flight globals index of this body - for computing reference id
            // [ParserTarget("flightGlobalsIndex")]
            // public NumericParser<Int32> flightGlobalsIndex
            // {
            //     get { return generatedBody.flightGlobalsIndex; }
            //     set { generatedBody.flightGlobalsIndex = value.value; }
            // }
            // Kill this with fire. I mean, it's not 2013 anymore

            // An identifier that is used for referencing the orbiting body. 
            // This must be unique!
            [ParserTarget("identifier")]
            public String identifier
            {
                get { return celestialBody.Get("identifier", celestialBody.transform.name); }
                set { celestialBody.Set("identifier", value); }
            }

            // Finalize the orbit of the body?
            [ParserTarget("finalizeOrbit")]
            public NumericParser<Boolean> finalizeOrbit
            {
                get { return celestialBody.Get("finalizeBody", false); }
                set { celestialBody.Set("finalizeBody", value.Value); }
            }

            // Whether this body should be taken into account for the main menu body stuff
            [ParserTarget("randomMainMenuBody")]
            public NumericParser<Boolean> randomMainMenuBody
            {
                get { return Templates.randomMainMenuBodies.Contains(name); }
                set { if (value) Templates.randomMainMenuBodies.Add(name); }
            }

            // Describes how often contracts should be generated for a body
            [ParserTarget("contractWeight")]
            public NumericParser<Int32> contractWeight
            {
                get { return celestialBody.Get("contractWeight", 30); }
                set { celestialBody.Set("contractWeight", value.Value); }
            }

            // Template property of a body - responsible for generating a PSystemBody from an existing one
            [PreApply]
            [ParserTarget("Template")]
            [KittopiaHideOption]
            public TemplateLoader template { get; set; }

            // Celestial body properties (description, mass, etc.)
            [ParserTarget("Properties", AllowMerge = true)]
            public PropertiesLoader properties { get; set; }

            // Wrapper around KSP's Orbit class for editing/loading
            [ParserTarget("Orbit", AllowMerge = true)]
            public OrbitLoader orbit { get; set; }

            // Wrapper around the settings for the world's scaled version
            [ParserTarget("ScaledVersion", AllowMerge = true)]
            public ScaledVersionLoader scaledVersion { get; set; }

            // Wrapper around the settings for the world's atmosphere
            [ParserTarget("Atmosphere", AllowMerge = true)]
            public AtmosphereLoader atmosphere { get; set; }

            // Wrapper around the settings for the PQS
            [ParserTarget("PQS", AllowMerge = true)]
            public PQSLoader pqs { get; set; }

            // Wrapper around the settings for the Ocean
            [ParserTarget("Ocean", AllowMerge = true)]
            public OceanLoader ocean { get; set; }

            // Wrapper around Ring class for editing/loading
            [ParserTargetCollection("Rings", AllowMerge = true)]
            public List<RingLoader> rings { get; set; }

            // Wrapper around Particle class for editing/loading
            [ParserTargetCollection("Particles", AllowMerge = true)]
            public List<ParticleLoader> particles { get; set; }

            // Wrapper around the settings for the SpaceCenter
            [ParserTarget("SpaceCenter", AllowMerge = true)]
            public SpaceCenterLoader spaceCenter { get; set; }

            // Wrapper around DebugMode settings
            [ParserTarget("Debug")]
            public DebugLoader debug { get; set; }

            // Post spawn orbit patcher
            [ParserTarget("PostSpawnOrbit")]
            public ConfigNode postSpawnOrbit
            {
                get { return celestialBody.Get<ConfigNode>("orbitPatches", null); }
                set { celestialBody.Set("orbitPatches", value); }
            }

            // Parser Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                // If we have a template, generatedBody *is* the template body
                if (template != null && template.body)
                {
                    generatedBody = template.body;

                    // Patch the game object names in the template
                    generatedBody.name = name;
                    generatedBody.celestialBody.bodyName = name;
                    generatedBody.celestialBody.transform.name = name;
                    generatedBody.celestialBody.bodyTransform.name = name;
                    generatedBody.scaledVersion.name = name;
                    if (generatedBody.pqsVersion != null)
                    {
                        generatedBody.pqsVersion.name = name;
                        generatedBody.pqsVersion.gameObject.name = name;
                        generatedBody.pqsVersion.transform.name = name;
                        foreach (PQS p in generatedBody.pqsVersion.GetComponentsInChildren<PQS>(true))
                            p.name = p.name.Replace(template.body.celestialBody.bodyName, name);
                        generatedBody.celestialBody.pqsController = generatedBody.pqsVersion;
                    }

                    // If we've changed the name, reset use_The_InName
                    if (generatedBody.name != template.originalBody.celestialBody.bodyName)
                    {
                        generatedBody.celestialBody.bodyDisplayName = generatedBody.celestialBody.bodyName;
                    }
                }

                // Otherwise we have to generate all the things for this body
                else
                {
                    // Create the PSystemBody object
                    GameObject generatedBodyGameObject = new GameObject(name);
                    generatedBodyGameObject.transform.parent = Utility.Deactivator;
                    generatedBody = generatedBodyGameObject.AddComponent<PSystemBody>();
                    generatedBody.flightGlobalsIndex = 0;

                    // Create the celestial body
                    GameObject generatedBodyProperties = new GameObject(name);
                    generatedBodyProperties.transform.parent = generatedBodyGameObject.transform;
                    generatedBody.celestialBody = generatedBodyProperties.AddComponent<CelestialBody>();
                    generatedBody.celestialBody.progressTree = null;

                    // Sensible defaults 
                    generatedBody.celestialBody.bodyName = name;
                    generatedBody.celestialBody.bodyDisplayName = name;
                    generatedBody.celestialBody.atmosphere = false;
                    generatedBody.celestialBody.ocean = false;

                    // Create the scaled version
                    generatedBody.scaledVersion = new GameObject(name);
                    generatedBody.scaledVersion.layer = Constants.GameLayers.ScaledSpace;
                    generatedBody.scaledVersion.transform.parent = Utility.Deactivator;
                }
                
                // Create accessors
                debug = new DebugLoader();
                scaledVersion = new ScaledVersionLoader();

                // Event
                Events.OnBodyApply.Fire(this, node);
            }

            // Parser Post Apply Event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                // Create a barycenter
                if (barycenter.Value)
                {
                    CreateBarycenter();
                }

                // Loads external parser targets
                Parser.LoadParserTargetsExternal(node, "Kopernicus", configName: "Kopernicus");

                // Post gen celestial body
                Utility.DumpObjectFields(generatedBody.celestialBody, " Celestial Body ");

                // Events
                Events.OnBodyPostApply.Fire(this, node);

                // We need to generate new scaled space meshes if 
                //   a) we are using a template and we've change either the radius or type of body
                //   b) we aren't using a template
                //   c) debug mode is active
                if (!scaledVersion.deferMesh &&
                    (template != null && (Math.Abs(template.radius - generatedBody.celestialBody.Radius) > 1.0 || template.type != scaledVersion.type.Value)
                    || template == null || debug.update))
                {

                    scaledVersion.RebuildScaledSpace();
                    Events.OnBodyGenerateScaledSpace.Fire(this, node);
                }
            }

            [KittopiaAction("Convert Body to Barycenter")]
            public void CreateBarycenter()
            {
                // Set the value accordingly
                barycenter = true;
                
                // Nuke the PQS
                if (generatedBody.pqsVersion != null)
                {
                    generatedBody.pqsVersion.transform.parent = null;
                    UnityEngine.Object.Destroy(generatedBody.pqsVersion);
                    generatedBody.pqsVersion = null;
                }

                // Stop ScaledSpace Cache
                scaledVersion.deferMesh = true;
                celestialBody.scaledBody?.SetActive(false);
            }

            /// <summary>
            /// Creates a new Body from the Injector context.
            /// </summary>
            public Body()
            {
                rings = new List<RingLoader>();
                particles = new List<ParticleLoader>();
            }

            /// <summary>
            /// Creates a new Body from a spawned CelestialBody.
            /// </summary>
            public Body(CelestialBody celestialBody)
            {
                this.celestialBody = celestialBody;
                
                // Create the accessors
                properties = new PropertiesLoader(celestialBody);
                if (celestialBody.orbitDriver != null)
                {
                    orbit = new OrbitLoader(celestialBody);
                }
                scaledVersion = new ScaledVersionLoader(celestialBody);
                if (celestialBody.atmosphere)
                {
                    atmosphere = new AtmosphereLoader(celestialBody);
                }
                pqs = new PQSLoader(celestialBody);
                ocean = new OceanLoader(celestialBody);
                rings = new List<RingLoader>();
                foreach (Ring ring in celestialBody.scaledBody.GetComponentsInChildren<Ring>(true))
                {
                    rings.Add(new RingLoader(ring));
                }
                particles = new List<ParticleLoader>();
                foreach (PlanetParticleEmitter particle in celestialBody.scaledBody
                    .GetComponentsInChildren<PlanetParticleEmitter>(true))
                {
                    particles.Add(new ParticleLoader(particle));
                }
                if (celestialBody.isHomeWorld)
                {
                    spaceCenter = new SpaceCenterLoader(celestialBody);
                }
                debug = new DebugLoader(celestialBody);
            }
        }
    }
}
