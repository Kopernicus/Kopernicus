Kopernicus
==============================
February 04, 2020
* Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
* Actively maintained by: Prestja and R-T-B.
* Formerly maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Democat3457, Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88, Majiir (CompatibilityChecker)
* Much thanks to Sarbian for ModuleManager and ModularFlightIntegrator

New in this version (1.9.1-7)
-------------------
1.) We no longer enforce the default shader setting, you may choose whatever you wish by default. Planet pack authors may set restrictions, but we have none out the gate.

2.) Planet pack authors (or you) may now specify the packs target shader level by by setting config/Kopernicus_Config.cfg EnforcedShaderLeve. 3 = Ultra/Atlas, 2 = High, 1 = Medium, and 0 = Low. Planet pack authors may use this via modulemanager module if needed as well. Note that we encourage of the KSP Parallax shader project over use of Squad's ATLAS shader. It is both equally performant and more flexible.

New in major release (1.9.1)

1.) The longstanding ringshader bugs have been fixed.

2.) Performance Optimization and bugfixes enabling KittopiaTech support.

3.) JNSQ & other large world "Farm patch" bug fixed.

4.) Particle support restored.

5.) All 1.9.1 bugs that are known fixed. Full support for 1.9.1

5.) Multistar support. The math on ECs in single star may be slightly different than stock in some situations, but it should be similar in most and not enough to matter.

4.) Added GameData\Kopernicus\Config\Kopernicus_Config.cfg file, with options to configure shader warnings and enable or disable shader locking, as well as set preferred shader level. Easy to edit, just look inside!

Note - reparenting Kerbin or the Sun can cause the sky to be incorrect in the space center view. It is, however, correct in the flight view and the flight map view.

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
