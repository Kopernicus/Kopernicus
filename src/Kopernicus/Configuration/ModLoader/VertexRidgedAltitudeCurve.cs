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
            public class VertexRidgedAltitudeCurve : ModLoader<PQSMod_VertexRidgedAltitudeCurve>
            {
                // deformity
                [ParserTarget("deformity")]
                public NumericParser<Single> deformity
                {
                    get { return mod.deformity; }
                    set { mod.deformity = value; }
                }

                // ridgedAddFrequency
                [ParserTarget("ridgedAddFrequency")]
                public NumericParser<Single> ridgedAddFrequency
                {
                    get { return mod.ridgedAddFrequency; }
                    set { mod.ridgedAddFrequency = value; }
                }

                // ridgedAddLacunarity
                [ParserTarget("ridgedAddLacunarity")]
                public NumericParser<Single> ridgedAddLacunarity
                {
                    get { return mod.ridgedAddLacunarity; }
                    set { mod.ridgedAddLacunarity = value; }
                }

                // ridgedAddOctaves
                [ParserTarget("ridgedAddOctaves")]
                public NumericParser<Int32> ridgedAddOctaves
                {
                    get { return mod.ridgedAddOctaves; }
                    set { mod.ridgedAddOctaves = Mathf.Clamp(value, 1, 30); }
                }

                // ridgedAddSeed
                [ParserTarget("ridgedAddSeed")]
                public NumericParser<Int32> ridgedAddSeed
                {
                    get { return mod.ridgedAddSeed; }
                    set { mod.ridgedAddSeed = value; }
                }

                // ridgedMinimum
                [ParserTarget("ridgedMinimum")]
                public NumericParser<Single> ridgedMinimum
                {
                    get { return mod.ridgedMinimum; }
                    set { mod.ridgedMinimum = value; }
                }
                
                // ridgedMode
                [ParserTarget("ridgedMode")]
                public EnumParser<KopernicusNoiseQuality> ridgedMode
                {
                    get { return (KopernicusNoiseQuality) (Int32) mod.ridgedMode; }
                    set { mod.ridgedMode = (NoiseQuality) (Int32) value.Value; }
                }

                // simplexCurve
                [ParserTarget("simplexCurve")]
                public FloatCurveParser simplexCurve
                {
                    get { return mod.simplexCurve; }
                    set { mod.simplexCurve = value; }
                }

                // simplexFrequency
                [ParserTarget("simplexFrequency")]
                public NumericParser<Double> simplexFrequency
                {
                    get { return mod.simplexFrequency; }
                    set { mod.simplexFrequency = value; }
                }

                // simplexHeightEnd
                [ParserTarget("simplexHeightEnd")]
                public NumericParser<Double> simplexHeightEnd
                {
                    get { return mod.simplexHeightEnd; }
                    set { mod.simplexHeightEnd = value; }
                }

                // simplexHeightStart
                [ParserTarget("simplexHeightStart")]
                public NumericParser<Double> simplexHeightStart
                {
                    get { return mod.simplexHeightStart; }
                    set { mod.simplexHeightStart = value; }
                }

                // simplexOctaves
                [ParserTarget("simplexOctaves")]
                public NumericParser<Double> simplexOctaves
                {
                    get { return mod.simplexOctaves; }
                    set { mod.simplexOctaves = value; }
                }

                // simplexPersistence
                [ParserTarget("simplexPersistence")]
                public NumericParser<Double> simplexPersistence
                {
                    get { return mod.simplexPersistence; }
                    set { mod.simplexPersistence = value; }
                }

                // simplexSeed
                [ParserTarget("simplexSeed")]
                public NumericParser<Int32> simplexSeed
                {
                    get { return mod.simplexSeed; }
                    set { mod.simplexSeed = value; }
                }
            }
        }
    }
}

