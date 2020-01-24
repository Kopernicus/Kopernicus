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
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 *
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kopernicus.Components.MaterialWrapper;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.MaterialLoader;
using Kopernicus.Configuration.ModLoader;
using Kopernicus.Configuration.Parsing;
using Kopernicus.OnDemand;
using Kopernicus.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kopernicus.Configuration
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PQSLoader : BaseLoader, IParserEventSubscriber, ITypeParser<PQS>
    {
        // PQS Material Type Enum
        public enum SurfaceMaterialType
        {
            Vacuum,
            AtmosphericBasic,
            AtmosphericMain,
            AtmosphericOptimized,
            AtmosphericExtra,
            AtmosphericOptimizedFastBlend,
            AtmosphericTriplanarZoomRotation
        }

        // PQS we are creating
        public PQS Value { get; set; }

        // Required PQSMods
        private readonly PQSMod_CelestialBodyTransform _transform;
        private readonly PQSMod_QuadMeshColliders _collider;

        // Surface physics material
        [ParserTarget("PhysicsMaterial", AllowMerge = true)]
        public PhysicsMaterialParser PhysicsMaterial
        {
            get { return _collider.physicsMaterial; }
            set { _collider.physicsMaterial = value; }
        }

        // PQS level of detail settings
        [ParserTarget("minLevel")]
        public NumericParser<Int32> MinLevel
        {
            get { return Value.minLevel; }
            set { Value.minLevel = value; }
        }

        [ParserTarget("maxLevel")]
        public NumericParser<Int32> MaxLevel
        {
            get { return Value.maxLevel; }
            set { Value.maxLevel = value; }
        }

        [ParserTarget("minDetailDistance")]
        public NumericParser<Double> MinDetailDistance
        {
            get { return Value.minDetailDistance; }
            set { Value.minDetailDistance = value; }
        }

        [ParserTarget("maxQuadLengthsPerFrame")]
        public NumericParser<Single> MaxQuadLengthsPerFrame
        {
            get { return Value.maxQuadLenghtsPerFrame; }
            set { Value.maxQuadLenghtsPerFrame = value; }
        }

        // CelestialBodyTransform fades. should more or less line up with ScaledVersion's fadeStart/fadeEnd
        [ParserTarget("fadeStart")]
        public NumericParser<Single> FadeStart
        {
            get { return _transform.planetFade.fadeStart; }
            set { _transform.planetFade.fadeStart = value; }
        }

        [ParserTarget("fadeEnd")]
        public NumericParser<Single> FadeEnd
        {
            get { return _transform.planetFade.fadeEnd; }
            set { _transform.planetFade.fadeEnd = value; }
        }

        [ParserTarget("deactivateAltitude")]
        public NumericParser<Double> DeactivateAltitude
        {
            get { return _transform.deactivateAltitude; }
            set { _transform.deactivateAltitude = value; }
        }

        // Map Export Arguments
        [ParserTarget("mapMaxHeight")]
        public NumericParser<Double> MapMaxHeight
        {
            get { return Value.mapMaxHeight; }
            set { Value.mapMaxHeight = value; }
        }

        [PreApply]
        [ParserTarget("materialType")]
        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        public EnumParser<SurfaceMaterialType> MaterialType
        {
            get
            {
                if (PQSMainOptimised.UsesSameShader(SurfaceMaterial))
                {
                    return SurfaceMaterialType.AtmosphericOptimized;
                }
                if (PQSMainShader.UsesSameShader(SurfaceMaterial))
                {
                    return SurfaceMaterialType.AtmosphericMain;
                }
                if (PQSProjectionAerialQuadRelative.UsesSameShader(SurfaceMaterial))
                {
                    return SurfaceMaterialType.AtmosphericBasic;
                }
                if (PQSProjectionSurfaceQuad.UsesSameShader(SurfaceMaterial))
                {
                    return SurfaceMaterialType.Vacuum;
                }
                if (PQSMainExtras.UsesSameShader(SurfaceMaterial))
                {
                    return SurfaceMaterialType.AtmosphericExtra;
                }
                if (PQSMainOptimisedFastBlend.UsesSameShader(SurfaceMaterial))
                {
                    return SurfaceMaterialType.AtmosphericOptimizedFastBlend;
                }
                if (PQSTriplanarZoomRotation.UsesSameShader(SurfaceMaterial))
                {
                    return SurfaceMaterialType.AtmosphericTriplanarZoomRotation;
                }
                return SurfaceMaterialType.Vacuum;
            }
            set
            {
                if (value.Value == SurfaceMaterialType.AtmosphericOptimized)
                {
                    SurfaceMaterial = new PQSMainOptimisedLoader();
                }
                else if (value.Value == SurfaceMaterialType.AtmosphericMain)
                {
                    SurfaceMaterial = new PQSMainShaderLoader();
                }
                else if (value.Value == SurfaceMaterialType.AtmosphericBasic)
                {
                    SurfaceMaterial = new PQSProjectionAerialQuadRelativeLoader();
                }
                else if (value.Value == SurfaceMaterialType.Vacuum)
                {
                    SurfaceMaterial = new PQSProjectionSurfaceQuadLoader();
                }
                else if (value.Value == SurfaceMaterialType.AtmosphericExtra)
                {
                    SurfaceMaterial = new PQSMainExtrasLoader();
                }
                else if (value.Value == SurfaceMaterialType.AtmosphericOptimizedFastBlend)
                {
                    SurfaceMaterial = new PQSMainOptimisedFastBlendLoader();
                }
                else if (value.Value == SurfaceMaterialType.AtmosphericTriplanarZoomRotation)
                {
                    SurfaceMaterial = new PQSTriplanarZoomRotationLoader();
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        // Surface Material of the PQS
        [ParserTarget("Material", AllowMerge = true, GetChild = false)]
        [KittopiaUntouchable]
        public Material SurfaceMaterial
        {
            get
            {
                switch (GameSettings.TERRAIN_SHADER_QUALITY)
                {
                    case 2 when Value.highQualitySurfaceMaterial != null:
                        return Value.highQualitySurfaceMaterial;
                    case 1 when Value.mediumQualitySurfaceMaterial != null:
                        return Value.mediumQualitySurfaceMaterial;
                    case 0 when Value.lowQualitySurfaceMaterial != null:
                        return Value.lowQualitySurfaceMaterial;
                    default:
                        return Value.surfaceMaterial;
                }
            }
            set
            {
                Value.surfaceMaterial = value;
                Value.lowQualitySurfaceMaterial = value;
                Value.mediumQualitySurfaceMaterial = value;
                Value.highQualitySurfaceMaterial = value;
            }
        }

        // Fallback Material of the PQS (its always the same material)
        [ParserTarget("FallbackMaterial", AllowMerge = true, GetChild = false)]
        [KittopiaUntouchable]
        public Material FallbackMaterial
        {
            get { return Value.fallbackMaterial; }
            set { Value.fallbackMaterial = value; }
        }

        // PQSMod loader
        [ParserTargetCollection("Mods", AllowMerge = true, NameSignificance = NameSignificance.Type)]
        [KittopiaUntouchable]
        [SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
        public readonly List<IModLoader> Mods = new List<IModLoader>();

        [KittopiaAction("Rebuild Sphere")]
        [KittopiaDescription("Rebuilds the surface of the planet.")]
        public void Rebuild()
        {
            Value.RebuildSphere();
        }

        /// <summary>
        /// Creates a new PQS Loader from the Injector context.
        /// </summary>
        public PQSLoader()
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
                    .FirstOrDefault(mod => mod.transform.parent == Value.transform);
                _collider = Value.GetComponentsInChildren<PQSMod_QuadMeshColliders>(true)
                    .FirstOrDefault(mod => mod.transform.parent == Value.transform);

                // Clone the surface material of the PQS
                if (MaterialType == SurfaceMaterialType.AtmosphericOptimized)
                {
                    SurfaceMaterial = new PQSMainOptimisedLoader(SurfaceMaterial);
                }
                else if (MaterialType == SurfaceMaterialType.AtmosphericMain)
                {
                    SurfaceMaterial = new PQSMainShaderLoader(SurfaceMaterial);
                }
                else if (MaterialType == SurfaceMaterialType.AtmosphericBasic)
                {
                    SurfaceMaterial = new PQSProjectionAerialQuadRelativeLoader(SurfaceMaterial);
                }
                else if (MaterialType == SurfaceMaterialType.Vacuum)
                {
                    SurfaceMaterial = new PQSProjectionSurfaceQuadLoader(SurfaceMaterial);
                }
                else if (MaterialType == SurfaceMaterialType.AtmosphericExtra)
                {
                    SurfaceMaterial = new PQSMainExtrasLoader(SurfaceMaterial);
                }
                else if (MaterialType == SurfaceMaterialType.AtmosphericOptimizedFastBlend)
                {
                    SurfaceMaterial = new PQSMainOptimisedFastBlendLoader(SurfaceMaterial);
                }
                else if (MaterialType == SurfaceMaterialType.AtmosphericTriplanarZoomRotation)
                {
                    SurfaceMaterial = new PQSTriplanarZoomRotationLoader(SurfaceMaterial);
                }

                SurfaceMaterial.name = Guid.NewGuid().ToString();

                // Clone the fallback material of the PQS
                FallbackMaterial = new PQSProjectionFallbackLoader(FallbackMaterial) {name = Guid.NewGuid().ToString()};
            }
            else
            {
                // Create a new PQS
                GameObject controllerRoot = new GameObject();
                controllerRoot.transform.parent = generatedBody.celestialBody.transform;
                Value = controllerRoot.AddComponent<PQS>();

                // I (Teknoman) am at this time unable to determine some of the magic parameters which cause the PQS to work...
                // And I (Thomas) am at this time just too lazy to do it differently...
                PSystemBody laythe = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
                Utility.CopyObjectFields(laythe.pqsVersion, Value);
                Value.surfaceMaterial = laythe.pqsVersion.surfaceMaterial;
                Value.fallbackMaterial = laythe.pqsVersion.fallbackMaterial;

                // Create the celestial body transform
                GameObject mod = new GameObject("_CelestialBody");
                mod.transform.parent = controllerRoot.transform;
                _transform = mod.AddComponent<PQSMod_CelestialBodyTransform>();
                _transform.sphere = Value;
                _transform.forceActivate = false;
                _transform.deactivateAltitude = 115000;
                _transform.forceRebuildOnTargetChange = false;
                _transform.planetFade = new PQSMod_CelestialBodyTransform.AltitudeFade
                {
                    fadeFloatName = "_PlanetOpacity",
                    fadeStart = 100000.0f,
                    fadeEnd = 110000.0f,
                    valueStart = 0.0f,
                    valueEnd = 1.0f,
                    secondaryRenderers = new List<GameObject>()
                };
                _transform.secondaryFades = new PQSMod_CelestialBodyTransform.AltitudeFade[0];
                _transform.requirements = PQS.ModiferRequirements.Default;
                _transform.modEnabled = true;
                _transform.order = 10;

                // Create the material direction
                // ReSharper disable Unity.InefficientPropertyAccess
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
                // ReSharper restore Unity.InefficientPropertyAccess
            }

            // Assigning the new PQS
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

            // hacky hack
            if (generatedBody.celestialBody.isHomeWorld && Value.gameObject.GetChild("KSC") == null)
            {
                PSystemBody kerbinTemplate = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Kerbin");
                GameObject scTree = kerbinTemplate.pqsVersion.gameObject.GetChild("KSC");
                GameObject newScTree = UnityEngine.Object.Instantiate(scTree, Value.transform, true);
                newScTree.transform.localPosition = scTree.transform.localPosition;
                newScTree.transform.localScale = scTree.transform.localScale;
                newScTree.transform.localRotation = scTree.transform.localRotation;
                newScTree.name = "KSC";
            }

            // Load existing mods
            foreach (PQSMod mod in Value.GetComponentsInChildren<PQSMod>(true))
            {
                Type modType = mod.GetType();
                Type modLoaderType = typeof(ModLoader<>).MakeGenericType(modType);
                foreach (Type loaderType in Parser.ModTypes)
                {
                    if (!modLoaderType.IsAssignableFrom(loaderType))
                    {
                        continue;
                    }

                    // We found our loader type
                    IModLoader loader = (IModLoader) Activator.CreateInstance(loaderType);
                    loader.Create(mod, Value);
                    Mods.Add(loader);
                }
            }
        }

        /// <summary>
        /// Creates a new PQS Loader from a spawned CelestialBody.
        /// </summary>
        [KittopiaConstructor(KittopiaConstructor.ParameterType.CelestialBody)]
        public PQSLoader(CelestialBody body)
        {
            // Is this a spawned body?
            if (body.scaledBody == null || Injector.IsInPrefab)
            {
                throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
            }

            if (body.pqsController != null)
            {
                // Save the PQSVersion
                Value = body.pqsController;

                // Get the required PQS information
                _transform = Value.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true)
                    .FirstOrDefault(mod => mod.transform.parent == Value.transform);
                _collider = Value.GetComponentsInChildren<PQSMod_QuadMeshColliders>(true)
                    .FirstOrDefault(mod => mod.transform.parent == Value.transform);

                // Clone the surface material of the PQS
                if (MaterialType == SurfaceMaterialType.AtmosphericOptimized)
                {
                    SurfaceMaterial = new PQSMainOptimisedLoader(SurfaceMaterial);
                }
                else if (MaterialType == SurfaceMaterialType.AtmosphericMain)
                {
                    SurfaceMaterial = new PQSMainShaderLoader(SurfaceMaterial);
                }
                else if (MaterialType == SurfaceMaterialType.AtmosphericBasic)
                {
                    SurfaceMaterial = new PQSProjectionAerialQuadRelativeLoader(SurfaceMaterial);
                }
                else if (MaterialType == SurfaceMaterialType.Vacuum)
                {
                    SurfaceMaterial = new PQSProjectionSurfaceQuadLoader(SurfaceMaterial);
                }
                else if (MaterialType == SurfaceMaterialType.AtmosphericExtra)
                {
                    SurfaceMaterial = new PQSMainExtrasLoader(SurfaceMaterial);
                }
                else if (MaterialType == SurfaceMaterialType.AtmosphericOptimizedFastBlend)
                {
                    SurfaceMaterial = new PQSMainOptimisedFastBlendLoader(SurfaceMaterial);
                }
                else if (MaterialType == SurfaceMaterialType.AtmosphericTriplanarZoomRotation)
                {
                    SurfaceMaterial = new PQSTriplanarZoomRotationLoader(SurfaceMaterial);
                }

                SurfaceMaterial.name = Guid.NewGuid().ToString();

                // Clone the fallback material of the PQS
                FallbackMaterial = new PQSProjectionFallbackLoader(FallbackMaterial) {name = Guid.NewGuid().ToString()};
            }
            else
            {
                // Create a new PQS
                GameObject controllerRoot = new GameObject();
                controllerRoot.transform.parent = body.transform;
                Value = controllerRoot.AddComponent<PQS>();

                // I (Teknoman) am at this time unable to determine some of the magic parameters which cause the PQS to work...
                // And I (Thomas) am at this time just too lazy to do it differently...
                PSystemBody laythe = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
                Utility.CopyObjectFields(laythe.pqsVersion, Value);
                Value.surfaceMaterial = laythe.pqsVersion.surfaceMaterial;

                // Create the fallback material (always the same shader)
                FallbackMaterial = new PQSProjectionFallbackLoader();
                Value.fallbackMaterial = FallbackMaterial;
                FallbackMaterial.name = Guid.NewGuid().ToString();

                // Create the celestial body transform
                GameObject mod = new GameObject("_CelestialBody");
                mod.transform.parent = controllerRoot.transform;
                _transform = mod.AddComponent<PQSMod_CelestialBodyTransform>();
                _transform.sphere = Value;
                _transform.forceActivate = false;
                _transform.deactivateAltitude = 115000;
                _transform.forceRebuildOnTargetChange = false;
                _transform.planetFade = new PQSMod_CelestialBodyTransform.AltitudeFade
                {
                    fadeFloatName = "_PlanetOpacity",
                    fadeStart = 100000.0f,
                    fadeEnd = 110000.0f,
                    valueStart = 0.0f,
                    valueEnd = 1.0f,
                    secondaryRenderers = new List<GameObject>()
                };
                _transform.secondaryFades = new PQSMod_CelestialBodyTransform.AltitudeFade[0];
                _transform.requirements = PQS.ModiferRequirements.Default;
                _transform.modEnabled = true;
                _transform.order = 10;

                // Create the material direction
                // ReSharper disable Unity.InefficientPropertyAccess
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
                // ReSharper restore Unity.InefficientPropertyAccess
            }

            // Assigning the new PQS
            body.pqsController = Value;
            Transform transform = body.transform;
            body.pqsController.name = transform.name;
            body.pqsController.transform.name = transform.name;
            body.pqsController.gameObject.name = transform.name;
            body.pqsController.radius = body.Radius;

            // Add an OnDemand Handler
            if (Value.GetComponentsInChildren<PQSMod_OnDemandHandler>(true).Length == 0)
            {
                OnDemandStorage.AddHandler(Value);
            }

            // Load existing mods
            // Unlike the above, this checks for the sphere reference because at runtime the ocean is a child of the
            // normal PQS, and we would be getting references to ocean mods without it.
            foreach (PQSMod mod in Value.GetComponentsInChildren<PQSMod>(true).Where(m => m.sphere == Value))
            {
                Type modType = mod.GetType();
                Type modLoaderType = typeof(ModLoader<>).MakeGenericType(modType);
                foreach (Type loaderType in Parser.ModTypes)
                {
                    if (!modLoaderType.IsAssignableFrom(loaderType))
                    {
                        continue;
                    }

                    // We found our loader type
                    IModLoader loader = (IModLoader) Activator.CreateInstance(loaderType);
                    loader.Create(mod, Value);
                    Mods.Add(loader);
                }
            }
        }

        [KittopiaDestructor]
        public void Destroy()
        {
            Object.Destroy(Value.gameObject);
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