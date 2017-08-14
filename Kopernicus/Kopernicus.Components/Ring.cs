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
using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// Class to render a ring around a planet
        /// </summary>
        public class Ring : MonoBehaviour
        {
            /// Settings
            public float innerRadius;
            public float outerRadius;
            /// <summary>
            /// Thickness of ring in milliradii
            /// </summary>
            public float thickness;
            public float planetRadius;
            public Quaternion rotation;
            public Texture2D texture;
            public Color color;
            public bool lockRotation;
            /// <summary>
            /// Number of seconds for the ring to complete one rotation.
            /// If zero, fall back to matching parent body if lockRotation=false,
            /// and standing perfectly still if it's true.
            /// </summary>
            public float rotationPeriod;
            /// <summary>
            /// Angle between the absolute reference direction and the ascending node.
            /// Works just like the corresponding property on celestial bodies.
            /// </summary>
            public float longitudeOfAscendingNode;
            public bool unlit;
            public bool useNewShader;
            public int steps = 128;
            /// <summary>
            /// Number of times the textures should be tiled around the cylinder
            /// If zero, use the old behavior of sampling a thin diagonal strip
            /// from (0,0) to (1,1).
            /// </summary>
            public int tiles = 0;
            /// <summary>
            /// For new shader, makes planet shadow softer (values larger than one) or less soft (smaller than one)
            /// softness still depends on distance from sun, distance from planet and radius of sun and planet
            /// </summary>
            public float penumbraMultiplier = 10f;

            /// <summary>
            /// The body around which this ring is located.
            /// Used to get rotation data to set the LAN.
            /// </summary>
            public CelestialBody referenceBody;

            public MeshRenderer ringMR;

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
                GameObject    parent     = transform.parent.gameObject;
                List<Vector3> vertices   = new List<Vector3>();
                List<Vector2> Uvs        = new List<Vector2>();
                List<int>     Tris       = new List<int>();

                // These are invariant, so avoid floating point division in tight loops
                float         degreeStep = 360f / steps;
                float         innerScale = innerRadius / parent.transform.localScale.x;
                float         outerScale = outerRadius / parent.transform.localScale.x;

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
                transform.position   = parent.transform.localPosition;
                gameObject.layer     = parent.layer;

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
                    ringMR.material.SetFloat("planetRadius",       planetRadius);
                    ringMR.material.SetFloat("penumbraMultiplier", penumbraMultiplier);
                }

                ringMR.material.color = color;

                ringMR.material.renderQueue = parentRenderer.material.renderQueue;
                parentRenderer.material.renderQueue--;
            }

            private const string newShader     = "Kopernicus/Rings",
                                 unlitShader   = "Unlit/Transparent",
                                 diffuseShader = "Transparent/Diffuse";

            private Shader getShader()
            {
                if (useNewShader)
                    return ShaderLoader.GetShader(newShader);
                else if (unlit)
                    return Shader.Find(unlitShader);
                else
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
                List<int>     Tris,
                float         degreeStep,
                float         innerScale,
                float         outerScale)
            {
                // Mesh wrapping
                for (float i = 0f; i < 360f; i += (360f / steps))
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
                for (float i = 0f; i < 360f; i += (360f / steps))
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
                int Wrapping = steps * 2;
                for (int i = 0; i < Wrapping; i += 2)
                {
                    Tris.Add((i    ) % Wrapping);
                    Tris.Add((i + 1) % Wrapping);
                    Tris.Add((i + 2) % Wrapping);

                    Tris.Add((i + 1) % Wrapping);
                    Tris.Add((i + 3) % Wrapping);
                    Tris.Add((i + 2) % Wrapping);
                }
                for (int i = 0; i < Wrapping; i += 2)
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
            private static Vector2 textureV(int numTiles, float degrees)
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
                List<int>     Tris,
                float         degreeStep,
                float         innerScale,
                float         outerScale,
                Vector3       thicknessOffset)
            {
                const float sideTexW  = 0.2f,
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

                int Wrapping = steps * 2;
                // Allow inner==outer for thin cylinders w/ only inner/outer faces
                if (innerRadius < outerRadius)
                {
                    // Top faces
                    for (float i = 0f; i < 360f; i += degreeStep)
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
                    for (float i = 0f; i < 360f; i += degreeStep)
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
                    for (int i = 0; i < Wrapping; i += 2)
                    {
                        Tris.Add(           (i    )           );
                        Tris.Add(           (i + 1) % Wrapping);
                        Tris.Add(           (i + 2) % Wrapping);

                        Tris.Add(           (i + 1) % Wrapping);
                        Tris.Add(           (i + 3) % Wrapping);
                        Tris.Add(           (i + 2) % Wrapping);

                        Tris.Add(Wrapping + (i + 2) % Wrapping);
                        Tris.Add(Wrapping + (i + 1) % Wrapping);
                        Tris.Add(Wrapping + (i    ) % Wrapping);

                        Tris.Add(Wrapping + (i + 2) % Wrapping);
                        Tris.Add(Wrapping + (i + 3) % Wrapping);
                        Tris.Add(Wrapping + (i + 1) % Wrapping);
                    }
                }

                // Inner and outer faces
                if (thickness > 0)
                {
                    int firstTop = vertices.Count;
                    // Mesh wrapping - top faces
                    // We need to generate one extra pair of vertices
                    // so the last triangles' texture coordinates don't
                    // go from a high value back to zero.
                    for (int i = 0; i <= steps; ++i)
                    {
                        float f = i * degreeStep;

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
                    for (int i = 0; i <= steps; ++i)
                    {
                        float f = i * degreeStep;

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
                    for (int i = 0; i < Wrapping; i += 2)
                    {
                        // Inner faces
                        Tris.Add(firstTop +              (i    ));
                        Tris.Add(firstTop +              (i + 2));
                        Tris.Add(firstTop + Wrapping+2 + (i    ));

                        Tris.Add(firstTop + Wrapping+2 + (i    ));
                        Tris.Add(firstTop +              (i + 2));
                        Tris.Add(firstTop + Wrapping+2 + (i + 2));

                        // Outer faces
                        Tris.Add(firstTop +              (i + 1));
                        Tris.Add(firstTop + Wrapping+2 + (i + 1));
                        Tris.Add(firstTop +              (i + 3));

                        Tris.Add(firstTop + Wrapping+2 + (i + 1));
                        Tris.Add(firstTop + Wrapping+2 + (i + 3));
                        Tris.Add(firstTop +              (i + 3));
                    }
                }
            }

            /// <summary>
            /// Update the scale and the lock
            /// </summary>
            void Update()
            {
                transform.localScale = transform.parent.localScale;
                setRotation();

                if (useNewShader)
                {
                    ringMR.material.SetFloat("sunRadius", (float)KopernicusStar.Current.sun.Radius);
                    Vector3 sunPosRelativeToPlanet = KopernicusStar.Current.sun.transform.position - ScaledSpace.ScaledToLocalSpace(transform.position);
                    ringMR.material.SetVector("sunPosRelativeToPlanet", sunPosRelativeToPlanet);
                }
            }

            /// <summary>
            /// Update the scale and the lock
            /// </summary>
            void FixedUpdate()
            {
                transform.localScale = transform.parent.localScale;
                setRotation();
            }

            /// <summary>
            /// Update the scale and the lock
            /// </summary>
            void LateUpdate()
            {
                transform.localScale = transform.parent.localScale;
                setRotation();
            }

            /// <summary>
            /// Populate our transform's rotation quaternion
            /// </summary>
            private void setRotation()
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
                        float degreesPerSecond = -360f / rotationPeriod;
                        float parentRotation = (float) (referenceBody.initialRotation + 360 * Planetarium.GetUniversalTime() / referenceBody.rotationPeriod);
                        transform.localRotation =
                            Quaternion.Euler(0, parentRotation - longitudeOfAscendingNode, 0)
                            * rotation
                            * Quaternion.Euler(0, (float)Planetarium.GetUniversalTime() * degreesPerSecond, 0);
                    } else {
                        float parentRotation = (float) (referenceBody.initialRotation + 360 * Planetarium.GetUniversalTime() / referenceBody.rotationPeriod);
                        transform.localRotation =
                            Quaternion.Euler(0, parentRotation - longitudeOfAscendingNode, 0)
                            * rotation;
                    }
                }
            }

        }
    }
}
