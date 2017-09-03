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
            public class OceanFX : ModLoader<PQSMod_OceanFX>
            {
                // angle
                [ParserTarget("angle")]
                public NumericParser<Single> angle
                {
                    get { return mod.angle; }
                    set { mod.angle = value; }
                }

                // The deformity of the map for the Quad Remover (?)
                [ParserTarget("blendA")]
                public NumericParser<Single> blendA
                {
                    get { return mod.blendA; }
                    set { mod.blendA = value; }
                }

                // blendB
                [ParserTarget("blendB")]
                public NumericParser<Single> blendB
                {
                    get { return mod.blendB; }
                    set { mod.blendB = value; }
                }

                // bump
                [ParserTarget("bump")]
                public Texture2DParser bump
                {
                    get { return mod.bump; }
                    set { mod.bump = value; }
                }

                // framesPerSecond
                [ParserTarget("framesPerSecond")]
                public NumericParser<Single> framesPerSecond
                {
                    get { return mod.framesPerSecond; }
                    set { mod.framesPerSecond = value; }
                }

                // fresnel (???)
                [ParserTarget("fresnel")]
                public Texture2DParser fresnel
                {
                    get { return mod.fresnel; }
                    set { mod.fresnel = value; }
                }

                // oceanOpacity
                [ParserTarget("oceanOpacity")]
                public NumericParser<Single> oceanOpacity
                {
                    get { return mod.oceanOpacity; }
                    set { mod.oceanOpacity = value; }
                }

                // refraction
                [ParserTarget("refraction")]
                public Texture2DParser refraction
                {
                    get { return mod.refraction; }
                    set { mod.refraction = value; }
                }

                // spaceAltitude
                [ParserTarget("spaceAltitude")]
                public NumericParser<Double> spaceAltitude
                {
                    get { return mod.spaceAltitude; }
                    set { mod.spaceAltitude = value; }
                }

                // spaceSurfaceBlend
                [ParserTarget("spaceSurfaceBlend")]
                public NumericParser<Single> spaceSurfaceBlend
                {
                    get { return mod.spaceSurfaceBlend; }
                    set { mod.spaceSurfaceBlend = value; }
                }

                // specColor
                [ParserTarget("specColor")]
                public ColorParser specColor
                {
                    get { return mod.specColor; }
                    set { mod.specColor = value; }
                }

                // texBlend
                [ParserTarget("texBlend")]
                public NumericParser<Single> texBlend
                {
                    get { return mod.texBlend; }
                    set { mod.texBlend = value; }
                }

                // txIndex
                [ParserTarget("txIndex")]
                public NumericParser<Int32> txIndex
                {
                    get { return mod.txIndex; }
                    set { mod.txIndex = value; }
                }

                // Watermain
                [ParserTarget("Watermain")]
                public ConfigNode watermain
                {
                    get
                    {
                        if (mod.watermain == null)
                            return new ConfigNode("Watermain");

                        // Not null
                        ConfigNode watermain = new ConfigNode("Watermain");
                        foreach (Texture2D texture in mod.watermain)
                            watermain.AddValue("waterTex-" + mod.watermain.ToList().IndexOf(texture), texture.name);
                        return watermain;
                    }
                    set
                    {
                        // Set the Watermain length
                        mod.waterMainLength = value.values.Count;

                        // If the array isn't there, recreate it
                        if (mod.watermain == null) mod.watermain = new Texture2D[(Int32)mod.waterMainLength];

                        // If the count doesn't matches, recreate the array
                        if (mod.watermain.Length != mod.waterMainLength)
                        {
                            mod.watermain = new Texture2D[(Int32)mod.waterMainLength];
                        }

                        // Load the textures
                        Int32 i = 0;
                        foreach (String s in value.GetValuesStartsWith("waterTex-"))
                        {
                            Texture2DParser texParser = new Texture2DParser();
                            texParser.SetFromString(s);
                            mod.watermain[i] = texParser.value;
                            i++;
                        }
                    }
                }

                // Create the mod
                public override void Create(PQS pqsVersion)
                {
                    // Call base
                    base.Create(pqsVersion);

                    // Create the base mod (I need to instance this one, because some parameters aren't loadable. :( )
                    PSystemBody Body = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, "Laythe");
                    foreach (PQS ocean in Body.pqsVersion.GetComponentsInChildren<PQS>(true))
                    {
                        if (ocean.name == "LaytheOcean")
                            Utility.CopyObjectFields(ocean.GetComponentsInChildren<PQSMod_OceanFX>(true)[0], mod);
                    }
                }
            }
        }
    }
}

