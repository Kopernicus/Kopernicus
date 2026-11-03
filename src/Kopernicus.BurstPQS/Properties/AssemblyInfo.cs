using System.Reflection;

[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: KSPAssembly("Kopernicus.BurstPQS", 1, 0)]
[assembly: KSPAssemblyDependency("Kopernicus", 1, 0)]
[assembly: KSPAssemblyDependency("0Harmony", 0, 0)]
[assembly: KSPAssemblyDependency("KSPTextureLoader", 1, 0, 26)]
[assembly: KSPAssemblyDependency("KSPBurst", 1, 5, 5)]
// BurstPQS v1.0.30 is the first version that no longer ships its own
// BurstPQS.Kopernicus plugin. Older versions keep using that one instead.
[assembly: KSPAssemblyDependency("BurstPQS", 1, 0, 30)]
