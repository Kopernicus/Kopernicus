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
			stars = PSystemManager.Instance.localBodies.Where (body => body.scaledBody.GetComponentsInChildren<KopernicusStarComponent> (true).Length > 0).ToList ();
		}

        void Update()
        {
			// TODO - it might be better to select the star based on the configuration of the celestial body heirachy.  Something is horrifically broken
			// with the space center camera.  Like random time warping, or refusing to believe the current sun is the sun...

			// If the game scene is the space center, we need to make sure the star is our home star
			CelestialBody selectedStar = null;
			if (HighLogic.LoadedScene == GameScenes.SPACECENTER) 
			{
				selectedStar = stars.Where (body => body.flightGlobalsIndex == 0).First ();
			}

            // Get the current position of the active vessel
			else if (PlanetariumCamera.fetch.enabled == true) 
			{
				Vector3 position = ScaledSpace.ScaledToLocalSpace (PlanetariumCamera.fetch.GetCameraTransform ().position);
				selectedStar = stars.OrderBy (star => FlightGlobals.getAltitudeAtPos (position, star)).First ();
			} 
			else if (FlightGlobals.ActiveVessel != null) 
			{
				Vector3 position = FlightGlobals.ActiveVessel.GetTransform ().position;
				selectedStar = stars.OrderBy (star => FlightGlobals.getAltitudeAtPos (position, star)).First ();
			}

			// Get the closest star
			if (selectedStar != Sun.Instance.sun && selectedStar != null) 
			{
				StarLightSwitcher.SetSun (selectedStar);
			}
        }

		// Set the active sun object
        public static void SetSun(CelestialBody CB)
        {
            // Set star as active star
            Sun.Instance.sun = CB;
			Planetarium.fetch.Sun = CB;
            Debug.Log("[Kopernicus]: StarLightSwitcher: Active star = " + CB.name);

			// Get the star component
			KopernicusStarComponent component = CB.scaledBody.GetComponent<KopernicusStarComponent> ();

            // Set sunflare color
			Sun.Instance.sunFlare.color = component.lightColor;

            // Set custom powerCurve for solar panels
            if (FlightGlobals.ActiveVessel != null)
            {
                foreach (ModuleDeployableSolarPanel sp in FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleDeployableSolarPanel>())
                {
					sp.OnStart (PartModule.StartState.Orbital);
					sp.powerCurve = component.powerCurve;
                }
            }
        }
    }
}
