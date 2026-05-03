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
/// A value-holding parser for shader multi-keyword properties (one of N
/// allowed string keywords). Validation against the allowed set lives on
/// <see cref="BaseMaterialLoader.SetMultiKeyword"/> so the property setter
/// only has to forward.
/// </summary>
[RequireConfigType(ConfigType.Value)]
public class MaterialMultiKeywordParser : IParsable
{
    public string Keyword { get; set; }

    public void SetFromString(string s) => Keyword = s;
    public string ValueToString() => Keyword;

    public MaterialMultiKeywordParser() { }
    public MaterialMultiKeywordParser(string keyword) => Keyword = keyword;

    public static implicit operator string(MaterialMultiKeywordParser parser) => parser?.Keyword;
    public static implicit operator MaterialMultiKeywordParser(string keyword) => new(keyword);
}
