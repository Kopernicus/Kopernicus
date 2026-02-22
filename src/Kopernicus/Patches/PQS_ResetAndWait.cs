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

using System.Collections;
using HarmonyLib;
using Kopernicus.RuntimeUtility;
using UnityEngine;

namespace Kopernicus.Patches;

// This delays ResetAndWaitCoroutine until after the current scene switch has
// completed. The reason why we need to do this involves asset bundles and how
// they are loaded.
//
// While the main thread is blocked loading an asset from an asset bundle unity
// will opportunistically run other work while it is waiting. When switching
// between vessels in flight mode the StartSphere called by ResetAndWaitCoroutine
// is perfectly timed to have the entire scene load run in the middle.
//
// Doing this ends up breaking KSP pretty hard. A bunch of objects related to the
// current scene haven't been destroyed yet, so the new singletons wake up, see
// that there is still another live instance, and then destroy themselves. This
// breaks a whole bunch of stuff, which makes it somewhat confusing to debug.
//
// By delaying the call to StartSphere until the scene switch is completed we
// can avoid this problem.
[HarmonyPatch(typeof(PQS), "ResetAndWaitCoroutine")]
internal static class PQS_ResetAndWait
{
    static bool SceneSwitchInProgress => RuntimeUtility.RuntimeUtility.SceneSwitchInProgress;

    static IEnumerator Postfix(IEnumerator __result, PQS __instance) =>
        ResetAndWaitCoroutine(__instance, __result);

    // This needs to be calleed ResetAndWaitCoroutine so that PQS calling
    // StopCoroutine("ResetAndWaitCoroutine") works as expected.
    static IEnumerator ResetAndWaitCoroutine(PQS pqs, IEnumerator coroutine)
    {
        pqs.Mod_OnSphereStart();
        while (!pqs.isAlive && !pqs.isDisabled)
        {
            yield return null;
            pqs.Mod_OnSphereStart();
        }

        if (SceneSwitchInProgress)
        {
            Debug.Log($"[Kopernicus] Delaying ResetAndWait of body {pqs.name} until scene switch is complete");
            yield return WaitUntilSceneSwitchComplete.Instance;
        }

        pqs.StartSphere(force: false);
    }

    class WaitUntilSceneSwitchComplete : CustomYieldInstruction
    {
        public static readonly WaitUntilSceneSwitchComplete Instance = new();

        public override bool keepWaiting => SceneSwitchInProgress;
    }
}
