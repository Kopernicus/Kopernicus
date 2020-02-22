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
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class VoronoiCraters : ModLoader<PQSMod_VoronoiCraters>
    {
        // colorOpacity
        [ParserTarget("colorOpacity")]
        public NumericParser<Single> ColorOpacity
        {
            get { return Mod.colorOpacity; }
            set { Mod.colorOpacity = value; }
        }

        // DebugColorMapping
        [ParserTarget("DebugColorMapping")]
        public NumericParser<Boolean> DebugColorMapping
        {
            get { return Mod.DebugColorMapping; }
            set { Mod.DebugColorMapping = value; }
        }

        // Deformation of the Voronoi
        [ParserTarget("deformation")]
        public NumericParser<Double> Deformation
        {
            get { return Mod.deformation; }
            set { Mod.deformation = value; }
        }

        // CraterCurve
        [ParserTargetCollection("CraterCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> CraterCurve
        {
            get { return Utility.AnimCurveToList(Mod.craterCurve); }
            set { Mod.craterCurve = Utility.ListToAnimCurve(value); }
        }

        // jitter
        [ParserTarget("jitter")]
        public NumericParser<Single> Jitter
        {
            get { return Mod.jitter; }
            set { Mod.jitter = value; }
        }

        // JitterCurve
        [ParserTargetCollection("JitterCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> JitterCurve
        {
            get { return Utility.AnimCurveToList(Mod.jitterCurve); }
            set { Mod.jitterCurve = Utility.ListToAnimCurve(value); }
        }

        // jitterHeight
        [ParserTarget("jitterHeight")]
        public NumericParser<Single> JitterHeight
        {
            get { return Mod.jitterHeight; }
            set { Mod.jitterHeight = value; }
        }

        // rFactor
        [ParserTarget("rFactor")]
        public NumericParser<Single> RFactor
        {
            get { return Mod.rFactor; }
            set { Mod.rFactor = value; }
        }

        // rOffset
        [ParserTarget("rOffset")]
        public NumericParser<Single> ROffset
        {
            get { return Mod.rOffset; }
            set { Mod.rOffset = value; }
        }

        // simplexFrequency
        [ParserTarget("simplexFrequency")]
        public NumericParser<Double> SimplexFrequency
        {
            get { return Mod.simplexFrequency; }
            set { Mod.simplexFrequency = value; }
        }

        // simplexOctaves
        [ParserTarget("simplexOctaves")]
        public NumericParser<Double> SimplexOctaves
        {
            get { return Mod.simplexOctaves; }
            set { Mod.simplexOctaves = value; }
        }

        // simplexPersistence
        [ParserTarget("simplexPersistence")]
        public NumericParser<Double> SimplexPersistence
        {
            get { return Mod.simplexPersistence; }
            set { Mod.simplexPersistence = value; }
        }

        // simplexSeed
        [ParserTarget("simplexSeed")]
        public NumericParser<Int32> SimplexSeed
        {
            get { return Mod.simplexSeed; }
            set { Mod.simplexSeed = value; }
        }

        // voronoiDisplacement
        [ParserTarget("voronoiDisplacement")]
        public NumericParser<Double> VoronoiDisplacement
        {
            get { return Mod.voronoiDisplacement; }
            set { Mod.voronoiDisplacement = value; }
        }

        // voronoiFrequency
        [ParserTarget("voronoiFrequency")]
        public NumericParser<Double> VoronoiFrequency
        {
            get { return Mod.voronoiFrequency; }
            set { Mod.voronoiFrequency = value; }
        }

        // voronoiSeed
        [ParserTarget("voronoiSeed")]
        public NumericParser<Int32> VoronoiSeed
        {
            get { return Mod.voronoiSeed; }
            set { Mod.voronoiSeed = value; }
        }

        // Create the mod
        public override void Create(PQS pqsVersion)
        {
            base.Create(pqsVersion);

            // Create the base mod
            PQSMod_VoronoiCraters clone =
                Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Mun").pqsVersion
                    .GetComponentsInChildren<PQSMod_VoronoiCraters>(true)[0];
            Utility.CopyObjectFields(clone, Mod, false);
        }

        // Create the mod
        public override void Create(PQSMod_VoronoiCraters mod, PQS pqsVersion)
        {
            base.Create(mod, pqsVersion);

            // Create the base mod if needed
            if (Mod.craterColourRamp != null)
            {
                return;
            }
            PQSMod_VoronoiCraters clone =
                Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Mun").pqsVersion
                    .GetComponentsInChildren<PQSMod_VoronoiCraters>(true)[0];
            Utility.CopyObjectFields(clone, Mod, false);
        }
    }
}

