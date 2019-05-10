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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using UnityEngine;

namespace Kopernicus.ConfigParser.BuiltinTypeParsers
{
    /// <summary>
    /// Parser for a Single curve
    /// </summary>
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class FloatCurveParser : ITypeParser<FloatCurve>
    {
        /// <summary>
        /// The value that is being parsed
        /// </summary>
        public FloatCurve Value { get; set; }

        [ParserTargetCollection("self", Key = "key", NameSignificance = NameSignificance.Key)]
        public List<NumericCollectionParser<Single>> Keys
        {
            get
            {
                return Value.Curve.keys.Select(k =>
                        new NumericCollectionParser<Single>(new List<Single>
                        {
                            k.time,
                            k.value,
                            k.inTangent,
                            k.outTangent
                        }))
                    .ToList();
            }
            set
            {
                if (value == null)
                {
                    Value = null;
                    return;
                }
                Value.Curve.keys = new Keyframe[0];
                foreach (NumericCollectionParser<Single> key in value)
                {
                    if (key.Value.Count < 2)
                    {
                        Debug.LogError("FloatCurve: Invalid line. Requires two values, 'time' and 'value'");
                    }
                    if (key.Value.Count == 4)
                    {
                        Value.Add(key.Value[0], key.Value[1], key.Value[2], key.Value[3]);
                    }
                    else
                    {
                        Value.Add(key.Value[0], key.Value[1]);
                    }
                }
            }
        }

        /// <summary>
        /// Interface a class can implement to support conversion to a ConfigNode
        /// </summary>
        public ConfigNode ValueToNode()
        {
            if (Value == null)
            {
                return null;
            }
            
            ConfigNode node = new ConfigNode();
            Value.Save(node);
            return node;
        }
        
        /// <summary>
        /// Create a new FloatCurveParser
        /// </summary>
        public FloatCurveParser()
        {
            Value = new FloatCurve();
        }
        
        /// <summary>
        /// Create a new FloatCurveParser from an already existing value
        /// </summary>
        public FloatCurveParser(FloatCurve curve)
        {
            Value = curve;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator FloatCurve(FloatCurveParser parser)
        {
            return parser.Value;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator AnimationCurve(FloatCurveParser parser)
        {
            return parser.Value.Curve;
        }
        
        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator FloatCurveParser(FloatCurve value)
        {
            return new FloatCurveParser(value);
        }
        
        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator FloatCurveParser(AnimationCurve value)
        {
            return new FloatCurveParser(new FloatCurve(value?.keys ?? new Keyframe[0]));
        }
    }
}