using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace Kopernicus.Patches;

[HarmonyPatch(typeof(PQS), "UpdateVisual")]
internal static class PQS_UpdateVisual
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var oldMethod = SymbolExtensions.GetMethodInfo<PQS>(pqs => pqs.GetSurfaceHeight(Vector3d.zero));
        var newMethod = SymbolExtensions.GetMethodInfo<PQS>(pqs => GetSurfaceHeightIfNecessary(pqs, default));

        var matcher = new CodeMatcher(instructions);
        matcher
            .MatchStartForward(new CodeMatch(OpCodes.Call, oldMethod))
            .ThrowIfInvalid("Could not find call to PQS::GetSurfaceHeight")
            .SetInstruction(new CodeInstruction(OpCodes.Call, newMethod));

        return matcher.Instructions();
    }

    // PQS.UpdateVisual calls GetSurfaceHeight in lots of cases where it doesn't
    // really need the actual surface height because the planet is so far away it
    // just doesn't matter. However, calling this actually ends up slowing down
    // everything by quite a bit because it ends up loading heightmap textures
    // for every single body.
    //
    // To prevent this, we skip returning the height if the planet is far enough
    // away that it wasn't going to be enabled anyway.
    static double GetSurfaceHeightIfNecessary(PQS pqs, Vector3d radialVector)
    {
        if (pqs.targetAltitude <= pqs.maxDetailDistance * pqs.radius)
            return pqs.GetSurfaceHeight(radialVector);

        return 0.0;
    }
}
