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
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Kopernicus.OnDemand
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PQSMod_OnDemandHandler : PQSMod
    {
        // A minimum frame delay so that we don't unload if there is a large lag spike.
        const int UnloadFrameDelay = 30;
        const int UpdateInterval = 5;

        static readonly WaitForEndOfFrame WaitForEndOfFrame = new();

        // Delayed unload
        // If non-zero the textures will be unloaded once the timer exceeds the value
        private long _unloadTime;
        private int _unloadFrame;

        private int _lastUpdateFrame = 0;

        // State
        private Coroutine _coroutine;

        private bool IsLoaded => _coroutine is not null;

        long GetUnloadTime()
        {
            if (sphere.isActive)
                return 0;

            return Stopwatch.GetTimestamp() +
                   Stopwatch.Frequency * OnDemandStorage.OnDemandUnloadDelay;
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

        internal void Activate()
        {
            if (IsLoaded)
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

                _coroutine = StartCoroutine(UnloadWatcher());
            }

            UpdateUnloadTime();
        }

        // Enabling
        public override void OnSphereActive()
        {
            Activate();

            // Enable the maps
            if (!OnDemandStorage.EnableBodyPqs(sphere.name))
                return;

            Debug.Log($"[OD] Enabling Body {sphere.name}");
        }

        public override void OnQuadPreBuild(PQ quad) => Activate();

        public override void OnVertexBuildHeight(PQS.VertexBuildData data)
        {
            // Linx's texture exporter calls OnVertexBuildHeight on multiple
            // and on invalid objects. We can't really handle that correctly,
            // but what we can do is avoid throwing an exception in that case.
            //
            // It's already known not to be compatible with OnDemand, so this
            // should make things (maybe?) work in the remaining cases.
            if (this.IsDestroyed() || sphere.IsDestroyed())
                return;

            Activate();
        }

        IEnumerator UnloadWatcher()
        {
            using var guard = new ClearCoroutineGuard(this);

            while (true)
            {
                yield return WaitForEndOfFrame;

                // We will never unload ourselves while the PQS sphere is loaded.
                if (sphere.isActive)
                {
                    // Make sure we periodically update the stats so that we don't
                    // immediately unload the textures when the sphere goes inactive.
                    if (_lastUpdateFrame + UpdateInterval < Time.frameCount)
                        UpdateUnloadTime();

                    continue;
                }

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

            if (OnDemandStorage.DisableBodyPqs(sphere.name))
                Debug.Log($"[OD] Disabling Body {sphere.name}");
        }

        struct ClearCoroutineGuard(PQSMod_OnDemandHandler handler) : IDisposable
        {
            public void Dispose() => handler._coroutine = null;
        }
    }
}
