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
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Kopernicus.OnDemand
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PQSMod_OnDemandHandler : PQSMod
    {
        // Delayed unload
        // If non-zero the textures will be unloaded once the timer exceeds the value
        private long _unloadTime;

        private int _lastCheckFrame = 0;

        // State
        private bool _isLoaded;

        private static CelestialBody activeBody;

        static long GetUnloadTime()
        {
            return Stopwatch.GetTimestamp() +
                   Stopwatch.Frequency * OnDemandStorage.OnDemandUnloadDelay;
        }

        internal void Activate()
        {
            if (_isLoaded)
            {
                if (_unloadTime == 0)
                    return;

                // Refresh unload time at most once every 5 frames per OnDemandHandler.
                if (_lastCheckFrame + 5 < Time.frameCount)
                    _unloadTime = GetUnloadTime();

                return;
            }

            // Occasionally KSP or other mods will call GetSurfaceHeight on an
            // inactive celestial body. This starts a timer so any MapSOs loaded
            // as a result of that will get unloaded eventually instead of
            // hanging around forever.

            _isLoaded = true;
            _unloadTime = GetUnloadTime();
            _lastCheckFrame = Time.frameCount;
            Debug.Log($"[OD] Enabling Body {sphere.name}");
        }

        // Disabling
        public override void OnSphereInactive()
        {
            // Don't update, if the Injector is still running
            if (!_isLoaded)
            {
                return;
            }

            // Set the time at which to unload
            _unloadTime = GetUnloadTime();
        }

        // Enabling
        public override void OnQuadPreBuild(PQ quad)
        {
            // It is supposed to be loaded now so clear the unload time
            _unloadTime = 0;

            // Don't update, if the Injector is still running
            if (_isLoaded)
                return;

            // Enable the maps
            if (!OnDemandStorage.EnableBodyPqs(sphere.name))
                return;

            _isLoaded = true;
            Debug.Log($"[OD] Enabling Body {sphere.name}");
        }

        public override void OnVertexBuildHeight(PQS.VertexBuildData data) =>
            Activate();

        private void LateUpdate()
        {
            // If we are in flight with a vessel, update the cached active body.
            if (FlightGlobals.ActiveVessel.IsNotNullOrDestroyed())
            {
                activeBody = FlightGlobals.ActiveVessel.mainBody;
            }

            // If we are the currently active body, do not unload.
            if (sphere.IsNotNullOrDestroyed())
            {
                if (activeBody.RefEquals(sphere.GetCelestialBody()))
                    return;
            }

            // If we aren't loaded or we're not wanting to unload then do nothing
            if (!_isLoaded || _unloadTime == 0)
                return;

            // If we're past the unload time then unload
            if (Stopwatch.GetTimestamp() <= _unloadTime)
                return;

            // Disable the maps
            if (OnDemandStorage.DisableBodyPqs(sphere.name))
                Debug.Log($"[OD] Disabling Body {sphere.name}");

            _isLoaded = false;
        }
    }
}
