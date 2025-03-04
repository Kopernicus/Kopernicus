﻿/**
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.OnDemand;
using Kopernicus.RuntimeUtility;
using UnityEngine;
using Object = System.Object;

namespace Kopernicus
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class Utility
    {
        // Static object representing the deactivator
        private static Transform _deactivator;

        //internal floatcurves
        private static FloatCurve gasGiantMassVsRadiusCurve = null;

        /// <summary>
        /// Get an object which is deactivated, essentially, and children are prefabs
        /// </summary>
        public static Transform Deactivator
        {
            get
            {
                if (_deactivator != null)
                {
                    return _deactivator;
                }

                GameObject deactivatorObject = new GameObject("__deactivator");
                deactivatorObject.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(deactivatorObject);
                return _deactivator = deactivatorObject.transform;
            }
        }

        public static double GasGiantMassFromRadius(double radius)
        {
            //first setup floatcurve if not done
            if (gasGiantMassVsRadiusCurve == null)
            {
                gasGiantMassVsRadiusCurve = new FloatCurve();
                gasGiantMassVsRadiusCurve.Add(0f, 1.765050f, 0f, 3.380041f);
                gasGiantMassVsRadiusCurve.Add(6.982271f, 25.365415f, 3.380041f, 1.124136f);
                gasGiantMassVsRadiusCurve.Add(7.397801f, 25.975951f, 1.814452f, 1.814452f);
                gasGiantMassVsRadiusCurve.Add(7.765162f, 26.754608f, 2.424746f, 2.424746f);
                gasGiantMassVsRadiusCurve.Add(7.844546f, 27.278340f, 12.024317f, 12.024317f);
                gasGiantMassVsRadiusCurve.Add(7.903090f, 28.278340f, 22.585327f, 0f);
            }
            return Math.Pow(10, gasGiantMassVsRadiusCurve.Evaluate((float)Math.Log10(radius)));
        }

        public static bool IsStockBody(CelestialBody body)
        {
            float rescaleFactor = RuntimeUtility.RuntimeUtility.KopernicusConfig.RescaleFactor;
            if (body.name.Equals("Sun") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(261600000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Moho") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(250000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Eve") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(700000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Gilly") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(13000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Kerbin") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(600000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Mun") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(200000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Minmus") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(60000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Duna") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(320000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Ike") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(130000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Dres") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(138000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Jool") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(6000000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Laythe") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(500000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Vall") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(300000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Bop") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(65000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Pol") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(44000 * rescaleFactor)))
            {
                return true;
            }
            else if (body.name.Equals("Eeloo") && ((int)Math.Round(body.Radius)) == ((int)Math.Round(210000 * rescaleFactor)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get a reference to the CelestialBody instance from a PQS instance
        /// </summary>
        public static CelestialBody GetCelestialBody(this PQS sphere) => sphere.PQSModCBTransform?.body;

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
                if (field.IsStatic)
                {
                    continue;
                }

                if (log)
                {
                    Logger.Active.Log("Copying \"" + field.Name + "\": " + (field.GetValue(destination) ?? "<NULL>") + " => " + (field.GetValue(source) ?? "<NULL>"));
                }
                field.SetValue(destination, field.GetValue(source));
            }
        }

        // Dump an object by reflection
        public static void DumpObjectFields(Object o, String title = "---------")
        {
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

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static void DumpObjectProperties(Object o, String title = "---------")
        {
            // Iterate through all of the properties
            Logger.Active.Log("--------- " + title + " ------------");
            foreach (PropertyInfo property in o.GetType().GetProperties())
            {
                if (property.CanRead)
                {
                    Logger.Active.Log(property.Name + " = " + property.GetValue(o, null));
                }
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
            {
                return body;
            }

            // Otherwise search children
            foreach (PSystemBody child in body.children)
            {
                PSystemBody b = FindBody(child, name);
                if (b != null)
                {
                    return b;
                }
            }

            // Return null because we didn't find shit
            return null;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
            {
                return;
            }

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
                {
                    GameObjectWalk(b.gameObject, "    " + prefix);
                }
            }
        }

        public static PQSMod[] GetMods(PQS sphere)
        {
            // if the PQS has not been started, the mods array will be empty.  Do the slow thing.
            // we COULD cache that list of mods, but I don't know if new ones may be created and missed
            if (sphere?.mods == null)
            {
                // why doesn't this just iterate over the immmediate children?
                // is it because we need to include inactive components?
                return sphere.GetComponentsInChildren<PQSMod>(true).Where(m => m.transform.parent == sphere.transform).ToArray();
            }

            return sphere.mods;
        }

        public static T GetMod<T>(PQS sphere) where T : PQSMod
        {
            var mods = GetMods(sphere);
            for (int i = mods.Length; i-- > 0;)
            {
                if (mods[i] is T mod)
                {
                    return mod;
                }
            }
            return null;
        }

        public static Boolean HasMod<T>(PQS sphere) where T : PQSMod
        {
            return GetMod<T>(sphere) != null;
        }

        public static T AddMod<T>(PQS sphere, Int32 order) where T : PQSMod
        {
            GameObject modObject = new GameObject(typeof(T).Name);
            modObject.transform.parent = sphere.transform;
            T mod = modObject.AddComponent<T>();
            mod.modEnabled = true;
            mod.order = order;
            mod.sphere = sphere;
            return mod;
        }

        public static void UpdateScaledMesh(GameObject scaledVersion, PQS pqs, CelestialBody body, String path,
            String cacheFile, Boolean exportMesh, Boolean useSpherical)
        {
            const Double R_JOOL = 6000000.0;
            const Single R_SCALED = 1000.0f;

            // Compute scale between Jool and this body
            Single scale = (Single)(body.Radius / R_JOOL);
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

            Directory.CreateDirectory(cacheDirectory ?? throw new InvalidOperationException());

            if (File.Exists(cacheFile) && exportMesh)
            {
                Logger.Active.Log("Body.PostApply(ConfigNode): Loading cached scaled space mesh: " + body.name);
                scaledVersion.GetComponent<MeshFilter>().sharedMesh = MeshPreloader.Meshes.ContainsKey(cacheFile)
                    ? MeshPreloader.Meshes[cacheFile]
                    : DeserializeMesh(cacheFile);

                scaledVersion.GetComponent<MeshFilter>().sharedMesh.indexFormat = scaledVersion.GetComponent<MeshFilter>().sharedMesh.vertexCount > 65536
                    ? UnityEngine.Rendering.IndexFormat.UInt32
                    : UnityEngine.Rendering.IndexFormat.UInt16;
            }

            // Otherwise we have to generate the mesh
            else
            {
                Logger.Active.Log("Body.PostApply(ConfigNode): Generating scaled space mesh: " + body.name);
                Mesh scaledMesh = ComputeScaledSpaceMesh(body, useSpherical ? null : pqs);
                scaledVersion.GetComponent<MeshFilter>().sharedMesh = scaledMesh;
                if (exportMesh)
                {
                    SerializeMesh(scaledMesh, cacheFile);
                }
            }

            // Apply mesh to the body
            if (scaledVersion.TryGetComponent<SphereCollider>(out var collider))
            {
                collider.radius = R_SCALED;
            }
            if (pqs != null && scaledVersion.gameObject != null && scaledVersion.transform != null)
            {
                scaledVersion.transform.localScale = Vector3.one * (Single)(pqs.radius / R_JOOL);
            }
        }

        // Generate the scaled space mesh using PQS (all results use scale of 1)
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static Mesh ComputeScaledSpaceMesh(CelestialBody body, PQS pqs)
        {
            // We need to get the body for Jool (to steal it's mesh)
            const Double R_SCALED_JOOL = 1000.0f;
            Double rMetersToScaledUnits = (Single)(R_SCALED_JOOL / body.Radius);

            // Generate a duplicate of the Jool mesh
            Mesh mesh = DuplicateMesh(Templates.ReferenceGeosphere);

            Logger.Active.Log(body);
            Logger.Active.Log(pqs);
            Logger.Active.Log(body.pqsController);

            // If this body has a PQS, we can create a more detailed object, otherwise just return the generic mesh
            if (pqs == null)
            {
                return mesh;
            }

            // first we enable all maps
            OnDemandStorage.EnableBody(body.bodyName);

            // In order to generate the scaled space we have to enable the mods.  Since this is
            // a prefab they don't get disabled as kill game performance.  To resolve this we
            // clone the PQS, use it, and then delete it when done. At runtime we can simply use
            // the PQS that is active
            GameObject pqsVersionGameObject =
                Injector.IsInPrefab ? Instantiate(pqs.gameObject) : pqs.gameObject;
            PQS pqsVersion = pqsVersionGameObject.GetComponent<PQS>();

            try
            {
                pqsVersion.name = pqsVersion.name.Replace("(Clone)", "");
            }
            catch
            {
                //Just in case there is no .name or pqsVersion
            }

            // Deactivate blacklisted Mods
            PQSMod[] mods = pqsVersion.GetComponentsInChildren<PQSMod>(true).OrderBy(m => m.order).ToArray();
            for (Int32 i = 0; i < mods.Length; i++)
            {
                // Disable mods that don't belong to us
                if (mods[i].transform.parent != pqsVersion.transform)
                {
                    mods[i].modEnabled = false;
                    continue;
                }

                switch (mods[i])
                {
                    // Disable the OnDemand notifier
                    case PQSMod_OnDemandHandler _:
                        mods[i].modEnabled = false;
                        continue;
                    case PQSMod_FlattenArea _:
                        typeof(PQSMod_FlattenArea).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                            .First(f => f.FieldType == typeof(Boolean)).SetValue(mods[i], true);
                        break;
                }
            }

            pqsVersion.StartUpSphere();
            pqsVersion.isBuildingMaps = true;

            // If we were able to find PQS mods
            if (mods.Length > 0)
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
                    PQS.VertexBuildData vertex = new PQS.VertexBuildData
                    {
                        directionFromCenter = direction, vertHeight = body.Radius, u = uv.x, v = uv.y
                    };

                    // Build from the PQS
                    for (Int32 m = 0; m < mods.Length; m++)
                    {
                        // Don't build disabled mods
                        if (!mods[m].modEnabled)
                        {
                            continue;
                        }

                        // Don't build mods that don't belong to us
                        if (mods[m].transform.parent != pqsVersion.transform)
                        {
                            continue;
                        }

                        mods[m].OnVertexBuildHeight(vertex);
                    }

                    // Check for sea level
                    if (pqsVersion.mapOcean)
                    {
                        vertex.vertHeight = Math.Max(vertex.vertHeight, body.Radius);
                    }

                    // Adjust the displacement
                    vertices[i] = direction * (Single)(vertex.vertHeight * rMetersToScaledUnits);
                }
                mesh.vertices = vertices;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                RecalculateTangents(mesh);
            }

            // Cleanup
            pqsVersion.isBuildingMaps = false;
            pqsVersion.DeactivateSphere();

            // If we are working with a copied PQS, clean it up
            if (Injector.IsInPrefab)
            {
                UnityEngine.Object.Destroy(pqsVersionGameObject);
            }
            OnDemandStorage.DisableBody(body.bodyName);

            // Return the generated scaled space mesh
            return mesh;
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static void CopyMesh(Mesh source, Mesh dest)
        {
            Vector3[] verts = new Vector3[source.vertexCount];
            source.vertices.CopyTo(verts, 0);
            dest.vertices = verts;

            Int32[] tris = new Int32[source.triangles.Length];
            source.triangles.CopyTo(tris, 0);
            dest.triangles = tris;

            Vector2[] uv = new Vector2[source.uv.Length];
            source.uv.CopyTo(uv, 0);
            dest.uv = uv;

            Vector2[] uv2 = new Vector2[source.uv2.Length];
            source.uv2.CopyTo(uv2, 0);
            dest.uv2 = uv2;

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

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static Mesh DuplicateMesh(Mesh source)
        {
            // Create new mesh object
            Mesh dest = new Mesh();
            CopyMesh(source, dest);
            return dest;
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static void RecalculateTangents(Mesh theMesh)
        {
            Int32 vertexCount = theMesh.vertexCount;
            Vector3[] vertices = theMesh.vertices;
            Vector3[] normals = theMesh.normals;
            Vector2[] uv = theMesh.uv;
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

                Vector2 w1 = uv[i1];
                Vector2 w2 = uv[i2];
                Vector2 w3 = uv[i3];

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
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static void SerializeMesh(Mesh mesh, String path)
        {
            // Open an output file stream
            FileStream outputStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            using (BinaryWriter writer = new BinaryWriter(outputStream))
            {

                // Indicate that this is version two of the .bin format
                writer.Write(-2);

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
                writer.Write(mesh.uv2.Length);
                foreach (Vector2 uv2 in mesh.uv2)
                {
                    writer.Write(uv2.x);
                    writer.Write(uv2.y);
                }
                writer.Write(mesh.normals.Length);
                foreach (Vector3 normal in mesh.normals)
                {
                    writer.Write(normal.x);
                    writer.Write(normal.y);
                    writer.Write(normal.z);
                }
                writer.Write(mesh.tangents.Length);
                foreach (Vector4 tangent in mesh.tangents)
                {
                    writer.Write(tangent.x);
                    writer.Write(tangent.y);
                    writer.Write(tangent.z);
                    writer.Write(tangent.w);
                }

                // Finish writing
                writer.Close();
                outputStream.Close();
            }
        }

        // Deserialize a mesh from disk
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static Mesh DeserializeMesh(String path)
        {
            FileStream inputStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using (BinaryReader reader = new BinaryReader(inputStream))
            {
                Mesh m;

                // Check if the file is bin v1 or v2
                Int32 first = reader.ReadInt32();
                Boolean v2 = first == -2;

                // Parse a v1 mesh
                if (!v2)
                {
                    // Get the vertices
                    Int32 count = first;
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
                    Int32 uvCount = reader.ReadInt32();
                    Vector2[] uvs = new Vector2[uvCount];
                    for (Int32 i = 0; i < uvCount; i++)
                    {
                        Vector2 uv;
                        uv.x = reader.ReadSingle();
                        uv.y = reader.ReadSingle();
                        uvs[i] = uv;
                    }

                    // Get the triangles
                    Int32 trisCount = reader.ReadInt32();
                    Int32[] triangles = new Int32[trisCount];
                    for (Int32 i = 0; i < trisCount; i++)
                    {
                        triangles[i] = reader.ReadInt32();
                    }

                    // Create the mesh
                    m = new Mesh
                    {
                        vertices = vertices, triangles = triangles, uv = uvs
                    };
                    m.RecalculateNormals();
                    RecalculateTangents(m);
                }
                else
                {
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
                    Int32 uvCount = reader.ReadInt32();
                    Vector2[] uvs = new Vector2[uvCount];
                    for (Int32 i = 0; i < uvCount; i++)
                    {
                        Vector2 uv;
                        uv.x = reader.ReadSingle();
                        uv.y = reader.ReadSingle();
                        uvs[i] = uv;
                    }

                    // Get the triangles
                    Int32 trisCount = reader.ReadInt32();
                    Int32[] triangles = new Int32[trisCount];
                    for (Int32 i = 0; i < trisCount; i++)
                    {
                        triangles[i] = reader.ReadInt32();
                    }

                    // Get the uv2s
                    Int32 uv2Count = reader.ReadInt32();
                    // ReSharper disable once InconsistentNaming
                    Vector2[] uv2s = new Vector2[uv2Count];
                    for (Int32 i = 0; i < uv2Count; i++)
                    {
                        Vector2 uv2;
                        uv2.x = reader.ReadSingle();
                        uv2.y = reader.ReadSingle();
                        uv2s[i] = uv2;
                    }

                    // Get the normals
                    Int32 normalCount = reader.ReadInt32();
                    Vector3[] normals = new Vector3[normalCount];
                    for (Int32 i = 0; i < normalCount; i++)
                    {
                        Vector3 normal;
                        normal.x = reader.ReadSingle();
                        normal.y = reader.ReadSingle();
                        normal.z = reader.ReadSingle();
                        normals[i] = normal;
                    }

                    // Get the tangents
                    Int32 tangentCount = reader.ReadInt32();
                    Vector4[] tangents = new Vector4[tangentCount];
                    for (Int32 i = 0; i < tangentCount; i++)
                    {
                        Vector4 tangent;
                        tangent.x = reader.ReadSingle();
                        tangent.y = reader.ReadSingle();
                        tangent.z = reader.ReadSingle();
                        tangent.w = reader.ReadSingle();
                        tangents[i] = tangent;
                    }

                    // Create the mesh
                    m = new Mesh
                    {
                        indexFormat = count > (65536) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16,
                        vertices = vertices,
                        triangles = triangles,
                        uv = uvs,
                        uv2 = uv2s,
                        normals = normals,
                        tangents = tangents
                    };
                }

                // Close
                reader.Close();
                inputStream.Close();

                return m;
            }
        }

        // Credit goes to Sigma88.
        public static Texture2D BumpToNormalMap(Texture2D source, PQS pqs, Single strength)
        {
            Double dS = pqs.radius * 2 * Math.PI / source.width;

            if (strength <= 0)
            {
                strength = 1;
            }

            Texture2D result = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);
            for (Int32 by = 0; by < result.height; by++)
            {
                for (Int32 bx = 0; bx < result.width; bx++)
                {
                    if (by == 0 || by == result.height - 1 || Math.Abs(source.GetPixel(bx, by).r) < 0.01)
                    {
                        result.SetPixel(bx, by, new Color(0.5f, 0.5f, 0.5f, 0.5f));
                    }
                    else
                    {
                        Int32 xN = bx - 1;
                        Int32 xP = bx + 1;

                        Int32 yN = by - 1;
                        Int32 yP = by + 1;

                        Double dX = (source.GetPixel(xN, by).r - source.GetPixel(xP, by).r) * pqs.radiusDelta;
                        Double dY = (source.GetPixel(bx, yN).r - source.GetPixel(bx, yP).r) * pqs.radiusDelta;

                        Double slopeX = (1 + dX / Math.Pow(dX * dX + dS * dS, 0.5) * strength) / 2;
                        Double slopeY = (1 + dY / Math.Pow(dY * dY + dS * dS, 0.5) * strength) / 2;

                        result.SetPixel(bx, by, new Color((Single)slopeY, (Single)slopeY, (Single)slopeY, (Single)slopeX));
                    }
                }
            }
            result.Apply();
            return result;
        }

        // Convert latitude-longitude-altitude with body radius to a vector.
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static Vector3 LLAtoECEF(Double lat, Double lon, Double alt, Double radius)
        {
            const Double DEGREES_TO_RADIANS = Math.PI / 180.0;
            lat = (lat - 90) * DEGREES_TO_RADIANS;
            lon *= DEGREES_TO_RADIANS;
            Double n = radius; // for now, it's still a sphere, so just the radius
            Double x = (n + alt) * -1.0 * Math.Sin(lat) * Math.Cos(lon);
            Double y = (n + alt) * Math.Cos(lat);
            Double z = (n + alt) * -1.0 * Math.Sin(lat) * Math.Sin(lon);
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

        // This function loads up some animationstates
        public static AnimationState[] SetUpAnimation(string animationName, Part part)
        {
            var states = new List<AnimationState>();
            foreach (var animation in part.FindModelAnimators(animationName))
            {
                var animationState = animation[animationName];
                animationState.speed = 0;
                animationState.enabled = true;
                // Clamp this or else weird things happen
                animationState.wrapMode = WrapMode.ClampForever;
                animation.Blend(animationName);
                states.Add(animationState);
            }
            // Convert 
            return states.ToArray();
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
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
            {
                Logger.Active.Log("MapSO grabber: Tried to grab " + url + " but body not found.");
            }

            if (retVal == null)
            {
                if (modFound)
                {
                    Logger.Active.Log("MapSO grabber: Tried to grab " + url +
                                      " but mods of correct name and type lack MapSO.");
                }
                else
                {
                    Logger.Active.Log("MapSO grabber: Tried to grab " + url +
                                      " but could not find PQSMod of that type of the given name");
                }
            }
            if (retVal != null)
            {
                retVal.name = url;
            }
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
            {
                types = new List<Type>();
            }
            if (blacklist == null)
            {
                Logger.Active.Log("Creating blacklist");
                blacklist = new List<Type>();
                if (!types.Contains(typeof(PQSMod_CelestialBodyTransform)))
                {
                    blacklist.Add(typeof(PQSMod_CelestialBodyTransform));
                }
                if (!types.Contains(typeof(PQSMod_MaterialSetDirection)))
                {
                    blacklist.Add(typeof(PQSMod_MaterialSetDirection));
                }
                if (!types.Contains(typeof(PQSMod_UVPlanetRelativePosition)))
                {
                    blacklist.Add(typeof(PQSMod_UVPlanetRelativePosition));
                }
                if (!types.Contains(typeof(PQSMod_QuadMeshColliders)))
                {
                    blacklist.Add(typeof(PQSMod_QuadMeshColliders));
                }
                Logger.Active.Log("Blacklist count = " + blacklist.Count);
            }

            if (addTypes)
            {
                Logger.Active.Log("Adding all found PQSMods in pqs " + p.name);
                foreach (PQSMod m in cpMods)
                {
                    Type mType = m.GetType();
                    if (types.Contains(mType) || blacklist.Contains(mType))
                    {
                        continue;
                    }

                    Logger.Active.Log("Adding to remove list: " + mType);
                    types.Add(mType);
                }
            }
            List<GameObject> toCheck = new List<GameObject>();
            foreach (Type mType in types)
            {
                List<PQSMod> mods = cpMods.Where(m => m.GetType() == mType).ToList();
                foreach (PQSMod delMod in mods)
                {
                    if (delMod == null)
                    {
                        continue;
                    }

                    Logger.Active.Log("Removed mod " + mType);
                    if (!toCheck.Contains(delMod.gameObject))
                    {
                        toCheck.Add(delMod.gameObject);
                    }

                    delMod.sphere = null;
                    switch (delMod)
                    {
                        case PQSCity mod:
                            {
                                PQSCity city = mod;
                                if (city.lod != null)
                                {
                                    foreach (PQSCity.LODRange range in city.lod)
                                    {
                                        if (range.objects != null)
                                        {
                                            foreach (GameObject o in range.objects)
                                            {
                                                UnityEngine.Object.DestroyImmediate(o);
                                            }
                                        }

                                        if (range.renderers == null)
                                        {
                                            continue;
                                        }
                                        foreach (GameObject o in range.renderers)
                                        {
                                            UnityEngine.Object.DestroyImmediate(o);
                                        }
                                    }
                                }
                                break;
                            }
                        case PQSCity2 mod:
                            {
                                PQSCity2 city = mod;
                                if (city.objects != null)
                                {
                                    foreach (PQSCity2.LodObject range in city.objects)
                                    {
                                        if (range.objects == null)
                                        {
                                            continue;
                                        }
                                        foreach (GameObject o in range.objects)
                                        {
                                            UnityEngine.Object.DestroyImmediate(o);
                                        }
                                    }
                                }
                                break;
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

        // Converts an unreadable texture into a readable one
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Texture2D CreateReadable(Texture2D original)
        {
            // Checks
            if (original == null)
            {
                return null;
            }
            if (original.width == 0 || original.height == 0)
            {
                return null;
            }

            // Create the new texture
            Texture2D finalTexture = new Texture2D(original.width, original.height);

            // isn't read or writeable ... we'll have to get tricky
            RenderTexture rt = RenderTexture.GetTemporary(original.width, original.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, 1);
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
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static TOut DoRecursive<TIn, TOut>(TIn start, Func<TIn, IEnumerable<TIn>> selector,
            Func<TOut, Boolean> check, Func<TIn, TOut> action)
        {
            TOut tout = action(start);
            if (check(tout))
            {
                return tout;
            }

            foreach (TIn tin in selector(start))
            {
                tout = DoRecursive(tin, selector, check, action);
                if (check(tout))
                {
                    return tout;
                }
            }

            return default(TOut);
        }

        // Runs a function recursively
        public static void DoRecursive<T>(T start, Func<T, IEnumerable<T>> selector, Action<T> action)
        {
            DoRecursive<T, Object>(start, selector, tout => false, tin =>
            {
                action(tin);
                return null;
            });
        }

        public static T Instantiate<T>(T original) where T : UnityEngine.Object
        {
            return UnityEngine.Object.Instantiate(original);
        }

        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : UnityEngine.Object
        {
            return UnityEngine.Object.Instantiate(original, position, rotation);
        }

        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation, Transform parent)
            where T : UnityEngine.Object
        {
            return UnityEngine.Object.Instantiate(original, position, rotation, parent);
        }

        public static T Instantiate<T>(T original, Transform parent) where T : UnityEngine.Object
        {
            return UnityEngine.Object.Instantiate(original, parent);
        }

        public static T Instantiate<T>(T original, Transform parent, Boolean worldPositionStays)
            where T : UnityEngine.Object
        {
            return UnityEngine.Object.Instantiate(original, parent, worldPositionStays);
        }

        public static Double Clamp(Double value, Double min, Double max)
        {
            if (value > max)
            {
                return max;
            }

            if (value > min)
            {
                return value;
            }

            return min;
        }

        public static FloatCurve ListToFloatCurve(List<NumericCollectionParser<Single>> parser)
        {
            if (parser == null)
            {
                return null;
            }

            // Additional error correction will only happen at runtime to keep the configs
            // compatible with the stock KSP behaviour
            if (!Injector.IsInPrefab)
            {
                // First make sure that all elements contain at least 2 values
                for (Int32 i = 0; i < parser.Count; i++)
                {
                    if (parser[i].Value.Count < 2)
                    {
                        while (parser[i].Value.Count < 2)
                        {
                            parser[i].Value.Add(0);
                        }
                    }
                }

                // Then make sure that no index is used twice
                for (Int32 i = 0; i < parser.Count; i++)
                {
                    while (parser.Count(p => Math.Abs(p.Value[0] - parser[i].Value[0]) < 0.001) > 1)
                    {
                        parser[i].Value[0] += 0.1f;
                    }
                }
            }

            FloatCurve curve = new FloatCurve();

            for (Int32 i = 0; i < parser.Count; i++)
            {
                NumericCollectionParser<Single> key = parser[i];

                if (key.Value.Count < 2)
                {
                    Debug.LogError("FloatCurve: Invalid line. Requires two values, 'time' and 'value'");
                }

                if (key.Value.Count == 4)
                {
                    curve.Add(key.Value[0], key.Value[1], key.Value[2], key.Value[3]);
                }
                else
                {
                    curve.Add(key.Value[0], key.Value[1]);
                }
            }

            return curve;
        }

        public static AnimationCurve ListToAnimCurve(List<NumericCollectionParser<Single>> parser)
        {
            return ListToFloatCurve(parser).Curve;
        }

        public static List<NumericCollectionParser<Single>> FloatCurveToList(FloatCurve curve)
        {
            if (curve == null)
            {
                return null;
            }

            List<NumericCollectionParser<Single>> list = new List<NumericCollectionParser<Single>>();

            for (Int32 i = 0; i < curve.Curve.length; i++)
            {
                Keyframe key = curve.Curve.keys[i];
                list.Add(new List<Single>
                {
                    key.time, key.value, key.inTangent, key.outTangent
                });
            }

            return list;
        }

        public static List<NumericCollectionParser<Single>> AnimCurveToList(AnimationCurve curve)
        {
            return FloatCurveToList(new FloatCurve(curve?.keys ?? new Keyframe[0]));
        }

        public static Func<S, T> CreateGetter<S, T>(FieldInfo field)
        {
            string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, typeof(T), new Type[1]
            {
                typeof(S)
            }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
            }
            gen.Emit(OpCodes.Ret);
            return (Func<S, T>)setterMethod.CreateDelegate(typeof(Func<S, T>));
        }

        public static Action<S, T> CreateSetter<S, T>(FieldInfo field)
        {
            string methodName = field.ReflectedType.FullName + ".set_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[2]
            {
                typeof(S), typeof(T)
            }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, field);
            }
            gen.Emit(OpCodes.Ret);
            return (Action<S, T>)setterMethod.CreateDelegate(typeof(Action<S, T>));
        }

        /// <summary>
        /// Returns the largest value that compares less than a specified value.
        /// </summary>
        /// <param name="x">The value to decrement.</param>
        /// <returns>The largest value that compares less than x, or NegativeInfinity if x equals NegativeInfinity, or NaN if x equals NaN.</returns>
        /// // https://github.com/dotnet/runtime/blob/af4efb1936b407ca5f4576e81484cf5687b79a26/src/libraries/System.Private.CoreLib/src/System/Math.cs#L210
        public static double BitDecrement(double x)
        {
            long bits = BitConverter.DoubleToInt64Bits(x);

            if (((bits >> 32) & 0x7FF00000) >= 0x7FF00000)
            {
                // NaN returns NaN
                // -Infinity returns -Infinity
                // +Infinity returns double.MaxValue
                return (bits == 0x7FF00000_00000000) ? double.MaxValue : x;
            }

            if (bits == 0x00000000_00000000)
            {
                // +0.0 returns -double.Epsilon
                return -double.Epsilon;
            }

            // Negative values need to be incremented
            // Positive values need to be decremented

            bits += ((bits < 0) ? +1 : -1);
            return BitConverter.Int64BitsToDouble(bits);
        }

        /// <summary>
        /// Get the biome definition at a given latitude/longitude
        /// </summary>
        /// <param name="body">The CelestialBody to check biomes on</param>
        /// <param name="lat">latitude in degrees (KSP usually exposes it as degrees)</param>
        /// <param name="lon">longitude in degrees (KSP usually exposes it as degrees)</param>
        /// <returns>null if the body doesn't have a biome map, the MapAttribute biome definition otherwise</returns>
        public static CBAttributeMapSO.MapAttribute GetBiome(CelestialBody body, double lat, double lon)
        {
            if (body.BiomeMap.IsNullOrDestroyed())
                return null;

            lat = ((lat + 180.0 + 90.0) % 180.0 - 90.0) * UtilMath.Deg2Rad; // clamp and convert to radians
            lon = ((lon + 360.0 + 180.0) % 360.0 - 180.0) * UtilMath.Deg2Rad; // clamp and convert to radians
            return body.BiomeMap.GetAtt(lat, lon);
        }

        /// <summary>
        /// Get the biome definition at a given world position
        /// </summary>
        /// <param name="body">The CelestialBody to check biomes on</param>
        /// <param name="position">World position</param>
        /// <returns>null if the body doesn't have a biome map, the MapAttribute biome definition otherwise</returns>
        public static CBAttributeMapSO.MapAttribute GetBiome(CelestialBody body, Vector3d position)
        {
            if (body.BiomeMap.IsNullOrDestroyed())
                return null;

            Vector3d rPos = (position - body.position).normalized;
            rPos = body.BodyFrame.WorldToLocal(rPos.xzy);
            double lat = Math.Asin(rPos.z);

            if (double.IsNaN(lat))
                lat = 0.0;

            double lon = Math.Atan2(rPos.y, rPos.x);
            if (double.IsNaN(lon))
                lon = 0.0;

            return body.BiomeMap.GetAtt(lat, lon);
        }
    }

#pragma warning disable IDE0041 // Use 'is null' check
    static class UnityExtensions
    {
        /// <summary>
        /// Perform a true reference equality comparison between two UnityEngine.Object references.<br/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RefEquals(this UnityEngine.Object unityObject, UnityEngine.Object otherUnityObject)
        {
            return ReferenceEquals(unityObject, otherUnityObject);
        }

        /// <summary>
        /// Perform a true reference inequality comparison between two UnityEngine.Object references.<br/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RefNotEquals(this UnityEngine.Object unityObject, UnityEngine.Object otherUnityObject)
        {
            return !ReferenceEquals(unityObject, otherUnityObject);
        }

        /// <summary>
        /// Equivalent as the Unity <c>==</c> operator, but 6-8 times faster.<br/>
        /// Use this if you want to perform an equality check where :<br/>
        /// - a destroyed <c>UnityEngine.Object</c> instance is considered equal to <c>null</c><br/>
        /// - two different destroyed <c>UnityEngine.Object</c> instances are not considered equal
        /// </summary>
        public static bool NotDestroyedRefEquals(this UnityEngine.Object unityObject, UnityEngine.Object otherUnityObject)
        {
            if (ReferenceEquals(unityObject, otherUnityObject))
                return true;

            if (ReferenceEquals(otherUnityObject, null) && unityObject.m_CachedPtr == IntPtr.Zero)
                return true;

            if (ReferenceEquals(unityObject, null) && otherUnityObject.m_CachedPtr == IntPtr.Zero)
                return true;

            return false;
        }

        /// <summary>
        /// Equivalent as the Unity <c>!=</c> operator, but 6-8 times faster.<br/>
        /// Use this if you want to perform an equality check where :<br/>
        /// - a destroyed <c>UnityEngine.Object</c> instance is considered equal to <c>null</c><br/>
        /// - two different destroyed <c>UnityEngine.Object</c> instances are not considered equal
        /// </summary>
        public static bool NotDestroyedRefNotEquals(this UnityEngine.Object unityObject, UnityEngine.Object otherUnityObject)
        {
            if (ReferenceEquals(unityObject, otherUnityObject))
                return false;

            if (ReferenceEquals(otherUnityObject, null) && unityObject.m_CachedPtr == IntPtr.Zero)
                return false;

            if (ReferenceEquals(unityObject, null) && otherUnityObject.m_CachedPtr == IntPtr.Zero)
                return false;

            return true;
        }

        /// <summary>
        /// True if this <paramref name="unityObject"/> instance is destroyed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDestroyed(this UnityEngine.Object unityObject)
        {
            return unityObject.m_CachedPtr == IntPtr.Zero;
        }

        /// <summary>
        /// True if this <paramref name="unityObject"/> reference is <c>null</c>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullRef(this UnityEngine.Object unityObject)
        {
            return ReferenceEquals(unityObject, null);
        }

        /// <summary>
        /// True if this <paramref name="unityObject"/> reference is not <c>null</c>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNullRef(this UnityEngine.Object unityObject)
        {
            return !ReferenceEquals(unityObject, null);
        }

        /// <summary>
        /// True if this <paramref name="unityObject"/> reference is <c>null</c> or if the instance is destroyed<br/>
        /// Equivalent as testing <c><paramref name="unityObject"/> == null</c> but 4-5 times faster.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrDestroyed(this UnityEngine.Object unityObject)
        {
            return ReferenceEquals(unityObject, null) || unityObject.m_CachedPtr == IntPtr.Zero;
        }

        /// <summary>
        /// True if this <paramref name="unityObject"/> reference is not <c>null</c> and the instance is not destroyed<br/>
        /// Equivalent as testing <c><paramref name="unityObject"/> != null</c> but 4-5 times faster.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNullOrDestroyed(this UnityEngine.Object unityObject)
        {
            return !ReferenceEquals(unityObject, null) && unityObject.m_CachedPtr != IntPtr.Zero;
        }

        /// <summary>
        /// Return <c>null</c> when this <paramref name="unityObject"/> reference is <c>null</c> or destroyed, otherwise return the <paramref name="unityObject"/> instance<br/>
        /// Allow using null conditional and null coalescing operators with <c>UnityEngine.Object</c> derivatives while conforming to the "a destroyed object is equal to null" Unity concept.<br/>
        /// Example :<br/>
        /// <c>float x = myUnityObject.DestroyedAsNull()?.myFloatField ?? 0f;</c><br/>
        /// will evaluate to <c>0f</c> when <c>myUnityObject</c> is destroyed, instead of returning the value still available on the managed instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DestroyedAsNull<T>(this T unityObject) where T : UnityEngine.Object
        {
            if (ReferenceEquals(unityObject, null) || unityObject.m_CachedPtr == IntPtr.Zero)
                return null;

            return unityObject;
        }

        public static void DestroyComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent<T>(out var component))
            {
                Component.Destroy(component);
            }
        }
    }
#pragma warning restore IDE0041 // Use 'is null' check
}

namespace Kopernicus.Components
{
    /// <summary>
    /// This used to be a performance oriented cache of biome positions. With the introduction of the performance optimized 
    /// KopernicusCBAttributeMapSO, querying the biome directly by usual means has become faster than using the cache, so it
    /// has been removed. As of writing (10/2023), the PQSMod_BiomeSampler.GetCachedBiome() method is called by Parallax, 
    /// which is why it is kept around.
    /// </summary>
    public class PQSMod_BiomeSampler
    {
        [Obsolete("Just use Utility.GetBiome(), this formerly implemented a cache for performance but we now have a faster base implementation")]
        public static string GetCachedBiome(double lat, double lon, CelestialBody cb)
        {
            return Utility.GetBiome(cb, lat, lon)?.name;
        }
    }
}
