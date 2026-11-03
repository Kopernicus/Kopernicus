using Kopernicus.BurstPQS.Map;
using BurstPQS.Map;
using HarmonyLib;
using Kopernicus.Components;
using Kopernicus.OnDemand;
using UnityEngine;

namespace Kopernicus.BurstPQS;

[KSPAddon(KSPAddon.Startup.Instantly, once: true)]
internal class Loader : MonoBehaviour
{
    void Awake()
    {
        var harmony = new Harmony("Kopernicus.BurstPQS");
        harmony.PatchAll();
    }

    void Start()
    {
        BurstMapSO.RegisterMapSOFactoryFunc<MapSODemand>(BurstKopernicusMapSO.Create);
        BurstMapSO.RegisterMapSOFactoryFunc<KopernicusMapSO>(BurstKopernicusMapSO.Create);
        BurstMapSO.RegisterMapSOFactoryFunc<KopernicusCBAttributeMapSO>(mapSO =>
            BurstMapSO.Create(new BurstCBAttributeMapSO(mapSO))
        );
    }
}
