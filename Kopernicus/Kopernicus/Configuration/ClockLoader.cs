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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Kopernicus.Components;

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
            public Second second { get; set; }

            // Load custom definition of minute
            [ParserTarget("Minute", allowMerge = true)]
            public Minute minute { get; set; }

            // Load custom definition of hour
            [ParserTarget("Hour", allowMerge = true)]
            public Hour hour { get; set; }

            // Load custom definition of day
            [ParserTarget("Day", allowMerge = true)]
            public Day day { get; set; }

            // Load custom definition of year
            [ParserTarget("Year", allowMerge = true)]
            public Year year { get; set; }

            // Parser Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                Templates.customClock = true;
            }

            // Parser Post Apply Event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
            }
        }

        // Load Second Definition
        [RequireConfigType(ConfigType.Node)]
        public class Second : BaseLoader, IParserEventSubscriber
        {
            // Second.singular
            [ParserTarget("singular")]
            public string singular
            {
                get { return ClockFormatter.S.singular; }
                set
                {
                    ClockFormatter.S.singular = value;
                    ClockFormatter.S.plural = value + "s";
                }
            }

            // Second.plural
            [ParserTarget("plural")]
            public string plural
            {
                get { return ClockFormatter.S.plural; }
                set { ClockFormatter.S.plural = value; }
            }

            // Second.symbol
            [ParserTarget("symbol")]
            public string symbol
            {
                get { return ClockFormatter.S.symbol; }
                set { ClockFormatter.S.symbol = value; }
            }

            // Second.value
            [ParserTarget("value")]
            public NumericParser<double> value
            {
                get { return ClockFormatter.S.value; }
                set { ClockFormatter.S.value = value; }
            }

            // Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
            }

            // Parser Post Apply Event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
            }
        }

        // Load Minute Definition
        [RequireConfigType(ConfigType.Node)]
        public class Minute : BaseLoader, IParserEventSubscriber
        {
            // Minute.singular
            [ParserTarget("singular")]
            public string singular
            {
                get { return ClockFormatter.M.singular; }
                set
                {
                    ClockFormatter.M.singular = value;
                    ClockFormatter.M.plural = value + "s";
                }
            }

            // Minute.plural
            [ParserTarget("plural")]
            public string plural
            {
                get { return ClockFormatter.M.plural; }
                set { ClockFormatter.M.plural = value; }
            }

            // Minute.symbol
            [ParserTarget("symbol")]
            public string symbol
            {
                get { return ClockFormatter.M.symbol; }
                set { ClockFormatter.M.symbol = value; }
            }

            // Minute.value
            [ParserTarget("value")]
            public NumericParser<double> value
            {
                get { return ClockFormatter.M.value; }
                set { ClockFormatter.M.value = value; }
            }

            // Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
            }

            // Parser Post Apply Event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
            }
        }

        // Load Minute Definition
        [RequireConfigType(ConfigType.Node)]
        public class Hour : BaseLoader, IParserEventSubscriber
        {
            // Hour.singular
            [ParserTarget("singular")]
            public string singular
            {
                get { return ClockFormatter.H.singular; }
                set
                {
                    ClockFormatter.H.singular = value;
                    ClockFormatter.H.plural = value + "s";
                }
            }

            // Hour.plural
            [ParserTarget("plural")]
            public string plural
            {
                get { return ClockFormatter.H.plural; }
                set { ClockFormatter.H.plural = value; }
            }

            // Hour.symbol
            [ParserTarget("symbol")]
            public string symbol
            {
                get { return ClockFormatter.H.symbol; }
                set { ClockFormatter.H.symbol = value; }
            }

            // Hour.value
            [ParserTarget("value")]
            public NumericParser<double> value
            {
                get { return ClockFormatter.H.value; }
                set { ClockFormatter.H.value = value; }
            }

            // Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
            }

            // Parser Post Apply Event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
            }
        }

        // Load Minute Definition
        [RequireConfigType(ConfigType.Node)]
        public class Day : BaseLoader, IParserEventSubscriber
        {
            // Day.singular
            [ParserTarget("singular")]
            public string singular
            {
                get { return ClockFormatter.D.singular; }
                set
                {
                    ClockFormatter.D.singular = value;
                    ClockFormatter.D.plural = value + "s";
                }
            }

            // Day.plural
            [ParserTarget("plural")]
            public string plural
            {
                get { return ClockFormatter.D.plural; }
                set { ClockFormatter.D.plural = value; }
            }

            // Day.symbol
            [ParserTarget("symbol")]
            public string symbol
            {
                get { return ClockFormatter.D.symbol; }
                set { ClockFormatter.D.symbol = value; }
            }

            // Day.value
            [ParserTarget("value")]
            public NumericParser<double> value
            {
                get { return ClockFormatter.D.value; }
                set { ClockFormatter.D.value = value; }
            }

            // Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
            }

            // Parser Post Apply Event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
            }
        }

        // Load Year Definition
        [RequireConfigType(ConfigType.Node)]
        public class Year : BaseLoader, IParserEventSubscriber
        {
            // Year.singular
            [ParserTarget("singular")]
            public string singular
            {
                get { return ClockFormatter.Y.singular; }
                set
                {
                    ClockFormatter.Y.singular = value;
                    ClockFormatter.Y.plural = value + "s";
                }
            }

            // Year.plural
            [ParserTarget("plural")]
            public string plural
            {
                get { return ClockFormatter.Y.plural; }
                set { ClockFormatter.Y.plural = value; }
            }

            // Year.symbol
            [ParserTarget("symbol")]
            public string symbol
            {
                get { return ClockFormatter.Y.symbol; }
                set { ClockFormatter.Y.symbol = value; }
            }

            // Year.value
            [ParserTarget("value")]
            public NumericParser<double> value
            {
                get { return ClockFormatter.Y.value; }
                set { ClockFormatter.Y.value = value; }
            }

            // Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
            }

            // Parser Post Apply Event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
            }
        }
    }
}
