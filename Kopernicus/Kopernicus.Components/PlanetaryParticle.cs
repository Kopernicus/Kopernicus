/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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
using System.Reflection;
using System.IO;
using System.Linq;

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
            public MeshFilter filter;

            /// Variables
            public string target = "None";
            public float speedScale = 0f;
            public float minEmission, maxEmission;
            public float minEnergy, maxEnergy;
            public float minSize, maxSize;
            public float sizeGrow;
            public Color[] colorAnimation;
            public Texture2D mainTexture;
            public Vector3 randomVelocity;
            public Vector3 scale = Vector3.one;
            public Mesh mesh;
            public bool collideable;
            public Vector3 force = Vector3.zero;

            /// <summary>
            /// Attaches a Planet Particle Emitter to a host, that has a meshfilter attached
            /// </summary>
            public static PlanetParticleEmitter Create(GameObject host)
            {
                // Create the GameObject
                GameObject emitter = GetWorldParticleCollider();
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
            void Awake()
            {
                if (!GetComponent<ParticleEmitter>())
                {
                    emitter = gameObject.AddComponent<MeshParticleEmitter>();
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

                filter = !GetComponent<MeshFilter>() ? gameObject.AddComponent<MeshFilter>() : GetComponent<MeshFilter>();
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
                filter.mesh = filter.sharedMesh = mesh ?? transform.parent.GetComponent<MeshFilter>().sharedMesh;
                animator.force = force;
            }

            /// <summary>
            /// The position of our target
            /// </summary>
            public Transform targetTransform { get; set; }

            /// <summary>
            /// Updates the target position and emits the particles
            /// </summary>
            void Update()
            {
                // We have a target
                if (target != "None")
                {
                    if (targetTransform == null) targetTransform = PSystemManager.Instance.localBodies.Find(b => b.transform.name == target).scaledBody.transform;
                    Vector3 speed = targetTransform.position;
                    speed -= transform.parent.position;
                    speed *= speedScale;
                    emitter.minEnergy = minEnergy / TimeWarp.CurrentRate;
                    emitter.maxEnergy = maxEnergy / TimeWarp.CurrentRate;
                    emitter.maxEmission = maxEmission * TimeWarp.CurrentRate;
                    emitter.minEmission = minEmission * TimeWarp.CurrentRate;
                    emitter.rndVelocity = randomVelocity * TimeWarp.CurrentRate;
                    speed *= TimeWarp.CurrentRate;
                    emitter.worldVelocity = speed;
                }
                transform.localScale = scale;
            }

            /// <summary>
            /// Detect Particle collisions
            /// </summary>
            /// <param name="other"></param>
            void OnParticleCollision(GameObject other)
            {
                // If we dont want collisions, abort
                if (!collideable)
                    return;

                // Don't collide with the planet
                if (other == transform.parent.gameObject)
                    return;

                // We need a rigidbody
                if (!other.GetComponent<Rigidbody>())
                    return;

                // Do funny things
                Particle partice = emitter.particles.OrderBy(p => Vector3.Distance(other.transform.position, p.position)).First();
                other.GetComponent<Rigidbody>().AddForceAtPosition(partice.velocity.normalized * partice.energy, partice.position, ForceMode.Impulse);
                partice.energy = 0;
            }

            /// <summary>
            /// Returns a copy of the world particle collider prefab
            /// </summary>
            public static GameObject GetWorldParticleCollider()
            {
                Stream stream = typeof(PlanetParticleEmitter).Assembly.GetManifestResourceStream("Kopernicus.Components.Assets.WorldParticleCollider.unity3d");
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                AssetBundle bundle = AssetBundle.LoadFromMemory(buffer);
                GameObject collider = Instantiate(bundle.LoadAsset("WorldParticleCollider", typeof(GameObject))) as GameObject;
                bundle.Unload(true);
                return collider;
            }
        }
    }
}