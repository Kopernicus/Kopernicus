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
 * 
 * Code based on KittiopaTech, modified by Thomas P.
 */

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Kopernicus
{
	namespace Configuration
	{
		[RequireConfigType(ConfigType.Node)]
        public class ParticleLoader : IParserEventSubscriber
        {
            // Set-up our parental objects
            ParticleEmitter MainEmitter;
            ParticleAnimator ParticleAnim;
            ParticleRenderer ParticleRender;
            string target;
            GameObject body;
            float speedScale;

            // Target of our particles
            [ParserTarget("target", optional = true, allowMerge = false)]
            public string targetLoader
            {
                set { target = value; }
            }

            // minEmission of particles
            [ParserTarget("minEmission", optional = true, allowMerge = false)]
            public NumericParser<float> minEmission
            {
                set { MainEmitter.minEmission = value.value; }
            }

            // maxEmission of particles
            [ParserTarget("maxEmission", optional = true, allowMerge = false)]
            public NumericParser<float> maxEmission
            {
                set { MainEmitter.maxEmission = value.value; }
            }

            // minimum lifespan of particles
            [ParserTarget("lifespanMin", optional = true, allowMerge = false)]
            public NumericParser<float> lifespanMin
            {
                set { MainEmitter.minEnergy = value.value; }
            }

            // maximum lifespan of particles
            [ParserTarget("lifespanMax", optional = true, allowMerge = false)]
            public NumericParser<float> lifespanMax
            {
                set { MainEmitter.maxEnergy = value.value; }

            }

            // minimum size of particles
            [ParserTarget("sizeMin", optional = true, allowMerge = false)]
            public NumericParser<float> sizeMin
            {
                set { MainEmitter.minSize = value.value; }
            }

            // maximum size of particles
            [ParserTarget("sizeMax", optional = true, allowMerge = false)]
            public NumericParser<float> sizeMax
            {
                set { MainEmitter.maxSize = value.value; }
            }

            // speedScale of particles
            [ParserTarget("speedScale", optional = true, allowMerge = false)]
            public NumericParser<float> speedScaleLoader
            {
                set { speedScale = value.value; }
            }

            // grow rate of particles
            [ParserTarget("rate", optional = true, allowMerge = false)]
            public NumericParser<float> rate
            {
                set { ParticleAnim.sizeGrow = value.value; }
            }

            // rand Velocity of particles
            [ParserTarget("randVelocity", optional = true, allowMerge = false)]
            public Vector3Parser randVelocity
            {
                set { MainEmitter.rndVelocity = value.value; }
            }

            // Texture of particles
            [ParserTarget("texture", optional = true, allowMerge = false)]
            public Texture2DParser texture
            {
                set { ParticleRender.material.mainTexture = value.value; }
            }

            public ParticleLoader(GameObject scaledPlanet)
            {
                this.body = scaledPlanet;
            }

            // Apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                MainEmitter = (ParticleEmitter)body.AddComponent("MeshParticleEmitter");
                ParticleRender = body.AddComponent<ParticleRenderer>();
                ParticleAnim = body.AddComponent<ParticleAnimator>();
                ParticleRender.material = new Material(Shader.Find("Particles/Alpha Blended"));

                MainEmitter.useWorldSpace = false;
                ParticleAnim.doesAnimateColor = true;
            }

            // Post-Apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                List<Color> colors = new List<Color>();
                foreach (string color in node.GetNode("Colors").GetValuesStartsWith("color"))
                {
                    Vector4 c = ConfigNode.ParseVector4(color);
                    colors.Add(new Color(c.x, c.y, c.z, c.w));

                }
                ParticleAnim.colorAnimation = colors.ToArray();
            }
            
            public void Update()
            {
                Vector3 Speedvec = ScaledSpace.Instance.scaledSpaceTransforms.Find(t => t.name == target).position;
                Speedvec -= body.transform.position;

                Speedvec *= speedScale;

                MainEmitter.minEnergy = MainEmitter.minEnergy / TimeWarp.CurrentRate;
                MainEmitter.maxEnergy = MainEmitter.maxEnergy / TimeWarp.CurrentRate;

                MainEmitter.maxEmission = MainEmitter.maxEmission * TimeWarp.CurrentRate;
                MainEmitter.minEmission = MainEmitter.minEmission * TimeWarp.CurrentRate;

                MainEmitter.rndVelocity = MainEmitter.rndVelocity * TimeWarp.CurrentRate;

                Speedvec *= TimeWarp.CurrentRate;

                MainEmitter.worldVelocity = Speedvec;
            }
        }
    }
}
