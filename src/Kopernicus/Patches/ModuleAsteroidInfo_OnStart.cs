using HarmonyLib;

namespace Kopernicus.Patches;

[HarmonyPatch(typeof(ModuleAsteroidInfo), "OnStart")]
static class ModuleAsteroidInfo_OnStart
{
    static bool Prefix(ModuleAsteroidInfo __instance, PartModule.StartState state)
    {
        __instance.baseMod = __instance.part.Modules.GetModule<ModuleAsteroid>(0);
        if (__instance.baseMod != null)
        {
            __instance.baseMod.OnStart(state);
            if (__instance.currentMassVal <= 1E-09)
            {
                __instance.currentMassVal = (double)__instance.part.mass * RuntimeUtility.RuntimeUtility.KopernicusConfig.ApplyDensityMultToMinorObjects;
            }
            if (__instance.massThresholdVal <= 1E-09)
            {
                __instance.SetupAsteroidResources();
            }
            __instance.part.force_activate();
            __instance.baseMod.SetAsteroidMass((float)__instance.currentMassVal);
            __instance.part.mass = (float)__instance.currentMassVal;
        }
        return false;
    }
}
