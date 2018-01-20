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

using System;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class VoronoiCraters : ModLoader<PQSMod_VoronoiCraters>
            {
                // colorOpacity
                [ParserTarget("colorOpacity")]
                public NumericParser<Single> colorOpacity
                {
                    get { return mod.colorOpacity; }
                    set { mod.colorOpacity = value; }
                }
                
                // DebugColorMapping
                [ParserTarget("DebugColorMapping")]
                public NumericParser<Boolean> DebugColorMapping
                {
                    get { return mod.DebugColorMapping; }
                    set { mod.DebugColorMapping = value; }
                }

                // Deformation of the Voronoi
                [ParserTarget("deformation")]
                public NumericParser<Double> deformation
                {
                    get { return mod.deformation; }
                    set { mod.deformation = value; }
                }

                // CraterCurve
                [ParserTarget("CraterCurve")]
                public FloatCurveParser craterCurve
                {
                    get { return mod.craterCurve; }
                    set { mod.craterCurve = value; }
                }

                // jitter
                [ParserTarget("jitter")]
                public NumericParser<Single> jitter
                {
                    get { return mod.jitter; }
                    set { mod.jitter = value; }
                }

                // JitterCurve
                [ParserTarget("JitterCurve")]
                public FloatCurveParser jitterCurve
                {
                    get { return mod.jitterCurve; }
                    set { mod.jitterCurve = value; }
                }

                // jitterHeight
                [ParserTarget("jitterHeight")]
                public NumericParser<Single> jitterHeight
                {
                    get { return mod.jitterHeight; }
                    set { mod.jitterHeight = value; }
                }

                // rFactor
                [ParserTarget("rFactor")]
                public NumericParser<Single> rFactor
                {
                    get { return mod.rFactor; }
                    set { mod.rFactor = value; }
                }

                // rOffset
                [ParserTarget("rOffset")]
                public NumericParser<Single> rOffset
                {
                    get { return mod.rOffset; }
                    set { mod.rOffset = value; }
                }

                // simplexFrequency
                [ParserTarget("simplexFrequency")]
                public NumericParser<Double> simplexFrequency
                {
                    get { return mod.simplexFrequency; }
                    set { mod.simplexFrequency = value; }
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

                // voronoiDisplacement
                [ParserTarget("voronoiDisplacement")]
                public NumericParser<Double> voronoiDisplacement
                {
                    get { return mod.voronoiDisplacement; }
                    set { mod.voronoiDisplacement = value; }
                }

                // voronoiFrequency
                [ParserTarget("voronoiFrequency")]
                public NumericParser<Double> voronoiFrequency
                {
                    get { return mod.voronoiFrequency; }
                    set { mod.voronoiFrequency = value; }
                }

                // voronoiSeed
                [ParserTarget("voronoiSeed")]
                public NumericParser<Int32> voronoiSeed
                {
                    get { return mod.voronoiSeed; }
                    set { mod.voronoiSeed = value; }
                }

                // Create the mod
                public override void Create(PQS pqsVersion)
                {
                    base.Create(pqsVersion);

                    // Create the base mod
                    PQSMod_VoronoiCraters clone =
                        Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Mun").pqsVersion
                            .GetComponentsInChildren<PQSMod_VoronoiCraters>(true)[0] as PQSMod_VoronoiCraters;
                    Utility.CopyObjectFields(clone, base.mod, false);
                }

                // Create the mod
                public override void Create(PQSMod_VoronoiCraters _mod, PQS pqsVersion)
                {
                    base.Create(_mod, pqsVersion);
                    
                    // Create the base mod if needed
                    if (mod.craterColourRamp == null)
                    {
                        PQSMod_VoronoiCraters clone =
                            Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Mun").pqsVersion
                                .GetComponentsInChildren<PQSMod_VoronoiCraters>(true)[0] as PQSMod_VoronoiCraters;
                        Utility.CopyObjectFields(clone, base.mod, false);
                    }
                }
            }
        }
    }
}

