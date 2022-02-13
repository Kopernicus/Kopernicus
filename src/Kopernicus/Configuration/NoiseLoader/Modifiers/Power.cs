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

using System.Diagnostics.CodeAnalysis;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Enumerations;

namespace Kopernicus.Configuration.NoiseLoader.Modifiers
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Power : NoiseLoader<LibNoise.Modifiers.Power>
    {
        [PreApply]
        [ParserTarget("Base", NameSignificance = NameSignificance.Type, Optional = false)]
        public INoiseLoader BaseModule { get; set; }

        [PreApply]
        [ParserTarget("Power", NameSignificance = NameSignificance.Type, Optional = false)]
        public INoiseLoader PowerModule { get; set; }

        public override void Apply(ConfigNode node)
        {
            Noise = new LibNoise.Modifiers.Power(BaseModule.Noise, PowerModule.Noise);
        }
    }
}