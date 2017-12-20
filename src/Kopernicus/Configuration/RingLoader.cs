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
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class RingLoader : BaseLoader, IParserEventSubscriber
        {
            // Set-up our custom ring
            public Ring ring;

            // Inner Radius of our ring
            [ParserTarget("innerRadius")]
            [KittopiaDescription("The lower bound of the ring (measured in meters from the center of the body).")]
            public NumericParser<Single> innerRadius
            {
                get { return ring.innerRadius; }
                set { ring.innerRadius = value; }
            }

            // Outer Radius of our ring
            [ParserTarget("outerRadius")]
            [KittopiaDescription("The upper bound of the ring (measured in meters from the center of the body).")]
            public NumericParser<Single> outerRadius
            {
                get { return ring.outerRadius; }
                set { ring.outerRadius = value; }
            }

            /// <summary>
            /// Distance between the top and bottom faces in milliradii
            /// </summary>
            [ParserTarget("thickness")]
            [KittopiaDescription("Distance between the top and bottom faces in milliradii.")]
            public NumericParser<Single> thickness 
            {
                get { return ring.thickness;  }
                set { ring.thickness = value; }
            }

            // Axis angle (inclination) of our ring
            [ParserTarget("angle")]
            [KittopiaDescription("Axis angle (inclination) of our ring.")]
            public NumericParser<Single> angle
            {
                get { return -ring.rotation.eulerAngles.x; }
                set { ring.rotation = Quaternion.Euler(-value, 0, 0); }
            }

            /// <summary>
            /// Angle between the absolute reference direction and the ascending node.
            /// Works just like the corresponding property on celestial bodies.
            /// </summary>
            [ParserTarget("longitudeOfAscendingNode")]
            [KittopiaDescription("Angle between the absolute reference direction and the ascending node.")]
            public NumericParser<Single> longitudeOfAscendingNode
            {
                get { return ring.longitudeOfAscendingNode;  }
                set { ring.longitudeOfAscendingNode = value; }
            }

            // Texture of our ring
            [ParserTarget("texture")]
            [KittopiaDescription("Texture of the ring")]
            public Texture2DParser texture
            {
                get { return ring.texture; }
                set { ring.texture = value; }
            }

            // Color of our ring
            [ParserTarget("color")]
            [KittopiaDescription("Color of the ring")]
            public ColorParser color
            {
                get { return ring.color; }
                set { ring.color = value; }
            }

            // Lock rotation of our ring?
            [ParserTarget("lockRotation")]
            [KittopiaDescription("Whether the rotation of the ring should always stay the same.")]
            public NumericParser<Boolean> lockRotation
            {
                get { return ring.lockRotation; }
                set { ring.lockRotation = value; }
            }

            /// <summary>
            /// Number of seconds for the ring to complete one rotation.
            /// If zero, fall back to matching parent body if lockRotation=false,
            /// and standing perfectly still if it's true.
            /// </summary>
            [ParserTarget("rotationPeriod")]
            [KittopiaDescription("Number of seconds for the ring to complete one rotation. If zero, fall back to matching parent body if lockRotation=false, and standing perfectly still if it's true.")]
            public NumericParser<Single> rotationPeriod
            {
                get { return ring.rotationPeriod;  }
                set { ring.rotationPeriod = value; }
            }

            // Unlit our ring?
            [ParserTarget("unlit")]
            [KittopiaDescription("Apply an unlit shader to the ring?")]
            public NumericParser<Boolean> unlit
            {
                get { return ring.unlit; }
                set { ring.unlit = value; }
            }

            // Use new shader with mie scattering and planet shadow?
            [ParserTarget("useNewShader")]
            [KittopiaDescription("Use the new custom ring shader instead of the builtin Unity shaders.")]
            public NumericParser<Boolean> useNewShader
            {
                get { return ring.useNewShader; }
                set { ring.useNewShader = value; }
            }

            // Penumbra multiplier for new shader
            [ParserTarget("penumbraMultiplier")]
            public NumericParser<Single> penumbraMultiplier
            {
                get { return ring.penumbraMultiplier; }
                set { ring.penumbraMultiplier = value; }
            }

            // Amount of vertices arount the ring
            [ParserTarget("steps")]
            [KittopiaDescription("Amount of vertices arount the ring.")]
            public NumericParser<Int32> steps
            {
                get { return ring.steps; }
                set { ring.steps = value; }
            }

            /// <summary>
            /// Number of times the texture should be tiled around the cylinder
            /// If zero, use the old behavior of sampling a thin diagonal strip
            /// from (0,0) to (1,1).
            /// </summary>
            [ParserTarget("tiles")]
            [KittopiaDescription("Number of times the texture should be tiled around the cylinder. If zero, use the old behavior of sampling a thin diagonal strip from (0,0) to (1,1).")]
            public NumericParser<Int32> tiles
            {
                get { return ring.tiles; }
                set { ring.tiles = value; }
            }

            /// <summary>
            /// This texture's opaque pixels cast shadows on our inner surface
            /// </summary>
            [ParserTarget("innerShadeTexture")]
            [KittopiaDescription("This texture's opaque pixels cast shadows on our inner surface.")]
            public Texture2DParser innerShadeTexture
            {
                get { return ring.innerShadeTexture;  }
                set { ring.innerShadeTexture = value; }
            }

            /// <summary>
            /// The inner shade texture repeats this many times over the inner surface
            /// </summary>
            [ParserTarget("innerShadeTiles")]
            [KittopiaDescription("The inner shade texture repeats this many times over the inner surface.")]
            public NumericParser<Int32> innerShadeTiles
            {
                get { return ring.innerShadeTiles;  }
                set { ring.innerShadeTiles = value; }
            }

            /// <summary>
            /// Number of seconds the inner shade texture takes to complete one rotation
            /// </summary>
            [ParserTarget("innerShadeRotationPeriod")]
            [KittopiaDescription("Number of seconds the inner shade texture takes to complete one rotation")]
            public NumericParser<Single> innerShadeRotationPeriod
            {
                get { return ring.innerShadeRotationPeriod;  }
                set { ring.innerShadeRotationPeriod = value; }
            }

            [KittopiaAction("Rebuild Ring")]
            [KittopiaDescription("Updates the mesh of the planetary ring.")]
            public void RebuildRing()
            {
                UnityEngine.Object.Destroy(ring.GetComponent<MeshRenderer>());
                UnityEngine.Object.Destroy(ring.GetComponent<MeshFilter>());
                ring.BuildRing();
            }

            [KittopiaDestructor]
            public void Destroy()
            {
                UnityEngine.Object.Destroy(ring.gameObject);
            }

            // Apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                Events.OnRingLoaderApply.Fire(this, node);
            }

            // Post-Apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                Events.OnRingLoaderPostApply.Fire(this, node);
            }

            // Initialize the RingLoader
            public RingLoader()
            {
                // Is this the parser context?
                if (!Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("Must be executed in Injector context.");
                }

                ring = new GameObject(generatedBody.name + "Ring").AddComponent<Ring>();
                ring.transform.parent = generatedBody.scaledVersion.transform;
                ring.planetRadius = (Single) generatedBody.celestialBody.Radius;

                // Need to check the parent body's rotation to orient the LAN properly
                ring.referenceBody = generatedBody.celestialBody;
            }

            /// <summary>
            /// Creates a new Ring Loader from a spawned CelestialBody.
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.CelestialBody, purpose = KittopiaConstructor.Purpose.Create)]
            public RingLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }
                
                ring = new GameObject(body.transform.name + "Ring").AddComponent<Ring>();
                ring.transform.parent = body.scaledBody.transform;
                ring.planetRadius = (Single) body.Radius;

                // Need to check the parent body's rotation to orient the LAN properly
                ring.referenceBody = body;
            }

            // Initialize the RingLoader
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public RingLoader(Ring ring)
            {
                this.ring = ring;
            }
        }
    }
}
