/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
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
            public double radius { get; set; }

            // Initial type of the body
            public BodyType type { get; set; }

            // PSystemBody to use as a template in lookup & clone
            public PSystemBody originalBody; 

            // Name of the body to use for the template
            [PreApply]
            [ParserTarget("name", optional = false)]
            public string name 
            {
                // Crawl the system prefab for the body
                set 
                { 
                    originalBody = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, value);
                    if(originalBody == null)
                    {
                        throw new TemplateNotFoundException("Unable to find: " + value);
                    }
                }
            }

            // Should we strip the PQS off
            [PreApply]
            [ParserTarget("removePQS")]
            public NumericParser<bool> removePQS = new NumericParser<bool> (false);

            // Should we strip the atmosphere off
            [ParserTarget("removeAtmosphere")]
            public NumericParser<bool> removeAtmosphere = new NumericParser<bool>(false);

            // Should we strip the ocean off
            [ParserTarget("removeOcean")]
            public NumericParser<bool> removeOcean = new NumericParser<bool>(false);

            // Collection of PQS mods to remove
            [ParserTarget("removePQSMods")]
            public StringCollectionParser removePQSMods;

            // Should we strip all Mods off
            [ParserTarget("removeAllPQSMods")]
            public NumericParser<bool> removeAllMods = new NumericParser<bool>(false);

            // Collection of PQS mods to remove
            [ParserTarget("removeProgressTree")]
            public NumericParser<bool> removeProgressTree = new NumericParser<bool> (true);

            // Remove coronas for star
            [ParserTarget("removeCoronas")]
            public NumericParser<bool> removeCoronas = new NumericParser<bool>(false);

            // Apply event
            void IParserEventSubscriber.Apply (ConfigNode node)
            {
                // Instantiate (clone) the template body
                GameObject bodyGameObject = UnityEngine.Object.Instantiate (originalBody.gameObject) as GameObject;
                bodyGameObject.name = originalBody.name;
                bodyGameObject.transform.parent = Utility.Deactivator;
                body = bodyGameObject.GetComponent<PSystemBody> ();
                body.children = new List<PSystemBody> ();

                // Clone the scaled version
                body.scaledVersion = UnityEngine.Object.Instantiate (originalBody.scaledVersion) as GameObject;
                body.scaledVersion.transform.parent = Utility.Deactivator;
                body.scaledVersion.name = originalBody.scaledVersion.name;
                
                // Clone the PQS version (if it has one) and we want the PQS
                if (body.pqsVersion != null && removePQS.value != true) 
                {
                    body.pqsVersion = UnityEngine.Object.Instantiate (originalBody.pqsVersion) as PQS;
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
            }

            // Post apply event
            void IParserEventSubscriber.PostApply (ConfigNode node)
            {
                // Should we remove the atmosphere
                if (body.celestialBody.atmosphere && removeAtmosphere.value) 
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

                // If we have a PQS
                if (body.pqsVersion != null) 
                {
                    Logger.Active.Log ("[Kopernicus]: Configuration.Template: Using Template \"" + body.celestialBody.bodyName + "\"");

                    // Should we remove the ocean?
                    if (body.celestialBody.ocean && removeOcean.value) 
                    {
                        // Find atmosphere the ocean PQS
                        PQS ocean = body.pqsVersion.GetComponentsInChildren<PQS> (true).Where (pqs => pqs != body.pqsVersion).First ();
                        PQSMod_CelestialBodyTransform cbt = body.pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform> (true).First ();

                        // Destroy the ocean PQS (this could be bad - destroying the secondary fades...)
                        cbt.planetFade.secondaryRenderers.Remove (ocean.gameObject);
                        cbt.secondaryFades = null;
                        ocean.transform.parent = null;
                        UnityEngine.Object.Destroy (ocean);
                    
                        // No more ocean :(
                        body.celestialBody.ocean = false;
                        body.pqsVersion.mapOcean = false;
                    }

                    // Selectively remove PQS Mods
                    if (removePQSMods != null && removePQSMods.value.LongCount() > 0)
                    {
                        // We need a List with Types to remove
                        List<Type> mods = new List<Type>();
                        Dictionary<string, Type> modsPerName = new Dictionary<string, Type>();
                        foreach (string mod in removePQSMods.value)
                        {
                            // If the definition has a name specified, grab that
                            string mType = mod;
                            string name = "";
                            if (mType.EndsWith("]"))
                            {
                                string[] split = mType.Split('[');
                                mType = split[0];
                                name = split[1].Remove(split[1].Length - 1);
                            }
                            
                            // Get the mods matching the string
                            string modName = mType;
                            if(!mod.Contains("PQS"))
                                modName = "PQSMod_" + mod;
                            if (name == "")
                            {
                                //mods.Add(Type.GetType(modName + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"));
                                Type t = Injector.ModTypes.Find(m => m.Name == modName);
                                if (t != null)
                                    mods.Add(t);
                            }
                            else
                            {
                                //modsPerName.Add(name, Type.GetType(modName + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"));
                                Type t = Injector.ModTypes.Find(m => m.Name == modName);
                                if (t != null)
                                    modsPerName.Add(name, t);
                            }
                        }
                        Utility.RemoveModsOfType(mods, body.pqsVersion);
                        foreach (KeyValuePair<string, Type> kvP in modsPerName)
                        {
                            int index = 0;
                            string name = kvP.Key;
                            if (name.Contains(';'))
                            {
                                string[] split = name.Split(';');
                                name = split[0];
                                Int32.TryParse(split[1], out index);
                            }
                            PQSMod[] allMods = body.pqsVersion.GetComponentsInChildren(kvP.Value, true).Select(m => m as PQSMod).Where(m => m.name == name).ToArray();
                            if (allMods.Length > 0)
                                UnityEngine.Object.DestroyImmediate(allMods[index]);
                        }
                    }

                    if (removeAllMods != null && removeAllMods.value)
                    {
                        // Remove all mods
                        Utility.RemoveModsOfType(null, body.pqsVersion);
                    }
                }

                // Should we remove the progress tree
                if (removeProgressTree.value) 
                {
                    body.celestialBody.progressTree = null;
                }

                // Figure out what kind of body we are
                if (body.scaledVersion.GetComponentsInChildren<SunShaderController>(true).Length > 0)
                    type = BodyType.Star;
                else if(body.celestialBody.atmosphere)
                    type = BodyType.Atmospheric;
                else
                    type = BodyType.Vacuum;

                // remove coronas
                if (type == BodyType.Star && removeCoronas)
                {
                    foreach (SunCoronas corona in body.scaledVersion.GetComponentsInChildren<SunCoronas>(true))
                        corona.GetComponent<Renderer>().enabled = false; // RnD hard refs Coronas, so we need to disable them
                }
            }

            // Private exception to throw in the case the template doesn't load
            public class TemplateNotFoundException : Exception
            {
                public TemplateNotFoundException(string s) : base(s)
                {

                }
            }
        }
    }
}

