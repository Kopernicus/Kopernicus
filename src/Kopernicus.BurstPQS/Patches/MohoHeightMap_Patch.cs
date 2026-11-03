using BurstPQS.Kopernicus.Map;
using BurstPQS.Map;
using HarmonyLib;
using Kopernicus;

namespace BurstPQS.Kopernicus.Patches;

/// <summary>
/// When <c>KopernicusConfig.UseStockMohoTemplate</c> is true, Kopernicus keeps the
/// stock Moho heightmap and expects it to use stock bilinear filtering (Y-axis wrapping
/// at poles). This patch intercepts the BurstMapSO creation for that specific MapSO
/// and wraps it with <see cref="StockBilinearBurstMapSO"/> to preserve stock behavior.
/// </summary>
[HarmonyPatch(typeof(BurstMapSO), nameof(BurstMapSO.Create), [typeof(MapSO)])]
static class MohoHeightMap_Patch
{
    static bool Prefix(MapSO mapSO, ref BurstMapSO __result)
    {
        if (Injector.moho_height != null && mapSO == Injector.moho_height)
        {
            __result = BurstMapSO.Create(new StockBilinearBurstMapSO(mapSO));
            return false;
        }

        return true;
    }
}
