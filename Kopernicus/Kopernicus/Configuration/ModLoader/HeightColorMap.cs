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
            public class HeightColorMap : ModLoader<PQSMod_HeightColorMap>, IParserEventSubscriber
            {
                // Land class loader 
                public class LandClassLoader
                {
                    // Land class object
                    public PQSMod_HeightColorMap.LandClass landClass;

                    // Name of the class
                    [ParserTarget("name")]
                    public string name 
                    {
                        get { return landClass.name; }
                        set { landClass.name = value; }
                    }

                    // Delete the landclass
                    [ParserTarget("delete")]
                    public NumericParser<bool> delete = new NumericParser<bool>(false);

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
                    public NumericParser<double> altitudeStart
                    {
                        get { return landClass.altStart; }
                        set { landClass.altStart = value; }
                    }

                    // Fractional altitude end
                    [ParserTarget("altitudeEnd")]
                    public NumericParser<double> altitudeEnd
                    {
                        get { return landClass.altEnd; }
                        set { landClass.altEnd = value; }
                    }

                    // Should we blend into the next class
                    [ParserTarget("lerpToNext")]
                    public NumericParser<bool> lerpToNext
                    {
                        get { return landClass.lerpToNext; }
                        set { landClass.lerpToNext = value; }
                    }

                    public LandClassLoader ()
                    {
                        // Initialize the land class
                        landClass = new PQSMod_HeightColorMap.LandClass("class", 0.0, 0.0, Color.white, Color.white, 0.0);
                    }

                    public LandClassLoader(PQSMod_HeightColorMap.LandClass c)
                    {
                        landClass = c;
                    }
                }

                // The deformity of the simplex terrain
                [ParserTarget("blend")]
                public NumericParser<float> blend
                {
                    get { return mod.blend; }
                    set { mod.blend = value.value; }
                }

                // The land classes
                public List<LandClassLoader> landClasses = new List<LandClassLoader> ();

                void IParserEventSubscriber.Apply(ConfigNode node)
                {
                    // Load the LandClasses manually, to support patching
                    if (mod.landClasses != null) mod.landClasses.ToList().ForEach(c => landClasses.Add(new LandClassLoader(c)));
                    if (node.HasNode("LandClasses"))
                    {
                        // Already patched classes
                        List<PQSMod_HeightColorMap.LandClass> patchedClasses = new List<PQSMod_HeightColorMap.LandClass>();

                        // Go through the nodes
                        foreach (ConfigNode lcNode in node.GetNode("LandClasses").nodes)
                        {
                            // The Loader
                            LandClassLoader loader = null;

                            // Are there existing LandClasses?
                            if (landClasses.Count > 0)
                            {
                                // Attempt to find a LandClass we can edit that we have not edited before
                                loader = landClasses.Where(m => !patchedClasses.Contains(m.landClass) && ((lcNode.HasValue("name") ? m.landClass.name == lcNode.GetValue("name") : true) || (lcNode.HasValue("index") ? landClasses.IndexOf(m).ToString() == lcNode.GetValue("index") : false)))
                                                                 .FirstOrDefault();

                                // Load the Loader (lol)
                                if (loader != null)
                                {
                                    Parser.LoadObjectFromConfigurationNode(loader, lcNode);
                                    landClasses.Remove(loader);
                                    patchedClasses.Add(loader.landClass);
                                }
                            }

                            // If we can't patch a LandClass, create a new one
                            if (loader == null)
                            {
                                loader = Parser.CreateObjectFromConfigNode<LandClassLoader>(lcNode);
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
                    PQSMod_HeightColorMap.LandClass[] landClassesArray = landClasses.Select(loader => loader.landClass).ToArray();
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

