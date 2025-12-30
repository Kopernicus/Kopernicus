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

using System;
using System.Diagnostics.CodeAnalysis;

namespace Kopernicus.ConfigParser.Attributes
{
    /// <summary>
    /// Attribute used to tag a class in another library to add it to the ParserTargets
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public class ParserTargetExternal : Attribute
    {
        /// <summary>
        /// Like the ParserTarget, if null, this will be determined via reflection
        /// </summary>
        public readonly String ConfigNodeName;

        /// <summary>
        /// The parser will look for this in the node with this name
        /// </summary>
        public readonly String ParentNodeName;

        /// <summary>
        /// The name of the mod calling the external target
        /// </summary>
        public readonly String ModName;

        // Constructor sets name
        public ParserTargetExternal(String parentNodeName, String configNodeName, String modName)
        {
            ParentNodeName = parentNodeName;
            ConfigNodeName = configNodeName;
            ModName = modName;
        }
    }
}