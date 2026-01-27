using System;
using HarmonyLib;
using UnityEngine;

namespace Kopernicus.Patches;

[HarmonyPatch(typeof(MapSO), "ConstructBilinearCoords", [typeof(float), typeof(float)])]
static class MapSOPPatch_Float
{
    private static bool Prefix(MapSO __instance, float x, float y)
    {
        if (ReferenceEquals(__instance, Injector.moho_height))
        {
            return true;
        }
        // X wraps around as it is longitude.
        x = Mathf.Abs(x - Mathf.Floor(x));
        __instance.centerX = x * __instance._width;
        __instance.minX = Mathf.FloorToInt(__instance.centerX);
        __instance.maxX = Mathf.CeilToInt(__instance.centerX);
        __instance.midX = __instance.centerX - __instance.minX;
        if (__instance.maxX == __instance._width)
            __instance.maxX = 0;

        // Y clamps as it is latitude and the poles don't wrap to each other.
        y = Mathf.Clamp(y, 0, 0.99999f);
        __instance.centerY = y * __instance._height;
        __instance.minY = Mathf.FloorToInt(__instance.centerY);
        __instance.maxY = Mathf.CeilToInt(__instance.centerY);
        __instance.midY = __instance.centerY - __instance.minY;
        if (__instance.maxY >= __instance._height)
            __instance.maxY = __instance._height - 1;

        return false;
    }
}
