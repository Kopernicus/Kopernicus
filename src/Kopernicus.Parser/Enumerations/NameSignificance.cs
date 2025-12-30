/**
 * Kopernicus ConfigNode Parser
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

namespace Kopernicus.ConfigParser.Enumerations
{
    /// <summary>
    /// Types of node name significance
    ///
    /// None - parser infers type from generic in ICollection
    /// Type - parser infers type from node name, if collection is generic, Type
    /// must be a subclass of the generic.Otherwise, it doesn't mater
    /// Key  - only nodes with a certain name will get loaded
    /// </summary>
    public enum NameSignificance
    {
        None,
        Type,
        Key
    }
}