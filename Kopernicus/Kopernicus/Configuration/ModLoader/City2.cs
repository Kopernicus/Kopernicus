/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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
using CommNet;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class City2 : ModLoader<PQSCity2>, IParserEventSubscriber
            {
                // snapToSurface
                [ParserTarget("snapToSurface")]
                public NumericParser<bool> snapToSurface
                {
                    get { return mod.snapToSurface; }
                    set { mod.snapToSurface = value; }
                }

                // alt
                [ParserTarget("alt")]
                public NumericParser<double> alt
                {
                    get { return mod.alt; }
                    set { mod.alt = value; }
                }

                // lat
                [ParserTarget("lat")]
                public NumericParser<double> lat
                {
                    get { return mod.lat; }
                    set { mod.lat = value; }
                }

                // lon
                [ParserTarget("lon")]
                public NumericParser<double> lon
                {
                    get { return mod.lon; }
                    set { mod.lon = value; }
                }

                // objectName
                [ParserTarget("objectName")]
                public string objectName
                {
                    get { return mod.objectName; }
                    set { mod.objectName = value; }
                }

                // up
                [ParserTarget("up")]
                public Vector3Parser up
                {
                    get { return mod.up; }
                    set { mod.up = value; }
                }

                // rotation
                [ParserTarget("rotation")]
                public NumericParser<double> rotation
                {
                    get { return mod.rotation; }
                    set { mod.rotation = value; }
                }

                // snapHeightOffset
                [ParserTarget("snapHeightOffset")]
                public NumericParser<double> snapHeightOffset
                {
                    get { return mod.snapHeightOffset; }
                    set { mod.snapHeightOffset = value; }
                }

                // The mesh for the mod
                [ParserTarget("model")]
                public MuParser model
                {
                    set
                    {
                        value.value.transform.parent = mod.transform;
                        Transform[] gameObjectList = mod.gameObject.GetComponentsInChildren<Transform>();
                        List<GameObject> rendererList = gameObjectList.Where(t => t.gameObject.GetComponent<Renderer>() != null).Select(t => t.gameObject).ToList();
                        mod.objects[0].objects = rendererList.ToArray();
                    }
                }

                // visibility Range
                [ParserTarget("visibilityRange")]
                public NumericParser<float> visibilityRange
                {
                    get { return mod.objects[0].visibleRange; }
                    set { mod.objects[0].visibleRange = value; }
                }

                // Commnet Station
                [ParserTarget("commnetStation")]
                public NumericParser<bool> commnetStation
                {
                    get { return mod.gameObject.GetComponentInChildren<CommNetHome>() != null; }
                    set
                    {
                        if (value)
                        {
                            CommNetHome station = mod.gameObject.AddComponent<CommNetHome>();
                            station.isKSC = false;
                        }
                    }
                }

                // Apply event
                void IParserEventSubscriber.Apply(ConfigNode node)
                {
                    mod.objects = new [] { new PQSCity2.LodObject() };
                }

                // Apply event
                void IParserEventSubscriber.PostApply(ConfigNode node) { }
            }
        }
    }
}

