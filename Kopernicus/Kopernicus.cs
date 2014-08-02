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
	public class Injector : MonoBehaviour 
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
			Debug.Log("[Kopernicus]: Injector.Awake(): Begin");

			// If the planetary manager does not work, well, error out
			if (PSystemManager.Instance == null) 
			{
				// Log the error
				Debug.LogError("[Kopernicus]: Injector.Awake(): If PSystemManager.Instance is null, there is nothing to do");
				return;
			}

			// THIS IS WHERE THE MAGIC HAPPENS - OVERWRITE THE SYSTEM PREFAB SO KSP ACCEPTS OUR CUSTOM SOLAR SYSTEM AS IF IT WERE FROM SQUAD
			PSystemManager.Instance.systemPrefab = KopernicusSystemSource.GenerateSystem ();

			// SEARCH FOR THE ARCHIVES CONTROLLER PREFAB AND OVERWRITE IT WITH THE CUSTOM SYSTEM
			RDArchivesController archivesController = KopernicusUtility.RecursivelyGetComponent<RDArchivesController> (AssetBase.RnDTechTree.GetRDScreenPrefab ().transform);
			archivesController.systemPrefab = PSystemManager.Instance.systemPrefab;

			// Done executing the awake function
			Debug.Log ("[Kopernicus]: Injector.Awake(): End");
		}

		// Log the destruction of the injector
		public void OnDestroy()
		{
			Debug.Log ("[Kopernicus]: Injection Complete");
		}
	}

	// Mod runtime utilitues
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class RuntimeUtility : MonoBehaviour
	{
		/**
		 * If Control-P are pressed, dump the PQS data of the current live body
		 **/
		public void Update()
		{
			// Print out the PQS state
			if( Input.GetKeyDown( KeyCode.P ) && Input.GetKey( KeyCode.LeftControl ) )
			{
				// Log the state of the PQS
				KopernicusUtility.DumpObject(FlightGlobals.currentMainBody.pqsController, " Live PQS ");

				// Dump the child PQSs
				foreach(PQS p in FlightGlobals.currentMainBody.pqsController.ChildSpheres)
				{
					KopernicusUtility.DumpObject(p, " " + p.ToString() + " ");
				}
			}
		}
		
		/**
		 * Awake() - flag this class as don't destroy on load
		 **/
		public void Awake ()
		{
			// Make sure the runtime utility isn't killed
			DontDestroyOnLoad (this);

			// Log
			Debug.Log ("[Kopernicus]: RuntimeUtility Started");
		}
	}

} //namespace

