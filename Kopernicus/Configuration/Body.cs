/**
 * Kopernicus Planetary System Modifier
 * Copyright (C) 2014 Bryce C Schroeder (bryce.schroeder@gmail.com), Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * http://www.ferazelhosting.net/~bryce/contact.html
 * 
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
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

// Disable the "private fields `` is assigned but its value is never used warning"
#pragma warning disable 0414

namespace Kopernicus
{
	namespace Configuration 
	{
		[RequireConfigType(ConfigType.Node)]
		public class Body : IParserEventSubscriber
		{
            // Path of the plugin (will eventually not matter much)
            public const string ScaledSpaceCacheDirectory = "GameData/Kopernicus/Cache";

			// Body we are trying to edit
			public PSystemBody generatedBody { get; private set; }

			// Reference body of the generated object
			public string referenceBody 
			{
				get { return (orbit != null) ? orbit.referenceBody : null; }
			}

			// Name of this body
			[PreApply]
			[ParserTarget("name", optional = false)]
			public string name { get; private set; }

			[ParserTarget("cbNameLater", optional = true)]
            private string cbNameLater
            {
                set
                {
                    if (!NameChanges.CBNames.ContainsKey(name))
                        NameChanges.CBNames[name] = new CBNameChanger(name, value);
                }
            }
			
			// Flight globals index of this body - for computing reference id
			[ParserTarget("flightGlobalsIndex", optional = true)]
			public NumericParser<int> flightGlobalsIndex 
			{
				set { generatedBody.flightGlobalsIndex = value.value; }
			}

			// Template property of a body - responsible for generating a PSystemBody from an existing one
			[PreApply]
			[ParserTarget("Template", optional = true)]
			private Template template;

			// Celestial body properties (description, mass, etc.)
			[ParserTarget("Properties", optional = true, allowMerge = true)]
			private Properties properties;

			// Wrapper around KSP's Orbit class for editing/loading
			[ParserTarget("Orbit", optional = true, allowMerge = true)]
			private OrbitLoader orbit;

			// Wrapper around the settings for the world's scaled version
			[ParserTarget("ScaledVersion", optional = true, allowMerge = true)]
			private ScaledVersion scaledVersion;
			
			// Wrapper around the settings for the world's atmosphere
			[ParserTarget("Atmosphere", optional = true, allowMerge = true)]
			private Atmosphere atmosphere;

			// Wrapper arounc the settings for the PQS
			[ParserTarget("PQS", optional = true, allowMerge = true)]
			private PQSLoader pqs;

            // Wrapper arounc the settings for the Ocean
            [ParserTarget("Ocean", optional = true, allowMerge = true)]
            private OceanPQS ocean;

            // Wrapper around Ring class for editing/loading
            [ParserTargetCollection("Rings", optional = true, nameSignificance = NameSignificance.None)]
            private List<RingLoader> rings = new List<RingLoader>();

            // Wrapper around Particle class for editing/loading
            [ParserTarget("Particle", optional = true, allowMerge = true)]
            private ParticleLoader particle;

			// Sun
			[ParserTarget("SolarPowerCurve", optional = true, allowMerge = false)]
			private FloatCurveParser solarPowerCurve;

            // Wrapper around the settings for the SpaceCenter
            [ParserTarget("SpaceCenter", optional = true, allowMerge = true)]
            private SpaceCenterSwitcher spaceCenter;

			// Parser Apply Event
			public void Apply (ConfigNode node)
			{
				// If we have a template, generatedBody *is* the template body
				if (template != null) 
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
						foreach (PQS p in generatedBody.pqsVersion.GetComponentsInChildren(typeof (PQS), true))
							p.name = p.name.Replace (template.body.celestialBody.bodyName, name);
					}
					
					// If this body has an orbit, create editor/loader
					if (generatedBody.orbitDriver != null) 
					{
						orbit = new OrbitLoader(generatedBody);
					}

					// If this body has a PQS, create editor/loader
					if (generatedBody.pqsVersion != null)
					{
						pqs = new PQSLoader(generatedBody.pqsVersion);

                        // If this body has an ocean PQS, create editor/loader
                        if (generatedBody.celestialBody.ocean == true)
                        {
                            foreach (PQS PQSocean in generatedBody.pqsVersion.GetComponentsInChildren<PQS>(true))
                            {
                                if (PQSocean.name == name + "Ocean")
                                {
                                    ocean = new OceanPQS(PQSocean);
                                    break;
                                }
                            }
                        }
					}

					// Create the scaled version editor/loader
					scaledVersion = new ScaledVersion(generatedBody.scaledVersion, generatedBody.celestialBody, template.type);
				}

				// Otherwise we have to generate all the things for this body
				else 
				{
					// Create the PSystemBody object
					GameObject generatedBodyGameObject = new GameObject (name);
					generatedBodyGameObject.transform.parent = Utility.Deactivator;
					generatedBody = generatedBodyGameObject.AddComponent<PSystemBody> ();
					generatedBody.flightGlobalsIndex = 0;

					// Create the celestial body
					GameObject generatedBodyProperties = new GameObject (name);
					generatedBodyProperties.transform.parent = generatedBodyGameObject.transform;
					generatedBody.celestialBody = generatedBodyProperties.AddComponent<CelestialBody> ();
					generatedBody.resources = generatedBodyProperties.AddComponent<PResource> ();
					generatedBody.celestialBody.progressTree = null;

					// Sensible defaults 
					generatedBody.celestialBody.bodyName = name;
					generatedBody.celestialBody.atmosphere = false;
					generatedBody.celestialBody.ocean = false;

					// Create the scaled version
					generatedBody.scaledVersion = new GameObject(name);
					generatedBody.scaledVersion.layer = Constants.GameLayers.ScaledSpace;
					generatedBody.scaledVersion.transform.parent = Utility.Deactivator;

					// Create the scaled version editor/loader
					scaledVersion = new ScaledVersion(generatedBody.scaledVersion, generatedBody.celestialBody, BodyType.Atmospheric);
				}

				// Create property editor/loader objects
				properties = new Properties (generatedBody.celestialBody);

				// Atmospheric settings
				atmosphere = new Atmosphere(generatedBody.celestialBody, generatedBody.scaledVersion);

                // Particles
                particle = new ParticleLoader(generatedBody.scaledVersion.gameObject);
			}

			// Parser Post Apply Event
			public void PostApply (ConfigNode node)
			{
                // Update any interrelated body properties
                properties.PostApplyUpdate();
				// If an orbit is defined, we orbit something
				if (orbit != null) 
				{
					// If this body needs orbit controllers, create them
					if (generatedBody.orbitDriver == null) 
					{
						generatedBody.orbitDriver = generatedBody.celestialBody.gameObject.AddComponent<OrbitDriver> ();
						generatedBody.orbitRenderer = generatedBody.celestialBody.gameObject.AddComponent<OrbitRenderer> ();
					}

					// Setup orbit
					generatedBody.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;
					orbit.Apply(generatedBody);
				}

				// If a PQS version was definied
				if (pqs != null) 
				{
					// Assign the generated PQS to our new world
					generatedBody.pqsVersion = pqs.pqsVersion;
                    generatedBody.pqsVersion.name = name;
                    generatedBody.pqsVersion.transform.name = name;
                    generatedBody.pqsVersion.gameObject.name = name;
                    generatedBody.pqsVersion.radius = generatedBody.celestialBody.Radius;

                    // If an ocean was defined
                    if (ocean != null)
                    {
                        if (generatedBody.celestialBody.ocean == false)
                        {
                            ocean.oceanRoot.transform.parent = generatedBody.pqsVersion.transform;

                            // Add the ocean PQS to the secondary renders of the CelestialBody Transform
                            PQSMod_CelestialBodyTransform transform = generatedBody.pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true).Where(mod => mod.transform.parent == generatedBody.pqsVersion.transform).FirstOrDefault();
                            transform.planetFade.secondaryRenderers.Add(ocean.oceanPQS.gameObject);

                            // Set up the ocean PQS
                            ocean.oceanPQS.parentSphere = generatedBody.pqsVersion;

                            // Names!
                            ocean.oceanPQS.name = generatedBody.pqsVersion.name + "Ocean";
                            ocean.oceanPQS.gameObject.name = generatedBody.pqsVersion.name + "Ocean";
                            ocean.oceanPQS.transform.name = generatedBody.pqsVersion.name + "Ocean";

                            // Ajust map settings of the parent PQS
                            generatedBody.pqsVersion.mapOcean = ocean.mapOcean;
                            generatedBody.celestialBody.ocean = ocean.mapOcean;
                            if (ocean.mapOceanColor != null) generatedBody.pqsVersion.mapOceanColor = ocean.mapOceanColor;
                            generatedBody.pqsVersion.mapOceanHeight = ocean.mapOceanHeight;
                        }
                        else
                        {
                            // Ajust map settings of the parent PQS
                            generatedBody.pqsVersion.mapOcean = ocean.mapOcean;
                            generatedBody.celestialBody.ocean = ocean.mapOcean;
                            if (ocean.mapOceanColor != null) generatedBody.pqsVersion.mapOceanColor = ocean.mapOceanColor;
                            generatedBody.pqsVersion.mapOceanHeight = ocean.mapOceanHeight;

                            // Set up the ocean PQS
                            ocean.oceanPQS.parentSphere = generatedBody.pqsVersion;
                        }
                    }

                    // ----------- DEBUG -------------
                    #if DEBUG
                    Utility.DumpObjectProperties(pqs.pqsVersion.surfaceMaterial, " ---- Surface Material (Post PQS Loader) ---- ");
                    Utility.GameObjectWalk(pqs.pqsVersion.gameObject, "  ");
                    #endif
                    // -------------------------------

                    // Don't do this, because we probably need to ajust the radius of the OceanPQS (and AFAIK is that the only child-PQS)
					/* Adjust the radius of the PQSs appropriately
					foreach (PQS p in generatedBody.pqsVersion.GetComponentsInChildren(typeof (PQS), true))
						p.radius = generatedBody.celestialBody.Radius;*/
				}

                // Create our RingLoaders
                foreach (RingLoader ring in rings)
                {
                    RingLoader.AddRing(generatedBody.scaledVersion.gameObject, ring.ring);
                }

                // If this body is a star
				if (scaledVersion.type.value == BodyType.Star) 
				{
					// Get the Kopernicus star component from the scaled version
					StarComponent component = generatedBody.scaledVersion.GetComponent<StarComponent> ();

					// If we have defined a custom power curve, load it
					if (solarPowerCurve != null) 
					{
						component.powerCurve = solarPowerCurve.curve;
					}
                }

                #region DebugMode
                // Prepare our Debug mode properties
                bool exportBin = true;
                bool inEveryCase = false;
                
                if (node.HasNode("Debug"))
                {
                    ConfigNode debug = node.GetNode("Debug");
                    inEveryCase = true;
                    if (debug.HasValue("exportBin")) exportBin = Boolean.Parse(debug.GetValue("exportBin"));
                }
                #endregion

                // If we're going to generate later, skip this.
                //if (!Templates.instance.finalizeBodies.Contains(name))
                //{
                    // We need to generate new scaled space meshes if 
                    //   a) we are using a template and we've change either the radius or type of body
                    //   b) we aren't using a template
                    //   c) debug mode is active
                    if (((template != null) && (Math.Abs(template.radius - generatedBody.celestialBody.Radius) > 1.0 || template.type != scaledVersion.type.value))
                        || template == null || inEveryCase)
                    {
                        Utility.UpdateScaledMesh(generatedBody.scaledVersion,
                                                    generatedBody.pqsVersion,
                                                    generatedBody.celestialBody,
                                                    ScaledSpaceCacheDirectory,
                                                    exportBin,
                                                    scaledVersion.useSphericalModel);
                    }
                //}

				// Post gen celestial body
				Utility.DumpObjectFields(generatedBody.celestialBody, " Celestial Body ");
			}
		}
	}
}

#pragma warning restore 0414
