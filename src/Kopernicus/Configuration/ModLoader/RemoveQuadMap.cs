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
    public class RemoveQuadMap : ModLoader<PQSMod_RemoveQuadMap>
    {
        // The map texture for the Quad Remover (?)
        [ParserTarget("map")]
        public MapSOParserGreyScale<MapSO> Map
        {
            get { return Mod.map; }
            set { Mod.map = value; }
        }

        // The deformity of the map for the Quad Remover (?)
        [ParserTarget("deformity")]
        public NumericParser<Single> MapDeformity
        {
            get { return Mod.mapDeformity; }
            set { Mod.mapDeformity = value; }
        }

        // The max. height for the Quad Remover (?)
        [ParserTarget("maxHeight")]
        public NumericParser<Single> MaxHeight
        {
            get { return Mod.maxHeight; }
            set { Mod.maxHeight = value; }
        }

        // The min texture for the Quad Remover (?)
        [ParserTarget("minHeight")]
        public NumericParser<Single> MinHeight
        {
            get { return Mod.minHeight; }
            set { Mod.minHeight = value; }
        }
    }
}
