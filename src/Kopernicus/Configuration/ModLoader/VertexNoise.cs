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
using System.Diagnostics.CodeAnalysis;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class VertexNoise : ModLoader<PQSMod_VertexNoise>
    {
        // falloff
        [ParserTarget("falloff")]
        public NumericParser<Single> Falloff
        {
            get { return Mod.falloff; }
            set { Mod.falloff = value; }
        }

        // mesaVsPlainsBias
        [ParserTarget("mesaVsPlainsBias")]
        public NumericParser<Single> MesaVsPlainsBias
        {
            get { return Mod.mesaVsPlainsBias; }
            set { Mod.mesaVsPlainsBias = value; }
        }

        // noiseDeformity
        [ParserTarget("noiseDeformity")]
        public NumericParser<Single> NoiseDeformity
        {
            get { return Mod.noiseDeformity; }
            set { Mod.noiseDeformity = value; }
        }

        // noisePasses
        [ParserTarget("noisePasses")]
        public NumericParser<Int32> NoisePasses
        {
            get { return Mod.noisePasses; }
            set { Mod.noisePasses = value; }
        }

        // plainSmoothness
        [ParserTarget("plainSmoothness")]
        public NumericParser<Single> PlainSmoothness
        {
            get { return Mod.plainSmoothness; }
            set { Mod.plainSmoothness = value; }
        }

        // plainsVsMountainSmoothness
        [ParserTarget("plainsVsMountainSmoothness")]
        public NumericParser<Single> PlainsVsMountainSmoothness
        {
            get { return Mod.plainsVsMountainSmoothness; }
            set { Mod.plainsVsMountainSmoothness = value; }
        }

        // plainsVsMountainThreshold
        [ParserTarget("plainsVsMountainThreshold")]
        public NumericParser<Single> PlainsVsMountainThreshold
        {
            get { return Mod.plainsVsMountainThreshold; }
            set { Mod.plainsVsMountainThreshold = value; }
        }

        // seed
        [ParserTarget("seed")]
        public NumericParser<Int32> Seed
        {
            get { return Mod.seed; }
            set { Mod.seed = value; }
        }

        // smoothness
        [ParserTarget("smoothness")]
        public NumericParser<Single> Smoothness
        {
            get { return Mod.smoothness; }
            set { Mod.smoothness = value; }
        }
    }
}
