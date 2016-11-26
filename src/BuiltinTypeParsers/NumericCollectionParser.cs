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
using System.Reflection;

namespace Kopernicus
{
    /// <summary>
    /// Simple parser for numeric collections 
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    public class NumericCollectionParser<T> : IParsable
    {
        public List<T> value;
        public MethodInfo parserMethod;
        public void SetFromString(String s)
        {
            // Need a new list
            value = new List<T>();

            // Get the tokens of this string
            foreach (String e in s.Split(' '))
            {
                value.Add((T)parserMethod.Invoke(null, new Object[] { e }));
            }
        }
        public NumericCollectionParser()
        {
            // Get the parse method for this object
            parserMethod = typeof(T).GetMethod("Parse", new [] { typeof(String) });
        }
        public NumericCollectionParser(IEnumerable<T> i) : this()
        {
            value = new List<T>(i);
        }

        // Convert
        public static implicit operator T[] (NumericCollectionParser<T> parser)
        {
            return parser.value.ToArray();
        }
        public static implicit operator NumericCollectionParser<T>(T[] value)
        {
            return new NumericCollectionParser<T>(value);
        }
    }
}