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

using Kopernicus.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kopernicus.Configuration.ModLoader;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration 
    {
        [RequireConfigType(ConfigType.Node)]
        public class OceanLoader : BaseLoader, IParserEventSubscriber, ITypeParser<PQS>
        {
            /// <summary>
            /// The Ocean PQS we're editing
            /// </summary>
            public PQS Value { get; set; }

            /// <summary>
            /// CelestialBody we're editing
            /// </summary>
            private CelestialBody body { get; set; }

            // We have an ocean?
            [ParserTarget("ocean")]
            public NumericParser<Boolean> mapOcean
            {
                get { return Value.parentSphere.mapOcean && body.ocean; }
                set { Value.parentSphere.mapOcean = body.ocean = value; }
            }

            // Color of the ocean on the map
            [ParserTarget("oceanColor")]
            public ColorParser oceanColor
            {
                get { return Value.parentSphere.mapOceanColor; }
                set { Value.parentSphere.mapOceanColor = value; }
            }

            // Height of the Ocean
            [ParserTarget("oceanHeight")]
            public NumericParser<Double> oceanHeight
            {
                get { return Value.parentSphere.mapOceanHeight; }
                set { Value.parentSphere.mapOceanHeight = value; }
            }

            // Density of the Ocean
            [ParserTarget("density")]
            public NumericParser<Double> density
            {
                get { return body.oceanDensity; }
                set { body.oceanDensity = value; }
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

            // Surface Material of the PQS
            [ParserTarget("Material", allowMerge = true, getChild = false)]
            public Material surfaceMaterial
            {
                get { return Value.surfaceMaterial; }
                set { Value.surfaceMaterial = value; }
            }

            // Fallback Material of the PQS (its always the same material)
            [ParserTarget("FallbackMaterial", allowMerge = true)]
            public Material fallbackMaterial
            {
                get { return Value.fallbackMaterial; }
                set { Value.fallbackMaterial = value; }
            }

            // PQSMod loader
            [ParserTargetCollection("Mods", allowMerge = true, nameSignificance = NameSignificance.Type)]
            public List<IModLoader> mods = new List<IModLoader>();

            // Killer-Ocean
            [ParserTarget("HazardousOcean", allowMerge = true)]
            public FloatCurveParser hazardousOcean;

            // Ocean-Fog
            [ParserTarget("Fog", allowMerge = true)]
            public FogLoader fog;
            
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
                    Value = generatedBody.pqsVersion.GetComponentsInChildren<PQS>(true).First(p => p.name.EndsWith("Ocean"));

                    // Get the required PQS information
                    surfaceMaterial = new PQSOceanSurfaceQuadLoader(surfaceMaterial);
                    surfaceMaterial.name = Guid.NewGuid().ToString();

                    // Clone the fallback material of the PQS
                    fallbackMaterial = new PQSOceanSurfaceQuadFallbackLoader(fallbackMaterial);
                    fallbackMaterial.name = Guid.NewGuid().ToString();
                }
                else
                {
                    // Create a new PQS
                    GameObject controllerRoot = new GameObject();
                    controllerRoot.transform.parent = generatedBody.pqsVersion.transform;
                    Value = controllerRoot.AddComponent<PQS>();

                    // I (Teknoman) am at this time unable to determine some of the magic parameters which cause the PQS to work...
                    // And I (Thomas) am at this time just too lazy to do it differently...
                    PSystemBody Laythe = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
                    foreach (PQS oc in Laythe.pqsVersion.GetComponentsInChildren<PQS>(true))
                    {
                        if (oc.name == "LaytheOcean")
                        {
                            // Copying Laythes Ocean-properties
                            Utility.CopyObjectFields(oc, Value);

                            // Load Surface material
                            surfaceMaterial = new PQSOceanSurfaceQuadLoader(oc.surfaceMaterial);

                            // Load Fallback-Material
                            fallbackMaterial = new PQSOceanSurfaceQuadFallbackLoader(oc.fallbackMaterial);
                            break;
                        }
                    }

                    // Load our new Material into the PQS
                    surfaceMaterial.name = Guid.NewGuid().ToString();

                    // Load fallback material into the PQS            
                    fallbackMaterial.name = Guid.NewGuid().ToString();

                    // Create the UV planet relative position
                    GameObject mod = new GameObject("_Material_SurfaceQuads");
                    mod.transform.parent = controllerRoot.transform;
                    PQSMod_UVPlanetRelativePosition uvs = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
                    uvs.sphere = Value;
                    uvs.requirements = PQS.ModiferRequirements.Default;
                    uvs.modEnabled = true;
                    uvs.order = 999999;

                    // Create the fallback material (always the same shader)
                    fallbackMaterial = new PQSProjectionFallbackLoader();
                    Value.fallbackMaterial = fallbackMaterial;
                    fallbackMaterial.name = Guid.NewGuid().ToString();
                }

                // Assing the new PQS
                Value.name = generatedBody.name + "Ocean";
                Value.transform.name = generatedBody.name + "Ocean";
                Value.gameObject.name = generatedBody.name + "Ocean";
                Value.radius = generatedBody.celestialBody.Radius;
                Value.parentSphere = generatedBody.pqsVersion;

                // Add the ocean PQS to the secondary renders of the CelestialBody Transform
                PQSMod_CelestialBodyTransform transform = generatedBody.pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true).FirstOrDefault(m => m.transform.parent == generatedBody.pqsVersion.transform);
                transform.planetFade.secondaryRenderers.Add(Value.gameObject);
                
                // Load existing mods
                foreach (PQSMod mod in Value.GetComponentsInChildren<PQSMod>(true)
                    .Where(m => m.sphere = Value))
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
            /// Creates a new Ocean Loader from a spawned CelestialBody.
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.CelestialBody)]
            public OceanLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }
                
                if (body.pqsController.GetComponentsInChildren<PQS>(true).Any(p => p.name.EndsWith("Ocean")))
                {
                    // Save the PQSVersion
                    Value = body.pqsController.GetComponentsInChildren<PQS>(true).First(p => p.name.EndsWith("Ocean"));

                    // Get the required PQS information
                    surfaceMaterial = new PQSOceanSurfaceQuadLoader(surfaceMaterial);
                    surfaceMaterial.name = Guid.NewGuid().ToString();

                    // Clone the fallback material of the PQS
                    fallbackMaterial = new PQSOceanSurfaceQuadFallbackLoader(fallbackMaterial);
                    fallbackMaterial.name = Guid.NewGuid().ToString();
                }
                else
                {
                    // Create a new PQS
                    GameObject controllerRoot = new GameObject();
                    controllerRoot.transform.parent = body.pqsController.transform;
                    Value = controllerRoot.AddComponent<PQS>();

                    // I (Teknoman) am at this time unable to determine some of the magic parameters which cause the PQS to work...
                    // And I (Thomas) am at this time just too lazy to do it differently...
                    PSystemBody Laythe = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
                    foreach (PQS oc in Laythe.pqsVersion.GetComponentsInChildren<PQS>(true))
                    {
                        if (oc.name == "LaytheOcean")
                        {
                            // Copying Laythes Ocean-properties
                            Utility.CopyObjectFields(oc, Value);

                            // Load Surface material
                            surfaceMaterial = new PQSOceanSurfaceQuadLoader(oc.surfaceMaterial);

                            // Load Fallback-Material
                            fallbackMaterial = new PQSOceanSurfaceQuadFallbackLoader(oc.fallbackMaterial);
                            break;
                        }
                    }

                    // Load our new Material into the PQS
                    surfaceMaterial.name = Guid.NewGuid().ToString();

                    // Load fallback material into the PQS            
                    fallbackMaterial.name = Guid.NewGuid().ToString();

                    // Create the UV planet relative position
                    GameObject mod = new GameObject("_Material_SurfaceQuads");
                    mod.transform.parent = controllerRoot.transform;
                    PQSMod_UVPlanetRelativePosition uvs = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
                    uvs.sphere = Value;
                    uvs.requirements = PQS.ModiferRequirements.Default;
                    uvs.modEnabled = true;
                    uvs.order = 999999;

                    // Create the fallback material (always the same shader)
                    fallbackMaterial = new PQSProjectionFallbackLoader();
                    Value.fallbackMaterial = fallbackMaterial;
                    fallbackMaterial.name = Guid.NewGuid().ToString();
                }

                // Assing the new PQS
                Value.name = body.transform.name + "Ocean";
                Value.transform.name = body.transform.name + "Ocean";
                Value.gameObject.name = body.transform.name + "Ocean";
                Value.radius = body.Radius;
                Value.parentSphere = body.pqsController;

                // Add the ocean PQS to the secondary renders of the CelestialBody Transform
                PQSMod_CelestialBodyTransform transform = body.pqsController.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true).FirstOrDefault(m => m.transform.parent == generatedBody.pqsVersion.transform);
                transform.planetFade.secondaryRenderers.Add(Value.gameObject);
                
                // Load existing mods
                foreach (PQSMod mod in Value.GetComponentsInChildren<PQSMod>(true)
                    .Where(m => m.sphere = Value))
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

            // Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                // Make assumptions
                mapOcean = true;
                
                // Share the current PQS
                Parser.SetState("Kopernicus:pqsVersion", () => Value);
                
                Events.OnOceanLoaderApply.Fire(this, node);
            }

            // Post Apply
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                // Load the Killer Ocean, if it is there
                if (hazardousOcean != null)
                {
                    Value.gameObject.AddComponent<HazardousOcean>().heatCurve = hazardousOcean;
                }
                
                // Reset the PQS state
                Parser.ClearState("Kopernicus:pqsVersion");

                // Event
                Events.OnOceanLoaderPostApply.Fire(this, node);
            }
        }
    }
}