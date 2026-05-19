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
    [MaterialLoader(PQSMainOptimisedLoader.SHADER_NAME)]
    public class PQSMainOptimisedLoader : PQSMaterialLoader
    {
        public const String SHADER_NAME = "Terrain/PQS/PQS Main - Optimised";
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

        // Near Blend, default = 0.5
        [ParserTarget("powerNear")]
        public NumericParser<float> PowerNearSetter
        {
            get => GetFloat("_powerNear");
            set => SetFloat("_powerNear", value);
        }

        // Far Blend, default = 0.5
        [ParserTarget("powerFar")]
        public NumericParser<float> PowerFarSetter
        {
            get => GetFloat("_powerFar");
            set => SetFloat("_powerFar", value);
        }

        // NearFar Start, default = 2000
        [ParserTarget("groundTexStart")]
        public NumericParser<float> GroundTexStartSetter
        {
            get => GetFloat("_groundTexStart");
            set => SetFloat("_groundTexStart", value);
        }

        // NearFar End, default = 10000
        [ParserTarget("groundTexEnd")]
        public NumericParser<float> GroundTexEndSetter
        {
            get => GetFloat("_groundTexEnd");
            set => SetFloat("_groundTexEnd", value);
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

        // Steep Bump Map, default = "bump" { }
        [ParserTarget("steepBumpMap")]
        public MaterialTextureParser SteepBumpMapSetter
        {
            get => null;
            set => SetTexture("_steepBumpMap", value);
        }

        [ParserTarget("steepBumpMapScale")]
        public Vector2Parser SteepBumpMapScaleSetter
        {
            get => GetTextureScale("_steepBumpMap");
            set => SetTextureScale("_steepBumpMap", value);
        }

        [ParserTarget("steepBumpMapOffset")]
        public Vector2Parser SteepBumpMapOffsetSetter
        {
            get => GetTextureOffset("_steepBumpMap");
            set => SetTextureOffset("_steepBumpMap", value);
        }

        // Steep Near Tiling, default = 1
        [ParserTarget("steepNearTiling")]
        public NumericParser<float> SteepNearTilingSetter
        {
            get => GetFloat("_steepNearTiling");
            set => SetFloat("_steepNearTiling", value);
        }

        // Steep Far Tiling, default = 1
        [ParserTarget("steepTiling")]
        public NumericParser<float> SteepTilingSetter
        {
            get => GetFloat("_steepTiling");
            set => SetFloat("_steepTiling", value);
        }

        // Low Texture, default = "white" { }
        [ParserTarget("lowTex")]
        public MaterialTextureParser LowTexSetter
        {
            get => null;
            set => SetTexture("_lowTex", value);
        }

        [ParserTarget("lowTexScale")]
        public Vector2Parser LowTexScaleSetter
        {
            get => GetTextureScale("_lowTex");
            set => SetTextureScale("_lowTex", value);
        }

        [ParserTarget("lowTexOffset")]
        public Vector2Parser LowTexOffsetSetter
        {
            get => GetTextureOffset("_lowTex");
            set => SetTextureOffset("_lowTex", value);
        }

        // Low Near Tiling, default = 1000
        [ParserTarget("lowNearTiling")]
        public NumericParser<float> LowNearTilingSetter
        {
            get => GetFloat("_lowNearTiling");
            set => SetFloat("_lowNearTiling", value);
        }

        // Low Far Tiling, default = 10
        [ParserTarget("lowMultiFactor")]
        public NumericParser<float> LowMultiFactorSetter
        {
            get => GetFloat("_lowMultiFactor");
            set => SetFloat("_lowMultiFactor", value);
        }

        // Mid Texture, default = "white" { }
        [ParserTarget("midTex")]
        public MaterialTextureParser MidTexSetter
        {
            get => null;
            set => SetTexture("_midTex", value);
        }

        [ParserTarget("midTexScale")]
        public Vector2Parser MidTexScaleSetter
        {
            get => GetTextureScale("_midTex");
            set => SetTextureScale("_midTex", value);
        }

        [ParserTarget("midTexOffset")]
        public Vector2Parser MidTexOffsetSetter
        {
            get => GetTextureOffset("_midTex");
            set => SetTextureOffset("_midTex", value);
        }

        // Mid Bump Map, default = "bump" { }
        [ParserTarget("midBumpMap")]
        public MaterialTextureParser MidBumpMapSetter
        {
            get => null;
            set => SetTexture("_midBumpMap", value);
        }

        [ParserTarget("midBumpMapScale")]
        public Vector2Parser MidBumpMapScaleSetter
        {
            get => GetTextureScale("_midBumpMap");
            set => SetTextureScale("_midBumpMap", value);
        }

        [ParserTarget("midBumpMapOffset")]
        public Vector2Parser MidBumpMapOffsetSetter
        {
            get => GetTextureOffset("_midBumpMap");
            set => SetTextureOffset("_midBumpMap", value);
        }

        // Mid Near Tiling, default = 1000
        [ParserTarget("midNearTiling")]
        public NumericParser<float> MidNearTilingSetter
        {
            get => GetFloat("_midNearTiling");
            set => SetFloat("_midNearTiling", value);
        }

        // Mid Far Tiling, default = 10
        [ParserTarget("midMultiFactor")]
        public NumericParser<float> MidMultiFactorSetter
        {
            get => GetFloat("_midMultiFactor");
            set => SetFloat("_midMultiFactor", value);
        }

        // Mid Bump Near Tiling, default = 1
        [ParserTarget("midBumpNearTiling")]
        public NumericParser<float> MidBumpNearTilingSetter
        {
            get => GetFloat("_midBumpNearTiling");
            set => SetFloat("_midBumpNearTiling", value);
        }

        // High Texture, default = "white" { }
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

        // High Near Tiling, default = 1000
        [ParserTarget("highNearTiling")]
        public NumericParser<float> HighNearTilingSetter
        {
            get => GetFloat("_highNearTiling");
            set => SetFloat("_highNearTiling", value);
        }

        // High Far Tiling, default = 10
        [ParserTarget("highMultiFactor")]
        public NumericParser<float> HighMultiFactorSetter
        {
            get => GetFloat("_highMultiFactor");
            set => SetFloat("_highMultiFactor", value);
        }

        // Low Transition Start, default = 0
        [ParserTarget("lowStart")]
        public NumericParser<float> LowStartSetter
        {
            get => GetFloat("_lowStart");
            set => SetFloat("_lowStart", value);
        }

        // Low Transition End, default = 0.3
        [ParserTarget("lowEnd")]
        public NumericParser<float> LowEndSetter
        {
            get => GetFloat("_lowEnd");
            set => SetFloat("_lowEnd", value);
        }

        // High Transition Start, default = 0.8
        [ParserTarget("highStart")]
        public NumericParser<float> HighStartSetter
        {
            get => GetFloat("_highStart");
            set => SetFloat("_highStart", value);
        }

        // High Transition End, default = 1
        [ParserTarget("highEnd")]
        public NumericParser<float> HighEndSetter
        {
            get => GetFloat("_highEnd");
            set => SetFloat("_highEnd", value);
        }

        // AP Global Density, default = 1
        [ParserTarget("globalDensity")]
        public NumericParser<float> GlobalDensitySetter
        {
            get => GetFloat("_globalDensity");
            set => SetFloat("_globalDensity", value);
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

        // Ocean Fog Dist, default = 1000
        [ParserTarget("oceanFogDistance")]
        public NumericParser<float> OceanFogDistanceSetter
        {
            get => GetFloat("_oceanFogDistance");
            set => SetFloat("_oceanFogDistance", value);
        }

        public override ShaderParser ShaderParser { get; set; } = Shader;

        // Constructors
        public PQSMainOptimisedLoader() { }

        public PQSMainOptimisedLoader(Material material) => Value = new(material);
    }
}
