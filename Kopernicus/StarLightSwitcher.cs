/**
 * Kopernicus Planetary System Modifier
 * 
 * Copyright (C) 2015 Bryce C Schroeder (bryce.schroeder@gmail.com)
 *                    Nathaniel R. Lewis (linux.robotdude@gmail.com)
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
 * 
 * Original code from Star Systems by OvenProofMars, Fastwings, medsouz
 * Modified by Thomas P., Nathaniel R. Lewis (Teknoman117) for Kopernicus
 * 
 */

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Kopernicus
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class StarLightSwitcher : MonoBehaviour
    {
		// List of celestial bodies that are stars
		private List<CelestialBody> stars;

		// MonoBehavior.Awake()
		void Awake()
		{
			// Don't kill us
			Debug.Log ("[Kopernicus]: StarLightSwitcher Started");
			DontDestroyOnLoad (this);
			stars = PSystemManager.Instance.localBodies.Where (body => body.scaledBody.GetComponentsInChildren<SunShaderController> (true).Length > 0).ToList ();
		}

        void Update()
        {
            // Get the current position of the active vessel
			Vector3 position = Vector3.zero;
			if (PlanetariumCamera.fetch.enabled == true) 
			{
				position = ScaledSpace.ScaledToLocalSpace (PlanetariumCamera.fetch.GetCameraTransform ().position);
			} 
			else if (FlightGlobals.ActiveVessel != null) 
			{
				position = FlightGlobals.ActiveVessel.GetTransform ().position;
			}

			// Get the closest star
			CelestialBody closestStar = stars.OrderBy (star => FlightGlobals.getAltitudeAtPos (position, star)).First ();
			if (closestStar != Sun.Instance.sun) 
			{
				SetSun (closestStar);
			}
        }

		// Set the active sun object
        public void SetSun(CelestialBody CB)
        {
            //Set star as active star
            Sun.Instance.sun = CB;
            Planetarium.fetch.Sun = CB;
            Debug.Log("Active sun set to: " + CB.name);

            //Set sunflare color
			//Sun.Instance.sunFlare.color = CB.light.color;

            //Reset solar panels (Credit to Kcreator)
            foreach (ModuleDeployableSolarPanel panel in FindObjectsOfType(typeof(ModuleDeployableSolarPanel)))
            {
                panel.OnStart(PartModule.StartState.Orbital);
            }
        }
    }
}
