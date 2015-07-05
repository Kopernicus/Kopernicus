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
		public class ScaledPlanetSimpleLoader : ScaledPlanetSimple
		{
			// Wrapper functions for loading from config (until ParserTarget redo)
			[ParserTarget("color", optional = true)]
			private ColorParser colorSetter 
			{
				set { base.color = value.value; }
			}

			[ParserTarget("specular", optional = true)]
			private ColorParser specularSetter 
			{
				set { base.specColor = value.value; }
			}

			[ParserTarget("shininess", optional = true)]
			private NumericParser<float> shininessSetter 
			{
				set {  base.shininess = value.value; }
			}
			
			[ParserTarget("texture", optional = true)]
			private Texture2DParser textureSetter 
			{
				set { base.mainTexture = value.value; }
			}
			
			[ParserTarget("normals", optional = true)]
			private Texture2DParser normalsSetter 
			{
				set { base.bumpMap = value.value; }
			}
			
			[ParserTarget("resources", optional = true)]
			private Texture2DParser resourcesSetter 
			{
				set { base.resourceMap = value.value; }
			}
			
			public ScaledPlanetSimpleLoader () : base() { }
			public ScaledPlanetSimpleLoader (string contents) : base (contents) { }
			public ScaledPlanetSimpleLoader (Material material) : base(material) { }
		}
	}
}
