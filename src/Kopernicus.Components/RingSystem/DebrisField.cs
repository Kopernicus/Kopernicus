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

#if FALSE
using System;
using LibNoise;
using UnityEngine;
using Random = System.Random;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// A component that adds an experimental debris field to a planetary ring
        /// </summary>
        public class DebrisField : IComponent<Ring>
        {
            /// <summary>
            /// The seed that is used to select the meshes
            /// </summary>
            public Int32 seed;

            /// <summary>
            /// The meshes that can be applied to the debris ring
            /// </summary>
            public GameObject[] meshes;

            // The noise that determines the offset
            public IModule xNoise;
            public IModule yNoise;
            public IModule zNoise;

            // The noise that determines the rotation
            public IModule xRotNoise;
            public IModule yRotNoise;
            public IModule zRotNoise;

            /// <summary>
            /// A vector that determines the offset, gets multiplied with the results of the noise modules
            /// </summary>
            public Vector3 offset;

            /// <summary>
            /// Defines the density of the debris ring
            /// </summary>
            public Texture2D densityMap;

            /// <summary>
            /// How many objects can fit into the ring
            /// </summary>
            public Int32 outwardSteps;

            // Apply event
            void IComponent<Ring>.Apply(Ring system) { }

            // PostApply event
            void IComponent<Ring>.PostApply(Ring system)
            {
                // Prepare some things
                Random random = new Random(seed);
                Single degreeStep = 360f / system.steps;
                Single innerScale = system.innerRadius / system.transform.parent.localScale.x;
                Single outerScale = system.outerRadius / system.transform.parent.localScale.x;
                Single deltaScale = outerScale - innerScale;
                Single tileScale = system.tiles > 0 ? 0.2f : 1f;
                
                // Mesh wrapping
                for (Single i = 0f; i < 360f; i += degreeStep)
                {
                    // Rotation
                    Vector3 eVert = Quaternion.Euler(0, i, 0) * Vector3.right;

                    for (Single j = 0f; j < deltaScale; j += (deltaScale / outwardSteps))
                    {
                        // Density
                        Int32 coord = (Int32) (j / deltaScale * densityMap.width);
                        Single density = densityMap.GetPixel(coord, coord).grayscale;
                        if (random.Next(0, 100) < density * 100)
                        {
                            Int32 coordTile = (Int32) (coord * tileScale);
                            Color pixelColor = system.texture.GetPixel(coordTile, coordTile);
                            Vector3d direction = new Vector3d(i / 360f, 0, j / deltaScale);
                            
                            // Select a mesh to apply
                            GameObject meshObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            meshObject.layer = system.gameObject.layer;
                            meshObject.transform.parent = system.transform;
                            meshObject.transform.localPosition = eVert * (j + innerScale) +
                                                                 new Vector3(
                                                                     offset.x * (Single)xNoise.GetValue(direction),
                                                                     offset.y * (Single)yNoise.GetValue(direction),
                                                                     offset.z * (Single)zNoise.GetValue(direction));
                            meshObject.GetComponent<Renderer>().sharedMaterial.color = pixelColor;
                            meshObject.transform.localRotation = Quaternion.Euler(
                                (Single) xRotNoise.GetValue(direction),
                                (Single) yRotNoise.GetValue(direction), (Single) zRotNoise.GetValue(direction));
                            DebrisLOD lod = new GameObject().AddComponent<DebrisLOD>();
                            lod.transform.parent = system.transform;
                            lod.Target = meshObject;
                        }
                    }
                }
            }

            // Update event
            void IComponent<Ring>.Update(Ring system)
            {
                
            }
            
            /// <summary>
            /// A monobehaviour that controls when a piece of debris will be visible
            /// </summary>
            public class DebrisLOD : MonoBehaviour
            {
                public GameObject Target;
                
                /// <summary>
                /// Check if the object is visible every frame. If not, disable rendering and colliders
                /// </summary>
                void Update()
                {
                    if ((HighLogic.LoadedSceneHasPlanetarium || MapView.MapIsEnabled) && Target.activeSelf)
                        Target.SetActive(false);
                    if (HighLogic.LoadedSceneIsFlight)
                    {
                        Boolean isInDistance =
                            (ScaledSpace.ScaledToLocalSpace(Target.transform.position) -
                             FlightGlobals.ActiveVessel.vesselTransform.position).magnitude <
                            PhysicsGlobals.Instance.VesselRangesDefault.orbit.unload;
                        if (Target.activeSelf && !isInDistance)
                        {
                            Target.SetActive(false);
                        }
                        if (!Target.activeSelf && isInDistance)
                        {
                            Target.SetActive(true);
                        }
                    }
                }
            }
        }
    }
}
#endif