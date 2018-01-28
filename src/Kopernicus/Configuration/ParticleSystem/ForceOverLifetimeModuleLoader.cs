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
using System.Linq;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class ForceOverLifetimeModuleLoader : ITypeParser<ParticleSystem.ForceOverLifetimeModule>
        {
            // The module we are editing
            private ParticleSystem.ForceOverLifetimeModule _value;
            public ParticleSystem.ForceOverLifetimeModule Value
            {
                get { return _value; }
                set { _value = value; }
            }

            [ParserTarget("randomized")]
            [KittopiaDescription("When randomly selecting values between two curves or constants, this flag will cause a new random force to be chosen on each frame.")]
            public NumericParser<Boolean> randomized
            {
                get { return _value.randomized; }
                set { _value.randomized = value; }
            }

            [ParserTarget("space")]
            [KittopiaDescription("Are the forces being applied in local or world space?")]
            public EnumParser<ParticleSystemSimulationSpace> space
            {
                get { return _value.space; }
                set { _value.space = value; }
            }

            [ParserTarget("X")]
            [KittopiaDescription("The curve defining particle forces in the X axis.")]
            public MinMaxCurveLoader x
            {
                get { return _value.x; }
                set { _value.x = value; }
            }

            [ParserTarget("Y")]
            [KittopiaDescription("The curve defining particle forces in the Y axis.")]
            public MinMaxCurveLoader y
            {
                get { return _value.y; }
                set { _value.y = value; }
            }

            [ParserTarget("Z")]
            [KittopiaDescription("The curve defining particle forces in the Z axis.")]
            public MinMaxCurveLoader z
            {
                get { return _value.z; }
                set { _value.z = value; }
            }

            [ParserTarget("enabled")]
            [KittopiaDescription("Enable/disable the Force Over Lifetime module.")]
            public NumericParser<Boolean> enabled
            {
                get { return _value.enabled; }
                set { _value.enabled = value; }
            }

            // Create a new module
            [KittopiaConstructor(KittopiaConstructor.Parameter.Empty, purpose = KittopiaConstructor.Purpose.Create)]
            public ForceOverLifetimeModuleLoader()
            {
                _value = new ParticleSystem.ForceOverLifetimeModule();
            }

            // Edit an existing module
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public ForceOverLifetimeModuleLoader(ParticleSystem.ForceOverLifetimeModule module)
            {
                _value = module;
            }

            public static implicit operator ParticleSystem.ForceOverLifetimeModule(ForceOverLifetimeModuleLoader loader)
            {
                return loader.Value;
            }

            public static implicit operator ForceOverLifetimeModuleLoader(ParticleSystem.ForceOverLifetimeModule value)
            {
                return new ForceOverLifetimeModuleLoader(value);
            }
        }
    }
}