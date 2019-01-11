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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// Modifications for the SunFlare component
        /// </summary>
        public class KopernicusSunFlare : SunFlare
        {
            protected override void Awake()
            {
                Camera.onPreCull += OnPreCull;
            }

            void OnPreCull(Camera camera)
            {
                Vector3d scaledSpace = target.transform.position - ScaledSpace.LocalToScaledSpace(sun.position);
                sunDirection = scaledSpace.normalized;
                if (sunDirection != Vector3d.zero)
                    transform.forward = sunDirection;
            }

            protected override void OnDestroy()
            {
                Camera.onPreCull -= OnPreCull;
                base.OnDestroy();
            }
            
            // Overload the stock LateUpdate function
            void LateUpdate()
            {
                Vector3d position = target.position;
                sunDirection = (position - ScaledSpace.LocalToScaledSpace(sun.position)).normalized;
                transform.forward = sunDirection;
                sunFlare.brightness = brightnessMultiplier *
                                      brightnessCurve.Evaluate(
                                          (Single) (1.0 / (Vector3d.Distance(position,
                                                               ScaledSpace.LocalToScaledSpace(sun.position)) /
                                                           (AU * ScaledSpace.InverseScaleFactor))));
                
                if (PlanetariumCamera.fetch.target != null && (HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.FLIGHT))
                {
                    Boolean state = true;
                    for (Int32 index = 0; index < PlanetariumCamera.fetch.targets.Count; index++)
                    {
                        MapObject mapTarget = PlanetariumCamera.fetch.targets[index];
                        if (mapTarget.type != MapObject.ObjectType.CelestialBody)
                        {
                            continue;
                        }

                        if (mapTarget.GetComponent<SphereCollider>() == null)
                        {
                            continue;
                        }

                        if (!mapTarget.GetComponent<MeshRenderer>().enabled)
                        {
                            continue;
                        }

                        if (mapTarget.celestialBody == sun)
                        {
                            continue;
                        }
                        
                        Vector3d targetDistance = PlanetariumCamera.fetch.transform.position - mapTarget.transform.position;
                        Single radius = mapTarget.GetComponent<SphereCollider>().radius;
                        Double num1 = 2.0 * Vector3d.Dot(-sunDirection, targetDistance);
                        Double num2 = Vector3d.Dot(targetDistance, targetDistance) - radius * (Double) radius;
                        Double d = num1 * num1 - 4.0 * num2;
                        if (d >= 0.0)
                        {
                            Double num3 = (-num1 + Math.Sqrt(d)) * 0.5;
                            Double num4 = (-num1 - Math.Sqrt(d)) * 0.5;
                            if (num3 >= 0.0 && num4 >= 0.0)
                                state = false;
                        }
                    }

                    SunlightEnabled(state);
                }
            }
        }
    }
}