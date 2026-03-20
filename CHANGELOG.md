# Kopernicus Changelog

## 242
1. Worked around a floating point hack that was interfering with burstPQS (resulting in gemoetric looking terrain in flight), now they play nicely.
2. A handful of tweaks to OnDemand, allowing it to swap textures in/out more efficiently.
3. We now use a CI to make releases.  It should be fine! (Pray for our automated souls, just in case).

## 241
1. Hotfixed a bug where using a BUILTIN/ texture path once could render it unusable in the future.  
   This was aparently introduced in r239.

## 240
1. Fixed a bug where all planet textures were being loaded as greyscale textures.
2. Fixed a number of other small bugs in `KopernicusMapSO`.
3. Fixed a bug where the homeworldname would not reset when uninstalling mods.
4. Changed default Koperncius_Config.cfg setting for UseOnDemandLoader to True. This 
   means new installs will have that set by default.  We aren't changing the setting 
   in existing installs yet, but may in the future as a one-time change as it really
   performs much better now (no real stutter to speak of). Consider switching to it, 
   it saves a good chunk of ram!

## 239
1. Kopernicus no longer errors when texture paths are left blank in configs.
   Instead we now use a default texture - either black or pink depending on map depth.
2. MapSODemand now memory maps dds textures by default, and otherwise reuses the memory
   of textures loaded from asset bundles. This should (hopefully) visibly reduce memory
   usage.
3. Fixed a bug where reading `MapSODemand.Width`/`Height` would return 0 before the
   first time a MapSODemand is loaded, causing `VertexHeightMitchellNetravali` to return
   NaN and therefore causing crashes with principia.
4. Fixed a bug where kopernicus was occasionally causing the next scene to be loaded
   one frame too soon, completely breaking KSPs UI.

## 238
1. Revert PR #760 as well since it was also causing inflated memory usage.

## 237
1. Revert PR #766 since it was causing inflated memory usage.

## 236
1. Minor improvements to load time and memory usage (#754, #748).
2. Improve tracking of solar panel state (#759 by @aebestach).
3. Make supported MapSO features consistent no matter whether on-demand storage
   is enabled (#760).

## 235
1. Major performance improvments in loading time and elsewhere courtesy of KSPTextureLoader.
2. Fixed broken asteroid density rescaling facilities for stock and Kopernicus generator.
3. R16 height maps are now natively supported by `MapSODemand`.

## 234
To see the changelog for previous versions you will need to look at the relevant github release.
