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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kopernicus.Configuration;
using KSPTextureLoader;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kopernicus.OnDemand
{
    /// <summary>
    /// A class to load textures for an object when it becomes visible.
    /// </summary>
    public class ScaledSpaceOnDemand : MonoBehaviour, ISerializationCallbackReceiver
    {
        // Path to the Texture
        [Obsolete("texture is no longer used and is included only for backwards compatibility")]
        public String texture;

        // Path to the normal map
        [Obsolete("normals is no longer used and is included only for backwards compatibility")]
        public String normals;

        private static readonly Int32 BumpMap = Shader.PropertyToID("_BumpMap");
        private static readonly Int32 MainTex = Shader.PropertyToID("_MainTex");

        public List<OnDemandTextureEntry> Entries { get; set; } = [];

        private readonly List<TextureHandle> Handles = [];

        // ScaledSpace MeshRenderer
        public MeshRenderer scaledRenderer;

        // State of the texture
        public Boolean isLoaded = true;

        // If non-zero the textures will be unloaded once the timer exceeds the value
        private Int64 _unloadTime;

        // The number of timestamp ticks in a second
        private Int64 _unloadDelay;

        // Active loading coroutine
        private IEnumerator loaderCoroutine;

        // Start(), get the scaled Mesh renderer
        public void Start()
        {
            _unloadDelay = System.Diagnostics.Stopwatch.Frequency * OnDemandStorage.OnDemandUnloadDelay;
            scaledRenderer = GetComponent<MeshRenderer>();
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);

            if (Entries is null)
                return;

#pragma warning disable CS0618 // Type or member is obsolete
            // Initialize texture and normals for back-compat.
            foreach (var entry in Entries)
            {
                if (entry.Key == "_MainTex")
                    texture = entry.Path;
                else if (entry.Key == "_BumpMap")
                    normals = entry.Path;
            }
#pragma warning restore CS0618 // Type or member is obsolete
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
            Debug.Log($"[OD] --> ScaledSpaceDemand.LoadTextures loading {name}");

            using var guard = new ClearEnumeratorGuard(this);

            // Clear out any existing handles. KSPTL doesn't unload things immediately
            // so if we're reloading the same textures then this will be a no-op.
            using (new ClearListGuard<TextureHandle>(Handles))
            {
                foreach (var handle in Handles)
                    handle.Dispose();
            }

            var shader = scaledRenderer.sharedMaterial.shader;
            foreach (var entry in Entries)
            {
                var handle = OnDemandStorage.LoadPropertyTexture(shader, entry.Key, entry.Path);
                if (!handle.IsComplete)
                    handle.OnCompleted += OnTextureLoadComplete;
                Handles.Add(handle);
            }

            MaterialPropertyBlock block = new();
            scaledRenderer.GetPropertyBlock(block);

            for (int i = 0; i < Handles.Count; ++i)
            {
                var handle = Handles[i];
                var entry = Entries[i];

                if (!handle.IsComplete)
                {
                    scaledRenderer.SetPropertyBlock(block);
                    yield return handle;
                }

                try
                {
                    block.SetTexture(entry.Id, handle.GetTexture());
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[OD] Failed to load texture {handle.Path}");
                    Debug.LogException(ex);
                }
            }

            scaledRenderer.SetPropertyBlock(block);

            // Events
            Events.OnScaledSpaceLoad.Fire(this);

            // Flags
            isLoaded = true;
        }

        public void UnloadTextures()
        {
            if (isLoaded)
                Debug.Log($"[OD] <--- ScaledSpaceDemand.UnloadTextures unloading {name}");

            using (new ClearListGuard<TextureHandle>(Handles))
            {
                foreach (var handle in Handles)
                {
                    Debug.Log($"[OD] Unloading {handle.Path}");
                    handle.Dispose();
                }
            }

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
            using var guard = new ClearListGuard<TextureHandle>(Handles);

            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);

            // Ensure textures get cleaned up if not done already
            foreach (var handle in Handles)
                handle.Dispose();
        }

        private static void OnTextureLoadComplete(TextureHandle handle)
        {
            Debug.Log($"[OD] Loaded texture {handle.Path}");
        }

        #region Serialization Callbacks
        // Unity's serializer won't serialize OnDemandTextureEntry so we need
        // to unpack it into something that it will serialize.
        [SerializeField]
        private List<string> entryKeys = [];
        [SerializeField]
        private List<string> entryPaths = [];

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            entryKeys.Clear();
            entryPaths.Clear();
            if (Entries is null)
                return;
            foreach (var entry in Entries)
            {
                entryKeys.Add(entry.Key);
                entryPaths.Add(entry.Path);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            var count = entryKeys?.Count ?? 0;
            Entries = new List<OnDemandTextureEntry>(count);
            for (int i = 0; i < count; ++i)
                Entries.Add(new OnDemandTextureEntry(entryKeys[i], entryPaths[i]));
        }
        #endregion

        struct ClearEnumeratorGuard(ScaledSpaceOnDemand parent) : IDisposable
        {
            public void Dispose() => parent.loaderCoroutine = null;
        }

        struct ClearListGuard<T>(List<T> list) : IDisposable
        {
            public void Dispose() => list.Clear();
        }
    }
}
