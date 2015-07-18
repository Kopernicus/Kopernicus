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
            public class VertexRidgedAltitudeCurve : ModLoader, IParserEventSubscriber
            {
                // Actual PQS mod we are loading
                private PQSMod_VertexRidgedAltitudeCurve _mod;

                // deformity
                [ParserTarget("deformity", optional = true)]
                private NumericParser<float> deformity
                {
                    set { _mod.deformity = value.value; }
                }

                // ridgedAddFrequency
                [ParserTarget("ridgedAddFrequency", optional = true)]
                private NumericParser<float> ridgedAddFrequency
                {
                    set { _mod.ridgedAddFrequency = value.value; }
                }

                // ridgedAddLacunarity
                [ParserTarget("ridgedAddLacunarity", optional = true)]
                private NumericParser<float> ridgedAddLacunarity
                {
                    set { _mod.ridgedAddLacunarity = value.value; }
                }

                // ridgedAddOctaves
                [ParserTarget("ridgedAddOctaves", optional = true)]
                private NumericParser<int> ridgedAddOctaves
                {
                    set { _mod.ridgedAddOctaves = value.value; }
                }

                // ridgedAddSeed
                [ParserTarget("ridgedAddSeed", optional = true)]
                private NumericParser<int> ridgedAddSeed
                {
                    set { _mod.ridgedAddSeed = value.value; }
                }

                // ridgedMinimum
                [ParserTarget("ridgedMinimum", optional = true)]
                private NumericParser<float> ridgedMinimum
                {
                    set { _mod.ridgedMinimum = value.value; }
                }
                
                // ridgedMode
                [ParserTarget("ridgedMode", optional = true)]
                private EnumParser<LibNoise.Unity.QualityMode> ridgedMode
                {
                    set { _mod.ridgedMode = value.value; }
                }

                // simplexCurve
                [ParserTarget("simplexCurve", optional = true)]
                private AnimationCurveParser simplexCurve
                {
                    set { _mod.simplexCurve = value.curve; }
                }

                // simplexFrequency
                [ParserTarget("simplexFrequency", optional = true)]
                private NumericParser<double> simplexFrequency
                {
                    set { _mod.simplexFrequency = value.value; }
                }

                // simplexHeightEnd
                [ParserTarget("simplexHeightEnd", optional = true)]
                private NumericParser<double> simplexHeightEnd
                {
                    set { _mod.simplexHeightEnd = value.value; }
                }

                // simplexHeightStart
                [ParserTarget("simplexHeightStart", optional = true)]
                private NumericParser<double> simplexHeightStart
                {
                    set { _mod.simplexHeightStart = value.value; }
                }

                // simplexOctaves
                [ParserTarget("simplexOctaves", optional = true)]
                private NumericParser<double> simplexOctaves
                {
                    set { _mod.simplexOctaves = value.value; }
                }

                // simplexPersistence
                [ParserTarget("simplexPersistence", optional = true)]
                private NumericParser<double> simplexPersistence
                {
                    set { _mod.simplexPersistence = value.value; }
                }

                // simplexSeed
                [ParserTarget("simplexSeed", optional = true)]
                private NumericParser<int> simplexSeed
                {
                    set { _mod.simplexSeed = value.value; }
                }

                void IParserEventSubscriber.Apply(ConfigNode node)
                {

                }

                void IParserEventSubscriber.PostApply(ConfigNode node)
                {

                }

                public VertexRidgedAltitudeCurve()
                {
                    // Create the base mod
                    GameObject modObject = new GameObject("VertexRidgedAltitudeCurve");
                    modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSMod_VertexRidgedAltitudeCurve>();
                    base.mod = _mod;
                }

                public VertexRidgedAltitudeCurve(PQSMod template)
                {
                    _mod = template as PQSMod_VertexRidgedAltitudeCurve;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
            }
        }
    }
}

