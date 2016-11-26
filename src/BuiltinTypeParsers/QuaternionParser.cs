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
    /// Parser for quaternion
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    public class QuaternionParser : IParsable
    {
        public Quaternion value;
        public void SetFromString(String s)
        {
            value = ConfigNode.ParseQuaternion(s);
        }
        public QuaternionParser()
        {

        }
        public QuaternionParser(Quaternion value)
        {
            this.value = value;
        }

        // Convert
        public static implicit operator Quaternion(QuaternionParser parser)
        {
            return parser.value;
        }
        public static implicit operator QuaternionParser(Quaternion value)
        {
            return new QuaternionParser(value);
        }
    }

    /// <summary>
    /// Parser for dual quaternion
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    public class QuaternionDParser : IParsable
    {
        public QuaternionD value;
        public void SetFromString(String s)
        {
            value = ConfigNode.ParseQuaternion(s);
        }
        public QuaternionDParser()
        {

        }
        public QuaternionDParser(QuaternionD value)
        {
            this.value = value;
        }

        // Convert
        public static implicit operator QuaternionD(QuaternionDParser parser)
        {
            return parser.value;
        }
        public static implicit operator QuaternionDParser(QuaternionD value)
        {
            return new QuaternionDParser(value);
        }
    }
}