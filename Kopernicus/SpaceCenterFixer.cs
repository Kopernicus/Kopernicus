using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;

namespace Kopernicus
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class SpaceCenterFixer : MonoBehaviour
    {
        static public string pqsName = "Kerbin"; // will be changed to new homeworld in Injector.
        public void Start()
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsEditor)
            {
                FixCameras();
            }
        }
        protected void FixCameras()
        {
            // Get the parental body
            CelestialBody body = null;
            if (Planetarium.fetch != null)
                body = Planetarium.fetch.Home;
            else
                body = FlightGlobals.Bodies.Find(b => b.isHomeWorld);

            // If there's no body, exit.
            if (body == null)
            {
                Debug.Log("[Kopernicus]: Couldn't find the parental body!");
                return;
            }

            // Get the KSC object
            PQSCity ksc = body.pqsController.GetComponentsInChildren<PQSCity>(true).Where(m => m.name == "KSC").First();
            
            // If there's no KSC, exit.
            if (ksc == null)
            {
                Debug.Log("[Kopernicus]: Couldn't find the KSC object!");
                return;
            }

            // Go throug the SpaceCenterCameras and fix them
            foreach (SpaceCenterCamera2 cam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
            {
                if (ksc.repositionToSphere || ksc.repositionToSphereSurface)
                {
                    double normalHeight = body.pqsController.GetSurfaceHeight((Vector3d) ksc.repositionRadial.normalized) - body.Radius;
                    if (ksc.repositionToSphereSurface)
                    {
                        normalHeight += ksc.repositionRadiusOffset;
                    }
                    cam.altitudeInitial = 0f - (float) normalHeight;
                }
                else
                {
                    cam.altitudeInitial = 0f - (float) ksc.repositionRadiusOffset;
                }
                cam.GetType().GetMethod("OnDestroy", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(cam, null);
                cam.GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(cam, null);
                Debug.Log("[Kopernicus]: Fixed SpaceCenterCamera");
            }
        }
    }
}
