/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
* Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using UnityEngine;

namespace Kopernicus
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class MapViewFixer : MonoBehaviour
    {
        static float  max3DlineDrawDist = 20000f;

        public void Start()
        {
            if (HighLogic.LoadedSceneHasPlanetarium && MapView.fetch != null)
            {
                try
                {
                    MapView.fetch.max3DlineDrawDist = max3DlineDrawDist;
                }
                catch (Exception e)
                {
                    Debug.Log("[Kopernicus]: MapView fixing failed: " + e.Message);
                }
            }
        }

        private bool isDone = false;

        // Fix the Zooming-Out bug
        public void LateUpdate()
        {
            if (HighLogic.LoadedSceneHasPlanetarium && MapView.fetch != null && !isDone)
            {
                // Fix the bug via switching away from Home and back immideatly. 
                PlanetariumCamera.fetch.SetTarget(PlanetariumCamera.fetch.targets[(PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) + 1) % PlanetariumCamera.fetch.targets.Count]);
                PlanetariumCamera.fetch.SetTarget(PlanetariumCamera.fetch.targets[(PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) - 1) + (((PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) - 1) >= 0) ? 0 : PlanetariumCamera.fetch.targets.Count)]);
                
                // Terminate for the moment.
                isDone = true;
            }
        }
    }
}