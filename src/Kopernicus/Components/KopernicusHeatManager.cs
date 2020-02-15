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

using ModularFI;
using System;

namespace Kopernicus.Components
{
    public static class KopernicusHeatManager
    {
        private static Double maxTemp = 0;
        private static Double sumTemp = 0;

        public static void NewTemp(Double newTemp, Boolean sum = false)
        {
            if (sum && newTemp > 0)
            {
                sumTemp += newTemp;
            }
            else if (newTemp > maxTemp)
            {
                maxTemp = newTemp;
            }
        }

        /// <summary>
        /// Override for <see cref="FlightIntegrator.CalculateBackgroundRadiationTemperature"/>
        /// </summary>
        internal static double RadiationTemperature(ModularFlightIntegrator flightIntegrator, Double baseTemp)
        {
            // Stock Behaviour
            baseTemp = UtilMath.Lerp(baseTemp, PhysicsGlobals.SpaceTemperature, flightIntegrator.DensityThermalLerp);

            // Kopernicus Heat Manager
            maxTemp = baseTemp;
            sumTemp = 0;

            Events.OnCalculateBackgroundRadiationTemperature.Fire(flightIntegrator);

            baseTemp = maxTemp + sumTemp;

            return baseTemp;
        }
    }
}
