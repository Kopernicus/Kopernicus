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
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SentinelMission;

namespace Kopernicus.Patches;

[HarmonyPatch(typeof(SentinelUtilities))]
[HarmonyPatch("FindInnerAndOuterBodies")]
[HarmonyPatch([typeof(double), typeof(CelestialBody), typeof(CelestialBody)], [ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out])]
static class SentinelUtilities_FindInnerAndOuterBodies
{
    static bool Prefix(ref bool __result, double SMA, out CelestialBody innerBody, out CelestialBody outerBody)
    {
        Dictionary<double, CelestialBody> dictionary = new Dictionary<double, CelestialBody>();
        for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
        {
            CelestialBody celestialBody = FlightGlobals.Bodies[i];
            if (celestialBody == Planetarium.fetch.Sun)
            {
                dictionary.TryAdd(0.0, celestialBody);
            }
            else if (celestialBody.referenceBody == Planetarium.fetch.Sun)
            {
                dictionary.TryAdd(celestialBody.orbit.semiMajorAxis, celestialBody);
            }
        }
        List<double> list = dictionary.Keys.ToList<double>();
        list.Sort();
        for (int j = 0; j < list.Count; j++)
        {
            if (list[j] <= SMA)
            {
                if (list[j + 1] > SMA)
                {
                    innerBody = dictionary[list[j]];
                    outerBody = dictionary[list[j + 1]];
                    __result = true;
                    return false;
                }
            }
        }
        innerBody = dictionary[list[0]];
        outerBody = dictionary[list[list.Count - 1]];
        __result = false;
        return false;
    }
}
