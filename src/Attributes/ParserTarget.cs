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
    /// Attribute used to tag a property or field which can be targeted by the parser
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class ParserTarget : Attribute
    {
        /// <summary>
        /// Storage key in config node tree.  If null, key is determined with reflection
        /// </summary>
        public String fieldName;

        /// <summary>
        /// Flag indiciating whether the presence of this value is required
        /// </summary>
        public Boolean optional = true;

        /// <summary>
        /// Flag indiciating whether the contents of the config tree can be merged
        /// via reflection with a potentially present field.  If the field is null,
        /// this flag is disregarged
        /// </summary>
        public Boolean allowMerge = false;

        /// <summary>
        /// Flag indicating whether the parser should try to call the getter on properties on this object
        /// </summary>
        public Boolean getChild = true;

        /// <summary>
        /// Whether the parser should merge all values in the config node into one
        /// </summary>
        public String getAll = null;

        // Constructor sets name
        public ParserTarget(String fieldName)
        {
            this.fieldName = fieldName;
        }
    }
}