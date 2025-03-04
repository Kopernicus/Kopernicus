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

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TangentTextureRanges : ModLoader<PQSMod_TangentTextureRanges>
    {
        // highEnd
        [ParserTarget("highEnd")]
        public NumericParser<Double> HighEnd
        {
            get { return Mod.highEnd; }
            set { Mod.highEnd = value; }
        }

        // highStart
        [ParserTarget("highStart")]
        public NumericParser<Double> HighStart
        {
            get { return Mod.highStart; }
            set { Mod.highStart = value; }
        }

        // lowEnd
        [ParserTarget("lowEnd")]
        public NumericParser<Double> LowEnd
        {
            get { return Mod.lowEnd; }
            set { Mod.lowEnd = value; }
        }

        // lowStart
        [ParserTarget("lowStart")]
        public NumericParser<Double> LowStart
        {
            get { return Mod.lowStart; }
            set { Mod.lowStart = value; }
        }

        // modulo
        [ParserTarget("modulo")]
        public NumericParser<Double> Modulo
        {
            get { return Mod.modulo; }
            set { Mod.modulo = value; }
        }
    }
}
