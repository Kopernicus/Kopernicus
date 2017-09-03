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

using Kopernicus.MaterialWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

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
            public NumericParser<int> minLevel 
            {
                get { return pqsVersion.minLevel; }
                set { pqsVersion.minLevel = value; }
            }

            [ParserTarget("maxLevel")]
            public NumericParser<int> maxLevel 
            {
                get { return pqsVersion.maxLevel; }
                set { pqsVersion.maxLevel = value; }
            }

            [ParserTarget("minDetailDistance")]
            public NumericParser<double> minDetailDistance 
            {
                get { return pqsVersion.minDetailDistance; }
                set { pqsVersion.minDetailDistance = value; }
            }

            [ParserTarget("maxQuadLengthsPerFrame")]
            public NumericParser<float> maxQuadLengthsPerFrame 
            {
                get { return pqsVersion.maxQuadLenghtsPerFrame; }
                set { pqsVersion.maxQuadLenghtsPerFrame = value; }
            }

            // CelestialBodyTransform fades. should more or less line up with ScaledVersion's fadeStart/fadeEnd
            [ParserTarget("fadeStart")]
            public NumericParser<float> fadeStart
            {
                get { return transform.planetFade.fadeStart; }
                set { transform.planetFade.fadeStart = value; }
            }

            [ParserTarget("fadeEnd")]
            public NumericParser<float> fadeEnd
            {
                get { return transform.planetFade.fadeEnd; }
                set { transform.planetFade.fadeEnd = value; }
            }

            [ParserTarget("deactivateAltitude")]
            public NumericParser<double> deactivateAltitude
            {
                get { return transform.deactivateAltitude; }
                set { transform.deactivateAltitude = value; }
            }

            // Map Export Arguments
            [ParserTarget("mapMaxHeight")]
            public NumericParser<double> mapMaxHeight
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

            /**
             * Constructor for new PQS
             **/
            public PQSLoader ()
            {
                if (generatedBody.pqsVersion != null)
                {
                    // Save the PQSVersion
                    pqsVersion = generatedBody.pqsVersion;

                    // Get the required PQS information
                    transform = pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true).FirstOrDefault(Mod => Mod.transform.parent == pqsVersion.transform);
                    lightDirection = pqsVersion.GetComponentsInChildren<PQSMod_MaterialSetDirection>(true).FirstOrDefault(Mod => Mod.transform.parent == pqsVersion.transform);
                    uvs = pqsVersion.GetComponentsInChildren<PQSMod_UVPlanetRelativePosition>(true).FirstOrDefault(Mod => Mod.transform.parent == pqsVersion.transform);
                    collider = pqsVersion.GetComponentsInChildren<PQSMod_QuadMeshColliders>(true).FirstOrDefault(Mod => Mod.transform.parent == pqsVersion.transform);

                    // Clone the surface material of the PQS
                    if (PQSMainOptimised.UsesSameShader(surfaceMaterial))
                    {
                        PQSMainOptimisedLoader loader = new PQSMainOptimisedLoader(surfaceMaterial);
                        loader.globalDensity = loader.globalDensity < 2 ? (float)-8E-06 : loader.globalDensity;
                        surfaceMaterial = loader;
                    }
                    else if (PQSMainShader.UsesSameShader(surfaceMaterial))
                    {
                        PQSMainShaderLoader loader = new PQSMainShaderLoader(surfaceMaterial);
                        loader.globalDensity = loader.globalDensity < 2 ? (float)-8E-06 : loader.globalDensity;
                        surfaceMaterial = loader;
                    }
                    else if (PQSProjectionAerialQuadRelative.UsesSameShader(surfaceMaterial))
                    {
                        PQSProjectionAerialQuadRelativeLoader loader = new PQSProjectionAerialQuadRelativeLoader(surfaceMaterial);
                        loader.globalDensity = loader.globalDensity < 2 ? (float)-8E-06 : loader.globalDensity;
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
                    return;
                }

                // Create a new PQS
                GameObject controllerRoot = new GameObject ();
                controllerRoot.transform.parent = Utility.Deactivator;
                pqsVersion = controllerRoot.AddComponent<PQS> ();

                // I (Teknoman) am at this time unable to determine some of the magic parameters which cause the PQS to work...
                // And I (Thomas) am at this time just too lazy to do it differently...
                PSystemBody Laythe = Utility.FindBody (PSystemManager.Instance.systemPrefab.rootBody, "Laythe");
                Utility.CopyObjectFields(Laythe.pqsVersion, pqsVersion);
                pqsVersion.surfaceMaterial = Laythe.pqsVersion.surfaceMaterial;

                // Create the fallback material (always the same shader)
                fallbackMaterial = new PQSProjectionFallbackLoader ();
                pqsVersion.fallbackMaterial = fallbackMaterial; 
                fallbackMaterial.name = Guid.NewGuid ().ToString ();

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
                collider.physicsMaterial = new PhysicMaterial();
                collider.physicsMaterial.name = "Ground";
                collider.physicsMaterial.dynamicFriction = 0.6f;
                collider.physicsMaterial.staticFriction = 0.8f;
                collider.physicsMaterial.bounciness = 0.0f;
                collider.physicsMaterial.frictionCombine = PhysicMaterialCombine.Maximum;
                collider.physicsMaterial.bounceCombine = PhysicMaterialCombine.Average;
                collider.requirements = PQS.ModiferRequirements.Default;
                collider.modEnabled = true;
                collider.order = 100;

                // Create physics material editor
                physicsMaterial = new PhysicsMaterialParser (collider.physicsMaterial);

                // Assing the new PQS
                generatedBody.pqsVersion = pqsVersion;
            }

            // Constructor for pre-existing PQS
            public PQSLoader (PQS pqsVersion)
            {
                this.pqsVersion = pqsVersion;

                // Get the required PQS information
                transform = pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform> (true).FirstOrDefault (mod => mod.transform.parent == pqsVersion.transform);
                lightDirection = pqsVersion.GetComponentsInChildren<PQSMod_MaterialSetDirection> (true).FirstOrDefault (mod => mod.transform.parent == pqsVersion.transform);
                uvs = pqsVersion.GetComponentsInChildren<PQSMod_UVPlanetRelativePosition> (true).FirstOrDefault (mod => mod.transform.parent == pqsVersion.transform);
                collider = pqsVersion.GetComponentsInChildren<PQSMod_QuadMeshColliders> (true).FirstOrDefault (mod => mod.transform.parent == pqsVersion.transform);

                // Clone the surface material of the PQS
                if (PQSMainOptimised.UsesSameShader(surfaceMaterial))
                {
                    PQSMainOptimisedLoader loader = new PQSMainOptimisedLoader(surfaceMaterial);
                    loader.globalDensity = loader.globalDensity < 2 ? (float)-8E-06 : loader.globalDensity;
                    surfaceMaterial = loader;
                }
                else if (PQSMainShader.UsesSameShader(surfaceMaterial))
                {
                    PQSMainShaderLoader loader = new PQSMainShaderLoader(surfaceMaterial);
                    loader.globalDensity = loader.globalDensity < 2 ? (float)-8E-06 : loader.globalDensity;
                    surfaceMaterial = loader;
                }
                else if (PQSProjectionAerialQuadRelative.UsesSameShader(surfaceMaterial))
                {
                    PQSProjectionAerialQuadRelativeLoader loader = new PQSProjectionAerialQuadRelativeLoader(surfaceMaterial);
                    loader.globalDensity = loader.globalDensity < 2 ? (float)-8E-06 : loader.globalDensity;
                    surfaceMaterial = loader;
                }
                else if (PQSProjectionSurfaceQuad.UsesSameShader(surfaceMaterial))
                {
                    surfaceMaterial = new PQSProjectionSurfaceQuadLoader(surfaceMaterial);
                }
                surfaceMaterial.name = Guid.NewGuid ().ToString ();

                // Clone the fallback material of the PQS
                fallbackMaterial = new PQSProjectionFallbackLoader (fallbackMaterial);
                fallbackMaterial.name = Guid.NewGuid ().ToString ();
            }

            // Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                Events.OnPQSLoaderApply.Fire(this, node);
            }

            // PostApply Event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                // Assign the generated PQS to our new world
                generatedBody.pqsVersion = pqsVersion;
                generatedBody.pqsVersion.name = generatedBody.name;
                generatedBody.pqsVersion.transform.name = generatedBody.name;
                generatedBody.pqsVersion.gameObject.name = generatedBody.name;
                generatedBody.pqsVersion.radius = generatedBody.celestialBody.Radius;

                // Add an OnDemand Handler
                OnDemand.OnDemandStorage.AddHandler(generatedBody.pqsVersion);

                // Load mods
                if (node.HasNode("Mods"))
                {
                    List<PQSMod> patchedMods = new List<PQSMod>();

                    // Get all loaded types
                    List<Type> types = Parser.ModTypes;

                    // Load mods manually because of patching
                    foreach (ConfigNode mod in node.GetNode("Mods").nodes)
                    {
                        // get the mod type
                        if (types.Count(t => t.Name == mod.name) == 0)
                            continue;
                        Type loaderType = types.FirstOrDefault(t => t.Name == mod.name);
                        Type modType = loaderType.BaseType.GetGenericArguments()[0];
                        if (loaderType == null || modType == null)
                        {
                            Debug.LogError("MOD NULL: Loadertype " + mod.name + " with mod type " + modType.Name + " and null? " + (loaderType == null) + (modType == null));
                            continue;
                        }
                        // Do any PQS Mods already exist on this PQS matching this mod?
                        IEnumerable<PQSMod> existingMods = pqsVersion.GetComponentsInChildren<PQSMod>(true).Where(m => m.GetType() == modType &&
                                                                                                                       m.transform.parent == pqsVersion.transform);

                        // Create the loader
                        object loader = Activator.CreateInstance(loaderType);

                        // Reflection, because C# being silly... :/
                        MethodInfo createNew = loaderType.GetMethod("Create", Type.EmptyTypes);
                        MethodInfo create = loaderType.GetMethod("Create", new Type[] { modType });

                        if (existingMods.Any())
                        {
                            // Attempt to find a PQS mod we can edit that we have not edited before
                            PQSMod existingMod = existingMods.FirstOrDefault(m => !patchedMods.Contains(m) && (!mod.HasValue("name") || (mod.HasValue("index") ? existingMods.ToList().IndexOf(m) == Int32.Parse(mod.GetValue("index")) && m.name == mod.GetValue("name") : m.name == mod.GetValue("name"))));
                            if (existingMod != null)
                            {
                                create.Invoke(loader, new[] { existingMod });
                                Parser.LoadObjectFromConfigurationNode(loader, mod, "Kopernicus");
                                patchedMods.Add(existingMod);
                                Logger.Active.Log("PQSLoader.PostApply(ConfigNode): Patched PQS Mod => " + modType);
                            }
                            else
                            {
                                createNew.Invoke(loader, null);
                                Parser.LoadObjectFromConfigurationNode(loader, mod, "Kopernicus");
                                Logger.Active.Log("PQSLoader.PostApply(ConfigNode): Added PQS Mod => " + modType);
                            }
                        }
                        else
                        {
                            createNew.Invoke(loader, null);
                            Parser.LoadObjectFromConfigurationNode(loader, mod, "Kopernicus");
                            Logger.Active.Log("PQSLoader.PostApply(ConfigNode): Added PQS Mod => " + modType);
                        }
                    }
                }

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
