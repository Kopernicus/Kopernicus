/* This version of ObjImporter first reads through the entire file, getting a count of how large
 * the final arrays will be, and then uses standard arrays for everything (as opposed to ArrayLists
 * or any other fancy things). 
 * 
 * Based on code by el anónimo
 */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kopernicus.Configuration.Parsing
{
    public static class ObjImporter
    {
        private struct MeshStruct
        {
            public Vector3[] Vertices;
            public Vector3[] Normals;
            public Vector2[] Uv;
            public Int32[] Triangles;
            public Vector3[] FaceData;
            public String FileName;
        }

        // Use this for initialization
        public static Mesh ImportFile(String filePath)
        {
            MeshStruct newMesh = CreateMeshStruct(filePath);
            PopulateMeshStruct(ref newMesh);

            Vector3[] newVerts = new Vector3[newMesh.FaceData.Length];
            Vector2[] newUVs = new Vector2[newMesh.FaceData.Length];
            Vector3[] newNormals = new Vector3[newMesh.FaceData.Length];
            Int32 i = 0;
            /* The following foreach loops through the face data and assigns the appropriate vertex, uv, or normal
         * for the appropriate Unity mesh array.
         */
            foreach (Vector3 v in newMesh.FaceData)
            {
                newVerts[i] = newMesh.Vertices[(Int32)v.x - 1];
                if (v.y >= 1)
                {
                    newUVs[i] = newMesh.Uv[(Int32)v.y - 1];
                }

                if (v.z >= 1)
                {
                    newNormals[i] = newMesh.Normals[(Int32)v.z - 1];
                }

                i++;
            }

            Mesh mesh = new Mesh
            {
                vertices = newVerts,
                uv = newUVs,
                normals = newNormals,
                triangles = newMesh.Triangles
            };


            mesh.RecalculateBounds();

            return mesh;
        }

        private static MeshStruct CreateMeshStruct(String filename)
        {
            Int32 triangles = 0;
            Int32 vertices = 0;
            Int32 vt = 0;
            Int32 vn = 0;
            Int32 face = 0;
            MeshStruct mesh = new MeshStruct {FileName = filename};
            using (StreamReader stream = File.OpenText(filename))
            {
                String entireText = stream.ReadToEnd();
                stream.Close();
                using (StringReader reader = new StringReader(entireText))
                {
                    String currentText = reader.ReadLine();
                    Char[] splitIdentifier = { ' ' };
                    while (currentText != null)
                    {
                        if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ")
                            && !currentText.StartsWith("vn "))
                        {
                            currentText = reader.ReadLine();
                            currentText = currentText?.Replace("  ", " ");
                        }
                        else
                        {
                            currentText = currentText.Trim();                           //Trim the current line
                            String[] brokenString = currentText.Split(splitIdentifier, 50);
                            // ReSharper disable once SwitchStatementMissingSomeCases
                            switch (brokenString[0])
                            {
                                case "v":
                                    vertices++;
                                    break;
                                case "vt":
                                    vt++;
                                    break;
                                case "vn":
                                    vn++;
                                    break;
                                case "f":
                                    face = face + brokenString.Length - 1;
                                    triangles += 3 * (brokenString.Length - 2); /*brokenString.Length is 3 or greater since a face must have at least
                                                                                     3 vertices.  For each additional vertex, there is an additional
                                                                                     triangle in the mesh (hence this formula).*/
                                    break;
                            }
                            currentText = reader.ReadLine();
                            currentText = currentText?.Replace("  ", " ");
                        }
                    }
                }
                mesh.Triangles = new Int32[triangles];
                mesh.Vertices = new Vector3[vertices];
                mesh.Uv = new Vector2[vt];
                mesh.Normals = new Vector3[vn];
                mesh.FaceData = new Vector3[face];
                return mesh;
            }
        }

        private static void PopulateMeshStruct(ref MeshStruct mesh)
        {
            using (StreamReader stream = File.OpenText(mesh.FileName))
            {
                String entireText = stream.ReadToEnd();
                stream.Close();
                using (StringReader reader = new StringReader(entireText))
                {
                    String currentText = reader.ReadLine();

                    Char[] splitIdentifier = { ' ' };
                    Char[] splitIdentifier2 = { '/' };
                    Int32 f = 0;
                    Int32 f2 = 0;
                    Int32 v = 0;
                    Int32 vn = 0;
                    Int32 vt = 0;
                    Int32 vt1 = 0;
                    Int32 vt2 = 0;
                    while (currentText != null)
                    {
                        if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ") &&
                            !currentText.StartsWith("vn ") && !currentText.StartsWith("g ") && !currentText.StartsWith("usemtl ") &&
                            !currentText.StartsWith("mtllib ") && !currentText.StartsWith("vt1 ") && !currentText.StartsWith("vt2 ") &&
                            !currentText.StartsWith("vc ") && !currentText.StartsWith("usemap "))
                        {
                            currentText = reader.ReadLine();
                            currentText = currentText?.Replace("  ", " ");
                        }
                        else
                        {
                            currentText = currentText.Trim();
                            String[] brokenString = currentText.Split(splitIdentifier, 50);
                            // ReSharper disable once SwitchStatementMissingSomeCases
                            switch (brokenString[0])
                            {
                                case "g":
                                    break;
                                case "usemtl":
                                    break;
                                case "usemap":
                                    break;
                                case "mtllib":
                                    break;
                                case "v":
                                    mesh.Vertices[v] = new Vector3(Convert.ToSingle(brokenString[1]), Convert.ToSingle(brokenString[2]),
                                        Convert.ToSingle(brokenString[3]));
                                    v++;
                                    break;
                                case "vt":
                                    mesh.Uv[vt] = new Vector2(Convert.ToSingle(brokenString[1]), Convert.ToSingle(brokenString[2]));
                                    vt++;
                                    break;
                                case "vt1":
                                    mesh.Uv[vt1] = new Vector2(Convert.ToSingle(brokenString[1]), Convert.ToSingle(brokenString[2]));
                                    vt1++;
                                    break;
                                case "vt2":
                                    mesh.Uv[vt2] = new Vector2(Convert.ToSingle(brokenString[1]), Convert.ToSingle(brokenString[2]));
                                    vt2++;
                                    break;
                                case "vn":
                                    mesh.Normals[vn] = new Vector3(Convert.ToSingle(brokenString[1]), Convert.ToSingle(brokenString[2]),
                                        Convert.ToSingle(brokenString[3]));
                                    vn++;
                                    break;
                                case "vc":
                                    break;
                                case "f":

                                    Int32 j = 1;
                                    List<Int32> intArray = new List<Int32>();
                                    while (j < brokenString.Length && ("" + brokenString[j]).Length > 0)
                                    {
                                        Vector3 temp = new Vector3();
                                        String[] brokenBrokenString = brokenString[j].Split(splitIdentifier2, 3);
                                        temp.x = Convert.ToInt32(brokenBrokenString[0]);
                                        if (brokenBrokenString.Length > 1)                                  //Some .obj files skip UV and normal
                                        {
                                            if (brokenBrokenString[1] != "")                                    //Some .obj files skip the uv and not the normal
                                            {
                                                temp.y = Convert.ToInt32(brokenBrokenString[1]);
                                            }
                                            temp.z = Convert.ToInt32(brokenBrokenString[2]);
                                        }
                                        j++;

                                        mesh.FaceData[f2] = temp;
                                        intArray.Add(f2);
                                        f2++;
                                    }
                                    j = 1;
                                    while (j + 2 < brokenString.Length)     //Create triangles out of the face data.  There will generally be more than 1 triangle per face.
                                    {
                                        mesh.Triangles[f] = intArray[0];
                                        f++;
                                        mesh.Triangles[f] = intArray[j];
                                        f++;
                                        mesh.Triangles[f] = intArray[j + 1];
                                        f++;

                                        j++;
                                    }
                                    break;
                            }
                            currentText = reader.ReadLine();
                            currentText = currentText?.Replace("  ", " ");       //Some .obj files insert Double spaces, this removes them.
                        }
                    }
                }
            }
        }
    }
}
