using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Kopernicus.Patches;

// Each cluster of surface triangles is closed into a solid by adding a single apex beneath the
// surface, and the result is cooked as a convex hull. That apex has to sit below every surface
// vertex in the cluster for the hull to have any depth. Stock searches for the lowest one, but
// the comparison in that search tests an unrelated loop counter instead of the radius being
// compared, so the search never narrows: the apex depth ends up taken from the centroid of
// whichever triangle happens to be first in the cluster.
//
// On a surface whose radius varies by tens of percent - which is every procedural asteroid - that
// centroid is often higher than the cluster's own lowest vertices. The apex then falls inside
// their hull, contributes no depth, and the solid cooks as a zero-thickness sheet that a vessel
// passes straight through. On level-5 geometry matching a 1.2km asteroid, roughly 15% of solids
// came out degenerate this way.
//
// Taking the minimum over the cluster's actual surface vertices restores the intent and gives
// every solid real inward thickness. Two smaller fixes ride along: the closing triangle is
// written at the three slots reserved for it rather than at an index that lands inside the
// triangle list, and the vertex deduplication runs against a dictionary instead of the stock
// AddUnique/IndexOf pair, which is quadratic in the cluster size.
[HarmonyPatch(typeof(SpaceObjectCollider.CompositeSolid), "BuildCompositeMesh")]
static class SpaceObjectCollider_CompositeSolid_BuildCompositeMesh
{
    static bool Prefix(SpaceObjectCollider.CompositeSolid __instance, Vector3[] srcVerts, Vector3[] srcNormals,
        Color c)
    {
        List<SpaceObjectCollider.Chunk> chunks = __instance.chunks;
        List<int> sourceIndices = new List<int>();
        Dictionary<int, int> localIndices = new Dictionary<int, int>();

        int[] tris = chunks.Count == 1 ? new int[12] : new int[chunks.Count * 3 + 3];
        int write = 0;
        for (int i = 0; i < chunks.Count; i++)
        {
            tris[write] = MapVertex(sourceIndices, localIndices, chunks[i].i0);
            tris[write + 1] = MapVertex(sourceIndices, localIndices, chunks[i].i1);
            tris[write + 2] = MapVertex(sourceIndices, localIndices, chunks[i].i2);
            write += 3;
        }

        Vector3[] verts;
        Vector3[] normals;
        if (chunks.Count == 1)
        {
            verts = new Vector3[4]
            {
                srcVerts[sourceIndices[0]],
                srcVerts[sourceIndices[1]],
                srcVerts[sourceIndices[2]],
                chunks[0].srfRadial
            };
            verts[3] *= Mathf.Sqrt(Mathf.Min(Mathf.Min(verts[0].sqrMagnitude, verts[1].sqrMagnitude),
                verts[2].sqrMagnitude)) * 0.95f;
            normals = new Vector3[4]
            {
                srcNormals[sourceIndices[0]],
                srcNormals[sourceIndices[1]],
                srcNormals[sourceIndices[2]],
                -chunks[0].srfRadial
            };
            tris[write] = 3;
            tris[write + 1] = 0;
            tris[write + 2] = 1;
            write += 3;
            tris[write] = 3;
            tris[write + 1] = 1;
            tris[write + 2] = 2;
            write += 3;
            tris[write] = 3;
            tris[write + 1] = 2;
            tris[write + 2] = 0;
        }
        else
        {
            verts = new Vector3[sourceIndices.Count + 1];
            normals = new Vector3[sourceIndices.Count + 1];
            float minSqrMagnitude = float.MaxValue;
            for (int j = 0; j < verts.Length - 1; j++)
            {
                verts[j] = srcVerts[sourceIndices[j]];
                normals[j] = srcNormals[sourceIndices[j]];
                minSqrMagnitude = Mathf.Min(minSqrMagnitude, verts[j].sqrMagnitude);
            }
            Vector3 srfRadial = __instance.GetSrfRadial();
            verts[verts.Length - 1] = srfRadial * Mathf.Sqrt(minSqrMagnitude) * 0.95f;
            normals[normals.Length - 1] = -srfRadial;

            // Stock closes the hull at index verts.Length - 1, which is inside the triangle list
            // it just wrote. The three reserved slots are at chunks.Count * 3.
            write = chunks.Count * 3;
            tris[write] = verts.Length - 1;
            tris[write + 1] = 0;
            tris[write + 2] = 1;
        }

        __instance.tris = tris;
        __instance.verts = verts;
        __instance.normals = normals;
        __instance.color = c;

        if (verts.Length <= 3)
        {
            Debug.LogError($"Invalid Solid: {verts.Length} defined, but at least 4 are required.");
            return false;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.normals = normals;
        mesh.triangles = tris;
        mesh.RecalculateBounds();
        __instance.mesh = mesh;
        return false;
    }

    // Reproduces AddUnique followed by IndexOf, in first-seen order, without the quadratic scan.
    private static int MapVertex(List<int> sourceIndices, Dictionary<int, int> localIndices, int vertex)
    {
        if (localIndices.TryGetValue(vertex, out int local))
        {
            return local;
        }
        local = sourceIndices.Count;
        sourceIndices.Add(vertex);
        localIndices.Add(vertex, local);
        return local;
    }
}
