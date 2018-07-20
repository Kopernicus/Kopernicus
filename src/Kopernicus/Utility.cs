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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Kopernicus.OnDemand;
using UnityEngine;
using Object = System.Object;

namespace Kopernicus
{
    public class Utility
    {
        /// <summary>
        /// Local Space Game Object
        /// </summary>
        public static GameObject LocalSpace
        {
            get { return GameObject.Find(PSystemManager.Instance.localSpaceName); }
        }

        // Static object representing the deactivator
        private static GameObject deactivator;

        /// <summary>
        /// Get an object which is deactivated, essentially, and children are prefabs
        /// </summary>
        public static Transform Deactivator
        {
            get
            {
                if (deactivator == null)
                {
                    deactivator = new GameObject("__deactivator");
                    deactivator.SetActive(false);
                    UnityEngine.Object.DontDestroyOnLoad(deactivator);
                }
                return deactivator.transform;
            }
        }

        /// <summary>
        /// Copy one objects fields to another object via reflection
        /// </summary>
        /// <param name="source">Object to copy fields from</param>
        /// <param name="destination">Object to copy fields to</param>
        /// <param name="log">Whether the function should log the actions it performs</param>
        public static void CopyObjectFields<T>(T source, T destination, Boolean log = true)
        {
            // Reflection based copy
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                // Only copy non static fields
                if (!field.IsStatic)
                {
                    if (log)
                    {
                        Logger.Active.Log("Copying \"" + field.Name + "\": " + (field.GetValue(destination) ?? "<NULL>") + " => " + (field.GetValue(source) ?? "<NULL>"));
                    }
                    field.SetValue(destination, field.GetValue(source));
                }
            }
        }

        /// <summary>
        /// Copy one objects properties to another object via reflection
        /// </summary>
        /// <param name="source">Object to copy fields from</param>
        /// <param name="destination">Object to copy fields to</param>
        /// <param name="log">Whether the function should log the actions it performs</param>
        public static void CopyObjectProperties<T>(T source, T destination, Boolean log = true)
        {
            // Reflection based copy
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                // Only copy editable
                if (property.CanRead && property.CanWrite)
                {
                    if (property.GetGetMethod().IsStatic)
                        continue;

                    System.Object sourceValue = property.GetValue(source, null);
                    System.Object destValue = property.GetValue(destination, null);

                    // Check if both values are equal
                    if (sourceValue == destValue)
                        continue;

                    // Log the fields
                    if (log)
                    {
                        Logger.Active.Log("Copying \"" + property.Name + "\": " + (destValue ?? "<NULL>") + " => " + (sourceValue ?? "<NULL>"));
                    }
                    property.SetValue(destination, sourceValue, null);
                }
            }
        }

        /// <summary>
        /// Recursively searches for a named transform in the Transform heirarchy.  The requirement of
        /// such a function is sad.This should really be in the Unity3D API.Transform.Find() only
        /// searches in the immediate children.
        /// </summary>
        /// <param name="transform">Transform in which is search for named child</param>
        /// <param name="name">Name of child to find</param>
        /// <returns>Desired transform or null if it could not be found</returns>
        public static Transform FindInChildren(Transform transform, String name)
        {
            // Is this null?
            if (transform == null)
            {
                return null;
            }

            // Are the names equivalent
            if (transform.name == name)
            {
                return transform;
            }

            // If we did not find a transform, search through the children
            foreach (Transform child in transform)
            {
                // Recurse into the child
                Transform t = FindInChildren(child, name);
                if (t != null)
                {
                    return t;
                }
            }

            // Return the transform (will be null if it was not found)
            return null;
        }

        // Dump an object by reflection
        public static void DumpObjectFields(Object o, String title = "---------")
        {
            // Dump the raw PQS of Dres (by reflection)
            Logger.Active.Log("---------" + title + "------------");
            foreach (FieldInfo field in o.GetType().GetFields())
            {
                if (!field.IsStatic)
                {
                    Logger.Active.Log(field.Name + " = " + field.GetValue(o));
                }
            }
            Logger.Active.Log("--------------------------------------");
        }

        public static void DumpObjectProperties(Object o, String title = "---------")
        {
            // Iterate through all of the properties
            Logger.Active.Log("--------- " + title + " ------------");
            foreach (PropertyInfo property in o.GetType().GetProperties())
            {
                if (property.CanRead)
                    Logger.Active.Log(property.Name + " = " + property.GetValue(o, null));
            }
            Logger.Active.Log("--------------------------------------");
        }

        /// <summary>
        /// Recursively searches for a named PSystemBody
        /// </summary>
        /// <param name="body">Parent body to begin search in</param>
        /// <param name="name">Name of body to find</param>
        /// <returns>Desired body or null if not found</returns>
        public static PSystemBody FindBody(PSystemBody body, String name)
        {
            // Is this the body wer are looking for?
            if (body.celestialBody.bodyName == name)
                return body;

            // Otherwise search children
            foreach (PSystemBody child in body.children)
            {
                PSystemBody b = FindBody(child, name);
                if (b != null)
                    return b;
            }

            // Return null because we didn't find shit
            return null;
        }

        // Copy of above, but finds homeworld
        public static PSystemBody FindHomeBody(PSystemBody body)
        {
            // Is this the body wer are looking for?
            if (body.celestialBody.isHomeWorld)
                return body;

            // Otherwise search children
            foreach (PSystemBody child in body.children)
            {
                PSystemBody b = FindHomeBody(child);
                if (b != null)
                    return b;
            }

            // Return null because we didn't find shit
            return null;
        }

        // Print out a tree containing all the objects in the game
        public static void PerformObjectDump()
        {
            Logger.Active.Log("--------- Object Dump -----------");
            foreach (GameObject b in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                // Essentially, we iterate through all game objects currently alive and search for 
                // the ones without a parent.  Extrememly inefficient and terrible, but its just for
                // exploratory purposes
                if (b.transform.parent == null)
                {
                    // Print out the tree of child objects
                    GameObjectWalk(b, "");
                }
            }
            Logger.Active.Log("---------------------------------");
        }

        public static void PrintTransform(Transform t, String title = "")
        {
            Logger.Active.Log("------" + title + "------");
            Logger.Active.Log("Position: " + t.localPosition);
            Logger.Active.Log("Rotation: " + t.localRotation);
            Logger.Active.Log("Scale: " + t.localScale);
            Logger.Active.Log("------------------");
        }

        // Print out the tree of components 
        public static void GameObjectWalk(GameObject o, String prefix = "")
        {
            // If null, don't do anything
            if (o == null)
                return;

            // Print this object
            Logger.Active.Log(prefix + o);
            Logger.Active.Log(prefix + " >>> Components <<< ");
            foreach (Component c in o.GetComponents(typeof(Component)))
            {
                Logger.Active.Log(prefix + " " + c);
            }
            Logger.Active.Log(prefix + " >>> ---------- <<< ");

            // Game objects are related to each other via transforms in Unity3D.
            foreach (Transform b in o.transform)
            {
                if (b != null)
                    GameObjectWalk(b.gameObject, "    " + prefix);
            }
        }

        // Print out the celestial bodies
        public static void PSystemBodyWalk(PSystemBody b, String prefix = "")
        {
            Logger.Active.Log(prefix + b.celestialBody.bodyName + ":" + b.flightGlobalsIndex);
            foreach (PSystemBody c in b.children)
            {
                PSystemBodyWalk(c, prefix + "    ");
            }
        }

        // slightly different:
        public static void DumpUpwards(Transform t, String prefix, Boolean useKLog = true)
        {
            String str = prefix + "Transform " + t.name;
            if (useKLog)
                Logger.Default.Log(str);
            else
                Debug.Log(str);

            foreach (Component c in t.GetComponents<Component>())
            {
                str = prefix + " has component " + c.name + " of type " + c.GetType().FullName;
                if (useKLog)
                    Logger.Default.Log(str);
                else
                    Debug.Log(str);
            }
            if (t.parent != null)
                DumpUpwards(t.parent, prefix + "  ");

        }
        public static void DumpDownwards(Transform t, String prefix, Boolean useKLog = true)
        {
            String str = prefix + "Transform " + t.name;
            if (useKLog)
                Logger.Default.Log(str);
            else
                Debug.Log("[Kopernicus] " + str);

            foreach (Component c in t.GetComponents<Component>())
            {
                str = prefix + " has component " + c.name + " of type " + c.GetType().FullName;
                if (useKLog)
                    Logger.Default.Log(str);
                else
                    Debug.Log("[Kopernicus] " + str);
            }
            if (t.childCount > 0)
                for (Int32 i = 0; i < t.childCount; ++i)
                    DumpDownwards(t.GetChild(i), prefix + "  ");

        }

        public static void UpdateScaledMesh(GameObject scaledVersion, PQS pqs, CelestialBody body, String path, String cacheFile, Boolean exportBin, Boolean useSpherical)
        {
            const Double rJool = 6000000.0;
            const Single rScaled = 1000.0f;

            // Compute scale between Jool and this body
            Single scale = (Single)(body.Radius / rJool);
            scaledVersion.transform.localScale = new Vector3(scale, scale, scale);

            // Attempt to load a cached version of the scale space
            String cacheDirectory = KSPUtil.ApplicationRootPath + "GameData/" + path;
            if (!String.IsNullOrEmpty(cacheFile))
            {
                cacheFile = KSPUtil.ApplicationRootPath + "GameData/" + cacheFile;
                cacheDirectory = Path.GetDirectoryName(cacheFile);

                Logger.Active.Log($"{body.name} is using custom cache file '{cacheFile}'");
            }
            else
            {
                cacheFile = cacheDirectory + "/" + body.name + ".bin";
            }
            Directory.CreateDirectory(cacheDirectory);

            if (File.Exists(cacheFile) && exportBin)
            {
                Logger.Active.Log("Body.PostApply(ConfigNode): Loading cached scaled space mesh: " + body.name);
                Mesh scaledMesh = DeserializeMesh(cacheFile);
                RecalculateTangents(scaledMesh);
                scaledVersion.GetComponent<MeshFilter>().sharedMesh = scaledMesh;
            }

            // Otherwise we have to generate the mesh
            else
            {
                Logger.Active.Log("Body.PostApply(ConfigNode): Generating scaled space mesh: " + body.name);
                Mesh scaledMesh = ComputeScaledSpaceMesh(body, useSpherical ? null : pqs);
                RecalculateTangents(scaledMesh);
                scaledVersion.GetComponent<MeshFilter>().sharedMesh = scaledMesh;
                if (exportBin)
                    SerializeMesh(scaledMesh, cacheFile);
            }

            // Apply mesh to the body
            SphereCollider collider = scaledVersion.GetComponent<SphereCollider>();
            if (collider != null) collider.radius = rScaled;
            if (pqs != null && scaledVersion.gameObject != null && scaledVersion.gameObject.transform != null)
            {
                scaledVersion.gameObject.transform.localScale = Vector3.one * (Single)(pqs.radius / rJool);
            }
        }

        // Generate the scaled space mesh using PQS (all results use scale of 1)
        public static Mesh ComputeScaledSpaceMesh(CelestialBody body, PQS pqs)
        {
            // We need to get the body for Jool (to steal it's mesh)
            const Double rScaledJool = 1000.0f;
            Double rMetersToScaledUnits = (Single)(rScaledJool / body.Radius);

            // Generate a duplicate of the Jool mesh
            Mesh mesh = DuplicateMesh(Templates.ReferenceGeosphere);

            Logger.Active.Log(body);
            Logger.Active.Log(pqs);
            Logger.Active.Log(body.pqsController);

            // If this body has a PQS, we can create a more detailed object
            if (pqs != null)
            {
                // first we enable all maps
                OnDemandStorage.EnableBody(body.bodyName);

                // The game object the PQS is attached to
                GameObject pqsVersionGameObject = null;
                if (Injector.IsInPrefab)
                {
                    // In order to generate the scaled space we have to enable the mods.  Since this is
                    // a prefab they don't get disabled as kill game performance.  To resolve this we 
                    // clone the PQS, use it, and then delete it when done
                    pqsVersionGameObject = UnityEngine.Object.Instantiate(pqs.gameObject);
                }
                else
                {
                    // At runtime we simply use the PQS that is active
                    pqsVersionGameObject = pqs.gameObject;
                }
                PQS pqsVersion = pqsVersionGameObject.GetComponent<PQS>();

                // Load the PQS of the ocean
                PQS pqsOcean = pqs.ChildSpheres?.FirstOrDefault();

                // Deactivate blacklisted Mods
                Type[] blacklist = { typeof(PQSMod_OnDemandHandler) };
                foreach (PQSMod mod in pqsVersion.GetComponentsInChildren<PQSMod>(true)
                    .Where(m => m.enabled && blacklist.Contains(m.GetType())))
                {
                    mod.modEnabled = false;
                }

                // Find the PQS mods and enable the PQS-sphere
                PQSMod[] mods = pqsVersion.GetComponentsInChildren<PQSMod>(true)
                    .Where(m => m.modEnabled && m.transform.parent == pqsVersion.transform).OrderBy(m => m.order).ToArray();
                foreach (PQSMod flatten in mods.Where(m => m is PQSMod_FlattenArea))
                {
                    flatten.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                        .First(f => f.FieldType == typeof(Boolean)).SetValue(flatten, true);
                }
                Logger.Active.Log(mods.Length);

                // Do the same for the ocean
                PQSMod[] oceanMods = new PQSMod[0];
                if (pqsOcean != null)
                {
                    oceanMods = pqsOcean.GetComponentsInChildren<PQSMod>(true)
                        .Where(m => m.modEnabled && m.transform.parent == pqsOcean.transform).OrderBy(m => m.order).ToArray();
                    foreach (PQSMod flatten in oceanMods.Where(m => m is PQSMod_FlattenArea))
                    {
                        flatten.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                            .First(f => f.FieldType == typeof(Boolean)).SetValue(flatten, true);
                    }

                    pqsOcean.StartUpSphere();
                    pqsOcean.isBuildingMaps = true;
                }

                pqsVersion.StartUpSphere();
                pqsVersion.isBuildingMaps = true;

                // If we were able to find PQS mods
                if (mods.Any())
                {
                    // Generate the PQS modifications
                    Vector3[] vertices = mesh.vertices;
                    for (Int32 i = 0; i < mesh.vertexCount; i++)
                    {
                        // Get the UV coordinate of this vertex
                        Vector2 uv = mesh.uv[i];

                        // Since this is a geosphere, normalizing the vertex gives the direction from center center
                        Vector3 direction = vertices[i];
                        direction.Normalize();

                        // Build the vertex data object for the PQS mods
                        PQS.VertexBuildData vertex = new PQS.VertexBuildData();
                        vertex.directionFromCenter = direction;
                        vertex.vertHeight = body.Radius;
                        vertex.u = uv.x;
                        vertex.v = uv.y;

                        // Build from the PQS
                        foreach (PQSMod mod in mods)
                            mod.OnVertexBuildHeight(vertex);

                        // Check for sea level
                        if (pqsOcean != null)
                        {
                            // Build the vertex data object for the ocean
                            PQS.VertexBuildData vertexOcean = new PQS.VertexBuildData();
                            vertexOcean.directionFromCenter = direction;
                            vertexOcean.vertHeight = body.Radius;
                            vertexOcean.u = uv.x;
                            vertexOcean.v = uv.y;

                            // Build from the PQS
                            foreach (PQSMod mod in oceanMods)
                                mod.OnVertexBuildHeight(vertexOcean);

                            vertex.vertHeight = Math.Max(vertex.vertHeight, vertexOcean.vertHeight);
                        }

                        // Adjust the displacement
                        vertices[i] = direction * (Single)(vertex.vertHeight * rMetersToScaledUnits);
                    }
                    mesh.vertices = vertices;
                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                }

                // Cleanup
                if (pqsOcean != null)
                {
                    pqsOcean.isBuildingMaps = false;
                    pqsOcean.DeactivateSphere();
                }
                pqsVersion.isBuildingMaps = false;
                pqsVersion.DeactivateSphere();

                // If we are working with a copied PQS, clean it up
                if (Injector.IsInPrefab)
                {
                    UnityEngine.Object.Destroy(pqsVersionGameObject);
                }
                OnDemandStorage.DisableBody(body.bodyName);
            }

            // Return the generated scaled space mesh
            return mesh;
        }

        public static void CopyMesh(Mesh source, Mesh dest)
        {
            Vector3[] verts = new Vector3[source.vertexCount];
            source.vertices.CopyTo(verts, 0);
            dest.vertices = verts;

            Int32[] tris = new Int32[source.triangles.Length];
            source.triangles.CopyTo(tris, 0);
            dest.triangles = tris;

            Vector2[] uvs = new Vector2[source.uv.Length];
            source.uv.CopyTo(uvs, 0);
            dest.uv = uvs;

            Vector2[] uv2s = new Vector2[source.uv2.Length];
            source.uv2.CopyTo(uv2s, 0);
            dest.uv2 = uv2s;

            Vector3[] normals = new Vector3[source.normals.Length];
            source.normals.CopyTo(normals, 0);
            dest.normals = normals;

            Vector4[] tangents = new Vector4[source.tangents.Length];
            source.tangents.CopyTo(tangents, 0);
            dest.tangents = tangents;

            Color[] colors = new Color[source.colors.Length];
            source.colors.CopyTo(colors, 0);
            dest.colors = colors;

            Color32[] colors32 = new Color32[source.colors32.Length];
            source.colors32.CopyTo(colors32, 0);
            dest.colors32 = colors32;
        }

        public static Mesh DuplicateMesh(Mesh source)
        {
            // Create new mesh object
            Mesh dest = new Mesh();

            Vector3[] verts = new Vector3[source.vertexCount];
            source.vertices.CopyTo(verts, 0);
            dest.vertices = verts;

            Int32[] tris = new Int32[source.triangles.Length];
            source.triangles.CopyTo(tris, 0);
            dest.triangles = tris;

            Vector2[] uvs = new Vector2[source.uv.Length];
            source.uv.CopyTo(uvs, 0);
            dest.uv = uvs;

            Vector2[] uv2s = new Vector2[source.uv2.Length];
            source.uv2.CopyTo(uv2s, 0);
            dest.uv2 = uv2s;

            Vector3[] normals = new Vector3[source.normals.Length];
            source.normals.CopyTo(normals, 0);
            dest.normals = normals;

            Vector4[] tangents = new Vector4[source.tangents.Length];
            source.tangents.CopyTo(tangents, 0);
            dest.tangents = tangents;

            Color[] colors = new Color[source.colors.Length];
            source.colors.CopyTo(colors, 0);
            dest.colors = colors;

            Color32[] colors32 = new Color32[source.colors32.Length];
            source.colors32.CopyTo(colors32, 0);
            dest.colors32 = colors32;

            return dest;
        }

        // Taken from Nathankell's RSS Utils.cs; uniformly scaled vertices
        public static void ScaleVerts(Mesh mesh, Single scaleFactor)
        {
            Vector3[] vertices = new Vector3[mesh.vertexCount];
            for (Int32 i = 0; i < mesh.vertexCount; i++)
            {
                Vector3 v = mesh.vertices[i];
                v *= scaleFactor;
                vertices[i] = v;
            }
            mesh.vertices = vertices;
        }

        public static void RecalculateTangents(Mesh theMesh)
        {
            Int32 vertexCount = theMesh.vertexCount;
            Vector3[] vertices = theMesh.vertices;
            Vector3[] normals = theMesh.normals;
            Vector2[] texcoords = theMesh.uv;
            Int32[] triangles = theMesh.triangles;
            Int32 triangleCount = triangles.Length / 3;

            Vector4[] tangents = new Vector4[vertexCount];
            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];

            Int32 tri = 0;

            for (Int32 i = 0; i < triangleCount; i++)
            {
                Int32 i1 = triangles[tri];
                Int32 i2 = triangles[tri + 1];
                Int32 i3 = triangles[tri + 2];

                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];

                Vector2 w1 = texcoords[i1];
                Vector2 w2 = texcoords[i2];
                Vector2 w3 = texcoords[i3];

                Single x1 = v2.x - v1.x;
                Single x2 = v3.x - v1.x;
                Single y1 = v2.y - v1.y;
                Single y2 = v3.y - v1.y;
                Single z1 = v2.z - v1.z;
                Single z2 = v3.z - v1.z;

                Single s1 = w2.x - w1.x;
                Single s2 = w3.x - w1.x;
                Single t1 = w2.y - w1.y;
                Single t2 = w3.y - w1.y;

                Single r = 1.0f / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;

                tri += 3;
            }
            for (Int32 i = 0; i < vertexCount; i++)
            {
                Vector3 n = normals[i];
                Vector3 t = tan1[i];

                // Gram-Schmidt orthogonalize
                Vector3.OrthoNormalize(ref n, ref t);

                tangents[i].x = t.x;
                tangents[i].y = t.y;
                tangents[i].z = t.z;

                // Calculate handedness
                tangents[i].w = Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f ? -1.0f : 1.0f;
            }
            theMesh.tangents = tangents;
        }

        // Serialize a mesh to disk
        public static void SerializeMesh(Mesh mesh, String path)
        {
            // Open an output filestream
            FileStream outputStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(outputStream);

            // Write the vertex count of the mesh
            writer.Write(mesh.vertices.Length);
            foreach (Vector3 vertex in mesh.vertices)
            {
                writer.Write(vertex.x);
                writer.Write(vertex.y);
                writer.Write(vertex.z);
            }
            writer.Write(mesh.uv.Length);
            foreach (Vector2 uv in mesh.uv)
            {
                writer.Write(uv.x);
                writer.Write(uv.y);
            }
            writer.Write(mesh.triangles.Length);
            foreach (Int32 triangle in mesh.triangles)
            {
                writer.Write(triangle);
            }

            // Finish writing
            writer.Close();
            outputStream.Close();
        }

        // Deserialize a mesh from disk
        public static Mesh DeserializeMesh(String path)
        {
            FileStream inputStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(inputStream);

            // Get the vertices
            Int32 count = reader.ReadInt32();
            Vector3[] vertices = new Vector3[count];
            for (Int32 i = 0; i < count; i++)
            {
                Vector3 vertex;
                vertex.x = reader.ReadSingle();
                vertex.y = reader.ReadSingle();
                vertex.z = reader.ReadSingle();
                vertices[i] = vertex;
            }

            // Get the uvs
            Int32 uv_count = reader.ReadInt32();
            Vector2[] uvs = new Vector2[uv_count];
            for (Int32 i = 0; i < uv_count; i++)
            {
                Vector2 uv;
                uv.x = reader.ReadSingle();
                uv.y = reader.ReadSingle();
                uvs[i] = uv;
            }

            // Get the triangles
            Int32 tris_count = reader.ReadInt32();
            Int32[] triangles = new Int32[tris_count];
            for (Int32 i = 0; i < tris_count; i++)
                triangles[i] = reader.ReadInt32();

            // Close
            reader.Close();
            inputStream.Close();

            // Create the mesh
            Mesh m = new Mesh();
            m.vertices = vertices;
            m.triangles = triangles;
            m.uv = uvs;
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        // Credit goes to Sigma88.
        public static Texture2D BumpToNormalMap(Texture2D source, PQS pqs, Single strength)
        {
            double dS = pqs.radius * 2 * Math.PI / source.width;

            if (!(strength > 0)) strength = 1;

            Texture2D result = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);
            for (Int32 by = 0; by < result.height; by++)
            {
                for (Int32 bx = 0; bx < result.width; bx++)
                {
                    if (by == 0 || by == result.height - 1 || source.GetPixel(bx, by).r == 0)
                    {
                        result.SetPixel(bx, by, new Color(0.5f, 0.5f, 0.5f, 0.5f));
                    }
                    else
                    {
                        int xN = (bx + result.width - 1) % result.width;
                        int xP = (bx + result.width + 1) % result.width;

                        int yN = by - 1;
                        int yP = by + 1;

                        double dX = (source.GetPixel(xP, by).r - source.GetPixel(xN, by).r) * pqs.radiusDelta;
                        double dY = (source.GetPixel(bx, yP).r - source.GetPixel(bx, yN).r) * pqs.radiusDelta;

                        double slopeX = (1 + dX / Math.Pow(dX * dX + dS * dS, 0.5) * strength) / 2;
                        double slopeY = (1 - dY / Math.Pow(dY * dY + dS * dS, 0.5) * strength) / 2;

                        result.SetPixel(bx, by, new Color((float)slopeY, (float)slopeY, (float)slopeY, (float)slopeX));
                    }
                }
            }
            result.Apply();
            return result;
        }

        // Convert latitude-longitude-altitude with body radius to a vector.
        public static Vector3 LLAtoECEF(Double lat, Double lon, Double alt, Double radius)
        {
            const Double degreesToRadians = Math.PI / 180.0;
            lat = (lat - 90) * degreesToRadians;
            lon *= degreesToRadians;
            Double x, y, z;
            Double n = radius; // for now, it's still a sphere, so just the radius
            x = (n + alt) * -1.0 * Math.Sin(lat) * Math.Cos(lon);
            y = (n + alt) * Math.Cos(lat); // for now, it's still a sphere, so no eccentricity
            z = (n + alt) * -1.0 * Math.Sin(lat) * Math.Sin(lon);
            return new Vector3((Single)x, (Single)y, (Single)z);
        }

        public static Boolean TextureExists(String path)
        {
            path = KSPUtil.ApplicationRootPath + "GameData/" + path;
            return File.Exists(path);
        }

        public static Texture2D LoadTexture(String path, Boolean compress, Boolean upload, Boolean unreadable)
        {
            return OnDemandStorage.LoadTexture(path, compress, upload, unreadable);
        }

        public static T FindMapSO<T>(String url) where T : MapSO
        {
            T retVal = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(m => m.name == url);
            if (retVal != null)
            {
                return retVal;
            }
            Boolean modFound = false;
            String trim = url.Replace("BUILTIN/", "");
            String mBody = Regex.Replace(trim, @"/.*", "");
            trim = Regex.Replace(trim, mBody + "/", "");
            String mTypeName = Regex.Replace(trim, @"/.*", "");
            String mName = Regex.Replace(trim, mTypeName + "/", "");
            PSystemBody body = FindBody(PSystemManager.Instance.systemPrefab.rootBody, mBody);
            if (body != null && body.pqsVersion != null)
            {
                Type mType = null;
                try
                {
                    mType = Type.GetType(mTypeName + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
                }
                catch (Exception e)
                {
                    Logger.Active.Log("MapSO grabber: Tried to grab " + url + " but type not found. VertexHeight type for reference = " + typeof(PQSMod_VertexHeightMap).FullName + ". Exception: " + e);
                }
                if (mType != null)
                {
                    PQSMod[] mods = body.pqsVersion.GetComponentsInChildren<PQSMod>(true).Where(m => m.GetType() == mType).ToArray();
                    foreach (PQSMod m in mods.Where(m => m.name == mName))
                    {
                        modFound = true;
                        foreach (FieldInfo fi in m.GetType().GetFields().Where(fi => fi.FieldType == typeof(MapSO)))
                        {
                            retVal = fi.GetValue(m) as T;
                            break;
                        }
                    }
                }
            }
            else
                Logger.Active.Log("MapSO grabber: Tried to grab " + url + " but body not found.");

            if (retVal == null)
            {
                if (modFound)
                    Logger.Active.Log("MapSO grabber: Tried to grab " + url + " but mods of correct name and type lack MapSO.");
                else
                    Logger.Active.Log("MapSO grabber: Tried to grab " + url + " but could not find PQSMod of that type of the given name");
            }
            if (retVal != null)
                retVal.name = url;
            return retVal;
        }

        /// <summary>
        /// Will remove all mods of given types (or all, if types null)
        /// </summary>
        /// <param name="types">If null, will remove all mods except blacklisted mods</param>
        /// <param name="p">PQS to remove from</param>
        /// <param name="blacklist">list of mod types to not remove (optional)</param>
        public static void RemoveModsOfType(List<Type> types, PQS p, List<Type> blacklist = null)
        {
            Logger.Active.Log("Removing mods from pqs " + p.name);
            List<PQSMod> cpMods = p.GetComponentsInChildren<PQSMod>(true).ToList();
            Boolean addTypes = types == null;
            if (addTypes)
                types = new List<Type>();
            if (blacklist == null)
            {
                Logger.Active.Log("Creating blacklist");
                blacklist = new List<Type>();
                if (!types.Contains(typeof(PQSMod_CelestialBodyTransform)))
                    blacklist.Add(typeof(PQSMod_CelestialBodyTransform));
                if (!types.Contains(typeof(PQSMod_MaterialSetDirection)))
                    blacklist.Add(typeof(PQSMod_MaterialSetDirection));
                if (!types.Contains(typeof(PQSMod_UVPlanetRelativePosition)))
                    blacklist.Add(typeof(PQSMod_UVPlanetRelativePosition));
                if (!types.Contains(typeof(PQSMod_QuadMeshColliders)))
                    blacklist.Add(typeof(PQSMod_QuadMeshColliders));
                Logger.Active.Log("Blacklist count = " + blacklist.Count);
            }

            if (addTypes)
            {
                Logger.Active.Log("Adding all found PQSMods in pqs " + p.name);
                foreach (PQSMod m in cpMods)
                {
                    Type mType = m.GetType();
                    if (!types.Contains(mType) && !blacklist.Contains(mType))
                    {
                        Logger.Active.Log("Adding to removelist: " + mType);
                        types.Add(mType);
                    }
                }
            }
            List<GameObject> toCheck = new List<GameObject>();
            foreach (Type mType in types)
            {
                List<PQSMod> mods = cpMods.Where(m => m.GetType() == mType).ToList();
                foreach (PQSMod delMod in mods)
                {
                    if (delMod != null)
                    {
                        Logger.Active.Log("Removed mod " + mType.ToString());
                        if (!toCheck.Contains(delMod.gameObject))
                            toCheck.Add(delMod.gameObject);
                        delMod.sphere = null;
                        if (delMod is PQSCity)
                        {
                            PQSCity city = delMod as PQSCity;
                            if (city.lod != null)
                            {
                                foreach (PQSCity.LODRange range in city.lod)
                                {
                                    if (range.objects != null)
                                    {
                                        foreach (GameObject o in range.objects)
                                            UnityEngine.Object.DestroyImmediate(o);
                                    }
                                    if (range.renderers != null)
                                    {
                                        foreach (GameObject o in range.renderers)
                                            UnityEngine.Object.DestroyImmediate(o);
                                    }
                                }
                            }
                        }
                        if (delMod is PQSCity2)
                        {
                            PQSCity2 city = delMod as PQSCity2;
                            if (city.objects != null)
                            {
                                foreach (PQSCity2.LodObject range in city.objects)
                                {
                                    if (range.objects != null)
                                    {
                                        foreach (GameObject o in range.objects)
                                            UnityEngine.Object.DestroyImmediate(o);
                                    }
                                }
                            }
                        }
                        cpMods.Remove(delMod);

                        // If no mod is left, delete the game object too
                        GameObject gameObject = delMod.gameObject;
                        UnityEngine.Object.DestroyImmediate(delMod);
                        PQSMod[] allRemainingMods = gameObject.GetComponentsInChildren<PQSMod>(true);
                        if (allRemainingMods.Length == 0)
                        {
                            UnityEngine.Object.DestroyImmediate(gameObject);
                        }
                    }
                }
            }
            // RemoveEmptyGO(toCheck);
        }

        public static void RemoveEmptyGO(List<GameObject> toCheck)
        {
            Int32 oCount = toCheck.Count;
            Int32 nCount = oCount;
            List<GameObject> toDestroy = new List<GameObject>();
            do
            {
                oCount = nCount;
                foreach (GameObject go in toCheck)
                {
                    if (go.transform.childCount == 0)
                    {
                        Component[] comps = go.GetComponents<Component>();
                        if (comps.Length == 0 || comps.Length == 1 && comps[0].GetType() == typeof(Transform))
                            toDestroy.Add(go);
                    }
                }
                foreach (GameObject go in toDestroy)
                {
                    toCheck.Remove(go);
                    GameObject.DestroyImmediate(go);
                }
                toDestroy.Clear();
                nCount = toCheck.Count;
            } while (nCount != oCount && nCount > 0);
        }

        public static void CBTCheck(PSystemBody body)
        {
            if (body.pqsVersion != null)
            {
                if (body.pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform>().Length > 0)
                    Logger.Default.Log("Body " + body.name + " has CBT.");
                else
                {
                    PQSMod_CelestialBodyTransform cbt = body.pqsVersion.GetComponentsInChildren(typeof(PQSMod_CelestialBodyTransform), true).FirstOrDefault() as PQSMod_CelestialBodyTransform;
                    if (cbt == null)
                    {
                        Logger.Default.Log("Body " + body.name + " *** LACKS CBT ***");
                        DumpDownwards(body.pqsVersion.transform, "*");
                    }
                    else
                    {
                        cbt.enabled = true;
                        cbt.modEnabled = true;
                        cbt.sphere = body.pqsVersion;
                        Logger.Default.Log("Body " + body.name + " lacks active CBT, activated.");
                    }
                }
            }
            if (body.children != null)
                foreach (PSystemBody b in body.children)
                    CBTCheck(b);
        }

        // Converts an unreadable texture into a readable one
        public static Texture2D CreateReadable(Texture2D original)
        {
            // Checks
            if (original == null) return null;
            if (original.width == 0 || original.height == 0) return null;

            // Create the new texture
            Texture2D finalTexture = new Texture2D(original.width, original.height);

            // isn't read or writeable ... we'll have to get tricksy
            RenderTexture rt = RenderTexture.GetTemporary(original.width, original.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, 1);
            Graphics.Blit(original, rt);
            RenderTexture.active = rt;

            // Load new texture
            finalTexture.ReadPixels(new Rect(0, 0, finalTexture.width, finalTexture.height), 0, 0);

            // Kill the old one
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            // Return
            return finalTexture;
        }

        // Runs a function recursively
        public static TOut DoRecursive<TIn, TOut>(TIn start, Func<TIn, IEnumerable<TIn>> selector, Func<TOut, Boolean> check, Func<TIn, TOut> action)
        {
            TOut tout = action(start);
            if (check(tout))
                return tout;
            foreach (TIn tin in selector(start))
            {
                tout = DoRecursive(tin, selector, check, action);
                if (check(tout))
                    return tout;
            }
            return default(TOut);
        }

        // Runs a function recursively
        public static void DoRecursive<T>(T start, Func<T, IEnumerable<T>> selector, Action<T> action)
        {
            DoRecursive<T, Object>(start, selector, tout => false, tin => { action(tin); return null; });
        }
    }
}
