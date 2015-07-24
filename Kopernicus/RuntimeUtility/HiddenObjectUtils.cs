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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kopernicus
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class HiddenObjectUtils : MonoBehaviour
    {
        MapObject previousMap = null;
        public static List<string> additions = new List<string>();

        public void Awake()
        {
            DontDestroyOnLoad(this);
            previousMap = PlanetariumCamera.fetch.initialTarget;
            GameEvents.onPlanetariumTargetChanged.Add(new EventData<MapObject>.OnEvent(onPlanetariumTargetChanged));
        }

        public void OnDestroy()
        {
            GameEvents.onPlanetariumTargetChanged.Remove(new EventData<MapObject>.OnEvent(onPlanetariumTargetChanged));
        }

        // Barycenter-Utils
        public void onPlanetariumTargetChanged(MapObject map)
        {
            // If we switched to a barycenter..
            if (map != null && previousMap != null)
            {
                if (Templates.barycenters.Contains(map.GetName()) || additions.Contains(map.GetName()))
                {
                    // Don't center the barycenter
                    List<MapObject> objects = PlanetariumCamera.fetch.targets;
                    int nextIndex = objects.IndexOf(previousMap) < objects.IndexOf(map) ? (objects.IndexOf(map) + 1) % objects.Count : objects.IndexOf(map) - 1 + (objects.IndexOf(map) - 1 >= 0 ? 0 : objects.Count);
                    PlanetariumCamera.fetch.SetTarget(objects[nextIndex]);
                    previousMap = objects[nextIndex];
                }
            }
        }

        // Helper class to deactivate the thumbnail in R&D
        public class RDBaryCenter : MonoBehaviour
        {
            public void Start()
            {
                // Loop through all barycenters
                foreach (string name in Templates.barycenters)
                {
                    // If we can find an Object, deactivate it
                    foreach (RDPlanetListItemContainer planetItem in Resources.FindObjectsOfTypeAll<RDPlanetListItemContainer>().Where(item => item.name == name))
                    {
                        planetItem.planet.SetActive(false);
                        planetItem.label_planetName.anchor = SpriteText.Anchor_Pos.Middle_Center;
                    }
                }

                // Process the additions
                foreach (string name in additions)
                {
                    // If we can find an Object, deactivate it
                    foreach (RDPlanetListItemContainer planetItem in Resources.FindObjectsOfTypeAll<RDPlanetListItemContainer>().Where(item => item.name == name))
                        planetItem.Hide();
                }
            }
        }
    }
}
