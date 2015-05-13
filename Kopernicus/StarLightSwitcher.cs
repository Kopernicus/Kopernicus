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

using Kopernicus.Configuration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kopernicus
{
	// Class to manage the properties of custom stars
	public class StarComponent : MonoBehaviour
	{
        // Solar power curve of the star
        public FloatCurve powerCurve;

        // Celestial body which represents the star
        public CelestialBody celestialBody { get; private set; }

        void Start()
        {
            // Find the celestial body we are attached to
            celestialBody = PSystemManager.Instance.localBodies.Where(body => body.scaledBody == gameObject).First();
            Logger.Default.Log("StarLightSwitcher.Start() => " + celestialBody.bodyName);
            Logger.Default.Flush();
        }

        public void SetAsActive()
        {
            LightShifterComponent lsc_old = Sun.Instance.sun.GetTransform().GetComponentsInChildren<LightShifterComponent>(true)[0];
            lsc_old.SetStatus(false, HighLogic.LoadedScene);

            // Set star as active star
            Sun.Instance.sun = celestialBody;
            Planetarium.fetch.Sun = celestialBody;
            Debug.Log("[Kopernicus]: StarLightSwitcher: Set active star => " + celestialBody.bodyName);

            LightShifterComponent lsc_new = celestialBody.GetTransform().GetComponentsInChildren<LightShifterComponent>(true)[0];
            lsc_new.SetStatus(true, HighLogic.LoadedScene);

            // Set SunFlare color
            Sun.Instance.sunFlare.color = lsc_new.sunLensFlareColor;
            Sun.Instance.SunlightEnabled(lsc_new.givesOffLight);

			// Set custom powerCurve for solar panels
			if (FlightGlobals.ActiveVessel != null)
			{
				foreach (ModuleDeployableSolarPanel sp in FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleDeployableSolarPanel>())
				{
					sp.OnStart (PartModule.StartState.Orbital);
                    sp.useCurve = true;
					sp.powerCurve = powerCurve;
				}
			}
		}

		public bool IsActiveStar()
		{
			return (Sun.Instance.sun == celestialBody);
		}
	}

	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class StarLightSwitcher : MonoBehaviour
    {
		// List of celestial bodies that are stars
		private List<StarComponent> stars;

		// On awake(), preserve the star
		void Awake()
		{
			Logger.Default.Log ("StarLightSwitcher.Awake(): Begin");
			Logger.Default.Flush ();
			DontDestroyOnLoad (this);
		}

		void Start()
		{
			// find all the stars in the system
			stars = PSystemManager.Instance.localBodies.SelectMany (body => body.scaledBody.GetComponentsInChildren<StarComponent>(true)).ToList();

			// Disable queued update because Planetarium is overly complicated and does strange things
			foreach (CelestialBody star in PSystemManager.Instance.localBodies)
				if (star.orbitDriver != null)
					star.orbitDriver.QueuedUpdate = false;

			// Flush the log
			Logger.Default.Flush ();
		}

        void Update()
        {
			// If the game scene is the space center, we need to make sure the star is our home star
			StarComponent selectedStar = null;
			if (HighLogic.LoadedScene == GameScenes.SPACECENTER) 
			{
				selectedStar = HomeStar ();
			}

			// If we are in the tracking station or game, 
			else if(HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.FLIGHT)
			{
	            // Get the current position of the active vessel
				if (PlanetariumCamera.fetch.enabled == true) 
				{
					Vector3 position = ScaledSpace.ScaledToLocalSpace (PlanetariumCamera.fetch.GetCameraTransform ().position);
					selectedStar = stars.OrderBy (star => FlightGlobals.getAltitudeAtPos (position, star.celestialBody)).First ();
				} 
				else if (FlightGlobals.ActiveVessel != null) 
				{
					Vector3 position = FlightGlobals.ActiveVessel.GetTransform ().position;
					selectedStar = stars.OrderBy (star => FlightGlobals.getAltitudeAtPos (position, star.celestialBody)).First ();
				}
			}

			// If the star has been changed, update everything
			if (selectedStar != null && !selectedStar.IsActiveStar()) 
			{
				selectedStar.SetAsActive ();
			}
        }

		// Select the home star
		public static StarComponent HomeStar()
		{
			return PSystemManager.Instance.localBodies.Where (body => body.flightGlobalsIndex == 0).First ().scaledBody.GetComponent<StarComponent> ();
		}

		// Debug a star's coronas
		public static void DebugSunScaledSpace(GameObject scaledVersion)
		{
			// Debug the scaled space size of the star
			Utility.PrintTransform (scaledVersion.transform, " " + scaledVersion.name + " Transform ");
			Utility.DumpObjectProperties (scaledVersion.renderer.material);

			// Get the sun corona objects in scaled space
			foreach (SunCoronas corona in scaledVersion.GetComponentsInChildren<SunCoronas>(true)) 
			{
				Logger.Active.Log ("---- Sun Corona ----");
				Utility.PrintTransform (corona.transform);
				Utility.DumpObjectProperties (corona);
				Utility.DumpObjectProperties (corona.renderer.material);
				Logger.Active.Log ("--------------------");
			}
		}
	}
}
