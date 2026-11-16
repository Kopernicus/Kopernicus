using System.Reflection;
using Kopernicus;
using Kopernicus.Constants;
using Kopernicus.RuntimeUtility;

[assembly: KSPAssembly("Kopernicus", 1, 0, BuildInfo.KopernicusVersion)]
[assembly: KSPAssemblyDependency("Kopernicus.Parser", 1, 0)]
[assembly: KSPAssemblyDependency("ModularFlightIntegrator", 1, 0)]
[assembly: KSPAssemblyDependency("0Harmony", 0, 0)]
[assembly: KSPAssemblyDependency("KSPTextureLoader", 1, 0, 26)]
#if KSP_RETOOLED
[assembly: KSPAssemblyDependency("KSP", 1, 13)]
#endif
[assembly: LogAggregator("GameData/ModuleManager.ConfigCache")]
[assembly: LogAggregator("Logs/Kopernicus/")]
[assembly: LogAggregator("KSP.log")]
