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
    public class PQSProjectionAerialQuadRelativeLoader : BaseMaterialLoader
    {
        public const String SHADER_NAME = "Terrain/PQS/Sphere Projection SURFACE QUAD (AP) ";
        private static readonly Shader Shader = Shader.Find(SHADER_NAME);
        public static bool UsesSameShader(Material m) => m != null && m.shader.name == SHADER_NAME;

        // Saturation, default = 1
        [ParserTarget("saturation")]
        public NumericParser<float> SaturationSetter
        {
            get => GetFloat("_saturation");
            set => SetFloat("_saturation", value);
        }

        // Contrast, default = 1
        [ParserTarget("contrast")]
        public NumericParser<float> ContrastSetter
        {
            get => GetFloat("_contrast");
            set => SetFloat("_contrast", value);
        }

        // Colour Unsaturation (A = Factor), default = (1,1,1,0)
        [ParserTarget("tintColor")]
        public ColorParser TintColorSetter
        {
            get => GetColor("_tintColor");
            set => SetColor("_tintColor", value);
        }

        // Near Tiling, default = 1000
        [ParserTarget("texTiling")]
        public NumericParser<float> TexTilingSetter
        {
            get => GetFloat("_texTiling");
            set => SetFloat("_texTiling", value);
        }

        // Near Blend, default = 0.5
        [ParserTarget("texPower")]
        public NumericParser<float> TexPowerSetter
        {
            get => GetFloat("_texPower");
            set => SetFloat("_texPower", value);
        }

        // Far Blend, default = 0.5
        [ParserTarget("multiPower")]
        public NumericParser<float> MultiPowerSetter
        {
            get => GetFloat("_multiPower");
            set => SetFloat("_multiPower", value);
        }

        // NearFar Start, default = 2000
        [ParserTarget("groundTexStart")]
        public NumericParser<float> GroundTexStartSetter
        {
            get => GetFloat("_groundTexStart");
            set => SetFloat("_groundTexStart", value);
        }

        // NearFar Start, default = 10000
        [ParserTarget("groundTexEnd")]
        public NumericParser<float> GroundTexEndSetter
        {
            get => GetFloat("_groundTexEnd");
            set => SetFloat("_groundTexEnd", value);
        }

        // Steep Tiling, default = 1
        [ParserTarget("steepTiling")]
        public NumericParser<float> SteepTilingSetter
        {
            get => GetFloat("_steepTiling");
            set => SetFloat("_steepTiling", value);
        }

        // Steep Blend, default = 1
        [ParserTarget("steepPower")]
        public NumericParser<float> SteepPowerSetter
        {
            get => GetFloat("_steepPower");
            set => SetFloat("_steepPower", value);
        }

        // Steep Fade Start, default = 20000
        [ParserTarget("steepTexStart")]
        public NumericParser<float> SteepTexStartSetter
        {
            get => GetFloat("_steepTexStart");
            set => SetFloat("_steepTexStart", value);
        }

        // Steep Fade End, default = 30000
        [ParserTarget("steepTexEnd")]
        public NumericParser<float> SteepTexEndSetter
        {
            get => GetFloat("_steepTexEnd");
            set => SetFloat("_steepTexEnd", value);
        }

        // Deep ground, default = "white" { }
        [ParserTarget("deepTex")]
        public MaterialTextureParser DeepTexSetter
        {
            get => null;
            set => SetTexture("_deepTex", value);
        }

        [ParserTarget("deepTexScale")]
        public Vector2Parser DeepTexScaleSetter
        {
            get => GetTextureScale("_deepTex");
            set => SetTextureScale("_deepTex", value);
        }

        [ParserTarget("deepTexOffset")]
        public Vector2Parser DeepTexOffsetSetter
        {
            get => GetTextureOffset("_deepTex");
            set => SetTextureOffset("_deepTex", value);
        }

        // Deep MT, default = "white" { }
        [ParserTarget("deepMultiTex")]
        public MaterialTextureParser DeepMultiTexSetter
        {
            get => null;
            set => SetTexture("_deepMultiTex", value);
        }

        [ParserTarget("deepMultiTexScale")]
        public Vector2Parser DeepMultiTexScaleSetter
        {
            get => GetTextureScale("_deepMultiTex");
            set => SetTextureScale("_deepMultiTex", value);
        }

        [ParserTarget("deepMultiTexOffset")]
        public Vector2Parser DeepMultiTexOffsetSetter
        {
            get => GetTextureOffset("_deepMultiTex");
            set => SetTextureOffset("_deepMultiTex", value);
        }

        // Deep MT Tiling, default = 1
        [ParserTarget("deepMultiFactor")]
        public NumericParser<float> DeepMultiFactorSetter
        {
            get => GetFloat("_deepMultiFactor");
            set => SetFloat("_deepMultiFactor", value);
        }

        // Main Texture, default = "white" { }
        [ParserTarget("mainTex")]
        public MaterialTextureParser MainTexSetter
        {
            get => null;
            set => SetTexture("_mainTex", value);
        }

        [ParserTarget("mainTexScale")]
        public Vector2Parser MainTexScaleSetter
        {
            get => GetTextureScale("_mainTex");
            set => SetTextureScale("_mainTex", value);
        }

        [ParserTarget("mainTexOffset")]
        public Vector2Parser MainTexOffsetSetter
        {
            get => GetTextureOffset("_mainTex");
            set => SetTextureOffset("_mainTex", value);
        }

        // Main MT, default = "white" { }
        [ParserTarget("mainMultiTex")]
        public MaterialTextureParser MainMultiTexSetter
        {
            get => null;
            set => SetTexture("_mainMultiTex", value);
        }

        [ParserTarget("mainMultiTexScale")]
        public Vector2Parser MainMultiTexScaleSetter
        {
            get => GetTextureScale("_mainMultiTex");
            set => SetTextureScale("_mainMultiTex", value);
        }

        [ParserTarget("mainMultiTexOffset")]
        public Vector2Parser MainMultiTexOffsetSetter
        {
            get => GetTextureOffset("_mainMultiTex");
            set => SetTextureOffset("_mainMultiTex", value);
        }

        // Main MT Tiling, default = 1
        [ParserTarget("mainMultiFactor")]
        public NumericParser<float> MainMultiFactorSetter
        {
            get => GetFloat("_mainMultiFactor");
            set => SetFloat("_mainMultiFactor", value);
        }

        // High Ground, default = "white" { }
        [ParserTarget("highTex")]
        public MaterialTextureParser HighTexSetter
        {
            get => null;
            set => SetTexture("_highTex", value);
        }

        [ParserTarget("highTexScale")]
        public Vector2Parser HighTexScaleSetter
        {
            get => GetTextureScale("_highTex");
            set => SetTextureScale("_highTex", value);
        }

        [ParserTarget("highTexOffset")]
        public Vector2Parser HighTexOffsetSetter
        {
            get => GetTextureOffset("_highTex");
            set => SetTextureOffset("_highTex", value);
        }

        // High MT, default = "white" { }
        [ParserTarget("highMultiTex")]
        public MaterialTextureParser HighMultiTexSetter
        {
            get => null;
            set => SetTexture("_highMultiTex", value);
        }

        [ParserTarget("highMultiTexScale")]
        public Vector2Parser HighMultiTexScaleSetter
        {
            get => GetTextureScale("_highMultiTex");
            set => SetTextureScale("_highMultiTex", value);
        }

        [ParserTarget("highMultiTexOffset")]
        public Vector2Parser HighMultiTexOffsetSetter
        {
            get => GetTextureOffset("_highMultiTex");
            set => SetTextureOffset("_highMultiTex", value);
        }

        // High MT Tiling, default = 1
        [ParserTarget("highMultiFactor")]
        public NumericParser<float> HighMultiFactorSetter
        {
            get => GetFloat("_highMultiFactor");
            set => SetFloat("_highMultiFactor", value);
        }

        // Snow, default = "white" { }
        [ParserTarget("snowTex")]
        public MaterialTextureParser SnowTexSetter
        {
            get => null;
            set => SetTexture("_snowTex", value);
        }

        [ParserTarget("snowTexScale")]
        public Vector2Parser SnowTexScaleSetter
        {
            get => GetTextureScale("_snowTex");
            set => SetTextureScale("_snowTex", value);
        }

        [ParserTarget("snowTexOffset")]
        public Vector2Parser SnowTexOffsetSetter
        {
            get => GetTextureOffset("_snowTex");
            set => SetTextureOffset("_snowTex", value);
        }

        // Snow MT, default = "white" { }
        [ParserTarget("snowMultiTex")]
        public MaterialTextureParser SnowMultiTexSetter
        {
            get => null;
            set => SetTexture("_snowMultiTex", value);
        }

        [ParserTarget("snowMultiTexScale")]
        public Vector2Parser SnowMultiTexScaleSetter
        {
            get => GetTextureScale("_snowMultiTex");
            set => SetTextureScale("_snowMultiTex", value);
        }

        [ParserTarget("snowMultiTexOffset")]
        public Vector2Parser SnowMultiTexOffsetSetter
        {
            get => GetTextureOffset("_snowMultiTex");
            set => SetTextureOffset("_snowMultiTex", value);
        }

        // Snow MT Tiling, default = 1
        [ParserTarget("snowMultiFactor")]
        public NumericParser<float> SnowMultiFactorSetter
        {
            get => GetFloat("_snowMultiFactor");
            set => SetFloat("_snowMultiFactor", value);
        }

        // Steep Texture, default = "white" { }
        [ParserTarget("steepTex")]
        public MaterialTextureParser SteepTexSetter
        {
            get => null;
            set => SetTexture("_steepTex", value);
        }

        [ParserTarget("steepTexScale")]
        public Vector2Parser SteepTexScaleSetter
        {
            get => GetTextureScale("_steepTex");
            set => SetTextureScale("_steepTex", value);
        }

        [ParserTarget("steepTexOffset")]
        public Vector2Parser SteepTexOffsetSetter
        {
            get => GetTextureOffset("_steepTex");
            set => SetTextureOffset("_steepTex", value);
        }

        // Deep Start, default = 0
        [ParserTarget("deepStart")]
        public NumericParser<float> DeepStartSetter
        {
            get => GetFloat("_deepStart");
            set => SetFloat("_deepStart", value);
        }

        // Deep End, default = 0.3
        [ParserTarget("deepEnd")]
        public NumericParser<float> DeepEndSetter
        {
            get => GetFloat("_deepEnd");
            set => SetFloat("_deepEnd", value);
        }

        // Main lower boundary start, default = 0
        [ParserTarget("mainLoStart")]
        public NumericParser<float> MainLoStartSetter
        {
            get => GetFloat("_mainLoStart");
            set => SetFloat("_mainLoStart", value);
        }

        // Main lower boundary end, default = 0.5
        [ParserTarget("mainLoEnd")]
        public NumericParser<float> MainLoEndSetter
        {
            get => GetFloat("_mainLoEnd");
            set => SetFloat("_mainLoEnd", value);
        }

        // Main upper boundary start, default = 0.3
        [ParserTarget("mainHiStart")]
        public NumericParser<float> MainHiStartSetter
        {
            get => GetFloat("_mainHiStart");
            set => SetFloat("_mainHiStart", value);
        }

        // Main upper boundary end, default = 0.5
        [ParserTarget("mainHiEnd")]
        public NumericParser<float> MainHiEndSetter
        {
            get => GetFloat("_mainHiEnd");
            set => SetFloat("_mainHiEnd", value);
        }

        // High lower boundary start, default = 0.6
        [ParserTarget("hiLoStart")]
        public NumericParser<float> HiLoStartSetter
        {
            get => GetFloat("_hiLoStart");
            set => SetFloat("_hiLoStart", value);
        }

        // High lower boundary end, default = 0.6
        [ParserTarget("hiLoEnd")]
        public NumericParser<float> HiLoEndSetter
        {
            get => GetFloat("_hiLoEnd");
            set => SetFloat("_hiLoEnd", value);
        }

        // High upper boundary start, default = 0.6
        [ParserTarget("hiHiStart")]
        public NumericParser<float> HiHiStartSetter
        {
            get => GetFloat("_hiHiStart");
            set => SetFloat("_hiHiStart", value);
        }

        // High upper boundary end, default = 0.9
        [ParserTarget("hiHiEnd")]
        public NumericParser<float> HiHiEndSetter
        {
            get => GetFloat("_hiHiEnd");
            set => SetFloat("_hiHiEnd", value);
        }

        // Snow Start, default = 0.9
        [ParserTarget("snowStart")]
        public NumericParser<float> SnowStartSetter
        {
            get => GetFloat("_snowStart");
            set => SetFloat("_snowStart", value);
        }

        // Snow End, default = 1
        [ParserTarget("snowEnd")]
        public NumericParser<float> SnowEndSetter
        {
            get => GetFloat("_snowEnd");
            set => SetFloat("_snowEnd", value);
        }

        // AP Fog Color, default = (0,0,1,1)
        [ParserTarget("fogColor")]
        public ColorParser FogColorSetter
        {
            get => GetColor("_fogColor");
            set => SetColor("_fogColor", value);
        }

        // AP Height Fall Off, default = 1
        [ParserTarget("heightFallOff")]
        public NumericParser<float> HeightFallOffSetter
        {
            get => GetFloat("_heightFallOff");
            set => SetFloat("_heightFallOff", value);
        }

        // AP Global Density, default = 1
        [ParserTarget("globalDensity")]
        public NumericParser<float> GlobalDensitySetter
        {
            get => GetFloat("_globalDensity");
            set => SetFloat("_globalDensity", value);
        }

        // AP Atmosphere Depth, default = 1
        [ParserTarget("atmosphereDepth")]
        public NumericParser<float> AtmosphereDepthSetter
        {
            get => GetFloat("_atmosphereDepth");
            set => SetFloat("_atmosphereDepth", value);
        }

        // FogColorRamp, default = "white" { }
        [ParserTarget("fogColorRamp")]
        public MaterialTextureParser FogColorRampSetter
        {
            get => null;
            set => SetTexture("_fogColorRamp", value);
        }

        // FogColorRamp, default = "white" { }
        [ParserTarget("FogColorRamp")]
        [KittopiaHideOption]
        public Gradient FogColorRampGradientSetter
        {
            set => SetGradient("_fogColorRamp", value);
        }

        [ParserTarget("fogColorRampScale")]
        public Vector2Parser FogColorRampScaleSetter
        {
            get => GetTextureScale("_fogColorRamp");
            set => SetTextureScale("_fogColorRamp", value);
        }

        [ParserTarget("fogColorRampOffset")]
        public Vector2Parser FogColorRampOffsetSetter
        {
            get => GetTextureOffset("_fogColorRamp");
            set => SetTextureOffset("_fogColorRamp", value);
        }

        // PlanetOpacity, default = 1
        [ParserTarget("planetOpacity")]
        public NumericParser<float> PlanetOpacitySetter
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
