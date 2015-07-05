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

            private Color groundColor = Color.clear;
            private Texture2D groundTexture;

            // latitude
            [ParserTarget("latitude", optional = true, allowMerge = false)]
            public NumericParser<double> latitude
            {
                set { ksc.latitude = value.value; hasLatitude = true; }
            }
            private bool hasLatitude = false;

            // longitude
            [ParserTarget("longitude", optional = true, allowMerge = false)]
            public NumericParser<double> longitude
            {
                set { ksc.longitude = value.value; hasLongitude = true; }
            }
            private bool hasLongitude = false;

            [ParserTarget("repositionRadial", optional = true, allowMerge = false)]
            public Vector3Parser repositionRadial
            {
                set { ksc.repositionRadial = value.value; hasReposRadial = true; }
            }
            private bool hasReposRadial = false;

            // decalLatitude
            [ParserTarget("decalLatitude", optional = true, allowMerge = false)]
            public NumericParser<double> decalLatitude
            {
                set { ksc.decalLatitude = value.value; hasDecalLatitude = true; }
            }
            private bool hasDecalLatitude = false;

            // decalLongitude
            [ParserTarget("decalLongitude", optional = true, allowMerge = false)]
            public NumericParser<double> decalLongitude
            {
                set { ksc.decalLongitude = value.value; hasDecalLongitude = true; }
            }
            private bool hasDecalLongitude = false;

            // lodvisibleRangeMultipler
            [ParserTarget("lodvisibleRangeMultipler", optional = true, allowMerge = false)]
            public NumericParser<double> lodvisibleRangeMultipler
            {
                set { ksc.lodvisibleRangeMult = value.value; hasLodMult = true; }
            }
            private bool hasLodMult = false;


            // reorientFinalAngle
            [ParserTarget("reorientFinalAngle", optional = true, allowMerge = false)]
            public NumericParser<float> reorientFinalAngle
            {
                set { ksc.reorientFinalAngle = value.value; hasReorientAngle = true; }
            }
            private bool hasReorientAngle = false;

            // reorientInitialUp
            [ParserTarget("reorientInitialUp", optional = true, allowMerge = false)]
            public Vector3Parser reorientInitialUp
            {
                set { ksc.reorientInitialUp = value.value; hasInitialUp = true; }
            }
            private bool hasInitialUp = false;

            // reorientToSphere
            [ParserTarget("reorientToSphere", optional = true, allowMerge = false)]
            public NumericParser<bool> reorientToSphere
            {
                set
                {
                    ksc.reorientToSphere = value.value;
                    hasReorientToSphere = true;
                }
            }
            private bool hasReorientToSphere = false;

            // repositionRadiusOffset
            [ParserTarget("repositionRadiusOffset", optional = true, allowMerge = false)]
            public NumericParser<double> repositionRadiusOffset
            {
                set { ksc.repositionRadiusOffset = value.value; hasReposRadiusOffset = true; }
            }
            private bool hasReposRadiusOffset = false;

            // repositionToSphere
            [ParserTarget("repositionToSphere", optional = true, allowMerge = false)]
            public NumericParser<bool> repositionToSphere
            {
                set
                {
                    ksc.repositionToSphere = value.value;
                    hasReposToSphere = false;
                }
            }
            private bool hasReposToSphere = false;

            // repositionToSphereSurface
            [ParserTarget("repositionToSphereSurface", optional = true, allowMerge = false)]
            public NumericParser<bool> repositionToSphereSurface
            {
                set
                {
                    ksc.repositionToSphereSurface = value.value;
                    hasReposToSphereSurface = true;
                }
            }
            private bool hasReposToSphereSurface = false;

            // repositionToSphereSurfaceAddHeight
            [ParserTarget("repositionToSphereSurfaceAddHeight", optional = true, allowMerge = false)]
            public NumericParser<bool> repositionToSphereSurfaceAddHeight
            {
                set
                {
                    ksc.repositionToSphereSurfaceAddHeight = value.value;
                    hasAddHeight = true;
                }
            }
            private bool hasAddHeight = false;

            // position
            [ParserTarget("position", optional = true, allowMerge = false)]
            public Vector3Parser position
            {
                set { ksc.position = value.value; hasPos = true; }
            }
            private bool hasPos = false;

            // radius
            [ParserTarget("radius", optional = true, allowMerge = false)]
            public NumericParser<double> radius
            {
                set { ksc.radius = value.value; hasRadius = true; }
            }
            private bool hasRadius = true;

            // heightMapDeformity
            [ParserTarget("heightMapDeformity", optional = true, allowMerge = false)]
            public NumericParser<double> heightMapDeformity
            {
                set { ksc.heightMapDeformity = value.value; hasDeformity = true; }
            }
            private bool hasDeformity = false;

            // absoluteOffset
            [ParserTarget("absoluteOffset", optional = true, allowMerge = false)]
            public NumericParser<double> absoluteOffset
            {
                set { ksc.absoluteOffset = value.value; hasAbsOffset = true; }
            }
            private bool hasAbsOffset = false;

            // absolute
            [ParserTarget("absolute", optional = true, allowMerge = false)]
            public NumericParser<bool> absolute
            {
                set
                {
                    ksc.absolute = value.value;
                    hasAbsolute = true;
                }
            }
            private bool hasAbsolute = false;

            // groundColor
            [ParserTarget("groundColor", optional = true, allowMerge = false)]
            public ColorParser groundColorParser
            {
                set { KSCGroundFixer.color = value.value; }
            }

            // Texture
            [ParserTarget("groundTexture", optional = true, allowMerge = false)]
            public Texture2DParser groundTextureParser
            {
                set { KSCGroundFixer.mainTexture = value.value; }
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
                if (hasLatitude && hasLongitude)
                {
                    pqsKSC.repositionRadial = Utility.LLAtoECEF(ksc.latitude, ksc.longitude, 0, body.Radius);
                }
                else if (hasReposRadial)
                {
                    pqsKSC.repositionRadial = ksc.repositionRadial;
                }

                if (hasInitialUp)
                {
                    pqsKSC.reorientInitialUp = ksc.reorientInitialUp;
                }

                if (hasReposToSphere)
                {
                    pqsKSC.repositionToSphere = ksc.repositionToSphere;
                }

                if (hasReposToSphereSurface)
                {
                    pqsKSC.repositionToSphereSurface = ksc.repositionToSphereSurface;
                }

                if (hasAddHeight)
                {
                    pqsKSC.repositionToSphereSurfaceAddHeight = ksc.repositionToSphereSurfaceAddHeight;
                }

                if (hasReorientToSphere)
                {
                    pqsKSC.reorientToSphere = ksc.reorientToSphere;
                }

                if (hasReposRadiusOffset)
                {
                    pqsKSC.repositionRadiusOffset = ksc.repositionRadiusOffset;
                }

                if (hasLodMult)
                {
                    foreach (PQSCity.LODRange lodRange in pqsKSC.lod)
                    {
                        lodRange.visibleRange *= (float)ksc.lodvisibleRangeMult;
                    }
                }

                if (hasReorientAngle)
                {
                    pqsKSC.reorientFinalAngle = ksc.reorientFinalAngle;
                }

                // Get the MapDecalTanget, i.e. the flat area around KSC
                PQSMod_MapDecalTangent pqsMap = body.pqsController.transform.GetComponentsInChildren<PQSMod_MapDecalTangent>(true).Where(m => m.name == "KSC").First();
                
                // Load new data into the mod
                if (hasRadius)
                {
                    pqsMap.radius = ksc.radius;
                }

                if (hasDeformity)
                {
                    pqsMap.heightMapDeformity = ksc.heightMapDeformity;
                }

                if (hasAbsOffset)
                {
                    pqsMap.absoluteOffset = ksc.absoluteOffset;
                }

                if (hasAbsolute)
                {
                    pqsMap.absolute = ksc.absolute;
                }

                if (hasDecalLatitude && hasDecalLongitude)
                {
                    pqsMap.position = Utility.LLAtoECEF(ksc.decalLatitude, ksc.decalLongitude, 0, body.Radius);
                }
                else if (hasPos)
                {
                    pqsMap.position = ksc.position;
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
            public Vector3 repositionRadial;
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
            public double decalLatitude;
            public double decalLongitude;
        }

        // This class is used to manipulate the grass color and texture of the KSC
        [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
        public class KSCGroundFixer : MonoBehaviour
        {
            // Declare public variables
            public static Texture2D mainTexture = null;
            public static Color color = Color.clear;

            private static bool isDone = false;

            // Every time this starts, it'll look for the standard-ground materials and change their settings.
            public void Update()
            {
                // Are we alive?
                if (!isDone)
                {
                    // Loop through all Materials and change their settings
                    foreach (Material material in UnityEngine.Resources.FindObjectsOfTypeAll<Material>().Where(m => m.color.ToString() == new Color(0.382f, 0.451f, 0.000f, 0.729f).ToString()))
                    {
                        // Patch the texture
                        if (mainTexture != null)
                        {
                            material.mainTexture = mainTexture;
                        }

                        // Patch the color
                        if (color != Color.clear)
                        {
                            material.color = color;
                        }
                    }
                   
                    // And Stop
                    isDone = true;
                }
            }
        }
    }
}
