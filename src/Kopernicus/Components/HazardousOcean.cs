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

namespace Kopernicus.Components
{
    /// <summary>
    /// Class to create a dangerous ocean that destroys ships
    ///
    /// Obsolete. Nothing to see here. Look at the shiny new things in HazardousBody instead.
    /// </summary>
    public class HazardousOcean : MonoBehaviour
    {
        // The amount of added heat per altitude
        public FloatCurve heatCurve;

        // The body we're attached to
        private CelestialBody _body;

        // The lava ocean
        private PQS _ocean;

        /// <summary>
        /// Get the body
        /// </summary>
        private void Start()
        {
            _body = Part.GetComponentUpwards<CelestialBody>(gameObject);
            _ocean = GetComponent<PQS>();
        }

        /// <summary>
        /// Update the heat. Heating is physics phenomenon so do it in the physics loop.
        /// </summary>
        public void FixedUpdate()
        {
            if (!FlightGlobals.ready)
            {
                return;
            }

            // Get all vessels
            List<Vessel> vessels = FlightGlobals.Vessels.FindAll(v => v.mainBody == _body);

            // Loop through them
            foreach (Vessel vessel in vessels)
            {
                Vector3 position = vessel.transform.position;
                Double distanceToPlanet =
                    Math.Abs(Vector3d.Distance(position, _body.transform.position)) -
                    _ocean.GetSurfaceHeight(_ocean.GetRelativePosition(position));
                Double heatingRate = heatCurve.Evaluate((Single) distanceToPlanet);
                foreach (Part part in vessel.Parts)
                {
                    // Multiplying by 50 to counteract the effect of multiplying by physics delta-time (1/50 by
                    // default). The heating rate is per frame, not per second, for legacy compatibility.
                    part.temperature += heatingRate * TimeWarp.fixedDeltaTime * 50;
                }
            }
        }
    }
}
