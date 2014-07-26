/* 
Kopernicus Planetary System Modifier
Copyright (C) 2014 Bryce C Schroeder
bryce.schroeder@gmail.com
http://www.ferazelhosting.net/~bryce/contact.html

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

 This library is intended to be used as a plugin for Kerbal Space Program
 which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 itself is governed by the terms of its EULA, not the license above.
 https://kerbalspaceprogram.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kopernicus {

public class KopernicusPlanet {
		public static Orbit orbit;
}
	

public class KopernicusSystemSource:MonoBehaviour {
	// This function returns a PSystem that will replace the stock systemPrefab
	// with one of the modder's design. KSP then loads the replacement planetary
	// system just as it would have loaded the stock system.
	public PSystem generateSystem() {
			print ("Kopernicus generating planetary system 1X2.");
			// PSystem class evidentally represents a Planetary System.
			var tempGO = new GameObject ("KopernicusSystem");
			PSystem kps = tempGO.AddComponent<PSystem>();
			print ("Kopernicus generating planetary system X3. KPS=");
			print (kps);
			print ("tempGO=");
			print (tempGO);
			//kps.Reset ();
			kps.mainToolbarSelected = 2; // initial value in stock systemPrefab. Unknown significance.
			kps.systemTimeScale = 1; 
			kps.systemScale = 1;
			//kps.rootBody = PSystemManager.Instance.systemPrefab.rootBody.children[2]; // worked
			kps.rootBody = PSystemManager.Instance.systemPrefab.rootBody; // do nothing overt

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

			//CelestialBody eve = PSystemManager.Instance.systemPrefab.rootBody.children [1].celestialBody;
			//CelestialBody moho = PSystemManager.Instance.systemPrefab.rootBody.children [0].celestialBody;
			//CelestialBody kerbin = PSystemManager.Instance.systemPrefab.rootBody.children [2].celestialBody;
			/*PSystemBody eve = PSystemManager.Instance.systemPrefab.rootBody.children [1];
			PSystemBody moho = PSystemManager.Instance.systemPrefab.rootBody.children [0];
			PSystemBody kerbin = PSystemManager.Instance.systemPrefab.rootBody.children [2];*/


			kps.systemName = "Testar"; // default is "Unnamed"

			// How to attach a debugger... tried sdb without success but I lack experience
			// with sdb
			print ("Kopernicus generating planetary system RETURNING:");
			print (kps);
			return kps;
			//return PSystemManager.Instance.systemPrefab;
	}
}


	//Instantly // MainMenu
[KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
public class Kopernicus:MonoBehaviour {
		public void Awake() {

			/* This was just code to make sure we were at the right point chronologically 
			  in the startup sequence. */
			print("Kopernicus.Awake called.");
			print ("Instance at time of Awake: ");
			if (PSystemManager.Instance == null) 
				print ("Kopernicus sees PSystemManger.Instance as null.");
			else
				print ("Kopernicus sees PSystemManager.Instance as non-null.");
			if (PSystemManager.Instance != null) {
				if (PSystemManager.Instance.systemPrefab == null)
					print ("Kopernicus sees systemPrefab as null");
				else {
					print ("Kopernicus sees systemPrefab as non-null.");
					print (PSystemManager.Instance.systemPrefab.rootBody.celestialBody.bodyName);
					print (PSystemManager.Instance.localBodies);
				}
			}

			// Classes will inherrit from this to support different sources of planetary systems
			// e.g. a text file and data files, a planetfactory ce style thing making reference
			// to existing planets, or some kind of procedural generator
			KopernicusSystemSource src = new KopernicusSystemSource();
			// more ersatz-debuggery. wish I knew how to attach the debugger to a running ksp mod
			// should probably research this
			/*print ("systemName"); print (PSystemManager.Instance.systemPrefab.systemName);
			print ("mainToolbarSelected"); print (PSystemManager.Instance.systemPrefab.mainToolbarSelected);
			print ("systemTimeScale"); print (PSystemManager.Instance.systemPrefab.systemTimeScale);
			print ("systemScale"); print (PSystemManager.Instance.systemPrefab.systemScale);
			print ("rootBody"); print (PSystemManager.Instance.systemPrefab.rootBody);*/



			PSystemManager.Instance.systemPrefab = src.generateSystem ();

			print ("Kopernicus looking again at systemPrefab:");
			print (PSystemManager.Instance.systemPrefab);
		}

}



} //namespace

