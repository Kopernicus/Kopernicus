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
