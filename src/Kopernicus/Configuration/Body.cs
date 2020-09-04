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
using System.IO;
using System.Linq;
using Kopernicus.Components;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Constants;
using Kopernicus.OnDemand;
using Kopernicus.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kopernicus.Configuration
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    [SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
    public class Body : IParserEventSubscriber
    {
        // Path of the ScaledSpaceCache (will eventually not matter much)
        public const String SCALED_SPACE_CACHE_DIRECTORY = "Kopernicus/Cache";

        // Body we are trying to edit
        public PSystemBody GeneratedBody { get; set; }

        // The CelestialBody component of the body
        private CelestialBody _celestialBody;

        public CelestialBody CelestialBody
        {
            get { return Injector.IsInPrefab ? GeneratedBody.celestialBody : _celestialBody; }
            set
            {
                if (!Injector.IsInPrefab)
                {
                    _celestialBody = value;
                }
            }
        }

        // Name of this body
        [PreApply]
        [ParserTarget("name", Optional = false)]
        [KittopiaHideOption]
        public String Name { get; set; }

        [ParserTarget("cacheFile")]
        public String CacheFile
        {
            get { return CelestialBody.Get<String>("cacheFile", null); }
            set { CelestialBody.Set("cacheFile", value); }
        }

        [ParserTarget("barycenter")]
        [KittopiaHideOption]
        public NumericParser<Boolean> Barycenter
        {
            get { return CelestialBody.Get("barycenter", false); }
            set
            {
                CelestialBody.Set("barycenter", value.Value);

                // Reuse the selectable option, to reduce the amount of boilerplate code we need.
                if (value.Value)
                {
                    CelestialBody.Set("selectable", !value.Value);
                }
            }
        }

        [ParserTarget("cbNameLater")]
        public String CbNameLater
        {
            get
            {
                NameChanger changer = CelestialBody.GetComponent<NameChanger>();
                return changer ? changer.newName : null;
            }
            set
            {
                // Change the displayName
                CelestialBody.bodyDisplayName = CelestialBody.bodyAdjectiveDisplayName = value;

                // Set the NameChanger component
                NameChanger changer = CelestialBody.gameObject.AddOrGetComponent<NameChanger>();
                changer.oldName = CelestialBody.bodyName;
                changer.newName = value;

                // Update the name
                if (!Injector.IsInPrefab)
                {
                    changer.Start();
                }
            }
        }

        // The main UBI (Unique Body Identifier) that is assigned to the body
        [ParserTarget("identifier")]
        public String Identifier
        {
            get { return CelestialBody.Get<String>("identifier", null); }
            set { CelestialBody.Set("identifier", value); }
        }

        // All other UBIs the body implements
        [ParserTargetCollection("self", Key = "implements", AllowMerge = false,
            NameSignificance = NameSignificance.Key)]
        public List<StringCollectionParser> Implements
        {
            get
            {
                return new List<StringCollectionParser>
                    {new StringCollectionParser(CelestialBody.Get("implements", new List<String>()))};
            }
            set { CelestialBody.Set("implements", value.SelectMany(v => v.Value).ToList()); }
        }

        // Finalize the orbit of the body?
        [ParserTarget("finalizeOrbit")]
        public NumericParser<Boolean> FinalizeOrbit
        {
            get { return CelestialBody.Get("finalizeBody", false); }
            set { CelestialBody.Set("finalizeBody", value.Value); }
        }

        // Whether this body should be taken into account for the main menu body stuff
        [ParserTarget("randomMainMenuBody")]
        public NumericParser<Boolean> RandomMainMenuBody
        {
            get { return Templates.RandomMainMenuBodies.Contains(Name); }
            set
            {
                if (value)
                {
                    Templates.RandomMainMenuBodies.Add(Name);
                }
            }
        }

        // Describes how often contracts should be generated for a body
        [ParserTarget("contractWeight")]
        public NumericParser<Int32> ContractWeight
        {
            get { return CelestialBody.Get("contractWeight", 30); }
            set { CelestialBody.Set("contractWeight", value.Value); }
        }

        // Template property of a body - responsible for generating a PSystemBody from an existing one
        [PreApply]
        [ParserTarget("Template")]
        [KittopiaHideOption]
        public TemplateLoader Template { get; set; }

        // Celestial body properties (description, mass, etc.)
        [ParserTarget("Properties", AllowMerge = true)]
        [KittopiaUntouchable]
        public PropertiesLoader Properties { get; set; }

        // Wrapper around KSP's Orbit class for editing/loading
        [ParserTarget("Orbit", AllowMerge = true)]
        [KittopiaUntouchable]
        public OrbitLoader Orbit { get; set; }

        // Wrapper around the settings for the world's scaled version
        [ParserTarget("ScaledVersion", AllowMerge = true)]
        [KittopiaUntouchable]
        public ScaledVersionLoader ScaledVersion { get; set; }

        // Wrapper around the settings for the world's atmosphere
        [ParserTarget("Atmosphere", AllowMerge = true)]
        public AtmosphereLoader Atmosphere { get; set; }

        // Wrapper around the settings for the PQS
        [ParserTarget("PQS", AllowMerge = true)]
        public PQSLoader Pqs { get; set; }

        // Wrapper around the settings for the Ocean
        [ParserTarget("Ocean", AllowMerge = true)]
        public OceanLoader Ocean { get; set; }

        // Wrapper around Ring class for editing/loading
        [ParserTargetCollection("Rings", AllowMerge = true)]
        public List<RingLoader> Rings { get; set; }

        // Wrapper around Particle class for editing/loading
        [ParserTargetCollection("Particles", AllowMerge = true)]
        public List<ParticleLoader> Particles { get; set; }

        [ParserTargetCollection("HazardousBody", AllowMerge = true)]
        public List<HazardousBodyLoader> HazardousBody { get; set; }

        // Wrapper around the settings for the SpaceCenter
        [ParserTarget("SpaceCenter", AllowMerge = true)]
        [KittopiaUntouchable]
        public SpaceCenterLoader SpaceCenter { get; set; }

        // Wrapper around DebugMode settings
        [ParserTarget("Debug")]
        [KittopiaUntouchable]
        public DebugLoader Debug { get; set; }

        // Post spawn orbit patcher
        [ParserTarget("PostSpawnOrbit")]
        [KittopiaHideOption]
        public ConfigNode PostSpawnOrbit
        {
            get { return CelestialBody.Get<ConfigNode>("orbitPatches", null); }
            set { CelestialBody.Set("orbitPatches", value); }
        }

        [KittopiaAction("Export Body")]
        [KittopiaDescription("Exports the body as a Kopernicus config file.")]
        public void ExportConfig()
        {
            ConfigNode config = PlanetConfigExporter.CreateConfig(this);
            ConfigNode kopernicus = new ConfigNode("@Kopernicus:NEEDS[!Kopernicus]");
            kopernicus.AddNode(config);
            ConfigNode wrapper = new ConfigNode();
            wrapper.AddNode(kopernicus);

            // Save the node
            String dir = "GameData/KittopiaTech/PluginData/" + CelestialBody.transform.name + "/" +
                         DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
            Directory.CreateDirectory(KSPUtil.ApplicationRootPath + dir);
            wrapper.Save(dir + "/" + Name + ".cfg",
                "KittopiaTech - a Kopernicus Visual Editor");
        }

        // Parser Apply Event
        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            if (Template != null && Template.Body)
            {
                // If we have a template, generatedBody *is* the template body
                GeneratedBody = Template.Body;
                // Patch the game object names in the template
                GeneratedBody.name = Name;
                GeneratedBody.celestialBody.bodyName = Name;
                GeneratedBody.celestialBody.transform.name = Name;
                GeneratedBody.scaledVersion.name = Name;
                if (GeneratedBody.pqsVersion != null)
                {
                    GeneratedBody.pqsVersion.name = Name;
                    GeneratedBody.pqsVersion.gameObject.name = Name;
                    GeneratedBody.pqsVersion.transform.name = Name;
                    foreach (PQS p in GeneratedBody.pqsVersion.GetComponentsInChildren<PQS>(true))
                    {
                        p.name = p.name.Replace(Template.Body.celestialBody.bodyName, Name);
                    }
                    GeneratedBody.celestialBody.pqsController = GeneratedBody.pqsVersion;
                }
                // If we've changed the name, reset use_The_InName
                if (GeneratedBody.name != Template.OriginalBody.celestialBody.bodyName)
                {
                    GeneratedBody.celestialBody.bodyDisplayName = GeneratedBody.celestialBody.bodyAdjectiveDisplayName = GeneratedBody.celestialBody.bodyName;
                }
#if KSP_VERSION_1_10_1
                if ((Template.OriginalBody.scaledVersion.name.Equals("Jool")) && (Versioning.version_minor > 9))
                {
                    if ((!Name.Equals("Jool")) || (Name.Equals("Jool") && (Template.Body.celestialBody.Radius > 6000000))) // This is a Jool-clone, or resized Jool.  We have to handle it special.
                    {
                        //Remove Gas Giant shaders for compatability
                        GasGiantMaterialControls GGMC = GeneratedBody.scaledVersion.GetComponent<GasGiantMaterialControls>();
                        MaterialBasedOnGraphicsSetting MBOGS = GeneratedBody.scaledVersion.GetComponent<MaterialBasedOnGraphicsSetting>();
                        GameObject.DestroyImmediate(GGMC);
                        GameObject.DestroyImmediate(MBOGS);
                    }
                }
#endif
                // Create accessors
                Debug = new DebugLoader();
                ScaledVersion = new ScaledVersionLoader();
            }
            // Otherwise we have to generate all the things for this body
            else
            {
                // Create the PSystemBody object
                GameObject generatedBodyGameObject = new GameObject(Name);
                generatedBodyGameObject.transform.parent = Utility.Deactivator;
                GeneratedBody = generatedBodyGameObject.AddComponent<PSystemBody>();
                GeneratedBody.flightGlobalsIndex = 0;

                // Create the celestial body
                GameObject generatedBodyProperties = new GameObject(Name);
                generatedBodyProperties.transform.parent = generatedBodyGameObject.transform;
                GeneratedBody.celestialBody = generatedBodyProperties.AddComponent<CelestialBody>();
                GeneratedBody.celestialBody.progressTree = null;

                // Sensible defaults 
                GeneratedBody.celestialBody.bodyName = Name;
                GeneratedBody.celestialBody.bodyDisplayName = GeneratedBody.celestialBody.bodyAdjectiveDisplayName = Name;
                GeneratedBody.celestialBody.atmosphere = false;
                GeneratedBody.celestialBody.ocean = false;

                // Create the scaled version
                GeneratedBody.scaledVersion = new GameObject(Name) {layer = GameLayers.SCALED_SPACE};
                GeneratedBody.scaledVersion.transform.parent = Utility.Deactivator;
                // Create accessors
                Debug = new DebugLoader();
                ScaledVersion = new ScaledVersionLoader();
                //Fix normals for gasgiants and newbodies (if needed, ignore the weird trycatch, it works)
                ScaledSpaceOnDemand onDemand = GeneratedBody.celestialBody.scaledBody.AddOrGetComponent<ScaledSpaceOnDemand>();
                try
                {
                    if (onDemand.normals.Length < 1)
                    {
                        onDemand.normals = "Kopernicus/Textures/generic_nm.dds";
                    }
                }
                catch
                {
                    onDemand.normals = "Kopernicus/Textures/generic_nm.dds";
                }
            }
            // Event
            Events.OnBodyApply.Fire(this, node);
        } // ^^^ Everything up there needs cleanup, highly redundant code.

        // Parser Post Apply Event
        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
            // Create a barycenter
            if (Barycenter.Value)
            {
                CreateBarycenter();
            }

            // Add an OnDemand CBMap trigger
            CelestialBody.gameObject.AddComponent<CBMapTrigger>();

            // Loads external parser targets
            Parser.LoadParserTargetsExternal(node, "Kopernicus", "Kopernicus");

            // Post gen celestial body
            Utility.DumpObjectFields(GeneratedBody.celestialBody, " Celestial Body ");

            // Events
            Events.OnBodyPostApply.Fire(this, node);

            // We need to generate new scaled space meshes if 
            //   a) we are using a template and we've change the radius of body
            //   b) we aren't using a template
            //   c) debug mode is active
            if (ScaledVersion.DeferMesh ||
                (Template == null || !(Math.Abs(Template.Radius - GeneratedBody.celestialBody.Radius) > 1.0)) &&
                Template != null && !Debug.Update)
            {
                return;
            }
            ScaledVersion.RebuildScaledSpace();
            Events.OnBodyGenerateScaledSpace.Fire(this, node);
        }

        [KittopiaAction("Convert Body to Barycenter")]
        public void CreateBarycenter()
        {
            // Set the value accordingly
            Barycenter = true;

            // Nuke the PQS
            if (GeneratedBody.pqsVersion != null)
            {
                GeneratedBody.pqsVersion.transform.parent = null;
                Object.Destroy(GeneratedBody.pqsVersion);
                GeneratedBody.pqsVersion = null;
            }

            // Stop ScaledSpace Cache
            ScaledVersion.DeferMesh = true;
            CelestialBody.scaledBody.SetActive(false);
        }

        /// <summary>
        /// Creates a new Body from the Injector context.
        /// </summary>
        public Body()
        {
            Rings = new List<RingLoader>();
            Particles = new List<ParticleLoader>();
        }

        /// <summary>
        /// Creates a new Body from a spawned CelestialBody.
        /// </summary>
        public Body(CelestialBody celestialBody)
        {
            CelestialBody = celestialBody;
            Name = celestialBody.transform.name;

            // Create the accessors
            Properties = new PropertiesLoader(celestialBody);
            if (celestialBody.orbitDriver != null)
            {
                Orbit = new OrbitLoader(celestialBody);
            }

            ScaledVersion = new ScaledVersionLoader(celestialBody);
            if (celestialBody.atmosphere)
            {
                Atmosphere = new AtmosphereLoader(celestialBody);
            }

            if (celestialBody.pqsController != null)
            {
                Pqs = new PQSLoader(celestialBody);
                if (celestialBody.pqsController.GetComponentsInChildren<PQS>(true)
                    .Any(p => p.name.EndsWith("Ocean")))
                {
                    Ocean = new OceanLoader(celestialBody);
                }
            }

            Rings = new List<RingLoader>();
            foreach (Ring ring in celestialBody.scaledBody.GetComponentsInChildren<Ring>(true))
            {
                Rings.Add(new RingLoader(ring));
            }

            Particles = new List<ParticleLoader>();
            foreach (PlanetParticleEmitter particle in celestialBody.scaledBody
                .GetComponentsInChildren<PlanetParticleEmitter>(true))
            {
                Particles.Add(new ParticleLoader(particle));
            }

            HazardousBody = new List<HazardousBodyLoader>();
            foreach (HazardousBody body in celestialBody.GetComponents<HazardousBody>())
            {
                HazardousBody.Add(new HazardousBodyLoader(body));
            }

            if (celestialBody.isHomeWorld)
            {
                SpaceCenter = new SpaceCenterLoader(celestialBody);
            }

            Debug = new DebugLoader(celestialBody);
        }
    }
}
