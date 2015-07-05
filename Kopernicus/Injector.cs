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
using System.Text;
using System.Reflection;

using UnityEngine;
using Kopernicus.Configuration;

namespace Kopernicus 
{
	// Hook the PSystemSpawn (creation of the planetary system) event in the KSP initialization lifecycle
	[KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
	public class Injector : MonoBehaviour 
	{
        public Templates templates = null;

        public static bool dontUpdate = true; // Should OnDemand update?
        
		/**
		 * Awake() is the first function called in the lifecycle of a Unity3D MonoBehaviour.  In the case of KSP,
		 * it happens to be called right before the game's PSystem is instantiated from PSystemManager.Instance.systemPrefab
		 *
		 * TL,DR - Custom planet injection happens here
		 * 
		 **/
		public void Awake()
		{
			// We're ALIVE
            Logger.Initialize();
			Logger.Default.SetAsActive ();
			Logger.Default.Log("Injector.Awake(): Begin");

			// Yo garbage collector - we have work to do man
			DontDestroyOnLoad(this);

			// If the planetary manager does not work, well, error out
			if (PSystemManager.Instance == null) 
			{
				// Log the error
				Logger.Default.Log("Injector.Awake(): If PSystemManager.Instance is null, there is nothing to do");
				return;
			}

			// Get the current time
			DateTime start = DateTime.Now;

            // Grab templates
            templates = new Templates();
           

			// THIS IS WHERE THE MAGIC HAPPENS - OVERWRITE THE SYSTEM PREFAB SO KSP ACCEPTS OUR CUSTOM SOLAR SYSTEM AS IF IT WERE FROM SQUAD
			PSystemManager.Instance.systemPrefab = (new Configuration.Loader()).Generate();

			// SEARCH FOR THE ARCHIVES CONTROLLER PREFAB AND OVERWRITE IT WITH THE CUSTOM SYSTEM
			RDArchivesController archivesController = AssetBase.RnDTechTree.GetRDScreenPrefab ().GetComponentsInChildren<RDArchivesController> (true).First ();
			archivesController.systemPrefab = PSystemManager.Instance.systemPrefab;

			// Clear space center instance so it will accept nouveau Kerbin
            SpaceCenter.Instance = null;

			// Add a handler so that we can do post spawn fixups.  
			PSystemManager.Instance.OnPSystemReady.Add(PostSpawnFixups);

			// Done executing the awake function
			TimeSpan duration = (DateTime.Now - start);
			Logger.Default.Log("Injector.Awake(): Completed in: " + duration.TotalMilliseconds + " ms");
			Logger.Default.Flush ();
		}

		// Post spawn fixups (ewwwww........)
		public void PostSpawnFixups ()
		{
            Debug.Log("[Kopernicus]: Post-Spawn");

			// Fix the flight globals index of each body and patch it's SOI
			int counter = 0;
			foreach (CelestialBody body in FlightGlobals.Bodies) 
			{
				body.flightGlobalsIndex = counter++;

                // Path the SOI
                if (Templates.sphereOfInfluence.ContainsKey(body.bodyTransform.name))
                {
                    body.sphereOfInfluence = Templates.sphereOfInfluence[body.bodyTransform.name];
                }

                // Patch the Hill Sphere
                if (Templates.hillSphere.ContainsKey(body.bodyTransform.name))
                {
                    body.hillSphere = Templates.hillSphere[body.bodyTransform.name];
                }

                // Make the Body a barycenter
                if (Templates.barycenters.Contains(body.GetTransform().name))
                {
                    body.scaledBody.SetActiveRecursively(false);
                }

				Logger.Default.Log ("Found Body: " + body.bodyName + ":" + body.flightGlobalsIndex + " -> SOI = " + body.sphereOfInfluence + ", Hill Sphere = " + body.hillSphere);
			}

            // Move KSC around
            if (SpaceCenterSwitcher.Instance != null)
            {
                SpaceCenterSwitcher.Instance.MoveKSC();
            }

			// Fix the maximum viewing distance of the map view camera (get the farthest away something can be from the root object)
			PSystemBody rootBody = PSystemManager.Instance.systemPrefab.rootBody;
            double maximumDistance = 1000d; // rootBody.children.Max(b => (b.orbitDriver != null) ? b.orbitDriver.orbit.semiMajorAxis * (1 + b.orbitDriver.orbit.eccentricity) : 0);
            if (rootBody != null)
            {
                maximumDistance = rootBody.celestialBody.Radius * 100d;
                if(rootBody.children != null && rootBody.children.Count > 0)
                {
                    foreach (PSystemBody body in rootBody.children)
                    {
                        if (body.orbitDriver != null)
                            maximumDistance = Math.Max(maximumDistance, body.orbitDriver.orbit.semiMajorAxis * (1d + body.orbitDriver.orbit.eccentricity));
                        else
                            Debug.Log("[Kopernicus]: Body " + body.name + " has no orbitdriver!");
                    }
                }
                else
                    Debug.Log("[Kopernicus]: Root body children null or 0");
            }
            else
                Debug.Log("[Kopernicus]: Root body null!");
            Debug.Log("Found max distance " + maximumDistance);
			PlanetariumCamera.fetch.maxDistance = ((float)maximumDistance * 3.0f) / ScaledSpace.Instance.scaleFactor;

			// Select the closest star to home
			StarLightSwitcher.HomeStar ().SetAsActive ();

            // Declare we're done.
            Templates.loadFinished = true;

            // Enable OnDemand Updates
            dontUpdate = false;

			// Fixups complete, time to surrender to fate
			Destroy (this);
		}

		// Log the destruction of the injector
		public void OnDestroy()
		{
			Logger.Default.Log("Injector.OnDestroy(): Complete");
			Logger.Default.Flush ();
		}
	}
} //namespace

