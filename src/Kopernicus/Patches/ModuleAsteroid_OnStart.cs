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
using System.Collections.Generic;
using HarmonyLib;
using Kopernicus.Configuration.DiscoverableObjects;
using Kopernicus.RuntimeUtility;
using UnityEngine;

namespace Kopernicus.Patches;

// Apply configured asteroid class radius ranges to the asteroid itself.
//
// Stock computes the radius in ModuleAsteroid.OnStart as
//    radius = paPrefab.radius * Random.Range(minRadiusMultiplier, maxRadiusMultiplier)
// We override the min/max radius multipliers on the module to be
//    minRadiusMultiplier = minRadius / paPrefab.radius;
//    maxRadiusMultiplier = maxRadius / paPrefab.radius;
// which makes stock code pick from the radius range we want it to.
[HarmonyPatch(typeof(ModuleAsteroid), nameof(ModuleAsteroid.OnStart))]
static class ModuleAsteroid_OnStart
{
    // Prefab radii, keyed by the Resources URL. Read from the prefab rather than hardcoded so
    // this keeps working if Squad or another mod ever changes them.
    private static readonly Dictionary<String, Single> PrefabRadii = new Dictionary<String, Single>();

    static void Prefix(ModuleAsteroid __instance)
    {
        Vessel vessel = __instance.vessel;
        if (vessel == null || vessel.DiscoveryInfo == null)
        {
            return;
        }

        // Objects we didn't spawn (contracts, Making History missions, stock's own spawner) have
        // no group, and a group without a ClassRadius node opts out. Both keep stock behaviour.
        Asteroid group = DiscoverableObjects.FindGroup(vessel.launchedFrom);
        Location.RandomRangeLoader range = group?.ClassRadius?.Get(vessel.DiscoveryInfo.objectSize);
        if (range == null)
        {
            return;
        }

        Single minRadius = range.MinValue.Value;
        Single maxRadius = range.MaxValue.Value;
        if (minRadius <= 0f || maxRadius <= 0f)
        {
            // Already warned about when the group lookup was built.
            return;
        }

        Single prefabRadius = GetPrefabRadius(__instance, vessel);
        if (prefabRadius <= 0f)
        {
            // Prefab is missing or degenerate - leave the multipliers alone and let stock log it.
            return;
        }

        __instance.minRadiusMultiplier = minRadius / prefabRadius;
        __instance.maxRadiusMultiplier = maxRadius / prefabRadius;
    }

    private static Single GetPrefabRadius(ModuleAsteroid module, Vessel vessel)
    {
        // Mirrors stock's URL resolution, including the case where a previous load already
        // persisted prefabBaseURL onto the part.
        String url = String.IsNullOrEmpty(module.prefabBaseURL)
            ? "Procedural/PA_" + vessel.DiscoveryInfo.objectSize
            : module.prefabBaseURL;

        if (PrefabRadii.TryGetValue(url, out Single cached))
        {
            return cached;
        }

        ProceduralAsteroid prefab = Resources.Load<ProceduralAsteroid>(url);
        Single radius = prefab ? prefab.radius : 0f;
        PrefabRadii[url] = radius;
        return radius;
    }
}
