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
using KSP.Localization;

namespace Kopernicus.Components
{
    /// <summary>
    /// An extension for the Solar Panel to calculate the flux properly
    /// </summary>
    public class KopernicusSolarPanels : PartModule
    {
        //Strings for Localization
        private static string SP_status_DirectSunlight = Localizer.Format("#Kopernicus_UI_DirectSunlight");//"Direct Sunlight"
        private static string SP_status_Underwater = Localizer.Format("#Kopernicus_UI_Underwater");//"Underwater"
        private static string button_AbsoluteExposure = Localizer.Format("#Kopernicus_UI_AbsoluteExposure");//"Use absolute exposure"
        private static string button_RelativeExposure = Localizer.Format("#Kopernicus_UI_RelativeExposure");//"Use relative exposure"
        private static string button_Auto = Localizer.Format("#Kopernicus_UI_AutoTracking");//"Auto"
        private static string SelectBody = Localizer.Format("#Kopernicus_UI_SelectBody");//"Select Tracking Body"
        private static string SelectBody_Msg = Localizer.Format("#Kopernicus_UI_SelectBody_Msg");// "Please select the Body you want to track with this Solar Panel."

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "#Kopernicus_UI_TrackingBody", isPersistant = true)]//Tracking Body
        [SuppressMessage("ReSharper", "NotAccessedField.Global")]
        public String trackingBodyName;

        [KSPField(isPersistant = true)]
        private Boolean _manualTracking;

        [KSPField(isPersistant = true)]
        private Boolean _relativeSunAoa;

        private ModuleDeployableSolarPanel[] SPs;

        private static readonly Double StockLuminosity;

        static KopernicusSolarPanels()
        {
            StockLuminosity = LightShifter.Prefab.solarLuminosity;
        }

        public void LatePostCalculateTracking()
        {
            for (Int32 n = 0; n < SPs.Length; n++)
            {
                ModuleDeployableSolarPanel SP = SPs[n];

                if (SP?.deployState == ModuleDeployablePart.DeployState.EXTENDED)
                {
                    Vector3 normalized = (SP.trackingTransformLocal.position - SP.panelRotationTransform.position).normalized;
                    FieldInfo trackingLOS = typeof(ModuleDeployableSolarPanel).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(f => f.Name == "trackingLOS");
                    LatePostCalculateTracking((bool)trackingLOS.GetValue(SP), normalized);
                }
            }
        }

        public void LatePostCalculateTracking(Boolean trackingLos, Vector3 trackingDirection)
        {
            for (Int32 n = SPs.Length; n > 0; n--)
            {
                ModuleDeployableSolarPanel SP = SPs[n - 1];

                // Maximum values
                Double maxEnergy = 0;
                KopernicusStar maxStar = null;

                // Override layer mask
                typeof(ModuleDeployableSolarPanel).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(f => f.Name == "planetLayerMask").SetValue(SP, ModularFlightIntegrator.SunLayerMask);

                // Efficiency modifier
                SP._efficMult = SP.temperatureEfficCurve.Evaluate((Single)part.skinTemperature) *
                             SP.timeEfficCurve.Evaluate(
                                 (Single)((Planetarium.GetUniversalTime() - SP.launchUT) * 1.15740740740741E-05)) *
                            SP.efficiencyMult;
                SP._flowRate = 0;
                SP.sunAOA = 0;

                // Go through all stars
                Int32 stars = KopernicusStar.Stars.Count;
                for (Int32 i = 0; i < stars; i++)
                {
                    KopernicusStar star = KopernicusStar.Stars[i];

                    // Calculate stuff
                    Transform sunTransform = star.sun.transform;
                    Vector3 trackDir = (sunTransform.position - SP.panelRotationTransform.position).normalized;
                    CelestialBody old = SP.trackingBody;
                    SP.trackingTransformLocal = sunTransform;
                    SP.trackingTransformScaled = star.sun.scaledBody.transform;

                    FieldInfo blockingObject = typeof(ModuleDeployableSolarPanel).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(f => f.Name == "blockingObject");
                    string blockingObjectName = (string)blockingObject.GetValue(SP);
                    trackingLos = SP.CalculateTrackingLOS(trackDir, ref blockingObjectName);
                    blockingObject.SetValue(SP, blockingObjectName);

                    SP.trackingTransformLocal = old.transform;
                    SP.trackingTransformScaled = old.scaledBody.transform;

                    // Calculate sun AOA
                    Single sunAoa;
                    if (!trackingLos)
                    {
                        sunAoa = 0f;
                        SP.status = Localizer.Format("#Kopernicus_UI_PanelBlocked", blockingObjectName);//"Blocked by " + blockingObjectName
                    }
                    else
                    {
                        SP.status = SP_status_DirectSunlight;//"Direct Sunlight"
                        if (SP.panelType == ModuleDeployableSolarPanel.PanelType.FLAT)
                        {
                            sunAoa = Mathf.Clamp(Vector3.Dot(SP.trackingDotTransform.forward, trackDir), 0f, 1f);
                        }
                        else if (SP.panelType != ModuleDeployableSolarPanel.PanelType.CYLINDRICAL)
                        {
                            sunAoa = 0.25f;
                        }
                        else
                        {
                            Vector3 direction;
                            if (SP.alignType == ModuleDeployableSolarPanel.PanelAlignType.PIVOT)
                            {
                                direction = SP.trackingDotTransform.forward;
                            }
                            else if (SP.alignType != ModuleDeployableSolarPanel.PanelAlignType.X)
                            {
                                direction = SP.alignType != ModuleDeployableSolarPanel.PanelAlignType.Y
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
                    if (!SP.useCurve)
                    {
                        if (!KopernicusStar.SolarFlux.ContainsKey(star.name))
                        {
                            continue;
                        }

                        distMult = (Single)(KopernicusStar.SolarFlux[star.name] / StockLuminosity);
                    }
                    else
                    {
                        distMult =
                         SP.powerCurve.Evaluate((star.sun.transform.position - SP.panelRotationTransform.position).magnitude);
                    }

                    // Calculate flow rate
                    Double panelFlowRate = sunAoa * SP._efficMult * distMult;
                    if (part.submergedPortion > 0)
                    {
                        Double altitudeAtPos =
                            -FlightGlobals.getAltitudeAtPos
                            (
                                (Vector3d)(((Transform)typeof(ModuleDeployableSolarPanel).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(f => f.Name == "secondaryTransform").GetValue(SP)).position),
                                vessel.mainBody
                            );
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

                        SP.status += ", " + SP_status_Underwater;//Underwater
                    }

                    SP.sunAOA += sunAoa;
                    Double energy = distMult * SP._efficMult;
                    if (energy > maxEnergy)
                    {
                        maxEnergy = energy;
                        maxStar = star;
                    }

                    // Apply the flow rate
                    SP._flowRate += panelFlowRate;
                }

                // Sun AOA
                SP.sunAOA /= _relativeSunAoa ? stars : 1;
                SP._distMult = Math.Abs(SP._flowRate) > 0.01 ? SP._flowRate / SP._efficMult / SP.sunAOA : 0;

                // We got the best star to use
                if (maxStar != null && maxStar.sun != SP.trackingBody)
                {
                    if (!_manualTracking)
                    {
                        SP.trackingBody = maxStar.sun;
                        SP.GetTrackingBodyTransforms();
                    }
                }

                // Use the flow rate
                SP.flowRate = (Single)(SP.resHandler.UpdateModuleResourceOutputs(SP._flowRate) * SP.flowMult);
            }
        }

        public void EarlyLateUpdate()
        {
            for (Int32 n = SPs.Length; n > 0; n--)
            {
                ModuleDeployableSolarPanel SP = SPs[n - 1];

                if (SP?.deployState == ModuleDeployablePart.DeployState.EXTENDED)
                {
                    // Update the name
                    trackingBodyName = SP.trackingBody.bodyDisplayName.Replace("^N", "");

                    // Update the guiName for SwitchAOAMode
                    Events["SwitchAoaMode"].guiName = _relativeSunAoa ? button_AbsoluteExposure : button_RelativeExposure;//Use absolute exposure//Use relative exposure
                }
            }
        }

        [KSPEvent(active = true, guiActive = false, guiName = "#Kopernicus_UI_SelectBody")]//Select Tracking Body
        public void ManualTracking()
        {
            // Assemble the buttons
            Int32 stars = KopernicusStar.Stars.Count;
            DialogGUIBase[] options = new DialogGUIBase[stars + 1];
            options[0] = new DialogGUIButton(button_Auto, () => { _manualTracking = false; }, true);//Auto
            for (Int32 i = 0; i < stars; i++)
            {
                CelestialBody body = KopernicusStar.Stars[i].sun;
                options[i + 1] = new DialogGUIButton
                (
                    body.bodyDisplayName.Replace("^N", ""),
                    () =>
                    {
                        for (int n = SPs?.Length ?? 0; n > 0; n--)
                        {
                            ModuleDeployableSolarPanel SP = SPs[n - 1];
                            _manualTracking = true;
                            SP.trackingBody = body;
                            SP.GetTrackingBodyTransforms();
                        }
                    },
                    true
                );
            }

            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog(
                "SelectTrackingBody",
                SelectBody_Msg,//Please select the Body you want to track with this Solar Panel.
                SelectBody,//Select Tracking Body
                UISkinManager.GetSkin("MainMenuSkin"),
                options), false, UISkinManager.GetSkin("MainMenuSkin"));
        }

        [KSPEvent(active = true, guiActive = true, guiName = "#Kopernicus_UI_RelativeExposure")]//Use relative exposure
        public void SwitchAoaMode()
        {
            _relativeSunAoa = !_relativeSunAoa;
        }

        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                TimingManager.LateUpdateAdd(TimingManager.TimingStage.Early, EarlyLateUpdate);
                TimingManager.FixedUpdateAdd(TimingManager.TimingStage.Late, LatePostCalculateTracking);

                SPs = GetComponents<ModuleDeployableSolarPanel>();

                if (SPs.Any(p => p.isTracking))
                {
                    Fields["trackingBodyName"].guiActive = true;
                    Events["ManualTracking"].guiActive = true;
                }
            }

            base.OnStart(state);
        }

        public void OnDestroy()
        {
            TimingManager.LateUpdateRemove(TimingManager.TimingStage.Early, EarlyLateUpdate);
            TimingManager.FixedUpdateRemove(TimingManager.TimingStage.Late, LatePostCalculateTracking);
        }
    }
}
