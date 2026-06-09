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
        // Ordering matters: PreloadMapSOs is a no-op once we are >= Preloaded, so
        // AutoLoaded deliberately sorts below it.
        //   AutoLoaded - background GetSurfaceHeight on an inactive body; nothing loaded
        //                eagerly (MapSODemand decodes maps lazily) + unload timer armed.
        //   Preloaded  - all maps staged (Preload, no decode); distinct from Loaded.
        enum MapLoadState
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
        private MapLoadState mapLoadState;
        private Coroutine unloadMapSOCoroutine;

        // If non-zero the MapSOs will be unloaded once the timer exceeds the value
        private long _unloadTime;
        private int _unloadFrame;

        private int _lastUpdateFrame = 0;

        // Map view calls DeactivateSphere (firing OnSphereInactive) but the sphere is
        // needed again the instant the map closes, so treat it as still in use to avoid
        // us unloading texture that will be needed the instant it loads back.
        private bool SurfaceInUse =>
            sphere.IsNotNullOrDestroyed() && (sphere.isActive || MapView.MapIsEnabled);

        void StopUnloadWatcher()
        {
            if (unloadMapSOCoroutine != null)
            {
                StopCoroutine(unloadMapSOCoroutine);
                unloadMapSOCoroutine = null;
            }
        }

        void PreloadMapSOs()
        {
            if (mapSOs.Count == 0 || mapLoadState >= MapLoadState.Preloaded)
                return;

            StopUnloadWatcher();

            mapLoadState = MapLoadState.Preloaded;
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
            if (mapSOs.Count == 0 || mapLoadState == MapLoadState.Loaded)
                return;

            StopUnloadWatcher();

            Debug.Log($"[OD] Loading {sphere.name}");

            mapLoadState = MapLoadState.Loaded;
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
            if (mapSOs.Count == 0 || mapLoadState == MapLoadState.Unloaded)
                return;

            // No StopUnloadWatcher(): this runs inside the watcher, which nulls
            // unloadMapSOCoroutine itself via MapSOCoroutineGuard.
            Debug.Log($"[OD] Unloading {sphere.name}");

            mapLoadState = MapLoadState.Unloaded;
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
            if (mapSOs.Count == 0)
                return;

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

                // If the sphere is active then we aren't needed. A future OnSphereInactive
                // call will start a new UnloadMapWatcherInstance so there's no need for us
                // to spin every frame.
                if (sphere.IsNotNullOrDestroyed() && sphere.isActive)
                    yield break;

                // OnSphereInactive happens when switching to the map view, even if we are
                // currently landed on the surface. Obviously we don't want to unload textures
                // in this case so keep them loaded until the user switches out of map view,
                // at which point we can recheck.
                if (MapView.MapIsEnabled)
                    continue;

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

            // Hold while the surface is in use so map view doesn't thrash textures.
            do
            {
                yield return UnloadDelay;
            }
            while (Time.frameCount < minEndFrame || SurfaceInUse);

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
            UpdateUnloadTime();
            StartTextureUnload();
        }

        public override void OnSphereReset()
        {
            StartMapSOUnload();
            UpdateUnloadTime();
            StartTextureUnload();
        }

        // Enabling
        public override void OnQuadPreBuild(PQ quad)
        {
            if (mapLoadState != MapLoadState.Unloaded)
                LoadMapSOs();

            UpdateUnloadTime();
        }

        public override void OnVertexBuildHeight(PQS.VertexBuildData data) =>
            Activate();
        #endregion

        internal void Activate()
        {
            if (mapSOs.Count == 0)
                return;

            if (mapLoadState != MapLoadState.Unloaded)
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
                mapLoadState = MapLoadState.AutoLoaded;
            }

            UpdateUnloadTime();
        }

        #region Serialization Callbacks
#pragma warning disable IDE0044 // Add readonly modifier
        // Unity's serializer doesn't handle the TextureListener type because
        // it is not defined at startup time. We unpack these ourselves so
        // that we can survive across serialization callbacks.
        [SerializeField]
        private List<string> listenerProperties = [];
        [SerializeField]
        private List<string> listenerPaths = [];
        [SerializeField]
        private List<UnityEngine.Object> listenerObjects = [];
#pragma warning restore IDE0044 // Add readonly modifier

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
