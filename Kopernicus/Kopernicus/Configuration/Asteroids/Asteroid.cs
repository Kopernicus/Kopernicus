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
            public class Asteroid
            {
                // Name
                [ParserTarget("name")]
                public string name { get; set; }

                // Spawning Locations
                [ParserTarget("Locations")]
                public Location location = new Location();

                // Spawn interval
                [ParserTarget("interval")]
                public NumericParser<float> interval { get; set; }

                // Maximal untracked lifetime
                [ParserTarget("maxUntrackedLifetime")]
                public NumericParser<float> maxUntrackedLifetime { get; set; }

                // Minimal untracked lifetime
                [ParserTarget("minUntrackedLifetime")]
                public NumericParser<float> minUntrackedLifetime { get; set; }

                // Probability of a spawn
                [ParserTarget("probability")]
                public NumericParser<float> probability { get; set; }

                // Min Limit
                [ParserTarget("spawnGroupMinLimit")]
                public NumericParser<int> spawnGroupMinLimit { get; set; }

                // Max Limit
                [ParserTarget("spawnGroupMaxLimit")]
                public NumericParser<int> spawnGroupMaxLimit { get; set; }

                // Whether the asteroid name should be unique per savegame
                [ParserTarget("uniqueName")]
                public NumericParser<bool> uniqueName = new NumericParser<bool>(false);

                // Config Node that overloads the created vessel node
                // Reason for the Caps: Sigma was messing up his configs
                [ParserTarget("VESSEL")]
                public ConfigNode vessel { get; set; }

                // Classes of the asteroid
                [ParserTarget("Size")]
                public FloatCurveParser size { get; set; }
            }
        }
    }
}
