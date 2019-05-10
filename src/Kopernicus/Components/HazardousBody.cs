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
    /// Regional heating for planet surfaces
    /// </summary>
    public class HazardousBody : MonoBehaviour
    {
        /// <summary>
        /// The average heat rate on the surface of the body. Gets multiplied by latitude, longitude and altitude curves
        /// </summary>
        public Double heatRate;

        /// <summary>
        /// How many seconds should pass between applying the heat rate to the ship.
        /// </summary>
        public Single heatInterval = 0.05f;

        /// <summary>
        /// Controls the heat rate at a certain latitude
        /// </summary>
        public FloatCurve latitudeCurve;

        /// <summary>
        /// Controls the heat rate at a certain longitude
        /// </summary>
        public FloatCurve longitudeCurve;

        /// <summary>
        /// Controls the heat rate at a certain altitude
        /// </summary>
        public FloatCurve altitudeCurve;

        /// <summary>
        /// Controls the amount of heat that is applied on each spot of the planet
        /// </summary>
        public MapSO heatMap;

        private CelestialBody _body;

        /// <summary>
        /// Get the body
        /// </summary>
        private void Start()
        {
            _body = GetComponent<CelestialBody>();

            StartCoroutine(Worker());
        }

        private void OnLevelWasLoaded(Int32 level)
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
                    yield return new WaitForSeconds(heatInterval);
                    continue;
                }

                // Get all vessels
                List<Vessel> vessels = FlightGlobals.Vessels.FindAll(v => v.mainBody == _body);

                // Loop through them
                foreach (Vessel vessel in vessels)
                {
                    Double altitude =
                        altitudeCurve.Evaluate((Single) Vector3d.Distance(vessel.transform.position,
                            _body.transform.position));
                    Double latitude = latitudeCurve.Evaluate((Single) vessel.latitude);
                    Double longitude = longitudeCurve.Evaluate((Single) vessel.longitude);

                    Double heat = altitude * latitude * longitude * heatRate;
                    if (heatMap)
                    {
                        heat *= heatMap.GetPixelFloat((longitude + 180) / 360f, (latitude + 90) / 180f);
                    }

                    foreach (Part part in vessel.Parts)
                    {
                        part.temperature += heat;
                    }
                }

                // Wait
                yield return new WaitForSeconds(heatInterval);
            }

            // ReSharper disable once IteratorNeverReturns
        }
    }
}