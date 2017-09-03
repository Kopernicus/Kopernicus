/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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

using Kopernicus.Components;
using Kopernicus.MaterialWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class LandControl : ModLoader<PQSLandControl>, IParserEventSubscriber
            {
                // Loader for a Simplex object
                [RequireConfigType(ConfigType.Node)]
                public class SimplexLoader
                {
                    // The edited object
                    public Simplex simplex;

                    // The frequency of the simplex noise
                    [ParserTarget("frequency")]
                    public NumericParser<Double> frequency
                    {
                        get { return simplex.frequency; }
                        set { simplex.frequency = value; }
                    }
                    
                    // Octaves of the simplex noise
                    [ParserTarget("octaves")]
                    public NumericParser<Double> octaves
                    {
                        get { return simplex.octaves; }
                        set { simplex.octaves = value; }
                    }

                    // Persistence of the simplex noise
                    [ParserTarget("persistence")]
                    public NumericParser<Double> persistence
                    {
                        get { return simplex.persistence; }
                        set { simplex.persistence = value; }
                    }

                    // The seed of the simplex noise
                    [ParserTarget("seed")]
                    public NumericParser<Int32> seed
                    {
                        set { simplex.seed = value; }
                    }

                    // Default constructor
                    public SimplexLoader()
                    {
                        simplex = new Simplex(0, 1, 1, 1);
                    }

                    // Runtime Constructor
                    public SimplexLoader(Simplex simplex)
                    {
                        if (simplex != null)
                            this.simplex = simplex;
                        else
                            this.simplex = new Simplex(0, 1, 1, 1);
                    }
                }

                // Loader for a Ground Scatter
                [RequireConfigType(ConfigType.Node)]
                public class LandClassScatterLoader : IParserEventSubscriber
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

                    public PQSLandControl.LandClassScatter scatter;

                    // Stock scatter material parser
                    [RequireConfigType(ConfigType.Value)]
                    public class StockMaterialParser : IParsable
                    {
                        public Material value;
                        public void SetFromString(String s)
                        {
                            String materialName = Regex.Replace(s, "BUILTIN/", "");
                            value = Resources.FindObjectsOfTypeAll<Material>().Where(material => material.name == materialName).FirstOrDefault();
                        }

                        // Convert
                        public static implicit operator Material(StockMaterialParser parser)
                        {
                            if (parser != null)
                                return parser.value;
                            else
                                return null;
                        }
                        public static implicit operator StockMaterialParser(Material material)
                        {
                            if (material == null)
                                return new StockMaterialParser();
                            Material m = new Material(material);
                            m.name = "BUILTIN/" + m.name;
                            return new StockMaterialParser { value = m };
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
                            else if (NormalBumped.UsesSameShader(customMaterial))
                                return ScatterMaterialType.BumpedDiffuse;
                            else if (NormalDiffuseDetail.UsesSameShader(customMaterial))
                                return ScatterMaterialType.DiffuseDetail;
                            else if (DiffuseWrap.UsesSameShader(customMaterial))
                                return ScatterMaterialType.DiffuseWrapped;
                            else if (AlphaTestDiffuse.UsesSameShader(customMaterial))
                                return ScatterMaterialType.CutoutDiffuse;
                            else if (AerialTransCutout.UsesSameShader(customMaterial))
                                return ScatterMaterialType.AerialCutout;
                            return null;
                        }
                        set
                        {
                            if (value.value == ScatterMaterialType.Diffuse)
                                customMaterial = new NormalDiffuseLoader();
                            else if (value.value == ScatterMaterialType.BumpedDiffuse)
                                customMaterial = new NormalBumpedLoader();
                            else if (value.value == ScatterMaterialType.DiffuseDetail)
                                customMaterial = new NormalDiffuseDetailLoader();
                            else if (value.value == ScatterMaterialType.DiffuseWrapped)
                                customMaterial = new DiffuseWrapLoader();
                            else if (value.value == ScatterMaterialType.CutoutDiffuse)
                                customMaterial = new AlphaTestDiffuseLoader();
                            else if (value.value == ScatterMaterialType.AerialCutout)
                                customMaterial = new AerialTransCutoutLoader();
                        }
                    }

                    // Should we delete the Scatter?
                    [ParserTarget("delete")]
                    public NumericParser<Boolean> delete = false;

                    // Should we add colliders to the scatter=
                    [ParserTarget("collide")]
                    public NumericParser<Boolean> collide = false;

                    // Should we add Science to the Scatter?
                    [ParserTarget("science")]
                    public NumericParser<Boolean> science = false;

                    // ConfigNode that stores the Experiment
                    [ParserTarget("Experiment")]
                    public ConfigNode experiment;

                    // Custom scatter material
                    [ParserTarget("Material", allowMerge = true, getChild = false)]
                    public Material customMaterial;

                    // The mesh
                    [ParserTarget("mesh")]
                    public MeshParser baseMesh
                    {
                        get { return scatter.baseMesh; }
                        set { scatter.baseMesh = value; }
                    }

                    // castShadows
                    [ParserTarget("castShadows")]
                    public NumericParser<Boolean> castShadows
                    {
                        get { return scatter.castShadows; }
                        set { scatter.castShadows = value; }
                    }

                    // densityFactor
                    [ParserTarget("densityFactor")]
                    public NumericParser<Double> densityFactor
                    {
                        get { return scatter.densityFactor; }
                        set { scatter.densityFactor = value; }
                    }

                    // Stock material
                    [ParserTarget("material")]
                    public StockMaterialParser material
                    {
                        get { return scatter.material; }
                        set { scatter.material = value; }
                    }

                    // maxCache
                    [ParserTarget("maxCache")]
                    public NumericParser<Int32> maxCache
                    {
                        get { return scatter.maxCache; }
                        set { scatter.maxCache = value; }
                    }

                    // maxCacheDelta
                    [ParserTarget("maxCacheDelta")]
                    public NumericParser<Int32> maxCacheDelta
                    {
                        get { return scatter.maxCacheDelta; }
                        set { scatter.maxCacheDelta = value; }
                    }

                    // maxLevelOffset
                    [ParserTarget("maxLevelOffset")]
                    public NumericParser<Int32> maxLevelOffset
                    {
                        get { return scatter.maxLevelOffset; }
                        set { scatter.maxLevelOffset = value; }
                    }

                    // maxScale
                    [ParserTarget("maxScale")]
                    public NumericParser<Single> maxScale
                    {
                        get { return scatter.maxScale; }
                        set { scatter.maxScale = value; }
                    }

                    // maxScatter
                    [ParserTarget("maxScatter")]
                    public NumericParser<Int32> maxScatter
                    {
                        get { return scatter.maxScatter; }
                        set { scatter.maxScatter = value; }
                    }

                    // maxSpeed
                    [ParserTarget("maxSpeed")]
                    public NumericParser<Double> maxSpeed
                    {
                        get { return scatter.maxSpeed; }
                        set { scatter.maxSpeed = value; }
                    }

                    // minScale
                    [ParserTarget("minScale")]
                    public NumericParser<Single> minScale
                    {
                        get { return scatter.minScale; }
                        set { scatter.minScale = value; }
                    }

                    // recieveShadows
                    [ParserTarget("recieveShadows")]
                    public NumericParser<Boolean> recieveShadows
                    {
                        get { return scatter.recieveShadows; }
                        set { scatter.recieveShadows = value; }
                    }

                    // Scatter name
                    [ParserTarget("name")]
                    public String scatterName
                    {
                        get { return scatter.scatterName; }
                        set { scatter.scatterName = value; }
                    }

                    // Scatter seed
                    [ParserTarget("seed")]
                    public NumericParser<Int32> seed
                    {
                        get { return scatter.seed; }
                        set { scatter.seed = value; }
                    }

                    // verticalOffset
                    [ParserTarget("verticalOffset")]
                    public NumericParser<Single> verticalOffset
                    {
                        get { return scatter.verticalOffset; }
                        set { scatter.verticalOffset = value; }
                    }

                    // Default Constructor
                    public LandClassScatterLoader()
                    {
                        scatter = new PQSLandControl.LandClassScatter();

                        // Initialize default parameters
                        scatter.maxCache = 512;
                        scatter.maxCacheDelta = 32;
                        scatter.maxSpeed = 1000;
                    }

                    // Runtime constructor
                    public LandClassScatterLoader(PQSLandControl.LandClassScatter scatter)
                    {
                        if (scatter != null)
                            this.scatter = scatter;
                        else
                        {
                            this.scatter = new PQSLandControl.LandClassScatter();

                            // Initialize default parameters
                            scatter.maxCache = 512;
                            scatter.maxCacheDelta = 32;
                            scatter.maxSpeed = 1000;
                        }
                    }

                    void IParserEventSubscriber.Apply(ConfigNode node) 
                    {
                        if (!customMaterial && scatter.material)
                        {
                            if (scatter.material.shader == new NormalDiffuse().shader)
                                customMaterial = new NormalDiffuseLoader(scatter.material);
                            else if (scatter.material.shader == new NormalBumped().shader)
                                customMaterial = new NormalBumpedLoader(scatter.material);
                            else if (scatter.material.shader == new NormalDiffuseDetail().shader)
                                customMaterial = new NormalDiffuseDetailLoader(scatter.material);
                            else if (scatter.material.shader == new DiffuseWrapLoader().shader)
                                customMaterial = new DiffuseWrapLoader(scatter.material);
                            else if (scatter.material.shader == new AlphaTestDiffuse().shader)
                                customMaterial = new AlphaTestDiffuseLoader(scatter.material);
                            else if (scatter.material.shader == new AerialTransCutout().shader)
                                customMaterial = new AerialTransCutoutLoader(scatter.material);
                        }
                    }

                    void IParserEventSubscriber.PostApply(ConfigNode node)
                    {
                        // If defined, use custom material
                        if (customMaterial != null) scatter.material = customMaterial;
                    }
                }

                // Loader for the Amount of a Scatter on a body
                [RequireConfigType(ConfigType.Node)]
                public class LandClassScatterAmountLoader
                {
                    public PQSLandControl.LandClassScatterAmount scatterAmount;

                    // density
                    [ParserTarget("density")]
                    public NumericParser<Double> density
                    {
                        get { return scatterAmount.density; }
                        set { scatterAmount.density = value; }
                    }

                    // The name of the scatter used
                    [ParserTarget("scatterName")]
                    public String scatterName
                    {
                        get { return scatterAmount.scatterName; }
                        set { scatterAmount.scatterName = value; }
                    }

                    // Default Constructor
                    public LandClassScatterAmountLoader()
                    {
                        scatterAmount = new PQSLandControl.LandClassScatterAmount();
                    }

                    // Runtime Constructor
                    public LandClassScatterAmountLoader(PQSLandControl.LandClassScatterAmount amount)
                    {
                        if (amount != null)
                            scatterAmount = amount;
                        else
                            scatterAmount = new PQSLandControl.LandClassScatterAmount();
                    }
                }

                // Loader for LerpRange
                [RequireConfigType(ConfigType.Node)]
                public class LerpRangeLoader
                {
                    public PQSLandControl.LerpRange lerpRange;

                    // endEnd
                    [ParserTarget("endEnd")]
                    public NumericParser<Double> endEnd
                    {
                        get { return lerpRange.endEnd; }
                        set { lerpRange.endEnd = value; }
                    }

                    // endStart
                    [ParserTarget("endStart")]
                    public NumericParser<Double> endStart
                    {
                        get { return lerpRange.endStart; }
                        set { lerpRange.endStart = value; }
                    }

                    // startEnd
                    [ParserTarget("startEnd")]
                    public NumericParser<Double> startEnd
                    {
                        get { return lerpRange.startEnd; }
                        set { lerpRange.startEnd = value; }
                    }

                    // startStart
                    [ParserTarget("startStart")]
                    public NumericParser<Double> startStart
                    {
                        get { return lerpRange.startStart; }
                        set { lerpRange.startStart = value; }
                    }

                    // Default constructor
                    public LerpRangeLoader()
                    {
                        lerpRange = new PQSLandControl.LerpRange();
                    }

                    // Runtime Constructor
                    public LerpRangeLoader(PQSLandControl.LerpRange lerp)
                    {
                        if (lerp != null)
                            lerpRange = lerp;
                        else
                            lerpRange = new PQSLandControl.LerpRange();
                    }
                }

                // Loader for LandClass
                [RequireConfigType(ConfigType.Node)]
                public class LandClassLoader : IParserEventSubscriber
                {
                    public PQSLandControl.LandClass landClass;

                    // Should we delete the LandClass?
                    [ParserTarget("delete")]
                    public NumericParser<Boolean> delete = false;

                    // alterApparentHeight
                    [ParserTarget("alterApparentHeight")]
                    public NumericParser<Single> alterApparentHeight
                    {
                        get { return landClass.alterApparentHeight; }
                        set { landClass.alterApparentHeight = value; }
                    }

                    // alterRealHeight
                    [ParserTarget("alterRealHeight")]
                    public NumericParser<Double> alterRealHeight
                    {
                        get { return landClass.alterRealHeight; }
                        set { landClass.alterRealHeight = value; }
                    }

                    // altitudeRange
                    [ParserTarget("altitudeRange", allowMerge = true)]
                    public LerpRangeLoader altitudeRange
                    {
                        get { return new LerpRangeLoader(landClass.altitudeRange); }
                        set { landClass.altitudeRange = value.lerpRange; }
                    }

                    // color
                    [ParserTarget("color")]
                    public ColorParser color
                    {
                        get { return landClass.color; }
                        set { landClass.color = value; }
                    }

                    // coverageBlend
                    [ParserTarget("coverageBlend")]
                    public NumericParser<Single> coverageBlend
                    {
                        get { return landClass.coverageBlend; }
                        set { landClass.coverageBlend = value; }
                    }

                    // coverageFrequency
                    [ParserTarget("coverageFrequency")]
                    public NumericParser<Single> coverageFrequency
                    {
                        get { return landClass.coverageFrequency; }
                        set { landClass.coverageFrequency = value; }
                    }

                    // coverageOctaves
                    [ParserTarget("coverageOctaves")]
                    public NumericParser<Int32> coverageOctaves
                    {
                        get { return landClass.coverageOctaves; }
                        set { landClass.coverageOctaves = value; }
                    }

                    // coveragePersistance
                    [ParserTarget("coveragePersistance")]
                    public NumericParser<Single> coveragePersistance
                    {
                        get { return landClass.coveragePersistance; }
                        set { landClass.coveragePersistance = value; }
                    }

                    // coverageSeed
                    [ParserTarget("coverageSeed")]
                    public NumericParser<Int32> coverageSeed
                    {
                        get { return landClass.coverageSeed; }
                        set { landClass.coverageSeed = value; }
                    }

                    // coverageSimplex
                    [ParserTarget("coverageSimplex", allowMerge = true)]
                    public SimplexLoader coverageSimplex
                    {
                        get { return new SimplexLoader(landClass.coverageSimplex); }
                        set { landClass.coverageSimplex = value.simplex; }
                    }

                    // The name of the landclass
                    [ParserTarget("name")]
                    public String landClassName
                    {
                        get { return landClass.landClassName; }
                        set { landClass.landClassName = value; }
                    }

                    // latDelta
                    [ParserTarget("latDelta")]
                    public NumericParser<Double> latDelta
                    {
                        get { return landClass.latDelta; }
                        set { landClass.latDelta = value; }
                    }

                    // latitudeDouble
                    [ParserTarget("latitudeDouble")]
                    public NumericParser<Boolean> latitudeDouble
                    {
                        get { return landClass.latitudeDouble; }
                        set { landClass.latitudeDouble = value; }
                    }

                    // latitudeDoubleRange
                    [ParserTarget("latitudeDoubleRange", allowMerge = true)]
                    public LerpRangeLoader latitudeDoubleRange
                    {
                        get { return new LerpRangeLoader(landClass.latitudeDoubleRange); }
                        set { landClass.latitudeDoubleRange = value.lerpRange; }
                    }

                    // latitudeRange
                    [ParserTarget("latitudeRange", allowMerge = true)]
                    public LerpRangeLoader latitudeRange
                    {
                        get { return new LerpRangeLoader(landClass.latitudeRange); }
                        set { landClass.latitudeRange = value.lerpRange; }
                    }

                    // lonDelta
                    [ParserTarget("lonDelta")]
                    public NumericParser<Double> lonDelta
                    {
                        get { return landClass.lonDelta; }
                        set { landClass.lonDelta = value; }
                    }

                    // longitudeRange
                    [ParserTarget("longitudeRange", allowMerge = true)]
                    public LerpRangeLoader longitudeRange
                    {
                        get { return new LerpRangeLoader(landClass.longitudeRange); }
                        set { landClass.longitudeRange = value.lerpRange; }
                    }

                    // minimumRealHeight
                    [ParserTarget("minimumRealHeight")]
                    public NumericParser<Double> minimumRealHeight
                    {
                        get { return landClass.minimumRealHeight; }
                        set { landClass.minimumRealHeight = value; }
                    }

                    // noiseBlend
                    [ParserTarget("noiseBlend")]
                    public NumericParser<Single> noiseBlend
                    {
                        get { return landClass.noiseBlend; }
                        set { landClass.noiseBlend = value; }
                    }

                    // noiseColor
                    [ParserTarget("noiseColor")]
                    public ColorParser noiseColor
                    {
                        get { return landClass.noiseColor; }
                        set { landClass.noiseColor = value; }
                    }

                    // noiseFrequency
                    [ParserTarget("noiseFrequency")]
                    public NumericParser<Single> noiseFrequency
                    {
                        get { return landClass.noiseFrequency; }
                        set { landClass.noiseFrequency = value; }
                    }

                    // noiseOctaves
                    [ParserTarget("noiseOctaves")]
                    public NumericParser<Int32> noiseOctaves
                    {
                        get { return landClass.noiseOctaves; }
                        set { landClass.noiseOctaves = value; }
                    }

                    // noisePersistance
                    [ParserTarget("noisePersistance")]
                    public NumericParser<Single> noisePersistance
                    {
                        get { return landClass.noisePersistance; }
                        set { landClass.noisePersistance = value; }
                    }

                    // noiseSeed
                    [ParserTarget("noiseSeed")]
                    public NumericParser<Int32> noiseSeed
                    {
                        get { return landClass.noiseSeed; }
                        set { landClass.noiseSeed = value; }
                    }

                    // noiseSimplex
                    [ParserTarget("noiseSimplex", allowMerge = true)]
                    public SimplexLoader noiseSimplex
                    {
                        get { return new SimplexLoader(landClass.noiseSimplex); }
                        set { landClass.noiseSimplex = value.simplex; }
                    }

                    // List of scatters used
                    [ParserTargetCollection("scatters", nameSignificance = NameSignificance.None)]
                    public List<LandClassScatterAmountLoader> scatter = new List<LandClassScatterAmountLoader>();

                    // Apply Event
                    void IParserEventSubscriber.Apply(ConfigNode node) { }

                    // Post Apply Event
                    void IParserEventSubscriber.PostApply(ConfigNode node)
                    {
                        if (scatter.Any() || landClass.scatter == null)
                            landClass.scatter = scatter.Select(s => s.scatterAmount).ToArray();
                    }

                    // Default constructor
                    public LandClassLoader()
                    {
                        landClass = new PQSLandControl.LandClass();
                        LerpRangeLoader range;

                        // Initialize default parameters
                        landClass.altDelta = 1;
                        landClass.color = new Color(0, 0, 0, 0);
                        landClass.coverageFrequency = 1;
                        landClass.coverageOctaves = 1;
                        landClass.coveragePersistance = 1;
                        landClass.coverageSeed = 1;
                        landClass.landClassName = "Base";
                        landClass.latDelta = 1;
                        landClass.lonDelta = 1;
                        landClass.noiseColor = new Color(0, 0, 0, 0);
                        landClass.noiseFrequency = 1;
                        landClass.noiseOctaves = 1;
                        landClass.noisePersistance = 1;
                        landClass.noiseSeed = 1;

                        range = new LerpRangeLoader();
                        range.lerpRange.endEnd = 1;
                        range.lerpRange.endStart = 1;
                        range.lerpRange.startEnd = 0;
                        range.lerpRange.startStart = 0;
                        altitudeRange = range;

                        range = new LerpRangeLoader();
                        range.lerpRange.endEnd = 1;
                        range.lerpRange.endStart = 1;
                        range.lerpRange.startEnd = 0;
                        range.lerpRange.startStart = 0;
                        latitudeRange = range;

                        range = new LerpRangeLoader();
                        range.lerpRange.endEnd = 1;
                        range.lerpRange.endStart = 1;
                        range.lerpRange.startEnd = 0;
                        range.lerpRange.startStart = 0;
                        latitudeDoubleRange = range;

                        range = new LerpRangeLoader();
                        range.lerpRange.endEnd = 2;
                        range.lerpRange.endStart = 2;
                        range.lerpRange.startEnd = -1;
                        range.lerpRange.startStart = -1;
                        longitudeRange = range;
                    }

                    // Runtime constructor
                    public LandClassLoader(PQSLandControl.LandClass landClass)
                    {
                        this.landClass = landClass;
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
                    set { mod.altitudeFrequency = value; }
                }

                // altitudeOctaves
                [ParserTarget("altitudeOctaves")]
                public NumericParser<Int32> altitudeOctaves
                {
                    get { return mod.altitudeOctaves; }
                    set { mod.altitudeOctaves = value; }
                }

                // altitudePersistance
                [ParserTarget("altitudePersistance")]
                public NumericParser<Single> altitudePersistance
                {
                    get { return mod.altitudePersistance; }
                    set { mod.altitudePersistance = value; }
                }

                // altitudeSeed
                [ParserTarget("altitudeSeed")]
                public NumericParser<Int32> altitudeSeed
                {
                    get { return mod.altitudeSeed; }
                    set { mod.altitudeSeed = value; }
                }

                // altitudeSimplex
                [ParserTarget("altitudeSimplex")]
                public SimplexLoader altitudeSimplex
                {
                    get { return new SimplexLoader(mod.altitudeSimplex); }
                    set { mod.altitudeSimplex = value.simplex; }
                }

                // createColors
                [ParserTarget("createColors")]
                public NumericParser<Boolean> createColors
                {
                    get { return mod.createColors; }
                    set { mod.createColors = value; }
                }

                // createScatter
                [ParserTarget("createScatter")]
                public NumericParser<Boolean> createScatter
                {
                    get { return mod.createScatter; }
                    set { mod.createScatter = value; }
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
                    set { mod.latitudeFrequency = value; }
                }

                // latitudeOctaves
                [ParserTarget("latitudeOctaves")]
                public NumericParser<Int32> latitudeOctaves
                {
                    get { return mod.latitudeOctaves; }
                    set { mod.latitudeOctaves = value; }
                }

                // latitudePersistance
                [ParserTarget("latitudePersistance")]
                public NumericParser<Single> latitudePersistance
                {
                    get { return mod.latitudePersistance; }
                    set { mod.latitudePersistance = value; }
                }

                // latitudeSeed
                [ParserTarget("latitudeSeed")]
                public NumericParser<Int32> latitudeSeed
                {
                    get { return mod.latitudeSeed; }
                    set { mod.latitudeSeed = value; }
                }

                // latitudeSimplex
                [ParserTarget("latitudeSimplex")]
                public SimplexLoader latitudeSimplex
                {
                    get { return new SimplexLoader(mod.latitudeSimplex); }
                    set { mod.latitudeSimplex = value.simplex; }
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
                    set { mod.longitudeFrequency = value; }
                }

                // longitudeOctaves
                [ParserTarget("longitudeOctaves")]
                public NumericParser<Int32> longitudeOctaves
                {
                    get { return mod.longitudeOctaves; }
                    set { mod.longitudeOctaves = value; }
                }

                // longitudePersistance
                [ParserTarget("longitudePersistance")]
                public NumericParser<Single> longitudePersistance
                {
                    get { return mod.longitudePersistance; }
                    set { mod.longitudePersistance = value; }
                }

                // longitudeSeed
                [ParserTarget("longitudeSeed")]
                public NumericParser<Int32> longitudeSeed
                {
                    get { return mod.longitudeSeed; }
                    set { mod.longitudeSeed = value; }
                }

                // longitudeSimplex
                [ParserTarget("longitudeSimplex")]
                public SimplexLoader longitudeSimplex
                {
                    get { return new SimplexLoader(mod.longitudeSimplex); }
                    set { mod.longitudeSimplex = value.simplex; }
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
                public List<LandClassScatterLoader> scatters = new List<LandClassScatterLoader>();

                // List of landclasses
                public List<LandClassLoader> landClasses = new List<LandClassLoader>();

                // Apply event
                void IParserEventSubscriber.Apply(ConfigNode node) { }

                // Post Apply Event
                void IParserEventSubscriber.PostApply(ConfigNode node)
                {
                    // Load the LandClasses manually, to support patching
                    if (mod.landClasses != null) mod.landClasses.ToList().ForEach(c => landClasses.Add(new LandClassLoader(c)));
                    if (node.HasNode("landClasses"))
                    {
                        // Already patched classes
                        List<PQSLandControl.LandClass> patchedClasses = new List<PQSLandControl.LandClass>();

                        // Go through the nodes
                        foreach (ConfigNode lcNode in node.GetNode("landClasses").nodes)
                        {
                            // The Loader
                            LandClassLoader loader = null;

                            // Are there existing LandClasses?
                            if (landClasses.Count > 0)
                            {
                                // Attempt to find a LandClass we can edit that we have not edited before
                                loader = landClasses.Where(m => !patchedClasses.Contains(m.landClass) && (lcNode.HasValue("name") ? m.landClass.landClassName == lcNode.GetValue("name") : false))
                                                                 .FirstOrDefault();

                                // Load the Loader (lol)
                                if (loader != null)
                                {
                                    Parser.LoadObjectFromConfigurationNode(loader, lcNode, "Kopernicus");
                                    landClasses.Remove(loader);
                                    patchedClasses.Add(loader.landClass);
                                }
                            }

                            // If we can't patch a LandClass, create a new one
                            if (loader == null)
                            {
                                loader = Parser.CreateObjectFromConfigNode<LandClassLoader>(lcNode, "Kopernicus"); 
                            }

                            // Add the Loader to the List
                            if (!loader.delete.value)
                                landClasses.Add(loader);
                        }
                    }

                    // Load the Scatters manually, to support patching
                    if (mod.scatters != null)  mod.scatters.ToList().ForEach(s => scatters.Add(new LandClassScatterLoader(s)));
                    if (node.HasNode("scatters"))
                    {
                        // Already patched scatters
                        List<PQSLandControl.LandClassScatter> patchedScatters = new List<PQSLandControl.LandClassScatter>();

                        // Go through the nodes
                        foreach (ConfigNode scatterNode in node.GetNode("scatters").nodes)
                        {
                            // The Loader
                            LandClassScatterLoader loader = null;

                            // Are there existing Scatters?
                            if (scatters.Count > 0)
                            {
                                // Attempt to find a Scatter we can edit that we have not edited before
                                loader = scatters.Where(m => !patchedScatters.Contains(m.scatter) && (scatterNode.HasValue("name") ? m.scatter.scatterName == scatterNode.GetValue("name") : false))
                                                                 .FirstOrDefault();


                                // Load the Loader (lol)
                                if (loader != null)
                                {
                                    Parser.LoadObjectFromConfigurationNode(loader, scatterNode, "Kopernicus");
                                    scatters.Remove(loader);
                                    patchedScatters.Add(loader.scatter);
                                }
                            }

                            // If we can't patch a Scatter, create a new one
                            if (loader == null)
                            {
                                loader = Parser.CreateObjectFromConfigNode<LandClassScatterLoader>(scatterNode, "Kopernicus");
                            }

                            // Add the Loader to the List
                            if (!loader.delete.value)
                                scatters.Add(loader);
                        }
                    }

                    if (scatters.Count > 0)
                        mod.scatters = scatters.Select(s => s.scatter).ToArray();
                    else
                        mod.scatters = new PQSLandControl.LandClassScatter[0];
                    if (landClasses.Count > 0)
                        mod.landClasses = landClasses.Select(c => c.landClass).ToArray();
                    else
                        mod.landClasses = new PQSLandControl.LandClass[0];

                    // Assign each scatter amount with their corresponding scatter
                    foreach (PQSLandControl.LandClass landClass in mod.landClasses)
                    {
                        foreach (PQSLandControl.LandClassScatterAmount amount in landClass.scatter)
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
                    }

                    // Add Colliders to the scatters
                    foreach (PQSLandControl.LandClassScatter scatter in mod.scatters)
                    {
                        // If nothing's there, abort
                        if (!scatters.Any(s => s.scatter.scatterName == scatter.scatterName))
                            continue;

                        // Get the Loader
                        LandClassScatterLoader loader = scatters.First(s => s.scatter.scatterName == scatter.scatterName);

                        // Create the Scatter-Parent
                        GameObject scatterParent = new GameObject("Scatter " + scatter.scatterName);
                        scatterParent.transform.parent = mod.sphere.transform;
                        scatterParent.transform.localPosition = Vector3.zero;
                        scatterParent.transform.localRotation = Quaternion.identity;
                        scatterParent.transform.localScale = Vector3.one;

                        // Add the ScatterExtension
                        Scatter.CreateInstance(scatterParent, loader.science, loader.collide, loader.experiment);
                    }
                }

                // Create defaults
                public override void Create()
                {
                    base.Create();

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
                }
            }
        }
    }
}
