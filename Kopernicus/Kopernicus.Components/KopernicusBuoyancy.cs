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
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
        // Our own implementation of PartBuoyancy
        public class KopernicusBuoyancy : PartBuoyancy
        {
            // The Part we are attached to
            public Part part
            {
                get { return GetComponent<Part>(); }
            }

            // The current ocean
            private PQS _ocean;
            private CelestialBody mainBody;
            private Action BaseFixedUpdate;

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

            private float GetAltitudeFromOcean(Vector3 position)
            {
                PQS ocean = GetOcean();
                return (float)ocean.GetSurfaceHeight(ocean.GetRelativePosition(position)) - (float)part.vessel.mainBody.Radius;
            }

            private PQS GetOcean()
            {
                if (_ocean == null || mainBody != part.vessel.mainBody)
                {
                    mainBody = part.vessel.mainBody;
                    _ocean = mainBody.GetComponentsInChildren<PQS>(true).FirstOrDefault(p => p.name == mainBody.transform.name + "Ocean");
                }
                return _ocean;
            }

            private Action GetBaseFixedUpdate()
            {
                if (BaseFixedUpdate == null)
                {
                    PartBuoyancy b = this;
                    MethodInfo info = typeof(PartBuoyancy).GetMethod("FixedUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
                    BaseFixedUpdate = (Action) Delegate.CreateDelegate(typeof(Action), b, info);
                }
                return BaseFixedUpdate;
            }
        }
    }
}