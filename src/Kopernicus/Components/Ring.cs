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
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Kopernicus.Components.ModularComponentSystem;
using Kopernicus.Components.Serialization;
using UnityEngine;

namespace Kopernicus.Components
{
    /// <summary>
    /// Class to render a ring around a planet
    /// </summary>
    public class Ring : SerializableMonoBehaviour, IComponentSystem<Ring>
    {
        /// <summary>
        /// Components that can be added to the Ring
        /// </summary>
        public List<IComponent<Ring>> Components
        {
            get { return components; }
            set { components = value; }
        }

        // Settings
        public Single innerRadius;
        public Single outerRadius;
        public FloatCurve innerMultCurve;
        public FloatCurve outerMultCurve;

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
        public Int32 tiles;

        /// <summary>
        /// For new shader, makes planet shadow softer (values larger than one) or less soft (smaller than one)
        /// softness still depends on distance from sun, distance from planet and radius of sun and planet
        /// </summary>
        public Single penumbraMultiplier = 10f;

        /// <summary>
        /// This texture's opaque pixels cast shadows on our inner surface
        /// </summary>
        public Texture2D innerShadeTexture;

        /// <summary>
        /// The inner shade texture repeats this many times over the inner surface
        /// </summary>
        public Int32 innerShadeTiles;

        /// <summary>
        /// Number of seconds the inner shade texture takes to complete one rotation
        /// </summary>
        public Single innerShadeRotationPeriod;

        /// <summary>
        /// Multiply the time by this to get the offset of the inner shade texture
        /// </summary>
        private Single _innerShadeOffsetRate;

        /// <summary>
        /// The body around which this ring is located.
        /// Used to get rotation data to set the LAN.
        /// </summary>
        public CelestialBody referenceBody;
        public MeshRenderer ringMr;
        
        [SerializeField]
        private List<IComponent<Ring>> components;

        private static readonly Int32 SunRadius = Shader.PropertyToID("sunRadius");
        private static readonly Int32 SunPosRelativeToPlanet = Shader.PropertyToID("sunPosRelativeToPlanet");
        private static readonly Int32 InnerShadeOffset = Shader.PropertyToID("innerShadeOffset");
        private static readonly Int32 InnerShadeTiles = Shader.PropertyToID("innerShadeTiles");
        private static readonly Int32 InnerShadeTexture = Shader.PropertyToID("_InnerShadeTexture");
        private static readonly Int32 PenumbraMultiplier = Shader.PropertyToID("penumbraMultiplier");
        private static readonly Int32 PlanetRadius = Shader.PropertyToID("planetRadius");
        private static readonly Int32 OuterRadius = Shader.PropertyToID("outerRadius");
        private static readonly Int32 InnerRadius = Shader.PropertyToID("innerRadius");
        private static readonly Int32 MainTex = Shader.PropertyToID("_MainTex");

        /// <summary>
        /// Create the module list
        /// </summary>
        private void Awake()
        {
            Components = new List<IComponent<Ring>>();
        }

        /// <summary>
        /// Create the Ring Mesh
        /// </summary>
        private void Start()
        {
            if (gameObject.GetComponent<MeshFilter>() == null)
            {
                BuildRing();
            }
        }

        /// <summary>
        /// Builds the Ring
        /// </summary>
        [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        public void BuildRing()
        {
            // Call the modules
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Apply(this);
            }

            // Create the ring mesh
            GameObject parent = transform.parent.gameObject;
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Int32> tris = new List<Int32>();

            // These are invariant, so avoid Singleing point division in tight loops
            Single degreeStep = 360f / steps;
            Vector3 scale = parent.transform.localScale;
            Single innerScale = innerRadius / scale.x;
            Single outerScale = outerRadius / scale.x;

            if (tiles > 0)
            {
                MakeTiledMesh(vertices, uvs, tris,
                    degreeStep, innerScale, outerScale,
                    thickness * Vector3.up / parent.transform.localScale.x);
            }
            else
            {
                MakeLinearMesh(vertices, uvs, tris,
                    degreeStep, innerScale, outerScale);
            }

            // Update Rotation
            transform.localRotation = rotation;

            // Update Scale and Layer
            Vector3 localScale = parent.transform.localScale;
            transform.localScale = localScale;
            transform.position = parent.transform.localPosition;
            gameObject.layer = parent.layer;

            // Create MeshFilter
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

            // Set mesh
            meshFilter.mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = tris.ToArray(),
                uv = uvs.ToArray()
            };
            meshFilter.mesh.RecalculateNormals();
            meshFilter.mesh.RecalculateBounds();
            meshFilter.sharedMesh = meshFilter.mesh;

            // Set texture
            ringMr = gameObject.AddComponent<MeshRenderer>();
            ringMr.sharedMaterial = new Material(GetShader());
            ringMr.sharedMaterial.SetTexture(MainTex, texture);

            ringMr.sharedMaterial.SetFloat(InnerRadius, innerRadius * localScale.x);
            ringMr.sharedMaterial.SetFloat(OuterRadius, outerRadius * localScale.x);

            if (useNewShader)
            {
                ringMr.sharedMaterial.SetFloat(PlanetRadius, planetRadius);
                ringMr.sharedMaterial.SetFloat(PenumbraMultiplier, penumbraMultiplier);

                if (innerShadeTexture != null)
                {
                    ringMr.sharedMaterial.SetTexture(InnerShadeTexture, innerShadeTexture);
                }

                if (innerShadeTiles > 0)
                {
                    ringMr.sharedMaterial.SetFloat(InnerShadeTiles, tiles / innerShadeTiles);
                }

                if (innerShadeRotationPeriod > 0 && rotationPeriod > 0)
                {
                    _innerShadeOffsetRate = innerShadeTiles * (
                                               1 / innerShadeRotationPeriod
                                               - 1 / rotationPeriod);
                }
            }

            Material sharedMaterial = ringMr.sharedMaterial;
            sharedMaterial.color = color;
            sharedMaterial.renderQueue = 3010;
            if (parent.GetChild("Atmosphere") != null)
            {
                parent.GetChild("Atmosphere").GetComponent<Renderer>().sharedMaterial.renderQueue = 3020;
            }

            // Call the modules
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].PostApply(this);
            }
        }

        /// <summary>
        /// The shaders used by the ring mesh
        /// </summary>
        private const String NEW_SHADER = "Kopernicus/Rings";
        private const String UNLIT_SHADER = "Unlit/Transparent";
        private const String DIFFUSE_SHADER = "Legacy Shaders/Transparent/Diffuse";

        /// <summary>
        /// Queries the shader the material should use
        /// </summary>
        private Shader GetShader()
        {
            return useNewShader ? ShaderLoader.GetShader(NEW_SHADER) : Shader.Find(unlit ? UNLIT_SHADER : DIFFUSE_SHADER);
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
        /// <param name="uvs">List of texture coordinates for the mesh</param>
        /// <param name="tris">List of triangles for the mesh</param>
        /// <param name="degreeStep">Width of each slice of the mesh in degrees</param>
        /// <param name="innerScale">Distance from center of parent to inner edge of ring</param>
        /// <param name="outerScale">Distance from center of parent to outer edge of ring</param>
        private void MakeLinearMesh(
            ICollection<Vector3> vertices,
            ICollection<Vector2> uvs,
            ICollection<Int32> tris,
            Single degreeStep,
            Single innerScale,
            Single outerScale)
        {
            // Mesh wrapping
            for (Single i = 0f; i < 360f; i += degreeStep)
            {
                // Rotation
                Vector3 eVert = Quaternion.Euler(0, i, 0) * Vector3.right;

                // Inner Radius
                vertices.Add(innerScale * innerMultCurve.Evaluate(i) * eVert);
                uvs.Add(Vector2.one);

                // Outer Radius
                vertices.Add(outerScale * outerMultCurve.Evaluate(i) * eVert);
                uvs.Add(Vector2.zero);
            }

            for (Single i = 0f; i < 360f; i += degreeStep)
            {
                // Rotation
                Vector3 eVert = Quaternion.Euler(0, i, 0) * Vector3.right;

                // Inner Radius
                vertices.Add(innerScale * innerMultCurve.Evaluate(i) * eVert);
                uvs.Add(Vector2.one);

                // Outer Radius
                vertices.Add(outerScale * outerMultCurve.Evaluate(i) * eVert);
                uvs.Add(Vector2.zero);
            }

            // Tri Wrapping
            Int32 wrapping = steps * 2;
            for (Int32 i = 0; i < wrapping; i += 2)
            {
                tris.Add(i % wrapping);
                tris.Add((i + 1) % wrapping);
                tris.Add((i + 2) % wrapping);

                tris.Add((i + 1) % wrapping);
                tris.Add((i + 3) % wrapping);
                tris.Add((i + 2) % wrapping);
            }

            for (Int32 i = 0; i < wrapping; i += 2)
            {
                tris.Add(wrapping + (i + 2) % wrapping);
                tris.Add(wrapping + (i + 1) % wrapping);
                tris.Add(wrapping + i % wrapping);

                tris.Add(wrapping + (i + 2) % wrapping);
                tris.Add(wrapping + (i + 3) % wrapping);
                tris.Add(wrapping + (i + 1) % wrapping);
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
        private static Vector2 TextureV(Int32 numTiles, Single degrees)
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
        /// <param name="uvs">List of texture coordinates for the mesh</param>
        /// <param name="tris">List of triangles for the mesh</param>
        /// <param name="degreeStep">Width of each slice of the mesh in degrees</param>
        /// <param name="innerScale">Distance from center of parent to inner edge of ring</param>
        /// <param name="outerScale">Distance from center of parent to outer edge of ring</param>
        /// <param name="thicknessOffset">A vector that can be added to add the thickness of the ring</param>
        private void MakeTiledMesh(
            ICollection<Vector3> vertices,
            ICollection<Vector2> uvs,
            ICollection<Int32> tris,
            Single degreeStep,
            Single innerScale,
            Single outerScale,
            Vector3 thicknessOffset)
        {
            const Single SIDE_TEX_W = 0.2f,
                INNER_TEX_W = 0.4f,
                OUTER_TEX_W = 0.4f;
            // Define coordinates for 3 textures:
            //   1. The "side" faces that point normal and antinormal
            //      (classic rings)
            Vector2 sideInnerU = Vector2.zero;
            Vector2 sideOuterU = sideInnerU + SIDE_TEX_W * Vector2.right;
            //   2. The "inner" face that points at the body
            Vector2 innerBottomU = sideOuterU;
            Vector2 innerTopU = innerBottomU + INNER_TEX_W * Vector2.right;
            //   3. The "outer" face that points away from the body
            Vector2 outerBottomU = innerTopU;
            Vector2 outerTopU = outerBottomU + OUTER_TEX_W * Vector2.right;

            Int32 wrapping = steps * 2;

            // Allow inner==outer for thin cylinders w/ only inner/outer faces
            if (innerRadius < outerRadius)
            {
                // Top faces
                for (Single i = 0f; i < 360f; i += degreeStep)
                {
                    // Rotation
                    Vector3 eVert = Quaternion.Euler(0, i, 0) * Vector3.right;
                    Vector2 texV = TextureV(tiles, i);

                    // Inner Radius
                    vertices.Add(innerScale * innerMultCurve.Evaluate(i) * eVert + thicknessOffset);
                    uvs.Add(sideInnerU + texV);

                    // Outer Radius
                    vertices.Add(outerScale * outerMultCurve.Evaluate(i) * eVert + thicknessOffset);
                    uvs.Add(sideOuterU + texV);
                }

                // Bottom faces
                for (Single i = 0f; i < 360f; i += degreeStep)
                {
                    // Rotation
                    Vector3 eVert = Quaternion.Euler(0, i, 0) * Vector3.right;
                    Vector2 texV = TextureV(tiles, i);

                    // Inner Radius
                    vertices.Add(innerScale * innerMultCurve.Evaluate(i) * eVert - thicknessOffset);
                    uvs.Add(sideInnerU + texV);

                    // Outer Radius
                    vertices.Add(outerScale * outerMultCurve.Evaluate(i) * eVert - thicknessOffset);
                    uvs.Add(sideOuterU + texV);
                }

                // Tri Wrapping
                for (Int32 i = 0; i < wrapping; i += 2)
                {
                    tris.Add(i);
                    tris.Add((i + 1) % wrapping);
                    tris.Add((i + 2) % wrapping);

                    tris.Add((i + 1) % wrapping);
                    tris.Add((i + 3) % wrapping);
                    tris.Add((i + 2) % wrapping);

                    tris.Add(wrapping + (i + 2) % wrapping);
                    tris.Add(wrapping + (i + 1) % wrapping);
                    tris.Add(wrapping + i % wrapping);

                    tris.Add(wrapping + (i + 2) % wrapping);
                    tris.Add(wrapping + (i + 3) % wrapping);
                    tris.Add(wrapping + (i + 1) % wrapping);
                }
            }

            // Inner and outer faces
            if (!(thickness > 0))
            {
                return;
            }
            
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
                Vector2 texV = TextureV(tiles, f);

                // Inner Radius
                vertices.Add(innerScale * innerMultCurve.Evaluate(i) * eVert + thicknessOffset);
                uvs.Add(innerTopU + texV);

                // Outer Radius
                vertices.Add(outerScale * outerMultCurve.Evaluate(i) * eVert + thicknessOffset);
                uvs.Add(outerTopU + texV);
            }

            // Mesh wrapping - bottom faces
            for (Int32 i = 0; i <= steps; ++i)
            {
                Single f = i * degreeStep;

                // Rotation
                Vector3 eVert = Quaternion.Euler(0, f, 0) * Vector3.right;
                Vector2 texV = TextureV(tiles, f);

                // Inner Radius
                vertices.Add(innerScale * innerMultCurve.Evaluate(i) * eVert - thicknessOffset);
                uvs.Add(innerBottomU + texV);

                // Outer Radius
                vertices.Add(outerScale * outerMultCurve.Evaluate(i) * eVert - thicknessOffset);
                uvs.Add(outerBottomU + texV);
            }

            // Tri Wrapping
            // No modulus this time because we want to use those
            // extra vertices instead of having the
            // texture coordinates loop back around.
            for (Int32 i = 0; i < wrapping; i += 2)
            {
                // Inner faces
                tris.Add(firstTop + i);
                tris.Add(firstTop + i + 2);
                tris.Add(firstTop + wrapping + 2 + i);

                tris.Add(firstTop + wrapping + 2 + i);
                tris.Add(firstTop + i + 2);
                tris.Add(firstTop + wrapping + 2 + i + 2);
            }

            // Outer faces should always draw after inner to
            // make the overlaps render correctly
            for (Int32 i = 0; i < wrapping; i += 2)
            {
                // Outer faces
                tris.Add(firstTop + i + 1);
                tris.Add(firstTop + wrapping + 2 + i + 1);
                tris.Add(firstTop + i + 3);

                tris.Add(firstTop + wrapping + 2 + i + 1);
                tris.Add(firstTop + wrapping + 2 + i + 3);
                tris.Add(firstTop + i + 3);
            }
        }

        /// <summary>
        /// Update the scale and the lock
        /// </summary>
        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        private void Update()
        {
            transform.localScale = transform.parent.localScale;
            SetRotation();

            if (useNewShader && ringMr.sharedMaterial != null
                             && KopernicusStar.Current != null && KopernicusStar.Current.sun.transform != null)
            {
                ringMr.sharedMaterial.SetFloat(SunRadius,
                    (Single) KopernicusStar.Current.sun.Radius);
                ringMr.sharedMaterial.SetVector(SunPosRelativeToPlanet,
                    (Vector3) (KopernicusStar.Current.sun.transform.position -
                               ScaledSpace.ScaledToLocalSpace(transform.position)));
                ringMr.sharedMaterial.SetFloat(InnerShadeOffset,
                    (Single) (Planetarium.GetUniversalTime() * _innerShadeOffsetRate));
            }

            // Call Modules
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Update(this);
            }
        }

        /// <summary>
        /// Update the scale and the lock
        /// </summary>
        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        private void FixedUpdate()
        {
            transform.localScale = transform.parent.localScale;
            SetRotation();

            // Call Modules
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Update(this);
            }
        }

        /// <summary>
        /// Update the scale and the lock
        /// </summary>
        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        private void LateUpdate()
        {
            transform.localScale = transform.parent.localScale;
            SetRotation();

            // Call Modules
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Update(this);
            }
        }

        /// <summary>
        /// Populate our transform's rotation quaternion
        /// </summary>
        private void SetRotation()
        {
            if (!lockRotation || referenceBody == null)
            {
                return;
            }

            // Setting transform.rotation does NOT give us a consistent
            // absolute orientation as you would expect from the documentation.
            // "World" coordinates seem to be set differently each time the
            // game is loaded. Instead, we use localRotation to orient the ring
            // relative to its parent body, subtract off the parent body's
            // rotation at the current moment in time, then add the LAN.
            // Note that eastward (prograde) rotation is negative in trigonometry.
            if (Math.Abs(rotationPeriod) > 0.01)
            {
                Single degreesPerSecond = -360f / rotationPeriod;
                Single parentRotation = (Single) (referenceBody.initialRotation +
                                                  360 * Planetarium.GetUniversalTime() /
                                                  referenceBody.rotationPeriod);
                transform.localRotation =
                    Quaternion.Euler(0, parentRotation - longitudeOfAscendingNode, 0)
                    * rotation
                    * Quaternion.Euler(0, (Single) Planetarium.GetUniversalTime() * degreesPerSecond, 0);
            }
            else
            {
                Single parentRotation = (Single) (referenceBody.initialRotation +
                                                  360 * Planetarium.GetUniversalTime() /
                                                  referenceBody.rotationPeriod);
                transform.localRotation =
                    Quaternion.Euler(0, parentRotation - longitudeOfAscendingNode, 0)
                    * rotation;
            }
        }
    }
}
