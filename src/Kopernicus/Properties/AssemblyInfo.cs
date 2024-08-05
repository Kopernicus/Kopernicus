using System.Reflection;
using Kopernicus.Constants;
using Kopernicus.RuntimeUtility;

[assembly: AssemblyTitle("Kopernicus")]
[assembly: AssemblyDescription("Planetary System Modifier for Kerbal Space Program")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Kopernicus Project")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("Copyright (C) Kopernicus Project")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: KSPAssembly("Kopernicus", 1, 0)]
[assembly: KSPAssemblyDependency("Kopernicus.Parser", 1, 0)]
[assembly: KSPAssemblyDependency("ModularFlightIntegrator", 1, 0)]
[assembly: AssemblyVersion("1.0.0")]
[assembly: AssemblyFileVersion("1.12." + CompatibilityChecker.KOPERNICUS + ".1")]
[assembly: LogAggregator("GameData/ModuleManager.ConfigCache")]
[assembly: LogAggregator("Logs/Kopernicus/")]
[assembly: LogAggregator("KSP.log")] 
