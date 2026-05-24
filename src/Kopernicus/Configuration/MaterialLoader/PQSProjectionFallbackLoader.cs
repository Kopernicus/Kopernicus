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
    [MaterialLoader(PQSProjectionFallbackLoader.SHADER_NAME)]
    public class PQSProjectionFallbackLoader : PQSMaterialLoader
    {
        public const String SHADER_NAME = "Terrain/PQS/Sphere Projection SURFACE QUAD (Fallback) ";
        private static readonly Shader Shader = Shader.Find(SHADER_NAME);
        public static bool UsesSameShader(Material m) => m != null && m.shader.name == SHADER_NAME;

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

        // Near Tiling, default = 1000
        [ParserTarget("texTiling")]
        public NumericParser<float> TexTiling
        {
            get => GetFloat("_texTiling");
            set => SetFloat("_texTiling", value);
        }

        // Near Blend, default = 0.5
        [ParserTarget("texPower")]
        public NumericParser<float> TexPower
        {
            get => GetFloat("_texPower");
            set => SetFloat("_texPower", value);
        }

        // Far Blend, default = 0.5
        [ParserTarget("multiPower")]
        public NumericParser<float> MultiPower
        {
            get => GetFloat("_multiPower");
            set => SetFloat("_multiPower", value);
        }

        // NearFar Start, default = 2000
        [ParserTarget("groundTexStart")]
        public NumericParser<float> GroundTexStart
        {
            get => GetFloat("_groundTexStart");
            set => SetFloat("_groundTexStart", value);
        }

        // NearFar Start, default = 10000
        [ParserTarget("groundTexEnd")]
        public NumericParser<float> GroundTexEnd
        {
            get => GetFloat("_groundTexEnd");
            set => SetFloat("_groundTexEnd", value);
        }

        // Multifactor, default = 0.5
        [ParserTarget("multiFactor")]
        public NumericParser<float> MultiFactor
        {
            get => GetFloat("_multiFactor");
            set => SetFloat("_multiFactor", value);
        }

        // Main Texture, default = "white" { }
        [ParserTarget("mainTex")]
        public MaterialTextureParser MainTex
        {
            get => GetTextureName("_mainTex");
            set => SetTexture("_mainTex", value);
        }

        [ParserTarget("mainTexScale")]
        public Vector2Parser MainTexScale
        {
            get => GetTextureScale("_mainTex");
            set => SetTextureScale("_mainTex", value);
        }

        [ParserTarget("mainTexOffset")]
        public Vector2Parser MainTexOffset
        {
            get => GetTextureOffset("_mainTex");
            set => SetTextureOffset("_mainTex", value);
        }

        // PlanetOpacity, default = 1
        [ParserTarget("planetOpacity")]
        public NumericParser<float> PlanetOpacity
        {
            get => GetFloat("_PlanetOpacity");
            set => SetFloat("_PlanetOpacity", value);
        }

        public override ShaderParser ShaderParser { get; set; } = Shader;
        public override NumericParser<bool> OnDemand { get; set; } = false;

        public override void OnParentApply(PQS pqs, PQSMod_OnDemandHandler handler)
        {
            if (Value == null || pqs == null)
                return;

            pqs.fallbackMaterial = Value;
            AttachTextureListener<PQSFallbackMaterialTextureListener>(pqs.gameObject, handler);
        }

        // Constructors
        public PQSProjectionFallbackLoader() { }

        public PQSProjectionFallbackLoader(Material material) => Value = new(material);
    }
}
