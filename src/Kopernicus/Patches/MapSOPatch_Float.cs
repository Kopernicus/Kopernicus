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
