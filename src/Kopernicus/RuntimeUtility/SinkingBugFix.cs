/**
 * Kopernicus Planetary System Modifier
 * ------------------------------------------------------------- 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 * 
 * This library is intended to be used as a plugin for Kerbal Space Program
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kopernicus.RuntimeUtility
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class SinkingBugFix : MonoBehaviour
    {
        internal static Dictionary<int, bool>[] colliderStatus;
        internal uint counter = 25;
        private static SinkingBugFix instance = null;

        private void Start()
        {
            if ((instance != null) || (HighLogic.LoadedScene.Equals(GameScenes.TRACKSTATION)))
            {
                this.gameObject.DestroyGameObjectImmediate();
                return;
            }
            else
            {
                instance = this;
            }
            if (RuntimeUtility.KopernicusConfig.DisableFarAwayColliders)
            {
                try
                {
                    colliderStatus = new Dictionary<int, bool>[FlightGlobals.Bodies.Count];
                    for (Int32 i = 0; i < FlightGlobals.Bodies.Count; i++)
                    {
                        colliderStatus[i] = new Dictionary<int, bool>();
                    }
                }
                catch
                {
                    return;
                }
            }
        }
        private void FixedUpdate()
        {
            if (RuntimeUtility.KopernicusConfig.DisableFarAwayColliders)
            {
                if (HighLogic.LoadedScene.Equals(GameScenes.SPACECENTER))
                {
                    this.gameObject.DestroyGameObjectImmediate();
                    return;
                }
                else if (HighLogic.LoadedScene.Equals(GameScenes.MAINMENU))
                {
                    try
                    {
                        FloatingOrigin.fetch.ResetOffset();
                    }
                    catch
                    { 
                    }
                    this.gameObject.DestroyGameObjectImmediate();
                    return;
                }
                CelestialBody mainBody = null;
                counter++;
                if (counter > 25)
                {
                    counter = 0;
                    Vector3 sceneCenter = Vector3.zero;
                    try
                    {
                        if (FlightGlobals.Bodies.Count > 1)
                        {
                            mainBody = FlightGlobals.currentMainBody;
                            sceneCenter = mainBody.transform.position;
                        }
                    }
                    catch
                    {
                        sceneCenter = Vector3.zero;
                    }
                    try
                    {
                        for (Int32 i = 0; i < FlightGlobals.Bodies.Count; i++)
                        {
                            CelestialBody cb = FlightGlobals.Bodies[i];
                            if ((cb.Get("barycenter", false) || (cb.Get("invisibleScaledSpace", false))))
                            {
                                continue;
                            }
                            else if (Vector3.Distance(sceneCenter, cb.transform.position) < 100000000000)
                            {
                                RestoreColliderState(cb, i);
                            }
                            else if (Vector3.Distance(sceneCenter, cb.transform.position) >= 100000000000)
                            {
                                HibernateColliderState(cb, i);
                            }
                        }
                    }
                    catch
                    {
                        //doesn't work in this scene, return
                        return;
                    }
                }
            }
        }
        private void RestoreColliderState(CelestialBody cb, int index)
        {
            foreach (Collider collider in cb.GetComponentsInChildren<Collider>(true))
            {
                if (colliderStatus[index].ContainsKey(collider.GetInstanceID()))
                {
                    collider.enabled = colliderStatus[index][collider.GetInstanceID()];
                    colliderStatus[index].Remove(collider.GetInstanceID());
                }
            }
            foreach (Collider collider in cb.scaledBody.GetComponentsInChildren<Collider>(true))
            {
                if (colliderStatus[index].ContainsKey(collider.GetInstanceID()))
                {
                    collider.enabled = colliderStatus[index][collider.GetInstanceID()];
                    colliderStatus[index].Remove(collider.GetInstanceID());
                }
            }
        }
        private void HibernateColliderState(CelestialBody cb, int index)
        {
            foreach (Collider collider in cb.GetComponentsInChildren<Collider>(true))
            {
                if ((!colliderStatus[index].ContainsKey(collider.GetInstanceID())))
                {
                    colliderStatus[index].Add(collider.GetInstanceID(), collider.enabled);
                }
                collider.enabled = false;
            }
            foreach (Collider collider in cb.scaledBody.GetComponentsInChildren<Collider>(true))
            {
                if (!colliderStatus[index].ContainsKey(collider.GetInstanceID()))
                {
                    colliderStatus[index].Add(collider.GetInstanceID(), collider.enabled);
                }
                collider.enabled = false;
            }
        }
        private void OnDisable()
        {
            ReenableAll();
        }

        private void OnDestroy()
        {
            ReenableAll();
        }

        private void ReenableAll()
        {
            try
            {
                for (Int32 i = 0; i < FlightGlobals.Bodies.Count; i++)
                {
                    CelestialBody cb = FlightGlobals.Bodies[i];
                    if ((cb.Get("barycenter", false) || (cb.Get("invisibleScaledSpace", false))))
                    {
                        continue;
                    }
                    try
                    {
                        RestoreColliderState(cb, i);
                    }
                    catch
                    {
                        //Guess we couldn't do that?  Keep trying anyways...
                    }
                }
            }
            catch
            {
                //must be time to stop...
            }
        }
    }
}
