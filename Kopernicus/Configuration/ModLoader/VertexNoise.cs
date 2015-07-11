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
            public class VertexNoise : ModLoader, IParserEventSubscriber
            {
                // Actual PQS mod we are loading
                private PQSMod_VertexNoise _mod;

                // falloff
                [ParserTarget("falloff", optional = true)]
                private NumericParser<float> falloff
                {
                    set { _mod.falloff = value.value; }
                }

                // mesaVsPlainsBias
                [ParserTarget("mesaVsPlainsBias", optional = true)]
                private NumericParser<float> mesaVsPlainsBias
                {
                    set { _mod.mesaVsPlainsBias = value.value; }
                }

                // noiseDeformity
                [ParserTarget("noiseDeformity", optional = true)]
                private NumericParser<float> noiseDeformity
                {
                    set { _mod.noiseDeformity = value.value; }
                }

                // noisePasses
                [ParserTarget("noisePasses", optional = true)]
                private NumericParser<int> noisePasses
                {
                    set { _mod.noisePasses = value.value; }
                }

                // plainSmoothness
                [ParserTarget("plainSmoothness", optional = true)]
                private NumericParser<float> plainSmoothness
                {
                    set { _mod.plainSmoothness = value.value; }
                }

                // plainsVsMountainSmoothness
                [ParserTarget("plainsVsMountainSmoothness", optional = true)]
                private NumericParser<float> plainsVsMountainSmoothness
                {
                    set { _mod.plainsVsMountainSmoothness = value.value; }
                }

                // plainsVsMountainThreshold
                [ParserTarget("plainsVsMountainThreshold", optional = true)]
                private NumericParser<float> plainsVsMountainThreshold
                {
                    set { _mod.plainsVsMountainThreshold = value.value; }
                }

                // seed
                [ParserTarget("seed", optional = true)]
                private NumericParser<int> seed
                {
                    set { _mod.seed = value.value; }
                }

                // smoothness
                [ParserTarget("smoothness", optional = true)]
                private NumericParser<float> smoothness
                {
                    set { _mod.smoothness = value.value; }
                }

                void IParserEventSubscriber.Apply(ConfigNode node)
                {

                }

                void IParserEventSubscriber.PostApply(ConfigNode node)
                {

                }

                public VertexNoise()
                {
                    // Create the base mod
                    GameObject modObject = new GameObject("VertexNoise");
                    modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSMod_VertexNoise>();
                    base.mod = _mod;
                }

                public VertexNoise(PQSMod template)
                {
                    _mod = template as PQSMod_VertexNoise;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
            }
        }
    }
}

