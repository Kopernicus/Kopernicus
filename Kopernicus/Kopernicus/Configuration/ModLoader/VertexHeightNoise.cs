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
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class VertexHeightNoise : ModLoader<PQSMod_VertexHeightNoise>
            {
                // The deformity of the simplex terrain noise
                [ParserTarget("deformity")]
                public NumericParser<Single> deformity
                {
                    get { return mod.deformity; }
                    set { mod.deformity = value; }
                }

                // The frequency of the simplex terrain noise
                [ParserTarget("frequency")]
                public NumericParser<Single> frequency
                {
                    get { return mod.frequency; }
                    set { mod.frequency = value; }
                }

                // Octaves of the simplex height noise
                [ParserTarget("octaves")]
                public NumericParser<Int32> octaves
                {
                    get { return mod.octaves; }
                    set { mod.octaves = Mathf.Clamp(value, 1, 30); }
                }

                // Persistence of the simplex height noise
                [ParserTarget("persistence")]
                public NumericParser<Single> persistence
                {
                    get { return mod.persistance; }
                    set { mod.persistance = value; }
                }

                // The seed of the simplex height noise
                [ParserTarget("seed")]
                public NumericParser<Int32> seed
                {
                    get { return mod.seed; }
                    set { mod.seed = value; }
                }

                // The type of the simplex height noise
                [ParserTarget("noiseType")]
                public EnumParser<KopernicusNoiseType> noiseType
                {
                    get { return (KopernicusNoiseType) (Int32) mod.noiseType; }
                    set { mod.noiseType = (PQSMod_VertexHeightNoise.NoiseType) (Int32) value.value; }
                }

                // The mode of the simplex height noise
                [ParserTarget("mode")]
                public EnumParser<KopernicusNoiseQuality> mode
                {
                    get { return (KopernicusNoiseQuality) (Int32) mod.mode; }
                    set { mod.mode = (NoiseQuality) (Int32) value.value; }
                }

                // The lacunarity of the simplex height noise
                [ParserTarget("lacunarity")]
                public NumericParser<Single> lacunarity
                {
                    get { return mod.lacunarity; }
                    set { mod.lacunarity = value; }
                }
            }
        }
    }
}

