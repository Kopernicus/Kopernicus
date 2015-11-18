Kopernicus Beta Release 5.2
==============================
November 18th, 2015
* Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
* Maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Majiir (CompatibilityChecker)

New in this version
-------------------
- Implemented working OnDemand-Loading library, for MapSO, CBAttributeMapSO and ScaledSpace Textures
- Removed .mu support for Scatter Meshes, because it doesn't fit well for this purpose
- Added support for BUILTIN/ Biome Maps
- Added operators to convert the internal parsers to their respective values
- Moved custom components into a seperate library
- Complete reengineering of the code. Kopernicus code is now much cleaner, easier to extend and to debug
- ModLoader is now powered by generics, so that the whole namespace is cleaner
- External ModLoaders don't have to use the Namespace Kopernicus.Configuration.ModLoader anymore. Use whatever you want :)
- Removed the custom ring shaders and reverted back to the builtin ones. Fixed the halo-bug differently
- Created components for Rings and the KSC mover, to make them more modular
- Added support for SOI-Debbugging (i.e. making the SOI visible). Use Debug { showSOI = true }
- Removed Debug { exportBin } option, and added exportMesh and update. exportMesh will force Kopernicus to write a mesh (or not), and update will force a ScaledSpace Update, which is neccisary for exportMesh.
- Cleaned up the runtime utility, everything is now in a single class (no KSPAdddon spam)
- Removed the old Finalize System
- Added Body { finalizeOrbit } which will apply the same changes to this single body like the old System did to all bodies
- Implemented BaseLoader, to fetch the currently edited PSystemBody. That allows us to create everything in the background.
- Renamed all loading classes to <ThingItLoads>Loader, to have a consistent naming scheme.
- Added getters and setters to almost all parser targets, to support dynamic generation of Kopernicus configs.
- Official KSP 1.0.5 support
- Removed the non-spherical ocean feature, because PartBuoyancy is way to complex now, so I can't replicate it's behaviour.
- Removed the energy curve for stars in Body {}
- Renamed ScaledVersion { SolarLightColor { } } to Light { }
- Added luminosity and insolation settings to Light { }, to patch the light behaviour that is executed by the FlightIntegrator
- Removed the need for Planetarium.fetch.Sun replacement, SolarPanels are now moddable (thanks NathanKell!)
- Added Majiir's CompatibilityChecker, to lock Kopernicus on unsupported versions
- Moved Ocean { HazardousOcean { HeatCurve { } } } to Ocean { HazardousOcean { } }
- Completely removed the debugging utility, including the exporting tool (Mod+E+P). You are encouraged to use KittopiaTechs exporter.
- Added density value to the Ocean node, to parse the density of the ocean
- Added FogParser to parse the Underwater-Fog from an Ocean { Fog { } } node.
- Many other changes that I forgot, if you find something that has changed, feel free to inform me.

New in 0.5.1
------------
- Fixed a bug where Kopernicus crashes if the body doesn't have a ScaledVersion { } node
- Made more things public, so that other mods can access them.

New in 0.5.2
------------
- Fixed a bug that was introduced through 0.5.1 bugfixing
- Fixed ScaledSpace OnDemand loading for gas giants

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