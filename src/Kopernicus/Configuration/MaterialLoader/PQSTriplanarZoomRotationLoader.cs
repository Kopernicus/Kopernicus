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
    [MaterialLoader(PQSTriplanarZoomRotationLoader.SHADER_NAME)]
    public class PQSTriplanarZoomRotationLoader : PQSMaterialLoader
    {
        public const String SHADER_NAME = "Terrain/PQS/PQS Triplanar Zoom Rotation";
        private static readonly Shader Shader = Shader.Find(SHADER_NAME);
        public static bool UsesSameShader(Material m) => m != null && m.shader.name == SHADER_NAME;

        // Factor, default = 10
        [ParserTarget("factor")]
        public NumericParser<float> Factor
        {
            get => GetFloat("_factor");
            set => SetFloat("_factor", value);
        }

        // Factor Blend Width, default = 0.1
        [ParserTarget("factorBlendWidth")]
        public NumericParser<float> FactorBlendWidth
        {
            get => GetFloat("_factorBlendWidth");
            set => SetFloat("_factorBlendWidth", value);
        }

        // Factor Rotation, default = 30
        [ParserTarget("factorRotation")]
        public NumericParser<float> FactorRotation
        {
            get => GetFloat("_factorRotation");
            set => SetFloat("_factorRotation", value);
        }

        // Saturation, default = 1
        [ParserTarget("saturation")]
        public NumericParser<float> Saturation
        {
            get => GetFloat("_saturation");
            set => SetFloat("_saturation", value);
        }

        // Contrast, default = 1
        [ParserTarget("contrast")]
        public NumericParser<float> Contrast
        {
            get => GetFloat("_contrast");
            set => SetFloat("_contrast", value);
        }

        // Colour Unsaturation (A = Factor), default = (1,1,1,0)
        [ParserTarget("tintColor")]
        public ColorParser TintColor
        {
            get => GetColor("_tintColor");
            set => SetColor("_tintColor", value);
        }

        // Specular Color, default = (0.2,0.2,0.2,0.2)
        [ParserTarget("specularColor")]
        public ColorParser SpecularColor
        {
            get => GetColor("_specularColor");
            set => SetColor("_specularColor", value);
        }

        // Brightness, default = 2
        [ParserTarget("albedoBrightness")]
        public NumericParser<float> AlbedoBrightness
        {
            get => GetFloat("_albedoBrightness");
            set => SetFloat("_albedoBrightness", value);
        }

        // Steep Blend, default = 1
        [ParserTarget("steepPower")]
        public NumericParser<float> SteepPower
        {
            get => GetFloat("_steepPower");
            set => SetFloat("_steepPower", value);
        }

        // Steep Fade Start, default = 20000
        [ParserTarget("steepTexStart")]
        public NumericParser<float> SteepTexStart
        {
            get => GetFloat("_steepTexStart");
            set => SetFloat("_steepTexStart", value);
        }

        // Steep Fade End, default = 30000
        [ParserTarget("steepTexEnd")]
        public NumericParser<float> SteepTexEnd
        {
            get => GetFloat("_steepTexEnd");
            set => SetFloat("_steepTexEnd", value);
        }

        // Steep Texture, default = "white" { }
        [ParserTarget("steepTex")]
        public MaterialTextureParser SteepTex
        {
            get => GetTextureName("_steepTex");
            set => SetTexture("_steepTex", value);
        }

        [ParserTarget("steepTexScale")]
        public Vector2Parser SteepTexScale
        {
            get => GetTextureScale("_steepTex");
            set => SetTextureScale("_steepTex", value);
        }

        [ParserTarget("steepTexOffset")]
        public Vector2Parser SteepTexOffset
        {
            get => GetTextureOffset("_steepTex");
            set => SetTextureOffset("_steepTex", value);
        }

        // Steep Bump Map, default = "bump" { }
        [ParserTarget("steepBumpMap")]
        public MaterialTextureParser SteepBumpMap
        {
            get => GetTextureName("_steepBumpMap");
            set => SetTexture("_steepBumpMap", value);
        }

        [ParserTarget("steepBumpMapScale")]
        public Vector2Parser SteepBumpMapScale
        {
            get => GetTextureScale("_steepBumpMap");
            set => SetTextureScale("_steepBumpMap", value);
        }

        [ParserTarget("steepBumpMapOffset")]
        public Vector2Parser SteepBumpMapOffset
        {
            get => GetTextureOffset("_steepBumpMap");
            set => SetTextureOffset("_steepBumpMap", value);
        }

        // Steep Near Tiling, default = 1
        [ParserTarget("steepNearTiling")]
        public NumericParser<float> SteepNearTiling
        {
            get => GetFloat("_steepNearTiling");
            set => SetFloat("_steepNearTiling", value);
        }

        // Steep Far Tiling, default = 1
        [ParserTarget("steepTiling")]
        public NumericParser<float> SteepTiling
        {
            get => GetFloat("_steepTiling");
            set => SetFloat("_steepTiling", value);
        }

        // Low Texture, default = "white" { }
        [ParserTarget("lowTex")]
        public MaterialTextureParser LowTex
        {
            get => GetTextureName("_lowTex");
            set => SetTexture("_lowTex", value);
        }

        [ParserTarget("lowTexScale")]
        public Vector2Parser LowTexScale
        {
            get => GetTextureScale("_lowTex");
            set => SetTextureScale("_lowTex", value);
        }

        [ParserTarget("lowTexOffset")]
        public Vector2Parser LowTexOffset
        {
            get => GetTextureOffset("_lowTex");
            set => SetTextureOffset("_lowTex", value);
        }

        // Low Tiling, default = 100000
        [ParserTarget("lowTiling")]
        public NumericParser<float> LowTiling
        {
            get => GetFloat("_lowTiling");
            set => SetFloat("_lowTiling", value);
        }

        // Mid Texture, default = "white" { }
        [ParserTarget("midTex")]
        public MaterialTextureParser MidTex
        {
            get => GetTextureName("_midTex");
            set => SetTexture("_midTex", value);
        }

        [ParserTarget("midTexScale")]
        public Vector2Parser MidTexScale
        {
            get => GetTextureScale("_midTex");
            set => SetTextureScale("_midTex", value);
        }

        [ParserTarget("midTexOffset")]
        public Vector2Parser MidTexOffset
        {
            get => GetTextureOffset("_midTex");
            set => SetTextureOffset("_midTex", value);
        }

        // Mid Tiling, default = 100000
        [ParserTarget("midTiling")]
        public NumericParser<float> MidTiling
        {
            get => GetFloat("_midTiling");
            set => SetFloat("_midTiling", value);
        }

        // Mid Bump Map, default = "bump" { }
        [ParserTarget("midBumpMap")]
        public MaterialTextureParser MidBumpMap
        {
            get => GetTextureName("_midBumpMap");
            set => SetTexture("_midBumpMap", value);
        }

        [ParserTarget("midBumpMapScale")]
        public Vector2Parser MidBumpMapScale
        {
            get => GetTextureScale("_midBumpMap");
            set => SetTextureScale("_midBumpMap", value);
        }

        [ParserTarget("midBumpMapOffset")]
        public Vector2Parser MidBumpMapOffset
        {
            get => GetTextureOffset("_midBumpMap");
            set => SetTextureOffset("_midBumpMap", value);
        }

        // Mid Bump Tiling, default = 100000
        [ParserTarget("midBumpTiling")]
        public NumericParser<float> MidBumpTiling
        {
            get => GetFloat("_midBumpTiling");
            set => SetFloat("_midBumpTiling", value);
        }

        // High Texture, default = "white" { }
        [ParserTarget("highTex")]
        public MaterialTextureParser HighTex
        {
            get => GetTextureName("_highTex");
            set => SetTexture("_highTex", value);
        }

        [ParserTarget("highTexScale")]
        public Vector2Parser HighTexScale
        {
            get => GetTextureScale("_highTex");
            set => SetTextureScale("_highTex", value);
        }

        [ParserTarget("highTexOffset")]
        public Vector2Parser HighTexOffset
        {
            get => GetTextureOffset("_highTex");
            set => SetTextureOffset("_highTex", value);
        }

        // High Tiling, default = 100000
        [ParserTarget("highTiling")]
        public NumericParser<float> HighTiling
        {
            get => GetFloat("_highTiling");
            set => SetFloat("_highTiling", value);
        }

        // Low Transition Start, default = 0
        [ParserTarget("lowStart")]
        public NumericParser<float> LowStart
        {
            get => GetFloat("_lowStart");
            set => SetFloat("_lowStart", value);
        }

        // Low Transition End, default = 0.3
        [ParserTarget("lowEnd")]
        public NumericParser<float> LowEnd
        {
            get => GetFloat("_lowEnd");
            set => SetFloat("_lowEnd", value);
        }

        // High Transition Start, default = 0.8
        [ParserTarget("highStart")]
        public NumericParser<float> HighStart
        {
            get => GetFloat("_highStart");
            set => SetFloat("_highStart", value);
        }

        // High Transition End, default = 1
        [ParserTarget("highEnd")]
        public NumericParser<float> HighEnd
        {
            get => GetFloat("_highEnd");
            set => SetFloat("_highEnd", value);
        }

        // AP Global Density, default = 1
        [ParserTarget("globalDensity")]
        public NumericParser<float> GlobalDensity
        {
            get => GetFloat("_globalDensity");
            set => SetFloat("_globalDensity", value);
        }

        // FogColorRamp, default = "white" { }
        [ParserTarget("fogColorRamp")]
        public MaterialTextureParser FogColorRamp
        {
            get => GetTextureName("_fogColorRamp");
            set => SetTexture("_fogColorRamp", value);
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

        // PlanetOpacity, default = 1
        [ParserTarget("planetOpacity")]
        public NumericParser<float> PlanetOpacity
        {
            get => GetFloat("_PlanetOpacity");
            set => SetFloat("_PlanetOpacity", value);
        }

        // Ocean Fog Dist, default = 1000
        [ParserTarget("oceanFogDistance")]
        public NumericParser<float> OceanFogDistance
        {
            get => GetFloat("_oceanFogDistance");
            set => SetFloat("_oceanFogDistance", value);
        }

        public override ShaderParser ShaderParser { get; set; } = Shader;

        // Constructors
        public PQSTriplanarZoomRotationLoader() { }

        public PQSTriplanarZoomRotationLoader(Material material) => Value = new(material);
    }
}
