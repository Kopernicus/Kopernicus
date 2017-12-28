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
using System.Collections.Generic;
using System.Linq;

namespace Kopernicus
{
    /// <summary>
    /// Simple parser for String arrays
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    public class StringCollectionParser : IParsable, ITypeParser<List<String>>
    {
        /// <summary>
        /// The value that is being parsed
        /// </summary>
        public List<String> Value { get; set; }
        
        /// <summary>
        /// Parse the Value from a string
        /// </summary>
        public void SetFromString(String s)
        {
            // Need a new list
            Value = new List<String>(s.Split(',').Select(a => a.Trim()));
        }
        
        /// <summary>
        /// Create a new StringCollectionParser
        /// </summary>
        public StringCollectionParser()
        {
            
        }
        
        /// <summary>
        /// Create a new StringCollectionParser from already existing values
        /// </summary>
        public StringCollectionParser(List<String> i)
        {
            Value = i;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator List<String>(StringCollectionParser parser)
        {
            return parser.Value;
        }
        
        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator StringCollectionParser(List<String> value)
        {
            return new StringCollectionParser(value);
        }
    }
}