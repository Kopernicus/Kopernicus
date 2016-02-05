/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Kopernicus.Components;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace Asteroids
        {
            [RequireConfigType(ConfigType.Node)]
            public class Location
            {
                // Loads the nearby-Orbits of this Asteroid
                [RequireConfigType(ConfigType.Node)]
                public class NearbyLoader
                {
                    // The body we are passing
                    [ParserTarget("body", optional = true)]
                    public string body { get; set; }

                    // The probability of this Orbit type
                    [ParserTarget("probability", optional = true)]
                    public NumericParser<float> probability { get; set; }

                    // Whether the body must be reached
                    [ParserTarget("reached", optional = true)]
                    public NumericParser<bool> reached { get; set; }
                }

                // Loads the flyby-Orbits of this Asteroid
                [RequireConfigType(ConfigType.Node)]
                public class FlybyLoader
                {
                    // The body we are passing
                    [ParserTarget("body", optional = true)]
                    public string body { get; set; }

                    // The minimum amount of days to closest approach
                    [ParserTarget("minDuration", optional = true)]
                    public NumericParser<float> minDuration { get; set; }

                    // The maximum amount of days to closest approach
                    [ParserTarget("maxDuration", optional = true)]
                    public NumericParser<float> maxDuration { get; set; }

                    // The probability of this Orbit type
                    [ParserTarget("probability", optional = true)]
                    public NumericParser<float> probability { get; set; }

                    // Whether the body must be reached
                    [ParserTarget("reached", optional = true)]
                    public NumericParser<bool> reached { get; set; }
                }

                // Loads the around-Orbits of this Asteroid
                [RequireConfigType(ConfigType.Node)]
                public class AroundLoader
                {
                    // The body we are passing
                    [ParserTarget("body", optional = true)]
                    public string body { get; set; }

                    // The minimum altitude
                    [ParserTarget("minAltitude", optional = true)]
                    public NumericParser<float> minAltitude { get; set; }

                    // The maximum altitude
                    [ParserTarget("maxAltitude", optional = true)]
                    public NumericParser<float> maxAltitude { get; set; }

                    // The probability of this Orbit type
                    [ParserTarget("probability", optional = true)]
                    public NumericParser<float> probability { get; set; }

                    // Whether the body must be reached
                    [ParserTarget("reached", optional = true)]
                    public NumericParser<bool> reached { get; set; }
                }

                // Nearby-Orbits
                [ParserTargetCollection("Nearby", allowMerge = true, optional = true)]
                public List<NearbyLoader> nearby = new List<NearbyLoader>();

                // Flyby-Orbits
                [ParserTargetCollection("Flyby", allowMerge = true, optional = true)]
                public List<FlybyLoader> flyby = new List<FlybyLoader>();

                // Around-Orbits
                [ParserTargetCollection("Around", allowMerge = true, optional = true)]
                public List<AroundLoader> around = new List<AroundLoader>();
            }
        }
    }
}
