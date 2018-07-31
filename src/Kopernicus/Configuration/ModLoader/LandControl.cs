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

using Kopernicus.Components;
using Kopernicus.MaterialWrapper;
using System;
using System.Linq;
using System.Reflection;
using Kopernicus.Components.PatchedMods;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class LandControl : ModLoader<PQSLandControlPatched>
            {
                // Loader for a Ground Scatter
                [RequireConfigType(ConfigType.Node)]
                public class LandClassScatterLoader : IPatchable, ITypeParser<PQSLandControl.LandClassScatter>
                {
                    public enum ScatterMaterialType
                    {
                        Diffuse,
                        BumpedDiffuse,
                        DiffuseDetail,
                        DiffuseWrapped,
                        CutoutDiffuse,
                        AerialCutout
                    };
                    
                    // The value we are editing
                    public PQSLandControl.LandClassScatter Value { get; set; }
                    public ModularScatter Scatter { get; set; }

                    /// <summary>
                    /// Returns the currently edited PQS
                    /// </summary>
                    protected PQS pqsVersion
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

                    [PreApply]
                    [ParserTarget("materialType")]
                    public EnumParser<ScatterMaterialType> materialType
                    {
                        get
                        {
                            if (customMaterial == null)
                                return null;
                            if (NormalDiffuse.UsesSameShader(customMaterial))
                                return ScatterMaterialType.Diffuse;
                            if (NormalBumped.UsesSameShader(customMaterial))
                                return ScatterMaterialType.BumpedDiffuse;
                            if (NormalDiffuseDetail.UsesSameShader(customMaterial))
                                return ScatterMaterialType.DiffuseDetail;
                            if (DiffuseWrap.UsesSameShader(customMaterial))
                                return ScatterMaterialType.DiffuseWrapped;
                            if (AlphaTestDiffuse.UsesSameShader(customMaterial))
                                return ScatterMaterialType.CutoutDiffuse;
                            if (AerialTransCutout.UsesSameShader(customMaterial))
                                return ScatterMaterialType.AerialCutout;
                            return null;
                        }
                        set
                        {
                            if (value == ScatterMaterialType.Diffuse)
                                customMaterial = new NormalDiffuseLoader();
                            else if (value == ScatterMaterialType.BumpedDiffuse)
                                customMaterial = new NormalBumpedLoader();
                            else if (value == ScatterMaterialType.DiffuseDetail)
                                customMaterial = new NormalDiffuseDetailLoader();
                            else if (value == ScatterMaterialType.DiffuseWrapped)
                                customMaterial = new DiffuseWrapLoader();
                            else if (value == ScatterMaterialType.CutoutDiffuse)
                                customMaterial = new AlphaTestDiffuseLoader();
                            else if (value == ScatterMaterialType.AerialCutout)
                                customMaterial = new AerialTransCutoutLoader();
                        }
                    }

                    // Should we delete the Scatter?
                    [ParserTarget("delete")]
                    [KittopiaHideOption]
                    public NumericParser<Boolean> delete = false;

                    [ParserTargetCollection("Components", AllowMerge = true, NameSignificance = NameSignificance.Type)]
                    public CallbackList<ComponentLoader<ModularScatter>> Components { get; set; }

                    // Custom scatter material
                    [ParserTarget("Material", AllowMerge = true, GetChild = false)]
                    public Material customMaterial
                    {
                        get { return Value.material; }
                        set { Value.material = value; }
                    }

                    // The mesh
                    [ParserTarget("mesh")]
                    public MeshParser baseMesh
                    {
                        get { return Value.baseMesh; }
                        set { Value.baseMesh = value; }
                    }

                    // castShadows
                    [ParserTarget("castShadows")]
                    public NumericParser<Boolean> castShadows
                    {
                        get { return Value.castShadows; }
                        set { Value.castShadows = value; }
                    }

                    // densityFactor
                    [ParserTarget("densityFactor")]
                    public NumericParser<Double> densityFactor
                    {
                        get { return Value.densityFactor; }
                        set { Value.densityFactor = value; }
                    }

                    // Stock material
                    [ParserTarget("material")]
                    public StockMaterialParser material
                    {
                        get { return Value.material; }
                        set { Value.material = value; }
                    }

                    // maxCache
                    [ParserTarget("maxCache")]
                    public NumericParser<Int32> maxCache
                    {
                        get { return Value.maxCache; }
                        set { Value.maxCache = value; }
                    }

                    // maxCacheDelta
                    [ParserTarget("maxCacheDelta")]
                    public NumericParser<Int32> maxCacheDelta
                    {
                        get { return Value.maxCacheDelta; }
                        set { Value.maxCacheDelta = value; }
                    }

                    // maxLevelOffset
                    [ParserTarget("maxLevelOffset")]
                    public NumericParser<Int32> maxLevelOffset
                    {
                        get { return Value.maxLevelOffset; }
                        set { Value.maxLevelOffset = value; }
                    }

                    // maxScale
                    [ParserTarget("maxScale")]
                    public NumericParser<Single> maxScale
                    {
                        get { return Value.maxScale; }
                        set { Value.maxScale = value; }
                    }

                    // maxScatter
                    [ParserTarget("maxScatter")]
                    public NumericParser<Int32> maxScatter
                    {
                        get { return Value.maxScatter; }
                        set { Value.maxScatter = value; }
                    }

                    // maxSpeed
                    [ParserTarget("maxSpeed")]
                    public NumericParser<Double> maxSpeed
                    {
                        get { return Value.maxSpeed; }
                        set { Value.maxSpeed = value; }
                    }

                    // minScale
                    [ParserTarget("minScale")]
                    public NumericParser<Single> minScale
                    {
                        get { return Value.minScale; }
                        set { Value.minScale = value; }
                    }

                    // recieveShadows
                    [ParserTarget("recieveShadows")]
                    public NumericParser<Boolean> recieveShadows
                    {
                        get { return Value.recieveShadows; }
                        set { Value.recieveShadows = value; }
                    }

                    // Scatter name
                    [PreApply]
                    [ParserTarget("name")]
                    public String name
                    {
                        get { return Value.scatterName; }
                        set { Value.scatterName = value; }
                    }
                    
                    // The value we are editing
                    // Scatter seed
                    [ParserTarget("seed")]
                    public NumericParser<Int32> seed
                    {
                        get { return Value.seed; }
                        set { Value.seed = value; }
                    }

                    // verticalOffset
                    [ParserTarget("verticalOffset")]
                    public NumericParser<Single> verticalOffset
                    {
                        get { return Value.verticalOffset; }
                        set { Value.verticalOffset = value; }
                    }

                    // Default Constructor
                    [KittopiaConstructor(KittopiaConstructor.Parameter.Empty)]
                    public LandClassScatterLoader()
                    {
                        Value = new PQSLandControl.LandClassScatter();

                        // Initialize default parameters
                        Value.maxCache = 512;
                        Value.maxCacheDelta = 32;
                        Value.maxSpeed = 1000;
                        
                        // Get the Scatter-Parent
                        GameObject scatterParent = new GameObject("Scatter");
                        scatterParent.transform.parent = Utility.Deactivator;
                        
                        // Add the scatter module
                        Scatter = scatterParent.AddOrGetComponent<ModularScatter>();
                        Scatter.scatter = Value;
                
                        // Create the Component callback
                        Components = new CallbackList<ComponentLoader<ModularScatter>> (e =>
                        {
                            Scatter.Components = Components.Select(c => c.Value).ToList();
                        });
                    }

                    // Runtime constructor
                    public LandClassScatterLoader(PQSLandControl.LandClassScatter value)
                    {
                        Value = value;

                        if (customMaterial)
                        {
                            if (NormalDiffuse.UsesSameShader(customMaterial))
                                customMaterial = new NormalDiffuseLoader(customMaterial);
                            else if (NormalBumped.UsesSameShader(customMaterial))
                                customMaterial = new NormalBumpedLoader(customMaterial);
                            else if (NormalDiffuseDetail.UsesSameShader(customMaterial))
                                customMaterial = new NormalDiffuseDetailLoader(customMaterial);
                            else if (DiffuseWrap.UsesSameShader(customMaterial))
                                customMaterial = new DiffuseWrapLoader(customMaterial);
                            else if (AlphaTestDiffuse.UsesSameShader(customMaterial))
                                customMaterial = new AlphaTestDiffuseLoader(customMaterial);
                            else if (AerialTransCutout.UsesSameShader(customMaterial))
                                customMaterial = new AerialTransCutoutLoader(customMaterial);
                        }
                        
                        // Get the Scatter-Parent
                        GameObject scatterParent = typeof(PQSLandControl.LandClassScatter)
                            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
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
                
                        // Create the Component callback
                        Components = new CallbackList<ComponentLoader<ModularScatter>> (e =>
                        {
                            Scatter.Components = Components.Select(c => c.Value).ToList();
                        });
                
                        // Load existing Modules
                        foreach (IComponent<ModularScatter> component in Scatter.Components)
                        {
                            Type componentType = component.GetType();
                            foreach (Type loaderType in Parser.ModTypes)
                            {
                                if (loaderType.BaseType == null)
                                    continue;
                                if (loaderType.BaseType.Namespace != "Kopernicus.Configuration")
                                    continue;
                                if (!loaderType.BaseType.Name.StartsWith("ComponentParser"))
                                    continue;
                                if (loaderType.BaseType.GetGenericArguments()[0] != typeof(ModularScatter))
                                    continue;
                                if (loaderType.BaseType.GetGenericArguments()[1] != componentType)
                                    continue;
                        
                                // We found our loader type
                                ComponentLoader<ModularScatter> loader = (ComponentLoader<ModularScatter>) Activator.CreateInstance(loaderType);
                                loader.Create(component);
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
                    public NumericParser<Boolean> delete = false;

                    // density
                    [ParserTarget("density")]
                    public NumericParser<Double> density
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
                    [KittopiaConstructor(KittopiaConstructor.Parameter.Empty)]
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
                    public NumericParser<Double> endEnd
                    {
                        get { return Value.endEnd; }
                        set { Value.endEnd = value; }
                    }

                    // endStart
                    [ParserTarget("endStart")]
                    public NumericParser<Double> endStart
                    {
                        get { return Value.endStart; }
                        set { Value.endStart = value; }
                    }

                    // startEnd
                    [ParserTarget("startEnd")]
                    public NumericParser<Double> startEnd
                    {
                        get { return Value.startEnd; }
                        set { Value.startEnd = value; }
                    }

                    // startStart
                    [ParserTarget("startStart")]
                    public NumericParser<Double> startStart
                    {
                        get { return Value.startStart; }
                        set { Value.startStart = value; }
                    }

                    // Default constructor
                    [KittopiaConstructor(KittopiaConstructor.Parameter.Empty)]
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
                public class LandClassLoader : IPatchable, ITypeParser<PQSLandControl.LandClass>
                {
                    // The value we are editing
                    public PQSLandControl.LandClass Value { get; set; }

                    // Should we delete the LandClass?
                    [ParserTarget("delete")]
                    [KittopiaHideOption]
                    public NumericParser<Boolean> delete = false;

                    // alterApparentHeight
                    [ParserTarget("alterApparentHeight")]
                    public NumericParser<Single> alterApparentHeight
                    {
                        get { return Value.alterApparentHeight; }
                        set { Value.alterApparentHeight = value; }
                    }

                    // alterRealHeight
                    [ParserTarget("alterRealHeight")]
                    public NumericParser<Double> alterRealHeight
                    {
                        get { return Value.alterRealHeight; }
                        set { Value.alterRealHeight = value; }
                    }

                    // altitudeRange
                    [ParserTarget("altitudeRange", AllowMerge = true)]
                    public LerpRangeLoader altitudeRange
                    {
                        get { return Value.altitudeRange; }
                        set { Value.altitudeRange = value; }
                    }

                    // color
                    [ParserTarget("color")]
                    public ColorParser color
                    {
                        get { return Value.color; }
                        set { Value.color = value; }
                    }

                    // coverageBlend
                    [ParserTarget("coverageBlend")]
                    public NumericParser<Single> coverageBlend
                    {
                        get { return Value.coverageBlend; }
                        set { Value.coverageBlend = value; }
                    }

                    // coverageFrequency
                    [ParserTarget("coverageFrequency")]
                    public NumericParser<Single> coverageFrequency
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
                    public NumericParser<Int32> coverageOctaves
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
                    public NumericParser<Single> coveragePersistance
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
                    public NumericParser<Int32> coverageSeed
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
                    public NumericParser<Double> latDelta
                    {
                        get { return Value.latDelta; }
                        set { Value.latDelta = value; }
                    }

                    // latitudeDouble
                    [ParserTarget("latitudeDouble")]
                    public NumericParser<Boolean> latitudeDouble
                    {
                        get { return Value.latitudeDouble; }
                        set { Value.latitudeDouble = value; }
                    }

                    // latitudeDoubleRange
                    [ParserTarget("latitudeDoubleRange", AllowMerge = true)]
                    public LerpRangeLoader latitudeDoubleRange
                    {
                        get { return new LerpRangeLoader(Value.latitudeDoubleRange); }
                        set { Value.latitudeDoubleRange = value.Value; }
                    }

                    // latitudeRange
                    [ParserTarget("latitudeRange", AllowMerge = true)]
                    public LerpRangeLoader latitudeRange
                    {
                        get { return new LerpRangeLoader(Value.latitudeRange); }
                        set { Value.latitudeRange = value.Value; }
                    }

                    // lonDelta
                    [ParserTarget("lonDelta")]
                    public NumericParser<Double> lonDelta
                    {
                        get { return Value.lonDelta; }
                        set { Value.lonDelta = value; }
                    }

                    // longitudeRange
                    [ParserTarget("longitudeRange", AllowMerge = true)]
                    public LerpRangeLoader longitudeRange
                    {
                        get { return new LerpRangeLoader(Value.longitudeRange); }
                        set { Value.longitudeRange = value.Value; }
                    }

                    // minimumRealHeight
                    [ParserTarget("minimumRealHeight")]
                    public NumericParser<Double> minimumRealHeight
                    {
                        get { return Value.minimumRealHeight; }
                        set { Value.minimumRealHeight = value; }
                    }

                    // noiseBlend
                    [ParserTarget("noiseBlend")]
                    public NumericParser<Single> noiseBlend
                    {
                        get { return Value.noiseBlend; }
                        set { Value.noiseBlend = value; }
                    }

                    // noiseColor
                    [ParserTarget("noiseColor")]
                    public ColorParser noiseColor
                    {
                        get { return Value.noiseColor; }
                        set { Value.noiseColor = value; }
                    }

                    // noiseFrequency
                    [ParserTarget("noiseFrequency")]
                    public NumericParser<Single> noiseFrequency
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
                    public NumericParser<Int32> noiseOctaves
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
                    public NumericParser<Single> noisePersistance
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
                    public NumericParser<Int32> noiseSeed
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
                    public CallbackList<LandClassScatterAmountLoader> scatter { get; set; }

                    // Default constructor
                    [KittopiaConstructor(KittopiaConstructor.Parameter.Empty)]
                    public LandClassLoader()
                    {
                        Value = new PQSLandControl.LandClass();
                        
                        // Initialize default parameters
                        Value.altDelta = 1;
                        Value.color = new Color(0, 0, 0, 0);
                        Value.coverageFrequency = 1;
                        Value.coverageOctaves = 1;
                        Value.coveragePersistance = 1;
                        Value.coverageSeed = 1;
                        Value.landClassName = "Base";
                        Value.latDelta = 1;
                        Value.lonDelta = 1;
                        Value.noiseColor = new Color(0, 0, 0, 0);
                        Value.noiseFrequency = 1;
                        Value.noiseOctaves = 1;
                        Value.noisePersistance = 1;
                        Value.noiseSeed = 1;

                        LerpRangeLoader range = new LerpRangeLoader();
                        range.Value.endEnd = 1;
                        range.Value.endStart = 1;
                        range.Value.startEnd = 0;
                        range.Value.startStart = 0;
                        altitudeRange = range;

                        range = new LerpRangeLoader();
                        range.Value.endEnd = 1;
                        range.Value.endStart = 1;
                        range.Value.startEnd = 0;
                        range.Value.startStart = 0;
                        latitudeRange = range;

                        range = new LerpRangeLoader();
                        range.Value.endEnd = 1;
                        range.Value.endStart = 1;
                        range.Value.startEnd = 0;
                        range.Value.startStart = 0;
                        latitudeDoubleRange = range;

                        range = new LerpRangeLoader();
                        range.Value.endEnd = 2;
                        range.Value.endStart = 2;
                        range.Value.startEnd = -1;
                        range.Value.startStart = -1;
                        longitudeRange = range;

                        // Create the scatter list
                        scatter = new CallbackList<LandClassScatterAmountLoader>(e =>
                        {
                            Value.scatter = scatter.Where(s => !s.delete).Select(s => s.Value).ToArray();
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
                            LerpRangeLoader range = new LerpRangeLoader();
                            range.Value.endEnd = 1;
                            range.Value.endStart = 1;
                            range.Value.startEnd = 0;
                            range.Value.startStart = 0;
                            altitudeRange = range;
                        }
                        if (Value.latitudeRange == null)
                        {
                            LerpRangeLoader range = new LerpRangeLoader();
                            range.Value.endEnd = 1;
                            range.Value.endStart = 1;
                            range.Value.startEnd = 0;
                            range.Value.startStart = 0;
                            latitudeRange = range;
                        }
                        if (Value.latitudeDoubleRange == null)
                        {
                            LerpRangeLoader range = new LerpRangeLoader();
                            range.Value.endEnd = 1;
                            range.Value.endStart = 1;
                            range.Value.startEnd = 0;
                            range.Value.startStart = 0;
                            latitudeDoubleRange = range;
                        }
                        if (Value.longitudeRange == null)
                        {
                            LerpRangeLoader range = new LerpRangeLoader();
                            range.Value.endEnd = 2;
                            range.Value.endStart = 2;
                            range.Value.startEnd = -1;
                            range.Value.startStart = -1;
                            longitudeRange = range;
                        }
                        
                        // Create the scatter list
                        scatter = new CallbackList<LandClassScatterAmountLoader>(e =>
                        {
                            Value.scatter = scatter.Where(s => !s.delete).Select(s => s.Value).ToArray();
                        });

                        // Load scatters
                        if (Value.scatter != null)
                        {
                            for (Int32 i = 0; i < Value.scatter.Length; i++)
                            {
                                // Only activate the callback if we are adding the last loader
                                scatter.Add(new LandClassScatterAmountLoader(Value.scatter[i]),
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
                public NumericParser<Single> altitudeBlend
                {
                    get { return mod.altitudeBlend; }
                    set { mod.altitudeBlend = value; }
                }

                // altitudeFrequency
                [ParserTarget("altitudeFrequency")]
                public NumericParser<Single> altitudeFrequency
                {
                    get { return mod.altitudeFrequency; }
                    set
                    {
                        mod.altitudeFrequency = value;
                        mod.altitudeSimplex = new Simplex(mod.altitudeSeed, mod.altitudeOctaves,
                            mod.altitudePersistance, mod.altitudeFrequency);
                    }
                }

                // altitudeOctaves
                [ParserTarget("altitudeOctaves")]
                public NumericParser<Int32> altitudeOctaves
                {
                    get { return mod.altitudeOctaves; }
                    set
                    {
                        mod.altitudeOctaves = value;
                        mod.altitudeSimplex = new Simplex(mod.altitudeSeed, mod.altitudeOctaves,
                            mod.altitudePersistance, mod.altitudeFrequency);
                    }
                }

                // altitudePersistance
                [ParserTarget("altitudePersistance")]
                public NumericParser<Single> altitudePersistance
                {
                    get { return mod.altitudePersistance; }
                    set
                    {
                        mod.altitudePersistance = value;
                        mod.altitudeSimplex = new Simplex(mod.altitudeSeed, mod.altitudeOctaves,
                            mod.altitudePersistance, mod.altitudeFrequency);
                    }
                }

                // altitudeSeed
                [ParserTarget("altitudeSeed")]
                public NumericParser<Int32> altitudeSeed
                {
                    get { return mod.altitudeSeed; }
                    set
                    {
                        mod.altitudeSeed = value;
                        mod.altitudeSimplex = new Simplex(mod.altitudeSeed, mod.altitudeOctaves,
                            mod.altitudePersistance, mod.altitudeFrequency);
                    }
                }

                // createColors
                [ParserTarget("createColors")]
                public NumericParser<Boolean> createColors
                {
                    // Yes, really do what we want!
                    get { return mod.reallyCreateColors; }
                    set { mod.reallyCreateColors = value; }
                }

                // createScatter
                [ParserTarget("createScatter")]
                public NumericParser<Boolean> createScatter
                {
                    // Yes, really do what we want!
                    get { return mod.reallyCreateScatter; }
                    set { mod.reallyCreateScatter = value; }
                }

                // heightMap
                [ParserTarget("heightMap")]
                public MapSOParser_GreyScale<MapSO> heightMap
                {
                    get { return mod.heightMap; }
                    set { mod.heightMap = value; }
                }

                // latitudeBlend
                [ParserTarget("latitudeBlend")]
                public NumericParser<Single> latitudeBlend
                {
                    get { return mod.latitudeBlend; }
                    set { mod.latitudeBlend = value; }
                }

                // latitudeFrequency
                [ParserTarget("latitudeFrequency")]
                public NumericParser<Single> latitudeFrequency
                {
                    get { return mod.latitudeFrequency; }
                    set
                    {
                        mod.latitudeFrequency = value;
                        mod.latitudeSimplex = new Simplex(mod.latitudeSeed, mod.latitudeOctaves,
                            mod.latitudePersistance, mod.latitudeFrequency);
                    }
                }

                // latitudeOctaves
                [ParserTarget("latitudeOctaves")]
                public NumericParser<Int32> latitudeOctaves
                {
                    get { return mod.latitudeOctaves; }
                    set
                    {
                        mod.latitudeOctaves = value;
                        mod.latitudeSimplex = new Simplex(mod.latitudeSeed, mod.latitudeOctaves,
                            mod.latitudePersistance, mod.latitudeFrequency);
                    }
                }

                // latitudePersistance
                [ParserTarget("latitudePersistance")]
                public NumericParser<Single> latitudePersistance
                {
                    get { return mod.latitudePersistance; }
                    set
                    {
                        mod.latitudePersistance = value;
                        mod.latitudeSimplex = new Simplex(mod.latitudeSeed, mod.latitudeOctaves,
                            mod.latitudePersistance, mod.latitudeFrequency);
                    }
                }

                // latitudeSeed
                [ParserTarget("latitudeSeed")]
                public NumericParser<Int32> latitudeSeed
                {
                    get { return mod.latitudeSeed; }
                    set
                    {
                        mod.latitudeSeed = value;
                        mod.latitudeSimplex = new Simplex(mod.latitudeSeed, mod.latitudeOctaves,
                            mod.latitudePersistance, mod.latitudeFrequency);
                    }
                }

                // longitudeBlend
                [ParserTarget("longitudeBlend")]
                public NumericParser<Single> longitudeBlend
                {
                    get { return mod.longitudeBlend; }
                    set { mod.longitudeBlend = value; }
                }

                // longitudeFrequency
                [ParserTarget("longitudeFrequency")]
                public NumericParser<Single> longitudeFrequency
                {
                    get { return mod.longitudeFrequency; }
                    set
                    {
                        mod.longitudeFrequency = value;
                        mod.longitudeSimplex = new Simplex(mod.longitudeSeed, mod.longitudeOctaves,
                            mod.longitudePersistance, mod.longitudeFrequency);
                    }
                }

                // longitudeOctaves
                [ParserTarget("longitudeOctaves")]
                public NumericParser<Int32> longitudeOctaves
                {
                    get { return mod.longitudeOctaves; }
                    set
                    {
                        mod.longitudeOctaves = value;
                        mod.longitudeSimplex = new Simplex(mod.longitudeSeed, mod.longitudeOctaves,
                            mod.longitudePersistance, mod.longitudeFrequency);
                    }
                }

                // longitudePersistance
                [ParserTarget("longitudePersistance")]
                public NumericParser<Single> longitudePersistance
                {
                    get { return mod.longitudePersistance; }
                    set
                    {
                        mod.longitudePersistance = value;
                        mod.longitudeSimplex = new Simplex(mod.longitudeSeed, mod.longitudeOctaves,
                            mod.longitudePersistance, mod.longitudeFrequency);
                    }
                }

                // longitudeSeed
                [ParserTarget("longitudeSeed")]
                public NumericParser<Int32> longitudeSeed
                {
                    get { return mod.longitudeSeed; }
                    set
                    {
                        mod.longitudeSeed = value;
                        mod.longitudeSimplex = new Simplex(mod.longitudeSeed, mod.longitudeOctaves,
                            mod.longitudePersistance, mod.longitudeFrequency);
                    }
                }

                // useHeightMap
                [ParserTarget("useHeightMap")]
                public NumericParser<Boolean> useHeightMap
                {
                    get { return mod.useHeightMap; }
                    set { mod.useHeightMap = value; }
                }

                // vHeightMax
                [ParserTarget("vHeightMax")]
                public NumericParser<Single> vHeightMax
                {
                    get { return mod.vHeightMax; }
                    set { mod.vHeightMax = value; }
                }

                // List of scatters
                [ParserTargetCollection("Scatters", AllowMerge = true)] 
                [ParserTargetCollection("scatters", AllowMerge = true)] 
                public CallbackList<LandClassScatterLoader> scatters { get; set; }

                // List of landclasses
                [ParserTargetCollection("LandClasses", AllowMerge = true)]
                [ParserTargetCollection("landClasses", AllowMerge = true)]
                public CallbackList<LandClassLoader> landClasses { get; set; }

                // Creates the a PQSMod of type T with given PQS
                public override void Create(PQS pqsVersion)
                {
                    base.Create(pqsVersion);

                    // Initialize default parameters
                    mod.altitudeSeed = 1;
                    mod.altitudeOctaves = 1;
                    mod.altitudePersistance = 1;
                    mod.altitudeFrequency = 1;
                    mod.latitudeSeed = 1;
                    mod.latitudeOctaves = 1;
                    mod.latitudePersistance = 1;
                    mod.latitudeFrequency = 1;
                    mod.longitudeSeed = 1;
                    mod.longitudeOctaves = 1;
                    mod.longitudePersistance = 1;
                    mod.longitudeFrequency = 1;
                    
                    // Create the callback list for Scatters
                    scatters = new CallbackList<LandClassScatterLoader>(e =>
                    {
                        foreach (LandClassScatterLoader loader in scatters)
                        {
                            loader.Scatter.transform.parent = mod.transform;
                        }

                        mod.scatters = scatters.Where(scatter => !scatter.delete)
                            .Select(scatter => scatter.Value).ToArray();
                    });
                    mod.scatters = new PQSLandControl.LandClassScatter[0];
                    
                    // Create the callback list for LandClasses
                    landClasses = new CallbackList<LandClassLoader> (e =>
                    {
                        // Assign each scatter amount with their corresponding scatter
                        foreach (PQSLandControl.LandClassScatterAmount amount in e.scatter)
                        {
                            Int32 i = 0;
                            while (i < mod.scatters.Length)
                            {
                                if (mod.scatters[i].scatterName.Equals(amount.scatterName)) break;
                                i++;
                            }

                            if (i < mod.scatters.Length)
                            {
                                amount.scatterIndex = i;
                                amount.scatter = mod.scatters[i];
                            }
                        }
                        
                        // Assign the new values
                        mod.landClasses = landClasses.Where(landClass => !landClass.delete)
                            .Select(landClass => landClass.Value).ToArray();
                    });
                    mod.landClasses = new PQSLandControl.LandClass[0];
                }

                // Grabs a PQSMod of type T from a parameter with a given PQS
                public override void Create(PQSLandControlPatched _mod, PQS pqsVersion)
                {
                    base.Create(_mod, pqsVersion);
                    
                    // Create the callback list for Scatters
                    scatters = new CallbackList<LandClassScatterLoader>(e =>
                    {
                        foreach (LandClassScatterLoader loader in scatters)
                        {
                            loader.Scatter.transform.parent = mod.transform;
                        }
                        
                        mod.scatters = scatters.Where(scatter => !scatter.delete)
                            .Select(scatter => scatter.Value).ToArray();
                    });
                    
                    // Load Scatters
                    if (mod.scatters != null)
                    {
                        for (Int32 i = 0; i < mod.scatters.Length; i++)
                        {
                            // Only activate the callback if we are adding the last loader
                            scatters.Add(new LandClassScatterLoader(mod.scatters[i]), i == mod.scatters.Length - 1);
                        }
                    }
                    else
                    {
                        mod.scatters = new PQSLandControl.LandClassScatter[0];
                    }
                    
                    // Create the callback list for LandClasses
                    landClasses = new CallbackList<LandClassLoader> (e =>
                    {
                        // Assign each scatter amount with their corresponding scatter
                        foreach (PQSLandControl.LandClassScatterAmount amount in e.scatter)
                        {
                            Int32 i = 0;
                            while (i < mod.scatters.Length)
                            {
                                if (mod.scatters[i].scatterName.Equals(amount.scatterName)) break;
                                i++;
                            }

                            if (i < mod.scatters.Length)
                            {
                                amount.scatterIndex = i;
                                amount.scatter = mod.scatters[i];
                            }
                        }
                        
                        // Assign the new values
                        mod.landClasses = landClasses.Where(landClass => !landClass.delete)
                            .Select(landClass => landClass.Value).ToArray();
                    });
                    
                    // Load LandClasses
                    if (mod.landClasses != null)
                    {
                        for (Int32 i = 0; i < mod.landClasses.Length; i++)
                        {
                            // Only activate the callback if we are adding the last loader
                            landClasses.Add(new LandClassLoader(mod.landClasses[i]), i == mod.landClasses.Length - 1);
                        }
                    }
                    else
                    {
                        mod.landClasses = new PQSLandControl.LandClass[0];
                    }
                }
            }
        }
    }
}
