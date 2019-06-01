/*
 * Based on source code by cineboxandrew
 * https://github.com/Voxelgon/Voxelgon/blob/Develop/Assets/Voxelgon/Asset/Asset.cs#L250
 *
 * Licensed under the Apache2 license
 */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kopernicus.Configuration.Parsing
{
    public static class ObjImporter
    {
        //reads a .obj file and returns a Mesh object
        public static Mesh ImportFile(String path)
        {
            Mesh mesh = new Mesh();

            List<Int32> triangles = new List<Int32>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<Int32[]> facedata = new List<Int32[]>();

            StreamReader sr = new StreamReader(path);
            String objContents = sr.ReadToEnd();
            sr.Close();

            using (StringReader reader = new StringReader(objContents))
            {
                String line;
                Char[] splitId = {' '};
                Char[] splitId2 = {'/'};

                line = reader.ReadLine();

                while (line != null)
                {
                    line = line.Replace("  ", " ");
                    line = line.Trim();

                    String[] brokenString = line.Split(splitId, 50);

                    switch (brokenString[0])
                    {
                        case "v":
                            Vector3 vertexVector = new Vector3();
                            vertexVector.x = Convert.ToSingle(brokenString[1]);
                            vertexVector.y = Convert.ToSingle(brokenString[2]);
                            vertexVector.z = Convert.ToSingle(brokenString[3]);

                            vertices.Add(vertexVector);
                            break;

                        case "vt":
                        case "vt1":
                        case "vt2":
                            Vector2 uvVector = new Vector2();
                            uvVector.x = Convert.ToSingle(brokenString[1]);
                            uvVector.y = Convert.ToSingle(brokenString[2]);

                            uv.Add(uvVector);
                            break;

                        case "vn":
                            Vector3 normalVector = new Vector3();
                            normalVector.x = Convert.ToSingle(brokenString[1]);
                            normalVector.y = Convert.ToSingle(brokenString[2]);
                            normalVector.z = Convert.ToSingle(brokenString[3]);

                            normals.Add(normalVector);
                            break;

                        case "f":
                            List<Int32> face = new List<Int32>();

                            for (Int32 j = 1; j < brokenString.Length && ("" + brokenString[j]).Length > 0; j++)
                            {
                                Int32[] faceDataObject = new Int32[3];
                                String[] facePolyString = brokenString[j].Split(splitId2, 3);

                                faceDataObject[0] = Convert.ToInt32(facePolyString[0]);

                                if (facePolyString.Length > 1)
                                {
                                    if (facePolyString[1] != "")
                                    {
                                        faceDataObject[1] = Convert.ToInt32(facePolyString[1]);
                                    }

                                    if (facePolyString.Length > 2)
                                    {
                                        faceDataObject[2] = Convert.ToInt32(facePolyString[2]);
                                    }
                                }

                                facedata.Add(faceDataObject);
                                face.Add(faceDataObject[0]);
                            }

                            for (Int32 k = 2; k < face.Count; k++)
                            {
                                triangles.Add(face[0] - 1);
                                triangles.Add(face[k - 1] - 1);
                                triangles.Add(face[k] - 1);
                            }

                            break;
                    }

                    line = reader.ReadLine();

                }
            }

            Vector3[] vertexArray = vertices.ToArray();
            Vector2[] uvArray = new Vector2[vertexArray.Length];
            Vector3[] normalArray = new Vector3[vertexArray.Length];

            Int32 i = 0;
            foreach (Int32[] f in facedata)
            {
                if (f[1] >= 1)
                {
                    uvArray[f[0] - 1] = uv[f[1] - 1];
                }

                if (f[2] >= 1)
                {
                    normalArray[f[0] - 1] = normals[f[2] - 1];
                }
                i++;
            }

            mesh.vertices = vertexArray;
            mesh.uv = uvArray;
            mesh.normals = normalArray;
            mesh.triangles = triangles.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}