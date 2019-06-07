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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using ModularFI;
using UnityEngine;

namespace Kopernicus.Components
{
    /// <summary>
    /// An extension for the Solar Panel to calculate the flux properly
    /// </summary>
    public class KopernicusSolarPanel : ModuleDeployableSolarPanel
    {
        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Tracking Body", isPersistant = true)]
        [SuppressMessage("ReSharper", "NotAccessedField.Global")]
        public String trackingBodyName;

        [KSPField(isPersistant = true)]
        private Boolean _manualTracking;

        [KSPField(isPersistant = true)]
        private Boolean _relativeSunAoa;

        private static readonly Double StockLuminosity;

        static KopernicusSolarPanel()
        {
            String filename = (String) typeof(PhysicsGlobals).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(f => f.FieldType == typeof(String)).GetValue(PhysicsGlobals.Instance);
            ConfigNode node = ConfigNode.Load(filename);
            String value = node.GetValue("solarLuminosityAtHome");
            if (value != null)
            {
                StockLuminosity = Double.Parse(value);
            }
        }

        [SuppressMessage("ReSharper", "RedundantAssignment")]
        public override void PostCalculateTracking(Boolean trackingLos, Vector3 trackingDirection)
        {
            // Maximum values
            Double maxEnergy = 0;
            KopernicusStar maxStar = null;

            // Override layer mask
            planetLayerMask = ModularFlightIntegrator.SunLayerMask;

            // Efficiency modifier
            _efficMult = temperatureEfficCurve.Evaluate((Single) part.skinTemperature) *
                         timeEfficCurve.Evaluate(
                             (Single) ((Planetarium.GetUniversalTime() - launchUT) * 1.15740740740741E-05)) *
                         efficiencyMult;
            _flowRate = 0;
            sunAOA = 0;

            // Go through all stars
            foreach (KopernicusStar star in KopernicusStar.Stars)
            {
                // Calculate stuff
                Transform sunTransform = star.sun.transform;
                Vector3 trackDir = (sunTransform.position - panelRotationTransform.position).normalized;
                CelestialBody old = trackingBody;
                trackingTransformLocal = sunTransform;
                trackingTransformScaled = star.sun.scaledBody.transform;
                trackingLos = CalculateTrackingLOS(trackDir, ref blockingObject);
                trackingTransformLocal = old.transform;
                trackingTransformScaled = old.scaledBody.transform;

                // Calculate sun AOA
                Single sunAoa;
                if (!trackingLos)
                {
                    sunAoa = 0f;
                    status = "Blocked by " + blockingObject;
                }
                else
                {
                    status = "Direct Sunlight";
                    if (panelType == PanelType.FLAT)
                    {
                        sunAoa = Mathf.Clamp(Vector3.Dot(trackingDotTransform.forward, trackDir), 0f, 1f);
                    }
                    else if (panelType != PanelType.CYLINDRICAL)
                    {
                        sunAoa = 0.25f;
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
                            direction = alignType != PanelAlignType.Y
                                ? part.partTransform.forward
                                : part.partTransform.up;
                        }
                        else
                        {
                            direction = part.partTransform.right;
                        }

                        sunAoa = (1f - Mathf.Abs(Vector3.Dot(direction, trackDir))) * 0.318309873f;
                    }
                }

                // Calculate distance multiplier
                Double distMult;
                if (!useCurve)
                {
                    if (!KopernicusStar.SolarFlux.ContainsKey(star.name))
                    {
                        continue;
                    }

                    distMult = (Single) (KopernicusStar.SolarFlux[star.name] / StockLuminosity);
                }
                else
                {
                    distMult =
                        powerCurve.Evaluate((star.sun.transform.position - panelRotationTransform.position).magnitude);
                }

                // Calculate flow rate
                Double panelFlowRate = sunAoa * _efficMult * distMult;
                if (part.submergedPortion > 0)
                {
                    Double altitudeAtPos =
                        -FlightGlobals.getAltitudeAtPos((Vector3d) secondaryTransform.position, vessel.mainBody);
                    altitudeAtPos = (altitudeAtPos * 3 + part.maxDepth) * 0.25;
                    if (altitudeAtPos < 0.5)
                    {
                        altitudeAtPos = 0.5;
                    }

                    Double num = 1 / (1 + altitudeAtPos * part.vessel.mainBody.oceanDensity);
                    if (part.submergedPortion >= 1)
                    {
                        panelFlowRate *= num;
                    }
                    else
                    {
                        panelFlowRate *= UtilMath.LerpUnclamped(1, num, part.submergedPortion);
                    }

                    status += ", Underwater";
                }

                sunAOA += sunAoa;
                Double energy = distMult * _efficMult;
                if (energy > maxEnergy)
                {
                    maxEnergy = energy;
                    maxStar = star;
                }

                // Apply the flow rate
                _flowRate += panelFlowRate;
            }

            // Sun AOA
            sunAOA /= _relativeSunAoa ? KopernicusStar.Stars.Count : 1;
            _distMult = Math.Abs(_flowRate) > 0.01 ? _flowRate / _efficMult / sunAOA : 0;

            // We got the best star to use
            if (maxStar != null && maxStar.sun != trackingBody)
            {
                if (!_manualTracking)
                {
                    trackingBody = maxStar.sun;
                    GetTrackingBodyTransforms();
                }
            }

            // Use the flow rate
            flowRate = (Single) (resHandler.UpdateModuleResourceOutputs(_flowRate) * flowMult);
        }

        public override void LateUpdate()
        {
            // Update the name
            trackingBodyName = trackingBody.bodyDisplayName.Replace("^N", "");

            // Update the guiName for SwitchAOAMode
            Events["SwitchAoaMode"].guiName = _relativeSunAoa ? "Use absolute exposure" : "Use relative exposure";
            base.LateUpdate();
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Select Tracking Body")]
        public void ManualTracking()
        {
            // Assemble the buttons
            DialogGUIBase[] options = new DialogGUIBase[KopernicusStar.Stars.Count + 1];
            options[0] = new DialogGUIButton("Auto", () => { _manualTracking = false; }, true);
            for (Int32 i = 0; i < KopernicusStar.Stars.Count; i++)
            {
                CelestialBody body = KopernicusStar.Stars[i].sun;
                options[i + 1] = new DialogGUIButton(body.bodyDisplayName.Replace("^N", ""), () =>
                {
                    _manualTracking = true;
                    trackingBody = body;
                    GetTrackingBodyTransforms();
                }, true);
            }

            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog(
                "SelectTrackingBody",
                "Please select the Body you want to track with this Solar Panel.",
                "Select Tracking Body",
                UISkinManager.GetSkin("MainMenuSkin"),
                options), false, UISkinManager.GetSkin("MainMenuSkin"));
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Use relative exposure")]
        public void SwitchAoaMode()
        {
            _relativeSunAoa = !_relativeSunAoa;
        }
    }
}
