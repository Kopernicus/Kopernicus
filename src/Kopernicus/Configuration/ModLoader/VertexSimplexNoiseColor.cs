﻿/**
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
    public class VertexSimplexNoiseColor : ModLoader<PQSMod_VertexSimplexNoiseColor>
    {
        // The deformity of the simplex terrain
        [ParserTarget("blend")]
        public NumericParser<Single> Blend
        {
            get { return Mod.blend; }
            set { Mod.blend = value; }
        }

        // Color of the class
        [ParserTarget("colorStart")]
        public ColorParser ColorStart
        {
            get { return Mod.colorStart; }
            set { Mod.colorStart = value; }
        }

        // Color of the class
        [ParserTarget("colorEnd")]
        public ColorParser ColorEnd
        {
            get { return Mod.colorEnd; }
            set { Mod.colorEnd = value; }
        }

        // The frequency of the simplex terrain
        [ParserTarget("frequency")]
        public NumericParser<Double> Frequency
        {
            get { return Mod.frequency; }
            set { Mod.frequency = value; }
        }

        // Octaves of the simplex height
        [ParserTarget("octaves")]
        public NumericParser<Double> Octaves
        {
            get { return Mod.octaves; }
            set { Mod.octaves = Utility.Clamp(value, 1, 30); }
        }

        // Persistence of the simplex height
        [ParserTarget("persistence")]
        public NumericParser<Double> Persistence
        {
            get { return Mod.persistence; }
            set { Mod.persistence = value; }
        }

        // The seed of the simplex height
        [ParserTarget("seed")]
        public NumericParser<Int32> Seed
        {
            get { return Mod.seed; }
            set { Mod.seed = value; }
        }
    }
}
