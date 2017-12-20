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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class HeightColorMap : ModLoader<PQSMod_HeightColorMap>
            {
                // Land class loader 
                [RequireConfigType(ConfigType.Node)]
                public class LandClassLoader : IPatchable
                {
                    // Land class object
                    public PQSMod_HeightColorMap.LandClass landClass;

                    // Name of the class
                    [ParserTarget("name")]
                    public String name 
                    {
                        get { return landClass.name; }
                        set { landClass.name = value; }
                    }

                    // Delete the landclass
                    [ParserTarget("delete")]
                    public NumericParser<Boolean> delete = false;

                    // Color of the class
                    [ParserTarget("color")]
                    public ColorParser color
                    {
                        get { return landClass.color; }
                        set { landClass.color = value; }
                    }

                    // Fractional altitude start
                    // altitude = (vertexHeight - vertexMinHeightOfPQS) / vertexHeightDeltaOfPQS
                    [ParserTarget("altitudeStart")]
                    public NumericParser<Double> altitudeStart
                    {
                        get { return landClass.altStart; }
                        set { landClass.altStart = value; }
                    }

                    // Fractional altitude end
                    [ParserTarget("altitudeEnd")]
                    public NumericParser<Double> altitudeEnd
                    {
                        get { return landClass.altEnd; }
                        set { landClass.altEnd = value; }
                    }

                    // Should we blend into the next class
                    [ParserTarget("lerpToNext")]
                    public NumericParser<Boolean> lerpToNext
                    {
                        get { return landClass.lerpToNext; }
                        set { landClass.lerpToNext = value; }
                    }

                    [KittopiaConstructor(KittopiaConstructor.Parameter.Empty, purpose = KittopiaConstructor.Purpose.Create)]
                    public LandClassLoader ()
                    {
                        // Initialize the land class
                        landClass = new PQSMod_HeightColorMap.LandClass("class", 0.0, 0.0, Color.white, Color.white, 0.0);
                    }

                    [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
                    public LandClassLoader(PQSMod_HeightColorMap.LandClass c)
                    {
                        landClass = c;
                    }
                }

                // The deformity of the simplex terrain
                [ParserTarget("blend")]
                public NumericParser<Single> blend
                {
                    get { return mod.blend; }
                    set { mod.blend = value.value; }
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
                            .Select(landClass => landClass.landClass).ToArray();
                        mod.lcCount = mod.landClasses.Length;
                    });
                }

                // Grabs a PQSMod of type T from a parameter with a given PQS
                public override void Create(PQSMod_HeightColorMap _mod, PQS pqsVersion)
                {
                    base.Create(_mod, pqsVersion);
                    
                    // Create the callback list
                    landClasses = new CallbackList<LandClassLoader> ((e) =>
                    {
                        mod.landClasses = landClasses.Where(landClass => !landClass.delete)
                            .Select(landClass => landClass.landClass).ToArray();
                        mod.lcCount = mod.landClasses.Length;
                    });
                    
                    // Load LandClasses
                    if (mod.landClasses != null)
                    {
                        foreach (PQSMod_HeightColorMap.LandClass landClass in mod.landClasses)
                        {
                            landClasses.Add(new LandClassLoader(landClass));
                        }
                    }
                }
            }
        }
    }
}

