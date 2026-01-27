using HarmonyLib;

namespace Kopernicus.Patches;

[HarmonyPatch(typeof(GameSettings), "WriteCfg")]
static class GameSettings_WriteCfg
{
    static bool Prefix(GameSettings __instance)
    {
        PQSCache.PQSPreset pqspreset = new PQSCache.PQSPreset();
        pqspreset.name = "Low";
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Kerbin", 4.0, 1, 8));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("KerbinOcean", 4.0, 1, 7));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Mun", 4.0, 1, 7));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Minmus", 4.0, 1, 6));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Bop", 4.0, 1, 6));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Duna", 4.0, 1, 7));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Eve", 4.0, 1, 7));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("EveOcean", 4.0, 1, 7));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Gilly", 4.0, 1, 6));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Ike", 4.0, 1, 7));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Laythe", 4.0, 1, 7));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("LaytheOcean", 4.0, 1, 7));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Moho", 4.0, 1, 7));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Tylo", 4.0, 1, 7));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Vall", 4.0, 1, 7));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Dres", 4.0, 1, 7));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Pol", 4.0, 1, 7));
        pqspreset.spherePresets.Add(new PQSCache.PQSSpherePreset("Eeloo", 4.0, 1, 7));
        PQSCache.PQSPreset pqspreset2 = new PQSCache.PQSPreset();
        pqspreset2.name = "Default";
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Kerbin", 6.0, 1, 9));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("KerbinOcean", 6.0, 1, 7));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Mun", 6.0, 1, 8));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Minmus", 6.0, 1, 6));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Bop", 6.0, 1, 6));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Duna", 6.0, 1, 8));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Eve", 6.0, 1, 9));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("EveOcean", 6.0, 1, 7));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Gilly", 6.0, 1, 6));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Ike", 6.0, 1, 6));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Laythe", 6.0, 1, 9));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("LaytheOcean", 6.0, 1, 7));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Moho", 6.0, 1, 8));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Tylo", 6.0, 1, 8));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Vall", 6.0, 1, 8));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Dres", 6.0, 1, 8));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Pol", 6.0, 1, 8));
        pqspreset2.spherePresets.Add(new PQSCache.PQSSpherePreset("Eeloo", 6.0, 1, 8));
        PQSCache.PQSPreset pqspreset3 = new PQSCache.PQSPreset();
        pqspreset3.name = "High";
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Kerbin", 8.0, 1, 10));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("KerbinOcean", 8.0, 1, 7));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Mun", 8.0, 1, 9));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Minmus", 8.0, 1, 7));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Bop", 8.0, 1, 6));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Duna", 8.0, 1, 9));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Eve", 8.0, 1, 10));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("EveOcean", 8.0, 1, 7));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Gilly", 8.0, 1, 7));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Ike", 8.0, 1, 7));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Laythe", 8.0, 1, 10));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("LaytheOcean", 8.0, 1, 7));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Moho", 8.0, 1, 9));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Tylo", 8.0, 1, 9));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Vall", 8.0, 1, 9));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Dres", 8.0, 1, 9));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Pol", 8.0, 1, 9));
        pqspreset3.spherePresets.Add(new PQSCache.PQSSpherePreset("Eeloo", 8.0, 1, 9));
        RuntimeUtility.RuntimeUtility.pqsLow = PQSCache.PresetList.presets[0];
        RuntimeUtility.RuntimeUtility.pqsDefault = PQSCache.PresetList.presets[1];
        RuntimeUtility.RuntimeUtility.pqsHigh = PQSCache.PresetList.presets[2];
        PQSCache.PresetList.presets[0] = pqspreset;
        PQSCache.PresetList.presets[1] = pqspreset2;
        PQSCache.PresetList.presets[2] = pqspreset3;
        return true;
    }
    static void Postfix(GameSettings __instance)
    {
        PQSCache.PresetList.presets[0] = RuntimeUtility.RuntimeUtility.pqsLow;
        PQSCache.PresetList.presets[1] = RuntimeUtility.RuntimeUtility.pqsDefault;
        PQSCache.PresetList.presets[2] = RuntimeUtility.RuntimeUtility.pqsHigh;
    }
}
