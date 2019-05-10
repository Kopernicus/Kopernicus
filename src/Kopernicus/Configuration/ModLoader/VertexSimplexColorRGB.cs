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
using UnityEngine;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class VertexSimplexColorRGB : ModLoader<PQSMod_VertexSimplexColorRGB>
    {
        // blendColor
        [ParserTarget("blendColor")]
        public ColorParser BlendColor
        {
            get { return new Color(Mod.rBlend, Mod.gBlend, Mod.bBlend, 1); }
            set
            {
                Mod.bBlend = value.Value.b;
                Mod.rBlend = value.Value.r;
                Mod.gBlend = value.Value.g;
            }
        }

        // blend
        [ParserTarget("blend")]
        public NumericParser<Single> Blend
        {
            get { return Mod.blend; }
            set { Mod.blend = value; }
        }

        // frequency
        [ParserTarget("frequency")]
        public NumericParser<Double> Frequency
        {
            get { return Mod.frequency; }
            set { Mod.frequency = value; }
        }

        // octaves
        [ParserTarget("octaves")]
        public NumericParser<Double> Octaves
        {
            get { return Mod.octaves; }
            set { Mod.octaves = value; }
        }

        // persistence
        [ParserTarget("persistence")]
        public NumericParser<Double> Persistence
        {
            get { return Mod.persistence; }
            set { Mod.persistence = value; }
        }

        // seed
        [ParserTarget("seed")]
        public NumericParser<Int32> Seed
        {
            get { return Mod.seed; }
            set { Mod.seed = value; }
        }
    }
}

