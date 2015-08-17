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
            public class HeightColorMapNoise : ModLoader, IParserEventSubscriber
            {
                // Land class loader 
                private class LandClassLoaderNoise : IParserEventSubscriber
                {
                    // Land class object
                    public PQSMod_HeightColorMapNoise.LandClass landClassNoise;

                    // Name of the class
                    [ParserTarget("name")]
                    private string name 
                    {
                        set { landClassNoise.name = value; }
                    }

                    // Delete the landclass
                    [ParserTarget("delete", optional = true)]
                    public NumericParser<bool> delete = new NumericParser<bool>(false);

                    // Color of the class
                    [ParserTarget("color")]
                    private ColorParser color
                    {
                        set { landClassNoise.color = value.value; }
                    }

                    // Fractional altitude start
                    // altitude = (vertexHeight - vertexMinHeightOfPQS) / vertexHeightDeltaOfPQS
                    [ParserTarget("altitudeStart")]
                    private NumericParser<double> altitudeStart
                    {
                        set { landClassNoise.altStart = value.value; }
                    }

                    // Fractional altitude end
                    [ParserTarget("altitudeEnd")]
                    private NumericParser<double> altitudeEnd
                    {
                        set { landClassNoise.altEnd = value.value; }
                    }

                    // Should we blend into the next class
                    [ParserTarget("lerpToNext")]
                    private NumericParser<bool> lerpToNext
                    {
                        set { landClassNoise.lerpToNext = value.value; }
                    }

                    void IParserEventSubscriber.Apply(ConfigNode node) { }

                    void IParserEventSubscriber.PostApply(ConfigNode node) { }

                    public LandClassLoaderNoise ()
                    {
                        // Initialize the land class
                        landClassNoise = new PQSMod_HeightColorMapNoise.LandClass("class", 0.0, 0.0, Color.white, Color.white, 0.0);
                    }

                    public LandClassLoaderNoise(PQSMod_HeightColorMapNoise.LandClass c)
                    {
                        this.landClassNoise = c;
                    }
                }

                // Actual PQS mod we are loading
                private PQSMod_HeightColorMapNoise _mod;

                // The deformity of the simplex terrain
                [ParserTarget("blend", optional = true)]
                private NumericParser<float> blend
                {
                    set { _mod.blend = value.value; }
                }

                // The land classes
                private List<LandClassLoaderNoise> landClasses = new List<LandClassLoaderNoise> ();

                void IParserEventSubscriber.Apply(ConfigNode node)
                {
                    // Load the LandClasses manually, to support patching
                    if (node.HasNode("LandClasses"))
                    {
                        // Already patched classes
                        List<PQSMod_HeightColorMapNoise.LandClass> patchedClasses = new List<PQSMod_HeightColorMapNoise.LandClass>();
                        if (_mod.landClasses != null)
                            _mod.landClasses.ToList().ForEach(c => landClasses.Add(new LandClassLoaderNoise(c)));

                        // Go through the nodes
                        foreach (ConfigNode lcNode in node.GetNode("LandClasses").nodes)
                        {
                            // The Loader
                            LandClassLoaderNoise loader = null;

                            // Are there existing LandClasses?
                            if (landClasses.Count > 0)
                            {
                                // Attempt to find a LandClass we can edit that we have not edited before
                                loader = landClasses.Where(m => !patchedClasses.Contains(m.landClassNoise) && ((lcNode.HasValue("name") ? m.landClassNoise.name == lcNode.GetValue("name") : false) || (lcNode.HasValue("index") ? landClasses.IndexOf(m).ToString() == lcNode.GetValue("index") : false)))
                                                                 .FirstOrDefault();

                                // Load the Loader (lol)
                                if (loader != null)
                                {
                                    Parser.LoadObjectFromConfigurationNode(loader, lcNode);
                                    if (loader.delete.value)
                                        landClasses.Remove(loader);
                                    else
                                        patchedClasses.Add(loader.landClassNoise);
                                }
                            }

                            // If we can't patch a LandClass, create a new one
                            if (loader == null)
                            {
                                loader = Parser.CreateObjectFromConfigNode(typeof(LandClassLoaderNoise), lcNode) as LandClassLoaderNoise;
                            }

                            // Add the Loader to the List
                            landClasses.Add(loader);
                        }
                    }
                }

                // Select the land class objects and push into the mod
                void IParserEventSubscriber.PostApply(ConfigNode node)
                {
                    PQSMod_HeightColorMapNoise.LandClass[] landClassesArray = landClasses.Select(loader => loader.landClassNoise).ToArray();
                    if (landClassesArray.Count() != 0)
                    {
                        _mod.landClasses = landClassesArray;
                    }
                    _mod.lcCount = _mod.landClasses.Count();
                }

                public HeightColorMapNoise()
                {
                    // Create the base mod
                    GameObject modObject = new GameObject("HeightColorMapNoise");
                    modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSMod_HeightColorMapNoise>();
                    base.mod = _mod;
                }

                public HeightColorMapNoise(PQSMod template)
                {
                    _mod = template as PQSMod_HeightColorMapNoise;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
            }
        }
    }
}

