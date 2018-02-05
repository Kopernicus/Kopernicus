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
        public class RotationOverLifetimeModuleLoader : ITypeParser<ParticleSystem.RotationOverLifetimeModule>
        {
            // The module we are editing
            private ParticleSystem.RotationOverLifetimeModule _value;
            public ParticleSystem.RotationOverLifetimeModule Value
            {
                get { return _value; }
                set { _value = value; }
            }

            [ParserTarget("X")]
            [KittopiaDescription("Rotation by speed curve for the X axis.")]
            public MinMaxCurveLoader x
            {
                get { return _value.x; }
                set { _value.x = value; }
            }

            [ParserTarget("Y")]
            [KittopiaDescription("Rotation by speed curve for the Y axis.")]
            public MinMaxCurveLoader y
            {
                get { return _value.y; }
                set { _value.y = value; }
            }

            [ParserTarget("Z")]
            [KittopiaDescription("Rotation by speed curve for the Z axis.")]
            public MinMaxCurveLoader z
            {
                get { return _value.z; }
                set { _value.z = value; }
            }

            [ParserTarget("separateAxes")]
            [KittopiaDescription("Set the rotation by speed on each axis separately.")]
            public NumericParser<Boolean> separateAxes
            {
                get { return _value.separateAxes; }
                set { _value.separateAxes = value; }
            }

            [ParserTarget("enabled")]
            [KittopiaDescription("Enable/disable the Rotation By Speed module.")]
            public NumericParser<Boolean> enabled
            {
                get { return _value.enabled; }
                set { _value.enabled = value; }
            }

            // Create a new module
            [KittopiaConstructor(KittopiaConstructor.Parameter.Empty, purpose = KittopiaConstructor.Purpose.Create)]
            public RotationOverLifetimeModuleLoader()
            {
                _value = new ParticleSystem.RotationOverLifetimeModule();
            }

            // Edit an existing module
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public RotationOverLifetimeModuleLoader(ParticleSystem.RotationOverLifetimeModule module)
            {
                _value = module;
            }

            public static implicit operator ParticleSystem.RotationOverLifetimeModule(RotationOverLifetimeModuleLoader loader)
            {
                return loader.Value;
            }

            public static implicit operator RotationOverLifetimeModuleLoader(ParticleSystem.RotationOverLifetimeModule value)
            {
                return new RotationOverLifetimeModuleLoader(value);
            }
        }
    }
}