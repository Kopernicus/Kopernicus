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
using Kopernicus.UI;
using UnityEngine;
using Gradient = Kopernicus.Configuration.Parsing.Gradient;

namespace Kopernicus.Configuration.MaterialLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PQSProjectionAerialQuadRelativeLoader : PQSProjectionAerialQuadRelative
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

        // Steep Tiling, default = 1
        [ParserTarget("steepTiling")]
        public NumericParser<Single> SteepTilingSetter
        {
            get { return SteepTiling; }
            set { SteepTiling = value; }
        }

        // Steep Blend, default = 1
        [ParserTarget("steepPower")]
        public NumericParser<Single> SteepPowerSetter
        {
            get { return SteepPower; }
            set { SteepPower = value; }
        }

        // Steep Fade Start, default = 20000
        [ParserTarget("steepTexStart")]
        public NumericParser<Single> SteepTexStartSetter
        {
            get { return SteepTexStart; }
            set { SteepTexStart = value; }
        }

        // Steep Fade End, default = 30000
        [ParserTarget("steepTexEnd")]
        public NumericParser<Single> SteepTexEndSetter
        {
            get { return SteepTexEnd; }
            set { SteepTexEnd = value; }
        }

        // Deep ground, default = "white" { }
        [ParserTarget("deepTex")]
        public Texture2DParser DeepTexSetter
        {
            get { return DeepTex; }
            set { DeepTex = value; }
        }

        [ParserTarget("deepTexScale")]
        public Vector2Parser DeepTexScaleSetter
        {
            get { return DeepTexScale; }
            set { DeepTexScale = value; }
        }

        [ParserTarget("deepTexOffset")]
        public Vector2Parser DeepTexOffsetSetter
        {
            get { return DeepTexOffset; }
            set { DeepTexOffset = value; }
        }

        // Deep MT, default = "white" { }
        [ParserTarget("deepMultiTex")]
        public Texture2DParser DeepMultiTexSetter
        {
            get { return DeepMultiTex; }
            set { DeepMultiTex = value; }
        }

        [ParserTarget("deepMultiTexScale")]
        public Vector2Parser DeepMultiTexScaleSetter
        {
            get { return DeepMultiTexScale; }
            set { DeepMultiTexScale = value; }
        }

        [ParserTarget("deepMultiTexOffset")]
        public Vector2Parser DeepMultiTexOffsetSetter
        {
            get { return DeepMultiTexOffset; }
            set { DeepMultiTexOffset = value; }
        }

        // Deep MT Tiling, default = 1
        [ParserTarget("deepMultiFactor")]
        public NumericParser<Single> DeepMultiFactorSetter
        {
            get { return DeepMultiFactor; }
            set { DeepMultiFactor = value; }
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

        // Main MT, default = "white" { }
        [ParserTarget("mainMultiTex")]
        public Texture2DParser MainMultiTexSetter
        {
            get { return MainMultiTex; }
            set { MainMultiTex = value; }
        }

        [ParserTarget("mainMultiTexScale")]
        public Vector2Parser MainMultiTexScaleSetter
        {
            get { return MainMultiTexScale; }
            set { MainMultiTexScale = value; }
        }

        [ParserTarget("mainMultiTexOffset")]
        public Vector2Parser MainMultiTexOffsetSetter
        {
            get { return MainMultiTexOffset; }
            set { MainMultiTexOffset = value; }
        }

        // Main MT Tiling, default = 1
        [ParserTarget("mainMultiFactor")]
        public NumericParser<Single> MainMultiFactorSetter
        {
            get { return MainMultiFactor; }
            set { MainMultiFactor = value; }
        }

        // High Ground, default = "white" { }
        [ParserTarget("highTex")]
        public Texture2DParser HighTexSetter
        {
            get { return HighTex; }
            set { HighTex = value; }
        }

        [ParserTarget("highTexScale")]
        public Vector2Parser HighTexScaleSetter
        {
            get { return HighTexScale; }
            set { HighTexScale = value; }
        }

        [ParserTarget("highTexOffset")]
        public Vector2Parser HighTexOffsetSetter
        {
            get { return HighTexOffset; }
            set { HighTexOffset = value; }
        }

        // High MT, default = "white" { }
        [ParserTarget("highMultiTex")]
        public Texture2DParser HighMultiTexSetter
        {
            get { return HighMultiTex; }
            set { HighMultiTex = value; }
        }

        [ParserTarget("highMultiTexScale")]
        public Vector2Parser HighMultiTexScaleSetter
        {
            get { return HighMultiTexScale; }
            set { HighMultiTexScale = value; }
        }

        [ParserTarget("highMultiTexOffset")]
        public Vector2Parser HighMultiTexOffsetSetter
        {
            get { return HighMultiTexOffset; }
            set { HighMultiTexOffset = value; }
        }

        // High MT Tiling, default = 1
        [ParserTarget("highMultiFactor")]
        public NumericParser<Single> HighMultiFactorSetter
        {
            get { return HighMultiFactor; }
            set { HighMultiFactor = value; }
        }

        // Snow, default = "white" { }
        [ParserTarget("snowTex")]
        public Texture2DParser SnowTexSetter
        {
            get { return SnowTex; }
            set { SnowTex = value; }
        }

        [ParserTarget("snowTexScale")]
        public Vector2Parser SnowTexScaleSetter
        {
            get { return SnowTexScale; }
            set { SnowTexScale = value; }
        }

        [ParserTarget("snowTexOffset")]
        public Vector2Parser SnowTexOffsetSetter
        {
            get { return SnowTexOffset; }
            set { SnowTexOffset = value; }
        }

        // Snow MT, default = "white" { }
        [ParserTarget("snowMultiTex")]
        public Texture2DParser SnowMultiTexSetter
        {
            get { return SnowMultiTex; }
            set { SnowMultiTex = value; }
        }

        [ParserTarget("snowMultiTexScale")]
        public Vector2Parser SnowMultiTexScaleSetter
        {
            get { return SnowMultiTexScale; }
            set { SnowMultiTexScale = value; }
        }

        [ParserTarget("snowMultiTexOffset")]
        public Vector2Parser SnowMultiTexOffsetSetter
        {
            get { return SnowMultiTexOffset; }
            set { SnowMultiTexOffset = value; }
        }

        // Snow MT Tiling, default = 1
        [ParserTarget("snowMultiFactor")]
        public NumericParser<Single> SnowMultiFactorSetter
        {
            get { return SnowMultiFactor; }
            set { SnowMultiFactor = value; }
        }

        // Steep Texture, default = "white" { }
        [ParserTarget("steepTex")]
        public Texture2DParser SteepTexSetter
        {
            get { return SteepTex; }
            set { SteepTex = value; }
        }

        [ParserTarget("steepTexScale")]
        public Vector2Parser SteepTexScaleSetter
        {
            get { return SteepTexScale; }
            set { SteepTexScale = value; }
        }

        [ParserTarget("steepTexOffset")]
        public Vector2Parser SteepTexOffsetSetter
        {
            get { return SteepTexOffset; }
            set { SteepTexOffset = value; }
        }

        // Deep Start, default = 0
        [ParserTarget("deepStart")]
        public NumericParser<Single> DeepStartSetter
        {
            get { return DeepStart; }
            set { DeepStart = value; }
        }

        // Deep End, default = 0.3
        [ParserTarget("deepEnd")]
        public NumericParser<Single> DeepEndSetter
        {
            get { return DeepEnd; }
            set { DeepEnd = value; }
        }

        // Main lower boundary start, default = 0
        [ParserTarget("mainLoStart")]
        public NumericParser<Single> MainLoStartSetter
        {
            get { return MainLoStart; }
            set { MainLoStart = value; }
        }

        // Main lower boundary end, default = 0.5
        [ParserTarget("mainLoEnd")]
        public NumericParser<Single> MainLoEndSetter
        {
            get { return MainLoEnd; }
            set { MainLoEnd = value; }
        }

        // Main upper boundary start, default = 0.3
        [ParserTarget("mainHiStart")]
        public NumericParser<Single> MainHiStartSetter
        {
            get { return MainHiStart; }
            set { MainHiStart = value; }
        }

        // Main upper boundary end, default = 0.5
        [ParserTarget("mainHiEnd")]
        public NumericParser<Single> MainHiEndSetter
        {
            get { return MainHiEnd; }
            set { MainHiEnd = value; }
        }

        // High lower boundary start, default = 0.6
        [ParserTarget("hiLoStart")]
        public NumericParser<Single> HiLoStartSetter
        {
            get { return HiLoStart; }
            set { HiLoStart = value; }
        }

        // High lower boundary end, default = 0.6
        [ParserTarget("hiLoEnd")]
        public NumericParser<Single> HiLoEndSetter
        {
            get { return HiLoEnd; }
            set { HiLoEnd = value; }
        }

        // High upper boundary start, default = 0.6
        [ParserTarget("hiHiStart")]
        public NumericParser<Single> HiHiStartSetter
        {
            get { return HiHiStart; }
            set { HiHiStart = value; }
        }

        // High upper boundary end, default = 0.9
        [ParserTarget("hiHiEnd")]
        public NumericParser<Single> HiHiEndSetter
        {
            get { return HiHiEnd; }
            set { HiHiEnd = value; }
        }

        // Snow Start, default = 0.9
        [ParserTarget("snowStart")]
        public NumericParser<Single> SnowStartSetter
        {
            get { return SnowStart; }
            set { SnowStart = value; }
        }

        // Snow End, default = 1
        [ParserTarget("snowEnd")]
        public NumericParser<Single> SnowEndSetter
        {
            get { return SnowEnd; }
            set { SnowEnd = value; }
        }

        // AP Fog Color, default = (0,0,1,1)
        [ParserTarget("fogColor")]
        public ColorParser FogColorSetter
        {
            get { return FogColor; }
            set { FogColor = value; }
        }

        // AP Height Fall Off, default = 1
        [ParserTarget("heightFallOff")]
        public NumericParser<Single> HeightFallOffSetter
        {
            get { return HeightFallOff; }
            set { HeightFallOff = value; }
        }

        // AP Global Density, default = 1
        [ParserTarget("globalDensity")]
        public NumericParser<Single> GlobalDensitySetter
        {
            get { return GlobalDensity; }
            set { GlobalDensity = value; }
        }

        // AP Atmosphere Depth, default = 1
        [ParserTarget("atmosphereDepth")]
        public NumericParser<Single> AtmosphereDepthSetter
        {
            get { return AtmosphereDepth; }
            set { AtmosphereDepth = value; }
        }

        // FogColorRamp, default = "white" { }
        [ParserTarget("fogColorRamp")]
        public Texture2DParser FogColorRampSetter
        {
            get { return FogColorRamp; }
            set { FogColorRamp = value; }
        }

        // FogColorRamp, default = "white" { }
        [ParserTarget("FogColorRamp")]
        [KittopiaHideOption]
        public Gradient FogColorRampGradientSetter
        {
            set
            {
                // Generate the ramp from a gradient
                Texture2D ramp = new Texture2D(512, 1);
                Color32[] colors = ramp.GetPixels32(0);
                for (Int32 i = 0; i < colors.Length; i++)
                {
                    // Compute the position in the gradient
                    Single k = (Single) i / colors.Length;
                    colors[i] = value.ColorAt(k);
                }

                ramp.SetPixels32(colors, 0);
                ramp.Apply(true, false);

                // Set the color ramp
                FogColorRamp = ramp;
            }
        }

        [ParserTarget("fogColorRampScale")]
        public Vector2Parser FogColorRampScaleSetter
        {
            get { return FogColorRampScale; }
            set { FogColorRampScale = value; }
        }

        [ParserTarget("fogColorRampOffset")]
        public Vector2Parser FogColorRampOffsetSetter
        {
            get { return FogColorRampOffset; }
            set { FogColorRampOffset = value; }
        }

        // PlanetOpacity, default = 1
        [ParserTarget("planetOpacity")]
        public NumericParser<Single> PlanetOpacitySetter
        {
            get { return PlanetOpacity; }
            set { PlanetOpacity = value; }
        }

        // Constructors
        public PQSProjectionAerialQuadRelativeLoader()
        {
        }

        [Obsolete("Creating materials from shader source String is no longer supported. Use Shader assets instead.")]
        public PQSProjectionAerialQuadRelativeLoader(String contents) : base(contents)
        {
        }

        public PQSProjectionAerialQuadRelativeLoader(Material material) : base(material)
        {
        }
    }
}
