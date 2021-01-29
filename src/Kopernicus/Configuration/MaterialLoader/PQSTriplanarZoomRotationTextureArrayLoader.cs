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
#if (KSP_VERSION_1_9_1 || KSP_VERSION_1_10_1 || KSP_VERSION_1_11_1)
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kopernicus.Components.MaterialWrapper;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Parsing;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus.Configuration.MaterialLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PQSTriplanarZoomRotationTextureArrayLoader : PQSTriplanarZoomRotationTextureArray
    {
        // Color Lerp Modifier, default = 1
        [ParserTarget("colorLerpModifier")]
        public NumericParser<Single> ColorLerpModifierSetter
        {
            get { return ColorLerpModifier; }
            set { ColorLerpModifier = value; }
        }
#if (KSP_VERSION_1_9_1 || KSP_VERSION_1_10_1 || KSP_VERSION_1_11_1)
        // Atlas Texture, default = 100000
        [ParserTarget("atlasTiling")]
        public NumericParser<Single> AtlasTilingSetter
        {
            get { return AtlasTiling; }
            set { AtlasTiling = value; }
        }

        // Atlas Texture, default = "white" { }
        [ParserTargetCollection("AtlasTex")]
        [KittopiaHideOption]
        public List<Texture2DParser> AtlasTexSetter
        {
            // TODO: Figure out if it is even possible to get the names out of a Texture2DArray
            get { return new List<Texture2DParser>(); }
            set
            {
                if (value.Count == 0)
                {
                    return;
                }

                List<Texture2D> textures = value.Select(t => Utility.CreateReadable(t.Value)).ToList();
                AtlasTex = KSPUtil.GenerateTexture2DArray(textures, TextureFormat.RGBA32, true);
            }
        }

        // Normal Atlas Texture, default = "white" { }
        [ParserTargetCollection("NormalTex")]
        [KittopiaHideOption]
        public List<Texture2DParser> NormalTexSetter
        {
            // TODO: Figure out if it is even possible to get the names out of a Texture2DArray
            get { return new List<Texture2DParser>(); }
            set
            {
                if (value.Count == 0)
                {
                    return;
                }

                List<Texture2D> textures = value.Select(t => Utility.CreateReadable(t.Value)).ToList();
                NormalTex = KSPUtil.GenerateTexture2DArray(textures, TextureFormat.RGBA32, true);
            }
        }
#endif
        // Factor, default = 10
        [ParserTarget("factor")]
        public NumericParser<Single> FactorSetter
        {
            get { return Factor; }
            set { Factor = value; }
        }

        // Factor Blend Width, default = 0.1
        [ParserTarget("factorBlendWidth")]
        public NumericParser<Single> FactorBlendWidthSetter
        {
            get { return FactorBlendWidth; }
            set { FactorBlendWidth = value; }
        }

        // Factor Rotation, default = 30
        [ParserTarget("factorRotation")]
        public NumericParser<Single> FactorRotationSetter
        {
            get { return FactorRotation; }
            set { FactorRotation = value; }
        }

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

        // Specular Color, default = (0.2,0.2,0.2,0.2)
        [ParserTarget("specularColor")]
        public ColorParser SpecularColorSetter
        {
            get { return SpecularColor; }
            set { SpecularColor = value; }
        }

        // Brightness, default = 2
        [ParserTarget("albedoBrightness")]
        public NumericParser<Single> AlbedoBrightnessSetter
        {
            get { return AlbedoBrightness; }
            set { AlbedoBrightness = value; }
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

        // Steep Bump Map, default = "bump" { }
        [ParserTarget("steepBumpMap")]
        public Texture2DParser SteepBumpMapSetter
        {
            get { return SteepBumpMap; }
            set { SteepBumpMap = value; }
        }

        [ParserTarget("steepBumpMapScale")]
        public Vector2Parser SteepBumpMapScaleSetter
        {
            get { return SteepBumpMapScale; }
            set { SteepBumpMapScale = value; }
        }

        [ParserTarget("steepBumpMapOffset")]
        public Vector2Parser SteepBumpMapOffsetSetter
        {
            get { return SteepBumpMapOffset; }
            set { SteepBumpMapOffset = value; }
        }

        // Steep Near Tiling, default = 1
        [ParserTarget("steepNearTiling")]
        public NumericParser<Single> SteepNearTilingSetter
        {
            get { return SteepNearTiling; }
            set { SteepNearTiling = value; }
        }

        // Steep Far Tiling, default = 1
        [ParserTarget("steepTiling")]
        public NumericParser<Single> SteepTilingSetter
        {
            get { return SteepTiling; }
            set { SteepTiling = value; }
        }

        // AP Global Density, default = 1
        [ParserTarget("globalDensity")]
        public NumericParser<Single> GlobalDensitySetter
        {
            get { return GlobalDensity; }
            set { GlobalDensity = value; }
        }

        // FogColorRamp, default = "white" { }
        [ParserTarget("fogColorRamp")]
        public Texture2DParser FogColorRampSetter
        {
            get { return FogColorRamp; }
            set { FogColorRamp = value; }
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

        // Ocean Fog Dist, default = 1000
        [ParserTarget("oceanFogDistance")]
        public NumericParser<Single> OceanFogDistanceSetter
        {
            get { return OceanFogDistance; }
            set { OceanFogDistance = value; }
        }

        // Constructors
        public PQSTriplanarZoomRotationTextureArrayLoader()
        {
        }

        [Obsolete("Creating materials from shader source String is no longer supported. Use Shader assets instead.")]
        public PQSTriplanarZoomRotationTextureArrayLoader(String contents) : base(contents)
        {
        }

        public PQSTriplanarZoomRotationTextureArrayLoader(Material material) : base(material)
        {
        }
    }
}
#endif