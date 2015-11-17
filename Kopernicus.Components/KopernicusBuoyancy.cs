/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
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
 *

using System;
using System.Collections.Generic;
using System.Linq;
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

            // Update the status of the water
            private void FixedUpdate()
            {
                if (!FlightGlobals.ready || !part.vessel.mainBody.ocean)
                    return;

                if (!part.rb)
                    return;

                if (GetOcean() == null)
                    return;

                // Get the center of the Buoyancy
                Vector3 oldCenter = centerOfBuoyancy;
                centerOfBuoyancy = part.partTransform.position + (part.partTransform.rotation * part.CenterOfBuoyancy);

                // Get the current altitude
                double altitude = GetAltitudeFromOcean(centerOfBuoyancy);

                // Get the force
                buoyancyForce = Mathf.Max(0f, (float)-altitude);
                float dot = 0f;

                // If we are already splashed
                if (buoyancyForce <= 0f || part.State == PartStates.DEAD)
                {
                    if (splashed)
                    {
                        part.WaterContact = false;
                        part.vessel.checkSplashed();
                        splashed = false;
                        rigidbody.drag = 0f;
                    }
                }
                else
                {
                    if (!splashed)
                    {
                        // Get the speed of the part
                        double speed = part.rb.velocity.sqrMagnitude + Krakensbane.GetFrameVelocity().sqrMagnitude;

                        if (speed > 4 && Vector3.Distance(part.partTransform.position, FlightGlobals.camera_position) < 5000f)
                        {
                            // Splash
                            Vector3 position = centerOfBuoyancy - (oldCenter - (Krakensbane.GetFrameVelocityV3f() * TimeWarp.fixedDeltaTime));
                            dot = Mathf.Abs(Vector3.Dot(position, FlightGlobals.getUpAxis(FlightGlobals.currentMainBody, centerOfBuoyancy)));
                            Vector3 finalPositon = centerOfBuoyancy - (dot == 0f ? Vector3.zero : position * (buoyancyForce / dot));
                            FXMonger.Splash(finalPositon, part.rb.velocity.magnitude / 10f);
                        }

                        // Set spalshed to true
                        splashed = true;

                        // Destroy the part
                        if (speed > part.crashTolerance * part.crashTolerance * 1.2f)
                        {
                            GameEvents.onCrashSplashdown.Fire(new EventReport(FlightEvents.SPLASHDOWN_CRASH, part, part.partInfo.title, "an unidentified object", 0, string.Empty, 0f));
                            part.Die();
                            return;
                        }

                        // Set the part to splashed down
                        part.WaterContact = true;
                        part.vessel.Splashed = true;
                        if (IsInvoking())
                            CancelInvoke();
                        Invoke("ReportSplashDown", 2f);
                    }

                    // Set forces
                    effectiveForce = (-FlightGlobals.getGeeForceAtPosition(part.partTransform.position) * (dot == 0f ? buoyancy : dot)) * Mathf.Min(buoyancyForce, 1.5f);
                    part.rb.drag = 3f;
                    part.rb.AddForceAtPosition(effectiveForce, centerOfBuoyancy, 0);
                }
            }

            private float GetAltitudeFromOcean(Vector3 position)
            {
                PQS ocean = GetOcean();
                return Mathf.Abs(Vector3.Distance(position, mainBody.position)) - (float)ocean.GetSurfaceHeight(ocean.GetRelativePosition(position));
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
        }
    }
}
*/