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
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace Kopernicus.Patches;

// This patch plugs a race condition that causes the PQS.UpdateSphere coroutine
// to never be spawned, leaving a planet as fully "low-res" with no collisions
// appearing. The root cause is that the old PQS.ResetAndWaitCoroutine instance
// sticks around and runs StartSphere before something sets PQS.isAlive to true.
//
// This poisons the PQS state so that the "real" instance of ResetAndWaitCoroutine
// never ends up starting the PQS.UpdateSphere coroutine.
//
// In stock this is only a performance issue because this happens before the scene
// switch starts, but the PQS_ResetAndWait_MoveNext patch delays the old coroutine
// until after the scene switch which allows this race condition to surface.
//
// KSPCF has a similar patch which fixes this, so we defer to that patch when
// KSPCF is installed.

[HarmonyPatch(typeof(PQS), nameof(PQS.ResetAndWait))]
internal static class PQS_ResetAndWait
{
    // KSPCF already has this patch. The PQSCoroutineLeaks patch also has some
    // other fixes so defer to that patch if KSPCF is installed.
    static bool Prepare() =>
        !AssemblyLoader.loadedAssemblies.Contains("KSPCommunityFixes");

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var m_ResetAndWaitCoroutine = SymbolExtensions.GetMethodInfo<PQS>(p => p.ResetAndWaitCoroutine());
        var m_StartCoroutine_String = SymbolExtensions.GetMethodInfo<MonoBehaviour>(b => b.StartCoroutine(""));
        var m_StartCoroutine_IEnumerator = SymbolExtensions.GetMethodInfo<MonoBehaviour>(b => b.StartCoroutine(default(IEnumerator)));

        // We go from
        //
        // ldarg.0
        // ldarg.0
        // call     PQS::ResetAndWaitCoroutine
        // dup                                  ; only if obfuscated
        // pop
        // call     MonoBehaviour::StartCoroutine(IEnumerator)
        //
        // to
        //
        // ldarg.0
        // ldarg.0
        // pop
        // ldstr    "ResetAndWaitCoroutine"
        // dup
        // pop
        // call     MonoBehaviour::StartCoroutine(string)

        var matcher = new CodeMatcher(instructions);
        matcher
            .MatchStartForward(new CodeMatch(OpCodes.Call, m_ResetAndWaitCoroutine))
            .RemoveInstruction()
            .Insert(
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldstr, "ResetAndWaitCoroutine")
            )
            .MatchStartForward(new CodeMatch(OpCodes.Call, m_StartCoroutine_IEnumerator))
            .SetOperandAndAdvance(m_StartCoroutine_String);

        return matcher.Instructions();
    }
}