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
using System.Collections.Generic;
using System.Linq;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Attributes;
using Kopernicus.Configuration.MaterialLoader.Parsing;
using Kopernicus.Configuration.Parsing;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus.Configuration.MaterialLoader
{
    [RequireConfigType(ConfigType.Node)]
    [MaterialLoader(PQSTriplanarZoomRotationTextureArrayLoader.SHADER_NAME)]
    public class PQSTriplanarZoomRotationTextureArrayLoader : MaterialLoader
    {
        public const String SHADER_NAME = "Terrain/PQS/PQS Triplanar Zoom Rotation Texture Array";
        private static readonly Shader Shader = Shader.Find(SHADER_NAME);

        // The texture-atlas mod (see ModLoader/TextureAtlas.cs) hot-swaps the shader to one of
        // the "- N Blend" permutations at runtime. They share the same property layout as the
        // base shader, so they all map to this loader/wrapper.
        public static bool UsesSameShader(Material m)
        {
            if (m == null) return false;
            String n = m.shader.name;
            return n == SHADER_NAME
                || n == SHADER_NAME + " - 1 Blend"
                || n == SHADER_NAME + " - 2 Blend"
                || n == SHADER_NAME + " - 3 Blend"
                || n == SHADER_NAME + " - 4 Blend";
        }

        // Color Lerp Modifier, default = 1
        [ParserTarget("colorLerpModifier")]
        public NumericParser<float> ColorLerpModifierSetter
        {
            get => GetFloat("_ColorLerpModifier");
            set => SetFloat("_ColorLerpModifier", value);
        }

        // Atlas Texture, default = 100000
        [ParserTarget("atlasTiling")]
        public NumericParser<float> AtlasTilingSetter
        {
            get => GetFloat("_AtlasTiling");
            set => SetFloat("_AtlasTiling", value);
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
                Value.SetTexture("_AtlasTex", KSPUtil.GenerateTexture2DArray(textures, TextureFormat.RGBA32, true));
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
                Value.SetTexture("_NormalTex", KSPUtil.GenerateTexture2DArray(textures, TextureFormat.RGBA32, true));
            }
        }

        // Factor, default = 10
        [ParserTarget("factor")]
        public NumericParser<float> FactorSetter
        {
            get => GetFloat("_factor");
            set => SetFloat("_factor", value);
        }

        // Factor Blend Width, default = 0.1
        [ParserTarget("factorBlendWidth")]
        public NumericParser<float> FactorBlendWidthSetter
        {
            get => GetFloat("_factorBlendWidth");
            set => SetFloat("_factorBlendWidth", value);
        }

        // Factor Rotation, default = 30
        [ParserTarget("factorRotation")]
        public NumericParser<float> FactorRotationSetter
        {
            get => GetFloat("_factorRotation");
            set => SetFloat("_factorRotation", value);
        }

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

        // Specular Color, default = (0.2,0.2,0.2,0.2)
        [ParserTarget("specularColor")]
        public ColorParser SpecularColorSetter
        {
            get => GetColor("_specularColor");
            set => SetColor("_specularColor", value);
        }

        // Brightness, default = 2
        [ParserTarget("albedoBrightness")]
        public NumericParser<float> AlbedoBrightnessSetter
        {
            get => GetFloat("_albedoBrightness");
            set => SetFloat("_albedoBrightness", value);
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
        public PQSTriplanarZoomRotationTextureArrayLoader() { }

        public PQSTriplanarZoomRotationTextureArrayLoader(Material material) => Value = new(material);
    }
}
