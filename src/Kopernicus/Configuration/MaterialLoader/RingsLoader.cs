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
    [MaterialLoader(SHADER_NAME)]
    public class RingsLoader : ScaledMaterialLoader
    {
        public const string SHADER_NAME = "Kopernicus/Rings";

        public static bool UsesSameShader(Material m) => m != null && m.shader != null && m.shader.name == SHADER_NAME;

        [PreApply]
        [ParserTarget("shader")]
        public override ShaderParser ShaderParser { get; set; }

        [ParserTarget("onDemand")]
        public override NumericParser<bool> OnDemand { get; set; } = OnDemandStorage.UseOnDemand;

        // The ring itself, sampled across its width. Alpha is opacity.
        [ParserTarget("texture")]
        public MaterialTextureParser MainTexture
        {
            get => GetTextureName("_MainTex");
            set => SetTexture("_MainTex", value);
        }

        // Texture to use when the ring is lit from behind. If not specified it defaults to whatever
        // was specified for `texture`.
        [ParserTarget("backlitTexture")]
        public MaterialTextureParser BacklitTexture
        {
            get => GetTextureName("_BacklitTexture");
            set => SetTexture("_BacklitTexture", value);
        }

        // Opaque pixels here cast a shadow on the inner surface of the ring.
        [ParserTarget("innerShadeTexture")]
        public MaterialTextureParser InnerShadeTexture
        {
            get => GetTextureName("_InnerShadeTexture");
            set => SetTexture("_InnerShadeTexture", value);
        }

        [ParserTarget("color")]
        public ColorParser Color
        {
            get => GetColor("_Color");
            set => SetColor("_Color", value);
        }

        // Front-lit half of the shader.
        [ParserTarget("albedoStrength")]
        public NumericParser<float> AlbedoStrength
        {
            get => GetFloat("albedoStrength");
            set => SetFloat("albedoStrength", value);
        }

        // Back-lit half of the shader.
        [ParserTarget("scatteringStrength")]
        public NumericParser<float> ScatteringStrength
        {
            get => GetFloat("scatteringStrength");
            set => SetFloat("scatteringStrength", value);
        }

        [ParserTarget("anisotropy")]
        public NumericParser<float> Anisotropy
        {
            get => GetFloat("anisotropy");
            set => SetFloat("anisotropy", value);
        }

        // Softens (>1) or sharpens (<1) the planet's shadow on the ring.
        [ParserTarget("penumbraMultiplier")]
        public NumericParser<float> PenumbraMultiplier
        {
            get => GetFloat("penumbraMultiplier");
            set => SetFloat("penumbraMultiplier", value);
        }

        [ParserTarget("fadeoutStartDistance")]
        public NumericParser<float> FadeoutStartDistance
        {
            get => GetFloat("fadeoutStartDistance");
            set => SetFloat("fadeoutStartDistance", value);
        }

        [ParserTarget("fadeoutStopDistance")]
        public NumericParser<float> FadeoutStopDistance
        {
            get => GetFloat("fadeoutStopDistance");
            set => SetFloat("fadeoutStopDistance", value);
        }

        [ParserTarget("fadeoutMinAlpha")]
        public NumericParser<float> FadeoutMinAlpha
        {
            get => GetFloat("fadeoutMinAlpha");
            set => SetFloat("fadeoutMinAlpha", value);
        }

        [ParserTarget("Detail", AllowMerge = true)]
        public RingDetailLoader Detail { get; set; } = new RingDetailLoader();

        public RingsLoader() { }

        public RingsLoader(Material material)
        {
            Value = material;
            if (material != null)
                ShaderParser = material.shader;
        }
        public void ApplyDeferred()
        {
            if (Value == null)
                return;

            if (Value.HasProperty("_BacklitTexture") && GetTextureName("_BacklitTexture") == null)
                SetTexture("_BacklitTexture", GetTextureName("_MainTex"));

            Detail?.Apply(this);
        }
    }
}