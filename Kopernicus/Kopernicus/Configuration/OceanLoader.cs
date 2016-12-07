/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kopernicus.Components;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration 
    {
        [RequireConfigType(ConfigType.Node)]
        public class OceanLoader : BaseLoader, IParserEventSubscriber
        {
            // PQS we're editing
            public PQS ocean { get; set; }
            public PQS pqsVersion { get; set; }
            public CelestialBody body { get; set; }
            public GameObject gameObject { get; set; }
            public PQSMod_UVPlanetRelativePosition uvs;

            // We have an ocean?
            [ParserTarget("ocean")]
            public NumericParser<bool> mapOcean
            {
                get { return pqsVersion.mapOcean && body.ocean; }
                set { pqsVersion.mapOcean = body.ocean = value; }
            }

            // Color of the ocean on the map
            [ParserTarget("oceanColor")]
            public ColorParser oceanColor
            {
                get { return pqsVersion.mapOceanColor; }
                set { pqsVersion.mapOceanColor = value; }
            }

            // Height of the Ocean
            [ParserTarget("oceanHeight")]
            public NumericParser<double> oceanHeight
            {
                get { return pqsVersion.mapOceanHeight; }
                set { pqsVersion.mapOceanHeight = value; }
            }

            // Density of the Ocean
            [ParserTarget("density")]
            public NumericParser<double> density
            {
                get { return body.oceanDensity; }
                set { body.oceanDensity = value; }
            }

            // PQS level of detail settings
            [ParserTarget("minLevel")]
            public NumericParser<int> minLevel
            {
                get { return ocean.minLevel; }
                set { ocean.minLevel = value; }
            }

            [ParserTarget("maxLevel")]
            public NumericParser<int> maxLevel
            {
                get { return ocean.maxLevel; }
                set { ocean.maxLevel = value; }
            }

            [ParserTarget("minDetailDistance")]
            public NumericParser<double> minDetailDistance
            {
                get { return ocean.minDetailDistance; }
                set { ocean.minDetailDistance = value; }
            }

            [ParserTarget("maxQuadLengthsPerFrame")]
            public NumericParser<float> maxQuadLengthsPerFrame
            {
                get { return ocean.maxQuadLenghtsPerFrame; }
                set { ocean.maxQuadLenghtsPerFrame = value; }
            }

            // Surface Material of the PQS
            [ParserTarget("Material", allowMerge = true, getChild = false)]
            public Material surfaceMaterial;

            // Fallback Material of the PQS (its always the same material)
            [ParserTarget("FallbackMaterial", allowMerge = true)]
            public PQSOceanSurfaceQuadFallbackLoader fallbackMaterial;

            // Killer-Ocean
            [ParserTarget("HazardousOcean", allowMerge = true)]
            public FloatCurveParser hazardousOcean;

            // Ocean-Fog
            [ParserTarget("Fog", allowMerge = true)]
            public FogLoader fog;

            // Runtime Constructor
            public OceanLoader(PQS ocean)
            {
                this.ocean = ocean;
                this.body = Part.GetComponentUpwards<CelestialBody>(ocean.gameObject);
                pqsVersion = body.pqsController;

                ocean.surfaceMaterial = new PQSOceanSurfaceQuadLoader(ocean.surfaceMaterial);
                surfaceMaterial = ocean.surfaceMaterial;
                surfaceMaterial.name = Guid.NewGuid().ToString();

                fallbackMaterial = new PQSOceanSurfaceQuadFallbackLoader(ocean.fallbackMaterial);
                ocean.fallbackMaterial = fallbackMaterial;
                fallbackMaterial.name = Guid.NewGuid().ToString();
            }

            // Default constructor
            public OceanLoader()
            {
                // Load existing Oceans
                if (generatedBody.pqsVersion.GetComponentsInChildren<PQS>(true).Any(p => p.name.EndsWith("Ocean")))
                {
                    ocean = generatedBody.pqsVersion.GetComponentsInChildren<PQS>(true).First(p => p.name.EndsWith("Ocean"));
                    pqsVersion = generatedBody.pqsVersion;
                    gameObject = ocean.gameObject;
                    body = generatedBody.celestialBody;

                    ocean.surfaceMaterial = new PQSOceanSurfaceQuadLoader(ocean.surfaceMaterial);
                    surfaceMaterial = ocean.surfaceMaterial;
                    surfaceMaterial.name = Guid.NewGuid().ToString();

                    fallbackMaterial = new PQSOceanSurfaceQuadFallbackLoader(ocean.fallbackMaterial);
                    ocean.fallbackMaterial = fallbackMaterial;
                    fallbackMaterial.name = Guid.NewGuid().ToString();
                    return;
                }

                // Generate the PQS object
                gameObject = new GameObject("Ocean");
                gameObject.layer = Constants.GameLayers.LocalSpace;
                ocean = gameObject.AddComponent<PQS>();
                pqsVersion = generatedBody.pqsVersion;
                body = generatedBody.celestialBody;

                // Setup materials
                PSystemBody Body = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, "Laythe");
                foreach (PQS oc in Body.pqsVersion.GetComponentsInChildren<PQS>(true))
                {
                    if (oc.name == "LaytheOcean")
                    {
                        // Copying Laythes Ocean-properties
                        Utility.CopyObjectFields<PQS>(oc, this.ocean);

                        // Load Surface material
                        surfaceMaterial = new PQSOceanSurfaceQuadLoader(oc.surfaceMaterial);

                        // Load Fallback-Material
                        fallbackMaterial = new PQSOceanSurfaceQuadFallbackLoader(oc.fallbackMaterial);
                        break;
                    }
                }

                // Load our new Material into the PQS
                ocean.surfaceMaterial = surfaceMaterial;
                surfaceMaterial.name = Guid.NewGuid().ToString();

                // Load fallback material into the PQS            
                ocean.fallbackMaterial = fallbackMaterial;
                fallbackMaterial.name = Guid.NewGuid().ToString();

                // Create the UV planet relative position
                GameObject mod = new GameObject("_Material_SurfaceQuads");
                mod.transform.parent = gameObject.transform;
                uvs = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
                uvs.sphere = ocean;
                uvs.requirements = PQS.ModiferRequirements.Default;
                uvs.modEnabled = true;
                uvs.order = 999999;
            }

            // Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                // Make assumptions
                mapOcean = true;
            }

            // Post Apply
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                // Load the Killer Ocean, if it is there
                if (hazardousOcean != null)
                {
                    ocean.gameObject.AddComponent<HazardousOcean>().heatCurve = hazardousOcean;
                }

                // Apply the Ocean
                ocean.transform.parent = generatedBody.pqsVersion.transform;

                // Add the ocean PQS to the secondary renders of the CelestialBody Transform
                PQSMod_CelestialBodyTransform transform = generatedBody.pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true).FirstOrDefault(mod => mod.transform.parent == generatedBody.pqsVersion.transform);
                transform.planetFade.secondaryRenderers.Add(ocean.gameObject);

                // Names!
                ocean.name = generatedBody.pqsVersion.name + "Ocean";
                ocean.gameObject.name = generatedBody.pqsVersion.name + "Ocean";
                ocean.transform.name = generatedBody.pqsVersion.name + "Ocean";

                // Set up the ocean PQS
                pqsVersion = generatedBody.pqsVersion;

                // Load mods
                if (!node.HasNode("Mods"))
                    return;
                List<PQSMod> patchedMods = new List<PQSMod>();

                // Get all loaded types
                List<Type> types = Injector.ModTypes;

                // Load mods manually because of patching
                foreach (ConfigNode mod in node.GetNode("Mods").nodes)
                {
                    // get the mod type
                    if (types.All(t => t.Name != mod.name))
                        continue;
                    Type loaderType = types.FirstOrDefault(t => t.Name == mod.name);
                    string testName = mod.name != "LandControl" ? "PQSMod_" + mod.name : "PQSLandControl";
                    Type modType = types.FirstOrDefault(t => t.Name == testName);
                    if (loaderType == null || modType == null)
                    {
                        Debug.LogError("MOD NULL: Loadertype " + mod.name + " with mod type " + testName + " and null? " + (loaderType == null) + (modType == null));
                        continue;
                    }

                    // Do any PQS Mods already exist on this PQS matching this mod?
                    IEnumerable<PQSMod> existingMods = ocean.GetComponentsInChildren<PQSMod>(true).Where(m => m.GetType() == modType &&
                                                                                                                    m.transform.parent == ocean.transform);

                    // Create the loader
                    object loader = Activator.CreateInstance(loaderType);

                    // Reflection, because C# being silly... :/
                    MethodInfo createNew = loaderType.GetMethod("Create", new Type[] { typeof(PQS) });
                    MethodInfo create = loaderType.GetMethod("Create", new Type[] { modType, typeof(PQS) });

                    if (existingMods.Any())
                    {
                        // Attempt to find a PQS mod we can edit that we have not edited before
                        PQSMod existingMod = existingMods.FirstOrDefault(m => !patchedMods.Contains(m) && (mod.HasValue("name") ? m.name == mod.GetValue("name") : true));
                        if (existingMod != null)
                        {
                            create.Invoke(loader, new object[] { existingMod, ocean });
                            Parser.LoadObjectFromConfigurationNode(loader, mod, "Kopernicus");
                            patchedMods.Add(existingMod);
                            Logger.Active.Log("OceanLoader.PostApply(ConfigNode): Patched PQS Mod => " + modType);
                        }
                        else
                        {
                            createNew.Invoke(loader, new object[] { ocean });
                            Parser.LoadObjectFromConfigurationNode(loader, mod, "Kopernicus");
                            Logger.Active.Log("OceanLoader.PostApply(ConfigNode): Added PQS Mod => " + modType);
                        }
                    }
                    else
                    {
                        createNew.Invoke(loader, new object[] { ocean });
                        Parser.LoadObjectFromConfigurationNode(loader, mod, "Kopernicus");
                        Logger.Active.Log("OceanLoader.PostApply(ConfigNode): Added PQS Mod => " + modType);
                    }
                }
            }
        }
    }
}