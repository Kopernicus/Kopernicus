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
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Diagnostics.CodeAnalysis;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using UnityEngine;

namespace Kopernicus.Configuration.NoiseLoader.Noise
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class FastTurbulence : NoiseLoader<LibNoise.FastTurbulence>
    {
        [ParserTarget("frequency")]
        public NumericParser<Double> Frequency
        {
            get { return Noise.Frequency; }
            set { Noise.Frequency = value; }
        }

        [ParserTarget("roughness")]
        public NumericParser<Int32> Roughness
        {
            get { return Noise.Roughness; }
            set { Noise.Roughness = Mathf.Clamp(value, 1, 30); }
        }

        [ParserTarget("power")]
        public NumericParser<Double> Power
        {
            get { return Noise.Power; }
            set { Noise.Power = value; }
        }

        [ParserTarget("seed")]
        public NumericParser<Int32> Seed
        {
            get { return Noise.Seed; }
            set { Noise.Seed = value; }
        }

        [PreApply]
        [ParserTarget("Source", NameSignificance = NameSignificance.Type)]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public INoiseLoader Module { get; set; }

        public override void Apply(ConfigNode node)
        {
            Noise = new LibNoise.FastTurbulence(Module.Noise);
        }
    }
}