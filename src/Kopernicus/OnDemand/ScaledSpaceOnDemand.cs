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
using System.Collections;
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
        private static readonly Int32 BumpMap = Shader.PropertyToID("_BumpMap");
        private static readonly Int32 MainTex = Shader.PropertyToID("_MainTex");

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

        // Texture handles
        private TextureHandle<Texture2D> bumpMapHandle;
        private TextureHandle<Texture2D> mainTexHandle;

        // Active loading coroutine
        private IEnumerator loaderCoroutine;

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
                return;

            // If we're past the unload time then unload
            if (System.Diagnostics.Stopwatch.GetTimestamp() > _unloadTime)
                UnloadTextures();
        }

        // OnBecameVisible(), load the texture
        private void OnBecameVisible()
        {
            // It is supposed to be loaded now so clear the unload time
            _unloadTime = 0;

            // If it is already loaded then do nothing
            if (isLoaded)
                return;

            // Otherwise we load it
            LoadTexturesAsync();
        }

        // OnBecameInvisible(), kill the texture
        private void OnBecameInvisible()
        {
            // If it's not loaded or in the process of loading then do nothing
            if (!isLoaded && loaderCoroutine is null)
                return;

            // Set the time at which to unload
            _unloadTime = System.Diagnostics.Stopwatch.GetTimestamp() + _unloadDelay;
        }

        public void LoadTextures()
        {
            if (loaderCoroutine is null)
                LoadTexturesAsync();

            while (loaderCoroutine?.MoveNext() ?? false) { }
        }

        /// <summary>
        /// Start loading the scaled-space textures for this body.
        /// </summary>
        /// 
        /// <remarks>
        /// To wait until the textures are loaded wait until <see cref="isLoaded"/>
        /// is true. If you need the textures to be loaded synchronously then you
        /// can call <see cref="LoadTextures"/>, be aware that this will result
        /// in significant stutter when doing so.
        /// </remarks>
        public void LoadTexturesAsync()
        {
            if (isLoaded || loaderCoroutine is not null)
                return;

            loaderCoroutine = DoLoadTexturesAsync();
            StartCoroutine(loaderCoroutine);
        }

        IEnumerator DoLoadTexturesAsync()
        {
            Debug.Log("[OD] --> ScaledSpaceDemand.LoadTextures loading " + texture + " and " + normals);

            using var guard = new ClearEnumeratorGuard(this);

            var options = new TextureLoadOptions
            {
                Unreadable = true,
                Hint = TextureLoadHint.BatchAsynchronous,
            };
            mainTexHandle = TextureLoader.LoadTexture<Texture2D>(texture, options);
            bumpMapHandle = TextureLoader.LoadTexture<Texture2D>(normals, options);

            // Load Diffuse
            yield return mainTexHandle;

            try
            {
                scaledRenderer.sharedMaterial.SetTexture(MainTex, mainTexHandle.GetTexture());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OD] Failed to load texture {texture}");
                Debug.LogException(ex);
            }

            // Load Normals
            yield return bumpMapHandle;

            try
            {
                scaledRenderer.sharedMaterial.SetTexture(BumpMap, bumpMapHandle.GetTexture());
            }
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
            if (!isLoaded && loaderCoroutine is null)
                return;

            // Schedule the texture for destruction in LateUpdate. If we get
            // loaded again then there won't be need to reload anything.
            _unloadTime = System.Diagnostics.Stopwatch.GetTimestamp();
        }

        private void OnDestroy()
        {
            // Ensure textures get cleaned up if not done already
            mainTexHandle?.Dispose();
            bumpMapHandle?.Dispose();

            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
        }

        struct ClearEnumeratorGuard(ScaledSpaceOnDemand parent) : IDisposable
        {
            public void Dispose() => parent.loaderCoroutine = null;
        }
    }
}
