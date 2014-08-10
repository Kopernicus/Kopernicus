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

namespace Kopernicus
{
	namespace Configuration 
	{
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

					// Patch PQS names
					if (generatedBody.pqsVersion != null) 
					{
						// Patch all of the PQS names
						foreach (PQS p in generatedBody.pqsVersion.GetComponentsInChildren(typeof (PQS), true))
							p.name = p.name.Replace (template.body.celestialBody.bodyName, name);

						//Utility.GameObjectWalk(generatedBody.pqsVersion.gameObject);
					}

					// Patch all of the names with the new name
					generatedBody.name = name;
					generatedBody.celestialBody.bodyName = name;
					generatedBody.scaledVersion.name = name;
					
					// If this body has an orbit driver, it has an orbit (sun by default does NOT have a driver).
					// create the orbit driver so parameters can be merged
					if (generatedBody.orbitDriver != null) 
					{
						orbit = new OrbitLoader(generatedBody.orbitDriver.orbit, generatedBody.orbitRenderer.orbitColor);
					}
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

					// sensible defaults
					generatedBody.celestialBody.bodyName = name;
					generatedBody.celestialBody.atmosphere = false;
					generatedBody.celestialBody.ocean = false;
				}

				// Create the properties object with the celestial body
				properties = new Properties (generatedBody.celestialBody);
			}

			// Parser Post Apply Event
			public void PostApply (ConfigNode node)
			{
				// First step is to check whether an orbit has been defined
				if (orbit != null) 
				{
					// If this body needs orbit controllers, create them
					if(generatedBody.orbitDriver == null)
					{
						generatedBody.orbitDriver = generatedBody.celestialBody.gameObject.AddComponent<OrbitDriver>();
						generatedBody.orbitRenderer = generatedBody.celestialBody.gameObject.AddComponent<OrbitRenderer>();
					}

					// Setup orbit
					generatedBody.orbitDriver.orbit = orbit.orbit;
					generatedBody.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;
					generatedBody.orbitRenderer.orbitColor = orbit.color.value;
				}
				
				// Adjust any PQS settings required
				if (generatedBody.pqsVersion != null) 
				{
					// Adjust the radius of the PQSs appropriately
					foreach (PQS p in generatedBody.pqsVersion.GetComponentsInChildren(typeof (PQS), true))
						p.radius = generatedBody.celestialBody.Radius;
					
					//Utility.GameObjectWalk(generatedBody.pqsVersion.gameObject);
				}


			}
		}
	}
}
