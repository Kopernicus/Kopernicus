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
            public class VertexSimplexMultiChromatic : ModLoader, IParserEventSubscriber
            {
                // Actual PQS mod we are loading
                private PQSMod_VertexSimplexMultiChromatic _mod;

                // The frequency of the alpha noise
                [ParserTarget("alphaFrequency")]
                private NumericParser<double> alphaFrequency
                {
                    set { _mod.alphaFrequency = value.value; }
                }

                // Octaves of the alpha noise
                [ParserTarget("alphaOctaves")]
                private NumericParser<double> alphaOctaves
                {
                    set { _mod.alphaOctaves = value.value; }
                }

                // Persistence of the alpha noise
                [ParserTarget("alphaPersistence")]
                private NumericParser<double> alphaPersistence
                {
                    set { _mod.alphaPersistence = value.value; }
                }

                // The seed of the alpha noise
                [ParserTarget("alphaSeed")]
                private NumericParser<int> alphaSeed
                {
                    set { _mod.alphaSeed = value.value; }
                }

                // Amount of color that will be applied
                [ParserTarget("blend")]
                private NumericParser<float> blend
                {
                    set { _mod.blend = value.value; }
                }

                // The frequency of the blue noise
                [ParserTarget("blueFrequency")]
                private NumericParser<double> blueFrequency
                {
                    set { _mod.blueFrequency = value.value; }
                }

                // Octaves of the blue noise
                [ParserTarget("blueOctaves")]
                private NumericParser<double> blueOctaves
                {
                    set { _mod.blueOctaves = value.value; }
                }

                // Persistence of the blue noise
                [ParserTarget("bluePersistence")]
                private NumericParser<double> bluePersistence
                {
                    set { _mod.bluePersistence = value.value; }
                }

                // The seed of the blue noise
                [ParserTarget("blueSeed")]
                private NumericParser<int> blueSeed
                {
                    set { _mod.blueSeed = value.value; }
                }

                // The frequency of the green noise
                [ParserTarget("greenFrequency")]
                private NumericParser<double> greenFrequency
                {
                    set { _mod.greenFrequency = value.value; }
                }

                // Octaves of the green noise
                [ParserTarget("greenOctaves")]
                private NumericParser<double> greenOctaves
                {
                    set { _mod.greenOctaves = value.value; }
                }

                // Persistence of the green noise
                [ParserTarget("greenPersistence")]
                private NumericParser<double> greenPersistence
                {
                    set { _mod.greenPersistence = value.value; }
                }

                // The seed of the green noise
                [ParserTarget("greenSeed")]
                private NumericParser<int> greenSeed
                {
                    set { _mod.greenSeed = value.value; }
                }

                // The frequency of the red noise
                [ParserTarget("redFrequency")]
                private NumericParser<double> redFrequency
                {
                    set { _mod.redFrequency = value.value; }
                }

                // Octaves of the red noise
                [ParserTarget("redOctaves")]
                private NumericParser<double> redOctaves
                {
                    set { _mod.redOctaves = value.value; }
                }

                // Persistence of the red noise
                [ParserTarget("redPersistence")]
                private NumericParser<double> redPersistence
                {
                    set { _mod.redPersistence = value.value; }
                }

                // The seed of the red noise
                [ParserTarget("redSeed")]
                private NumericParser<int> redSeed
                {
                    set { _mod.redSeed = value.value; }
                }

                void IParserEventSubscriber.Apply(ConfigNode node)
                {

                }

                void IParserEventSubscriber.PostApply(ConfigNode node)
                {

                }

                public VertexSimplexMultiChromatic()
                {
                    // Create the base mod
                    GameObject modObject = new GameObject("VertexSimplexMultiChromatic");
                    modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSMod_VertexSimplexMultiChromatic>();
                    base.mod = _mod;
                }

                public VertexSimplexMultiChromatic(PQSMod template)
                {
                    _mod = template as PQSMod_VertexSimplexMultiChromatic;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
            }
        }
    }
}

