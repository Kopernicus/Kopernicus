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
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus.Components
{
    // Our own implementation of PartBuoyancy
    public class KopernicusBuoyancy : PartBuoyancy
    {
        // The Part we are attached to
        private Part Part
        {
            get { return GetComponent<Part>(); }
        }

        // The current ocean
        private PQS _ocean;
        private CelestialBody _mainBody;
        private Action _baseFixedUpdate;

        // Update the status of the water
        private void FixedUpdate()
        {
            if (GetOcean() != null)
            {
                // Change water level
                waterLevel = GetAltitudeFromOcean(centerOfDisplacement);
            }

            // Call base
            GetBaseFixedUpdate()();
        }

        private Single GetAltitudeFromOcean(Vector3 position)
        {
            PQS ocean = GetOcean();
            return (Single) ocean.GetSurfaceHeight(ocean.GetRelativePosition(position)) -
                   (Single) Part.vessel.mainBody.Radius;
        }

        private PQS GetOcean()
        {
            if (_ocean != null && _mainBody == Part.vessel.mainBody)
            {
                return _ocean;
            }

            _mainBody = Part.vessel.mainBody;
            _ocean = _mainBody.GetComponentsInChildren<PQS>(true)
                .FirstOrDefault(p => p.name == _mainBody.transform.name + "Ocean");

            return _ocean;
        }

        private Action GetBaseFixedUpdate()
        {
            if (_baseFixedUpdate != null)
            {
                return _baseFixedUpdate;
            }

            PartBuoyancy b = this;
            MethodInfo info =
                typeof(PartBuoyancy).GetMethod("FixedUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
            _baseFixedUpdate = (Action) Delegate.CreateDelegate(typeof(Action), b, info);

            return _baseFixedUpdate;
        }
    }
}