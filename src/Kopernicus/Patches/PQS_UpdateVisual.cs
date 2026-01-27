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

    // PQS.Start calls GetSurfaceHeight for every body in the system, this includes ones that
    // have no conceivable chance of being near the camera. This ends up being incredibly
    // slow because it is necessary to load the heightmaps for all the planets in the system.
    //
    // This is a modified version of GetSurfaceHeight that skips returning the height if we're
    // still in the loading screen and the planet is far enough away that it wouldn't be
    // subdivided anyway.
    //
    // UpdateVisual gets called every frame during regular gameplay anyways, so this has no
    // effect on anything outside of the main menu scene.
    static double GetSurfaceHeightIfNecessary(PQS pqs, Vector3d radialVector)
    {
        if (HighLogic.LoadedScene != GameScenes.LOADING)
            return pqs.GetSurfaceHeight(radialVector);

        if (pqs.targetAltitude <= pqs.maxDetailDistance * pqs.radius)
            return pqs.GetSurfaceHeight(radialVector);

        return 0.0;
    }
}
