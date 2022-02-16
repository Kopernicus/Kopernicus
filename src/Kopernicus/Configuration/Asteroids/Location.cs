/**
 * Kopernicus Planetary System Modifier
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
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;

namespace Kopernicus.Configuration.Asteroids
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Location
    {
        // Loads the nearby-Orbits of this Asteroid
        [RequireConfigType(ConfigType.Node)]
        public class NearbyLoader
        {
            // The body we are passing
            [ParserTarget("body")]
            public String Body { get; set; }

            // eccentricity
            [ParserTarget("eccentricity")]
            public RandomRangeLoader Eccentricity = new RandomRangeLoader(0.0001f, 0.01f);

            // semiMajorAxis
            [ParserTarget("semiMajorAxis")]
            public RandomRangeLoader SemiMajorAxis = new RandomRangeLoader(0.999f, 1.001f);

            // inclination
            [ParserTarget("inclination")] public RandomRangeLoader Inclination = new RandomRangeLoader(-0.001f, 0.001f);

            // longitudeOfAscendingNode
            [ParserTarget("longitudeOfAscendingNode")]
            public RandomRangeLoader LongitudeOfAscendingNode = new RandomRangeLoader(0.999f, 1.001f);

            // argumentOfPeriapsis
            [ParserTarget("argumentOfPeriapsis")]
            public RandomRangeLoader ArgumentOfPeriapsis = new RandomRangeLoader(0.999f, 1.001f);

            // meanAnomalyAtEpoch
            [ParserTarget("meanAnomalyAtEpoch")]
            public RandomRangeLoader MeanAnomalyAtEpoch = new RandomRangeLoader(0.999f, 1.001f);

            // The probability of this Orbit type
            [ParserTarget("probability")]
            public NumericParser<Single> Probability { get; set; }

            // Whether the body must be reached
            [ParserTarget("reached")]
            public NumericParser<Boolean> Reached { get; set; }
        }

        // Loads the flyby-Orbits of this Asteroid
        [RequireConfigType(ConfigType.Node)]
        public class FlybyLoader
        {
            // The body we are passing
            [ParserTarget("body")]
            public String Body { get; set; }

            // The minimum amount of days to closest approach
            [ParserTarget("minDuration")]
            public NumericParser<Single> MinDuration { get; set; }

            // The maximum amount of days to closest approach
            [ParserTarget("maxDuration")]
            public NumericParser<Single> MaxDuration { get; set; }

            // The probability of this Orbit type
            [ParserTarget("probability")]
            public NumericParser<Single> Probability { get; set; }

            // Whether the body must be reached
            [ParserTarget("reached")]
            public NumericParser<Boolean> Reached { get; set; }
        }

        // Loads the around-Orbits of this Asteroid
        [RequireConfigType(ConfigType.Node)]
        public class AroundLoader
        {
            // The body we are passing
            [ParserTarget("body")]
            public String Body { get; set; }

            // eccentricity
            [ParserTarget("eccentricity")]
            public RandomRangeLoader Eccentricity = new RandomRangeLoader(0.0001f, 0.01f);

            // semiMajorAxis
            [ParserTarget("semiMajorAxis")]
            public RandomRangeLoader SemiMajorAxis { get; set; }

            // inclination
            [ParserTarget("inclination")]
            public RandomRangeLoader Inclination = new RandomRangeLoader(-0.001f, 0.001f);

            // longitudeOfAscendingNode
            [ParserTarget("longitudeOfAscendingNode")]
            public RandomRangeLoader LongitudeOfAscendingNode = new RandomRangeLoader(0.999f, 1.001f);

            // argumentOfPeriapsis
            [ParserTarget("argumentOfPeriapsis")]
            public RandomRangeLoader ArgumentOfPeriapsis = new RandomRangeLoader(0.999f, 1.001f);

            // meanAnomalyAtEpoch
            [ParserTarget("meanAnomalyAtEpoch")]
            public RandomRangeLoader MeanAnomalyAtEpoch = new RandomRangeLoader(0.999f, 1.001f);

            // epoch
            [ParserTarget("epoch")]
            public RandomRangeLoader Epoch = new RandomRangeLoader(0.999f, 1.001f);

            // The probability of this Orbit type
            [ParserTarget("probability")]
            public NumericParser<Single> Probability { get; set; }

            // Whether the body must be reached
            [ParserTarget("reached")]
            public NumericParser<Boolean> Reached { get; set; }
        }

        // Loads a random range value
        [RequireConfigType(ConfigType.Node)]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public class RandomRangeLoader
        {
            // The min value
            [ParserTarget("minValue")]
            public NumericParser<Single> MinValue { get; set; }

            // The max value
            [ParserTarget("maxValue")]
            public NumericParser<Single> MaxValue { get; set; }

            // Convert this to Int32, and return a random value
            public static implicit operator Single(RandomRangeLoader loader)
            {
                return UnityEngine.Random.Range(loader.MinValue, loader.MaxValue);
            }

            // Create a loader from given values
            public RandomRangeLoader()
            {
                MaxValue = 1;
                MinValue = 0;
            }

            // Create a loader from given values
            public RandomRangeLoader(Single minValue, Single maxValue)
            {
                MaxValue = maxValue;
                MinValue = minValue;
            }
        }

        // Nearby-Orbits
        [ParserTargetCollection("Nearby", AllowMerge = true)]
        public List<NearbyLoader> Nearby = new List<NearbyLoader>();

        // Flyby-Orbits
        [ParserTargetCollection("Flyby", AllowMerge = true)]
        public List<FlybyLoader> Flyby = new List<FlybyLoader>();

        // Around-Orbits
        [ParserTargetCollection("Around", AllowMerge = true)]
        public List<AroundLoader> Around = new List<AroundLoader>();
    }
}