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

using System;
using System.Collections.Generic;
using System.Linq;
using Kopernicus.Components.PatchedMods;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class TemplateLoader : IParserEventSubscriber
        {
            // Cloned PSystemBody to expose to the config system
            public PSystemBody body;

            // Initial radius of the body
            public Double radius { get; set; }

            // Initial type of the body
            public BodyType type { get; set; }

            // PSystemBody to use as a template in lookup & clone
            public PSystemBody originalBody;

            // Name of the body to use for the template
            [PreApply]
            [ParserTarget("name", Optional = false)]
            public String name
            {
                // Crawl the system prefab for the body
                set
                {
                    originalBody = Utility.FindBody(Injector.StockSystemPrefab.rootBody, value);
                    if (originalBody == null)
                    {
                        throw new TemplateNotFoundException("Unable to find: " + value);
                    }
                }
            }

            // Should we strip the PQS off
            [PreApply]
            [ParserTarget("removePQS")]
            public NumericParser<Boolean> removePQS = false;

            // Should we strip the atmosphere off
            [ParserTarget("removeAtmosphere")]
            public NumericParser<Boolean> removeAtmosphere = false;

            // Should we remove the biomes
            [ParserTarget("removeBiomes")]
            public NumericParser<Boolean> removeBiomes
            {
                get { return body.Get("removeBiomes", true); }
                set { body.Set("removeBiomes", value.Value); }
            }

            // Should we strip the ocean off
            [ParserTarget("removeOcean")]
            public NumericParser<Boolean> removeOcean = false;

            // Collection of PQS mods to remove
            [ParserTarget("removePQSMods")]
            public StringCollectionParser removePQSMods;

            // Should we strip all Mods off
            [ParserTarget("removeAllPQSMods")]
            public NumericParser<Boolean> removeAllMods = false;

            // Collection of PQS mods to remove
            [ParserTarget("removeProgressTree")]
            public NumericParser<Boolean> removeProgressTree = true;

            // Remove coronas for star
            [ParserTarget("removeCoronas")]
            public NumericParser<Boolean> removeCoronas = false;

            // Apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                // Waaaah
                SpaceCenter.Instance = null;

                // Instantiate (clone) the template body
                GameObject bodyGameObject = UnityEngine.Object.Instantiate(originalBody.gameObject) as GameObject;
                bodyGameObject.name = originalBody.name;
                bodyGameObject.transform.parent = Utility.Deactivator;
                body = bodyGameObject.GetComponent<PSystemBody>();
                body.children = new List<PSystemBody>();

                // Clone the scaled version
                body.scaledVersion = UnityEngine.Object.Instantiate(originalBody.scaledVersion) as GameObject;
                body.scaledVersion.transform.parent = Utility.Deactivator;
                body.scaledVersion.name = originalBody.scaledVersion.name;

                // Clone the PQS version (if it has one) and we want the PQS
                if (body.pqsVersion != null && removePQS.Value != true)
                {
                    body.pqsVersion = UnityEngine.Object.Instantiate(originalBody.pqsVersion) as PQS;
                    body.pqsVersion.transform.parent = Utility.Deactivator;
                    body.pqsVersion.name = originalBody.pqsVersion.name;
                }
                else
                {
                    // Make sure we have no ties to the PQS, as we wanted to remove it or it didn't exist
                    body.pqsVersion = null;
                    body.celestialBody.ocean = false;
                }

                // Store the initial radius (so scaled version can be computed)
                radius = body.celestialBody.Radius;

                // Event
                Events.OnTemplateLoaderApply.Fire(this, node);
            }

            // Post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                // Should we remove the atmosphere
                if (body.celestialBody.atmosphere && removeAtmosphere.Value)
                {
                    // Find atmosphere from ground and destroy the game object
                    AtmosphereFromGround atmosphere = body.scaledVersion.GetComponentsInChildren<AtmosphereFromGround>(true)[0];
                    atmosphere.transform.parent = null;
                    UnityEngine.Object.Destroy(atmosphere.gameObject);

                    // Destroy the light controller
                    MaterialSetDirection light = body.scaledVersion.GetComponentsInChildren<MaterialSetDirection>(true)[0];
                    UnityEngine.Object.Destroy(light);

                    // No more atmosphere :(
                    body.celestialBody.atmosphere = false;
                }
                
                Logger.Active.Log("[Kopernicus]: Configuration.Template: Using Template \"" + body.celestialBody.bodyName + "\"");

                // If we have a PQS
                if (body.pqsVersion != null)
                {
                    // Should we remove the ocean?
                    if (body.celestialBody.ocean && removeOcean.Value)
                    {
                        // Find atmosphere the ocean PQS
                        PQS ocean = body.pqsVersion.GetComponentsInChildren<PQS>(true).Where(pqs => pqs != body.pqsVersion).First();
                        PQSMod_CelestialBodyTransform cbt = body.pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true).First();

                        // Destroy the ocean PQS (this could be bad - destroying the secondary fades...)
                        cbt.planetFade.secondaryRenderers.Remove(ocean.gameObject);
                        cbt.secondaryFades = null;
                        ocean.transform.parent = null;
                        UnityEngine.Object.Destroy(ocean);

                        // No more ocean :(
                        body.celestialBody.ocean = false;
                        body.pqsVersion.mapOcean = false;
                    }

                    // Selectively remove PQS Mods
                    if (removePQSMods != null && removePQSMods.Value.LongCount() > 0)
                    {
                        // We need a List with Types to remove
                        List<Type> mods = new List<Type>();
                        Dictionary<String, Type> modsPerName = new Dictionary<String, Type>();
                        foreach (String mod in removePQSMods.Value)
                        {
                            // If the definition has a name specified, grab that
                            String mType = mod;
                            String name = "";
                            if (mType.EndsWith("]"))
                            {
                                String[] split = mType.Split('[');
                                mType = split[0];
                                name = split[1].Remove(split[1].Length - 1);
                            }

                            // Get the mods matching the String
                            String modName = mType;
                            if (!mod.Contains("PQS"))
                                modName = "PQSMod_" + mod;
                            if (name == "")
                            {
                                //mods.Add(Type.GetType(modName + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"));
                                Type t = Parser.ModTypes.Find(m => m.Name == modName);
                                if (t != null)
                                    mods.Add(t);
                            }
                            else
                            {
                                //modsPerName.Add(name, Type.GetType(modName + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"));
                                Type t = Parser.ModTypes.Find(m => m.Name == modName);
                                if (t != null)
                                    modsPerName.Add(name, t);
                            }
                        }
                        Utility.RemoveModsOfType(mods, body.pqsVersion);
                        foreach (KeyValuePair<String, Type> kvP in modsPerName)
                        {
                            Int32 index = 0;
                            String name = kvP.Key;
                            if (name.Contains(';'))
                            {
                                String[] split = name.Split(';');
                                name = split[0];
                                Int32.TryParse(split[1], out index);
                            }
                            PQSMod[] allMods = body.pqsVersion.GetComponentsInChildren(kvP.Value, true).Select(m => m as PQSMod).Where(m => m.name == name).ToArray();
                            if (allMods.Length > 0)
                            {
                                if (allMods[index] is PQSCity)
                                {
                                    PQSCity city = allMods[index] as PQSCity;
                                    if (city.lod != null)
                                    {
                                        foreach (PQSCity.LODRange range in city.lod)
                                        {
                                            if (range.objects != null)
                                            {
                                                foreach (GameObject o in range.objects)
                                                    UnityEngine.Object.DestroyImmediate(o);
                                            }
                                            if (range.renderers != null)
                                            {
                                                foreach (GameObject o in range.renderers)
                                                    UnityEngine.Object.DestroyImmediate(o);
                                            }
                                        }
                                    }
                                }
                                if (allMods[index] is PQSCity2)
                                {
                                    PQSCity2 city = allMods[index] as PQSCity2;
                                    if (city.objects != null)
                                    {
                                        foreach (PQSCity2.LodObject range in city.objects)
                                        {
                                            if (range.objects != null)
                                            {
                                                foreach (GameObject o in range.objects)
                                                    UnityEngine.Object.DestroyImmediate(o);
                                            }
                                        }
                                    }
                                }
                                
                                // If no mod is left, delete the game object too
                                GameObject gameObject = allMods[index].gameObject;
                                UnityEngine.Object.DestroyImmediate(allMods[index]);
                                PQSMod[] allRemainingMods = gameObject.GetComponentsInChildren<PQSMod>(true);
                                if (allRemainingMods.Length == 0)
                                {
                                    UnityEngine.Object.DestroyImmediate(gameObject);
                                }
                            }
                        }
                    }

                    if (removeAllMods != null && removeAllMods.Value)
                    {
                        // Remove all mods
                        Utility.RemoveModsOfType(null, body.pqsVersion);
                    }

                    foreach (PQSLandControl landControl in body.pqsVersion.GetComponentsInChildren<PQSLandControl>())
                    {
                        Utility.CopyObjectFields(landControl, landControl.gameObject.AddComponent<PQSLandControlPatched>());
                        UnityEngine.Object.Destroy(landControl);
                    }
                }

                // Should we remove the progress tree
                if (removeProgressTree.Value)
                {
                    body.celestialBody.progressTree = null;
                }

                // Figure out what kind of body we are
                if (body.scaledVersion.GetComponentsInChildren<SunShaderController>(true).Length > 0)
                    type = BodyType.Star;
                else if (body.celestialBody.atmosphere)
                    type = BodyType.Atmospheric;
                else
                    type = BodyType.Vacuum;

                // remove coronas
                if (type == BodyType.Star && removeCoronas)
                {
                    foreach (SunCoronas corona in body.scaledVersion.GetComponentsInChildren<SunCoronas>(true))
                        corona.GetComponent<Renderer>().enabled = false; // RnD hard refs Coronas, so we need to disable them
                }

                // Event
                Events.OnTemplateLoaderPostApply.Fire(this, node);
            }

            // Private exception to throw in the case the template doesn't load
            public class TemplateNotFoundException : Exception
            {
                public TemplateNotFoundException(String s) : base(s)
                {

                }
            }
        }
    }
}
