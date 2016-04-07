/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
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
            public Quaternion rotation;
            public Texture2D texture;
            public Color color;
            public bool lockRotation;
            public bool unlit;
            public int steps = 128;

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
                // Prepare
                GameObject parent = transform.parent.gameObject;
                List<Vector3> vertices = new List<Vector3>();
                List<Vector2> Uvs = new List<Vector2>();
                List<int> Tris = new List<int>();
                List<Vector3> Normals = new List<Vector3>();

                // Mesh wrapping
                for (float i = 0f; i < 360f; i += (360f / steps))
                {
                    // Rotation
                    Vector3 eVert = Quaternion.Euler(0, i, 0) * Vector3.right;

                    // Inner Radius
                    vertices.Add(eVert * (innerRadius * (1f / parent.transform.localScale.x)));
                    Normals.Add(Vector3.left);
                    Uvs.Add(Vector2.one);

                    // Outer Radius
                    vertices.Add(eVert * (outerRadius * (1f / parent.transform.localScale.x)));
                    Normals.Add(Vector3.left);
                    Uvs.Add(Vector2.zero);
                }
                for (float i = 0f; i < 360f; i += (360f / steps))
                {
                    // Rotation
                    Vector3 eVert = Quaternion.Euler(0, i, 0) * Vector3.right;

                    // Inner Radius
                    vertices.Add(eVert * (innerRadius * (1f / parent.transform.localScale.x)));
                    Normals.Add(Vector3.left);
                    Uvs.Add(Vector2.one);

                    // Outer Radius
                    vertices.Add(eVert * (outerRadius * (1f / parent.transform.localScale.x)));
                    Normals.Add(Vector3.left);
                    Uvs.Add(Vector2.zero);
                }

                // Tri Wrapping
                int Wrapping = steps * 2;
                for (int i = 0; i < Wrapping; i += 2)
                {
                    Tris.Add((i) % Wrapping);
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
                    Tris.Add(Wrapping + (i) % Wrapping);

                    Tris.Add(Wrapping + (i + 2) % Wrapping);
                    Tris.Add(Wrapping + (i + 3) % Wrapping);
                    Tris.Add(Wrapping + (i + 1) % Wrapping);
                }

                // Update Rotation
                transform.localRotation = rotation;

                // Update Scale and Layer
                transform.localScale = parent.transform.localScale;
                transform.position = parent.transform.localPosition;
                gameObject.layer = parent.layer;

                // Create MeshFilter
                MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

                // Set mesh
                meshFilter.mesh = new Mesh();
                meshFilter.mesh.vertices = vertices.ToArray();
                meshFilter.mesh.triangles = Tris.ToArray();
                meshFilter.mesh.uv = Uvs.ToArray();
                meshFilter.mesh.RecalculateNormals();
                meshFilter.mesh.RecalculateBounds();
                meshFilter.mesh.Optimize();
                meshFilter.sharedMesh = meshFilter.mesh;

                // Set texture
                MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
                Renderer parentRenderer = parent.GetComponent<Renderer>();
                if (unlit)
                    meshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));
                else
                    meshRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
                meshRenderer.material.mainTexture = texture;
                meshRenderer.material.color = color;
                meshRenderer.material.renderQueue = parentRenderer.material.renderQueue;
                parentRenderer.material.renderQueue--;
            }

            /// <summary>
            /// Update the scale and the lock
            /// </summary>
            void Update()
            {
                transform.localScale = transform.parent.localScale;
                if (lockRotation) transform.rotation = rotation;
            }

            /// <summary>
            /// Update the scale and the lock
            /// </summary>
            void FixedUpdate()
            {
                transform.localScale = transform.parent.localScale;
                if (lockRotation) transform.rotation = rotation;
            }

            /// <summary>
            /// Update the scale and the lock
            /// </summary>
            void LateUpdate()
            {
                transform.localScale = transform.parent.localScale;
                if (lockRotation) transform.rotation = rotation;
            }
        }
    }
}