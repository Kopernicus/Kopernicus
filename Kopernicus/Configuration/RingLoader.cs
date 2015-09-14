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
 *
 * Code based on KittiopaTech, modified by Thomas P.
 */

using System.Collections.Generic;

using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class RingLoader : IParserEventSubscriber
        {
            // Set-up our custom ring
            public Ring ring;

            // Our Scaled Planet
            public GameObject ScaledPlanet { get; set; }

            // Inner Radius of our ring
            [ParserTarget("innerRadius", optional = true, allowMerge = false)]
            public NumericParser<double> innerRadius
            {
                set { ring.innerRadius = value.value; }
            }

            // Outer Radius of our ring
            [ParserTarget("outerRadius", optional = true, allowMerge = false)]
            public NumericParser<double> outerRadius
            {
                set { ring.outerRadius = value.value; }
            }

            // Axis angle of our ring
            [ParserTarget("angle", optional = true, allowMerge = false)]
            public NumericParser<float> angle
            {
                set { ring.angle = value.value; }
            }

            // Texture of our ring
            [ParserTarget("texture", optional = true, allowMerge = false)]
            public Texture2DParser texture
            {
                set { ring.texture = value.value; }
            }

            // Color of our ring
            [ParserTarget("color", optional = true, allowMerge = false)]
            public ColorParser color
            {
                set { ring.color = value.value; }
            }

            // Lock rotation of our ring?
            [ParserTarget("lockRotation", optional = true, allowMerge = false)]
            public NumericParser<bool> lockRotation
            {
                set { ring.lockRotation = value.value; }
            }

            // Unlit our ring?
            [ParserTarget("unlit", optional = true, allowMerge = false)]
            public NumericParser<bool> unlit
            {
                set { ring.unlit = value.value; }
            }

            [ParserTarget("steps", optional = true, allowMerge = false)]
            public NumericParser<int> steps
            {
                set { ring.steps = value.value; }
            }

            // Initialize the RingLoader
            public RingLoader()
            {
                ring = new Ring();
            }

            // Apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
            }

            // Post-Apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
            }

            // Rings
            public static void AddRing(GameObject ScaledPlanet, Ring ring)
            {
                Logger.Active.Log("Adding Ring to " + ScaledPlanet.name);
                Vector3 StartVec = new Vector3(1, 0, 0);
                var vertices = new List<Vector3>();
                var Uvs = new List<Vector2>();
                var Tris = new List<int>();
                var Normals = new List<Vector3>();

                for (float i = 0.0f; i < 360.0f; i += (360.0f / ring.steps))
                {
                    var eVert = Quaternion.Euler(0, i, 0) * StartVec;

                    //Inner Radius
                    vertices.Add(eVert * ((float)ring.innerRadius * (1f / ScaledPlanet.transform.localScale.x)));
                    Normals.Add(-Vector3.right);
                    Uvs.Add(new Vector2(0, 0));

                    //Outer Radius
                    vertices.Add(eVert * ((float)ring.outerRadius * (1f / ScaledPlanet.transform.localScale.x)));
                    Normals.Add(-Vector3.right);
                    Uvs.Add(new Vector2(1, 1));
                }

                for (float i = 0.0f; i < 360.0f; i += (360.0f / ring.steps))
                {
                    var eVert = Quaternion.Euler(0, i, 0) * StartVec;

                    //Inner Radius
                    vertices.Add(eVert * ((float)ring.innerRadius * (1f / ScaledPlanet.transform.localScale.x)));
                    Normals.Add(-Vector3.right);
                    Uvs.Add(new Vector2(0, 0));

                    //Outer Radius
                    vertices.Add(eVert * ((float)ring.outerRadius * (1f / ScaledPlanet.transform.localScale.x)));
                    Normals.Add(-Vector3.right);
                    Uvs.Add(new Vector2(1, 1));
                }

                //Tri Wrapping
                int Wrapping = (ring.steps * 2);
                for (int i = 0; i < (ring.steps * 2); i += 2)
                {
                    Tris.Add((i) % Wrapping);
                    Tris.Add((i + 1) % Wrapping);
                    Tris.Add((i + 2) % Wrapping);

                    Tris.Add((i + 1) % Wrapping);
                    Tris.Add((i + 3) % Wrapping);
                    Tris.Add((i + 2) % Wrapping);
                }

                for (int i = 0; i < (ring.steps * 2); i += 2)
                {
                    Tris.Add(Wrapping + (i + 2) % Wrapping);
                    Tris.Add(Wrapping + (i + 1) % Wrapping);
                    Tris.Add(Wrapping + (i) % Wrapping);

                    Tris.Add(Wrapping + (i + 2) % Wrapping);
                    Tris.Add(Wrapping + (i + 3) % Wrapping);
                    Tris.Add(Wrapping + (i + 1) % Wrapping);
                }

                //Create GameObject
                GameObject RingObject = new GameObject("PlanetaryRingObject");
                RingObject.transform.parent = ScaledPlanet.transform;
                RingObject.transform.position = ScaledPlanet.transform.localPosition;
                RingObject.transform.localRotation = Quaternion.Euler(ring.angle, 0, 0);

                RingObject.transform.localScale = ScaledPlanet.transform.localScale;
                RingObject.layer = ScaledPlanet.layer;

                //Create MeshFilter
                MeshFilter RingMesh = RingObject.AddComponent<MeshFilter>();

                //Set mesh
                RingMesh.mesh = new Mesh();
                RingMesh.mesh.vertices = vertices.ToArray();
                RingMesh.mesh.triangles = Tris.ToArray();
                RingMesh.mesh.uv = Uvs.ToArray();
                RingMesh.mesh.RecalculateNormals();
                RingMesh.mesh.RecalculateBounds();
                RingMesh.mesh.Optimize();
                RingMesh.sharedMesh = RingMesh.mesh;

                //Set texture
                //MeshRenderer PlanetRenderer = (MeshRenderer)ScaledPlanet.GetComponentsInChildren<MeshRenderer>()[0];
                MeshRenderer RingRender = RingObject.AddComponent<MeshRenderer>();
                RingRender.material = ScaledPlanet.renderer.material;

                if (ring.unlit)
                {
                    Material material = new Material(Shaders.Shaders.UnlitNew);
                    RingRender.material = material;
                }
                else
                {
                    Material material = new Material(Shaders.Shaders.DiffuseNew);
                    RingRender.material = material;
                }

                RingRender.material.mainTexture = ring.texture;
                RingRender.material.color = ring.color;

                RingRender.material.renderQueue = ScaledPlanet.renderer.material.renderQueue + 2;
                ScaledPlanet.AddComponent<EVEFixer>().targetQueue = ScaledPlanet.renderer.material.renderQueue + 1;

                RingObject.AddComponent<ReScaler>();

                if (ring.lockRotation)
                {
                    Quaternion m_rotAngleLock = RingObject.transform.localRotation;
                    AngleLocker m_ringAngleLock = (AngleLocker)RingObject.AddComponent<AngleLocker>();
                    m_ringAngleLock.RotationLock = m_rotAngleLock;
                }

                GameObject.DontDestroyOnLoad(RingObject);
            }
        }

        //=====================================//
        //          Helper classes!            //
        //=====================================//

        public class Ring
        {
            public double innerRadius { get; set; }
            public double outerRadius { get; set; }
            public float angle { get; set; }
            public Texture2D texture { get; set; }
            public Color color { get; set; }
            public bool lockRotation { get; set; }
            public bool unlit { get; set; }
            public int steps { get; set; }

            public Ring()
            {
                steps = 128;
            }
        }

        // Class to fix the renderQueue of EVE 7.4 clouds
        public class EVEFixer : MonoBehaviour
        {
            public int targetQueue;

            public void LateUpdate()
            {
                foreach (Transform cloud in transform)
                    if (cloud.name == "New Game Object" && cloud.gameObject.GetComponents<MeshRenderer>().Length == 1 && cloud.gameObject.GetComponents<MeshFilter>().Length == 1)
                        cloud.gameObject.GetComponent<MeshRenderer>().sharedMaterial.renderQueue = targetQueue;
            }
        }

        public class AngleLocker : MonoBehaviour
        {
            public Quaternion RotationLock;

            private void Update()
            {
                transform.rotation = RotationLock;
            }

            private void FixedUpdate()
            {
                transform.rotation = RotationLock;
            }

            private void LateUpdate()
            {
                transform.rotation = RotationLock;
            }
        }

        public class ReScaler : MonoBehaviour
        {
            private void Update()
            {
                transform.localScale = transform.parent.localScale;
            }

            private void FixedUpdate()
            {
                transform.localScale = transform.parent.localScale;
            }

            private void LateUpdate()
            {
                transform.localScale = transform.parent.localScale;
            }
        }
    }
}