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
using Kopernicus.UI;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class AtmosphereLoader : BaseLoader, IParserEventSubscriber, ITypeParser<CelestialBody>
        {
            /// <summary>
            /// CelestialBody we're modifying
            /// </summary>
            public CelestialBody Value { get; set; }

            // Do we have an atmosphere?
            [PreApply]
            [ParserTarget("enabled")]
            [KittopiaDescription("Whether the body has an atmosphere.")]
            public NumericParser<Boolean> enabled 
            {
                get { return Value.atmosphere; }
                set { Value.atmosphere = value; }
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
                get { return Value.atmosphereContainsOxygen; }
                set { Value.atmosphereContainsOxygen = value; }
            }

            // Density at sea level
            [ParserTarget("staticDensityASL")]
            [KittopiaDescription("Atmospherical density at sea level. Used to calculate the parameters of the atmosphere if no curves are used.")]
            public NumericParser<Double> atmDensityASL
            {
                get { return Value.atmDensityASL; }
                set { Value.atmDensityASL = value; }
            }

            // atmosphereAdiabaticIndex
            [ParserTarget("adiabaticIndex")]
            public NumericParser<Double> atmosphereAdiabaticIndex
            {
                get { return Value.atmosphereAdiabaticIndex; }
                set { Value.atmosphereAdiabaticIndex = value; }
            }

            // atmosphere cutoff altitude (x3, for backwards compatibility)
            [ParserTarget("maxAltitude")]
            [KittopiaHideOption]
            public NumericParser<Double> maxAltitude
            {
                get { return Value.atmosphereDepth; }
                set { Value.atmosphereDepth = value; }
            }
            [ParserTarget("altitude")]
            [KittopiaHideOption]
            public NumericParser<Double> altitude
            {
                get { return Value.atmosphereDepth; }
                set { Value.atmosphereDepth = value; }
            }
            [ParserTarget("atmosphereDepth")]
            [KittopiaDescription("The height of the atmosphere.")]
            public NumericParser<Double> atmosphereDepth
            {
                get { return Value.atmosphereDepth; }
                set { Value.atmosphereDepth = value; }
            }

            // atmosphereGasMassLapseRate
            [ParserTarget("gasMassLapseRate")]
            public NumericParser<Double> atmosphereGasMassLapseRate
            {
                get { return Value.atmosphereGasMassLapseRate; }
                set { Value.atmosphereGasMassLapseRate = value; }
            }

            // atmosphereMolarMass
            [ParserTarget("atmosphereMolarMass")]
            public NumericParser<Double> atmosphereMolarMass
            {
                get { return Value.atmosphereMolarMass; }
                set { Value.atmosphereMolarMass = value; }
            }

            // Pressure curve
            [ParserTarget("pressureCurve")]
            [KittopiaDescription("Assigns a pressure value to a height value inside of the atmosphere.")]
            public FloatCurveParser pressureCurve
            {
                get { return Value.atmosphereUsePressureCurve ? Value.atmospherePressureCurve : null; }
                set
                {
                    Value.atmospherePressureCurve = value;
                    Value.atmosphereUsePressureCurve = true;
                }
            }

            // atmospherePressureCurveIsNormalized
            [ParserTarget("pressureCurveIsNormalized")]
            [KittopiaDescription("Whether the pressure curve should use absolute (0 - atmosphereDepth) or relative (0 - 1) values.")]
            public NumericParser<Boolean> atmospherePressureCurveIsNormalized
            {
                get { return Value.atmospherePressureCurveIsNormalized; }
                set { Value.atmospherePressureCurveIsNormalized = value; }
            }

            // Static pressure at sea level (all worlds are set to 1.0f?)
            [ParserTarget("staticPressureASL")]
            [KittopiaDescription("The static pressure at sea level. Used to calculate the parameters of the atmosphere if no curves are used.")]
            public NumericParser<Double> staticPressureASL
            {
                get { return Value.atmospherePressureSeaLevel; }
                set { Value.atmospherePressureSeaLevel = value; }
            }

            // Temperature curve (see below)
            [ParserTarget("temperatureCurve")]
            [KittopiaDescription("Assigns a temperature value to a height value inside of the atmosphere.")]
            public FloatCurveParser temperatureCurve 
            {
                get { return Value.atmosphereUseTemperatureCurve ? Value.atmosphereTemperatureCurve : null; }
                set
                {
                    Value.atmosphereTemperatureCurve = value;
                    Value.atmosphereUseTemperatureCurve = true;
                }
            }

            // atmosphereTemperatureCurveIsNormalized
            [ParserTarget("temperatureCurveIsNormalized")]
            [KittopiaDescription("Whether the temperature curve should use absolute (0 - atmosphereDepth) or relative (0 - 1) values.")]
            public NumericParser<Boolean> atmosphereTemperatureCurveIsNormalized
            {
                get { return Value.atmosphereTemperatureCurveIsNormalized; }
                set { Value.atmosphereTemperatureCurveIsNormalized = value; }
            }

            // atmosphereTemperatureLapseRate
            [ParserTarget("temperatureLapseRate")]
            public NumericParser<Double> atmosphereTemperatureLapseRate
            {
                get { return Value.atmosphereTemperatureLapseRate; }
                set { Value.atmosphereTemperatureLapseRate = value; }
            }

            // TemperatureSeaLevel
            [ParserTarget("temperatureSeaLevel")]
            [KittopiaDescription("The static temperature at sea level. Used to calculate the parameters of the atmosphere if no curves are used.")]
            public NumericParser<Double> atmosphereTemperatureSeaLevel
            {
                get { return Value.atmosphereTemperatureSeaLevel; }
                set { Value.atmosphereTemperatureSeaLevel = value; }
            }

            // atmosphereTemperatureSunMultCurve
            [ParserTarget("temperatureSunMultCurve")]
            public FloatCurveParser atmosphereTemperatureSunMultCurve
            {
                get { return Value.atmosphereTemperatureSunMultCurve; }
                set { Value.atmosphereTemperatureSunMultCurve = value; }
            }

            // Temperature latitude bias
            [ParserTarget("temperatureLatitudeBiasCurve")]
            public FloatCurveParser latitudeTemperatureBiasCurve
            {
                get { return Value.latitudeTemperatureBiasCurve; }
                set { Value.latitudeTemperatureBiasCurve = value; }
            }

            // latitudeTemperatureSunMultCurve
            [ParserTarget("temperatureLatitudeSunMultCurve")]
            public FloatCurveParser latitudeTemperatureSunMultCurve
            {
                get { return Value.latitudeTemperatureSunMultCurve; }
                set { Value.latitudeTemperatureSunMultCurve = value; }
            }

            // axialTemperatureSunMultCurve
            [ParserTarget("temperatureAxialSunBiasCurve")]
            public FloatCurveParser axialTemperatureSunBiasCurve
            {
                get { return Value.axialTemperatureSunBiasCurve; }
                set { Value.axialTemperatureSunBiasCurve = value; }
            }
            
            // axialTemperatureSunMultCurve
            [ParserTarget("temperatureAxialSunMultCurve")]
            public FloatCurveParser axialTemperatureSunMultCurve
            {
                get { return Value.axialTemperatureSunMultCurve; }
                set { Value.axialTemperatureSunMultCurve = value; }
            }
            
            // eccentricityTemperatureBiasCurve
            [ParserTarget("temperatureEccentricityBiasCurve")]
            public FloatCurveParser eccentricityTemperatureBiasCurve
            {
                get { return Value.eccentricityTemperatureBiasCurve; }
                set { Value.eccentricityTemperatureBiasCurve = value; }
            }

            // ambient atmosphere color
            [ParserTarget("ambientColor")]
            [KittopiaDescription("All objects inside of the atmosphere will slightly shine in this color.")]
            public ColorParser ambientColor 
            {
                get { return Value.atmosphericAmbientColor; }
                set { Value.atmosphericAmbientColor = value.Value; }
            }

            // AFG
            [ParserTarget("AtmosphereFromGround", AllowMerge = true)]
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
                AtmosphereFromGround[] afgs = Value.GetComponentsInChildren<AtmosphereFromGround>();
                foreach (AtmosphereFromGround afg in afgs)
                {
                    UnityEngine.Object.Destroy(afg.gameObject);
                }

                // Disable the Light controller
                MaterialSetDirection[] msds = Value.GetComponentsInChildren<MaterialSetDirection>();
                foreach (MaterialSetDirection msd in msds)
                {
                    UnityEngine.Object.Destroy(msd.gameObject);
                }

                // No Atmosphere :(
                Value.atmosphere = false;
            }

            // Parser apply event
            void IParserEventSubscriber.Apply (ConfigNode node)
            {
                // If we don't want an atmosphere, ignore this step
                if(!Value.atmosphere || !addAFG)
                    return;

                // If we don't already have an atmospheric shell generated
                if (Value.scaledBody.GetComponentsInChildren<AtmosphereFromGround> (true).Length == 0) 
                {
                    // Setup known defaults
                    Value.atmospherePressureSeaLevel = 1.0f;
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
                Value = generatedBody.celestialBody;
                Value.scaledBody = generatedBody.scaledVersion;
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
                Value = body;
                if (Value.afg)
                {
                    atmosphereFromGround = new AtmosphereFromGroundLoader(Value);
                }
            }
        }
    }
}
