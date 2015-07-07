/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
* Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
 * ------------------------------------------------------------- 
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
                set { celestialBody.GeeASL = value.value; hasGASL = true; }
			}
            private bool hasGASL = false;
			
			// Mass
			[ParserTarget("mass", optional = true)]
			private NumericParser<double> mass
			{
                set { celestialBody.Mass = value.value; hasMass = true; }
			}
            private bool hasMass = false;

            // Grav Param
            [ParserTarget("gravParameter", optional = true)]
            private NumericParser<double> gravParameter
            {
                set { celestialBody.gMagnitudeAtCenter = celestialBody.gravParameter = value.value; hasGravParam = true; }
            }
            private bool hasGravParam = false;
			
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
			
			// Is the body tidally locked to its parent?
			[ParserTarget("tidallyLocked", optional = true)]
			private NumericParser<bool> tidallyLocked
			{
				set { celestialBody.tidallyLocked = value.value; }
			}

			// Initial rotation of the world
			[ParserTarget("initialRotation", optional = true)]
			private NumericParser<double> initialRotation
			{
				set { celestialBody.initialRotation = value.value; }
			}
			
            // albedo
            [ParserTarget("albedo", optional = true)]
            private NumericParser<double> albedo
            {
                set { celestialBody.albedo = value.value; }
            }

            // emissivity
            [ParserTarget("emissivity", optional = true)]
            private NumericParser<double> emissivity
            {
                set { celestialBody.emissivity = value.value; }
            }

            // coreTemperatureOffset
            [ParserTarget("coreTemperatureOffset", optional = true)]
            private NumericParser<double> coreTemperatureOffset
            {
                set { celestialBody.coreTemperatureOffset = value.value; }
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

			// Sphere of Influence
			[ParserTarget("sphereOfInfluence", optional = true)]
			private NumericParser<double> sphereOfInfluence
			{
                set { Templates.sphereOfInfluence.Add(celestialBody.name, value.value); }
			}

            // Hill Sphere
            [ParserTarget("hillSphere", optional = true)]
            private NumericParser<double> hillSphere
            {
                set { Templates.hillSphere.Add(celestialBody.bodyTransform.name, value.value); }
            }

            // solarRotationPeriod
            [ParserTarget("solarRotationPeriod", optional = true)]
            private NumericParser<bool> solarRotationPeriod
            {
                set { celestialBody.solarRotationPeriod = value.value; }
            }

			// Science values of this body
			[ParserTarget("ScienceValues", optional = true, allowMerge = true)]
			private ScienceValues scienceValues;

			// Biomes of this body
			[PreApply]
			[ParserTargetCollection("Biomes", optional = true, nameSignificance = NameSignificance.None)]
			private List<Biome> biomes = new List<Biome>();

			// DEPRECATED -- Biome definition texture (from GameDatabase)
			[ParserTarget("biomeMapD", optional = true)]
			private Texture2DParser biomeMapDeprecated
			{
				set 
				{
					if (value.value != null) 
					{
						celestialBody.BiomeMap = ScriptableObject.CreateInstance<CBAttributeMapSO> ();
						celestialBody.BiomeMap.exactSearch = false;
						celestialBody.BiomeMap.nonExactThreshold = 0.05f;
						celestialBody.BiomeMap.CreateMap (MapSO.MapDepth.RGB, value.value);
						celestialBody.BiomeMap.Attributes = biomes.Select (b => b.attribute).ToArray ();
					}
				}
			}

			// Biome definition via MapSO parser
			[ParserTarget("biomeMap", optional = true)]
			private MapSOParser_RGB<CBAttributeMapSO> biomeMap
			{
				set 
				{
					if (value.value != null) 
					{
						celestialBody.BiomeMap = value.value;
						celestialBody.BiomeMap.exactSearch = false;
						celestialBody.BiomeMap.nonExactThreshold = 0.05f;
						celestialBody.BiomeMap.Attributes = biomes.Select (b => b.attribute).ToArray ();
					}
				}
			}

            [ParserTarget("useTheInName", optional = true)]
            public NumericParser<bool> useTheInName
            {
                set { celestialBody.use_The_InName = value.value; }
            }

			void IParserEventSubscriber.Apply (ConfigNode node) { }

			void IParserEventSubscriber.PostApply (ConfigNode node)
			{
				// Debug the fields (TODO - remove)
				Utility.DumpObjectFields (celestialBody.scienceValues, " Science Values ");
				if (celestialBody.BiomeMap != null) 
				{
					foreach (CBAttributeMapSO.MapAttribute biome in celestialBody.BiomeMap.Attributes) 
					{
						Logger.Active.Log ("Found Biome: " + biome.name + " : " + biome.mapColor + " : " + biome.value);
					}
				}

				// TODO - tentative fix, needs to be able to be configured (if it can be?)
				if (celestialBody.progressTree == null) 
				{
					celestialBody.progressTree = new KSPAchievements.CelestialBodySubtree (celestialBody);
					Logger.Active.Log ("Added Progress Tree");
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
			}

            public void PostApplyUpdate()
            {
                // Mass
                if (hasGravParam)
                    GravParamToOthers();
                else if (hasMass)
                    MassToOthers();
                else
                    GeeASLToOthers();
            }
            // Mass converters
            private void GeeASLToOthers()
            {
                double rsq = celestialBody.Radius;
                rsq *= rsq;
                celestialBody.gMagnitudeAtCenter = celestialBody.GeeASL * 9.81 * rsq;
                celestialBody.gravParameter = celestialBody.gMagnitudeAtCenter;
                celestialBody.Mass = celestialBody.gravParameter * (1 / 6.674E-11);
                Logger.Active.Log("Via surface G, set gravParam to " + celestialBody.gravParameter + ", mass to " + celestialBody.Mass);
            }

            // converts mass to Gee ASL using a body's radius.
            private void MassToOthers()
            {
                double rsq = celestialBody.Radius;
                rsq *= rsq;
                celestialBody.GeeASL = celestialBody.Mass * (6.674E-11 / 9.81) / rsq;
                celestialBody.gMagnitudeAtCenter = celestialBody.GeeASL * 9.81 * rsq;
                celestialBody.gravParameter = celestialBody.gMagnitudeAtCenter;
                Logger.Active.Log("Via mass, set gravParam to " + celestialBody.gravParameter + ", surface G to " + celestialBody.GeeASL);
            }

            private void GravParamToOthers()
            {
                double rsq = celestialBody.Radius;
                rsq *= rsq;
                celestialBody.Mass = celestialBody.gravParameter * (1 / 6.674E-11);
                celestialBody.GeeASL = celestialBody.gravParameter / 9.81 / rsq;
                celestialBody.gMagnitudeAtCenter = celestialBody.gravParameter;
                Logger.Active.Log("Via gravParam, set mass to " + celestialBody.Mass + ", surface G to " + celestialBody.GeeASL);
            }
		}
	}
}

#pragma warning restore 0414
