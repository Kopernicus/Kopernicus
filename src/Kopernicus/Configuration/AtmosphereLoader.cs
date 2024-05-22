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
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;
using Kopernicus.UI;

namespace Kopernicus.Configuration
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
        [KittopiaHideOption]
        public NumericParser<Boolean> Enabled
        {
            get { return Value.atmosphere; }
            set { Value.atmosphere = value; }
        }

        // Whether an AFG should get added
        [PreApply]
        [ParserTarget("addAFG")]
        [KittopiaHideOption]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
        public NumericParser<Boolean> AddAfg = true;

        // Does this atmosphere contain oxygen
        [ParserTarget("oxygen")]
        [KittopiaDescription("Whether the atmosphere contains oxygen.")]
        public NumericParser<Boolean> Oxygen
        {
            get { return Value.atmosphereContainsOxygen; }
            set { Value.atmosphereContainsOxygen = value; }
        }

        // Density at sea level
        [ParserTarget("staticDensityASL")]
        [KittopiaDescription(
            "Atmospheric density at sea level. Used to calculate the parameters of the atmosphere if no curves are used.")]
        public NumericParser<Double> AtmDensityAsl
        {
            get { return Value.atmDensityASL; }
            set { Value.atmDensityASL = value; }
        }

        // atmosphereAdiabaticIndex
        [ParserTarget("adiabaticIndex")]
        public NumericParser<Double> AtmosphereAdiabaticIndex
        {
            get { return Value.atmosphereAdiabaticIndex; }
            set { Value.atmosphereAdiabaticIndex = value; }
        }

        // atmosphere cutoff altitude (x3, for backwards compatibility)
        [ParserTarget("atmosphereDepth")]
        [ParserTarget("altitude")]
        [ParserTarget("maxAltitude")]
        [KittopiaDescription("The height of the atmosphere.")]
        public NumericParser<Double> AtmosphereDepth
        {
            get { return Value.atmosphereDepth; }
            set { Value.atmosphereDepth = value; }
        }

        // atmosphereGasMassLapseRate
        [ParserTarget("gasMassLapseRate")]
        public NumericParser<Double> AtmosphereGasMassLapseRate
        {
            get { return Value.atmosphereGasMassLapseRate; }
            set { Value.atmosphereGasMassLapseRate = value; }
        }

        // atmosphereMolarMass
        [ParserTarget("atmosphereMolarMass")]
        public NumericParser<Double> AtmosphereMolarMass
        {
            get { return Value.atmosphereMolarMass; }
            set { Value.atmosphereMolarMass = value; }
        }

        // Pressure curve
        [ParserTargetCollection("pressureCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        [KittopiaDescription("Assigns a pressure value to a height value inside of the atmosphere.")]
        public List<NumericCollectionParser<Single>> PressureCurve
        {
            get { return Utility.FloatCurveToList(Value.atmosphereUsePressureCurve ? Value.atmospherePressureCurve : null); }
            set
            {
                Value.atmospherePressureCurve = Utility.ListToFloatCurve(value);
                Value.atmosphereUsePressureCurve = true;
            }
        }

        // atmospherePressureCurveIsNormalized
        [ParserTarget("pressureCurveIsNormalized")]
        [KittopiaDescription(
            "Whether the pressure curve should use absolute (0 - atmosphereDepth) or relative (0 - 1) values.")]
        public NumericParser<Boolean> AtmospherePressureCurveIsNormalized
        {
            get { return Value.atmospherePressureCurveIsNormalized; }
            set { Value.atmospherePressureCurveIsNormalized = value; }
        }

        // Shock Temp Mult
        [ParserTarget("shockTemperatureMultiplier")]
        [KittopiaDescription(
            "Multiplies the External/Shock Temperature by this value. This is relevant for aerodynamic heating.")]
        public NumericParser<Double> ShockTemperatureMultiplier
        {
            get { return Value.shockTemperatureMultiplier; }
            set { Value.shockTemperatureMultiplier = value; }
        }

        // Static pressure at sea level (all worlds are set to 1.0f?)
        [ParserTarget("staticPressureASL")]
        [KittopiaDescription(
            "The static pressure at sea level. Used to calculate the parameters of the atmosphere if no curves are used.")]
        public NumericParser<Double> StaticPressureAsl
        {
            get { return Value.atmospherePressureSeaLevel; }
            set { Value.atmospherePressureSeaLevel = value; }
        }

        // Temperature curve (see below)
        [ParserTargetCollection("temperatureCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        [KittopiaDescription("Assigns a temperature value to a height value inside of the atmosphere.")]
        public List<NumericCollectionParser<Single>> TemperatureCurve
        {
            get { return Utility.FloatCurveToList(Value.atmosphereUseTemperatureCurve ? Value.atmosphereTemperatureCurve : null); }
            set
            {
                Value.atmosphereTemperatureCurve = Utility.ListToFloatCurve(value);
                Value.atmosphereUseTemperatureCurve = true;
            }
        }

        // atmosphereTemperatureCurveIsNormalized
        [ParserTarget("temperatureCurveIsNormalized")]
        [KittopiaDescription(
            "Whether the temperature curve should use absolute (0 - atmosphereDepth) or relative (0 - 1) values.")]
        public NumericParser<Boolean> AtmosphereTemperatureCurveIsNormalized
        {
            get { return Value.atmosphereTemperatureCurveIsNormalized; }
            set { Value.atmosphereTemperatureCurveIsNormalized = value; }
        }

        // atmosphereTemperatureLapseRate
        [ParserTarget("temperatureLapseRate")]
        public NumericParser<Double> AtmosphereTemperatureLapseRate
        {
            get { return Value.atmosphereTemperatureLapseRate; }
            set { Value.atmosphereTemperatureLapseRate = value; }
        }

        // TemperatureSeaLevel
        [ParserTarget("temperatureSeaLevel")]
        [KittopiaDescription(
            "The static temperature at sea level. Used to calculate the parameters of the atmosphere if no curves are used.")]
        public NumericParser<Double> AtmosphereTemperatureSeaLevel
        {
            get { return Value.atmosphereTemperatureSeaLevel; }
            set { Value.atmosphereTemperatureSeaLevel = value; }
        }

        // atmosphereTemperatureSunMultCurve
        [ParserTargetCollection("temperatureSunMultCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> AtmosphereTemperatureSunMultCurve
        {
            get { return Utility.FloatCurveToList(Value.atmosphereTemperatureSunMultCurve); }
            set { Value.atmosphereTemperatureSunMultCurve = Utility.ListToFloatCurve(value); }
        }

        // Temperature latitude bias
        [ParserTargetCollection("temperatureLatitudeBiasCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> LatitudeTemperatureBiasCurve
        {
            get { return Utility.FloatCurveToList(Value.latitudeTemperatureBiasCurve); }
            set { Value.latitudeTemperatureBiasCurve = Utility.ListToFloatCurve(value); }
        }

        // latitudeTemperatureSunMultCurve
        [ParserTargetCollection("temperatureLatitudeSunMultCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> LatitudeTemperatureSunMultCurve
        {
            get { return Utility.FloatCurveToList(Value.latitudeTemperatureSunMultCurve); }
            set { Value.latitudeTemperatureSunMultCurve = Utility.ListToFloatCurve(value); }
        }

        // axialTemperatureSunMultCurve
        [ParserTargetCollection("temperatureAxialSunBiasCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> AxialTemperatureSunBiasCurve
        {
            get { return Utility.FloatCurveToList(Value.axialTemperatureSunBiasCurve); }
            set { Value.axialTemperatureSunBiasCurve = Utility.ListToFloatCurve(value); }
        }

        // axialTemperatureSunMultCurve
        [ParserTargetCollection("temperatureAxialSunMultCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> AxialTemperatureSunMultCurve
        {
            get { return Utility.FloatCurveToList(Value.axialTemperatureSunMultCurve); }
            set { Value.axialTemperatureSunMultCurve = Utility.ListToFloatCurve(value); }
        }

        // eccentricityTemperatureBiasCurve
        [ParserTargetCollection("temperatureEccentricityBiasCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> EccentricityTemperatureBiasCurve
        {
            get { return Utility.FloatCurveToList(Value.eccentricityTemperatureBiasCurve); }
            set { Value.eccentricityTemperatureBiasCurve = Utility.ListToFloatCurve(value); }
        }

        // ambient atmosphere color
        [ParserTarget("ambientColor")]
        [KittopiaDescription("All objects inside of the atmosphere will slightly shine in this color.")]
        public ColorParser AmbientColor
        {
            get { return Value.atmosphericAmbientColor; }
            set { Value.atmosphericAmbientColor = value.Value; }
        }

        // AFG
        [ParserTarget("AtmosphereFromGround", AllowMerge = true)]
        [KittopiaDescription("The atmosphere effect that is seen on the horizon.")]
        public AtmosphereFromGroundLoader AtmosphereFromGround { get; set; }

        // light color
        [ParserTarget("lightColor")]
        [KittopiaHideOption]
        public ColorParser LightColor
        {
            get { return AtmosphereFromGround?.WaveLength; }
            set
            {
                if (AtmosphereFromGround == null)
                {
                    AtmosphereFromGround = new AtmosphereFromGroundLoader();
                }

                AtmosphereFromGround.WaveLength = value;
            }
        }

        [KittopiaDestructor]
        public void Destroy()
        {
            // No Atmosphere :(
            Value.atmosphere = false;
        }

        // Parser apply event
        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            // If we don't want an atmosphere, ignore this step
            if (!Value.atmosphere || !AddAfg)
            {
                return;
            }

            // If we don't already have an atmospheric shell generated
            if (Value.scaledBody.GetComponentsInChildren<AtmosphereFromGround>(true).Length == 0)
            {
                // Setup known defaults
                Value.atmospherePressureSeaLevel = 1.0f;
            }

            // Create the AFG Loader
            AtmosphereFromGround = new AtmosphereFromGroundLoader();

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
        [KittopiaConstructor(KittopiaConstructor.ParameterType.CelestialBody)]
        public AtmosphereLoader(CelestialBody body)
        {
            // Is this a spawned body?
            if (body.scaledBody == null || Injector.IsInPrefab)
            {
                throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
            }

            // Store values
            Value = body;
            if (Value.afg)
            {
                AtmosphereFromGround = new AtmosphereFromGroundLoader(Value);
            }

            Value.atmosphere = true;
        }
    }
}
