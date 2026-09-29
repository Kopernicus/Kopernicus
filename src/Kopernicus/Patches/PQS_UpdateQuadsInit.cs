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

// PQS.UpdateQuadsInit is the point where each quad gets its surfaceMaterial
// assigned. When PQS.useSharedMaterial is false (e.g. when PQSMod_MaterialQuadRelative
// is in use) Unity clones surfaceMaterial into a per-quad instance via
// MeshRenderer::set_material, so any texture writes that happen after this point
// only reach future quads, never the ones that were just assigned.
//
// PQSMod.OnSphereStart fires before UpdateQuadsInit on the first start, but the
// ActivateSphere path that runs when isStarted=true && isAlive=false skips
// OnSphereStart entirely and goes straight to UpdateQuadsInit. There is no stock
// callback that fires reliably in every path, so we synthesize one here.
[HarmonyPatch(typeof(PQS), "UpdateQuadsInit")]
internal static class PQS_UpdateQuadsInit
{
    static void Prefix(PQS __instance) =>
        Events.OnPQSSphereStartedPreInit.Fire(__instance);
}
