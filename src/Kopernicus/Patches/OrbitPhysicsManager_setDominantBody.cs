using HarmonyLib;

namespace Kopernicus.Patches;

/// <summary>
/// Clear the outgoing dominant body's inverseRotation so that only the current dominant body
/// is ever in the rotating frame. The SOI transition has already re-expressed vessel state in
/// the new reference body's frame, so this has no velocity side effect.
/// </summary>
[HarmonyPatch(typeof(OrbitPhysicsManager), "setDominantBody")]
static class OrbitPhysicsManager_setDominantBody
{
    static void Prefix(OrbitPhysicsManager __instance, CelestialBody body)
    {
        CelestialBody outgoing = __instance.dominantBody;
        if (outgoing != null && outgoing != body)
        {
            outgoing.inverseRotation = false;
        }
    }
}
