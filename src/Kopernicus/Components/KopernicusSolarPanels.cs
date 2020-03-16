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
    public class KopernicusSolarPanelsFixer : PartModule
    {
        ModuleDeployableSolarPanel SP;

        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                SP = GetComponent<ModuleDeployableSolarPanel>();
            }
        }

        // Runs before the MDSP FixedUpdate
        public virtual void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                // Start from the target body
                if (SP != null)
                {
                    KopernicusStar star = KopernicusStar.CelestialBodies[SP.trackingBody];
                    PhysicsGlobals.SolarLuminosityAtHome = star.shifter.solarLuminosity;
                    PhysicsGlobals.SolarInsolationAtHome = star.shifter.solarInsolation;
                    KopernicusStar.CalculatePhysics();

                    vessel.solarFlux = CalculateFlux(star);
                }
            }
        }

        public Double CalculateFlux(Sun star)
        {
            // Get sunVector
            Boolean directSunlight = false;
            Vector3 integratorPosition = vessel.transform.position;
            Vector3d scaledSpace = ScaledSpace.LocalToScaledSpace(integratorPosition);
            Vector3 position = star.sun.scaledBody.transform.position;
            Double scale = Math.Max((position - scaledSpace).magnitude, 1);
            Vector3 sunVector = (position - scaledSpace) / scale;
            Ray ray = new Ray(ScaledSpace.LocalToScaledSpace(integratorPosition), sunVector);

            // Get Solar Flux
            Double realDistanceToSun = 0;
            if (!Physics.Raycast(ray, out RaycastHit raycastHit, Single.MaxValue, ModularFlightIntegrator.SunLayerMask))
            {
                directSunlight = true;
                realDistanceToSun = scale * ScaledSpace.ScaleFactor - star.sun.Radius;
            }
            else if (raycastHit.transform.GetComponent<ScaledMovement>().celestialBody == star.sun)
            {
                realDistanceToSun = ScaledSpace.ScaleFactor * raycastHit.distance;
                directSunlight = true;
            }

            if (directSunlight)
            {
                Double output = PhysicsGlobals.SolarLuminosity / (12.5663706143592 * realDistanceToSun * realDistanceToSun) * PhysicsGlobals.SolarLuminosityAtHome / 1360;
                return output;
            }

            return 0;
        }
    }

    /// <summary>
    /// An extension for the Solar Panel to calculate the flux properly
    /// </summary>
    public class KopernicusSolarPanels : KopernicusSolarPanelsFixer
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

        public override void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                for (Int32 n = 0; n < SPs.Length; n++)
                {
                    ModuleDeployableSolarPanel SP = SPs[n];

                    if (SP.deployState == ModuleDeployablePart.DeployState.EXTENDED)
                    {
                        CelestialBody trackingBody = SP.trackingBody;
                        KopernicusStar trackingStar = null;

                        Double bestFlux = vessel.solarFlux;
                        KopernicusStar bestStar = null;

                        Double totalFlux = 0;
                        Single totalAoA = SP.sunAOA;
                        Double _totalFlow = SP._flowRate;
                        Single totalFlow = SP.flowRate;

                        for (Int32 s = 0; s < KopernicusStar.Stars.Count; s++)
                        {
                            KopernicusStar star = KopernicusStar.Stars[s];

                            if (star.sun == trackingBody)
                            {
                                trackingStar = star;
                            }
                            else
                            {
                                // Use this star
                                PhysicsGlobals.SolarLuminosityAtHome = star.shifter.solarLuminosity;
                                PhysicsGlobals.SolarInsolationAtHome = star.shifter.solarInsolation;
                                KopernicusStar.CalculatePhysics();
                                vessel.solarFlux = CalculateFlux(star);

                                // Change the tracking body
                                SP.trackingBody = star.sun;
                                SP.GetTrackingBodyTransforms();

                                // Set Tracking Speed to zero
                                Single trackingSpeed = SP.trackingSpeed;
                                SP.trackingSpeed = 0;

                                // Run The MDSP CalculateTracking
                                SP.CalculateTracking();

                                // Add to TotalFlux and TotalAoA
                                totalFlux += vessel.solarFlux;
                                totalAoA += SP.sunAOA;
                                _totalFlow += SP._flowRate;
                                totalFlow += SP.flowRate;

                                if (bestFlux < vessel.solarFlux)
                                {
                                    bestFlux = vessel.solarFlux;
                                    bestStar = star;
                                }

                                // Restore Tracking Speed
                                SP.trackingSpeed = trackingSpeed;
                            }
                        }

                        // Restore the tracking body
                        SP.trackingBody = trackingStar.sun;
                        SP.GetTrackingBodyTransforms();

                        // Restore the starting star
                        PhysicsGlobals.SolarLuminosityAtHome = trackingStar.shifter.solarLuminosity;
                        PhysicsGlobals.SolarInsolationAtHome = trackingStar.shifter.solarInsolation;
                        KopernicusStar.CalculatePhysics();

                        totalFlux += CalculateFlux(trackingStar);

                        vessel.solarFlux = totalFlux;
                        SP.sunAOA = totalAoA;
                        SP.sunAOA /= _relativeSunAoa ? KopernicusStar.Stars.Count : 1;
                        SP._flowRate = _totalFlow;
                        SP.flowRate = totalFlow;

                        // We got the best star to use
                        if (bestStar != null && bestStar.sun != SP.trackingBody)
                        {
                            if (!_manualTracking)
                            {
                                SP.trackingBody = bestStar.sun;
                                SP.GetTrackingBodyTransforms();
                                continue;
                            }
                        }
                    }
                }

                // Restore The Current Star
                PhysicsGlobals.SolarLuminosityAtHome = KopernicusStar.Current.shifter.solarLuminosity;
                PhysicsGlobals.SolarInsolationAtHome = KopernicusStar.Current.shifter.solarInsolation;
                KopernicusStar.CalculatePhysics();
            }
        }

        void EarlyLateUpdate()
        {
            for (Int32 n = 0; n < SPs.Length; n++)
            {
                ModuleDeployableSolarPanel SP = SPs[n];

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
                        for (int n = 0; n < SPs.Length; n++)
                        {
                            ModuleDeployableSolarPanel SP = SPs[n];
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

                SPs = GetComponents<ModuleDeployableSolarPanel>();

                if (SPs.Any(p => p.isTracking))
                {
                    Fields["trackingBodyName"].guiActive = true;
                    Events["ManualTracking"].guiActive = true;
                }
            }
        }

        public void OnDestroy()
        {
            TimingManager.LateUpdateRemove(TimingManager.TimingStage.Early, EarlyLateUpdate);
        }
    }
}
