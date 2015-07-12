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
            public class VoronoiCraters : ModLoader, IParserEventSubscriber
            {
                // Actual PQS mod we are loading
                private PQSMod_VoronoiCraters _mod;

                // colorOpacity
                [ParserTarget("colorOpacity", optional = true)]
                private NumericParser<float> colorOpacity
                {
                    set { _mod.colorOpacity = value.value; }
                }

                // DebugColorMapping
                [ParserTarget("DebugColorMapping", optional = true)]
                private NumericParser<bool> DebugColorMapping
                {
                    set { _mod.DebugColorMapping = value.value; }
                }

                // Deformation of the Voronoi
                [ParserTarget("deformation", optional = true)]
                private NumericParser<double> deformation
                {
                    set { _mod.deformation = value.value; }
                }

                // CraterCurve
                [ParserTarget("CraterCurve", optional = true)]
                private FloatCurveParser craterCurve
                {
                    set { _mod.craterCurve = value.curve.Curve; }
                }

                // jitter
                [ParserTarget("jitter", optional = true)]
                private NumericParser<float> jitter
                {
                    set { _mod.jitter = value.value; }
                }

                // JitterCurve
                [ParserTarget("JitterCurve", optional = true)]
                private FloatCurveParser jitterCurve
                {
                    set { _mod.jitterCurve = value.curve.Curve; }
                }

                // jitterHeight
                [ParserTarget("jitterHeight", optional = true)]
                private NumericParser<float> jitterHeight
                {
                    set { _mod.jitterHeight = value.value; }
                }

                // rFactor
                [ParserTarget("rFactor", optional = true)]
                private NumericParser<float> rFactor
                {
                    set { _mod.rFactor = value.value; }
                }

                // rOffset
                [ParserTarget("rOffset", optional = true)]
                private NumericParser<float> rOffset
                {
                    set { _mod.rOffset = value.value; }
                }

                // simplexFrequency
                [ParserTarget("simplexFrequency", optional = true)]
                private NumericParser<double> simplexFrequency
                {
                    set { _mod.simplexFrequency = value.value; }
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

                // voronoiDisplacement
                [ParserTarget("voronoiDisplacement", optional = true)]
                private NumericParser<double> voronoiDisplacement
                {
                    set { _mod.voronoiDisplacement = value.value; }
                }

                // voronoiFrequency
                [ParserTarget("voronoiFrequency", optional = true)]
                private NumericParser<double> voronoiFrequency
                {
                    set { _mod.voronoiFrequency = value.value; }
                }

                // voronoiSeed
                [ParserTarget("voronoiSeed", optional = true)]
                private NumericParser<int> voronoiSeed
                {
                    set { _mod.voronoiSeed = value.value; }
                }

                void IParserEventSubscriber.Apply(ConfigNode node)
                {
                    
                        
                }

                void IParserEventSubscriber.PostApply(ConfigNode node)
                {

                }

                public VoronoiCraters()
                {
                    // Create the base mod
                    GameObject modObject = new GameObject("VoronoiCraters");
                    modObject.transform.parent = Utility.Deactivator;
                    PQSMod_VoronoiCraters clone = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, "Mun").pqsVersion.GetComponentsInChildren<PQSMod_VoronoiCraters>(true)[0] as PQSMod_VoronoiCraters;
                    _mod = modObject.AddComponent<PQSMod_VoronoiCraters>();
                    Utility.CopyObjectFields<PQSMod_VoronoiCraters>(clone, _mod, false);
                    base.mod = _mod;
                }

                public VoronoiCraters(PQSMod template)
                {
                    _mod = template as PQSMod_VoronoiCraters;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
            }
        }
    }
}

