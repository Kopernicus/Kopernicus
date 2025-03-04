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
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [RequireConfigType(ConfigType.Node)]
    public class StandardLoader : Standard
    {
        // Color, default = (1.000000,1.000000,1.000000,1.000000)
        [ParserTarget("color", Optional = true)]
        public ColorParser colorSetter
        {
            get { return Color; }
            set { Color = value; }
        }

        // Albedo, default = "white" { }
        [ParserTarget("mainTex", Optional = true)]
        public Texture2DParser mainTexSetter
        {
            get { return MainTex; }
            set { MainTex = value; }
        }

        [ParserTarget("mainTexScale", Optional = true)]
        public Vector2Parser mainTexScaleSetter
        {
            get { return MainTexScale; }
            set { MainTexScale = value; }
        }

        [ParserTarget("mainTexOffset", Optional = true)]
        public Vector2Parser mainTexOffsetSetter
        {
            get { return MainTexOffset; }
            set { MainTexOffset = value; }
        }

        // Alpha Cutoff, default = 0.500000
        [ParserTarget("cutoff", Optional = true)]
        public NumericParser<Single> cutoffSetter
        {
            get { return Cutoff; }
            set { Cutoff = value; }
        }

        // Smoothness, default = 0.500000
        [ParserTarget("glossiness", Optional = true)]
        public NumericParser<Single> glossinessSetter
        {
            get { return Glossiness; }
            set { Glossiness = value; }
        }

        // Smoothness Scale, default = 1.000000
        [ParserTarget("glossMapScale", Optional = true)]
        public NumericParser<Single> glossMapScaleSetter
        {
            get { return GlossMapScale; }
            set { GlossMapScale = value; }
        }

        // Smoothness texture channel, default = 0.000000
        [ParserTarget("smoothnessTextureChannel", Optional = true)]
        public EnumParser<TextureChannel> smoothnessTextureChannelSetter
        {
            get { return SmoothnessTextureChannel; }
            set { SmoothnessTextureChannel = value; }
        }

        // Metallic, default = 0.000000
        [ParserTarget("metallic", Optional = true)]
        public NumericParser<Single> metallicSetter
        {
            get { return Metallic; }
            set { Metallic = value; }
        }

        // Metallic, default = "white" { }
        [ParserTarget("metallicGlossMap", Optional = true)]
        public Texture2DParser metallicGlossMapSetter
        {
            get { return MetallicGlossMap; }
            set { MetallicGlossMap = value; }
        }

        [ParserTarget("metallicGlossMapScale", Optional = true)]
        public Vector2Parser metallicGlossMapScaleSetter
        {
            get { return MetallicGlossMapScale; }
            set { MetallicGlossMapScale = value; }
        }

        [ParserTarget("metallicGlossMapOffset", Optional = true)]
        public Vector2Parser metallicGlossMapOffsetSetter
        {
            get { return MetallicGlossMapOffset; }
            set { MetallicGlossMapOffset = value; }
        }

        // Specular Highlights, default = 1.000000
        [ParserTarget("specularHighlights", Optional = true)]
        public NumericParser<Boolean> specularHighlightsSetter
        {
            get { return SpecularHighlights; }
            set { SpecularHighlights = value; }
        }

        // Glossy Reflections, default = 1.000000
        [ParserTarget("glossyReflections", Optional = true)]
        public NumericParser<Boolean> glossyReflectionsSetter
        {
            get { return GlossyReflections; }
            set { GlossyReflections = value; }
        }

        // Scale, default = 1.000000
        [ParserTarget("bumpScale", Optional = true)]
        public NumericParser<Single> bumpScaleSetter
        {
            get { return BumpScale; }
            set { BumpScale = value; }
        }

        // Normal Map, default = "bump" { }
        [ParserTarget("bumpMap", Optional = true)]
        public Texture2DParser bumpMapSetter
        {
            get { return BumpMap; }
            set { BumpMap = value; }
        }

        [ParserTarget("bumpMapScale", Optional = true)]
        public Vector2Parser bumpMapScaleSetter
        {
            get { return BumpMapScale; }
            set { BumpMapScale = value; }
        }

        [ParserTarget("bumpMapOffset", Optional = true)]
        public Vector2Parser bumpMapOffsetSetter
        {
            get { return BumpMapOffset; }
            set { BumpMapOffset = value; }
        }

        // Height Scale, default = 0.020000
        [ParserTarget("parallax", Optional = true)]
        public NumericParser<Single> parallaxSetter
        {
            get { return Parallax; }
            set { Parallax = value; }
        }

        // Height Map, default = "black" { }
        [ParserTarget("parallaxMap", Optional = true)]
        public Texture2DParser parallaxMapSetter
        {
            get { return ParallaxMap; }
            set { ParallaxMap = value; }
        }

        [ParserTarget("parallaxMapScale", Optional = true)]
        public Vector2Parser parallaxMapScaleSetter
        {
            get { return ParallaxMapScale; }
            set { ParallaxMapScale = value; }
        }

        [ParserTarget("parallaxMapOffset", Optional = true)]
        public Vector2Parser parallaxMapOffsetSetter
        {
            get { return ParallaxMapOffset; }
            set { ParallaxMapOffset = value; }
        }

        // Strength, default = 1.000000
        [ParserTarget("occlusionStrength", Optional = true)]
        public NumericParser<Single> occlusionStrengthSetter
        {
            get { return OcclusionStrength; }
            set { OcclusionStrength = value; }
        }

        // Occlusion, default = "white" { }
        [ParserTarget("occlusionMap", Optional = true)]
        public Texture2DParser occlusionMapSetter
        {
            get { return OcclusionMap; }
            set { OcclusionMap = value; }
        }

        [ParserTarget("occlusionMapScale", Optional = true)]
        public Vector2Parser occlusionMapScaleSetter
        {
            get { return OcclusionMapScale; }
            set { OcclusionMapScale = value; }
        }

        [ParserTarget("occlusionMapOffset", Optional = true)]
        public Vector2Parser occlusionMapOffsetSetter
        {
            get { return OcclusionMapOffset; }
            set { OcclusionMapOffset = value; }
        }

        // Color, default = (0.000000,0.000000,0.000000,1.000000)
        [ParserTarget("emissionColor", Optional = true)]
        public ColorParser emissionColorSetter
        {
            get { return EmissionColor; }
            set { EmissionColor = value; }
        }

        // Emission, default = "white" { }
        [ParserTarget("emissionMap", Optional = true)]
        public Texture2DParser emissionMapSetter
        {
            get { return EmissionMap; }
            set { EmissionMap = value; }
        }

        [ParserTarget("emissionMapScale", Optional = true)]
        public Vector2Parser emissionMapScaleSetter
        {
            get { return EmissionMapScale; }
            set { EmissionMapScale = value; }
        }

        [ParserTarget("emissionMapOffset", Optional = true)]
        public Vector2Parser emissionMapOffsetSetter
        {
            get { return EmissionMapOffset; }
            set { EmissionMapOffset = value; }
        }

        // Detail Mask, default = "white" { }
        [ParserTarget("detailMask", Optional = true)]
        public Texture2DParser detailMaskSetter
        {
            get { return DetailMask; }
            set { DetailMask = value; }
        }

        [ParserTarget("detailMaskScale", Optional = true)]
        public Vector2Parser detailMaskScaleSetter
        {
            get { return DetailMaskScale; }
            set { DetailMaskScale = value; }
        }

        [ParserTarget("detailMaskOffset", Optional = true)]
        public Vector2Parser detailMaskOffsetSetter
        {
            get { return DetailMaskOffset; }
            set { DetailMaskOffset = value; }
        }

        // Detail Albedo x2, default = "grey" { }
        [ParserTarget("detailAlbedoMap", Optional = true)]
        public Texture2DParser detailAlbedoMapSetter
        {
            get { return DetailAlbedoMap; }
            set { DetailAlbedoMap = value; }
        }

        [ParserTarget("detailAlbedoMapScale", Optional = true)]
        public Vector2Parser detailAlbedoMapScaleSetter
        {
            get { return DetailAlbedoMapScale; }
            set { DetailAlbedoMapScale = value; }
        }

        [ParserTarget("detailAlbedoMapOffset", Optional = true)]
        public Vector2Parser detailAlbedoMapOffsetSetter
        {
            get { return DetailAlbedoMapOffset; }
            set { DetailAlbedoMapOffset = value; }
        }

        // Normal Map, default = "bump" { }
        [ParserTarget("detailNormalMap", Optional = true)]
        public Texture2DParser detailNormalMapSetter
        {
            get { return DetailNormalMap; }
            set { DetailNormalMap = value; }
        }

        // Scale, default = 1.000000
        [ParserTarget("detailNormalMapScale", Optional = true)]
        public Vector2Parser detailNormalMapScaleSetter
        {
            get { return DetailNormalMapScale; }
            set { DetailNormalMapScale = value; }
        }

        [ParserTarget("detailNormalMapOffset", Optional = true)]
        public Vector2Parser detailNormalMapOffsetSetter
        {
            get { return DetailNormalMapOffset; }
            set { DetailNormalMapOffset = value; }
        }

        // UV Set for secondary textures, default = 0.000000
        [ParserTarget("UVSec", Optional = true)]
        public EnumParser<UvSet> UVSecSetter
        {
            get { return UvSec; }
            set { UvSec = value; }
        }

        // __mode, default = 0.000000
        [ParserTarget("mode", Optional = true)]
        public EnumParser<BlendMode> modeSetter
        {
            get { return Mode; }
            set { Mode = value; }
        }

        // __src, default = 1.000000
        [ParserTarget("srcBlend", Optional = true)]
        public NumericParser<Single> srcBlendSetter
        {
            get { return SrcBlend; }
            set { SrcBlend = value; }
        }

        // __dst, default = 0.000000
        [ParserTarget("dstBlend", Optional = true)]
        public NumericParser<Single> dstBlendSetter
        {
            get { return DstBlend; }
            set { DstBlend = value; }
        }

        // __zw, default = 1.000000
        [ParserTarget("ZWrite", Optional = true)]
        public NumericParser<Single> ZWriteSetter
        {
            get { return ZWrite; }
            set { ZWrite = value; }
        }

        // Constructors
        public StandardLoader()
        {
        }

        [Obsolete("Creating materials from shader source String is no longer supported. Use Shader assets instead.")]
        public StandardLoader(String contents) : base(contents)
        {
        }

        public StandardLoader(Material material) : base(material)
        {
        }
    }
}
