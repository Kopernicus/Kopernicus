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
			public class VertexVoronoi : ModLoader, IParserEventSubscriber
			{
				// Actual PQS mod we are loading
				private PQSMod_VertexVoronoi _mod;

				// Deformation of the Voronoi
				[ParserTarget("deformation", optional = true)]
				private NumericParser<double> deformation
				{
					set { _mod.deformation = value.value; }
				}

                // Displacement of the Voronoi
				[ParserTarget("displacement", optional = true)]
				private NumericParser<double> voronoiDisplacement
				{
					set { _mod.voronoiDisplacement = value.value; }
				}

                // Enabled distance of the Voronoi
				[ParserTarget("enableDistance", optional = true)]
				private NumericParser<bool> voronoiEnableDistance
				{
					set { _mod.voronoiEnableDistance = value.value; }
				}

                // Frequency of the Voronoi
				[ParserTarget("frequency", optional = true)]
				private NumericParser<double> frequency
				{
					set { _mod.voronoiFrequency = value.value; }
				}

                // Seed of the Voronoi
				[ParserTarget("seed", optional = true)]
				private NumericParser<int> seed
				{
					set { _mod.voronoiSeed = value.value; }
				}

				void IParserEventSubscriber.Apply(ConfigNode node)
				{

				}

				void IParserEventSubscriber.PostApply(ConfigNode node)
				{

				}

                public VertexVoronoi()
				{
					// Create the base mod
                    GameObject modObject = new GameObject("VertexVoronoi");
					modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSMod_VertexVoronoi>();
					base.mod = _mod;
				}

                public VertexVoronoi(PQSMod template)
                {
                    _mod = template as PQSMod_VertexVoronoi;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
			}
		}
	}
}

