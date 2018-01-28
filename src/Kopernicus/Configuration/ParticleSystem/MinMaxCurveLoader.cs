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

using System;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class MinMaxCurveLoader : ITypeParser<ParticleSystem.MinMaxCurve>
        {
            // The curve object we are editing 
            private ParticleSystem.MinMaxCurve _value;
            public ParticleSystem.MinMaxCurve Value
            {
                get { return _value; }
                set { _value = value; }
            }
            
            [ParserTarget("constant")]
            [KittopiaDescription("Constant value for the curve (mode = Constant)")]
            public NumericParser<Single> constant
            {
                get { return _value.constant; }
                set { _value.constant = value; }
            }
            
            [ParserTarget("constantMin")]
            [KittopiaDescription("Constant lower bound for the curve (mode = TwoConstants)")]
            public NumericParser<Single> constantMin
            {
                get { return _value.constantMin; }
                set { _value.constantMin = value; }
            }
           
            [ParserTarget("constantMax")]
            [KittopiaDescription("Constant upper bound for the curve (mode = TwoConstants)")]
            public NumericParser<Single> constantMax
            {
                get { return _value.constantMax; }
                set { _value.constantMax = value; }
            }
            
            [ParserTarget("curveScalar")]
            [KittopiaDescription("A multiplier that is applied to the curves (mode = Curve / mode = TwoCurves)")]
            public NumericParser<Single> curveScalar
            {
                get { return _value.curveScalar; }
                set { _value.curveScalar = value; }
            }
            
            [ParserTarget("Curve")]
            [KittopiaDescription("The curve (mode = Curve)")]
            public FloatCurveParser curve
            {
                get { return _value.curve; }
                set { _value.curve = value; }
            }
            
            [ParserTarget("CurveMax")]
            [KittopiaDescription("Curve that defines the upper bound of the final curve (mode = TwoCurves)")]
            public FloatCurveParser curveMax
            {
                get { return _value.curveMax; }
                set { _value.curveMax = value; }
            }
            
            [ParserTarget("CurveMin")]
            [KittopiaDescription("Curve that defines the lower bound of the final curve (mode = TwoCurves)")]
            public FloatCurveParser curveMin
            {
                get { return _value.curveMin; }
                set { _value.curveMin = value; }
            }
            
            [ParserTarget("mode")]
            [KittopiaDescription("The mode that defines how the MinMaxCurve should use it's properties")]
            public EnumParser<ParticleSystemCurveMode> mode
            {
                get { return _value.mode; }
                set { _value.mode = value; }
            }

            // Create a new curve 
            [KittopiaConstructor(KittopiaConstructor.Parameter.Empty, purpose = KittopiaConstructor.Purpose.Create)]
            public MinMaxCurveLoader()
            {
                _value = new ParticleSystem.MinMaxCurve();
            }

            // Edit an existing curve
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public MinMaxCurveLoader(ParticleSystem.MinMaxCurve curve)
            {
                _value = curve;
            }

            public static implicit operator ParticleSystem.MinMaxCurve(MinMaxCurveLoader loader)
            {
                return loader.Value;
            }

            public static implicit operator MinMaxCurveLoader(ParticleSystem.MinMaxCurve value)
            {
                return new MinMaxCurveLoader(value);
            }
        }
    }
}