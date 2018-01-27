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
            private KopernicusStar _star;

            /// <summary>
            /// The colliders that are used to detect occlusion
            /// </summary>
            private OcclusionNetComponent[] _colliders;

            void Start()
            {
                _star = GetComponent<KopernicusStar>();
                Mesh mesh = _star.sun.scaledBody.GetComponent<MeshFilter>().sharedMesh;
                _colliders = new OcclusionNetComponent[mesh.vertexCount / Resolution];
                for (Int32 i = 0; i < mesh.vertexCount; i += Resolution)
                {
                    OcclusionNetComponent component = new GameObject().AddComponent<OcclusionNetComponent>();
                    component.transform.parent = _star.transform.parent;
                    component.Body = _star.sun;
                    component.Target = _star.target;
                    component.Vertex = mesh.vertices[i];
                    _colliders[i / Resolution] = component;
                }
            }

            void Update()
            {
                Int32 amount = 0;
                for (Int32 i = 0; i < _colliders.Length; i++)
                {
                    _colliders[i].Target = _star.target;
                    _colliders[i].enabled =
                        _star.shifter.intensityCurve.Evaluate((Single) Vector3d.Distance(_star.sun.position,
                            ScaledSpace.ScaledToLocalSpace(_star.target.position))) > IntensityThreshold;
                    if (_colliders[i].IsOccluded || !_colliders[i].enabled)
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
                        IsOccluded = true;
                        return;
                    }

                    Vector3 vertexPos = Body.scaledBody.transform.position + new Vector3(
                                            Vertex.x * Body.scaledBody.transform.localScale.x,
                                            Vertex.y * Body.scaledBody.transform.localScale.y,
                                            Vertex.z * Body.scaledBody.transform.localScale.z);
                    Collider[] colliders = Physics.OverlapCapsule(Target.position, vertexPos, 1f,
                        ModularFlightIntegrator.SunLayerMask);
                    IsOccluded = !colliders.Any(c =>
                    {
                        Debug.Log(c);
                        ScaledMovement m = c.GetComponent<ScaledMovement>();
                        if (m == null)
                        {
                            return false;
                        }

                        if (m.celestialBody == FlightGlobals.currentMainBody)
                        {
                            return false;
                        }

                        if (m.celestialBody != Body)
                        {
                            return true;
                        }

                        return false;
                    });
                }
            }
        }
    }
}