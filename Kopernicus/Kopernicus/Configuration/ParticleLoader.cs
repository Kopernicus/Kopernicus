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
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class ParticleLoader : BaseLoader, IParserEventSubscriber
        {
            // Set-up our parental objects
            PlanetParticleEmitter particle;
            GameObject scaledVersion;

            // Target of our particles
            [ParserTarget("target")]
            public String target
            {
                get { return particle.target; }
                set { particle.target = value; }
            }

            // minEmission of particles
            [ParserTarget("minEmission")]
            public NumericParser<Single> minEmission
            {
                get { return particle.minEmission; }
                set { particle.minEmission = value; }
            }

            // maxEmission of particles
            [ParserTarget("maxEmission")]
            public NumericParser<Single> maxEmission
            {
                get { return particle.maxEmission; }
                set { particle.maxEmission = value; }
            }

            // minimum lifespan of particles
            [ParserTarget("lifespanMin")]
            public NumericParser<Single> lifespanMin
            {
                get { return particle.minEnergy; }
                set { particle.minEnergy = value; }
            }

            // maximum lifespan of particles
            [ParserTarget("lifespanMax")]
            public NumericParser<Single> lifespanMax
            {
                get { return particle.maxEnergy; }
                set { particle.maxEnergy = value; }
            }

            // minimum size of particles
            [ParserTarget("sizeMin")]
            public NumericParser<Single> sizeMin
            {
                get { return particle.minSize; }
                set { particle.minSize = value; }
            }

            // maximum size of particles
            [ParserTarget("sizeMax")]
            public NumericParser<Single> sizeMax
            {
                get { return particle.maxSize; }
                set { particle.maxSize = value; }
            }

            // speedScale of particles
            [ParserTarget("speedScale")]
            public NumericParser<Single> speedScaleLoader
            {
                get { return particle.speedScale; }
                set { particle.speedScale = value; }
            }

            // grow rate of particles
            [ParserTarget("rate")]
            public NumericParser<Single> rate
            {
                get { return particle.sizeGrow; }
                set { particle.sizeGrow = value; }
            }

            // rand Velocity of particles
            [ParserTarget("randVelocity")]
            public Vector3Parser randVelocity
            {
                get { return particle.randomVelocity; }
                set { particle.randomVelocity = value; }
            }

            // Texture of particles
            [ParserTarget("texture")]
            public Texture2DParser texture
            {
                get { return particle.mainTexture as Texture2D; }
                set { particle.mainTexture = value; }
            }

            // scale
            [ParserTarget("scale")]
            public Vector3Parser scale
            {
                get { return particle.scale; }
                set { particle.scale = value; }
            }

            // mesh
            [ParserTarget("mesh")]
            public MeshParser mesh
            {
                get { return particle.mesh; }
                set { particle.mesh = value; }
            }

            // Whether the particles should collide with stuff
            [ParserTarget("collide")]
            public NumericParser<Boolean> collide
            {
                get { return particle.collideable; }
                set { particle.collideable = value; }
            }

            // force
            [ParserTarget("force")]
            public Vector3Parser force
            {
                get { return particle.force; }
                set { particle.force = value; }
            }

            /// <summary>
            /// Creates a new Particle Loader from the Injector context.
            /// </summary>
            public ParticleLoader()
            {
                // Is this the parser context?
                if (generatedBody == null)
                    throw new InvalidOperationException("Must be executed in Injector context.");

                // Store values
                scaledVersion = generatedBody.scaledVersion;
                particle = PlanetParticleEmitter.Create(scaledVersion);
            }

            /// <summary>
            /// Creates a new Particle Loader from a spawned CelestialBody.
            /// </summary>
            public ParticleLoader(CelestialBody body, GameObject particleHost)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null)
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");

                // Store values
                scaledVersion = body.scaledBody;
                particle = particleHost.GetComponent<PlanetParticleEmitter>();
            }

            /// <summary>
            /// Creates a new Particle Loader from a custom PSystemBody.
            /// </summary>
            public ParticleLoader(PSystemBody body)
            {
                // Set generatedBody
                if (body == null)
                    throw new InvalidOperationException("The body cannot be null.");
                generatedBody = body;

                // Store values
                scaledVersion = generatedBody.scaledVersion;
                particle = PlanetParticleEmitter.Create(scaledVersion);
            }

            // Apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                Events.OnParticleLoaderApply.Fire(this, node);
            }

            // Post-Apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                List<Color> colors = new List<Color>();
                foreach (String color in node.GetNode("Colors").GetValuesStartsWith("color"))
                {
                    Vector4 c = ConfigNode.ParseVector4(color);
                    colors.Add(new Color(c.x, c.y, c.z, c.w));
                }
                particle.colorAnimation = colors.ToArray();
                Events.OnParticleLoaderPostApply.Fire(this, node);
            }

        }
    }
}
