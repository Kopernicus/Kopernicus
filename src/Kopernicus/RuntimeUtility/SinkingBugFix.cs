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
        internal static Dictionary<Collider, bool>[] colliderStatus;
        internal uint counter = 1500;
        private static SinkingBugFix instance = null;
        private static bool safeToJustDisengage = false;

        private void Start()
        {
            if ((instance != null) || ((!HighLogic.LoadedSceneIsFlight) && (!HighLogic.LoadedScene.Equals(GameScenes.MAINMENU))))
            {
                instance = null;
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
                    colliderStatus = new Dictionary<Collider, bool>[FlightGlobals.Bodies.Count];
                    for (Int32 i = 0; i < FlightGlobals.Bodies.Count; i++)
                    {
                        colliderStatus[i] = new Dictionary<Collider, bool>();
                        RecordColliderState(FlightGlobals.Bodies[i], i);
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
                if (counter > 1500)
                {
                    counter = 0;
                    if (FlightGlobals.ActiveVessel != null)
                    {
                        if ((FlightGlobals.ActiveVessel.radarAltitude > 2500) || (!FlightGlobals.currentMainBody.hasSolidSurface))
                        {
                            if (safeToJustDisengage == false)
                            {
                                ReenableAll();
                                safeToJustDisengage = true;
                                return;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    safeToJustDisengage = false;
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
                                RestoreColliderState(i);
                            }
                            else if (Vector3.Distance(sceneCenter, cb.transform.position) >= 100000000000)
                            {
                                HibernateColliderState(i);
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
        private void RestoreColliderState(int index)
        {
            foreach (Collider collider in colliderStatus[index].Keys)
            {
                collider.enabled = colliderStatus[index][collider];
                colliderStatus[index].Remove(collider);
            }
        }
        private void HibernateColliderState(int index)
        {
            foreach (Collider collider in colliderStatus[index].Keys)
            {
                colliderStatus[index][collider] = collider.enabled;
                collider.enabled = false;
            }
        }
        private void RecordColliderState(CelestialBody cb, int index)
        {
            foreach (Collider collider in cb.GetComponentsInChildren<Collider>(true))
            {
                if ((!colliderStatus[index].ContainsKey(collider)))
                {
                    colliderStatus[index].Add(collider, collider.enabled);
                }
                else
                {
                    colliderStatus[index][collider] = collider.enabled;
                }
            }
            foreach (Collider collider in cb.scaledBody.GetComponentsInChildren<Collider>(true))
            {
                if (!colliderStatus[index].ContainsKey(collider))
                {
                    colliderStatus[index].Add(collider, collider.enabled);
                }
                else
                {
                    colliderStatus[index][collider] = collider.enabled;
                }
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
                        RestoreColliderState(i);
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
