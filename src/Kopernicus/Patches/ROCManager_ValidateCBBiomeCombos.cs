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
using UnityEngine;

namespace Kopernicus.Patches;

[HarmonyPatch(typeof(ROCManager), "ValidateCBBiomeCombos")]
static class ROCManager_ValidateCBBiomeCombos
{
    private static Func<ROCManager, string, CelestialBody> ValidCelestialBody;
    private static Func<ROCManager, CelestialBody, string, bool> ValidCBBiome;
    static bool Prefix(ROCManager __instance)
    {
        List<ROCDefinition> rocDefinitions = __instance.rocDefinitions;
        ValidCelestialBody = AccessTools.MethodDelegate<Func<ROCManager, string, CelestialBody>>(AccessTools.Method(typeof(ROCManager), "ValidCelestialBody"));
        ValidCBBiome = AccessTools.MethodDelegate<Func<ROCManager, CelestialBody, string, bool>>(AccessTools.Method(typeof(ROCManager), "ValidCBBiome"));
        for (int num = rocDefinitions.Count - 1; num >= 0; num--)
        {
            for (int num2 = rocDefinitions[num].myCelestialBodies.Count - 1; num2 >= 0; num2--)
            {
                CelestialBody celestialBody = ValidCelestialBody(__instance, rocDefinitions[num].myCelestialBodies[num2].name);
                if (celestialBody.IsNullOrDestroyed())
                {
                    Debug.LogWarningFormat("[ROCManager]: Invalid CelestialBody Name {0} on ROC Definition {1}. Removed entry.", rocDefinitions[num].myCelestialBodies[num2].name, rocDefinitions[num].type);
                    rocDefinitions[num].myCelestialBodies.RemoveAt(num2);
                    continue; // missing in stock code
                }
                else
                {
                    for (int num3 = rocDefinitions[num].myCelestialBodies[num2].biomes.Count - 1; num3 >= 0; num3--)
                    {
                        if (!ValidCBBiome(__instance, celestialBody, rocDefinitions[num].myCelestialBodies[num2].biomes[num3]))
                        {
                            Debug.LogWarningFormat("[ROCManager]: Invalid Biome Name {0} for Celestial Body {1} on ROC Definition {2}. Removed entry.", rocDefinitions[num].myCelestialBodies[num2].biomes[num3], rocDefinitions[num].myCelestialBodies[num2].name, rocDefinitions[num].type);
                            rocDefinitions[num].myCelestialBodies[num2].biomes.RemoveAt(num3);
                        }
                    }
                }
                if (rocDefinitions[num].myCelestialBodies[num2].biomes.Count == 0) // ArgumentOutOfRangeException for myCelestialBodies[num2] when the previous if evaluate to true
                {
                    Debug.LogWarningFormat("[ROCManager]: No Valid Biomes for Celestial Body {0} on ROC Definition {1}. Removed entry.", rocDefinitions[num].myCelestialBodies[num2].name, rocDefinitions[num].type);
                    rocDefinitions[num].myCelestialBodies.RemoveAt(num2);
                }
            }
            if (rocDefinitions[num].myCelestialBodies.Count == 0)
            {
                Debug.LogWarningFormat("[ROCManager]: No Valid Celestial Bodies on ROC Definition {0}. Removed entry.", rocDefinitions[num].type);
                rocDefinitions.RemoveAt(num);
            }
        }
        return false;
    }
}
