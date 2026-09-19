using System.Collections.Generic;
using HarmonyLib;

namespace Kopernicus.Patches;

// Stock links each triangle to its edge neighbours with a nested scan over every other triangle,
// on the main thread, per object. It is cheap enough at L3 (320 tris) and L4 (1280 tris) but
// for anything higher the perf impact starts to become problematic.
// An edge dictionary gives the same neighbour assignments in linear time, so the swap does not have to pay for it.
[HarmonyPatch(typeof(SpaceObjectCollider), "LinkChunks")]
static class SpaceObjectCollider_LinkChunks
{
    static bool Prefix(SpaceObjectCollider.Chunk[] chunks)
    {
        int count = chunks.Length;
        Dictionary<long, int> edgeOwners = new Dictionary<long, int>(count * 2);
        for (int i = 0; i < count; i++)
        {
            LinkChunkEdge(chunks, edgeOwners, chunks[i].i0, chunks[i].i1, i);
            LinkChunkEdge(chunks, edgeOwners, chunks[i].i1, chunks[i].i2, i);
            LinkChunkEdge(chunks, edgeOwners, chunks[i].i0, chunks[i].i2, i);
        }
        return false;
    }

    private static void LinkChunkEdge(SpaceObjectCollider.Chunk[] chunks, Dictionary<long, int> edgeOwners,
        int vA, int vB, int index)
    {
        long edgeKey = GetEdgeKey(vA, vB);
        if (edgeOwners.TryGetValue(edgeKey, out int owner))
        {
            // SetNeighbour resolves the unordered pair against each chunk's own winding, so the
            // same pair sets the correct slot on both sides.
            chunks[index].SetNeighbour(vA, vB, chunks[owner]);
            chunks[owner].SetNeighbour(vA, vB, chunks[index]);
            edgeOwners.Remove(edgeKey);
        }
        else
        {
            edgeOwners.Add(edgeKey, index);
        }
    }

    private static long GetEdgeKey(int vA, int vB)
    {
        if (vA >= vB)
        {
            int swap = vA;
            vA = vB;
            vB = swap;
        }
        return ((long)vA << 32) + vB;
    }
}
