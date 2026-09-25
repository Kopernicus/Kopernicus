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
