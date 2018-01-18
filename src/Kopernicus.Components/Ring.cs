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
using LibNoise;
using UnityEngine;
using Random = System.Random;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// Class to render a ring around a planet
        /// </summary>
        public class Ring : MonoBehaviour, IComponentSystem<Ring>
        {
            /// <summary>
            /// Components that can be added to the Ring
            /// </summary>
            public List<IComponent<Ring>> Components { get; set; }
            
            // Settings
            public Single innerRadius;
            public Single outerRadius;

            /// <summary>
            /// Thickness of ring in milliradii
            /// </summary>
            public Single thickness;

            public Single planetRadius;
            public Quaternion rotation;
            public Texture2D texture;
            public Color color;
            public Boolean lockRotation;

            /// <summary>
            /// Number of seconds for the ring to complete one rotation.
            /// If zero, fall back to matching parent body if lockRotation=false,
            /// and standing perfectly still if it's true.
            /// </summary>
            public Single rotationPeriod;

            /// <summary>
            /// Angle between the absolute reference direction and the ascending node.
            /// Works just like the corresponding property on celestial bodies.
            /// </summary>
            public Single longitudeOfAscendingNode;

            public Boolean unlit;
            public Boolean useNewShader;
            public Int32 steps = 128;

            /// <summary>
            /// Number of times the textures should be tiled around the cylinder
            /// If zero, use the old behavior of sampling a thin diagonal strip
            /// from (0,0) to (1,1).
            /// </summary>
            public Int32 tiles = 0;

            /// <summary>
            /// For new shader, makes planet shadow softer (values larger than one) or less soft (smaller than one)
            /// softness still depends on distance from sun, distance from planet and radius of sun and planet
            /// </summary>
            public Single penumbraMultiplier = 10f;

            /// <summary>
            /// This texture's opaque pixels cast shadows on our inner surface
            /// </summary>
            public Texture2D innerShadeTexture = null;

            /// <summary>
            /// The inner shade texture repeats this many times over the inner surface
            /// </summary>
            public Int32 innerShadeTiles = 0;

            /// <summary>
            /// Number of seconds the inner shade texture takes to complete one rotation
            /// </summary>
            public Single innerShadeRotationPeriod = 0;

            /// <summary>
            /// Multiply the time by this to get the offset of the inner shade texture
            /// </summary>
            private Single innerShadeOffsetRate = 0;

            /// <summary>
            /// The body around which this ring is located.
            /// Used to get rotation data to set the LAN.
            /// </summary>
            public CelestialBody referenceBody;

            public MeshRenderer ringMR;

            /// <summary>
            /// Create the module list
            /// </summary>
            void Awake()
            {
                Components = new List<IComponent<Ring>>();
            }

            /// <summary>
            /// Create the Ring Mesh
            /// </summary>
            void Start()
            {
                if (gameObject.GetComponent<MeshFilter>() == null)
                    BuildRing();
            }

            /// <summary>
            /// Builds the Ring
            /// </summary>
            public void BuildRing()
            {
                // Call the modules
                Components.ForEach(m => m.Apply(this));
                
                // Create the ring mesh
                GameObject parent = transform.parent.gameObject;
                List<Vector3> vertices = new List<Vector3>();
                List<Vector2> Uvs = new List<Vector2>();
                List<Int32> Tris = new List<Int32>();

                // These are invariant, so avoid Singleing point division in tight loops
                Single degreeStep = 360f / steps;
                Single innerScale = innerRadius / parent.transform.localScale.x;
                Single outerScale = outerRadius / parent.transform.localScale.x;

                if (tiles > 0)
                    MakeTiledMesh(vertices, Uvs, Tris,
                                  degreeStep, innerScale, outerScale,
                                  thickness * Vector3.up / parent.transform.localScale.x);
                else
                    MakeLinearMesh(vertices, Uvs, Tris,
                                   degreeStep, innerScale, outerScale);

                // Update Rotation
                transform.localRotation = rotation;

                // Update Scale and Layer
                transform.localScale = parent.transform.localScale;
                transform.position = parent.transform.localPosition;
                gameObject.layer = parent.layer;

                // Create MeshFilter
                MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

                // Set mesh
                meshFilter.mesh = new Mesh() {
                    vertices  = vertices.ToArray(),
                    triangles = Tris.ToArray(),
                    uv        = Uvs.ToArray()
                };
                meshFilter.mesh.RecalculateNormals();
                meshFilter.mesh.RecalculateBounds();
                meshFilter.mesh.Optimize();
                meshFilter.sharedMesh = meshFilter.mesh;

                // Set texture
                ringMR = gameObject.AddComponent<MeshRenderer>();
                Renderer parentRenderer = parent.GetComponent<Renderer>();
                ringMR.material = new Material(getShader());
                ringMR.material.SetTexture("_MainTex", texture);

                ringMR.material.SetFloat("innerRadius", innerRadius * parent.transform.localScale.x);
                ringMR.material.SetFloat("outerRadius", outerRadius * parent.transform.localScale.x);

                if (useNewShader)
                {
                    ringMR.material.SetFloat("planetRadius", planetRadius);
                    ringMR.material.SetFloat("penumbraMultiplier", penumbraMultiplier);

                    if (innerShadeTexture != null) {
                        ringMR.material.SetTexture("_InnerShadeTexture", innerShadeTexture);
                    }
                    if (innerShadeTiles > 0) {
                        ringMR.material.SetFloat("innerShadeTiles", tiles / innerShadeTiles);
                    }
                    if (innerShadeRotationPeriod > 0 && rotationPeriod > 0) {
                        innerShadeOffsetRate = innerShadeTiles * (
                              1 / innerShadeRotationPeriod
                            - 1 / rotationPeriod);
                    }
                }

                ringMR.material.color = color;
                ringMR.material.renderQueue = parentRenderer.material.renderQueue;
                parentRenderer.material.renderQueue--;
                
                // Call the modules
                Components.ForEach(m => m.PostApply(this));
            }

            /// <summary>
            /// The shaders used by the ring mesh
            /// </summary>
            private const String newShader = "Kopernicus/Rings",
                                 unlitShader = "Unlit/Transparent",
                                 diffuseShader = "Transparent/Diffuse";

            /// <summary>
            /// Queries the shader the material should use
            /// </summary>
            private Shader getShader()
            {
                if (useNewShader)
                    return ShaderLoader.GetShader(newShader);
                if (unlit)
                    return Shader.Find(unlitShader);
                return Shader.Find(diffuseShader);
            }

            /// <summary>
            /// Generate a simple mesh for a non-tiled ring.
            /// A line from one corner to the opposite corner is
            /// sampled from the texture to draw the ring.
            /// (Backwards compatible)
            ///
            /// | \     |
            /// |   \   |
            /// |     \ |
            /// </summary>
            /// <param name="vertices">List of vertices for the mesh</param>
            /// <param name="Uvs">List of texture coordinates for the mesh</param>
            /// <param name="Tris">List of triangles for the mesh</param>
            /// <param name="degreeStep">Width of each slice of the mesh in degrees</param>
            /// <param name="innerScale">Distance from center of parent to inner edge of ring</param>
            /// <param name="outerScale">Distance from center of parent to outer edge of ring</param>
            private void MakeLinearMesh(
                List<Vector3> vertices,
                List<Vector2> Uvs,
                List<Int32>     Tris,
                Single         degreeStep,
                Single         innerScale,
                Single         outerScale)
            {
                // Mesh wrapping
                for (Single i = 0f; i < 360f; i += degreeStep)
                {
                    // Rotation
                    Vector3 eVert = Quaternion.Euler(0, i, 0) * Vector3.right;

                    // Inner Radius
                    vertices.Add(eVert * innerScale);
                    Uvs.Add(Vector2.one);

                    // Outer Radius
                    vertices.Add(eVert * outerScale);
                    Uvs.Add(Vector2.zero);
                }
                for (Single i = 0f; i < 360f; i += degreeStep)
                {
                    // Rotation
                    Vector3 eVert = Quaternion.Euler(0, i, 0) * Vector3.right;

                    // Inner Radius
                    vertices.Add(eVert * innerScale);
                    Uvs.Add(Vector2.one);

                    // Outer Radius
                    vertices.Add(eVert * outerScale);
                    Uvs.Add(Vector2.zero);
                }

                // Tri Wrapping
                Int32 Wrapping = steps * 2;
                for (Int32 i = 0; i < Wrapping; i += 2)
                {
                    Tris.Add((i    ) % Wrapping);
                    Tris.Add((i + 1) % Wrapping);
                    Tris.Add((i + 2) % Wrapping);

                    Tris.Add((i + 1) % Wrapping);
                    Tris.Add((i + 3) % Wrapping);
                    Tris.Add((i + 2) % Wrapping);
                }
                for (Int32 i = 0; i < Wrapping; i += 2)
                {
                    Tris.Add(Wrapping + (i + 2) % Wrapping);
                    Tris.Add(Wrapping + (i + 1) % Wrapping);
                    Tris.Add(Wrapping + (i    ) % Wrapping);

                    Tris.Add(Wrapping + (i + 2) % Wrapping);
                    Tris.Add(Wrapping + (i + 3) % Wrapping);
                    Tris.Add(Wrapping + (i + 1) % Wrapping);
                }
            }

            /// <summary>
            /// Generate the vertical texture coordinate for the cylinder
            /// slice at a given degree offset, when tiling the texture a
            /// given number of times.
            /// </summary>
            /// <param name="numTiles">Number of times the texture is to be tiled around the cylinder</param>
            /// <param name="degrees">Angle between the requested vertex and an absolute reference direction in degrees</param>
            /// <returns>
            /// A vector that can be added to a U-vector to get the full
            /// texture coordinate.
            /// </returns>
            private static Vector2 textureV(Int32 numTiles, Single degrees)
            {
                return numTiles * (degrees / 360f) * Vector2.up;
            }

            /// <summary>
            /// The texture is split into sections, each of which
            /// is tiled over different parts of the mesh.
            /// The tiling is top-to-bottom.
            ///
            /// |      |            |            |
            /// | side |   inner    |   outer    |
            /// |      |            |            |
            /// </summary>
            /// <param name="vertices">List of vertices for the mesh</param>
            /// <param name="Uvs">List of texture coordinates for the mesh</param>
            /// <param name="Tris">List of triangles for the mesh</param>
            /// <param name="degreeStep">Width of each slice of the mesh in degrees</param>
            /// <param name="innerScale">Distance from center of parent to inner edge of ring</param>
            /// <param name="outerScale">Distance from center of parent to outer edge of ring</param>
            /// <param name="thicknessOffset">A vector that can be added to add the thickness of the ring</param>
            private void MakeTiledMesh(
                List<Vector3> vertices,
                List<Vector2> Uvs,
                List<Int32>     Tris,
                Single         degreeStep,
                Single         innerScale,
                Single         outerScale,
                Vector3       thicknessOffset)
            {
                const Single sideTexW  = 0.2f,
                            innerTexW = 0.4f,
                            outerTexW = 0.4f;
                // Define coordinates for 3 textures:
                //   1. The "side" faces that point normal and antinormal
                //      (classic rings)
                Vector2 sideInnerU   = Vector2.zero;
                Vector2 sideOuterU   = sideInnerU   + sideTexW  * Vector2.right;
                //   2. The "inner" face that points at the body
                Vector2 innerBottomU = sideOuterU;
                Vector2 innerTopU    = innerBottomU + innerTexW * Vector2.right;
                //   3. The "outer" face that points away from the body
                Vector2 outerBottomU = innerTopU;
                Vector2 outerTopU    = outerBottomU + outerTexW * Vector2.right;

                Int32 Wrapping = steps * 2;
                // Allow inner==outer for thin cylinders w/ only inner/outer faces
                if (innerRadius < outerRadius)
                {
                    // Top faces
                    for (Single i = 0f; i < 360f; i += degreeStep)
                    {
                        // Rotation
                        Vector3 eVert = Quaternion.Euler(0, i, 0) * Vector3.right;
                        Vector2 texV  = textureV(tiles, i);

                        // Inner Radius
                        vertices.Add(eVert * innerScale + thicknessOffset);
                        Uvs.Add(sideInnerU + texV);

                        // Outer Radius
                        vertices.Add(eVert * outerScale + thicknessOffset);
                        Uvs.Add(sideOuterU + texV);
                    }
                    // Bottom faces
                    for (Single i = 0f; i < 360f; i += degreeStep)
                    {
                        // Rotation
                        Vector3 eVert = Quaternion.Euler(0, i, 0) * Vector3.right;
                        Vector2 texV  = textureV(tiles, i);

                        // Inner Radius
                        vertices.Add(eVert * innerScale - thicknessOffset);
                        Uvs.Add(sideInnerU + texV);

                        // Outer Radius
                        vertices.Add(eVert * outerScale - thicknessOffset);
                        Uvs.Add(sideOuterU + texV);
                    }
                    // Tri Wrapping
                    for (Int32 i = 0; i < Wrapping; i += 2)
                    {
                        Tris.Add(i);
                        Tris.Add((i + 1) % Wrapping);
                        Tris.Add((i + 2) % Wrapping);

                        Tris.Add((i + 1) % Wrapping);
                        Tris.Add((i + 3) % Wrapping);
                        Tris.Add((i + 2) % Wrapping);

                        Tris.Add(Wrapping + (i + 2) % Wrapping);
                        Tris.Add(Wrapping + (i + 1) % Wrapping);
                        Tris.Add(Wrapping + i % Wrapping);

                        Tris.Add(Wrapping + (i + 2) % Wrapping);
                        Tris.Add(Wrapping + (i + 3) % Wrapping);
                        Tris.Add(Wrapping + (i + 1) % Wrapping);
                    }
                }

                // Inner and outer faces
                if (thickness > 0)
                {
                    Int32 firstTop = vertices.Count;
                    // Mesh wrapping - top faces
                    // We need to generate one extra pair of vertices
                    // so the last triangles' texture coordinates don't
                    // go from a high value back to zero.
                    for (Int32 i = 0; i <= steps; ++i)
                    {
                        Single f = i * degreeStep;

                        // Rotation
                        Vector3 eVert = Quaternion.Euler(0, f, 0) * Vector3.right;
                        Vector2 texV  = textureV(tiles, f);

                        // Inner Radius
                        vertices.Add(eVert * innerScale + thicknessOffset);
                        Uvs.Add(innerTopU + texV);

                        // Outer Radius
                        vertices.Add(eVert * outerScale + thicknessOffset);
                        Uvs.Add(outerTopU + texV);
                    }
                    // Mesh wrapping - bottom faces
                    for (Int32 i = 0; i <= steps; ++i)
                    {
                        Single f = i * degreeStep;

                        // Rotation
                        Vector3 eVert = Quaternion.Euler(0, f, 0) * Vector3.right;
                        Vector2 texV  = textureV(tiles, f);

                        // Inner Radius
                        vertices.Add(eVert * innerScale - thicknessOffset);
                        Uvs.Add(innerBottomU + texV);

                        // Outer Radius
                        vertices.Add(eVert * outerScale - thicknessOffset);
                        Uvs.Add(outerBottomU + texV);
                    }
                    // Tri Wrapping
                    // No modulus this time because we want to use those
                    // extra vertices instead of having the
                    // texture coordinates loop back around.
                    for (Int32 i = 0; i < Wrapping; i += 2)
                    {
                        // Inner faces
                        Tris.Add(firstTop + i);
                        Tris.Add(firstTop + (i + 2));
                        Tris.Add(firstTop + Wrapping + 2 + i);

                        Tris.Add(firstTop + Wrapping + 2 + i);
                        Tris.Add(firstTop + (i + 2));
                        Tris.Add(firstTop + Wrapping + 2 + (i + 2));
                    }
                    // Outer faces should always draw after inner to
                    // make the overlaps render correctly
                    for (Int32 i = 0; i < Wrapping; i += 2)
                    {
                        // Outer faces
                        Tris.Add(firstTop + (i + 1));
                        Tris.Add(firstTop + Wrapping + 2 + (i + 1));
                        Tris.Add(firstTop + (i + 3));

                        Tris.Add(firstTop + Wrapping + 2 + (i + 1));
                        Tris.Add(firstTop + Wrapping + 2 + (i + 3));
                        Tris.Add(firstTop + (i + 3));
                    }
                }
            }

            /// <summary>
            /// Update the scale and the lock
            /// </summary>
            void Update()
            {
                transform.localScale = transform.parent.localScale;
                SetRotation();

                if (useNewShader && ringMR?.material != null
                    && KopernicusStar.Current?.sun?.transform != null)
                {
                    ringMR.material.SetFloat("sunRadius",
                        (Single) KopernicusStar.Current.sun.Radius);
                    ringMR.material.SetVector("sunPosRelativeToPlanet",
                        (Vector3) (KopernicusStar.Current.sun.transform.position -
                                   ScaledSpace.ScaledToLocalSpace(transform.position)));
                    ringMR.material.SetFloat("innerShadeOffset",
                        (Single) (Planetarium.GetUniversalTime() * innerShadeOffsetRate));
                }

                // Call Modules
                Components.ForEach(m => m.Update(this));
            }

            /// <summary>
            /// Update the scale and the lock
            /// </summary>
            void FixedUpdate()
            {
                transform.localScale = transform.parent.localScale;
                SetRotation();
                
                // Call Modules
                Components.ForEach(m => m.Update(this));
            }

            /// <summary>
            /// Update the scale and the lock
            /// </summary>
            void LateUpdate()
            {
                transform.localScale = transform.parent.localScale;
                SetRotation();
                
                // Call Modules
                Components.ForEach(m => m.Update(this));
            }

            /// <summary>
            /// Populate our transform's rotation quaternion
            /// </summary>
            private void SetRotation()
            {
                if (lockRotation && referenceBody != null) {
                    // Setting transform.rotation does NOT give us a consistent
                    // absolute orientation as you would expect from the documentation.
                    // "World" coordinates seem to be set differently each time the
                    // game is loaded. Instead, we use localRotation to orient the ring
                    // relative to its parent body, subtract off the parent body's
                    // rotation at the current moment in time, then add the LAN.
                    // Note that eastward (prograde) rotation is negative in trigonometry.
                    if (rotationPeriod != 0f) {
                        Single degreesPerSecond = -360f / rotationPeriod;
                        Single parentRotation = (Single) (referenceBody.initialRotation + 360 * Planetarium.GetUniversalTime() / referenceBody.rotationPeriod);
                        transform.localRotation =
                            Quaternion.Euler(0, parentRotation - longitudeOfAscendingNode, 0)
                            * rotation
                            * Quaternion.Euler(0, (Single)Planetarium.GetUniversalTime() * degreesPerSecond, 0);
                    } else {
                        Single parentRotation = (Single) (referenceBody.initialRotation + 360 * Planetarium.GetUniversalTime() / referenceBody.rotationPeriod);
                        transform.localRotation =
                            Quaternion.Euler(0, parentRotation - longitudeOfAscendingNode, 0)
                            * rotation;
                    }
                }
            }
        }
    }
}
