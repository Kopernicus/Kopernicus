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
using UnityEngine;

using Kopernicus.MaterialWrapper;

namespace Kopernicus
{
	namespace Configuration 
	{
		[RequireConfigType(ConfigType.Node)]
		public class Body : IParserEventSubscriber
		{
			// Body we are trying to edit
			public PSystemBody generatedBody { get; private set; }

			// Reference body of the generated object
			public string referenceBody 
			{
				get { return (orbit != null) ? orbit.referenceBody : null; }
			}

			// Name of this body
			[PreApply]
			[ParserTarget("name", optional = false, allowMerge = false)]
			public string name { get; private set; }
			
			// Flight globals index of this body - for computing reference id
			[ParserTarget("flightGlobalsIndex", optional = true, allowMerge = false)]
			public NumericParser<int> flightGlobalsIndex 
			{
				set { generatedBody.flightGlobalsIndex = value.value; }
			}

			// Template property of a body - responsible for generating a PSystemBody from an existing one
			[PreApply]
			[ParserTarget("Template", optional = true, allowMerge = false)]
			private Template template;

			// Celestial body properties (description, mass, etc.)
			[ParserTarget("Properties", optional = true, allowMerge = true)]
			private Properties properties;

			// Wrapper around KSP's Orbit class for editing/loading
			[ParserTarget("Orbit", optional = true, allowMerge = true)]
			private OrbitLoader orbit;

			// Wrapper around the settings for the world's scaled version
			//[ParserTarget("ScaledVersion", optional = true, allowMerge = true)]
			//private ScaledVersion scaledVersion;

			// PQS
			// Atmosphere
			// Sun

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
						// Patch all of the PQS names
						foreach (PQS p in generatedBody.pqsVersion.GetComponentsInChildren(typeof (PQS), true))
							p.name = p.name.Replace (template.body.celestialBody.bodyName, name);
					}
					
					// Ensure this body has orbit drivers
					if (generatedBody.orbitDriver == null) 
					{
						generatedBody.orbitDriver = generatedBody.celestialBody.gameObject.AddComponent<OrbitDriver> ();
						generatedBody.orbitRenderer = generatedBody.celestialBody.gameObject.AddComponent<OrbitRenderer> ();
					}
					
					// Create the scaled version editor/loader
					//scaledVersion = new ScaledVersion(generatedBody.scaledVersion, generatedBody.celestialBody, template.type);
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

					// Sensible defaults 
					generatedBody.celestialBody.bodyName = name;
					generatedBody.celestialBody.atmosphere = false;
					generatedBody.celestialBody.ocean = false;

					// Create orbit drivers
					generatedBody.orbitDriver = generatedBody.celestialBody.gameObject.AddComponent<OrbitDriver> ();
					generatedBody.orbitRenderer = generatedBody.celestialBody.gameObject.AddComponent<OrbitRenderer> ();

					// Create the scaled version
					generatedBody.scaledVersion = new GameObject(name);
					generatedBody.scaledVersion.layer = Constants.GameLayers.ScaledSpace;
					generatedBody.scaledVersion.transform.parent = Utility.Deactivator;

					// Create the sphere collider for the scaled version (IS THIS OKAY FOR SUNs????)
					SphereCollider collider = generatedBody.scaledVersion.AddComponent<SphereCollider>();
					collider.center = Vector3.zero;
					collider.radius = 1000.0f;

					// Create the scaled version editor/loader
					//scaledVersion = new ScaledVersion(generatedBody.scaledVersion, generatedBody.celestialBody, BodyType.Atmospheric);
				}

				// Create property editor/loader objects
				properties = new Properties (generatedBody.celestialBody);
				orbit = new OrbitLoader(generatedBody);
			}

			// Parser Post Apply Event
			public void PostApply (ConfigNode node)
			{
				// If the reference body is null, we assume it doesn't orbit anything
				if (orbit.referenceBody == null) 
				{
					UnityEngine.Object.Destroy(generatedBody.orbitDriver);
					UnityEngine.Object.Destroy(generatedBody.orbitRenderer);
				}

				// If this body was generated from a template
				if (template != null) 
				{
					// Correct the scaling of the the scaled version (we actually need to regenerate the mesh)
					generatedBody.scaledVersion.transform.localScale = template.scale * (float)(generatedBody.celestialBody.Radius / template.radius);
				}

				// Adjust any PQS settings required
				if (generatedBody.pqsVersion != null) 
				{
					// Adjust the radius of the PQSs appropriately
					foreach (PQS p in generatedBody.pqsVersion.GetComponentsInChildren(typeof (PQS), true))
						p.radius = generatedBody.celestialBody.Radius;
				}
			}
		}
	}
}
