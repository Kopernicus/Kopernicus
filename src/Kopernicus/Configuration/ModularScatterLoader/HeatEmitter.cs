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
using Kopernicus.Components.ModularScatter;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Parsing;
using Kopernicus.UI;

namespace Kopernicus.Configuration.ModularScatterLoader
{
    /// <summary>
    /// The loader for heat emitting scatter objects
    /// </summary>
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class HeatEmitter : ComponentLoader<ModularScatter, HeatEmitterComponent>
    {
        // The ambient temperature.
        [ParserTarget("temperature")]
        [KittopiaDescription("The ambient temperature.")]
        public NumericParser<Double> AmbientTemp
        {
            get { return Value.ambientTemp; }
            set { Value.ambientTemp = value; }
        }

        // If the ambientTemp should be added.
        [ParserTarget("sumTemp")]
        [KittopiaDescription("If the ambientTemp should be added.")]
        public NumericParser<Boolean> SumTemp
        {
            get { return Value.sumTemp; }
            set { Value.sumTemp = value; }
        }

        // The name of the biome.
        [ParserTarget("biomeName")]
        [KittopiaDescription("The name of the biome.")]
        public String BiomeName
        {
            get { return Value.biomeName; }
            set { Value.biomeName = value; }
        }

        // Multiplier curve to change ambientTemp with altitude
        [ParserTarget("AltitudeCurve")]
        [KittopiaDescription("Multiplier curve to change ambientTemp with altitude.")]
        public FloatCurveParser AltitudeCurve
        {
            get { return Value.altitudeCurve; }
            set { Value.altitudeCurve = value; }
        }

        // Multiplier curve to change ambientTemp with latitude
        [ParserTarget("LatitudeCurve")]
        [KittopiaDescription("Multiplier curve to change ambientTemp with latitude.")]
        public FloatCurveParser LatitudeCurve
        {
            get { return Value.latitudeCurve; }
            set { Value.latitudeCurve = value; }
        }

        // Multiplier curve to change ambientTemp with longitude
        [ParserTarget("LongitudeCurve")]
        [KittopiaDescription("Multiplier curve to change ambientTemp with longitude.")]
        public FloatCurveParser LongitudeCurve
        {
            get { return Value.longitudeCurve; }
            set { Value.longitudeCurve = value; }
        }

        // Multiplier curve to change ambientTemp with distance
        [ParserTarget("DistanceCurve")]
        [KittopiaDescription("Multiplier curve to change ambientTemp with distance.")]
        public FloatCurveParser DistanceCurve
        {
            get { return Value.distanceCurve; }
            set { Value.distanceCurve = value; }
        }

        // Multiplier map for ambientTemp
        [ParserTarget("HeatMap")]
        [KittopiaDescription("Greyscale map for fine control of the ambientTemp on a planet. black = 0, white = 1")]
        public MapSOParserGreyScale<MapSO> HeatMap
        {
            get { return Value.heatMap; }
            set { Value.heatMap = value; }
        }
    }
}
