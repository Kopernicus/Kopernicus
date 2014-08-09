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

namespace Kopernicus
{
	namespace Configuration
	{
		public class Template : IParserEventSubscriber
		{
			// Cloned PSystemBody to expose to the config system
			public PSystemBody body;

			// PSystemBody to use as a template in lookup & clone
			private PSystemBody originalBody; 

			// Name of the body to use for the template
			[PreApply]
			[ParserTarget("name", optional = false, allowMerge = false)]
			private string name 
			{
				// Crawl the system prefab for the body
				set 
				{ 
					originalBody = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, value);
					if(originalBody == null)
					{
						throw new TemplateNotFoundException("Unable to find: " + value);
					}
				}
			}

			// Collection of PQS mods to remove 
			// something about a collection (probably strings)
			// a collection of mods

			// Apply event
			public void Apply (ConfigNode node)
			{
				// Instantiate (clone) the template body
				GameObject bodyGameObject = UnityEngine.Object.Instantiate (originalBody.gameObject) as GameObject;
				bodyGameObject.name = originalBody.name;
				bodyGameObject.transform.parent = Utility.Deactivator;
				body = bodyGameObject.GetComponent<PSystemBody> ();

				// Clone the scaled version
				body.scaledVersion = UnityEngine.Object.Instantiate (originalBody.scaledVersion) as GameObject;
				body.scaledVersion.transform.parent = Utility.Deactivator;
				body.scaledVersion.name = originalBody.scaledVersion.name;
				
				// Clone the PQS version (if it has one)
				if (body.pqsVersion != null) 
				{
					body.pqsVersion = UnityEngine.Object.Instantiate (originalBody.pqsVersion) as PQS;
					body.pqsVersion.transform.parent = Utility.Deactivator;
					body.pqsVersion.name = originalBody.pqsVersion.name;
				}
			}

			// Post apply event
			public void PostApply(ConfigNode node)
			{
				// Do something about removing PQS

				Debug.Log ("[Kopernicus]: Configuration.Template: Using Template \"" + body.celestialBody.bodyName + "\"");
			}

			// Private exception to throw in the case the template doesn't load
			private class TemplateNotFoundException : Exception
			{
				public TemplateNotFoundException(string s) : base(s)
				{

				}
			}
		}
	}
}

