/**
 * Kopernicus Planetary System Modifier
 * Copyright (C) 2014 Bryce C Schroeder (bryce.schroeder@gmail.com), Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * http://www.ferazelhosting.net/~bryce/contact.html
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 * 
 * This library is intended to be used as a plugin for Kerbal Space Program
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using Kopernicus.MaterialWrapper;

namespace Kopernicus
{
    // This class will add a planet. It will be supplanted by
    // versions which do more useful things, such as loading a planet from
    // configuration files, loading and modifying a stock planet from KSP's
    // resources (e.g. to allow modders to create modifications of the stock
    // Kerbol system, or to support backward compatibility with PlanetFactory),
    // or even procedurally generating a planet.
    // DOES NOT CURRENTLY HANDLE THE CASE OF GENERATING THE ROOT (Sun)
    // This code necessarily has side effects of execution, because of the way AddBody
    // works (we can't make a planet *not* in a system and return it.)
    public class KopernicusPlanetSource
    {
        // Path of the plugin (will eventually not matter much)
        const string PluginDirectory = "GameData/Kopernicus/PluginData/";

        public static PSystemBody GeneratePlanet (PSystem system, string name, Orbit orbit = null) 
        {
            return GenerateSystemBody (system, system.rootBody, name, orbit);
        }

        public static PSystemBody GenerateSystemBody(PSystem system, PSystemBody parent, String name, Orbit orbit = null) 
        {
            PSystemBody Jool = Utility.FindBody (system.rootBody, "Jool");  // Need the geosphere for scaled version
            PSystemBody Laythe = Utility.FindBody (system.rootBody, "Laythe"); // Need pqs and ocean definitions

            //Utility.DumpObject (Laythe.celestialBody, " Laythe Celestial Body ");
            //Utility.DumpObject (Laythe.pqsVersion, " Laythe PQS ");
            //Transform laytheOcean = Utility.FindInChildren (Laythe.pqsVersion.transform, "LaytheOcean");
            //Utility.DumpObject (laytheOcean.GetComponent<PQS> (), " Laythe Ocean PQS ");

            // AddBody makes the GameObject and stuff. It also attaches it to the system and parent.
            PSystemBody body = system.AddBody (parent);

            // set up the various parameters
            body.name = name;
            body.flightGlobalsIndex = 100;

            // Some parameters of the celestialBody, which represents the actual planet...
            // PSystemBody is more of a container that associates the planet with its orbit 
            // and position in the planetary system, etc.
            body.celestialBody.bodyName               = name;
            body.celestialBody.bodyDescription        = "Merciful Kod, this thing just APPEARED! And unlike last time, it wasn't bird droppings on the telescope.";
            body.celestialBody.Radius                 = 320000;
            //body.celestialBody.Radius                 = 3380100;
            body.celestialBody.GeeASL                 = 0.3;
            //body.celestialBody.Mass                   = 6.4185E+23;
            body.celestialBody.Mass                   = 4.5154812E+21;
            body.celestialBody.timeWarpAltitudeLimits = (float[])Laythe.celestialBody.timeWarpAltitudeLimits.Clone();
            body.celestialBody.rotationPeriod         = 88642.6848;
            body.celestialBody.rotates                = true;
            body.celestialBody.BiomeMap               = GenerateCBAttributeMapSO(name);//Dres.celestialBody.BiomeMap;//
            body.celestialBody.scienceValues          = Laythe.celestialBody.scienceValues;
            body.celestialBody.ocean                  = false;

            // Presumably true of Kerbin. I do not know what the consequences are of messing with this exactly.
            // I think this just affects where the Planetarium/Tracking station starts.
            body.celestialBody.isHomeWorld            = false;

            // Setup the orbit of "Kopernicus."  The "Orbit" class actually is built to support serialization straight
            // from Squad, so storing these to files (and loading them) will be pretty easy.
            body.orbitRenderer.orbitColor             = Color.magenta;
            body.orbitDriver.celestialBody            = body.celestialBody;
            body.orbitDriver.updateMode               = OrbitDriver.UpdateMode.UPDATE;
            if (orbit == null) 
                body.orbitDriver.orbit = new Orbit (0.0, 0.0, 47500000000, 0, 0, 0, 0, system.rootBody.celestialBody);
            else
                body.orbitDriver.orbit = orbit;


            #region PSystemBody.pqsVersion generation

            // Create the PQS controller game object for Kopernicus
            GameObject controllerRoot       = new GameObject(name);
            controllerRoot.layer            = Constants.GameLayers.LocalSpace;
            controllerRoot.transform.parent = Utility.Deactivator;

            // Create the PQS object and pull all the values from Dres (has some future proofing i guess? adapts to PQS changes)
            body.pqsVersion = controllerRoot.AddComponent<PQS>();
            Utility.CopyObjectFields(Laythe.pqsVersion, body.pqsVersion);
            //body.pqsVersion.surfaceMaterial = new PQSProjectionSurfaceQuad();
            //body.pqsVersion.fallbackMaterial = new PQSProjectionFallback();
            body.pqsVersion.surfaceMaterial = new PQSProjectionAerialQuadRelative(Laythe.pqsVersion.surfaceMaterial); // use until we determine all the functions of the shader textures
            body.pqsVersion.fallbackMaterial = new PQSProjectionFallback(Laythe.pqsVersion.fallbackMaterial);
            body.pqsVersion.radius = body.celestialBody.Radius;
            body.pqsVersion.mapOcean = false;

            // Debug
            Utility.DumpObjectProperties(body.pqsVersion.surfaceMaterial, " Surface Material ");
            Utility.DumpObjectProperties(body.pqsVersion.fallbackMaterial, " Fallback Material ");

            // Detail defaults
            body.pqsVersion.maxQuadLenghtsPerFrame = 0.03f;
            body.pqsVersion.minLevel = 1;
            body.pqsVersion.maxLevel = 10;
            body.pqsVersion.minDetailDistance = 8;

            // Create the celestial body transform
            GameObject mod = new GameObject("_CelestialBody");
            mod.transform.parent = controllerRoot.transform;
            PQSMod_CelestialBodyTransform celestialBodyTransform = mod.AddComponent<PQSMod_CelestialBodyTransform>();
            celestialBodyTransform.sphere = body.pqsVersion;
            celestialBodyTransform.forceActivate = false;
            celestialBodyTransform.deactivateAltitude = 115000;
            celestialBodyTransform.forceRebuildOnTargetChange = false;
            celestialBodyTransform.planetFade = new PQSMod_CelestialBodyTransform.AltitudeFade();
            celestialBodyTransform.planetFade.fadeFloatName = "_PlanetOpacity";
            celestialBodyTransform.planetFade.fadeStart = 100000.0f;
            celestialBodyTransform.planetFade.fadeEnd = 110000.0f;
            celestialBodyTransform.planetFade.valueStart = 0.0f;
            celestialBodyTransform.planetFade.valueEnd = 1.0f;
            celestialBodyTransform.planetFade.secondaryRenderers = new List<GameObject>();
            celestialBodyTransform.secondaryFades = new PQSMod_CelestialBodyTransform.AltitudeFade[0];
            celestialBodyTransform.requirements = PQS.ModiferRequirements.Default;
            celestialBodyTransform.modEnabled = true;
            celestialBodyTransform.order = 10;

            // Create the color PQS mods
            mod = new GameObject("_Color");
            mod.transform.parent = controllerRoot.transform;
            PQSMod_VertexSimplexNoiseColor vertexSimplexNoiseColor = mod.AddComponent<PQSMod_VertexSimplexNoiseColor>();
            vertexSimplexNoiseColor.sphere = body.pqsVersion;
            vertexSimplexNoiseColor.seed = 45;
            vertexSimplexNoiseColor.blend = 1.0f;
            vertexSimplexNoiseColor.colorStart = new Color(0.768656731f, 0.6996614f, 0.653089464f, 1);
            vertexSimplexNoiseColor.colorEnd = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            vertexSimplexNoiseColor.octaves = 12.0;
            vertexSimplexNoiseColor.persistence = 0.5;
            vertexSimplexNoiseColor.frequency = 2.0;
            vertexSimplexNoiseColor.requirements = PQS.ModiferRequirements.MeshColorChannel;
            vertexSimplexNoiseColor.modEnabled = true;
            vertexSimplexNoiseColor.order = 200;

            PQSMod_HeightColorMap heightColorMap = mod.AddComponent<PQSMod_HeightColorMap>();
            heightColorMap.sphere = body.pqsVersion;
            List<PQSMod_HeightColorMap.LandClass> landClasses = new List<PQSMod_HeightColorMap.LandClass>();

            PQSMod_HeightColorMap.LandClass landClass = new PQSMod_HeightColorMap.LandClass("AbyPl", 0.0, 0.5, new Color(0.0f, 0.0f, 0.0f, 1.0f), Color.white, double.NaN);
            landClass.lerpToNext = true;
            landClasses.Add(landClass);

            landClass = new PQSMod_HeightColorMap.LandClass("Beach", 0.5, 0.550000011920929, new Color(0.164179087f, 0.164179087f, 0.164179087f, 1.0f), Color.white, double.NaN);
            landClass.lerpToNext = true;
            landClasses.Add(landClass);

            landClass = new PQSMod_HeightColorMap.LandClass("Beach", 0.550000011920929, 1.0, new Color(0.373134315f, 0.373134315f, 0.373134315f, 1.0f), Color.white, double.NaN);
            landClass.lerpToNext = false;
            landClasses.Add(landClass);

            // Generate an array from the land classes list
            heightColorMap.landClasses = landClasses.ToArray();
            heightColorMap.blend = 0.7f;
            heightColorMap.lcCount = 3;
            heightColorMap.requirements = PQS.ModiferRequirements.MeshColorChannel;
            heightColorMap.modEnabled = true;
            heightColorMap.order = 201;

            // Create the alititude alpha mods
            mod = new GameObject("_Material_ModProjection");
            mod.transform.parent = controllerRoot.transform;
            PQSMod_AltitudeAlpha altitudeAlpha = mod.AddComponent<PQSMod_AltitudeAlpha>();
            altitudeAlpha.sphere = body.pqsVersion;
            altitudeAlpha.atmosphereDepth = 4000.0;
            altitudeAlpha.invert = false;
            altitudeAlpha.requirements = PQS.ModiferRequirements.Default;
            altitudeAlpha.modEnabled = false;
            altitudeAlpha.order = 999999999;

            // Create the aerial perspective material
            mod = new GameObject("_Material_AerialPerspective");
            mod.transform.parent = controllerRoot.transform;
            PQSMod_AerialPerspectiveMaterial aerialPerspectiveMaterial = mod.AddComponent<PQSMod_AerialPerspectiveMaterial>();
            aerialPerspectiveMaterial.sphere = body.pqsVersion;
            aerialPerspectiveMaterial.globalDensity = -0.00001f;
            aerialPerspectiveMaterial.heightFalloff = 6.75f;
            aerialPerspectiveMaterial.atmosphereDepth = 150000;
            aerialPerspectiveMaterial.DEBUG_SetEveryFrame = true;
            aerialPerspectiveMaterial.cameraAlt = 0;
            aerialPerspectiveMaterial.cameraAtmosAlt = 0;
            aerialPerspectiveMaterial.heightDensAtViewer = 0;
            aerialPerspectiveMaterial.requirements = PQS.ModiferRequirements.Default;
            aerialPerspectiveMaterial.modEnabled = true;
            aerialPerspectiveMaterial.order = 100;

            // Create the UV planet relative position
            mod = new GameObject("_Material_SurfaceQuads");
            mod.transform.parent = controllerRoot.transform;
            PQSMod_UVPlanetRelativePosition planetRelativePosition = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
            planetRelativePosition.sphere = body.pqsVersion;
            planetRelativePosition.requirements = PQS.ModiferRequirements.Default;
            planetRelativePosition.modEnabled = true;
            planetRelativePosition.order = 999999;

            // Create the height noise module
            mod = new GameObject("_HeightNoise");
            mod.transform.parent = controllerRoot.transform;
            PQSMod_VertexHeightMap vertexHeightMap = mod.gameObject.AddComponent<PQSMod_VertexHeightMap>();
            vertexHeightMap.sphere = body.pqsVersion;
            //vertexHeightMap.heightMapDeformity = 29457.0;
            vertexHeightMap.heightMapDeformity = 10000.0;
            vertexHeightMap.heightMapOffset = -1000.0;
            vertexHeightMap.scaleDeformityByRadius = false;
            vertexHeightMap.requirements = PQS.ModiferRequirements.MeshCustomNormals | PQS.ModiferRequirements.VertexMapCoords;
            vertexHeightMap.modEnabled = true;
            vertexHeightMap.order = 20;

            // Load the heightmap for this planet
            Texture2D map = new Texture2D(4, 4, TextureFormat.Alpha8, false);
            map.LoadImage(System.IO.File.ReadAllBytes(KSPUtil.ApplicationRootPath + PluginDirectory + "/Planets/" + name + "/Height.png"));
            vertexHeightMap.heightMap = ScriptableObject.CreateInstance<MapSO>();
            vertexHeightMap.heightMap.CreateMap(MapSO.MapDepth.Greyscale, map);
            UnityEngine.Object.DestroyImmediate(map);

            // Create the simplex height module
            PQSMod_VertexSimplexHeight vertexSimplexHeight = mod.AddComponent<PQSMod_VertexSimplexHeight>();
            vertexSimplexHeight.sphere = body.pqsVersion;
            vertexSimplexHeight.seed = 670000;
            vertexSimplexHeight.deformity = 1700.0;
            vertexSimplexHeight.octaves = 12.0;
            vertexSimplexHeight.persistence = 0.5;
            vertexSimplexHeight.frequency = 4.0;
            vertexSimplexHeight.requirements = PQS.ModiferRequirements.MeshCustomNormals;
            vertexSimplexHeight.modEnabled = true;
            vertexSimplexHeight.order = 21;

            // SERIOUSLY RECOMMENDED FOR NO OCEAN WORLDS
            // Create the flatten ocean module
            PQSMod_FlattenOcean flattenOcean = mod.AddComponent<PQSMod_FlattenOcean>();
            flattenOcean.sphere = body.pqsVersion;
            flattenOcean.oceanRadius = 1.0;
            flattenOcean.requirements = PQS.ModiferRequirements.MeshCustomNormals;
            flattenOcean.modEnabled = true;
            flattenOcean.order = 25;

            // Creat the vertex height noise module
            PQSMod_VertexHeightNoise vertexHeightNoise = mod.AddComponent<PQSMod_VertexHeightNoise>();
            vertexHeightNoise.sphere = body.pqsVersion;
            vertexHeightNoise.noiseType = PQSMod_VertexHeightNoise.NoiseType.RiggedMultifractal;
            vertexHeightNoise.deformity = 1000.0f;
            vertexHeightNoise.seed = 5906;
            vertexHeightNoise.frequency = 2.0f;
            vertexHeightNoise.lacunarity = 2.5f;
            vertexHeightNoise.persistance = 0.5f;
            vertexHeightNoise.octaves = 4;
            vertexHeightNoise.mode = LibNoise.Unity.QualityMode.Low;
            vertexHeightNoise.requirements = PQS.ModiferRequirements.MeshColorChannel;
            vertexHeightNoise.modEnabled = true;
            vertexHeightNoise.order = 22;

            // Create the material direction
            mod = new GameObject("_Material_SunLight");
            mod.transform.parent = controllerRoot.gameObject.transform;
            PQSMod_MaterialSetDirection materialSetDirection = mod.AddComponent<PQSMod_MaterialSetDirection>();
            materialSetDirection.sphere = body.pqsVersion;
            materialSetDirection.valueName = "_sunLightDirection";
            materialSetDirection.requirements = PQS.ModiferRequirements.Default;
            materialSetDirection.modEnabled = true;
            materialSetDirection.order = 100;

            // Crete the quad mesh colliders
            mod = new GameObject("QuadMeshColliders");
            mod.transform.parent = controllerRoot.gameObject.transform;
            PQSMod_QuadMeshColliders quadMeshColliders = mod.AddComponent<PQSMod_QuadMeshColliders>();
            quadMeshColliders.sphere = body.pqsVersion;
            quadMeshColliders.maxLevelOffset = 0;
            quadMeshColliders.physicsMaterial = new PhysicMaterial();
            quadMeshColliders.physicsMaterial.name = "Ground";
            quadMeshColliders.physicsMaterial.dynamicFriction = 0.6f;
            quadMeshColliders.physicsMaterial.staticFriction = 0.8f;
            quadMeshColliders.physicsMaterial.bounciness = 0.0f;
            quadMeshColliders.physicsMaterial.frictionDirection2 = Vector3.zero;
            quadMeshColliders.physicsMaterial.dynamicFriction2 = 0.0f;
            quadMeshColliders.physicsMaterial.staticFriction2 = 0.0f;
            quadMeshColliders.physicsMaterial.frictionCombine = PhysicMaterialCombine.Maximum;
            quadMeshColliders.physicsMaterial.bounceCombine = PhysicMaterialCombine.Average;
            quadMeshColliders.requirements = PQS.ModiferRequirements.Default;
            quadMeshColliders.modEnabled = true;
            quadMeshColliders.order = 100;

            // Create the simplex height absolute
            mod = new GameObject("_FineDetail");
            mod.transform.parent = controllerRoot.gameObject.transform;
            PQSMod_VertexSimplexHeightAbsolute vertexSimplexHeightAbsolute = mod.AddComponent<PQSMod_VertexSimplexHeightAbsolute>();
            vertexSimplexHeightAbsolute.sphere = body.pqsVersion;
            vertexSimplexHeightAbsolute.seed = 4234;
            vertexSimplexHeightAbsolute.deformity = 400.0;
            vertexSimplexHeightAbsolute.octaves = 6.0;
            vertexSimplexHeightAbsolute.persistence = 0.5;
            vertexSimplexHeightAbsolute.frequency = 18.0;
            vertexSimplexHeightAbsolute.requirements = PQS.ModiferRequirements.Default;
            vertexSimplexHeightAbsolute.modEnabled = true;
            vertexSimplexHeightAbsolute.order = 30;

            // Surface color map
            mod = new GameObject("_LandClass");
            mod.transform.parent = body.pqsVersion.gameObject.transform;
            PQSMod_VertexColorMap colorMap = mod.AddComponent<PQSMod_VertexColorMap>();
            colorMap.sphere = body.pqsVersion;
            colorMap.order = 500;
            colorMap.modEnabled = true;

            // Decompress and load the color
            map = new Texture2D(4, 4, TextureFormat.RGB24, false);
            map.LoadImage(System.IO.File.ReadAllBytes(KSPUtil.ApplicationRootPath + PluginDirectory + "/Planets/" + name + "/Color.png"));
            colorMap.vertexColorMap = ScriptableObject.CreateInstance<MapSO>();
            colorMap.vertexColorMap.CreateMap(MapSO.MapDepth.RGB, map);
            UnityEngine.Object.DestroyImmediate(map);

            #endregion

            #region PSystemBody.scaledVersion generation

            // Create the scaled version of the planet for use in map view
            body.scaledVersion = new GameObject(name);
            body.scaledVersion.layer = Constants.GameLayers.ScaledSpace;
            body.scaledVersion.transform.parent = Utility.Deactivator;

            // DEPRECATED - USE PQSMeshWrapper
            // Make sure the scaled version cooresponds to the size of the body
            // Turns out that the localScale is directly related to the planet size.  
            // Jool's local scale is {1,1,1}, Kerbin's is {0.1,0.1,0.1}.  Jool's 
            // radius is 6000 km, Kerbin's is 600 km.  Notice the relation?
            float scale = (float) body.celestialBody.Radius / 6000000.0f;
            body.scaledVersion.transform.localScale = new Vector3(scale, scale, scale);

            // Generate a mesh to fit the PQS we generated (it would be cool to generate this FROM the PQS)
            Mesh mesh = new Mesh();
            Utility.CopyMesh(Jool.scaledVersion.GetComponent<MeshFilter>().sharedMesh, mesh);

            // Iterate though the UVs
            // Geosphere with a radius of 1000, cooresponds to an object 6000km in radius
            Vector3[] vertices = mesh.vertices;
            for(int i = 0; i < mesh.vertexCount; i++)
            {
                // Get the height offset from the height map
                Vector2 uv = mesh.uv[i];
                float displacement = vertexHeightMap.heightMap.GetPixelFloat(uv.x, uv.y);

                // Since this is a geosphere, normalizing the vertex gives the vector to translate on
                Vector3 v = vertices[i];
                v.Normalize();

                // Calculate the real height displacement (in meters), normalized vector "v" scale (1 unit = 6 km)
                displacement = (float) vertexHeightMap.heightMapOffset + (displacement * (float) vertexHeightMap.heightMapDeformity);
                Vector3 offset = v * ((displacement / 6000.0f) / scale);

                // Adjust the displacement
                vertices[i] += offset;
            }
            mesh.vertices = vertices;
            mesh.RecalculateNormals();

            // Create the mesh filter
            MeshFilter meshFilter = body.scaledVersion.AddComponent<MeshFilter> ();
            meshFilter.mesh = mesh;

            // Load and compress the color texture for the custom planet
            Texture2D colorTexture = new Texture2D(4, 4, TextureFormat.RGBA32, true);
            colorTexture.LoadImage(System.IO.File.ReadAllBytes(KSPUtil.ApplicationRootPath + PluginDirectory + "/Planets/" + name + "/Color.png"));
            colorTexture.Compress(true);
            colorTexture.Apply(true, true);

            // Load and compress the color texture for the custom planet
            Texture2D normalTexture = new Texture2D(4, 4, TextureFormat.RGB24, true);
            normalTexture.LoadImage(System.IO.File.ReadAllBytes(KSPUtil.ApplicationRootPath + PluginDirectory + "/Planets/" + name + "/Normals.png"));
            //normalTexture = GameDatabase.BitmapToUnityNormalMap(normalTexture);
            normalTexture.Compress(true);
            normalTexture.Apply(true, true);

            // Create the renderer and material for the scaled version
            MeshRenderer renderer = body.scaledVersion.AddComponent<MeshRenderer>();
            //ScaledPlanetSimple material = new ScaledPlanetSimple();   // for atmosphereless planets
            ScaledPlanetRimAerial material = new ScaledPlanetRimAerial();
            material.color       = Color.white;
            material.specColor   = Color.black;
            material.mainTexture = colorTexture;
            material.bumpMap     = normalTexture;
            renderer.material    = material;

            // Create the sphere collider
            SphereCollider collider = body.scaledVersion.AddComponent<SphereCollider> ();
            collider.center         = Vector3.zero;
            collider.radius         = 1000.0f;

            // Create the ScaledSpaceFader to fade the orbit out where we view it (maybe?)
            ScaledSpaceFader fader = body.scaledVersion.AddComponent<ScaledSpaceFader> ();
            fader.celestialBody    = body.celestialBody;
            fader.fadeStart        = 95000.0f;
            fader.fadeEnd          = 100000.0f;
            fader.floatName        = "_Opacity";

            #endregion

            #region Atmosphere 
            //--------------------- PROPERTIES EXCLUSIVE TO BODIES WITH ATMOSPHERE

            // Load the atmosphere gradient (compress it, does not need to be high quality)
            Texture2D atmosphereGradient = new Texture2D(4, 4, TextureFormat.RGB24, true);
            atmosphereGradient.LoadImage(System.IO.File.ReadAllBytes(KSPUtil.ApplicationRootPath + PluginDirectory + "/Planets/" + name + "/AtmosphereGradient.png"));
            atmosphereGradient.Compress(true);
            atmosphereGradient.wrapMode = TextureWrapMode.Clamp;
            atmosphereGradient.mipMapBias = 0.0f;
            atmosphereGradient.Apply(true, true);

            // Set the additional settings in the scaledVersion body's shader
            material.rimPower = 2.06f;
            material.rimBlend = 0.3f;
            material.rimColorRamp = atmosphereGradient;

            // Atmosphere specific properties (for scaled version root) (copied from duna) 
            MaterialSetDirection materialLightDirection = body.scaledVersion.AddComponent<MaterialSetDirection>();
            materialLightDirection.valueName            = "_localLightDirection";

            // Create the atmosphere shell itself
            GameObject scaledAtmosphere               = new GameObject("atmosphere");
            scaledAtmosphere.transform.parent         = body.scaledVersion.transform;
            scaledAtmosphere.layer                    = Constants.GameLayers.ScaledSpaceAtmosphere;
            meshFilter                                = scaledAtmosphere.AddComponent<MeshFilter>();
            meshFilter.sharedMesh                     = Jool.scaledVersion.GetComponent<MeshFilter>().sharedMesh;
            renderer                                  = scaledAtmosphere.AddComponent<MeshRenderer>();
            renderer.material                         = new Kopernicus.MaterialWrapper.AtmosphereFromGround();
            AtmosphereFromGround atmosphereRenderInfo = scaledAtmosphere.AddComponent<AtmosphereFromGround>();
            atmosphereRenderInfo.waveLength           = new Color(0.509f, 0.588f, 0.643f, 0.000f);

            // Technical info for atmosphere
            body.celestialBody.atmosphere = true;
            body.celestialBody.atmosphereContainsOxygen = true;
            body.celestialBody.staticPressureASL = 1.0; // can't find anything that references this, especially with the equation in mind - where is this used?
            body.celestialBody.altitudeMultiplier = 1.4285f; // ditto
            body.celestialBody.atmosphereScaleHeight = 4.0;   // pressure (in atm) = atmosphereMultipler * e ^ -(altitude / (atmosphereScaleHeight * 1000))
            body.celestialBody.atmosphereMultiplier = 0.8f;
            body.celestialBody.atmoshpereTemperatureMultiplier = 1.0f; // how does this coorespond?
            body.celestialBody.maxAtmosphereAltitude = 55000.0f;  // i guess this is so the math doesn't drag out?
            body.celestialBody.useLegacyAtmosphere = true;
            body.celestialBody.atmosphericAmbientColor = new Color(0.306f, 0.187f, 0.235f, 1.000f);
            #endregion

            #region Ocean
            // ---------------- FOR BODIES WITH OCEANS ----------
            /*body.celestialBody.ocean = true;

            // Setup the laythe ocean info in master pqs
            body.pqsVersion.mapOcean = true;
            body.pqsVersion.mapOceanColor = new Color(0.117f, 0.126f, 0.157f, 1.000f);
            body.pqsVersion.mapOceanHeight = 0.0f;

            // Generate the PQS object
            GameObject oceanRoot       = new GameObject(name + "Ocean");
            oceanRoot.transform.parent = body.pqsVersion.transform;
            oceanRoot.layer            = Constants.GameLayers.LocalSpace;
            PQS oceanPQS               = oceanRoot.AddComponent<PQS>();

            // Add this new PQS to the secondary renderers of the altitude fade controller
            celestialBodyTransform.planetFade.secondaryRenderers.Add(oceanRoot);

            // Setup the PQS object data
            Utility.CopyObjectFields<PQS>(laytheOcean.GetComponent<PQS>(), oceanPQS);
            oceanPQS.radius            = body.pqsVersion.radius;
            oceanPQS.surfaceMaterial   = new PQSOceanSurfaceQuad(laytheOcean.GetComponent<PQS>().surfaceMaterial);
            oceanPQS.fallbackMaterial  = new PQSOceanSurfaceQuadFallback(laytheOcean.GetComponent<PQS>().fallbackMaterial);
            Utility.DumpObjectProperties(oceanPQS.surfaceMaterial, oceanPQS.surfaceMaterial.ToString());
            Utility.DumpObjectProperties(oceanPQS.fallbackMaterial, oceanPQS.fallbackMaterial.ToString());

            // Create the aerial perspective material
            mod = new GameObject("_Material_AerialPerspective");
            mod.transform.parent = oceanRoot.transform;
            aerialPerspectiveMaterial = mod.AddComponent<PQSMod_AerialPerspectiveMaterial>();
            aerialPerspectiveMaterial.sphere = body.pqsVersion;
            aerialPerspectiveMaterial.globalDensity = -0.00001f;
            aerialPerspectiveMaterial.heightFalloff = 6.75f;
            aerialPerspectiveMaterial.atmosphereDepth = 150000;
            aerialPerspectiveMaterial.DEBUG_SetEveryFrame = true;
            aerialPerspectiveMaterial.cameraAlt = 0;
            aerialPerspectiveMaterial.cameraAtmosAlt = 0;
            aerialPerspectiveMaterial.heightDensAtViewer = 0;
            aerialPerspectiveMaterial.requirements = PQS.ModiferRequirements.Default;
            aerialPerspectiveMaterial.modEnabled = true;
            aerialPerspectiveMaterial.order = 100;
            
            // Create the UV planet relative position
            mod = new GameObject("_Material_SurfaceQuads");
            mod.transform.parent = oceanRoot.transform;
            planetRelativePosition = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
            planetRelativePosition.sphere = body.pqsVersion;
            planetRelativePosition.requirements = PQS.ModiferRequirements.Default;
            planetRelativePosition.modEnabled = true;
            planetRelativePosition.order = 100;

            // Create the quad map remover (da fuck?)
            mod = new GameObject("QuadRemoveMap");
            mod.transform.parent = oceanRoot.transform;
            PQSMod_RemoveQuadMap removeQuadMap = mod.AddComponent<PQSMod_RemoveQuadMap>();
            removeQuadMap.mapDeformity = 0.0f;
            removeQuadMap.minHeight = 0.0f;
            removeQuadMap.maxHeight = 0.5f;
            removeQuadMap.requirements = PQS.ModiferRequirements.Default;
            removeQuadMap.modEnabled = true;
            removeQuadMap.order = 1000;

            // Load the heightmap into whatever the hell this is
            map = new Texture2D(4, 4, TextureFormat.Alpha8, false);
            map.LoadImage(System.IO.File.ReadAllBytes(KSPUtil.ApplicationRootPath + PluginDirectory + "/Planets/" + name + "/Height.png"));
            removeQuadMap.map = ScriptableObject.CreateInstance<MapSO>();
            removeQuadMap.map.CreateMap(MapSO.MapDepth.Greyscale, map);
            UnityEngine.Object.DestroyImmediate(map);

            // Setup the ocean effects
            mod = new GameObject("OceanFX");
            mod.transform.parent = oceanRoot.transform;
            PQSMod_OceanFX oceanFX = mod.AddComponent<PQSMod_OceanFX>();
            oceanFX.watermain = Utility.RecursivelyGetComponent<PQSMod_OceanFX>(laytheOcean).watermain.Clone() as Texture2D[];
            oceanFX.requirements = PQS.ModiferRequirements.Default;
            oceanFX.modEnabled = true;
            oceanFX.order = 100;*/

            #endregion

            // Return the new body
            return body;
        }

        // This function generates the biomes for the planet. (Coupled with an
        // appropriate Texture2D.)
        public static CBAttributeMapSO GenerateCBAttributeMapSO(string name)
        {
            Debug.Log("[Kopernicus] GenerateCBAttributeMapSO begins  r4");
            CBAttributeMapSO rv = ScriptableObject.CreateInstance<CBAttributeMapSO>();
            Texture2D map = new Texture2D(4, 4, TextureFormat.RGB24, false);
            Debug.Log("[Kopernicus] map=" + map);
            map.LoadImage(System.IO.File.ReadAllBytes(KSPUtil.ApplicationRootPath + PluginDirectory + "/Planets/" + name + "/Biomes.png"));
            //map.Compress(true); // This might make the edges funny.
            map.Compress(false);
            // If it will let us take in an indexed color PNG that would be preferable - bcs
            map.Apply(true, false);
            rv.CreateMap(MapSO.MapDepth.RGB, map);
            //rv.nonExactThreshold = -1f; // This will theoretically return the "best match". It also depends on float comparison, but at least it's to a constant.
            rv.nonExactThreshold = 0.05f; // 0.6 0.0 0.4 matches 1 0 0 at .33 so .1 is fairly generous and should be reduced for maps with many colors
            // "exact" match is a broken concept (floats should never be compared to floats with == or !=) and (as implemented as of KSP .24.2) should
            // never be used.
            rv.exactSearch = false;
            rv.Attributes = new CBAttributeMapSO.MapAttribute[3];

            rv.Attributes[0] = new CBAttributeMapSO.MapAttribute();
            rv.Attributes[0].name = "PolarCaps";
            rv.Attributes[0].value = 1.5f;
            rv.Attributes[0].mapColor = new Color(0f, 1f, 0f);

            rv.Attributes[1] = new CBAttributeMapSO.MapAttribute();
            rv.Attributes[1].name = "Mares";
            rv.Attributes[1].value = 1.0f;
            rv.Attributes[1].mapColor = new Color(0f, 0f, 1f);

            rv.Attributes[2] = new CBAttributeMapSO.MapAttribute();
            rv.Attributes[2].name = "Dunes";
            rv.Attributes[2].value = 1.0f;
            rv.Attributes[2].mapColor = new Color(1f, 0f, 0f);

            //rv.defaultAttribute = rv.Attributes [0];
            /*
            // Troubleshooting fosssil
            Vector4 pixelBilinear;
            Vector4 color;
            for (float lat = -3f; lat < 3f; lat += .1f)
                for (float lon = 0f; lon < 6f; lon += .1f) {
                    float innerlon = lon - 1.5707963267948966f;
                    
                    float v = (float)(lat / 3.1415926535897931) + 0.5f;
                    float u = (float)(innerlon / 6.2831853071795862f);
                    pixelBilinear = rv.Map.GetPixelBilinear (u, v); 
                    color = rv.Attributes [2].mapColor;
                    

                    Debug.Log ("X<A "+ lat + " "+lon+" clr="+ color + " pbl=" + pixelBilinear+" rv=" + rv.GetAtt (lat, lon).name + ":" + 
                           (color-pixelBilinear).sqrMagnitude);
                }
            */
            Debug.Log("[Kopernicus] GenerateCBAttributeMapSO ends");

            return rv;
        }
    }

    // Add a clone of a stock planet (in a different orbit)
    // This is a kind of KopernicusPlanetSource
    public class StockPlanetSource 
    {
        public static PSystemBody GeneratePlanet (PSystem system, string templateName, string name, Orbit orbit) 
        {
            return GenerateSystemBody (system, system.rootBody, templateName, name, orbit);
        }

        public static PSystemBody GenerateSystemBody(PSystem system, PSystemBody parent, string templateName, string name, Orbit orbit) 
        {
            // Look up the template body
            PSystemBody template = Utility.FindBody (system.rootBody, templateName);

            // Create a new celestial body as a clone of this template
            PSystemBody clone = system.AddBody (parent);
            clone.name = name;

            // Set up the celestial body of the clone (back up the orbit driver)
            Utility.CopyObjectFields<CelestialBody> (template.celestialBody, clone.celestialBody);
            clone.celestialBody.bodyName = name;
            clone.celestialBody.orbitingBodies = null;
            clone.celestialBody.orbitDriver = clone.orbitDriver;
            clone.flightGlobalsIndex = 100 + template.flightGlobalsIndex;

            // Setup the orbit driver
            clone.orbitRenderer.orbitColor = template.orbitRenderer.orbitColor;
            clone.orbitDriver.orbit = orbit;
            clone.orbitDriver.celestialBody = clone.celestialBody;
            clone.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;

            // Setup the deactivator object
            GameObject deactivator = new GameObject ("Deactivator");
            deactivator.SetActive (false);
            UnityEngine.Object.DontDestroyOnLoad (deactivator);

            // Clone the template's PQS
            clone.pqsVersion = UnityEngine.Object.Instantiate (template.pqsVersion) as PQS;
            clone.pqsVersion.transform.parent = deactivator.transform;
            clone.pqsVersion.name = name;

            // Clone the scaled space
            clone.scaledVersion = UnityEngine.Object.Instantiate (template.scaledVersion) as GameObject;
            clone.scaledVersion.transform.parent = deactivator.transform;
            clone.scaledVersion.name = name;

            return clone;
        }
    }
}

