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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;

namespace Kopernicus 
{
	// Hook the PSystemSpawn (creation of the planetary system) event in the KSP initialization lifecycle
	[KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
	public class KopernicusInjector : MonoBehaviour 
	{
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
			Debug.Log("[Kopernicus]: KopernicusInjector.Awake(): Begin");

			// If the planetary manager does not work, well, error out
			if (PSystemManager.Instance == null) 
			{
				// Log the error
				Debug.LogError("[Kopernicus]: KopernicusInjector.Awake(): If PSystemManager.Instance is null, there is nothing to do");
				return;
			}

			// Prevent the Unity3D scene loader from culling this behavior (Please don't kill us)
			DontDestroyOnLoad (this);

			// THIS IS WHERE THE MAGIC HAPPENS - OVERWRITE THE SYSTEM PREFAB SO KSP ACCEPTS OUR CUSTOM SOLAR SYSTEM AS IF IT WERE FROM SQUAD
			PSystemManager.Instance.systemPrefab = KopernicusSystemSource.GenerateSystem ();

			// SEARCH FOR THE ARCHIVES CONTROLLER PREFAB AND OVERWRITE IT WITH THE CUSTOM SYSTEM
			RDArchivesController archivesController = KopernicusUtility.RecursivelyGetComponent<RDArchivesController> (AssetBase.RnDTechTree.GetRDScreenPrefab ().transform);
			archivesController.systemPrefab = PSystemManager.Instance.systemPrefab;

			// Register hanlders for important game events.
			PSystemManager.Instance.OnPSystemReady.Add (OnPSystemReady);
			GameEvents.onGUIRnDComplexSpawn.Add (OnRnDComplexSpawn);
			GameEvents.onGUIRnDComplexDespawn.Add (OnRnDComplexDespawn);

			// Done executing the awake function
			Debug.Log ("[Kopernicus]: KopernicusInjector.Awake(): End");
		}

		/**
		 * If Control-P are pressed, dump the PQS of the current live body
		 **/
		public void Update()
		{
			if( Input.GetKeyDown( KeyCode.P ) && Input.GetKey( KeyCode.LeftControl ) )
			{
				KopernicusUtility.DumpObject(FlightGlobals.currentMainBody.pqsController, " Live PQS ");
				KopernicusUtility.GameObjectWalk(FlightGlobals.currentMainBody.pqsController.gameObject);
			}
		}

		/**
		 *  OnPSystemReady() called when the PSystem is loaded.  Somehow get a handle?
		 **/
		public void OnPSystemReady()
		{
			Debug.Log ("[Kopernicus]: KopernicusInjector.OnPSystemReady(): Begin");

			// Run the PQS mod alteration
			KopernicusPlanetSource.ActivateSystemBody("Kopernicus");

			Debug.Log ("[Kopernicus]: KopernicusInjector.OnPSystemReady(): End");
		}

		/**
		 * Well ain't this a bitch - Since we can't generate prefab objects at runtime, the scaled version in the prefab is 
		 * technically live.  This means it will end up getting rendered.  This is bad - it just randomly pops around the view.  
		 * We disable it to prevent this problem and then manually enable the scaled space verison later.  However, when the 
		 * RnD center clones scaledVersion internally for the icons, it gets a disabled object.  We have to enable
		 * scaledVersion in the prefab only while in the RnD center.  This is pretty hacky, but I don't see a way
		 * around it unless we can remove an arbitrary game object from the render queue in such a way that Instantiate 
		 * doesn't copy it
		 **/
		public void OnRnDComplexSpawn()
		{
			PSystemBody body = KopernicusUtility.FindBody (PSystemManager.Instance.systemPrefab.rootBody, "Kopernicus");
			body.scaledVersion.SetActive (true);
		}

		public void OnRnDComplexDespawn()
		{
			PSystemBody body = KopernicusUtility.FindBody (PSystemManager.Instance.systemPrefab.rootBody, "Kopernicus");
			body.scaledVersion.SetActive (false);
		}
	}

} //namespace

