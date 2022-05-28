using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using KSP.IO;

namespace Kopernicus.ShadowMan
{
    public class ReflectionProbeChecker : MonoBehaviour
    {
        Dictionary<Camera,ReflectionProbeFixer> camToFixer =  new Dictionary<Camera,ReflectionProbeFixer>() ;

        public ReflectionProbeChecker()
        {
        }

        public void OnUpdate()
        {
            gameObject.transform.position = ShadowMan.Instance.nearCamera.transform.position + (ShadowMan.Instance.nearCamera.transform.forward * -5000f);
        }

        public void OnWillRenderObject()
        {
            Camera cam = Camera.current;
            if (!cam)
                return;

            if (!camToFixer.ContainsKey(cam))
            {
                if (cam.name == "Reflection Probes Camera")
                {
                    camToFixer[cam] = (ReflectionProbeFixer)cam.gameObject.AddComponent(typeof(ReflectionProbeFixer));

                    //Grab the reflection probe and set it's distance to 100km so reflections no longer disappear when using cameraTools stationary camera
                    ReflectionProbe[] probes = Resources.FindObjectsOfTypeAll<ReflectionProbe> ();
                    foreach (ReflectionProbe _probe in probes)
                    {
                        _probe.size = new Vector3(100000f, 100000f, 100000f);
                    }

                    Utils.LogDebug("Added reflection probe fixer to " + cam.name);
                }
                else
                {
                    //we add it anyway to avoid doing a string compare
                    camToFixer[cam] = null;
                }
            }

        }

        public void OnDestroy()
        {
            if (camToFixer.Count != 0)
            {
                foreach (var _val in camToFixer.Values)
                {
                    if (!ReferenceEquals(_val, null))
                    {
                        Component.Destroy(_val);
                        UnityEngine.Object.Destroy(_val);
                    }
                }
                camToFixer.Clear();
            }
        }
    }
}

