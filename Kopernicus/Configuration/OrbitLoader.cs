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
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Kopernicus
{
	namespace Configuration
	{
		// See: http://en.wikipedia.org/wiki/Argument_of_periapsis#mediaviewer/File:Orbit1.svg
		[RequireConfigType(ConfigType.Node)]
		public class OrbitLoader
		{
			// Orbit driver and orbit renderer we are generating data for
			private OrbitDriver orbitDriver;
			private OrbitRenderer orbitRenderer;

			// Orbit renderer color
			[ParserTarget("color", optional = true)]
			private ColorParser color 
			{
				set { orbitRenderer.orbitColor = value.value; }
			}

			// CamVsSmaRatios
			[ParserTarget("camVsSmaRatio", optional = true)]
			private NumericCollectionParser<float> camVsSmaRatioBounds 
			{
				set
				{
					// Throw an exception if an incorrect number of values were passed
					if(value.value.Count != 2)
						throw new Exception("camVsSmaRatioBounds: Incorrect number of arguments passed");

					// Set the bounds in orbit renderer
					orbitRenderer.lowerCamVsSmaRatio = value.value[0];
					orbitRenderer.upperCamVsSmaRatio = value.value[1];
				}
			}

			[ParserTarget("inclination", optional = true)]
			private NumericParser<double> inclination 
			{
				set { orbitDriver.orbit.inclination = value.value; }
			}
			
			[ParserTarget("eccentricity", optional = true)]
			private NumericParser<double> eccentricity
			{
				set { orbitDriver.orbit.eccentricity = value.value; }
			}

			[ParserTarget("semiMajorAxis", optional = true)]
			private NumericParser<double> semiMajorAxis
			{
				set { orbitDriver.orbit.semiMajorAxis = value.value; }
			}

			[ParserTarget("longitudeOfAscendingNode", optional = true)]
			private NumericParser<double> longitudeOfAscendingNode
			{
				set { orbitDriver.orbit.LAN = value.value; }
			}

			[ParserTarget("argumentOfPeriapsis", optional = true)]
			private NumericParser<double> argumentOfPeriapsis
			{
				set { orbitDriver.orbit.argumentOfPeriapsis = value.value; }
			}

			[ParserTarget("meanAnomalyAtEpoch", optional = true)]
			private NumericParser<double> meanAnomalyAtEpoch
			{
				set { orbitDriver.orbit.meanAnomalyAtEpoch = value.value; }
			}

			[ParserTarget("epoch", optional = true)]
			private NumericParser<double> epoch
			{
				set { orbitDriver.orbit.epoch = value.value; }
			}
			
			// Reference body to orbit
			[ParserTarget("referenceBody", optional = true)]
			public string referenceBody { get; private set; }

			// Copy orbit details provided
			public OrbitLoader (PSystemBody body)
			{
				// Reference body defaults to null
				referenceBody = null;

				// Get the orbit driver and renderer of this body
				orbitRenderer = body.orbitRenderer;
				orbitDriver = body.orbitDriver;

				// Make sure the orbit driver is in update mode
				orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;

				// Make sure we have an orbit object
				if(orbitDriver.orbit == null)
					orbitDriver.orbit = new Orbit();
			}
		}
	}
}

