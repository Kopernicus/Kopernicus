Kopernicus
==============================
June 07, 2019
* Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
* Maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88, Majiir (CompatibilityChecker)
* Much thanks to Sarbian for ModuleManager and ModularFlightIntegrator

New in this version (1.7.1-1)
-------------------
- refactored a large part of the codebase to align more with the coding style
  preferences in my IDE. This also includes merging the three core Kopernicus
  .dlls into one .dll again (+ Kopernicus.Parser).
- Cache more properties of the generated scaled space mesh and preload them,
  to reduce the amount of calculations and disk IO that is done when loading
  planets
- Reduce the amount of IO that is done by the logger
- The `texture` and `normals` values from `ScaledVersion/Material` are no 
  longer loaded on demand. If you want on demand loading for scaled space 
  textures, you have to specify their paths inside of a 
  `ScaledVersion/OnDemand` node, using `texture` and `normals`, just like 
  before.
- Allow loading `fogColorRamp` through a gradient instead of a texture
- Abort loading planets if System.cfg was modified directly. There is no 
  reason to do that except if you want to make your mod unremovable without 
  breaking every other mod. Planet packs are supposed to use ModuleManager 
  (I am looking at you, KerbalGalaxy Remastered)
- Fix an error that prevented bodies with `invisible = True` from staying 
  invisible, unless they templated the sun.
- Take over the mesh generation / spawning for scatter meshes. Stock merges
  them into one big mesh which makes it impossible to detect their actual
  position, or give them sub-objects. Every scatter instance is now it's own
  dedicated object, which greatly simplyfies things.
- Added a `HeatEmitter` scatter component that turns the scatter objects into a 
  heat source.
- Removed `ScatterExperiment` and replaced it with `ModuleSurfaceObjectTrigger`.
  It's a part module that can be added to parts, and enable / disable
  KSPActions when the vessel is near a surface object with a certain name
  (surface objects are scatters and PQSCity). It also sends events to all
  other PartModules on the part, so plugin developers could handle scatter
  with custom logic
- Added a `useBetterDensity` option to the Scatter config. It tries to 
  randomize the density a bit, and avoid rounding it down to an int like stock 
  does. In stock, a scatter can either appear once (or more) per quad, or not 
  at all. The idea behind useBetterDensity is to allow them to spawn, but not 
  strictly on every quad.
- Added a `ignoreDensityGameSetting` option to the Scatter config. If 
  everything fails, this can be used to stop the density game setting from 
  influencing your scatter, so you only have to find one density config that 
  works. The scatter will be treated as if the game setting was set to 100%
- Allow scatter rotation control through the `rotation` option. By default it 
  is set to `rotation = 0 360` which means the scatter can rotate between 0 
  and 360 degrees. Set it to `rotation = 0 0` to fix the rotation
- Allow specifying multiple meshes and randomly selecting one through the 
  `Meshes` node in the scatter config. It can contain multiple entries (choose 
  the key as you like), that point to .obj files in GameData. Kopernicus will 
  load them all and assign one randomly to every scatter object that gets 
  spawned.
- Added a `spawnChance` option in the scatter config, that can be used to fine
  tune density even more. 
- The ScatterColliders component allows specifying a simplified collision mesh
  (.obj), through the `collider` option, as opposed to using the visual mesh for
  collisions
- Added a scatter module that forces the scatters to spawn at sea level (with a
  settable offset), this can be used to create floating objects.
- Readded the ability to set a custom mapMaxHeight for texture exports in 
  Kittopia
  
__ATTENTION:__
- This release will break plugins that depend on Kopernicus
- While the old files still work, it is recommended to regenerate your scaled
  space cache, to take advantage of the improved format and caching
- You have to adjust your configs to get scaled space OD working again
- Since this release reduces the amount of dlls that come with Kopernicus, you
  __have__ to delete the Kopernicus folder from GameData before installing. Do
  not __overwrite__ the folder. CKAN users probably have to uninstall and 
  reinstall Kopernicus.

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
