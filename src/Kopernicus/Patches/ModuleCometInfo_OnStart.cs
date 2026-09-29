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
using HarmonyLib;

namespace Kopernicus.Patches;

[HarmonyPatch(typeof(ModuleCometInfo), "OnStart")]
static class ModuleCometInfo_OnStart
{
    static bool Prefix(ModuleCometInfo __instance, PartModule.StartState state)
    {
        __instance.baseMod = __instance.part.Modules.GetModule<ModuleComet>(0);
        if (__instance.baseMod != null)
        {
            __instance.baseMod.OnStart(state);
            if (__instance.currentMassVal <= 1E-09)
            {
                __instance.currentMassVal = (double)__instance.part.mass * RuntimeUtility.RuntimeUtility.KopernicusConfig.ApplyDensityMultToMinorObjects;
            }
            if (__instance.massThresholdVal <= 1E-09)
            {
                __instance.SetupCometResources();
            }
            __instance.part.force_activate();
            __instance.baseMod.SetCometMass((float)__instance.currentMassVal);
            __instance.part.mass = (float)__instance.currentMassVal;
        }
        return false;
    }
}
