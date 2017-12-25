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
using System;
using System.Collections.Generic;
using System.Linq;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class ParticleLoader : BaseLoader, IParserEventSubscriber, ITypeParser<PlanetParticleEmitter>
        {
            // Set-up our parental objects
            public PlanetParticleEmitter Value { get; set; }

            // Target of our particles
            [ParserTarget("target")]
            public String target
            {
                get { return Value.target; }
                set { Value.target = value; }
            }

            // minEmission of particles
            [ParserTarget("minEmission")]
            public NumericParser<Single> minEmission
            {
                get { return Value.minEmission; }
                set { Value.minEmission = value; }
            }

            // maxEmission of particles
            [ParserTarget("maxEmission")]
            public NumericParser<Single> maxEmission
            {
                get { return Value.maxEmission; }
                set { Value.maxEmission = value; }
            }

            // minimum lifespan of particles
            [ParserTarget("lifespanMin")]
            public NumericParser<Single> lifespanMin
            {
                get { return Value.minEnergy; }
                set { Value.minEnergy = value; }
            }

            // maximum lifespan of particles
            [ParserTarget("lifespanMax")]
            public NumericParser<Single> lifespanMax
            {
                get { return Value.maxEnergy; }
                set { Value.maxEnergy = value; }
            }

            // minimum size of particles
            [ParserTarget("sizeMin")]
            public NumericParser<Single> sizeMin
            {
                get { return Value.minSize; }
                set { Value.minSize = value; }
            }

            // maximum size of particles
            [ParserTarget("sizeMax")]
            public NumericParser<Single> sizeMax
            {
                get { return Value.maxSize; }
                set { Value.maxSize = value; }
            }

            // speedScale of particles
            [ParserTarget("speedScale")]
            public NumericParser<Single> speedScaleLoader
            {
                get { return Value.speedScale; }
                set { Value.speedScale = value; }
            }

            // grow rate of particles
            [ParserTarget("rate")]
            public NumericParser<Single> rate
            {
                get { return Value.sizeGrow; }
                set { Value.sizeGrow = value; }
            }

            // rand Velocity of particles
            [ParserTarget("randVelocity")]
            public Vector3Parser randVelocity
            {
                get { return Value.randomVelocity; }
                set { Value.randomVelocity = value; }
            }

            // Texture of particles
            [ParserTarget("texture")]
            public Texture2DParser texture
            {
                get { return Value.mainTexture as Texture2D; }
                set { Value.mainTexture = value; }
            }

            // scale
            [ParserTarget("scale")]
            public Vector3Parser scale
            {
                get { return Value.scale; }
                set { Value.scale = value; }
            }

            // mesh
            [ParserTarget("mesh")]
            public MeshParser mesh
            {
                get { return Value.mesh; }
                set { Value.mesh = value; }
            }

            // Whether the particles should collide with stuff
            [ParserTarget("collide")]
            public NumericParser<Boolean> collide
            {
                get { return Value.collideable; }
                set { Value.collideable = value; }
            }

            // force
            [ParserTarget("force")]
            public Vector3Parser force
            {
                get { return Value.force; }
                set { Value.force = value; }
            }
            
            // Colors
            [ParserTargetCollection("Colors")]
            public List<ColorParser> colors
            {
                get { return Value.colorAnimation.Select(c => new ColorParser(c)).ToList(); }
                set { Value.colorAnimation = value.Select(c => c.Value).ToArray(); }
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
                Value.colorAnimation = new Color[0];
            }

            /// <summary>
            /// Creates a new Particle Loader on a spawned CelestialBody.
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.CelestialBody, purpose = KittopiaConstructor.Purpose.Create)]
            public ParticleLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }

                // Store values
                Value = PlanetParticleEmitter.Create(body.scaledBody);
                Value.colorAnimation = new Color[0];
            }

            /// <summary>
            /// Creates a new Particle Loader from an already existing emitter
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public ParticleLoader(PlanetParticleEmitter particle)
            {
                // Store values
                Value = particle;

                // Null safe
                if (Value.colorAnimation == null)
                {
                    Value.colorAnimation = new Color[0];
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
}
