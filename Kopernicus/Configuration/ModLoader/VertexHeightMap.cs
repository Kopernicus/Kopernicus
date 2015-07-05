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
using UnityEngine;

namespace Kopernicus
{
	namespace Configuration
	{
		namespace ModLoader
		{
			[RequireConfigType(ConfigType.Node)]
			public class VertexHeightMap : ModLoader, IParserEventSubscriber
			{
				// Actual PQS mod we are loading
				private PQSMod_VertexHeightMap _mod;

				// The map texture for the planet
				[ParserTarget("map", optional = true)]
				private MapSOParser_GreyScale<MapSO> heightMap
				{
                    set { _mod.heightMap = value.value; }
				}

				// Height map offset
				[ParserTarget("offset", optional = true)]
				private NumericParser<double> heightMapOffset 
				{
					set { _mod.heightMapOffset = value.value; }
				}

				// Height map offset
				[ParserTarget("deformity", optional = true)]
				private NumericParser<double> heightMapDeformity
				{
					set { _mod.heightMapDeformity = value.value; }
				}

				// Height map offset
				[ParserTarget("scaleDeformityByRadius", optional = true)]
				private NumericParser<bool> scaleDeformityByRadius
				{
					set { _mod.scaleDeformityByRadius = value.value; }
				}


				void IParserEventSubscriber.Apply(ConfigNode node)
				{
                    
				}

				void IParserEventSubscriber.PostApply(ConfigNode node)
				{

				}

				public VertexHeightMap ()
				{
					// Create the base mod
					GameObject modObject = new GameObject ("VertexHeightMap");
					modObject.transform.parent = Utility.Deactivator;
					_mod = modObject.AddComponent<PQSMod_VertexHeightMap> ();
					_mod.requirements = PQS.ModiferRequirements.MeshCustomNormals | PQS.ModiferRequirements.VertexMapCoords;
					base.mod = _mod;
				}

                public VertexHeightMap(PQSMod template)
                {
                    _mod = template as PQSMod_VertexHeightMap;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
			}
		}
	}
}

