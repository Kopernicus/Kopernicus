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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Linq;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class HeightColorMapNoise : ModLoader<PQSMod_HeightColorMapNoise>
            {
                // Land class loader 
                [RequireConfigType(ConfigType.Node)]
                public class LandClassLoader : IPatchable, ITypeParser<PQSMod_HeightColorMapNoise.LandClass>
                {
                    // Land class object
                    public PQSMod_HeightColorMapNoise.LandClass Value { get; set; }

                    // Name of the class
                    [ParserTarget("name")]
                    public String name 
                    {
                        get { return Value.name; }
                        set { Value.name = value; }
                    }

                    // Delete the landclass
                    [ParserTarget("delete")]
                    public NumericParser<Boolean> delete = false;

                    // Color of the class
                    [ParserTarget("color")]
                    public ColorParser color
                    {
                        get { return Value.color; }
                        set { Value.color = value; }
                    }

                    // Fractional altitude start
                    // altitude = (vertexHeight - vertexMinHeightOfPQS) / vertexHeightDeltaOfPQS
                    [ParserTarget("altitudeStart")]
                    public NumericParser<Double> altitudeStart
                    {
                        get { return Value.altStart; }
                        set { Value.altStart = value; }
                    }

                    // Fractional altitude end
                    [ParserTarget("altitudeEnd")]
                    public NumericParser<Double> altitudeEnd
                    {
                        get { return Value.altEnd; }
                        set { Value.altEnd = value; }
                    }

                    // Should we blend into the next class
                    [ParserTarget("lerpToNext")]
                    public NumericParser<Boolean> lerpToNext
                    {
                        get { return Value.lerpToNext; }
                        set { Value.lerpToNext = value; }
                    }

                    public LandClassLoader()
                    {
                        // Initialize the land class
                        Value = new PQSMod_HeightColorMapNoise.LandClass("class", 0.0, 0.0, Color.white, Color.white, 0.0);
                    }

                    public LandClassLoader(PQSMod_HeightColorMapNoise.LandClass c)
                    {
                        Value = c;
                    }
                }

                // The deformity of the simplex terrain
                [ParserTarget("blend")]
                public NumericParser<Single> blend
                {
                    get { return mod.blend; }
                    set { mod.blend = value; }
                }

                // The land classes
                [ParserTargetCollection("LandClasses", allowMerge = true)]
                public CallbackList<LandClassLoader> landClasses { get; set; }

                // Creates the a PQSMod of type T with given PQS
                public override void Create(PQS pqsVersion)
                {
                    base.Create(pqsVersion);
                    
                    // Create the callback list
                    landClasses = new CallbackList<LandClassLoader> ((e) =>
                    {
                        mod.landClasses = landClasses.Where(landClass => !landClass.delete)
                            .Select(landClass => landClass.Value).ToArray();
                        mod.lcCount = mod.landClasses.Length;
                    });
                    mod.landClasses = new PQSMod_HeightColorMapNoise.LandClass[mod.lcCount = 0];
                }

                // Grabs a PQSMod of type T from a parameter with a given PQS
                public override void Create(PQSMod_HeightColorMapNoise _mod, PQS pqsVersion)
                {
                    base.Create(_mod, pqsVersion);
                    
                    // Create the callback list
                    landClasses = new CallbackList<LandClassLoader> ((e) =>
                    {
                        mod.landClasses = landClasses.Where(landClass => !landClass.delete)
                            .Select(landClass => landClass.Value).ToArray();
                        mod.lcCount = mod.landClasses.Length;
                    });
                    
                    // Load LandClasses
                    if (mod.landClasses != null)
                    {
                        for (Int32 i = 0; i < mod.landClasses.Length; i++)
                        {
                            // Only activate the callback if we are adding the last loader
                            landClasses.Add(new LandClassLoader(mod.landClasses[i]), i == mod.landClasses.Length - 1);
                        }
                    }
                    else
                    {
                        mod.landClasses = new PQSMod_HeightColorMapNoise.LandClass[mod.lcCount = 0];
                    }
                }
            }
        }
    }
}

