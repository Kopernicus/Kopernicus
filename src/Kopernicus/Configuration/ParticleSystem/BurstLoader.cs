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
        public class BurstLoader : ITypeParser<ParticleSystem.Burst>
        {
            // The curve object we are editing 
            private ParticleSystem.Burst _value;
            public ParticleSystem.Burst Value
            {
                get { return _value; }
                set { _value = value; }
            }
            
            [ParserTarget("maxCount")]
            [KittopiaDescription("Maximum number of bursts to be emitted.")]
            public NumericParser<Int16> maxCount
            {
                get { return _value.maxCount; }
                set { _value.maxCount = value; }
            }
            
            [ParserTarget("minCount")]
            [KittopiaDescription("Minimum number of bursts to be emitted.")]
            public NumericParser<Int16> minCount
            {
                get { return _value.minCount; }
                set { _value.minCount = value; }
            }
           
            [ParserTarget("time")]
            [KittopiaDescription("The time that each burst occurs.")]
            public NumericParser<Single> time
            {
                get { return _value.time; }
                set { _value.time = value; }
            }

            // Create a new curve 
            [KittopiaConstructor(KittopiaConstructor.Parameter.Empty, purpose = KittopiaConstructor.Purpose.Create)]
            public BurstLoader()
            {
                _value = new ParticleSystem.Burst();
            }

            // Edit an existing curve
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public BurstLoader(ParticleSystem.Burst burst)
            {
                _value = burst;
            }

            public static implicit operator ParticleSystem.Burst(BurstLoader loader)
            {
                return loader.Value;
            }

            public static implicit operator BurstLoader(ParticleSystem.Burst value)
            {
                return new BurstLoader(value);
            }
        }
    }
}