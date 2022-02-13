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
    public class ScaledPlanetRimAerialStandardLoader : ScaledPlanetRimAerialStandard
    {
        // Main Color, default = (1,1,1,1)
        [ParserTarget("color")]
        public ColorParser ColorSetter
        {
            get { return Color; }
            set { Color = value; }
        }

        // Specular Color, default = (0.5,0.5,0.5,1)
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

        // Base (RGB) Gloss (A), default = "white" { }
        [ParserTarget("texture")]
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
        [ParserTarget("normals")]
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

        // Opacity, default = 1
        [ParserTarget("opacity")]
        public NumericParser<Single> OpacitySetter
        {
            get { return Opacity; }
            set { Opacity = value; }
        }

        // Rim Power, default = 3
        [ParserTarget("rimPower")]
        public NumericParser<Single> RimPowerSetter
        {
            get { return RimPower; }
            set { RimPower = value; }
        }

        // Rim Blend, default = 1
        [ParserTarget("rimBlend")]
        public NumericParser<Single> RimBlendSetter
        {
            get { return RimBlend; }
            set { RimBlend = value; }
        }

        // RimColorRamp, default = "white" { }
        [ParserTarget("rimColorRamp")]
        public Texture2DParser RimColorRampSetter
        {
            get { return RimColorRamp; }
            set { RimColorRamp = value; }
        }

        [ParserTarget("rimColorRampScale")]
        public Vector2Parser RimColorRampScaleSetter
        {
            get { return RimColorRampScale; }
            set { RimColorRampScale = value; }
        }

        [ParserTarget("rimColorRampOffset")]
        public Vector2Parser RimColorRampOffsetSetter
        {
            get { return RimColorRampOffset; }
            set { RimColorRampOffset = value; }
        }

        [ParserTarget("Gradient")]
        [KittopiaHideOption]
        public Gradient RimColorRampGradientSetter
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
                RimColorRamp = ramp;
            }
        }

        // LightDirection, default = (1,0,0,0)
        [ParserTarget("localLightDirection")]
        public Vector4Parser LocalLightDirectionSetter
        {
            get { return LocalLightDirection; }
            set { LocalLightDirection = value; }
        }

        // Resource Map (RGB), default = "black" { }
        [ParserTarget("resourceMap")]
        public Texture2DParser ResourceMapSetter
        {
            get { return ResourceMap; }
            set { ResourceMap = value; }
        }

        [ParserTarget("resourceMapScale")]
        public Vector2Parser ResourceMapScaleSetter
        {
            get { return ResourceMapScale; }
            set { ResourceMapScale = value; }
        }

        [ParserTarget("resourceMapOffset")]
        public Vector2Parser ResourceMapOffsetSetter
        {
            get { return ResourceMapOffset; }
            set { ResourceMapOffset = value; }
        }

        // Constructors
        public ScaledPlanetRimAerialStandardLoader()
        {
        }

        [Obsolete("Creating materials from shader source String is no longer supported. Use Shader assets instead.")]
        public ScaledPlanetRimAerialStandardLoader(String contents) : base(contents)
        {
        }

        public ScaledPlanetRimAerialStandardLoader(Material material) : base(material)
        {
        }
    }
}
