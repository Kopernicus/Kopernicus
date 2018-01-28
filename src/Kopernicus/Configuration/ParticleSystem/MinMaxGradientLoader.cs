/**
 * Kopernicus Planetary System Modifier
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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class MinMaxGradientLoader : ITypeParser<ParticleSystem.MinMaxGradient>
        {
            // The gradient object we are editing 
            private ParticleSystem.MinMaxGradient _value;
            public ParticleSystem.MinMaxGradient Value
            {
                get { return _value; }
                set { _value = value; }
            }
            
            [ParserTarget("color")]
            [KittopiaDescription("A constant color. (mode = Color)")]
            public ColorParser color
            {
                get { return _value.color; }
                set { _value.color = value; }
            }
            
            [ParserTarget("colorMax")]
            [KittopiaDescription("A constant color for the upper bound. (mode = TwoColors)")]
            public ColorParser colorMax
            {
                get { return _value.colorMax; }
                set { _value.colorMax = value; }
            }
           
            [ParserTarget("colorMin")]
            [KittopiaDescription("A constant color for the lower bound. (mode = TwoColors)")]
            public ColorParser colorMin
            {
                get { return _value.colorMin; }
                set { _value.colorMin = value; }
            }
            
            [ParserTarget("Gradient")]
            [KittopiaDescription("The gradient. (mode = Gradient)")]
            public Gradient gradient
            {
                get { return _value.gradient; }
                set { _value.gradient = value; }
            }
            
            [ParserTarget("GradientMax")]
            [KittopiaDescription("A gradient for the upper bound. (mode = TwoGradients)")]
            public Gradient gradientMax
            {
                get { return _value.gradientMax; }
                set { _value.gradientMax = value; }
            }
            
            [ParserTarget("GradientMin")]
            [KittopiaDescription("A gradient for the lower bound. (mode = TwoGradients)")]
            public Gradient gradientMin
            {
                get { return _value.gradientMin; }
                set { _value.gradientMin = value; }
            }
            
            [ParserTarget("mode")]
            [KittopiaDescription("The mode that the min-max gradient will use to evaluate colors.")]
            public EnumParser<ParticleSystemGradientMode> mode
            {
                get { return _value.mode; }
                set { _value.mode = value; }
            }

            // Create a new gradient 
            [KittopiaConstructor(KittopiaConstructor.Parameter.Empty, purpose = KittopiaConstructor.Purpose.Create)]
            public MinMaxGradientLoader()
            {
                _value = new ParticleSystem.MinMaxGradient();
            }

            // Edit an existing gradient
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public MinMaxGradientLoader(ParticleSystem.MinMaxGradient gradient)
            {
                _value = gradient;
            }

            public static implicit operator ParticleSystem.MinMaxGradient(MinMaxGradientLoader loader)
            {
                return loader.Value;
            }

            public static implicit operator MinMaxGradientLoader(ParticleSystem.MinMaxGradient value)
            {
                return new MinMaxGradientLoader(value);
            }
        }
    }
}