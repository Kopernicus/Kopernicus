using HarmonyLib;

namespace Kopernicus.Patches;

// StartSphere sets isActive = false, then calls UpdateQuadsInit, which builds the quadtree and
// makes its leaves visible.
//
// PQSMod_CelestialBodyTransform.OnPreUpdate is intended to hide them on the next update,
// but neither of its branches is reachable when a scene begins above deactivateAltitude:
//
//   deactivate   `else if (sphere.isActive)`            already false
//   activate     visibleAltitude < deactivateAltitude   also false
//
// DeactivateSphere carries the same `if (isActive)` guard, so SetVisible(false) never runs either.
// The quads stay enabled at any altitude, frozen at the partial quadtree UpdateQuadsInit built.

[HarmonyPatch(typeof(PQS), "StartSphere")]
internal static class PQS_StartSphere
{
    static void Postfix(PQS __instance)
    {
        if (!HighLogic.LoadedSceneIsFlight || __instance.isActive)
            return;

        HideQuads(__instance);
    }

    // Reproduces the private PQS.SetVisible(false). Child spheres carry the ocean.
    static void HideQuads(PQS pqs)
    {
        if (pqs == null)
            return;

        if (pqs.quads != null)
        {
            foreach (PQ quad in pqs.quads)
            {
                quad?.SetMasterInvisible();
            }
        }

        if (pqs.ChildSpheres != null)
        {
            foreach (PQS child in pqs.ChildSpheres)
            {
                HideQuads(child);
            }
        }
    }
}
