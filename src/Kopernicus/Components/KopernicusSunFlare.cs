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
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Kopernicus.Components
{
    /// <summary>
    /// Modifications for the SunFlare component
    /// </summary>
    public class KopernicusSunFlare : SunFlare
    {
        protected override void Awake()
        {
            Camera.onPreCull += PreCull;
        }

        [SuppressMessage("ReSharper", "Unity.IncorrectMethodSignature")]
        private void PreCull(Camera camera)
        {
            Vector3d scaledSpace = target.transform.position - ScaledSpace.LocalToScaledSpace(sun.position);
            sunDirection = scaledSpace.normalized;
            if (sunDirection != Vector3d.zero)
            {
                transform.forward = sunDirection;
            }
        }

        [SuppressMessage("ReSharper", "DelegateSubtraction")]
        protected override void OnDestroy()
        {
            Camera.onPreCull -= PreCull;
            base.OnDestroy();
        }

        private double CheckRaySphereIntersection(Vector3d rayDir, Vector3d offset, double radius)
        {
            double dir = Vector3d.Dot(rayDir, offset);
            Vector3d ClosestPoint = offset - Math.Max(dir, 0d) * rayDir;
            return ClosestPoint.magnitude - radius;
        }

        private float CheckAtmoDensity(CelestialBody b, float density, Vector3d cameraPosition)
        {
            MapObject mapTarget = b.MapObject;
            if (mapTarget == null)
                return density;

            if (!mapTarget.GetComponent<MeshRenderer>().enabled)
                return density;

            if (mapTarget.celestialBody == sun)
            {
                foreach (CelestialBody n in b.orbitingBodies)
                    density += CheckAtmoDensity(n, 0f, cameraPosition);
                return density;
            }

            Vector3d targetDistance = PlanetariumCamera.fetch.transform.position - mapTarget.transform.position;
            if (b.atmosphere)
            {
                float altitude = (float)CheckRaySphereIntersection(cameraPosition, targetDistance * ScaledSpace.ScaleFactor, b.Radius);
                FloatCurve curve = b.atmospherePressureCurve;
                density += curve.Evaluate(Mathf.Clamp(altitude, curve.minTime, curve.maxTime));
            }
            
            if (b.orbitingBodies.Count != 0)
            {
                if (CheckRaySphereIntersection(cameraPosition, targetDistance * ScaledSpace.ScaleFactor, b.sphereOfInfluence) > 0)
                    return density;

                foreach (CelestialBody n in b.orbitingBodies)
                    density += CheckAtmoDensity(n, 0f, cameraPosition);
            }
            return density;
        }

        private void AtmosphericScattering()
        {
            Vector3d cameraPosition = (PlanetariumCamera.fetch.transform.position - sun.transform.position).normalized;
            float density = CheckAtmoDensity(FlightGlobals.Bodies[0], 0f, cameraPosition);
            density = Mathf.Sqrt(density / 150f);
            float r = Mathf.Exp(.1f - density * .3f);
            float g = Mathf.Exp(.1f - density * 1.1f);
            float b = Mathf.Exp(.1f - density * 2.6f);
            sunFlare.color = new Color(r, g, b);
        }

        // Overload the stock LateUpdate function
        private void LateUpdate()
        {
            sunFlare.fadeSpeed = 10000f;
            Vector3d position = target.position;
            sunDirection = (position - ScaledSpace.LocalToScaledSpace(sun.position)).normalized;
            transform.forward = sunDirection;
            sunFlare.brightness = brightnessMultiplier *
                                  brightnessCurve.Evaluate(
                                      (Single)(1.0 / (Vector3d.Distance(position,
                                                           ScaledSpace.LocalToScaledSpace(sun.position)) /
                                                       (AU * ScaledSpace.InverseScaleFactor))));
            sunFlare.enabled = true;
            
            if (PlanetariumCamera.fetch.target == null ||
                HighLogic.LoadedScene != GameScenes.TRACKSTATION && HighLogic.LoadedScene != GameScenes.FLIGHT)
            {
                return;
            }

            if (RuntimeUtility.RuntimeUtility.KopernicusConfig.EnableAtmosphericExtinction)
                AtmosphericScattering();
        }
    }
}
