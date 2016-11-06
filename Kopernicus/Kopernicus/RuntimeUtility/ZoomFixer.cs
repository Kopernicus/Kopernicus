/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using UnityEngine;


namespace Kopernicus
{
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class TrackingStationZoomFix : MonoBehaviour
    {
        void Update()
        {
            ZoomFixer.zoomFix();
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class MapModeZoomFix : MonoBehaviour
    {
        void Update()
        {
            ZoomFixer.zoomFix();
        }
    }

    public class ZoomFixer : MonoBehaviour
    {
        public static void zoomFix()
        {
            MapObject target = PlanetariumCamera.fetch.target;
            if (target != null && target.celestialBody != null)
            {
                string name = target.celestialBody.transform.name;
                if (Templates.maxZoom.ContainsKey(name))
                {
                    if (Templates.maxZoom[name] != PlanetariumCamera.fetch.minDistance)
                        PlanetariumCamera.fetch.minDistance = Templates.maxZoom[name];
                }
                else if (PlanetariumCamera.fetch.minDistance != 10)
                    PlanetariumCamera.fetch.minDistance = 10;
            }
        }
    }
}
