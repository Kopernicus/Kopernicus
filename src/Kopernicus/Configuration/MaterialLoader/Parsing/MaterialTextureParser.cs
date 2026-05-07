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

using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;

namespace Kopernicus.Configuration.MaterialLoader.Parsing;

/// <summary>
/// A value-holding parser for shader texture properties. The parsed string
/// path is preserved so <see cref="BaseMaterialLoader.SetTexture(string, MaterialTextureParser)"/>
/// can route through the on-demand pipeline, BUILTIN/ lookups, or
/// synchronous loading as needed.
/// </summary>
[RequireConfigType(ConfigType.Value)]
public class MaterialTextureParser : IParsable
{
    public string Path { get; set; }

    public void SetFromString(string s) => Path = s;
    public string ValueToString() => Path;

    public MaterialTextureParser() { }
    public MaterialTextureParser(string path) => Path = path;

    public static implicit operator string(MaterialTextureParser parser) => parser?.Path;
    public static implicit operator MaterialTextureParser(string path) => new(path);
}
