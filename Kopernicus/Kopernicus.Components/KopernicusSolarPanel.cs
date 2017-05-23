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
            }

            public override void CalculateTracking()
            {
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

                    // Calculate SunAOA
                    Single _sunAOA = 0;
                    Vector3 direction = (trackingTransformLocal.position - panelRotationTransform.position).normalized;
                    trackingLOS = CalculateTrackingLOS(direction, ref blockingObject);
                    if (!trackingLOS)
                    {
                        _sunAOA = 0f;
                    }
                    else
                    {
                        if (panelType == PanelType.FLAT)
                        {
                            _sunAOA = Mathf.Clamp(Vector3.Dot(trackingDotTransform.forward, direction), 0f, 1f);
                        }
                        else if (this.panelType != ModuleDeployableSolarPanel.PanelType.CYLINDRICAL)
                        {
                            _sunAOA = 0.25f;
                        }
                        else
                        {
                            Vector3 direction2;
                            if (alignType == PanelAlignType.PIVOT)
                            {
                                direction2 = trackingDotTransform.forward;
                            }
                            else if (alignType != PanelAlignType.X)
                            {
                                direction2 = (alignType != PanelAlignType.Y ? part.partTransform.forward : part.partTransform.up);
                            }
                            else
                            {
                                direction2 = part.partTransform.right;
                            }
                            _sunAOA = (1f - Mathf.Abs(Vector3.Dot(direction2, direction))) * 0.318309873f;
                        }
                    }

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
                base.CalculateTracking();
            }
        }
    }
}