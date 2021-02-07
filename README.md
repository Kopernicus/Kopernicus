Kopernicus
==============================
February 07, 2021
* Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
* Actively maintained by: Prestja and R-T-B.
* Formerly maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Democat3457, Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88, Majiir (CompatibilityChecker)
* Much thanks to Sarbian for ModuleManager and ModularFlightIntegrator

New in this latest version (release-27)

1.) 1.10.1 and 1.11.1 support has been added to stable branch.

2.) Kopernicus_Config.cfg now accepts parameter UseKopernicusAsteroidSystem with accepted values "true" (default, use Kopernicus Custom Asteroid Generator and scrub Sentinel contracts), "false" (use external mod asteroid generator, or none at all), and "stock" (uses stock game generator, only good for stock system). These values are case insensitive and DO NOT use quotes.

3.) Various vetted bugfixes to many things (shaders, solar code, etc) from the bleeding edge have been imported.  This fixes nearly all known bugs across all platforms (other than those listed below), and should result in a very stable experience.

4.) Performance improvements were managed as well, also taken from tested bleeding edge experiments.

5.) ModularFlightIntegrator updated to latest mainline 1.2.9 (you may update this with updates sourced from Sarbian freely).

Known Bugs:

1.) At interstellar ranges, heat can sometimes behave strangely. It is best to turn off part heating when traveling far far away.

2.) When zooming out all the way in map view at interstellar ranges, the navbal sometimes behaves oddly. We are working on this and all the interstellar bugs actively.


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
