/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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

namespace Kopernicus
{
    namespace Configuration
    {
        // Load Clock Definitions
        [RequireConfigType(ConfigType.Node)]
        public class ClockLoader : IParserEventSubscriber
        {
            // Load custom definition of seconds
            [ParserTarget("Second", allowMerge = true)]
            public ClockFormatLoader second = new ClockFormatLoader("Second", "Seconds", "s", 1);

            // Load custom definition of minute
            [ParserTarget("Minute", allowMerge = true)]
            public ClockFormatLoader minute = new ClockFormatLoader("Minute", "Minutes", "m", 60);

            // Load custom definition of hour
            [ParserTarget("Hour", allowMerge = true)]
            public ClockFormatLoader hour = new ClockFormatLoader("Hour", "Hours", "h", 3600);

            // Load custom definition of day
            [ParserTarget("Day", allowMerge = true)]
            public ClockFormatLoader day = new ClockFormatLoader("Day", "Days", "d", 3600 * (GameSettings.KERBIN_TIME ? 6 : 24));

            // Load custom definition of year
            [ParserTarget("Year", allowMerge = true)]
            public ClockFormatLoader year = new ClockFormatLoader("Year", "Years", "y", 3600 * (GameSettings.KERBIN_TIME ? 6 * 426 : 24 * 365));

            // Parser Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                Templates.customClock = true;
            }

            // Parser Post Apply Event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                ClockFormatter.S = second;
                ClockFormatter.M = minute;
                ClockFormatter.H = hour;
                ClockFormatter.D = day;
                ClockFormatter.Y = year;
            }
        }

        [RequireConfigType(ConfigType.Node)]
        public class ClockFormatLoader : BaseLoader, IParserEventSubscriber
        {
            [ParserTarget("singular")]
            public string singular;

            [ParserTarget("plural")]
            public string plural;

            [ParserTarget("symbol")]
            public string symbol;

            [ParserTarget("value")]
            public NumericParser<double> value;

            // Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
            }

            // Parser Post Apply Event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
            }

            public ClockFormatLoader(string singular, string plural, string symbol, double value)
            {
                this.singular = singular;
                this.plural = plural;
                this.symbol = symbol;
                this.value = value;
            }
        }
    }
}
