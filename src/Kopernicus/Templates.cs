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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kopernicus
{
    // Globally used values
    public class Templates
    {
        // The reference mesh for ScaledSpace (Jools Mesh)
        public static Mesh ReferenceGeosphere { get; set; }

        // Finalize Orbits stuff
        public static Double SOIMinRadiusMult = 2.0d;
        public static Double SOIMinAltitude = 40000d;

        // Max view distance
        public static Double maxViewDistance = -1d;

        // Global base epoch
        public static Double epoch { get; set; }

        // Whether the main menu should be edited by Kopernicus
        public static Boolean kopernicusMainMenu = true;

        // The body that should appear in MainMenu
        public static String menuBody { get; set; }

        // Whether the main menu body should be randomized
        public static List<String> randomMainMenuBodies { get; set; }

        // Initialisation
        static Templates()
        {
            // We need to get the body for Jool (to steal it's mesh)
            PSystemBody Jool = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Jool");

            // Return it's mesh
            ReferenceGeosphere = Jool.scaledVersion.GetComponent<MeshFilter>().sharedMesh;

            // Main Menu body
            menuBody = "Kerbin";
            
            // Random Main Menu bodies
            randomMainMenuBodies = new List<String>();
        }
    }
}
