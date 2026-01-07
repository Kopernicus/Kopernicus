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
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using UnityEngine;

namespace Kopernicus.ConfigParser.BuiltinTypeParsers
{
    /// <summary>
    /// Parser for quaternion
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class QuaternionParser : IParsable, ITypeParser<Quaternion>
    {
        /// <summary>
        /// The value that is being parsed
        /// </summary>
        public Quaternion Value { get; set; }

        /// <summary>
        /// Parse the Value from a string
        /// </summary>
        public void SetFromString(String s)
        {
            Value = ConfigNode.ParseQuaternion(s);
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            return ConfigNode.WriteQuaternion(Value);
        }

        /// <summary>
        /// Create a new QuaternionParser
        /// </summary>
        public QuaternionParser()
        {

        }

        /// <summary>
        /// Create a new QuaternionParser from an already existing value
        /// </summary>
        public QuaternionParser(Quaternion value)
        {
            Value = value;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator Quaternion(QuaternionParser parser)
        {
            return parser.Value;
        }

        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator QuaternionParser(Quaternion value)
        {
            return new QuaternionParser(value);
        }
    }

    /// <summary>
    /// Parser for dual quaternion
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class QuaternionDParser : IParsable, ITypeParser<QuaternionD>
    {
        /// <summary>
        /// The value that is being parsed
        /// </summary>
        public QuaternionD Value { get; set; }

        /// <summary>
        /// Parse the Value from a string
        /// </summary>
        public void SetFromString(String s)
        {
            Value = ConfigNode.ParseQuaternionD(s);
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            return ConfigNode.WriteQuaternion(Value);
        }

        /// <summary>
        /// Create a new QuaternionDParser
        /// </summary>
        public QuaternionDParser()
        {

        }

        /// <summary>
        /// Create a new QuaternionDParser from an already existing value
        /// </summary>
        public QuaternionDParser(QuaternionD value)
        {
            Value = value;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator QuaternionD(QuaternionDParser parser)
        {
            return parser.Value;
        }

        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator QuaternionDParser(QuaternionD value)
        {
            return new QuaternionDParser(value);
        }
    }
}