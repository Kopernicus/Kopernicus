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
using System.Collections.Generic;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class ColorBySpeedModuleLoader : ITypeParser<ParticleSystem.ColorBySpeedModule>
        {
            // The module we are editing
            private ParticleSystem.ColorBySpeedModule _value;
            public ParticleSystem.ColorBySpeedModule Value
            {
                get { return _value; }
                set { _value = value; }
            }

            [ParserTarget("Color")]
            [KittopiaDescription("The curve controlling the particle colors.")]
            public MinMaxGradientLoader color
            {
                get { return _value.color; }
                set { _value.color = value; }
            }

            [ParserTarget("enabled")]
            [KittopiaDescription("Enable/disable the Color By Speed module.")]
            public NumericParser<Boolean> enabled
            {
                get { return _value.enabled; }
                set { _value.enabled = value; }
            }

            [ParserTarget("range")]
            [KittopiaDescription("Apply the color gradient between these minimum and maximum speeds.")]
            public Vector2Parser range
            {
                get { return _value.range; }
                set { _value.range = value; }
            }

            // Create a new module
            [KittopiaConstructor(KittopiaConstructor.Parameter.Empty, purpose = KittopiaConstructor.Purpose.Create)]
            public ColorBySpeedModuleLoader()
            {
                _value = new ParticleSystem.ColorBySpeedModule();
            }

            // Edit an existing module
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public ColorBySpeedModuleLoader(ParticleSystem.ColorBySpeedModule module)
            {
                _value = module;
            }

            public static implicit operator ParticleSystem.ColorBySpeedModule(ColorBySpeedModuleLoader loader)
            {
                return loader.Value;
            }

            public static implicit operator ColorBySpeedModuleLoader(ParticleSystem.ColorBySpeedModule value)
            {
                return new ColorBySpeedModuleLoader(value);
            }
        }
    }
}