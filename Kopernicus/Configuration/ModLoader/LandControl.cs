/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class LandControl : ModLoader, IParserEventSubscriber
            {
                private class SimplexLoader
                {
                    public Simplex simplex;

                    // The frequency of the simplex noise
                    [ParserTarget("frequency", optional = true)]
                    private NumericParser<double> frequency
                    {
                        set { simplex.frequency = value.value; }
                    }
                    
                    // Octaves of the simplex noise
                    [ParserTarget("octaves", optional = true)]
                    private NumericParser<double> octaves
                    {
                        set { simplex.octaves = value.value; }
                    }

                    // Persistence of the simplex noise
                    [ParserTarget("persistence", optional = true)]
                    private NumericParser<double> persistence
                    {
                        set { simplex.persistence = value.value; }
                    }

                    // The seed of the simplex noise
                    [ParserTarget("seed", optional = true)]
                    private NumericParser<int> seed
                    {
                        set { simplex.seed = value.value; }
                    }

                    public SimplexLoader()
                    {
                        simplex = new Simplex(0, 1, 1, 1);
                    }
                }

                private class LandClassScatterLoader : IParserEventSubscriber
                {
                    private enum ScatterMaterialType
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
                    private class StockMaterialParser : IParsable
                    {
                        public Material value;
                        public void SetFromString(string s)
                        {
                            string materialName = Regex.Replace(s, "BUILTIN/", "");
                            value = UnityEngine.Resources.FindObjectsOfTypeAll<Material>().Where(material => material.name == materialName).FirstOrDefault();
                        }
                    }

                    [PreApply]
                    [ParserTarget("materialType", optional = true)]
                    private EnumParser<ScatterMaterialType> materialType
                    {
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
                    [ParserTarget("delete", optional = true)]
                    public NumericParser<bool> delete = new NumericParser<bool>(false);

                    // Should we add colliders to the scatter=
                    [ParserTarget("collide", optional = true)]
                    public NumericParser<bool> collide = new NumericParser<bool>(false);

                    // Should we add Science to the Scatter?
                    [ParserTarget("science", optional = true)]
                    public NumericParser<bool> science = new NumericParser<bool>(false);

                    // ConfigNode that stores the Experiment
                    public ConfigNode experiment;

                    // Custom scatter material
                    [ParserTarget("Material", optional = true, allowMerge = true)]
                    private Material customMaterial;

                    // The mesh
                    [ParserTarget("mesh", optional = true)]
                    private MeshParser baseMesh
                    {
                        set { scatter.baseMesh = value.mesh; }
                    }

                    // castShadows
                    [ParserTarget("castShadows", optional = true)]
                    private NumericParser<bool> castShadows
                    {
                        set { scatter.castShadows = value.value; }
                    }

                    // densityFactor
                    [ParserTarget("densityFactor", optional = true)]
                    private NumericParser<double> densityFactor
                    {
                        set { scatter.densityFactor = value.value; }
                    }

                    // Stock material
                    [ParserTarget("material", optional = true)]
                    private StockMaterialParser material
                    {
                        set { scatter.material = value.value; }
                    }

                    // maxCache
                    [ParserTarget("maxCache", optional = true)]
                    private NumericParser<int> maxCache
                    {
                        set { scatter.maxCache = value.value; }
                    }

                    // maxCacheDelta
                    [ParserTarget("maxCacheDelta", optional = true)]
                    private NumericParser<int> maxCacheDelta
                    {
                        set { scatter.maxCacheDelta = value.value; }
                    }

                    // maxLevelOffset
                    [ParserTarget("maxLevelOffset", optional = true)]
                    private NumericParser<int> maxLevelOffset
                    {
                        set { scatter.maxLevelOffset = value.value; }
                    }

                    // maxScale
                    [ParserTarget("maxScale", optional = true)]
                    private NumericParser<float> maxScale
                    {
                        set { scatter.maxScale = value.value; }
                    }

                    // maxScatter
                    [ParserTarget("maxScatter", optional = true)]
                    private NumericParser<int> maxScatter
                    {
                        set { scatter.maxScatter = value.value; }
                    }

                    // maxSpeed
                    [ParserTarget("maxSpeed", optional = true)]
                    private NumericParser<double> maxSpeed
                    {
                        set { scatter.maxSpeed = value.value; }
                    }

                    // minScale
                    [ParserTarget("minScale", optional = true)]
                    private NumericParser<float> minScale
                    {
                        set { scatter.minScale = value.value; }
                    }

                    // recieveShadows
                    [ParserTarget("recieveShadows", optional = true)]
                    private NumericParser<bool> recieveShadows
                    {
                        set { scatter.recieveShadows = value.value; }
                    }

                    // Scatter name
                    [ParserTarget("name")]
                    private string scatterName
                    {
                        set { scatter.scatterName = value; }
                    }

                    // Scatter seed
                    [ParserTarget("seed", optional = true)]
                    private NumericParser<int> seed
                    {
                        set { scatter.seed = value.value; }
                    }

                    // verticalOffset
                    [ParserTarget("verticalOffset", optional = true)]
                    private NumericParser<float> verticalOffset
                    {
                        set { scatter.verticalOffset = value.value; }
                    }

                    public LandClassScatterLoader()
                    {
                        scatter = new PQSLandControl.LandClassScatter();

                        // Initialize default parameters
                        scatter.maxCache = 512;
                        scatter.maxCacheDelta = 32;
                        scatter.maxSpeed = 1000;
                    }

                    public LandClassScatterLoader(PQSLandControl.LandClassScatter scatter)
                    {
                        this.scatter = scatter;
                    }

                    public void Apply(ConfigNode node) 
                    {
                        if (node.HasNode("EXPERIMENT"))
                            experiment = node.GetNode("EXPERIMENT");
                    }

                    public void PostApply(ConfigNode node)
                    {
                        // If defined, use custom material
                        if (customMaterial != null) scatter.material = customMaterial;
                    }
                }

                private class LandClassScatterAmountLoader
                {
                    public PQSLandControl.LandClassScatterAmount scatterAmount;

                    // density
                    [ParserTarget("density", optional = true)]
                    private NumericParser<double> density
                    {
                        set { scatterAmount.density = value.value; }
                    }

                    // The name of the scatter used
                    [ParserTarget("scatterName")]
                    private String scatterName
                    {
                        set { scatterAmount.scatterName = value; }
                    }

                    public LandClassScatterAmountLoader()
                    {
                        scatterAmount = new PQSLandControl.LandClassScatterAmount();
                    }
                }
                
                private class LerpRangeLoader
                {
                    public PQSLandControl.LerpRange lerpRange;

                    // endEnd
                    [ParserTarget("endEnd", optional = true)]
                    private NumericParser<double> endEnd
                    {
                        set { lerpRange.endEnd = value.value; }
                    }

                    // endStart
                    [ParserTarget("endStart", optional = true)]
                    private NumericParser<double> endStart
                    {
                        set { lerpRange.endStart = value.value; }
                    }

                    // startEnd
                    [ParserTarget("startEnd", optional = true)]
                    private NumericParser<double> startEnd
                    {
                        set { lerpRange.startEnd = value.value; }
                    }

                    // startStart
                    [ParserTarget("startStart", optional = true)]
                    private NumericParser<double> startStart
                    {
                        set { lerpRange.startStart = value.value; }
                    }

                    public LerpRangeLoader()
                    {
                        lerpRange = new PQSLandControl.LerpRange();
                    }
                }

                private class LandClassLoader : IParserEventSubscriber
                {
                    public PQSLandControl.LandClass landClass;

                    // Should we delete the LandClass?
                    [ParserTarget("delete", optional = true)]
                    public NumericParser<bool> delete = new NumericParser<bool>(false);

                    // alterApparentHeight
                    [ParserTarget("alterApparentHeight", optional = true)]
                    private NumericParser<float> alterApparentHeight
                    {
                        set { landClass.alterApparentHeight = value.value; }
                    }

                    // alterRealHeight
                    [ParserTarget("alterRealHeight", optional = true)]
                    private NumericParser<double> alterRealHeight
                    {
                        set { landClass.alterRealHeight = value.value; }
                    }

                    // altitudeRange
                    [ParserTarget("altitudeRange", optional = true)]
                    private LerpRangeLoader altitudeRange
                    {
                        set { landClass.altitudeRange = value.lerpRange; }
                    }

                    // color
                    [ParserTarget("color", optional = true)]
                    private ColorParser color
                    {
                        set { landClass.color = value.value; }
                    }

                    // coverageBlend
                    [ParserTarget("coverageBlend", optional = true)]
                    private NumericParser<float> coverageBlend
                    {
                        set { landClass.coverageBlend = value.value; }
                    }

                    // coverageFrequency
                    [ParserTarget("coverageFrequency", optional = true)]
                    private NumericParser<float> coverageFrequency
                    {
                        set { landClass.coverageFrequency = value.value; }
                    }

                    // coverageOctaves
                    [ParserTarget("coverageOctaves", optional = true)]
                    private NumericParser<int> coverageOctaves
                    {
                        set { landClass.coverageOctaves = value.value; }
                    }

                    // coveragePersistance
                    [ParserTarget("coveragePersistance", optional = true)]
                    private NumericParser<float> coveragePersistance
                    {
                        set { landClass.coveragePersistance = value.value; }
                    }

                    // coverageSeed
                    [ParserTarget("coverageSeed", optional = true)]
                    private NumericParser<int> coverageSeed
                    {
                        set { landClass.coverageSeed = value.value; }
                    }

                    // coverageSimplex
                    [ParserTarget("coverageSimplex", optional = true)]
                    private SimplexLoader coverageSimplex
                    {
                        set { landClass.coverageSimplex = value.simplex; }
                    }

                    // The name of the landclass
                    [ParserTarget("name", optional = true)]
                    private string landClassName
                    {
                        set { landClass.landClassName = value; }
                    }

                    // latDelta
                    [ParserTarget("latDelta", optional = true)]
                    private NumericParser<double> latDelta
                    {
                        set { landClass.latDelta = value.value; }
                    }

                    // latitudeDouble
                    [ParserTarget("latitudeDouble", optional = true)]
                    private NumericParser<bool> latitudeDouble
                    {
                        set { landClass.latitudeDouble = value.value; }
                    }

                    // latitudeDoubleRange
                    [ParserTarget("latitudeDoubleRange", optional = true)]
                    private LerpRangeLoader latitudeDoubleRange
                    {
                        set { landClass.latitudeDoubleRange = value.lerpRange; }
                    }

                    // latitudeRange
                    [ParserTarget("latitudeRange", optional = true)]
                    private LerpRangeLoader latitudeRange
                    {
                        set { landClass.latitudeRange = value.lerpRange; }
                    }

                    // lonDelta
                    [ParserTarget("lonDelta", optional = true)]
                    private NumericParser<double> lonDelta
                    {
                        set { landClass.lonDelta = value.value; }
                    }

                    // longitudeRange
                    [ParserTarget("longitudeRange", optional = true)]
                    private LerpRangeLoader longitudeRange
                    {
                        set { landClass.longitudeRange = value.lerpRange; }
                    }

                    // minimumRealHeight
                    [ParserTarget("minimumRealHeight", optional = true)]
                    private NumericParser<double> minimumRealHeight
                    {
                        set { landClass.minimumRealHeight = value.value; }
                    }

                    // noiseBlend
                    [ParserTarget("noiseBlend", optional = true)]
                    private NumericParser<float> noiseBlend
                    {
                        set { landClass.noiseBlend = value.value; }
                    }

                    // noiseColor
                    [ParserTarget("noiseColor", optional = true)]
                    private ColorParser noiseColor
                    {
                        set { landClass.noiseColor = value.value; }
                    }

                    // noiseFrequency
                    [ParserTarget("noiseFrequency", optional = true)]
                    private NumericParser<float> noiseFrequency
                    {
                        set { landClass.noiseFrequency = value.value; }
                    }

                    // noiseOctaves
                    [ParserTarget("noiseOctaves", optional = true)]
                    private NumericParser<int> noiseOctaves
                    {
                        set { landClass.noiseOctaves = value.value; }
                    }

                    // noisePersistance
                    [ParserTarget("noisePersistance", optional = true)]
                    private NumericParser<float> noisePersistance
                    {
                        set { landClass.noisePersistance = value.value; }
                    }

                    // noiseSeed
                    [ParserTarget("noiseSeed", optional = true)]
                    private NumericParser<int> noiseSeed
                    {
                        set { landClass.noiseSeed = value.value; }
                    }

                    // noiseSimplex
                    [ParserTarget("noiseSimplex", optional = true)]
                    private SimplexLoader noiseSimplex
                    {
                        set { landClass.noiseSimplex = value.simplex; }
                    }

                    // List of scatters used
                    [ParserTargetCollection("scatters", optional = true, nameSignificance = NameSignificance.None)]
                    private List<LandClassScatterAmountLoader> scatter = new List<LandClassScatterAmountLoader>();

                    public void Apply(ConfigNode node)
                    {

                    }

                    public void PostApply(ConfigNode node)
                    {
                        landClass.scatter = this.scatter.Select(s => s.scatterAmount).ToArray();
                    }

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

                    public LandClassLoader(PQSLandControl.LandClass landClass)
                    {
                        this.landClass = landClass;
                    }
                }

                // Actual PQS mod we are loading
                private PQSLandControl _mod;

                // altitudeBlend
                [ParserTarget("altitudeBlend", optional = true)]
                private NumericParser<float> altitudeBlend
                {
                    set { _mod.altitudeBlend = value.value; }
                }

                // altitudeFrequency
                [ParserTarget("altitudeFrequency", optional = true)]
                private NumericParser<float> altitudeFrequency
                {
                    set { _mod.altitudeFrequency = value.value; }
                }

                // altitudeOctaves
                [ParserTarget("altitudeOctaves", optional = true)]
                private NumericParser<int> altitudeOctaves
                {
                    set { _mod.altitudeOctaves = value.value; }
                }

                // altitudePersistance
                [ParserTarget("altitudePersistance", optional = true)]
                private NumericParser<float> altitudePersistance
                {
                    set { _mod.altitudePersistance = value.value; }
                }

                // altitudeSeed
                [ParserTarget("altitudeSeed", optional = true)]
                private NumericParser<int> altitudeSeed
                {
                    set { _mod.altitudeSeed = value.value; }
                }

                // altitudeSimplex
                [ParserTarget("altitudeSimplex", optional = true)]
                private SimplexLoader altitudeSimplex
                {
                    set { _mod.altitudeSimplex = value.simplex; }
                }

                // createColors
                [ParserTarget("createColors", optional = true)]
                private NumericParser<bool> createColors
                {
                    set { _mod.createColors = value.value; }
                }

                // createScatter
                [ParserTarget("createScatter", optional = true)]
                private NumericParser<bool> createScatter
                {
                    set { _mod.createScatter = value.value; }
                }

                // heightMap
                [ParserTarget("heightMap", optional = true)]
                private MapSOParser_GreyScale<MapSO> heightMap
                {
                    set { _mod.heightMap = value.value; }
                }

                // latitudeBlend
                [ParserTarget("latitudeBlend", optional = true)]
                private NumericParser<float> latitudeBlend
                {
                    set { _mod.latitudeBlend = value.value; }
                }

                // latitudeFrequency
                [ParserTarget("latitudeFrequency", optional = true)]
                private NumericParser<float> latitudeFrequency
                {
                    set { _mod.latitudeFrequency = value.value; }
                }

                // latitudeOctaves
                [ParserTarget("latitudeOctaves", optional = true)]
                private NumericParser<int> latitudeOctaves
                {
                    set { _mod.latitudeOctaves = value.value; }
                }

                // latitudePersistance
                [ParserTarget("latitudePersistance", optional = true)]
                private NumericParser<float> latitudePersistance
                {
                    set { _mod.latitudePersistance = value.value; }
                }

                // latitudeSeed
                [ParserTarget("latitudeSeed", optional = true)]
                private NumericParser<int> latitudeSeed
                {
                    set { _mod.latitudeSeed = value.value; }
                }

                // latitudeSimplex
                [ParserTarget("latitudeSimplex", optional = true)]
                private SimplexLoader latitudeSimplex
                {
                    set { _mod.latitudeSimplex = value.simplex; }
                }

                // longitudeBlend
                [ParserTarget("longitudeBlend", optional = true)]
                private NumericParser<float> longitudeBlend
                {
                    set { _mod.longitudeBlend = value.value; }
                }

                // longitudeFrequency
                [ParserTarget("longitudeFrequency", optional = true)]
                private NumericParser<float> longitudeFrequency
                {
                    set { _mod.longitudeFrequency = value.value; }
                }

                // longitudeOctaves
                [ParserTarget("longitudeOctaves", optional = true)]
                private NumericParser<int> longitudeOctaves
                {
                    set { _mod.longitudeOctaves = value.value; }
                }

                // longitudePersistance
                [ParserTarget("longitudePersistance", optional = true)]
                private NumericParser<float> longitudePersistance
                {
                    set { _mod.longitudePersistance = value.value; }
                }

                // longitudeSeed
                [ParserTarget("longitudeSeed", optional = true)]
                private NumericParser<int> longitudeSeed
                {
                    set { _mod.longitudeSeed = value.value; }
                }

                // longitudeSimplex
                [ParserTarget("longitudeSimplex", optional = true)]
                private SimplexLoader longitudeSimplex
                {
                    set { _mod.longitudeSimplex = value.simplex; }
                }

                // useHeightMap
                [ParserTarget("useHeightMap", optional = true)]
                private NumericParser<bool> useHeightMap
                {
                    set { _mod.useHeightMap = value.value; }
                }

                // vHeightMax
                [ParserTarget("vHeightMax", optional = true)]
                private NumericParser<float> vHeightMax
                {
                    set { _mod.vHeightMax = value.value; }
                }

                // List of scatters
                private List<LandClassScatterLoader> scatters = new List<LandClassScatterLoader>();

                // List of landclasses
                private List<LandClassLoader> landClasses = new List<LandClassLoader>();

                void IParserEventSubscriber.Apply(ConfigNode node)
                {

                }

                void IParserEventSubscriber.PostApply(ConfigNode node)
                {
                    // Load the LandClasses manually, to support patching
                    if (node.HasNode("landClasses"))
                    {
                        // Already patched classes
                        List<PQSLandControl.LandClass> patchedClasses = new List<PQSLandControl.LandClass>();
                        if (_mod.landClasses != null)
                            _mod.landClasses.ToList().ForEach(c => landClasses.Add(new LandClassLoader(c)));

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
                                    Parser.LoadObjectFromConfigurationNode(loader, lcNode);
                                    if (loader.delete.value)
                                        landClasses.Remove(loader);
                                    else
                                        patchedClasses.Add(loader.landClass);
                                }
                            }

                            // If we can't patch a LandClass, create a new one
                            if (loader == null)
                            {
                                loader = Parser.CreateObjectFromConfigNode(typeof(LandClassLoader), lcNode) as LandClassLoader; 
                            }

                            // Add the Loader to the List
                            landClasses.Add(loader);
                        }
                    }

                    // Load the Scatters manually, to support patching
                    if (node.HasNode("scatters"))
                    {
                        // Already patched scatters
                        List<PQSLandControl.LandClassScatter> patchedScatters = new List<PQSLandControl.LandClassScatter>();
                        if (_mod.scatters != null)
                            _mod.scatters.ToList().ForEach(s => scatters.Add(new LandClassScatterLoader(s)));

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
                                    Parser.LoadObjectFromConfigurationNode(loader, scatterNode);
                                    if (loader.delete.value)
                                        scatters.Remove(loader);
                                    else 
                                        patchedScatters.Add(loader.scatter);
                                }
                            }

                            // If we can't patch a Scatter, create a new one
                            if (loader == null)
                            {
                                loader = Parser.CreateObjectFromConfigNode(typeof(LandClassScatterLoader), scatterNode) as LandClassScatterLoader;
                            }

                            // Add the Loader to the List
                            scatters.Add(loader);
                        }
                    }

                    if (scatters.Count > 0)
                        _mod.scatters = scatters.Select(s => s.scatter).ToArray();
                    if (landClasses.Count > 0)
                        _mod.landClasses = landClasses.Select(c => c.landClass).ToArray();

                    // Assign each scatter amount with their corresponding scatter
                    foreach (PQSLandControl.LandClass landClass in _mod.landClasses)
                    {
                        foreach (PQSLandControl.LandClassScatterAmount amount in landClass.scatter)
                        {
                            int i = 0;
                            while (i < _mod.scatters.Length)
                            {
                                if (_mod.scatters[i].scatterName.Equals(amount.scatterName)) break;
                                i++;
                            }

                            if (i < _mod.scatters.Length)
                            {
                                amount.scatterIndex = i;
                                amount.scatter = _mod.scatters[i];
                            }
                        }
                    }
                }

                // We need the Sphere-Transform :D
                public void SphereApply()
                {
                    // Add Colliders to the scatters
                    foreach (PQSLandControl.LandClassScatter scatter in _mod.scatters)
                    {
                        // If nothing's there, abort
                        if (!scatters.Any(s => s.scatter.scatterName == scatter.scatterName))
                            continue;

                        // Get the Loader
                        LandClassScatterLoader loader = scatters.First(s => s.scatter.scatterName == scatter.scatterName);

                        // Create the Scatter-Parent
                        GameObject scatterParent = new GameObject("Scatter " + scatter.scatterName);
                        scatterParent.transform.parent = _mod.sphere.transform;
                        scatterParent.transform.localPosition = Vector3.zero;
                        scatterParent.transform.localRotation = Quaternion.identity;
                        scatterParent.transform.localScale = Vector3.one;

                        // Add the ScatterExtension
                        ScatterExtension.CreateInstance(scatterParent, loader.science.value, loader.collide.value, loader.experiment);
                    }
                }

                public LandControl()
                {
                    // Create the base mod
                    GameObject modObject = new GameObject("LandControl");
                    modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSLandControl>();
                    base.mod = _mod;

                    // Initialize default parameters
                    _mod.altitudeSeed = 1;
                    _mod.altitudeOctaves = 1;
                    _mod.altitudePersistance = 1;
                    _mod.altitudeFrequency = 1;
                    _mod.latitudeSeed = 1;
                    _mod.latitudeOctaves = 1;
                    _mod.latitudePersistance = 1;
                    _mod.latitudeFrequency = 1;
                    _mod.longitudeSeed = 1;
                    _mod.longitudeOctaves = 1;
                    _mod.longitudePersistance = 1;
                    _mod.longitudeFrequency = 1;
                }

                public LandControl(PQSMod template)
                {
                    _mod = template as PQSLandControl;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
            }

            // Component to add Colliders and Science to the Scatter-Objects
            public class ScatterExtension : MonoBehaviour
            {
                // Experiment ID for this Scatter
                public ConfigNode experiment;

                // Should we add Science?
                public bool science = false;

                // Should we add Colliders?
                public bool colliders = false;

                // Create a new ScatterExtension
                public static ScatterExtension CreateInstance(GameObject o, bool science, bool collide, ConfigNode exp)
                {
                    ScatterExtension scatter = o.AddComponent<ScatterExtension>();
                    scatter.science = science;
                    scatter.colliders = collide;
                    scatter.experiment = exp;
                    return scatter;
                }

                // Register us as the parental Object for the Scatter
                public void Start()
                {
                    PQSLandControl landControl = transform.parent.GetComponentInChildren<PQSLandControl>();
                    PQSLandControl.LandClassScatter scatter = landControl.scatters.Where(s => s.scatterName == name.Split(' ').Last()).First();
                    scatter.GetType().GetField("scatterParent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(scatter, gameObject);

                    // The ConfigNode is lost, so find it again!
                    PSystemBody body = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, transform.parent.name);
                    experiment = body.pqsVersion.gameObject.GetChild(name).GetComponent<ScatterExtension>().experiment;
                }

                private bool isDone = false;

                // Add colliders to all the Scatters
                public void Update()
                {
                    // If we already added Stuff, abort
                    if (isDone)
                        return;

                    // If there's nothing to do, abort
                    if (transform.childCount == 0)
                        return;

                    // If we should add Science, add it
                    if (science)
                        ScatterScience.CreateInstance(gameObject, experiment);

                    foreach (Transform t in transform)
                    {
                        // If we should add colliders, add them
                        MeshCollider collider = t.gameObject.AddComponent<MeshCollider>();
                        if (colliders)
                        {
                            collider.sharedMesh = t.gameObject.GetComponent<MeshFilter>().sharedMesh;
                            collider.sharedMesh.Optimize();
                        }

                        // Done
                        isDone = true;
                    }
                }
            }

            // Component to add SCIENCE!! to the GroundScatter
            public class ScatterScience : MonoBehaviour
            {
                // Contains the patched vessels (=> Kerbals)
                public static List<Guid> vesselID = new List<Guid>();

                // Contains the blocked List of Base-Events
                public List<BaseEvent> blocked = new List<BaseEvent>();

                // Which experiment should we run?
                public ConfigNode experimentNode;

                // The experiment
                public ModuleScienceExperiment experiment;

                // Create a new ScatterScience Instance
                public static ScatterScience CreateInstance(GameObject o, ConfigNode exp)
                {
                    ScatterScience science = o.AddComponent<ScatterScience>();
                    science.experimentNode = exp;
                    return science;
                } 

                // Patch generated vessels.
                public void Update()
                {
                    if (!FlightGlobals.ActiveVessel)
                        return;

                    if (!FlightGlobals.ActiveVessel.isEVA)
                        return;

                    if (FlightGlobals.ActiveVessel.evaController.part.FindModulesImplementing<ModuleScienceExperiment>()
                        .Where(e => e.experimentID == experimentNode.GetValue("experimentID")).Count() == 0 && 
                        !vesselID.Contains(FlightGlobals.ActiveVessel.id) &&
                        !FlightGlobals.ActiveVessel.packed)
                        AddScienceExperiment();

                    if (experiment == null)
                        return;

                    ToggleExperiment(Physics.OverlapSphere(FlightGlobals.ship_position, 10f).Where(c => c.gameObject.transform.parent.name == name).Count() > 0);
                }

                public void AddScienceExperiment()
                {
                    // If the Node is null, abort
                    if (experimentNode == null)
                        return;

                    // Create the ScienceExperiment
                    Part kerbal = FlightGlobals.ActiveVessel.evaController.part;
                    experiment = kerbal.AddModule(typeof(ModuleScienceExperiment).Name) as ModuleScienceExperiment;

                    // I can't find a function that loads the module from the config node... :/ Doing it manually
                    Type type = experiment.GetType();
                    foreach (ConfigNode.Value value in experimentNode.values)
                    {
                        try
                        {
                            FieldInfo field = type.GetField(value.name);
                            if (field.FieldType == typeof(string))
                                field.SetValue(experiment, value.value);
                            else if (field.FieldType.GetMethod("Parse") != null)
                                field.SetValue(experiment, field.FieldType.GetMethod("Parse").Invoke(null, new object[] { value.value }));
                        }
                        catch { }
                    }

                    // Deactivate some things
                    experiment.resettable = false;

                    // Start the experiment
                    experiment.OnStart(PartModule.StartState.None);
                    vesselID.Add(kerbal.vessel.id);
                }

                public void ToggleExperiment(bool state)
                {
                    if (state && blocked.Contains(experiment.Events["DeployExperiment"]))
                    {
                        experiment.Events["DeployExperiment"].guiActive = true;
                        blocked.Remove(experiment.Events["DeployExperiment"]);
                    } 
                    else if (!state && !blocked.Contains(experiment.Events["DeployExperiment"]))
                    {
                        experiment.Events["DeployExperiment"].guiActive = false;
                        blocked.Add(experiment.Events["DeployExperiment"]);
                    }
                }
            }
        }
    }
}
