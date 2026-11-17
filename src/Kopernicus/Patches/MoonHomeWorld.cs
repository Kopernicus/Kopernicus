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

// Stock KSP assumes the home world orbits the star. These patches let it orbit a planet instead. KSP Retooled supports
// that by itself, so none of this is built for it.
#if !KSP_RETOOLED
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FinePrint.Contracts;
using HarmonyLib;
using KSP.UI;
using SentinelMission;
using UnityEngine;

namespace Kopernicus.Patches;

/// <summary>
/// SetSpaceCentre makes the home world the main body before it reverses the driver
/// </summary>
[HarmonyPatch(typeof(PSystemSetup), "SetSpaceCentre")]
static class PSystemSetup_SetSpaceCentre
{
    static void Postfix()
    {
        CelestialBody home = FlightGlobals.currentMainBody;
        if (home != null)
        {
            BodyHierarchy.ReverseChain(home);
        }
    }
}

[HarmonyPatch(typeof(Planetarium), "FindRootBody")]
static class Planetarium_FindRootBody
{
    static bool Prefix(CelestialBody cb, ref CelestialBody __result)
    {
        __result = BodyHierarchy.FindUpdateRoot(cb);
        return false;
    }
}

/// <summary>
/// Comet and sentinel distances are scaled by the home world's distance from the star, which stock gets from the
/// home world's own orbit. For a moon that is its orbit around the planet, so the planet's orbit is used instead.
/// </summary>
[HarmonyPatch]
static class HomeWorld_SolarOrbit
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        return MoonHomeWorld.MethodsReadingBodyOrbit(
            (typeof(CometManager), "GenerateHomeSpawnDistance"),
            (typeof(CometVessel), "OnStart"),
            (typeof(CometVessel), "UpdateVFXDimensions"),
            (typeof(CometSampleContract), "Generate"),
            (typeof(CometSampleContract), "GetSynopsys"),
            (typeof(SentinelScenario), "SpawnComet"));
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return MoonHomeWorld.ReplaceBodyOrbit(instructions, nameof(BodyHierarchy.SolarOrbit));
    }
}

/// <summary>
/// Warp to next morning works out the length of the solar day from the body's own orbit as its year. For a moon the
/// year is the planet's orbit. A body that turns once a year or slower keeps its sidereal day, which stock only
/// checked for in one of the three places.
/// </summary>
[HarmonyPatch]
static class UIWarpToNextMorning_SolarDay
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        return MoonHomeWorld.MethodsReadingBodyOrbit(
            (typeof(UIWarpToNextMorning), "Setup"),
            (typeof(UIWarpToNextMorning), "CalculateTimeToDaylightFromVessel"));
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return MoonHomeWorld.ReplaceBodyOrbit(instructions, nameof(BodyHierarchy.SolarDayOrbit));
    }
}

static class MoonHomeWorld
{
    private static readonly MethodInfo BodyOrbitGetter = AccessTools.PropertyGetter(typeof(CelestialBody), nameof(CelestialBody.orbit));

    /// <summary>
    /// Returns every overload of the named methods that reads <see cref="CelestialBody.orbit"/>. In all of them the
    /// only body whose orbit is read is the one the patch is about.
    /// </summary>
    public static IEnumerable<MethodBase> MethodsReadingBodyOrbit(params (Type type, string name)[] targets)
    {
        foreach ((Type type, string name) in targets)
        {
            bool found = false;
            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(type))
            {
                if (method.Name != name || !ReadsBodyOrbit(method))
                {
                    continue;
                }

                found = true;
                yield return method;
            }

            if (!found)
            {
                Debug.LogWarning("[Kopernicus] " + type.Name + "." + name + " does not read a body's orbit, so it was not patched for a home world that orbits a planet");
            }
        }
    }

    /// <summary>Replaces every read of <see cref="CelestialBody.orbit"/> with a call to a method of <see cref="BodyHierarchy"/>.</summary>
    public static IEnumerable<CodeInstruction> ReplaceBodyOrbit(IEnumerable<CodeInstruction> instructions, string replacementName)
    {
        MethodInfo replacement = AccessTools.Method(typeof(BodyHierarchy), replacementName, [typeof(CelestialBody)]);
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.Calls(BodyOrbitGetter))
            {
                // Changed in place so the labels and exception blocks of the instruction stay with it
                instruction.opcode = OpCodes.Call;
                instruction.operand = replacement;
            }

            yield return instruction;
        }
    }

    private static bool ReadsBodyOrbit(MethodBase method)
    {
        if (method.GetMethodBody() == null)
        {
            return false;
        }

        foreach (KeyValuePair<OpCode, object> instruction in PatchProcessor.ReadMethodBody(method))
        {
            if (instruction.Value is MethodInfo called && called == BodyOrbitGetter)
            {
                return true;
            }
        }

        return false;
    }
}
#endif
