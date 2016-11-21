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

using System;
using System.Collections.Generic;
using Kopernicus.Configuration;
using UnityEngine;

namespace Kopernicus
{
    // Globally used values
    public class Templates
    {
        // The reference mesh for ScaledSpace (Jools Mesh)
        public static Mesh ReferenceGeosphere { get; set; }

        // Finalize Orbits stuff
        public static double SOIMinRadiusMult = 2.0d;
        public static double SOIMinAltitude = 40000d;

        // Max view distance
        public static double maxViewDistance = -1d;

        // Use Kopernicus Time
        public static bool useKopernicusTime = false;

        // Use Custom Clock
        public static bool customClock = false;

        // Global base epoch
        public static double epoch { get; set; }

        // The body that should appear in MainMenu
        public static string menuBody { get; set; }

        // Initialisation
        static Templates()
        {
            // We need to get the body for Jool (to steal it's mesh)
            PSystemBody Jool = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, "Jool");

            // Return it's mesh
            ReferenceGeosphere = Jool.scaledVersion.GetComponent<MeshFilter>().sharedMesh;

            // Main Menu body
            menuBody = "Kerbin";
        }
    }
}
