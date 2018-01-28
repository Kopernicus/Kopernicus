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
        public class EmissionModuleLoader : ITypeParser<ParticleSystem.EmissionModule>
        {
            // The module we are editing
            private ParticleSystem.EmissionModule _value;
            public ParticleSystem.EmissionModule Value
            {
                get { return _value; }
                set { _value = value; }
            }

            [ParserTarget("rate")]
            [KittopiaDescription("The rate at which new particles are spawned.")]
            public MinMaxCurveLoader rate
            {
                get { return _value.rate; }
                set { _value.rate = value.Value; }
            }

            [ParserTarget("type")]
            [KittopiaDescription("The emission type.")]
            public EnumParser<ParticleSystemEmissionType> type
            {
                get { return _value.type; }
                set { _value.type = value; }
            }

            [ParserTarget("enabled")]
            [KittopiaDescription("Enable/disable the Emission module.")]
            public NumericParser<Boolean> enabled
            {
                get { return _value.enabled; }
                set { _value.enabled = value; }
            }
            
            [ParserTargetCollection("Bursts", AllowMerge = true)]
            public CallbackList<BurstLoader> bursts { get; set; }

            // Create a new module
            [KittopiaConstructor(KittopiaConstructor.Parameter.Empty, purpose = KittopiaConstructor.Purpose.Create)]
            public EmissionModuleLoader()
            {
                _value = new ParticleSystem.EmissionModule();
                bursts = new CallbackList<BurstLoader>(e =>
                {
                    _value.SetBursts(bursts.Select(b => b.Value).ToArray());
                });
            }

            // Edit an existing module
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public EmissionModuleLoader(ParticleSystem.EmissionModule module)
            {
                _value = module;
                bursts = new CallbackList<BurstLoader>(e =>
                {
                    _value.SetBursts(bursts.Select(b => b.Value).ToArray());
                });
                
                ParticleSystem.Burst[] burstArray = new ParticleSystem.Burst[module.burstCount];
                module.GetBursts(burstArray);
                for (Int32 i = 0; i < module.burstCount; i++)
                {
                    bursts.Add(burstArray[i], i + 1 == module.burstCount);
                }
            }

            public static implicit operator ParticleSystem.EmissionModule(EmissionModuleLoader loader)
            {
                return loader.Value;
            }

            public static implicit operator EmissionModuleLoader(ParticleSystem.EmissionModule value)
            {
                return new EmissionModuleLoader(value);
            }
        }
    }
}