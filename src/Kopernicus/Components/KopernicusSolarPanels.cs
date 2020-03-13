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

        public void EarlyPostCalculateTracking()
        {
            Debug.Log("SigmaLog: EarlyPostCalculateTracking: OLD PhysicsGlobals.SolarLuminosityAtHome = " + PhysicsGlobals.SolarLuminosityAtHome);
            PhysicsGlobals.SolarLuminosityAtHome = Double.PositiveInfinity;
            Debug.Log("SigmaLog: EarlyPostCalculateTracking: TEMP1 PhysicsGlobals.SolarLuminosityAtHome = " + PhysicsGlobals.SolarLuminosityAtHome);
        }

        public void LatePostCalculateTracking()
        {
            for (Int32 n = 0; n < SPs.Length; n++)
            {
                ModuleDeployableSolarPanel SP = SPs[n];
                CelestialBody oldBody = SP.trackingBody;
                KopernicusStar oldStar = null;

                for (Int32 s = 0; s < KopernicusStar.Stars.Count; s++)
                {
                    KopernicusStar star = KopernicusStar.Stars[s];

                    if (star.sun == oldBody)
                    {
                        oldStar = star;
                    }
                    else
                    {
                        Test(SP, star);
                    }
                }

                Test(SP, oldStar);
            }

            Debug.Log("SigmaLog: LatePostCalculateTracking: TEMP2 PhysicsGlobals.SolarLuminosityAtHome = " + PhysicsGlobals.SolarLuminosityAtHome);
            PhysicsGlobals.SolarLuminosityAtHome = KopernicusStar.Current.shifter.solarLuminosity;
            Debug.Log("SigmaLog: LatePostCalculateTracking: NEW PhysicsGlobals.SolarLuminosityAtHome = " + PhysicsGlobals.SolarLuminosityAtHome);
            return;
            //for (Int32 n = 0; n < SPs.Length; n++)
            //{
            //    ModuleDeployableSolarPanel SP = SPs[n];
            //    Debug.Log("SigmaLog: LatePostCalculateTracking: SPs[" + n + "] = " + SP + ", DeployState = " + SP?.deployState);

            //    if (SP?.deployState == ModuleDeployablePart.DeployState.EXTENDED)
            //    {
            //        Vector3 normalized = (SP.trackingTransformLocal.position - SP.panelRotationTransform.position).normalized;
            //        LatePostCalculateTracking(SP, normalized);
            //    }
            //}
        }

        void Test(ModuleDeployableSolarPanel SP, KopernicusStar star)
        {
            SP.trackingTransformLocal = star.sun.transform;
            SP.trackingTransformScaled = star.sun.scaledBody.transform;

            PhysicsGlobals.SolarLuminosityAtHome = star.shifter.solarLuminosity;
            vessel.solarFlux = Flux(star);

            Vector3 normalized = (SP.trackingTransformLocal.position - SP.panelRotationTransform.position).normalized;

            FieldInfo Blocker = typeof(ModuleDeployableSolarPanel).GetField("blockingObject", BindingFlags.Instance | BindingFlags.NonPublic);
            String blocker = Blocker.GetValue(SP) as String;
            Boolean trackingLos = SP.CalculateTrackingLOS(normalized, ref blocker);
            Blocker.SetValue(SP, blocker);

            SP.PostCalculateTracking(trackingLos, normalized);
        }

        Double Flux(KopernicusStar star)
        {
            Debug.Log("SigmaLog: KopernicusSolarPanel.Flux: star = " + star?.sun);
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
                Debug.Log("SigmaLog: KopernicusSolarPanel.Flux:     PhysicsGlobals.SolarLuminosity = " + PhysicsGlobals.SolarLuminosity + ", PhysicsGlobals.SolarLuminosityAtHome = " + PhysicsGlobals.SolarLuminosityAtHome);
                Debug.Log("SigmaLog: KopernicusSolarPanel.Flux:     return ==> " + (PhysicsGlobals.SolarLuminosity / (12.5663706143592 * realDistanceToSun * realDistanceToSun)));
                return PhysicsGlobals.SolarLuminosity / (12.5663706143592 * realDistanceToSun * realDistanceToSun);
            }

            Debug.Log("SigmaLog: KopernicusSolarPanel.Flux:     return ==> " + 0);
            return 0;
        }

        public void EarlyLateUpdate()
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
                TimingManager.FixedUpdateAdd(TimingManager.TimingStage.FlightIntegrator, EarlyPostCalculateTracking);
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
            TimingManager.FixedUpdateRemove(TimingManager.TimingStage.FlightIntegrator, EarlyPostCalculateTracking);
            TimingManager.FixedUpdateRemove(TimingManager.TimingStage.Late, LatePostCalculateTracking);
        }
    }
}
