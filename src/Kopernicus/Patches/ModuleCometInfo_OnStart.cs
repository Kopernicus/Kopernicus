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
