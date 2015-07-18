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
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class VertexHeightOblate : ModLoader, IParserEventSubscriber
            {
                // Mod-Fix, because of hell-ish buggyness
                public class PQSMod_OblateFixed : PQSMod_VertexHeightOblate
                {
                    // Re-arrange the Vertex Build order
                    public override void OnVertexBuild(PQS.VertexBuildData data)
                    {
                        // Do nothing
                        // base.OnVertexBuild(data);
                    }

                    // Build the height in the correct function
                    public override void OnVertexBuildHeight(PQS.VertexBuildData data)
                    {
                        // Tricky...
                        base.OnVertexBuild(data);
                    }
                }

                // Actual PQS mod we are loading
                private PQSMod_OblateFixed _mod;

                // The height where the terrain is oblated
                [ParserTarget("height", optional = true)]
                private NumericParser<double> height
                {
                    set { _mod.height = value.value; }
                }

                // The pow of the terrain
                [ParserTarget("pow", optional = true)]
                private NumericParser<double> pow
                {
                    set { _mod.pow = value.value; }
                }

                void IParserEventSubscriber.Apply(ConfigNode node)
                {

                }

                void IParserEventSubscriber.PostApply(ConfigNode node)
                {

                }

                public VertexHeightOblate()
                {
                    // Create the base mod
                    GameObject modObject = new GameObject("VertexHeightOblate");
                    modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSMod_OblateFixed>();
                    base.mod = _mod;
                }

                public VertexHeightOblate(PQSMod template)
                {
                    _mod = template as PQSMod_OblateFixed;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
            }
        }
    }
}

