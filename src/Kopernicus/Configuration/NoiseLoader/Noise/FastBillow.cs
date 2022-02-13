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
using Kopernicus.Configuration.Enumerations;
using LibNoise;
using UnityEngine;

namespace Kopernicus.Configuration.NoiseLoader.Noise
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class FastBillow : NoiseLoader<LibNoise.FastBillow>
    {
        [ParserTarget("frequency")]
        public NumericParser<Double> Frequency
        {
            get { return Noise.Frequency; }
            set { Noise.Frequency = value; }
        }

        [ParserTarget("lacunarity")]
        public NumericParser<Double> Lacunarity
        {
            get { return Noise.Lacunarity; }
            set { Noise.Lacunarity = value; }
        }

        [ParserTarget("quality")]
        public EnumParser<KopernicusNoiseQuality> Quality
        {
            get { return (KopernicusNoiseQuality)(Int32)Noise.NoiseQuality; }
            set { Noise.NoiseQuality = (NoiseQuality)(Int32)value.Value; }
        }

        [ParserTarget("octaves")]
        public NumericParser<Int32> Octaves
        {
            get { return Noise.OctaveCount; }
            set { Noise.OctaveCount = Mathf.Clamp(value, 1, 30); }
        }

        [ParserTarget("persistence")]
        public NumericParser<Double> Persistence
        {
            get { return Noise.Persistence; }
            set { Noise.Persistence = value; }
        }

        [ParserTarget("seed")]
        public NumericParser<Int32> Seed
        {
            get { return Noise.Seed; }
            set { Noise.Seed = value; }
        }
    }
}