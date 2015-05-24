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
                
                // re-implement cam.Start()
                // fields
                Type camType = cam.GetType();
                FieldInfo camPQS = null;
                FieldInfo transform1 = null;
                FieldInfo transform2 = null;
                FieldInfo surfaceObj = null;

                // get fields
                FieldInfo[] fields = camType.GetFields();
                for (int i = 0; i < fields.Length; ++i)
                {
                    FieldInfo fi = fields[i];
                    if (fi.FieldType == typeof(PQS))
                        camPQS = fi;
                    else if (fi.FieldType == typeof(Transform) && transform1 == null)
                        transform1 = fi;
                    else if (fi.FieldType == typeof(Transform) && transform2 == null)
                        transform2 = fi;
                    else if (fi.FieldType == typeof(SurfaceObject))
                        surfaceObj = fi;
                }
                if (camPQS != null && transform1 != null && transform2 != null && surfaceObj != null)
                {
                    camPQS.SetValue(cam, body.pqsController);

                    Transform initialTransform = body.pqsController.transform.Find(cam.initialPositionTransformName);
                    if (initialTransform != null)
                    {
                        transform1.SetValue(cam, initialTransform);
                        cam.transform.NestToParent(initialTransform);
                    }
                    else
                    {
                        Debug.Log("SSC2 can't find initial transform!");
                        Transform initialTrfOrig = transform1.GetValue(cam) as Transform;
                        if(initialTrfOrig != null)
                            cam.transform.NestToParent(initialTrfOrig);
                        else
                            Debug.Log("SSC2 own initial transform null!");
                    }
                    Transform camTransform = transform2.GetValue(cam) as Transform;
                    if (camTransform != null)
                    {
                        camTransform.NestToParent(cam.transform);
                        if (FlightCamera.fetch != null && FlightCamera.fetch.transform != null)
                        {
                            FlightCamera.fetch.transform.NestToParent(camTransform);
                        }
                    }
                    else
                        Debug.Log("SSC2 cam transform null!");

                    cam.ResetCamera();

                    SurfaceObject so = surfaceObj.GetValue(cam) as SurfaceObject;
                    if (so != null)
                    {
                        so.ReturnToParent();
                        DestroyImmediate(so);
                    }
                    else
                        Debug.Log("SSC2 surfaceObject is null!");

                    surfaceObj.SetValue(cam, SurfaceObject.Create(initialTransform.gameObject, FlightGlobals.currentMainBody, 3, KFSMUpdateMode.FIXEDUPDATE));

                    Debug.Log("[Kopernicus]: Fixed SpaceCenterCamera");
                }
                else
                    Debug.Log("[Kopernicus]: ERROR fixing space center camera, could not find some fields");
            }
        }
    }
}
