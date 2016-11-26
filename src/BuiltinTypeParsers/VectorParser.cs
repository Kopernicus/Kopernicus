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
using UnityEngine;

namespace Kopernicus
{
    /// <summary>
    /// Parser for vec2 
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    public class Vector2Parser : IParsable
    {
        public Vector2 value;
        public void SetFromString(String s)
        {
            value = ConfigNode.ParseVector2(s);
        }
        public Vector2Parser()
        {

        }
        public Vector2Parser(Vector2 value)
        {
            this.value = value;
        }

        // Convert
        public static implicit operator Vector2(Vector2Parser parser)
        {
            return parser.value;
        }
        public static implicit operator Vector2Parser(Vector2 value)
        {
            return new Vector2Parser(value);
        }
    }

    /// <summary>
    /// Parser for vec3
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    public class Vector3Parser : IParsable
    {
        public Vector3 value;
        public void SetFromString(String s)
        {
            value = ConfigNode.ParseVector3(s);
        }
        public Vector3Parser()
        {

        }
        public Vector3Parser(Vector3 value)
        {
            this.value = value;
        }

        // Convert
        public static implicit operator Vector3(Vector3Parser parser)
        {
            return parser.value;
        }
        public static implicit operator Vector3Parser(Vector3 value)
        {
            return new Vector3Parser(value);
        }
    }

    /// <summary>
    /// Parser for vec3d
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    public class Vector3DParser : IParsable
    {
        public Vector3d value;
        public void SetFromString(String s)
        {
            value = ConfigNode.ParseVector3D(s);
        }
        public Vector3DParser()
        {

        }
        public Vector3DParser(Vector3d value)
        {
            this.value = value;
        }

        // Convert
        public static implicit operator Vector3d(Vector3DParser parser)
        {
            return parser.value;
        }
        public static implicit operator Vector3DParser(Vector3d value)
        {
            return new Vector3DParser(value);
        }
    }

    /// <summary>
    /// Parser for vec4
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    public class Vector4Parser : IParsable
    {
        public Vector4 value;
        public void SetFromString(String s)
        {
            value = ConfigNode.ParseVector4(s);
        }
        public Vector4Parser()
        {

        }
        public Vector4Parser(Vector4 value)
        {
            this.value = value;
        }

        // Convert
        public static implicit operator Vector4(Vector4Parser parser)
        {
            return parser.value;
        }
        public static implicit operator Vector4Parser(Vector4 value)
        {
            return new Vector4Parser(value);
        }
    }
}