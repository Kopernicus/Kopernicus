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

namespace Kopernicus.Configuration.DiscoverableObjects
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Asteroid
    {
        // Name
        [ParserTarget("name")]
        public String Name { get; set; }

        // Spawning Locations
        [ParserTarget("Locations")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
        public Location Location = new Location();

        // Spawn interval
        [ParserTarget("interval")]
        public NumericParser<Single> Interval { get; set; }

        // Maximal untracked lifetime
        [ParserTarget("maxUntrackedLifetime")]
        public NumericParser<Single> MaxUntrackedLifetime { get; set; }

        // Minimal untracked lifetime
        [ParserTarget("minUntrackedLifetime")]
        public NumericParser<Single> MinUntrackedLifetime { get; set; }

        // Probability of a spawn
        [ParserTarget("probability")]
        public NumericParser<Single> Probability { get; set; }

        // Min Limit
        [ParserTarget("spawnGroupMinLimit")]
        public NumericParser<Int32> SpawnGroupMinLimit { get; set; }

        // Max Limit
        [ParserTarget("spawnGroupMaxLimit")]
        public NumericParser<Int32> SpawnGroupMaxLimit { get; set; }

        // Whether the asteroid name should be unique per saved game
        [ParserTarget("uniqueName")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
        public NumericParser<Boolean> UniqueName = false;

        // Config Node that overloads the created vessel node
        // Reason for the Caps: Sigma was messing up his configs
        [ParserTarget("VESSEL")]
        public ConfigNode Vessel { get; set; }

        // Classes of the asteroid
        [ParserTargetCollection("Size", Key = "key", NameSignificance = NameSignificance.Key)]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private List<NumericCollectionParser<Single>> SizeSetter
        {
            get { return Utility.FloatCurveToList(Size); }
            set { Size = Utility.ListToFloatCurve(value); }
        }

        // A float curve that governs how likely each asteroid size and class is to be generated.
        // It should range from 0 to 1, with 0 mapping to MinClass and 1 to MaxClass.
        public FloatCurve Size { get; set; }

        // The smallest size class this group generates. The Size curve maps its [0,1] output onto
        // the MinClass..MaxClass range, so narrowing the range here concentrates the distribution
        // instead of spreading it over every class the game defines.
        [ParserTarget("minClass")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
        public EnumParser<UntrackedObjectClass> MinClass = UntrackedObjectClass.A;

        // The largest size class this group generates. Defaults to I so the class spread matches
        // what Kopernicus has always produced; stock's own spawner only goes up to E by default.
        [ParserTarget("maxClass")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
        public EnumParser<UntrackedObjectClass> MaxClass = UntrackedObjectClass.I;

        // Physical radius per size class, in meters. Optional - any class left out keeps the stock
        // ModuleAsteroid scaling. See ClassRadiusLoader for why this exists.
        [ParserTarget("ClassRadius")]
        public ClassRadiusLoader ClassRadius { get; set; }

        public int InternalOrderID { get; set; }
    }
}
