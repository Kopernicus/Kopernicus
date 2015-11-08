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
 
using Kopernicus.Components;
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
            [ParserTarget("innerRadius", optional = true, allowMerge = false)]
            public NumericParser<float> innerRadius
            {
                get { return ring.innerRadius; }
                set { ring.innerRadius = value; }
            }

            // Outer Radius of our ring
            [ParserTarget("outerRadius", optional = true, allowMerge = false)]
            public NumericParser<float> outerRadius
            {
                get { return ring.outerRadius; }
                set { ring.outerRadius = value; }
            }

            // Axis angle of our ring
            [ParserTarget("angle", optional = true, allowMerge = false)]
            public NumericParser<float> angle
            {
                get { return ring.rotation.eulerAngles.x; }
                set { ring.rotation = Quaternion.Euler(value, 0, 0); }
            }

            // Texture of our ring
            [ParserTarget("texture", optional = true, allowMerge = false)]
            public Texture2DParser texture
            {
                get { return ring.texture; }
                set { ring.texture = value; }
            }

            // Color of our ring
            [ParserTarget("color", optional = true, allowMerge = false)]
            public ColorParser color
            {
                get { return ring.color; }
                set { ring.color = value; }
            }

            // Lock rotation of our ring?
            [ParserTarget("lockRotation", optional = true, allowMerge = false)]
            public NumericParser<bool> lockRotation
            {
                get { return ring.lockRotation; }
                set { ring.lockRotation = value; }
            }

            // Unlit our ring?
            [ParserTarget("unlit", optional = true, allowMerge = false)]
            public NumericParser<bool> unlit
            {
                get { return ring.unlit; }
                set { ring.unlit = value; }
            }

            // Amount of vertices arount the ring
            [ParserTarget("steps", optional = true, allowMerge = false)]
            public NumericParser<int> steps
            {
                get { return ring.steps; }
                set { ring.steps = value; }
            }

            // Initialize the RingLoader
            public RingLoader()
            {
                ring = new GameObject(generatedBody.name + "Ring").AddComponent<Ring>();
                ring.transform.parent = generatedBody.scaledVersion.transform;
            }

            // Initialize the RingLoader
            public RingLoader(Ring ring)
            {
                this.ring = ring;
            }

            // Apply event
            void IParserEventSubscriber.Apply(ConfigNode node) { }

            // Post-Apply event
            void IParserEventSubscriber.PostApply(ConfigNode node) { }
        }
    }
}