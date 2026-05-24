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
        public ColorParser color
        {
            get => GetColor("_Color");
            set => SetColor("_Color", value);
        }

        // Albedo, default = "white" { }
        [ParserTarget("mainTex")]
        public MaterialTextureParser mainTex
        {
            get => GetTextureName("_MainTex");
            set => SetTexture("_MainTex", value);
        }

        [ParserTarget("mainTexScale")]
        public Vector2Parser mainTexScale
        {
            get => GetTextureScale("_MainTex");
            set => SetTextureScale("_MainTex", value);
        }

        [ParserTarget("mainTexOffset")]
        public Vector2Parser mainTexOffset
        {
            get => GetTextureOffset("_MainTex");
            set => SetTextureOffset("_MainTex", value);
        }

        // Alpha Cutoff, default = 0.500000
        [ParserTarget("cutoff")]
        public NumericParser<float> cutoff
        {
            get => GetFloat("_Cutoff");
            set => SetFloat("_Cutoff", value);
        }

        // Smoothness, default = 0.500000
        [ParserTarget("glossiness")]
        public NumericParser<float> glossiness
        {
            get => GetFloat("_Glossiness");
            set => SetFloat("_Glossiness", value);
        }

        // Smoothness Scale, default = 1.000000
        [ParserTarget("glossMapScale")]
        public NumericParser<float> glossMapScale
        {
            get => GetFloat("_GlossMapScale");
            set => SetFloat("_GlossMapScale", value);
        }

        // Smoothness texture channel, default = 0.000000
        [ParserTarget("smoothnessTextureChannel")]
        public NumericParser<float> smoothnessTextureChannel
        {
            get => GetFloat("_SmoothnessTextureChannel");
            set => SetFloat("_SmoothnessTextureChannel", value);
        }

        // Specular, default = (0.2,0.2,0.2,1)
        [ParserTarget("specColor")]
        public ColorParser specColor
        {
            get => GetColor("_SpecColor");
            set => SetColor("_SpecColor", value);
        }

        // Specular, default = "white" { }
        [ParserTarget("specGlossMap")]
        public MaterialTextureParser specGlossMap
        {
            get => GetTextureName("_SpecGlossMap");
            set => SetTexture("_SpecGlossMap", value);
        }

        [ParserTarget("metallicGlossMapScale")]
        public Vector2Parser metallicGlossMapScale
        {
            get => GetTextureScale("_SpecGlossMap");
            set => SetTextureScale("_SpecGlossMap", value);
        }

        [ParserTarget("metallicGlossMapOffset")]
        public Vector2Parser metallicGlossMapOffset
        {
            get => GetTextureOffset("_SpecGlossMap");
            set => SetTextureOffset("_SpecGlossMap", value);
        }

        // Specular Highlights, default = 1.000000
        [ParserTarget("specularHighlights")]
        public NumericParser<float> specularHighlights
        {
            get => GetFloat("_SpecularHighlights");
            set => SetFloat("_SpecularHighlights", value);
        }

        // Glossy Reflections, default = 1.000000
        [ParserTarget("glossyReflections")]
        public NumericParser<float> glossyReflections
        {
            get => GetFloat("_GlossyReflections");
            set => SetFloat("_GlossyReflections", value);
        }

        // Scale, default = 1.000000
        [ParserTarget("bumpScale")]
        public NumericParser<float> bumpScale
        {
            get => GetFloat("_BumpScale");
            set => SetFloat("_BumpScale", value);
        }

        // Normal Map, default = "bump" { }
        [ParserTarget("bumpMap")]
        public MaterialTextureParser bumpMap
        {
            get => GetTextureName("_BumpMap");
            set => SetTexture("_BumpMap", value);
        }

        [ParserTarget("bumpMapScale")]
        public Vector2Parser bumpMapScale
        {
            get => GetTextureScale("_BumpMap");
            set => SetTextureScale("_BumpMap", value);
        }

        [ParserTarget("bumpMapOffset")]
        public Vector2Parser bumpMapOffset
        {
            get => GetTextureOffset("_BumpMap");
            set => SetTextureOffset("_BumpMap", value);
        }

        // Height Scale, default = 0.020000
        [ParserTarget("parallax")]
        public NumericParser<float> parallax
        {
            get => GetFloat("_Parallax");
            set => SetFloat("_Parallax", value);
        }

        // Height Map, default = "black" { }
        [ParserTarget("parallaxMap")]
        public MaterialTextureParser parallaxMap
        {
            get => GetTextureName("_ParallaxMap");
            set => SetTexture("_ParallaxMap", value);
        }

        [ParserTarget("parallaxMapScale")]
        public Vector2Parser parallaxMapScale
        {
            get => GetTextureScale("_ParallaxMap");
            set => SetTextureScale("_ParallaxMap", value);
        }

        [ParserTarget("parallaxMapOffset")]
        public Vector2Parser parallaxMapOffset
        {
            get => GetTextureOffset("_ParallaxMap");
            set => SetTextureOffset("_ParallaxMap", value);
        }

        // Strength, default = 1.000000
        [ParserTarget("occlusionStrength")]
        public NumericParser<float> occlusionStrength
        {
            get => GetFloat("_OcclusionStrength");
            set => SetFloat("_OcclusionStrength", value);
        }

        // Occlusion, default = "white" { }
        [ParserTarget("occlusionMap")]
        public MaterialTextureParser occlusionMap
        {
            get => GetTextureName("_OcclusionMap");
            set => SetTexture("_OcclusionMap", value);
        }

        [ParserTarget("occlusionMapScale")]
        public Vector2Parser occlusionMapScale
        {
            get => GetTextureScale("_OcclusionMap");
            set => SetTextureScale("_OcclusionMap", value);
        }

        [ParserTarget("occlusionMapOffset")]
        public Vector2Parser occlusionMapOffset
        {
            get => GetTextureOffset("_OcclusionMap");
            set => SetTextureOffset("_OcclusionMap", value);
        }

        // Color, default = (0.000000,0.000000,0.000000,1.000000)
        [ParserTarget("emissionColor")]
        public ColorParser emissionColor
        {
            get => GetColor("_EmissionColor");
            set => SetColor("_EmissionColor", value);
        }

        // Emission, default = "white" { }
        [ParserTarget("emissionMap")]
        public MaterialTextureParser emissionMap
        {
            get => GetTextureName("_EmissionMap");
            set => SetTexture("_EmissionMap", value);
        }

        [ParserTarget("emissionMapScale")]
        public Vector2Parser emissionMapScale
        {
            get => GetTextureScale("_EmissionMap");
            set => SetTextureScale("_EmissionMap", value);
        }

        [ParserTarget("emissionMapOffset")]
        public Vector2Parser emissionMapOffset
        {
            get => GetTextureOffset("_EmissionMap");
            set => SetTextureOffset("_EmissionMap", value);
        }

        // Detail Mask, default = "white" { }
        [ParserTarget("detailMask")]
        public MaterialTextureParser detailMask
        {
            get => GetTextureName("_DetailMask");
            set => SetTexture("_DetailMask", value);
        }

        [ParserTarget("detailMaskScale")]
        public Vector2Parser detailMaskScale
        {
            get => GetTextureScale("_DetailMask");
            set => SetTextureScale("_DetailMask", value);
        }

        [ParserTarget("detailMaskOffset")]
        public Vector2Parser detailMaskOffset
        {
            get => GetTextureOffset("_DetailMask");
            set => SetTextureOffset("_DetailMask", value);
        }

        // Detail Albedo x2, default = "grey" { }
        [ParserTarget("detailAlbedoMap")]
        public MaterialTextureParser detailAlbedoMap
        {
            get => GetTextureName("_DetailAlbedoMap");
            set => SetTexture("_DetailAlbedoMap", value);
        }

        [ParserTarget("detailAlbedoMapScale")]
        public Vector2Parser detailAlbedoMapScale
        {
            get => GetTextureScale("_DetailAlbedoMap");
            set => SetTextureScale("_DetailAlbedoMap", value);
        }

        [ParserTarget("detailAlbedoMapOffset")]
        public Vector2Parser detailAlbedoMapOffset
        {
            get => GetTextureOffset("_DetailAlbedoMap");
            set => SetTextureOffset("_DetailAlbedoMap", value);
        }

        // Normal Map, default = "bump" { }
        [ParserTarget("detailNormalMap")]
        public MaterialTextureParser detailNormalMap
        {
            get => GetTextureName("_DetailNormalMap");
            set => SetTexture("_DetailNormalMap", value);
        }

        // Scale, default = 1.000000
        [ParserTarget("detailNormalMapScale")]
        public Vector2Parser detailNormalMapScale
        {
            get => GetTextureScale("_DetailNormalMap");
            set => SetTextureScale("_DetailNormalMap", value);
        }

        [ParserTarget("detailNormalMapOffset")]
        public Vector2Parser detailNormalMapOffset
        {
            get => GetTextureOffset("_DetailNormalMap");
            set => SetTextureOffset("_DetailNormalMap", value);
        }

        // UV Set for secondary textures, default = 0.000000
        [ParserTarget("UVSec")]
        public NumericParser<float> UVSec
        {
            get => GetFloat("_UVSec");
            set => SetFloat("_UVSec", value);
        }

        // __mode, default = 0.000000
        [ParserTarget("mode")]
        public NumericParser<float> mode
        {
            get => GetFloat("_Mode");
            set => SetFloat("_Mode", value);
        }

        // __src, default = 1.000000
        [ParserTarget("srcBlend")]
        public NumericParser<float> srcBlend
        {
            get => GetFloat("_SrcBlend");
            set => SetFloat("_SrcBlend", value);
        }

        // __dst, default = 0.000000
        [ParserTarget("dstBlend")]
        public NumericParser<float> dstBlend
        {
            get => GetFloat("_DstBlend");
            set => SetFloat("_DstBlend", value);
        }

        // __zw, default = 1.000000
        [ParserTarget("ZWrite")]
        public NumericParser<float> ZWrite
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
