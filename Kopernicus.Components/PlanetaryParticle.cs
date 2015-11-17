/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// Class to create a particle effect on a planet
        /// </summary>
        public class PlanetParticleEmitter : MonoBehaviour
        {
            /// Components
            public ParticleEmitter emitter;
            public ParticleAnimator animator;
            public ParticleRenderer renderer;

            /// Variables
            public string target = "Sun";
            public float speedScale = 0f;
            public float minEmission, maxEmission;
            public float minEnergy, maxEnergy;
            public float minSize, maxSize;
            public float sizeGrow;
            public Color[] colorAnimation;
            public Texture2D mainTexture;
            public Vector3 randomVelocity;

            /// <summary>
            /// The main initialisation. Here we create the subcomponents.
            /// </summary>
            void Awake()
            {
                if (!GetComponent<ParticleEmitter>())
                {
                    emitter = (ParticleEmitter)gameObject.AddComponent("MeshParticleEmitter");
                    emitter.useWorldSpace = false;
                    emitter.emit = true;
                }
                else
                {
                    emitter = GetComponent<ParticleEmitter>();
                }

                if (!GetComponent<ParticleAnimator>())
                {
                    animator = gameObject.AddComponent<ParticleAnimator>();
                    animator.doesAnimateColor = true;
                }
                else
                {
                    animator = GetComponent<ParticleAnimator>();
                }

                if (!GetComponent<ParticleRenderer>())
                {
                    renderer = gameObject.AddComponent<ParticleRenderer>();
                    renderer.material = new Material(Shader.Find("Particles/Alpha Blended"));
                }
                else
                {
                    renderer = GetComponent<ParticleRenderer>();
                }
            }

            /// <summary>
            /// Load the values into the components
            /// </summary>
            void Start()
            {
                emitter.minSize = minSize;
                emitter.maxSize = maxSize;
                animator.sizeGrow = sizeGrow;
                animator.colorAnimation = colorAnimation;
                renderer.material.mainTexture = mainTexture;
            }

            /// <summary>
            /// Updates the target position and emits the particles
            /// </summary>
            void Update()
            {
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