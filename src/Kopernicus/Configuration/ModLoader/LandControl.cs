﻿/**
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
using System.Reflection;
using Kopernicus.Components.MaterialWrapper;
using Kopernicus.Components.ModularComponentSystem;
using Kopernicus.Components.ModularScatter;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Enumerations;
using Kopernicus.Configuration.MaterialLoader;
using Kopernicus.Configuration.Parsing;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class LandControl : ModLoader<PQSLandControl>
    {
        // Loader for a Ground Scatter
        [RequireConfigType(ConfigType.Node)]
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public class LandClassScatterLoader : IPatchable, ITypeParser<PQSLandControl.LandClassScatter>
        {
            // The value we are editing
            public PQSLandControl.LandClassScatter Value { get; set; }
            public ModularScatter Scatter { get; set; }

            /// <summary>
            /// Returns the currently edited PQS
            /// </summary>
            protected PQS PqsVersion
            {
                get
                {
                    try
                    {
                        return Parser.GetState<PQS>("Kopernicus:pqsVersion");
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            // Scatter name
            [PreApply]
            [ParserTarget("name")]
            public String name
            {
                get { return Value.scatterName; }
                set { Value.scatterName = value; }
            }

            // Should we delete the Scatter?
            [ParserTarget("delete")]
            [KittopiaHideOption]
            [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
            public NumericParser<Boolean> Delete = false;

            [PreApply]
            [ParserTarget("materialType")]
            public EnumParser<ScatterMaterialType> Type
            {
                get
                {
                    if (NormalDiffuse.UsesSameShader(Value.material))
                    {
                        return ScatterMaterialType.Diffuse;
                    }
                    if (NormalBumped.UsesSameShader(Value.material))
                    {
                        return ScatterMaterialType.BumpedDiffuse;
                    }
                    if (NormalDiffuseDetail.UsesSameShader(Value.material))
                    {
                        return ScatterMaterialType.DiffuseDetail;
                    }
                    if (DiffuseWrap.UsesSameShader(Value.material))
                    {
                        return ScatterMaterialType.DiffuseWrapped;
                    }
                    if (AlphaTestDiffuse.UsesSameShader(Value.material))
                    {
                        return ScatterMaterialType.CutoutDiffuse;
                    }
                    if (AerialTransCutout.UsesSameShader(Value.material))
                    {
                        return ScatterMaterialType.AerialCutout;
                    }
                    if (Standard.UsesSameShader(Value.material))
                    {
                        return ScatterMaterialType.Standard;
                    }
                    if (StandardSpecular.UsesSameShader(Value.material))
                    {
                        return ScatterMaterialType.StandardSpecular;
                    }
                    if (KSPBumped.UsesSameShader(Value.material))
                    {
                        return ScatterMaterialType.KSPBumped;
                    }
                    if (KSPBumpedSpecular.UsesSameShader(Value.material))
                    {
                        return ScatterMaterialType.KSPBumpedSpecular;
                    }

                    throw new Exception("The shader '" + Value.material.shader.name + "' is not supported.");
                }
                set
                {
                    Boolean isDiffuse = NormalDiffuse.UsesSameShader(Value.material);
                    Boolean isBumped = NormalBumped.UsesSameShader(Value.material);
                    Boolean isDetail = NormalDiffuseDetail.UsesSameShader(Value.material);
                    Boolean isWrapped = DiffuseWrap.UsesSameShader(Value.material);
                    Boolean isCutout = AlphaTestDiffuse.UsesSameShader(Value.material);
                    Boolean isAerial = AerialTransCutout.UsesSameShader(Value.material);
                    Boolean isStandard = Standard.UsesSameShader(Value.material);
                    Boolean isStandardSpecular = StandardSpecular.UsesSameShader(Value.material);
                    Boolean isKspBumped = KSPBumped.UsesSameShader(Value.material);
                    Boolean isKspBumpedSpecular = KSPBumpedSpecular.UsesSameShader(Value.material);

                    switch (value.Value)
                    {
                        case ScatterMaterialType.Diffuse when !isDiffuse:
                            Value.material = new NormalDiffuseLoader();
                            break;
                        case ScatterMaterialType.BumpedDiffuse when !isBumped:
                            Value.material = new NormalBumpedLoader();
                            break;
                        case ScatterMaterialType.DiffuseDetail when !isDetail:
                            Value.material = new NormalDiffuseDetailLoader();
                            break;
                        case ScatterMaterialType.DiffuseWrapped when !isWrapped:
                            Value.material = new DiffuseWrapLoader();
                            break;
                        case ScatterMaterialType.CutoutDiffuse when !isCutout:
                            Value.material = new AlphaTestDiffuseLoader();
                            break;
                        case ScatterMaterialType.AerialCutout when !isAerial:
                            Value.material = new AerialTransCutoutLoader();
                            break;
                        case ScatterMaterialType.Standard when !isStandard:
                            Value.material = new StandardLoader();
                            break;
                        case ScatterMaterialType.StandardSpecular when !isStandardSpecular:
                            Value.material = new StandardSpecularLoader();
                            break;
                        case ScatterMaterialType.KSPBumped when !isKspBumped:
                            Value.material = new KSPBumpedLoader();
                            break;
                        case ScatterMaterialType.KSPBumpedSpecular when !isKspBumpedSpecular:
                            Value.material = new KSPBumpedSpecularLoader();
                            break;
                        default:
                            return;
                    }
                }
            }

            // Custom scatter material
            [ParserTarget("Material", AllowMerge = true)]
            public Material Material
            {
                get
                {
                    Boolean isDiffuse = Value.material is NormalDiffuseLoader;
                    Boolean isBumped = Value.material is NormalBumpedLoader;
                    Boolean isDetail = Value.material is NormalDiffuseDetailLoader;
                    Boolean isWrapped = Value.material is DiffuseWrapLoader;
                    Boolean isCutout = Value.material is AlphaTestDiffuseLoader;
                    Boolean isAerial = Value.material is AerialTransCutoutLoader;
                    Boolean isStandard = Value.material is StandardLoader;
                    Boolean isStandardSpecular = Value.material is StandardSpecularLoader;
                    Boolean isKspBumped = Value.material is KSPBumpedLoader;
                    Boolean isKspBumpedSpecular = Value.material is KSPBumpedSpecularLoader;

                    switch (Type.Value)
                    {
                        case ScatterMaterialType.Diffuse when !isDiffuse:
                            Value.material = new NormalDiffuseLoader(Value.material);
                            goto default;
                        case ScatterMaterialType.BumpedDiffuse when !isBumped:
                            Value.material = new NormalBumpedLoader(Value.material);
                            goto default;
                        case ScatterMaterialType.DiffuseDetail when !isDetail:
                            Value.material = new NormalDiffuseDetailLoader(Value.material);
                            goto default;
                        case ScatterMaterialType.DiffuseWrapped when !isWrapped:
                            Value.material = new DiffuseWrapLoader(Value.material);
                            goto default;
                        case ScatterMaterialType.CutoutDiffuse when !isCutout:
                            Value.material = new AlphaTestDiffuseLoader(Value.material);
                            goto default;
                        case ScatterMaterialType.AerialCutout when !isAerial:
                            Value.material = new AerialTransCutoutLoader(Value.material);
                            goto default;
                        case ScatterMaterialType.Standard when !isStandard:
                            Value.material = new StandardLoader(Value.material);
                            goto default;
                        case ScatterMaterialType.StandardSpecular when !isStandardSpecular:
                            Value.material = new StandardSpecularLoader(Value.material);
                            goto default;
                        case ScatterMaterialType.KSPBumped when !isKspBumped:
                            Value.material = new KSPBumpedLoader(Value.material);
                            goto default;
                        case ScatterMaterialType.KSPBumpedSpecular when !isKspBumpedSpecular:
                            Value.material = new KSPBumpedSpecularLoader(Value.material);
                            goto default;
                        default:
                            return Value.material;
                    }
                }
                set
                {
                    Boolean isDiffuse = value is NormalDiffuseLoader;
                    Boolean isBumped = value is NormalBumpedLoader;
                    Boolean isDetail = value is NormalDiffuseDetailLoader;
                    Boolean isWrapped = value is DiffuseWrapLoader;
                    Boolean isCutout = value is AlphaTestDiffuseLoader;
                    Boolean isAerial = value is AerialTransCutoutLoader;
                    Boolean isStandard = value is StandardLoader;
                    Boolean isStandardSpecular = value is StandardSpecularLoader;
                    Boolean isKspBumped = Value.material is KSPBumpedLoader;
                    Boolean isKspBumpedSpecular = Value.material is KSPBumpedSpecularLoader;

                    switch (Type.Value)
                    {
                        case ScatterMaterialType.Diffuse when !isDiffuse:
                            Value.material = new NormalDiffuseLoader(value);
                            break;
                        case ScatterMaterialType.BumpedDiffuse when !isBumped:
                            Value.material = new NormalBumpedLoader(value);
                            break;
                        case ScatterMaterialType.DiffuseDetail when !isDetail:
                            Value.material = new NormalDiffuseDetailLoader(value);
                            break;
                        case ScatterMaterialType.DiffuseWrapped when !isWrapped:
                            Value.material = new DiffuseWrapLoader(value);
                            break;
                        case ScatterMaterialType.CutoutDiffuse when !isCutout:
                            Value.material = new AlphaTestDiffuseLoader(value);
                            break;
                        case ScatterMaterialType.AerialCutout when !isAerial:
                            Value.material = new AerialTransCutoutLoader(value);
                            break;
                        case ScatterMaterialType.Standard when !isStandard:
                            Value.material = new StandardLoader(value);
                            break;
                        case ScatterMaterialType.StandardSpecular when !isStandardSpecular:
                            Value.material = new StandardSpecularLoader(value);
                            break;
                        case ScatterMaterialType.KSPBumped when !isKspBumped:
                            Value.material = new KSPBumpedLoader(value);
                            break;
                        case ScatterMaterialType.KSPBumpedSpecular when !isKspBumpedSpecular:
                            Value.material = new KSPBumpedSpecularLoader(value);
                            break;
                        default:
                            Value.material = value;
                            break;
                    }
                }
            }

            // Stock material
            [ParserTarget("material", AllowMerge = true)]
            public StockMaterialParser StockMaterial
            {
                get { return Material; }
                set { Material = value; }
            }

            // The biome list of the landclass
            [ParserTarget("allowedBiomes")]
            public StringCollectionParser AllowedBiomes
            {
                get { return Scatter.allowedBiomes; }
                set { Scatter.allowedBiomes = value; }
            }

            // lethalRadius, a per scatter lethal kill radius.  Zero means disable.
            [ParserTarget("lethalRadius")]
            public NumericParser<Int32> LethalRadius
            {
                get { return Scatter.lethalRadius; }
                set { Scatter.lethalRadius = value; }
            }

            // lethalRadiusMsg, a message for when a kerbal dies from lethalRadius.  Empty string means disabled.
            [ParserTarget("lethalRadiusMsg")]
            public String LethalRadiusMsg
            {
                get { return Scatter.lethalRadiusMsg; }
                set { Scatter.lethalRadiusMsg = value; }
            }

            // lethalRadiusWarnMsg, a message for when a kerbal comes close to a lethal radius (within 200% of the killzone). Empty string means disabled.
            [ParserTarget("lethalRadiusWarnMsg")]
            public String LethalRadiusWarnMsg
            {
                get { return Scatter.lethalRadiusWarnMsg; }
                set { Scatter.lethalRadiusWarnMsg = value; }
            }

            // lethalRadiusAntiSpamMult is a user definable parameter to say how many seconds the delay should do to avoid "spamming"
            // lethalRadiusAntiSpamMult is tied to the physics framerate so extremely low fps may affect it.  Zero means disable.
            [ParserTarget("lethalRadiusAntiSpamMult")]
            public NumericParser<Int32> LethalRadiusAntiSpamMult
            {
                get { return Scatter.antiSpamCounterMult; }
                set { Scatter.antiSpamCounterMult = value; }
            }

            // The mesh
            [ParserTarget("mesh")]
            public MeshParser BaseMesh
            {
                get { return Scatter.baseMesh; }
                set { Scatter.baseMesh = value; }
            }

            [ParserTargetCollection("Meshes", AllowMerge = true)]
            public List<MeshParser> Meshes
            {
                get { return Scatter.meshes.Select(m => (MeshParser)m).ToList(); }
                set { Scatter.meshes = value.Select(m => m.Value).ToList(); }
            }

            // castShadows
            [ParserTarget("castShadows")]
            public NumericParser<Boolean> CastShadows
            {
                get { return Value.castShadows; }
                set { Value.castShadows = value; }
            }

            // densityFactor
            [ParserTarget("densityFactor")]
            public NumericParser<Double> DensityFactor
            {
                get { return Value.densityFactor; }
                set { Value.densityFactor = value; }
            }

            // maxCache
            [ParserTarget("maxCache")]
            public NumericParser<Int32> MaxCache
            {
                get { return Value.maxCache; }
                set { Value.maxCache = value; }
            }

            // maxCacheDelta
            [ParserTarget("maxCacheDelta")]
            public NumericParser<Int32> MaxCacheDelta
            {
                get { return Value.maxCacheDelta; }
                set { Value.maxCacheDelta = value; }
            }

            // maxLevelOffset
            [ParserTarget("maxLevelOffset")]
            public NumericParser<Int32> MaxLevelOffset
            {
                get { return Value.maxLevelOffset; }
                set { Value.maxLevelOffset = value; }
            }

            // maxScale
            [ParserTarget("maxScale")]
            public NumericParser<Single> MaxScale
            {
                get { return Value.maxScale; }
                set { Value.maxScale = value; }
            }

            // maxScatter
            [ParserTarget("maxScatter")]
            public NumericParser<Int32> MaxScatter
            {
                get { return Value.maxScatter; }
                set { Value.maxScatter = value; }
            }

            // maxSpeed
            [ParserTarget("maxSpeed")]
            public NumericParser<Double> MaxSpeed
            {
                get { return Value.maxSpeed; }
                set { Value.maxSpeed = value; }
            }

            // minScale
            [ParserTarget("minScale")]
            public NumericParser<Single> MinScale
            {
                get { return Value.minScale; }
                set { Value.minScale = value; }
            }

            // recieveShadows
            [ParserTarget("recieveShadows")]
            public NumericParser<Boolean> RecieveShadows
            {
                get { return Value.recieveShadows; }
                set { Value.recieveShadows = value; }
            }

            // The value we are editing
            // Scatter seed
            [ParserTarget("seed")]
            public NumericParser<Int32> Seed
            {
                get { return Value.seed; }
                set { Value.seed = value; }
            }

            // verticalOffset
            [ParserTarget("verticalOffset")]
            public NumericParser<Single> VerticalOffset
            {
                get { return Value.verticalOffset; }
                set { Value.verticalOffset = value; }
            }

            [ParserTarget("instancing")]
            public NumericParser<Boolean> Instancing
            {
                get { return Value.material.enableInstancing; }
                set { Value.material.enableInstancing = value; }
            }

            [ParserTarget("rotation")]
            public NumericCollectionParser<Single> Rotation
            {
                get { return Scatter.rotation; }
                set { Scatter.rotation = value; }
            }

            [ParserTarget("useBetterDensity")]
            public NumericParser<Boolean> UseBetterDensity
            {
                get { return Scatter.useBetterDensity; }
                set { Scatter.useBetterDensity = value; }
            }

            [ParserTarget("spawnChance")]
            public NumericParser<Single> SpawnChance
            {
                get { return Scatter.spawnChance; }
                set { Scatter.spawnChance = value; }
            }

            [ParserTarget("ignoreDensityGameSetting")]
            public NumericParser<Boolean> IgnoreDensityGameSetting
            {
                get { return Scatter.ignoreDensityGameSetting; }
                set { Scatter.ignoreDensityGameSetting = value; }
            }

            [ParserTargetCollection("Components", AllowMerge = true, NameSignificance = NameSignificance.Type)]
            public CallbackList<ComponentLoader<ModularScatter>> Components { get; set; }

            // Default Constructor
            [KittopiaConstructor(KittopiaConstructor.ParameterType.Empty)]
            public LandClassScatterLoader()
            {
                // Initialize default parameters
                Value = new PQSLandControl.LandClassScatter
                {
                    maxCache = 512, maxCacheDelta = 32, maxSpeed = 1000
                };

                // Get the Scatter-Parent
                GameObject scatterParent = new GameObject("Scatter");
                scatterParent.transform.parent = Utility.Deactivator;

                // Add the scatter module
                Scatter = scatterParent.AddOrGetComponent<ModularScatter>();
                Scatter.scatter = Value;

                // Create the Component callback
                Components = new CallbackList<ComponentLoader<ModularScatter>>(e =>
                {
                    Scatter.Components = Components.Select(c => c.Value).ToList();
                });
            }

            // Runtime constructor
            public LandClassScatterLoader(PQSLandControl.LandClassScatter value)
            {
                Value = value;

                // Get the Scatter-Parent
                GameObject scatterParent = typeof(PQSLandControl.LandClassScatter)
                    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(f => f.FieldType == typeof(GameObject))?.GetValue(Value) as GameObject;

                // If the GameObject is null, create one
                if (scatterParent == null)
                {
                    scatterParent = new GameObject("Scatter");
                    scatterParent.transform.parent = Utility.Deactivator;
                }

                // Add the scatter module
                Scatter = scatterParent.AddOrGetComponent<ModularScatter>();
                Scatter.scatter = Value;
                if (Value.baseMesh && Value.baseMesh.name != "Kopernicus-CubeDummy")
                {
                    Scatter.baseMesh = Value.baseMesh;
                }

                // Create the Component callback
                Components = new CallbackList<ComponentLoader<ModularScatter>>(e =>
                {
                    Scatter.Components = Components.Select(c => c.Value).ToList();
                });

                // Load existing Modules
                for (Int32 i = 0; i < Scatter.Components.Count; i++)
                {
                    Type componentType = Scatter.Components[i].GetType();
                    Type componentLoaderType =
                        typeof(ComponentLoader<,>).MakeGenericType(typeof(ModularScatter), componentType);

                    for (Int32 j = 0; j < Parser.ModTypes.Count; j++)
                    {
                        if (!componentLoaderType.IsAssignableFrom(Parser.ModTypes[j]))
                        {
                            continue;
                        }

                        // We found our loader type
                        ComponentLoader<ModularScatter> loader =
                            (ComponentLoader<ModularScatter>)Activator.CreateInstance(Parser.ModTypes[j]);
                        loader.Create(Scatter.Components[i]);
                        Components.Add(loader);
                    }
                }
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator PQSLandControl.LandClassScatter(LandClassScatterLoader parser)
            {
                return parser.Value;
            }

            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator LandClassScatterLoader(PQSLandControl.LandClassScatter value)
            {
                return new LandClassScatterLoader(value);
            }
        }

        // Loader for the Amount of a Scatter on a body
        [RequireConfigType(ConfigType.Node)]
        public class LandClassScatterAmountLoader : IPatchable, ITypeParser<PQSLandControl.LandClassScatterAmount>
        {
            // The value we are editing
            public PQSLandControl.LandClassScatterAmount Value { get; set; }

            // Should we delete the ScatterAmount?
            [ParserTarget("delete")]
            [KittopiaHideOption]
            [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
            public NumericParser<Boolean> Delete = false;

            // density
            [ParserTarget("density")]
            public NumericParser<Double> Density
            {
                get { return Value.density; }
                set { Value.density = value; }
            }

            // The name of the scatter used
            [ParserTarget("scatterName")]
            public String name
            {
                get { return Value.scatterName; }
                set { Value.scatterName = value; }
            }

            // Default Constructor
            [KittopiaConstructor(KittopiaConstructor.ParameterType.Empty)]
            public LandClassScatterAmountLoader()
            {
                Value = new PQSLandControl.LandClassScatterAmount();
            }

            // Runtime Constructor
            public LandClassScatterAmountLoader(PQSLandControl.LandClassScatterAmount amount)
            {
                Value = amount;
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator PQSLandControl.LandClassScatterAmount(LandClassScatterAmountLoader parser)
            {
                return parser.Value;
            }

            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator LandClassScatterAmountLoader(PQSLandControl.LandClassScatterAmount value)
            {
                return new LandClassScatterAmountLoader(value);
            }
        }

        // Loader for LerpRange
        [RequireConfigType(ConfigType.Node)]
        public class LerpRangeLoader : ITypeParser<PQSLandControl.LerpRange>
        {
            // The value we are editing
            public PQSLandControl.LerpRange Value { get; set; }

            // endEnd
            [ParserTarget("endEnd")]
            public NumericParser<Double> EndEnd
            {
                get { return Value.endEnd; }
                set { Value.endEnd = value; }
            }

            // endStart
            [ParserTarget("endStart")]
            public NumericParser<Double> EndStart
            {
                get { return Value.endStart; }
                set { Value.endStart = value; }
            }

            // startEnd
            [ParserTarget("startEnd")]
            public NumericParser<Double> StartEnd
            {
                get { return Value.startEnd; }
                set { Value.startEnd = value; }
            }

            // startStart
            [ParserTarget("startStart")]
            public NumericParser<Double> StartStart
            {
                get { return Value.startStart; }
                set { Value.startStart = value; }
            }

            // Default constructor
            [KittopiaConstructor(KittopiaConstructor.ParameterType.Empty)]
            public LerpRangeLoader()
            {
                Value = new PQSLandControl.LerpRange();
            }

            // Runtime Constructor
            public LerpRangeLoader(PQSLandControl.LerpRange lerp)
            {
                Value = lerp;
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator PQSLandControl.LerpRange(LerpRangeLoader parser)
            {
                return parser.Value;
            }

            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator LerpRangeLoader(PQSLandControl.LerpRange value)
            {
                return new LerpRangeLoader(value);
            }
        }

        // Loader for LandClass
        [RequireConfigType(ConfigType.Node)]
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public class LandClassLoader : IPatchable, ITypeParser<PQSLandControl.LandClass>
        {
            // The value we are editing
            public PQSLandControl.LandClass Value { get; set; }

            // Should we delete the LandClass?
            [ParserTarget("delete")]
            [KittopiaHideOption]
            [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
            public NumericParser<Boolean> Delete = false;

            // alterApparentHeight
            [ParserTarget("alterApparentHeight")]
            public NumericParser<Single> AlterApparentHeight
            {
                get { return Value.alterApparentHeight; }
                set { Value.alterApparentHeight = value; }
            }

            // alterRealHeight
            [ParserTarget("alterRealHeight")]
            public NumericParser<Double> AlterRealHeight
            {
                get { return Value.alterRealHeight; }
                set { Value.alterRealHeight = value; }
            }

            // altitudeRange
            [ParserTarget("altitudeRange", AllowMerge = true)]
            public LerpRangeLoader AltitudeRange
            {
                get { return Value.altitudeRange; }
                set { Value.altitudeRange = value; }
            }

            // color
            [ParserTarget("color")]
            public ColorParser Color
            {
                get
                {
                    return Value.color;
                }
                set
                {
                    Value.color = value;
                }
            }

            // coverageBlend
            [ParserTarget("coverageBlend")]
            public NumericParser<Single> CoverageBlend
            {
                get { return Value.coverageBlend; }
                set { Value.coverageBlend = value; }
            }

            // coverageFrequency
            [ParserTarget("coverageFrequency")]
            public NumericParser<Single> CoverageFrequency
            {
                get { return Value.coverageFrequency; }
                set
                {
                    Value.coverageFrequency = value;
                    Value.coverageSimplex = new Simplex(Value.coverageSeed, Value.coverageOctaves,
                        Value.coveragePersistance, Value.coverageFrequency);
                }
            }

            // coverageOctaves
            [ParserTarget("coverageOctaves")]
            public NumericParser<Int32> CoverageOctaves
            {
                get { return Value.coverageOctaves; }
                set
                {
                    Value.coverageOctaves = value;
                    Value.coverageSimplex = new Simplex(Value.coverageSeed, Value.coverageOctaves,
                        Value.coveragePersistance, Value.coverageFrequency);
                }
            }

            // coveragePersistance
            [ParserTarget("coveragePersistance")]
            public NumericParser<Single> CoveragePersistance
            {
                get { return Value.coveragePersistance; }
                set
                {
                    Value.coveragePersistance = value;
                    Value.coverageSimplex = new Simplex(Value.coverageSeed, Value.coverageOctaves,
                        Value.coveragePersistance, Value.coverageFrequency);
                }
            }

            // coverageSeed
            [ParserTarget("coverageSeed")]
            public NumericParser<Int32> CoverageSeed
            {
                get { return Value.coverageSeed; }
                set
                {
                    Value.coverageSeed = value;
                    Value.coverageSimplex = new Simplex(Value.coverageSeed, Value.coverageOctaves,
                        Value.coveragePersistance, Value.coverageFrequency);
                }
            }

            // The name of the landclass
            [ParserTarget("name")]
            public String name
            {
                get { return Value.landClassName; }
                set { Value.landClassName = value; }
            }
            // latDelta
            [ParserTarget("latDelta")]
            public NumericParser<Double> LatDelta
            {
                get { return Value.latDelta; }
                set { Value.latDelta = value; }
            }

            // latitudeDouble
            [ParserTarget("latitudeDouble")]
            public NumericParser<Boolean> LatitudeDouble
            {
                get { return Value.latitudeDouble; }
                set { Value.latitudeDouble = value; }
            }

            // latitudeDoubleRange
            [ParserTarget("latitudeDoubleRange", AllowMerge = true)]
            public LerpRangeLoader LatitudeDoubleRange
            {
                get { return new LerpRangeLoader(Value.latitudeDoubleRange); }
                set { Value.latitudeDoubleRange = value.Value; }
            }

            // latitudeRange
            [ParserTarget("latitudeRange", AllowMerge = true)]
            public LerpRangeLoader LatitudeRange
            {
                get { return new LerpRangeLoader(Value.latitudeRange); }
                set { Value.latitudeRange = value.Value; }
            }

            // lonDelta
            [ParserTarget("lonDelta")]
            public NumericParser<Double> LonDelta
            {
                get { return Value.lonDelta; }
                set { Value.lonDelta = value; }
            }

            // longitudeRange
            [ParserTarget("longitudeRange", AllowMerge = true)]
            public LerpRangeLoader LongitudeRange
            {
                get { return new LerpRangeLoader(Value.longitudeRange); }
                set { Value.longitudeRange = value.Value; }
            }

            // minimumRealHeight
            [ParserTarget("minimumRealHeight")]
            public NumericParser<Double> MinimumRealHeight
            {
                get { return Value.minimumRealHeight; }
                set { Value.minimumRealHeight = value; }
            }

            // noiseBlend
            [ParserTarget("noiseBlend")]
            public NumericParser<Single> NoiseBlend
            {
                get { return Value.noiseBlend; }
                set { Value.noiseBlend = value; }
            }

            // noiseColor
            [ParserTarget("noiseColor")]
            public ColorParser NoiseColor
            {
                get
                {
                    return Value.noiseColor;
                }
                set
                {
                    Value.noiseColor = value;
                }
            }

            // noiseFrequency
            [ParserTarget("noiseFrequency")]
            public NumericParser<Single> NoiseFrequency
            {
                get { return Value.noiseFrequency; }
                set
                {
                    Value.noiseFrequency = value;
                    Value.noiseSimplex = new Simplex(Value.noiseSeed, Value.noiseOctaves,
                        Value.noisePersistance, Value.noiseFrequency);
                }
            }

            // noiseOctaves
            [ParserTarget("noiseOctaves")]
            public NumericParser<Int32> NoiseOctaves
            {
                get { return Value.noiseOctaves; }
                set
                {
                    Value.noiseOctaves = value;
                    Value.noiseSimplex = new Simplex(Value.noiseSeed, Value.noiseOctaves,
                        Value.noisePersistance, Value.noiseFrequency);
                }
            }

            // noisePersistance
            [ParserTarget("noisePersistance")]
            public NumericParser<Single> NoisePersistance
            {
                get { return Value.noisePersistance; }
                set
                {
                    Value.noisePersistance = value;
                    Value.noiseSimplex = new Simplex(Value.noiseSeed, Value.noiseOctaves,
                        Value.noisePersistance, Value.noiseFrequency);
                }
            }

            // noiseSeed
            [ParserTarget("noiseSeed")]
            public NumericParser<Int32> NoiseSeed
            {
                get { return Value.noiseSeed; }
                set
                {
                    Value.noiseSeed = value;
                    Value.noiseSimplex = new Simplex(Value.noiseSeed, Value.noiseOctaves,
                        Value.noisePersistance, Value.noiseFrequency);
                }
            }

            // List of scatters used
            [ParserTargetCollection("Scatters", AllowMerge = true)]
            [ParserTargetCollection("scatters", AllowMerge = true)]
            public CallbackList<LandClassScatterAmountLoader> Scatter { get; set; }

            // Default constructor
            [KittopiaConstructor(KittopiaConstructor.ParameterType.Empty)]
            public LandClassLoader()
            {
                Value = new PQSLandControl.LandClass
                {
                    altDelta = 1,
                    color = new Color(0, 0, 0, 0),
                    coverageFrequency = 1,
                    coverageOctaves = 1,
                    coveragePersistance = 1,
                    coverageSeed = 1,
                    landClassName = "Base",
                    latDelta = 1,
                    lonDelta = 1,
                    noiseColor = new Color(0, 0, 0, 0),
                    noiseFrequency = 1,
                    noiseOctaves = 1,
                    noisePersistance = 1,
                    noiseSeed = 1
                };

                // Initialize default parameters

                AltitudeRange = new LerpRangeLoader
                {
                    Value =
                    {
                        endEnd = 1, endStart = 1, startEnd = 0, startStart = 0
                    }
                };
                LatitudeRange = new LerpRangeLoader
                {
                    Value =
                    {
                        endEnd = 1, endStart = 1, startEnd = 0, startStart = 0
                    }
                };
                LatitudeDoubleRange = new LerpRangeLoader
                {
                    Value =
                    {
                        endEnd = 1, endStart = 1, startEnd = 0, startStart = 0
                    }
                };
                LongitudeRange = new LerpRangeLoader
                {
                    Value =
                    {
                        endEnd = 2, endStart = 2, startEnd = -1, startStart = -1
                    }
                };

                // Create the scatter list
                Scatter = new CallbackList<LandClassScatterAmountLoader>(e =>
                {
                    Value.scatter = Scatter.Where(s => !s.Delete).Select(s => s.Value).ToArray();
                });
                Value.scatter = new PQSLandControl.LandClassScatterAmount[0];
            }

            // Runtime constructor
            public LandClassLoader(PQSLandControl.LandClass value)
            {
                Value = value;

                // Initialize default parameters
                if (Value.altitudeRange == null)
                {
                    LerpRangeLoader range = new LerpRangeLoader
                    {
                        Value =
                        {
                            endEnd = 1, endStart = 1, startEnd = 0, startStart = 0
                        }
                    };
                    AltitudeRange = range;
                }

                if (Value.latitudeRange == null)
                {
                    LerpRangeLoader range = new LerpRangeLoader
                    {
                        Value =
                        {
                            endEnd = 1, endStart = 1, startEnd = 0, startStart = 0
                        }
                    };
                    LatitudeRange = range;
                }

                if (Value.latitudeDoubleRange == null)
                {
                    LerpRangeLoader range = new LerpRangeLoader
                    {
                        Value =
                        {
                            endEnd = 1, endStart = 1, startEnd = 0, startStart = 0
                        }
                    };
                    LatitudeDoubleRange = range;
                }

                if (Value.longitudeRange == null)
                {
                    LerpRangeLoader range = new LerpRangeLoader
                    {
                        Value =
                        {
                            endEnd = 2, endStart = 2, startEnd = -1, startStart = -1
                        }
                    };
                    LongitudeRange = range;
                }

                // Create the scatter list
                Scatter = new CallbackList<LandClassScatterAmountLoader>(e =>
                {
                    Value.scatter = Scatter.Where(s => !s.Delete).Select(s => s.Value).ToArray();
                });

                // Load scatters
                if (Value.scatter != null)
                {
                    for (Int32 i = 0; i < Value.scatter.Length; i++)
                    {
                        // Only activate the callback if we are adding the last loader
                        Scatter.Add(new LandClassScatterAmountLoader(Value.scatter[i]),
                            i == Value.scatter.Length - 1);
                    }
                }
                else
                {
                    Value.scatter = new PQSLandControl.LandClassScatterAmount[0];
                }
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator PQSLandControl.LandClass(LandClassLoader parser)
            {
                return parser.Value;
            }

            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator LandClassLoader(PQSLandControl.LandClass value)
            {
                return new LandClassLoader(value);
            }
        }

        // altitudeBlend
        [ParserTarget("altitudeBlend")]
        public NumericParser<Single> AltitudeBlend
        {
            get { return Mod.altitudeBlend; }
            set { Mod.altitudeBlend = value; }
        }

        // altitudeFrequency
        [ParserTarget("altitudeFrequency")]
        public NumericParser<Single> AltitudeFrequency
        {
            get { return Mod.altitudeFrequency; }
            set
            {
                Mod.altitudeFrequency = value;
                Mod.altitudeSimplex = new Simplex(Mod.altitudeSeed, Mod.altitudeOctaves,
                    Mod.altitudePersistance, Mod.altitudeFrequency);
            }
        }

        // altitudeOctaves
        [ParserTarget("altitudeOctaves")]
        public NumericParser<Int32> AltitudeOctaves
        {
            get { return Mod.altitudeOctaves; }
            set
            {
                Mod.altitudeOctaves = value;
                Mod.altitudeSimplex = new Simplex(Mod.altitudeSeed, Mod.altitudeOctaves,
                    Mod.altitudePersistance, Mod.altitudeFrequency);
            }
        }

        // altitudePersistance
        [ParserTarget("altitudePersistance")]
        public NumericParser<Single> AltitudePersistance
        {
            get { return Mod.altitudePersistance; }
            set
            {
                Mod.altitudePersistance = value;
                Mod.altitudeSimplex = new Simplex(Mod.altitudeSeed, Mod.altitudeOctaves,
                    Mod.altitudePersistance, Mod.altitudeFrequency);
            }
        }

        // altitudeSeed
        [ParserTarget("altitudeSeed")]
        public NumericParser<Int32> AltitudeSeed
        {
            get { return Mod.altitudeSeed; }
            set
            {
                Mod.altitudeSeed = value;
                Mod.altitudeSimplex = new Simplex(Mod.altitudeSeed, Mod.altitudeOctaves,
                    Mod.altitudePersistance, Mod.altitudeFrequency);
            }
        }

        // createColors
        [ParserTarget("createColors")]
        public NumericParser<Boolean> CreateColors
        {
            get { return Mod.createColors; }
            set { Mod.createColors = value; }
        }

        // createScatter
        [ParserTarget("createScatter")]
        public NumericParser<Boolean> CreateScatter
        {
            get { return Mod.createScatter; }
            set { Mod.createScatter = value; }
        }

        // heightMap
        [ParserTarget("heightMap")]
        public MapSOParserGreyScale<MapSO> HeightMap
        {
            get { return Mod.heightMap; }
            set { Mod.heightMap = value; }
        }

        // latitudeBlend
        [ParserTarget("latitudeBlend")]
        public NumericParser<Single> LatitudeBlend
        {
            get { return Mod.latitudeBlend; }
            set { Mod.latitudeBlend = value; }
        }

        // latitudeFrequency
        [ParserTarget("latitudeFrequency")]
        public NumericParser<Single> LatitudeFrequency
        {
            get { return Mod.latitudeFrequency; }
            set
            {
                Mod.latitudeFrequency = value;
                Mod.latitudeSimplex = new Simplex(Mod.latitudeSeed, Mod.latitudeOctaves,
                    Mod.latitudePersistance, Mod.latitudeFrequency);
            }
        }

        // latitudeOctaves
        [ParserTarget("latitudeOctaves")]
        public NumericParser<Int32> LatitudeOctaves
        {
            get { return Mod.latitudeOctaves; }
            set
            {
                Mod.latitudeOctaves = value;
                Mod.latitudeSimplex = new Simplex(Mod.latitudeSeed, Mod.latitudeOctaves,
                    Mod.latitudePersistance, Mod.latitudeFrequency);
            }
        }

        // latitudePersistance
        [ParserTarget("latitudePersistance")]
        public NumericParser<Single> LatitudePersistance
        {
            get { return Mod.latitudePersistance; }
            set
            {
                Mod.latitudePersistance = value;
                Mod.latitudeSimplex = new Simplex(Mod.latitudeSeed, Mod.latitudeOctaves,
                    Mod.latitudePersistance, Mod.latitudeFrequency);
            }
        }

        // latitudeSeed
        [ParserTarget("latitudeSeed")]
        public NumericParser<Int32> LatitudeSeed
        {
            get { return Mod.latitudeSeed; }
            set
            {
                Mod.latitudeSeed = value;
                Mod.latitudeSimplex = new Simplex(Mod.latitudeSeed, Mod.latitudeOctaves,
                    Mod.latitudePersistance, Mod.latitudeFrequency);
            }
        }

        // longitudeBlend
        [ParserTarget("longitudeBlend")]
        public NumericParser<Single> LongitudeBlend
        {
            get { return Mod.longitudeBlend; }
            set { Mod.longitudeBlend = value; }
        }

        // longitudeFrequency
        [ParserTarget("longitudeFrequency")]
        public NumericParser<Single> LongitudeFrequency
        {
            get { return Mod.longitudeFrequency; }
            set
            {
                Mod.longitudeFrequency = value;
                Mod.longitudeSimplex = new Simplex(Mod.longitudeSeed, Mod.longitudeOctaves,
                    Mod.longitudePersistance, Mod.longitudeFrequency);
            }
        }

        // longitudeOctaves
        [ParserTarget("longitudeOctaves")]
        public NumericParser<Int32> LongitudeOctaves
        {
            get { return Mod.longitudeOctaves; }
            set
            {
                Mod.longitudeOctaves = value;
                Mod.longitudeSimplex = new Simplex(Mod.longitudeSeed, Mod.longitudeOctaves,
                    Mod.longitudePersistance, Mod.longitudeFrequency);
            }
        }

        // longitudePersistance
        [ParserTarget("longitudePersistance")]
        public NumericParser<Single> LongitudePersistance
        {
            get { return Mod.longitudePersistance; }
            set
            {
                Mod.longitudePersistance = value;
                Mod.longitudeSimplex = new Simplex(Mod.longitudeSeed, Mod.longitudeOctaves,
                    Mod.longitudePersistance, Mod.longitudeFrequency);
            }
        }

        // longitudeSeed
        [ParserTarget("longitudeSeed")]
        public NumericParser<Int32> LongitudeSeed
        {
            get { return Mod.longitudeSeed; }
            set
            {
                Mod.longitudeSeed = value;
                Mod.longitudeSimplex = new Simplex(Mod.longitudeSeed, Mod.longitudeOctaves,
                    Mod.longitudePersistance, Mod.longitudeFrequency);
            }
        }

        // useHeightMap
        [ParserTarget("useHeightMap")]
        public NumericParser<Boolean> UseHeightMap
        {
            get { return Mod.useHeightMap; }
            set { Mod.useHeightMap = value; }
        }

        // vHeightMax
        [ParserTarget("vHeightMax")]
        public NumericParser<Single> VHeightMax
        {
            get { return Mod.vHeightMax; }
            set { Mod.vHeightMax = value; }
        }

        // List of scatters
        [ParserTargetCollection("Scatters", AllowMerge = true)]
        [ParserTargetCollection("scatters", AllowMerge = true)]
        public CallbackList<LandClassScatterLoader> Scatters { get; set; }

        // List of landclasses
        [ParserTargetCollection("LandClasses", AllowMerge = true)]
        [ParserTargetCollection("landClasses", AllowMerge = true)]
        public CallbackList<LandClassLoader> LandClasses { get; set; }

        // Creates the a PQSMod of type T with given PQS
        public override void Create(PQS pqsVersion)
        {
            base.Create(pqsVersion);

            // Initialize default parameters
            Mod.altitudeSeed = 1;
            Mod.altitudeOctaves = 1;
            Mod.altitudePersistance = 1;
            Mod.altitudeFrequency = 1;
            Mod.latitudeSeed = 1;
            Mod.latitudeOctaves = 1;
            Mod.latitudePersistance = 1;
            Mod.latitudeFrequency = 1;
            Mod.longitudeSeed = 1;
            Mod.longitudeOctaves = 1;
            Mod.longitudePersistance = 1;
            Mod.longitudeFrequency = 1;

            // Create the callback list for Scatters
            Scatters = new CallbackList<LandClassScatterLoader>(e =>
            {
                foreach (LandClassScatterLoader loader in Scatters)
                {
                    loader.Scatter.transform.parent = Mod.transform;
                }

                Mod.scatters = Scatters.Where(scatter => !scatter.Delete)
                    .Select(scatter => scatter.Value).ToArray();
            });
            Mod.scatters = new PQSLandControl.LandClassScatter[0];

            // Create the callback list for LandClasses
            LandClasses = new CallbackList<LandClassLoader>(e =>
            {
                // Assign each scatter amount with their corresponding scatter
                foreach (PQSLandControl.LandClassScatterAmount amount in e.Scatter)
                {
                    Int32 i = 0;
                    while (i < Mod.scatters.Length)
                    {
                        if (Mod.scatters[i].scatterName.Equals(amount.scatterName))
                        {
                            break;
                        }

                        i++;
                    }

                    if (i >= Mod.scatters.Length)
                    {
                        continue;
                    }
                    amount.scatterIndex = i;
                    amount.scatter = Mod.scatters[i];
                }

                // Assign the new values
                Mod.landClasses = LandClasses.Where(landClass => !landClass.Delete)
                    .Select(landClass => landClass.Value).ToArray();
            });
            Mod.landClasses = new PQSLandControl.LandClass[0];
        }

        // Grabs a PQSMod of type T from a parameter with a given PQS
        public override void Create(PQSLandControl mod, PQS pqsVersion)
        {
            base.Create(mod, pqsVersion);

            // Create the callback list for Scatters
            Scatters = new CallbackList<LandClassScatterLoader>(e =>
            {
                foreach (LandClassScatterLoader loader in Scatters)
                {
                    loader.Scatter.transform.parent = Mod.transform;
                }

                Mod.scatters = Scatters.Where(scatter => !scatter.Delete)
                    .Select(scatter => scatter.Value).ToArray();
            });

            // Load Scatters
            if (Mod.scatters != null)
            {
                for (Int32 i = 0; i < Mod.scatters.Length; i++)
                {
                    // Only activate the callback if we are adding the last loader
                    Scatters.Add(new LandClassScatterLoader(Mod.scatters[i]), i == Mod.scatters.Length - 1);
                }
            }
            else
            {
                Mod.scatters = new PQSLandControl.LandClassScatter[0];
            }

            // Create the callback list for LandClasses
            LandClasses = new CallbackList<LandClassLoader>(e =>
            {
                // Assign each scatter amount with their corresponding scatter
                foreach (PQSLandControl.LandClassScatterAmount amount in e.Scatter)
                {
                    Int32 i = 0;
                    while (i < Mod.scatters.Length)
                    {
                        if (Mod.scatters[i].scatterName.Equals(amount.scatterName))
                        {
                            break;
                        }

                        i++;
                    }

                    if (i >= Mod.scatters.Length)
                    {
                        continue;
                    }
                    amount.scatterIndex = i;
                    amount.scatter = Mod.scatters[i];
                }

                // Assign the new values
                Mod.landClasses = LandClasses.Where(landClass => !landClass.Delete)
                    .Select(landClass => landClass.Value).ToArray();
            });

            // Load LandClasses
            if (Mod.landClasses != null)
            {
                for (Int32 i = 0; i < Mod.landClasses.Length; i++)
                {
                    // Only activate the callback if we are adding the last loader
                    LandClasses.Add(new LandClassLoader(Mod.landClasses[i]), i == Mod.landClasses.Length - 1);
                }
            }
            else
            {
                Mod.landClasses = new PQSLandControl.LandClass[0];
            }
        }
    }
}
