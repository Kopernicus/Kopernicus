/**
 * Kopernicus Planetary System Modifier
 * Copyright (C) 2014 Bryce C Schroeder (bryce.schroeder@gmail.com), Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * http://www.ferazelhosting.net/~bryce/contact.html
 * 
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
    // Temporary generator for the custom solar system.  Will be replaced by a more complex system with individual planet definitions, existing planet
    // modifications (add biomes n' such), etc.
    public class KopernicusSystemSource 
    {
        // System name
        public const String systemName = "Kopernican System";

        // This function generates a duplicate of a PSystem
        public static PSystem DuplicateSystem(PSystem originalSystem)
        {	
            // Create a new planetary system object
            GameObject gameObject = new GameObject (originalSystem.systemName);
            UnityEngine.Object.DontDestroyOnLoad (gameObject);
            PSystem system = gameObject.AddComponent<PSystem> ();
            
            // Set the planetary system defaults (pulled from PSystemManager.Instance.systemPrefab)
            system.systemName          = originalSystem.systemName;
            system.systemTimeScale     = 1.0; 
            system.systemScale         = 1.0;
            system.mainToolbarSelected = 2;   // initial value in stock systemPrefab. Unknown significance.

            // Clone the root body of the system (we could set it up from scratch, but we do need the pqsVersion entries, which this DOES NOT copy
            GameObject rootBody = (GameObject) UnityEngine.Object.Instantiate (originalSystem.rootBody.gameObject);
            UnityEngine.Object.DontDestroyOnLoad (rootBody);
            system.rootBody = rootBody.GetComponent<PSystemBody> ();

            // Return the new system
            return system;
        }

        // This function returns a PSystem that will replace the stock systemPrefab
        // with one of the modder's design. KSP then loads the replacement planetary
        // system just as it would have loaded the stock system.
        public static PSystem GenerateSystem() 
        {
            // If the planetary manager does not work, well, error out
            if (PSystemManager.Instance == null) 
            {
                // Log the error
                Debug.LogError("[Kopernicus]: KopernicusSystemSource.GenerateSystem() can not be called if PSystemManager.Instance is null");
                return null;
            }
            
            // Setup the template solar system object from the original game
            PSystem system = DuplicateSystem (PSystemManager.Instance.systemPrefab);

            // Create "Kopernicus"
            // Note that due to the way AddBody works, this is a function with side effects
            // rather than something that returns a planet. Perhaps it should be named differently
            // from the GenerateSystem method to emphasize this difference in usage??
            KopernicusPlanetSource.GeneratePlanet (system, "Kopernicus");
            //StockPlanetSource.GeneratePlanet(system, "Laythe", "Bob", new Orbit (0.0, 0.0, 47500000000, 0, 0, 0, 0, system.rootBody.celestialBody));

            // Return the newly created planetary system
            return system;
        }
    }
}

