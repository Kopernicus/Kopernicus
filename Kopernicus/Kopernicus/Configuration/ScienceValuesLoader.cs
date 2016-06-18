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

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class ScienceValuesLoader : BaseLoader
        {
            // Science parameters we are going to be modifying
            public CelestialBodyScienceParams scienceParams { get; set; }

            // Science multipler (?) for landed science
            [ParserTarget("landedDataValue")]
            public NumericParser<float> landedDataValue 
            {
                get { return scienceParams.LandedDataValue; }
                set { scienceParams.LandedDataValue = value; }
            }

            // Science multipler (?) for splashed down science
            [ParserTarget("splashedDataValue")]
            public NumericParser<float> splashedDataValue 
            {
                get { return scienceParams.SplashedDataValue; }
                set { scienceParams.SplashedDataValue = value; }
            }

            // Science multipler (?) for flying low science
            [ParserTarget("flyingLowDataValue")]
            public NumericParser<float> flyingLowDataValue 
            {
                get { return scienceParams.FlyingLowDataValue; }
                set { scienceParams.FlyingLowDataValue = value; }
            }

            // Science multipler (?) for flying high science
            [ParserTarget("flyingHighDataValue")]
            public NumericParser<float> flyingHighDataValue 
            {
                get { return scienceParams.FlyingHighDataValue; }
                set { scienceParams.FlyingHighDataValue = value; }
            }
            
            // Science multipler (?) for in space low science
            [ParserTarget("inSpaceLowDataValue")]
            public NumericParser<float> inSpaceLowDataValue
            {
                get { return scienceParams.InSpaceLowDataValue; }
                set { scienceParams.InSpaceLowDataValue = value; }
            }
            
            // Science multipler (?) for in space high science
            [ParserTarget("inSpaceHighDataValue")]
            public NumericParser<float> inSpaceHighDataValue
            {
                get { return scienceParams.InSpaceHighDataValue; }
                set { scienceParams.InSpaceHighDataValue = value; }
            }
            
            // Some number describing recovery value (?) on this body.  Could be a multiplier
            // for value OR describe a multiplier for recovery of a craft returning from this
            // body....
            [ParserTarget("recoveryValue")]
            public NumericParser<float> recoveryValue
            {
                get { return scienceParams.RecoveryValue; }
                set { scienceParams.RecoveryValue = value; }
            }

            // Altitude when "flying at <body>" transistions from/to "from <body>'s upper atmosphere"
            [ParserTarget("flyingAltitudeThreshold")]
            public NumericParser<float> flyingAltitudeThreshold
            {
                get { return scienceParams.flyingAltitudeThreshold; }
                set { scienceParams.flyingAltitudeThreshold = value.value; }
            }
            
            // Altitude when "in space low" transistions from/to "in space high"
            [ParserTarget("spaceAltitudeThreshold")]
            public NumericParser<float> spaceAltitudeThreshold
            {
                get { return scienceParams.spaceAltitudeThreshold; }
                set { scienceParams.spaceAltitudeThreshold = value.value; }
            }

            // Default constructor
            public ScienceValuesLoader()
            {
                scienceParams = generatedBody.celestialBody.scienceValues;
            }

            // Standard constructor takes a science parameters object
            public ScienceValuesLoader (CelestialBodyScienceParams scienceParams)
            {
                this.scienceParams = scienceParams;
            }
        }
    }
}

