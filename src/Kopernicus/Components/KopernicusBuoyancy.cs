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

using UnityEngine;

namespace Kopernicus.Components
{
    // Our own modifications to PartBuoyancy
    public class KopernicusBuoyancy : MonoBehaviour
    {
        private Part _part;
        private PartBuoyancy _buoyancy;

        // The current ocean
        private PQS _ocean;
        private CelestialBody _mainBody;

        private void Start()
        {
            _part = GetComponent<Part>();
            _buoyancy = GetComponent<PartBuoyancy>();
        }

        private void Update()
        {
            if (_mainBody != _part.vessel.mainBody)
            {
                _mainBody = _part.vessel.mainBody;
                _ocean = null;
            }

            if (_ocean == null)
            {
                _ocean = _mainBody.pqsController.GetComponentInChildren<PQS>();
            }

            if (_ocean != null)
            {
                _buoyancy.waterLevel =
                    _ocean.GetSurfaceHeight(_ocean.GetRelativePosition(_buoyancy.centerOfDisplacement)) -
                    _part.vessel.mainBody.Radius;
            }
        }
    }
}