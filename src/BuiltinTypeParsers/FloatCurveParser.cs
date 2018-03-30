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

using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Kopernicus
{
    /// <summary>
    /// Parser for a Single curve
    /// </summary>
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class FloatCurveParser : IParserEventSubscriber, ITypeParser<FloatCurve>, IConfigNodeWritable
    {
        /// <summary>
        /// The value that is being parsed
        /// </summary>
        public FloatCurve Value { get; set; }
        
        // Build the curve from the data found in the node
        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            Value = new FloatCurve();
            Value.Load(node);
        }

        // We don't use this
        void IParserEventSubscriber.PostApply(ConfigNode node) { }

        /// <summary>
        /// Interface a class can implment to support conversion to a ConfigNode
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
            Value = null;
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