/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 * 
 * Code based on KittiopaTech, modified by Thomas P.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class HazardousOcean : IParserEventSubscriber
        {
            // Set-up our Killer-Ocean
            public HazardousOceanController controller;

            // Our Hazardous Ocean
            public PQS OceanPQS
            {
                set { controller.oceanPQS = value; }
            }
            
            // Heating Curve of our Ocean
            [ParserTarget("HeatCurve", optional = true, allowMerge = false)]
            public FloatCurveParser HeatCurve
            {
                set { controller.heatCurve = value.curve; }
            }

            // Initialize the HazardousOcean
            public HazardousOcean()
            {

            }

            // Apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                GameObject controllerObject = new GameObject("HazardousOcean");
                controllerObject.transform.parent = Utility.Deactivator;
                controller = controllerObject.AddComponent<HazardousOceanController>();
            }

            // Post-Apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                
            }
        }

        //=====================================//
        //      Killer-Ocean Controller        //
        //=====================================//

        public class HazardousOceanController : MonoBehaviour
        {
            public FloatCurve heatCurve;
            public PQS oceanPQS;
            private string bodyName = "";

            public void Update()
            {
                if (FlightGlobals.Vessels != null && FlightGlobals.Bodies != null)
                {
                    bodyName = oceanPQS.name.Replace("Ocean", "");
                    List<Vessel> vessels = FlightGlobals.Vessels.FindAll(v => v.mainBody.GetTransform().name == bodyName);

                    foreach (Vessel vessel in vessels)
                    {
                        double distanceToPlanet = Math.Abs(Vector3d.Distance(vessel.GetTransform().position, oceanPQS.transform.position)) - oceanPQS.radius;
                        double heatingRate = heatCurve.Evaluate((float) distanceToPlanet);
                        foreach (Part part in vessel.Parts)
                        {
                            part.temperature += heatingRate;
                        }
                    }
                }
            }
        }
    }
}
