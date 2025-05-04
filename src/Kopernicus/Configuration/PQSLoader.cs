﻿/**
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
using CommNet;
using Kopernicus.Components;
using Kopernicus.Components.MaterialWrapper;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Enumerations;
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
    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    public class PQSLoader : BaseLoader, IParserEventSubscriber, ITypeParser<PQS>
    {
        // PQS we are creating
        public PQS Value { get; set; }

        // Required PQSMods
        private readonly PQSMod_CelestialBodyTransform _transform;
        private readonly PQSMod_QuadMeshColliders _collider;
        private bool isMainFastBlend;
        private bool isTriplanarAtlas;

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

        /// <summary>
        /// A helper property that returns the surface material that Kopernicus will support and use.
        /// </summary>
        private Material BasicSurfaceMaterial
        {
            get
            {
#if (!KSP_VERSION_1_8)
                switch (GameSettings.TERRAIN_SHADER_QUALITY)
                {
                    case 3:
                        if (Value.ultraQualitySurfaceMaterial != null)
                        {
                            return Value.ultraQualitySurfaceMaterial;
                        }
                        goto case 2;
                    case 2:
                        if (Value.highQualitySurfaceMaterial != null)
                        {
                            return Value.highQualitySurfaceMaterial;
                        }
                        goto case 1;
                    case 1:
                        if (Value.mediumQualitySurfaceMaterial != null)
                        {
                            return Value.mediumQualitySurfaceMaterial;
                        }
                        goto case 0;
                    case 0:
                        if (Value.lowQualitySurfaceMaterial != null)
                        {
                            return Value.lowQualitySurfaceMaterial;
                        }
                        goto default;
                    default:
                        return Value.surfaceMaterial;
                }
#else
                switch (GameSettings.TERRAIN_SHADER_QUALITY)
                {
                    case 2:
                        if (Value.highQualitySurfaceMaterial != null)
                        {
                            return Value.highQualitySurfaceMaterial;
                        }
                        goto case 1;
                    case 1:
                        if (Value.mediumQualitySurfaceMaterial != null)
                        {
                            return Value.mediumQualitySurfaceMaterial;
                        }
                        goto case 0;
                    case 0:
                        if (Value.lowQualitySurfaceMaterial != null)
                        {
                            return Value.lowQualitySurfaceMaterial;
                        }
                        goto default;
                    default:
                        return Value.surfaceMaterial;
                }
#endif
            }
            set
            {
#if (!KSP_VERSION_1_8)
                Value.ultraQualitySurfaceMaterial = value;
#endif
                Value.highQualitySurfaceMaterial = value;
                Value.mediumQualitySurfaceMaterial = value;
                Value.lowQualitySurfaceMaterial = value;
                Value.surfaceMaterial = value;
            }
        }
#if (KSP_VERSION_1_8)
        [PreApply]
        [ParserTarget("materialType")]
        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        public EnumParser<SurfaceMaterialType> MaterialType
        {
            get
            {
                if (PQSProjectionSurfaceQuad.UsesSameShader(BasicSurfaceMaterial))
                {
                    return SurfaceMaterialType.Vacuum;
                }
                if (PQSProjectionAerialQuadRelative.UsesSameShader(BasicSurfaceMaterial))
                {
                    return SurfaceMaterialType.Basic;
                }
                if (PQSMainShader.UsesSameShader(BasicSurfaceMaterial))
                {
                    return SurfaceMaterialType.Main;
                }
                if (PQSMainOptimised.UsesSameShader(BasicSurfaceMaterial))
                {
                    return SurfaceMaterialType.Optimized;
                }
                if (PQSMainExtras.UsesSameShader(BasicSurfaceMaterial))
                {
                    return SurfaceMaterialType.Extra;
                }
                if (PQSMainOptimisedFastBlend.UsesSameShader(BasicSurfaceMaterial))
                {
                    return SurfaceMaterialType.OptimizedFastBlend;
                }
                if (PQSTriplanarZoomRotation.UsesSameShader(BasicSurfaceMaterial))
                {
                    return SurfaceMaterialType.Triplanar;
                }
                throw new Exception("The shader '" + BasicSurfaceMaterial.shader.name + "' is not supported.");
            }
            set
            {
                Boolean isVaccum = PQSProjectionSurfaceQuad.UsesSameShader(BasicSurfaceMaterial);
                Boolean isBasic = PQSProjectionAerialQuadRelative.UsesSameShader(BasicSurfaceMaterial);
                Boolean isMain = PQSMainShader.UsesSameShader(BasicSurfaceMaterial);
                Boolean isOptimised = PQSMainOptimised.UsesSameShader(BasicSurfaceMaterial);
                Boolean isExtra = PQSMainExtras.UsesSameShader(BasicSurfaceMaterial);
                Boolean isOptimisedFastBlend = PQSMainOptimisedFastBlend.UsesSameShader(BasicSurfaceMaterial);
                Boolean isTriplanar = PQSTriplanarZoomRotation.UsesSameShader(BasicSurfaceMaterial);
                switch (value.Value)
                {
                    case SurfaceMaterialType.Vacuum when !isVaccum:
                        BasicSurfaceMaterial = new PQSProjectionSurfaceQuadLoader();
                        break;
                    case SurfaceMaterialType.Basic when !isBasic:
                        BasicSurfaceMaterial = new PQSProjectionAerialQuadRelativeLoader();
                        break;
                    case SurfaceMaterialType.Main when !isMain:
                        BasicSurfaceMaterial = new PQSMainShaderLoader();
                        break;
                    case SurfaceMaterialType.Optimized when !isOptimised:
                        BasicSurfaceMaterial = new PQSMainOptimisedLoader();
                        break;
                    case SurfaceMaterialType.Extra when !isExtra:
                        BasicSurfaceMaterial = new PQSMainExtrasLoader();
                        break;
                    case SurfaceMaterialType.OptimizedFastBlend when !isOptimisedFastBlend:
                        BasicSurfaceMaterial = new PQSMainOptimisedFastBlendLoader();
                        break;
                    case SurfaceMaterialType.Triplanar when !isTriplanar:
                        BasicSurfaceMaterial = new PQSTriplanarZoomRotationLoader();
                        break;
                    default:
                        return;
                }
            }
        }
#endif
#if (!KSP_VERSION_1_8)
        [PreApply]
        [ParserTarget("materialType")]
        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        public EnumParser<NewShaderSurfaceMaterialType> NewMaterialType
        {
            get
            {
                if (PQSProjectionSurfaceQuad.UsesSameShader(BasicSurfaceMaterial))
                {
                    return NewShaderSurfaceMaterialType.Vacuum;
                }
                if (PQSProjectionAerialQuadRelative.UsesSameShader(BasicSurfaceMaterial))
                {
                    return NewShaderSurfaceMaterialType.Basic;
                }
                if (PQSMainShader.UsesSameShader(BasicSurfaceMaterial))
                {
                    return NewShaderSurfaceMaterialType.Main;
                }
                if (PQSMainOptimised.UsesSameShader(BasicSurfaceMaterial))
                {
                    return NewShaderSurfaceMaterialType.Optimized;
                }
                if (PQSMainExtras.UsesSameShader(BasicSurfaceMaterial))
                {
                    return NewShaderSurfaceMaterialType.Extra;
                }
                if (PQSMainOptimisedFastBlend.UsesSameShader(BasicSurfaceMaterial))
                {
                    return NewShaderSurfaceMaterialType.OptimizedFastBlend;
                }
                if (PQSTriplanarZoomRotation.UsesSameShader(BasicSurfaceMaterial))
                {
                    return NewShaderSurfaceMaterialType.Triplanar;
                }
                if (PQSMainFastBlend.UsesSameShader(BasicSurfaceMaterial))
                {
                    return NewShaderSurfaceMaterialType.MainFastBlend;
                }
                if (PQSTriplanarZoomRotationTextureArray.UsesSameShader(BasicSurfaceMaterial))
                {
                    return NewShaderSurfaceMaterialType.TriplanarAtlas;
                }
                throw new Exception("The shader '" + BasicSurfaceMaterial.shader.name + "' is not supported.");
            }
            set
            {
                Boolean isVaccum = PQSProjectionSurfaceQuad.UsesSameShader(BasicSurfaceMaterial);
                Boolean isBasic = PQSProjectionAerialQuadRelative.UsesSameShader(BasicSurfaceMaterial);
                Boolean isMain = PQSMainShader.UsesSameShader(BasicSurfaceMaterial);
                Boolean isOptimised = PQSMainOptimised.UsesSameShader(BasicSurfaceMaterial);
                Boolean isExtra = PQSMainExtras.UsesSameShader(BasicSurfaceMaterial);
                isMainFastBlend = PQSMainFastBlend.UsesSameShader(BasicSurfaceMaterial);
                Boolean isOptimisedFastBlend = PQSMainOptimisedFastBlend.UsesSameShader(BasicSurfaceMaterial);
                Boolean isTriplanar = PQSTriplanarZoomRotation.UsesSameShader(BasicSurfaceMaterial);
                isTriplanarAtlas = PQSTriplanarZoomRotationTextureArray.UsesSameShader(BasicSurfaceMaterial);
                switch (value.Value)
                {
                    case NewShaderSurfaceMaterialType.Vacuum when !isVaccum:
                        BasicSurfaceMaterial = new PQSProjectionSurfaceQuadLoader();
                        break;
                    case NewShaderSurfaceMaterialType.Basic when !isBasic:
                        BasicSurfaceMaterial = new PQSProjectionAerialQuadRelativeLoader();
                        break;
                    case NewShaderSurfaceMaterialType.Main when !isMain:
                        BasicSurfaceMaterial = new PQSMainShaderLoader();
                        break;
                    case NewShaderSurfaceMaterialType.Optimized when !isOptimised:
                        BasicSurfaceMaterial = new PQSMainOptimisedLoader();
                        break;
                    case NewShaderSurfaceMaterialType.Extra when !isExtra:
                        BasicSurfaceMaterial = new PQSMainExtrasLoader();
                        break;
                    case NewShaderSurfaceMaterialType.MainFastBlend when !isMainFastBlend:
                        BasicSurfaceMaterial = new PQSMainFastBlendLoader();
                        break;
                    case NewShaderSurfaceMaterialType.OptimizedFastBlend when !isOptimisedFastBlend:
                        BasicSurfaceMaterial = new PQSMainOptimisedFastBlendLoader();
                        break;
                    case NewShaderSurfaceMaterialType.Triplanar when !isTriplanar:
                        BasicSurfaceMaterial = new PQSTriplanarZoomRotationLoader();
                        break;
                    case NewShaderSurfaceMaterialType.TriplanarAtlas when !isTriplanarAtlas:
                        BasicSurfaceMaterial = new PQSTriplanarZoomRotationTextureArrayLoader();
                        break;
                    default:
                        return;
                }
            }
        }
#endif
        // Surface Material of the PQS
        [ParserTarget("Material", AllowMerge = true, GetChild = false)]
        [KittopiaUntouchable]
        public Material SurfaceMaterial
        {
            get
            {
                Boolean isVaccum = BasicSurfaceMaterial is PQSProjectionSurfaceQuadLoader;
                Boolean isBasic = BasicSurfaceMaterial is PQSProjectionAerialQuadRelativeLoader;
                Boolean isMain = BasicSurfaceMaterial is PQSMainShaderLoader;
                Boolean isOptimised = BasicSurfaceMaterial is PQSMainOptimisedLoader;
                Boolean isExtra = BasicSurfaceMaterial is PQSMainExtrasLoader;
#if (!KSP_VERSION_1_8)
                isMainFastBlend = BasicSurfaceMaterial is PQSMainFastBlendLoader;
#endif
                Boolean isOptimisedFastBlend = BasicSurfaceMaterial is PQSMainOptimisedFastBlendLoader;
                Boolean isTriplanar = BasicSurfaceMaterial is PQSTriplanarZoomRotationLoader;
#if (!KSP_VERSION_1_8)
                isTriplanarAtlas = BasicSurfaceMaterial is PQSTriplanarZoomRotationTextureArrayLoader;
#endif
#if (KSP_VERSION_1_8)
                switch (MaterialType.Value)
                {
                    case SurfaceMaterialType.Vacuum when !isVaccum:
                        BasicSurfaceMaterial = new PQSProjectionSurfaceQuadLoader(BasicSurfaceMaterial);
                        goto default;
                    case SurfaceMaterialType.Basic when !isBasic:
                        BasicSurfaceMaterial = new PQSProjectionAerialQuadRelativeLoader(BasicSurfaceMaterial);
                        goto default;
                    case SurfaceMaterialType.Main when !isMain:
                        BasicSurfaceMaterial = new PQSMainShaderLoader(BasicSurfaceMaterial);
                        goto default;
                    case SurfaceMaterialType.Optimized when !isOptimised:
                        BasicSurfaceMaterial = new PQSMainOptimisedLoader(BasicSurfaceMaterial);
                        goto default;
                    case SurfaceMaterialType.Extra when !isExtra:
                        BasicSurfaceMaterial = new PQSMainExtrasLoader(BasicSurfaceMaterial);
                        goto default;
                    case SurfaceMaterialType.OptimizedFastBlend when !isOptimisedFastBlend:
                        BasicSurfaceMaterial = new PQSMainOptimisedFastBlendLoader(BasicSurfaceMaterial);
                        goto default;
                    case SurfaceMaterialType.Triplanar when !isTriplanar:
                        BasicSurfaceMaterial = new PQSTriplanarZoomRotationLoader(BasicSurfaceMaterial);
                        goto default;
                    default:
                        return BasicSurfaceMaterial;
                }
#endif
#if (!KSP_VERSION_1_8)
                switch (NewMaterialType.Value)
                {
                    case NewShaderSurfaceMaterialType.Vacuum when !isVaccum:
                        BasicSurfaceMaterial = new PQSProjectionSurfaceQuadLoader(BasicSurfaceMaterial);
                        goto default;
                    case NewShaderSurfaceMaterialType.Basic when !isBasic:
                        BasicSurfaceMaterial = new PQSProjectionAerialQuadRelativeLoader(BasicSurfaceMaterial);
                        goto default;
                    case NewShaderSurfaceMaterialType.Main when !isMain:
                        BasicSurfaceMaterial = new PQSMainShaderLoader(BasicSurfaceMaterial);
                        goto default;
                    case NewShaderSurfaceMaterialType.Optimized when !isOptimised:
                        BasicSurfaceMaterial = new PQSMainOptimisedLoader(BasicSurfaceMaterial);
                        goto default;
                    case NewShaderSurfaceMaterialType.Extra when !isExtra:
                        BasicSurfaceMaterial = new PQSMainExtrasLoader(BasicSurfaceMaterial);
                        goto default;
                    case NewShaderSurfaceMaterialType.MainFastBlend when !isMainFastBlend:
                        BasicSurfaceMaterial = new PQSMainFastBlendLoader(BasicSurfaceMaterial);
                        goto default;
                    case NewShaderSurfaceMaterialType.OptimizedFastBlend when !isOptimisedFastBlend:
                        BasicSurfaceMaterial = new PQSMainOptimisedFastBlendLoader(BasicSurfaceMaterial);
                        goto default;
                    case NewShaderSurfaceMaterialType.Triplanar when !isTriplanar:
                        BasicSurfaceMaterial = new PQSTriplanarZoomRotationLoader(BasicSurfaceMaterial);
                        goto default;
                    case NewShaderSurfaceMaterialType.TriplanarAtlas when !isTriplanarAtlas:
                        BasicSurfaceMaterial = new PQSTriplanarZoomRotationTextureArrayLoader(BasicSurfaceMaterial);
                        goto default;
                    default:
                        return BasicSurfaceMaterial;
                }
#endif
            }
            set
            {
                Boolean isVaccum = value is PQSProjectionSurfaceQuadLoader;
                Boolean isBasic = value is PQSProjectionAerialQuadRelativeLoader;
                Boolean isMain = value is PQSMainShaderLoader;
                Boolean isOptimised = value is PQSMainOptimisedLoader;
                Boolean isExtra = value is PQSMainExtrasLoader;
#if (!KSP_VERSION_1_8)
                if (!(Versioning.version_minor < 9))
                {
                    isMainFastBlend = value is PQSMainFastBlendLoader;
                }
#endif
                Boolean isOptimisedFastBlend = value is PQSMainOptimisedFastBlendLoader;
                Boolean isTriplanar = value is PQSTriplanarZoomRotationLoader;
#if (!KSP_VERSION_1_8)
                if (!(Versioning.version_minor < 9))
                {
                    isTriplanarAtlas = value is PQSTriplanarZoomRotationTextureArrayLoader;
                }
#endif
                // We need to set the material before we check it, so we can reuse the code in MaterialType
                BasicSurfaceMaterial = value;
#if (KSP_VERSION_1_8)
                switch (MaterialType.Value)
                {
                    case SurfaceMaterialType.Vacuum when !isVaccum:
                        BasicSurfaceMaterial = new PQSProjectionSurfaceQuadLoader(value);
                        break;
                    case SurfaceMaterialType.Basic when !isBasic:
                        BasicSurfaceMaterial = new PQSProjectionAerialQuadRelativeLoader(value);
                        break;
                    case SurfaceMaterialType.Main when !isMain:
                        BasicSurfaceMaterial = new PQSMainShaderLoader(value);
                        break;
                    case SurfaceMaterialType.Optimized when !isOptimised:
                        BasicSurfaceMaterial = new PQSMainOptimisedLoader(value);
                        break;
                    case SurfaceMaterialType.Extra when !isExtra:
                        BasicSurfaceMaterial = new PQSMainExtrasLoader(value);
                        break;
                    case SurfaceMaterialType.OptimizedFastBlend when !isOptimisedFastBlend:
                        BasicSurfaceMaterial = new PQSMainOptimisedFastBlendLoader(value);
                        break;
                    case SurfaceMaterialType.Triplanar when !isTriplanar:
                        BasicSurfaceMaterial = new PQSTriplanarZoomRotationLoader(value);
                        break;
                    default:
                        BasicSurfaceMaterial = value;
                        break;
                }
#endif
#if (!KSP_VERSION_1_8)
                switch (NewMaterialType.Value)
                {
                    case NewShaderSurfaceMaterialType.Vacuum when !isVaccum:
                        BasicSurfaceMaterial = new PQSProjectionSurfaceQuadLoader(value);
                        break;
                    case NewShaderSurfaceMaterialType.Basic when !isBasic:
                        BasicSurfaceMaterial = new PQSProjectionAerialQuadRelativeLoader(value);
                        break;
                    case NewShaderSurfaceMaterialType.Main when !isMain:
                        BasicSurfaceMaterial = new PQSMainShaderLoader(value);
                        break;
                    case NewShaderSurfaceMaterialType.Optimized when !isOptimised:
                        BasicSurfaceMaterial = new PQSMainOptimisedLoader(value);
                        break;
                    case NewShaderSurfaceMaterialType.Extra when !isExtra:
                        BasicSurfaceMaterial = new PQSMainExtrasLoader(value);
                        break;
                    case NewShaderSurfaceMaterialType.MainFastBlend when !isMainFastBlend:
                        BasicSurfaceMaterial = new PQSMainFastBlendLoader(value);
                        break;
                    case NewShaderSurfaceMaterialType.OptimizedFastBlend when !isOptimisedFastBlend:
                        BasicSurfaceMaterial = new PQSMainOptimisedFastBlendLoader(value);
                        break;
                    case NewShaderSurfaceMaterialType.Triplanar when !isTriplanar:
                        BasicSurfaceMaterial = new PQSTriplanarZoomRotationLoader(value);
                        break;
                    case NewShaderSurfaceMaterialType.TriplanarAtlas when !isTriplanarAtlas:
                        BasicSurfaceMaterial = new PQSTriplanarZoomRotationTextureArrayLoader(value);
                        break;
                    default:
                        BasicSurfaceMaterial = value;
                        break;
                }
#endif
            }
        }

        // Fallback Material of the PQS (its always the same material)
        [ParserTarget("FallbackMaterial", AllowMerge = true, GetChild = false)]
        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        [KittopiaUntouchable]
        public Material FallbackMaterial
        {
            get
            {
                if (!(Value.fallbackMaterial is PQSProjectionFallbackLoader))
                {
                    Value.fallbackMaterial = new PQSProjectionFallbackLoader(Value.fallbackMaterial);
                }

                return Value.fallbackMaterial;
            }
            set
            {
                if (value is PQSProjectionFallbackLoader)
                {
                    Value.fallbackMaterial = value;
                }
                else
                {
                    Value.fallbackMaterial = new PQSProjectionFallbackLoader(value);
                }
            }
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
                _transform = Utility.GetMod<PQSMod_CelestialBodyTransform>(Value);
                _collider = Utility.GetMod<PQSMod_QuadMeshColliders>(Value);
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

                // Create the fallback material (always the same shader)
                FallbackMaterial = new PQSProjectionFallbackLoader();

                // Create the celestial body transform
                _transform = Utility.AddMod<PQSMod_CelestialBodyTransform>(Value, 10);
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

                // Crete the quad mesh colliders
                _collider = Utility.AddMod<PQSMod_QuadMeshColliders>(Value, 100);
                _collider.maxLevelOffset = 0;

                // Create the material direction
                Utility.AddMod<PQSMod_MaterialSetDirection>(Value, 100).valueName = "_sunLightDirection";

                // Create the UV planet relative position
                Utility.AddMod<PQSMod_UVPlanetRelativePosition>(Value, 999999);
            }

            // Assigning the new PQS
            generatedBody.pqsVersion = Value;
            generatedBody.pqsVersion.name = generatedBody.name;
            generatedBody.pqsVersion.transform.name = generatedBody.name;
            generatedBody.pqsVersion.gameObject.name = generatedBody.name;
            generatedBody.pqsVersion.radius = generatedBody.celestialBody.Radius;
            generatedBody.celestialBody.pqsController = generatedBody.pqsVersion;

            // Add an OnDemand Handler
            if (!Utility.HasMod<PQSMod_OnDemandHandler>(Value))
            {
                Utility.AddMod<PQSMod_OnDemandHandler>(Value, 0);
            }
            // Add fixes for TextureAtlas
#if (!KSP_VERSION_1_8)
            if (!(Versioning.version_minor < 9))
            {
                if (!Utility.HasMod<PQSMod_TextureAtlasFixer>(Value))
                {
                    Utility.AddMod<PQSMod_TextureAtlasFixer>(Value, 0);
                }
            }
#endif
            // hacky hack
            if (generatedBody.celestialBody.name.Equals(RuntimeUtility.RuntimeUtility.KopernicusConfig.HomeWorldName) && (Value.gameObject.GetChild("KSC") == null))
            {
                PSystemBody kerbinTemplate = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Kerbin");
                GameObject scTree = kerbinTemplate.pqsVersion.gameObject.GetChild("KSC");
                GameObject newScTree = Object.Instantiate(scTree, Value.transform, true);
                newScTree.transform.localPosition = scTree.transform.localPosition;
                newScTree.transform.localScale = scTree.transform.localScale;
                newScTree.transform.localRotation = scTree.transform.localRotation;
                newScTree.name = "KSC";
            }

            // Add the PQSROCControl mod for surface anomalies
            if (!Utility.HasMod<PQSROCControl>(Value))
            {
                PQSROCControl roc = Utility.AddMod<PQSROCControl>(Value, 999999);
                roc.rocs = new List<LandClassROC>();
                roc.currentCBName = Value.name;
            }
            else
            {
                PQSROCControl roc = Utility.GetMod<PQSROCControl>(Value);
                roc.currentCBName = Value.name;
            }

            // Load existing mods
            PQSMod[] mods = Utility.GetMods(Value);
            for (Int32 i = 0; i < mods.Length; i++)
            {
                Type modType = mods[i].GetType();
                if ((modType.Name.Equals("PQSCity") && mods[i].name.Equals("KSC2")) && RuntimeUtility.RuntimeUtility.KopernicusConfig.UseOriginalKSC2)
                {
                    PSystemBody kerbinTemplate = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Kerbin");
                    PQSCity scTree = kerbinTemplate.pqsVersion.GetComponentsInChildren<PQSCity>(true).First(m => m.name == "KSC2");
                    PQSCity newScTree = Object.Instantiate(scTree, Value.transform, true);
                    newScTree.transform.localPosition = scTree.transform.localPosition;
                    newScTree.transform.localScale = scTree.transform.localScale;
                    newScTree.transform.localRotation = scTree.transform.localRotation;
                    Utility.CopyObjectFields<PQSCity>(scTree, newScTree);
                    newScTree.name = "KSC2";
                    GameObject.DestroyImmediate(mods[i].gameObject.GetComponentInChildren<CommNetHome>());
                    GameObject.DestroyImmediate(mods[i]);
                }
                else
                {
                    Type modLoaderType = typeof(ModLoader<>).MakeGenericType(modType);

                    for (Int32 j = 0; j < Parser.ModTypes.Count; j++)
                    {
                        if (!modLoaderType.IsAssignableFrom(Parser.ModTypes[j]))
                        {
                            continue;
                        }

                        IModLoader loader = (IModLoader)Activator.CreateInstance(Parser.ModTypes[j]);
                        loader.Create(mods[i], Value);
                        Mods.Add(loader);
                    }
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
                _transform = Utility.GetMod<PQSMod_CelestialBodyTransform>(Value);
                _collider = Utility.GetMod<PQSMod_QuadMeshColliders>(Value);
            }
            else
            {
                // Create a new PQS
                GameObject controllerRoot = new GameObject();
                controllerRoot.transform.parent = body.transform;
                Value = controllerRoot.AddComponent<PQS>();

                PSystemBody laythe = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
                Utility.CopyObjectFields(laythe.pqsVersion, Value);

                // Create the fallback material (always the same shader)
                FallbackMaterial = new PQSProjectionFallbackLoader();

                // Create the celestial body transform
                _transform = Utility.AddMod<PQSMod_CelestialBodyTransform>(Value, 10);
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

                // Crete the quad mesh colliders
                _collider = Utility.AddMod<PQSMod_QuadMeshColliders>(Value, 100);
                _collider.maxLevelOffset = 0;

                // Create the material direction
                Utility.AddMod<PQSMod_MaterialSetDirection>(Value, 100).valueName = "_sunLightDirection";

                // Create the UV planet relative position
                Utility.AddMod<PQSMod_UVPlanetRelativePosition>(Value, 999999);
            }

            // Assigning the new PQS
            body.pqsController = Value;
            Transform transform = body.transform;
            body.pqsController.name = transform.name;
            body.pqsController.transform.name = transform.name;
            body.pqsController.gameObject.name = transform.name;
            body.pqsController.radius = body.Radius;
            // Add an OnDemand Handler
            if (!Utility.HasMod<PQSMod_OnDemandHandler>(Value))
            {
                Utility.AddMod<PQSMod_OnDemandHandler>(Value, 0);
            }
#if (!KSP_VERSION_1_8)
            if (!(Versioning.version_minor < 9))
            {
                if (!Utility.HasMod<PQSMod_TextureAtlasFixer>(Value))
                {
                    Utility.AddMod<PQSMod_TextureAtlasFixer>(Value, 0);
                }
            }
#endif
            // Add the PQSROCControl mod for surface anomalies
            if (!Utility.HasMod<PQSROCControl>(Value))
            {
                PQSROCControl roc = Utility.AddMod<PQSROCControl>(Value, 999999);
                roc.rocs = new List<LandClassROC>();
                roc.currentCBName = Value.name;
            }
            else
            {
                PQSROCControl roc = Utility.GetMod<PQSROCControl>(Value);
                roc.currentCBName = Value.name;
            }

            // Load existing mods
            PQSMod[] mods = Utility.GetMods(Value);
            for (Int32 i = 0; i < mods.Length; i++)
            {
                Type modType = mods[i].GetType();
                if ((modType.Name.Equals("PQSCity") && mods[i].name.Equals("KSC2")) && RuntimeUtility.RuntimeUtility.KopernicusConfig.UseOriginalKSC2)
                {
                    GameObject.DestroyImmediate(mods[i].gameObject.GetComponentInChildren<CommNetHome>());
                    GameObject.DestroyImmediate(mods[i]);
                }
                else
                {
                    Type modLoaderType = typeof(ModLoader<>).MakeGenericType(modType);

                    for (Int32 j = 0; j < Parser.ModTypes.Count; j++)
                    {
                        if (!modLoaderType.IsAssignableFrom(Parser.ModTypes[j]))
                        {
                            continue;
                        }

                        IModLoader loader = (IModLoader)Activator.CreateInstance(Parser.ModTypes[j]);
                        loader.Create(mods[i], Value);
                        Mods.Add(loader);
                    }
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
            //Utility.GameObjectWalk(Value.gameObject, "  ");
            // -------------------------------
        }
    }
}
