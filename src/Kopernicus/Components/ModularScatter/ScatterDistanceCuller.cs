using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Kopernicus.Components.ModularScatter
{
    [RequireComponent(typeof(MeshRenderer))]
    class ScatterDistanceCuller : MonoBehaviour
    {
        private Boolean init = false;
        private int counter = 0;
        private PQSMod_LandClassScatterQuad surfaceObjectQuad;
        MeshRenderer[] surfaceObjects;
        private int maxdistance = 1;
        private void Start()
        {

        }
        private void Update()
        {
            //Rate Limit it to doing a cull-calculation every 60 frames, which should be plenty since we don't update more anyways.  These are very heavy.
            counter++;
            if (counter > 60)
            {
                if (!init)
                {
                    init = true;
                    maxdistance = Kopernicus.RuntimeUtility.RuntimeUtility.KopernicusConfig.ScatterCullDistance;
                }
                surfaceObjectQuad = GetComponentInParent<PQSMod_LandClassScatterQuad>();
                surfaceObjects = surfaceObjectQuad.GetComponentsInChildren<MeshRenderer>(true);
                counter = 0;
                int distance = 15000;
                if (HighLogic.LoadedSceneIsFlight)
                {
                    distance = (int)Vector3.Distance(Camera.current.transform.position, surfaceObjectQuad.transform.position);
                }
                else
                {
                    distance = 0;
                }
                for (int i = 0; i < surfaceObjects.Length; i++)
                {
                    MeshRenderer surfaceObject = surfaceObjects[i];
                    if (distance > maxdistance)
                    {
                        surfaceObject.enabled = false;
                    }
                    else
                    {
                        surfaceObject.enabled = true;
                    }
                }
            }
            else
            {
                return;
            }
        }
    }
}
