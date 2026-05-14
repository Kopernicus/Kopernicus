using HarmonyLib;

namespace Kopernicus.Patches;

// PQS.UpdateQuadsInit is the point where each quad gets its surfaceMaterial
// assigned. When PQS.useSharedMaterial is false (e.g. when PQSMod_MaterialQuadRelative
// is in use) Unity clones surfaceMaterial into a per-quad instance via
// MeshRenderer::set_material, so any texture writes that happen after this point
// only reach future quads, never the ones that were just assigned.
//
// PQSMod.OnSphereStart fires before UpdateQuadsInit on the first start, but the
// ActivateSphere path that runs when isStarted=true && isAlive=false skips
// OnSphereStart entirely and goes straight to UpdateQuadsInit. There is no stock
// callback that fires reliably in every path, so we synthesize one here.
[HarmonyPatch(typeof(PQS), "UpdateQuadsInit")]
internal static class PQS_UpdateQuadsInit
{
    static void Prefix(PQS __instance) =>
        Events.OnPQSSphereStartedPreInit.Fire(__instance);
}
