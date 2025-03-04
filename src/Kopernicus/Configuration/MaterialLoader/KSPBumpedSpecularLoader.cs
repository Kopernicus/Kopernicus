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
    public class KSPBumpedSpecularLoader : KSPBumpedSpecular
    {
        // Base (RGB), default = "white" { }
        [ParserTarget("mainTex")]
        public Texture2DParser MainTexSetter
        {
            get { return MainTex; }
            set { MainTex = value; }
        }

        [ParserTarget("mainTexScale")]
        public Vector2Parser MainTexScaleSetter
        {
            get { return MainTexScale; }
            set { MainTexScale = value; }
        }

        [ParserTarget("mainTexOffset")]
        public Vector2Parser MainTexOffsetSetter
        {
            get { return MainTexOffset; }
            set { MainTexOffset = value; }
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

        // Main Color, default = (1,1,1,1)
        [ParserTarget("color")]
        public ColorParser ColorSetter
        {
            get { return Color; }
            set { Color = value; }
        }

        // _SpecColor, default = (0.5,0.5,0.5,1)
        [ParserTarget("specColor")]
        public ColorParser SpecColorSetter
        {
            get { return SpecColor; }
            set { SpecColor = value; }
        }

        // _Shininess, default = 1
        [ParserTarget("shininess")]
        public NumericParser<Single> ShininessSetter
        {
            get { return Shininess; }
            set { Shininess = value; }
        }

        // _Opacity, default = 1
        [ParserTarget("opacity")]
        public NumericParser<Single> OpacitySetter
        {
            get { return Opacity; }
            set { Opacity = value; }
        }

        // _RimFalloff, default = 0.1
        [ParserTarget("rimFalloff")]
        public NumericParser<Single> RimFalloffSetter
        {
            get { return RimFalloff; }
            set { RimFalloff = value; }
        }

        // _RimColor, default = (0,0,0,0)
        [ParserTarget("rimColor")]
        public ColorParser RimColorSetter
        {
            get { return RimColor; }
            set { RimColor = value; }
        }

        // _TemperatureColor, default = (0,0,0,0)
        [ParserTarget("temperatureColor")]
        public ColorParser TemperatureColorSetter
        {
            get { return TemperatureColor; }
            set { TemperatureColor = value; }
        }

        // Burn Color, default = (1,1,1,1)
        [ParserTarget("burnColor")]
        public ColorParser BurnColorSetter
        {
            get { return BurnColor; }
            set { BurnColor = value; }
        }

        // Underwater Fog Factor, default = 0
        [ParserTarget("underwaterFogFactor")]
        public NumericParser<Single> UnderwaterFogFactorSetter
        {
            get { return UnderwaterFogFactor; }
            set { UnderwaterFogFactor = value; }
        }

        // Constructors
        public KSPBumpedSpecularLoader()
        {
        }

        [Obsolete("Creating materials from shader source String is no longer supported. Use Shader assets instead.")]
        public KSPBumpedSpecularLoader(String contents) : base(contents)
        {
        }

        public KSPBumpedSpecularLoader(Material material) : base(material)
        {
        }
    }
}
