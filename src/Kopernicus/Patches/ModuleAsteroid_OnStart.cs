using System;
using System.Collections.Generic;
using HarmonyLib;
using Kopernicus.Configuration.DiscoverableObjects;
using Kopernicus.RuntimeUtility;
using UnityEngine;

namespace Kopernicus.Patches;

// Applies the per-class radius ranges from an asteroid group's ClassRadius node.
// Stock computes the object's radius in ModuleAsteroid.OnStart as
//   radius = paPrefab.radius * Random.Range(minRadiusMultiplier, maxRadiusMultiplier)
// where paPrefab.radius is baked into the PA_<class> Unity prefab and the two multipliers are
// PartModule fields shared by every class. Writing the multipliers here, before the original
// method runs, lets a config ask for a radius in meters per class without touching stock's own
// logic: the roll still happens in stock code, off Random.InitState(seed) with the persisted
// seed, so an object's size stays a pure function of its seed and the config.
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
