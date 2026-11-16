using System.Reflection;

[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: KSPAssembly("Kopernicus.BurstPQS", 1, 0)]
[assembly: KSPAssemblyDependency("Kopernicus", 1, 0)]
[assembly: KSPAssemblyDependency("0Harmony", 0, 0)]
[assembly: KSPAssemblyDependency("KSPTextureLoader", 1, 0, 26)]
[assembly: KSPAssemblyDependency("KSPBurst", 1, 5, 5)]
#if KSP_RETOOLED
[assembly: KSPAssemblyDependency("KSP", 1, 13)]
#endif
// BurstPQS v1.0.30 replaces its own BurstPQS.Kopernicus plugin with type forwards
// to this assembly. Older versions keep using that plugin instead.
[assembly: KSPAssemblyDependency("BurstPQS", 1, 0, 30)]
