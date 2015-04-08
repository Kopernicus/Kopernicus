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

using System.IO;

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
			private const string ScaledSpaceCacheDirectory = "GameData/Kopernicus/Cache";

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

            // Wrapper around Ring class for editing/loading
            [ParserTargetCollection("Rings", optional = true, nameSignificance = NameSignificance.None)]
            private List<RingLoader> rings = new List<RingLoader>();

            // Wrapper around Particle class for editing/loading
            [ParserTarget("Particle", optional = true, allowMerge = true)]
            private ParticleLoader particle;

			// Sun
			[ParserTarget("SolarPowerCurve", optional = true, allowMerge = false)]
			private FloatCurveParser solarPowerCurve;

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
					generatedBody.scaledVersion.name = name;
					if (generatedBody.pqsVersion != null) 
					{
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
					// ----------- DEBUG -------------
					#if DEBUG
					Utility.DumpObjectProperties (pqs.pqsVersion.surfaceMaterial, " ---- Surface Material (Post PQS Loader) ---- ");
					Utility.GameObjectWalk (pqs.pqsVersion.gameObject, "  ");
					#endif
					// -------------------------------

					// Assign the generated PQS to our new world
					generatedBody.pqsVersion = pqs.pqsVersion;

					// Adjust the radius of the PQSs appropriately
					foreach (PQS p in generatedBody.pqsVersion.GetComponentsInChildren(typeof (PQS), true))
						p.radius = generatedBody.celestialBody.Radius;
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

				// We need to generate new scaled space meshes if 
				//   a) we are using a template and we've change either the radius or type of body
				//   b) we aren't using a template
				if (((template != null) && (Math.Abs(template.radius - generatedBody.celestialBody.Radius) > 1.0 || template.type != scaledVersion.type.value))
				    || template == null)
				{
					const double rJool = 6000000.0;
					const float  rScaled = 1000.0f;

					// Compute scale between Jool and this body
					float scale = (float)(generatedBody.celestialBody.Radius / rJool);
					generatedBody.scaledVersion.transform.localScale = new Vector3(scale, scale, scale);

					// Attempt to load a cached version of the scale space
					string CacheDirectory = KSPUtil.ApplicationRootPath + ScaledSpaceCacheDirectory;
					string CacheFile = CacheDirectory + "/" + generatedBody.name + ".bin";
					Directory.CreateDirectory (CacheDirectory);
                    if (File.Exists(CacheFile) && !File.Exists(ScaledSpaceCacheDirectory + "/DEBUG"))
                    {
                        Logger.Active.Log("[Kopernicus]: Body.PostApply(ConfigNode): Loading cached scaled space mesh: " + generatedBody.name);
                        Mesh scaledMesh = Utility.DeserializeMesh(CacheFile);
                        Utility.RecalculateTangents(scaledMesh);
                        generatedBody.scaledVersion.GetComponent<MeshFilter>().sharedMesh = scaledMesh;

                    }

                    // Otherwise we have to generate the mesh
                    else
                    {
                        Logger.Active.Log("[Kopernicus]: Body.PostApply(ConfigNode): Generating scaled space mesh: " + generatedBody.name);
                        Mesh scaledVersionMesh = ComputeScaledSpaceMesh(generatedBody);
                        Utility.RecalculateTangents(scaledVersionMesh);
                        generatedBody.scaledVersion.GetComponent<MeshFilter>().sharedMesh = scaledVersionMesh;
                        if (!File.Exists(ScaledSpaceCacheDirectory + "/DEBUG"))
                            Utility.SerializeMesh(scaledVersionMesh, CacheFile);
                    }

					// Apply mesh to the body
					SphereCollider collider = generatedBody.scaledVersion.GetComponent<SphereCollider>();
					if (collider != null) collider.radius = rScaled;

                    if (generatedBody.pqsVersion != null)
                    {
                        generatedBody.scaledVersion.gameObject.transform.localScale = Vector3.one * (float)(generatedBody.pqsVersion.radiusMin / rJool);
                    }
				}

				// Post gen celestial body
				Utility.DumpObjectFields(generatedBody.celestialBody, " Celestial Body ");
			}

			// Generate the scaled space mesh using PQS (all results use scale of 1)
			public static Mesh ComputeScaledSpaceMesh (PSystemBody body)
			{
				// We need to get the body for Jool (to steal it's mesh)
				const double rScaledJool = 1000.0f;
			    double rMetersToScaledUnits = (float) (rScaledJool / body.celestialBody.Radius);

                // Generate a duplicate of the Jool mesh
				Mesh mesh = Utility.DuplicateMesh (Utility.ReferenceGeosphere());

				// If this body has a PQS, we can create a more detailed object
				if (body.pqsVersion != null) 
				{
					// In order to generate the scaled space we have to enable the mods.  Since this is
					// a prefab they don't get disabled as kill game performance.  To resolve this we 
					// clone the PQS, use it, and then delete it when done
					GameObject pqsVersionGameObject = UnityEngine.Object.Instantiate(body.pqsVersion.gameObject) as GameObject;
					PQS pqsVersion = pqsVersionGameObject.GetComponent<PQS>();
				
					// Find and enable the PQS mods that modify height
                    IEnumerable<PQSMod> mods = pqsVersion.GetComponentsInChildren<PQSMod>(true);

					foreach(PQSMod mod in mods)
						mod.OnSetup();

					// If we were able to find PQS mods
					if(mods.Count() > 0)
					{
						// Generate the PQS modifications
						Vector3[] vertices = mesh.vertices;
						for(int i = 0; i < mesh.vertexCount; i++)
						{
							// Get the UV coordinate of this vertex
							Vector2 uv = mesh.uv[i];

							// Since this is a geosphere, normalizing the vertex gives the direction from center center
							Vector3 direction = vertices[i];
							direction.Normalize();

							// Build the vertex data object for the PQS mods
							PQS.VertexBuildData vertex = new PQS.VertexBuildData();
							vertex.directionFromCenter = direction;
							vertex.vertHeight = body.celestialBody.Radius;
							vertex.u = uv.x;
							vertex.v = uv.y;
							
							// Build from the PQS
							foreach(PQSMod mod in mods)
								mod.OnVertexBuildHeight(vertex);
							
							// Adjust the displacement
							vertices [i] = direction * (float)(vertex.vertHeight * rMetersToScaledUnits);
						}
						mesh.vertices = vertices;
						mesh.RecalculateNormals();
						mesh.RecalculateBounds ();
					}

					// Cleanup
					UnityEngine.Object.Destroy(pqsVersionGameObject);
				}

				// Return the generated scaled space mesh
				return mesh;
			}
		}
	}
}

#pragma warning restore 0414
