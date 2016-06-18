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
                    [ParserTarget("body")]
                    public string body { get; set; }

                    // eccentricity
                    [ParserTarget("eccentricity")]
                    public RandomRangeLoader eccentricity = new RandomRangeLoader(0.0001f, 0.01f);

                    // semiMajorAxis
                    [ParserTarget("semiMajorAxis")]
                    public RandomRangeLoader semiMajorAxis = new RandomRangeLoader(0.999f, 1.001f);

                    // inclination
                    [ParserTarget("inclination")]
                    public RandomRangeLoader inclination = new RandomRangeLoader(-0.001f, 0.001f);

                    // longitudeOfAscendingNode
                    [ParserTarget("longitudeOfAscendingNode")]
                    public RandomRangeLoader longitudeOfAscendingNode = new RandomRangeLoader(0.999f, 1.001f);

                    // argumentOfPeriapsis
                    [ParserTarget("argumentOfPeriapsis")]
                    public RandomRangeLoader argumentOfPeriapsis = new RandomRangeLoader(0.999f, 1.001f);

                    // meanAnomalyAtEpoch
                    [ParserTarget("meanAnomalyAtEpoch")]
                    public RandomRangeLoader meanAnomalyAtEpoch = new RandomRangeLoader(0.999f, 1.001f);

                    // The probability of this Orbit type
                    [ParserTarget("probability")]
                    public NumericParser<float> probability { get; set; }

                    // Whether the body must be reached
                    [ParserTarget("reached")]
                    public NumericParser<bool> reached { get; set; }
                }

                // Loads the flyby-Orbits of this Asteroid
                [RequireConfigType(ConfigType.Node)]
                public class FlybyLoader
                {
                    // The body we are passing
                    [ParserTarget("body")]
                    public string body { get; set; }

                    // The minimum amount of days to closest approach
                    [ParserTarget("minDuration")]
                    public NumericParser<float> minDuration { get; set; }

                    // The maximum amount of days to closest approach
                    [ParserTarget("maxDuration")]
                    public NumericParser<float> maxDuration { get; set; }

                    // The probability of this Orbit type
                    [ParserTarget("probability")]
                    public NumericParser<float> probability { get; set; }

                    // Whether the body must be reached
                    [ParserTarget("reached")]
                    public NumericParser<bool> reached { get; set; }
                }

                // Loads the around-Orbits of this Asteroid
                [RequireConfigType(ConfigType.Node)]
                public class AroundLoader
                {
                    // The body we are passing
                    [ParserTarget("body")]
                    public string body { get; set; }

                    // eccentricity
                    [ParserTarget("eccentricity")]
                    public RandomRangeLoader eccentricity = new RandomRangeLoader(0.0001f, 0.01f);

                    // semiMajorAxis
                    [ParserTarget("semiMajorAxis")]
                    public RandomRangeLoader semiMajorAxis { get; set; }

                    // inclination
                    [ParserTarget("inclination")]
                    public RandomRangeLoader inclination = new RandomRangeLoader(-0.001f, 0.001f);

                    // longitudeOfAscendingNode
                    [ParserTarget("longitudeOfAscendingNode")]
                    public RandomRangeLoader longitudeOfAscendingNode = new RandomRangeLoader(0.999f, 1.001f);

                    // argumentOfPeriapsis
                    [ParserTarget("argumentOfPeriapsis")]
                    public RandomRangeLoader argumentOfPeriapsis = new RandomRangeLoader(0.999f, 1.001f);

                    // meanAnomalyAtEpoch
                    [ParserTarget("meanAnomalyAtEpoch")]
                    public RandomRangeLoader meanAnomalyAtEpoch = new RandomRangeLoader(0.999f, 1.001f);

                    // epoch
                    [ParserTarget("epoch")]
                    public RandomRangeLoader epoch = new RandomRangeLoader(0.999f, 1.001f);

                    // The probability of this Orbit type
                    [ParserTarget("probability")]
                    public NumericParser<float> probability { get; set; }

                    // Whether the body must be reached
                    [ParserTarget("reached")]
                    public NumericParser<bool> reached { get; set; }
                }

                // Loads a random range value
                [RequireConfigType(ConfigType.Node)]
                public class RandomRangeLoader
                {
                    // The min value
                    [ParserTarget("minValue")]
                    public NumericParser<float> minValue { get; set; }

                    // The max value
                    [ParserTarget("maxValue")]
                    public NumericParser<float> maxValue { get; set; }

                    // Convert this to int, and return a random value
                    public static implicit operator float(RandomRangeLoader loader)
                    {
                        return UnityEngine.Random.Range(loader.minValue, loader.maxValue);
                    }

                    // Create a loader from given values
                    public RandomRangeLoader()
                    {
                        this.maxValue = 1;
                        this.minValue = 0;
                    }

                    // Create a loader from given values
                    public RandomRangeLoader(float minValue, float maxValue)
                    {
                        this.maxValue = maxValue;
                        this.minValue = minValue;
                    }
                }

                // Nearby-Orbits
                [ParserTargetCollection("Nearby", allowMerge = true)]
                public List<NearbyLoader> nearby = new List<NearbyLoader>();

                // Flyby-Orbits
                [ParserTargetCollection("Flyby", allowMerge = true)]
                public List<FlybyLoader> flyby = new List<FlybyLoader>();

                // Around-Orbits
                [ParserTargetCollection("Around", allowMerge = true)]
                public List<AroundLoader> around = new List<AroundLoader>();
            }
        }
    }
}
