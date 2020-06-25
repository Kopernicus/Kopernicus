/**
 * Kopernicus Planetary System Modifier
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
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Kopernicus.Components;
using Kopernicus.Constants;
using UnityEngine;

namespace Kopernicus.RuntimeUtility
{
    // Class to manage the properties of custom stars
    public class StarComponent : MonoBehaviour
    {
        // Celestial body which represents the star
        public CelestialBody CelestialBody { get; set; }

        private void Start()
        {
            // Find the celestial body we are attached to
            CelestialBody = PSystemManager.Instance.localBodies.FirstOrDefault(body => body.scaledBody == gameObject);
            if (CelestialBody != null)
            {
                Logger.Default.Log("StarLightSwitcher.Start() => " + CelestialBody.bodyName);
            }
            Logger.Default.Flush();
        }

        public void SetAsActive()
        {
            // Set star as active star
            Debug.Log("[Kopernicus]: StarLightSwitcher: Set active star => " + CelestialBody.bodyName);
            KopernicusStar.Current = KopernicusStar.Stars.Find(s => s.sun == CelestialBody);

            // Set custom powerCurve for solar panels and reset Radiators
            if (FlightGlobals.ActiveVessel)
            {
                // Radiators
                foreach (ModuleDeployableRadiator rad in FlightGlobals.VesselsLoaded.SelectMany(v => v.FindPartModulesImplementing<ModuleDeployableRadiator>()))
                {
                    rad.trackingBody = CelestialBody;
                    rad.trackingTransformLocal = CelestialBody.transform;
                    if (CelestialBody.scaledBody)
                    {
                        rad.trackingTransformScaled = CelestialBody.scaledBody.transform;
                    }
                    rad.GetTrackingBodyTransforms();
                }
            }

            // Apply Ambient Light
            KopernicusStar.Current.shifter.ApplyAmbient();
            KopernicusStar.Current.shifter.ApplyPhysics();

            // Apply Sky
            GalaxyCubeControl.Instance.sunRef = KopernicusStar.Current;
            foreach (SkySphereControl c in Resources.FindObjectsOfTypeAll<SkySphereControl>())
            {
                c.sunRef = KopernicusStar.Current;
            }
            Events.OnRuntimeUtilitySwitchStar.Fire(KopernicusStar.Current);
        }

        public Boolean IsActiveStar()
        {
            return KopernicusStar.Current.sun == CelestialBody;
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class StarLightSwitcher : MonoBehaviour
    {
        // List of celestial bodies that are stars
        public List<StarComponent> stars;

        // On awake(), preserve the star
        private void Awake()
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

        private void Start()
        {
            // find all the stars in the system
            stars = PSystemManager.Instance.localBodies.SelectMany (body => body.scaledBody.GetComponentsInChildren<StarComponent>(true)).ToList();

            // Flush the log
            Logger.Default.Flush ();

            // GameScenes
            GameEvents.onLevelWasLoadedGUIReady.Add(scene => HomeStar().SetAsActive());
        }

        private void Update()
        {
            StarComponent selectedStar = null;

            // If we are in the tracking station, space center or game, 
            if (HighLogic.LoadedScene != GameScenes.TRACKSTATION && HighLogic.LoadedScene != GameScenes.FLIGHT &&
                HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                return;
            }
            
            // Get the current position of the active vessel
            if (PlanetariumCamera.fetch.enabled) 
            {
                Vector3 position = ScaledSpace.ScaledToLocalSpace (PlanetariumCamera.fetch.GetCameraTransform ().position);
                selectedStar = stars.OrderBy (star => FlightGlobals.getAltitudeAtPos (position, star.CelestialBody)).First ();
            } 
            else if (FlightGlobals.ActiveVessel) 
            {
                Vector3 position = FlightGlobals.ActiveVessel.GetTransform ().position;
                selectedStar = stars.OrderBy (star => FlightGlobals.getAltitudeAtPos (position, star.CelestialBody)).First ();
            }
            else if (SpaceCenter.Instance && SpaceCenter.Instance.SpaceCenterTransform)
            {
                Vector3 position = SpaceCenter.Instance.SpaceCenterTransform.position;
                selectedStar = stars.OrderBy(star => FlightGlobals.getAltitudeAtPos(position, star.CelestialBody)).First();
            }

            // If the star has been changed, update everything
            if (selectedStar && !selectedStar.IsActiveStar())
            {
                selectedStar.SetAsActive();
            }
            else if (!selectedStar && !HomeStar().IsActiveStar())
            {
                HomeStar().SetAsActive();
            }
        }

        // Select the home star
        private static StarComponent HomeStar()
        {
            return PSystemManager.Instance.localBodies.First (body => body.flightGlobalsIndex == 0).scaledBody.GetComponent<StarComponent> ();
        }
    }
}
