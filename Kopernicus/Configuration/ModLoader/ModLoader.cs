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
			[RequireConfigType(Kopernicus.Configuration.ConfigType.Node)]
			public class ModLoader
			{
				// The mod loader must always be able to return a mod
				public PQSMod mod 
				{
					get;
					set;
				}

                // Is the Mod added or patched (Logging relevant)
                public bool patched = false;

				// Mod loader provides basic PQS mod loading functions
				[ParserTarget("order", optional = true)]
				protected NumericParser<int> order
				{
					set { mod.order = value.value; }
				}

				// Mod loader provides basic PQS mod loading functions
				[ParserTarget("enabled", optional = true)]
				protected NumericParser<bool> enabled
				{
					set { mod.modEnabled = value.value; }
				}
			}
		}
	}
}

