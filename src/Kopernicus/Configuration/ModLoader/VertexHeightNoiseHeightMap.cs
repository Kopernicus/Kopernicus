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
using Kopernicus.Configuration.Parsing;
using UnityEngine;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class VertexHeightNoiseHeightMap : ModLoader<PQSMod_VertexHeightNoiseHeightMap>
    {
        // The texture of the simplex terrain
        [ParserTarget("map")]
        public Texture2DParser Map
        {
            get { return Mod.heightMap; }
            set { Mod.heightMap = Utility.CreateReadable(value); }
        }

        // Where the heightMap starts
        [ParserTarget("heightStart")]
        public NumericParser<Single> HeightStart
        {
            get { return Mod.heightStart; }
            set { Mod.heightStart = value; }
        }

        // Where the heightMap ends
        [ParserTarget("heightEnd")]
        public NumericParser<Single> HeightEnd
        {
            get { return Mod.heightEnd; }
            set { Mod.heightEnd = value; }
        }

        // The deformity of the simplex terrain
        [ParserTarget("deformity")]
        public NumericParser<Single> Deformity
        {
            get { return Mod.deformity; }
            set { Mod.deformity = value; }
        }

        // The frequency of the simplex terrain
        [ParserTarget("frequency")]
        public NumericParser<Single> Frequency
        {
            get { return Mod.frequency; }
            set { Mod.frequency = value; }
        }

        // Octaves of the simplex height
        [ParserTarget("octaves")]
        public NumericParser<Int32> Octaves
        {
            get { return Mod.octaves; }
            set { Mod.octaves = Mathf.Clamp(value, 1, 30); }
        }

        // Persistence of the simplex height
        [ParserTarget("persistance")]
        public NumericParser<Single> Persistance
        {
            get { return Mod.persistance; }
            set { Mod.persistance = value; }
        }

        // The seed of the simplex height
        [ParserTarget("seed")]
        public NumericParser<Int32> Seed
        {
            get { return Mod.seed; }
            set { Mod.seed = value; }
        }

        // The lacunarity of the simplex height
        [ParserTarget("lacunarity")]
        public NumericParser<Single> Lacunarity
        {
            get { return Mod.lacunarity; }
            set { Mod.lacunarity = value; }
        }
    }
}

