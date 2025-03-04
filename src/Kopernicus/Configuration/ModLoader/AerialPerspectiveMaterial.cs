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
    public class AerialPerspectiveMaterial : ModLoader<PQSMod_AerialPerspectiveMaterial>
    {
        // atmosphereDepth
        [ParserTarget("atmosphereDepth")]
        public NumericParser<Single> AtmosphereDepth
        {
            get { return Mod.atmosphereDepth; }
            set { Mod.atmosphereDepth = value; }
        }

        // DEBUG_SetEveryFrame
        [ParserTarget("DEBUG_SetEveryFrame")]
        public NumericParser<Boolean> DebugSetEveryFrame
        {
            get { return Mod.DEBUG_SetEveryFrame; }
            set { Mod.DEBUG_SetEveryFrame = value; }
        }

        // Global density of the material
        [ParserTarget("globalDensity")]
        public NumericParser<Single> GlobalDensity
        {
            get { return Mod.globalDensity; }
            set { Mod.globalDensity = value; }
        }

        // heightFalloff
        [ParserTarget("heightFalloff")]
        public NumericParser<Single> HeightFalloff
        {
            get { return Mod.heightFalloff; }
            set { Mod.heightFalloff = value; }
        }

        // atmosphereDepth
        [ParserTarget("oceanDepth")]
        public NumericParser<Single> OceanDepth
        {
            get { return Mod.oceanDepth; }
            set { Mod.oceanDepth = value; }
        }
    }
}
