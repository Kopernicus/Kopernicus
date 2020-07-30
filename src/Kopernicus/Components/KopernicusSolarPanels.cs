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
using UnityEngine;
using KSP.Localization;

namespace Kopernicus.Components
{
    /// <summary>
    /// This <see cref="PartModule"/> should be added before any <see cref="ModuleDeployableSolarPanel"/>.
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

        /// <summary>
        /// Runs before <see cref="ModuleDeployableSolarPanel.FixedUpdate"/>.
        /// </summary>
        void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                // Start from the target body
                if (SP != null)
                {
                    KopernicusStar star = KopernicusStar.CelestialBodies[SP.trackingBody];
                    star.shifter.ApplyPhysics();

                    vessel.solarFlux = star.CalculateFluxAt(vessel) * PhysicsGlobals.SolarLuminosityAtHome / 1360;
                }
            }
        }
    }

    /// <summary>
    /// This <see cref="PartModule"/> should be added after all <see cref="ModuleDeployableSolarPanel"/>.
    /// </summary>
    public class KopernicusSolarPanels : PartModule
    {
        //Strings for Localization
        private static string SP_status_DirectSunlight = Localizer.Format("#Kopernicus_UI_DirectSunlight");  // "Direct Sunlight"
        private static string SP_status_Underwater = Localizer.Format("#Kopernicus_UI_Underwater");          // "Underwater"
        private static string button_Auto = Localizer.Format("#Kopernicus_UI_AutoTracking");                 // "Auto"
        private static string SelectBody = Localizer.Format("#Kopernicus_UI_SelectBody");                    // "Select Tracking Body"
        private static string SelectBody_Msg = Localizer.Format("#Kopernicus_UI_SelectBody_Msg");            // "Please select the Body you want to track with this Solar Panel."

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "#Kopernicus_UI_TrackingBody", isPersistant = true)]
        [SuppressMessage("ReSharper", "NotAccessedField.Global")]
        public String trackingBodyName;

        [KSPField(isPersistant = true)]
        private Boolean _manualTracking;

        /// <summary>
        /// The list of all <see cref="ModuleDeployableSolarPanel"/><i>s</i> on this <see cref="Part"/>.
        /// </summary>
        private ModuleDeployableSolarPanel[] SPs;

        /// <summary>
        /// Runs before <see cref="ModuleDeployableSolarPanel.FixedUpdate"/>.
        /// </summary>
        void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                for (Int32 n = 0; n < SPs.Length; n++)
                {
                    ModuleDeployableSolarPanel SP = SPs[n];

                    if (SP.deployState == ModuleDeployablePart.DeployState.EXTENDED)
                    {
                        KopernicusStar trackingStar = KopernicusStar.CelestialBodies[SP.trackingBody];

                        Double bestFlux = vessel.solarFlux * 1360 / PhysicsGlobals.SolarLuminosityAtHome;
                        KopernicusStar bestStar = trackingStar;
                        Double totalFlux = 0;
                        Double _totalFlow = 0;
                        Single totalFlow = 0;
                        double homeStarLumaConstant = 1360;
                        for (Int32 s = 0; s < KopernicusStar.Stars.Count; s++) //yes, we have to iterate this twice to find the home star luma.  Fun.
                        {
                            KopernicusStar star = KopernicusStar.Stars[s];
                            if (KopernicusStar.GetLocalStar(FlightGlobals.GetHomeBody()).name == star.sun.name)
                            {
                                star.shifter.ApplyPhysics();
                                homeStarLumaConstant = PhysicsGlobals.SolarLuminosity;
                            }
                        }
                        for (Int32 s = 0; s < KopernicusStar.Stars.Count; s++)
                        {
                            KopernicusStar star = KopernicusStar.Stars[s];
                            // Use this star
                            star.shifter.ApplyPhysics();
                            double starFlux = star.CalculateFluxAt(vessel);

                            // Change the tracking body
                            SP.trackingBody = star.sun;
                            SP.GetTrackingBodyTransforms();

                            // Set Tracking Speed to zero
                            Single trackingSpeed = SP.trackingSpeed;
                            SP.trackingSpeed = 0;

                            // Run The MDSP CalculateTracking
                            SP.CalculateTracking();

                            if (bestFlux < starFlux)
                            {
                                bestFlux = starFlux;
                                bestStar = star;
                            }

                            // Add to TotalFlux and EC tally
                            totalFlux += starFlux;
                            float panelEffectivness = ((SP.chargeRate / 24.4f) / 56.37091313591871f) * SP.sunAOA; //56.blahblah is a weird constant determined to convert flux to EC in stock game terminology.  We have reasons, honest.
                            totalFlow += (float)starFlux * panelEffectivness;
                            totalFlow += ((float)starFlux * panelEffectivness) * (1360 / (float)homeStarLumaConstant);
                            _totalFlow += totalFlow / SP.chargeRate;

                            // Restore the starting star
                            trackingStar.shifter.ApplyPhysics();

                            // Restore Tracking Speed
                            SP.trackingSpeed = trackingSpeed;

                            // Restore the old tracking body
                            SP.trackingBody = trackingStar.sun;
                            SP.GetTrackingBodyTransforms();

                        }
                        // Restore the old tracking body
                        SP.trackingBody = trackingStar.sun;
                        SP.GetTrackingBodyTransforms();

                        // Run The MDSP CalculateTracking
                        SP.CalculateTracking();

                        //see if tracked star is blocked or not
                        if (SP.sunAOA > 0)
                        {
                            //this ensures the "blocked" GUI option is set right, if we're exposed to you we're not blocked
                            vessel.directSunlight = true;
                        }
                        // We got the best star to use
                        if ((bestStar != null && bestStar.sun != SP.trackingBody) && (!_manualTracking))
                        {
                            SP.trackingBody = bestStar.sun;
                            SP.GetTrackingBodyTransforms();
                        }
                        else
                        {
                        }

                        vessel.solarFlux = totalFlux;
                        SP._flowRate = _totalFlow;
                        SP.flowRate = totalFlow;
                    }
                }

                // Restore The Current Star
                KopernicusStar.Current.shifter.ApplyPhysics();
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

                    if (!_manualTracking)
                    {
                        trackingBodyName = Localizer.Format("#Kopernicus_UI_AutoTrackingBodyName", trackingBodyName);
                    }
                }
            }
        }

        [KSPEvent(active = true, guiActive = false, guiName = "#Kopernicus_UI_SelectBody")]
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
                    () => SetTrackingBody(body),
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

        public void SetTrackingBody(CelestialBody sun)
        {
            for (int n = 0; n < SPs.Length; n++)
            {
                ModuleDeployableSolarPanel SP = SPs[n];
                _manualTracking = true;
                SP.trackingBody = sun;
                SP.GetTrackingBodyTransforms();
            }
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

                if (_manualTracking)
                {
                    CelestialBody trackingBody = FlightGlobals.Bodies.FirstOrDefault(b => b.bodyDisplayName.Replace("^N", "") == trackingBodyName);

                    if (trackingBody != null)
                    {
                        SetTrackingBody(trackingBody);
                    }
                    else
                    {
                        _manualTracking = false;
                    }
                }
            }
        }

        public void OnDestroy()
        {
            TimingManager.LateUpdateRemove(TimingManager.TimingStage.Early, EarlyLateUpdate);
        }
    }
}
