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
        private int counter = 0;
        private PQSMod_LandClassScatterQuad surfaceObjectQuad;
        KopernicusSurfaceObject[] kopSurfaceObjects;
        private static int maxdistance = -1;
        private void Update()
        {
            //Rate Limit it to doing a cull-calculation every 10 physics-second frames, which should be plenty.  These are very heavy.
            counter++;
            if (counter > 500)
            {
                counter = 0;
                if (maxdistance == -1)
                {
                    maxdistance = Kopernicus.RuntimeUtility.RuntimeUtility.KopernicusConfig.ScatterCullDistance;
                }
                //if 0 abort.
                if (maxdistance == 0)
                {
                    return;
                }
                surfaceObjectQuad = GetComponentInParent<PQSMod_LandClassScatterQuad>();
                kopSurfaceObjects = surfaceObjectQuad.GetComponentsInChildren<KopernicusSurfaceObject>(true);
                if (kopSurfaceObjects.Length == 0)
                {
                    return;
                }
                int distance = 15000;
                if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel)
                {
                    distance = (int)Vector3.Distance(FlightGlobals.ActiveVessel.transform.position, surfaceObjectQuad.transform.position);
                }
                else
                {
                    distance = 0;
                }
                //Optimization checks
                if ((distance > maxdistance) && (kopSurfaceObjects[0].GetComponent<MeshRenderer>().enabled == false))
                {
                    return;
                }
                if ((distance <= maxdistance) && (kopSurfaceObjects[0].GetComponent<MeshRenderer>().enabled == true))
                {
                    return;
                }
                for (int i = 0; i < kopSurfaceObjects.Length; i++)
                {
                    MeshRenderer surfaceObject = kopSurfaceObjects[i].GetComponent<MeshRenderer>();
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
