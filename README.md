Kopernicus
==============================
July 7th, 2025
* Created by: BryceSchroeder and Nathaniel R. Lewis (Teknoman117)
* Actively maintained by: Prestja and R-T-B.
* Formerly maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Democat3457, Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88, Majiir (CompatibilityChecker), blackrack/LGHassen (shaders/GPL'd scatterer code)
* Much thanks to Sarbian for ModuleManager and ModularFlightIntegrator

New in this latest version release-227:

1.) This release represents a multitude of accumulated bugfixes we wanted to get out before focusing on the next big thing in earnest, which is comet support.  This aims to be the last release until those are done, which will most likely take until years end at this rate, so...  it's basically an attempt to fix all currently known bugs.  They are listed below in order of severity.  All of them should be fixed, or mostly fixed to the point said feature is at least usable.  Enjoy!

2.) Improved ring "jitter" behavior in the ring shader when using some of the new features recently added.  I am aware there are still some issues with tilted rings, but those will have to wait for the big comet release to looka at, sorry (ring shader code really isn't my thing).  At least most conventional rings behave well now.

3.) Fixed KSCLightsAlwaysOn sometimes not uh...  keeping the lights on?  Now they always stay on with this setting, not just 99% of the time, or when they feel like it.

4.) Removed some dead Shadow Manager code.  Doubt it will do much for the end user, but it makes the codebase more maintainable. Fun fact for those who don't know, Kopernicus Shadow Mananager is actually just scatterer 0.7xx series without the scattering, donated by blackrack.

5.) Improved the extremely poor performance in the "PrincipiaFriendlySOIComputation" option.  Now it only performs moderately bad!  Still only really needed if you want a very strictly acurate SOI in a Principia environment, instead of an estimate.  ie, not worth the performance hit fot 99% of users.

6.) Removed the use of the deprecated UnityEngine function "www" in the Shadow Manager shader loader.  It now uses a file based shader loader, instead of a web request.  Why it was ever using a web request to a disk based shader file is a question for the ancients of Unity lore.

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
