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
			
			// Create a new planetary system object
			GameObject gameObject = new GameObject (systemName);
			PSystem system = gameObject.AddComponent<PSystem> ();
			
			// Set the planetary system defaults (pulled from PSystemManager.Instance.systemPrefab)
			system.systemName          = systemName;
			system.systemTimeScale     = 1.0; 
			system.systemScale         = 1.0;
			system.mainToolbarSelected = 2;   // initial value in stock systemPrefab. Unknown significance.
			
			//CelestialBody eve = PSystemManager.Instance.systemPrefab.rootBody.children [1].celestialBody;
			//CelestialBody moho = PSystemManager.Instance.systemPrefab.rootBody.children [0].celestialBody;
			//CelestialBody kerbin = PSystemManager.Instance.systemPrefab.rootBody.children [2].celestialBody;
			//PSystemBody eve = PSystemManager.Instance.systemPrefab.rootBody.children [1];
			//PSystemBody moho = PSystemManager.Instance.systemPrefab.rootBody.children [0];
			//PSystemBody kerbin = PSystemManager.Instance.systemPrefab.rootBody.children [2];
			
			// ---------- TEMPORARY ------------
			// Reparent the existing PSystemBody tree to the new system.  For testing purposes
			system.rootBody = PSystemManager.Instance.systemPrefab.rootBody;
			
			/*kps.rootBody = kps.AddBody (null); // many of these properties are set up by AddBody but not meaningfully
			kps.rootBody.celestialBody = PSystemManager.Instance.systemPrefab.rootBody.celestialBody; //
			kps.rootBody.planetariumCameraInitial = PSystemManager.Instance.systemPrefab.rootBody.planetariumCameraInitial;
			kps.rootBody.flightGlobalsIndex = PSystemManager.Instance.systemPrefab.rootBody.flightGlobalsIndex;
			kps.rootBody.pqsVersion = PSystemManager.Instance.systemPrefab.rootBody.pqsVersion;
			kps.rootBody.scaledVersion = PSystemManager.Instance.systemPrefab.rootBody.scaledVersion;
			kps.rootBody.resources = PSystemManager.Instance.systemPrefab.rootBody.resources; //
			kps.rootBody.orbitRenderer = PSystemManager.Instance.systemPrefab.rootBody.orbitRenderer; //
			kps.rootBody.orbitDriver = PSystemManager.Instance.systemPrefab.rootBody.orbitDriver; //
			//XXkps.rootBody.children = new List<PSystemBody> ();*/
			
			// Find the dres prefab
			PSystemBody Dres = KopernicusUtility.FindBody (system.rootBody, "Dres");
			
			// Try to figure out where the PQS controller comes from
			if (Dres.celestialBody.pqsController == null) 
			{	
				Debug.Log ("[Kopernicus]: KopernicusSystemSource.GenerateSystem(): Where does the PQS controller come from???");
			} else 
			{
				Debug.Log ("[Kopernicus]: KopernicusSystemSource.GenerateSystem(): Dres prefab has a PQS controller");
			}
			
			// Create "Kopernicus"
			PSystemBody body = system.AddBody(system.rootBody);
			body.name = "Kopernicus";
			body.celestialBody.bodyName = "Kopernicus";
			body.celestialBody.Radius = 300000;
			body.orbitDriver.orbit = new Orbit (0.0, 0.0, 150000000000, 0, 0, 0, 0, system.rootBody.celestialBody);
			body.orbitRenderer.orbitColor = Color.magenta;
			body.flightGlobalsIndex = 100;
			
			/** Relavent snippet from scaled version dump 
			 * [LOG 00:57:21.294] ---------- Scaled Version Dump -----------
			 * [LOG 00:57:21.294] Dres (UnityEngine.GameObject)
			 * [LOG 00:57:21.294]  >>> Components <<< 
			 * [LOG 00:57:21.294]  Dres (UnityEngine.Transform)
			 * [LOG 00:57:21.294]  Dres (UnityEngine.MeshFilter)
			 * [LOG 00:57:21.294]  Dres (UnityEngine.MeshRenderer)
			 * [LOG 00:57:21.294]  Dres (UnityEngine.SphereCollider)
			 * [LOG 00:57:21.294]  Dres (ScaledSpaceFader)
			 * [LOG 00:57:21.295]  >>> ---------- <<< 
			 * [LOG 00:57:21.295] -----------------------------------------
			 */
			// Temporarily clone the Dres scaled version for the structure
			GameObject scaledVersion = (GameObject) UnityEngine.Object.Instantiate(Dres.scaledVersion);
			scaledVersion.name = "Kopernicus";
			body.scaledVersion = scaledVersion;
			
			// Adjust the scaled space fader to our new celestial body
			ScaledSpaceFader fader = scaledVersion.GetComponent<ScaledSpaceFader> ();
			fader.celestialBody = body.celestialBody;
			
			// Return the newly created planetary system
			return system;
		}
	}
}

