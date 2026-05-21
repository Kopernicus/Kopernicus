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
    [MaterialLoader(ScaledPlanetRimLightLoader.SHADER_NAME)]
    public class ScaledPlanetRimLightLoader : ScaledMaterialLoader
    {
        public const String SHADER_NAME = "Terrain/Scaled Planet (RimLight)";
        private static readonly Shader Shader = Shader.Find(SHADER_NAME);
        public static bool UsesSameShader(Material m) => m != null && m.shader.name == SHADER_NAME;

        // Main Color, default = (1,1,1,1)
        [ParserTarget("color")]
        public ColorParser Color
        {
            get => GetColor("_Color");
            set => SetColor("_Color", value);
        }

        // Specular Color, default = (0.5,0.5,0.5,1)
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

        // Base (RGB) Gloss (A), default = "white" { }
        [ParserTarget("mainTex")]
        public MaterialTextureParser MainTex
        {
            get => GetTexture("_MainTex")?.name;
            set => SetTexture("_MainTex", value);
        }

        [ParserTarget("mainTexScale")]
        public Vector2Parser MainTexScale
        {
            get => GetTextureScale("_MainTex");
            set => SetTextureScale("_MainTex", value);
        }

        [ParserTarget("mainTexOffset")]
        public Vector2Parser MainTexOffset
        {
            get => GetTextureOffset("_MainTex");
            set => SetTextureOffset("_MainTex", value);
        }

        // Normalmap, default = "bump" { }
        [ParserTarget("bumpMap")]
        public MaterialTextureParser BumpMap
        {
            get => GetTexture("_BumpMap")?.name;
            set => SetTexture("_BumpMap", value);
        }

        [ParserTarget("bumpMapScale")]
        public Vector2Parser BumpMapScale
        {
            get => GetTextureScale("_BumpMap");
            set => SetTextureScale("_BumpMap", value);
        }

        [ParserTarget("bumpMapOffset")]
        public Vector2Parser BumpMapOffset
        {
            get => GetTextureOffset("_BumpMap");
            set => SetTextureOffset("_BumpMap", value);
        }

        // Opacity, default = 1
        [ParserTarget("opacity")]
        public NumericParser<float> Opacity
        {
            get => GetFloat("_Opacity");
            set => SetFloat("_Opacity", value);
        }

        // Rim Color, default = (0.26,0.19,0.16,0)
        [ParserTarget("rimColor")]
        public ColorParser RimColor
        {
            get => GetColor("_RimColor");
            set => SetColor("_RimColor", value);
        }

        // Rim Power, default = 3
        [ParserTarget("rimPower")]
        public NumericParser<float> RimPower
        {
            get => GetFloat("_RimPower");
            set => SetFloat("_RimPower", value);
        }

        // Resource Map (RGB), default = "black" { }
        [ParserTarget("resourceMap")]
        public MaterialTextureParser ResourceMap
        {
            get => GetTexture("_ResourceMap")?.name;
            set => SetTexture("_ResourceMap", value);
        }

        [ParserTarget("resourceMapScale")]
        public Vector2Parser ResourceMapScale
        {
            get => GetTextureScale("_ResourceMap");
            set => SetTextureScale("_ResourceMap", value);
        }

        [ParserTarget("resourceMapOffset")]
        public Vector2Parser ResourceMapOffset
        {
            get => GetTextureOffset("_ResourceMap");
            set => SetTextureOffset("_ResourceMap", value);
        }

        public override ShaderParser ShaderParser { get; set; } = Shader;

        // Constructors
        public ScaledPlanetRimLightLoader() { }

        public ScaledPlanetRimLightLoader(Material material) => Value = new(material);
    }
}
