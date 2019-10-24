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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kopernicus.Components;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class VertexPlanet : ModLoader<PQSMod_VertexPlanet>
    {
        // Loader for the SimplexWrapper
        [RequireConfigType(ConfigType.Node)]
        public class SimplexLoader : ITypeParser<KopernicusSimplexWrapper>
        {
            // Loaded wrapper
            public KopernicusSimplexWrapper Value { get; set; }

            // deformity
            [ParserTarget("deformity")]
            public NumericParser<Double> Deformity
            {
                get { return Value.deformity; }
                set { Value.deformity = value; }
            }

            // frequency
            [ParserTarget("frequency")]
            public NumericParser<Double> Frequency
            {
                get { return Value.frequency; }
                set { Value.frequency = value; }
            }

            // octaves
            [ParserTarget("octaves")]
            public NumericParser<Double> Octaves
            {
                get { return Value.octaves; }
                set { Value.octaves = Utility.Clamp(value, 1, 30); }
            }

            // persistance
            [ParserTarget("persistance")]
            public NumericParser<Double> Persistance
            {
                get { return Value.persistance; }
                set { Value.persistance = value; }
            }

            // seed
            [ParserTarget("seed")]
            public NumericParser<Int32> Seed
            {
                get { return Value.Seed; }
                set { Value.Seed = value; }
            }

            // Default constructor
            [KittopiaConstructor(KittopiaConstructor.ParameterType.Empty)]
            public SimplexLoader()
            {
                Value = new KopernicusSimplexWrapper(0, 0, 0, 0);
            }

            // Runtime constructor
            public SimplexLoader(PQSMod_VertexPlanet.SimplexWrapper simplex)
            {
                Value = new KopernicusSimplexWrapper(simplex);
            }

            // Runtime constructor
            public SimplexLoader(KopernicusSimplexWrapper simplex)
            {
                Value = simplex;
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator KopernicusSimplexWrapper(SimplexLoader parser)
            {
                return parser.Value;
            }

            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator SimplexLoader(KopernicusSimplexWrapper value)
            {
                return value == null ? null : new SimplexLoader(value);
            }

            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator SimplexLoader(PQSMod_VertexPlanet.SimplexWrapper value)
            {
                return value == null ? null : new SimplexLoader(value);
            }
        }

        // Loader for Noise
        [RequireConfigType(ConfigType.Node)]
        public class NoiseModLoader : ITypeParser<PQSMod_VertexPlanet.NoiseModWrapper>
        {
            // The loaded noise
            public PQSMod_VertexPlanet.NoiseModWrapper Value { get; set; }

            // deformity
            [ParserTarget("deformity")]
            public NumericParser<Double> Deformity
            {
                get { return Value.deformity; }
                set { Value.deformity = value; }
            }

            // frequency
            [ParserTarget("frequency")]
            public NumericParser<Double> Frequency
            {
                get { return Value.frequency; }
                set { Value.frequency = value; }
            }

            // octaves
            [ParserTarget("octaves")]
            public NumericParser<Int32> Octaves
            {
                get { return Value.octaves; }
                set { Value.octaves = Mathf.Clamp(value, 1, 30); }
            }

            // persistance
            [ParserTarget("persistance")]
            public NumericParser<Double> Persistance
            {
                get { return Value.persistance; }
                set { Value.persistance = value; }
            }

            // seed
            [ParserTarget("seed")]
            public NumericParser<Int32> SeedLoader
            {
                get { return Value.seed; }
                set { Value.seed = value; }
            }

            // Default constructor
            [KittopiaConstructor(KittopiaConstructor.ParameterType.Empty)]
            public NoiseModLoader()
            {
                Value = new PQSMod_VertexPlanet.NoiseModWrapper(0, 0, 0, 0);
            }

            // Runtime Constructor
            public NoiseModLoader(PQSMod_VertexPlanet.NoiseModWrapper noise)
            {
                Value = noise;
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator PQSMod_VertexPlanet.NoiseModWrapper(NoiseModLoader parser)
            {
                return parser.Value;
            }

            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator NoiseModLoader(PQSMod_VertexPlanet.NoiseModWrapper value)
            {
                return new NoiseModLoader(value);
            }
        }

        // Land class loader 
        [RequireConfigType(ConfigType.Node)]
        public class LandClassLoader : IPatchable, ITypeParser<PQSMod_VertexPlanet.LandClass>
        {
            // Land class object
            public PQSMod_VertexPlanet.LandClass Value { get; set; }

            // Name of the class
            [ParserTarget("name")]
            public String name
            {
                get { return Value.name; }
                set { Value.name = value; }
            }

            // Should we delete this
            [ParserTarget("delete")]
            [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
            public NumericParser<Boolean> Delete = false;

            // baseColor
            [ParserTarget("baseColor")]
            public ColorParser BaseColor
            {
                get { return Value.baseColor; }
                set { Value.baseColor = value; }
            }

            // colorNoise
            [ParserTarget("colorNoise")]
            public ColorParser ColorNoise
            {
                get { return Value.colorNoise; }
                set { Value.colorNoise = value; }
            }

            // colorNoiseAmount
            [ParserTarget("colorNoiseAmount")]
            public NumericParser<Double> ColorNoiseAmount
            {
                get { return Value.colorNoiseAmount; }
                set { Value.colorNoiseAmount = value; }
            }

            // colorNoiseMap
            [ParserTarget("SimplexNoiseMap", AllowMerge = true)]
            public SimplexLoader ColorNoiseMap
            {
                get { return Value.colorNoiseMap; }
                set { Value.colorNoiseMap = value; }
            }

            // fractalEnd
            [ParserTarget("fractalEnd")]
            public NumericParser<Double> FractalEnd
            {
                get { return Value.fractalEnd; }
                set { Value.fractalEnd = value; }
            }

            // fractalStart
            [ParserTarget("fractalStart")]
            public NumericParser<Double> FractalStart
            {
                get { return Value.fractalStart; }
                set { Value.fractalStart = value; }
            }

            // lerpToNext
            [ParserTarget("lerpToNext")]
            public NumericParser<Boolean> LerpToNext
            {
                get { return Value.lerpToNext; }
                set { Value.lerpToNext = value; }
            }

            // fractalDelta
            [ParserTarget("fractalDelta")]
            public NumericParser<Double> FractalDelta
            {
                get { return Value.fractalDelta; }
                set { Value.fractalDelta = value; }
            }

            // endHeight
            [ParserTarget("endHeight")]
            public NumericParser<Double> EndHeight
            {
                get { return Value.endHeight; }
                set { Value.endHeight = value; }
            }

            // startHeight
            [ParserTarget("startHeight")]
            public NumericParser<Double> StartHeight
            {
                get { return Value.startHeight; }
                set { Value.startHeight = value; }
            }

            // Default constructor
            [KittopiaConstructor(KittopiaConstructor.ParameterType.Empty)]
            public LandClassLoader()
            {
                Value = new PQSMod_VertexPlanet.LandClass("class", 0.0, 0.0, Color.white, Color.white, 0.0);
            }

            // Runtime constructor
            public LandClassLoader(PQSMod_VertexPlanet.LandClass land)
            {
                Value = land;
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator PQSMod_VertexPlanet.LandClass(LandClassLoader parser)
            {
                return parser?.Value;
            }

            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator LandClassLoader(PQSMod_VertexPlanet.LandClass value)
            {
                return value != null ? new LandClassLoader(value) : null;
            }
        }

        // buildHeightColors
        [ParserTarget("buildHeightColors")]
        public NumericParser<Boolean> BuildHeightColors
        {
            get { return Mod.buildHeightColors; }
            set { Mod.buildHeightColors = value; }
        }

        // colorDeformity
        [ParserTarget("colorDeformity")]
        public NumericParser<Double> ColorDeformity
        {
            get { return Mod.colorDeformity; }
            set { Mod.colorDeformity = value; }
        }

        // continental
        [ParserTarget("ContinentalSimplex", AllowMerge = true)]
        public SimplexLoader Continental
        {
            get { return Mod.continental; }
            set { Mod.continental = value; }
        }

        // continentalRuggedness
        [ParserTarget("RuggednessSimplex", AllowMerge = true)]
        public SimplexLoader ContinentalRuggedness
        {
            get { return Mod.continentalRuggedness; }
            set { Mod.continentalRuggedness = value; }
        }

        // continentalSharpness
        [ParserTarget("SharpnessNoise", AllowMerge = true)]
        public NoiseModLoader ContinentalSharpness
        {
            get { return Mod.continentalSharpness; }
            set { Mod.continentalSharpness = value; }
        }

        // continentalSharpnessMap
        [ParserTarget("SharpnessSimplexMap", AllowMerge = true)]
        public SimplexLoader ContinentalSharpnessMap
        {
            get { return Mod.continentalSharpnessMap; }
            set { Mod.continentalSharpnessMap = value; }
        }

        // deformity
        [ParserTarget("deformity")]
        public NumericParser<Double> Deformity
        {
            get { return Mod.deformity; }
            set { Mod.deformity = value; }
        }

        // The land classes
        [ParserTargetCollection("LandClasses", AllowMerge = true)]
        public CallbackList<LandClassLoader> LandClasses { get; set; }

        // oceanDepth
        [ParserTarget("oceanDepth")]
        public NumericParser<Double> OceanDepth
        {
            get { return Mod.oceanDepth; }
            set { Mod.oceanDepth = value; }
        }

        // oceanLevel
        [ParserTarget("oceanLevel")]
        public NumericParser<Double> OceanLevel
        {
            get { return Mod.oceanLevel; }
            set { Mod.oceanLevel = value; }
        }

        // oceanSnap
        [ParserTarget("oceanSnap")]
        public NumericParser<Boolean> OceanSnap
        {
            get { return Mod.oceanSnap; }
            set { Mod.oceanSnap = value; }
        }

        // oceanStep
        [ParserTarget("oceanStep")]
        public NumericParser<Double> OceanStep
        {
            get { return Mod.oceanStep; }
            set { Mod.oceanStep = value; }
        }

        // seed
        [ParserTarget("seed")]
        public NumericParser<Int32> Seed
        {
            get { return Mod.seed; }
            set { Mod.seed = value; }
        }

        // terrainRidgeBalance
        [ParserTarget("terrainRidgeBalance")]
        public NumericParser<Double> TerrainRidgeBalance
        {
            get { return Mod.terrainRidgeBalance; }
            set { Mod.terrainRidgeBalance = value; }
        }

        // terrainRidgesMax
        [ParserTarget("terrainRidgesMax")]
        public NumericParser<Double> TerrainRidgesMax
        {
            get { return Mod.terrainRidgesMax; }
            set { Mod.terrainRidgesMax = value; }
        }

        // terrainRidgesMin
        [ParserTarget("terrainRidgesMin")]
        public NumericParser<Double> TerrainRidgesMin
        {
            get { return Mod.terrainRidgesMin; }
            set { Mod.terrainRidgesMin = value; }
        }

        // terrainShapeEnd
        [ParserTarget("terrainShapeEnd")]
        public NumericParser<Double> TerrainShapeEnd
        {
            get { return Mod.terrainShapeEnd; }
            set { Mod.terrainShapeEnd = value; }
        }

        // terrainShapeStart
        [ParserTarget("terrainShapeStart")]
        public NumericParser<Double> TerrainShapeStart
        {
            get { return Mod.terrainShapeStart; }
            set { Mod.terrainShapeStart = value; }
        }

        // terrainSmoothing
        [ParserTarget("terrainSmoothing")]
        public NumericParser<Double> TerrainSmoothing
        {
            get { return Mod.terrainSmoothing; }
            set { Mod.terrainSmoothing = value; }
        }

        // terrainType
        [ParserTarget("TerrainTypeSimplex", AllowMerge = true)]
        public SimplexLoader TerrainType
        {
            get { return Mod.terrainType; }
            set { Mod.terrainType = value; }
        }

        // Creates the a PQSMod of type T with given PQS
        public override void Create(PQS pqsVersion)
        {
            base.Create(pqsVersion);

            // Create mod components
            Continental = new SimplexLoader();
            ContinentalRuggedness = new SimplexLoader();
            ContinentalSharpness = new NoiseModLoader();
            ContinentalSharpnessMap = new SimplexLoader();
            TerrainType = new SimplexLoader();

            // Create the callback list
            LandClasses = new CallbackList<LandClassLoader>(e =>
            {
                Mod.landClasses = LandClasses.Where(landClass => !landClass.Delete)
                    .Select(landClass => landClass.Value).ToArray();
            });
            Mod.landClasses = new PQSMod_VertexPlanet.LandClass[0];
        }

        // Grabs a PQSMod of type T from a parameter with a given PQS
        public override void Create(PQSMod_VertexPlanet mod, PQS pqsVersion)
        {
            base.Create(mod, pqsVersion);

            // Create mod components
            if (Continental == null)
            {
                Continental = new SimplexLoader();
            }
            else if (Mod.continental.GetType() == typeof(PQSMod_VertexPlanet.SimplexWrapper))
            {
                Continental = new SimplexLoader(Mod.continental);
            }

            if (ContinentalRuggedness == null)
            {
                ContinentalRuggedness = new SimplexLoader();
            }
            else if (Mod.continentalRuggedness.GetType() == typeof(PQSMod_VertexPlanet.SimplexWrapper))
            {
                ContinentalRuggedness = new SimplexLoader(Mod.continentalRuggedness);
            }

            if (ContinentalSharpness == null)
            {
                ContinentalSharpness = new NoiseModLoader();
            }

            if (ContinentalSharpnessMap == null)
            {
                ContinentalSharpnessMap = new SimplexLoader();
            }
            else if (Mod.continentalSharpnessMap.GetType() == typeof(PQSMod_VertexPlanet.SimplexWrapper))
            {
                ContinentalSharpnessMap = new SimplexLoader(Mod.continentalSharpnessMap);
            }

            if (TerrainType == null)
            {
                TerrainType = new SimplexLoader();
            }
            else if (Mod.terrainType.GetType() == typeof(PQSMod_VertexPlanet.SimplexWrapper))
            {
                TerrainType = new SimplexLoader(Mod.terrainType);
            }

            // Create the callback list
            LandClasses = new CallbackList<LandClassLoader>(e =>
            {
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
                Mod.landClasses = new PQSMod_VertexPlanet.LandClass[0];
            }
        }
    }
}

