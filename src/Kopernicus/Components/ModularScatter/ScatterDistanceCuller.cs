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
        private static int maxdistance = -1;
        private bool isEnabled = true;
        private void FixedUpdate()
        {
            //Rate Limit it to doing a cull-calculation every 5 physics-second frames, which should be plenty.  These are very heavy.
            counter++;
            if (counter > 250)
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
                if ((distance > maxdistance) && (isEnabled == false))
                {
                    return;
                }
                if ((distance <= maxdistance) && (isEnabled == true))
                {
                    return;
                }
                if (distance > maxdistance)
                {
                    surfaceObjectQuad.SendMessage("SetActive", false);
                    isEnabled = false;
                }
                else
                {
                    KopernicusSurfaceObject[] kopSurfaceObjects = surfaceObjectQuad.GetComponentsInChildren<KopernicusSurfaceObject>(true);
                    for (int i = 0; i < kopSurfaceObjects.Length; i++)
                    {
                        MeshRenderer surfaceObject = kopSurfaceObjects[i].GetComponent<MeshRenderer>();
                        surfaceObject.enabled = true;
                        isEnabled = true;
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
