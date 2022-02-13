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

namespace Kopernicus.Configuration.NoiseLoader.Modifiers
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TranslateInput : NoiseLoader<LibNoise.Modifiers.TranslateInput>
    {
        [ParserTarget("x")]
        public NumericParser<Double> X
        {
            get { return Noise.X; }
            set { Noise.X = value; }
        }

        [ParserTarget("y")]
        public NumericParser<Double> Y
        {
            get { return Noise.Y; }
            set { Noise.Y = value; }
        }

        [ParserTarget("z")]
        public NumericParser<Double> Z
        {
            get { return Noise.Z; }
            set { Noise.Z = value; }
        }

        [PreApply]
        [ParserTarget("Source", NameSignificance = NameSignificance.Type, Optional = false)]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public INoiseLoader SourceModule { get; set; }

        public override void Apply(ConfigNode node)
        {
            Noise = new LibNoise.Modifiers.TranslateInput(SourceModule.Noise, 0, 0, 0);
        }
    }
}