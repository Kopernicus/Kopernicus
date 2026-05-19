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
using UnityEngine;

namespace Kopernicus.Configuration.MaterialLoader
{
    [RequireConfigType(ConfigType.Node)]
    [MaterialLoader(StandardSpecularLoader.SHADER_NAME)]
    public class StandardSpecularLoader : MaterialLoader
    {
        public const String SHADER_NAME = "Standard (Specular setup)";
        private static readonly Shader Shader = Shader.Find(SHADER_NAME);
        public static bool UsesSameShader(Material m) => m != null && m.shader.name == SHADER_NAME;

        // Color, default = (1.000000,1.000000,1.000000,1.000000)
        [ParserTarget("color")]
        public ColorParser colorSetter
        {
            get => GetColor("_Color");
            set => SetColor("_Color", value);
        }

        // Albedo, default = "white" { }
        [ParserTarget("mainTex")]
        public MaterialTextureParser mainTexSetter
        {
            get => null;
            set => SetTexture("_MainTex", value);
        }

        [ParserTarget("mainTexScale")]
        public Vector2Parser mainTexScaleSetter
        {
            get => GetTextureScale("_MainTex");
            set => SetTextureScale("_MainTex", value);
        }

        [ParserTarget("mainTexOffset")]
        public Vector2Parser mainTexOffsetSetter
        {
            get => GetTextureOffset("_MainTex");
            set => SetTextureOffset("_MainTex", value);
        }

        // Alpha Cutoff, default = 0.500000
        [ParserTarget("cutoff")]
        public NumericParser<float> cutoffSetter
        {
            get => GetFloat("_Cutoff");
            set => SetFloat("_Cutoff", value);
        }

        // Smoothness, default = 0.500000
        [ParserTarget("glossiness")]
        public NumericParser<float> glossinessSetter
        {
            get => GetFloat("_Glossiness");
            set => SetFloat("_Glossiness", value);
        }

        // Smoothness Scale, default = 1.000000
        [ParserTarget("glossMapScale")]
        public NumericParser<float> glossMapScaleSetter
        {
            get => GetFloat("_GlossMapScale");
            set => SetFloat("_GlossMapScale", value);
        }

        // Smoothness texture channel, default = 0.000000
        [ParserTarget("smoothnessTextureChannel")]
        public NumericParser<float> smoothnessTextureChannelSetter
        {
            get => GetFloat("_SmoothnessTextureChannel");
            set => SetFloat("_SmoothnessTextureChannel", value);
        }

        // Specular, default = (0.2,0.2,0.2,1)
        [ParserTarget("specColor")]
        public ColorParser specColorSetter
        {
            get => GetColor("_SpecColor");
            set => SetColor("_SpecColor", value);
        }

        // Specular, default = "white" { }
        [ParserTarget("specGlossMap")]
        public MaterialTextureParser specGlossMapSetter
        {
            get => null;
            set => SetTexture("_SpecGlossMap", value);
        }

        [ParserTarget("metallicGlossMapScale")]
        public Vector2Parser metallicGlossMapScaleSetter
        {
            get => GetTextureScale("_SpecGlossMap");
            set => SetTextureScale("_SpecGlossMap", value);
        }

        [ParserTarget("metallicGlossMapOffset")]
        public Vector2Parser metallicGlossMapOffsetSetter
        {
            get => GetTextureOffset("_SpecGlossMap");
            set => SetTextureOffset("_SpecGlossMap", value);
        }

        // Specular Highlights, default = 1.000000
        [ParserTarget("specularHighlights")]
        public NumericParser<float> specularHighlightsSetter
        {
            get => GetFloat("_SpecularHighlights");
            set => SetFloat("_SpecularHighlights", value);
        }

        // Glossy Reflections, default = 1.000000
        [ParserTarget("glossyReflections")]
        public NumericParser<float> glossyReflectionsSetter
        {
            get => GetFloat("_GlossyReflections");
            set => SetFloat("_GlossyReflections", value);
        }

        // Scale, default = 1.000000
        [ParserTarget("bumpScale")]
        public NumericParser<float> bumpScaleSetter
        {
            get => GetFloat("_BumpScale");
            set => SetFloat("_BumpScale", value);
        }

        // Normal Map, default = "bump" { }
        [ParserTarget("bumpMap")]
        public MaterialTextureParser bumpMapSetter
        {
            get => null;
            set => SetTexture("_BumpMap", value);
        }

        [ParserTarget("bumpMapScale")]
        public Vector2Parser bumpMapScaleSetter
        {
            get => GetTextureScale("_BumpMap");
            set => SetTextureScale("_BumpMap", value);
        }

        [ParserTarget("bumpMapOffset")]
        public Vector2Parser bumpMapOffsetSetter
        {
            get => GetTextureOffset("_BumpMap");
            set => SetTextureOffset("_BumpMap", value);
        }

        // Height Scale, default = 0.020000
        [ParserTarget("parallax")]
        public NumericParser<float> parallaxSetter
        {
            get => GetFloat("_Parallax");
            set => SetFloat("_Parallax", value);
        }

        // Height Map, default = "black" { }
        [ParserTarget("parallaxMap")]
        public MaterialTextureParser parallaxMapSetter
        {
            get => null;
            set => SetTexture("_ParallaxMap", value);
        }

        [ParserTarget("parallaxMapScale")]
        public Vector2Parser parallaxMapScaleSetter
        {
            get => GetTextureScale("_ParallaxMap");
            set => SetTextureScale("_ParallaxMap", value);
        }

        [ParserTarget("parallaxMapOffset")]
        public Vector2Parser parallaxMapOffsetSetter
        {
            get => GetTextureOffset("_ParallaxMap");
            set => SetTextureOffset("_ParallaxMap", value);
        }

        // Strength, default = 1.000000
        [ParserTarget("occlusionStrength")]
        public NumericParser<float> occlusionStrengthSetter
        {
            get => GetFloat("_OcclusionStrength");
            set => SetFloat("_OcclusionStrength", value);
        }

        // Occlusion, default = "white" { }
        [ParserTarget("occlusionMap")]
        public MaterialTextureParser occlusionMapSetter
        {
            get => null;
            set => SetTexture("_OcclusionMap", value);
        }

        [ParserTarget("occlusionMapScale")]
        public Vector2Parser occlusionMapScaleSetter
        {
            get => GetTextureScale("_OcclusionMap");
            set => SetTextureScale("_OcclusionMap", value);
        }

        [ParserTarget("occlusionMapOffset")]
        public Vector2Parser occlusionMapOffsetSetter
        {
            get => GetTextureOffset("_OcclusionMap");
            set => SetTextureOffset("_OcclusionMap", value);
        }

        // Color, default = (0.000000,0.000000,0.000000,1.000000)
        [ParserTarget("emissionColor")]
        public ColorParser emissionColorSetter
        {
            get => GetColor("_EmissionColor");
            set => SetColor("_EmissionColor", value);
        }

        // Emission, default = "white" { }
        [ParserTarget("emissionMap")]
        public MaterialTextureParser emissionMapSetter
        {
            get => null;
            set => SetTexture("_EmissionMap", value);
        }

        [ParserTarget("emissionMapScale")]
        public Vector2Parser emissionMapScaleSetter
        {
            get => GetTextureScale("_EmissionMap");
            set => SetTextureScale("_EmissionMap", value);
        }

        [ParserTarget("emissionMapOffset")]
        public Vector2Parser emissionMapOffsetSetter
        {
            get => GetTextureOffset("_EmissionMap");
            set => SetTextureOffset("_EmissionMap", value);
        }

        // Detail Mask, default = "white" { }
        [ParserTarget("detailMask")]
        public MaterialTextureParser detailMaskSetter
        {
            get => null;
            set => SetTexture("_DetailMask", value);
        }

        [ParserTarget("detailMaskScale")]
        public Vector2Parser detailMaskScaleSetter
        {
            get => GetTextureScale("_DetailMask");
            set => SetTextureScale("_DetailMask", value);
        }

        [ParserTarget("detailMaskOffset")]
        public Vector2Parser detailMaskOffsetSetter
        {
            get => GetTextureOffset("_DetailMask");
            set => SetTextureOffset("_DetailMask", value);
        }

        // Detail Albedo x2, default = "grey" { }
        [ParserTarget("detailAlbedoMap")]
        public MaterialTextureParser detailAlbedoMapSetter
        {
            get => null;
            set => SetTexture("_DetailAlbedoMap", value);
        }

        [ParserTarget("detailAlbedoMapScale")]
        public Vector2Parser detailAlbedoMapScaleSetter
        {
            get => GetTextureScale("_DetailAlbedoMap");
            set => SetTextureScale("_DetailAlbedoMap", value);
        }

        [ParserTarget("detailAlbedoMapOffset")]
        public Vector2Parser detailAlbedoMapOffsetSetter
        {
            get => GetTextureOffset("_DetailAlbedoMap");
            set => SetTextureOffset("_DetailAlbedoMap", value);
        }

        // Normal Map, default = "bump" { }
        [ParserTarget("detailNormalMap")]
        public MaterialTextureParser detailNormalMapSetter
        {
            get => null;
            set => SetTexture("_DetailNormalMap", value);
        }

        // Scale, default = 1.000000
        [ParserTarget("detailNormalMapScale")]
        public Vector2Parser detailNormalMapScaleSetter
        {
            get => GetTextureScale("_DetailNormalMap");
            set => SetTextureScale("_DetailNormalMap", value);
        }

        [ParserTarget("detailNormalMapOffset")]
        public Vector2Parser detailNormalMapOffsetSetter
        {
            get => GetTextureOffset("_DetailNormalMap");
            set => SetTextureOffset("_DetailNormalMap", value);
        }

        // UV Set for secondary textures, default = 0.000000
        [ParserTarget("UVSec")]
        public NumericParser<float> UVSecSetter
        {
            get => GetFloat("_UVSec");
            set => SetFloat("_UVSec", value);
        }

        // __mode, default = 0.000000
        [ParserTarget("mode")]
        public NumericParser<float> modeSetter
        {
            get => GetFloat("_Mode");
            set => SetFloat("_Mode", value);
        }

        // __src, default = 1.000000
        [ParserTarget("srcBlend")]
        public NumericParser<float> srcBlendSetter
        {
            get => GetFloat("_SrcBlend");
            set => SetFloat("_SrcBlend", value);
        }

        // __dst, default = 0.000000
        [ParserTarget("dstBlend")]
        public NumericParser<float> dstBlendSetter
        {
            get => GetFloat("_DstBlend");
            set => SetFloat("_DstBlend", value);
        }

        // __zw, default = 1.000000
        [ParserTarget("ZWrite")]
        public NumericParser<float> ZWriteSetter
        {
            get => GetFloat("_ZWrite");
            set => SetFloat("_ZWrite", value);
        }

        public override ShaderParser ShaderParser { get; set; } = Shader;

        // Constructors
        public StandardSpecularLoader() { }

        public StandardSpecularLoader(Material material) => Value = new(material);
    }
}
