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
                [ParserTargetCollection("Watermain")]
                public List<Texture2DParser> watermain
                {
                    get { return mod.watermain.Select(t => new Texture2DParser(t)).ToList(); }
                    set
                    {
                        mod.watermain = value.Select(t => t.Value).ToArray();
                        mod.waterMainLength = value.Count;
                    }
                }

                // Create the mod
                public override void Create(PQS pqsVersion)
                {
                    // Call base
                    base.Create(pqsVersion);

                    // Create the base mod (I need to instance this one, because some parameters aren't loadable. :( )
                    PSystemBody Body = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
                    foreach (PQS ocean in Body.pqsVersion.GetComponentsInChildren<PQS>(true))
                    {
                        if (ocean.name == "LaytheOcean")
                            Utility.CopyObjectFields(ocean.GetComponentsInChildren<PQSMod_OceanFX>(true)[0], mod);
                    }
                }

                // Create the mod
                public override void Create(PQSMod_OceanFX _mod, PQS pqsVersion)
                {
                    // Call base
                    base.Create(_mod, pqsVersion);

                    // Create the base mod if needed
                    if (mod.reflection == null)
                    {
                        PSystemBody Body = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
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
}

