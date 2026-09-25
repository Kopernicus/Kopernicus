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
using HarmonyLib;
using UnityEngine;

namespace Kopernicus.Patches;

// Asteroid and comet colliders are built by decomposing a mesh into convex solids. Stock feeds
// that decomposition the crude convexSphere proxy while discarding the detailed colliderSphere
// mesh it generated moments earlier - PSpaceObject.Setup hands CreateCollider the convex mesh,
// and CreateConvexCollider, which is what the proxy was built for, is never called at all.
//
// The resulting mismatch scales with the object. At stock asteroid sizes it is centimetres, but
// on a kilometre-scale asteroid the same relative error is tens of metres of collider sitting
// below the visible surface, so a vessel touches down on nothing.
//
// Both how much the swap gains and what it costs vary by class, because the sphere assets are not
// shared across prefabs. PA_A pairs an L2 proxy (42 verts) with an L3 collider (162), PA_B and
// PA_C pair L3 with L4 (642), and PA_D through PA_I pair L3 with L5 (2562). So class A starts from
// a proxy four times coarser than everyone else's - 2.7% of radius out against 0.7-0.9% - while
// being the cheapest to fix, at 4x the triangles where classes D and up cost 16x.
//
// Patching CreateCollider rather than Setup keeps convexColliderMesh pointing at the proxy, so
// the proxy is still released by OnDestroy and ModuleComet still reads the bounds it expects.
[HarmonyPatch(typeof(PSpaceObject), "CreateCollider")]
static class PSpaceObject_CreateCollider
{
    static void Prefix(PSpaceObject __instance, ref Mesh colliderMesh)
    {
        // Stock builds no collider at all from a null mesh; don't conjure one.
        if (colliderMesh == null)
        {
            return;
        }

        // Setup assigns both fields before reaching CreateCollider. Every stock prefab sets
        // colliderSphere, but a modded one need not, and the visual mesh is then the most
        // detailed thing available. From class D up colliderSphere is the visualSphere asset
        // anyway, so stock has already collapsed the two.
        Mesh source = __instance.colliderMesh != null ? __instance.colliderMesh : __instance.visualMesh;
        if (source == null)
        {
            return;
        }

        // Comets fall out here rather than needing a special case: PC_A through PC_I use one L5
        // asset for visualSphere, colliderSphere and convexSphere alike, and the optimizeCollider
        // path assigns the optimized mesh to both fields, so proxy and source are equally detailed
        // and the gain is zero.
        float gain = SpaceObjectColliderUtil.GetRelativeSagitta(colliderMesh.vertexCount)
                     - SpaceObjectColliderUtil.GetRelativeSagitta(source.vertexCount);
        if (gain * SpaceObjectColliderUtil.GeneratingRadius < SpaceObjectColliderUtil.MaxColliderErrorMetres)
        {
            return;
        }

        colliderMesh = source;
    }
}
