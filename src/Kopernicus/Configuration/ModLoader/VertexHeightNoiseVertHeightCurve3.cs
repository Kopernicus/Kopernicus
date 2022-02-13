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
using Kopernicus.Configuration.Enumerations;
using LibNoise;
using UnityEngine;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class VertexHeightNoiseVertHeightCurve3 : ModLoader<PQSMod_VertexHeightNoiseVertHeightCurve3>
    {
        // Maximum deformity
        [ParserTarget("deformityMax")]
        public NumericParser<Double> DeformityMax
        {
            get { return Mod.deformityMax; }
            set { Mod.deformityMax = value; }
        }

        // Minimum deformity
        [ParserTarget("deformityMin")]
        public NumericParser<Double> DeformityMin
        {
            get { return Mod.deformityMin; }
            set { Mod.deformityMin = value; }
        }

        // Deformity multiplier curve
        [ParserTargetCollection("inputHeightCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> InputHeightCurve
        {
            get { return Utility.AnimCurveToList(Mod.inputHeightCurve); }
            set { Mod.inputHeightCurve = Utility.ListToAnimCurve(value); }
        }

        // Ending height
        [ParserTarget("inputHeightEnd")]
        public NumericParser<Double> InputHeightEnd
        {
            get { return Mod.inputHeightEnd; }
            set { Mod.inputHeightEnd = value; }
        }

        // Starting height
        [ParserTarget("inputHeightStart")]
        public NumericParser<Double> InputHeightStart
        {
            get { return Mod.inputHeightStart; }
            set { Mod.inputHeightStart = value; }
        }

        // The frequency of the simplex multiplier
        [ParserTarget("multiplierFrequency")]
        public NumericParser<Double> MultiplierFrequency
        {
            get { return Mod.curveMultiplier.frequency; }
            set { Mod.curveMultiplier.frequency = value; }
        }

        // Octaves of the simplex multiplier
        [ParserTarget("multiplierOctaves")]
        public NumericParser<Int32> MultiplierOctaves
        {
            get { return Mod.curveMultiplier.octaves; }
            set { Mod.curveMultiplier.octaves = value; }
        }

        // Persistence of the simplex multiplier
        [ParserTarget("multiplierPersistence")]
        public NumericParser<Double> MultiplierPersistence
        {
            get { return Mod.curveMultiplier.persistence; }
            set { Mod.curveMultiplier.persistence = value; }
        }

        // The seed of the simplex multiplier
        [ParserTarget("multiplierSeed")]
        public NumericParser<Int32> MultiplierSeed
        {
            get { return Mod.curveMultiplier.seed; }
            set { Mod.curveMultiplier.seed = value; }
        }

        // The frequency of the simplex noise on deformity
        [ParserTarget("deformityFrequency")]
        public NumericParser<Double> DeformityFrequency
        {
            get { return Mod.deformity.frequency; }
            set { Mod.deformity.frequency = value; }
        }

        // Octaves of the simplex noise on deformity
        [ParserTarget("deformityOctaves")]
        public NumericParser<Int32> DeformityOctaves
        {
            get { return Mod.deformity.octaves; }
            set { Mod.deformity.octaves = value; }
        }

        // Persistence of the simplex noise on deformity
        [ParserTarget("deformityPersistence")]
        public NumericParser<Double> DeformityPersistence
        {
            get { return Mod.deformity.persistence; }
            set { Mod.deformity.persistence = value; }
        }

        // The seed of the simplex noise on deformity
        [ParserTarget("deformitySeed")]
        public NumericParser<Int32> DeformitySeed
        {
            get { return Mod.deformity.seed; }
            set { Mod.deformity.seed = value; }
        }

        // The frequency of the additive noise
        [ParserTarget("ridgedAddFrequency")]
        public NumericParser<Double> RidgedAddFrequency
        {
            get { return Mod.ridgedAdd.frequency; }
            set { Mod.ridgedAdd.frequency = value; }
        }

        // Lacunarity of the additive noise
        [ParserTarget("ridgedAddLacunarity")]
        public NumericParser<Double> RidgedAddLacunarity
        {
            get { return Mod.ridgedAdd.lacunarity; }
            set { Mod.ridgedAdd.lacunarity = value; }
        }

        // Octaves of the additive noise
        [ParserTarget("ridgedAddOctaves")]
        public NumericParser<Int32> RidgedAddOctaves
        {
            get { return Mod.ridgedAdd.octaves; }
            set { Mod.ridgedAdd.octaves = Mathf.Clamp(value, 1, 30); }
        }

        // The quality of the additive noise
        [ParserTarget("ridgedAddQuality")]
        public EnumParser<KopernicusNoiseQuality> RidgedAddQuality
        {
            get { return (KopernicusNoiseQuality)(Int32)Mod.ridgedAdd.quality; }
            set { Mod.ridgedAdd.quality = (NoiseQuality)(Int32)value.Value; }
        }

        // The seed of the additive noise
        [ParserTarget("ridgedAddSeed")]
        public NumericParser<Int32> RidgedAddSeed
        {
            get { return Mod.ridgedAdd.seed; }
            set { Mod.ridgedAdd.seed = value; }
        }

        // The frequency of the subtractive noise
        [ParserTarget("ridgedSubFrequency")]
        public NumericParser<Double> RidgedSubFrequency
        {
            get { return Mod.ridgedSub.frequency; }
            set { Mod.ridgedSub.frequency = value; }
        }

        // Lacunarity of the subtractive noise
        [ParserTarget("ridgedSubLacunarity")]
        public NumericParser<Double> RidgedSubLacunarity
        {
            get { return Mod.ridgedSub.lacunarity; }
            set { Mod.ridgedSub.lacunarity = value; }
        }

        // Octaves of the subtractive noise
        [ParserTarget("ridgedSubOctaves")]
        public NumericParser<Int32> RidgedSubOctaves
        {
            get { return Mod.ridgedSub.octaves; }
            set { Mod.ridgedSub.octaves = Mathf.Clamp(value, 1, 30); }
        }

        // The quality of the subtractive noise
        [ParserTarget("ridgedSubQuality")]
        public EnumParser<KopernicusNoiseQuality> RidgedSubQuality
        {
            get { return (KopernicusNoiseQuality)(Int32)Mod.ridgedSub.quality; }
            set { Mod.ridgedSub.quality = (NoiseQuality)(Int32)value.Value; }
        }

        // The seed of the subtractive noise
        [ParserTarget("ridgedSubSeed")]
        public NumericParser<Int32> RidgedSubSeed
        {
            get { return Mod.ridgedSub.seed; }
            set { Mod.ridgedSub.seed = value; }
        }

        // Create the mod
        public override void Create(PQS pqsVersion)
        {
            base.Create(pqsVersion);

            // Construct the internal objects.
            Mod.curveMultiplier = new PQSMod_VertexHeightNoiseVertHeightCurve3.SimplexNoise();
            Mod.deformity = new PQSMod_VertexHeightNoiseVertHeightCurve3.SimplexNoise();
            Mod.ridgedAdd = new PQSMod_VertexHeightNoiseVertHeightCurve3.RidgedNoise();
            Mod.ridgedSub = new PQSMod_VertexHeightNoiseVertHeightCurve3.RidgedNoise();
        }

        // Create the mod
        public override void Create(PQSMod_VertexHeightNoiseVertHeightCurve3 mod, PQS pqsVersion)
        {
            base.Create(mod, pqsVersion);

            // Construct the internal objects.
            if (Mod.curveMultiplier == null)
            {
                Mod.curveMultiplier = new PQSMod_VertexHeightNoiseVertHeightCurve3.SimplexNoise();
            }

            if (Mod.deformity == null)
            {
                Mod.deformity = new PQSMod_VertexHeightNoiseVertHeightCurve3.SimplexNoise();
            }

            if (Mod.ridgedAdd == null)
            {
                Mod.ridgedAdd = new PQSMod_VertexHeightNoiseVertHeightCurve3.RidgedNoise();
            }

            if (Mod.ridgedSub == null)
            {
                Mod.ridgedSub = new PQSMod_VertexHeightNoiseVertHeightCurve3.RidgedNoise();
            }
        }
    }
}


