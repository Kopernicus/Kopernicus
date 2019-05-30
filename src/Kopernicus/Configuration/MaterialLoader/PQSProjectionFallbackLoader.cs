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
using Kopernicus.Components.MaterialWrapper;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Parsing;
using UnityEngine;

namespace Kopernicus.Configuration.MaterialLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PQSProjectionFallbackLoader : PQSProjectionFallback
    {
        // Saturation, default = 1
        [ParserTarget("saturation")]
        public NumericParser<Single> SaturationSetter
        {
            get { return Saturation; }
            set { Saturation = value; }
        }

        // Contrast, default = 1
        [ParserTarget("contrast")]
        public NumericParser<Single> ContrastSetter
        {
            get { return Contrast; }
            set { Contrast = value; }
        }

        // Colour Unsaturation (A = Factor), default = (1,1,1,0)
        [ParserTarget("tintColor")]
        public ColorParser TintColorSetter
        {
            get { return TintColor; }
            set { TintColor = value; }
        }

        // Near Tiling, default = 1000
        [ParserTarget("texTiling")]
        public NumericParser<Single> TexTilingSetter
        {
            get { return TexTiling; }
            set { TexTiling = value; }
        }

        // Near Blend, default = 0.5
        [ParserTarget("texPower")]
        public NumericParser<Single> TexPowerSetter
        {
            get { return TexPower; }
            set { TexPower = value; }
        }

        // Far Blend, default = 0.5
        [ParserTarget("multiPower")]
        public NumericParser<Single> MultiPowerSetter
        {
            get { return MultiPower; }
            set { MultiPower = value; }
        }

        // NearFar Start, default = 2000
        [ParserTarget("groundTexStart")]
        public NumericParser<Single> GroundTexStartSetter
        {
            get { return GroundTexStart; }
            set { GroundTexStart = value; }
        }

        // NearFar Start, default = 10000
        [ParserTarget("groundTexEnd")]
        public NumericParser<Single> GroundTexEndSetter
        {
            get { return GroundTexEnd; }
            set { GroundTexEnd = value; }
        }

        // Multifactor, default = 0.5
        [ParserTarget("multiFactor")]
        public NumericParser<Single> MultiFactorSetter
        {
            get { return MultiFactor; }
            set { MultiFactor = value; }
        }

        // Main Texture, default = "white" { }
        [ParserTarget("mainTex")]
        public Texture2DParser MainTexSetter
        {
            get { return MainTex; }
            set { MainTex = value; }
        }

        [ParserTarget("mainTexScale")]
        public Vector2Parser MainTexScaleSetter
        {
            get { return MainTexScale; }
            set { MainTexScale = value; }
        }

        [ParserTarget("mainTexOffset")]
        public Vector2Parser MainTexOffsetSetter
        {
            get { return MainTexOffset; }
            set { MainTexOffset = value; }
        }

        // PlanetOpacity, default = 1
        [ParserTarget("planetOpacity")]
        public NumericParser<Single> PlanetOpacitySetter
        {
            get { return PlanetOpacity; }
            set { PlanetOpacity = value; }
        }

        // Constructors
        public PQSProjectionFallbackLoader()
        {
        }

        [Obsolete("Creating materials from shader source String is no longer supported. Use Shader assets instead.")]
        public PQSProjectionFallbackLoader(String contents) : base(contents)
        {
        }

        public PQSProjectionFallbackLoader(Material material) : base(material)
        {
        }
    }
}
