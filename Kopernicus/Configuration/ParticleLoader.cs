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
            PlanetaryParticle particle;
            GameObject body;

            // Target of our particles
            [ParserTarget("target", optional = true, allowMerge = false)]
            public string targetLoader
            {
                set { particle.target = value; }
            }

            // minEmission of particles
            [ParserTarget("minEmission", optional = true, allowMerge = false)]
            public NumericParser<float> minEmission
            {
                set { particle.minEmission = value.value; }
            }

            // maxEmission of particles
            [ParserTarget("maxEmission", optional = true, allowMerge = false)]
            public NumericParser<float> maxEmission
            {
                set { particle.maxEmission = value.value; }
            }

            // minimum lifespan of particles
            [ParserTarget("lifespanMin", optional = true, allowMerge = false)]
            public NumericParser<float> lifespanMin
            {
                set { particle.minEnergy = value.value; }
            }

            // maximum lifespan of particles
            [ParserTarget("lifespanMax", optional = true, allowMerge = false)]
            public NumericParser<float> lifespanMax
            {
                set { particle.maxEnergy = value.value; }

            }

            // minimum size of particles
            [ParserTarget("sizeMin", optional = true, allowMerge = false)]
            public NumericParser<float> sizeMin
            {
                set { particle.emitter.minSize = value.value; }
            }

            // maximum size of particles
            [ParserTarget("sizeMax", optional = true, allowMerge = false)]
            public NumericParser<float> sizeMax
            {
                set { particle.emitter.maxSize = value.value; }
            }

            // speedScale of particles
            [ParserTarget("speedScale", optional = true, allowMerge = false)]
            public NumericParser<float> speedScaleLoader
            {
                set { particle.speedScale = value.value; }
            }

            // grow rate of particles
            [ParserTarget("rate", optional = true, allowMerge = false)]
            public NumericParser<float> rate
            {
                set { particle.animator.sizeGrow = value.value; }
            }

            // rand Velocity of particles
            [ParserTarget("randVelocity", optional = true, allowMerge = false)]
            public Vector3Parser randVelocity
            {
                set { particle.randomVelocity = value.value; }
            }

            // Texture of particles
            [ParserTarget("texture", optional = true, allowMerge = false)]
            public Texture2DParser texture
            {
                set { particle.Renderer.material.mainTexture = value.value; }
            }
            
            public ParticleLoader(GameObject scaledPlanet)
            {
                this.body = scaledPlanet;
            }

            // Apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                particle = PlanetaryParticle.CreateInstance(body);
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
                particle.animator.colorAnimation = colors.ToArray();
            }

            public class PlanetaryParticle : MonoBehaviour
            {
                public ParticleEmitter emitter;
                public ParticleAnimator animator;
                public ParticleRenderer Renderer;
                public string target = "Sun";
                public float speedScale = 0f;
                public float minEmission, maxEmission;
                public float minEnergy, maxEnergy;
                public Vector3 randomVelocity;

                public static PlanetaryParticle CreateInstance(GameObject body)
                {
                    PlanetaryParticle p = body.AddComponent<PlanetaryParticle>();
                    p.emitter = (ParticleEmitter)body.AddComponent("MeshParticleEmitter");
                    p.animator = body.AddComponent<ParticleAnimator>();
                    p.Renderer = body.AddComponent<ParticleRenderer>();
                    p.Renderer.material = new Material(Shader.Find("Particles/Alpha Blended"));
                    p.emitter.useWorldSpace = false;
                    p.animator.doesAnimateColor = true;
                    DontDestroyOnLoad(p);
                    return p;
                }

                public void Update()
                {
                    emitter.emit = true;
                    Vector3 speed = ScaledSpace.Instance.scaledSpaceTransforms.Find(t => t.name == target).position;
                    speed -= transform.position;
                    speed *= speedScale;
                    emitter.minEnergy = minEnergy / TimeWarp.CurrentRate;
                    emitter.maxEnergy = maxEnergy / TimeWarp.CurrentRate;
                    emitter.maxEmission = maxEmission * TimeWarp.CurrentRate;
                    emitter.minEmission = minEmission * TimeWarp.CurrentRate;
                    emitter.rndVelocity = randomVelocity * TimeWarp.CurrentRate;
                    speed *= TimeWarp.CurrentRate;
                    emitter.worldVelocity = speed;
                }
            }
        }
    }
}
