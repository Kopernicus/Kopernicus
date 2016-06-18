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
            public class VoronoiCraters : ModLoader<PQSMod_VoronoiCraters>
            {
                // colorOpacity
                [ParserTarget("colorOpacity")]
                public NumericParser<float> colorOpacity
                {
                    get { return mod.colorOpacity; }
                    set { mod.colorOpacity = value; }
                }
                
                // DebugColorMapping
                [ParserTarget("DebugColorMapping")]
                public NumericParser<bool> DebugColorMapping
                {
                    get { return mod.DebugColorMapping; }
                    set { mod.DebugColorMapping = value; }
                }

                // Deformation of the Voronoi
                [ParserTarget("deformation")]
                public NumericParser<double> deformation
                {
                    get { return mod.deformation; }
                    set { mod.deformation = value; }
                }

                // CraterCurve
                [ParserTarget("CraterCurve")]
                public FloatCurveParser craterCurve
                {
                    get { return mod.craterCurve != null ? new FloatCurve(mod.craterCurve.keys) : new FloatCurve(); }
                    set { mod.craterCurve = value.curve.Curve; }
                }

                // jitter
                [ParserTarget("jitter")]
                public NumericParser<float> jitter
                {
                    get { return mod.jitter; }
                    set { mod.jitter = value; }
                }

                // JitterCurve
                [ParserTarget("JitterCurve")]
                public FloatCurveParser jitterCurve
                {
                    get { return mod.jitterCurve != null ? new FloatCurve(mod.jitterCurve.keys) : new FloatCurve(); }
                    set { mod.jitterCurve = value.curve.Curve; }
                }

                // jitterHeight
                [ParserTarget("jitterHeight")]
                public NumericParser<float> jitterHeight
                {
                    get { return mod.jitterHeight; }
                    set { mod.jitterHeight = value; }
                }

                // rFactor
                [ParserTarget("rFactor")]
                public NumericParser<float> rFactor
                {
                    get { return mod.rFactor; }
                    set { mod.rFactor = value; }
                }

                // rOffset
                [ParserTarget("rOffset")]
                public NumericParser<float> rOffset
                {
                    get { return mod.rOffset; }
                    set { mod.rOffset = value; }
                }

                // simplexFrequency
                [ParserTarget("simplexFrequency")]
                public NumericParser<double> simplexFrequency
                {
                    get { return mod.simplexFrequency; }
                    set { mod.simplexFrequency = value; }
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

                // voronoiDisplacement
                [ParserTarget("voronoiDisplacement")]
                public NumericParser<double> voronoiDisplacement
                {
                    get { return mod.voronoiDisplacement; }
                    set { mod.voronoiDisplacement = value; }
                }

                // voronoiFrequency
                [ParserTarget("voronoiFrequency")]
                public NumericParser<double> voronoiFrequency
                {
                    get { return mod.voronoiFrequency; }
                    set { mod.voronoiFrequency = value; }
                }

                // voronoiSeed
                [ParserTarget("voronoiSeed")]
                public NumericParser<int> voronoiSeed
                {
                    get { return mod.voronoiSeed; }
                    set { mod.voronoiSeed = value; }
                }

                // Create the mod
                public override void Create()
                {
                    base.Create();

                    // Create the base mod
                    PQSMod_VoronoiCraters clone = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, "Mun").pqsVersion.GetComponentsInChildren<PQSMod_VoronoiCraters>(true)[0] as PQSMod_VoronoiCraters;
                    Utility.CopyObjectFields(clone, base.mod, false);
                }
                
            }
        }
    }
}

