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

using System;
using System.IO;
using Kopernicus.Configuration;
using KSPTextureLoader;
using UnityEngine;

namespace Kopernicus.OnDemand
{
    /// <summary>
    /// Class to load ScaledSpace Textures on Demand
    /// </summary>
    public class ScaledSpaceOnDemand : MonoBehaviour
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
        private Int64 _unloadTime;

        // The number of timestamp ticks in a second
        private Int64 _unloadDelay;
        private static readonly Int32 BumpMap = Shader.PropertyToID("_BumpMap");
        private static readonly Int32 MainTex = Shader.PropertyToID("_MainTex");

        // Texture handles
        private TextureHandle<Texture2D> bumpMapHandle;
        private TextureHandle<Texture2D> mainTexHandle;

        // Start(), get the scaled Mesh renderer
        public void Start()
        {
            _unloadDelay = System.Diagnostics.Stopwatch.Frequency * OnDemandStorage.OnDemandUnloadDelay;
            scaledRenderer = GetComponent<MeshRenderer>();
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
            UnloadTextures();
        }

        private void LateUpdate()
        {
            // If we aren't loaded or we're not wanting to unload then do nothing
            if (!isLoaded || _unloadTime == 0)
            {
                return;
            }

            // If we're past the unload time then unload
            if (System.Diagnostics.Stopwatch.GetTimestamp() > _unloadTime)
            {
                UnloadTextures();
            }
        }

        // OnBecameVisible(), load the texture
        private void OnBecameVisible()
        {
            // It is supposed to be loaded now so clear the unload time
            _unloadTime = 0;

            // If it is already loaded then do nothing
            if (isLoaded)
            {
                return;
            }

            // Otherwise we load it
            LoadTextures();
        }

        // OnBecameInvisible(), kill the texture
        private void OnBecameInvisible()
        {
            // If it's not loaded then do nothing
            if (!isLoaded)
            {
                return;
            }

            // Set the time at which to unload
            _unloadTime = System.Diagnostics.Stopwatch.GetTimestamp() + _unloadDelay;
        }

        public void LoadTextures()
        {
            Debug.Log("[OD] --> ScaledSpaceDemand.LoadTextures loading " + texture + " and " + normals);

            var options = new TextureLoadOptions
            {
                Unreadable = true,
                Hint = TextureLoadHint.BatchSynchronous,
            };
            mainTexHandle = TextureLoader.LoadTexture<Texture2D>(texture, options);
            bumpMapHandle = TextureLoader.LoadTexture<Texture2D>(normals, options);

            // Load Diffuse
            try
            {
                scaledRenderer.sharedMaterial.SetTexture(MainTex, mainTexHandle.GetTexture());
            }
            // Ignore cases where the texture did not exist.
            catch (FileNotFoundException) { }
            catch (Exception ex)
            {
                Debug.LogError($"[OD] Failed to load texture {texture}");
                Debug.LogException(ex);
            }

            // Load Normals
            try
            {
                scaledRenderer.sharedMaterial.SetTexture(BumpMap, bumpMapHandle.GetTexture());
            }
            // Ignore cases where the texture did not exist.
            catch (FileNotFoundException) { }
            catch (Exception ex)
            {
                Debug.LogError($"[OD] Failed to load texture {texture}");
                Debug.LogException(ex);
            }

            // Events
            Events.OnScaledSpaceLoad.Fire(this);

            // Flags
            isLoaded = true;
        }

        public void UnloadTextures()
        {
            Debug.Log("[OD] <--- ScaledSpaceDemand.UnloadTextures destroying " + texture + " and " + normals);

            // Kill Diffuse
            mainTexHandle?.Dispose();
            mainTexHandle = null;

            // Kill Normals
            bumpMapHandle?.Dispose();
            bumpMapHandle = null;

            // Events
            Events.OnScaledSpaceUnload.Fire(this);

            // Flags
            isLoaded = false;
        }

        // Unload all textures when we switch to a new scene
        private void OnGameSceneLoadRequested(GameScenes scene)
        {
            UnloadTextures();
        }

        private void OnDestroy()
        {
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
        }
    }
}
