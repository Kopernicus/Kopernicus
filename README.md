Kopernicus
==============================
March 14, 2021
* Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
* Actively maintained by: Prestja and R-T-B.
* Formerly maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Democat3457, Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88, Majiir (CompatibilityChecker)
* Much thanks to Sarbian for ModuleManager and ModularFlightIntegrator

New in this latest version (release-35)

1.) Release bundling has changed. There are now two releases, the legacy release series 1.8.1 that supports all 1.8.x releases and the modern release series whose version number stays in sync with the latest KSP. The modern series supports all releases post 1.9.0, including 1.9.0, with the same dll. Like magic. Please download the right version.

2.) Disabled new multistar EC math calculation by default, as some modded inflight vessels may not behave well with it (limited testing honestly). To enable it, please back up your save, and download the MultiStarSolarPanels.cfg linked with this release, and place it in GameData/Kopernicus/Config. It may work fine, we really need additional data.

3.) Added a new parameter to Kopernicus_Config.cfg called SolarRefreshRate, which controls the amount of time in seconds between calculations of the multistar solar EC math (when enabled, see above). Raise it if you suffer performance issues from the multistar math. Must be an integer, and the default of 1 second is slower than stock but should be fine for most all instances and strike a balance between performance and speed.

4.) A bug in which freshly generated config files were not loaded has been fixed.

5.) Some minor scatter related bugs have been corrected.

6.) Updated MFI build included in zip where possible (not legacy) to latest version (1.2.10.0).

7.) Nyan cats are no longer used to signify an error, we use a simple error message now (we were using reflection to activate them and that's bad practice).

Known Bugs:

1.) At interstellar ranges, heat can sometimes behave strangely, sometimes related to map zoom (be careful zooming out). It is best to turn off part heating when traveling far far away.

2.) When zooming out all the way out in map view at interstellar ranges, the navbal furthermore sometimes behaves oddly. We are working on this and all the interstellar bugs actively.



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
