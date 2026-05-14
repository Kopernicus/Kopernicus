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
using System.Diagnostics.CodeAnalysis;
using KSPTextureLoader;
using UnityEngine;
using Object = UnityEngine.Object;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Kopernicus.OnDemand
{
    public class PQSMod_OnDemandHandler : PQSMod, ISerializationCallbackReceiver
    {
        enum State
        {
            Unloaded,
            AutoLoaded,
            Preloaded,
            Loaded
        }

        struct TextureListener(string property, string path, IOnDemandTextureListener listener)
        {
            public readonly string property = property;
            public readonly string path = path;
            public readonly IOnDemandTextureListener listener = listener;
        }

        // A minimum frame delay so that we don't unload if there is a large lag spike.
        const int UnloadFrameDelay = 30;
        const int UpdateInterval = 5;

        static readonly WaitForEndOfFrame WaitForEndOfFrame = new();
        static readonly WaitForSecondsRealtime UnloadDelay = new(60);


        #region PQS
        // PQS and on-demand MapSO data
        private List<ILoadOnDemand> mapSOs = [];
        private State mapSOState;
        private Coroutine unloadMapSOCoroutine;

        // If non-zero the MapSOs will be unloaded once the timer exceeds the value
        private long _unloadTime;
        private int _unloadFrame;

        private int _lastUpdateFrame = 0;

        void PreloadMapSOs()
        {
            if (mapSOState >= State.Preloaded)
                return;

            if (unloadMapSOCoroutine != null)
            {
                StopCoroutine(unloadMapSOCoroutine);
                unloadMapSOCoroutine = null;
            }

            mapSOState = State.Preloaded;
            foreach (var entry in mapSOs)
            {
                if (entry is not IPreloadOnDemand preload)
                    continue;

                try
                {
                    preload.Preload();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        void LoadMapSOs()
        {
            if (mapSOState == State.Loaded)
                return;

            if (unloadMapSOCoroutine != null)
            {
                StopCoroutine(unloadMapSOCoroutine);
                unloadMapSOCoroutine = null;
            }

            Debug.Log($"[OD] Loading {sphere.name}");

            mapSOState = State.Loaded;
            foreach (var entry in mapSOs)
            {
                try
                {
                    entry.Load();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        void UnloadMapSOs()
        {
            if (mapSOState == State.Unloaded)
                return;

            Debug.Log($"[OD] Unloading {sphere.GetCelestialBody().bodyName}");

            mapSOState = State.Unloaded;
            foreach (var entry in mapSOs)
            {
                try
                {
                    entry.Unload();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        void StartMapSOUnload()
        {
            if (unloadMapSOCoroutine is not null)
                return;

            unloadMapSOCoroutine = StartCoroutine(UnloadMapWatcher());
        }

        IEnumerator UnloadMapWatcher()
        {
            using var guard = new MapSOCoroutineGuard(this);

            while (true)
            {
                yield return WaitForEndOfFrame;

                // If we are the currently active body, do not unload.
                if (sphere.IsNotNullOrDestroyed() && FlightGlobals.ActiveVessel.IsNotNullOrDestroyed())
                {
                    var activeBody = FlightGlobals.ActiveVessel.mainBody;
                    if (activeBody.RefEquals(sphere.GetCelestialBody()))
                        continue;
                }

                // We stay loadeed for at least UnloadFrameDelay frames, so that
                // lag spikes do not result in unloading things unexpectedly.
                if (Time.frameCount <= _unloadFrame)
                    continue;

                // Otherwise we need to wait at least OnDemandUnloadDelay before
                // we unload anything.
                if (Stopwatch.GetTimestamp() <= _unloadTime)
                    continue;

                break;
            }

            UnloadMapSOs();
        }

        void UpdateUnloadTime()
        {
            if (sphere.isActive)
            {
                _unloadTime = 0;
                _unloadFrame = 0;
            }
            else
            {
                _unloadTime = Stopwatch.GetTimestamp() +
                              Stopwatch.Frequency * OnDemandStorage.OnDemandUnloadDelay;
                _unloadFrame = Time.frameCount + UnloadFrameDelay;
            }

            _lastUpdateFrame = Time.frameCount;
        }

        struct MapSOCoroutineGuard(PQSMod_OnDemandHandler mod) : IDisposable
        {
            public void Dispose() => mod.unloadMapSOCoroutine = null;
        }
        #endregion

        #region Materials
        // Material texture data
        private readonly List<TextureListener> listeners = [];
        private readonly List<TextureHandle> handles = [];
        private Coroutine loadCoroutine;
        private Coroutine unloadCoroutine;

        private bool TexturesLoaded => handles.Count != 0;

        /// <summary>
        /// Add a new listener that gets notified when the texture load completes.
        /// </summary>
        public void AddTextureListener<T>(string property, string path, T listener)
            where T : UnityEngine.Object, IOnDemandTextureListener
        {
            listeners.Add(new(property, path, listener));
        }

        void StartTextureLoad()
        {
            // Cancel any pending unload — keep what's already loaded.
            if (unloadCoroutine != null)
            {
                StopCoroutine(unloadCoroutine);
                unloadCoroutine = null;
            }

            // Cancel any in-flight load coroutine. Existing handles are kept
            // (they may already be partially or fully loaded); we'll restart
            // the callback coroutine after topping up handles for any
            // listeners added since the existing handles were created.
            if (loadCoroutine != null)
            {
                StopCoroutine(loadCoroutine);
                loadCoroutine = null;
            }

            for (int i = handles.Count; i < listeners.Count; ++i)
            {
                var entry = listeners[i];
                handles.Add(OnDemandStorage.LoadPropertyTexture(
                    entry.listener.Shader,
                    entry.property,
                    entry.path
                ));
            }

            if (handles.Count == 0)
                return;

            loadCoroutine = StartCoroutine(DoTextureCallbacks());
        }

        void StartTextureUnload()
        {
            if (!TexturesLoaded)
                return;

            if (unloadCoroutine != null)
                return;

            if (loadCoroutine != null)
            {
                StopCoroutine(loadCoroutine);
                loadCoroutine = null;
            }

            unloadCoroutine = StartCoroutine(DoUnloadTextures());
        }

        void UnloadTextures()
        {
            if (loadCoroutine != null)
            {
                StopCoroutine(loadCoroutine);
                loadCoroutine = null;
            }
            if (unloadCoroutine != null)
            {
                StopCoroutine(unloadCoroutine);
                unloadCoroutine = null;
            }

            if (!TexturesLoaded)
                return;

            foreach (var handle in handles)
                handle?.Dispose();
            handles.Clear();
        }

        IEnumerator DoTextureCallbacks()
        {
            using var guard = new LoadCoroutineGuard(this);

            bool immediate = false;
            foreach (var entry in listeners)
            {
                if (!entry.listener.Immediate)
                    continue;

                immediate = true;
                break;
            }

            for (int i = 0; i < handles.Count; ++i)
            {
                var entry = listeners[i];
                var handle = handles[i];

                if (!immediate && !handle.IsComplete)
                    yield return handle;

                try
                {
                    var texture = handle.GetTexture();
                    entry.listener.OnTextureLoaded(entry.property, texture);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[OD] Failed to load texture {handle.Path}");
                    Debug.LogException(ex);
                }
            }
        }

        IEnumerator DoUnloadTextures()
        {
            using var guard = new UnloadCoroutineGuard(this);
            var minEndFrame = Time.frameCount + UnloadFrameDelay;

            do
            {
                yield return UnloadDelay;
            }
            while (Time.frameCount < minEndFrame);

            UnloadTextures();
        }


        struct LoadCoroutineGuard(PQSMod_OnDemandHandler mod) : IDisposable
        {
            public void Dispose() => mod.loadCoroutine = null;
        }

        struct UnloadCoroutineGuard(PQSMod_OnDemandHandler mod) : IDisposable
        {
            public void Dispose() => mod.unloadCoroutine = null;
        }
        #endregion

        #region Events
        public override void OnSetup()
        {
            mapSOs = OnDemandStorage.GetMaps(sphere.name);
        }

        void OnEnable()
        {
            Events.OnPQSSphereStartedPreInit.Add(OnSphereStartedPreInit);
        }

        void OnDisable()
        {
            UnloadMapSOs();
            UnloadTextures();
            Events.OnPQSSphereStartedPreInit.Remove(OnSphereStartedPreInit);
        }

        void OnSphereStartedPreInit(PQS pqs)
        {
            if (pqs != sphere)
                return;

            PreloadMapSOs();
            StartTextureLoad();
        }

        public override void OnSphereActive()
        {
            PreloadMapSOs();
            StartTextureLoad();
        }

        public override void OnSphereInactive()
        {
            StartMapSOUnload();
            StartTextureUnload();
        }

        public override void OnSphereReset()
        {
            StartMapSOUnload();
            StartTextureUnload();
        }

        // Enabling
        public override void OnQuadPreBuild(PQ quad)
        {
            if (mapSOState != State.Unloaded)
                LoadMapSOs();

            UpdateUnloadTime();
        }

        public override void OnVertexBuildHeight(PQS.VertexBuildData data) =>
            Activate();
        #endregion

        internal void Activate()
        {
            if (mapSOState != State.Unloaded)
            {
                // Refresh unload time at most once every 5 frames per OnDemandHandler.
                if (_lastUpdateFrame + UpdateInterval >= Time.frameCount)
                    return;
            }
            else
            {
                // Occasionally KSP or other mods will call GetSurfaceHeight on an
                // inactive celestial body. This starts a timer so any MapSOs loaded
                // as a result of that will get unloaded eventually instead of
                // hanging around forever.

                StartMapSOUnload();
                mapSOState = State.AutoLoaded;
            }

            UpdateUnloadTime();
        }

        #region Serialization Callbacks
        // Unity's serializer (used by the prefab→live Instantiate clone in
        // PSystemSpawn) only handles primitives, strings, UnityEngine.Object
        // references, and List<T> of those. Mod-DLL [Serializable] structs are
        // silently dropped, and IOnDemandTextureListener is a plain managed
        // interface so its references would be lost on the cloned component.
        // Round trip the subset of listeners whose backing object derives from
        // UnityEngine.Object through these parallel SerializeField lists.
        [SerializeField]
        private readonly List<string> listenerProperties = [];
        [SerializeField]
        private readonly List<string> listenerPaths = [];
        [SerializeField]
        private readonly List<UnityEngine.Object> listenerObjects = [];

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            listenerProperties.Clear();
            listenerPaths.Clear();
            listenerObjects.Clear();

            foreach (var entry in listeners)
            {
                if (entry.listener is not Object obj)
                    continue;

                listenerProperties.Add(entry.property);
                listenerPaths.Add(entry.path);
                listenerObjects.Add(obj);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            listeners.Clear();

            var count = listenerObjects?.Count ?? 0;
            for (int i = 0; i < count; ++i)
            {
                if (listenerObjects[i] is not IOnDemandTextureListener listener)
                    continue;

                listeners.Add(new TextureListener(
                    listenerProperties[i],
                    listenerPaths[i],
                    listener
                ));
            }
        }
        #endregion
    }
}
