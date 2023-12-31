Kopernicus
==============================
December 30th, 2023
* Created by: BryceSchroeder and Nathaniel R. Lewis (Teknoman117)
* Actively maintained by: Prestja and R-T-B.
* Formerly maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Democat3457, Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88, Majiir (CompatibilityChecker), blackrack/LGHassen (shaders/GPL'd scatterer code)
* Much thanks to Sarbian for ModuleManager and ModularFlightIntegrator

New in this latest bleeding edge prerelease version release-196 RC1:

1.) Added Kopernicus_Config.cfg option HomeWorldName, a string, default set to "Kerbin."  This option enables you to for the first time ever, to have a homeworld and KSC at something other than "Kerbin" by changing this simple option, without mucking about with displayName, or worse, cbNameLater, a deprecated option.  cbNameLater will likely be removed in the near future if this works well.

2.) Note you can't just randomly swap your homeworld, you need to edit in a KSC into your target site like always.  This feature is more for pack authors.

3.) Because this change involved a lot of restructuring of Kopernicus (which for years, has assumed the homeworld will always be "Kerbin"), this is a bleeding edge testing release for at least a few days.

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
