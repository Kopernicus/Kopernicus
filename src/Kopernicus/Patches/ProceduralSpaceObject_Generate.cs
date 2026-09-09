using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Kopernicus.Patches;

// Records the physical radius of the space object being generated so the collider patches can see
// how big it actually is.
[HarmonyPatch]
static class ProceduralSpaceObject_Generate
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(ProceduralAsteroid), nameof(ProceduralAsteroid.Generate));
        yield return AccessTools.Method(typeof(ProceduralComet), nameof(ProceduralComet.Generate));
    }

    static void Prefix(float radius)
    {
        SpaceObjectColliderUtil.GeneratingRadius = radius;
    }

    static void Finalizer()
    {
        SpaceObjectColliderUtil.GeneratingRadius = 0f;
    }
}
