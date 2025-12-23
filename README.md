Kopernicus
==============================
December 22nd, 2025
* Created by: BryceSchroeder and Nathaniel R. Lewis (Teknoman117)
* Actively maintained by: R-T-B.
* Formerly maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Democat3457, Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88, Majiir (CompatibilityChecker), blackrack/LGHassen (shaders/GPL'd scatterer code)
* Much thanks to Sarbian for ModuleManager and ModularFlightIntegrator

New in this latest version release-232:

1.) This is sadly not the comet release I promised, that will come by the end of January.  Still, it's a very potent release with optimizations galore to scene switch times, load times, OnDemand performance, the works.  Read on below, because there are some changes...

2.) Starting with this release, Kopernicus no longer bundles it's dependencies in the zip.  They were often outdated and this was just a kind of silly practice.  We will list the dependencies on github and the parent thread to help users who are not on CKAN navigate this, but you really should be using CKAN.

3.) On that note, this release adds a dependency to KSPTextureLoader, a high performance async loading texture library that really speeds up the mod in general.  Please grab it here: https://github.com/Phantomical/KSPTextureLoader/releases

4.) In addition to being faster, this enabled Kopernicus to support more formats, and we do now.  New format support includes .dds BC7 (a better option than DXT1 and DXT5) and BC5 (a better option than DXT5nm).

5.) In addition to all that, there were foundational changes to the asteroid code, and some bugfixes to the density scaling code (if anyone uses that).  None of this should affect users but mentioning it for completeness sake.

6.) A couple final notes...  one this should be considered a "major" release and is slightly more likely to bug out than most updates given the complexity of the changes.  We tested it this one better than nearly any release before to be frank, but still, if you have issues, report them, as we are eager to help!  Oh, and this would not be possible without the help of github contributors @Phantomical and @ballisticfox, thank you so much. 

Dependencies: We depend on HarmonyKSP, ModuleManager, ModularFlightIntegrator, and KSPTextureLoader.
https://github.com/KSPModdingLibs/HarmonyKSP/releases
https://ksp.sarbian.com/jenkins/job/ModuleManager/
https://ksp.sarbian.com/jenkins/job/ModularFlightIntegrator/
https://github.com/Phantomical/KSPTextureLoader/releases

About
-----
Kopernicus is a KSP add-on that allows for modification of stock planets and the creation of new planets via modification of the system prefab.  Why is this advantageous you might ask?  Previous planet adder mods, such as Planet Factory, modified the live planetary system and had to keep multiple hacks actively running to provide these worlds.  We strive to provide the least hacky solution by introducing planets into the game in the exact same manner Squad would.  

Kopernicus is a one step process.  It is started before the planetary system is created and rewrites a property called PSystemManager.Instance.systemPrefab.  The game itself then creates *our* planetary system as if it were blessed by Squad themselves.  The mod’s function ends here, and it terminates.  Kopernicus introduced worlds require zero maintenance by third party code, all support is driven entirely by built in functionality.  This yields an incredibly stable and incredibly flexible environment for planetary creation.

One caveat exists that prevents absolute control over a planetary system.  Due to the way Squad defines how launch control and ship recovery work, a planet called Kerbin must exist and must have a reference body id of 1.  While it is easy to provide this scenario, as a star to orbit and a single planet satisfies this, any would be universe architect needs to be aware so they don’t get mystery errors. The displayed name of the body (i.e. the celestialBody.bodyName property) can be changed after the spawn of the PSystem  

Each celestial body in KSP has a property called “flightGlobalsIndex.”  This number does not directly correspond to the reference id, but is related.  Once all of the celestial bodies have been spawned, references to them are written into the localBodies list.  This list is then sorted in increasing order of the flight globals index.  The index the body is located at after the sort becomes the reference body id.

We have yet to see if removing or rearranging the other bodies causes any sort of errors in the science or contracts system.


Instructions
------------
- Copy the contents of the GameData/ folder to KSP’s GameData/ folder
- Launch KSP and enjoy!
- Please report any bugs you may find to either or both of the email addresses above.

Examples
----------
Selectively copy folders inside of [KopernicusExamples/](https://github.com/Kopernicus/KopernicusExamples/) into a GameData/KopernicusExamples/ folder.  There are a number of examples of how to use Kopernicus.

Building
----------
To build Kopernicus from source, **don't edit the project file**.

Instead, define a **Reference Path** pointing to the **root** of your local KSP install.

In Visual Studio and Rider, this can be done within the IDE UI, by going to the project properties window and then in the `Reference Path` tab.
If you want to set it manually, create a `Kopernicus.csproj.user` file next to the `src\Kopernicus\Kopernicus.csproj` file with the following content :
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <ReferencePath>Absolute\Path\To\Your\KSP\Install\Folder\Root\</ReferencePath>
  </PropertyGroup>
</Project>
```
