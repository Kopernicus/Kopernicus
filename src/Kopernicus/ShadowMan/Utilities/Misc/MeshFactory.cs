using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace Kopernicus.ShadowMan
{
    public static class MeshFactory
    {
        public enum PLANE { XY, XZ, YZ };

        public static Mesh MakePlane(int w, int h, PLANE plane, bool _01, bool cw, float UVoffset = 0f, float UVscale = 1f)
        {
            Vector3[] vertices = new Vector3[w*h];
            Vector2[] texcoords = new Vector2[w*h];
            Vector3[] normals = new Vector3[w*h];
            int[] indices = new int[w*h*6];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Vector2 uv = new Vector3((float)x / (float)(w-1), (float)y / (float)(h-1));

                    uv.y *= UVscale;
                    uv.y += UVoffset;

                    Vector2 p = new Vector2();

                    if (_01)
                        p = uv;
                    else
                    {
                        p.x = (uv.x - 0.5f) * 2.0f;
                        p.y = (uv.y - 0.5f) * 2.0f;
                    }

                    Vector3 pos, norm;

                    switch ((int)plane)
                    {
                        case (int)PLANE.XY:
                            pos = new Vector3(p.x, p.y, 0.0f);
                            norm = new Vector3(0.0f, 0.0f, 1.0f);
                            break;

                        case (int)PLANE.XZ:
                            pos = new Vector3(p.x, 0.0f, p.y);
                            norm = new Vector3(0.0f, 1.0f, 0.0f);
                            break;

                        case (int)PLANE.YZ:
                            pos = new Vector3(0.0f, p.x, p.y);
                            norm = new Vector3(1.0f, 0.0f, 0.0f);
                            break;

                        default:
                            pos = new Vector3(p.x, p.y, 0.0f);
                            norm = new Vector3(0.0f, 0.0f, 1.0f);
                            break;
                    }

                    texcoords[x + y * w] = uv;
                    vertices[x + y * w] = pos;
                    normals[x + y * w] = norm;
                }
            }

            int num = 0;
            for (int x = 0; x < w - 1; x++)
            {
                for (int y = 0; y < h - 1; y++)
                {
                    if (cw)
                    {
                        indices[num++] = x + y * w;
                        indices[num++] = x + (y + 1) * w;
                        indices[num++] = (x + 1) + y * w;

                        indices[num++] = x + (y + 1) * w;
                        indices[num++] = (x + 1) + (y + 1) * w;
                        indices[num++] = (x + 1) + y * w;
                    }
                    else
                    {
                        indices[num++] = x + y * w;
                        indices[num++] = (x + 1) + y * w;
                        indices[num++] = x + (y + 1) * w;

                        indices[num++] = x + (y + 1) * w;
                        indices[num++] = (x + 1) + y * w;
                        indices[num++] = (x + 1) + (y + 1) * w;
                    }
                }
            }

            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.uv = texcoords;
            mesh.triangles = indices;
            mesh.normals = normals;

            return mesh;
        }

        public static Mesh MakePlaneWithFrustumIndexes() //basically the same above for w=2, h=2, XY, false, false + frustum corner indexes stored in the vertexes
                                                         //the frustum corner indexes later allow to reconstruct world viewdir inexpensively in the shader
                                                         //by passing the frustum corners directions in world space
        {

            Vector3[] vertices = new Vector3[4];
            Vector2[] texcoords = new Vector2[4];
            int[] indices = new int[6];

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    Vector2 uv = new Vector3((float)x , (float)y);
                    Vector2 p = new Vector2();

                    p.x = (uv.x - 0.5f) * 2.0f;
                    p.y = (uv.y - 0.5f) * 2.0f;

                    Vector3 pos = new Vector3(p.x, p.y, (float)(x + 2*y));

                    texcoords[x + y * 2] = uv;
                    vertices[x + y * 2] = pos;
                }
            }

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 2;
            indices[4] = 1;
            indices[5] = 3;

            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.uv = texcoords;
            mesh.triangles = indices;

            return mesh;
        }

        //Makes a plane with 32bit index format, for meshes bigger than 256*256
        public static Mesh MakePlane32BitIndexFormat(int w, int h, PLANE plane, bool _01, bool cw, float UVoffset = 0f, float UVscale = 1f)
        {

            Vector3[] vertices = new Vector3[w*h];
            Vector2[] texcoords = new Vector2[w*h];
            int[] indices = new int[w*h*6];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Vector2 uv = new Vector3((float)x / (float)(w-1), (float)y / (float)(h-1));

                    uv.y *= UVscale;
                    uv.y += UVoffset;

                    Vector2 p = new Vector2();

                    if (_01)
                        p = uv;
                    else
                    {
                        p.x = (uv.x - 0.5f) * 2.0f;
                        p.y = (uv.y - 0.5f) * 2.0f;
                    }

                    Vector3 pos;

                    switch ((int)plane)
                    {
                        case (int)PLANE.XY:
                            pos = new Vector3(p.x, p.y, 0.0f);
                            break;

                        case (int)PLANE.XZ:
                            pos = new Vector3(p.x, 0.0f, p.y);
                            break;

                        case (int)PLANE.YZ:
                            pos = new Vector3(0.0f, p.x, p.y);
                            break;

                        default:
                            pos = new Vector3(p.x, p.y, 0.0f);
                            break;
                    }

                    texcoords[x + y * w] = uv;
                    vertices[x + y * w] = pos;
                }
            }

            int num = 0;
            for (int x = 0; x < w - 1; x++)
            {
                for (int y = 0; y < h - 1; y++)
                {
                    if (cw)
                    {
                        indices[num++] = x + y * w;
                        indices[num++] = x + (y + 1) * w;
                        indices[num++] = (x + 1) + y * w;

                        indices[num++] = x + (y + 1) * w;
                        indices[num++] = (x + 1) + (y + 1) * w;
                        indices[num++] = (x + 1) + y * w;
                    }
                    else
                    {
                        indices[num++] = x + y * w;
                        indices[num++] = (x + 1) + y * w;
                        indices[num++] = x + (y + 1) * w;

                        indices[num++] = x + (y + 1) * w;
                        indices[num++] = (x + 1) + y * w;
                        indices[num++] = (x + 1) + (y + 1) * w;
                    }
                }
            }

            Mesh mesh = new Mesh();

            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(new List<Vector3>(vertices));
            mesh.SetUVs(0, new List<Vector2>(texcoords));
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);


            return mesh;
        }
    }
}

