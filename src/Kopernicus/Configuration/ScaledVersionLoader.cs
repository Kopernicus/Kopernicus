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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kopernicus.Components;
using Kopernicus.Components.MaterialWrapper;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Enumerations;
using Kopernicus.Configuration.MaterialLoader;
using Kopernicus.Configuration.Parsing;
using Kopernicus.OnDemand;
using Kopernicus.RuntimeUtility;
using Kopernicus.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kopernicus.Configuration
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class ScaledVersionLoader : BaseLoader, IParserEventSubscriber, ITypeParser<CelestialBody>
    {
        // Node name which represents the scaled version material
        private const String MATERIAL_NODE_NAME = "Material";

        // Scaled representation of a planet for map view to modify
        public CelestialBody Value { get; set; }

        [RequireConfigType(ConfigType.Node)]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
        public class OnDemandConfig
        {
            [ParserTarget("texture")]
            [ParserTarget("mainTex")]
            public String Texture { get; set; }

            [ParserTarget("normals")]
            [ParserTarget("bumpMap")]
            public String Normals { get; set; }
        }

        // Type of object this body's scaled version is
        [PreApply]
        [ParserTarget("type")]
        [KittopiaHideOption]
        public EnumParser<ScaledMaterialType> Type
        {
            get
            {
                Material material = Value.scaledBody.GetComponent<Renderer>().sharedMaterial;

                if (ScaledPlanetSimple.UsesSameShader(material))
                {
                    return ScaledMaterialType.Vacuum;
                }
                if (ScaledPlanetRimAerial.UsesSameShader(material))
                {
                    return ScaledMaterialType.Atmospheric;
                }
                if (ScaledPlanetRimAerialStandard.UsesSameShader(material))
                {
                    return ScaledMaterialType.AtmosphericStandard;
                }
                if (EmissiveMultiRampSunspots.UsesSameShader(material))
                {
                    return ScaledMaterialType.Star;
                }
                if (material.shader.name.Equals("Terrain/Gas Giant")) // Kludge for Joolian shader loading for now
                {
                    return ScaledMaterialType.Atmospheric;
                }
                throw new Exception("The shader '" + material.shader.name + "' is not supported.");
            }
            set
            {
                Renderer renderer = Value.scaledBody.GetComponent<Renderer>();

                Boolean isVaccum = ScaledPlanetSimple.UsesSameShader(renderer.sharedMaterial);
                Boolean isAtmospheric = ScaledPlanetRimAerial.UsesSameShader(renderer.sharedMaterial);
                Boolean isAtmosphericStandard = ScaledPlanetRimAerialStandard.UsesSameShader(renderer.sharedMaterial);
                Boolean isStar = EmissiveMultiRampSunspots.UsesSameShader(renderer.sharedMaterial);

                switch (value.Value)
                {
                    case ScaledMaterialType.Vacuum when !isVaccum:
                        renderer.sharedMaterial = new ScaledPlanetSimpleLoader();
                        break;
                    case ScaledMaterialType.Atmospheric when !isAtmospheric:
                        renderer.sharedMaterial = new ScaledPlanetRimAerialLoader();
                        break;
                    case ScaledMaterialType.AtmosphericStandard when !isAtmosphericStandard:
                        renderer.sharedMaterial = new ScaledPlanetRimAerialStandardLoader();
                        break;
                    case ScaledMaterialType.Star when !isStar:
                        renderer.sharedMaterial = new EmissiveMultiRampSunspotsLoader();
                        break;
                    default:
                        return;
                }
            }
        }

        // Set the altitude where the fade to scaled space starts
        [ParserTarget("fadeStart")]
        public NumericParser<Single> FadeStart
        {
            get
            {
                ScaledSpaceFader fader = Value.scaledBody.GetComponentInChildren<ScaledSpaceFader>();

                if (fader)
                {
                    return fader.fadeStart;
                }

                return 0;
            }
            set
            {
                ScaledSpaceFader fader = Value.scaledBody.GetComponentInChildren<ScaledSpaceFader>();

                if (fader)
                {
                    fader.fadeStart = value;
                }
            }
        }

        // Set the altitude where the fade to scaled space starts
        [ParserTarget("fadeEnd")]
        public NumericParser<Single> FadeEnd
        {
            get
            {
                ScaledSpaceFader fader = Value.scaledBody.GetComponentInChildren<ScaledSpaceFader>();

                if (fader)
                {
                    return fader.fadeEnd;
                }

                return 0;
            }
            set
            {
                ScaledSpaceFader fader = Value.scaledBody.GetComponentInChildren<ScaledSpaceFader>();

                if (fader)
                {
                    fader.fadeEnd = value;
                }
            }
        }

        // Create the Kopernicus LightShifter
        [ParserTarget("Light", AllowMerge = true)]
        [KittopiaUntouchable]
        public LightShifterLoader LightShifter
        {
            get
            {
                if (Type != ScaledMaterialType.Star)
                {
                    return null;
                }

                LightShifter shifter = Value.scaledBody.GetComponentInChildren<LightShifter>();
                if (shifter != null)
                {
                    return new LightShifterLoader(Value);
                }

                return Injector.IsInPrefab ? new LightShifterLoader() : null;
            }
            set { value.Value.transform.parent = Value.scaledBody.transform; }
        }

        // Coronas for a star's scaled version
        [ParserTargetCollection("Coronas", NameSignificance = NameSignificance.None)]
        [KittopiaUntouchable]
        public readonly List<CoronaLoader> Coronas = new List<CoronaLoader>();

        private static readonly Int32 MainTex = Shader.PropertyToID("_MainTex");
        private static readonly Int32 BumpMap = Shader.PropertyToID("_BumpMap");

        [ParserTarget("sphericalModel")]
        public NumericParser<Boolean> SphericalModel
        {
            get { return Value.Get("sphericalModel", false); }
            set { Value.Set("sphericalModel", value.Value); }
        }

        [ParserTarget("deferMesh")]
        public NumericParser<Boolean> DeferMesh
        {
            get { return Value.Get("deferMesh", false); }
            set { Value.Set("deferMesh", value.Value); }
        }

        [ParserTarget("invisible")]
        public NumericParser<Boolean> Invisible
        {
            get { return Value.Get("invisibleScaledSpace", false); }
            set
            {
                Value.Set("invisibleScaledSpace", value.Value);
                DeferMesh = value.Value;
            }
        }

        [ParserTarget("Material", AllowMerge = true)]
        [KittopiaUntouchable]
        public Material Material
        {
            get
            {
                Renderer renderer = Value.scaledBody.GetComponent<Renderer>();

                Boolean isVaccum = renderer.sharedMaterial is ScaledPlanetSimpleLoader;
                Boolean isAtmospheric = renderer.sharedMaterial is ScaledPlanetRimAerialLoader;
                Boolean isAtmosphericStandard = renderer.sharedMaterial is ScaledPlanetRimAerialStandardLoader;
                Boolean isStar = renderer.sharedMaterial is EmissiveMultiRampSunspotsLoader;

                switch (Type.Value)
                {
                    case ScaledMaterialType.Vacuum when !isVaccum:
                        renderer.sharedMaterial = new ScaledPlanetSimpleLoader(renderer.sharedMaterial);
                        goto default;
                    case ScaledMaterialType.Atmospheric when !isAtmospheric:
                        renderer.sharedMaterial = new ScaledPlanetRimAerialLoader(renderer.sharedMaterial);
                        goto default;
                    case ScaledMaterialType.AtmosphericStandard when !isAtmosphericStandard:
                        renderer.sharedMaterial = new ScaledPlanetRimAerialStandardLoader(renderer.sharedMaterial);
                        goto default;
                    case ScaledMaterialType.Star when !isStar:
                        renderer.sharedMaterial = new EmissiveMultiRampSunspotsLoader(renderer.sharedMaterial);
                        goto default;
                    default:
                        return renderer.sharedMaterial;
                }
            }
            set
            {
                Renderer renderer = Value.scaledBody.GetComponent<Renderer>();

                Boolean isVaccum = value is ScaledPlanetSimpleLoader;
                Boolean isAtmospheric = value is ScaledPlanetRimAerialLoader;
                Boolean isAtmosphericStandard = value is ScaledPlanetRimAerialStandardLoader;
                Boolean isStar = value is EmissiveMultiRampSunspotsLoader;

                switch (Type.Value)
                {
                    case ScaledMaterialType.Vacuum when !isVaccum:
                        renderer.sharedMaterial = new ScaledPlanetSimpleLoader(value);
                        break;
                    case ScaledMaterialType.Atmospheric when !isAtmospheric:
                        renderer.sharedMaterial = new ScaledPlanetRimAerialLoader(value);
                        break;
                    case ScaledMaterialType.AtmosphericStandard when !isAtmosphericStandard:
                        renderer.sharedMaterial = new ScaledPlanetRimAerialStandardLoader(value);
                        break;
                    case ScaledMaterialType.Star when !isStar:
                        renderer.sharedMaterial = new EmissiveMultiRampSunspotsLoader(value);
                        break;
                    default:
                        renderer.sharedMaterial = value;
                        break;
                }
            }
        }

        [ParserTarget("OnDemand")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public OnDemandConfig OnDemandTextures { get; set; }

        [ParserTarget("TextureOptions", AllowMerge = true)]
        [KittopiaHideOption(Export = false, Show = true)]
        [KittopiaUntouchable]
        public PlanetTextureExporter.TextureOptions Options
        {
            get { return Value.Get<PlanetTextureExporter.TextureOptions>("textureOptions", null); }
            set { Value.Set("textureOptions", value); }
        }

        [KittopiaAction("Rebuild ScaledSpace Mesh")]
        public void RebuildScaledSpace()
        {
            Utility.UpdateScaledMesh(Value.scaledBody, Value.pqsController, Value, Body.SCALED_SPACE_CACHE_DIRECTORY,
                Value.Get("cacheFile", ""), Value.Get("exportMesh", true), SphericalModel);
        }

        [KittopiaAction("Rebuild ScaledSpace Textures")]
        public IEnumerator RebuildTextures()
        {
            return PlanetTextureExporter.UpdateTextures(Value, Options);
        }

        // Parser apply event
        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            // Are we a planet or moon?
            if (Type != ScaledMaterialType.Star)
            {
                // If we are not a star, we need a scaled space fader and a sphere collider
                ScaledSpaceFader fader = Value.scaledBody.GetComponent<ScaledSpaceFader>();
                if (fader == null)
                {
                    fader = Value.scaledBody.AddComponent<SharedScaledSpaceFader>();
                    fader.floatName = "_Opacity";
                    fader.celestialBody = Value;
                }
                else if (!(fader is SharedScaledSpaceFader))
                {
                    Utility.CopyObjectFields(fader, Value.scaledBody.AddComponent<SharedScaledSpaceFader>());
                    Object.DestroyImmediate(fader);
                }

                // Add a sphere collider if we need one
                if (Value.scaledBody.GetComponent<SphereCollider>() == null)
                {
                    SphereCollider collider = Value.scaledBody.AddComponent<SphereCollider>();
                    collider.center = Vector3.zero;
                    collider.radius = 1000.0f;
                }
            }

            // Otherwise we are (likely) a star
            else if (Type == ScaledMaterialType.Star)
            {
                // Add the SunShaderController behavior
                SunShaderController controller = Value.scaledBody.GetComponent<SunShaderController>();
                if (controller == null)
                {
                    Value.scaledBody.AddComponent<SharedSunShaderController>();
                }
                else if (!(controller is SharedSunShaderController))
                {
                    Utility.CopyObjectFields(controller, Value.scaledBody.AddComponent<SharedSunShaderController>());
                    Object.DestroyImmediate(controller);
                }

                // Add the ScaledSun behavior
                // TODO - apparently there can only be one of these (or it destroys itself)
                if (Value.scaledBody.GetComponent<ScaledSun>() == null)
                {
                    Value.scaledBody.AddComponent<ScaledSun>();
                }

                // Add the Kopernicus star component
                Value.scaledBody.AddComponent<StarComponent>();

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
            if (Type == ScaledMaterialType.Star)
            {
                // Restore backed up coronas if no custom ones were specified
                if (!Coronas.Any())
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
                        Object.Destroy(corona.gameObject);
                    }
                }
            }

            // If we use OnDemand, we need to delete the original textures and reload them
            if (Type != ScaledMaterialType.Star && OnDemandTextures != null)
            {
                if (OnDemandStorage.UseOnDemand)
                {
                    ScaledSpaceOnDemand onDemand = Value.scaledBody.AddComponent<ScaledSpaceOnDemand>();
                    onDemand.texture = OnDemandTextures.Texture;
                    onDemand.normals = OnDemandTextures.Normals;

                    // Delete the original scaled space textures
                    if (OnDemandTextures.Texture != null)
                    {
                        Texture2D texture = new Texture2D(1, 1);
                        texture.Apply();
                        Material.SetTexture(MainTex, texture);
                    }

                    if (OnDemandTextures.Normals != null)
                    {
                        Texture2D texture = new Texture2D(1, 1);
                        texture.Apply();
                        Material.SetTexture(BumpMap, texture);
                    }
                }
                else
                {
                    // If OD isn't enabled, load the textures, assign them and don't care anymore
                    Texture2DParser parser = new Texture2DParser();

                    if (OnDemandTextures.Texture != null)
                    {
                        parser.SetFromString(OnDemandTextures.Texture);
                        Material.SetTexture(MainTex, parser);
                    }

                    if (OnDemandTextures.Normals != null)
                    {
                        parser.SetFromString(OnDemandTextures.Normals);
                        Material.SetTexture(BumpMap, parser);
                    }
                }
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

            // Ensure scaled version at least has a mesh filter and mesh renderer
            if (Value.scaledBody.GetComponent<MeshFilter>() == null)
            {
                Value.scaledBody.AddComponent<MeshFilter>();
            }

            if (Value.scaledBody.GetComponent<MeshRenderer>() == null)
            {
                Value.scaledBody.AddComponent<MeshRenderer>();
                Value.scaledBody.GetComponent<Renderer>().sharedMaterial = null;
            }

            if (Options != null)
            {
                return;
            }

            Options = new PlanetTextureExporter.TextureOptions();
            if (generatedBody.pqsVersion == null)
            {
                return;
            }

            Options.Resolution = generatedBody.pqsVersion.mapFilesize;
            Options.TextureDelta = generatedBody.pqsVersion.mapMaxHeight;
        }

        /// <summary>
        /// Creates a new ScaledVersion Loader from a spawned CelestialBody.
        /// </summary>
        public ScaledVersionLoader(CelestialBody body)
        {
            // Is this a spawned body?
            if (body.scaledBody == null || Injector.IsInPrefab)
            {
                throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
            }

            // Get the scaled version object
            Value = body;
            Coronas = Value.scaledBody.GetComponentsInChildren<SunCoronas>(true).Select(c => new CoronaLoader(c))
                .ToList();
            if (!Coronas.Any())
            {
                Coronas = null;
            }

            if (Type != ScaledMaterialType.Star)
            {
                ScaledSpaceFader fader = Value.scaledBody.GetComponent<ScaledSpaceFader>();
                if (!(fader is SharedScaledSpaceFader))
                {
                    Utility.CopyObjectFields(fader, Value.scaledBody.AddComponent<SharedScaledSpaceFader>());
                    Object.Destroy(fader);
                }
            }
            else
            {
                SunShaderController controller = Value.scaledBody.GetComponent<SunShaderController>();
                if (controller != null && !(controller is SharedSunShaderController))
                {
                    Utility.CopyObjectFields(controller, Value.scaledBody.AddComponent<SharedSunShaderController>());
                    Object.Destroy(controller);
                }
            }

            if (Options != null)
            {
                return;
            }

            Options = new PlanetTextureExporter.TextureOptions();
            if (body.pqsController == null)
            {
                return;
            }

            Options.Resolution = body.pqsController.mapFilesize;
            Options.TextureDelta = body.pqsController.mapMaxHeight;
        }
    }
}