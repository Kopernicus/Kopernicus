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
using System.Linq;
using Kopernicus.Components;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;
using Kopernicus.UI;
using UnityEngine;
using Gradient = Kopernicus.Configuration.Parsing.Gradient;
using Object = UnityEngine.Object;

namespace Kopernicus.Configuration
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class ParticleLoader : BaseLoader, IParserEventSubscriber, ITypeParser<PlanetParticleEmitter>
    {
        // Set-up our parental objects
        public PlanetParticleEmitter Value { get; set; }

        // Target of our particles
        [ParserTarget("target")]
        public String Target
        {
            get { return Value.target; }
            set { Value.target = value; }
        }

        // Shader of the particles
        [ParserTarget("shader", Optional = true)]
        public String ShaderName
        {
            get { return Value.shaderName; }
            set { Value.shaderName = value; }
        }

        // Shape of the particles
        [ParserTarget("shape", Optional = true)]
        public EnumParser<KSPParticleEmitter.EmissionShape> EmissionShape
        {
            get { return Value.emitter.shape; }
            set { Value.emitter.shape = value; }
        }

        // 1-dimensional shape of the particles
        [ParserTarget("shape1D", Optional = true)]
        public NumericParser<Single> Shape1D
        {
            get { return Value.emitter.shape1D; }
            set { Value.emitter.shape1D = value; }
        }

        // 2-dimensional shape of the particles
        [ParserTarget("shape2D", Optional = true)]
        public Vector2Parser Shape2D
        {
            get { return Value.emitter.shape2D; }
            set { Value.emitter.shape2D = value; }
        }

        // 3-dimensional shape of the particles
        [ParserTarget("shape3D", Optional = true)]
        public Vector3Parser Shape3D
        {
            get { return Value.emitter.shape3D; }
            set { Value.emitter.shape3D = value; }
        }

        // minEmission of particles
        [ParserTarget("minEmission")]
        public NumericParser<Int32> MinEmission
        {
            get { return Value.minEmission; }
            set { Value.minEmission = value; }
        }

        // maxEmission of particles
        [ParserTarget("maxEmission")]
        public NumericParser<Int32> MaxEmission
        {
            get { return Value.maxEmission; }
            set { Value.maxEmission = value; }
        }

        // minimum lifespan of particles
        [ParserTarget("lifespanMin")]
        public NumericParser<Single> LifespanMin
        {
            get { return Value.minEnergy; }
            set { Value.minEnergy = value; }
        }

        // maximum lifespan of particles
        [ParserTarget("lifespanMax")]
        public NumericParser<Single> LifespanMax
        {
            get { return Value.maxEnergy; }
            set { Value.maxEnergy = value; }
        }

        // minimum size of particles
        [ParserTarget("sizeMin")]
        public NumericParser<Single> SizeMin
        {
            get { return Value.emitter.minSize; }
            set { Value.emitter.minSize = value; }
        }

        // maximum size of particles
        [ParserTarget("sizeMax")]
        public NumericParser<Single> SizeMax
        {
            get { return Value.emitter.maxSize; }
            set { Value.emitter.maxSize = value; }
        }

        // speedScale of particles
        [ParserTarget("speedScale")]
        public NumericParser<Single> SpeedScaleLoader
        {
            get { return Value.speedScale; }
            set { Value.speedScale = value; }
        }

        // grow rate of particles
        [ParserTarget("rate")]
        public NumericParser<Single> Rate
        {
            get { return Value.emitter.sizeGrow; }
            set { Value.emitter.sizeGrow = value; }
        }

        // rand Velocity of particles
        [ParserTarget("randVelocity")]
        public Vector3Parser RandVelocity
        {
            get { return Value.randomVelocity; }
            set { Value.randomVelocity = value; }
        }

        // Texture of particles
        [ParserTarget("texture")]
        public Texture2DParser Texture
        {
            get { return Value.mainTexture; }
            set { Value.mainTexture = value; }
        }

        // scale
        [ParserTarget("scale")]
        public Vector3Parser Scale
        {
            get { return Value.scale; }
            set { Value.scale = value; }
        }

        // Mesh to emit particles from
        [ParserTarget("emitMesh")]
        public MeshParser EmitMesh
        {
            get { return Value.mesh; }
            set { Value.mesh = value; }
        }

        // Whether the particles should collide with stuff
        [ParserTarget("collide")]
        public NumericParser<Boolean> Collide
        {
            get { return Value.collideable; }
            set { Value.collideable = value; }
        }

        // Collision mesh
        [ParserTarget("bounce", Optional = true)]
        public NumericParser<Single> CollisionMesh
        {
            get { return Value.bounce; }
            set { Value.bounce = value; }
        }

        // Damping of the particles after they collide
        [ParserTarget("damping", Optional = true)]
        public NumericParser<Single> Damping
        {
            get { return Value.emitter.damping; }
            set { Value.emitter.damping = value; }
        }

        // Whether the particles should cast shadows
        [ParserTarget("shadowCast")]
        public NumericParser<Boolean> CastShadows
        {
            get { return Value.emitter.castShadows; }
            set { Value.emitter.castShadows = value; }
        }

        // Whether the particles should be affected by shadows
        [ParserTarget("shadowEffect")]
        public NumericParser<Boolean> ReceiveShadows
        {
            get { return Value.emitter.recieveShadows; }
            set { Value.emitter.recieveShadows = value; }
        }

        // Whether to use an auto random seed
        [ParserTarget("autoSeed", Optional = true)]
        public NumericParser<Boolean> AutoRandomSeed
        {
            get { return Value.emitter.ps.useAutoRandomSeed; }
            set { Value.emitter.ps.useAutoRandomSeed = value; }
        }

        // Seed for the particle generation (cannot be negative)
        [ParserTarget("seed", Optional = true)]
        public NumericParser<UInt32> Seed
        {
            get { return Value.emitter.ps.randomSeed; }
            set { Value.emitter.ps.randomSeed = value; }
        }

        // force
        [ParserTarget("force")]
        public Vector3Parser Force
        {
            get
            {
                if (Value.emitter.force == null)
                {
                    Value.emitter.force = Vector3.zero;
                }
                return Value.emitter.force;
            }
            set { Value.emitter.force = value; }
        }

        // Colors
        [ParserTargetCollection("Colors")]
        public List<ColorParser> Colors
        {
            get { return Value.emitter.colorAnimation.Select(c => new ColorParser(c)).ToList(); }
            set { Value.emitter.colorAnimation = value.Select(c => c.Value).ToArray(); }
        }

        // Lifetime colors (length two)
        [ParserTarget("lifetimeColors", Optional = true)]
        public List<ColorParser> LifetimeColors
        {
            get { return Value.lifetimeColors.Select(c => new ColorParser(c)).ToList(); }
            set { Value.lifetimeColors = value.Select(c => c.Value).ToArray(); }
        }

        [KittopiaDestructor]
        public void Destroy()
        {
            Object.Destroy(Value.gameObject);
        }

        /// <summary>
        /// Creates a new Particle Loader from the Injector context.
        /// </summary>
        public ParticleLoader()
        {
            // Is this the parser context?
            if (!Injector.IsInPrefab)
            {
                throw new InvalidOperationException("Must be executed in Injector context.");
            }

            // Store values
            Value = PlanetParticleEmitter.Create(generatedBody.scaledVersion);
            Value.emitter.colorAnimation = new Color[0];
        }

        /// <summary>
        /// Creates a new Particle Loader on a spawned CelestialBody.
        /// </summary>
        [KittopiaConstructor(KittopiaConstructor.ParameterType.CelestialBody)]
        public ParticleLoader(CelestialBody body)
        {
            // Is this a spawned body?
            if (body.scaledBody == null || Injector.IsInPrefab)
            {
                throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
            }

            // Store values
            Value = PlanetParticleEmitter.Create(body.scaledBody);
            Value.emitter.colorAnimation = new Color[0];
        }

        /// <summary>
        /// Creates a new Particle Loader from an already existing emitter
        /// </summary>
        public ParticleLoader(PlanetParticleEmitter particle)
        {
            // Store values
            Value = particle;

            // Null safe
            if (Value.emitter.colorAnimation == null)
            {
                Value.emitter.colorAnimation = new Color[0];
            }
        }

        // Apply event
        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            Events.OnParticleLoaderApply.Fire(this, node);
        }

        // Post-Apply event
        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
            Events.OnParticleLoaderPostApply.Fire(this, node);
        }

    }
}
