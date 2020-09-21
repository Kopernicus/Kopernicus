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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using KSP.Localization;
using UnityEngine;

namespace Kopernicus
{
    // Globally used values
    public static class Templates
    {
        // The reference mesh for ScaledSpace (Jools Mesh)
        public static readonly Mesh ReferenceGeosphere;
        public static readonly PSystemBody ReferenceEelooPSB;

        // Finalize Orbits stuff
        public const Double SOI_MIN_RADIUS_MULTIPLIER = 2.0d;
        public const Double SOI_MIN_ALTITUDE = 40000d;

        // Max view distance
        public static Double MaxViewDistance = -1d;

        // Global base epoch
        public static Double Epoch;

        // Whether the main menu should be edited by Kopernicus
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")] 
        [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
        public static Boolean KopernicusMainMenu = true;

        // The body that should appear in MainMenu
        public static String MenuBody;

        // Whether the main menu body should be randomized
        public static readonly List<String> RandomMainMenuBodies;
        
        // The localized names of the presets
        public static readonly List<String> PresetDisplayNames;
        
        // The launch sites that should get removed
        public static List<String> RemoveLaunchSites;

        // Whether to force 3D rendering on orbits.
        public static Boolean Force3DOrbits;
        
        // A backup of all targets available in MapView
        public static List<MapObject> MapTargets;
        
        // Initialisation
        static Templates()
        {
            // We need to get the body for Jool (to steal it's mesh)
            PSystemBody jool = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Jool");

            // Return it's mesh
            ReferenceGeosphere = jool.scaledVersion.GetComponent<MeshFilter>().sharedMesh;

            // Main Menu body
            MenuBody = "Kerbin";
            
            // Random Main Menu bodies
            RandomMainMenuBodies = new List<String>();
            
            // Presets
            PresetDisplayNames = new List<String>
            {
                Localizer.Format("#autoLOC_6001506"),
                Localizer.Format("#autoLOC_6001507"),
                Localizer.Format("#autoLOC_6001508")
            };
            
            // Launch Sites
            RemoveLaunchSites = new List<String>();
        }
    }
}
