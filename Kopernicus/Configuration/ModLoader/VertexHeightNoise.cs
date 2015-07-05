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
			public class VertexHeightNoise : ModLoader, IParserEventSubscriber
			{
				// Actual PQS mod we are loading
				private PQSMod_VertexHeightNoise _mod;

				// The deformity of the simplex terrain noise
				[ParserTarget("deformity", optional = true)]
				private NumericParser<float> deformity
				{
					set { _mod.deformity = value.value; }
				}

				// The frequency of the simplex terrain noise
				[ParserTarget("frequency", optional = true)]
				private NumericParser<float> frequency
				{
					set { _mod.frequency = value.value; }
				}

                // Octaves of the simplex height noise
				[ParserTarget("octaves", optional = true)]
				private NumericParser<int> octaves
				{
					set { _mod.octaves = value.value; }
				}

                // Persistence of the simplex height noise
				[ParserTarget("persistence", optional = true)]
				private NumericParser<float> persistence
				{
					set { _mod.persistance = value.value; }
				}

                // The seed of the simplex height noise
				[ParserTarget("seed", optional = true)]
				private NumericParser<int> seed
				{
					set { _mod.seed = value.value; }
				}

                // The type of the simplex height noise
				[ParserTarget("noiseType", optional = true)]
				private EnumParser<PQSMod_VertexHeightNoise.NoiseType> noiseType
				{
					set { _mod.noiseType = value.value; }
				}

                // The mode of the simplex height noise
				[ParserTarget("mode", optional = true)]
				private EnumParser<LibNoise.Unity.QualityMode> mode
				{
					set { _mod.mode = value.value; }
				}

                // The lacunarity of the simplex height noise
				[ParserTarget("lacunarity", optional = true)]
				private NumericParser<float> lacunarity
				{
					set { _mod.lacunarity = value.value; }
				}

				void IParserEventSubscriber.Apply(ConfigNode node)
				{

				}

				void IParserEventSubscriber.PostApply(ConfigNode node)
				{

				}

                public VertexHeightNoise()
				{
					// Create the base mod
                    GameObject modObject = new GameObject("VertexHeightNoise");
					modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSMod_VertexHeightNoise>();
                    _mod.requirements = PQS.ModiferRequirements.MeshColorChannel;
					base.mod = _mod;
				}

                public VertexHeightNoise(PQSMod template)
                {
                    _mod = template as PQSMod_VertexHeightNoise;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
			}
		}
	}
}

