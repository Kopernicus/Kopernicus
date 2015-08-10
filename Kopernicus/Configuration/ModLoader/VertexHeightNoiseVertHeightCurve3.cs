/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
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
            public class VertexHeightNoiseVertHeightCurve3 : ModLoader, IParserEventSubscriber
            {
                // Actual PQS mod we are loading
                private PQSMod_VertexHeightNoiseVertHeightCurve3 _mod;

                // Maximum deformity
                [ParserTarget("deformityMax")]
                private NumericParser<double> deformityMax
                {
                    set { _mod.deformityMax = value.value; }
                }

                // Minimum deformity
                [ParserTarget("deformityMin")]
                private NumericParser<double> deformityMin
                {
                    set { _mod.deformityMin = value.value; }
                }

                // Deformity multiplier curve
                [ParserTarget("inputHeightCurve")]
                private FloatCurveParser inputHeightCurve
                {
                    set { _mod.inputHeightCurve = value.curve.Curve; }
                }

                // Ending height
                [ParserTarget("inputHeightEnd")]
                private NumericParser<double> inputHeightEnd
                {
                    set { _mod.inputHeightEnd = value.value; }
                }

                // Starting height
                [ParserTarget("inputHeightStart")]
                private NumericParser<double> inputHeightStart
                {
                    set { _mod.inputHeightStart = value.value; }
                }

                // The frequency of the simplex multiplier
                [ParserTarget("multiplierFrequency", optional = true)]
                private NumericParser<double> multiplierFrequency
                {
                    set { _mod.curveMultiplier.frequency = value.value; }
                }

                // Octaves of the simplex multiplier
                [ParserTarget("multiplierOctaves", optional = true)]
                private NumericParser<int> multiplierOctaves
                {
                    set { _mod.curveMultiplier.octaves = value.value; }
                }

                // Persistence of the simplex multiplier
                [ParserTarget("multiplierPersistence", optional = true)]
                private NumericParser<double> multiplierPersistence
                {
                    set { _mod.curveMultiplier.persistence = value.value; }
                }

                // The seed of the simplex multiplier
                [ParserTarget("multiplierSeed", optional = true)]
                private NumericParser<int> multiplierSeed
                {
                    set { _mod.curveMultiplier.seed = value.value; }
                }

                // The frequency of the simplex noise on deformity
                [ParserTarget("deformityFrequency", optional = true)]
                private NumericParser<double> deformityFrequency
                {
                    set { _mod.deformity.frequency = value.value; }
                }

                // Octaves of the simplex noise on deformity
                [ParserTarget("deformityOctaves", optional = true)]
                private NumericParser<int> deformityOctaves
                {
                    set { _mod.deformity.octaves = value.value; }
                }

                // Persistence of the simplex noise on deformity
                [ParserTarget("deformityPersistence", optional = true)]
                private NumericParser<double> deformityPersistence
                {
                    set { _mod.deformity.persistence = value.value; }
                }

                // The seed of the simplex noise on deformity
                [ParserTarget("deformitySeed", optional = true)]
                private NumericParser<int> deformitySeed
                {
                    set { _mod.deformity.seed = value.value; }
                }

                // The frequency of the additive noise
                [ParserTarget("ridgedAddFrequency", optional = true)]
                private NumericParser<double> ridgedAddFrequency
                {
                    set { _mod.ridgedAdd.frequency = value.value; }
                }

                // Lacunarity of the additive noise
                [ParserTarget("ridgedAddLacunarity", optional = true)]
                private NumericParser<double> ridgedAddLacunarity
                {
                    set { _mod.ridgedAdd.lacunarity = value.value; }
                }

                // Octaves of the additive noise
                [ParserTarget("ridgedAddOctaves", optional = true)]
                private NumericParser<int> ridgedAddOctaves
                {
                    set { _mod.ridgedAdd.octaves = value.value; }
                }

                // The quality of the additive noise
                [ParserTarget("ridgedAddQuality", optional = true)]
                private EnumParser<LibNoise.Unity.QualityMode> ridgedAddQuality
                {
                    set { _mod.ridgedAdd.quality = value.value; }
                }

                // The seed of the additive noise
                [ParserTarget("ridgedAddSeed", optional = true)]
                private NumericParser<int> ridgedAddSeed
                {
                    set { _mod.ridgedAdd.seed = value.value; }
                }

                // The frequency of the subtractive noise
                [ParserTarget("ridgedSubFrequency", optional = true)]
                private NumericParser<double> ridgedSubFrequency
                {
                    set { _mod.ridgedSub.frequency = value.value; }
                }

                // Lacunarity of the subtractive noise
                [ParserTarget("ridgedSubLacunarity", optional = true)]
                private NumericParser<double> ridgedSubLacunarity
                {
                    set { _mod.ridgedSub.lacunarity = value.value; }
                }

                // Octaves of the subtractive noise
                [ParserTarget("ridgedSubOctaves", optional = true)]
                private NumericParser<int> ridgedSubOctaves
                {
                    set { _mod.ridgedSub.octaves = value.value; }
                }

                // The quality of the subtractive noise
                [ParserTarget("ridgedSubQuality", optional = true)]
                private EnumParser<LibNoise.Unity.QualityMode> ridgedSubQuality
                {
                    set { _mod.ridgedSub.quality = value.value; }
                }

                // The seed of the subtractive noise
                [ParserTarget("ridgedSubSeed", optional = true)]
                private NumericParser<int> ridgedSubSeed
                {
                    set { _mod.ridgedSub.seed = value.value; }
                }

                void IParserEventSubscriber.Apply(ConfigNode node)
                {

                }

                void IParserEventSubscriber.PostApply(ConfigNode node)
                {

                }

                public VertexHeightNoiseVertHeightCurve3()
                {
                    // Create the base mod
                    GameObject modObject = new GameObject("VertexHeightNoiseVertHeightCurve3");
                    modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent <PQSMod_VertexHeightNoiseVertHeightCurve3>();

                    // Construct the internal objects.
                    _mod.curveMultiplier = new PQSMod_VertexHeightNoiseVertHeightCurve3.SimplexNoise();
                    _mod.deformity = new PQSMod_VertexHeightNoiseVertHeightCurve3.SimplexNoise();
                    _mod.ridgedAdd = new PQSMod_VertexHeightNoiseVertHeightCurve3.RidgedNoise();
                    _mod.ridgedSub = new PQSMod_VertexHeightNoiseVertHeightCurve3.RidgedNoise();
   
                    base.mod = _mod;
                }

                public VertexHeightNoiseVertHeightCurve3(PQSMod template)
                {
                    _mod = template as PQSMod_VertexHeightNoiseVertHeightCurve3;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
            }
        }
    }
}


