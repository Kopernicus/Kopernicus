/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class HeightColorMap : ModLoader, IParserEventSubscriber
            {
                // Land class loader 
                private class LandClassLoader : IParserEventSubscriber
                {
                    // Land class object
                    public PQSMod_HeightColorMap.LandClass landClass;

                    // Name of the class
                    [ParserTarget("name")]
                    private string name 
                    {
                        set { landClass.name = value; }
                    }
                        
                    // Color of the class
                    [ParserTarget("color")]
                    private ColorParser color
                    {
                        set { landClass.color = value.value; }
                    }

                    // Fractional altitude start
                    // altitude = (vertexHeight - vertexMinHeightOfPQS) / vertexHeightDeltaOfPQS
                    [ParserTarget("altitudeStart")]
                    private NumericParser<double> altitudeStart
                    {
                        set { landClass.altStart = value.value; }
                    }

                    // Fractional altitude end
                    [ParserTarget("altitudeEnd")]
                    private NumericParser<double> altitudeEnd
                    {
                        set { landClass.altEnd = value.value; }
                    }

                    // Should we blend into the next class
                    [ParserTarget("lerpToNext")]
                    private NumericParser<bool> lerpToNext
                    {
                        set { landClass.lerpToNext = value.value; }
                    }

                    void IParserEventSubscriber.Apply(ConfigNode node) { }

                    void IParserEventSubscriber.PostApply(ConfigNode node) { }

                    public LandClassLoader ()
                    {
                        // Initialize the land class
                        landClass = new PQSMod_HeightColorMap.LandClass("class", 0.0, 0.0, Color.white, Color.white, 0.0);
                    }
                }

                // Actual PQS mod we are loading
                private PQSMod_HeightColorMap _mod;

                // The deformity of the simplex terrain
                [ParserTarget("blend", optional = true)]
                private NumericParser<float> blend
                {
                    set { _mod.blend = value.value; }
                }

                // The land classes
                [ParserTargetCollection("LandClasses", optional = true, nameSignificance = NameSignificance.None)]
                private List<LandClassLoader> landClasses = new List<LandClassLoader> ();

                void IParserEventSubscriber.Apply(ConfigNode node)
                {

                }

                // Select the land class objects and push into the mod
                void IParserEventSubscriber.PostApply(ConfigNode node)
                {
                    PQSMod_HeightColorMap.LandClass[] landClassesArray = landClasses.Select(loader => loader.landClass).ToArray();
                    if (landClassesArray.Count() != 0)
                    {
                        _mod.landClasses = landClassesArray;
                    }
                }

                public HeightColorMap()
                {
                    // Create the base mod
                    GameObject modObject = new GameObject("HeightColorMap");
                    modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSMod_HeightColorMap> ();
                    base.mod = _mod;
                }

                public HeightColorMap(PQSMod template)
                {
                    _mod = template as PQSMod_HeightColorMap;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
            }
        }
    }
}

