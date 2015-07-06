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
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Kopernicus.MaterialWrapper;

// Disable the "private fields `` is assigned but its value is never used warning"
#pragma warning disable 0414

namespace Kopernicus
{
	namespace Configuration 
	{
		[RequireConfigType(ConfigType.Node)]
		public class OceanPQS : IParserEventSubscriber
		{
            // PQS we're editing
            public PQS oceanPQS { get; private set; }
            public GameObject oceanRoot = new GameObject();

            // PQS info data
            public bool mapOcean = true;
            public Color mapOceanColor;
            public double mapOceanHeight;

            private PQSMod_UVPlanetRelativePosition uvs;

            // We have an ocean?
            [ParserTarget("ocean", optional = true)]
            private NumericParser<bool> ocean
            {
                set { mapOcean = value.value; }
            }

            // Color of the ocean on the map
            [ParserTarget("oceanColor", optional = true)]
            private ColorParser oceanColor
            {
                set { mapOceanColor = value.value; }
            }

            // Height of the Ocean
            [ParserTarget("oceanHeight", optional = true)]
            private NumericParser<double> oceanHeight
            {
                set { mapOceanHeight = value.value; }
            }

			// PQS level of detail settings
			[ParserTarget("minLevel", optional = true)]
			private NumericParser<int> minLevel 
			{
                set { oceanPQS.minLevel = value.value; }
			}

			[ParserTarget("maxLevel", optional = true)]
			private NumericParser<int> maxLevel 
			{
                set { oceanPQS.maxLevel = value.value; }
			}

			[ParserTarget("minDetailDistance", optional = true)]
			private NumericParser<double> minDetailDistance 
			{
                set { oceanPQS.minDetailDistance = value.value; }
			}

			[ParserTarget("maxQuadLengthsPerFrame", optional = true)]
			private NumericParser<float> maxQuadLengthsPerFrame 
			{
                set { oceanPQS.maxQuadLenghtsPerFrame = value.value; }
			}

			// Surface Material of the PQS
			[ParserTarget("Material", optional = true, allowMerge = true)]
            private Material surfaceMaterial;

			// Fallback Material of the PQS (its always the same material)
			[ParserTarget("FallbackMaterial", optional = true, allowMerge = true)]
            private PQSOceanSurfaceQuadFallbackLoader fallbackMaterial;
				
			// PQS Mods
			[ParserTargetCollection("Mods", optional = true, nameSignificance = NameSignificance.Type, typePrefix = "Kopernicus.Configuration.ModLoader.")]
			private List<ModLoader.ModLoader> mods = new List<ModLoader.ModLoader> ();

            // Killer-Ocean
            [ParserTarget("HazardousOcean", optional = true, allowMerge = true)]
            private HazardousOcean hazardousOcean;

			/**
			 * Constructor for existing Ocean
			 **/
			public OceanPQS (PQS oceanPQS)
			{
                this.oceanPQS = oceanPQS;
                
                oceanPQS.surfaceMaterial = new PQSOceanSurfaceQuadLoader(oceanPQS.surfaceMaterial);
                surfaceMaterial = oceanPQS.surfaceMaterial;
                surfaceMaterial.name = Guid.NewGuid().ToString();

                fallbackMaterial = new PQSOceanSurfaceQuadFallbackLoader(oceanPQS.fallbackMaterial);
                oceanPQS.fallbackMaterial = fallbackMaterial;
                fallbackMaterial.name = Guid.NewGuid().ToString();
            }

			/**
			 * Constructor for new Ocean
			 * 
			 **/
			public OceanPQS ()
			{
				// Generate the PQS object
                oceanRoot.layer = Constants.GameLayers.LocalSpace;
                oceanPQS = oceanRoot.AddComponent<PQS>();

                // Setup materials

                PSystemBody Body = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, "Laythe");
                foreach (PQS ocean in Body.pqsVersion.GetComponentsInChildren<PQS>(true))
                {
                    if (ocean.name == "LaytheOcean")
                    {
                        // Copying Laythes Ocean-properties
                        Utility.CopyObjectFields<PQS>(ocean, oceanPQS);

                        // Load Surface material
                        surfaceMaterial = new PQSOceanSurfaceQuadLoader(ocean.surfaceMaterial);

                        // Load Fallback-Material
                        fallbackMaterial = new PQSOceanSurfaceQuadFallbackLoader(ocean.fallbackMaterial);
                        break;
                    }
                }

                // Load our new Material into the PQS
                oceanPQS.surfaceMaterial = surfaceMaterial;
                surfaceMaterial.name = Guid.NewGuid().ToString();

                // Load fallback material into the PQS            
                oceanPQS.fallbackMaterial = fallbackMaterial;
                fallbackMaterial.name = Guid.NewGuid().ToString();

                // Create the UV planet relative position
                GameObject mod = new GameObject("_Material_SurfaceQuads");
                mod.transform.parent = oceanRoot.transform;
                uvs = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
                uvs.sphere = oceanPQS;
                uvs.requirements = PQS.ModiferRequirements.Default;
                uvs.modEnabled = true;
                uvs.order = 999999;
			}


            List<ModLoader.ModLoader> patchedMods = new List<ModLoader.ModLoader>();
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                if (node.HasNode("Mods"))
                {
                    // Patch the existing mods
                    foreach (ConfigNode mod in node.GetNode("Mods").nodes)
                    {
                        if (oceanPQS.GetComponentsInChildren<PQSMod>(true).Where(m => m.GetType().Name.Contains(mod.name)).Count() != 0)
                        {
                            Type t = Type.GetType("Kopernicus.Configuration.ModLoader." + mod.name);
                            ConstructorInfo cInfo = t.GetConstructor(new Type[] { typeof(PQSMod) });
                            try
                            {
                                PQSMod pqsMod = oceanPQS.GetComponentsInChildren<PQSMod>(true).Where(
                                    m => m.GetType().Name.Contains(mod.name) &&
                                         patchedMods.Where(M => M.mod.name == m.name &&
                                         M.mod.GetType() == m.GetType()).Count() == 0 &&
                                         ((mod.HasValue("name")) ? m.name == mod.GetValue("name") : true)
                                    ).First();

                                ModLoader.ModLoader patchedMod = cInfo.Invoke(new object[] { pqsMod }) as ModLoader.ModLoader;
                                Parser.LoadObjectFromConfigurationNode(patchedMod, mod);
                                patchedMod.patched = true;
                                patchedMods.Add(patchedMod);
                            }
                            catch
                            {
                                Logger.Active.Log("Couldn't find enough Mods of Type " + t + "!");
                            }
                        }
                    }
                }
            }

            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                // Remove the patched mods from the main list
                foreach (ModLoader.ModLoader remove in mods.Where(m => patchedMods.Select(p => p.mod.GetType()).Contains(m.mod.GetType())))
                {
                    remove.mod.transform.parent = null;
                    remove.mod.sphere = null;
                    remove.mod = null;
                }

                // Apply patched mods
                foreach (ModLoader.ModLoader loader in patchedMods)
                {
                    if (loader.mod != null)
                    {
                        loader.mod.transform.parent = oceanPQS.transform;
                        loader.mod.gameObject.layer = Constants.GameLayers.LocalSpace;
                        loader.mod.sphere = oceanPQS;
                        Logger.Active.Log("OceanPQS.PostApply(ConfigNode): Patched OceanPQS Mod => " + loader.mod.GetType());
                    }
                }

                // Apply the new mods
                foreach (ModLoader.ModLoader loader in mods)
                {
                    loader.mod.transform.parent = oceanPQS.transform;
                    loader.mod.sphere = oceanPQS;
                    loader.mod.gameObject.layer = Constants.GameLayers.LocalSpace;
                    Logger.Active.Log("OceanPQS.PostApply(ConfigNode): Added OceanPQS Mod => " + loader.mod.GetType());
                }

                // Apply our Killer-Ocean (if set)
                if (hazardousOcean != null)
                {
                    hazardousOcean.controller.gameObject.transform.parent = oceanPQS.transform;
                    hazardousOcean.OceanPQS = oceanPQS;
                }

                // == DUMP OCEAN MATERIALS == //
                Utility.DumpObjectProperties(oceanPQS.surfaceMaterial, " OCEAN SURFACE MATERIAL ");
                Utility.DumpObjectProperties(oceanPQS.fallbackMaterial, " OCEAN FALLBACK MATERIAL ");
			}
		}
	}
}

#pragma warning restore 0414
