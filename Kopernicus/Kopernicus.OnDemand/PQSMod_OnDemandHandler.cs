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

namespace Kopernicus
{
    namespace OnDemand
    {
        public class PQSMod_OnDemandHandler : PQSMod
        {
            // Delayed unload
            private long unloadTime;                // If non-zero the textures will be unloaded once the timer exceeds the value

            // State
            private bool isLoaded;

            // Disabling
            public override void OnSphereInactive()
            {
                // Don't update, if the Injector is still running
                if (!isLoaded)
                    return;

                // Set the time at which to unload
                unloadTime = System.Diagnostics.Stopwatch.GetTimestamp() + 
                    System.Diagnostics.Stopwatch.Frequency * OnDemandStorage.onDemandUnloadDelay;
            }

            // Enabling
            public override void OnQuadPreBuild(PQ quad)
            {
                // It is supposed to be loaded now so clear the unload time
                unloadTime = 0;

                // Don't update, if the Injector is still running
                if (isLoaded)
                    return;

                // Enable the maps
                if (OnDemandStorage.EnableBody(sphere.name))
                {
                    isLoaded = true;
                    Debug.Log("[OD] Enabling Body " + base.sphere.name + ": " + isLoaded);
                }
            }

            void LateUpdate()
            {
                // If we aren't loaded or we're not wanting to unload then do nothing
                if (!isLoaded || unloadTime == 0)
                    return;

                // If we're past the unload time then unload
                if (System.Diagnostics.Stopwatch.GetTimestamp() > unloadTime)
                {
                    // Disable the maps
                    if (OnDemandStorage.DisableBody(sphere.name))
                    {
                        Debug.Log("[OD] Disabling Body " + base.sphere.name + ": " + isLoaded);
                    }
                    isLoaded = false;
                }
            }
        }
    }
}