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
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Parsing;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class MapDecal : ModLoader<PQSMod_MapDecal>
    {
        // absolute
        [ParserTarget("absolute")]
        public NumericParser<Boolean> Absolute
        {
            get { return Mod.absolute; }
            set { Mod.absolute = value; }
        }

        // absoluteOffset
        [ParserTarget("absoluteOffset")]
        public NumericParser<Single> AbsoluteOffset
        {
            get { return Mod.absoluteOffset; }
            set { Mod.absoluteOffset = value; }
        }

        // angle
        [ParserTarget("angle")]
        public NumericParser<Single> Angle
        {
            get { return Mod.angle; }
            set { Mod.angle = value; }
        }

        // colorMap
        [ParserTarget("colorMap")]
        public MapSOParserRGB<MapSO> ColorMap
        {
            get { return Mod.colorMap; }
            set { Mod.colorMap = value; }
        }

        // cullBlack
        [ParserTarget("cullBlack")]
        public NumericParser<Boolean> CullBlack
        {
            get { return Mod.cullBlack; }
            set { Mod.cullBlack = value; }
        }

        // DEBUG_HighlightInclusion
        [ParserTarget("DEBUG_HighlightInclusion")]
        public NumericParser<Boolean> DebugHighlightInclusion
        {
            get { return Mod.DEBUG_HighlightInclusion; }
            set { Mod.DEBUG_HighlightInclusion = value; }
        }

        // heightMap
        [ParserTarget("heightMap")]
        public MapSOParserGreyScale<MapSO> HeightMap
        {
            get { return Mod.heightMap; }
            set { Mod.heightMap = value; }
        }

        // heightMapDeformity
        [ParserTarget("heightMapDeformity")]
        public NumericParser<Double> HeightMapDeformity
        {
            get { return Mod.heightMapDeformity; }
            set { Mod.heightMapDeformity = value; }
        }

        // position
        [ParserTarget("position")]
        public Vector3Parser Position
        {
            get { return Mod.position; }
            set { Mod.position = value; }
        }

        // position v2
        [ParserTarget("Position")]
        private PositionParser Position2
        {
            set { Mod.position = value; }
        }

        // removeScatter
        [ParserTarget("removeScatter")]
        public NumericParser<Boolean> RemoveScatter
        {
            get { return Mod.removeScatter; }
            set { Mod.removeScatter = value; }
        }

        // radius
        [ParserTarget("radius")]
        public NumericParser<Double> Radius
        {
            get { return Mod.radius; }
            set { Mod.radius = value; }
        }

        // smoothColor
        [ParserTarget("smoothColor")]
        public NumericParser<Single> SmoothColor
        {
            get { return Mod.smoothColor; }
            set { Mod.smoothColor = value; }
        }

        // smoothHeight
        [ParserTarget("smoothHeight")]
        public NumericParser<Single> SmoothHeight
        {
            get { return Mod.smoothHeight; }
            set { Mod.smoothHeight = value; }
        }

        // useAlphaHeightSmoothing
        [ParserTarget("useAlphaHeightSmoothing")]
        public NumericParser<Boolean> UseAlphaHeightSmoothing
        {
            get { return Mod.useAlphaHeightSmoothing; }
            set { Mod.useAlphaHeightSmoothing = value; }
        }
    }
}
