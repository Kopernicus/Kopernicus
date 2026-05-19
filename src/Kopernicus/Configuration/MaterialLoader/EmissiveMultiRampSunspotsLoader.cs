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
    [MaterialLoader(EmissiveMultiRampSunspotsLoader.SHADER_NAME)]
    public class EmissiveMultiRampSunspotsLoader : MaterialLoader
    {
        public const String SHADER_NAME = "Emissive Multi Ramp Sunspots";
        private static readonly Shader Shader = Shader.Find(SHADER_NAME);
        public static bool UsesSameShader(Material m) => m != null && m.shader.name == SHADER_NAME;

        // Ramp Map (RGBA), default = "white" { }
        [ParserTarget("rampMap")]
        public MaterialTextureParser RampMapSetter
        {
            get => null;
            set => SetTexture("_RampMap", value);
        }

        [ParserTarget("rampMapScale")]
        public Vector2Parser RampMapScaleSetter
        {
            get => GetTextureScale("_RampMap");
            set => SetTextureScale("_RampMap", value);
        }

        [ParserTarget("rampMapOffset")]
        public Vector2Parser RampMapOffsetSetter
        {
            get => GetTextureOffset("_RampMap");
            set => SetTextureOffset("_RampMap", value);
        }

        // Noise Map (RGBA), default = "white" { }
        [ParserTarget("noiseMap")]
        public MaterialTextureParser NoiseMapSetter
        {
            get => null;
            set => SetTexture("_NoiseMap", value);
        }

        [ParserTarget("noiseMapScale")]
        public Vector2Parser NoiseMapScaleSetter
        {
            get => GetTextureScale("_NoiseMap");
            set => SetTextureScale("_NoiseMap", value);
        }

        [ParserTarget("noiseMapOffset")]
        public Vector2Parser NoiseMapOffsetSetter
        {
            get => GetTextureOffset("_NoiseMap");
            set => SetTextureOffset("_NoiseMap", value);
        }

        // Emission Color 0, default = (1,1,1,1)
        [ParserTarget("emitColor0")]
        public ColorParser EmitColor0Setter
        {
            get => GetColor("_EmitColor0");
            set => SetColor("_EmitColor0", value);
        }

        // Emission Color 1, default = (1,1,1,1)
        [ParserTarget("emitColor1")]
        public ColorParser EmitColor1Setter
        {
            get => GetColor("_EmitColor1");
            set => SetColor("_EmitColor1", value);
        }

        // Sunspot Map (R), default = "white" { }
        [ParserTarget("sunspotTex")]
        public MaterialTextureParser SunspotTexSetter
        {
            get => null;
            set => SetTexture("_SunspotTex", value);
        }

        [ParserTarget("sunspotTexScale")]
        public Vector2Parser SunspotTexScaleSetter
        {
            get => GetTextureScale("_SunspotTex");
            set => SetTextureScale("_SunspotTex", value);
        }

        [ParserTarget("sunspotTexOffset")]
        public Vector2Parser SunspotTexOffsetSetter
        {
            get => GetTextureOffset("_SunspotTex");
            set => SetTextureOffset("_SunspotTex", value);
        }

        // Sunspot Power, default = 1
        [ParserTarget("sunspotPower")]
        public NumericParser<float> SunspotPowerSetter
        {
            get => GetFloat("_SunspotPower");
            set => SetFloat("_SunspotPower", value);
        }

        // Sunspot Color, default = (0,0,0,0)
        [ParserTarget("sunspotColor")]
        public ColorParser SunspotColorSetter
        {
            get => GetColor("_SunspotColor");
            set => SetColor("_SunspotColor", value);
        }

        // Rimlight Color, default = (1,1,1,1)
        [ParserTarget("rimColor")]
        public ColorParser RimColorSetter
        {
            get => GetColor("_RimColor");
            set => SetColor("_RimColor", value);
        }

        // Rimlight Power, default = 0.2
        [ParserTarget("rimPower")]
        public NumericParser<float> RimPowerSetter
        {
            get => GetFloat("_RimPower");
            set => SetFloat("_RimPower", value);
        }

        // Rimlight Blend, default = 0.2
        [ParserTarget("rimBlend")]
        public NumericParser<float> RimBlendSetter
        {
            get => GetFloat("_RimBlend");
            set => SetFloat("_RimBlend", value);
        }

        public override ShaderParser ShaderParser { get; set; } = Shader;

        // Constructors
        public EmissiveMultiRampSunspotsLoader() { }

        public EmissiveMultiRampSunspotsLoader(Material material) => Value = new(material);
    }
}
