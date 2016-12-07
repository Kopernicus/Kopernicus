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

using LibNoise.Unity.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class VertexPlanet : ModLoader<PQSMod_VertexPlanet>, IParserEventSubscriber
            {
                // Loader for the SimplexWrapper
                public class SimplexWrapper : IParserEventSubscriber
                {
                    // Loaded wrapper
                    public PQSMod_VertexPlanet.SimplexWrapper wrapper;

                    // deformity
                    [ParserTarget("deformity")]
                    public NumericParser<double> deformity
                    {
                        get { return wrapper.deformity; }
                        set { wrapper.deformity = value; }
                    }

                    // frequency
                    [ParserTarget("frequency")]
                    public NumericParser<double> frequency
                    {
                        get { return wrapper.frequency; }
                        set { wrapper.frequency = value; }
                    }

                    // octaves
                    [ParserTarget("octaves")]
                    public NumericParser<double> octaves
                    {
                        get { return wrapper.octaves; }
                        set { wrapper.octaves = value; }
                    }

                    // persistance
                    [ParserTarget("persistance")]
                    public NumericParser<double> persistance
                    {
                        get { return wrapper.persistance; }
                        set { wrapper.persistance = value; }
                    }

                    // seed
                    [ParserTarget("seed")]
                    public NumericParser<int> seed = new NumericParser<int>(0);

                    // Apply Event
                    void IParserEventSubscriber.Apply(ConfigNode node) { }

                    // Post Apply Event
                    void IParserEventSubscriber.PostApply(ConfigNode node)
                    {
                        wrapper.Setup(seed);
                    }

                    // Default constructor
                    public SimplexWrapper()
                    {
                        wrapper = new PQSMod_VertexPlanet.SimplexWrapper(0, 0, 0, 0);
                    }

                    // Runtime constructor
                    public SimplexWrapper(PQSMod_VertexPlanet.SimplexWrapper simplex)
                    {
                        wrapper = simplex;
                    }
                }

                // Loader for Noise
                public class NoiseModWrapper : IParserEventSubscriber
                {
                    // The loaded noise
                    public PQSMod_VertexPlanet.NoiseModWrapper wrapper;

                    // Parser for RiggedMultifractal (Yes, you can use any ModuleBase, but I don't want to code them all...)
                    public class RiggedParser
                    {
                        // Noise
                        public RiggedMultifractal noise;

                        // frequency
                        [ParserTarget("frequency")]
                        public NumericParser<double> frequency
                        {
                            get { return noise.Frequency; }
                            set { noise.Frequency = value; }
                        }

                        // lacunarity
                        [ParserTarget("lacunarity")]
                        public NumericParser<double> lacunarity
                        {
                            get { return noise.Lacunarity; }
                            set { noise.Lacunarity = value; }
                        }

                        // octaveCount
                        [ParserTarget("octaveCount")]
                        public NumericParser<int> octaveCount
                        {
                            get { return noise.OctaveCount; }
                            set { noise.OctaveCount = value; }
                        }

                        // quality
                        [ParserTarget("quality")]
                        public EnumParser<LibNoise.Unity.QualityMode> quality
                        {
                            get { return noise.Quality; }
                            set { noise.Quality = value; }
                        }

                        // seed
                        [ParserTarget("seed")]
                        public NumericParser<int> seed
                        {
                            get { return noise.Seed; }
                            set { noise.Seed = value; }
                        }

                        // Default Constructor
                        public RiggedParser()
                        {
                            noise = new RiggedMultifractal();
                        }

                        // Runtime Constructor
                        public RiggedParser(RiggedMultifractal rigged)
                        {
                            noise = rigged;
                        }
                        
                    }

                    // deformity
                    [ParserTarget("deformity")]
                    public NumericParser<double> deformity
                    {
                        get { return wrapper.deformity; }
                        set { wrapper.deformity = value; }
                    }

                    // frequency
                    [ParserTarget("frequency")]
                    public NumericParser<double> frequency
                    {
                        get { return wrapper.frequency; }
                        set { wrapper.frequency = value; }
                    }

                    // octaves
                    [ParserTarget("octaves")]
                    public NumericParser<int> octaves
                    {
                        get { return wrapper.octaves; }
                        set { wrapper.octaves = value; }
                    }

                    // persistance
                    [ParserTarget("persistance")]
                    public NumericParser<double> persistance
                    {
                        get { return wrapper.persistance; }
                        set { wrapper.persistance = value; }
                    }

                    // seed
                    [ParserTarget("seed")]
                    public NumericParser<int> seedLoader
                    {
                        get { return wrapper.seed; }
                        set { wrapper.seed = value; }
                    }

                    // noise
                    [ParserTarget("Noise", allowMerge = true)]
                    public RiggedParser riggedNoise { get; set; }

                    // Apply Event
                    void IParserEventSubscriber.Apply(ConfigNode node)
                    {
                        if (wrapper.noise is RiggedMultifractal)
                            riggedNoise = new RiggedParser((RiggedMultifractal)wrapper.noise);
                    }

                    // Post Apply Event
                    void IParserEventSubscriber.PostApply(ConfigNode node)
                    {
                        wrapper.Setup(riggedNoise.noise);
                    }

                    // Default constructor
                    public NoiseModWrapper()
                    {
                        wrapper = new PQSMod_VertexPlanet.NoiseModWrapper(0, 0, 0, 0);
                    }

                    // Runtime Constructor
                    public NoiseModWrapper(PQSMod_VertexPlanet.NoiseModWrapper noise)
                    {
                        wrapper = noise;
                    }
                }

                // Land class loader 
                public class LandClassLoader : IParserEventSubscriber
                {
                    // Land class object
                    public PQSMod_VertexPlanet.LandClass landClass;

                    // Name of the class
                    [ParserTarget("name")]
                    public string name
                    {
                        get { return landClass.name; }
                        set { landClass.name = value; }
                    }

                    // Should we delete this
                    [ParserTarget("delete")]
                    public NumericParser<bool> delete = new NumericParser<bool>(false);

                    // baseColor
                    [ParserTarget("baseColor")]
                    public ColorParser baseColor
                    {
                        get { return landClass.baseColor; }
                        set { landClass.baseColor = value; }
                    }

                    // colorNoise
                    [ParserTarget("colorNoise")]
                    public ColorParser colorNoise
                    {
                        get { return landClass.colorNoise; }
                        set { landClass.colorNoise = value; }
                    }

                    // colorNoiseAmount
                    [ParserTarget("colorNoiseAmount")]
                    public NumericParser<double> colorNoiseAmount
                    {
                        get { return landClass.colorNoiseAmount; }
                        set { landClass.colorNoiseAmount = value; }
                    }

                    // colorNoiseMap
                    [ParserTarget("SimplexNoiseMap", allowMerge = true)]
                    public SimplexWrapper colorNoiseMap { get; set; }

                    // fractalEnd
                    [ParserTarget("fractalEnd")]
                    public NumericParser<double> fractalEnd
                    {
                        get { return landClass.fractalEnd; }
                        set { landClass.fractalEnd = value; }
                    }

                    // fractalStart
                    [ParserTarget("fractalStart")]
                    public NumericParser<double> fractalStart
                    {
                        get { return landClass.fractalStart; }
                        set { landClass.fractalStart = value; }
                    }

                    // lerpToNext
                    [ParserTarget("lerpToNext")]
                    public NumericParser<bool> lerpToNext
                    {
                        get { return landClass.lerpToNext; }
                        set { landClass.lerpToNext = value; }
                    }

                    // fractalDelta
                    [ParserTarget("fractalDelta")]
                    public NumericParser<double> fractalDelta
                    {
                        get { return landClass.fractalDelta; }
                        set { landClass.fractalDelta = value; }
                    }

                    // endHeight
                    [ParserTarget("endHeight")]
                    public NumericParser<double> endHeight
                    {
                        get { return landClass.endHeight; }
                        set { landClass.endHeight = value; }
                    }

                    // startHeight
                    [ParserTarget("startHeight")]
                    public NumericParser<double> startHeight
                    {
                        get { return landClass.startHeight; }
                        set { landClass.startHeight = value; }
                    }

                    // Apply Event
                    void IParserEventSubscriber.Apply(ConfigNode node) { }

                    // Post Apply Event
                    void IParserEventSubscriber.PostApply(ConfigNode node)
                    {
                        landClass.colorNoiseMap = colorNoiseMap.wrapper;
                    }

                    // Default constructor
                    public LandClassLoader()
                    {
                        landClass = new PQSMod_VertexPlanet.LandClass("class", 0.0, 0.0, Color.white, Color.white, 0.0);
                    }

                    // Runtime constructor
                    public LandClassLoader(PQSMod_VertexPlanet.LandClass land)
                    {
                        landClass = land;
                        colorNoiseMap = new SimplexWrapper(landClass.colorNoiseMap);
                    }
                }

                // buildHeightColors
                [ParserTarget("buildHeightColors")]
                public NumericParser<bool> buildHeightColors 
                {
                    get { return mod.buildHeightColors; }
                    set { mod.buildHeightColors = value; }
                }

                // colorDeformity
                [ParserTarget("colorDeformity")]
                public NumericParser<double> colorDeformity
                {
                    get { return mod.colorDeformity; }
                    set { mod.colorDeformity = value; }
                }

                // continental
                [ParserTarget("ContinentalSimplex", allowMerge = true)]
                public SimplexWrapper continental { get; set; }

                // continentalRuggedness
                [ParserTarget("RuggednessSimplex", allowMerge = true)]
                public SimplexWrapper continentalRuggedness { get; set; }

                // continentalSharpness
                [ParserTarget("SharpnessNoise", allowMerge = true)]
                public NoiseModWrapper continentalSharpness { get; set; }

                // continentalSharpnessMap
                [ParserTarget("SharpnessSimplexMap", allowMerge = true)]
                public SimplexWrapper continentalSharpnessMap { get; set; }

                // deformity
                [ParserTarget("deformity")]
                public NumericParser<double> deformity
                {
                    get { return mod.deformity; }
                    set { mod.deformity = value; }
                }

                // landClasses
                public List<LandClassLoader> landClasses = new List<LandClassLoader>();

                // oceanDepth
                [ParserTarget("oceanDepth")]
                public NumericParser<double> oceanDepth
                {
                    get { return mod.oceanDepth; }
                    set { mod.oceanDepth = value; }
                }

                // oceanLevel
                [ParserTarget("oceanLevel")]
                public NumericParser<double> oceanLevel
                {
                    get { return mod.oceanLevel; }
                    set { mod.oceanLevel = value; }
                }

                // oceanSnap
                [ParserTarget("oceanSnap")]
                public NumericParser<bool> oceanSnap
                {
                    get { return mod.oceanSnap; }
                    set { mod.oceanSnap = value; }
                }

                // oceanStep
                [ParserTarget("oceanStep")]
                public NumericParser<double> oceanStep
                {
                    get { return mod.oceanStep; }
                    set { mod.oceanStep = value; }
                }

                // seed
                [ParserTarget("seed")]
                public NumericParser<int> seed
                {
                    get { return mod.seed; }
                    set { mod.seed = value; }
                }

                // terrainRidgeBalance
                [ParserTarget("terrainRidgeBalance")]
                public NumericParser<double> terrainRidgeBalance
                {
                    get { return mod.terrainRidgeBalance; }
                    set { mod.terrainRidgeBalance = value; }
                }

                // terrainRidgesMax
                [ParserTarget("terrainRidgesMax")]
                public NumericParser<double> terrainRidgesMax
                {
                    get { return mod.terrainRidgesMax; }
                    set { mod.terrainRidgesMax = value; }
                }

                // terrainRidgesMin
                [ParserTarget("terrainRidgesMin")]
                public NumericParser<double> terrainRidgesMin
                {
                    get { return mod.terrainRidgesMin; }
                    set { mod.terrainRidgesMin = value; }
                }

                // terrainShapeEnd
                [ParserTarget("terrainShapeEnd")]
                public NumericParser<double> terrainShapeEnd
                {
                    get { return mod.terrainShapeEnd; }
                    set { mod.terrainShapeEnd = value; }
                }

                // terrainShapeStart
                [ParserTarget("terrainShapeStart")]
                public NumericParser<double> terrainShapeStart
                {
                    get { return mod.terrainShapeStart; }
                    set { mod.terrainShapeStart = value; }
                }

                // terrainSmoothing
                [ParserTarget("terrainSmoothing")]
                public NumericParser<double> terrainSmoothing
                {
                    get { return mod.terrainSmoothing; }
                    set { mod.terrainSmoothing = value; }
                }

                // terrainType
                [ParserTarget("TerrainTypeSimplex", allowMerge = true)]
                public SimplexWrapper terrainType { get; set; }
                
                // Apply Event
                void IParserEventSubscriber.Apply(ConfigNode node) { }

                // Post Apply Event
                void IParserEventSubscriber.PostApply(ConfigNode node)
                {
                    // Apply Simplex and NoiseMod
                    mod.continental = continental.wrapper;
                    mod.continentalRuggedness = continentalRuggedness.wrapper;
                    mod.continentalSharpness = continentalSharpness.wrapper;
                    mod.continentalSharpnessMap = continentalSharpnessMap.wrapper;
                    mod.terrainType = terrainType.wrapper;

                    // Load the LandClasses manually, to support patching
                    if (!node.HasNode("LandClasses"))
                        return;

                    // Already patched classes
                    List<PQSMod_VertexPlanet.LandClass> patchedClasses = new List<PQSMod_VertexPlanet.LandClass>();
                    if (mod.landClasses != null)
                        mod.landClasses.ToList().ForEach(c => landClasses.Add(new LandClassLoader(c)));

                    // Go through the nodes
                    foreach (ConfigNode lcNode in node.GetNode("LandClasses").nodes)
                    {
                        // The Loader
                        LandClassLoader loader = null;

                        // Are there existing LandClasses?
                        if (landClasses.Count > 0)
                        {
                            // Attempt to find a LandClass we can edit that we have not edited before
                            loader = landClasses.Where(m => !patchedClasses.Contains(m.landClass) && ((lcNode.HasValue("name") ? m.landClass.name == lcNode.GetValue("name") : true) || (lcNode.HasValue("index") ? landClasses.IndexOf(m).ToString() == lcNode.GetValue("index") : false)))
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

                    // Apply the landclasses
                    mod.landClasses = landClasses.Select(l => l.landClass).ToArray();
                }

                // Create the mod
                public override void Create(PQSMod_VertexPlanet _mod)
                {
                    base.Create(_mod);

                    // Create base types
                    continental = new SimplexWrapper(mod.continental);
                    continentalRuggedness = new SimplexWrapper(mod.continentalRuggedness);
                    continentalSharpness = new NoiseModWrapper(mod.continentalSharpness);
                    continentalSharpnessMap = new SimplexWrapper(mod.continentalSharpnessMap);
                    terrainType = new SimplexWrapper(mod.terrainType);
                }
            }
        }
    }
}

