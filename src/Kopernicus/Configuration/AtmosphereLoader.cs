/**
 * Kopernicus Planetary System Modifier
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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.ComponentModel;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class AtmosphereLoader : BaseLoader, IParserEventSubscriber
        {
            /// <summary>
            /// CelestialBody we're modifying
            /// </summary>
            public CelestialBody celestialBody;

            /// <summary>
            /// The ScaledSpace object of the body we're modifying
            /// </summary>
            public GameObject scaledVersion;

            // Do we have an atmosphere?
            [PreApply]
            [ParserTarget("enabled")]
            [KittopiaDescription("Whether the body has an atmosphere.")]
            public NumericParser<Boolean> enabled 
            {
                get { return celestialBody.atmosphere; }
                set { celestialBody.atmosphere = value; }
            }

            // Whether an AFG should get added
            [PreApply]
            [ParserTarget("addAFG")]
            [KittopiaHideOption]
            public NumericParser<Boolean> addAFG = true;

            // Does this atmosphere contain oxygen
            [ParserTarget("oxygen")]
            [KittopiaDescription("Whether the atmosphere contains oxygen.")]
            public NumericParser<Boolean> oxygen 
            {
                get { return celestialBody.atmosphereContainsOxygen; }
                set { celestialBody.atmosphereContainsOxygen = value; }
            }

            // Density at sea level
            [ParserTarget("staticDensityASL")]
            [KittopiaDescription("Atmospherical density at sea level. Used to calculate the parameters of the atmosphere if no curves are used.")]
            public NumericParser<Double> atmDensityASL
            {
                get { return celestialBody.atmDensityASL; }
                set { celestialBody.atmDensityASL = value; }
            }

            // atmosphereAdiabaticIndex
            [ParserTarget("adiabaticIndex")]
            public NumericParser<Double> atmosphereAdiabaticIndex
            {
                get { return celestialBody.atmosphereAdiabaticIndex; }
                set { celestialBody.atmosphereAdiabaticIndex = value; }
            }

            // atmosphere cutoff altitude (x3, for backwards compatibility)
            [ParserTarget("maxAltitude")]
            [KittopiaHideOption]
            public NumericParser<Double> maxAltitude
            {
                get { return celestialBody.atmosphereDepth; }
                set { celestialBody.atmosphereDepth = value; }
            }
            [ParserTarget("altitude")]
            [KittopiaHideOption]
            public NumericParser<Double> altitude
            {
                get { return celestialBody.atmosphereDepth; }
                set { celestialBody.atmosphereDepth = value; }
            }
            [ParserTarget("atmosphereDepth")]
            [KittopiaDescription("The height of the atmosphere.")]
            public NumericParser<Double> atmosphereDepth
            {
                get { return celestialBody.atmosphereDepth; }
                set { celestialBody.atmosphereDepth = value; }
            }

            // atmosphereGasMassLapseRate
            [ParserTarget("gasMassLapseRate")]
            public NumericParser<Double> atmosphereGasMassLapseRate
            {
                get { return celestialBody.atmosphereGasMassLapseRate; }
                set { celestialBody.atmosphereGasMassLapseRate = value; }
            }

            // atmosphereMolarMass
            [ParserTarget("atmosphereMolarMass")]
            public NumericParser<Double> atmosphereMolarMass
            {
                get { return celestialBody.atmosphereMolarMass; }
                set { celestialBody.atmosphereMolarMass = value; }
            }

            // Pressure curve
            [ParserTarget("pressureCurve")]
            [KittopiaDescription("Assigns a pressure value to a height value inside of the atmosphere.")]
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
            [ParserTarget("pressureCurveIsNormalized")]
            [KittopiaDescription("Whether the pressure curve should use absolute (0 - atmosphereDepth) or relative (0 - 1) values.")]
            public NumericParser<Boolean> atmospherePressureCurveIsNormalized
            {
                get { return celestialBody.atmospherePressureCurveIsNormalized; }
                set { celestialBody.atmospherePressureCurveIsNormalized = value; }
            }

            // Static pressure at sea level (all worlds are set to 1.0f?)
            [ParserTarget("staticPressureASL")]
            [KittopiaDescription("The static pressure at sea level. Used to calculate the parameters of the atmosphere if no curves are used.")]
            public NumericParser<Double> staticPressureASL
            {
                get { return celestialBody.atmospherePressureSeaLevel; }
                set { celestialBody.atmospherePressureSeaLevel = value; }
            }

            // Temperature curve (see below)
            [ParserTarget("temperatureCurve")]
            [KittopiaDescription("Assigns a temperature value to a height value inside of the atmosphere.")]
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
            [ParserTarget("temperatureCurveIsNormalized")]
            [KittopiaDescription("Whether the temperature curve should use absolute (0 - atmosphereDepth) or relative (0 - 1) values.")]
            public NumericParser<Boolean> atmosphereTemperatureCurveIsNormalized
            {
                get { return celestialBody.atmosphereTemperatureCurveIsNormalized; }
                set { celestialBody.atmosphereTemperatureCurveIsNormalized = value; }
            }

            // atmosphereTemperatureLapseRate
            [ParserTarget("temperatureLapseRate")]
            public NumericParser<Double> atmosphereTemperatureLapseRate
            {
                get { return celestialBody.atmosphereTemperatureLapseRate; }
                set { celestialBody.atmosphereTemperatureLapseRate = value; }
            }

            // TemperatureSeaLevel
            [ParserTarget("temperatureSeaLevel")]
            [KittopiaDescription("The static temperature at sea level. Used to calculate the parameters of the atmosphere if no curves are used.")]
            public NumericParser<Double> atmosphereTemperatureSeaLevel
            {
                get { return celestialBody.atmosphereTemperatureSeaLevel; }
                set { celestialBody.atmosphereTemperatureSeaLevel = value; }
            }

            // atmosphereTemperatureSunMultCurve
            [ParserTarget("temperatureSunMultCurve")]
            public FloatCurveParser atmosphereTemperatureSunMultCurve
            {
                get { return celestialBody.atmosphereTemperatureSunMultCurve; }
                set { celestialBody.atmosphereTemperatureSunMultCurve = value; }
            }

            // Temperature latitude bias
            [ParserTarget("temperatureLatitudeBiasCurve")]
            public FloatCurveParser latitudeTemperatureBiasCurve
            {
                get { return celestialBody.latitudeTemperatureBiasCurve; }
                set { celestialBody.latitudeTemperatureBiasCurve = value; }
            }

            // latitudeTemperatureSunMultCurve
            [ParserTarget("temperatureLatitudeSunMultCurve")]
            public FloatCurveParser latitudeTemperatureSunMultCurve
            {
                get { return celestialBody.latitudeTemperatureSunMultCurve; }
                set { celestialBody.latitudeTemperatureSunMultCurve = value; }
            }

            // axialTemperatureSunMultCurve
            [ParserTarget("temperatureAxialSunBiasCurve")]
            public FloatCurveParser axialTemperatureSunBiasCurve
            {
                get { return celestialBody.axialTemperatureSunBiasCurve; }
                set { celestialBody.axialTemperatureSunBiasCurve = value; }
            }
            
            // axialTemperatureSunMultCurve
            [ParserTarget("temperatureAxialSunMultCurve")]
            public FloatCurveParser axialTemperatureSunMultCurve
            {
                get { return celestialBody.axialTemperatureSunMultCurve; }
                set { celestialBody.axialTemperatureSunMultCurve = value; }
            }
            
            // eccentricityTemperatureBiasCurve
            [ParserTarget("temperatureEccentricityBiasCurve")]
            public FloatCurveParser eccentricityTemperatureBiasCurve
            {
                get { return celestialBody.eccentricityTemperatureBiasCurve; }
                set { celestialBody.eccentricityTemperatureBiasCurve = value; }
            }

            // ambient atmosphere color
            [ParserTarget("ambientColor")]
            [KittopiaDescription("All objects inside of the atmosphere will slightly shine in this color.")]
            public ColorParser ambientColor 
            {
                get { return celestialBody.atmosphericAmbientColor; }
                set { celestialBody.atmosphericAmbientColor = value.value; }
            }

            // AFG
            [ParserTarget("AtmosphereFromGround", allowMerge = true)]
            [KittopiaDescription("The atmosphere effect that is seen on the horizon.")]
            public AtmosphereFromGroundLoader atmosphereFromGround { get; set; }

            // light color
            [ParserTarget("lightColor")]
            [KittopiaHideOption]
            public ColorParser lightColor 
            {
                get { return atmosphereFromGround?.waveLength; }
                set
                {
                    if (atmosphereFromGround == null)
                        atmosphereFromGround = new AtmosphereFromGroundLoader();
                    atmosphereFromGround.waveLength = value;
                }
            }

            [KittopiaAction("Remove Atmosphere", destructive = true)]
            [KittopiaDescription("Removes the Atmosphere of this body.")]
            public void RemoveAtmosphere()
            {
                // Remove the Atmosphere from Ground
                AtmosphereFromGround[] afgs = celestialBody.GetComponentsInChildren<AtmosphereFromGround>();
                foreach (AtmosphereFromGround afg in afgs)
                {
                    UnityEngine.Object.Destroy(afg.gameObject);
                }

                // Disable the Light controller
                MaterialSetDirection[] msds = celestialBody.GetComponentsInChildren<MaterialSetDirection>();
                foreach (MaterialSetDirection msd in msds)
                {
                    UnityEngine.Object.Destroy(msd.gameObject);
                }

                // No Atmosphere :(
                celestialBody.atmosphere = false;
            }

            // Parser apply event
            void IParserEventSubscriber.Apply (ConfigNode node)
            {
                // If we don't want an atmosphere, ignore this step
                if(!celestialBody.atmosphere || !addAFG)
                    return;

                // If we don't already have an atmospheric shell generated
                if (scaledVersion.GetComponentsInChildren<AtmosphereFromGround> (true).Length == 0) 
                {
                    // Setup known defaults
                    celestialBody.atmospherePressureSeaLevel = 1.0f;
                }
                
                // Create the AFG Loader
                atmosphereFromGround = new AtmosphereFromGroundLoader();

                // Event
                Events.OnAtmosphereLoaderApply.Fire(this, node);
            }

            // Parser post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                Events.OnAtmosphereLoaderPostApply.Fire(this, node);
            }

            /// <summary>
            /// Creates a new Atmosphere Loader from the Injector context.
            /// </summary>
            public AtmosphereLoader()
            {
                // Is this the parser context?
                if (!Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("Must be executed in Injector context.");
                }
                
                // Store values
                celestialBody = generatedBody.celestialBody;
                scaledVersion = generatedBody.scaledVersion;
            }

            /// <summary>
            /// Creates a new Atmosphere Loader from a spawned CelestialBody.
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.CelestialBody)]
            public AtmosphereLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }

                // Store values
                celestialBody = body;
                scaledVersion = body.scaledBody;
                if (celestialBody.afg)
                {
                    atmosphereFromGround = new AtmosphereFromGroundLoader(celestialBody);
                }
            }
        }
    }
}
