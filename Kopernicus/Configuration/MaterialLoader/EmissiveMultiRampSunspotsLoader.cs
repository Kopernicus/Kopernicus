/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
* Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Kopernicus.MaterialWrapper;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class EmissiveMultiRampSunspotsLoader : EmissiveMultiRampSunspots
        {
            [ParserTarget("noiseMap", optional = true)]
            private Texture2DParser noiseMapSetter 
            {
                set { base.noiseMap = value.value; }
            }

            [ParserTarget("emitColor0", optional = true)]
            private ColorParser emitColor0Setter 
            {
                set { base.emitColor0 = value.value; }
            }

            [ParserTarget("emitColor1", optional = true)]
            private ColorParser emitColor1Setter 
            {
                set { base.emitColor1 = value.value; }
            }

            [ParserTarget("sunspotTexture", optional = true)]
            private Texture2DParser sunspotTextureSetter 
            {
                set { base.sunspotTex = value.value; }
            }
            
            [ParserTarget("sunspotPower", optional = true)]
            private NumericParser<float> sunspotPowerSetter 
            {
                set { base.sunspotPower = value.value; }
            }
            
            [ParserTarget("sunspotColor", optional = true)]
            private ColorParser sunspotColorSetter 
            {
                set { base.sunspotColor = value.value; }
            }

            [ParserTarget("rimColor", optional = true)]
            private ColorParser rimColorSetter 
            {
                set { base.rimColor = value.value; }
            }
            
            [ParserTarget("rimPower", optional = true)]
            private NumericParser<float> rimPowerSetter 
            {
                set { base.rimPower = value.value; }
            }
            
            [ParserTarget("rimBlend", optional = true)]
            private NumericParser<float> rimBlendSetter 
            {
                set { base.rimBlend = value.value; }
            }

            // Constructors
            public EmissiveMultiRampSunspotsLoader () : base() { }
            public EmissiveMultiRampSunspotsLoader (string contents) : base (contents) { }
            public EmissiveMultiRampSunspotsLoader (Material material) : base(material) { }
        }
    }
}
