Kopernicus
==============================
June 17, 2022
* Created by: BryceSchroeder and Nathaniel R. Lewis (Teknoman117)
* Actively maintained by: Prestja and R-T-B.
* Formerly maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Democat3457, Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88, Majiir (CompatibilityChecker), blackrack/LGHassen (shaders/GPL'd scatterer code)
* Much thanks to Sarbian for ModuleManager and ModularFlightIntegrator

New in this latest version release-123:

1.) Final round of improvements to the JNSQ-specific workarounds for the PQS flickering fix to avoid loadloop erros, and also help with regards to rescales (they produced black terrain).

2.) Holy crap this is actually stable now. #Famouslastwords

Known Bugs:

1.) Not exactly a bug, but worth mentioning: The Kopernicus_Config.cfg file is rewritten when the game exits. This means any edits made while playing the game will not be preserved. Edit the file only with the game exited, please.

2.) At interstellar ranges, heat can sometimes behave strangely, sometimes related to map zoom (be careful zooming out). It is best to turn off part heating when traveling far far away.

3.) When zooming out all the way out in map view at interstellar ranges, the navball furthermore sometimes behaves oddly. We are working on this and monitoring all the interstellar bugs actively.

4.) Very Old craft files may complain about a missing module. This is a cosmetic error and can be ignored. Reload and re-save the craft to remove the error.

Known Caveats:

1.) The 1.12.x release series works on 1.12.x,1.11.x,1.10.x, and 1.9.x. The 1.8 release is for 1.8.x.

2.) Multistar Solar panel support requires an additional config file, attached to release.

3.) As of release-107, scatter density underwent a bugfix on all bodies globally that results in densities acting more dense than before on some select configs.  Some mods may need to adjust.  Normally we'd not change things like this, but this is technically the correct stock behavior of the node so...  if you need the old behavior, see config option UseIncorrectScatterDensityLogic.

4.) As of Release-119, LandControl createColors is no longer obeyed on homeworlds, it is forced off.  Feedback is welcome on this decision, but it is better than the bug we had before (it being forced on and creating neon worlds).  Very few mods to my knowledge use this parameter, but I am open to being proven wrong.

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
