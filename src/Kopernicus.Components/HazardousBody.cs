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
			
            // cache to avoid tons of garbage creation
            // otherwise every instance of HazardousBody will create and dispose of an instance of WaitForSeconds every time the loop iterates
            private WaitForSeconds Delay;
			
            private Single InverseInterval;
            
            /// <summary>
            /// Get the body
            /// </summary>
            void Start()
            {
                _body = GetComponent<CelestialBody>();
                Delay = new WaitForSeconds(HeatInterval);
                // Now we don't need to use division operators each frame because those are REALLY expensive.
                InverseInterval = 1f / HeatInterval;
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
                        yield return Delay;
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
                            // let's avoid division operators.
                            // 1f / 360f = ~0.002777778f
                            // 1f / 180f = ~0.005555556f
                            // not 100% precise but it's accurate enough to be an insignificant difference, yet it's a huge performance boost.
                            // especially if we take into account that this code will be run multiple times per frame depending on the amount of hazardous planets...
                            heat *= HeatMap.GetPixelFloat((longitude + 180) * 0.002777778f, (latitude + 90) * 0.005555556f);
                        }
                        foreach (Part part in vessel.Parts)
                            part.temperature += heat * InverseInterval;
                    }
                    
                    // Wait
                    yield return Delay;
                }
                // ReSharper disable once IteratorNeverReturns
            }
        }
    }
}

// rewritten to avoid coroutines due to their memory usage.
/* From Unity:
Calling StartCoroutine() creates a small amount of garbage, because of the classes that Unity must create instances of to manage the coroutine.
With that in mind, calls to StartCoroutine() should be limited while our game is interactive and performance is a concern.
To reduce garbage created in this way, any coroutines that must run at performance-critical times should be started in advance and we should be
particularly careful when using nested coroutines that may contain delayed calls to StartCoroutine().
*/

/*
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
			
            // How far is the script in the HeatInterval cycle?
            private Single IntervalPosition = 0f;
			
            // precompute the division: typically you can run 100 multiplications in the time it takes for one division operator to finish.
            // we might as well allocate 32 more bits per planet for the sake of performance.
            private Single Frequency;
            
            /// <summary>
            /// Get the body
            /// </summary>
            void Start()
            {
                _body = GetComponent<CelestialBody>();
				
                // precompute interval frequency
                Frequency = 1f / HeatInterval;
            }
			
            void Update()
            {
                // check how much time passed since the last routine
                if(IntervalPosition > HeatInterval)
                {
                    // update time: both the general progression of time (Time.deltaTime) as well as reset the counter by subtracting by HeatInterval
                    IntervalPosition += Time.deltaTime - HeatInterval;
                    
                    if (!FlightGlobals.ready)
                    {
                        // abort the routine here.
                        return;
                    }
                    // Get all vessels
                    List<Vessel> vessels = FlightGlobals.Vessels.FindAll(v => v.mainBody == _body);

                    // Loop through them
                    foreach (Vessel vessel in vessels)
                    {
                        Double altitude = AltitudeCurve.Evaluate((Single)Vector3d.Distance(vessel.transform.position, _body.transform.position));
                        Double latitude = LatitudeCurve.Evaluate((Single)vessel.latitude);
                        Double longitude = LongitudeCurve.Evaluate((Single)vessel.longitude);
                        
                        Double heat = altitude * latitude * longitude * HeatRate;
                        if (HeatMap != null)
                        {
                            //heat *= HeatMap.GetPixelFloat((longitude + 180) / 360f, (latitude + 90) / 180f);

                            // let's avoid division operators.
                            // 1f / 360f = ~0.002777778f
                            // 1f / 180f = ~0.005555556f
                            // not 100% precise but it's accurate enough to be an insignificant difference, yet it's a huge performance boost.
                            // especially if we take into account that this code will be run multiple times per frame depending on the amount of hazardous planets...
                            heat *= HeatMap.GetPixelFloat((longitude + 180) * 0.002777778f, (latitude + 90) * 0.005555556f);
                        }
                        foreach (Part part in vessel.Parts)
                            part.temperature += heat * Frequency;
                    }
                    // we already reset the counter.
                }
                else
                    IntervalPosition += Time.deltaTime; // progress the counter
            }
        }
    }
}
*/
