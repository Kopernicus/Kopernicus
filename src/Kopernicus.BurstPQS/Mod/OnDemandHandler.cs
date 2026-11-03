using BurstPQS;
using Kopernicus.OnDemand;

namespace BurstPQS.Kopernicus.Mod;

/// <summary>
/// Adapter for <see cref="PQSMod_OnDemandHandler"/>. This mod manages
/// on-demand texture loading/unloading and does not modify vertex data.
/// The base class passthrough of <see cref="PQSMod.OnQuadPreBuild"/>
/// handles triggering texture loads; the original MonoBehaviour's
/// <c>LateUpdate</c> and <c>OnSphereInactive</c> handle unloading.
/// </summary>
[BatchPQSMod(typeof(PQSMod_OnDemandHandler))]
public class OnDemandHandler(PQSMod_OnDemandHandler mod)
    : BatchPQSMod<PQSMod_OnDemandHandler>(mod)
{ }
