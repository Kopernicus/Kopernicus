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
            public class VertexHeightNoiseVertHeightCurve3 : ModLoader<PQSMod_VertexHeightNoiseVertHeightCurve3>
            {
                // Maximum deformity
                [ParserTarget("deformityMax")]
                public NumericParser<double> deformityMax
                {
                    get { return mod.deformityMax; }
                    set { mod.deformityMax = value; }
                }

                // Minimum deformity
                [ParserTarget("deformityMin")]
                public NumericParser<double> deformityMin
                {
                    get { return mod.deformityMin; }
                    set { mod.deformityMin = value; }
                }

                // Deformity multiplier curve
                [ParserTarget("inputHeightCurve")]
                public FloatCurveParser inputHeightCurve
                {
                    get { return mod.inputHeightCurve != null ? new FloatCurve(mod.inputHeightCurve.keys) : new FloatCurve(); }
                    set { mod.inputHeightCurve = value.curve.Curve; }
                }

                // Ending height
                [ParserTarget("inputHeightEnd")]
                public NumericParser<double> inputHeightEnd
                {
                    get { return mod.inputHeightEnd; }
                    set { mod.inputHeightEnd = value; }
                }

                // Starting height
                [ParserTarget("inputHeightStart")]
                public NumericParser<double> inputHeightStart
                {
                    get { return mod.inputHeightStart; }
                    set { mod.inputHeightStart = value; }
                }

                // The frequency of the simplex multiplier
                [ParserTarget("multiplierFrequency", optional = true)]
                public NumericParser<double> multiplierFrequency
                {
                    get { return mod.curveMultiplier.frequency; }
                    set { mod.curveMultiplier.frequency = value; }
                }

                // Octaves of the simplex multiplier
                [ParserTarget("multiplierOctaves", optional = true)]
                public NumericParser<int> multiplierOctaves
                {
                    get { return mod.curveMultiplier.octaves; }
                    set { mod.curveMultiplier.octaves = value; }
                }

                // Persistence of the simplex multiplier
                [ParserTarget("multiplierPersistence", optional = true)]
                public NumericParser<double> multiplierPersistence
                {
                    get { return mod.curveMultiplier.persistence; }
                    set { mod.curveMultiplier.persistence = value; }
                }

                // The seed of the simplex multiplier
                [ParserTarget("multiplierSeed", optional = true)]
                public NumericParser<int> multiplierSeed
                {
                    get { return mod.curveMultiplier.seed; }
                    set { mod.curveMultiplier.seed = value; }
                }

                // The frequency of the simplex noise on deformity
                [ParserTarget("deformityFrequency", optional = true)]
                public NumericParser<double> deformityFrequency
                {
                    get { return mod.deformity.frequency; }
                    set { mod.deformity.frequency = value; }
                }

                // Octaves of the simplex noise on deformity
                [ParserTarget("deformityOctaves", optional = true)]
                public NumericParser<int> deformityOctaves
                {
                    get { return mod.deformity.octaves; }
                    set { mod.deformity.octaves = value; }
                }

                // Persistence of the simplex noise on deformity
                [ParserTarget("deformityPersistence", optional = true)]
                public NumericParser<double> deformityPersistence
                {
                    get { return mod.deformity.persistence; }
                    set { mod.deformity.persistence = value; }
                }

                // The seed of the simplex noise on deformity
                [ParserTarget("deformitySeed", optional = true)]
                public NumericParser<int> deformitySeed
                {
                    get { return mod.deformity.seed; }
                    set { mod.deformity.seed = value; }
                }

                // The frequency of the additive noise
                [ParserTarget("ridgedAddFrequency", optional = true)]
                public NumericParser<double> ridgedAddFrequency
                {
                    get { return mod.ridgedAdd.frequency; }
                    set { mod.ridgedAdd.frequency = value; }
                }

                // Lacunarity of the additive noise
                [ParserTarget("ridgedAddLacunarity", optional = true)]
                public NumericParser<double> ridgedAddLacunarity
                {
                    get { return mod.ridgedAdd.lacunarity; }
                    set { mod.ridgedAdd.lacunarity = value; }
                }

                // Octaves of the additive noise
                [ParserTarget("ridgedAddOctaves", optional = true)]
                public NumericParser<int> ridgedAddOctaves
                {
                    get { return mod.ridgedAdd.octaves; }
                    set { mod.ridgedAdd.octaves = value; }
                }

                // The quality of the additive noise
                [ParserTarget("ridgedAddQuality", optional = true)]
                public EnumParser<LibNoise.Unity.QualityMode> ridgedAddQuality
                {
                    get { return mod.ridgedAdd.quality; }
                    set { mod.ridgedAdd.quality = value; }
                }

                // The seed of the additive noise
                [ParserTarget("ridgedAddSeed", optional = true)]
                public NumericParser<int> ridgedAddSeed
                {
                    get { return mod.ridgedAdd.seed; }
                    set { mod.ridgedAdd.seed = value; }
                }

                // The frequency of the subtractive noise
                [ParserTarget("ridgedSubFrequency", optional = true)]
                public NumericParser<double> ridgedSubFrequency
                {
                    get { return mod.ridgedSub.frequency; }
                    set { mod.ridgedSub.frequency = value; }
                }

                // Lacunarity of the subtractive noise
                [ParserTarget("ridgedSubLacunarity", optional = true)]
                public NumericParser<double> ridgedSubLacunarity
                {
                    get { return mod.ridgedSub.lacunarity; }
                    set { mod.ridgedSub.lacunarity = value; }
                }

                // Octaves of the subtractive noise
                [ParserTarget("ridgedSubOctaves", optional = true)]
                public NumericParser<int> ridgedSubOctaves
                {
                    get { return mod.ridgedSub.octaves; }
                    set { mod.ridgedSub.octaves = value; }
                }

                // The quality of the subtractive noise
                [ParserTarget("ridgedSubQuality", optional = true)]
                public EnumParser<LibNoise.Unity.QualityMode> ridgedSubQuality
                {
                    get { return mod.ridgedSub.quality; }
                    set { mod.ridgedSub.quality = value; }
                }

                // The seed of the subtractive noise
                [ParserTarget("ridgedSubSeed", optional = true)]
                public NumericParser<int> ridgedSubSeed
                {
                    get { return mod.ridgedSub.seed; }
                    set { mod.ridgedSub.seed = value; }
                }

                // Create the mod
                public override void Create()
                {
                    base.Create();

                    // Construct the internal objects.
                    mod.curveMultiplier = new PQSMod_VertexHeightNoiseVertHeightCurve3.SimplexNoise();
                    mod.deformity = new PQSMod_VertexHeightNoiseVertHeightCurve3.SimplexNoise();
                    mod.ridgedAdd = new PQSMod_VertexHeightNoiseVertHeightCurve3.RidgedNoise();
                    mod.ridgedSub = new PQSMod_VertexHeightNoiseVertHeightCurve3.RidgedNoise();
                }
            }
        }
    }
}


