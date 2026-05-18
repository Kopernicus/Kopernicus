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
    [MaterialLoader(NormalBumpedLoader.SHADER_NAME)]
    public class NormalBumpedLoader : BaseMaterialLoader
    {
        public const String SHADER_NAME = "Legacy Shaders/Bumped Diffuse";
        private static readonly Shader Shader = Shader.Find(SHADER_NAME);
        public static bool UsesSameShader(Material m) => m != null && m.shader.name == SHADER_NAME;

        // Main Color, default = (1,1,1,1)
        [ParserTarget("color")]
        public ColorParser ColorSetter
        {
            get => GetColor("_Color");
            set => SetColor("_Color", value);
        }

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

        public override ShaderParser ShaderParser { get; set; } = Shader;

        // Constructors
        public NormalBumpedLoader() { }

        public NormalBumpedLoader(Material material) => Value = new(material);
    }
}
