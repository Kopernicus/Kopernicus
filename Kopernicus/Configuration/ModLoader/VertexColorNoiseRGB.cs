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

using System;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class VertexColorNoiseRGB : ModLoader<PQSMod_VertexColorNoiseRGB>
            {
                // Amount of color that will be applied
                [ParserTarget("blend")]
                public NumericParser<float> blend
                {
                    get { return mod.blend; }
                    set { mod.blend = value; }
                }

                // Amount of red
                [ParserTarget("rBlend")]
                public NumericParser<float> rBlend
                {
                    get { return mod.rBlend; }
                    set { mod.rBlend = value; }
                }

                // Amount of green
                [ParserTarget("gBlend")]
                public NumericParser<float> gBlend
                {
                    get { return mod.gBlend; }
                    set { mod.gBlend = value; }
                }

                // Amount of blue
                [ParserTarget("bBlend")]
                public NumericParser<float> bBlend
                {
                    get { return mod.bBlend; }
                    set { mod.bBlend = value; }
                }

                // The frequency of the noise
                [ParserTarget("frequency")]
                public NumericParser<float> frequency
                {
                    get { return mod.frequency; }
                    set { mod.frequency = value; }
                }

                // Lacunarity of the noise
                [ParserTarget("lacunarity")]
                public NumericParser<float> lacunarity
                {
                    get { return mod.lacunarity; }
                    set { mod.lacunarity = value; }
                }

                // Noise quality
                [ParserTarget("mode")]
                public EnumParser<LibNoise.Unity.QualityMode> mode
                {
                    get { return mod.mode; }
                    set { mod.mode = value; }
                }

                // Noise algorithm
                [ParserTarget("noiseType")]
                public EnumParser<PQSMod_VertexColorNoiseRGB.NoiseType> noiseType
                {
                    get { return mod.noiseType; }
                    set { mod.noiseType = value; }
                }

                // Octaves of the noise
                [ParserTarget("octaves")]
                public NumericParser<int> octaves
                {
                    get { return mod.octaves; }
                    set { mod.octaves = value; }
                }

                // Persistance of the noise
                [ParserTarget("persistance")]
                public NumericParser<float> persistance
                {
                    get { return mod.persistance; }
                    set { mod.persistance = value; }
                }

                // The seed of the noise
                [ParserTarget("seed")]
                public NumericParser<int> seed
                {
                    get { return mod.seed; }
                    set { mod.seed = value; }
                }
            }
        }
    }
}

