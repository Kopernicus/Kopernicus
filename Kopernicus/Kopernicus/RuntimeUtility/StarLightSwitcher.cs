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

        // Celestial body which represents the star
        public CelestialBody celestialBody { get; set; }

        // We need to patch the sun Transform of the Radiators
        private static FieldInfo radiatorSun { get; }

        // get the FieldInfo
        static StarComponent()
        {
            radiatorSun = typeof(ModuleDeployableRadiator).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(f => f.FieldType == typeof(Transform));
        }

        void Start()
        {
            // Find the celestial body we are attached to
            celestialBody = PSystemManager.Instance.localBodies.First(body => body.scaledBody == gameObject);
            Logger.Default.Log("StarLightSwitcher.Start() => " + celestialBody.bodyName);
            Logger.Default.Flush();
        }

        public void SetAsActive()
        {
            // Set star as active star
            Debug.Log("[Kopernicus]: StarLightSwitcher: Set active star => " + celestialBody.bodyName);
            KopernicusStar.Current = KopernicusStar.Stars.Find(s => s.sun == celestialBody);

            // Set custom powerCurve for solar panels and reset Radiators
            if (FlightGlobals.ActiveVessel != null)
            {
                // SolarPanels
                foreach (ModuleDeployableSolarPanel sp in FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleDeployableSolarPanel>())
                {
                    sp.sunTransformLocal = celestialBody.transform;
                    if (celestialBody.scaledBody)
                    {
                        sp.sunTransformScaled = celestialBody.scaledBody.transform;
                    }
                }

                // Radiators
                foreach (ModuleDeployableRadiator rad in FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleDeployableRadiator>())
                    radiatorSun.SetValue(rad, celestialBody.transform);
            }

            // Apply Ambient Light
            KopernicusStar.Current.shifter.ApplyAmbient();
            PhysicsGlobals.RadiationFactor = KopernicusStar.Current.shifter.radiationFactor;

            // Apply Sky
            GalaxyCubeControl.Instance.sunRef = KopernicusStar.Current;
            foreach (SkySphereControl c in Resources.FindObjectsOfTypeAll<SkySphereControl>())
                c.sunRef = KopernicusStar.Current;
        }

        public bool IsActiveStar()
        {
            return (KopernicusStar.Current.sun == celestialBody);
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

        void Start()
        {
            // find all the stars in the system
            stars = PSystemManager.Instance.localBodies.SelectMany (body => body.scaledBody.GetComponentsInChildren<StarComponent>(true)).ToList();

            // Flush the log
            Logger.Default.Flush ();

            // GameScenes
            GameEvents.onLevelWasLoaded.Add(scene => HomeStar().SetAsActive());
        }

        void Update()
        {
            StarComponent selectedStar = null;

            // If we are in the tracking station, space center or game, 
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                // Get the current position of the active vessel
                if (PlanetariumCamera.fetch.enabled) 
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
            return PSystemManager.Instance.localBodies.First (body => body.flightGlobalsIndex == 0).scaledBody.GetComponent<StarComponent> ();
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
