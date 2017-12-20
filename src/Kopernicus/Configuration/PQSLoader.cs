/**
 * Kopernicus Planetary System Modifier
 * ------------------------------------------------------------- 
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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using Kopernicus.MaterialWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kopernicus.Configuration.ModLoader;
using Kopernicus.OnDemand;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration 
    {
        [RequireConfigType(ConfigType.Node)]
        public class PQSLoader : BaseLoader, IParserEventSubscriber
        {
            // PQS Material Type Enum
            public enum PQSMaterialType
            {
                Vacuum,
                AtmosphericBasic,
                AtmosphericMain,
                AtmosphericOptimized,
                AtmosphericExtra
            };

            // PQS we are creating
            public PQS pqsVersion { get; set; }

            // Required PQSMods
            public PQSMod_CelestialBodyTransform   transform;
            public PQSMod_MaterialSetDirection     lightDirection;
            public PQSMod_UVPlanetRelativePosition uvs;
            public PQSMod_QuadMeshColliders        collider;
            
            // Surface physics material
            [ParserTarget("PhysicsMaterial", allowMerge = true)]
            public PhysicsMaterialParser physicsMaterial
            {
                get { return collider.physicsMaterial; }
                set { collider.physicsMaterial = value; }
            }

            // PQS level of detail settings
            [ParserTarget("minLevel")]
            public NumericParser<Int32> minLevel 
            {
                get { return pqsVersion.minLevel; }
                set { pqsVersion.minLevel = value; }
            }

            [ParserTarget("maxLevel")]
            public NumericParser<Int32> maxLevel 
            {
                get { return pqsVersion.maxLevel; }
                set { pqsVersion.maxLevel = value; }
            }

            [ParserTarget("minDetailDistance")]
            public NumericParser<Double> minDetailDistance 
            {
                get { return pqsVersion.minDetailDistance; }
                set { pqsVersion.minDetailDistance = value; }
            }

            [ParserTarget("maxQuadLengthsPerFrame")]
            public NumericParser<Single> maxQuadLengthsPerFrame 
            {
                get { return pqsVersion.maxQuadLenghtsPerFrame; }
                set { pqsVersion.maxQuadLenghtsPerFrame = value; }
            }

            // CelestialBodyTransform fades. should more or less line up with ScaledVersion's fadeStart/fadeEnd
            [ParserTarget("fadeStart")]
            public NumericParser<Single> fadeStart
            {
                get { return transform.planetFade.fadeStart; }
                set { transform.planetFade.fadeStart = value; }
            }

            [ParserTarget("fadeEnd")]
            public NumericParser<Single> fadeEnd
            {
                get { return transform.planetFade.fadeEnd; }
                set { transform.planetFade.fadeEnd = value; }
            }

            [ParserTarget("deactivateAltitude")]
            public NumericParser<Double> deactivateAltitude
            {
                get { return transform.deactivateAltitude; }
                set { transform.deactivateAltitude = value; }
            }

            // Map Export Arguments
            [ParserTarget("mapMaxHeight")]
            public NumericParser<Double> mapMaxHeight
            {
                get { return pqsVersion.mapMaxHeight; }
                set { pqsVersion.mapMaxHeight = value; }
            }

            [PreApply]
            [ParserTarget("materialType")]
            public EnumParser<PQSMaterialType> materialType
            {
                set 
                {
                    if (value.value == PQSMaterialType.AtmosphericOptimized)
                        surfaceMaterial = new PQSMainOptimisedLoader();
                    else if (value.value == PQSMaterialType.AtmosphericMain)
                        surfaceMaterial = new PQSMainShaderLoader();
                    else if (value.value == PQSMaterialType.AtmosphericBasic)
                        surfaceMaterial = new PQSProjectionAerialQuadRelativeLoader();
                    else if (value.value == PQSMaterialType.Vacuum)
                        surfaceMaterial = new PQSProjectionSurfaceQuadLoader();
                    else if (value.value == PQSMaterialType.AtmosphericExtra)
                        surfaceMaterial = new PQSMainExtrasLoader();
                }
            }

            // Surface Material of the PQS
            [ParserTarget("Material", allowMerge = true, getChild = false)]
            public Material surfaceMaterial
            {
                get { return pqsVersion.surfaceMaterial; }
                set { pqsVersion.surfaceMaterial = value; }
            }

            // Fallback Material of the PQS (its always the same material)
            [ParserTarget("FallbackMaterial", allowMerge = true, getChild = false)]
            public Material fallbackMaterial
            {
                get { return pqsVersion.fallbackMaterial; }
                set { pqsVersion.fallbackMaterial = value; }
            }

            // PQSMod loader
            [ParserTargetCollection("Mods", allowMerge = true, nameSignificance = NameSignificance.Type)]
            public List<IModLoader> mods = new List<IModLoader>();

            /// <summary>
            /// Creates a new PQS Loader from the Injector context.
            /// </summary>
            public PQSLoader ()
            {
                // Is this the parser context?
                if (!Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("Must be executed in Injector context.");
                }

                if (generatedBody.pqsVersion != null)
                {
                    // Save the PQSVersion
                    pqsVersion = generatedBody.pqsVersion;

                    // Get the required PQS information
                    transform = pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true)
                        .FirstOrDefault(Mod => Mod.transform.parent == pqsVersion.transform);
                    lightDirection = pqsVersion.GetComponentsInChildren<PQSMod_MaterialSetDirection>(true)
                        .FirstOrDefault(Mod => Mod.transform.parent == pqsVersion.transform);
                    uvs = pqsVersion.GetComponentsInChildren<PQSMod_UVPlanetRelativePosition>(true)
                        .FirstOrDefault(Mod => Mod.transform.parent == pqsVersion.transform);
                    collider = pqsVersion.GetComponentsInChildren<PQSMod_QuadMeshColliders>(true)
                        .FirstOrDefault(Mod => Mod.transform.parent == pqsVersion.transform);

                    // Clone the surface material of the PQS
                    if (PQSMainOptimised.UsesSameShader(surfaceMaterial))
                    {
                        PQSMainOptimisedLoader loader = new PQSMainOptimisedLoader(surfaceMaterial);
                        loader.globalDensity = loader.globalDensity < 2 ? (Single) (-8E-06) : loader.globalDensity;
                        surfaceMaterial = loader;
                    }
                    else if (PQSMainShader.UsesSameShader(surfaceMaterial))
                    {
                        PQSMainShaderLoader loader = new PQSMainShaderLoader(surfaceMaterial);
                        loader.globalDensity = loader.globalDensity < 2 ? (Single) (-8E-06) : loader.globalDensity;
                        surfaceMaterial = loader;
                    }
                    else if (PQSProjectionAerialQuadRelative.UsesSameShader(surfaceMaterial))
                    {
                        PQSProjectionAerialQuadRelativeLoader loader =
                            new PQSProjectionAerialQuadRelativeLoader(surfaceMaterial);
                        loader.globalDensity = loader.globalDensity < 2 ? (Single) (-8E-06) : loader.globalDensity;
                        surfaceMaterial = loader;
                    }
                    else if (PQSProjectionSurfaceQuad.UsesSameShader(surfaceMaterial))
                    {
                        surfaceMaterial = new PQSProjectionSurfaceQuadLoader(surfaceMaterial);
                    }
                    surfaceMaterial.name = Guid.NewGuid().ToString();

                    // Clone the fallback material of the PQS
                    fallbackMaterial = new PQSProjectionFallbackLoader(fallbackMaterial);
                    fallbackMaterial.name = Guid.NewGuid().ToString();
                }
                else
                {

                    // Create a new PQS
                    GameObject controllerRoot = new GameObject();
                    controllerRoot.transform.parent = Utility.Deactivator;
                    pqsVersion = controllerRoot.AddComponent<PQS>();

                    // I (Teknoman) am at this time unable to determine some of the magic parameters which cause the PQS to work...
                    // And I (Thomas) am at this time just too lazy to do it differently...
                    PSystemBody Laythe = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, "Laythe");
                    Utility.CopyObjectFields(Laythe.pqsVersion, pqsVersion);
                    pqsVersion.surfaceMaterial = Laythe.pqsVersion.surfaceMaterial;

                    // Create the fallback material (always the same shader)
                    fallbackMaterial = new PQSProjectionFallbackLoader();
                    pqsVersion.fallbackMaterial = fallbackMaterial;
                    fallbackMaterial.name = Guid.NewGuid().ToString();

                    // Create the celestial body transform
                    GameObject mod = new GameObject("_CelestialBody");
                    mod.transform.parent = controllerRoot.transform;
                    transform = mod.AddComponent<PQSMod_CelestialBodyTransform>();
                    transform.sphere = pqsVersion;
                    transform.forceActivate = false;
                    transform.deactivateAltitude = 115000;
                    transform.forceRebuildOnTargetChange = false;
                    transform.planetFade = new PQSMod_CelestialBodyTransform.AltitudeFade();
                    transform.planetFade.fadeFloatName = "_PlanetOpacity";
                    transform.planetFade.fadeStart = 100000.0f;
                    transform.planetFade.fadeEnd = 110000.0f;
                    transform.planetFade.valueStart = 0.0f;
                    transform.planetFade.valueEnd = 1.0f;
                    transform.planetFade.secondaryRenderers = new List<GameObject>();
                    transform.secondaryFades = new PQSMod_CelestialBodyTransform.AltitudeFade[0];
                    transform.requirements = PQS.ModiferRequirements.Default;
                    transform.modEnabled = true;
                    transform.order = 10;

                    // Create the material direction
                    mod = new GameObject("_Material_SunLight");
                    mod.transform.parent = controllerRoot.gameObject.transform;
                    lightDirection = mod.AddComponent<PQSMod_MaterialSetDirection>();
                    lightDirection.sphere = pqsVersion;
                    lightDirection.valueName = "_sunLightDirection";
                    lightDirection.requirements = PQS.ModiferRequirements.Default;
                    lightDirection.modEnabled = true;
                    lightDirection.order = 100;

                    // Create the UV planet relative position
                    mod = new GameObject("_Material_SurfaceQuads");
                    mod.transform.parent = controllerRoot.transform;
                    uvs = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
                    uvs.sphere = pqsVersion;
                    uvs.requirements = PQS.ModiferRequirements.Default;
                    uvs.modEnabled = true;
                    uvs.order = 999999;

                    // Crete the quad mesh colliders
                    mod = new GameObject("QuadMeshColliders");
                    mod.transform.parent = controllerRoot.gameObject.transform;
                    collider = mod.AddComponent<PQSMod_QuadMeshColliders>();
                    collider.sphere = pqsVersion;
                    collider.maxLevelOffset = 0;
                    collider.requirements = PQS.ModiferRequirements.Default;
                    collider.modEnabled = true;
                    collider.order = 100;
                }

                // Assing the new PQS
                generatedBody.pqsVersion = pqsVersion;
                generatedBody.pqsVersion.name = generatedBody.name;
                generatedBody.pqsVersion.transform.name = generatedBody.name;
                generatedBody.pqsVersion.gameObject.name = generatedBody.name;
                generatedBody.pqsVersion.radius = generatedBody.celestialBody.Radius;

                // Add an OnDemand Handler
                if (pqsVersion.GetComponentsInChildren<PQSMod_OnDemandHandler>(true).Length == 0)
                {
                    OnDemandStorage.AddHandler(pqsVersion);
                }
                
                // Load existing mods
                foreach (PQSMod mod in pqsVersion.GetComponentsInChildren<PQSMod>(true)
                    .Where(m => m.sphere = pqsVersion))
                {
                    Type modType = mod.GetType();
                    foreach (Type loaderType in Parser.ModTypes)
                    {
                        if (loaderType.BaseType == null)
                            continue;
                        if (loaderType.BaseType.Namespace != "Kopernicus.Configuration.ModLoader")
                            continue;
                        if (!loaderType.BaseType.Name.StartsWith("ModLoader"))
                            continue;
                        if (loaderType.BaseType.GetGenericArguments()[0] != modType)
                            continue;
                        
                        // We found our loader type
                        IModLoader loader = (IModLoader) Activator.CreateInstance(loaderType);
                        loader.Create(mod, pqsVersion);
                        mods.Add(loader);
                    }
                }
            }

            /// <summary>
            /// Creates a new PQS Loader from a spawned CelestialBody.
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.CelestialBody)]
            public PQSLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }

                if (body.pqsController != null)
                {
                    // Save the PQSVersion
                    pqsVersion = body.pqsController;

                    // Get the required PQS information
                    transform = pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true)
                        .FirstOrDefault(Mod => Mod.transform.parent == pqsVersion.transform);
                    lightDirection = pqsVersion.GetComponentsInChildren<PQSMod_MaterialSetDirection>(true)
                        .FirstOrDefault(Mod => Mod.transform.parent == pqsVersion.transform);
                    uvs = pqsVersion.GetComponentsInChildren<PQSMod_UVPlanetRelativePosition>(true)
                        .FirstOrDefault(Mod => Mod.transform.parent == pqsVersion.transform);
                    collider = pqsVersion.GetComponentsInChildren<PQSMod_QuadMeshColliders>(true)
                        .FirstOrDefault(Mod => Mod.transform.parent == pqsVersion.transform);

                    // Clone the surface material of the PQS
                    if (PQSMainOptimised.UsesSameShader(surfaceMaterial))
                    {
                        PQSMainOptimisedLoader loader = new PQSMainOptimisedLoader(surfaceMaterial);
                        loader.globalDensity = loader.globalDensity < 2 ? (Single) (-8E-06) : loader.globalDensity;
                        surfaceMaterial = loader;
                    }
                    else if (PQSMainShader.UsesSameShader(surfaceMaterial))
                    {
                        PQSMainShaderLoader loader = new PQSMainShaderLoader(surfaceMaterial);
                        loader.globalDensity = loader.globalDensity < 2 ? (Single) (-8E-06) : loader.globalDensity;
                        surfaceMaterial = loader;
                    }
                    else if (PQSProjectionAerialQuadRelative.UsesSameShader(surfaceMaterial))
                    {
                        PQSProjectionAerialQuadRelativeLoader loader =
                            new PQSProjectionAerialQuadRelativeLoader(surfaceMaterial);
                        loader.globalDensity = loader.globalDensity < 2 ? (Single) (-8E-06) : loader.globalDensity;
                        surfaceMaterial = loader;
                    }
                    else if (PQSProjectionSurfaceQuad.UsesSameShader(surfaceMaterial))
                    {
                        surfaceMaterial = new PQSProjectionSurfaceQuadLoader(surfaceMaterial);
                    }
                    surfaceMaterial.name = Guid.NewGuid().ToString();

                    // Clone the fallback material of the PQS
                    fallbackMaterial = new PQSProjectionFallbackLoader(fallbackMaterial);
                    fallbackMaterial.name = Guid.NewGuid().ToString();
                }
                else
                {
                    // Create a new PQS
                    GameObject controllerRoot = new GameObject();
                    controllerRoot.transform.parent = Utility.Deactivator;
                    pqsVersion = controllerRoot.AddComponent<PQS>();

                    // I (Teknoman) am at this time unable to determine some of the magic parameters which cause the PQS to work...
                    // And I (Thomas) am at this time just too lazy to do it differently...
                    PSystemBody Laythe = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
                    Utility.CopyObjectFields(Laythe.pqsVersion, pqsVersion);
                    pqsVersion.surfaceMaterial = Laythe.pqsVersion.surfaceMaterial;

                    // Create the fallback material (always the same shader)
                    fallbackMaterial = new PQSProjectionFallbackLoader();
                    pqsVersion.fallbackMaterial = fallbackMaterial;
                    fallbackMaterial.name = Guid.NewGuid().ToString();

                    // Create the celestial body transform
                    GameObject mod = new GameObject("_CelestialBody");
                    mod.transform.parent = controllerRoot.transform;
                    transform = mod.AddComponent<PQSMod_CelestialBodyTransform>();
                    transform.sphere = pqsVersion;
                    transform.forceActivate = false;
                    transform.deactivateAltitude = 115000;
                    transform.forceRebuildOnTargetChange = false;
                    transform.planetFade = new PQSMod_CelestialBodyTransform.AltitudeFade();
                    transform.planetFade.fadeFloatName = "_PlanetOpacity";
                    transform.planetFade.fadeStart = 100000.0f;
                    transform.planetFade.fadeEnd = 110000.0f;
                    transform.planetFade.valueStart = 0.0f;
                    transform.planetFade.valueEnd = 1.0f;
                    transform.planetFade.secondaryRenderers = new List<GameObject>();
                    transform.secondaryFades = new PQSMod_CelestialBodyTransform.AltitudeFade[0];
                    transform.requirements = PQS.ModiferRequirements.Default;
                    transform.modEnabled = true;
                    transform.order = 10;

                    // Create the material direction
                    mod = new GameObject("_Material_SunLight");
                    mod.transform.parent = controllerRoot.gameObject.transform;
                    lightDirection = mod.AddComponent<PQSMod_MaterialSetDirection>();
                    lightDirection.sphere = pqsVersion;
                    lightDirection.valueName = "_sunLightDirection";
                    lightDirection.requirements = PQS.ModiferRequirements.Default;
                    lightDirection.modEnabled = true;
                    lightDirection.order = 100;

                    // Create the UV planet relative position
                    mod = new GameObject("_Material_SurfaceQuads");
                    mod.transform.parent = controllerRoot.transform;
                    uvs = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
                    uvs.sphere = pqsVersion;
                    uvs.requirements = PQS.ModiferRequirements.Default;
                    uvs.modEnabled = true;
                    uvs.order = 999999;

                    // Crete the quad mesh colliders
                    mod = new GameObject("QuadMeshColliders");
                    mod.transform.parent = controllerRoot.gameObject.transform;
                    collider = mod.AddComponent<PQSMod_QuadMeshColliders>();
                    collider.sphere = pqsVersion;
                    collider.maxLevelOffset = 0;
                    collider.requirements = PQS.ModiferRequirements.Default;
                    collider.modEnabled = true;
                    collider.order = 100;
                }

                // Assing the new PQS
                body.pqsController = pqsVersion;
                body.pqsController.name = body.transform.name;
                body.pqsController.transform.name = body.transform.name;
                body.pqsController.gameObject.name = body.transform.name;
                body.pqsController.radius = body.Radius;

                // Add an OnDemand Handler
                if (pqsVersion.GetComponentsInChildren<PQSMod_OnDemandHandler>(true).Length == 0)
                {
                    OnDemandStorage.AddHandler(pqsVersion);
                }
                
                // Load existing mods
                foreach (PQSMod mod in pqsVersion.GetComponentsInChildren<PQSMod>(true)
                    .Where(m => m.sphere = pqsVersion))
                {
                    Type modType = mod.GetType();
                    foreach (Type loaderType in Parser.ModTypes)
                    {
                        if (loaderType.BaseType == null)
                            continue;
                        if (loaderType.BaseType.Namespace != "Kopernicus.Configuration.ModLoader")
                            continue;
                        if (!loaderType.BaseType.Name.StartsWith("ModLoader"))
                            continue;
                        if (loaderType.BaseType.GetGenericArguments()[0] != modType)
                            continue;
                        
                        // We found our loader type
                        IModLoader loader = (IModLoader) Activator.CreateInstance(loaderType);
                        loader.Create(mod, pqsVersion);
                        mods.Add(loader);
                    }
                }
            }

            // Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                // Share the current PQS
                Parser.SetState("Kopernicus:pqsVersion", () => pqsVersion);
                
                Events.OnPQSLoaderApply.Fire(this, node);
            }

            // PostApply Event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                // Reset the PQS state
                Parser.ClearState("Kopernicus:pqsVersion");

                // Event
                Events.OnPQSLoaderPostApply.Fire(this, node);

                // ----------- DEBUG -------------
                // Utility.DumpObjectProperties(pqsVersion.surfaceMaterial, " ---- Surface Material (Post PQS Loader) ---- ");
                Utility.GameObjectWalk(pqsVersion.gameObject, "  ");
                // -------------------------------
            }
        }
    }
}
