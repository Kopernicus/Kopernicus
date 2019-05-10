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
    public class VertexSimplexMultiChromatic : ModLoader<PQSMod_VertexSimplexMultiChromatic>
    {
        // The frequency of the alpha noise
        [ParserTarget("alphaFrequency")]
        public NumericParser<Double> AlphaFrequency
        {
            get { return Mod.alphaFrequency; }
            set { Mod.alphaFrequency = value; }
        }

        // Octaves of the alpha noise
        [ParserTarget("alphaOctaves")]
        public NumericParser<Double> AlphaOctaves
        {
            get { return Mod.alphaOctaves; }
            set { Mod.alphaOctaves = value; }
        }

        // Persistence of the alpha noise
        [ParserTarget("alphaPersistence")]
        public NumericParser<Double> AlphaPersistence
        {
            get { return Mod.alphaPersistence; }
            set { Mod.alphaPersistence = value; }
        }

        // The seed of the alpha noise
        [ParserTarget("alphaSeed")]
        public NumericParser<Int32> AlphaSeed
        {
            get { return Mod.alphaSeed; }
            set { Mod.alphaSeed = value; }
        }

        // Amount of color that will be applied
        [ParserTarget("blend")]
        public NumericParser<Single> Blend
        {
            get { return Mod.blend; }
            set { Mod.blend = value; }
        }

        // The frequency of the blue noise
        [ParserTarget("blueFrequency")]
        public NumericParser<Double> BlueFrequency
        {
            get { return Mod.blueFrequency; }
            set { Mod.blueFrequency = value; }
        }

        // Octaves of the blue noise
        [ParserTarget("blueOctaves")]
        public NumericParser<Double> BlueOctaves
        {
            get { return Mod.blueOctaves; }
            set { Mod.blueOctaves = value; }
        }

        // Persistence of the blue noise
        [ParserTarget("bluePersistence")]
        public NumericParser<Double> BluePersistence
        {
            get { return Mod.bluePersistence; }
            set { Mod.bluePersistence = value; }
        }

        // The seed of the blue noise
        [ParserTarget("blueSeed")]
        public NumericParser<Int32> BlueSeed
        {
            get { return Mod.blueSeed; }
            set { Mod.blueSeed = value; }
        }

        // The frequency of the green noise
        [ParserTarget("greenFrequency")]
        public NumericParser<Double> GreenFrequency
        {
            get { return Mod.greenFrequency; }
            set { Mod.greenFrequency = value; }
        }

        // Octaves of the green noise
        [ParserTarget("greenOctaves")]
        public NumericParser<Double> GreenOctaves
        {
            get { return Mod.greenOctaves; }
            set { Mod.greenOctaves = value; }
        }

        // Persistence of the green noise
        [ParserTarget("greenPersistence")]
        public NumericParser<Double> GreenPersistence
        {
            get { return Mod.greenPersistence; }
            set { Mod.greenPersistence = value; }
        }

        // The seed of the green noise
        [ParserTarget("greenSeed")]
        public NumericParser<Int32> GreenSeed
        {
            get { return Mod.greenSeed; }
            set { Mod.greenSeed = value; }
        }

        // The frequency of the red noise
        [ParserTarget("redFrequency")]
        public NumericParser<Double> RedFrequency
        {
            get { return Mod.redFrequency; }
            set { Mod.redFrequency = value; }
        }

        // Octaves of the red noise
        [ParserTarget("redOctaves")]
        public NumericParser<Double> RedOctaves
        {
            get { return Mod.redOctaves; }
            set { Mod.redOctaves = value; }
        }

        // Persistence of the red noise
        [ParserTarget("redPersistence")]
        public NumericParser<Double> RedPersistence
        {
            get { return Mod.redPersistence; }
            set { Mod.redPersistence = value; }
        }

        // The seed of the red noise
        [ParserTarget("redSeed")]
        public NumericParser<Int32> RedSeed
        {
            get { return Mod.redSeed; }
            set { Mod.redSeed = value; }
        }
    }
}

