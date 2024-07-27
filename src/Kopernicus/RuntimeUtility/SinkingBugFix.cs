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

using System.Collections.Generic;
using UnityEngine;

namespace Kopernicus.RuntimeUtility
{
    /// <summary>
    /// Workaround for PhysX/Unity bug causing raycasts to start failing when any collider is enabled at a distance further than 1e7 from the origin.
    /// This notably result in wheels and landing legs falling through the ground, but can have other side effects like solar panels not being occluded.
    /// This happens more consistently the further away colliders are, with a near 100% reproduction rate at around 1e15.
    /// The workaround is to check the distance of every body, and if that body is further than 1e7, query for all potential colliders parented to it,
    /// and disable them. We only check for colliders when the distance threshold is reached, as this is a slow operation.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    [DefaultExecutionOrder(-105)]
    public class SinkingBugFix : MonoBehaviour
    {
        private const double ENABLE_DIST = 1e7;
        private const double DISABLE_DIST = ENABLE_DIST + 1e6;

        private static List<Collider> colliderBuffer = new List<Collider>();

        private BodyColliderTracker[] bodies;

        void Start()
        {
            switch (HighLogic.LoadedScene)
            {
                case GameScenes.MAINMENU:
                case GameScenes.SPACECENTER:
                case GameScenes.EDITOR:
                case GameScenes.TRACKSTATION:
                case GameScenes.FLIGHT:
                    break;
                default:
                    DestroyImmediate(gameObject);
                    return;
            }

            if (!RuntimeUtility.KopernicusConfig.DisableFarAwayColliders)
            {
                DestroyImmediate(gameObject);
                return;
            }

            bodies = new BodyColliderTracker[FlightGlobals.Bodies.Count];

            for (int i = FlightGlobals.Bodies.Count; i-- > 0;)
                bodies[i] = new BodyColliderTracker(FlightGlobals.Bodies[i]);
        }

        void FixedUpdate()
        {
            foreach (BodyColliderTracker bodyColliders in bodies)
                bodyColliders.CheckDistancesAndSetCollidersState();
        }

        void OnDestroy()
        {
            if (bodies != null)
                foreach (BodyColliderTracker bodyColliders in bodies)
                    bodyColliders.RestoreStateOnDestroy();
        }

        private class BodyColliderTracker
        {
            private CelestialBody body;
            private Collider scaledSpaceCollider;
            private List<Collider> disabledColliders;
            private bool localEnabled = true;
            private bool scaledEnabled = true;
            private bool hasScaled;

            public BodyColliderTracker(CelestialBody body)
            {
                this.body = body;
                scaledSpaceCollider = body.scaledBody.GetComponentInChildren<Collider>();
                hasScaled = scaledSpaceCollider.IsNotNullRef();
            }

            public void CheckDistancesAndSetCollidersState()
            {
                double localDistFromOrigin = body.position.magnitude;
                if (localEnabled && localDistFromOrigin > DISABLE_DIST)
                {
                    localEnabled = false;
                    body.GetComponentsInChildren(true, colliderBuffer);

                    if (disabledColliders == null)
                        disabledColliders = new List<Collider>(colliderBuffer.Count);

                    for (int i = colliderBuffer.Count; i-- > 0;)
                    {
                        Collider collider = colliderBuffer[i];
                        if (collider.enabled)
                        {
                            disabledColliders.Add(collider);
                            collider.enabled = false;
                        }
                    }

                    colliderBuffer.Clear();
                }
                else if (!localEnabled && localDistFromOrigin < ENABLE_DIST)
                {
                    localEnabled = true;
                    for (int i = disabledColliders.Count; i-- > 0;)
                    {
                        Collider collider = disabledColliders[i];
                        if (!collider.IsDestroyed())
                            collider.enabled = true;
                    }

                    disabledColliders.Clear();
                }

                if (hasScaled)
                {
                    double scaledDistFromOrigin = scaledSpaceCollider.transform.position.magnitude;
                    if (scaledEnabled && scaledDistFromOrigin > DISABLE_DIST)
                    {
                        scaledEnabled = false;
                        scaledSpaceCollider.enabled = false;
                    }
                    else if (!scaledEnabled && scaledDistFromOrigin <= ENABLE_DIST)
                    {
                        scaledEnabled = true;
                        scaledSpaceCollider.enabled = true;
                    }
                }
            }

            public void RestoreStateOnDestroy()
            {
                if (!localEnabled)
                {
                    for (int i = disabledColliders.Count; i-- > 0;)
                    {
                        Collider collider = disabledColliders[i];
                        if (collider.IsDestroyed())
                            continue;

                        collider.enabled = true;
                    }
                }

                if (hasScaled && !scaledEnabled)
                {
                    scaledSpaceCollider.enabled = true;
                }
            }
        }
    }
}
