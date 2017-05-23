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

using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// An extension for the Solar Panel to calculate the flux properly
        /// </summary>
        public class KopernicusSolarPanel : ModuleDeployableSolarPanel
        {
            public static Double stockLuminosity;

            static KopernicusSolarPanel()
            {
                String filename = (String) typeof(PhysicsGlobals).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).First(f => f.FieldType == typeof(String)).GetValue(PhysicsGlobals.Instance);
                ConfigNode node = ConfigNode.Load(filename);
                String value = node.GetValue("solarLuminosityAtHome");
                if (value != null)
                {
                    stockLuminosity = Double.Parse(value);
                }
            }

            public override void PostCalculateTracking(Boolean trackingLOS, Vector3 trackingDirection)
            {
                // Ugly hack ahead
                Boolean oldUseCurve = useCurve;
                if (!useCurve)
                {
                    Single distMult = 0;
                    foreach (KopernicusStar star in KopernicusStar.Stars)
                    {
                        if (KopernicusStar.SolarFlux.ContainsKey(star.name))
                            distMult += (Single)(KopernicusStar.SolarFlux[star.name] / stockLuminosity);
                    }
                    powerCurve = new FloatCurve(new [] { new Keyframe(0, distMult), new Keyframe(1, distMult) });
                    useCurve = true;
                }
                base.PostCalculateTracking(trackingLOS, trackingDirection);
                useCurve = oldUseCurve;
                
                // Maximum values
                Single maxAOA = 0;
                KopernicusStar maxStar = null;

                // Go through all stars
                foreach (KopernicusStar star in KopernicusStar.Stars)
                {
                    // Set the body as active
                    trackingBody = star.sun;
                    trackingTransformLocal = star.sun.transform;
                    if (star.sun.scaledBody)
                    {
                        trackingTransformScaled = star.sun.scaledBody.transform;
                    }
                    GetTrackingBodyTransforms();

                    // Calculate sun AOA
                    Single _sunAOA = 0f;
                    if (!trackingLOS)
                    {
                        sunAOA = 0f;
                    }
                    else
                    {
                        status = "Direct Sunlight";
                        if (panelType == PanelType.FLAT)
                        {
                            _sunAOA = Mathf.Clamp(Vector3.Dot(trackingDotTransform.forward, trackingDirection), 0f, 1f);
                        }
                        else if (panelType != PanelType.CYLINDRICAL)
                        {
                            _sunAOA = 0.25f;
                        }
                        else
                        {
                            Vector3 direction;
                            if (alignType == PanelAlignType.PIVOT)
                            {
                                direction = trackingDotTransform.forward;
                            }
                            else if (alignType != PanelAlignType.X)
                            {
                                direction = alignType != PanelAlignType.Y ? part.partTransform.forward : part.partTransform.up;
                            }
                            else
                            {
                                direction = part.partTransform.right;
                            }
                            _sunAOA = (1f - Mathf.Abs(Vector3.Dot(direction, trackingDirection))) * 0.318309873f;
                        }
                    }

                    // Calculate distance multiplier
                    Double __distMult = 1;
                    if (!useCurve)
                    {
                        __distMult = (Single)(KopernicusStar.SolarFlux[star.name] / stockLuminosity);
                    }
                    else
                    {
                        __distMult = powerCurve.Evaluate((trackingTransformLocal.position - panelRotationTransform.position).magnitude);
                    }

                    // Calculate flow rate
                    _efficMult = (temperatureEfficCurve.Evaluate((Single) part.skinTemperature) * timeEfficCurve.Evaluate((Single) ((Planetarium.GetUniversalTime() - launchUT) * 1.15740740740741E-05)) * efficiencyMult);
                    Double __flowRate = _sunAOA * _efficMult * __distMult;
                    if (part.submergedPortion > 0)
                    {
                        Double altitudeAtPos = -FlightGlobals.getAltitudeAtPos((Vector3d) secondaryTransform.position, vessel.mainBody);
                        altitudeAtPos = (altitudeAtPos * 3 + part.maxDepth) * 0.25;
                        if (altitudeAtPos < 0.5)
                        {
                            altitudeAtPos = 0.5;
                        }
                        Double num = 1 / (1 + altitudeAtPos * part.vessel.mainBody.oceanDensity);
                        if (part.submergedPortion >= 1)
                        {
                            __flowRate = __flowRate * num;
                        }
                        else
                        {
                            __flowRate = __flowRate * UtilMath.LerpUnclamped(1, num, part.submergedPortion);
                        }
                        status += ", Underwater";
                    }

                    // Apply the flow rate
                    _flowRate += __flowRate;

                    // Check if we have a new maximum
                    if (_sunAOA > maxAOA)
                    {
                        maxAOA = _sunAOA;
                        maxStar = star;
                    }
                }

                // We got the best star to use
                trackingBody = maxStar.sun;
                trackingTransformLocal = maxStar.sun.transform;
                if (maxStar.sun.scaledBody)
                {
                    trackingTransformScaled = maxStar.sun.scaledBody.transform;
                }
                GetTrackingBodyTransforms();

                // Use the flow rate
                flowRate = (Single)(resHandler.UpdateModuleResourceOutputs(_flowRate) * flowMult);
            }
        }
    }
}