using System;
using HarmonyLib;

namespace Kopernicus.Patches;

[HarmonyPatch(typeof(MapSO), "ConstructBilinearCoords", [typeof(double), typeof(double)])]
public static class MapSOPatch_Double
{
    private static bool Prefix(MapSO __instance, double x, double y)
    {
        if (ReferenceEquals(__instance, Injector.moho_height))
        {
            return true;
        }
        // X wraps around as it is longitude.
        x = Math.Abs(x - Math.Floor(x));
        __instance.centerXD = x * __instance._width;
        __instance.minX = (int)Math.Floor(__instance.centerXD);
        __instance.maxX = (int)Math.Ceiling(__instance.centerXD);
        __instance.midX = (float)__instance.centerXD - __instance.minX;
        if (__instance.maxX == __instance._width)
            __instance.maxX = 0;

        // Y clamps as it is latitude and the poles don't wrap to each other.
        y = Math.Min(Math.Max(y, 0), 0.99999);
        __instance.centerYD = y * __instance._height;
        __instance.minY = (int)Math.Floor(__instance.centerYD);
        __instance.maxY = (int)Math.Ceiling(__instance.centerYD);
        __instance.midY = (float)__instance.centerYD - __instance.minY;
        if (__instance.maxY >= __instance._height)
            __instance.maxY = __instance._height - 1;

        return false;
    }
}
