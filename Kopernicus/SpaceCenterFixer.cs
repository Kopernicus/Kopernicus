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
            return;
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsEditor)
            {
                FixCameras();
                FixSpaceCenterMain();
            }
        }
        protected void FixSpaceCenterMain()
        {
            SpaceCenterMain[] mains = Resources.FindObjectsOfTypeAll<SpaceCenterMain>();
            if (mains != null && mains.Length > 0)
            {
                foreach (SpaceCenterMain m in mains)
                {
                    List<string> newlist = new List<string>();
                    foreach (string s in m.gameObjectsToDisable)
                    {
                        newlist.Add(s.Replace("Kerbin", pqsName));
                    }
                    m.gameObjectsToDisable = newlist;
                }
            }
            else
                Debug.Log("[Kopernicus]: No objects of type SpaceCenterMain found to fix");
        }
        protected void FixCameras()
        {
            SpaceCenterCamera[] cams = Resources.FindObjectsOfTypeAll<SpaceCenterCamera>();
            Type camType = typeof(SpaceCenterCamera);
            if (cams != null && cams.Length > 0)
            {
                foreach (SpaceCenterCamera cam in cams)
                {
                    cam.pqsName = pqsName;
                    camType.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(cam, null);
                    cam.ResetCamera();
                }
            }
            else
            {
                Debug.Log("[Kopernicus]: no cameras of type SpaceCenterCamera");
            }


            SpaceCenterCamera2[] cams2 = Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>();
            Type camType2 = typeof(SpaceCenterCamera2);
            PQSCity ksc = SpaceCenter.Instance.transform.parent.GetComponent<PQSCity>();
            double altitudeInitial = 0d;
            bool resetHeight = false;
            if (ksc != null)
            {
                resetHeight = true;
                if (ksc.repositionToSphere || ksc.repositionToSphereSurface)
                {
                    double nomHeight = ksc.sphere.GetSurfaceHeight((Vector3d)ksc.repositionRadial.normalized) - ksc.sphere.radius;
                    if (ksc.repositionToSphereSurface)
                    {
                        nomHeight += ksc.repositionRadiusOffset;
                    }
                    altitudeInitial = -nomHeight;
                }
                else
                {
                    altitudeInitial = -ksc.repositionRadiusOffset;
                }
            }
            if (cams2 != null && cams2.Length > 0)
            {
                Debug.Log("[Kopernicus]: Found " + cams2.Length + " cameras of type SpaceCenterCamera2. Fixing.");
                foreach (SpaceCenterCamera2 cam in cams2)
                {
                    cam.pqsName = pqsName;
                    if (resetHeight)
                    {
                        //cam.altitudeInitial = (float)altitudeInitial;
                        cam.altitudeInitial = 10f;
                    }
                    camType2.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(cam, null);
                }
            }
            else
            {
                Debug.Log("[Kopernicus]: no cameras of type SpaceCenterCamera2");
            }
        }
    }
}
