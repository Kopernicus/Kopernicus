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
using System.Linq;
using Kopernicus.Components;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class ScaledVersionLoader : BaseLoader, IParserEventSubscriber, ITypeParser<CelestialBody>
        {
            // Node name which represents the scaled version material
            public const String materialNodeName = "Material";

            // Scaled representation of a planet for map view to modify
            public CelestialBody Value { get; set; }

            // Type of object this body's scaled version is
            [PreApply]
            [ParserTarget("type")]
            [KittopiaHideOption]
            public EnumParser<BodyType> type { get; set; }

            // Set the altitude where the fade to scaled space starts
            [ParserTarget("fadeStart")]
            public NumericParser<Single> fadeStart
            {
                get
                {
                    return Value.scaledBody.GetComponentInChildren<ScaledSpaceFader>()
                        ? Value.scaledBody.GetComponentInChildren<ScaledSpaceFader>().fadeStart
                        : 0;
                }
                set { Value.scaledBody.GetComponent<ScaledSpaceFader>().fadeStart = value; }
            }

            // Set the altitude where the fade to scaled space starts
            [ParserTarget("fadeEnd")]
            public NumericParser<Single> fadeEnd
            {
                get
                {
                    return Value.scaledBody.GetComponent<ScaledSpaceFader>()
                        ? Value.scaledBody.GetComponent<ScaledSpaceFader>().fadeEnd
                        : 0;
                }
                set { Value.scaledBody.GetComponent<ScaledSpaceFader>().fadeEnd = value; }
            }

            // Create the Kopernicus LightShifter
            [ParserTarget("Light", AllowMerge = true)]
            public LightShifterLoader lightShifter
            {
                get
                {
                    if (type == BodyType.Star)
                    {
                        LightShifter shifter = Value.scaledBody.GetComponentInChildren<LightShifter>();
                        if (shifter != null)
                        {
                            return new LightShifterLoader(Value);
                        }
                        if (Injector.IsInPrefab)
                        {
                            return new LightShifterLoader();
                        }
                    }
                    return null;
                }
                set { value.Value.transform.parent = Value.scaledBody.transform; }
            }

            // Coronas for a star's scaled version
            [ParserTargetCollection("Coronas", NameSignificance = NameSignificance.None)]
            public List<CoronaLoader> coronas = new List<CoronaLoader>();

            [ParserTarget("sphericalModel")]
            public NumericParser<Boolean> sphericalModel
            {
                get { return Value.Get("sphericalModel", false); }
                set { Value.Set("sphericalModel", value.Value); }
            }

            [ParserTarget("deferMesh")]
            public NumericParser<Boolean> deferMesh
            {
                get { return Value.Get("deferMesh", false); }
                set { Value.Set("deferMesh", value.Value); }
            }

            [ParserTarget("Material", AllowMerge = true)]
            public Material material
            {
                get { return Value.scaledBody.GetComponent<Renderer>().sharedMaterial; }
                set { Value.scaledBody.GetComponent<Renderer>().sharedMaterial = value; }
            }

            [KittopiaAction("Rebuild ScaledSpace Mesh")]
            public void RebuildScaledSpace()
            {
                Utility.UpdateScaledMesh(Value.scaledBody, Value.pqsController, Value, Body.ScaledSpaceCacheDirectory,
                    Value.Get("cacheFile", ""), Value.Get("exportMesh", true), sphericalModel);
            }

            // Parser apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                // Get any existing material we might have on this scaled version
                ConfigNode data = node.GetNode(materialNodeName);

                // Check for bad condition (no material, no new material)
                if (material == null && data == null)
                {
                    throw new Exception("Scaled version has no material information");
                }

                // Are we a planet or moon?
                if (type.Value != BodyType.Star)
                {
                    // If we are not a star, we need a scaled space fader and a sphere collider
                    if (Value.scaledBody.GetComponent<ScaledSpaceFader>() == null)
                    {
                        ScaledSpaceFader fader = Value.scaledBody.AddComponent<ScaledSpaceFader>();
                        fader.floatName = "_Opacity";
                        fader.celestialBody = Value;
                    }

                    // Add a sphere collider if we need one
                    if (Value.scaledBody.GetComponent<SphereCollider>() == null)
                    {
                        SphereCollider collider = Value.scaledBody.AddComponent<SphereCollider>();
                        collider.center = Vector3.zero;
                        collider.radius = 1000.0f;
                    }

                    // Generate new atmospheric body material
                    if (type.Value == BodyType.Atmospheric)
                    {
                        if (material != null && ScaledPlanetRimAerial.UsesSameShader(material))
                        {
                            material = new ScaledPlanetRimAerialLoader(material);
                        }
                        else
                        {
                            material = new ScaledPlanetRimAerialLoader();
                        }
                        material.name = Guid.NewGuid().ToString();
                    }

                    // Generate new vacuum body material
                    else
                    {
                        if (material != null && ScaledPlanetSimple.UsesSameShader(material))
                        {
                            material = new ScaledPlanetSimpleLoader(material);
                        }
                        else
                        {
                            material = new ScaledPlanetSimpleLoader();
                        }
                        material.name = Guid.NewGuid().ToString();
                    }
                }

                // Otherwise we are a star
                else
                {
                    // Add the SunShaderController behavior
                    if (Value.scaledBody.GetComponent<SunShaderController>() == null)
                        Value.scaledBody.AddComponent<SunShaderController>();

                    // Add the ScaledSun behavior
                    // TODO - apparently there can only be one of these (or it destroys itself)
                    if (Value.scaledBody.GetComponent<ScaledSun>() == null)
                        Value.scaledBody.AddComponent<ScaledSun>();

                    // Add the Kopernicus star componenet
                    Value.scaledBody.AddComponent<StarComponent>();

                    // Generate a new material for the star
                    EmissiveMultiRampSunspotsLoader newMaterial = null;
                    if (material != null && EmissiveMultiRampSunspots.UsesSameShader(material))
                    {
                        material = new EmissiveMultiRampSunspotsLoader(material);
                    }
                    else
                    {
                        material = new EmissiveMultiRampSunspotsLoader();
                    }
                    material.name = Guid.NewGuid().ToString();


                    // Backup existing coronas
                    foreach (SunCoronas corona in Value.scaledBody.GetComponentsInChildren<SunCoronas>(true))
                    {
                        corona.transform.parent = Utility.Deactivator;
                    }
                }

                // Event
                Events.OnScaledVersionLoaderApply.Fire(this, node);
            }

            // Post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                Logger.Active.Log("============= Scaled Version Dump ===================");
                Utility.GameObjectWalk(Value.scaledBody);
                Logger.Active.Log("===========================================");

                // If we are a star, we need to generate the coronas 
                if (type.Value == BodyType.Star)
                {
                    // Restore backed up coronas if no custom ones were specified
                    if (!coronas.Any())
                    {
                        foreach (SunCoronas corona in Utility.Deactivator.GetComponentsInChildren<SunCoronas>(true)
                            .Where(c => c.transform.parent == Utility.Deactivator))
                        {
                            corona.transform.parent = Value.scaledBody.transform;
                        }
                    }
                    else
                    {
                        foreach (SunCoronas corona in Utility.Deactivator.GetComponentsInChildren<SunCoronas>(true)
                            .Where(c => c.transform.parent == Utility.Deactivator))
                        {
                            corona.transform.parent = null;
                            UnityEngine.Object.Destroy(corona.gameObject);
                        }
                    }
                }

                // If we use OnDemand, we need to delete the original textures and reload them
                if (OnDemandStorage.useOnDemand && type.Value != BodyType.Star)
                {
                    Texture2D texture =
                        Value.scaledBody.GetComponent<Renderer>().material.GetTexture("_MainTex") as Texture2D;
                    Texture2D normals =
                        Value.scaledBody.GetComponent<Renderer>().material.GetTexture("_BumpMap") as Texture2D;
                    ScaledSpaceOnDemand onDemand = Value.scaledBody.AddComponent<ScaledSpaceOnDemand>();
                    if (texture != null)
                        onDemand.texture = texture.name;
                    if (normals != null)
                        onDemand.normals = normals.name;
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
                Value = generatedBody.celestialBody;
                Value.scaledBody = generatedBody.scaledVersion;
                type = new EnumParser<BodyType>(Loader.currentBody.template == null
                    ? BodyType.Atmospheric
                    : Loader.currentBody.template.type);

                // Ensure scaled version at least has a mesh filter and mesh renderer
                if (Value.scaledBody.GetComponent<MeshFilter>() == null)
                {
                    Value.scaledBody.AddComponent<MeshFilter>();
                }
                if (Value.scaledBody.GetComponent<MeshRenderer>() == null)
                {
                    Value.scaledBody.AddComponent<MeshRenderer>();
                    Value.scaledBody.GetComponent<Renderer>().material = null;
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
                Value = body;
                coronas = Value.scaledBody.GetComponentsInChildren<SunCoronas>(true).Select(c => new CoronaLoader(c))
                    .ToList();

                // Figure out what kind of body we are
                if (Value.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length > 0)
                    type = BodyType.Star;
                else if (Value.atmosphere)
                    type = BodyType.Atmospheric;
                else
                    type = BodyType.Vacuum;

                // Assign the proper scaled space loaders
                if (material == null)
                {
                    return;
                }
                if (ScaledPlanetSimple.UsesSameShader(material))
                {
                    material = new ScaledPlanetSimpleLoader(material);
                }
                if (ScaledPlanetRimAerial.UsesSameShader(material))
                {
                    material = new ScaledPlanetRimAerialLoader(material);
                }
                if (EmissiveMultiRampSunspots.UsesSameShader(material))
                {
                    material = new EmissiveMultiRampSunspotsLoader(material);
                }
            }
        }
    }
}
