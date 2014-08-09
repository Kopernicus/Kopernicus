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

			// Name of this body
			[PreApply]
			[ParserTarget("name", optional = false, allowMerge = false)]
			public string name { get; private set; }
			
			// Template property of a body (not loaded automagically)
			[PreApply]
			[ParserTarget("Template", optional = true, allowMerge = false)]
			private Template template;

			// Flight globals index of this body
			[ParserTarget("flightGlobalsIndex", optional = false, allowMerge = false)]
			public NumericParser<int> flightGlobalsIndex 
			{
				set { generatedBody.flightGlobalsIndex = value.value; }
			}

			// Celestial body properties
			[ParserTarget("Properties", optional = true, allowMerge = true)]
			private Properties properties;

			// Orbit
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
					if(generatedBody.pqsVersion != null)
					{
						// Patch all of the PQS names
						foreach(PQS p in generatedBody.pqsVersion.GetComponentsInChildren(typeof (PQS), true))
						{
							Debug.Log ("[Kopernicus]: Configuration.Body: Patching \"" + p.name + "\"");
							p.name = p.name.Replace(template.body.celestialBody.bodyName, name);
						}
					}

					// Patch all of the names with the new name
					generatedBody.name = name;
					generatedBody.celestialBody.bodyName = name;
					generatedBody.scaledVersion.name = name;
				}

				// Otherwise we have to generate all the things for this body
				else 
				{
					// Create the PSystemBody object
					GameObject generatedBodyGameObject = new GameObject(name);
					generatedBodyGameObject.transform.parent = Utility.Deactivator;
					generatedBody = generatedBodyGameObject.AddComponent<PSystemBody>();

					// Create the celestial body
					GameObject generatedBodyProperties = new GameObject(name);
					generatedBodyProperties.transform.parent = generatedBodyGameObject.transform;
					generatedBody.celestialBody = generatedBodyProperties.AddComponent<CelestialBody>();
					generatedBody.resources = generatedBodyProperties.AddComponent<PResource>();

					// temporary - as this should be created by orbit
					generatedBody.orbitDriver = generatedBodyProperties.AddComponent<OrbitDriver>();
					generatedBody.orbitRenderer = generatedBodyProperties.AddComponent<OrbitRenderer>();

					// sensible defaults
					generatedBody.celestialBody.bodyName = name;
					generatedBody.celestialBody.atmosphere = false;
					generatedBody.celestialBody.ocean = false;
				}

				// By default we have no orbiting bodies
				generatedBody.celestialBody.orbitingBodies = new List<CelestialBody>();

				// Create the properties object with the celestial body
				properties = new Properties(generatedBody.celestialBody);
			}

			// Parser Post Apply Event
			public void PostApply(ConfigNode node)
			{
				//Debug.Log("[Kopernicus]: Configuration.Body: Loaded body named: " + name);
				Utility.DumpObjectFields(generatedBody.celestialBody, " Post Load Celestial Body ");

			}
		}
	}
}

/**
	//
	// Methods
	//
	public PSystemBody AddBody (PSystemBody parent)
	{
		GameObject gameObject = new GameObject ();
		gameObject.transform.parent = base.transform;
		gameObject.name = . (123326);
		PSystemBody pSystemBody = gameObject.AddComponent<PSystemBody> ();
		if (parent != null)
		{
			while (true)
			{
				switch (6)
				{
				case 0:
					continue;
				}
				break;
			}
			if (!true)
			{
				RuntimeMethodHandle arg_53_0 = methodof (PSystem.AddBody (PSystemBody)).MethodHandle;
			}
			parent.children.Add (pSystemBody);
		}
		GameObject gameObject2 = new GameObject ();
		gameObject2.transform.parent = gameObject.transform;
		gameObject2.name = . (123349);
		pSystemBody.celestialBody = gameObject2.AddComponent<CelestialBody> ();
		pSystemBody.resources = gameObject2.AddComponent<PResource> ();
		if (parent != null)
		{
			while (true)
			{
				switch (7)
				{
				case 0:
					continue;
				}
				break;
			}
			pSystemBody.orbitDriver = gameObject2.AddComponent<OrbitDriver> ();
			pSystemBody.orbitDriver.referenceBody = parent.celestialBody;
			pSystemBody.orbitRenderer = gameObject2.AddComponent<OrbitRenderer> ();
		}
		return pSystemBody;
	}
}
*/

