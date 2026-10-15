

using Kopernicus.Components;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Parsing;

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
namespace Kopernicus.Configuration.ModLoader;

[RequireConfigType(ConfigType.Node)]
public class ScatterMask : ModLoader<PQSMod_ScatterMask>
{
    /// <summary>
    /// The latitude, in degrees, of the center of the scatter mask.
    /// </summary>
    [ParserTarget("lat", Optional = false)]
    public NumericParser<double> Latitude
    {
        get => Mod.lat;
        set => Mod.lat = value;
    }

    /// <summary>
    /// The longitude, in degrees, of the center of the scatter mask.
    /// </summary>
    [ParserTarget("lon", Optional = false)]
    public NumericParser<double> Longitude
    {
        get => Mod.lon;
        set => Mod.lon = value;
    }

    /// <summary>
    /// The distance, in meters, covered by any one edge of the scatter mask.
    /// </summary>
    [ParserTarget("width", Optional = false)]
    public NumericParser<double> Width
    {
        get => Mod.width;
        set => Mod.width = value;
    }

    /// <summary>
    /// The rotation angle applied to the scatter mask. 0 is +V being due north.
    /// </summary>
    [ParserTarget("angle")]
    public NumericParser<float> Angle
    {
        get => Mod.angle;
        set => Mod.angle = value;
    }

    /// <summary>
    /// The mask texture. This should be a greyscale texture. Black prevents
    /// scatters from spawning, anything else allows them.
    /// </summary>
    [ParserTarget("mask")]
    public MapSOParserGreyScale<MapSO> Mask
    {
        get => Mod.mask;
        set => Mod.mask = value;
    }

    /// <summary>
    /// Turn this on to show the scatter mask directly on the terrain.
    /// </summary>
    [ParserTarget("debugShowColorMap")]
    public NumericParser<bool> DebugShowColorMap
    {
        get => Mod.debugShowColorMap;
        set => Mod.debugShowColorMap = value;
    }
}

