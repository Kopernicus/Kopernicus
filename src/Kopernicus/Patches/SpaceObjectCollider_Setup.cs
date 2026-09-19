using System;
using HarmonyLib;
using UnityEngine;

namespace Kopernicus.Patches;

// Collider generation is throttled by distance, and the stock thresholds are absolute metres.
// 2500m sits barely clear of the surface of a kilometre-scale asteroid, so essentially every
// approach reads as "far away" and generation crawls at one collider per frame. Scaling the far
// threshold with the object makes a given point on the ramp mean the same thing at any size.
//
// Only the PSpaceObject overload is patched; the PAsteroid one forwards straight to it. Setup runs
// inside PSpaceObject.CreateCollider, so it is still within the bracket that supplies the radius;
// a Setup reached any other way sees 0 and leaves the stock range alone.
[HarmonyPatch(typeof(SpaceObjectCollider), "Setup",
    typeof(PSpaceObject), typeof(Mesh), typeof(Vector3), typeof(Func<Transform, float>),
    typeof(float), typeof(float), typeof(Callback))]
static class SpaceObjectCollider_Setup
{
    static void Prefix(ref float maxRange)
    {
        float radius = SpaceObjectColliderUtil.GeneratingRadius;
        if (radius <= 0f)
        {
            return;
        }

        maxRange = Mathf.Max(maxRange, radius * 3f);
    }
}
