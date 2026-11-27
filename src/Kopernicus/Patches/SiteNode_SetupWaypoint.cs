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
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace Kopernicus.Patches;

/// <summary>
/// Stock creates the KSC map waypoint on a body named "Kerbin". If the home world has another name, the waypoint
/// falls back to the Sun. Its map context menu then anchors to a point behind the camera and closes right away.
/// </summary>
[HarmonyPatch(typeof(SiteNode), "SetupWaypoint")]
static class SiteNode_SetupWaypoint
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);
        matcher
            .MatchStartForward(new CodeMatch(OpCodes.Ldstr, "Kerbin"))
            .ThrowIfInvalid("Could not find the \"Kerbin\" body name in SiteNode::SetupWaypoint")
            .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SiteNode_SetupWaypoint), nameof(HomeBodyName))));

        return matcher.Instructions();
    }

    static string HomeBodyName()
    {
        return FlightGlobals.GetHomeBody().GetName();
    }
}
