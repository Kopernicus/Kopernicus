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
    [MaterialLoader(AerialTransCutoutLoader.SHADER_NAME)]
    public class AerialTransCutoutLoader : MaterialLoader
    {
        public const String SHADER_NAME = "Terrain/PQS/Aerial Cutout";
        private static readonly Shader Shader = Shader.Find(SHADER_NAME);
        public static bool UsesSameShader(Material m) => m != null && m.shader.name == SHADER_NAME;

        // Main Color, default = (1,1,1,1)
        [ParserTarget("color")]
        public ColorParser Color
        {
            get => GetColor("_Color");
            set => SetColor("_Color", value);
        }

        // Base (RGB) Trans (A), default = "white" { }
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

        // Alpha cutoff, default = 0.5
        [ParserTarget("texCutoff")]
        public NumericParser<float> TexCutoff
        {
            get => GetFloat("_texCutoff");
            set => SetFloat("_texCutoff", value);
        }

        // AP Fog Color, default = (0,0,1,1)
        [ParserTarget("fogColor")]
        public ColorParser FogColor
        {
            get => GetColor("_fogColor");
            set => SetColor("_fogColor", value);
        }

        // AP Height Fall Off, default = 1
        [ParserTarget("heightFallOff")]
        public NumericParser<float> HeightFallOff
        {
            get => GetFloat("_heightFallOff");
            set => SetFloat("_heightFallOff", value);
        }

        // AP Global Density, default = 1
        [ParserTarget("globalDensity")]
        public NumericParser<float> GlobalDensity
        {
            get => GetFloat("_globalDensity");
            set => SetFloat("_globalDensity", value);
        }

        // AP Atmosphere Depth, default = 1
        [ParserTarget("atmosphereDepth")]
        public NumericParser<float> AtmosphereDepth
        {
            get => GetFloat("_atmosphereDepth");
            set => SetFloat("_atmosphereDepth", value);
        }

        public override ShaderParser ShaderParser { get; set; } = Shader;

        // Constructors
        public AerialTransCutoutLoader() { }

        public AerialTransCutoutLoader(Material material) => Value = new(material);
    }
}
