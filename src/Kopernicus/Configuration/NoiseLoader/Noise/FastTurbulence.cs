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
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace NoiseLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class FastTurbulence : NoiseLoader<LibNoise.FastTurbulence>
            {
                [ParserTarget("frequency")]
                public NumericParser<Double> frequency
                {
                    get { return noise.Frequency; }
                    set { noise.Frequency = value; }
                }

                [ParserTarget("roughness")]
                public NumericParser<Int32> roughness
                {
                    get { return noise.Roughness; }
                    set { noise.Roughness = Mathf.Clamp(value, 1, 30); }
                }

                [ParserTarget("power")]
                public NumericParser<Double> power
                {
                    get { return noise.Power; }
                    set { noise.Power = value; }
                }

                [ParserTarget("seed")]
                public NumericParser<Int32> seed
                {
                    get { return noise.Seed; }
                    set { noise.Seed = value; }
                }

                [PreApply]
                [ParserTarget("Source", NameSignificance = NameSignificance.Type)]
                public INoiseLoader module;

                public override void Apply(ConfigNode node)
                {
                    noise = new LibNoise.FastTurbulence(module.Noise);
                }
            }
        }
    }
}