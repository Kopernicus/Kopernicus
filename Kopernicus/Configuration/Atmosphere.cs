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

            // atmosphere cutoff altitude (x3, for backwards compatibility)
            [ParserTarget("altitude", optional = true)]
            private NumericParser<double> maxAltitude
            {
                set { celestialBody.atmosphereDepth = value.value; }
            }
            [ParserTarget("maxAltitude", optional = true)]
            private NumericParser<double> maxAltitude2
            {
                set { celestialBody.atmosphereDepth = value.value; }
            }
            [ParserTarget("atmosphereDepth", optional = true)]
            private NumericParser<double> atmosphereDepth
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

            // Temperature latitude bias
            [ParserTarget("temperatureLatitudeBiasCurve", optional = true)]
            private FloatCurveParser latitudeTemperatureBiasCurve
            {
                set
                {
                    celestialBody.latitudeTemperatureBiasCurve = value.curve;
                }
            }

            // latitudeTemperatureSunMultCurve
            [ParserTarget("temperatureLatitudeSunMultCurve", optional = true)]
            private FloatCurveParser latitudeTemperatureSunMultCurve
            {
                set
                {
                    celestialBody.latitudeTemperatureSunMultCurve = value.curve;
                }
            }

            // axialTemperatureSunMultCurve
            [ParserTarget("temperatureAxialSunMultCurve", optional = true)]
            private FloatCurveParser axialTemperatureSunMultCurve
            {
                set
                {
                    celestialBody.axialTemperatureSunMultCurve = value.curve;
                }
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
                AtmosphereFromGround afg = scaledVersion.GetComponentsInChildren<AtmosphereFromGround>(true)[0] as AtmosphereFromGround;
                if (node.HasNode("AtmosphereFromGround") && afg != null)
                {
                    AtmosphereFromGroundParser atmoFG = new AtmosphereFromGroundParser(afg, celestialBody);
                    Parser.LoadObjectFromConfigurationNode(atmoFG, node.GetNode("AtmosphereFromGround"));
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
            private const float INVSCALEFACTOR = (1f / 6000f); // since ScaledSpace doesn't exist to query.

            // AtmosphereFromGround we're modifying
            public AtmosphereFromGround afg;
            private CelestialBody body;

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

            // innerRadius
            [ParserTarget("innerRadius", optional = true)]
            private NumericParser<float> innerRadius
            {
                set{ afg.innerRadius = value.value * INVSCALEFACTOR; }
            }

            // invWaveLength
            [ParserTarget("invWaveLength", optional = true)]
            private ColorParser invWaveLength
            {
                set
                {
                    afg.invWaveLength = value.value;
                    afg.waveLength = new Color((float)Math.Sqrt(Math.Sqrt(1d / afg.invWaveLength[0])), (float)Math.Sqrt(Math.Sqrt(1d / afg.invWaveLength[1])), (float)Math.Sqrt(Math.Sqrt(1d / afg.invWaveLength[2])), 0.5f);
                }
            }

            // Km
            [ParserTarget("Km", optional = true)]
            private NumericParser<float> Km
            {
                set { afg.Km = value.value; }
            }

            // Kr
            [ParserTarget("Kr", optional = true)]
            private NumericParser<float> Kr
            {
                set { afg.Kr = value.value; }
            }

            // outerRadius
            [ParserTarget("outerRadius", optional = true)]
            private NumericParser<float> outerRadius
            {
                set { afg.outerRadius = value.value * INVSCALEFACTOR; }
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

            [ParserTarget("transformScale", optional = true)]
            private Vector3Parser transformScale
            {
                set { afg.transform.localScale = value.value; afg.doScale = false; }
            }

            // waveLength
            [ParserTarget("waveLength", optional = true)]
            private ColorParser waveLength
            {
                set
                {
                    afg.waveLength = value.value;
                    afg.invWaveLength = new Color((float)(1d/Math.Pow(afg.waveLength[0], 4)), (float)(1d/Math.Pow(afg.waveLength[1], 4)), (float)(1d/Math.Pow(afg.waveLength[2], 4)), 0.5f);
                }
            }

            // outerRadiusMult
            [ParserTarget("outerRadiusMult", optional = true)]
            private NumericParser<float> outerRadiusMult
            {
                set { afg.outerRadius = (((float)body.Radius) * value.value) * INVSCALEFACTOR; }
            }

            // innerRadiusMult
            [ParserTarget("innerRadiusMult", optional = true)]
            private NumericParser<float> innerRadiusMult
            {
                set { afg.innerRadius = afg.outerRadius * value.value; }
            }

            public static void CalculatedMembers(AtmosphereFromGround atmo)
            {
                atmo.g2 = atmo.g * atmo.g;
                atmo.KrESun = atmo.Kr * atmo.ESun;
                atmo.KmESun = atmo.Km * atmo.ESun;
                atmo.Kr4PI = atmo.Kr * 4f * Mathf.PI;
                atmo.Km4PI = atmo.Km * 4f * Mathf.PI;
                atmo.outerRadius2 = atmo.outerRadius * atmo.outerRadius;
                atmo.innerRadius2 = atmo.innerRadius * atmo.innerRadius;
                atmo.scale = 1f / (atmo.outerRadius - atmo.innerRadius);
                atmo.scaleOverScaleDepth = atmo.scale / atmo.scaleDepth;

                if (atmo.doScale)
                    atmo.transform.localScale = Vector3.one * 1.025f;
            }

            // Parser apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                // Set Defaults
                afg.planet = body;
                afg.ESun = 30f;
                afg.Kr = 0.00125f;
                afg.Km = 0.00015f;
                
                afg.samples = 4f;
                afg.g = -0.85f;
                if (afg.waveLength == new Color(0f, 0f, 0f, 0f))
                {
                    afg.waveLength = new Color(0.65f, 0.57f, 0.475f, 0.5f);
                }
                afg.outerRadius = (((float)body.Radius) * 1.025f) * INVSCALEFACTOR;
                afg.innerRadius = afg.outerRadius * 0.975f;
                afg.scaleDepth = -0.25f;
                afg.invWaveLength = new Color((float)(1d / Math.Pow(afg.waveLength[0], 4)), (float)(1d / Math.Pow(afg.waveLength[1], 4)), (float)(1d / Math.Pow(afg.waveLength[2], 4)), 0.5f);

                CalculatedMembers(afg);
            }

            // Parser post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                CalculatedMembers(afg); // with the new values.
                AFGInfo.StoreAFG(afg);
            }

            public AtmosphereFromGroundParser(AtmosphereFromGround afg, CelestialBody body)
            {
                this.afg = afg;
                this.body = body;
            }

        }
    }
}
