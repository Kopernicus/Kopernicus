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
 
using Kopernicus.Components;
using System.Collections.Generic;
using System.Reflection;
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
        public CelestialBody celestialBody { get; set; }

        // We need to patch the sun Transform of the Radiators
        private static FieldInfo radiatorSun { get; set; }

        // get the FieldInfo
        static StarComponent()
        {
            radiatorSun = typeof(ModuleDeployableRadiator).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(f => f.FieldType == typeof(Transform));
        }

        void Start()
        {
            // Find the celestial body we are attached to
            celestialBody = PSystemManager.Instance.localBodies.Where(body => body.scaledBody == gameObject).First();
            Logger.Default.Log("StarLightSwitcher.Start() => " + celestialBody.bodyName);
            Logger.Default.Flush();
        }

        public void SetAsActive(bool forcedUpdate = false)
        {
            // Only reset the Sun / SolarPanels if we don't force an update
            if (!forcedUpdate)
            {
                // Set star as active star
                Debug.Log("[Kopernicus]: StarLightSwitcher: Set active star => " + celestialBody.bodyName);

                // Set custom powerCurve for solar panels and reset Radiators
                if (FlightGlobals.ActiveVessel != null)
                {
                    // SoalrPanels
                    foreach (ModuleDeployableSolarPanel sp in FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleDeployableSolarPanel>())
                        sp.sunTransform = celestialBody.transform;

                    // Radiators
                    foreach (ModuleDeployableRadiator rad in FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleDeployableRadiator>())
                        radiatorSun.SetValue(rad, celestialBody.transform);

                    // Flight Integrator
                    FlightIntegrator.sunBody = celestialBody;
                }
            }

            // Reset the LightShifter
            LightShifter lsc = null;
            LightShifter[] comps = Sun.Instance.sun.scaledBody.GetComponentsInChildren<LightShifter>(true);
            if (comps != null && comps.Length > 0)
            {
                lsc = comps[0];
                lsc.SetStatus(false, HighLogic.LoadedScene);
            }
            Sun.Instance.sun = celestialBody;
            comps = celestialBody.scaledBody.GetComponentsInChildren<LightShifter>(true);
            if (comps != null && comps.Length > 0)
            {
                lsc = comps[0];
                lsc.SetStatus(true, HighLogic.LoadedScene);

                // Set SunFlare color
                Sun.Instance.sunFlare.color = lsc.sunLensFlareColor;
                Sun.Instance.SunlightEnabled(lsc.givesOffLight);

                // Set other stuff
                Sun.Instance.AU = lsc.AU;
                Sun.Instance.brightnessCurve = lsc.brightnessCurve.Curve;

                // Physics Globals
                PhysicsGlobals.SolarLuminosityAtHome = lsc.solarLuminosity;
                PhysicsGlobals.SolarInsolationAtHome = lsc.solarInsolation;
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
        public List<StarComponent> stars;

        // On awake(), preserve the star
        void Awake()
        {
            // Don't run if Kopernicus is incompatible
            if (!CompatibilityChecker.IsCompatible())
            {
                Destroy(this);
                return;
            }

            Logger.Default.Log ("StarLightSwitcher.Awake(): Begin");
            Logger.Default.Flush ();
            DontDestroyOnLoad (this);
        }

        // On Scene Change, update the current star to "fix" PSystemSetup
        private bool forcedUpdate = false;
        private void OnLevelWasLoaded(int level)
        {
            if (HighLogic.LoadedSceneHasPlanetarium || HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER)
                forcedUpdate = true;
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
            StarComponent selectedStar = null;
        
            // If forceUpdate is enabled, update the active star
            if (forcedUpdate && Sun.Instance)
            {
                stars.First(s => s.IsActiveStar()).SetAsActive(forcedUpdate);
                // reset forced Update
                forcedUpdate = false;
            }

            // If we are in the tracking station, space center or game, 
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.SPACECENTER)
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
                else if (SpaceCenter.Instance != null && SpaceCenter.Instance.SpaceCenterTransform != null)
                {
                    Vector3 position = SpaceCenter.Instance.SpaceCenterTransform.position;
                    selectedStar = stars.OrderBy(star => FlightGlobals.getAltitudeAtPos(position, star.celestialBody)).First();
                }

                // If the star has been changed, update everything
                if (selectedStar != null && !selectedStar.IsActiveStar())
                {
                    selectedStar.SetAsActive();
                }
                else if (selectedStar == null && !HomeStar().IsActiveStar())
                {
                    HomeStar().SetAsActive();
                }
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
            Utility.DumpObjectProperties (scaledVersion.GetComponent<Renderer>().material);

            // Get the sun corona objects in scaled space
            foreach (SunCoronas corona in scaledVersion.GetComponentsInChildren<SunCoronas>(true)) 
            {
                Logger.Active.Log ("---- Sun Corona ----");
                Utility.PrintTransform (corona.transform);
                Utility.DumpObjectProperties (corona);
                Utility.DumpObjectProperties (corona.GetComponent<Renderer>().material);
                Logger.Active.Log ("--------------------");
            }
        }
    }
}
