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

				// Stage 1 - Load all of the bodies
				foreach (ConfigNode bodyNode in rootConfig.GetNodes(bodyNodeName)) 
				{
					// Load this body from the 
					Body body = Parser.CreateObjectFromConfigNode<Body> (bodyNode);
					bodies.Add (body.name, body);

					Debug.Log ("[Kopernicus]: Configuration.Loader: Loaded Body: " + body.name);
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

				// Stage 5 - sort by distance from parent (discover how this effects local bodies)
				RecursivelySortBodies (system.rootBody);

				return system;
			}

			// Sort bodies by distance from parent body
			private void RecursivelySortBodies (PSystemBody body)
			{
				body.children = body.children.OrderBy (b => b.celestialBody.orbit.semiMajorAxis * (1 + b.celestialBody.orbit.eccentricity)).ToList ();
				foreach (PSystemBody child in body.children) 
				{
					RecursivelySortBodies (child);
				}
			}
		}
	}
}

