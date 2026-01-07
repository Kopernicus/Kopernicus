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
    /// Parser for vec2 
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Vector2Parser : IParsable, ITypeParser<Vector2>
    {
        /// <summary>
        /// The value that is being parsed
        /// </summary>
        public Vector2 Value { get; set; }

        /// <summary>
        /// Parse the Value from a string
        /// </summary>
        public void SetFromString(String s)
        {
            Value = ConfigNode.ParseVector2(s);
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            return ConfigNode.WriteVector(Value);
        }

        /// <summary>
        /// Create a new Vector2Parser
        /// </summary>
        public Vector2Parser()
        {

        }

        /// <summary>
        /// Create a new Vector2Parser from an already existing value
        /// </summary>
        public Vector2Parser(Vector2 value)
        {
            Value = value;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator Vector2(Vector2Parser parser)
        {
            return parser.Value;
        }

        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator Vector2Parser(Vector2 value)
        {
            return new Vector2Parser(value);
        }
    }

    /// <summary>
    /// Parser for vec3
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Vector3Parser : IParsable, ITypeParser<Vector3>
    {
        /// <summary>
        /// The value that is being parsed
        /// </summary>
        public Vector3 Value { get; set; }

        /// <summary>
        /// Parse the Value from a string
        /// </summary>
        public void SetFromString(String s)
        {
            Value = ConfigNode.ParseVector3(s);
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            return ConfigNode.WriteVector(Value);
        }

        /// <summary>
        /// Create a new Vector3Parser
        /// </summary>
        public Vector3Parser()
        {

        }

        /// <summary>
        /// Create a new Vector3Parser from an already existing value
        /// </summary>
        public Vector3Parser(Vector3 value)
        {
            Value = value;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator Vector3(Vector3Parser parser)
        {
            return parser.Value;
        }

        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator Vector3Parser(Vector3 value)
        {
            return new Vector3Parser(value);
        }
    }

    /// <summary>
    /// Parser for vec3d
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Vector3DParser : IParsable, ITypeParser<Vector3d>
    {
        /// <summary>
        /// The value that is being parsed
        /// </summary>
        public Vector3d Value { get; set; }

        /// <summary>
        /// Parse the Value from a string
        /// </summary>
        public void SetFromString(String s)
        {
            Value = ConfigNode.ParseVector3D(s);
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            return ConfigNode.WriteVector(Value);
        }

        /// <summary>
        /// Create a new Vector3DParser
        /// </summary>
        public Vector3DParser()
        {

        }

        /// <summary>
        /// Create a new Vector3DParser from an already existing value
        /// </summary>
        public Vector3DParser(Vector3d value)
        {
            Value = value;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator Vector3d(Vector3DParser parser)
        {
            return parser.Value;
        }

        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator Vector3DParser(Vector3d value)
        {
            return new Vector3DParser(value);
        }
    }

    /// <summary>
    /// Parser for vec4
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Vector4Parser : IParsable, ITypeParser<Vector4>
    {
        /// <summary>
        /// The value that is being parsed
        /// </summary>
        public Vector4 Value { get; set; }

        /// <summary>
        /// Parse the Value from a string
        /// </summary>
        public void SetFromString(String s)
        {
            Value = ConfigNode.ParseVector4(s);
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            return ConfigNode.WriteVector(Value);
        }

        /// <summary>
        /// Create a new Vector4Parser
        /// </summary>
        public Vector4Parser()
        {

        }

        /// <summary>
        /// Create a new Vector4Parser from an already existing value
        /// </summary>
        public Vector4Parser(Vector4 value)
        {
            Value = value;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator Vector4(Vector4Parser parser)
        {
            return parser.Value;
        }

        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator Vector4Parser(Vector4 value)
        {
            return new Vector4Parser(value);
        }
    }
}