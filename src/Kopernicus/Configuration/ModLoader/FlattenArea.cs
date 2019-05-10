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
using Kopernicus.Configuration.Parsing;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class FlattenArea : ModLoader<PQSMod_FlattenArea>
    {
        // DEBUG_showColors
        [ParserTarget("DEBUG_showColors")]
        public NumericParser<Boolean> DebugShowColors
        {
            get { return Mod.DEBUG_showColors; }
            set { Mod.DEBUG_showColors = value; }
        }

        // flattenTo
        [ParserTarget("flattenTo")]
        public NumericParser<Double> FlattenTo
        {
            get { return Mod.flattenTo; }
            set { Mod.flattenTo = value; }
        }

        // innerRadius
        [ParserTarget("innerRadius")]
        public NumericParser<Double> InnerRadius
        {
            get { return Mod.innerRadius; }
            set { Mod.innerRadius = value; }
        }

        // outerRadius
        [ParserTarget("outerRadius")]
        public NumericParser<Double> OuterRadius
        {
            get { return Mod.outerRadius; }
            set { Mod.outerRadius = value; }
        }

        // position
        [ParserTarget("position")]
        public Vector3Parser Position
        {
            get { return Mod.position; }
            set { Mod.position = value; }
        }

        // position v2
        [ParserTarget("Position")]
        private PositionParser Position2
        {
            set { Mod.position = value; }
        }

        // smoothEnd
        [ParserTarget("smoothEnd")]
        public NumericParser<Double> SmoothEnd
        {
            get { return Mod.smoothEnd; }
            set { Mod.smoothEnd = value; }
        }

        // smoothStart
        [ParserTarget("smoothStart")]
        public NumericParser<Double> SmoothStart
        {
            get { return Mod.smoothStart; }
            set { Mod.smoothStart = value; }
        }
    }
}

