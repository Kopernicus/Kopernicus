Kopernicus Beta Release 6
==============================
December 12th, 2015
* Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
* Maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Majiir (CompatibilityChecker)

New in this version
-------------------
- Fixed the Biome-View in Tracking Station
- Fixed PQSMaterials
- Fixed LandControl
- Added the ability to create multiple Particle Emitters with custom meshes (mesh = <path>.obj) and scales (scale = 1.0, 1.0, 1.0)
- Particles are now in a Particles {} node (Body { Particles { Particle { <your definition> } Particle { <your other definition> } } })
- Added early support for particle collisions, toggleable using collide = true in the Particle settings (defaults to false)
- Added the ability to apply a constant force to the particles (force = 1.0, 1.0, 1.0)
- Added asteroid customization through Asteroid { } nodes in the Kopernicus { } node. See System.cfg for the stock configuration
- Added support for HSBA() colors (0-255)
- Fixed AtmosphereFromGround loading
- Removed Debug Spam and fixed some exceptions in the parser
- The SOI-Debugging lines are better visible and nicer now
- The changes to the main menu should persist between scene changes
- EVE clouds should get copied to the main menu again
- Added visibility tweaking (icon and mode in Orbit { } and selectable in Properties { })
- PQSMods are now sorted using the order value when caching the scaled space
- Various other fixes and things that I forgot

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