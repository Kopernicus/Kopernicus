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
    public class StandardSpecularLoader : StandardSpecular
    {
        // Color, default = (1.000000,1.000000,1.000000,1.000000)
        [ParserTarget("color")]
        public ColorParser colorSetter
        {
            get { return Color; }
            set { Color = value; }
        }

        // Albedo, default = "white" { }
        [ParserTarget("mainTex")]
        public Texture2DParser mainTexSetter
        {
            get { return MainTex; }
            set { MainTex = value; }
        }

        [ParserTarget("mainTexScale")]
        public Vector2Parser mainTexScaleSetter
        {
            get { return MainTexScale; }
            set { MainTexScale = value; }
        }

        [ParserTarget("mainTexOffset")]
        public Vector2Parser mainTexOffsetSetter
        {
            get { return MainTexOffset; }
            set { MainTexOffset = value; }
        }

        // Alpha Cutoff, default = 0.500000
        [ParserTarget("cutoff")]
        public NumericParser<Single> cutoffSetter
        {
            get { return Cutoff; }
            set { Cutoff = value; }
        }

        // Smoothness, default = 0.500000
        [ParserTarget("glossiness")]
        public NumericParser<Single> glossinessSetter
        {
            get { return Glossiness; }
            set { Glossiness = value; }
        }

        // Smoothness Scale, default = 1.000000
        [ParserTarget("glossMapScale")]
        public NumericParser<Single> glossMapScaleSetter
        {
            get { return GlossMapScale; }
            set { GlossMapScale = value; }
        }

        // Smoothness texture channel, default = 0.000000
        [ParserTarget("smoothnessTextureChannel")]
        public EnumParser<TextureChannel> smoothnessTextureChannelSetter
        {
            get { return SmoothnessTextureChannel; }
            set { SmoothnessTextureChannel = value; }
        }

        // Specular, default = (0.2,0.2,0.2,1)
        [ParserTarget("specColor")]
        public ColorParser specColorSetter
        {
            get { return SpecColor; }
            set { SpecColor = value; }
        }

        // Specular, default = "white" { }
        [ParserTarget("specGlossMap")]
        public Texture2DParser specGlossMapSetter
        {
            get { return SpecGlossMap; }
            set { SpecGlossMap = value; }
        }

        [ParserTarget("metallicGlossMapScale")]
        public Vector2Parser metallicGlossMapScaleSetter
        {
            get { return MetallicGlossMapScale; }
            set { MetallicGlossMapScale = value; }
        }

        [ParserTarget("metallicGlossMapOffset")]
        public Vector2Parser metallicGlossMapOffsetSetter
        {
            get { return MetallicGlossMapOffset; }
            set { MetallicGlossMapOffset = value; }
        }

        // Specular Highlights, default = 1.000000
        [ParserTarget("specularHighlights")]
        public NumericParser<Boolean> specularHighlightsSetter
        {
            get { return SpecularHighlights; }
            set { SpecularHighlights = value; }
        }

        // Glossy Reflections, default = 1.000000
        [ParserTarget("glossyReflections")]
        public NumericParser<Boolean> glossyReflectionsSetter
        {
            get { return GlossyReflections; }
            set { GlossyReflections = value; }
        }

        // Scale, default = 1.000000
        [ParserTarget("bumpScale")]
        public NumericParser<Single> bumpScaleSetter
        {
            get { return BumpScale; }
            set { BumpScale = value; }
        }

        // Normal Map, default = "bump" { }
        [ParserTarget("bumpMap")]
        public Texture2DParser bumpMapSetter
        {
            get { return BumpMap; }
            set { BumpMap = value; }
        }

        [ParserTarget("bumpMapScale")]
        public Vector2Parser bumpMapScaleSetter
        {
            get { return BumpMapScale; }
            set { BumpMapScale = value; }
        }

        [ParserTarget("bumpMapOffset")]
        public Vector2Parser bumpMapOffsetSetter
        {
            get { return BumpMapOffset; }
            set { BumpMapOffset = value; }
        }

        // Height Scale, default = 0.020000
        [ParserTarget("parallax")]
        public NumericParser<Single> parallaxSetter
        {
            get { return Parallax; }
            set { Parallax = value; }
        }

        // Height Map, default = "black" { }
        [ParserTarget("parallaxMap")]
        public Texture2DParser parallaxMapSetter
        {
            get { return ParallaxMap; }
            set { ParallaxMap = value; }
        }

        [ParserTarget("parallaxMapScale")]
        public Vector2Parser parallaxMapScaleSetter
        {
            get { return ParallaxMapScale; }
            set { ParallaxMapScale = value; }
        }

        [ParserTarget("parallaxMapOffset")]
        public Vector2Parser parallaxMapOffsetSetter
        {
            get { return ParallaxMapOffset; }
            set { ParallaxMapOffset = value; }
        }

        // Strength, default = 1.000000
        [ParserTarget("occlusionStrength")]
        public NumericParser<Single> occlusionStrengthSetter
        {
            get { return OcclusionStrength; }
            set { OcclusionStrength = value; }
        }

        // Occlusion, default = "white" { }
        [ParserTarget("occlusionMap")]
        public Texture2DParser occlusionMapSetter
        {
            get { return OcclusionMap; }
            set { OcclusionMap = value; }
        }

        [ParserTarget("occlusionMapScale")]
        public Vector2Parser occlusionMapScaleSetter
        {
            get { return OcclusionMapScale; }
            set { OcclusionMapScale = value; }
        }

        [ParserTarget("occlusionMapOffset")]
        public Vector2Parser occlusionMapOffsetSetter
        {
            get { return OcclusionMapOffset; }
            set { OcclusionMapOffset = value; }
        }

        // Color, default = (0.000000,0.000000,0.000000,1.000000)
        [ParserTarget("emissionColor")]
        public ColorParser emissionColorSetter
        {
            get { return EmissionColor; }
            set { EmissionColor = value; }
        }

        // Emission, default = "white" { }
        [ParserTarget("emissionMap")]
        public Texture2DParser emissionMapSetter
        {
            get { return EmissionMap; }
            set { EmissionMap = value; }
        }

        [ParserTarget("emissionMapScale")]
        public Vector2Parser emissionMapScaleSetter
        {
            get { return EmissionMapScale; }
            set { EmissionMapScale = value; }
        }

        [ParserTarget("emissionMapOffset")]
        public Vector2Parser emissionMapOffsetSetter
        {
            get { return EmissionMapOffset; }
            set { EmissionMapOffset = value; }
        }

        // Detail Mask, default = "white" { }
        [ParserTarget("detailMask")]
        public Texture2DParser detailMaskSetter
        {
            get { return DetailMask; }
            set { DetailMask = value; }
        }

        [ParserTarget("detailMaskScale")]
        public Vector2Parser detailMaskScaleSetter
        {
            get { return DetailMaskScale; }
            set { DetailMaskScale = value; }
        }

        [ParserTarget("detailMaskOffset")]
        public Vector2Parser detailMaskOffsetSetter
        {
            get { return DetailMaskOffset; }
            set { DetailMaskOffset = value; }
        }

        // Detail Albedo x2, default = "grey" { }
        [ParserTarget("detailAlbedoMap")]
        public Texture2DParser detailAlbedoMapSetter
        {
            get { return DetailAlbedoMap; }
            set { DetailAlbedoMap = value; }
        }

        [ParserTarget("detailAlbedoMapScale")]
        public Vector2Parser detailAlbedoMapScaleSetter
        {
            get { return DetailAlbedoMapScale; }
            set { DetailAlbedoMapScale = value; }
        }

        [ParserTarget("detailAlbedoMapOffset")]
        public Vector2Parser detailAlbedoMapOffsetSetter
        {
            get { return DetailAlbedoMapOffset; }
            set { DetailAlbedoMapOffset = value; }
        }

        // Normal Map, default = "bump" { }
        [ParserTarget("detailNormalMap")]
        public Texture2DParser detailNormalMapSetter
        {
            get { return DetailNormalMap; }
            set { DetailNormalMap = value; }
        }

        // Scale, default = 1.000000
        [ParserTarget("detailNormalMapScale")]
        public Vector2Parser detailNormalMapScaleSetter
        {
            get { return DetailNormalMapScale; }
            set { DetailNormalMapScale = value; }
        }

        [ParserTarget("detailNormalMapOffset")]
        public Vector2Parser detailNormalMapOffsetSetter
        {
            get { return DetailNormalMapOffset; }
            set { DetailNormalMapOffset = value; }
        }

        // UV Set for secondary textures, default = 0.000000
        [ParserTarget("UVSec")]
        public EnumParser<UvSet> UVSecSetter
        {
            get { return UvSec; }
            set { UvSec = value; }
        }

        // __mode, default = 0.000000
        [ParserTarget("mode")]
        public EnumParser<BlendMode> modeSetter
        {
            get { return Mode; }
            set { Mode = value; }
        }

        // __src, default = 1.000000
        [ParserTarget("srcBlend")]
        public NumericParser<Single> srcBlendSetter
        {
            get { return SrcBlend; }
            set { SrcBlend = value; }
        }

        // __dst, default = 0.000000
        [ParserTarget("dstBlend")]
        public NumericParser<Single> dstBlendSetter
        {
            get { return DstBlend; }
            set { DstBlend = value; }
        }

        // __zw, default = 1.000000
        [ParserTarget("ZWrite")]
        public NumericParser<Single> ZWriteSetter
        {
            get { return ZWrite; }
            set { ZWrite = value; }
        }

        // Constructors
        public StandardSpecularLoader()
        {
        }

        [Obsolete("Creating materials from shader source String is no longer supported. Use Shader assets instead.")]
        public StandardSpecularLoader(String contents) : base(contents)
        {
        }

        public StandardSpecularLoader(Material material) : base(material)
        {
        }
    }
}
