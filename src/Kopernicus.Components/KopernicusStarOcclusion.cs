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
using ModularFI;
using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// Determines how many vertices of a stars scaled space mesh are occluded by other bodies
        /// </summary>
        public class KopernicusStarOcclusion : MonoBehaviour
        {
            /// <summary>
            /// Defines how many vertices should be used. Every n-th vertex will get checked.
            /// </summary>
            public const Int32 Resolution = 50;

            /// <summary>
            /// Above which intensity should the occlusion get checked.
            /// </summary>
            public const Single IntensityThreshold = 0.11f;

            /// <summary>
            /// The percentage of surface area that is visible.
            /// </summary>
            public Single VisibleArea { get; private set; } = 1f;

            /// <summary>
            /// The star component we are attached to
            /// </summary>
            public KopernicusStar Star;

            /// <summary>
            /// The colliders that are used to detect occlusion
            /// </summary>
            private OcclusionNetComponent[] _colliders;

            void Start()
            {
                Mesh mesh = Star.sun.scaledBody.GetComponent<MeshFilter>().sharedMesh;
                _colliders = new OcclusionNetComponent[mesh.vertexCount / Resolution];
                for (Int32 i = 0; i < mesh.vertexCount; i += Resolution)
                {
                    OcclusionNetComponent component = new GameObject().AddComponent<OcclusionNetComponent>();
                    component.transform.parent = Star.transform.parent;
                    component.Body = Star.sun;
                    component.Vertex = mesh.vertices[i];
                    _colliders[i / Resolution] = component;
                }
            }

            void Update()
            {
                if (!HighLogic.LoadedSceneIsFlight || MapView.MapIsEnabled)
                {
                    VisibleArea = 1;
                    return;
                }
                Int32 amount = 0;
                for (Int32 i = 0; i < _colliders.Length; i++)
                {
                    _colliders[i].Target = FlightGlobals.ActiveVessel.transform;
                    if (!_colliders[i].IsOccluded)
                    {
                        amount++;
                    }
                }

                VisibleArea = amount / (Single) _colliders.Length;
            }

            /// <summary>
            /// This component represents a straight line from the camera position to a specific vertex on the surface of the star.
            /// It checks every frame whether other celestial bodies are occluding this connection and exposes this information.
            /// </summary>
            public class OcclusionNetComponent : MonoBehaviour
            {
                public Vector3 Vertex;
                public CelestialBody Body;
                public Transform Target;
                public Boolean IsOccluded { get; private set; }

                void Update()
                {
                    if (Target == null)
                    {
                        return;
                    }
                    
                    // The SpaceCenter scene doesn't load a full planetary system, or at least it messes with it 
                    // a lot. Instead of using the last value from flight there, we should just assume
                    // the star is fully visible there, until someone comes up with a better solution
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    {
                        IsOccluded = false;
                        return;
                    }

                    Vector3 vertexPos = ScaledSpace.ScaledToLocalSpace(Body.scaledBody.transform.TransformPoint(Vertex));
                    for (Int32 i = 0; i < PSystemManager.Instance.localBodies.Count; i++)
                    {
                        CelestialBody body = PSystemManager.Instance.localBodies[i];
                        if (body == Body)
                        {
                            continue;
                        }

                        if (IsPositionOccluded(vertexPos, Target, body))
                        {
                            IsOccluded = true;
                            return;
                        }
                    }

                    IsOccluded = false;
                }
                
                public static Boolean IsPositionOccluded(Vector3d worldPosition, Transform target, CelestialBody byBody)
                {
                    Vector3d camPos = target.position;
                    Vector3d VC = (byBody.position - camPos) / (byBody.Radius - 100);
                    Vector3d VT = (worldPosition - camPos) / (byBody.Radius - 100);

                    Double VT_VC = Vector3d.Dot(VT, VC);

                    // In front of the horizon plane
                    if (VT_VC < VC.sqrMagnitude - 1) return false;

                    return VT_VC * VT_VC / VT.sqrMagnitude > VC.sqrMagnitude - 1;
                }
            }
        }
    }
}