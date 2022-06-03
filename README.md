Kopernicus
==============================
June 2, 2022
* Created by: BryceSchroeder and Nathaniel R. Lewis (Teknoman117)
* Actively maintained by: Prestja and R-T-B.
* Formerly maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Democat3457, Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88, Majiir (CompatibilityChecker), blackrack/LGHassen (shaders/GPL'd scatterer code)
* Much thanks to Sarbian for ModuleManager and ModularFlightIntegrator

New in this latest version release-96:

1.) AddWatchdog.cfg, the sinking landgear fix, has been reintegrated into the stock mod, since we have found a way to cloak it much better. This cloaking means no side effects like unattainable missions, though some mods may still not like this. If you find a mod that does not like the Watchdog, you can disable it. See next point.

2.) Added Kopernicus_Config.cfg parameter EnableKopernicusWatchdog, boolean, default true. Turn to false if you have a mod with a compatability/loading issue with the Watchdog present (this should be far less mods since we now cloak it properly, but Principia still has issues without a custom build, for example). If you have to use that option, please report the mod in question in the KSP Forums Kopernicus thread for updating/fixing.

3.) Kopernicus.Parser.dll has been optimized quite a bit, resulting in a 20% load time performance increase on average. Remember to extract the complete Kopernicus release zip to and replace file to get this benefit (CKAN does this automatically).

Known Bugs:

1.) Not exactly a bug, but worth mentioning: The Kopernicus_Config.cfg file is rewritten when the game exits. This means any edits made while playing the game will not be preserved. Edit the file only with the game exited, please.

2.) At interstellar ranges, heat can sometimes behave strangely, sometimes related to map zoom (be careful zooming out). It is best to turn off part heating when traveling far far away.

3.) When zooming out all the way out in map view at interstellar ranges, the navball furthermore sometimes behaves oddly. We are working on this and nmonitoring all the interstellar bugs actively.

4.) Very Old craft files may complain about a missing module. This is a cosmetic error and can be ignored. Reload and re-save the craft to remove the error.

Known Caveats:

1.) The 1.12.x release series works on 1.12.x,1.11.x,1.10.x, and 1.9.x. The 1.8 release is for 1.8.x.

2.) Mutlistar Solar panel support requires an additional config file, attached to release.

3.) If you use the default config of Kopernicus, A fake, invisible celestial body is then used to fix the distant landing-gear sinking bug and other graphical issues (why this works is quite the mystery). This body is called "KopernicusWatchdog" and will a.) intentionally keep it's distance from you at all times and is b.) invisible. The body is usually hidden from in game processes, but this is new territory having to use another moving celestial to correct a bug, so there may be older mods that don't work. Please report them if so. Principia is a known mod that does not work, a fork (including source) fixing it's issues is available upon PM request to R-T-B.

4.) When using the ultra/atlas shader, ocean effect customization (color etc) is unreliable. Please use only high shader level (shader level 2) or lower if working with stock ocean effects. This does not effect scatterer or other ocean rendering mods.


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
