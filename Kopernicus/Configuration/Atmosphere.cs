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
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Kopernicus
{
	namespace Configuration
	{
		[RequireConfigType(ConfigType.Node)]
		public class Atmosphere : IParserEventSubscriber
		{
			// Resoruces that will be edited
			private GameObject scaledVersion;
			private CelestialBody celestialBody;

			// Do we have an atmosphere?
			[PreApply]
			[ParserTarget("enabled", optional = true)]
			private NumericParser<bool> enabled 
			{
				set { celestialBody.atmosphere = value.value; }
			}

			// Does this atmosphere contain oxygen
			[ParserTarget("oxygen", optional = true)]
			private NumericParser<bool> oxygen 
			{
				set { celestialBody.atmosphereContainsOxygen = value.value; }
			}
			
			// Temperature curve (see below)
			[ParserTarget("temperatureCurve", optional = true)]
			private AnimationCurveParser temperatureCurve 
			{
				set { celestialBody.temperatureCurve = value.curve; }
			}
			
			// Temperature multipler - I'm going to go out on a limb and suggest that this probably
			// functions similarly to the new atmosphere model.  Essentially
			// (temperature = temperatureMultipler * temperatureCurve[altitude]) 
			[ParserTarget("temperatureMultiplier", optional = true)]
			private NumericParser<float> temperatureMultiplier 
			{
				set { celestialBody.atmoshpereTemperatureMultiplier = value.value; }
			}
			
			// Static pressure at sea level (all worlds are set to 1.0f?)
			[ParserTarget("staticPressureASL", optional = true)]
			private NumericParser<float> staticPressureASL 
			{
				set { celestialBody.staticPressureASL = value.value; }
			}
			
			// ditto (all worlds set to 1.4285f).  Could be a *really* ancient atmosphere model
			[ParserTarget("altitudeMultiplier", optional = true)]
			private NumericParser<float> altitudeMultiplier 
			{
				set { celestialBody.altitudeMultiplier = value.value; }
			}

			// Pressure curve (pressure = pressure multipler * pressureCurve[altitude])
			[ParserTarget("pressureCurve", optional = true)]
			private AnimationCurveParser pressureCurve 
			{
				set { celestialBody.pressureCurve = value.curve; }
			}

			// Pressure multipler (pressure = pressure multipler * pressureCurve[altitude])
			[ParserTarget("pressureMultiplier", optional = true)]
			private NumericParser<float> pressureMultiplier 
			{
				set { celestialBody.pressureMultiplier = value.value; }
			}
			
			// Use legacy atmosphere - the fact that every stock world uses legacy may suggest that
			// the new atmosphere model may not work....
			[ParserTarget("enableLegacyAtmosphere", optional = true)]
			private NumericParser<bool> enableLegacyAtmosphere 
			{
				set { celestialBody.useLegacyAtmosphere = value.value; }
			}
			
			// pressure (in atm) = multipler * e ^ -(altitude / (scaleHeight * 1000))
			[ParserTarget("multiplier", optional = true)]
			private NumericParser<float> multiplier 
			{
				set { celestialBody.atmosphereMultiplier = value.value; }
			}
			
			// pressure (in atm) = atmosphereMultipler * e ^ -(altitude / (atmosphereScaleHeight * 1000))
			[ParserTarget("scaleHeight", optional = true)]
			private NumericParser<float> scaleHeight 
			{
				set { celestialBody.atmosphereScaleHeight = value.value; }
			}

			// I honestly think this may actually offset the altitude.  all stock worlds
			// have it set to 0f, but the sun has it set to 700f (but doesn't use an
			// atmosphere)
			[ParserTarget("altitudeOffset", optional = true)]
			private NumericParser<float> altitudeOffset 
			{
				set { celestialBody.altitudeOffset = value.value; }
			}
			
			// atmosphere cutoff altitude
			[ParserTarget("maxAltitude", optional = true)]
			private NumericParser<float> maxAltitude 
			{
				set { celestialBody.maxAtmosphereAltitude = value.value; }
			}
			
			// ambient atmosphere color
			[ParserTarget("ambientColor", optional = true)]
			private ColorParser ambientColor 
			{
				set { celestialBody.atmosphericAmbientColor = value.value; }
			}

			// light color
			[ParserTarget("lightColor", optional = true)]
			private ColorParser lightColor 
			{
				set { scaledVersion.GetComponentsInChildren<AtmosphereFromGround> (true) [0].waveLength = value.value; }
			}

			// Parser apply event
			void IParserEventSubscriber.Apply (ConfigNode node)
			{ 
				// If we don't want an atmosphere, ignore this step
				if(!celestialBody.atmosphere)
					return;

				// If we don't already have an atmospheric shell generated
				if (scaledVersion.GetComponentsInChildren<AtmosphereFromGround> (true).Length == 0) 
				{
					// Add the material light direction behavior
					MaterialSetDirection materialLightDirection = scaledVersion.AddComponent<MaterialSetDirection>();
					materialLightDirection.valueName            = "_localLightDirection";

					// Create the atmosphere shell game object
					GameObject scaledAtmosphere       = new GameObject("atmosphere");
					scaledAtmosphere.transform.parent = scaledVersion.transform;
					scaledAtmosphere.layer            = Constants.GameLayers.ScaledSpaceAtmosphere;
					MeshRenderer renderer             = scaledAtmosphere.AddComponent<MeshRenderer>();
					renderer.material                 = new Kopernicus.MaterialWrapper.AtmosphereFromGround();
					MeshFilter meshFilter             = scaledAtmosphere.AddComponent<MeshFilter>();
					meshFilter.sharedMesh             = Utility.ReferenceGeosphere ();
					scaledAtmosphere.AddComponent<AtmosphereFromGround>();
				}
			}

			// Parser post apply event
			void IParserEventSubscriber.PostApply (ConfigNode node) { } 

			// Store the scaled version and celestial body we are modifying internally
			public Atmosphere (CelestialBody celestialBody, GameObject scaledVersion)
			{
				this.scaledVersion = scaledVersion;
				this.celestialBody = celestialBody;
			}
		}
	}
}
