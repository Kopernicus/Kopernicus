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

using Kopernicus;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace NoiseLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class Power : NoiseLoader<LibNoise.Modifiers.Power>
            {
                [PreApply]
                [ParserTarget("Base", nameSignificance = NameSignificance.Type, optional = false)]
                public INoiseLoader baseModule;

                [PreApply]
                [ParserTarget("Power", nameSignificance = NameSignificance.Type, optional = false)]
                public INoiseLoader powerModule;

                public override void Apply(ConfigNode node)
                {
                    noise = new LibNoise.Modifiers.Power(baseModule.Noise, powerModule.Noise);
                }
            }
        }
    }
}