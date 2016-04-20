Kopernicus
==============================
April 20th, 2016
* Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
* Maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Majiir (CompatibilityChecker)

New in this version (1.0)
-------------------
* Fixed an issues with subobjects on PQSMods (like landclasses)
* Fixed an issue with PQS on template-less worlds
* Fixed a Material issue in LandControl
* Added the biome map settings "nonExactThreshold" and "exactSearch" to the Properties node
* Added a loader for mapMaxHeight in the PQS node
* Added a loader for lacunarity in VertexHeightNoiseVertHeight
* Added the ability to reference PQSMods by name and index in the component tree in removePQSMods. (removePQSMods = PQSMod_VertexHeightMap[_Height, 1] would remove the second heightmap named _Height)
* Updated the code to make use of the updated KSP 1.1 and Unity 5 API
* Tried to implement async loading of scaled space textures (on demand), but failed because Unity was doing weird things :(
* Removed PQSMod_FixedOblate and PQSMod_FixedOffset - the fixes are now included in the stock versions
* Removed oceanFogColorAltMult because it is gone in KSP
* Fixed an issue with custom orbit modes
* Actually remove hidden bodies in RnD, not just remove their thumbnail and center the name
* Updated the shader wrappers to the 1.1 versions and did some updates to the generator
* Added maxViewDistance override for tracking station to the Kopernicus node
* Added null checks
* Added more null checks
* Fixed OceanFX creation
* Made some changes to make changing the scaled space material possible (again?)
* Only build rings if they aren't built
* Added removeCoronas option in Template that hides the coronas of a star
* Removed Win64 warning from CompatibilityChecker (it might still appear as I didn't bump the version number of CC and other mods could override Kopernicus checker)
* Use GL calls to draw the lines for the SOI debugger instead of Vectrosity, makes them nicer and easier to use
* Made the orbit generation for the asteroids more customizeable (there will be some mods using it out soon, so I won't explain the structure now)
* Added the ability to patch the config node that gets created by the asteroid loader and that contains the vessel that gets created (Savegame VESSEL syntax)
* Automated unloading of MapSO's after being unused for 1 minute
* FINALLY fixed the issue where adding an AFG to a templated planet would cause a black sky and no AFG \o/
* Fixed the previously added null checks
* Added a loader for the fade multiplier in scaled space fader

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
