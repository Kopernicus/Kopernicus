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
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Kopernicus.Patches;

[HarmonyPatch(typeof(PQSLandControl), "OnVertexBuildHeight")]
static class PQSLandControl_OnVertexBuildHeight
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        FieldInfo PQSMod_sphere_field = AccessTools.Field(typeof(PQSMod), nameof(PQSMod.sphere));
        FieldInfo PQS_sx_field = AccessTools.Field(typeof(PQS), nameof(PQS.sx));
        MethodInfo GetLongitudeFromSX_method = AccessTools.Method(typeof(PQSLandControl_OnVertexBuildHeight), nameof(GetLongitudeFromSX));

        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        for (int i = 0; i < code.Count - 1; i++)
        {
            if (code[i].opcode == OpCodes.Ldfld && ReferenceEquals(code[i].operand, PQSMod_sphere_field)
                                                && code[i + 1].opcode == OpCodes.Ldfld && ReferenceEquals(code[i + 1].operand, PQS_sx_field))
            {
                code[i + 1].opcode = OpCodes.Call;
                code[i + 1].operand = GetLongitudeFromSX_method;
            }
        }

        return code;
    }

    /// <summary>
    /// Transform the from the sx [-0.25, 0.75] longitude range convention where [-0.25, 0] maps to [270°, 360°]
    /// and [0, 0.75] maps to [0°, 270°] into a linear [0,1] longitude range.
    /// </summary>
    public static double GetLongitudeFromSX(PQS sphere)
    {
        if (sphere.sx < 0.0)
            return sphere.sx + 1.0;
        return sphere.sx;
    }
}
