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
		public class ScienceValues
		{
			// Science parameters we are going to be modifying
			public CelestialBodyScienceParams scienceParams { get; private set; }

			// Science multipler (?) for landed science
			[ParserTarget("landedDataValue", optional = true, allowMerge = false)]
			private NumericParser<float> landedDataValue 
			{
				set { scienceParams.LandedDataValue = value.value; }
			}

			// Science multipler (?) for splashed down science
			[ParserTarget("splashedDataValue", optional = true, allowMerge = false)]
			private NumericParser<float> splashedDataValue 
			{
				set { scienceParams.SplashedDataValue = value.value; }
			}

			// Science multipler (?) for flying low science
			[ParserTarget("flyingLowDataValue", optional = true, allowMerge = false)]
			private NumericParser<float> flyingLowDataValue 
			{
				set { scienceParams.FlyingLowDataValue = value.value; }
			}

			// Science multipler (?) for flying high science
			[ParserTarget("flyingHighDataValue", optional = true, allowMerge = false)]
			private NumericParser<float> flyingHighDataValue 
			{
				set { scienceParams.FlyingHighDataValue = value.value; }
			}
			
			// Science multipler (?) for in space low science
			[ParserTarget("inSpaceLowDataValue", optional = true, allowMerge = false)]
			private NumericParser<float> inSpaceLowDataValue
			{
				set { scienceParams.InSpaceLowDataValue = value.value; }
			}
			
			// Science multipler (?) for in space high science
			[ParserTarget("inSpaceHighDataValue", optional = true, allowMerge = false)]
			private NumericParser<float> inSpaceHighDataValue
			{
				set { scienceParams.InSpaceHighDataValue = value.value; }
			}
			
			// Some number describing recovery value (?) on this body.  Could be a multiplier
			// for value OR describe a multiplier for recovery of a craft returning from this
			// body....
			[ParserTarget("recoveryValue", optional = true, allowMerge = false)]
			private NumericParser<float> recoveryValue
			{
				set { scienceParams.RecoveryValue = value.value; }
			}

			// Altitude when "flying at <body>" transistions from/to "from <body>'s upper atmosphere"
			[ParserTarget("flyingAltitudeThreshold", optional = true, allowMerge = false)]
			private NumericParser<float> flyingAltitudeThreshold
			{
				set { scienceParams.flyingAltitudeThreshold = value.value; }
			}
			
			// Altitude when "in space low" transistions from/to "in space high"
			[ParserTarget("spaceAltitudeThreshold", optional = true, allowMerge = false)]
			private NumericParser<float> spaceAltitudeThreshold
			{
				set { scienceParams.spaceAltitudeThreshold = value.value; }
			}

			// Standard constructor takes a science parameters object
			public ScienceValues (CelestialBodyScienceParams scienceParams)
			{
				this.scienceParams = scienceParams;
			}
		}
	}
}

