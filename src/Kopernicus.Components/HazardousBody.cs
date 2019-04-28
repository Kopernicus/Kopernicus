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
using System.Collections.Generic;
using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// Regional heating for planet surfaces
        /// </summary>
        public class HazardousBody : MonoBehaviour
        {
            /// <summary>
            /// The average heat rate on the surface of the body. Gets multiplied by latitude, longitude and altitude curves
            /// </summary>
            public Double HeatRate;

            /// <summary>
            /// How many seconds should pass between applying the heat rate to the ship.
            /// </summary>
            public Single HeatInterval = 0.05f;

            /// <summary>
            /// Controls the heat rate at a certain latitude
            /// </summary>
            public FloatCurve LatitudeCurve;

            /// <summary>
            /// Controls the heat rate at a certain longitude
            /// </summary>
            public FloatCurve LongitudeCurve;

            /// <summary>
            /// Controls the heat rate at a certain altitude
            /// </summary>
            public FloatCurve AltitudeCurve;

            /// <summary>
            /// Controls the amount of heat that is applied on each spot of the planet
            /// </summary>
            public MapSO HeatMap;

            private CelestialBody _body;
            
            /// <summary>
            /// Get the body
            /// </summary>
            void Start()
            {
                _body = GetComponent<CelestialBody>();
                
                StartCoroutine(Worker());
            }

            void OnLevelWasLoaded(Int32 level)
            {
                StartCoroutine(Worker());
            }

            /// <summary>
            /// Update the heat
            /// </summary>
            /// <returns></returns>
            private IEnumerator<WaitForSeconds> Worker()
            {
                while (true)
                {
                    if (!FlightGlobals.ready)
                    {
                        yield return new WaitForSeconds(HeatInterval);
                        continue;
                    }

                    // Get all vessels
                    List<Vessel> vessels = FlightGlobals.Vessels.FindAll(v => v.mainBody == _body);

                    // Loop through them
                    foreach (Vessel vessel in vessels)
                    {
                        Double altitude =
                            AltitudeCurve.Evaluate((Single)Vector3d.Distance(vessel.transform.position, _body.transform.position));
                        Double latitude = LatitudeCurve.Evaluate((Single)vessel.latitude);
                        Double longitude = LongitudeCurve.Evaluate((Single)vessel.longitude);
                        
                        Double heat = altitude * latitude * longitude * HeatRate;
                        if (HeatMap != null)
                        {
                            heat *= HeatMap.GetPixelFloat((longitude + 180) / 360f, (latitude + 90) / 180f);
                        }
                        foreach (Part part in vessel.Parts)
                            part.temperature += heat * Time.deltaTime;
                    }
                    
                    // Wait
                    yield return new WaitForSeconds(HeatInterval);
                }
                // ReSharper disable once IteratorNeverReturns
            }
        }
    }
}
