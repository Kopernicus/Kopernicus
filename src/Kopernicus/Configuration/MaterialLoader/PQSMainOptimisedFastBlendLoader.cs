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
    public class PQSMainOptimisedFastBlendLoader : PQSMainOptimisedFastBlend
    {
        // Saturation, default = 1
        [ParserTarget("saturation")]
        public NumericParser<Single> SaturationSetter
        {
            get { return Saturation; }
            set { Saturation = value; }
        }

        // Contrast, default = 1
        [ParserTarget("contrast")]
        public NumericParser<Single> ContrastSetter
        {
            get { return Contrast; }
            set { Contrast = value; }
        }

        // Colour Unsaturation (A = Factor), default = (1,1,1,0)
        [ParserTarget("tintColor")]
        public ColorParser TintColorSetter
        {
            get { return TintColor; }
            set { TintColor = value; }
        }

        // Near Blend, default = 0.5
        [ParserTarget("powerNear")]
        public NumericParser<Single> PowerNearSetter
        {
            get { return PowerNear; }
            set { PowerNear = value; }
        }

        // Far Blend, default = 0.5
        [ParserTarget("powerFar")]
        public NumericParser<Single> PowerFarSetter
        {
            get { return PowerFar; }
            set { PowerFar = value; }
        }

        // NearFar Start, default = 2000
        [ParserTarget("groundTexStart")]
        public NumericParser<Single> GroundTexStartSetter
        {
            get { return GroundTexStart; }
            set { GroundTexStart = value; }
        }

        // NearFar End, default = 10000
        [ParserTarget("groundTexEnd")]
        public NumericParser<Single> GroundTexEndSetter
        {
            get { return GroundTexEnd; }
            set { GroundTexEnd = value; }
        }

        // Steep Blend, default = 1
        [ParserTarget("steepPower")]
        public NumericParser<Single> SteepPowerSetter
        {
            get { return SteepPower; }
            set { SteepPower = value; }
        }

        // Steep Fade Start, default = 20000
        [ParserTarget("steepTexStart")]
        public NumericParser<Single> SteepTexStartSetter
        {
            get { return SteepTexStart; }
            set { SteepTexStart = value; }
        }

        // Steep Fade End, default = 30000
        [ParserTarget("steepTexEnd")]
        public NumericParser<Single> SteepTexEndSetter
        {
            get { return SteepTexEnd; }
            set { SteepTexEnd = value; }
        }

        // Steep Texture, default = "white" { }
        [ParserTarget("steepTex")]
        public Texture2DParser SteepTexSetter
        {
            get { return SteepTex; }
            set { SteepTex = value; }
        }

        [ParserTarget("steepTexScale")]
        public Vector2Parser SteepTexScaleSetter
        {
            get { return SteepTexScale; }
            set { SteepTexScale = value; }
        }

        [ParserTarget("steepTexOffset")]
        public Vector2Parser SteepTexOffsetSetter
        {
            get { return SteepTexOffset; }
            set { SteepTexOffset = value; }
        }

        // Steep Bump Map, default = "bump" { }
        [ParserTarget("steepBumpMap")]
        public Texture2DParser SteepBumpMapSetter
        {
            get { return SteepBumpMap; }
            set { SteepBumpMap = value; }
        }

        [ParserTarget("steepBumpMapScale")]
        public Vector2Parser SteepBumpMapScaleSetter
        {
            get { return SteepBumpMapScale; }
            set { SteepBumpMapScale = value; }
        }

        [ParserTarget("steepBumpMapOffset")]
        public Vector2Parser SteepBumpMapOffsetSetter
        {
            get { return SteepBumpMapOffset; }
            set { SteepBumpMapOffset = value; }
        }

        // Steep Near Tiling, default = 1
        [ParserTarget("steepNearTiling")]
        public NumericParser<Single> SteepNearTilingSetter
        {
            get { return SteepNearTiling; }
            set { SteepNearTiling = value; }
        }

        // Steep Far Tiling, default = 1
        [ParserTarget("steepTiling")]
        public NumericParser<Single> SteepTilingSetter
        {
            get { return SteepTiling; }
            set { SteepTiling = value; }
        }

        // Low Texture, default = "white" { }
        [ParserTarget("lowTex")]
        public Texture2DParser LowTexSetter
        {
            get { return LowTex; }
            set { LowTex = value; }
        }

        [ParserTarget("lowTexScale")]
        public Vector2Parser LowTexScaleSetter
        {
            get { return LowTexScale; }
            set { LowTexScale = value; }
        }

        [ParserTarget("lowTexOffset")]
        public Vector2Parser LowTexOffsetSetter
        {
            get { return LowTexOffset; }
            set { LowTexOffset = value; }
        }

        // Low Near Tiling, default = 1000
        [ParserTarget("lowNearTiling")]
        public NumericParser<Single> LowNearTilingSetter
        {
            get { return LowNearTiling; }
            set { LowNearTiling = value; }
        }

        // Low Far Tiling, default = 10
        [ParserTarget("lowMultiFactor")]
        public NumericParser<Single> LowMultiFactorSetter
        {
            get { return LowMultiFactor; }
            set { LowMultiFactor = value; }
        }

        // Mid Texture, default = "white" { }
        [ParserTarget("midTex")]
        public Texture2DParser MidTexSetter
        {
            get { return MidTex; }
            set { MidTex = value; }
        }

        [ParserTarget("midTexScale")]
        public Vector2Parser MidTexScaleSetter
        {
            get { return MidTexScale; }
            set { MidTexScale = value; }
        }

        [ParserTarget("midTexOffset")]
        public Vector2Parser MidTexOffsetSetter
        {
            get { return MidTexOffset; }
            set { MidTexOffset = value; }
        }

        // Mid Bump Map, default = "bump" { }
        [ParserTarget("midBumpMap")]
        public Texture2DParser MidBumpMapSetter
        {
            get { return MidBumpMap; }
            set { MidBumpMap = value; }
        }

        [ParserTarget("midBumpMapScale")]
        public Vector2Parser MidBumpMapScaleSetter
        {
            get { return MidBumpMapScale; }
            set { MidBumpMapScale = value; }
        }

        [ParserTarget("midBumpMapOffset")]
        public Vector2Parser MidBumpMapOffsetSetter
        {
            get { return MidBumpMapOffset; }
            set { MidBumpMapOffset = value; }
        }

        // Mid Near Tiling, default = 1000
        [ParserTarget("midNearTiling")]
        public NumericParser<Single> MidNearTilingSetter
        {
            get { return MidNearTiling; }
            set { MidNearTiling = value; }
        }

        // Mid Far Tiling, default = 10
        [ParserTarget("midMultiFactor")]
        public NumericParser<Single> MidMultiFactorSetter
        {
            get { return MidMultiFactor; }
            set { MidMultiFactor = value; }
        }

        // Mid Bump Near Tiling, default = 1
        [ParserTarget("midBumpNearTiling")]
        public NumericParser<Single> MidBumpNearTilingSetter
        {
            get { return MidBumpNearTiling; }
            set { MidBumpNearTiling = value; }
        }

        // High Texture, default = "white" { }
        [ParserTarget("highTex")]
        public Texture2DParser HighTexSetter
        {
            get { return HighTex; }
            set { HighTex = value; }
        }

        [ParserTarget("highTexScale")]
        public Vector2Parser HighTexScaleSetter
        {
            get { return HighTexScale; }
            set { HighTexScale = value; }
        }

        [ParserTarget("highTexOffset")]
        public Vector2Parser HighTexOffsetSetter
        {
            get { return HighTexOffset; }
            set { HighTexOffset = value; }
        }

        // High Near Tiling, default = 1000
        [ParserTarget("highNearTiling")]
        public NumericParser<Single> HighNearTilingSetter
        {
            get { return HighNearTiling; }
            set { HighNearTiling = value; }
        }

        // High Far Tiling, default = 10
        [ParserTarget("highMultiFactor")]
        public NumericParser<Single> HighMultiFactorSetter
        {
            get { return HighMultiFactor; }
            set { HighMultiFactor = value; }
        }

        // Low Transition Start, default = 0
        [ParserTarget("lowStart")]
        public NumericParser<Single> LowStartSetter
        {
            get { return LowStart; }
            set { LowStart = value; }
        }

        // Low Transition End, default = 0.3
        [ParserTarget("lowEnd")]
        public NumericParser<Single> LowEndSetter
        {
            get { return LowEnd; }
            set { LowEnd = value; }
        }

        // High Transition Start, default = 0.8
        [ParserTarget("highStart")]
        public NumericParser<Single> HighStartSetter
        {
            get { return HighStart; }
            set { HighStart = value; }
        }

        // High Transition End, default = 1
        [ParserTarget("highEnd")]
        public NumericParser<Single> HighEndSetter
        {
            get { return HighEnd; }
            set { HighEnd = value; }
        }

        // AP Global Density, default = 1
        [ParserTarget("globalDensity")]
        public NumericParser<Single> GlobalDensitySetter
        {
            get { return GlobalDensity; }
            set { GlobalDensity = value; }
        }

        // FogColorRamp, default = "white" { }
        [ParserTarget("fogColorRamp")]
        public Texture2DParser FogColorRampSetter
        {
            get { return FogColorRamp; }
            set { FogColorRamp = value; }
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

        // PlanetOpacity, default = 1
        [ParserTarget("planetOpacity")]
        public NumericParser<Single> PlanetOpacitySetter
        {
            get { return PlanetOpacity; }
            set { PlanetOpacity = value; }
        }

        // Ocean Fog Dist, default = 1000
        [ParserTarget("oceanFogDistance")]
        public NumericParser<Single> OceanFogDistanceSetter
        {
            get { return OceanFogDistance; }
            set { OceanFogDistance = value; }
        }

        // Constructors
        public PQSMainOptimisedFastBlendLoader()
        {
        }

        [Obsolete("Creating materials from shader source String is no longer supported. Use Shader assets instead.")]
        public PQSMainOptimisedFastBlendLoader(String contents) : base(contents)
        {
        }

        public PQSMainOptimisedFastBlendLoader(Material material) : base(material)
        {
        }
    }
}
