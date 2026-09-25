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
namespace Kopernicus.Patches;

// Shared state for the space object (asteroid / comet) collider patches.
internal static class SpaceObjectColliderUtil
{
    // How far the collider may sit below the visible surface before rebuilding it from the
    // detailed mesh is worth the extra solids. See PSpaceObject_CreateCollider.
    //
    // Stock never reaches this. Asteroid prefab radii run PA_A 3.24m, PA_B 5.4m, PA_C 9m,
    // PA_D 15m, and PA_E through PA_I all 25m - the progression stops at class E - so the worst
    // stock case is a class E through I at ~31m after the 0.75-1.25 roll, which is 0.28m out.
    // Comet prefabs do keep scaling, to PC_I at 193m, but they use one sphere asset for all three
    // meshes and so have no error to correct and nothing to swap in.
    internal const float MaxColliderErrorMetres = 0.75f;

    // Physical radius of the space object currently being generated, or 0 outside a generation
    // pass. Set by ProceduralSpaceObject_Generate, which brackets the whole synchronous build.
    // This is the radius stock itself is building the meshes from, so it already accounts for
    // whatever resized the object.
    internal static float GeneratingRadius;

    // How far a triangulated sphere falls short of the surface it approximates, as a fraction of
    // the radius - the chordal deviation, or sagitta, of each flat facet against the curve it cuts
    // across. A geodesic sphere of V vertices has an angular vertex spacing of roughly 3.5/sqrt(V)
    // radians, and a chord subtending angle a has a sagitta of R*(1-cos(a/2)), or about R*a^2/8 at
    // these angles, so the shortfall is around 1.53/V of the radius. Across the stock sphere assets
    // that is 3.6% at L2 (42 verts), 0.94% at L3 (162), 0.24% at L4 (642) and 0.06% at L5 (2562).
    internal static float GetRelativeSagitta(int vertexCount)
    {
        return vertexCount > 0 ? 1.53f / vertexCount : 0f;
    }
}
