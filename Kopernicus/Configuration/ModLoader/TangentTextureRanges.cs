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
			public class TangentTextureRanges : ModLoader, IParserEventSubscriber
			{
				// Actual PQS mod we are loading
                private PQSMod_TangentTextureRanges _mod;

				// highEnd
				[ParserTarget("highEnd", optional = true)]
				private NumericParser<double> highEnd
				{
					set { _mod.highEnd = value.value; }
				}

				// highStart
				[ParserTarget("highStart", optional = true)]
				private NumericParser<double> highStart
				{
					set { _mod.highStart = value.value; }
				}

                // lowEnd
				[ParserTarget("lowEnd", optional = true)]
				private NumericParser<double> lowEnd
				{
					set { _mod.lowEnd = value.value; }
				}

                // lowStart
				[ParserTarget("lowStart", optional = true)]
				private NumericParser<double> lowStart
				{
					set { _mod.lowStart = value.value; }
				}

                // modulo
				[ParserTarget("modulo", optional = true)]
				private NumericParser<double> modulo
				{
					set { _mod.modulo = value.value; }
				}

				void IParserEventSubscriber.Apply(ConfigNode node)
				{

				}

				void IParserEventSubscriber.PostApply(ConfigNode node)
				{

				}

                public TangentTextureRanges()
				{
					// Create the base mod
                    GameObject modObject = new GameObject("TangentTextureRanges");
					modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSMod_TangentTextureRanges>();
					base.mod = _mod;
				}

                public TangentTextureRanges(PQSMod template)
                {
                    _mod = template as PQSMod_TangentTextureRanges;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
			}
		}
	}
}

