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

using Kopernicus.MaterialWrapper;
using Kopernicus.OnDemand;
using System;
using System.Collections.Generic;
using Kopernicus.Components;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class ScaledVersionLoader : BaseLoader, IParserEventSubscriber
        {
            // Node name which represents the scaled version material
            public const String materialNodeName = "Material";

            // Scaled representation of a planet for map view to modify
            public GameObject scaledVersion;
            public CelestialBody owner;
            public StarComponent component;

            // Type of object this body's scaled version is
            [PreApply]
            [ParserTarget("type")]
            [KittopiaHideOption]
            public EnumParser<BodyType> type { get; set; }

            // Set the altitude where the fade to scaled space starts
            [ParserTarget("fadeStart")]
            public NumericParser<Single> fadeStart 
            {
                get { return scaledVersion.GetComponentInChildren<ScaledSpaceFader>() ? scaledVersion.GetComponentInChildren<ScaledSpaceFader>().fadeStart : 0; }
                set { scaledVersion.GetComponent<ScaledSpaceFader>().fadeStart = value; }
            }
            
            // Set the altitude where the fade to scaled space starts
            [ParserTarget("fadeEnd")]
            public NumericParser<Single> fadeEnd
            {
                get { return scaledVersion.GetComponent<ScaledSpaceFader>() ? scaledVersion.GetComponent<ScaledSpaceFader>().fadeEnd : 0; }
                set { scaledVersion.GetComponent<ScaledSpaceFader>().fadeEnd = value; }
            }

            // Create the Kopernicus LightShifter
            [ParserTarget("Light", allowMerge = true)]
            public LightShifterLoader lightShifter
            {
                get
                {
                    if (type == BodyType.Star)
                    {
                        LightShifter shifter = scaledVersion.GetComponentInChildren<LightShifter>();
                        if (shifter != null)
                        {
                            return new LightShifterLoader(owner);
                        }
                        if (Injector.IsInPrefab)
                        {
                            return new LightShifterLoader();
                        }
                    }
                    return null;
                }
                set { value.Value.transform.parent = scaledVersion.transform; }
            }

            // Coronas for a star's scaled version
            [ParserTargetCollection("Coronas", nameSignificance = NameSignificance.None)]
            public List<CoronaLoader> coronas = new List<CoronaLoader>();

            [ParserTarget("sphericalModel")]
            public NumericParser<Boolean> sphericalModel = new NumericParser<Boolean>(false);

            [ParserTarget("deferMesh")]
            public NumericParser<Boolean> deferMesh = new NumericParser<Boolean>(false);

            // Parser apply event
            void IParserEventSubscriber.Apply (ConfigNode node)
            {
                // Get any existing material we might have on this scaled version
                Material material = scaledVersion.GetComponent<Renderer>().sharedMaterial;
                ConfigNode data = node.GetNode (materialNodeName);

                // Check for bad condition (no material, no new material)
                if (material == null && data == null) 
                {
                    throw new Exception("Scaled version has no material information");
                }

                // Are we a planet or moon?
                if (type.Value != BodyType.Star)
                {
                    // If we are not a star, we need a scaled space fader and a sphere collider
                    if (scaledVersion.GetComponent<ScaledSpaceFader>() == null)
                    {
                        ScaledSpaceFader fader = scaledVersion.AddComponent<ScaledSpaceFader>();
                        fader.floatName = "_Opacity";
                        fader.celestialBody = owner;
                    }

                    // Add a sphere collider if we need one
                    if (scaledVersion.GetComponent<SphereCollider>() == null)
                    {
                        SphereCollider collider = scaledVersion.AddComponent<SphereCollider>();
                        collider.center = Vector3.zero;
                        collider.radius = 1000.0f;
                    }

                    // Generate new atmospheric body material
                    if (type.Value == BodyType.Atmospheric)
                    {
                        ScaledPlanetRimAerialLoader newMaterial = null;
                        if (material != null && ScaledPlanetRimAerial.UsesSameShader(material))
                        {
                            newMaterial = new ScaledPlanetRimAerialLoader(material);
                            if (data != null)
                                Parser.LoadObjectFromConfigurationNode(newMaterial, data, "Kopernicus");
                        }
                        else
                        {
                            if (data == null)
                                throw new Exception("Scaled version has no material information");
                            newMaterial =
                                Parser.CreateObjectFromConfigNode<ScaledPlanetRimAerialLoader>(data, "Kopernicus");
                        }
                        newMaterial.name = Guid.NewGuid().ToString();
                        if (newMaterial.rimColorRamp != null)
                        {
                            newMaterial.rimColorRamp.wrapMode = TextureWrapMode.Clamp;
                            newMaterial.rimColorRamp.mipMapBias = 0.0f;
                        }
                        scaledVersion.GetComponent<Renderer>().sharedMaterial = newMaterial;
                    }

                    // Generate new vacuum body material
                    else
                    {
                        ScaledPlanetSimpleLoader newMaterial = null;
                        if (material != null && ScaledPlanetSimple.UsesSameShader(material))
                        {
                            newMaterial = new ScaledPlanetSimpleLoader(material);
                            if (data != null)
                                Parser.LoadObjectFromConfigurationNode(newMaterial, data, "Kopernicus");
                        }
                        else
                        {
                            if (data == null)
                                throw new Exception("Scaled version has no material information");
                            newMaterial =
                                Parser.CreateObjectFromConfigNode<ScaledPlanetSimpleLoader>(data, "Kopernicus");
                        }
                        newMaterial.name = Guid.NewGuid().ToString();
                        scaledVersion.GetComponent<Renderer>().sharedMaterial = newMaterial;
                    }
                }

                // Otherwise we are a star
                else
                {
                    // Add the SunShaderController behavior
                    if (scaledVersion.GetComponent<SunShaderController>() == null)
                        scaledVersion.AddComponent<SunShaderController>();

                    // Add the ScaledSun behavior
                    // TODO - apparently there can only be one of these (or it destroys itself)
                    if (scaledVersion.GetComponent<ScaledSun>() == null)
                        scaledVersion.AddComponent<ScaledSun>();

                    // Add the Kopernicus star componenet
                    component = scaledVersion.AddComponent<StarComponent>();

                    // Generate a new material for the star
                    EmissiveMultiRampSunspotsLoader newMaterial = null;
                    if (material != null && EmissiveMultiRampSunspots.UsesSameShader(material))
                    {
                        newMaterial = new EmissiveMultiRampSunspotsLoader(material);
                        if (data != null)
                            Parser.LoadObjectFromConfigurationNode(newMaterial, data, "Kopernicus");
                    }
                    else
                    {
                        if (data == null)
                            throw new Exception("Scaled version has no material information");
                        newMaterial =
                            Parser.CreateObjectFromConfigNode<EmissiveMultiRampSunspotsLoader>(data, "Kopernicus");
                    }

                    newMaterial.name = Guid.NewGuid().ToString();
                    scaledVersion.GetComponent<Renderer>().sharedMaterial = newMaterial;


                    // Backup existing coronas
                    foreach (SunCoronas corona in scaledVersion.GetComponentsInChildren<SunCoronas>(true))
                    {
                        corona.transform.parent = Utility.Deactivator;
                    }
                }

                // Event
                Events.OnScaledVersionLoaderApply.Fire(this, node);
            }

            // Post apply event
            void IParserEventSubscriber.PostApply (ConfigNode node)
            {
                Logger.Active.Log("============= Scaled Version Dump ===================");
                Utility.GameObjectWalk(scaledVersion);
                Logger.Active.Log("===========================================");

                // If we are a star, we need to generate the coronas 
                if (type.Value == BodyType.Star) 
                {
                    // Restore backed up coronas if no custom ones were specified
                    if (coronas.Count == 0)
                    {
                        foreach (SunCoronas corona in Utility.Deactivator.GetComponentsInChildren<SunCoronas>(true))
                        {
                            corona.transform.parent = scaledVersion.transform;
                        }
                    }
                    else
                    {
                        foreach (SunCoronas corona in Utility.Deactivator.GetComponentsInChildren<SunCoronas>(true))
                        {
                            corona.transform.parent = null;
                            UnityEngine.Object.Destroy(corona.gameObject);
                        }
                    }
                }

                // If we use OnDemand, we need to delete the original textures and reload them
                if (OnDemandStorage.useOnDemand && type.Value != BodyType.Star)
                {
                    Texture2D texture = scaledVersion.GetComponent<Renderer>().material.GetTexture("_MainTex") as Texture2D;
                    Texture2D normals = scaledVersion.GetComponent<Renderer>().material.GetTexture("_BumpMap") as Texture2D;
                    ScaledSpaceDemand demand = scaledVersion.AddComponent<ScaledSpaceDemand>();
                    if (texture != null)
                        demand.texture = texture.name;
                    if (normals != null)
                        demand.normals = normals.name;
                }

                // Event
                Events.OnScaledVersionLoaderPostApply.Fire(this, node);
            }

            /// <summary>
            /// Creates a new ScaledVersion Loader from the Injector context.
            /// </summary>
            public ScaledVersionLoader()
            {
                // Is this the parser context?
                if (!Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("Must be executed in Injector context.");
                }
                
                // Get the scaled version object
                scaledVersion = generatedBody.scaledVersion;
                owner = generatedBody.celestialBody;
                type = new EnumParser<BodyType>(Loader.currentBody.template == null ? BodyType.Atmospheric : Loader.currentBody.template.type);

                // Ensure scaled version at least has a mesh filter and mesh renderer
                if (scaledVersion.GetComponent<MeshFilter>() == null)
                {
                    scaledVersion.AddComponent<MeshFilter>();
                }
                if (scaledVersion.GetComponent<MeshRenderer>() == null)
                {
                    scaledVersion.AddComponent<MeshRenderer>();
                    scaledVersion.GetComponent<Renderer>().material = null;
                }
            }

            /// <summary>
            /// Creates a new ScaledVersion Loader from a spawned CelestialBody.
            /// </summary>
            public ScaledVersionLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }

                // Get the scaled version object
                scaledVersion = body.scaledBody;
                owner = body;

                // Figure out what kind of body we are
                if (scaledVersion.GetComponentsInChildren<SunShaderController>(true).Length > 0)
                    type = BodyType.Star;
                else if (owner.atmosphere)
                    type = BodyType.Atmospheric;
                else
                    type = BodyType.Vacuum;
            }
        }
    }
}
