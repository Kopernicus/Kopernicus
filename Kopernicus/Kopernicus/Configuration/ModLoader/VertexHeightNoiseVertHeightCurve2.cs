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
            public class VertexHeightNoiseVertHeightCurve2 : ModLoader<PQSMod_VertexHeightNoiseVertHeightCurve2>
            {
                // deformity
                [ParserTarget("deformity")]
                public NumericParser<float> deformity
                {
                    get { return mod.deformity; }
                    set { mod.deformity = value; }
                }

                // ridgedAddFrequency
                [ParserTarget("ridgedAddFrequency")]
                public NumericParser<float> ridgedAddFrequency
                {
                    get { return mod.ridgedAddFrequency; }
                    set { mod.ridgedAddFrequency = value; }
                }

                // ridgedAddLacunarity
                [ParserTarget("ridgedAddLacunarity")]
                public NumericParser<float> ridgedAddLacunarity
                {
                    get { return mod.ridgedAddLacunarity; }
                    set { mod.ridgedAddLacunarity = value; }
                }

                // ridgedAddOctaves
                [ParserTarget("ridgedAddOctaves")]
                public NumericParser<int> ridgedAddOctaves
                {
                    get { return mod.ridgedAddOctaves; }
                    set { mod.ridgedAddOctaves = value; }
                }

                // ridgedAddOctaves
                [ParserTarget("ridgedAddSeed")]
                public NumericParser<int> ridgedAddSeed
                {
                    get { return mod.ridgedAddSeed; }
                    set { mod.ridgedAddSeed = value; }
                }

                // ridgedMode
                [ParserTarget("ridgedMode")]
                public EnumParser<LibNoise.Unity.QualityMode> ridgedMode
                {
                    get { return mod.ridgedMode; }
                    set { mod.ridgedMode = value; }
                }

                // ridgedSubFrequency
                [ParserTarget("ridgedSubFrequency")]
                public NumericParser<float> ridgedSubFrequency
                {
                    get { return mod.ridgedSubFrequency; }
                    set { mod.ridgedSubFrequency = value; }
                }

                // ridgedSubLacunarity
                [ParserTarget("ridgedSubLacunarity")]
                public NumericParser<float> ridgedSubLacunarity
                {
                    get { return mod.ridgedSubLacunarity; }
                    set { mod.ridgedSubLacunarity = value; }
                }

                // ridgedSubOctaves
                [ParserTarget("ridgedSubOctaves")]
                public NumericParser<int> ridgedSubOctaves
                {
                    get { return mod.ridgedSubOctaves; }
                    set { mod.ridgedSubOctaves = value; }
                }

                // ridgedSubSeed
                [ParserTarget("ridgedSubSeed")]
                public NumericParser<int> ridgedSubSeed
                {
                    get { return mod.ridgedSubSeed; }
                    set { mod.ridgedSubSeed = value; }
                }

                // simplexCurve
                [ParserTarget("simplexCurve")]
                public FloatCurveParser simplexCurve
                {
                    get { return mod.simplexCurve != null ? new FloatCurve(mod.simplexCurve.keys) : new FloatCurve(); }
                    set { mod.simplexCurve = value.curve.Curve; }
                }

                // simplexFrequency
                [ParserTarget("simplexFrequency")]
                public NumericParser<double> simplexFrequency
                {
                    get { return mod.simplexFrequency; }
                    set { mod.simplexFrequency = value; }
                }

                // simplexHeightEnd
                [ParserTarget("simplexHeightEnd")]
                public NumericParser<double> simplexHeightEnd
                {
                    get { return mod.simplexHeightEnd; }
                    set { mod.simplexHeightEnd = value; }
                }

                // simplexHeightStart
                [ParserTarget("simplexHeightStart")]
                public NumericParser<double> simplexHeightStart
                {
                    get { return mod.simplexHeightStart; }
                    set { mod.simplexHeightStart = value; }
                }

                // simplexOctaves
                [ParserTarget("simplexOctaves")]
                public NumericParser<double> simplexOctaves
                {
                    get { return mod.simplexOctaves; }
                    set { mod.simplexOctaves = value; }
                }

                // simplexPersistence
                [ParserTarget("simplexPersistence")]
                public NumericParser<double> simplexPersistence
                {
                    get { return mod.simplexPersistence; }
                    set { mod.simplexPersistence = value; }
                }

                // simplexSeed
                [ParserTarget("simplexSeed")]
                public NumericParser<int> simplexSeed
                {
                    get { return mod.simplexSeed; }
                    set { mod.simplexSeed = value; }
                }
            }
        }
    }
}

