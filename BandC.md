Known Bugs:

1.) Not exactly a bug, but worth mentioning: The Kopernicus_Config.cfg file is rewritten when the game exits. This means any manual (not in the GUI) edits made while playing the game will not be preserved. Edit the file only with the game exited, please.

2.) At interstellar ranges, heat can sometimes behave strangely, sometimes related to map zoom (be careful zooming out). It is best to turn off part heating when traveling far far away. (I am not sure if this is still releavent as of Release-159, feedback welcome).

3.) When zooming out all the way out in map view at interstellar ranges, the navball furthermore sometimes behaves oddly. We are working on this and monitoring all the interstellar bugs actively. (I am not sure if this is still releavent as of Release-159, feedback welcome).

4.) Very Old craft files may complain about a missing module. This is a cosmetic error and can be ignored. Reload and re-save the craft to remove the error.

5.) Sometimes when reloading a quicksave to KSC, you will get the KSC sunken into the ground. This is cosmetic only, another reload of the same save will fix it. (This error has been around forever, just now listing it).

6.) When you uninstall a mod that had installed a Terrain Detail preset you were using, it may be listed still in the Graphics settings as "New Text." This is by design. If it bothers you, please reinstall the mod that setup that preset, or delete settings.cfg and let it regenerate.

7.) Some mods that used custom Terrain Presets may require you to delete your settings.cfg file and reset your settings with this release. This is rare, but can happen. See [this](https://forum.kerbalspaceprogram.com/index.php?/topic/200143-112x-kopernicus-stable-branch-last-updated-march-7th-2023/&do=findComment&comment=4258139) post for details

Known Caveats:

1.) The 1.12.x release series works on 1.12.x. The 1.11.x,1.10.x,1.9.x and 1.8.x releases are deprecated

2.) Multistar Solar panel support requires an additional config file, attached to release.

3.) As of release-107, scatter density underwent a bugfix on all bodies globally that results in densities acting more dense than before on some select configs. Some mods may need to adjust. Normally we'd not change things like this, but this is technically the correct stock behavior of the node so... if you need the old behavior, see config option UseIncorrectScatterDensityLogic.

4.) As of release-151, polar generation behavior has changed slightly. Though it will be safer overall for new missions, be careful loading existing craft there. This is probably not lethal but I don't want you to be unaware. Maybe make a save just in case? ;)

5.) The "collider fix" as it's called, which fixes the event in which you sink into the terrain on distant bodies, is now on by default. If you really need distant colliders, turn this off, but you'd best have a good reason (I can't think of any).

6.) The particle system was hopelessly broken and has been since sometime past 1.10.x. Few mods used it, so it has been removed completely as of Release-146.

7.) Because we now unpack multipart PQSCity's correctly, you may find some PQSCity structures are in the earth or floating. Report such bugs to your planet pack author as this is an intended change (only cosmetic).