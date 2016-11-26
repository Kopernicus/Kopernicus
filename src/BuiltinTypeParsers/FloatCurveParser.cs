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

namespace Kopernicus
{
    /// <summary>
    /// Parser for a float curve
    /// </summary>
    [RequireConfigType(ConfigType.Node)]
    public class FloatCurveParser : IParserEventSubscriber
    {
        public FloatCurve curve { get; set; }

        // Build the curve from the data found in the node
        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            curve = new FloatCurve();
            curve.Load(node);
        }

        // We don't use this
        void IParserEventSubscriber.PostApply(ConfigNode node) { }

        // Default constructor
        public FloatCurveParser()
        {
            curve = null;
        }

        // Default constructor
        public FloatCurveParser(FloatCurve curve)
        {
            this.curve = curve;
        }

        // Convert
        public static implicit operator FloatCurve(FloatCurveParser parser)
        {
            return parser.curve;
        }
        public static implicit operator FloatCurveParser(FloatCurve value)
        {
            return new FloatCurveParser(value);
        }
    }
}