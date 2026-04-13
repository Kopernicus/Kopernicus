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

using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
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
[HarmonyPatch]
internal static class PQS_ResetAndWait
{
    static bool SceneSwitchInProgress => RuntimeUtility.RuntimeUtility.SceneSwitchInProgress;
    const int CustomState = 12331;

    static MethodBase TargetMethod() =>
        AccessTools.EnumeratorMoveNext(SymbolExtensions.GetMethodInfo<PQS>(pqs => pqs.ResetAndWaitCoroutine()));

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
    {
        // KSPCF patches ResetAndWait to start the coroutine by its string name.
        // As far as I can tell this actually makes it so our patch is never
        // called. Instead of trying to debug this too hard we just take the
        // approach here of patching a new state into the state machine for the
        // returned enumerator. Nobody else is crazy enough to actually do that
        // so we can know we're safe from being tampered with here.

        var moveNext = AccessTools.EnumeratorMoveNext(SymbolExtensions.GetMethodInfo<PQS>(pqs => pqs.ResetAndWaitCoroutine()));
        var type = moveNext.DeclaringType;

        var state = AccessTools.Field(type, "<>1__state");
        var current = AccessTools.Field(type, "<>2__current");
        var pqs = AccessTools.Field(type, "<>4__this");

        var currentLocal = gen.DeclareLocal(typeof(object));
        var customResumeLabel = gen.DefineLabel();
        var skipYieldLabel = gen.DefineLabel();

        var matcher = new CodeMatcher(instructions, gen);

        // Add CustomState to the state dispatch.
        // Find the first ldc.i4.0 + ret (the default "return false" for unknown states).
        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldc_I4_0),
            new CodeMatch(OpCodes.Ret));

        matcher.Insert(
            new CodeInstruction(OpCodes.Ldloc_0),
            new CodeInstruction(OpCodes.Ldc_I4, CustomState),
            new CodeInstruction(OpCodes.Beq, customResumeLabel));

        // Insert scene switch check before StartSphere.
        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(PQS), "StartSphere")));

        matcher.Advance(-2);

        var existingLabels = new List<Label>(matcher.Labels);
        matcher.Labels.Clear();
        matcher.AddLabels(new[] { skipYieldLabel });

        matcher.InsertAndAdvance([
            // CheckSceneSwitch(pqs, out current)
            new CodeInstruction(OpCodes.Ldarg_0).WithLabels(existingLabels),
            new CodeInstruction(OpCodes.Ldfld, pqs),
            new CodeInstruction(OpCodes.Ldloca, currentLocal),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PQS_ResetAndWait), nameof(CheckSceneSwitch))),
            new CodeInstruction(OpCodes.Brfalse, skipYieldLabel),

            // yield return current
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldloc, currentLocal),
            new CodeInstruction(OpCodes.Stfld, current),
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldc_I4, CustomState),
            new CodeInstruction(OpCodes.Stfld, state),
            new CodeInstruction(OpCodes.Ldc_I4_1),
            new CodeInstruction(OpCodes.Ret),

            // Resume from CustomState: set state to -1 and fall through to StartSphere
            new CodeInstruction(OpCodes.Ldarg_0).WithLabels(customResumeLabel),
            new CodeInstruction(OpCodes.Ldc_I4_M1),
            new CodeInstruction(OpCodes.Stfld, state)
        ]);

        return matcher.Instructions();
    }

    static bool CheckSceneSwitch(PQS pqs, out object current)
    {
        if (!SceneSwitchInProgress)
        {
            current = null;
            return false;
        }

        Debug.Log($"[Kopernicus] Delaying ResetAndWait of body {pqs.name} until scene switch is complete");
        current = WaitUntilSceneSwitchComplete.Instance;
        return true;
    }

    class WaitUntilSceneSwitchComplete : CustomYieldInstruction
    {
        public static readonly WaitUntilSceneSwitchComplete Instance = new();

        public override bool keepWaiting => SceneSwitchInProgress;
    }
}
