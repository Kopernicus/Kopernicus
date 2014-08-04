Kopernicus Pre-Alpha Release 2
==============================
August 3rd, 2014

Copyright (C) 2014 Bryce C Schroeder (bryce.schroeder@gmail.com)
                   Nathaniel R. Lewis (linux.robotdude@gmail.com) 

About
——
Kopernicus is a KSP add-on that allows for modification of stock planets and the creation of new planets via modification of the system prefab.  Why is this advantageous you might ask?  Previous planet adder mods, such as Planet Factory, modified the live planetary system and had to keep multiple hacks actively running to provide these worlds.  We strive to provide the least hacky solution by introducing planets into the game in the exact same manner Squad would.  

Kopernicus is a one step process.  It is started before the planetary system is created and rewrites a property called PSystemManager.Instance.systemPrefab.  The game itself then creates *our* planetary system as if it were blessed by Squad themselves.  Our very own Brood Parasite!  The mod’s function ends here, and it terminates.  Kopernicus introduced worlds require zero maintenance by third party code, all support is driven entirely by built in functionality.  This yields an incredibly stable and incredibly flexible environment for planetary creation.

One caveat exists that prevents absolute control over a planetary system.  Due to the way Squad defines how launch control and ship recovery work, a planet called Kerbin must exist and must have a reference body id of 1.  While it is easy to provide this scenario, as a star to orbit and a single planet satisfies this, any would be universe architect needs to be aware so they don’t get mystery errors.  

One needs a little knowledge of the working of KSP to understand the reference body IDs.  Each celestial body in KSP has a property called “flightGlobalsIndex.”  This number does not directly correspond to the reference id, but is related.  Once all of the celestial bodies have been spawned, references to them are written into the localBodies list.  This list is then sorted in increasing order of the flight globals index.  The index the body is located at after the sort becomes the reference body id.

We have yet to see if removing or rearranging the other bodies causes any sort of errors in the science or contracts system.


Instructions
——————
1) Copy the contents of the GameData/ folder to KSP’s GameData/ folder
2) Launch KSP and enjoy!
3) Please report any bugs you may find to either or both of the email addresses above.


Information
—————
This release is not configurable, as a configuration system has not been written yet.  A new planet called “Kopernicus” will be added to the game via modification of the system prefab.  This completes the function of the mod, and since the magic is worked before the PSystem spawn, technically, PlanetFactory can actually run on top of this mod. Although I will be required to find you and kill you for doing this.  

Kopernicus uses the real height map and surface texture from Mars, obtained from the Real Solar System texture package.  The license for which is included as RSStextures.txt.

Kopernicus is on a circular, 0 degree inclination orbit between Dres and Jool, which should make travel from Kerbin relatively simple.  It has the same 320 km radius as Duna, but the atmospheric characteristics of Laythe.  The pressure is given via this equation:

pressure (atm) = 0.8 * e^-(altitude / 4000)

The tallest feature on Kopernicus is 9 km above sea level (the summit of Olympus Mons), and the lowest points are at the floor of the Valles Marineris and the northern lowlands.

Three biomes are defined: Dunes, Mares, and Poles - all of which have proper science support.  We also inject the prefab into the science archives, so Kopernicus is visible, along with its science.

The ocean is disabled because it looked stupid (mainly because we don’t fully understand all the available options)

We have yet to explore progress tree integration and contracts.

Debugging Features
—————————
After the Kopernicus.Injector behavior rewrites the system prefab and terminates, a behavior called Kopernicus.RuntimeUtility is started to provide debugging functions.

PQS Inspector (Control-P): Pressing Control-P will explode your console with information about the PQS controller of the body currently being orbited

PQS Quad Visualizer (Control-S, Control-C): Pressing Control-S will draw green lines on the normal vectors to the surface of the planet at the PQ nodes.  This lets the LOD structure be visualized.  Used for testing LOD levels and whether your temperamental ocean just wants to be invisible or is decidedly non present.  Pressing Control-C will remove the lines (and save your framerate).

