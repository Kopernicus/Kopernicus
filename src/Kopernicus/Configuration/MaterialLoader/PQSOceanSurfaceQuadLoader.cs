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
using Kopernicus.UI;
using UnityEngine;
using Gradient = Kopernicus.Configuration.Parsing.Gradient;

namespace Kopernicus.Configuration.MaterialLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PQSOceanSurfaceQuadLoader : PQSOceanSurfaceQuad
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

        // Normal Tiling, default = 1
        [ParserTarget("bTiling")]
        public NumericParser<Single> BTilingSetter
        {
            get { return BTiling; }
            set { BTiling = value; }
        }

        // Normal map, default = "bump" { }
        [ParserTarget("bumpMap")]
        public Texture2DParser BumpMapSetter
        {
            get { return BumpMap; }
            set { BumpMap = value; }
        }

        [ParserTarget("bumpMapScale")]
        public Vector2Parser BumpMapScaleSetter
        {
            get { return BumpMapScale; }
            set { BumpMapScale = value; }
        }

        [ParserTarget("bumpMapOffset")]
        public Vector2Parser BumpMapOffsetSetter
        {
            get { return BumpMapOffset; }
            set { BumpMapOffset = value; }
        }

        // Water Movement, default = 1
        [ParserTarget("displacement")]
        public NumericParser<Single> DisplacementSetter
        {
            get { return Displacement; }
            set { Displacement = value; }
        }

        // Texture Displacement, default = 1
        [ParserTarget("texDisplacement")]
        public NumericParser<Single> TexDisplacementSetter
        {
            get { return TexDisplacement; }
            set { TexDisplacement = value; }
        }

        // Water Freq, default = 1
        [ParserTarget("dispFreq")]
        public NumericParser<Single> DispFreqSetter
        {
            get { return DispFreq; }
            set { DispFreq = value; }
        }

        // Mix, default = 1
        [ParserTarget("mix")]
        public NumericParser<Single> MixSetter
        {
            get { return Mix; }
            set { Mix = value; }
        }

        // Opacity, default = 1
        [ParserTarget("oceanOpacity")]
        public NumericParser<Single> OceanOpacitySetter
        {
            get { return OceanOpacity; }
            set { OceanOpacity = value; }
        }

        // Falloff Power, default = 1
        [ParserTarget("falloffPower")]
        public NumericParser<Single> FalloffPowerSetter
        {
            get { return FalloffPower; }
            set { FalloffPower = value; }
        }

        // Falloff Exp, default = 2
        [ParserTarget("falloffExp")]
        public NumericParser<Single> FalloffExpSetter
        {
            get { return FalloffExp; }
            set { FalloffExp = value; }
        }

        // AP Fog Color, default = (0,0,1,1)
        [ParserTarget("fogColor")]
        public ColorParser FogColorSetter
        {
            get { return FogColor; }
            set { FogColor = value; }
        }

        // AP Height Fall Off, default = 1
        [ParserTarget("heightFallOff")]
        public NumericParser<Single> HeightFallOffSetter
        {
            get { return HeightFallOff; }
            set { HeightFallOff = value; }
        }

        // AP Global Density, default = 1
        [ParserTarget("globalDensity")]
        public NumericParser<Single> GlobalDensitySetter
        {
            get { return GlobalDensity; }
            set { GlobalDensity = value; }
        }

        // AP Atmosphere Depth, default = 1
        [ParserTarget("atmosphereDepth")]
        public NumericParser<Single> AtmosphereDepthSetter
        {
            get { return AtmosphereDepth; }
            set { AtmosphereDepth = value; }
        }

        // FogColorRamp, default = "white" { }
        [ParserTarget("fogColorRamp")]
        public Texture2DParser FogColorRampSetter
        {
            get { return FogColorRamp; }
            set { FogColorRamp = value; }
        }

        // FogColorRamp, default = "white" { }
        [ParserTarget("FogColorRamp")]
        [KittopiaHideOption]
        public Gradient FogColorRampGradientSetter
        {
            set
            {
                // Generate the ramp from a gradient
                Texture2D ramp = new Texture2D(512, 1);
                Color32[] colors = ramp.GetPixels32(0);
                for (Int32 i = 0; i < colors.Length; i++)
                {
                    // Compute the position in the gradient
                    Single k = (Single) i / colors.Length;
                    colors[i] = value.ColorAt(k);
                }

                ramp.SetPixels32(colors, 0);
                ramp.Apply(true, false);

                // Set the color ramp
                FogColorRamp = ramp;
            }
        }

        [ParserTarget("fogColorRampScale")]
        public Vector2Parser FogColorRampScaleSetter
        {
            get { return FogColorRampScale; }
            set { FogColorRampScale = value; }
        }

        [ParserTarget("fogColorRampOffset")]
        public Vector2Parser FogColorRampOffsetSetter
        {
            get { return FogColorRampOffset; }
            set { FogColorRampOffset = value; }
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

        // NormalXYFudge, default = 0.1
        [ParserTarget("normalXYFudge")]
        public NumericParser<Single> NormalXyFudgeSetter
        {
            get { return NormalXyFudge; }
            set { NormalXyFudge = value; }
        }

        // NormalZFudge, default = 1.1
        [ParserTarget("normalZFudge")]
        public NumericParser<Single> NormalZFudgeSetter
        {
            get { return NormalZFudge; }
            set { NormalZFudge = value; }
        }

        // Constructors
        public PQSOceanSurfaceQuadLoader()
        {
        }

        [Obsolete("Creating materials from shader source String is no longer supported. Use Shader assets instead.")]
        public PQSOceanSurfaceQuadLoader(String contents) : base(contents)
        {
        }

        public PQSOceanSurfaceQuadLoader(Material material) : base(material)
        {
        }
    }
}
