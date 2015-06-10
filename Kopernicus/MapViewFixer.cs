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

        public void Awake()
        {
            if (HighLogic.LoadedSceneHasPlanetarium && PlanetariumCamera.fetch != null)
            {
                try
                {
                    PlanetariumCamera.fetch.SetTarget("Earth");
                }
                catch (Exception e)
                {
                    Debug.Log("[Kopernicus]: MapView fixing failed: " + e.Message);
                }
            }
        }

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
    }
}