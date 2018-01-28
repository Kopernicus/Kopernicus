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