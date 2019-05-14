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
        // The average heat emitted by the scatter
        [ParserTarget("heat")]
        [KittopiaDescription("The average heat emitted by the scatter.")]
        public NumericParser<Double> Heat
        {
            get { return Value.HeatRate; }
            set { Value.HeatRate = value; }
        }

        // How many seconds pass between applying the heat to a vessel
        [ParserTarget("interval")]
        [KittopiaDescription("How many seconds pass between applying the heat to a vessel.")]
        public NumericParser<Double> Interval
        {
            get { return Value.HeatInterval; }
            set { Value.HeatInterval = value; }
        }
        
        // Controls the how much of the average heat gets applied at a certain distance
        [ParserTarget("DistanceCurve")]
        [KittopiaDescription("Controls the how much of the average heat gets applied at a certain distance.")]
        public FloatCurveParser DistanceCurve
        {
            get { return Value.HeatCurve; }
            set { Value.HeatCurve = value; }
        }
    }
}