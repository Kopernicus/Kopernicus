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
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace Kopernicus.Patches;

/// <summary>
/// Stock sets the rotation period of a tidally locked body to its orbital period on every update
/// and it is always positive. For orbits with an inclination above 90 degrees the rotation has to
/// be negative as otherwise the body spins against its orbit and does not stay locked.
/// </summary>
[HarmonyPatch(typeof(CelestialBody), nameof(CelestialBody.CBUpdate))]
internal static class CelestialBody_CBUpdate
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var periodField = AccessTools.Field(typeof(Orbit), nameof(Orbit.period));
        var rotationPeriodField = AccessTools.Field(typeof(CelestialBody), nameof(CelestialBody.rotationPeriod));
        var signedPeriod = AccessTools.Method(typeof(CelestialBody_CBUpdate), nameof(TidallyLockedRotationPeriod));

        // rotationPeriod = orbit.period;
        var matcher = new CodeMatcher(instructions);
        matcher
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldfld, periodField),
                new CodeMatch(OpCodes.Stfld, rotationPeriodField))
            .ThrowIfInvalid("Could not find tidally locked rotation period assignment in CelestialBody::CBUpdate")
            .Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, signedPeriod));

        return matcher.Instructions();
    }

    static Double TidallyLockedRotationPeriod(Double orbitalPeriod, CelestialBody body)
    {
        return Utility.SignedOrbitalPeriod(body, orbitalPeriod);
    }
}
