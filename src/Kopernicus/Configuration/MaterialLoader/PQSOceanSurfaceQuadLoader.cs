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
    [MaterialLoader(PQSOceanSurfaceQuadLoader.SHADER_NAME)]
    public class PQSOceanSurfaceQuadLoader : PQSMaterialLoader
    {
        public const String SHADER_NAME = "Terrain/PQS/Ocean Surface Quad";
        private static readonly Shader Shader = Shader.Find(SHADER_NAME);
        public static bool UsesSameShader(Material m) => m != null && m.shader.name == SHADER_NAME;

        // Main Color, default = (1,1,1,1)
        [ParserTarget("color")]
        public ColorParser ColorSetter
        {
            get => GetColor("_Color");
            set => SetColor("_Color", value);
        }

        // Color From Space, default = (1,1,1,1)
        [ParserTarget("colorFromSpace")]
        public ColorParser ColorFromSpaceSetter
        {
            get => GetColor("_ColorFromSpace");
            set => SetColor("_ColorFromSpace", value);
        }

        // Specular Color, default = (1,1,1,1)
        [ParserTarget("specColor")]
        public ColorParser SpecColorSetter
        {
            get => GetColor("_SpecColor");
            set => SetColor("_SpecColor", value);
        }

        // Shininess, default = 0.078125
        [ParserTarget("shininess")]
        public NumericParser<float> ShininessSetter
        {
            get => GetFloat("_Shininess");
            set => SetFloat("_Shininess", value);
        }

        // Gloss, default = 0.078125
        [ParserTarget("gloss")]
        public NumericParser<float> GlossSetter
        {
            get => GetFloat("_Gloss");
            set => SetFloat("_Gloss", value);
        }

        // Tex Tiling, default = 1
        [ParserTarget("tiling")]
        public NumericParser<float> TilingSetter
        {
            get => GetFloat("_tiling");
            set => SetFloat("_tiling", value);
        }

        // Tex0, default = "white" { }
        [ParserTarget("waterTex")]
        public MaterialTextureParser WaterTexSetter
        {
            get => null;
            set => SetTexture("_WaterTex", value);
        }

        [ParserTarget("waterTexScale")]
        public Vector2Parser WaterTexScaleSetter
        {
            get => GetTextureScale("_WaterTex");
            set => SetTextureScale("_WaterTex", value);
        }

        [ParserTarget("waterTexOffset")]
        public Vector2Parser WaterTexOffsetSetter
        {
            get => GetTextureOffset("_WaterTex");
            set => SetTextureOffset("_WaterTex", value);
        }

        // Tex1, default = "white" { }
        [ParserTarget("waterTex1")]
        public MaterialTextureParser WaterTex1Setter
        {
            get => null;
            set => SetTexture("_WaterTex1", value);
        }

        [ParserTarget("waterTex1Scale")]
        public Vector2Parser WaterTex1ScaleSetter
        {
            get => GetTextureScale("_WaterTex1");
            set => SetTextureScale("_WaterTex1", value);
        }

        [ParserTarget("waterTex1Offset")]
        public Vector2Parser WaterTex1OffsetSetter
        {
            get => GetTextureOffset("_WaterTex1");
            set => SetTextureOffset("_WaterTex1", value);
        }

        // Normal Tiling, default = 1
        [ParserTarget("bTiling")]
        public NumericParser<float> BTilingSetter
        {
            get => GetFloat("_bTiling");
            set => SetFloat("_bTiling", value);
        }

        // Normal map, default = "bump" { }
        [ParserTarget("bumpMap")]
        public MaterialTextureParser BumpMapSetter
        {
            get => null;
            set => SetTexture("_BumpMap", value);
        }

        [ParserTarget("bumpMapScale")]
        public Vector2Parser BumpMapScaleSetter
        {
            get => GetTextureScale("_BumpMap");
            set => SetTextureScale("_BumpMap", value);
        }

        [ParserTarget("bumpMapOffset")]
        public Vector2Parser BumpMapOffsetSetter
        {
            get => GetTextureOffset("_BumpMap");
            set => SetTextureOffset("_BumpMap", value);
        }

        // Water Movement, default = 1
        [ParserTarget("displacement")]
        public NumericParser<float> DisplacementSetter
        {
            get => GetFloat("_displacement");
            set => SetFloat("_displacement", value);
        }

        // Texture Displacement, default = 1
        [ParserTarget("texDisplacement")]
        public NumericParser<float> TexDisplacementSetter
        {
            get => GetFloat("_texDisplacement");
            set => SetFloat("_texDisplacement", value);
        }

        // Water Freq, default = 1
        [ParserTarget("dispFreq")]
        public NumericParser<float> DispFreqSetter
        {
            get => GetFloat("_dispFreq");
            set => SetFloat("_dispFreq", value);
        }

        // Mix, default = 1
        [ParserTarget("mix")]
        public NumericParser<float> MixSetter
        {
            get => GetFloat("_Mix");
            set => SetFloat("_Mix", value);
        }

        // Opacity, default = 1
        [ParserTarget("oceanOpacity")]
        public NumericParser<float> OceanOpacitySetter
        {
            get => GetFloat("_oceanOpacity");
            set => SetFloat("_oceanOpacity", value);
        }

        // Falloff Power, default = 1
        [ParserTarget("falloffPower")]
        public NumericParser<float> FalloffPowerSetter
        {
            get => GetFloat("_falloffPower");
            set => SetFloat("_falloffPower", value);
        }

        // Falloff Exp, default = 2
        [ParserTarget("falloffExp")]
        public NumericParser<float> FalloffExpSetter
        {
            get => GetFloat("_falloffExp");
            set => SetFloat("_falloffExp", value);
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

        // FadeStart, default = 1
        [ParserTarget("fadeStart")]
        public NumericParser<float> FadeStartSetter
        {
            get => GetFloat("_fadeStart");
            set => SetFloat("_fadeStart", value);
        }

        // FadeEnd, default = 1
        [ParserTarget("fadeEnd")]
        public NumericParser<float> FadeEndSetter
        {
            get => GetFloat("_fadeEnd");
            set => SetFloat("_fadeEnd", value);
        }

        // PlanetOpacity, default = 1
        [ParserTarget("planetOpacity")]
        public NumericParser<float> PlanetOpacitySetter
        {
            get => GetFloat("_PlanetOpacity");
            set => SetFloat("_PlanetOpacity", value);
        }

        // NormalXYFudge, default = 0.1
        [ParserTarget("normalXYFudge")]
        public NumericParser<float> NormalXyFudgeSetter
        {
            get => GetFloat("_NormalXYFudge");
            set => SetFloat("_NormalXYFudge", value);
        }

        // NormalZFudge, default = 1.1
        [ParserTarget("normalZFudge")]
        public NumericParser<float> NormalZFudgeSetter
        {
            get => GetFloat("_NormalZFudge");
            set => SetFloat("_NormalZFudge", value);
        }

        public override ShaderParser ShaderParser { get; set; } = Shader;

        // Constructors
        public PQSOceanSurfaceQuadLoader() { }

        public PQSOceanSurfaceQuadLoader(Material material) => Value = new(material);
    }
}
