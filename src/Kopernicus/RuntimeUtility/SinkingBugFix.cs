using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kopernicus.RuntimeUtility
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SinkingBugFix : MonoBehaviour
    {
        internal static Dictionary<int, bool>[] colliderStatus = new Dictionary<int, bool>[FlightGlobals.Bodies.Count];
        internal uint counter = 0;

        private void Start()
        {
            if (RuntimeUtility.KopernicusConfig.DisableFarAwayColliders)
            {
                for (Int32 i = 0; i < FlightGlobals.Bodies.Count; i++)
                {
                    colliderStatus[i] = new Dictionary<int, bool>();
                }
            }
        }
        private void FixedUpdate()
        {
            if ((RuntimeUtility.KopernicusConfig.DisableFarAwayColliders) && (HighLogic.LoadedSceneIsFlight))
            {
                counter++;
                if (counter > 20)
                {
                    counter = 0;
                    CelestialBody mainBody = FlightGlobals.currentMainBody;
                    for (Int32 i = 0; i < FlightGlobals.Bodies.Count; i++)
                    {
                        CelestialBody cb = FlightGlobals.Bodies[i];
                        if ((cb.Get("barycenter", false) || (cb.Get("invisibleScaledSpace", false))))
                        {
                            continue;
                        }
                        else if (cb == FlightGlobals.currentMainBody || (Vector3.Distance(FlightGlobals.currentMainBody.transform.position, cb.transform.position) < 100000000000))
                        {
                            RestoreColliderState(cb, i);
                        }
                        else if (Vector3.Distance(FlightGlobals.currentMainBody.transform.position, cb.transform.position) > 100000000000)
                        {
                            HibernateColliderState(cb, i);
                        }
                    }
                }
            }
        }
        private void RestoreColliderState(CelestialBody cb, int index)
        {
            foreach (Collider collider in cb.GetComponentsInChildren<Collider>(true))
            {
                if (colliderStatus[index].ContainsKey(collider.gameObject.GetInstanceID()))
                {
                    collider.enabled = colliderStatus[index][collider.gameObject.GetInstanceID()];
                }
                else
                {
                    colliderStatus[index].Add(collider.gameObject.GetInstanceID(), collider.enabled);
                }
            }
        }
        private void HibernateColliderState(CelestialBody cb, int index)
        {
            foreach (Collider collider in cb.GetComponentsInChildren<Collider>(true))
            {
                if (!colliderStatus[index].ContainsKey(collider.gameObject.GetInstanceID()))
                {
                    colliderStatus[index].Add(collider.gameObject.GetInstanceID(), collider.enabled);
                }
                collider.enabled = false;
            }
        }
    }
}
