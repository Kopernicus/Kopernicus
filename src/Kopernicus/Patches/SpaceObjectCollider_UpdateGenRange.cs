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

// The stock per-frame budgets are fixed item counts tuned for objects that decompose into a few
// dozen solids. The solid count follows the triangle count, not the object's size, so anything
// built from the 5120-triangle L5 mesh produces thousands of them however big it is - and at long
// range stock would trickle those out one collider per frame for the better part of a minute, so
// the object is still generating when the vessel arrives and there is nothing there to hit.
//
// Taking the larger of the stock budget and a work-proportional one bounds the total frame count
// without ever generating slower than stock.
[HarmonyPatch(typeof(SpaceObjectCollider), "UpdateGenRange")]
static class SpaceObjectCollider_UpdateGenRange
{
    // Worst-case number of frames a full generation pass is allowed to spread over.
    private const float MaxGenFrames = 120f;

    static void Postfix(SpaceObjectCollider __instance)
    {
        // All zero means the object is close enough to generate in one frame without yielding.
        if (__instance.marchStepsPerFrame == 0 && __instance.meshGensPerFrame == 0 &&
            __instance.colliderGensPerFrame == 0)
        {
            return;
        }

        float frames = Mathf.Max(1f, Mathf.Lerp(1f, MaxGenFrames, __instance.rangeScale));

        // The first call happens before either collection exists, in which case there is nothing
        // to scale against yet and the stock budgets stand.
        int chunkCount = __instance.chunks != null ? __instance.chunks.Length : 0;
        int solidCount = __instance.solids != null ? __instance.solids.Count : 0;

        __instance.marchStepsPerFrame =
            Mathf.Max(__instance.marchStepsPerFrame, Mathf.CeilToInt(chunkCount / frames));
        __instance.meshGensPerFrame =
            Mathf.Max(__instance.meshGensPerFrame, Mathf.CeilToInt(solidCount / frames));
        __instance.colliderGensPerFrame =
            Mathf.Max(__instance.colliderGensPerFrame, Mathf.CeilToInt(solidCount / frames));
    }
}
