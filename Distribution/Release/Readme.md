Kopernicus
==============================
June 22th, 2016
* Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
* Maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Majiir (CompatibilityChecker)
* Much thanks to Sarbian for ModuleManager and ModularFlightIntegrator

New in this version (1.1.2)
-------------------
* Fix mesh colliders very often being out-of-sync with renderers in KSP 1.1.
* ScaledSpace OnDemand optimisations by Padishar
* Fixed the Asteroid ScenarioModule, because it prevented other Scenarios from getting spawned
* Added support for multiple Lensflares
* Added a dependency on ModularFlightIntegrator, and rewrote the Flux code to support heat from multiple stars
* Made the spacecenter scene less buggy (it is still broken like hell, and I dont find the component responsible for it)
* Added a loader for star radiation
* Fixed the luminosity code for stars
* Added a switch to prevent an AFG from getting created (Atmosphere { addAFG = false })
* Randomization for the main menu body
* Fix for main menu bodies using cbNameLater
* Fixed BUILTIN MapSO
* Experimental support for custom lensflares, from Unity asset bundles (Light { sunFlare = Kopernicus/Files/flare.unity3d:nameoftheasset)
* Add new navballSwitchRadiusMultLow to Properties
* 1.1.3 API changes (sunDotTransform)

Note - reparenting Kerbin or the Sun causes the sky to be incorrect in the space center view. It is, however, correct in the flight view and the flight map view.  Reparenting the sun causes other stars positions to not update in the tracking station for some reason.

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

Information
-----------
Coming Soon

Compiling
----------
To compile Kopernicus you need to add the following assemblies from your KSP_Data/Managed folder into the toplevel directory of the repository:

* Assembly-CSharp.dll
* Assembly-CSharp-firstpass.dll
* KSPUtil.dll
* UnityEngine.dll
* UnityEngine.UI.dll

After that you can compile the .sln file with VS, MonoDevelop or other build tools. If you have Linux experience you can use Makefiles too, however, you will need Microsofts Roslyn compiler for it. 
