Kopernicus
==============================
August 4th, 2023
* Created by: BryceSchroeder and Nathaniel R. Lewis (Teknoman117)
* Actively maintained by: Prestja and R-T-B.
* Formerly maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Democat3457, Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88, Majiir (CompatibilityChecker), blackrack/LGHassen (shaders/GPL'd scatterer code)
* Much thanks to Sarbian for ModuleManager and ModularFlightIntegrator

New in this latest version release-181:

1.)  Implemneted dictionary caching of GetBrightest.  This uses a tiny bit more ram, but improves performance of the function massively, and as it is one of the most called functions in Kopernicus, this helps close the performance gap with Single Star mode quite a bit.

2.) Fixed a bug where sometimes GetBrightest would erronously return the center Sun even when it was not the Brightest body.

Known Bugs:

1.) Not exactly a bug, but worth mentioning: The Kopernicus_Config.cfg file is rewritten when the game exits. This means any manual (not in the GUI) edits made while playing the game will not be preserved. Edit the file only with the game exited, please.

2.) At interstellar ranges, heat can sometimes behave strangely, sometimes related to map zoom (be careful zooming out). It is best to turn off part heating when traveling far far away. (I am not sure if this is still releavent as of Release-159, feedback welcome).

3.) When zooming out all the way out in map view at interstellar ranges, the navball furthermore sometimes behaves oddly. We are working on this and monitoring all the interstellar bugs actively. (I am not sure if this is still releavent as of Release-159, feedback welcome).

4.) Very Old craft files may complain about a missing module. This is a cosmetic error and can be ignored. Reload and re-save the craft to remove the error.

5.) Sometimes when reloading a quicksave to KSC, you will get the KSC sunken into the ground. This is cosmetic only, another reload of the same save will fix it. (This error has been around forever, just now listing it).

6.) When you uninstall a mod that had installed a Terrain Detail preset you were using, it may be listed still in the Graphics settings as "New Text." This is by design. If it bothers you, please reinstall the mod that setup that preset, or delete settings.cfg and let it regenerate.

7.) Some mods that used custom Terrain Presets may require you to delete your settings.cfg file and reset your settings with this release. This is rare, but can happen. See this post for details

Known Caveats:

1.) The 1.12.x release series works on 1.12.x. The 1.11.x,1.10.x,1.9.x and 1.8.x releases are deprecated

2.) Multistar Solar panel support requires an additional config file, attached to release.

3.) As of release-107, scatter density underwent a bugfix on all bodies globally that results in densities acting more dense than before on some select configs. Some mods may need to adjust. Normally we'd not change things like this, but this is technically the correct stock behavior of the node so... if you need the old behavior, see config option UseIncorrectScatterDensityLogic.

4.) As of release-151, polar generation behavior has changed slightly. Though it will be safer overall for new missions, be careful loading existing craft there. This is probably not lethal but I don't want you to be unaware. Maybe make a save just in case? ;)

5.) The "collider fix" as it's called, which fixes the event in which you sink into the terrain on distant bodies, is now on by default. If you really need distant colliders, turn this off, but you'd best have a good reason (I can't think of any).

6.) The particle system was hopelessly broken and has been since sometime past 1.10.x. Few mods used it, so it has been removed completely as of Release-146.

7.) Because we now unpack multipart PQSCity's correctly, you may find some PQSCity structures are in the earth or floating. Report such bugs to your planet pack author as this is an intended change (only cosmetic).

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
