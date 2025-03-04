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
    public class EmissiveMultiRampSunspotsLoader : EmissiveMultiRampSunspots
    {
        // Ramp Map (RGBA), default = "white" { }
        [ParserTarget("rampMap")]
        public Texture2DParser RampMapSetter
        {
            get { return RampMap; }
            set { RampMap = value; }
        }

        [ParserTarget("rampMapScale")]
        public Vector2Parser RampMapScaleSetter
        {
            get { return RampMapScale; }
            set { RampMapScale = value; }
        }

        [ParserTarget("rampMapOffset")]
        public Vector2Parser RampMapOffsetSetter
        {
            get { return RampMapOffset; }
            set { RampMapOffset = value; }
        }

        // Noise Map (RGBA), default = "white" { }
        [ParserTarget("noiseMap")]
        public Texture2DParser NoiseMapSetter
        {
            get { return NoiseMap; }
            set { NoiseMap = value; }
        }

        [ParserTarget("noiseMapScale")]
        public Vector2Parser NoiseMapScaleSetter
        {
            get { return NoiseMapScale; }
            set { NoiseMapScale = value; }
        }

        [ParserTarget("noiseMapOffset")]
        public Vector2Parser NoiseMapOffsetSetter
        {
            get { return NoiseMapOffset; }
            set { NoiseMapOffset = value; }
        }

        // Emission Color 0, default = (1,1,1,1)
        [ParserTarget("emitColor0")]
        public ColorParser EmitColor0Setter
        {
            get { return EmitColor0; }
            set { EmitColor0 = value; }
        }

        // Emission Color 1, default = (1,1,1,1)
        [ParserTarget("emitColor1")]
        public ColorParser EmitColor1Setter
        {
            get { return EmitColor1; }
            set { EmitColor1 = value; }
        }

        // Sunspot Map (R), default = "white" { }
        [ParserTarget("sunspotTex")]
        public Texture2DParser SunspotTexSetter
        {
            get { return SunspotTex; }
            set { SunspotTex = value; }
        }

        [ParserTarget("sunspotTexScale")]
        public Vector2Parser SunspotTexScaleSetter
        {
            get { return SunspotTexScale; }
            set { SunspotTexScale = value; }
        }

        [ParserTarget("sunspotTexOffset")]
        public Vector2Parser SunspotTexOffsetSetter
        {
            get { return SunspotTexOffset; }
            set { SunspotTexOffset = value; }
        }

        // Sunspot Power, default = 1
        [ParserTarget("sunspotPower")]
        public NumericParser<Single> SunspotPowerSetter
        {
            get { return SunspotPower; }
            set { SunspotPower = value; }
        }

        // Sunspot Color, default = (0,0,0,0)
        [ParserTarget("sunspotColor")]
        public ColorParser SunspotColorSetter
        {
            get { return SunspotColor; }
            set { SunspotColor = value; }
        }

        // Rimlight Color, default = (1,1,1,1)
        [ParserTarget("rimColor")]
        public ColorParser RimColorSetter
        {
            get { return RimColor; }
            set { RimColor = value; }
        }

        // Rimlight Power, default = 0.2
        [ParserTarget("rimPower")]
        public NumericParser<Single> RimPowerSetter
        {
            get { return RimPower; }
            set { RimPower = value; }
        }

        // Rimlight Blend, default = 0.2
        [ParserTarget("rimBlend")]
        public NumericParser<Single> RimBlendSetter
        {
            get { return RimBlend; }
            set { RimBlend = value; }
        }

        // Constructors
        public EmissiveMultiRampSunspotsLoader()
        {
        }

        [Obsolete("Creating materials from shader source String is no longer supported. Use Shader assets instead.")]
        public EmissiveMultiRampSunspotsLoader(String contents) : base(contents)
        {
        }

        public EmissiveMultiRampSunspotsLoader(Material material) : base(material)
        {
        }
    }
}
