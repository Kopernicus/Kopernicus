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

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class MaterialFadeAltitudeDouble : ModLoader<PQSMod_MaterialFadeAltitudeDouble>
    {
        // inFadeEnd
        [ParserTarget("inFadeEnd")]
        public NumericParser<Single> InFadeEnd
        {
            get { return Mod.inFadeEnd; }
            set { Mod.inFadeEnd = value; }
        }

        // inFadeStart
        [ParserTarget("inFadeStart")]
        public NumericParser<Single> InFadeStart
        {
            get { return Mod.inFadeStart; }
            set { Mod.inFadeStart = value; }
        }

        // outFadeEnd
        [ParserTarget("outFadeEnd")]
        public NumericParser<Single> OutFadeEnd
        {
            get { return Mod.outFadeEnd; }
            set { Mod.outFadeEnd = value; }
        }

        // outFadeStart
        [ParserTarget("outFadeStart")]
        public NumericParser<Single> OutFadeStart
        {
            get { return Mod.outFadeStart; }
            set { Mod.outFadeStart = value; }
        }

        // valueEnd
        [ParserTarget("valueEnd")]
        public NumericParser<Single> ValueEnd
        {
            get { return Mod.valueEnd; }
            set { Mod.valueEnd = value; }
        }

        // valueMid
        [ParserTarget("valueMid")]
        public NumericParser<Single> ValueMid
        {
            get { return Mod.valueMid; }
            set { Mod.valueMid = value; }
        }

        // valueStart
        [ParserTarget("valueStart")]
        public NumericParser<Single> ValueStart
        {
            get { return Mod.valueStart; }
            set { Mod.valueStart = value; }
        }
    }
}
