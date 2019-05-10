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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;
using Kopernicus.UI;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class HeightColorMap : ModLoader<PQSMod_HeightColorMap>
    {
        // Land class loader 
        [RequireConfigType(ConfigType.Node)]
        public class LandClassLoader : IPatchable, ITypeParser<PQSMod_HeightColorMap.LandClass>
        {
            // Land class object
            public PQSMod_HeightColorMap.LandClass Value { get; set; }

            // Name of the class
            [ParserTarget("name")]
            public String name
            {
                get { return Value.name; }
                set { Value.name = value; }
            }

            // Delete the landclass
            [ParserTarget("delete")]
            [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
            public NumericParser<Boolean> Delete = false;

            // Color of the class
            [ParserTarget("color")]
            public ColorParser Color
            {
                get { return Value.color; }
                set { Value.color = value; }
            }

            // Fractional altitude start
            // altitude = (vertexHeight - vertexMinHeightOfPQS) / vertexHeightDeltaOfPQS
            [ParserTarget("altitudeStart")]
            public NumericParser<Double> AltitudeStart
            {
                get { return Value.altStart; }
                set { Value.altStart = value; }
            }

            // Fractional altitude end
            [ParserTarget("altitudeEnd")]
            public NumericParser<Double> AltitudeEnd
            {
                get { return Value.altEnd; }
                set { Value.altEnd = value; }
            }

            // Should we blend into the next class
            [ParserTarget("lerpToNext")]
            public NumericParser<Boolean> LerpToNext
            {
                get { return Value.lerpToNext; }
                set { Value.lerpToNext = value; }
            }

            [KittopiaConstructor(KittopiaConstructor.ParameterType.Empty)]
            public LandClassLoader()
            {
                // Initialize the land class
                Value = new PQSMod_HeightColorMap.LandClass("class", 0.0, 0.0, UnityEngine.Color.white, UnityEngine.Color.white, 0.0);
            }

            public LandClassLoader(PQSMod_HeightColorMap.LandClass c)
            {
                Value = c;
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator PQSMod_HeightColorMap.LandClass(LandClassLoader parser)
            {
                return parser.Value;
            }

            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator LandClassLoader(PQSMod_HeightColorMap.LandClass value)
            {
                return new LandClassLoader(value);
            }
        }

        // The deformity of the simplex terrain
        [ParserTarget("blend")]
        public NumericParser<Single> Blend
        {
            get { return Mod.blend; }
            set { Mod.blend = value.Value; }
        }

        // The land classes
        [ParserTargetCollection("LandClasses", AllowMerge = true)]
        public CallbackList<LandClassLoader> LandClasses { get; set; }

        // Creates the a PQSMod of type T with given PQS
        public override void Create(PQS pqsVersion)
        {
            base.Create(pqsVersion);

            // Create the callback list
            LandClasses = new CallbackList<LandClassLoader>(e =>
            {
                Mod.landClasses = LandClasses.Where(landClass => !landClass.Delete)
                    .Select(landClass => landClass.Value).ToArray();
                Mod.lcCount = Mod.landClasses.Length;
            });
            Mod.landClasses = new PQSMod_HeightColorMap.LandClass[Mod.lcCount = 0];
        }

        // Grabs a PQSMod of type T from a parameter with a given PQS
        public override void Create(PQSMod_HeightColorMap mod, PQS pqsVersion)
        {
            base.Create(mod, pqsVersion);

            // Create the callback list
            LandClasses = new CallbackList<LandClassLoader>(e =>
            {
                Mod.landClasses = LandClasses.Where(landClass => !landClass.Delete)
                    .Select(landClass => landClass.Value).ToArray();
                Mod.lcCount = Mod.landClasses.Length;
            });

            // Load LandClasses
            if (Mod.landClasses != null)
            {
                for (Int32 i = 0; i < Mod.landClasses.Length; i++)
                {
                    // Only activate the callback if we are adding the last loader
                    LandClasses.Add(new LandClassLoader(Mod.landClasses[i]), i == Mod.landClasses.Length - 1);
                }
            }
            else
            {
                Mod.landClasses = new PQSMod_HeightColorMap.LandClass[Mod.lcCount = 0];
            }
        }
    }
}

