/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
* Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

// Disable the "private fields `` is assigned but its value is never used warning"
#pragma warning disable 0414

namespace Kopernicus
{
    namespace Configuration 
    {
        [RequireConfigType(ConfigType.Node)]
        public class PQSLoader : IParserEventSubscriber
        {
            // PQS Material Type Enum
            private enum PQSMaterialType
            {
                Vacuum,
                AtmosphericBasic,
                AtmosphericMain,
                AtmosphericOptimized
            };

            // PQS we are creating
            public PQS pqsVersion { get; private set; }

            // Required PQSMods
            private PQSMod_CelestialBodyTransform   transform;
            private PQSMod_MaterialSetDirection     lightDirection;
            private PQSMod_UVPlanetRelativePosition uvs;
            private PQSMod_QuadMeshColliders        collider;
            
            // Surface physics material
            [ParserTarget("PhysicsMaterial", optional = true, allowMerge = true)]
            private PhysicsMaterialParser physicsMaterial
            {
                set { collider.physicsMaterial = value.material; }
            }

            // PQS level of detail settings
            [ParserTarget("minLevel", optional = true)]
            private NumericParser<int> minLevel 
            {
                set { pqsVersion.minLevel = value.value; }
            }

            [ParserTarget("maxLevel", optional = true)]
            private NumericParser<int> maxLevel 
            {
                set { pqsVersion.maxLevel = value.value; }
            }

            [ParserTarget("minDetailDistance", optional = true)]
            private NumericParser<double> minDetailDistance 
            {
                set { pqsVersion.minDetailDistance = value.value; }
            }

            [ParserTarget("maxQuadLengthsPerFrame", optional = true)]
            private NumericParser<float> maxQuadLengthsPerFrame 
            {
                set { pqsVersion.maxQuadLenghtsPerFrame = value.value; }
            }

            // CelestialBodyTransform fades. should more or less line up with ScaledVersion's fadeStart/fadeEnd
            [ParserTarget("pqsFadeStart", optional = true)]
            private NumericParser<float> fadeStart
            {
                set { transform.planetFade.fadeStart = value.value; }
            }

            [ParserTarget("pqsFadeEnd", optional = true)]
            private NumericParser<float> fadeEnd
            {
                set { transform.planetFade.fadeEnd = value.value; }
            }

            [ParserTarget("deactivateAltitude", optional = true)]
            private NumericParser<double> deactivateAltitude
            {
                set { transform.deactivateAltitude = value.value; }
            }

            [PreApply]
            [ParserTarget("materialType", optional = true)]
            private EnumParser<PQSMaterialType> materialType
            {
                set 
                {
                    if (value.value == PQSMaterialType.AtmosphericOptimized)
                        pqsVersion.surfaceMaterial = new PQSMainOptimisedLoader ();
                    else if (value.value == PQSMaterialType.AtmosphericMain)
                        pqsVersion.surfaceMaterial = new PQSMainShaderLoader ();
                    else if (value.value == PQSMaterialType.AtmosphericBasic)
                        pqsVersion.surfaceMaterial = new PQSProjectionAerialQuadRelativeLoader ();
                    else if (value.value == PQSMaterialType.Vacuum)
                        pqsVersion.surfaceMaterial = new PQSProjectionSurfaceQuadLoader ();

                    surfaceMaterial = pqsVersion.surfaceMaterial;
                }
            }

            // Surface Material of the PQS
            [ParserTarget("Material", optional = true, allowMerge = true)]
            private Material surfaceMaterial;

            // Fallback Material of the PQS (its always the same material)
            [ParserTarget("FallbackMaterial", optional = true, allowMerge = true)]
            private PQSProjectionFallbackLoader fallbackMaterial;

            /**
             * Constructor for new PQS
             **/
            public PQSLoader ()
            {
                // Create a new PQS
                GameObject controllerRoot = new GameObject ();
                controllerRoot.transform.parent = Utility.Deactivator;
                this.pqsVersion = controllerRoot.AddComponent<PQS> ();

                // I am at this time unable to determine some of the magic parameters which cause the PQS to work...
                PSystemBody Laythe = Utility.FindBody (PSystemManager.Instance.systemPrefab.rootBody, "Laythe");
                Utility.CopyObjectFields(Laythe.pqsVersion, pqsVersion);
                pqsVersion.surfaceMaterial = Laythe.pqsVersion.surfaceMaterial;

                // These parameters magically make the PQS work for some reason.  Need to decipher...
                /*pqsVersion.maxFrameTime = 0.075f;
                pqsVersion.subdivisionThreshold = 1;
                pqsVersion.collapseSeaLevelValue = 2;
                pqsVersion.collapseAltitudeValue = 16;
                pqsVersion.collapseAltitudeMax = 10000000;
                pqsVersion.visRadSeaLevelValue = 5;
                pqsVersion.visRadAltitudeValue = 1.79999995231628;
                pqsVersion.visRadAltitudeMax = 10000;*/

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
                collider.physicsMaterial.frictionDirection2 = Vector3.zero;
                collider.physicsMaterial.dynamicFriction2 = 0.0f;
                collider.physicsMaterial.staticFriction2 = 0.0f;
                collider.physicsMaterial.frictionCombine = PhysicMaterialCombine.Maximum;
                collider.physicsMaterial.bounceCombine = PhysicMaterialCombine.Average;
                collider.requirements = PQS.ModiferRequirements.Default;
                collider.modEnabled = true;
                collider.order = 100;

                // Create physics material editor
                physicsMaterial = new PhysicsMaterialParser (collider.physicsMaterial);
            }

            /**
             * Constructor for pre-existing PQS
             * 
             * @param pqsVersion Existing PQS to augment
             **/
            public PQSLoader (PQS pqsVersion)
            {
                this.pqsVersion = pqsVersion;

                // Get the required PQS information
                transform = pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform> (true).Where (mod => mod.transform.parent == pqsVersion.transform).FirstOrDefault ();
                lightDirection = pqsVersion.GetComponentsInChildren<PQSMod_MaterialSetDirection>(true).Where (mod => mod.transform.parent == pqsVersion.transform).FirstOrDefault ();
                uvs = pqsVersion.GetComponentsInChildren<PQSMod_UVPlanetRelativePosition>(true).Where (mod => mod.transform.parent == pqsVersion.transform).FirstOrDefault ();
                collider = pqsVersion.GetComponentsInChildren<PQSMod_QuadMeshColliders>(true).Where (mod => mod.transform.parent == pqsVersion.transform).FirstOrDefault ();

                // Create physics material editor
                physicsMaterial = new PhysicsMaterialParser (collider.physicsMaterial);

                // Clone the surface material of the PQS
                if (PQSMainOptimisedLoader.UsesSameShader(pqsVersion.surfaceMaterial))
                {
                    pqsVersion.surfaceMaterial = new PQSMainOptimisedLoader(pqsVersion.surfaceMaterial);
                    if (((PQSMainOptimisedLoader)pqsVersion.surfaceMaterial).globalDensity < 2)
                    {
                        ((PQSMainOptimisedLoader)pqsVersion.surfaceMaterial).globalDensity = (float)-8E-06;
                    }
                }
                else if (PQSMainShaderLoader.UsesSameShader(pqsVersion.surfaceMaterial))
                {
                    pqsVersion.surfaceMaterial = new PQSMainShaderLoader(pqsVersion.surfaceMaterial);
                    if (((PQSMainShaderLoader)pqsVersion.surfaceMaterial).globalDensity < 2)
                    {
                        ((PQSMainShaderLoader)pqsVersion.surfaceMaterial).globalDensity = (float)-8E-06;
                    }
                }
                else if (PQSProjectionAerialQuadRelativeLoader.UsesSameShader(pqsVersion.surfaceMaterial))
                {
                    pqsVersion.surfaceMaterial = new PQSProjectionAerialQuadRelativeLoader(pqsVersion.surfaceMaterial);
                    if (((PQSProjectionAerialQuadRelativeLoader)pqsVersion.surfaceMaterial).globalDensity < 2)
                    {
                        ((PQSProjectionAerialQuadRelativeLoader)pqsVersion.surfaceMaterial).globalDensity = (float)-8E-06;
                    }
                }
                else if (PQSProjectionSurfaceQuadLoader.UsesSameShader(pqsVersion.surfaceMaterial))
                {
                    pqsVersion.surfaceMaterial = new PQSProjectionSurfaceQuadLoader(pqsVersion.surfaceMaterial);
                }
                surfaceMaterial = pqsVersion.surfaceMaterial;
                surfaceMaterial.name = Guid.NewGuid ().ToString ();

                // Clone the fallback material of the PQS
                fallbackMaterial = new PQSProjectionFallbackLoader (pqsVersion.fallbackMaterial);
                pqsVersion.fallbackMaterial = fallbackMaterial; 
                fallbackMaterial.name = Guid.NewGuid ().ToString ();
            }

            void IParserEventSubscriber.Apply(ConfigNode node)
            {

            }

            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                if (!node.HasNode ("Mods"))
                    return;

                List<PQSMod> patchedMods = new List<PQSMod>();

                // Load mods manually because of patching
                foreach (ConfigNode mod in node.GetNode ("Mods").nodes) 
                {
                    Type loaderType = Type.GetType ("Kopernicus.Configuration.ModLoader." + mod.name);
                    if (loaderType == null)
                    {
                        foreach (AssemblyLoader.LoadedAssembly assembly in AssemblyLoader.loadedAssemblies)
                        {
                            foreach (Type type in assembly.assembly.GetExportedTypes())
                            {
                                if (type.ToString() == "Kopernicus.Configuration.ModLoader." + mod.name)
                                {
                                    loaderType = type;
                                }
                            }
                        }
                    }
                    Type modType = Type.GetType ((mod.name != "LandControl" ? "PQSMod_" + mod.name : "PQSLandControl") + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

                    // Do any PQS Mods already exist on this PQS matching this mod?
                    IEnumerable<PQSMod> existingMods = pqsVersion.GetComponentsInChildren<PQSMod>(true).Where(m => m.GetType().Equals(modType) && 
                                                                                                                    m.transform.parent == pqsVersion.transform);
                    ModLoader.ModLoader loader = null;
                    if (existingMods.Count () > 0) 
                    {
                        // Attempt to find a PQS mod we can edit that we have not edited before
                        PQSMod existingMod = existingMods.Where (m => !patchedMods.Contains(m) && (mod.HasValue ("name") ? m.name == mod.GetValue ("name") : true))
                                                         .FirstOrDefault ();
                        if (existingMod != null) 
                        {
                            loader = Parser.CreateObjectFromConfigNode (loaderType, mod, new object[] { existingMod }) as ModLoader.ModLoader;
                            patchedMods.Add (existingMod);

                            Logger.Active.Log("PQSLoader.PostApply(ConfigNode): Patched PQS Mod => " + modType);
                        }
                    }

                    if (loader == null) 
                    {
                        loader = Parser.CreateObjectFromConfigNode (loaderType, mod) as ModLoader.ModLoader;
                        Logger.Active.Log ("PQSLoader.PostApply(ConfigNode): Added PQS Mod => " + modType);
                    }

                    if (loader.mod != null)
                    {
                        loader.mod.transform.parent = pqsVersion.transform;
                        loader.mod.gameObject.layer = Constants.GameLayers.LocalSpace;
                        loader.mod.sphere = pqsVersion;
                        if (loader.GetType() == typeof(ModLoader.LandControl))
                            (loader as ModLoader.LandControl).SphereApply();
                    }
                }
            }
        }
    }
}

#pragma warning restore 0414
