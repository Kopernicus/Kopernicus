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

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class Biome
        {
            // The map attribute object we are creating
            public CBAttributeMapSO.MapAttribute attribute { get; private set; }

            // The name of this biome
            [ParserTarget("name")]
            private string name 
            {
                set { attribute.name = value; }
            }

            // The science multiplier for this biome
            [ParserTarget("value")]
            private NumericParser<float> value 
            {
                set { attribute.value = value.value; }
            }

            // The color in the map for this attribute
            [ParserTarget("color")]
            private ColorParser color 
            {
                set { attribute.mapColor = value.value; }
            }

            // Allocate the biome descriptor
            public Biome ()
            {
                attribute = new CBAttributeMapSO.MapAttribute();
            }
            
            // Get reference to existing biome descriptor
            public Biome (CBAttributeMapSO.MapAttribute attribute)
            {
                this.attribute = attribute;
            }
        }
    }
}

