using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using KSP;

namespace Kopernicus
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class MapViewFixer : MonoBehaviour
    {
        static float  max3DlineDrawDist = 20000f;

        public void Start()
        {
            if (HighLogic.LoadedSceneHasPlanetarium && MapView.fetch != null)
            {
                try
                {
                    MapView.fetch.max3DlineDrawDist = max3DlineDrawDist;
                }
                catch (Exception e)
                {
                    Debug.Log("[Kopernicus]: MapView fixing failed: " + e.Message);
                }
            }
        }

        private bool isDone = false;

        // Fix the Zooming-Out bug
        public void LateUpdate()
        {
            if (HighLogic.LoadedSceneHasPlanetarium && MapView.fetch != null && !isDone)
            {
                // Fix the bug via switching away from Home and back immideatly. 
                PlanetariumCamera.fetch.SetTarget(PlanetariumCamera.fetch.targets[(PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) + 1) % PlanetariumCamera.fetch.targets.Count]);
                PlanetariumCamera.fetch.SetTarget(PlanetariumCamera.fetch.targets[(PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) - 1) + (((PlanetariumCamera.fetch.targets.IndexOf(PlanetariumCamera.fetch.target) - 1) >= 0) ? 0 : PlanetariumCamera.fetch.targets.Count)]);
                
                // Terminate for the moment.
                isDone = true;
            }
        }
    }
}