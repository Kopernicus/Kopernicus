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
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class RingLoader : BaseLoader, IParserEventSubscriber
        {
            // Set-up our custom ring
            public Ring ring { get; set; }

            // Inner Radius of our ring
            [ParserTarget("innerRadius")]
            public NumericParser<Single> innerRadius
            {
                get { return ring.innerRadius; }
                set { ring.innerRadius = value; }
            }

            // Outer Radius of our ring
            [ParserTarget("outerRadius")]
            public NumericParser<Single> outerRadius
            {
                get { return ring.outerRadius; }
                set { ring.outerRadius = value; }
            }

            /// <summary>
            /// Distance between the top and bottom faces in milliradii
            /// </summary>
            [ParserTarget("thickness")]
            public NumericParser<Single> thickness {
                get { return ring.thickness;  }
                set { ring.thickness = value; }
            }

            // Axis angle (inclination) of our ring
            [ParserTarget("angle")]
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
            public NumericParser<Single> longitudeOfAscendingNode
            {
                get { return ring.longitudeOfAscendingNode;  }
                set { ring.longitudeOfAscendingNode = value; }
            }

            // Texture of our ring
            [ParserTarget("texture")]
            public Texture2DParser texture
            {
                get { return ring.texture; }
                set { ring.texture = value; }
            }

            // Color of our ring
            [ParserTarget("color")]
            public ColorParser color
            {
                get { return ring.color; }
                set { ring.color = value; }
            }

            // Lock rotation of our ring?
            [ParserTarget("lockRotation")]
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
            public NumericParser<Single> rotationPeriod
            {
                get { return ring.rotationPeriod;  }
                set { ring.rotationPeriod = value; }
            }

            // Unlit our ring?
            [ParserTarget("unlit")]
            public NumericParser<Boolean> unlit
            {
                get { return ring.unlit; }
                set { ring.unlit = value; }
            }

            // Use new shader with mie scattering and planet shadow?
            [ParserTarget("useNewShader")]
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
            public NumericParser<Int32> tiles
            {
                get { return ring.tiles; }
                set { ring.tiles = value; }
            }

            /// <summary>
            /// This texture's opaque pixels cast shadows on our inner surface
            /// </summary>
            [ParserTarget("innerShadeTexture")]
            public Texture2DParser innerShadeTexture
            {
                get { return ring.innerShadeTexture;  }
                set { ring.innerShadeTexture = value; }
            }

            /// <summary>
            /// The inner shade texture repeats this many times over the inner surface
            /// </summary>
            [ParserTarget("innerShadeTiles")]
            public NumericParser<Int32> innerShadeTiles
            {
                get { return ring.innerShadeTiles;  }
                set { ring.innerShadeTiles = value; }
            }

            /// <summary>
            /// Number of seconds the inner shade texture takes to complete one rotation
            /// </summary>
            [ParserTarget("innerShadeRotationPeriod")]
            public NumericParser<Single> innerShadeRotationPeriod
            {
                get { return ring.innerShadeRotationPeriod;  }
                set { ring.innerShadeRotationPeriod = value; }
            }

            // Initialize the RingLoader
            public RingLoader()
            {
                ring = new GameObject(generatedBody.name + "Ring").AddComponent<Ring>();
                ring.transform.parent = generatedBody.scaledVersion.transform;
                ring.planetRadius = (Single) generatedBody.celestialBody.Radius;

                // Need to check the parent body's rotation to orient the LAN properly
                ring.referenceBody = generatedBody.celestialBody;
            }

            // Initialize the RingLoader
            public RingLoader(Ring ring)
            {
                this.ring = ring;
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
        }
    }
}
