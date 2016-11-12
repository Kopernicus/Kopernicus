/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */
 
 using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// Class to create a dangerous ocean that destroys ships
        /// </summary>
        public class HazardousOcean : MonoBehaviour
        {
            /// The amount of added heat per altitude
            public FloatCurve heatCurve;

            /// The body we're attached to
            private CelestialBody body;

            /// The lava ocean
            private PQS ocean;

            /// <summary>
            /// Get the body
            /// </summary>
            void Start()
            {
                body = Part.GetComponentUpwards<CelestialBody>(gameObject);
                ocean = GetComponent<PQS>();
            }

            /// <summary>
            /// Update the heat
            /// </summary>
            public void Update()
            {
                if (!FlightGlobals.ready)
                    return;

                // Get all vessels
                List<Vessel> vessels = FlightGlobals.Vessels.FindAll(v => v.mainBody == body);

                // Loop through them
                foreach (Vessel vessel in vessels)
                {
                    double distanceToPlanet = Math.Abs(Vector3d.Distance(vessel.transform.position, body.transform.position)) - ocean.GetSurfaceHeight(ocean.GetRelativePosition(vessel.transform.position));
                    double heatingRate = heatCurve.Evaluate((float)distanceToPlanet);
                    foreach (Part part in vessel.Parts)
                        part.temperature += heatingRate;
                }
            }
        }
    }
}