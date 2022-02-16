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
    public class VertexHeightNoiseVertHeightCurve2 : ModLoader<PQSMod_VertexHeightNoiseVertHeightCurve2>
    {
        // deformity
        [ParserTarget("deformity")]
        public NumericParser<Single> Deformity
        {
            get { return Mod.deformity; }
            set { Mod.deformity = value; }
        }

        // ridgedAddFrequency
        [ParserTarget("ridgedAddFrequency")]
        public NumericParser<Single> RidgedAddFrequency
        {
            get { return Mod.ridgedAddFrequency; }
            set { Mod.ridgedAddFrequency = value; }
        }

        // ridgedAddLacunarity
        [ParserTarget("ridgedAddLacunarity")]
        public NumericParser<Single> RidgedAddLacunarity
        {
            get { return Mod.ridgedAddLacunarity; }
            set { Mod.ridgedAddLacunarity = value; }
        }

        // ridgedAddOctaves
        [ParserTarget("ridgedAddOctaves")]
        public NumericParser<Int32> RidgedAddOctaves
        {
            get { return Mod.ridgedAddOctaves; }
            set { Mod.ridgedAddOctaves = Mathf.Clamp(value, 1, 30); }
        }

        // ridgedAddOctaves
        [ParserTarget("ridgedAddSeed")]
        public NumericParser<Int32> RidgedAddSeed
        {
            get { return Mod.ridgedAddSeed; }
            set { Mod.ridgedAddSeed = value; }
        }

        // ridgedMode
        [ParserTarget("ridgedMode")]
        public EnumParser<KopernicusNoiseQuality> RidgedMode
        {
            get { return (KopernicusNoiseQuality)(Int32)Mod.ridgedMode; }
            set { Mod.ridgedMode = (NoiseQuality)(Int32)value.Value; }
        }

        // ridgedSubFrequency
        [ParserTarget("ridgedSubFrequency")]
        public NumericParser<Single> RidgedSubFrequency
        {
            get { return Mod.ridgedSubFrequency; }
            set { Mod.ridgedSubFrequency = value; }
        }

        // ridgedSubLacunarity
        [ParserTarget("ridgedSubLacunarity")]
        public NumericParser<Single> RidgedSubLacunarity
        {
            get { return Mod.ridgedSubLacunarity; }
            set { Mod.ridgedSubLacunarity = value; }
        }

        // ridgedSubOctaves
        [ParserTarget("ridgedSubOctaves")]
        public NumericParser<Int32> RidgedSubOctaves
        {
            get { return Mod.ridgedSubOctaves; }
            set { Mod.ridgedSubOctaves = Mathf.Clamp(value, 1, 30); }
        }

        // ridgedSubSeed
        [ParserTarget("ridgedSubSeed")]
        public NumericParser<Int32> RidgedSubSeed
        {
            get { return Mod.ridgedSubSeed; }
            set { Mod.ridgedSubSeed = value; }
        }

        // simplexCurve
        [ParserTargetCollection("simplexCurve", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> SimplexCurve
        {
            get { return Utility.AnimCurveToList(Mod.simplexCurve); }
            set { Mod.simplexCurve = Utility.ListToAnimCurve(value); }
        }

        // simplexFrequency
        [ParserTarget("simplexFrequency")]
        public NumericParser<Double> SimplexFrequency
        {
            get { return Mod.simplexFrequency; }
            set { Mod.simplexFrequency = value; }
        }

        // simplexHeightEnd
        [ParserTarget("simplexHeightEnd")]
        public NumericParser<Double> SimplexHeightEnd
        {
            get { return Mod.simplexHeightEnd; }
            set { Mod.simplexHeightEnd = value; }
        }

        // simplexHeightStart
        [ParserTarget("simplexHeightStart")]
        public NumericParser<Double> SimplexHeightStart
        {
            get { return Mod.simplexHeightStart; }
            set { Mod.simplexHeightStart = value; }
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
    }
}

