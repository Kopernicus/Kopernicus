/**
 * Kopernicus ConfigNode Parser
 * ====================================
 * Created by: Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P.
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
 * which is copyright 2011-2016 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 *
 * https://kerbalspaceprogram.com
 */

using System;

namespace Kopernicus
{
    /// <summary>
    /// Attribute used to tag a class in another library to add it to the ParserTargets
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ParserTargetExternal : Attribute
    {
        /// <summary>
        /// Like the ParserTarget, if null, this will be determined via reflection
        /// </summary>
        public String configNodeName;

        /// <summary>
        /// The parser will look for this in the node with this name
        /// </summary>
        public String parentNodeName;

        /// <summary>
        /// The name of the mod calling the external target
        /// </summary>
        public String modName;

        // Constructor sets name
        public ParserTargetExternal(String parentNodeName, String configNodeName, String modName)
        {
            this.parentNodeName = parentNodeName;
            this.configNodeName = configNodeName;
            this.modName = modName;
        }
    }
}