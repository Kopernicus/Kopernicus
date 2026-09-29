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

namespace Kopernicus.Patches;

// StartSphere sets isActive = false, then calls UpdateQuadsInit, which builds the quadtree and
// makes its leaves visible.
//
// PQSMod_CelestialBodyTransform.OnPreUpdate is intended to hide them on the next update,
// but neither of its branches is reachable when a scene begins above deactivateAltitude:
//
//   deactivate   `else if (sphere.isActive)`            already false
//   activate     visibleAltitude < deactivateAltitude   also false
//
// DeactivateSphere carries the same `if (isActive)` guard, so SetVisible(false) never runs either.
// The quads stay enabled at any altitude, frozen at the partial quadtree UpdateQuadsInit built.

[HarmonyPatch(typeof(PQS), "StartSphere")]
internal static class PQS_StartSphere
{
    static void Postfix(PQS __instance)
    {
        if (!HighLogic.LoadedSceneIsFlight || __instance.isActive)
            return;

        HideQuads(__instance);
    }

    // Reproduces the private PQS.SetVisible(false). Child spheres carry the ocean.
    static void HideQuads(PQS pqs)
    {
        if (pqs == null)
            return;

        if (pqs.quads != null)
        {
            foreach (PQ quad in pqs.quads)
            {
                quad?.SetMasterInvisible();
            }
        }

        if (pqs.ChildSpheres != null)
        {
            foreach (PQS child in pqs.ChildSpheres)
            {
                HideQuads(child);
            }
        }
    }
}
