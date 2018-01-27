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
using UnityEngine;

namespace Kopernicus
{
    namespace OnDemand
    {
        /// <summary>
        /// Class to load ScaledSpace Textures on Demand
        /// </summary>
        public class ScaledSpaceDemand : MonoBehaviour
        {
            // Path to the Texture
            public String texture;

            // Path to the normal map
            public String normals;

            // ScaledSpace MeshRenderer
            public MeshRenderer scaledRenderer;

            // State of the texture
            public Boolean isLoaded = true;

            // If non-zero the textures will be unloaded once the timer exceeds the value
            private long unloadTime;

            // The number of timestamp ticks in a second
            private long unloadDelay;

            // The body we're attached to
            private CelestialBody body;

            // Whether the body was visible in the last frame
            private Boolean lastIsInView;

            // Start(), get the scaled Mesh renderer
            void Start()
            {
                unloadDelay = System.Diagnostics.Stopwatch.Frequency * OnDemandStorage.onDemandUnloadDelay;
                scaledRenderer = GetComponent<MeshRenderer>();
                body = PSystemManager.Instance.localBodies.Find(b => b.scaledBody == gameObject);
                lastIsInView = false;
                UnloadTextures();
            }

            void LateUpdate()
            {
                // If we are rendered, load the textures
                Boolean isInView =
                    IsInView(
                        HighLogic.LoadedSceneHasPlanetarium || MapView.MapIsEnabled
                            ? PlanetariumCamera.Camera
                            : ScaledCamera.Instance.cam, body);
                if (isInView && !lastIsInView)
                {
                    OnBodyBecameVisible();
                }
                else if (!isInView && lastIsInView)
                {
                    OnBodyBecameInvisible();
                }
                lastIsInView = isInView;

                // If we aren't loaded or we're not wanting to unload then do nothing
                if (!isLoaded || unloadTime == 0)
                    return;

                // If we're past the unload time then unload
                if (System.Diagnostics.Stopwatch.GetTimestamp() > unloadTime)
                    UnloadTextures();
            }

            // OnBecameVisible(), load the texture
            void OnBodyBecameVisible()
            {
                // It is supposed to be loaded now so clear the unload time
                unloadTime = 0;

                // If it is already loaded then do nothing
                if (isLoaded)
                    return;

                // Otherwise we load it
                LoadTextures();
            }

            // OnBecameInvisible(), kill the texture
            void OnBodyBecameInvisible()
            {
                // If it's not loaded then do nothing
                if (!isLoaded)
                    return;

                // Set the time at which to unload
                unloadTime = System.Diagnostics.Stopwatch.GetTimestamp() + unloadDelay;
            }

            internal void LoadTextures()
            {
                Debug.Log("[OD] --> ScaledSpaceDemand.LoadTextures loading " + texture + " and " + normals);

                // Load Diffuse
                if (OnDemandStorage.TextureExists(texture))
                {
                    scaledRenderer.material.SetTexture("_MainTex",
                        OnDemandStorage.LoadTexture(texture, false, true, true));
                }

                // Load Normals
                if (OnDemandStorage.TextureExists(normals))
                {
                    scaledRenderer.material.SetTexture("_BumpMap",
                        OnDemandStorage.LoadTexture(normals, false, true, false));
                }

                // Events
                Events.OnScaledSpaceLoad.Fire(this);

                // Flags
                isLoaded = true;
            }

            internal void UnloadTextures()
            {
                Debug.Log("[OD] <--- ScaledSpaceDemand.UnloadTextures destroying " + texture + " and " + normals);

                // Kill Diffuse
                if (OnDemandStorage.TextureExists(texture))
                {
                    DestroyImmediate(scaledRenderer.material.GetTexture("_MainTex"));
                }

                // Kill Normals
                if (OnDemandStorage.TextureExists(normals))
                {
                    DestroyImmediate(scaledRenderer.material.GetTexture("_BumpMap"));
                }

                // Events
                Events.OnScaledSpaceUnload.Fire(this);

                // Flags
                isLoaded = false;
            }

            private bool IsInView(Camera cam, CelestialBody body)
            {
                if (body == null)
                    return false;

                Vector3 pointOnScreen =
                    cam.WorldToScreenPoint(body.scaledBody.GetComponentInChildren<Renderer>().bounds.center);

                //Is in front
                if (pointOnScreen.z < 0)
                {
                    return false;
                }

                //Is in FOV
                if (pointOnScreen.x < 0 || pointOnScreen.x > Screen.width ||
                    pointOnScreen.y < 0 || pointOnScreen.y > Screen.height)
                {
                    return false;
                }

                RaycastHit hit;
                if (Physics.Linecast(cam.transform.position,
                    body.scaledBody.GetComponentInChildren<Renderer>().bounds.center, out hit))
                {
                    if (hit.transform.name != body.scaledBody.name)
                    {
                        return false;
                    }
                }

                Single pixelSize = (Single) body.Radius * 2 * Mathf.Rad2Deg * Screen.height /
                                   (Vector3.Distance(cam.transform.position,
                                        body.scaledBody.GetComponentInChildren<Renderer>().bounds.center) *
                                    cam.fieldOfView);
                return pixelSize >= 1;
            }
        }
    }
}