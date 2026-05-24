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
using Kopernicus.OnDemand;
using UnityEngine;

namespace Kopernicus.Configuration.MaterialLoader
{
    [RequireConfigType(ConfigType.Node)]
    [MaterialLoader(PQSOceanSurfaceQuadFallbackLoader.SHADER_NAME)]
    public class PQSOceanSurfaceQuadFallbackLoader : PQSMaterialLoader
    {
        public const String SHADER_NAME = "Terrain/PQS/Ocean Surface Quad (Fallback)";
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
            get => GetTextureName("_WaterTex");
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
            get => GetTextureName("_WaterTex1");
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

        public override ShaderParser ShaderParser { get; set; } = Shader;

        public override void OnParentApply(PQS pqs, PQSMod_OnDemandHandler handler)
        {
            if (Value == null || pqs == null)
                return;

            pqs.fallbackMaterial = Value;
            AttachTextureListener<PQSFallbackMaterialTextureListener>(pqs.gameObject, handler);
        }

        // Constructors
        public PQSOceanSurfaceQuadFallbackLoader() { }

        public PQSOceanSurfaceQuadFallbackLoader(Material material) => Value = new(material);
    }
}
