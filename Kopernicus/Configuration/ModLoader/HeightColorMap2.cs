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
            public class HeightColorMap2 : ModLoader, IParserEventSubscriber
            {
                // Land class loader 
                private class LandClassLoader2 : IParserEventSubscriber
                {
                    // Land class object
                    public PQSMod_HeightColorMap2.LandClass landClass2;

                    // Name of the class
                    [ParserTarget("name")]
                    private string name 
                    {
                        set { landClass2.name = value; }
                    }

                    // Delete the landclass
                    [ParserTarget("delete", optional = true)]
                    public NumericParser<bool> delete = new NumericParser<bool>(false);

                    // Color of the class
                    [ParserTarget("color")]
                    private ColorParser color
                    {
                        set { landClass2.color = value.value; }
                    }

                    // Fractional altitude start
                    // altitude = (vertexHeight - vertexMinHeightOfPQS) / vertexHeightDeltaOfPQS
                    [ParserTarget("altitudeStart")]
                    private NumericParser<double> altitudeStart
                    {
                        set { landClass2.altStart = value.value; }
                    }

                    // Fractional altitude end
                    [ParserTarget("altitudeEnd")]
                    private NumericParser<double> altitudeEnd
                    {
                        set { landClass2.altEnd = value.value; }
                    }

                    // Should we blend into the next class
                    [ParserTarget("lerpToNext")]
                    private NumericParser<bool> lerpToNext
                    {
                        set { landClass2.lerpToNext = value.value; }
                    }

                    void IParserEventSubscriber.Apply(ConfigNode node) { }

                    void IParserEventSubscriber.PostApply(ConfigNode node) { }

                    public LandClassLoader2 ()
                    {
                        // Initialize the land class
                        landClass2 = new PQSMod_HeightColorMap2.LandClass("class", 0.0, 0.0, Color.white, Color.white, 0.0);
                    }

                    public LandClassLoader2(PQSMod_HeightColorMap2.LandClass c)
                    {
                        this.landClass2 = c;
                    }
                }

                // Actual PQS mod we are loading
                private PQSMod_HeightColorMap2 _mod;

                // The deformity of the simplex terrain
                [ParserTarget("blend", optional = true)]
                private NumericParser<float> blend
                {
                    set { _mod.blend = value.value; }
                }

                // The deformity of the simplex terrain
                [ParserTarget("maxHeight", optional = true)]
                private NumericParser<double> maxHeight
                {
                    set { _mod.maxHeight = value.value; }
                }

                // The deformity of the simplex terrain
                [ParserTarget("minHeight", optional = true)]
                private NumericParser<double> minHeight
                {
                    set { _mod.minHeight = value.value; }
                }

                // The land classes
                private List<LandClassLoader2> landClasses = new List<LandClassLoader2> ();

                void IParserEventSubscriber.Apply(ConfigNode node)
                {
                    // Load the LandClasses manually, to support patching
                    if (node.HasNode("LandClasses"))
                    {
                        // Already patched classes
                        List<PQSMod_HeightColorMap2.LandClass> patchedClasses = new List<PQSMod_HeightColorMap2.LandClass>();
                        if (_mod.landClasses != null)
                            _mod.landClasses.ToList().ForEach(c => landClasses.Add(new LandClassLoader2(c)));

                        // Go through the nodes
                        foreach (ConfigNode lcNode in node.GetNode("LandClasses").nodes)
                        {
                            // The Loader
                            LandClassLoader2 loader = null;

                            // Are there existing LandClasses?
                            if (landClasses.Count > 0)
                            {
                                // Attempt to find a LandClass we can edit that we have not edited before
                                loader = landClasses.Where(m => !patchedClasses.Contains(m.landClass2) && ((lcNode.HasValue("name") ? m.landClass2.name == lcNode.GetValue("name") : false) || (lcNode.HasValue("index") ? landClasses.IndexOf(m).ToString() == lcNode.GetValue("index") : false)))
                                                                 .FirstOrDefault();

                                // Load the Loader (lol)
                                if (loader != null)
                                {
                                    Parser.LoadObjectFromConfigurationNode(loader, lcNode);
                                    if (loader.delete.value)
                                        landClasses.Remove(loader);
                                    else
                                        patchedClasses.Add(loader.landClass2);
                                }
                            }

                            // If we can't patch a LandClass, create a new one
                            if (loader == null)
                            {
                                loader = Parser.CreateObjectFromConfigNode(typeof(LandClassLoader2), lcNode) as LandClassLoader2;
                            }

                            // Add the Loader to the List
                            landClasses.Add(loader);
                        }
                    }
                }

                // Select the land class objects and push into the mod
                void IParserEventSubscriber.PostApply(ConfigNode node)
                {
                    PQSMod_HeightColorMap2.LandClass[] landClassesArray = landClasses.Select(loader => loader.landClass2).ToArray();
                    if (landClassesArray.Count() != 0)
                    {
                        _mod.landClasses = landClassesArray;
                    }
                    _mod.lcCount = _mod.landClasses.Count();
                }

                public HeightColorMap2()
                {
                    // Create the base mod
                    GameObject modObject = new GameObject("HeightColorMap2");
                    modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSMod_HeightColorMap2> ();
                    base.mod = _mod;
                }

                public HeightColorMap2(PQSMod template)
                {
                    _mod = template as PQSMod_HeightColorMap2;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
            }
        }
    }
}

