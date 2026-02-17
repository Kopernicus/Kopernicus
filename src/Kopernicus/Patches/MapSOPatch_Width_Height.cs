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

using System.Runtime.CompilerServices;
using HarmonyLib;
using Kopernicus.OnDemand;

namespace Kopernicus.Patches;

// Patch MapSO.Width and MapSO.Height so that they load the texture if it
// implements ILoadOnDemand and the width/height are 0.
//
// This way MapSO.Width/Height always return the correct dimensions even
// if the texture isn't loaded yet.

[HarmonyPatch(typeof(MapSO), nameof(MapSO.Width), MethodType.Getter)]
internal class MapSOPatch_Width
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void Prefix(MapSO __instance)
    {
        if (__instance._width != 0)
            return;

        TryLoad(__instance);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void TryLoad(MapSO mapSO)
    {
        if (mapSO is not ILoadOnDemand ondemand)
            return;

        ondemand.Load();
    }
}

[HarmonyPatch(typeof(MapSO), nameof(MapSO.Height), MethodType.Getter)]
internal class MapSOPatch_Height
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void Prefix(MapSO __instance)
    {
        if (__instance._height != 0)
            return;

        TryLoad(__instance);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void TryLoad(MapSO mapSO)
    {
        if (mapSO is not ILoadOnDemand ondemand)
            return;

        ondemand.Load();
    }
}
