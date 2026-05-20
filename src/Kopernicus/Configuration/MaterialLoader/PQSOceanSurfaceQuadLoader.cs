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
    public class PQSOceanSurfaceQuadLoader : MaterialLoader
    {
        public const String SHADER_NAME = "Terrain/PQS/Ocean Surface Quad";
        private static readonly Shader Shader = Shader.Find(SHADER_NAME);
        public static bool UsesSameShader(Material m) => m != null && m.shader.name == SHADER_NAME;

        // Main Color, default = (1,1,1,1)
        [ParserTarget("color")]
        public ColorParser Color
        {
            get => GetColor("_Color");
            set => SetColor("_Color", value);
        }

        // Color From Space, default = (1,1,1,1)
        [ParserTarget("colorFromSpace")]
        public ColorParser ColorFromSpace
        {
            get => GetColor("_ColorFromSpace");
            set => SetColor("_ColorFromSpace", value);
        }

        // Specular Color, default = (1,1,1,1)
        [ParserTarget("specColor")]
        public ColorParser SpecColor
        {
            get => GetColor("_SpecColor");
            set => SetColor("_SpecColor", value);
        }

        // Shininess, default = 0.078125
        [ParserTarget("shininess")]
        public NumericParser<float> Shininess
        {
            get => GetFloat("_Shininess");
            set => SetFloat("_Shininess", value);
        }

        // Gloss, default = 0.078125
        [ParserTarget("gloss")]
        public NumericParser<float> Gloss
        {
            get => GetFloat("_Gloss");
            set => SetFloat("_Gloss", value);
        }

        // Tex Tiling, default = 1
        [ParserTarget("tiling")]
        public NumericParser<float> Tiling
        {
            get => GetFloat("_tiling");
            set => SetFloat("_tiling", value);
        }

        // Tex0, default = "white" { }
        [ParserTarget("waterTex")]
        public MaterialTextureParser WaterTex
        {
            get => GetTexture("_WaterTex")?.name;
            set => SetTexture("_WaterTex", value);
        }

        [ParserTarget("waterTexScale")]
        public Vector2Parser WaterTexScale
        {
            get => GetTextureScale("_WaterTex");
            set => SetTextureScale("_WaterTex", value);
        }

        [ParserTarget("waterTexOffset")]
        public Vector2Parser WaterTexOffset
        {
            get => GetTextureOffset("_WaterTex");
            set => SetTextureOffset("_WaterTex", value);
        }

        // Tex1, default = "white" { }
        [ParserTarget("waterTex1")]
        public MaterialTextureParser WaterTex1
        {
            get => GetTexture("_WaterTex1")?.name;
            set => SetTexture("_WaterTex1", value);
        }

        [ParserTarget("waterTex1Scale")]
        public Vector2Parser WaterTex1Scale
        {
            get => GetTextureScale("_WaterTex1");
            set => SetTextureScale("_WaterTex1", value);
        }

        [ParserTarget("waterTex1Offset")]
        public Vector2Parser WaterTex1Offset
        {
            get => GetTextureOffset("_WaterTex1");
            set => SetTextureOffset("_WaterTex1", value);
        }

        // Normal Tiling, default = 1
        [ParserTarget("bTiling")]
        public NumericParser<float> BTiling
        {
            get => GetFloat("_bTiling");
            set => SetFloat("_bTiling", value);
        }

        // Normal map, default = "bump" { }
        [ParserTarget("bumpMap")]
        public MaterialTextureParser BumpMap
        {
            get => GetTexture("_BumpMap")?.name;
            set => SetTexture("_BumpMap", value);
        }

        [ParserTarget("bumpMapScale")]
        public Vector2Parser BumpMapScale
        {
            get => GetTextureScale("_BumpMap");
            set => SetTextureScale("_BumpMap", value);
        }

        [ParserTarget("bumpMapOffset")]
        public Vector2Parser BumpMapOffset
        {
            get => GetTextureOffset("_BumpMap");
            set => SetTextureOffset("_BumpMap", value);
        }

        // Water Movement, default = 1
        [ParserTarget("displacement")]
        public NumericParser<float> Displacement
        {
            get => GetFloat("_displacement");
            set => SetFloat("_displacement", value);
        }

        // Texture Displacement, default = 1
        [ParserTarget("texDisplacement")]
        public NumericParser<float> TexDisplacement
        {
            get => GetFloat("_texDisplacement");
            set => SetFloat("_texDisplacement", value);
        }

        // Water Freq, default = 1
        [ParserTarget("dispFreq")]
        public NumericParser<float> DispFreq
        {
            get => GetFloat("_dispFreq");
            set => SetFloat("_dispFreq", value);
        }

        // Mix, default = 1
        [ParserTarget("mix")]
        public NumericParser<float> Mix
        {
            get => GetFloat("_Mix");
            set => SetFloat("_Mix", value);
        }

        // Opacity, default = 1
        [ParserTarget("oceanOpacity")]
        public NumericParser<float> OceanOpacity
        {
            get => GetFloat("_oceanOpacity");
            set => SetFloat("_oceanOpacity", value);
        }

        // Falloff Power, default = 1
        [ParserTarget("falloffPower")]
        public NumericParser<float> FalloffPower
        {
            get => GetFloat("_falloffPower");
            set => SetFloat("_falloffPower", value);
        }

        // Falloff Exp, default = 2
        [ParserTarget("falloffExp")]
        public NumericParser<float> FalloffExp
        {
            get => GetFloat("_falloffExp");
            set => SetFloat("_falloffExp", value);
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

        // FadeStart, default = 1
        [ParserTarget("fadeStart")]
        public NumericParser<float> FadeStart
        {
            get => GetFloat("_fadeStart");
            set => SetFloat("_fadeStart", value);
        }

        // FadeEnd, default = 1
        [ParserTarget("fadeEnd")]
        public NumericParser<float> FadeEnd
        {
            get => GetFloat("_fadeEnd");
            set => SetFloat("_fadeEnd", value);
        }

        // PlanetOpacity, default = 1
        [ParserTarget("planetOpacity")]
        public NumericParser<float> PlanetOpacity
        {
            get => GetFloat("_PlanetOpacity");
            set => SetFloat("_PlanetOpacity", value);
        }

        // NormalXYFudge, default = 0.1
        [ParserTarget("normalXYFudge")]
        public NumericParser<float> NormalXyFudge
        {
            get => GetFloat("_NormalXYFudge");
            set => SetFloat("_NormalXYFudge", value);
        }

        // NormalZFudge, default = 1.1
        [ParserTarget("normalZFudge")]
        public NumericParser<float> NormalZFudge
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
