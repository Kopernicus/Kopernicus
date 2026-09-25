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
