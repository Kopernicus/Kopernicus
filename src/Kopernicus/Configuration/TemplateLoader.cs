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
using Kopernicus.Configuration.Enumerations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kopernicus.Configuration
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "UnassignedField.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TemplateLoader : IParserEventSubscriber
    {
        // Cloned PSystemBody to expose to the config system
        public PSystemBody Body;

        // Initial radius of the body
        public Double Radius { get; set; }

        // PSystemBody to use as a template in lookup & clone
        public PSystemBody OriginalBody;

        // Name of the body to use for the template
        [PreApply]
        [ParserTarget("name", Optional = false)]
        public String Name
        {
            // Crawl the system prefab for the body
            set
            {
                OriginalBody = Utility.FindBody(Injector.StockSystemPrefab.rootBody, value);
                if (OriginalBody == null)
                {
                    throw new TemplateNotFoundException("Unable to find: " + value);
                }
            }
        }

        // Should we strip the PQS off
        [PreApply]
        [ParserTarget("removePQS")]
        public NumericParser<Boolean> RemovePqs = false;

        // Should we strip the atmosphere off
        [ParserTarget("removeAtmosphere")]
        public NumericParser<Boolean> RemoveAtmosphere = false;

        // Should we remove the biomes
        [ParserTarget("removeBiomes")]
        public NumericParser<Boolean> RemoveBiomes
        {
            get { return Body.Get("removeBiomes", true); }
            set { Body.Set("removeBiomes", value.Value); }
        }

        // Should we strip the ocean off
        [ParserTarget("removeOcean")]
        public NumericParser<Boolean> RemoveOcean = false;

        // Collection of PQS mods to remove
        [ParserTarget("removePQSMods")]
        public StringCollectionParser RemovePqsMods;

        // Should we strip all Mods off
        [ParserTarget("removeAllPQSMods")]
        public NumericParser<Boolean> RemoveAllMods = false;

        // Collection of PQS mods to remove
        [ParserTarget("removeProgressTree")]
        public NumericParser<Boolean> RemoveProgressTree = true;

        // Remove coronas for star
        [ParserTarget("removeCoronas")]
        public NumericParser<Boolean> RemoveCoronas = false;

        // Apply event
        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            // Waaaah
            SpaceCenter.Instance = null;

            // Instantiate (clone) the template body
            GameObject bodyGameObject = UnityEngine.Object.Instantiate(OriginalBody.gameObject, Utility.Deactivator, true);
            bodyGameObject.name = OriginalBody.name;
            Body = bodyGameObject.GetComponent<PSystemBody>();
            Body.children = new List<PSystemBody>();

            // Clone the scaled version
            Body.scaledVersion = UnityEngine.Object.Instantiate(OriginalBody.scaledVersion, Utility.Deactivator, true);
            Body.scaledVersion.name = OriginalBody.scaledVersion.name;

            // Clone the PQS version (if it has one) and we want the PQS
            if (Body.pqsVersion != null && RemovePqs.Value != true)
            {
                Body.pqsVersion = UnityEngine.Object.Instantiate(OriginalBody.pqsVersion, Utility.Deactivator, true);
                Body.pqsVersion.name = OriginalBody.pqsVersion.name;
            }
            else
            {
                // Make sure we have no ties to the PQS, as we wanted to remove it or it didn't exist
                Body.pqsVersion = null;
                Body.celestialBody.ocean = false;
            }

            // Store the initial radius (so scaled version can be computed)
            Radius = Body.celestialBody.Radius;

            // Event
            Events.OnTemplateLoaderApply.Fire(this, node);
        }

        // Post apply event
        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
            // Should we remove the atmosphere
            if (Body.celestialBody.atmosphere && RemoveAtmosphere.Value)
            {
                // Find atmosphere from ground and destroy the game object
                AtmosphereFromGround atmosphere =
                    Body.scaledVersion.GetComponentsInChildren<AtmosphereFromGround>(true)[0];
                atmosphere.transform.parent = null;
                Object.Destroy(atmosphere.gameObject);

                // Destroy the light controller
                MaterialSetDirection light = Body.scaledVersion.GetComponentsInChildren<MaterialSetDirection>(true)[0];
                Object.Destroy(light);

                // No more atmosphere :(
                Body.celestialBody.atmosphere = false;
            }

            Logger.Active.Log("Using Template \"" + Body.celestialBody.bodyName + "\"");

            // If we have a PQS
            if (Body.pqsVersion != null)
            {
                // We only support one surface material per body, so use the one with the highest quality available
                Material surfaceMaterial = Body.pqsVersion.ultraQualitySurfaceMaterial;

                if (!surfaceMaterial)
                {
                    surfaceMaterial = Body.pqsVersion.highQualitySurfaceMaterial;
                }
                if (!surfaceMaterial)
                {
                    surfaceMaterial = Body.pqsVersion.mediumQualitySurfaceMaterial;
                }
                if (!surfaceMaterial)
                {
                    surfaceMaterial = Body.pqsVersion.lowQualitySurfaceMaterial;
                }
                if (!surfaceMaterial)
                {
                    surfaceMaterial = Body.pqsVersion.surfaceMaterial;
                }

                Body.pqsVersion.ultraQualitySurfaceMaterial = surfaceMaterial;
                Body.pqsVersion.highQualitySurfaceMaterial = surfaceMaterial;
                Body.pqsVersion.mediumQualitySurfaceMaterial = surfaceMaterial;
                Body.pqsVersion.lowQualitySurfaceMaterial = surfaceMaterial;
                Body.pqsVersion.surfaceMaterial = surfaceMaterial;

                // Should we remove the ocean?
                if (Body.celestialBody.ocean)
                {
                    // Find atmosphere the ocean PQS
                    PQS ocean = Body.pqsVersion.GetComponentsInChildren<PQS>(true)
                        .First(pqs => pqs != Body.pqsVersion);
                    PQSMod_CelestialBodyTransform cbt = Body.pqsVersion
                        .GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true).First();

                    // We only support one surface material per body, so use the one with the highest quality available
                    surfaceMaterial = ocean.ultraQualitySurfaceMaterial;

                    if (!surfaceMaterial)
                    {
                        surfaceMaterial = ocean.highQualitySurfaceMaterial;
                    }
                    if (!surfaceMaterial)
                    {
                        surfaceMaterial = ocean.mediumQualitySurfaceMaterial;
                    }
                    if (!surfaceMaterial)
                    {
                        surfaceMaterial = ocean.lowQualitySurfaceMaterial;
                    }
                    if (!surfaceMaterial)
                    {
                        surfaceMaterial = ocean.surfaceMaterial;
                    }

                    ocean.ultraQualitySurfaceMaterial = surfaceMaterial;
                    ocean.highQualitySurfaceMaterial = surfaceMaterial;
                    ocean.mediumQualitySurfaceMaterial = surfaceMaterial;
                    ocean.lowQualitySurfaceMaterial = surfaceMaterial;
                    ocean.surfaceMaterial = surfaceMaterial;

                    if (RemoveOcean.Value)
                    {
                        // Destroy the ocean PQS (this could be bad - destroying the secondary fades...)
                        cbt.planetFade.secondaryRenderers.Remove(ocean.gameObject);
                        cbt.secondaryFades = null;
                        ocean.transform.parent = null;
                        Object.Destroy(ocean);

                        // No more ocean :(
                        Body.celestialBody.ocean = false;
                        Body.pqsVersion.mapOcean = false;
                    }
                }

                // Selectively remove PQS Mods
                if (RemovePqsMods != null && RemovePqsMods.Value.LongCount() > 0)
                {
                    // We need a List with Types to remove
                    List<Type> mods = new List<Type>();
                    Dictionary<String, Type> modsPerName = new Dictionary<String, Type>();
                    foreach (String mod in RemovePqsMods.Value)
                    {
                        // If the definition has a name specified, grab that
                        String mType = mod;
                        String mName = "";
                        if (mType.EndsWith("]"))
                        {
                            String[] split = mType.Split('[');
                            mType = split[0];
                            mName = split[1].Remove(split[1].Length - 1);
                        }

                        // Get the mods matching the String
                        String modName = mType;
                        if (!mod.Contains("PQS"))
                        {
                            modName = "PQSMod_" + mod;
                        }

                        if (mName == "")
                        {
                            //mods.Add(Type.GetType(modName + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"));
                            Type t = Parser.ModTypes.Find(m => m.Name == modName);
                            if (t != null)
                            {
                                mods.Add(t);
                            }
                        }
                        else
                        {
                            //modsPerName.Add(name, Type.GetType(modName + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"));
                            Type t = Parser.ModTypes.Find(m => m.Name == modName);
                            if (t != null)
                            {
                                modsPerName.Add(mName, t);
                            }
                        }
                    }

                    Utility.RemoveModsOfType(mods, Body.pqsVersion);
                    foreach (KeyValuePair<String, Type> kvP in modsPerName)
                    {
                        Int32 index = 0;
                        String modName = kvP.Key;
                        if (modName.Contains(';'))
                        {
                            String[] split = modName.Split(';');
                            modName = split[0];
                            Int32.TryParse(split[1], out index);
                        }

                        PQSMod[] allMods = Body.pqsVersion.GetComponentsInChildren(kvP.Value, true)
                            .OfType<PQSMod>().Where(m => m.name == modName).ToArray();
                        if (allMods.Length <= 0)
                        {
                            continue;
                        }
                        if (allMods[index] is PQSCity)
                        {
                            PQSCity city = (PQSCity)allMods[index];
                            if (city.lod != null)
                            {
                                foreach (PQSCity.LODRange range in city.lod)
                                {
                                    if (range.objects != null)
                                    {
                                        foreach (GameObject o in range.objects)
                                        {
                                            Object.DestroyImmediate(o);
                                        }
                                    }

                                    if (range.renderers == null)
                                    {
                                        continue;
                                    }
                                    {
                                        foreach (GameObject o in range.renderers)
                                        {
                                            Object.DestroyImmediate(o);
                                        }
                                    }
                                }
                            }
                        }

                        if (allMods[index] is PQSCity2)
                        {
                            PQSCity2 city = (PQSCity2)allMods[index];
                            if (city.objects != null)
                            {
                                foreach (PQSCity2.LodObject range in city.objects)
                                {
                                    if (range.objects == null)
                                    {
                                        continue;
                                    }
                                    foreach (GameObject o in range.objects)
                                    {
                                        Object.DestroyImmediate(o);
                                    }
                                }
                            }
                        }

                        // If no mod is left, delete the game object too
                        GameObject gameObject = allMods[index].gameObject;
                        Object.DestroyImmediate(allMods[index]);
                        PQSMod[] allRemainingMods = gameObject.GetComponentsInChildren<PQSMod>(true);
                        if (allRemainingMods.Length == 0)
                        {
                            Object.DestroyImmediate(gameObject);
                        }
                    }
                }

                if (RemoveAllMods != null && RemoveAllMods.Value)
                {
                    // Remove all mods
                    Utility.RemoveModsOfType(null, Body.pqsVersion);
                }
            }

            // Should we remove the progress tree
            if (RemoveProgressTree.Value)
            {
                Body.celestialBody.progressTree = null;
            }

            // Figure out what kind of body we are
            Boolean isStar = Body.scaledVersion.GetComponentsInChildren<SunShaderController>(true).Length > 0;

            // remove coronas
            if (isStar && RemoveCoronas)
            {
                foreach (SunCoronas corona in Body.scaledVersion.GetComponentsInChildren<SunCoronas>(true))
                {
                    // RnD hard refs Coronas, so we need to disable them
                    corona.GetComponent<Renderer>().enabled = false;
                }
            }

            // Event
            Events.OnTemplateLoaderPostApply.Fire(this, node);
        }

        // Private exception to throw in the case the template doesn't load
        private class TemplateNotFoundException : Exception
        {
            public TemplateNotFoundException(String s) : base(s)
            {

            }
        }
    }
}
