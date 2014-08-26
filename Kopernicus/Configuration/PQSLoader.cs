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
		public class PQSLoader : IParserEventSubscriber
		{
			// PQS we are creating
			public PQS pqsVersion { get; private set; }

			// Required PQSMods
			private PQSMod_CelestialBodyTransform   transform;
			private PQSMod_MaterialSetDirection     lightDirection;
			private PQSMod_UVPlanetRelativePosition uvs;
			private PQSMod_QuadMeshColliders        collider;
			
			// Surface physics material
			[ParserTarget("PhysicsMaterial", optional = true, allowMerge = true)]
			private PhysicsMaterialParser physicsMaterial
			{
				set { collider.physicsMaterial = value.material; }
			}

			/**
			 * Constructor for new PQS
			 **/
			public PQSLoader ()
			{
				// Create a new PQS
				GameObject root = new GameObject ();
				root.transform.parent = Utility.Deactivator;
				this.pqsVersion = root.AddComponent<PQS> ();

				// TODO - Copy internal settings from a different body

				// Create the required mods for the pqs
				GameObject required = new GameObject ("_Required");
				required.transform.parent = pqsVersion.transform;

				// Create the celestial body transform
				transform = required.AddComponent<PQSMod_CelestialBodyTransform> ();
				transform.requirements = PQS.ModiferRequirements.Default;
				transform.modEnabled = true;
				transform.order = 10;

				// TODO - Many things to set up for transform

				// Create the material direction setter
				lightDirection = required.AddComponent<PQSMod_MaterialSetDirection> ();
				lightDirection.valueName = "_sunLightDirection";
				lightDirection.requirements = PQS.ModiferRequirements.Default;
				lightDirection.modEnabled = true;
				lightDirection.order = 100;

				// Create the pqs quad UV controller
				uvs = required.AddComponent<PQSMod_UVPlanetRelativePosition> ();
				uvs.requirements = PQS.ModiferRequirements.Default;
				uvs.modEnabled = true;
				uvs.order = int.MaxValue;

				// Create the pqs quad collider
				collider = required.AddComponent<PQSMod_QuadMeshColliders> ();
				collider.physicsMaterial = new PhysicMaterial ();
				collider.maxLevelOffset = 0;
				collider.requirements = PQS.ModiferRequirements.Default;
				collider.modEnabled = true;
				collider.order = 100;
				
				// Create physics material editor
				physicsMaterial = new PhysicsMaterialParser (collider.physicsMaterial);
			}

			/**
			 * Constructor for pre-existing PQS
			 * 
			 * @param pqsVersion Existing PQS to augment
			 **/
			public PQSLoader (PQS pqsVersion)
			{
				this.pqsVersion = pqsVersion;

				// Get the required PQS information
				transform = pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform> (true).Where (mod => mod.transform.parent == pqsVersion.transform).FirstOrDefault ();
				lightDirection = pqsVersion.GetComponentsInChildren<PQSMod_MaterialSetDirection>(true).Where (mod => mod.transform.parent == pqsVersion.transform).FirstOrDefault ();
				uvs = pqsVersion.GetComponentsInChildren<PQSMod_UVPlanetRelativePosition>(true).Where (mod => mod.transform.parent == pqsVersion.transform).FirstOrDefault ();
				collider = pqsVersion.GetComponentsInChildren<PQSMod_QuadMeshColliders>(true).Where (mod => mod.transform.parent == pqsVersion.transform).FirstOrDefault ();

				// Create physics material editor
				physicsMaterial = new PhysicsMaterialParser (collider.physicsMaterial);
			}


			void IParserEventSubscriber.Apply(ConfigNode node)
			{

			}

			void IParserEventSubscriber.PostApply(ConfigNode node)
			{

			}
		}
	}
}

#pragma warning restore 0414
