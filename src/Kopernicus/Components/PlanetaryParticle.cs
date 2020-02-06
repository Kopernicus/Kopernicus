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
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

#pragma warning disable 618

using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Kopernicus.Components
{
    /// <summary>
    /// Class to create a particle effect on a planet
    /// </summary>
    public class PlanetParticleEmitter : MonoBehaviour
    {
        // Components
        public KSPParticleEmitter emitter;
        public MeshFilter filter;

        // Variables
        public String target = "None";
        public Single speedScale;
        public Int32 minEmission, maxEmission;
        public Single minEnergy, maxEnergy;
        public Single minSize, maxSize;
        public Single sizeGrow;
        public Color[] colorAnimation;
        public Texture2D mainTexture;
        public Vector3 randomVelocity;
        public Vector3 scale = Vector3.one;
        public Mesh mesh;
        public Boolean collideable;
        public Vector3 force = Vector3.zero;

        /// <summary>
        /// Attaches a Planet Particle Emitter to a host, that has a meshfilter attached
        /// </summary>
        public static PlanetParticleEmitter Create(GameObject host)
        {
            // Create the GameObject
            GameObject emitter = new GameObject();
            emitter.transform.parent = host.transform;
            emitter.transform.localPosition = Vector3.zero;
            emitter.SetLayerRecursive(10);
            emitter.name = "Particles";

            // Add the Particle Emitter
            return emitter.AddComponent<PlanetParticleEmitter>();
        }

        /// <summary>
        /// The main initialisation. Here we create the subcomponents.
        /// </summary>
        private void Awake()
        {
            if (!GetComponent<KSPParticleEmitter>())
            {
                emitter = gameObject.AddComponent<KSPParticleEmitter>();
                emitter.useWorldSpace = false;
                emitter.emit = true;
                emitter.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
                emitter.doesAnimateColor = true;
            }
            else
            {
                emitter = GetComponent<KSPParticleEmitter>();
            }

            filter = !GetComponent<MeshFilter>() ? gameObject.AddComponent<MeshFilter>() : GetComponent<MeshFilter>();
        }

        /// <summary>
        /// The position of our target
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public Transform TargetTransform { get; set; }

        /// <summary>
        /// Updates the target position and emits the particles
        /// </summary>
        private void Update()
        {
            // Update the values
            emitter.minSize = minSize;
            emitter.maxSize = maxSize;
            emitter.sizeGrow = sizeGrow;
            emitter.colorAnimation = colorAnimation;
            emitter.material.mainTexture = mainTexture;
            filter.mesh = filter.sharedMesh = mesh ? mesh : transform.parent.GetComponent<MeshFilter>().sharedMesh;
            emitter.force = force;

            // We have a target
            if (target != "None")
            {
                if (!TargetTransform)
                {
                    TargetTransform = UBI.GetBody(target).scaledBody.transform;
                }

                Vector3 speed = TargetTransform.position;
                speed -= transform.parent.position;
                speed *= speedScale;
                emitter.minEnergy = minEnergy / TimeWarp.CurrentRate;
                emitter.maxEnergy = maxEnergy / TimeWarp.CurrentRate;
                emitter.maxEmission = Convert.ToInt32(maxEmission * TimeWarp.CurrentRate);
                emitter.minEmission = Convert.ToInt32(minEmission * TimeWarp.CurrentRate);
                emitter.rndVelocity = randomVelocity * TimeWarp.CurrentRate;
                speed *= TimeWarp.CurrentRate;
                emitter.worldVelocity = speed;
            }

            transform.localScale = scale;
        }
    }
}