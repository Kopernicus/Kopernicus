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
			public PSystem Generate()
			{
				// Dictionary of bodies generated
				Dictionary<string, PSystemBody> bodies = new Dictionary<string, PSystemBody>();

				// Retrieve the root config node
				ConfigNode rootConfig = GameDatabase.Instance.GetConfigs (rootNodeName) [0].config;

				// Iterate through body configs
				foreach (ConfigNode bodyNode in rootConfig.GetNodes(bodyNodeName))
				{
					// Load this body from the 
					PSystemBody body = Parser.CreateObjectFromConfigNode<Body>(bodyNode).generatedBody;
					bodies.Add(body.celestialBody.name, body);

					Debug.Log("[Kopernicus]: Configuration.Loader: Loaded Body: " + body.celestialBody.bodyName);
				}

				return null;
			}
		}
	}
}

