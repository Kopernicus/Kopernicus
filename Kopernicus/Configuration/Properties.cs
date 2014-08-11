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
		[RequireConfigType(ConfigType.Node)]
		public class Properties : IParserEventSubscriber
		{
			// Celestial body to edit
			public CelestialBody celestialBody { get; private set; }

			// Body description
			[ParserTarget("description", optional = true)]
			private string description 
			{
				set { celestialBody.bodyDescription = value; }
			}

			// Radius
			[ParserTarget("radius", optional = true)]
			private NumericParser<double> radius 
			{
				set { celestialBody.Radius = value.value; }
			}
			
			// GeeASL
			[ParserTarget("geeASL", optional = true)]
			private NumericParser<double> geeASL 
			{
				set { celestialBody.GeeASL = value.value; }
			}
			
			// Mass
			[ParserTarget("mass", optional = true)]
			private NumericParser<double> mass
			{
				set { celestialBody.Mass = value.value; }
			}
			
			// Does the body rotate?
			[ParserTarget("rotates", optional = true)]
			private NumericParser<bool> rotates
			{
				set { celestialBody.rotates = value.value; }
			}
			
			// Rotation period of the world
			[ParserTarget("rotationPeriod", optional = true)]
			private NumericParser<double> rotationPeriod
			{
				set { celestialBody.rotationPeriod = value.value; }
			}
			
			// Is this the home world
			[ParserTarget("isHomeWorld", optional = true)]
			private NumericParser<bool> isHomeWorld
			{
				set { celestialBody.isHomeWorld = value.value; }
			}

			// Time warp altitude limits
			[ParserTarget("timewarpAltitudeLimits", optional = true)]
			private NumericCollectionParser<float> timewarpAltitudeLimits 
			{
				set { celestialBody.timeWarpAltitudeLimits = value.value.ToArray (); }
			}

			// Science values of this body
			[ParserTarget("ScienceValues", optional = true, allowMerge = true)]
			private ScienceValues scienceValues;

			// Biomes of this body
			[ParserTargetCollection("Biomes", optional = true, nameSignificance = NameSignificance.None)]
			private List<Biome> biomes = new List<Biome>();

			// Biome definition texture
			[ParserTarget("biomeMap", optional = true)]
			private Texture2DParser biomeMap 
			{
				set { celestialBody.BiomeMap.Map = value.value; }
			}


			public void Apply (ConfigNode node) { }

			public void PostApply (ConfigNode node)
			{
				// Migrate the biome attributes to the biome map
				celestialBody.BiomeMap.Attributes = new CBAttributeMap.MapAttribute[biomes.Count];
				int index = 0;
				foreach (Biome biome in biomes) 
				{
					celestialBody.BiomeMap.Attributes[index++] = biome.attribute;
				}

				// Debug the science fields
				Utility.DumpObjectFields (celestialBody.scienceValues, " Science Values ");

				// Debug the biomes
				foreach(CBAttributeMap.MapAttribute biome in celestialBody.BiomeMap.Attributes)
				{
					Debug.Log("Found Biome: " + biome.name + " : " + biome.mapColor);
				}
			}

			// Properties requires a celestial body referece, as this class is designed to edit the body
			public Properties (CelestialBody celestialBody)
			{
				this.celestialBody = celestialBody;

				// We require a science values object
				if (this.celestialBody.scienceValues == null) 
					this.celestialBody.scienceValues = new CelestialBodyScienceParams ();
				
				// Create the science values cache
				scienceValues = new ScienceValues (this.celestialBody.scienceValues);

				// We require a map attributes object
				if (this.celestialBody.BiomeMap == null) 
				{
					this.celestialBody.BiomeMap = new CBAttributeMap ();
					this.celestialBody.BiomeMap.defaultAttribute = new CBAttributeMap.MapAttribute ();
					this.celestialBody.BiomeMap.Attributes = new CBAttributeMap.MapAttribute[0];
					this.celestialBody.BiomeMap.exactSearch = false;
					this.celestialBody.BiomeMap.nonExactThreshold = 0.05f; // blame this if things go wrong
				}

				// Populate the biomes list with any existing map attributes
				foreach (CBAttributeMap.MapAttribute attribute in this.celestialBody.BiomeMap.Attributes) 
					biomes.Add(new Biome(attribute));
			}
		}
	}
}

