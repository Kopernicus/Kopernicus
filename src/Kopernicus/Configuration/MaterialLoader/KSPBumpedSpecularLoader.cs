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
    [MaterialLoader(KSPBumpedSpecularLoader.SHADER_NAME)]
    public class KSPBumpedSpecularLoader : MaterialLoader
    {
        public const String SHADER_NAME = "KSP/Bumped Specular";
        private static readonly Shader Shader = Shader.Find(SHADER_NAME);
        public static bool UsesSameShader(Material m) => m != null && m.shader.name == SHADER_NAME;

        // Base (RGB), default = "white" { }
        [ParserTarget("mainTex")]
        public MaterialTextureParser MainTexSetter
        {
            get => null;
            set => SetTexture("_MainTex", value);
        }

        [ParserTarget("mainTexScale")]
        public Vector2Parser MainTexScaleSetter
        {
            get => GetTextureScale("_MainTex");
            set => SetTextureScale("_MainTex", value);
        }

        [ParserTarget("mainTexOffset")]
        public Vector2Parser MainTexOffsetSetter
        {
            get => GetTextureOffset("_MainTex");
            set => SetTextureOffset("_MainTex", value);
        }

        // Normal map, default = "bump" { }
        [ParserTarget("bumpMap")]
        public MaterialTextureParser BumpMapSetter
        {
            get => null;
            set => SetTexture("_BumpMap", value);
        }

        [ParserTarget("bumpMapScale")]
        public Vector2Parser BumpMapScaleSetter
        {
            get => GetTextureScale("_BumpMap");
            set => SetTextureScale("_BumpMap", value);
        }

        [ParserTarget("bumpMapOffset")]
        public Vector2Parser BumpMapOffsetSetter
        {
            get => GetTextureOffset("_BumpMap");
            set => SetTextureOffset("_BumpMap", value);
        }

        // Main Color, default = (1,1,1,1)
        [ParserTarget("color")]
        public ColorParser ColorSetter
        {
            get => GetColor("_Color");
            set => SetColor("_Color", value);
        }

        // _SpecColor, default = (0.5,0.5,0.5,1)
        [ParserTarget("specColor")]
        public ColorParser SpecColorSetter
        {
            get => GetColor("_SpecColor");
            set => SetColor("_SpecColor", value);
        }

        // _Shininess, default = 1
        [ParserTarget("shininess")]
        public NumericParser<float> ShininessSetter
        {
            get => GetFloat("_Shininess");
            set => SetFloat("_Shininess", value);
        }

        // _Opacity, default = 1
        [ParserTarget("opacity")]
        public NumericParser<float> OpacitySetter
        {
            get => GetFloat("_Opacity");
            set => SetFloat("_Opacity", value);
        }

        // _RimFalloff, default = 0.1
        [ParserTarget("rimFalloff")]
        public NumericParser<float> RimFalloffSetter
        {
            get => GetFloat("_RimFalloff");
            set => SetFloat("_RimFalloff", value);
        }

        // _RimColor, default = (0,0,0,0)
        [ParserTarget("rimColor")]
        public ColorParser RimColorSetter
        {
            get => GetColor("_RimColor");
            set => SetColor("_RimColor", value);
        }

        // _TemperatureColor, default = (0,0,0,0)
        [ParserTarget("temperatureColor")]
        public ColorParser TemperatureColorSetter
        {
            get => GetColor("_TemperatureColor");
            set => SetColor("_TemperatureColor", value);
        }

        // Burn Color, default = (1,1,1,1)
        [ParserTarget("burnColor")]
        public ColorParser BurnColorSetter
        {
            get => GetColor("_BurnColor");
            set => SetColor("_BurnColor", value);
        }

        // Underwater Fog Factor, default = 0
        [ParserTarget("underwaterFogFactor")]
        public NumericParser<float> UnderwaterFogFactorSetter
        {
            get => GetFloat("_UnderwaterFogFactor");
            set => SetFloat("_UnderwaterFogFactor", value);
        }

        public override ShaderParser ShaderParser { get; set; } = Shader;

        // Constructors
        public KSPBumpedSpecularLoader() { }

        public KSPBumpedSpecularLoader(Material material) => Value = new(material);
    }
}
