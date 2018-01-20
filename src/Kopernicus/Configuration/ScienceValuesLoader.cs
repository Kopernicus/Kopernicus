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
        public class ScienceValuesLoader : BaseLoader, IParserEventSubscriber, ITypeParser<CelestialBodyScienceParams>
        {
            // Science parameters we are going to be modifying
            public CelestialBodyScienceParams Value { get; set; }

            // Science multipler (?) for landed science
            [ParserTarget("landedDataValue")]
            [KittopiaDescription("Science multipler for landed science.")]
            public NumericParser<Single> landedDataValue 
            {
                get { return Value.LandedDataValue; }
                set { Value.LandedDataValue = value; }
            }

            // Science multipler (?) for splashed down science
            [ParserTarget("splashedDataValue")]
            [KittopiaDescription("Science multipler for splashed down science.")]
            public NumericParser<Single> splashedDataValue 
            {
                get { return Value.SplashedDataValue; }
                set { Value.SplashedDataValue = value; }
            }

            // Science multipler (?) for flying low science
            [ParserTarget("flyingLowDataValue")]
            [KittopiaDescription("Science multipler for flying low science.")]
            public NumericParser<Single> flyingLowDataValue 
            {
                get { return Value.FlyingLowDataValue; }
                set { Value.FlyingLowDataValue = value; }
            }

            // Science multipler (?) for flying high science
            [ParserTarget("flyingHighDataValue")]
            [KittopiaDescription("Science multipler for flying high science.")]
            public NumericParser<Single> flyingHighDataValue 
            {
                get { return Value.FlyingHighDataValue; }
                set { Value.FlyingHighDataValue = value; }
            }
            
            // Science multipler (?) for in space low science
            [ParserTarget("inSpaceLowDataValue")]
            [KittopiaDescription("Science multipler for in space low science.")]
            public NumericParser<Single> inSpaceLowDataValue
            {
                get { return Value.InSpaceLowDataValue; }
                set { Value.InSpaceLowDataValue = value; }
            }
            
            // Science multipler (?) for in space high science
            [ParserTarget("inSpaceHighDataValue")]
            [KittopiaDescription("Science multipler for in space high science.")]
            public NumericParser<Single> inSpaceHighDataValue
            {
                get { return Value.InSpaceHighDataValue; }
                set { Value.InSpaceHighDataValue = value; }
            }
            
            // Some number describing recovery value (?) on this body.  Could be a multiplier
            // for value OR describe a multiplier for recovery of a craft returning from this
            // body....
            [ParserTarget("recoveryValue")]
            public NumericParser<Single> recoveryValue
            {
                get { return Value.RecoveryValue; }
                set { Value.RecoveryValue = value; }
            }

            // Altitude when "flying at <body>" transistions from/to "from <body>'s upper atmosphere"
            [ParserTarget("flyingAltitudeThreshold")]
            [KittopiaDescription("Altitude when \"flying at <body>\" transistions from/to \"from <body>'s upper atmosphere\"")]
            public NumericParser<Single> flyingAltitudeThreshold
            {
                get { return Value.flyingAltitudeThreshold; }
                set { Value.flyingAltitudeThreshold = value; }
            }
            
            // Altitude when "in space low" transistions from/to "in space high"
            [ParserTarget("spaceAltitudeThreshold")]
            [KittopiaDescription("Altitude when \"in space low\" transistions from/to \"in space high\"")]
            public NumericParser<Single> spaceAltitudeThreshold
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
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element)]
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
}

