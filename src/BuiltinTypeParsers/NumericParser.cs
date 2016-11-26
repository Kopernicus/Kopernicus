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
using System.Reflection;

namespace Kopernicus
{
    /// <summary>
    /// Simple parser for numerics
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    public class NumericParser<T> : IParsable
    {
        public T value;
        public MethodInfo parserMethod;

        public void SetFromString(String s)
        {
            value = (T)parserMethod.Invoke(null, new Object[] { s });
        }

        public NumericParser()
        {
            // Get the parse method for this object
            parserMethod = (typeof(T)).GetMethod("Parse", new [] { typeof(String) });
        }

        public NumericParser(T i) : this()
        {
            value = i;
        }

        // Convert
        public static implicit operator T(NumericParser<T> parser)
        {
            return parser.value;
        }
        public static implicit operator NumericParser<T>(T value)
        {
            return new NumericParser<T>(value);
        }
    }
}