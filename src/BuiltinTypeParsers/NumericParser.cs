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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Kopernicus
{
    /// <summary>
    /// Simple parser for numerics
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class NumericParser<T> : IParsable, ITypeParser<T>
    {
        /// <summary>
        /// The value that is being parsed
        /// </summary>
        public T Value { get; set; }
        
        /// <summary>
        /// The method that is used to parse the string
        /// </summary>
        private readonly MethodInfo _parserMethod;
        
        /// <summary>
        /// Parse the Value from a string
        /// </summary>
        public void SetFromString(String s)
        {
            Value = (T)_parserMethod.Invoke(null, new Object[] { s });
        }
        
        /// <summary>
        /// Create a new NumericParser
        /// </summary>
        public NumericParser()
        {
            // Get the parse method for this object
            _parserMethod = typeof(T).GetMethod("Parse", new [] { typeof(String) });
        }
        
        /// <summary>
        /// Create a new NumericParser from an already existing value
        /// </summary>
        public NumericParser(T i) : this()
        {
            Value = i;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator T(NumericParser<T> parser)
        {
            return parser.Value;
        }
        
        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator NumericParser<T>(T value)
        {
            return new NumericParser<T>(value);
        }
    }
}