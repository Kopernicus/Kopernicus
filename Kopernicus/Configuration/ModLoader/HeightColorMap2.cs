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
            public class HeightColorMap2 : ModLoader<PQSMod_HeightColorMap2>, IParserEventSubscriber
            {
                // Land class loader 
                public class LandClassLoader2
                {
                    // Land class object
                    public PQSMod_HeightColorMap2.LandClass landClass2;

                    // Name of the class
                    [ParserTarget("name")]
                    public string name 
                    {
                        get { return landClass2.name; }
                        set { landClass2.name = value; }
                    }

                    // Delete the landclass
                    [ParserTarget("delete", optional = true)]
                    public NumericParser<bool> delete = new NumericParser<bool>(false);

                    // Color of the class
                    [ParserTarget("color")]
                    public ColorParser color
                    {
                        get { return landClass2.color; }
                        set { landClass2.color = value; }
                    }

                    // Fractional altitude start
                    // altitude = (vertexHeight - vertexMinHeightOfPQS) / vertexHeightDeltaOfPQS
                    [ParserTarget("altitudeStart")]
                    public NumericParser<double> altitudeStart
                    {
                        get { return landClass2.altStart; }
                        set { landClass2.altStart = value; }
                    }

                    // Fractional altitude end
                    [ParserTarget("altitudeEnd")]
                    public NumericParser<double> altitudeEnd
                    {
                        get { return landClass2.altEnd; }
                        set { landClass2.altEnd = value; }
                    }

                    // Should we blend into the next class
                    [ParserTarget("lerpToNext")]
                    public NumericParser<bool> lerpToNext
                    {
                        get { return landClass2.lerpToNext; }
                        set { landClass2.lerpToNext = value; }
                    }

                    public LandClassLoader2 ()
                    {
                        // Initialize the land class
                        landClass2 = new PQSMod_HeightColorMap2.LandClass("class", 0.0, 0.0, Color.white, Color.white, 0.0);
                    }

                    public LandClassLoader2(PQSMod_HeightColorMap2.LandClass c)
                    {
                        landClass2 = c;
                    }
                }

                // The deformity of the simplex terrain
                [ParserTarget("blend", optional = true)]
                public NumericParser<float> blend
                {
                    get { return mod.blend; }
                    set { mod.blend = value; }
                }

                // The deformity of the simplex terrain
                [ParserTarget("maxHeight", optional = true)]
                public NumericParser<double> maxHeight
                {
                    get { return mod.maxHeight; }
                    set { mod.maxHeight = value; }
                }

                // The deformity of the simplex terrain
                [ParserTarget("minHeight", optional = true)]
                public NumericParser<double> minHeight
                {
                    get { return mod.minHeight; }
                    set { mod.minHeight = value; }
                }

                // The land classes
                public List<LandClassLoader2> landClasses = new List<LandClassLoader2> ();

                void IParserEventSubscriber.Apply(ConfigNode node)
                {
                    // Load the LandClasses manually, to support patching
                    if (mod.landClasses != null) mod.landClasses.ToList().ForEach(c => landClasses.Add(new LandClassLoader2(c)));
                    if (node.HasNode("LandClasses"))
                    {
                        // Already patched classes
                        List<PQSMod_HeightColorMap2.LandClass> patchedClasses = new List<PQSMod_HeightColorMap2.LandClass>();

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
                                    landClasses.Remove(loader);
                                    patchedClasses.Add(loader.landClass2);
                                }
                            }

                            // If we can't patch a LandClass, create a new one
                            if (loader == null)
                            {
                                loader = Parser.CreateObjectFromConfigNode<LandClassLoader2>(lcNode);
                            }

                            // Add the Loader to the List
                            if (!loader.delete.value)
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
                        mod.landClasses = landClassesArray;
                    }
                    mod.lcCount = mod.landClasses.Length;
                }
            }
        }
    }
}

