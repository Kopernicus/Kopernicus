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

namespace Kopernicus.OnDemand
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PQSMod_OnDemandHandler : PQSMod
    {
        // Delayed unload
        // If non-zero the textures will be unloaded once the timer exceeds the value
        private Int64 _unloadTime;

        // State
        private Boolean _isLoaded;

        private static CelestialBody activeBody;

        // Disabling
        public override void OnSphereInactive()
        {
            // Don't update, if the Injector is still running
            if (!_isLoaded)
            {
                return;
            }

            // Set the time at which to unload
            _unloadTime = System.Diagnostics.Stopwatch.GetTimestamp() +
                          System.Diagnostics.Stopwatch.Frequency * OnDemandStorage.OnDemandUnloadDelay;
        }

        // Enabling
        public override void OnQuadPreBuild(PQ quad)
        {
            // It is supposed to be loaded now so clear the unload time
            _unloadTime = 0;

            // Don't update, if the Injector is still running
            if (_isLoaded)
            {
                return;
            }

            // Enable the maps
            if (!OnDemandStorage.EnableBodyPqs(sphere.name))
            {
                return;
            }

            _isLoaded = true;
            Debug.Log("[OD] Enabling Body " + sphere.name + ": " + _isLoaded);
        }

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
                {
                    return;
                }
            }

            // If we aren't loaded or we're not wanting to unload then do nothing
            if (!_isLoaded || _unloadTime == 0)
            {
                return;
            }

            // If we're past the unload time then unload
            if (System.Diagnostics.Stopwatch.GetTimestamp() <= _unloadTime)
            {
                return;
            }

            // Disable the maps
            if (OnDemandStorage.DisableBodyPqs(sphere.name))
            {
                Debug.Log("[OD] Disabling Body " + sphere.name + ": " + _isLoaded);
            }

            _isLoaded = false;
        }
    }
}
