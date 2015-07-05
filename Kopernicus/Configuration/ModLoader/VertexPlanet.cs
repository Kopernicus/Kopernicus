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
			public class VertexPlanet : ModLoader, IParserEventSubscriber
			{
				// Actual PQS mod we are loading
				private PQSMod_VertexPlanet _mod;

                private class SimplexWrapper : IParserEventSubscriber
                {
                    public PQSMod_VertexPlanet.SimplexWrapper wrapper;
                    private int seed;

                    // deformity
                    [ParserTarget("deformity", optional = true)]
                    private NumericParser<double> deformity
                    {
                        set { wrapper.deformity = value.value; }
                    }

                    // frequency
                    [ParserTarget("frequency", optional = true)]
                    private NumericParser<double> frequency
                    {
                        set { wrapper.frequency = value.value; }
                    }

                    // octaves
                    [ParserTarget("octaves", optional = true)]
                    private NumericParser<double> octaves
                    {
                        set { wrapper.octaves = value.value; }
                    }

                    // persistance
                    [ParserTarget("persistance", optional = true)]
                    private NumericParser<double> persistance
                    {
                        set { wrapper.persistance = value.value; }
                    }

                    // seed
                    [ParserTarget("seed", optional = true)]
                    private NumericParser<int> seedLoader
                    {
                        set { seed = value.value; }
                    }

                    void IParserEventSubscriber.Apply(ConfigNode node)
                    {

                    }

                    void IParserEventSubscriber.PostApply(ConfigNode node)
                    {
                        wrapper.Setup(seed);
                    }

                    public SimplexWrapper()
                    {
                        wrapper = new PQSMod_VertexPlanet.SimplexWrapper(0, 0, 0, 0);
                    }
                }

                private class NoiseModWrapper : IParserEventSubscriber
                {
                    public PQSMod_VertexPlanet.NoiseModWrapper wrapper;

                    private class RiggedParser : IParserEventSubscriber
                    {
                        public LibNoise.Unity.Generator.RiggedMultifractal noise;

                        // frequency
                        [ParserTarget("frequency", optional = true)]
                        private NumericParser<double> frequency
                        {
                            set { noise.Frequency = value.value; }
                        }

                        // lacunarity
                        [ParserTarget("lacunarity", optional = true)]
                        private NumericParser<double> lacunarity
                        {
                            set { noise.Lacunarity = value.value; }
                        }

                        // octaveCount
                        [ParserTarget("octaveCount", optional = true)]
                        private NumericParser<int> octaveCount
                        {
                            set { noise.OctaveCount = value.value; }
                        }

                        // quality
                        [ParserTarget("quality", optional = true)]
                        private EnumParser<LibNoise.Unity.QualityMode> quality
                        {
                            set { noise.Quality = value.value; }
                        }

                        // seed
                        [ParserTarget("seed", optional = true)]
                        private NumericParser<int> seed
                        {
                            set { noise.Seed = value.value; }
                        }

                        void IParserEventSubscriber.Apply(ConfigNode node)
                        {

                        }

                        void IParserEventSubscriber.PostApply(ConfigNode node)
                        {
                            
                        }

                        public RiggedParser()
                        {
                            noise = new RiggedMultifractal();
                        }
                        
                    }

                    // deformity
                    [ParserTarget("deformity", optional = true)]
                    private NumericParser<double> deformity
                    {
                        set { wrapper.deformity = value.value; }
                    }

                    // frequency
                    [ParserTarget("frequency", optional = true)]
                    private NumericParser<double> frequency
                    {
                        set { wrapper.frequency = value.value; }
                    }

                    // octaves
                    [ParserTarget("octaves", optional = true)]
                    private NumericParser<int> octaves
                    {
                        set { wrapper.octaves = value.value; }
                    }

                    // persistance
                    [ParserTarget("persistance", optional = true)]
                    private NumericParser<double> persistance
                    {
                        set { wrapper.persistance = value.value; }
                    }

                    // seed
                    [ParserTarget("seed", optional = true)]
                    private NumericParser<int> seedLoader
                    {
                        set { wrapper.seed = value.value; }
                    }

                    // noise
                    [ParserTarget("Noise", optional = true)]
                    private RiggedParser riggedNoise;

                    void IParserEventSubscriber.Apply(ConfigNode node)
                    {

                    }

                    void IParserEventSubscriber.PostApply(ConfigNode node)
                    {
                        wrapper.Setup(riggedNoise.noise);
                    }

                    public NoiseModWrapper()
                    {
                        wrapper = new PQSMod_VertexPlanet.NoiseModWrapper(0, 0, 0, 0);
                    }
                }

                // Land class loader 
                private class LandClassWrapper : IParserEventSubscriber
                {
                    // Land class object
                    public PQSMod_VertexPlanet.LandClass landClass;

                    // Name of the class
                    [ParserTarget("name")]
                    private string name
                    {
                        set { landClass.name = value; }
                    }

                    // baseColor
                    [ParserTarget("baseColor")]
                    private ColorParser baseColor
                    {
                        set { landClass.baseColor = value.value; }
                    }

                    // colorNoise
                    [ParserTarget("colorNoise")]
                    private ColorParser colorNoise
                    {
                        set { landClass.colorNoise = value.value; }
                    }

                    // colorNoiseAmount
                    [ParserTarget("colorNoiseAmount")]
                    private NumericParser<double> colorNoiseAmount
                    {
                        set { landClass.colorNoiseAmount = value.value; }
                    }

                    // colorNoiseMap
                    [ParserTarget("SimplexNoiseMap")]
                    private SimplexWrapper colorNoiseMap
                    {
                        set { landClass.colorNoiseMap = value.wrapper; }
                    }

                    // fractalEnd
                    [ParserTarget("fractalEnd")]
                    private NumericParser<double> fractalEnd
                    {
                        set { landClass.fractalEnd = value.value; }
                    }

                    // fractalStart
                    [ParserTarget("fractalStart")]
                    private NumericParser<double> fractalStart
                    {
                        set { landClass.fractalStart = value.value; }
                    }

                    // lerpToNext
                    [ParserTarget("lerpToNext")]
                    private NumericParser<bool> lerpToNext
                    {
                        set { landClass.lerpToNext = value.value; }
                    }

                    // fractalDelta
                    [ParserTarget("fractalDelta")]
                    private NumericParser<double> fractalDelta
                    {
                        set { landClass.fractalDelta = value.value; }
                    }

                    // endHeight
                    [ParserTarget("endHeight")]
                    private NumericParser<double> endHeight
                    {
                        set { landClass.endHeight = value.value; }
                    }

                    // startHeight
                    [ParserTarget("startHeight")]
                    private NumericParser<double> startHeight
                    {
                        set { landClass.startHeight = value.value; }
                    }

                    void IParserEventSubscriber.Apply(ConfigNode node) { }

                    void IParserEventSubscriber.PostApply(ConfigNode node) { }

                    public LandClassWrapper()
                    {
                        // Initialize the land class
                        landClass = new PQSMod_VertexPlanet.LandClass("class", 0.0, 0.0, Color.white, Color.white, 0.0);
                    }
                }

                // buildHeightColors
                [ParserTarget("buildHeightColors", optional = true)]
                private NumericParser<bool> buildHeightColors 
				{
					set { _mod.buildHeightColors = value.value; }
				}

                // colorDeformity
                [ParserTarget("colorDeformity", optional = true)]
                private NumericParser<double> colorDeformity
                {
                    set { _mod.colorDeformity = value.value; }
                }

                // continental
                [ParserTarget("ContinentalSimplex", optional = true)]
                private SimplexWrapper continental
                {
                    set { _mod.continental = value.wrapper; }
                }

                // continentalRuggedness
                [ParserTarget("RuggednessSimplex", optional = true)]
                private SimplexWrapper continentalRuggedness
                {
                    set { _mod.continentalRuggedness = value.wrapper; }
                }

                // continentalSharpness
                [ParserTarget("SharpnessNoise", optional = true)]
                private NoiseModWrapper continentalSharpness
                {
                    set { _mod.continentalSharpness = value.wrapper; }
                }

                // continentalSharpnessMap
                [ParserTarget("SharpnessSimplexMap", optional = true)]
                private SimplexWrapper continentalSharpnessMap
                {
                    set { _mod.continentalSharpnessMap = value.wrapper; }
                }

                // deformity
                [ParserTarget("deformity", optional = true)]
                private NumericParser<double> deformity
                {
                    set { _mod.deformity = value.value; }
                }

                // landClasses
                [ParserTargetCollection("LandClasses", optional = true, nameSignificance = NameSignificance.None)]
                private List<LandClassWrapper> landClasses = new List<LandClassWrapper>();

                // oceanDepth
                [ParserTarget("oceanDepth", optional = true)]
                private NumericParser<double> oceanDepth
                {
                    set { _mod.oceanDepth = value.value; }
                }

                // oceanLevel
                [ParserTarget("oceanLevel", optional = true)]
                private NumericParser<double> oceanLevel
                {
                    set { _mod.oceanLevel = value.value; }
                }

                // oceanSnap
                [ParserTarget("oceanSnap", optional = true)]
                private NumericParser<bool> oceanSnap
                {
                    set { _mod.oceanSnap = value.value; }
                }

                // oceanStep
                [ParserTarget("oceanStep", optional = true)]
                private NumericParser<double> oceanStep
                {
                    set { _mod.oceanStep = value.value; }
                }

                // seed
                [ParserTarget("seed", optional = true)]
                private NumericParser<int> seed
                {
                    set { _mod.seed = value.value; }
                }

                // terrainRidgeBalance
                [ParserTarget("terrainRidgeBalance", optional = true)]
                private NumericParser<double> terrainRidgeBalance
                {
                    set { _mod.terrainRidgeBalance = value.value; }
                }

                // terrainRidgesMax
                [ParserTarget("terrainRidgesMax", optional = true)]
                private NumericParser<double> terrainRidgesMax
                {
                    set { _mod.terrainRidgesMax = value.value; }
                }

                // terrainRidgesMin
                [ParserTarget("terrainRidgesMin", optional = true)]
                private NumericParser<double> terrainRidgesMin
                {
                    set { _mod.terrainRidgesMin = value.value; }
                }

                // terrainShapeEnd
                [ParserTarget("terrainShapeEnd", optional = true)]
                private NumericParser<double> terrainShapeEnd
                {
                    set { _mod.terrainShapeEnd = value.value; }
                }

                // terrainShapeStart
                [ParserTarget("terrainShapeStart", optional = true)]
                private NumericParser<double> terrainShapeStart
                {
                    set { _mod.terrainShapeStart = value.value; }
                }

                // terrainSmoothing
                [ParserTarget("terrainSmoothing", optional = true)]
                private NumericParser<double> terrainSmoothing
                {
                    set { _mod.terrainSmoothing = value.value; }
                }

                // terrainType
                [ParserTarget("TerrainTypeSimplex", optional = true)]
                private SimplexWrapper terrainType
                {
                    set { _mod.terrainType = value.wrapper; }
                }

				void IParserEventSubscriber.Apply(ConfigNode node)
				{

				}

				void IParserEventSubscriber.PostApply(ConfigNode node)
				{
                    PQSMod_VertexPlanet.LandClass[] landClassesArray = landClasses.Select(loader => loader.landClass).ToArray();
                    if (landClassesArray.Count() != 0)
                    {
                        _mod.landClasses = landClassesArray;
                    }
				}

                public VertexPlanet()
				{
					// Create the base mod
                    GameObject modObject = new GameObject("VertexPlanet");
					modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSMod_VertexPlanet>();
					base.mod = _mod;
				}

                public VertexPlanet(PQSMod template)
                {
                    _mod = template as PQSMod_VertexPlanet;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
			}
		}
	}
}

