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
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class VertexHeightMapStep : ModLoader<PQSMod_VertexHeightMapStep>
    {
        // The map texture for the planet
        [ParserTarget("map", Optional = false)]
        public Texture2DParser HeightMap
        {
            get { return Mod.heightMap; }
            set { Mod.heightMap = Utility.CreateReadable(value); }
        }

        // Height map offset
        [ParserTarget("offset")]
        public NumericParser<Double> HeightMapOffset
        {
            get { return Mod.heightMapOffset; }
            set { Mod.heightMapOffset = value; }
        }

        // Height map offset
        [ParserTarget("deformity")]
        public NumericParser<Double> HeightMapDeformity
        {
            get { return Mod.heightMapDeformity; }
            set { Mod.heightMapDeformity = value; }
        }

        // Height map offset
        [ParserTarget("scaleDeformityByRadius")]
        public NumericParser<Boolean> ScaleDeformityByRadius
        {
            get { return Mod.scaleDeformityByRadius; }
            set { Mod.scaleDeformityByRadius = value; }
        }

        [ParserTarget("coastHeight")]
        public NumericParser<Double> CoastHeight
        {
            get { return Mod.coastHeight; }
            set { Mod.coastHeight = value; }
        }
    }
}
