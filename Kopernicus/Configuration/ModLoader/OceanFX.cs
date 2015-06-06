/**
 * Kopernicus Planetary System Modifier
 * Copyright (C) 2014 Bryce C Schroeder (bryce.schroeder@gmail.com), Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * http://www.ferazelhosting.net/~bryce/contact.html
 * 
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
            public class OceanFX : ModLoader, IParserEventSubscriber
			{
				// Actual PQS mod we are loading
                private PQSMod_OceanFX _mod;

                // angle
				[ParserTarget("angle", optional = true)]
				private NumericParser<float> angle
				{
					set { _mod.angle = value.value; }
				}

                // The deformity of the map for the Quad Remover (?)
				[ParserTarget("blendA", optional = true)]
				private NumericParser<float> blendA
				{
					set { _mod.blendA = value.value; }
				}

                // blendB
				[ParserTarget("blendB", optional = true)]
				private NumericParser<float> blendB
				{
					set { _mod.blendB = value.value; }
				}

                // bump
				[ParserTarget("bump", optional = true)]
				private Texture2DParser bump
				{
					set { _mod.bump = value.value; }
				}

                // framesPerSecond
				[ParserTarget("framesPerSecond", optional = true)]
				private NumericParser<float> framesPerSecond
				{
					set { _mod.framesPerSecond = value.value; }
				}

                // fresnel (???)
				[ParserTarget("fresnel", optional = true)]
				private Texture2DParser fresnel
				{
					set { _mod.fresnel = value.value; }
				}

                // oceanOpacity
				[ParserTarget("oceanOpacity", optional = true)]
				private NumericParser<float> oceanOpacity
				{
					set { _mod.oceanOpacity = value.value; }
				}

                // spaceAltitude
				[ParserTarget("spaceAltitude", optional = true)]
				private NumericParser<double> spaceAltitude
				{
					set { _mod.spaceAltitude = value.value; }
				}

                // spaceSurfaceBlend
				[ParserTarget("spaceSurfaceBlend", optional = true)]
				private NumericParser<float> spaceSurfaceBlend
				{
					set { _mod.spaceSurfaceBlend = value.value; }
				}

                // specColor
				[ParserTarget("specColor", optional = true)]
				private ColorParser specColor
				{
					set { _mod.specColor = value.value; }
				}

                // texBlend
				[ParserTarget("texBlend", optional = true)]
				private NumericParser<float> texBlend
				{
					set { _mod.texBlend = value.value; }
				}

                // txIndex
				[ParserTarget("txIndex", optional = true)]
				private NumericParser<int> txIndex
				{
					set { _mod.txIndex = value.value; }
				}

                // waterMainLength
				[ParserTarget("waterMainLength", optional = true)]
				private NumericParser<float> waterMainLength
				{
					set { _mod.waterMainLength = value.value; }
				}

				void IParserEventSubscriber.Apply(ConfigNode node)
				{
                    
				}

				void IParserEventSubscriber.PostApply(ConfigNode node)
				{

				}

                public OceanFX()
				{
					// Create the base mod (I need to instance this one, because some parameters aren't loadable. :( )
                    PSystemBody Body = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, "Laythe");
                    foreach (PQS ocean in Body.pqsVersion.GetComponentsInChildren<PQS>(true))
                    {
                        if (ocean.name == "LaytheOcean")
                        {
                            _mod = PQSMod.Instantiate(ocean.GetComponentsInChildren<PQSMod_OceanFX>(true)[0]) as PQSMod_OceanFX;
                            _mod.gameObject.transform.parent = Utility.Deactivator;
                            _mod.name = "OceanFX";
                            _mod.gameObject.name = "OceanFX";
                            _mod.transform.name = "OceanFX";
                        }
                    } 
					base.mod = _mod;
				}
			}
		}
	}
}

