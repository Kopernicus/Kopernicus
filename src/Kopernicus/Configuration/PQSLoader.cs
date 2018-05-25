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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kopernicus.Configuration.ModLoader;
using Kopernicus.OnDemand;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration 
    {
        [RequireConfigType(ConfigType.Node)]
        public class PQSLoader : BaseLoader, IParserEventSubscriber, ITypeParser<PQS>
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
            public PQS Value { get; set; }

            // Required PQSMods
            private readonly PQSMod_CelestialBodyTransform   _transform;
            private readonly PQSMod_QuadMeshColliders        _collider;
            
            // Surface physics material
            [ParserTarget("PhysicsMaterial", AllowMerge = true)]
            public PhysicsMaterialParser physicsMaterial
            {
                get { return _collider.physicsMaterial; }
                set { _collider.physicsMaterial = value; }
            }

            // PQS level of detail settings
            [ParserTarget("minLevel")]
            public NumericParser<Int32> minLevel 
            {
                get { return Value.minLevel; }
                set { Value.minLevel = value; }
            }

            [ParserTarget("maxLevel")]
            public NumericParser<Int32> maxLevel 
            {
                get { return Value.maxLevel; }
                set { Value.maxLevel = value; }
            }

            [ParserTarget("minDetailDistance")]
            public NumericParser<Double> minDetailDistance 
            {
                get { return Value.minDetailDistance; }
                set { Value.minDetailDistance = value; }
            }

            [ParserTarget("maxQuadLengthsPerFrame")]
            public NumericParser<Single> maxQuadLengthsPerFrame 
            {
                get { return Value.maxQuadLenghtsPerFrame; }
                set { Value.maxQuadLenghtsPerFrame = value; }
            }

            // CelestialBodyTransform fades. should more or less line up with ScaledVersion's fadeStart/fadeEnd
            [ParserTarget("fadeStart")]
            public NumericParser<Single> fadeStart
            {
                get { return _transform.planetFade.fadeStart; }
                set { _transform.planetFade.fadeStart = value; }
            }

            [ParserTarget("fadeEnd")]
            public NumericParser<Single> fadeEnd
            {
                get { return _transform.planetFade.fadeEnd; }
                set { _transform.planetFade.fadeEnd = value; }
            }

            [ParserTarget("deactivateAltitude")]
            public NumericParser<Double> deactivateAltitude
            {
                get { return _transform.deactivateAltitude; }
                set { _transform.deactivateAltitude = value; }
            }

            // Map Export Arguments
            [ParserTarget("mapMaxHeight")]
            public NumericParser<Double> mapMaxHeight
            {
                get { return Value.mapMaxHeight; }
                set { Value.mapMaxHeight = value; }
            }

            [PreApply]
            [ParserTarget("materialType")]
            public EnumParser<PQSMaterialType> materialType
            {
                get
                {
                    if (PQSMainOptimised.UsesSameShader(surfaceMaterial))
                        return PQSMaterialType.AtmosphericOptimized;
                    if (PQSMainShader.UsesSameShader(surfaceMaterial))
                        return PQSMaterialType.AtmosphericMain;
                    if (PQSProjectionAerialQuadRelative.UsesSameShader(surfaceMaterial))
                        return PQSMaterialType.AtmosphericBasic;
                    if (PQSProjectionSurfaceQuad.UsesSameShader(surfaceMaterial))
                        return PQSMaterialType.Vacuum;
                    if (PQSMainExtras.UsesSameShader(surfaceMaterial))
                        return PQSMaterialType.AtmosphericExtra;
                    return PQSMaterialType.Vacuum;
                }
                set 
                {
                    if (value.Value == PQSMaterialType.AtmosphericOptimized)
                        surfaceMaterial = new PQSMainOptimisedLoader();
                    else if (value.Value == PQSMaterialType.AtmosphericMain)
                        surfaceMaterial = new PQSMainShaderLoader();
                    else if (value.Value == PQSMaterialType.AtmosphericBasic)
                        surfaceMaterial = new PQSProjectionAerialQuadRelativeLoader();
                    else if (value.Value == PQSMaterialType.Vacuum)
                        surfaceMaterial = new PQSProjectionSurfaceQuadLoader();
                    else if (value.Value == PQSMaterialType.AtmosphericExtra)
                        surfaceMaterial = new PQSMainExtrasLoader();
                }
            }

            // Surface Material of the PQS
            [ParserTarget("Material", AllowMerge = true, GetChild = false)]
            [KittopiaUntouchable]
            public Material surfaceMaterial
            {
                get { return Value.surfaceMaterial; }
                set { Value.surfaceMaterial = value; }
            }

            // Fallback Material of the PQS (its always the same material)
            [ParserTarget("FallbackMaterial", AllowMerge = true, GetChild = false)]
            [KittopiaUntouchable]
            public Material fallbackMaterial
            {
                get { return Value.fallbackMaterial; }
                set { Value.fallbackMaterial = value; }
            }

            // PQSMod loader
            [ParserTargetCollection("Mods", AllowMerge = true, NameSignificance = NameSignificance.Type)]
            [KittopiaUntouchable]
            public List<IModLoader> mods = new List<IModLoader>();

            [KittopiaAction("Rebuild Sphere")]
            [KittopiaDescription("Rebuilds the surface of the planet.")]
            public void Rebuild()
            {
                Value.RebuildSphere();
            }

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
                    Value = generatedBody.pqsVersion;

                    // Get the required PQS information
                    _transform = Value.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true)
                        .FirstOrDefault(Mod => Mod.transform.parent == Value.transform);
                    _collider = Value.GetComponentsInChildren<PQSMod_QuadMeshColliders>(true)
                        .FirstOrDefault(Mod => Mod.transform.parent == Value.transform);

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
                    controllerRoot.transform.parent = generatedBody.celestialBody.transform;
                    Value = controllerRoot.AddComponent<PQS>();

                    // I (Teknoman) am at this time unable to determine some of the magic parameters which cause the PQS to work...
                    // And I (Thomas) am at this time just too lazy to do it differently...
                    PSystemBody Laythe = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
                    Utility.CopyObjectFields(Laythe.pqsVersion, Value);
                    Value.surfaceMaterial = Laythe.pqsVersion.surfaceMaterial;
                    Value.fallbackMaterial = Laythe.pqsVersion.fallbackMaterial;

                    // Create the celestial body transform
                    GameObject mod = new GameObject("_CelestialBody");
                    mod.transform.parent = controllerRoot.transform;
                    _transform = mod.AddComponent<PQSMod_CelestialBodyTransform>();
                    _transform.sphere = Value;
                    _transform.forceActivate = false;
                    _transform.deactivateAltitude = 115000;
                    _transform.forceRebuildOnTargetChange = false;
                    _transform.planetFade = new PQSMod_CelestialBodyTransform.AltitudeFade();
                    _transform.planetFade.fadeFloatName = "_PlanetOpacity";
                    _transform.planetFade.fadeStart = 100000.0f;
                    _transform.planetFade.fadeEnd = 110000.0f;
                    _transform.planetFade.valueStart = 0.0f;
                    _transform.planetFade.valueEnd = 1.0f;
                    _transform.planetFade.secondaryRenderers = new List<GameObject>();
                    _transform.secondaryFades = new PQSMod_CelestialBodyTransform.AltitudeFade[0];
                    _transform.requirements = PQS.ModiferRequirements.Default;
                    _transform.modEnabled = true;
                    _transform.order = 10;

                    // Create the material direction
                    mod = new GameObject("_Material_SunLight");
                    mod.transform.parent = controllerRoot.gameObject.transform;
                    PQSMod_MaterialSetDirection lightDirection = mod.AddComponent<PQSMod_MaterialSetDirection>();
                    lightDirection.sphere = Value;
                    lightDirection.valueName = "_sunLightDirection";
                    lightDirection.requirements = PQS.ModiferRequirements.Default;
                    lightDirection.modEnabled = true;
                    lightDirection.order = 100;

                    // Create the UV planet relative position
                    mod = new GameObject("_Material_SurfaceQuads");
                    mod.transform.parent = controllerRoot.transform;
                    PQSMod_UVPlanetRelativePosition uvs = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
                    uvs.sphere = Value;
                    uvs.requirements = PQS.ModiferRequirements.Default;
                    uvs.modEnabled = true;
                    uvs.order = 999999;

                    // Crete the quad mesh colliders
                    mod = new GameObject("QuadMeshColliders");
                    mod.transform.parent = controllerRoot.gameObject.transform;
                    _collider = mod.AddComponent<PQSMod_QuadMeshColliders>();
                    _collider.sphere = Value;
                    _collider.maxLevelOffset = 0;
                    _collider.requirements = PQS.ModiferRequirements.Default;
                    _collider.modEnabled = true;
                    _collider.order = 100;
                }

                // Assing the new PQS
                generatedBody.pqsVersion = Value;
                generatedBody.pqsVersion.name = generatedBody.name;
                generatedBody.pqsVersion.transform.name = generatedBody.name;
                generatedBody.pqsVersion.gameObject.name = generatedBody.name;
                generatedBody.pqsVersion.radius = generatedBody.celestialBody.Radius;
                generatedBody.celestialBody.pqsController = generatedBody.pqsVersion;

                // Add an OnDemand Handler
                if (Value.GetComponentsInChildren<PQSMod_OnDemandHandler>(true).Length == 0)
                {
                    OnDemandStorage.AddHandler(Value);
                }
                
                // Load existing mods
                foreach (PQSMod mod in Value.GetComponentsInChildren<PQSMod>(true)
                    .Where(m => m.sphere == Value))
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
                        loader.Create(mod, Value);
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
                    Value = body.pqsController;

                    // Get the required PQS information
                    _transform = Value.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true)
                        .FirstOrDefault(Mod => Mod.transform.parent == Value.transform);
                    _collider = Value.GetComponentsInChildren<PQSMod_QuadMeshColliders>(true)
                        .FirstOrDefault(Mod => Mod.transform.parent == Value.transform);

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
                    controllerRoot.transform.parent = body.transform;
                    Value = controllerRoot.AddComponent<PQS>();

                    // I (Teknoman) am at this time unable to determine some of the magic parameters which cause the PQS to work...
                    // And I (Thomas) am at this time just too lazy to do it differently...
                    PSystemBody Laythe = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
                    Utility.CopyObjectFields(Laythe.pqsVersion, Value);
                    Value.surfaceMaterial = Laythe.pqsVersion.surfaceMaterial;

                    // Create the fallback material (always the same shader)
                    fallbackMaterial = new PQSProjectionFallbackLoader();
                    Value.fallbackMaterial = fallbackMaterial;
                    fallbackMaterial.name = Guid.NewGuid().ToString();

                    // Create the celestial body transform
                    GameObject mod = new GameObject("_CelestialBody");
                    mod.transform.parent = controllerRoot.transform;
                    _transform = mod.AddComponent<PQSMod_CelestialBodyTransform>();
                    _transform.sphere = Value;
                    _transform.forceActivate = false;
                    _transform.deactivateAltitude = 115000;
                    _transform.forceRebuildOnTargetChange = false;
                    _transform.planetFade = new PQSMod_CelestialBodyTransform.AltitudeFade();
                    _transform.planetFade.fadeFloatName = "_PlanetOpacity";
                    _transform.planetFade.fadeStart = 100000.0f;
                    _transform.planetFade.fadeEnd = 110000.0f;
                    _transform.planetFade.valueStart = 0.0f;
                    _transform.planetFade.valueEnd = 1.0f;
                    _transform.planetFade.secondaryRenderers = new List<GameObject>();
                    _transform.secondaryFades = new PQSMod_CelestialBodyTransform.AltitudeFade[0];
                    _transform.requirements = PQS.ModiferRequirements.Default;
                    _transform.modEnabled = true;
                    _transform.order = 10;

                    // Create the material direction
                    mod = new GameObject("_Material_SunLight");
                    mod.transform.parent = controllerRoot.gameObject.transform;
                    PQSMod_MaterialSetDirection lightDirection = mod.AddComponent<PQSMod_MaterialSetDirection>();
                    lightDirection.sphere = Value;
                    lightDirection.valueName = "_sunLightDirection";
                    lightDirection.requirements = PQS.ModiferRequirements.Default;
                    lightDirection.modEnabled = true;
                    lightDirection.order = 100;

                    // Create the UV planet relative position
                    mod = new GameObject("_Material_SurfaceQuads");
                    mod.transform.parent = controllerRoot.transform;
                    PQSMod_UVPlanetRelativePosition uvs = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
                    uvs.sphere = Value;
                    uvs.requirements = PQS.ModiferRequirements.Default;
                    uvs.modEnabled = true;
                    uvs.order = 999999;

                    // Crete the quad mesh colliders
                    mod = new GameObject("QuadMeshColliders");
                    mod.transform.parent = controllerRoot.gameObject.transform;
                    _collider = mod.AddComponent<PQSMod_QuadMeshColliders>();
                    _collider.sphere = Value;
                    _collider.maxLevelOffset = 0;
                    _collider.requirements = PQS.ModiferRequirements.Default;
                    _collider.modEnabled = true;
                    _collider.order = 100;
                }

                // Assing the new PQS
                body.pqsController = Value;
                body.pqsController.name = body.transform.name;
                body.pqsController.transform.name = body.transform.name;
                body.pqsController.gameObject.name = body.transform.name;
                body.pqsController.radius = body.Radius;

                // Add an OnDemand Handler
                if (Value.GetComponentsInChildren<PQSMod_OnDemandHandler>(true).Length == 0)
                {
                    OnDemandStorage.AddHandler(Value);
                }
                
                // Load existing mods
                foreach (PQSMod mod in Value.GetComponentsInChildren<PQSMod>(true)
                    .Where(m => m.sphere == Value))
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
                        loader.Create(mod, Value);
                        mods.Add(loader);
                    }
                }
            }

            [KittopiaDestructor]
            public void Destroy()
            {
                UnityEngine.Object.Destroy(Value.gameObject);
            }

            // Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                // Share the current PQS
                Parser.SetState("Kopernicus:pqsVersion", () => Value);
                
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
                Utility.GameObjectWalk(Value.gameObject, "  ");
                // -------------------------------
            }
        }
    }
}
