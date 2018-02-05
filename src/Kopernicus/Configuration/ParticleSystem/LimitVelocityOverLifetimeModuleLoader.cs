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
        public class LimitVelocityOverLifetimeModuleLoader : ITypeParser<ParticleSystem.LimitVelocityOverLifetimeModule>
        {
            // The module we are editing
            private ParticleSystem.LimitVelocityOverLifetimeModule _value;
            public ParticleSystem.LimitVelocityOverLifetimeModule Value
            {
                get { return _value; }
                set { _value = value; }
            }

            [ParserTarget("dampen")]
            [KittopiaDescription("Controls how much the velocity that exceeds the velocity limit should be dampened.")]
            public NumericParser<Single> dampen
            {
                get { return _value.dampen; }
                set { _value.dampen = value; }
            }

            [ParserTarget("limit")]
            [KittopiaDescription("Maximum velocity curve, when not using one curve per axis.")]
            public MinMaxCurveLoader limit
            {
                get { return _value.limit; }
                set { _value.limit = value; }
            }

            [ParserTarget("limitX")]
            [KittopiaDescription("Maximum velocity curve for the X axis.")]
            public MinMaxCurveLoader limitX
            {
                get { return _value.limitX; }
                set { _value.limitX = value; }
            }

            [ParserTarget("limitY")]
            [KittopiaDescription("Maximum velocity curve for the Y axis.")]
            public MinMaxCurveLoader limitY
            {
                get { return _value.limitY; }
                set { _value.limitY = value; }
            }

            [ParserTarget("limitZ")]
            [KittopiaDescription("Maximum velocity curve for the Z axis.")]
            public MinMaxCurveLoader limitZ
            {
                get { return _value.limitZ; }
                set { _value.limitZ = value; }
            }

            [ParserTarget("separateAxes")]
            [KittopiaDescription("Set the velocity limit on each axis separately.")]
            public NumericParser<Boolean> separateAxes
            {
                get { return _value.separateAxes; }
                set { _value.separateAxes = value; }
            }

            [ParserTarget("space")]
            [KittopiaDescription("Specifies if the velocity limits are in local space (rotated with the transform) or world space.")]
            public EnumParser<ParticleSystemSimulationSpace> space
            {
                get { return _value.space; }
                set { _value.space = value; }
            }

            [ParserTarget("enabled")]
            [KittopiaDescription("Enable/disable the Limit Force Over Lifetime module.")]
            public NumericParser<Boolean> enabled
            {
                get { return _value.enabled; }
                set { _value.enabled = value; }
            }

            // Create a new module
            [KittopiaConstructor(KittopiaConstructor.Parameter.Empty, purpose = KittopiaConstructor.Purpose.Create)]
            public LimitVelocityOverLifetimeModuleLoader()
            {
                _value = new ParticleSystem.LimitVelocityOverLifetimeModule();
            }

            // Edit an existing module
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public LimitVelocityOverLifetimeModuleLoader(ParticleSystem.LimitVelocityOverLifetimeModule module)
            {
                _value = module;
            }

            public static implicit operator ParticleSystem.LimitVelocityOverLifetimeModule(LimitVelocityOverLifetimeModuleLoader loader)
            {
                return loader.Value;
            }

            public static implicit operator LimitVelocityOverLifetimeModuleLoader(ParticleSystem.LimitVelocityOverLifetimeModule value)
            {
                return new LimitVelocityOverLifetimeModuleLoader(value);
            }
        }
    }
}