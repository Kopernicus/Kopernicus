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

using Kopernicus.MaterialWrapper;

namespace Kopernicus
{
	namespace Configuration
	{
		[RequireConfigType(ConfigType.Node)]
		public class ScaledVersion : IParserEventSubscriber
		{
			// Node name which represents the scaled version material
			private const string materialNodeName = "Material";

			// Scaled representation of a planet for map view to modify
			private GameObject scaledVersion;
			private CelestialBody owner;

			// Type of object this body's scaled version is
			[PreApply]
			[ParserTarget("type", optional = true)]
			public EnumParser<BodyType> type { get; private set; }

			// Set the altitude where the fade to scaled space starts
			[ParserTarget("fadeStart", optional = true)]
			private NumericParser<float> fadeStart 
			{
				set { scaledVersion.GetComponent<ScaledSpaceFader> ().fadeStart = value.value; }
			}
			
			// Set the altitude where the fade to scaled space starts
			[ParserTarget("fadeEnd", optional = true)]
			private NumericParser<float> fadeEnd
			{
				set { scaledVersion.GetComponent<ScaledSpaceFader> ().fadeEnd = value.value; }
			}

			// Parser apply event
			void IParserEventSubscriber.Apply (ConfigNode node)
			{
				// Do we have material edits
				if (!node.HasNode (materialNodeName)) 
					return;

				// Get any existing material we might have on this scaled version
				Material material = scaledVersion.renderer.sharedMaterial;
				ConfigNode data = node.GetNode (materialNodeName);

				if (type.value != BodyType.Star) 
				{
					// If we are not a star, we need a scaled space fader and a sphere collider
					if (scaledVersion.GetComponent<ScaledSpaceFader> () == null) 
					{
						ScaledSpaceFader fader = scaledVersion.AddComponent<ScaledSpaceFader> ();
						fader.floatName = "_Opacity";
						fader.celestialBody = owner;
					}

					// sphere collider? can the root body have one?

					// If we have an atmosphere
					if (type.value == BodyType.Atmospheric) 
					{
						ScaledPlanetRimAerialLoader newMaterial = null;
						if (material != null) 
						{
							newMaterial = new ScaledPlanetRimAerialLoader (material);
							Parser.LoadObjectFromConfigurationNode (newMaterial, data);
						} 
						else 
						{
							newMaterial = Parser.CreateObjectFromConfigNode<ScaledPlanetRimAerialLoader> (data);
						}
						newMaterial.name = Guid.NewGuid().ToString();
						newMaterial.rimColorRamp.wrapMode = TextureWrapMode.Clamp;
						newMaterial.rimColorRamp.mipMapBias = 0.0f;
						scaledVersion.renderer.material = newMaterial;
						Debug.Log("Ramp: " + (newMaterial as ScaledPlanetRimAerial).rimColorRamp);
					}

					// Otherwise we are a vacuum body
					else 
					{
						ScaledPlanetSimpleLoader newMaterial = null;
						if (material != null) 
						{
							newMaterial = new ScaledPlanetSimpleLoader (material);
							Parser.LoadObjectFromConfigurationNode (newMaterial, data);
						} 
						else 
						{
							newMaterial = Parser.CreateObjectFromConfigNode<ScaledPlanetSimpleLoader> (data);
						}
						newMaterial.name = Guid.NewGuid().ToString();
						scaledVersion.renderer.material = newMaterial;
					}
				}

				// Otherwise we are a star
				else 
				{
					Debug.LogWarning("[Kopernicus]: Configuration.ScaledVersion: Implement modification of star scaled version");

					if (material != null) 
					{
						scaledVersion.renderer.material = new EmissiveMultiRampSunspots (material);
						Parser.LoadObjectFromConfigurationNode (scaledVersion.renderer.material, data);
					} 
					else 
					{
						scaledVersion.renderer.material = Parser.CreateObjectFromConfigNode<EmissiveMultiRampSunspots> (data);
					}
				}
			}

			// Post apply event
			void IParserEventSubscriber.PostApply (ConfigNode node) { }

			/**
			 * Default constructor - takes the scaledVersion game object
			 * 
			 * @param scaledVersion Scaled representation of a planet for map view to modify
			 **/
			public ScaledVersion (GameObject scaledVersion, CelestialBody owner, BodyType initialType)
			{
				// Get the scaled version object
				this.scaledVersion = scaledVersion;
				this.owner = owner;
				this.type = new EnumParser<BodyType>(initialType);

				// Ensure scaled version at least has a mesh filter and mesh renderer
				if(scaledVersion.GetComponent<MeshFilter>() == null)
					scaledVersion.AddComponent<MeshFilter>();
				if(scaledVersion.GetComponent<MeshRenderer>() == null)
				{
					scaledVersion.AddComponent<MeshRenderer>();
					scaledVersion.renderer.material = null;
				}
			}
		}
	}
}
