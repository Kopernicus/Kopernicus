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
