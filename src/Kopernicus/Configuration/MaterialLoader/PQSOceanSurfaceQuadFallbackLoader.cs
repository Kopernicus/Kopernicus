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
using System.Diagnostics.CodeAnalysis;
using Kopernicus.Components.MaterialWrapper;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Parsing;
using UnityEngine;

namespace Kopernicus.Configuration.MaterialLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PQSOceanSurfaceQuadFallbackLoader : PQSOceanSurfaceQuadFallback
    {
        // Main Color, default = (1,1,1,1)
        [ParserTarget("color")]
        public ColorParser ColorSetter
        {
            get { return Color; }
            set { Color = value; }
        }

        // Color From Space, default = (1,1,1,1)
        [ParserTarget("colorFromSpace")]
        public ColorParser ColorFromSpaceSetter
        {
            get { return ColorFromSpace; }
            set { ColorFromSpace = value; }
        }

        // Specular Color, default = (1,1,1,1)
        [ParserTarget("specColor")]
        public ColorParser SpecColorSetter
        {
            get { return SpecColor; }
            set { SpecColor = value; }
        }

        // Shininess, default = 0.078125
        [ParserTarget("shininess")]
        public NumericParser<Single> ShininessSetter
        {
            get { return Shininess; }
            set { Shininess = value; }
        }

        // Gloss, default = 0.078125
        [ParserTarget("gloss")]
        public NumericParser<Single> GlossSetter
        {
            get { return Gloss; }
            set { Gloss = value; }
        }

        // Tex Tiling, default = 1
        [ParserTarget("tiling")]
        public NumericParser<Single> TilingSetter
        {
            get { return Tiling; }
            set { Tiling = value; }
        }

        // Tex0, default = "white" { }
        [ParserTarget("waterTex")]
        public Texture2DParser WaterTexSetter
        {
            get { return WaterTex; }
            set { WaterTex = value; }
        }

        [ParserTarget("waterTexScale")]
        public Vector2Parser WaterTexScaleSetter
        {
            get { return WaterTexScale; }
            set { WaterTexScale = value; }
        }

        [ParserTarget("waterTexOffset")]
        public Vector2Parser WaterTexOffsetSetter
        {
            get { return WaterTexOffset; }
            set { WaterTexOffset = value; }
        }

        // Tex1, default = "white" { }
        [ParserTarget("waterTex1")]
        public Texture2DParser WaterTex1Setter
        {
            get { return WaterTex1; }
            set { WaterTex1 = value; }
        }

        [ParserTarget("waterTex1Scale")]
        public Vector2Parser WaterTex1ScaleSetter
        {
            get { return WaterTex1Scale; }
            set { WaterTex1Scale = value; }
        }

        [ParserTarget("waterTex1Offset")]
        public Vector2Parser WaterTex1OffsetSetter
        {
            get { return WaterTex1Offset; }
            set { WaterTex1Offset = value; }
        }

        // FadeStart, default = 1
        [ParserTarget("fadeStart")]
        public NumericParser<Single> FadeStartSetter
        {
            get { return FadeStart; }
            set { FadeStart = value; }
        }

        // FadeEnd, default = 1
        [ParserTarget("fadeEnd")]
        public NumericParser<Single> FadeEndSetter
        {
            get { return FadeEnd; }
            set { FadeEnd = value; }
        }

        // PlanetOpacity, default = 1
        [ParserTarget("planetOpacity")]
        public NumericParser<Single> PlanetOpacitySetter
        {
            get { return PlanetOpacity; }
            set { PlanetOpacity = value; }
        }

        // Constructors
        public PQSOceanSurfaceQuadFallbackLoader()
        {
        }

        [Obsolete("Creating materials from shader source String is no longer supported. Use Shader assets instead.")]
        public PQSOceanSurfaceQuadFallbackLoader(String contents) : base(contents)
        {
        }

        public PQSOceanSurfaceQuadFallbackLoader(Material material) : base(material)
        {
        }
    }
}
