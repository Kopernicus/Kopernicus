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
        public static double SOIMinRadiusMult /*{ get; set; }*/ = 2.0d;
        public static double SOIMinAltitude /*{ get; set; }*/ = 40000d;

        // Max view distance
        public static double maxViewDistance = -1d;

        // Global base epoch
        public static double epoch { get; set; }

        // Bodies with finalized Orbits
        public static List<string> finalizeBodies { get; set; }

        // The body that should appear in MainMenu
        public static string menuBody { get; set; }

        // SOI's
        public static Dictionary<string, double> sphereOfInfluence { get; set; }
        public static Dictionary<string, double> hillSphere { get; set; }

        // Barycenters
        public static List<string> barycenters { get; set; }

        // Bodies who aren't selectable
        public static List<string> notSelectable { get; set; }

        // Orbits
        public static Dictionary<string, OrbitRenderer.DrawIcons> drawIcons { get; set; }
        public static Dictionary<string, OrbitRenderer.DrawMode> drawMode { get; set; }

        // RnD
        public static Dictionary<string, PropertiesLoader.RDVisibility> hiddenRnD { get; set; }

        // Initialisation
        static Templates()
        {
            // We need to get the body for Jool (to steal it's mesh)
            PSystemBody Jool = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, "Jool");

            // Return it's mesh
            ReferenceGeosphere = Jool.scaledVersion.GetComponent<MeshFilter>().sharedMesh;

            // Create Dictionaries
            sphereOfInfluence = new Dictionary<string, double>();
            hillSphere = new Dictionary<string, double>();
            drawIcons = new Dictionary<string, OrbitRenderer.DrawIcons>();
            drawMode = new Dictionary<string, OrbitRenderer.DrawMode>();
            hiddenRnD = new Dictionary<String, PropertiesLoader.RDVisibility>();

            // Create lists
            barycenters = new List<string>();
            notSelectable = new List<string>();
            finalizeBodies = new List<string>();

            // Main Menu body
            menuBody = "Kerbin";
        }
    }
}
