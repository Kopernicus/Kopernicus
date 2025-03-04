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
    public class ScienceValuesLoader : BaseLoader, IParserEventSubscriber, ITypeParser<CelestialBodyScienceParams>
    {
        // Science parameters we are going to be modifying
        public CelestialBodyScienceParams Value { get; set; }

        // Science multiplier (?) for landed science
        [ParserTarget("landedDataValue")]
        [KittopiaDescription("Science multiplier for landed science.")]
        public NumericParser<Single> LandedDataValue
        {
            get { return Value.LandedDataValue; }
            set { Value.LandedDataValue = value; }
        }

        // Science multiplier (?) for splashed down science
        [ParserTarget("splashedDataValue")]
        [KittopiaDescription("Science multiplier for splashed down science.")]
        public NumericParser<Single> SplashedDataValue
        {
            get { return Value.SplashedDataValue; }
            set { Value.SplashedDataValue = value; }
        }

        // Science multiplier (?) for flying low science
        [ParserTarget("flyingLowDataValue")]
        [KittopiaDescription("Science multiplier for flying low science.")]
        public NumericParser<Single> FlyingLowDataValue
        {
            get { return Value.FlyingLowDataValue; }
            set { Value.FlyingLowDataValue = value; }
        }

        // Science multiplier (?) for flying high science
        [ParserTarget("flyingHighDataValue")]
        [KittopiaDescription("Science multiplier for flying high science.")]
        public NumericParser<Single> FlyingHighDataValue
        {
            get { return Value.FlyingHighDataValue; }
            set { Value.FlyingHighDataValue = value; }
        }

        // Science multiplier (?) for in space low science
        [ParserTarget("inSpaceLowDataValue")]
        [KittopiaDescription("Science multiplier for in space low science.")]
        public NumericParser<Single> InSpaceLowDataValue
        {
            get { return Value.InSpaceLowDataValue; }
            set { Value.InSpaceLowDataValue = value; }
        }

        // Science multiplier (?) for in space high science
        [ParserTarget("inSpaceHighDataValue")]
        [KittopiaDescription("Science multiplier for in space high science.")]
        public NumericParser<Single> InSpaceHighDataValue
        {
            get { return Value.InSpaceHighDataValue; }
            set { Value.InSpaceHighDataValue = value; }
        }

        // Some number describing recovery value (?) on this body.  Could be a multiplier
        // for value OR describe a multiplier for recovery of a craft returning from this
        // body....
        [ParserTarget("recoveryValue")]
        public NumericParser<Single> RecoveryValue
        {
            get { return Value.RecoveryValue; }
            set { Value.RecoveryValue = value; }
        }

        // Altitude when "flying at <body>" transitions from/to "from <body>'s upper atmosphere"
        [ParserTarget("flyingAltitudeThreshold")]
        [KittopiaDescription(
            "Altitude when \"flying at <body>\" transitions from/to \"from <body>'s upper atmosphere\"")]
        public NumericParser<Single> FlyingAltitudeThreshold
        {
            get { return Value.flyingAltitudeThreshold; }
            set { Value.flyingAltitudeThreshold = value; }
        }

        // Altitude when "in space low" transitions from/to "in space high"
        [ParserTarget("spaceAltitudeThreshold")]
        [KittopiaDescription("Altitude when \"in space low\" transitions from/to \"in space high\"")]
        public NumericParser<Single> SpaceAltitudeThreshold
        {
            get { return Value.spaceAltitudeThreshold; }
            set { Value.spaceAltitudeThreshold = value; }
        }

        // Default constructor
        public ScienceValuesLoader()
        {
            Value = generatedBody.celestialBody.scienceValues;
        }

        // Standard constructor takes a science parameters object
        public ScienceValuesLoader(CelestialBodyScienceParams value)
        {
            Value = value;
        }

        // Apply event
        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            Events.OnScienceValuesLoaderApply.Fire(this, node);
        }

        // Post-Apply event
        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
            Events.OnScienceValuesLoaderPostApply.Fire(this, node);
        }
    }
}
