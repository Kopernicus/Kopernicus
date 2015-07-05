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
            public class MapDecalTangent : ModLoader, IParserEventSubscriber
            {
                // Actual PQS mod we are loading
                private PQSMod_MapDecalTangent _mod;

                // absolute
                [ParserTarget("absolute", optional = true)]
                private NumericParser<bool> absolute
                {
                    set { _mod.absolute = value.value; }
                }

                // absoluteOffset
                [ParserTarget("absoluteOffset", optional = true)]
                private NumericParser<double> absoluteOffset
                {
                    set { _mod.absoluteOffset = value.value; }
                }

                // angle
                [ParserTarget("angle", optional = true)]
                private NumericParser<float> angle
                {
                    set { _mod.angle = value.value; }
                }

                // colorMap
                [ParserTarget("colorMap", optional = true)]
                private MapSOParser_RGB<MapSO> colorMap
                {
                    set { _mod.colorMap = value.value; }
                }

                // cullBlack
                [ParserTarget("cullBlack", optional = true)]
                private NumericParser<bool> cullBlack
                {
                    set { _mod.cullBlack = value.value; }
                }

                // DEBUG_HighlightInclusion
                [ParserTarget("DEBUG_HighlightInclusion", optional = true)]
                private NumericParser<bool> DEBUG_HighlightInclusion
                {
                    set { _mod.DEBUG_HighlightInclusion = value.value; }
                }

                // heightMap
                [ParserTarget("heightMap", optional = true)]
                private MapSOParser_GreyScale<MapSO> heightMap
                {
                    set { _mod.heightMap = value.value; }
                }

                // heightMapDeformity
                [ParserTarget("heightMapDeformity", optional = true)]
                private NumericParser<double> heightMapDeformity
                {
                    set { _mod.heightMapDeformity = value.value; }
                }

                // position
                [ParserTarget("position", optional = true)]
                private Vector3Parser position
                {
                    set { _mod.position = value.value; }
                }

                // removeScatter
                [ParserTarget("removeScatter", optional = true)]
                private NumericParser<bool> removeScatter
                {
                    set { _mod.removeScatter = value.value; }
                }

                // radius
                [ParserTarget("radius", optional = true)]
                private NumericParser<double> radius
                {
                    set { _mod.radius = value.value; }
                }

                // smoothColor
                [ParserTarget("smoothColor", optional = true)]
                private NumericParser<float> smoothColor
                {
                    set { _mod.smoothColor = value.value; }
                }

                // smoothHeight
                [ParserTarget("smoothHeight", optional = true)]
                private NumericParser<float> smoothHeight
                {
                    set { _mod.smoothHeight = value.value; }
                }

                // useAlphaHeightSmoothing
                [ParserTarget("useAlphaHeightSmoothing", optional = true)]
                private NumericParser<bool> useAlphaHeightSmoothing
                {
                    set { _mod.useAlphaHeightSmoothing = value.value; }
                }

                void IParserEventSubscriber.Apply(ConfigNode node)
                {

                }

                void IParserEventSubscriber.PostApply(ConfigNode node)
                {

                }

                public MapDecalTangent()
                {
                    // Create the base mod
                    GameObject modObject = new GameObject("MapDecalTangent");
                    modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSMod_MapDecalTangent>();
                    base.mod = _mod;
                }

                public MapDecalTangent(PQSMod template)
                {
                    _mod = template as PQSMod_MapDecalTangent;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
            }
        }
    }
}

