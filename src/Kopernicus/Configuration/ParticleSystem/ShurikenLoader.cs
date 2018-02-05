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

using System;
using Kopernicus.Components;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class ShurikenLoader : BaseLoader, ITypeParser<ParticleSystem>
        {
            // The particles that we are adding to the celestial body
            public ParticleSystem Value { get; set; }

            // Access the particle system collision module.
            [ParserTarget("Collision")]
            public CollisionModuleLoader collision
            {
                get { return Value.collision; }
                set { Utility.CopyObjectProperties(value.Value, Value.collision, false); }
            }

            // Access the particle system color by speed module.
            [ParserTarget("ColorBySpeed")]
            public ColorBySpeedModuleLoader colorBySpeed
            {
                get { return Value.colorBySpeed; }
                set { Utility.CopyObjectProperties(value.Value, Value.colorBySpeed, false); }
            }

            // Access the particle system color over lifetime module.
            [ParserTarget("ColorOverLifetime")]
            public ColorOverLifetimeModuleLoader colorOverLifetime
            {
                get { return Value.colorOverLifetime; }
                set { Utility.CopyObjectProperties(value.Value, Value.colorOverLifetime, false); }
            }

            // Access the particle system emission module.
            [ParserTarget("Emission")]
            public EmissionModuleLoader emission
            {
                get { return Value.emission; }
                set { Utility.CopyObjectProperties(value.Value, Value.emission, false); }
            }

            // Access the particle system external forces module.
            [ParserTarget("ExternalForces")]
            public ExternalForcesModuleLoader externalForces
            {
                get { return Value.externalForces; }
                set { Utility.CopyObjectProperties(value.Value, Value.externalForces, false); }
            }

            // Access the particle system force over lifetime module.
            [ParserTarget("ForceOverLifetime")]
            public ForceOverLifetimeModuleLoader forceOverLifetime
            {
                get { return Value.forceOverLifetime; }
                set { Utility.CopyObjectProperties(value.Value, Value.forceOverLifetime, false); }
            }

            [ParserTarget("gravityMultiplier")]
            [KittopiaDescription("Scale being applied to the gravity defined by Physics.gravity.")]
            public NumericParser<Single> gravityMultiplier
            {
                get { return Value.gravityModifier; }
                set { Value.gravityModifier = value; }
            }

            // Access the particle system inherit velocity module.
            [ParserTarget("InheritVelocity")]
            public InheritVelocityModuleLoader inheritVelocity
            {
                get { return Value.inheritVelocity; }
                set { Utility.CopyObjectProperties(value.Value, Value.inheritVelocity, false); }
            }
            
            // Access the particle system limit velocity over lifetime module.
            [ParserTarget("LimitVelocityOverLifetime")]
            public LimitVelocityOverLifetimeModuleLoader limitVelocityOverLifetime
            {
                get { return Value.limitVelocityOverLifetime; }
                set { Utility.CopyObjectProperties(value.Value, Value.limitVelocityOverLifetime, false); }
            }
            
            [ParserTarget("loop")]
            [KittopiaDescription("Is the particle system looping?")]
            public NumericParser<Boolean> loop
            {
                get { return Value.loop; }
                set { Value.loop = value; }
            }
            
            [ParserTarget("maxParticles")]
            [KittopiaDescription("The maximum number of particles to emit.")]
            public NumericParser<Int32> maxParticles
            {
                get { return Value.maxParticles; }
                set { Value.maxParticles = value; }
            }
            
            [ParserTarget("playbackSpeed")]
            [KittopiaDescription("The playback speed of the particle system. 1 is normal playback speed.")]
            public NumericParser<Single> playbackSpeed
            {
                get { return Value.playbackSpeed; }
                set { Value.playbackSpeed = value; }
            }
            
            [ParserTarget("randomSeed")]
            [KittopiaDescription("Override the random seed used for the particle system emission.")]
            public NumericParser<UInt32> randomSeed
            {
                get { return Value.randomSeed; }
                set { Value.randomSeed = value; }
            }
            
            // Access the particle system rotation by speed module.
            [ParserTarget("RotationBySpeed")]
            public RotationBySpeedModuleLoader rotationBySpeed
            {
                get { return Value.rotationBySpeed; }
                set { Utility.CopyObjectProperties(value.Value, Value.rotationBySpeed, false); }
            }
            
            // Access the particle system rotation over lifetime module.
            [ParserTarget("RotationOverLifetime")]
            public RotationOverLifetimeModuleLoader rotationOverLifetime
            {
                get { return Value.rotationOverLifetime; }
                set { Utility.CopyObjectProperties(value.Value, Value.rotationOverLifetime, false); }
            }
            
            [ParserTarget("scalingMode")]
            [KittopiaDescription("The scaling mode applied to particle sizes and positions.")]
            public EnumParser<ParticleSystemScalingMode> scalingMode
            {
                get { return Value.scalingMode; }
                set { Value.scalingMode = value; }
            }
            
            /// <summary>
            /// Creates a new Shuriken Loader from the Injector context.
            /// </summary>
            public ShurikenLoader()
            {
                // Is this the parser context?
                if (!Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("Must be executed in Injector context.");
                }
                
                GameObject gameObject = new GameObject("ParticleSystem");
                gameObject.transform.parent = generatedBody.scaledVersion.transform;
                Value = gameObject.AddComponent<ParticleSystem>();
            }

            /// <summary>
            /// Creates a new Shuriken Loader from a spawned CelestialBody.
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.CelestialBody, purpose = KittopiaConstructor.Purpose.Create)]
            public ShurikenLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }
                
                GameObject gameObject = new GameObject("ParticleSystem");
                gameObject.transform.parent = body.scaledBody.transform;
                Value = gameObject.AddComponent<ParticleSystem>();
            }

            /// <summary>
            /// Edits an existing Shuriken Loader
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public ShurikenLoader(ParticleSystem value)
            {
                Value = value;
            }
        }
    }
}