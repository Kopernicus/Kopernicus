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

            // Density at sea level
            [ParserTarget("staticDensityASL", optional = true)]
            private NumericParser<double> atmDensityASL
            {
                set { celestialBody.atmDensityASL = value.value; }
            }

            // atmosphereAdiabaticIndex
            [ParserTarget("adiabaticIndex", optional = true)]
            private NumericParser<double> atmosphereAdiabaticIndex
            {
                set { celestialBody.atmosphereAdiabaticIndex = value.value; }
            }

            // atmosphere cutoff altitude
            [ParserTarget("altitude", optional = true)]
            private NumericParser<double> maxAltitude
            {
                set { celestialBody.atmosphereDepth = value.value; }
            }

            // atmosphereGasMassLapseRate
            [ParserTarget("gasMassLapseRate", optional = true)]
            private NumericParser<double> atmosphereGasMassLapseRate
            {
                set { celestialBody.atmosphereGasMassLapseRate = value.value; }
            }

            // atmosphereMolarMass
            [ParserTarget("atmosphereMolarMass", optional = true)]
            private NumericParser<double> atmosphereMolarMass
            {
                set { celestialBody.atmosphereMolarMass = value.value; }
            }

            // Pressure curve (pressure = pressure multipler * pressureCurve[altitude])
            [ParserTarget("pressureCurve", optional = true)]
            private FloatCurveParser pressureCurve
            {
                set
                {
                    celestialBody.atmospherePressureCurve = value.curve;
                    celestialBody.atmosphereUsePressureCurve = true;
                }
            }

            // atmospherePressureCurveIsNormalized
            [ParserTarget("pressureCurveIsNormalized", optional = true)]
            private NumericParser<bool> atmospherePressureCurveIsNormalized
            {
                set { celestialBody.atmospherePressureCurveIsNormalized = value.value; }
            }

            // Static pressure at sea level (all worlds are set to 1.0f?)
            [ParserTarget("staticPressureASL", optional = true)]
            private NumericParser<float> staticPressureASL
            {
                set { celestialBody.atmospherePressureSeaLevel = value.value; }
            }

			// Temperature curve (see below)
			[ParserTarget("temperatureCurve", optional = true)]
			private FloatCurveParser temperatureCurve 
			{
                set
                {
                    celestialBody.atmosphereTemperatureCurve = value.curve;
                    celestialBody.atmosphereUseTemperatureCurve = true;
                }
			}

            // atmosphereTemperatureCurveIsNormalized
            [ParserTarget("temperatureCurveIsNormalized", optional = true)]
            private NumericParser<bool> atmosphereTemperatureCurveIsNormalized
            {
                set { celestialBody.atmosphereTemperatureCurveIsNormalized = value.value; }
            }

            // atmosphereTemperatureLapseRate
            [ParserTarget("temperatureLapseRate", optional = true)]
            private NumericParser<double> atmosphereTemperatureLapseRate
            {
                set { celestialBody.atmosphereTemperatureLapseRate = value.value; }
            }

            // TemperatureSeaLevel
            [ParserTarget("temperatureSeaLevel", optional = true)]
            private NumericParser<double> atmosphereTemperatureSeaLevel
            {
                set { celestialBody.atmosphereTemperatureSeaLevel = value.value; }
            }

            // atmosphereTemperatureSunMultCurve
            [ParserTarget("temperatureSunMultCurve", optional = true)]
            private FloatCurveParser atmosphereTemperatureSunMultCurve
            {
                set { celestialBody.atmosphereTemperatureSunMultCurve = value.curve; }
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

					// Setup known defaults
					celestialBody.atmospherePressureSeaLevel = 1.0f;
					// celestialBody.atmosphereMultiplier = 1.4285f;
				}
			}

			// Parser post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                // Manipulate AFG
                if (node.HasNode("AtmosphereFromGround"))
                {
                    AtmosphereFromGroundParser atmoFG = new AtmosphereFromGroundParser(scaledVersion.GetComponentsInChildren<AtmosphereFromGround>(true)[0]);
                }
            }

			// Store the scaled version and celestial body we are modifying internally
			public Atmosphere (CelestialBody celestialBody, GameObject scaledVersion)
			{
				this.scaledVersion = scaledVersion;
				this.celestialBody = celestialBody;
			}
		}

        [RequireConfigType(ConfigType.Node)]
        public class AtmosphereFromGroundParser : IParserEventSubscriber
        {
            // AtmosphereFromGround we're modifying
            public AtmosphereFromGround afg;

            // cameraHeight
            [ParserTarget("cameraHeight", optional = true)]
            private NumericParser<float> cameraHeight
            {
                set { afg.cameraHeight = value.value; }
            }

            // cameraHeight2
            [ParserTarget("cameraHeight2", optional = true)]
            private NumericParser<float> cameraHeight2
            {
                set { afg.cameraHeight2 = value.value; }
            }

            // cameraPos
            [ParserTarget("cameraPos", optional = true)]
            private Vector3Parser cameraPos
            {
                set { afg.cameraPos = value.value; }
            }

            // DEBUG_alwaysUpdateAll
            [ParserTarget("DEBUG_alwaysUpdateAll", optional = true)]
            private NumericParser<bool> DEBUG_alwaysUpdateAll
            {
                set { afg.DEBUG_alwaysUpdateAll = value.value; }
            }

            // doScale
            [ParserTarget("doScale", optional = true)]
            private NumericParser<bool> doScale
            {
                set { afg.doScale = value.value; }
            }

            // ESun
            [ParserTarget("ESun", optional = true)]
            private NumericParser<float> ESun
            {
                set { afg.ESun = value.value; }
            }

            // g
            [ParserTarget("g", optional = true)]
            private NumericParser<float> g
            {
                set { afg.g = value.value; }
            }

            // g2
            [ParserTarget("g2", optional = true)]
            private NumericParser<float> g2
            {
                set { afg.g2 = value.value; }
            }

            // innerRadius
            [ParserTarget("innerRadius", optional = true)]
            private NumericParser<float> innerRadius
            {
                set { afg.innerRadius = value.value; }
            }

            // innerRadius2
            [ParserTarget("innerRadius2", optional = true)]
            private NumericParser<float> innerRadius2
            {
                set { afg.innerRadius2 = value.value; }
            }

            // invWaveLength
            [ParserTarget("invWaveLength", optional = true)]
            private ColorParser invWaveLength
            {
                set { afg.invWaveLength = value.value; }
            }

            // Km
            [ParserTarget("Km", optional = true)]
            private NumericParser<float> Km
            {
                set { afg.Km = value.value; }
            }

            // Km4PI 
            [ParserTarget("Km4PI", optional = true)]
            private NumericParser<float> Km4PI 
            {
                set { afg.Km4PI = value.value; }
            }

            // KmESun
            [ParserTarget("KmESun", optional = true)]
            private NumericParser<float> KmESun
            {
                set { afg.KmESun = value.value; }
            }

            // Kr
            [ParserTarget("Kr", optional = true)]
            private NumericParser<float> Kr
            {
                set { afg.Kr = value.value; }
            }

            // Kr4PI 
            [ParserTarget("Kr4PI", optional = true)]
            private NumericParser<float> Kr4PI
            {
                set { afg.Kr4PI = value.value; }
            }

            // KrESun
            [ParserTarget("KrESun", optional = true)]
            private NumericParser<float> KrESun
            {
                set { afg.KrESun = value.value; }
            }

            // outerRadius
            [ParserTarget("outerRadius", optional = true)]
            private NumericParser<float> outerRadius
            {
                set { afg.outerRadius = value.value; }
            }

            // outerRadius
            [ParserTarget("outerRadius2", optional = true)]
            private NumericParser<float> outerRadius2
            {
                set { afg.outerRadius2 = value.value; }
            }

            // samples
            [ParserTarget("samples", optional = true)]
            private NumericParser<float> samples
            {
                set { afg.samples = value.value; }
            }

            // scale
            [ParserTarget("scale", optional = true)]
            private NumericParser<float> scale
            {
                set { afg.scale = value.value; }
            }

            // scaleDepth
            [ParserTarget("scaleDepth", optional = true)]
            private NumericParser<float> scaleDepth
            {
                set { afg.scaleDepth = value.value; }
            }

            // scaleOverScaleDepth
            [ParserTarget("scaleOverScaleDepth", optional = true)]
            private NumericParser<float> scaleOverScaleDepth
            {
                set { afg.scaleOverScaleDepth = value.value; }
            }

            // sunLightDirection
            [ParserTarget("sunLightDirection", optional = true)]
            private Vector3Parser sunLightDirection
            {
                set { afg.sunLightDirection = value.value; }
            }

            // Parser apply event
            void IParserEventSubscriber.Apply(ConfigNode node) { }

            // Parser post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                if (afg != null)
                {
                    MethodInfo afgSetMaterial = typeof(AtmosphereFromGround).GetMethod("SetMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
                    afgSetMaterial.Invoke(afg, new object[] { true });
                }
            }

            public AtmosphereFromGroundParser(AtmosphereFromGround afg)
            {
                this.afg = afg;
            }
        }
	}
}
