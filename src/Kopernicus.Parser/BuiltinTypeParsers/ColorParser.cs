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
using System.Reflection;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using UnityEngine;

namespace Kopernicus.ConfigParser.BuiltinTypeParsers
{
    /// <summary>
    /// Parser for color
    /// </summary>
    [RequireConfigType(ConfigType.Value)]
    public class ColorParser : IParsable, ITypeParser<Color>
    {
        /// <summary>
        /// The value that is being parsed
        /// </summary>
        public Color Value { get; set; }

        /// <summary>
        /// Parse the Value from a string
        /// </summary>
        public void SetFromString(String str)
        {
            if (str.StartsWith("RGBA("))
            {
                str = str.Replace("RGBA(", "");
                str = str.Replace(")", "");
                str = str.Replace(" ", "");
                String[] colorArray = str.Split(',');

                Value = new Color(Single.Parse(colorArray[0]) / 255, Single.Parse(colorArray[1]) / 255,
                    Single.Parse(colorArray[2]) / 255, Single.Parse(colorArray[3]) / 255);
            }
            else if (str.StartsWith("RGB("))
            {
                str = str.Replace("RGB(", "");
                str = str.Replace(")", "");
                str = str.Replace(" ", "");
                String[] colorArray = str.Split(',');

                Value = new Color(Single.Parse(colorArray[0]) / 255, Single.Parse(colorArray[1]) / 255,
                    Single.Parse(colorArray[2]) / 255, 1);
            }
            else if (str.StartsWith("HSBA("))
            {
                str = str.Replace("HSBA(", "");
                str = str.Replace(")", "");
                str = str.Replace(" ", "");
                String[] colorArray = str.Split(',');

                // Parse
                Single h = Single.Parse(colorArray[0]) / 255f;
                Single s = Single.Parse(colorArray[1]) / 255f;
                Single b = Single.Parse(colorArray[2]) / 255f;

                // RGB
                Color value = new Color(b, b, b, Single.Parse(colorArray[3]) / 255f);
                if (Math.Abs(s) > 0)
                {
                    Single max = b;
                    Single dif = b * s;
                    Single min = b - dif;
                    h *= 360f;

                    // Check
                    if (h < 60f)
                    {
                        value.r = max;
                        value.g = h * dif / 60f + min;
                        value.b = min;
                    }
                    else if (h < 120f)
                    {
                        value.r = -(h - 120f) * dif / 60f + min;
                        value.g = max;
                        value.b = min;
                    }
                    else if (h < 180f)
                    {
                        value.r = min;
                        value.g = max;
                        value.b = (h - 120f) * dif / 60f + min;
                    }
                    else if (h < 240f)
                    {
                        value.r = min;
                        value.g = -(h - 240f) * dif / 60f + min;
                        value.b = max;
                    }
                    else if (h < 300f)
                    {
                        value.r = (h - 240f) * dif / 60f + min;
                        value.g = min;
                        value.b = max;
                    }
                    else if (h <= 360f)
                    {
                        value.r = max;
                        value.g = min;
                        value.b = -(h - 360f) * dif / 60 + min;
                    }
                    else
                    {
                        value.r = 0;
                        value.g = 0;
                        value.b = 0;
                    }
                }

                Value = value;
            }
            else if (str.StartsWith("XKCD."))
            {
                PropertyInfo color = typeof(XKCDColors).GetProperty(str.Replace("XKCD.", ""),
                    BindingFlags.Static | BindingFlags.Public);
                if (color != null)
                {
                    Value = (Color)color.GetValue(null, null);
                }
            }
            else if (str.StartsWith("#"))
            {
                Value = XKCDColors.ColorTranslator.FromHtml(str);
            }
            else
            {
                Value = ConfigNode.ParseColor(str);
            }
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            return ConfigNode.WriteColor(Value);
        }

        /// <summary>
        /// Create a new ColorParser
        /// </summary>
        public ColorParser()
        {
            Value = Color.white;
        }

        /// <summary>
        /// Create a new ColorParser from an already existing Color
        /// </summary>
        public ColorParser(Color i)
        {
            Value = i;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator Color(ColorParser parser)
        {
            return parser.Value;
        }

        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator ColorParser(Color value)
        {
            return new ColorParser(value);
        }
    }
}