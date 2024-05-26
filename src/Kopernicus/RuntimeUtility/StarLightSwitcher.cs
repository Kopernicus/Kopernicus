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

            Logger.Default.Log("StarLightSwitcher.Awake(): Begin");
            Logger.Default.Flush();
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            // find all the stars in the system
            stars = PSystemManager.Instance.localBodies.SelectMany(body => body.scaledBody.GetComponentsInChildren<StarComponent>(true)).ToList();

            // Flush the log
            Logger.Default.Flush();

            // GameScenes
            GameEvents.onLevelWasLoadedGUIReady.Add(scene => HomeStar().SetAsActive());
        }

        private void Update()
        {
            if (KopernicusStar.UseMultiStarLogic)
            {
                //StarComponent selectedStar = null; Do we need this at all?

                Vector3d position = Vector3d.zero;
                bool positionIsKnown = false;

                // If we are in the tracking station, space center or flight, get position of the focused object
                switch (HighLogic.LoadedScene)
                {
                    case GameScenes.TRACKSTATION:
                        if (PlanetariumCamera.fetch.enabled)
                        {
                            position = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.fetch.GetCameraTransform().position);
                            positionIsKnown = true;
                        }
                        break;
                    case GameScenes.SPACECENTER:
                        if (SpaceCenter.Instance.IsNotNullOrDestroyed() && SpaceCenter.Instance.SpaceCenterTransform.IsNotNullOrDestroyed())
                        {
                            position = SpaceCenter.Instance.SpaceCenterTransform.position;
                            positionIsKnown = true;
                        }
                        break;
                    case GameScenes.FLIGHT:
                        if (FlightGlobals.ActiveVessel.IsNotNullOrDestroyed())
                        {
                            position = FlightGlobals.ActiveVessel.GetTransform().position;
                            positionIsKnown = true;
                        }
                        break;
                    default:
                        return;
                }

                // if position couldn't be found, abort
                if (!positionIsKnown)
                {
                    if (!HomeStar().IsActiveStar())
                        HomeStar().SetAsActive();

                    return;
                }

                // find which star is the closest from the focused object
                StarComponent closestStar = null;
                double closestDistance = double.MaxValue;
                for (int i = stars.Count; i-- > 0;)
                {
                    double distance = (position - stars[i].CelestialBody.position).magnitude;
                    if (distance < closestDistance)
                    {
                        closestStar = stars[i];
                        closestDistance = distance;
                    }
                }

                if (!closestStar.IsActiveStar())
                    closestStar.SetAsActive();
            }
        }

        // Select the home star
        private static StarComponent HomeStar()
        {
            return PSystemManager.Instance.localBodies.First(body => body.flightGlobalsIndex == 0).scaledBody.GetComponent<StarComponent>();
        }
    }
}
