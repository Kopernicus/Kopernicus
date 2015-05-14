/** 
 * Kopernicus Planetary System Modifier
 * Copyright (C) 2014 Bryce C Schroeder (bryce.schroeder@gmail.com), Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * http://www.ferazelhosting.net/~bryce/contact.html
 * 
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

using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Kopernicus
{
	namespace Configuration
	{
		[RequireConfigType(ConfigType.Node)]
        public class SpaceCenterSwitcher : IParserEventSubscriber
        {
            public KSC ksc;
            public static SpaceCenterSwitcher Instance;

            private bool[] hasChanged = new bool[] { false, false, false, false, false };

            // latitude
            [ParserTarget("latitude", optional = true, allowMerge = false)]
            public NumericParser<double> latitude
            {
                set { ksc.latitude = value.value; }
            }

            // longitude
            [ParserTarget("longitude", optional = true, allowMerge = false)]
            public NumericParser<double> longitude
            {
                set { ksc.longitude = value.value; }
            }

            // lodvisibleRangeMultipler
            [ParserTarget("lodvisibleRangeMultipler", optional = true, allowMerge = false)]
            public NumericParser<double> lodvisibleRangeMultipler
            {
                set { ksc.lodvisibleRangeMult = value.value; }
            }

            // reorientFinalAngle
            [ParserTarget("reorientFinalAngle", optional = true, allowMerge = false)]
            public NumericParser<float> reorientFinalAngle
            {
                set { ksc.reorientFinalAngle = value.value; }
            }

            // reorientInitialUp
            [ParserTarget("reorientInitialUp", optional = true, allowMerge = false)]
            public Vector3Parser reorientInitialUp
            {
                set { ksc.reorientInitialUp = value.value; }
            }

            // reorientToSphere
            [ParserTarget("reorientToSphere", optional = true, allowMerge = false)]
            public NumericParser<bool> reorientToSphere
            {
                set
                {
                    ksc.reorientToSphere = value.value;
                    hasChanged[0] = true;
                }
            }

            // repositionRadiusOffset
            [ParserTarget("repositionRadiusOffset", optional = true, allowMerge = false)]
            public NumericParser<double> repositionRadiusOffset
            {
                set { ksc.repositionRadiusOffset = value.value; }
            }

            // repositionToSphere
            [ParserTarget("repositionToSphere", optional = true, allowMerge = false)]
            public NumericParser<bool> repositionToSphere
            {
                set
                {
                    ksc.repositionToSphere = value.value; 
                    hasChanged[1] = true;
                }
            }

            // repositionToSphereSurface
            [ParserTarget("repositionToSphereSurface", optional = true, allowMerge = false)]
            public NumericParser<bool> repositionToSphereSurface
            {
                set
                {
                    ksc.repositionToSphereSurface = value.value;
                    hasChanged[2] = true;
                }
            }

            // repositionToSphereSurfaceAddHeight
            [ParserTarget("repositionToSphereSurfaceAddHeight", optional = true, allowMerge = false)]
            public NumericParser<bool> repositionToSphereSurfaceAddHeight
            {
                set
                {
                    ksc.repositionToSphereSurfaceAddHeight = value.value;
                    hasChanged[3] = true;
                }
            }

            // position
            [ParserTarget("position", optional = true, allowMerge = false)]
            public Vector3Parser position
            {
                set { ksc.position = value.value; }
            }

            // radius
            [ParserTarget("radius", optional = true, allowMerge = false)]
            public NumericParser<double> radius
            {
                set { ksc.radius = value.value; }
            }

            // heightMapDeformity
            [ParserTarget("heightMapDeformity", optional = true, allowMerge = false)]
            public NumericParser<double> heightMapDeformity
            {
                set { ksc.heightMapDeformity = value.value; }
            }

            // absoluteOffset
            [ParserTarget("absoluteOffset", optional = true, allowMerge = false)]
            public NumericParser<double> absoluteOffset
            {
                set { ksc.absoluteOffset = value.value; }
            }

            // absolute
            [ParserTarget("absolute", optional = true, allowMerge = false)]
            public NumericParser<bool> absolute
            {
                set
                {
                    ksc.absolute = value.value;
                    hasChanged[4] = true;
                }
            }

            public SpaceCenterSwitcher()
            {
                Instance = this;
                ksc = new KSC();
            }

            // Apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
            }

            // Post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
            }

            public void MoveKSC()
            {
                // Get the KSC-object from Kerbins PQS
                CelestialBody body = FlightGlobals.Bodies.Where(b => b.bodyName == "Kerbin").First();
                PQSCity pqsKSC = body.pqsController.transform.GetComponentsInChildren<PQSCity>(true).Where(m => m.name == "KSC").First();

                // Load new data into the PQSCity
                if (ksc.latitude != double.NaN && ksc.longitude != double.NaN)
                {
                    pqsKSC.repositionRadial = Utility.LLAtoECEF(ksc.latitude, ksc.longitude, 0, body.Radius);
                }

                if (ksc.reorientInitialUp != null)
                {
                    pqsKSC.reorientInitialUp = ksc.reorientInitialUp;
                }

                if (hasChanged[1])
                {
                    pqsKSC.repositionToSphere = ksc.repositionToSphere;
                }

                if (hasChanged[2])
                {
                    pqsKSC.repositionToSphereSurface = ksc.repositionToSphereSurface;
                }

                if (hasChanged[3])
                {
                    pqsKSC.repositionToSphereSurfaceAddHeight = ksc.repositionToSphereSurfaceAddHeight;
                }

                if (hasChanged[0])
                {
                    pqsKSC.reorientToSphere = ksc.reorientToSphere;
                }

                if (ksc.repositionRadiusOffset != double.NaN)
                {
                    pqsKSC.repositionRadiusOffset = ksc.repositionRadiusOffset;
                }

                if (ksc.lodvisibleRangeMult != double.NaN)
                {
                    foreach (PQSCity.LODRange lodRange in pqsKSC.lod)
                    {
                        lodRange.visibleRange *= (float)ksc.lodvisibleRangeMult;
                    }
                }

                if (ksc.reorientFinalAngle != float.NaN)
                {
                    pqsKSC.reorientFinalAngle = ksc.reorientFinalAngle;
                }

                // Get the MapDecalTanget, i.e. the flat area around KSC
                PQSMod_MapDecalTangent pqsMap = body.pqsController.transform.GetComponentsInChildren<PQSMod_MapDecalTangent>(true).Where(m => m.name == "KSC").First();
                
                // Load new data into the mod
                if (ksc.radius != double.NaN)
                {
                    pqsMap.radius = ksc.radius;
                }

                if (ksc.heightMapDeformity != double.NaN)
                {
                    pqsMap.heightMapDeformity = ksc.heightMapDeformity;
                }

                if (ksc.absoluteOffset != double.NaN)
                {
                    pqsMap.absoluteOffset = ksc.absoluteOffset;
                }

                if (hasChanged[4])
                {
                    pqsMap.absolute = ksc.absolute;
                }

                if (ksc.latitude != double.NaN && ksc.longitude != double.NaN)
                {
                    pqsMap.position = Utility.LLAtoECEF(ksc.latitude, ksc.longitude, 0, body.Radius);
                }
                
                // Reset the PQSMods
                pqsKSC.OnSetup();
                pqsKSC.OnPostSetup();

                SpaceCenter.Instance.transform.localPosition = pqsKSC.transform.localPosition;
                SpaceCenter.Instance.transform.localRotation = pqsKSC.transform.localRotation;

                pqsMap.OnSetup();
            }
        }

        public class KSC
        {
            // PQSCity
            public double latitude;
            public double longitude;
            public Vector3 reorientInitialUp;
            public bool repositionToSphere;
            public bool repositionToSphereSurface;
            public bool repositionToSphereSurfaceAddHeight;
            public bool reorientToSphere;
            public double repositionRadiusOffset;
            public double lodvisibleRangeMult;
            public float reorientFinalAngle;

            // PQSMod_MapDecalTangent
            public Vector3 position;
            public double radius;
            public double heightMapDeformity;
            public double absoluteOffset;
            public bool absolute;
        }
    }
}
