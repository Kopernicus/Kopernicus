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
using Kopernicus.Components;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.MaterialLoader;
using Kopernicus.Configuration.ModLoader;
using Kopernicus.Configuration.Parsing;
using Kopernicus.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kopernicus.Configuration
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public class OceanLoader : BaseLoader, IParserEventSubscriber, ITypeParser<PQS>
    {
        /// <summary>
        /// The Ocean PQS we're editing
        /// </summary>
        public PQS Value { get; set; }

        /// <summary>
        /// CelestialBody we're editing
        /// </summary>
        public CelestialBody Body { get; set; }

        // We have an ocean?
        [ParserTarget("ocean")]
        public NumericParser<Boolean> MapOcean
        {
            get { return Value.parentSphere.mapOcean && Body.ocean; }
            set { Value.parentSphere.mapOcean = Body.ocean = value; }
        }

        // Color of the ocean on the map
        [ParserTarget("oceanColor")]
        public ColorParser OceanColor
        {
            get { return Value.parentSphere.mapOceanColor; }
            set { Value.parentSphere.mapOceanColor = value; }
        }

        // Height of the Ocean
        [ParserTarget("oceanHeight")]
        public NumericParser<Double> OceanHeight
        {
            get { return Value.parentSphere.mapOceanHeight; }
            set { Value.parentSphere.mapOceanHeight = value; }
        }

        // Density of the Ocean
        [ParserTarget("density")]
        public NumericParser<Double> Density
        {
            get { return Body.oceanDensity; }
            set { Body.oceanDensity = value; }
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
        [ParserTarget("FallbackMaterial", AllowMerge = true)]
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

        // Ocean-Fog
        [ParserTarget("Fog", AllowMerge = true)]
        [KittopiaUntouchable]
        public FogLoader Fog { get; set; }

        /// <summary>
        /// Creates a new Ocean Loader from the Injector context.
        /// </summary>
        public OceanLoader()
        {
            // Is this the parser context?
            if (!Injector.IsInPrefab)
            {
                throw new InvalidOperationException("Must be executed in Injector context.");
            }

            if (generatedBody.pqsVersion.GetComponentsInChildren<PQS>(true).Any(p => p.name.EndsWith("Ocean")))
            {
                // Save the PQSVersion
                Value = generatedBody.pqsVersion.GetComponentsInChildren<PQS>(true)
                    .First(p => p.name.EndsWith("Ocean"));

                // Get the required PQS information
                SurfaceMaterial = new PQSOceanSurfaceQuadLoader(SurfaceMaterial) {name = Guid.NewGuid().ToString()};

                // Clone the fallback material of the PQS
                FallbackMaterial = new PQSOceanSurfaceQuadFallbackLoader(FallbackMaterial)
                {
                    name = Guid.NewGuid().ToString()
                };
            }
            else
            {
                // Create a new PQS
                GameObject controllerRoot = new GameObject();
                controllerRoot.transform.parent = generatedBody.pqsVersion.transform;
                Value = controllerRoot.AddComponent<PQS>();

                // I (Teknoman) am at this time unable to determine some of the magic parameters which cause the PQS to work...
                // And I (Thomas) am at this time just too lazy to do it differently...
                PSystemBody laythe = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
                foreach (PQS oc in laythe.pqsVersion.GetComponentsInChildren<PQS>(true))
                {
                    if (oc.name != "LaytheOcean")
                    {
                        continue;
                    }
                    
                    // Copying Laythes Ocean-properties
                    Utility.CopyObjectFields(oc, Value);

                    // Load Surface material
                    SurfaceMaterial = new PQSOceanSurfaceQuadLoader(oc.surfaceMaterial);

                    // Load Fallback-Material
                    FallbackMaterial = new PQSOceanSurfaceQuadFallbackLoader(oc.fallbackMaterial);
                    break;
                }

                // Load our new Material into the PQS
                SurfaceMaterial.name = Guid.NewGuid().ToString();

                // Load fallback material into the PQS            
                FallbackMaterial.name = Guid.NewGuid().ToString();

                // Create the UV planet relative position
                GameObject mod = new GameObject("_Material_SurfaceQuads");
                mod.transform.parent = controllerRoot.transform;
                PQSMod_UVPlanetRelativePosition uvs = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
                uvs.sphere = Value;
                uvs.requirements = PQS.ModiferRequirements.Default;
                uvs.modEnabled = true;
                uvs.order = 999999;
            }

            // Assigning the new PQS
            Body = generatedBody.celestialBody;
            Value.name = generatedBody.name + "Ocean";
            Value.transform.name = generatedBody.name + "Ocean";
            Value.gameObject.name = generatedBody.name + "Ocean";
            Value.radius = generatedBody.celestialBody.Radius;
            Value.parentSphere = generatedBody.pqsVersion;

            // Add the ocean PQS to the secondary renders of the CelestialBody Transform
            PQSMod_CelestialBodyTransform transform = generatedBody.pqsVersion
                .GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true)
                .FirstOrDefault(m => m.transform.parent == generatedBody.pqsVersion.transform);
            if (transform != null)
            {
                transform.planetFade.secondaryRenderers.Add(Value.gameObject);
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
        /// Creates a new Ocean Loader from a spawned CelestialBody.
        /// </summary>
        [KittopiaConstructor(KittopiaConstructor.ParameterType.CelestialBody)]
        public OceanLoader(CelestialBody body)
        {
            // Is this a spawned body?
            if (body.scaledBody == null || Injector.IsInPrefab)
            {
                throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
            }

            if (body.pqsController.GetComponentsInChildren<PQS>(true).Any(p => p.name.EndsWith("Ocean")))
            {
                // Save the PQSVersion
                Value = body.pqsController.GetComponentsInChildren<PQS>(true).First(p => p.name.EndsWith("Ocean"));

                // Get the required PQS information
                SurfaceMaterial = new PQSOceanSurfaceQuadLoader(SurfaceMaterial) {name = Guid.NewGuid().ToString()};

                // Clone the fallback material of the PQS
                FallbackMaterial = new PQSOceanSurfaceQuadFallbackLoader(FallbackMaterial)
                {
                    name = Guid.NewGuid().ToString()
                };
            }
            else
            {
                // Create a new PQS
                GameObject controllerRoot = new GameObject();
                controllerRoot.transform.parent = body.pqsController.transform;
                Value = controllerRoot.AddComponent<PQS>();

                // I (Teknoman) am at this time unable to determine some of the magic parameters which cause the PQS to work...
                // And I (Thomas) am at this time just too lazy to do it differently...
                PSystemBody laythe = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
                foreach (PQS oc in laythe.pqsVersion.GetComponentsInChildren<PQS>(true))
                {
                    if (oc.name != "LaytheOcean")
                    {
                        continue;
                    }
                    
                    // Copying Laythes Ocean-properties
                    Utility.CopyObjectFields(oc, Value);

                    // Load Surface material
                    SurfaceMaterial = new PQSOceanSurfaceQuadLoader(oc.surfaceMaterial);

                    // Load Fallback-Material
                    FallbackMaterial = new PQSOceanSurfaceQuadFallbackLoader(oc.fallbackMaterial);
                    break;
                }

                // Load our new Material into the PQS
                SurfaceMaterial.name = Guid.NewGuid().ToString();

                // Load fallback material into the PQS            
                FallbackMaterial.name = Guid.NewGuid().ToString();

                // Create the UV planet relative position
                GameObject mod = new GameObject("_Material_SurfaceQuads");
                mod.transform.parent = controllerRoot.transform;
                PQSMod_UVPlanetRelativePosition uvs = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
                uvs.sphere = Value;
                uvs.requirements = PQS.ModiferRequirements.Default;
                uvs.modEnabled = true;
                uvs.order = 999999;
            }

            // Assigning the new PQS
            Body = body;
            Transform bodyTransform = body.transform;
            Value.name = bodyTransform.name + "Ocean";
            Value.transform.name = bodyTransform.name + "Ocean";
            Value.gameObject.name = bodyTransform.name + "Ocean";
            Value.radius = body.Radius;
            Value.parentSphere = body.pqsController;

            // Add the ocean PQS to the secondary renders of the CelestialBody Transform
            PQSMod_CelestialBodyTransform transform = body.pqsController
                .GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true)
                .FirstOrDefault(m => m.transform.parent == body.pqsController.transform);
            if (transform != null)
            {
                transform.planetFade.secondaryRenderers.Add(Value.gameObject);
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

            Fog = new FogLoader(body);
        }

        // Apply Event
        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            // Make assumptions
            MapOcean = true;

            // Share the current PQS
            Parser.SetState("Kopernicus:pqsVersion", () => Value);

            // Event
            Events.OnOceanLoaderApply.Fire(this, node);
        }

        // Post Apply
        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
            // Reset the PQS state
            Parser.ClearState("Kopernicus:pqsVersion");

            // Event
            Events.OnOceanLoaderPostApply.Fire(this, node);
        }
    }
}