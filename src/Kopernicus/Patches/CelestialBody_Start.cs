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

/// <summary>
/// Stock converts a solar day length into a sidereal rotation period assuming a prograde orbit.
/// For orbits with an inclination above 90 degrees the orbital period needs to be negative
/// in that conversion. Otherwise the body ends up with the wrong rotation period.
/// </summary>
[HarmonyPatch(typeof(CelestialBody), "Start")]
static class CelestialBody_Start
{
    static void Prefix(CelestialBody __instance, out Boolean __state)
    {
        __state = false;
        if (!__instance.solarRotationPeriod || __instance.orbitDriver == null || __instance.orbit.inclination <= 90)
        {
            return;
        }

        Orbit orbit = __instance.orbit;
        Double solarDay = __instance.rotationPeriod;
        Double orbitalPeriod = 2.0 * Math.PI * Math.Sqrt(Math.Pow(Math.Abs(orbit.semiMajorAxis), 3.0) / orbit.referenceBody.gravParameter);
        __instance.rotationPeriod = Utility.SolarToSiderealRotationPeriod(__instance, solarDay, orbitalPeriod);
        Debug.Log("[Kopernicus] " + __instance.bodyName + " has a retrograde orbit. Solar day length is " +
                  KSPUtil.PrintTime(solarDay, 3, false, true) + ", sidereal day length is " +
                  KSPUtil.PrintTime(__instance.rotationPeriod, 3, false, true));

        // Skip the stock conversion by setting the field to false.
        // Then restore the state in the finalizer.
        __instance.solarRotationPeriod = false;
        __state = true;
    }

    static void Finalizer(CelestialBody __instance, Boolean __state)
    {
        if (__state)
        {
            __instance.solarRotationPeriod = true;
        }
    }
}
