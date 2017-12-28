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

using LibNoise;
using System;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace NoiseLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class RidgedMultifractal : NoiseLoader<LibNoise.RidgedMultifractal>
            {
                [ParserTarget("frequency")]
                public NumericParser<Double> frequency
                {
                    get { return noise.Frequency; }
                    set { noise.Frequency = value; }
                }

                [ParserTarget("lacunarity")]
                public NumericParser<Double> lacunarity
                {
                    get { return noise.Lacunarity; }
                    set { noise.Lacunarity = value; }
                }
                
                [ParserTarget("quality")]
                public EnumParser<KopernicusNoiseQuality> quality
                {
                    get { return (KopernicusNoiseQuality)(Int32)noise.NoiseQuality; }
                    set { noise.NoiseQuality = (NoiseQuality)(Int32)value.Value; }
                }

                [ParserTarget("octaves")]
                public NumericParser<Int32> octaves
                {
                    get { return noise.OctaveCount; }
                    set { noise.OctaveCount = Mathf.Clamp(value, 1, 30); }
                }

                [ParserTarget("seed")]
                public NumericParser<Int32> seed
                {
                    get { return noise.Seed; }
                    set { noise.Seed = value; }
                }
            }
        }
    }
}