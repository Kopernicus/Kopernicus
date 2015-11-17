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
                [ParserTarget("angle", optional = true)]
                public NumericParser<float> angle
                {
                    get { return mod.angle; }
                    set { mod.angle = value; }
                }

                // The deformity of the map for the Quad Remover (?)
                [ParserTarget("blendA", optional = true)]
                public NumericParser<float> blendA
                {
                    get { return mod.blendA; }
                    set { mod.blendA = value; }
                }

                // blendB
                [ParserTarget("blendB", optional = true)]
                public NumericParser<float> blendB
                {
                    get { return mod.blendB; }
                    set { mod.blendB = value; }
                }

                // bump
                [ParserTarget("bump", optional = true)]
                public Texture2DParser bump
                {
                    get { return mod.bump; }
                    set { mod.bump = value; }
                }

                // framesPerSecond
                [ParserTarget("framesPerSecond", optional = true)]
                public NumericParser<float> framesPerSecond
                {
                    get { return mod.framesPerSecond; }
                    set { mod.framesPerSecond = value; }
                }

                // fresnel (???)
                [ParserTarget("fresnel", optional = true)]
                public Texture2DParser fresnel
                {
                    get { return mod.fresnel; }
                    set { mod.fresnel = value; }
                }

                // oceanOpacity
                [ParserTarget("oceanOpacity", optional = true)]
                public NumericParser<float> oceanOpacity
                {
                    get { return mod.oceanOpacity; }
                    set { mod.oceanOpacity = value; }
                }

                // refraction
                [ParserTarget("refraction", optional = true)]
                public Texture2DParser refraction
                {
                    get { return mod.refraction; }
                    set { mod.refraction = value; }
                }

                // spaceAltitude
                [ParserTarget("spaceAltitude", optional = true)]
                public NumericParser<double> spaceAltitude
                {
                    get { return mod.spaceAltitude; }
                    set { mod.spaceAltitude = value; }
                }

                // spaceSurfaceBlend
                [ParserTarget("spaceSurfaceBlend", optional = true)]
                public NumericParser<float> spaceSurfaceBlend
                {
                    get { return mod.spaceSurfaceBlend; }
                    set { mod.spaceSurfaceBlend = value; }
                }

                // specColor
                [ParserTarget("specColor", optional = true)]
                public ColorParser specColor
                {
                    get { return mod.specColor; }
                    set { mod.specColor = value; }
                }

                // texBlend
                [ParserTarget("texBlend", optional = true)]
                public NumericParser<float> texBlend
                {
                    get { return mod.texBlend; }
                    set { mod.texBlend = value; }
                }

                // txIndex
                [ParserTarget("txIndex", optional = true)]
                public NumericParser<int> txIndex
                {
                    get { return mod.txIndex; }
                    set { mod.txIndex = value; }
                }

                // Watermain
                [ParserTarget("Watermain", optional = true)]
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
                        if (mod.watermain == null) mod.watermain = new Texture2D[(int)mod.waterMainLength];

                        // If the count doesn't matches, recreate the array
                        if (mod.watermain.Length != mod.waterMainLength)
                        {
                            mod.watermain = new Texture2D[(int)mod.waterMainLength];
                        }

                        // Load the textures
                        int i = 0;
                        foreach (string s in value.GetValuesStartsWith("waterTex-"))
                        {
                            Texture2DParser texParser = new Texture2DParser();
                            texParser.SetFromString(s);
                            mod.watermain[i] = texParser.value;
                            i++;
                        }
                    }
                }

                // Create the mod
                public override void Create()
                {
                    // Create the base mod (I need to instance this one, because some parameters aren't loadable. :( )
                    PSystemBody Body = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, "Laythe");
                    foreach (PQS ocean in Body.pqsVersion.GetComponentsInChildren<PQS>(true))
                    {
                        if (ocean.name == "LaytheOcean")
                        {
                            mod = Object.Instantiate(ocean.GetComponentsInChildren<PQSMod_OceanFX>(true)[0]) as PQSMod_OceanFX;
                            mod.name = "OceanFX";
                            mod.gameObject.name = "OceanFX";
                            mod.transform.name = "OceanFX";
                        }
                    }
                    mod.transform.parent = generatedBody.pqsVersion.transform;
                    mod.sphere = generatedBody.pqsVersion;
                    mod.gameObject.layer = Constants.GameLayers.LocalSpace;
                }
            }
        }
    }
}

