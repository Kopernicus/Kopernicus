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

        // Overload the stock LateUpdate function
        private void LateUpdate()
        {
            Vector3d position = target.position;
            sunDirection = (position - ScaledSpace.LocalToScaledSpace(sun.position)).normalized;
            transform.forward = sunDirection;
            sunFlare.brightness = brightnessMultiplier *
                                  brightnessCurve.Evaluate(
                                      (Single)(1.0 / (Vector3d.Distance(position,
                                                           ScaledSpace.LocalToScaledSpace(sun.position)) /
                                                       (AU * ScaledSpace.InverseScaleFactor))));
            sunFlare.enabled = true;

            // The stock code does a lot of work here to calculate obstruction
            // however it excludes bodies that have localscale <1 or >3, which is pretty much everything but jool
            // (scaled space bodies seem to be 1000 scaled-space units in radius and then scaled with localscale (jool is scale 1))
            // There must be some other system (maybe stock unity behaviors?) that is handling occlusion, because it still works correctly for the skipped bodies - it just fades out instead of going out immediately
            // The stock code is extremely slow for systems with lots of stars and map targets (it iterates over vessels too!) so we get a massive speed boost by just skipping it and letting the unity system handle it
        }
    }
}
