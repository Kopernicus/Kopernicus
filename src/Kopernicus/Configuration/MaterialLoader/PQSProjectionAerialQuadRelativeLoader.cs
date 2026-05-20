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
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Attributes;
using Kopernicus.Configuration.MaterialLoader.Parsing;
using Kopernicus.Configuration.Parsing;
using Kopernicus.UI;
using UnityEngine;
using Gradient = Kopernicus.Configuration.Parsing.Gradient;

namespace Kopernicus.Configuration.MaterialLoader
{
    [RequireConfigType(ConfigType.Node)]
    [MaterialLoader(PQSProjectionAerialQuadRelativeLoader.SHADER_NAME)]
    public class PQSProjectionAerialQuadRelativeLoader : MaterialLoader
    {
        public const String SHADER_NAME = "Terrain/PQS/Sphere Projection SURFACE QUAD (AP) ";
        private static readonly Shader Shader = Shader.Find(SHADER_NAME);
        public static bool UsesSameShader(Material m) => m != null && m.shader.name == SHADER_NAME;

        // Saturation, default = 1
        [ParserTarget("saturation")]
        public NumericParser<float> Saturation
        {
            get => GetFloat("_saturation");
            set => SetFloat("_saturation", value);
        }

        // Contrast, default = 1
        [ParserTarget("contrast")]
        public NumericParser<float> Contrast
        {
            get => GetFloat("_contrast");
            set => SetFloat("_contrast", value);
        }

        // Colour Unsaturation (A = Factor), default = (1,1,1,0)
        [ParserTarget("tintColor")]
        public ColorParser TintColor
        {
            get => GetColor("_tintColor");
            set => SetColor("_tintColor", value);
        }

        // Near Tiling, default = 1000
        [ParserTarget("texTiling")]
        public NumericParser<float> TexTiling
        {
            get => GetFloat("_texTiling");
            set => SetFloat("_texTiling", value);
        }

        // Near Blend, default = 0.5
        [ParserTarget("texPower")]
        public NumericParser<float> TexPower
        {
            get => GetFloat("_texPower");
            set => SetFloat("_texPower", value);
        }

        // Far Blend, default = 0.5
        [ParserTarget("multiPower")]
        public NumericParser<float> MultiPower
        {
            get => GetFloat("_multiPower");
            set => SetFloat("_multiPower", value);
        }

        // NearFar Start, default = 2000
        [ParserTarget("groundTexStart")]
        public NumericParser<float> GroundTexStart
        {
            get => GetFloat("_groundTexStart");
            set => SetFloat("_groundTexStart", value);
        }

        // NearFar Start, default = 10000
        [ParserTarget("groundTexEnd")]
        public NumericParser<float> GroundTexEnd
        {
            get => GetFloat("_groundTexEnd");
            set => SetFloat("_groundTexEnd", value);
        }

        // Steep Tiling, default = 1
        [ParserTarget("steepTiling")]
        public NumericParser<float> SteepTiling
        {
            get => GetFloat("_steepTiling");
            set => SetFloat("_steepTiling", value);
        }

        // Steep Blend, default = 1
        [ParserTarget("steepPower")]
        public NumericParser<float> SteepPower
        {
            get => GetFloat("_steepPower");
            set => SetFloat("_steepPower", value);
        }

        // Steep Fade Start, default = 20000
        [ParserTarget("steepTexStart")]
        public NumericParser<float> SteepTexStart
        {
            get => GetFloat("_steepTexStart");
            set => SetFloat("_steepTexStart", value);
        }

        // Steep Fade End, default = 30000
        [ParserTarget("steepTexEnd")]
        public NumericParser<float> SteepTexEnd
        {
            get => GetFloat("_steepTexEnd");
            set => SetFloat("_steepTexEnd", value);
        }

        // Deep ground, default = "white" { }
        [ParserTarget("deepTex")]
        public MaterialTextureParser DeepTex
        {
            get => GetTexture("_deepTex")?.name;
            set => SetTexture("_deepTex", value);
        }

        [ParserTarget("deepTexScale")]
        public Vector2Parser DeepTexScale
        {
            get => GetTextureScale("_deepTex");
            set => SetTextureScale("_deepTex", value);
        }

        [ParserTarget("deepTexOffset")]
        public Vector2Parser DeepTexOffset
        {
            get => GetTextureOffset("_deepTex");
            set => SetTextureOffset("_deepTex", value);
        }

        // Deep MT, default = "white" { }
        [ParserTarget("deepMultiTex")]
        public MaterialTextureParser DeepMultiTex
        {
            get => GetTexture("_deepMultiTex")?.name;
            set => SetTexture("_deepMultiTex", value);
        }

        [ParserTarget("deepMultiTexScale")]
        public Vector2Parser DeepMultiTexScale
        {
            get => GetTextureScale("_deepMultiTex");
            set => SetTextureScale("_deepMultiTex", value);
        }

        [ParserTarget("deepMultiTexOffset")]
        public Vector2Parser DeepMultiTexOffset
        {
            get => GetTextureOffset("_deepMultiTex");
            set => SetTextureOffset("_deepMultiTex", value);
        }

        // Deep MT Tiling, default = 1
        [ParserTarget("deepMultiFactor")]
        public NumericParser<float> DeepMultiFactor
        {
            get => GetFloat("_deepMultiFactor");
            set => SetFloat("_deepMultiFactor", value);
        }

        // Main Texture, default = "white" { }
        [ParserTarget("mainTex")]
        public MaterialTextureParser MainTex
        {
            get => GetTexture("_mainTex")?.name;
            set => SetTexture("_mainTex", value);
        }

        [ParserTarget("mainTexScale")]
        public Vector2Parser MainTexScale
        {
            get => GetTextureScale("_mainTex");
            set => SetTextureScale("_mainTex", value);
        }

        [ParserTarget("mainTexOffset")]
        public Vector2Parser MainTexOffset
        {
            get => GetTextureOffset("_mainTex");
            set => SetTextureOffset("_mainTex", value);
        }

        // Main MT, default = "white" { }
        [ParserTarget("mainMultiTex")]
        public MaterialTextureParser MainMultiTex
        {
            get => GetTexture("_mainMultiTex")?.name;
            set => SetTexture("_mainMultiTex", value);
        }

        [ParserTarget("mainMultiTexScale")]
        public Vector2Parser MainMultiTexScale
        {
            get => GetTextureScale("_mainMultiTex");
            set => SetTextureScale("_mainMultiTex", value);
        }

        [ParserTarget("mainMultiTexOffset")]
        public Vector2Parser MainMultiTexOffset
        {
            get => GetTextureOffset("_mainMultiTex");
            set => SetTextureOffset("_mainMultiTex", value);
        }

        // Main MT Tiling, default = 1
        [ParserTarget("mainMultiFactor")]
        public NumericParser<float> MainMultiFactor
        {
            get => GetFloat("_mainMultiFactor");
            set => SetFloat("_mainMultiFactor", value);
        }

        // High Ground, default = "white" { }
        [ParserTarget("highTex")]
        public MaterialTextureParser HighTex
        {
            get => GetTexture("_highTex")?.name;
            set => SetTexture("_highTex", value);
        }

        [ParserTarget("highTexScale")]
        public Vector2Parser HighTexScale
        {
            get => GetTextureScale("_highTex");
            set => SetTextureScale("_highTex", value);
        }

        [ParserTarget("highTexOffset")]
        public Vector2Parser HighTexOffset
        {
            get => GetTextureOffset("_highTex");
            set => SetTextureOffset("_highTex", value);
        }

        // High MT, default = "white" { }
        [ParserTarget("highMultiTex")]
        public MaterialTextureParser HighMultiTex
        {
            get => GetTexture("_highMultiTex")?.name;
            set => SetTexture("_highMultiTex", value);
        }

        [ParserTarget("highMultiTexScale")]
        public Vector2Parser HighMultiTexScale
        {
            get => GetTextureScale("_highMultiTex");
            set => SetTextureScale("_highMultiTex", value);
        }

        [ParserTarget("highMultiTexOffset")]
        public Vector2Parser HighMultiTexOffset
        {
            get => GetTextureOffset("_highMultiTex");
            set => SetTextureOffset("_highMultiTex", value);
        }

        // High MT Tiling, default = 1
        [ParserTarget("highMultiFactor")]
        public NumericParser<float> HighMultiFactor
        {
            get => GetFloat("_highMultiFactor");
            set => SetFloat("_highMultiFactor", value);
        }

        // Snow, default = "white" { }
        [ParserTarget("snowTex")]
        public MaterialTextureParser SnowTex
        {
            get => GetTexture("_snowTex")?.name;
            set => SetTexture("_snowTex", value);
        }

        [ParserTarget("snowTexScale")]
        public Vector2Parser SnowTexScale
        {
            get => GetTextureScale("_snowTex");
            set => SetTextureScale("_snowTex", value);
        }

        [ParserTarget("snowTexOffset")]
        public Vector2Parser SnowTexOffset
        {
            get => GetTextureOffset("_snowTex");
            set => SetTextureOffset("_snowTex", value);
        }

        // Snow MT, default = "white" { }
        [ParserTarget("snowMultiTex")]
        public MaterialTextureParser SnowMultiTex
        {
            get => GetTexture("_snowMultiTex")?.name;
            set => SetTexture("_snowMultiTex", value);
        }

        [ParserTarget("snowMultiTexScale")]
        public Vector2Parser SnowMultiTexScale
        {
            get => GetTextureScale("_snowMultiTex");
            set => SetTextureScale("_snowMultiTex", value);
        }

        [ParserTarget("snowMultiTexOffset")]
        public Vector2Parser SnowMultiTexOffset
        {
            get => GetTextureOffset("_snowMultiTex");
            set => SetTextureOffset("_snowMultiTex", value);
        }

        // Snow MT Tiling, default = 1
        [ParserTarget("snowMultiFactor")]
        public NumericParser<float> SnowMultiFactor
        {
            get => GetFloat("_snowMultiFactor");
            set => SetFloat("_snowMultiFactor", value);
        }

        // Steep Texture, default = "white" { }
        [ParserTarget("steepTex")]
        public MaterialTextureParser SteepTex
        {
            get => GetTexture("_steepTex")?.name;
            set => SetTexture("_steepTex", value);
        }

        [ParserTarget("steepTexScale")]
        public Vector2Parser SteepTexScale
        {
            get => GetTextureScale("_steepTex");
            set => SetTextureScale("_steepTex", value);
        }

        [ParserTarget("steepTexOffset")]
        public Vector2Parser SteepTexOffset
        {
            get => GetTextureOffset("_steepTex");
            set => SetTextureOffset("_steepTex", value);
        }

        // Deep Start, default = 0
        [ParserTarget("deepStart")]
        public NumericParser<float> DeepStart
        {
            get => GetFloat("_deepStart");
            set => SetFloat("_deepStart", value);
        }

        // Deep End, default = 0.3
        [ParserTarget("deepEnd")]
        public NumericParser<float> DeepEnd
        {
            get => GetFloat("_deepEnd");
            set => SetFloat("_deepEnd", value);
        }

        // Main lower boundary start, default = 0
        [ParserTarget("mainLoStart")]
        public NumericParser<float> MainLoStart
        {
            get => GetFloat("_mainLoStart");
            set => SetFloat("_mainLoStart", value);
        }

        // Main lower boundary end, default = 0.5
        [ParserTarget("mainLoEnd")]
        public NumericParser<float> MainLoEnd
        {
            get => GetFloat("_mainLoEnd");
            set => SetFloat("_mainLoEnd", value);
        }

        // Main upper boundary start, default = 0.3
        [ParserTarget("mainHiStart")]
        public NumericParser<float> MainHiStart
        {
            get => GetFloat("_mainHiStart");
            set => SetFloat("_mainHiStart", value);
        }

        // Main upper boundary end, default = 0.5
        [ParserTarget("mainHiEnd")]
        public NumericParser<float> MainHiEnd
        {
            get => GetFloat("_mainHiEnd");
            set => SetFloat("_mainHiEnd", value);
        }

        // High lower boundary start, default = 0.6
        [ParserTarget("hiLoStart")]
        public NumericParser<float> HiLoStart
        {
            get => GetFloat("_hiLoStart");
            set => SetFloat("_hiLoStart", value);
        }

        // High lower boundary end, default = 0.6
        [ParserTarget("hiLoEnd")]
        public NumericParser<float> HiLoEnd
        {
            get => GetFloat("_hiLoEnd");
            set => SetFloat("_hiLoEnd", value);
        }

        // High upper boundary start, default = 0.6
        [ParserTarget("hiHiStart")]
        public NumericParser<float> HiHiStart
        {
            get => GetFloat("_hiHiStart");
            set => SetFloat("_hiHiStart", value);
        }

        // High upper boundary end, default = 0.9
        [ParserTarget("hiHiEnd")]
        public NumericParser<float> HiHiEnd
        {
            get => GetFloat("_hiHiEnd");
            set => SetFloat("_hiHiEnd", value);
        }

        // Snow Start, default = 0.9
        [ParserTarget("snowStart")]
        public NumericParser<float> SnowStart
        {
            get => GetFloat("_snowStart");
            set => SetFloat("_snowStart", value);
        }

        // Snow End, default = 1
        [ParserTarget("snowEnd")]
        public NumericParser<float> SnowEnd
        {
            get => GetFloat("_snowEnd");
            set => SetFloat("_snowEnd", value);
        }

        // AP Fog Color, default = (0,0,1,1)
        [ParserTarget("fogColor")]
        public ColorParser FogColor
        {
            get => GetColor("_fogColor");
            set => SetColor("_fogColor", value);
        }

        // AP Height Fall Off, default = 1
        [ParserTarget("heightFallOff")]
        public NumericParser<float> HeightFallOff
        {
            get => GetFloat("_heightFallOff");
            set => SetFloat("_heightFallOff", value);
        }

        // AP Global Density, default = 1
        [ParserTarget("globalDensity")]
        public NumericParser<float> GlobalDensity
        {
            get => GetFloat("_globalDensity");
            set => SetFloat("_globalDensity", value);
        }

        // AP Atmosphere Depth, default = 1
        [ParserTarget("atmosphereDepth")]
        public NumericParser<float> AtmosphereDepth
        {
            get => GetFloat("_atmosphereDepth");
            set => SetFloat("_atmosphereDepth", value);
        }

        // FogColorRamp, default = "white" { }
        [ParserTarget("fogColorRamp")]
        public MaterialTextureParser FogColorRamp
        {
            get => GetTexture("_fogColorRamp")?.name;
            set => SetTexture("_fogColorRamp", value);
        }

        // FogColorRamp, default = "white" { }
        [ParserTarget("FogColorRamp")]
        [KittopiaHideOption]
        public Gradient FogColorRampGradient
        {
            set => SetGradient("_fogColorRamp", value);
        }

        [ParserTarget("fogColorRampScale")]
        public Vector2Parser FogColorRampScale
        {
            get => GetTextureScale("_fogColorRamp");
            set => SetTextureScale("_fogColorRamp", value);
        }

        [ParserTarget("fogColorRampOffset")]
        public Vector2Parser FogColorRampOffset
        {
            get => GetTextureOffset("_fogColorRamp");
            set => SetTextureOffset("_fogColorRamp", value);
        }

        // PlanetOpacity, default = 1
        [ParserTarget("planetOpacity")]
        public NumericParser<float> PlanetOpacity
        {
            get => GetFloat("_PlanetOpacity");
            set => SetFloat("_PlanetOpacity", value);
        }

        public override ShaderParser ShaderParser { get; set; } = Shader;

        // Constructors
        public PQSProjectionAerialQuadRelativeLoader() { }

        public PQSProjectionAerialQuadRelativeLoader(Material material) => Value = new(material);
    }
}
