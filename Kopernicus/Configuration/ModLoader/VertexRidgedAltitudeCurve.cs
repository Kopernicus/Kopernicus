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
            public class VertexRidgedAltitudeCurve : ModLoader<PQSMod_VertexRidgedAltitudeCurve>
            {
                // deformity
                [ParserTarget("deformity", optional = true)]
                public NumericParser<float> deformity
                {
                    get { return mod.deformity; }
                    set { mod.deformity = value; }
                }

                // ridgedAddFrequency
                [ParserTarget("ridgedAddFrequency", optional = true)]
                public NumericParser<float> ridgedAddFrequency
                {
                    get { return mod.ridgedAddFrequency; }
                    set { mod.ridgedAddFrequency = value; }
                }

                // ridgedAddLacunarity
                [ParserTarget("ridgedAddLacunarity", optional = true)]
                public NumericParser<float> ridgedAddLacunarity
                {
                    get { return mod.ridgedAddLacunarity; }
                    set { mod.ridgedAddLacunarity = value; }
                }

                // ridgedAddOctaves
                [ParserTarget("ridgedAddOctaves", optional = true)]
                public NumericParser<int> ridgedAddOctaves
                {
                    get { return mod.ridgedAddOctaves; }
                    set { mod.ridgedAddOctaves = value; }
                }

                // ridgedAddSeed
                [ParserTarget("ridgedAddSeed", optional = true)]
                public NumericParser<int> ridgedAddSeed
                {
                    get { return mod.ridgedAddSeed; }
                    set { mod.ridgedAddSeed = value; }
                }

                // ridgedMinimum
                [ParserTarget("ridgedMinimum", optional = true)]
                public NumericParser<float> ridgedMinimum
                {
                    get { return mod.ridgedMinimum; }
                    set { mod.ridgedMinimum = value; }
                }
                
                // ridgedMode
                [ParserTarget("ridgedMode", optional = true)]
                public EnumParser<LibNoise.Unity.QualityMode> ridgedMode
                {
                    get { return mod.ridgedMode; }
                    set { mod.ridgedMode = value; }
                }

                // simplexCurve
                [ParserTarget("simplexCurve", optional = true)]
                public FloatCurveParser simplexCurve
                {
                    get { return mod.simplexCurve != null ? new FloatCurve(mod.simplexCurve.keys) : new FloatCurve(); }
                    set { mod.simplexCurve = value.curve.Curve; }
                }

                // simplexFrequency
                [ParserTarget("simplexFrequency", optional = true)]
                public NumericParser<double> simplexFrequency
                {
                    get { return mod.simplexFrequency; }
                    set { mod.simplexFrequency = value; }
                }

                // simplexHeightEnd
                [ParserTarget("simplexHeightEnd", optional = true)]
                public NumericParser<double> simplexHeightEnd
                {
                    get { return mod.simplexHeightEnd; }
                    set { mod.simplexHeightEnd = value; }
                }

                // simplexHeightStart
                [ParserTarget("simplexHeightStart", optional = true)]
                public NumericParser<double> simplexHeightStart
                {
                    get { return mod.simplexHeightStart; }
                    set { mod.simplexHeightStart = value; }
                }

                // simplexOctaves
                [ParserTarget("simplexOctaves", optional = true)]
                public NumericParser<double> simplexOctaves
                {
                    get { return mod.simplexOctaves; }
                    set { mod.simplexOctaves = value; }
                }

                // simplexPersistence
                [ParserTarget("simplexPersistence", optional = true)]
                public NumericParser<double> simplexPersistence
                {
                    get { return mod.simplexPersistence; }
                    set { mod.simplexPersistence = value; }
                }

                // simplexSeed
                [ParserTarget("simplexSeed", optional = true)]
                public NumericParser<int> simplexSeed
                {
                    get { return mod.simplexSeed; }
                    set { mod.simplexSeed = value; }
                }
            }
        }
    }
}

