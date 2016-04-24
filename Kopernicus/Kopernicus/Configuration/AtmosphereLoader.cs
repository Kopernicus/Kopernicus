/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using UnityEngine;

using Kopernicus.Components;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class AtmosphereLoader : BaseLoader, IParserEventSubscriber
        {
            // Resoruces that will be edited
            public GameObject scaledVersion;
            public CelestialBody celestialBody;

            // Do we have an atmosphere?
            [PreApply]
            [ParserTarget("enabled", optional = true)]
            public NumericParser<bool> enabled 
            {
                get { return celestialBody.atmosphere; }
                set { celestialBody.atmosphere = value; }
            }

            // Does this atmosphere contain oxygen
            [ParserTarget("oxygen", optional = true)]
            public NumericParser<bool> oxygen 
            {
                get { return celestialBody.atmosphereContainsOxygen; }
                set { celestialBody.atmosphereContainsOxygen = value; }
            }

            // Density at sea level
            [ParserTarget("staticDensityASL", optional = true)]
            public NumericParser<double> atmDensityASL
            {
                get { return celestialBody.atmDensityASL; }
                set { celestialBody.atmDensityASL = value; }
            }

            // atmosphereAdiabaticIndex
            [ParserTarget("adiabaticIndex", optional = true)]
            public NumericParser<double> atmosphereAdiabaticIndex
            {
                get { return celestialBody.atmosphereAdiabaticIndex; }
                set { celestialBody.atmosphereAdiabaticIndex = value; }
            }

            // atmosphere cutoff altitude (x3, for backwards compatibility)
            [ParserTarget("maxAltitude", optional = true)]
            public NumericParser<double> maxAltitude
            {
                get { return celestialBody.atmosphereDepth; }
                set { celestialBody.atmosphereDepth = value; }
            }
            [ParserTarget("altitude", optional = true)]
            public NumericParser<double> altitude
            {
                get { return celestialBody.atmosphereDepth; }
                set { celestialBody.atmosphereDepth = value; }
            }
            [ParserTarget("atmosphereDepth", optional = true)]
            public NumericParser<double> atmosphereDepth
            {
                get { return celestialBody.atmosphereDepth; }
                set { celestialBody.atmosphereDepth = value; }
            }

            // atmosphereGasMassLapseRate
            [ParserTarget("gasMassLapseRate", optional = true)]
            public NumericParser<double> atmosphereGasMassLapseRate
            {
                get { return celestialBody.atmosphereGasMassLapseRate; }
                set { celestialBody.atmosphereGasMassLapseRate = value; }
            }

            // atmosphereMolarMass
            [ParserTarget("atmosphereMolarMass", optional = true)]
            public NumericParser<double> atmosphereMolarMass
            {
                get { return celestialBody.atmosphereMolarMass; }
                set { celestialBody.atmosphereMolarMass = value; }
            }

            // Pressure curve
            [ParserTarget("pressureCurve", optional = true)]
            public FloatCurveParser pressureCurve
            {
                get { return celestialBody.atmosphereUsePressureCurve ? celestialBody.atmospherePressureCurve : null; }
                set
                {
                    celestialBody.atmospherePressureCurve = value;
                    celestialBody.atmosphereUsePressureCurve = true;
                }
            }

            // atmospherePressureCurveIsNormalized
            [ParserTarget("pressureCurveIsNormalized", optional = true)]
            public NumericParser<bool> atmospherePressureCurveIsNormalized
            {
                get { return celestialBody.atmospherePressureCurveIsNormalized; }
                set { celestialBody.atmospherePressureCurveIsNormalized = value; }
            }

            // Static pressure at sea level (all worlds are set to 1.0f?)
            [ParserTarget("staticPressureASL", optional = true)]
            public NumericParser<double> staticPressureASL
            {
                get { return celestialBody.atmospherePressureSeaLevel; }
                set { celestialBody.atmospherePressureSeaLevel = value; }
            }

            // Temperature curve (see below)
            [ParserTarget("temperatureCurve", optional = true)]
            public FloatCurveParser temperatureCurve 
            {
                get { return celestialBody.atmosphereUseTemperatureCurve ? celestialBody.atmosphereTemperatureCurve : null; }
                set
                {
                    celestialBody.atmosphereTemperatureCurve = value;
                    celestialBody.atmosphereUseTemperatureCurve = true;
                }
            }

            // atmosphereTemperatureCurveIsNormalized
            [ParserTarget("temperatureCurveIsNormalized", optional = true)]
            public NumericParser<bool> atmosphereTemperatureCurveIsNormalized
            {
                get { return celestialBody.atmosphereTemperatureCurveIsNormalized; }
                set { celestialBody.atmosphereTemperatureCurveIsNormalized = value; }
            }

            // atmosphereTemperatureLapseRate
            [ParserTarget("temperatureLapseRate", optional = true)]
            public NumericParser<double> atmosphereTemperatureLapseRate
            {
                get { return celestialBody.atmosphereTemperatureLapseRate; }
                set { celestialBody.atmosphereTemperatureLapseRate = value; }
            }

            // TemperatureSeaLevel
            [ParserTarget("temperatureSeaLevel", optional = true)]
            public NumericParser<double> atmosphereTemperatureSeaLevel
            {
                get { return celestialBody.atmosphereTemperatureSeaLevel; }
                set { celestialBody.atmosphereTemperatureSeaLevel = value; }
            }

            // atmosphereTemperatureSunMultCurve
            [ParserTarget("temperatureSunMultCurve", optional = true)]
            public FloatCurveParser atmosphereTemperatureSunMultCurve
            {
                get { return celestialBody.atmosphereTemperatureSunMultCurve; }
                set { celestialBody.atmosphereTemperatureSunMultCurve = value; }
            }

            // Temperature latitude bias
            [ParserTarget("temperatureLatitudeBiasCurve", optional = true)]
            public FloatCurveParser latitudeTemperatureBiasCurve
            {
                get { return celestialBody.latitudeTemperatureBiasCurve; }
                set { celestialBody.latitudeTemperatureBiasCurve = value; }
            }

            // latitudeTemperatureSunMultCurve
            [ParserTarget("temperatureLatitudeSunMultCurve", optional = true)]
            public FloatCurveParser latitudeTemperatureSunMultCurve
            {
                get { return celestialBody.latitudeTemperatureSunMultCurve; }
                set { celestialBody.latitudeTemperatureSunMultCurve = value; }
            }

            // axialTemperatureSunMultCurve
            [ParserTarget("temperatureAxialSunBiasCurve", optional = true)]
            public FloatCurveParser axialTemperatureSunBiasCurve
            {
                get { return celestialBody.axialTemperatureSunBiasCurve; }
                set { celestialBody.axialTemperatureSunBiasCurve = value; }
            }
            
            // axialTemperatureSunMultCurve
            [ParserTarget("temperatureAxialSunMultCurve", optional = true)]
            public FloatCurveParser axialTemperatureSunMultCurve
            {
                get { return celestialBody.axialTemperatureSunMultCurve; }
                set { celestialBody.axialTemperatureSunMultCurve = value; }
            }
            
            // eccentricityTemperatureBiasCurve
            [ParserTarget("temperatureEccentricityBiasCurve", optional = true)]
            public FloatCurveParser eccentricityTemperatureBiasCurve
            {
                get { return celestialBody.eccentricityTemperatureBiasCurve; }
                set { celestialBody.eccentricityTemperatureBiasCurve = value; }
            }

            // ambient atmosphere color
            [ParserTarget("ambientColor", optional = true)]
            public ColorParser ambientColor 
            {
                get { return celestialBody.atmosphericAmbientColor; }
                set { celestialBody.atmosphericAmbientColor = value.value; }
            }

            // light color
            [ParserTarget("lightColor", optional = true)]
            public ColorParser lightColor 
            {
                get { return scaledVersion.GetComponentsInChildren<AtmosphereFromGround>(true)[0].waveLength; }
                set { scaledVersion.GetComponentsInChildren<AtmosphereFromGround> (true) [0].waveLength = value.value; }
            }

            // AFG
            [ParserTarget("AtmosphereFromGround", optional = true, allowMerge = true)]
            public AtmosphereFromGroundLoader atmosphereFromGround { get; set; }

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
                    GameObject scaledAtmosphere       = new GameObject("Atmosphere");
                    scaledAtmosphere.transform.parent = scaledVersion.transform;
                    scaledAtmosphere.layer            = Constants.GameLayers.ScaledSpaceAtmosphere;
                    MeshRenderer renderer             = scaledAtmosphere.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial           = new MaterialWrapper.AtmosphereFromGround();
                    MeshFilter meshFilter             = scaledAtmosphere.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh             = Templates.ReferenceGeosphere;
                    scaledAtmosphere.AddComponent<AtmosphereFromGround>();
                    atmosphereFromGround = new AtmosphereFromGroundLoader();

                    // Setup known defaults
                    celestialBody.atmospherePressureSeaLevel = 1.0f;
                }
            }

            // Parser post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                if (atmosphereFromGround != null)
                    AFGInfo.StoreAFG(atmosphereFromGround.afg);
            }

            // Default constructor
            public AtmosphereLoader()
            {
                scaledVersion = generatedBody.scaledVersion;
                celestialBody = generatedBody.celestialBody;
            }

            // Runtime Constructor
            public AtmosphereLoader(CelestialBody body)
            {
                scaledVersion = body.scaledBody;
                celestialBody = body;
            }
        }
    }
}
