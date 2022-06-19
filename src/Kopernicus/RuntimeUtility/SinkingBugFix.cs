using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kopernicus.RuntimeUtility
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SinkingBugFix : MonoBehaviour
    {
        internal static Dictionary<int, bool>[] colliderStatus = new Dictionary<int, bool>[FlightGlobals.Bodies.Count];

        private void Start()
        {
            for (Int32 i = 0; i < FlightGlobals.Bodies.Count; i++)
            {
                colliderStatus[i] = new Dictionary<int, bool>();
            }
        }
        private void Update()
        {
            CelestialBody mainBody = FlightGlobals.currentMainBody;
            for (Int32 i = 0; i < FlightGlobals.Bodies.Count; i++)
            {
                CelestialBody cb = FlightGlobals.Bodies[i];
                if ((cb.Get("barycenter", false) || (cb.Get("invisibleScaledSpace", false))))
                {
                    continue;
                }
                if (cb == FlightGlobals.currentMainBody)
                {
                    RestoreColliderState(cb, i);
                }
                else if (Vector3.Distance(FlightGlobals.currentMainBody.transform.position, cb.transform.position) > 100000000000)
                {
                    HibernateColliderState(cb, i);
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
            return;
        }
        private void HibernateColliderState(CelestialBody cb, int index)
        {
            foreach (Collider collider in cb.GetComponentsInChildren<Collider>(true))
            {
                if (!colliderStatus[index].ContainsKey(collider.gameObject.GetInstanceID()))
                {
                    colliderStatus[index].Add(collider.gameObject.GetInstanceID(), collider.enabled);
                }
                else
                {
                    colliderStatus[index][collider.gameObject.GetInstanceID()] = collider.enabled;
                }
                collider.enabled = false;
            }
        }
    }
}
