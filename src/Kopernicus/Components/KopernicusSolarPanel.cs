/**U
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

using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;
using KSP.Localization;
using System.Linq;
using System.Reflection;

namespace Kopernicus.Components
{
    public class KopernicusSolarPanel : PartModule
    {
        #region Declarations
        /// <summary>Unit to show in the UI, this is the only configurable field for this module. Default is actually set in OnLoad and if a rateUnit is set for ElectricCharge and this is not specified, the rateUnit will be used instead.</summary>
        [KSPField]
        public string EcUIUnit = string.Empty;

        /// <summary>Main PAW info label</summary>
        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "#Kopernicus_SolarPanelFixer_solarPanelStatus")]//Solar Panel Status
		public string panelStatus = string.Empty;
        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "#Kopernicus_SolarPanelFixer_energy")]//Energy Output
		public string panelStatusEnergy = string.Empty;
        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "#Kopernicus_SolarPanelFixer_exposure")]//exposure
		public string panelStatusSunAOA = string.Empty;
        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "#Kopernicus_SolarPanelFixer_wear")]//wear
		public string panelStatusWear = string.Empty;

        /// <summary>nominal rate at 1 UA (Kerbin distance from the sun)</summary>
        [KSPField(isPersistant = true)]
        public double nominalRate = 10.0; // doing this on the purpose of not breaking existing saves

        /// <summary>current state of the module</summary>
        [KSPField(isPersistant = true)]
        public PanelState state;

        /// <summary>tracked star/sun body index</summary>
        [KSPField(isPersistant = true)]
        public int trackedSunIndex = 0;

        /// <summary>has the player manually selected the star to be tracked ?</summary>
        [KSPField(isPersistant = true)]
        private bool manualTracking = false;

        /// <summary>
		/// Time based output degradation curve. Keys in hours, values in [0;1] range.
		/// Copied from the target solar panel module if supported and present.
		/// If defined in the SolarPanelFixer config, the target module curve will be overriden.
		/// </summary>
		[KSPField(isPersistant = true)]
        public FloatCurve timeEfficCurve;
        private static FloatCurve teCurve = null;
        private bool prefabDefinesTimeEfficCurve = false;

        /// <summary>UT of part creation in flight, used to evaluate the timeEfficCurve</summary>
        [KSPField(isPersistant = true)]
        public double launchUT = -1.0;

        /// <summary>internal object for handling the various hacks depending on the target solar panel module</summary>
        public SupportedPanel SolarPanel { get; private set; }

        /// <summary>current state of the module</summary>
        public bool isInitialized = false;

        /// <summary>can be used by external mods to get the current EC/s</summary>
        [KSPField]
        public double currentOutput;

        /// <summary>Identifies whether RealismOverhaul is enabled or not</summary>
        public bool ROFlag = false;

        // The following fields are local to FixedUpdate() but are shared for status string updates in Update()
        // Their value can be inconsistent, don't rely on them for anything else
        private double exposureFactor;
        private double wearFactor;
        private ExposureState exposureState;
        private string mainOccludingPart;
        private string rateFormat;
        private static StringBuilder sb=new StringBuilder(256);
        private ExposureState exposureStatus;


        public enum PanelState
        {
            Unknown = 0,
            Retracted,
            Extending,
            Extended,
            ExtendedFixed,
            Retracting,
            Static,
            Broken,
            Failure
        }

        public enum ExposureState
        {
            Disabled,
            Exposed,
            NotVisible,
            OccludedPart,
            BadOrientation
        }

        private const string prefix = "#Kopernicus_";
        public static string GetLoc(string template) => Localizer.Format(prefix + template);
        private static string SolarPanelFixer_occludedby = GetLoc("SolarPanelFixer_occludedby"); // "occluded by <<1>>"
        private static string SolarPanelFixer_notvisible  = GetLoc("SolarPanelFixer_notvisible"); // "Not Visible"
        private static string SolarPanelFixer_badorientation = GetLoc("SolarPanelFixer_badorientation"); // "bad orientation"
        private static string SolarPanelFixer_exposure = GetLoc("SolarPanelFixer_exposure"); // "exposure"
        private static string SolarPanelFixer_wear = GetLoc("SolarPanelFixer_wear"); // "wear"
        private static string SolarPanelFixer_sunDirect = GetLoc("SolarPanelFixer_sunDirect"); // "Sun Direct"
        private static string SolarPanelFixer_Selecttrackedstar = GetLoc("SolarPanelFixer_Selecttrackedstar"); // "Select tracked star"
        private static string SolarPanelFixer_SelectTrackingBody = GetLoc("SolarPanelFixer_SelectTrackingBody"); // "Select Tracking Body"
        private static string SolarPanelFixer_SelectTrackedstar_msg = GetLoc("SolarPanelFixer_SelectTrackedstar_msg"); // "Select the star you want to track with this solar panel."
        private static string SolarPanelFixer_Automatic = GetLoc("SolarPanelFixer_Automatic"); // "Automatic"
        private static string SolarPanelFixer_retracted = GetLoc("SolarPanelFixer_retracted"); // "retracted"
        private static string SolarPanelFixer_extending = GetLoc("SolarPanelFixer_extending"); // "extending"
        private static string SolarPanelFixer_retracting = GetLoc("SolarPanelFixer_retracting"); // "retracting"
        private static string SolarPanelFixer_broken = GetLoc("SolarPanelFixer_broken"); // "broken"
        private static string SolarPanelFixer_failure = GetLoc("SolarPanelFixer_failure"); // "failure"
        private static string SolarPanelFixer_invalidstate = GetLoc("SolarPanelFixer_invalidstate"); // "invalid state"
        private static string SolarPanelFixer_Trackedstar = GetLoc("SolarPanelFixer_Trackedstar"); // "Tracked star"
        private static string SolarPanelFixer_AutoTrack = GetLoc("SolarPanelFixer_AutoTrack"); // "[Auto] : "

        CelestialBody trackedSun;
        //declare internal float curves
        private static readonly FloatCurve temperatureEfficCurve= new FloatCurve();
        private static readonly FloatCurve AtmosphericAttenutationAirMassMultiplier = new FloatCurve();
        private static readonly FloatCurve AtmosphericAttenutationSolarAngleMultiplier = new FloatCurve();
        
        public static string resourceName=null;
        #endregion

        #region KSP/Unity methods + background update

        [KSPEvent(active = true, guiActive = true, guiName = "#Kopernicus_SolarPanelFixer_Selecttrackedstar")]//Select tracked star
        public void ManualTracking()
        {
            KopernicusStar[] orderedStars = KopernicusStar.Stars
                    .OrderBy(s => Vector3.Distance(vessel.transform.position, s.sun.position)).ToArray();
            Int32 stars = orderedStars.Count();
            DialogGUIBase[] options = new DialogGUIBase[stars + 1];
            // Assemble the buttons
            options[0] = new DialogGUIButton(SolarPanelFixer_Automatic, () => { manualTracking = false; }, true); //"Automatic"
            for (Int32 i = 0; i < stars; i++)
            {
                CelestialBody body = orderedStars[i].sun;
                options[i + 1] = new DialogGUIButton(body.bodyDisplayName.Replace("^N", ""), () =>
                {
                    manualTracking = true;
                    trackedSunIndex = body.flightGlobalsIndex;
                    SolarPanel.SetTrackedBody(body);
                    trackedSun = FlightGlobals.Bodies[trackedSunIndex];
                }, true);
            }

            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog(
                SolarPanelFixer_SelectTrackingBody,//"Select Tracking Body"
                SolarPanelFixer_SelectTrackedstar_msg,//"Select the star you want to track with this solar panel."
                SolarPanelFixer_Selecttrackedstar,//"Select tracked star"
                UISkinManager.GetSkin("MainMenuSkin"),
                options), false, UISkinManager.GetSkin("MainMenuSkin"));
        }
        public override void OnAwake()
        {
            if (teCurve == null) teCurve = new FloatCurve();
        }

        public override void OnLoad(ConfigNode node)
        {
            if (HighLogic.LoadedScene == GameScenes.LOADING)
            {
                prefabDefinesTimeEfficCurve = node.HasNode("timeEfficCurve");
                if (string.IsNullOrEmpty(EcUIUnit))
                {
                    EcUIUnit = "EC/s";
                }
            }
            if (SolarPanel == null && !GetSolarPanelModule())
                return;

            if (HighLogic.LoadedSceneIsEditor) return;

            // apply states changes we have done through automation
            if ((state == PanelState.Retracted || state == PanelState.Extended || state == PanelState.ExtendedFixed) && state != SolarPanel.GetState())
                SolarPanel.SetDeployedStateOnLoad(state);

            // apply reliability broken state and ensure we are correctly initialized (in case we are repaired mid-flight)
            // note : this rely on the fact that the reliability module is disabling the SolarPanelFixer monobehavior from OnStart, after OnLoad has been called
            if (!isEnabled)
            {
                ReliabilityEvent(true);
                OnStart(StartState.None);
            }
        }

        public override void OnStart(StartState startState)
        {
            temperatureEfficCurve.Add(4f, 1.2f, 0.0f, -0.0006f);
            temperatureEfficCurve.Add(300f, 1f, -0.0008f, -0.0008f);
            temperatureEfficCurve.Add(1200f, 0.134f, -0.00035f, -0.00035f);
            temperatureEfficCurve.Add(1900f, 0.02f, -3.72E-05f, -3.72E-05f);
            temperatureEfficCurve.Add(2500f, 0.01f, 0.0f, 0.0f);
            AtmosphericAttenutationAirMassMultiplier.Add(0f, 1f, 0f, 0f);
            AtmosphericAttenutationAirMassMultiplier.Add(5f, 0.982f, -0.010f, -0.010f);
            AtmosphericAttenutationAirMassMultiplier.Add(10f, 0.891f, -0.032f, -0.032f);
            AtmosphericAttenutationAirMassMultiplier.Add(15f, 0.746f, -0.025f, -0.025f);
            AtmosphericAttenutationAirMassMultiplier.Add(20f, 0.657f, -0.014f, -0.014f);
            AtmosphericAttenutationAirMassMultiplier.Add(30f, 0.550f, -0.0081f, -0.0081f);
            AtmosphericAttenutationAirMassMultiplier.Add(40f, 0.484f, -0.0053f, -0.0053f);
            AtmosphericAttenutationAirMassMultiplier.Add(50f, 0.439f, -0.0039f, -0.0039f);
            AtmosphericAttenutationAirMassMultiplier.Add(60f, 0.405f, -0.0030f, -0.0030f);
            AtmosphericAttenutationAirMassMultiplier.Add(80f, 0.357f, -0.0020f, -0.0020f);
            AtmosphericAttenutationAirMassMultiplier.Add(100f, 0.324f, -0.0014f, -0.0014f);
            AtmosphericAttenutationAirMassMultiplier.Add(150f, 0.271f, -0.00079f, -0.00079f);
            AtmosphericAttenutationAirMassMultiplier.Add(200f, 0.239f, -0.00052f, -0.00052f);
            AtmosphericAttenutationAirMassMultiplier.Add(300f, 0.200f, -0.00029f, -0.00029f);
            AtmosphericAttenutationAirMassMultiplier.Add(500f, 0.159f, -0.00014f, -0.00014f);
            AtmosphericAttenutationAirMassMultiplier.Add(800f, 0.130f, -0.00007f, -0.00007f);
            AtmosphericAttenutationAirMassMultiplier.Add(1200f, 0.108f, -0.00004f, 0f);
            AtmosphericAttenutationSolarAngleMultiplier.Add(0f, 1f, 0f, 0f);
            AtmosphericAttenutationSolarAngleMultiplier.Add(15f, 0.985f, -0.0020f, -0.0020f);
            AtmosphericAttenutationSolarAngleMultiplier.Add(30f, 0.940f, -0.0041f, -0.0041f);
            AtmosphericAttenutationSolarAngleMultiplier.Add(45f, 0.862f, -0.0064f, -0.0064f);
            AtmosphericAttenutationSolarAngleMultiplier.Add(60f, 0.746f, -0.0092f, -0.0092f);
            AtmosphericAttenutationSolarAngleMultiplier.Add(75f, 0.579f, -0.0134f, -0.0134f);
            AtmosphericAttenutationSolarAngleMultiplier.Add(90f, 0.336f, -0.0185f, -0.0185f);
            AtmosphericAttenutationSolarAngleMultiplier.Add(105f, 0.100f, -0.008f, -0.008f);
            AtmosphericAttenutationSolarAngleMultiplier.Add(120f, 0.050f, 0f, 0f);

            // don't break tutorial scenarios
            // TODO : does this actually work ?
            if (DisableScenario(this)) return;

            if (SolarPanel == null && !GetSolarPanelModule())
            {
                isInitialized = true;
                return;
            }

            // disable everything if the target module data/logic acquisition has failed
            if (!SolarPanel.OnStart(isInitialized, ref nominalRate))
                enabled = isEnabled = moduleIsEnabled = false;

            isInitialized = true;

            try
            {
                trackedSun = FlightGlobals.Bodies[trackedSunIndex];
            }
            catch
            {

            }

            if (!prefabDefinesTimeEfficCurve)
                timeEfficCurve = SolarPanel.GetTimeCurve();

            if (HighLogic.LoadedSceneIsFlight && launchUT < 0.0)
                launchUT = Planetarium.GetUniversalTime();

            // setup star selection GUI
            Events["ManualTracking"].active = KopernicusStar.Stars.Count > 1 && SolarPanel.IsTracking;
            Events["ManualTracking"].guiActive = state == PanelState.Extended || state == PanelState.ExtendedFixed || state == PanelState.Static;

            // setup target module animation for custom star tracking
            SolarPanel.SetTrackedBody(trackedSun);

            // set how many decimal points are needed to show the panel Ec output in the UI
            rateFormat = "F3";
        }

        public void Update()
        {
            // sanity check
            if (SolarPanel == null) return;

            // call Update specfic handling, if any
            SolarPanel.OnUpdate();

            // Do nothing else in the editor
            if (HighLogic.LoadedSceneIsEditor) return;

            // Update tracked body selection button (Kopernicus multi-star support)
            if (Events["ManualTracking"].active && (state == PanelState.Extended || state == PanelState.ExtendedFixed || state == PanelState.Static))
            {
                Events["ManualTracking"].guiActive = true;
                Events["ManualTracking"].guiName = BuildString(SolarPanelFixer_Trackedstar + " ", manualTracking ? ": " : SolarPanelFixer_AutoTrack, FlightGlobals.Bodies[trackedSunIndex].bodyDisplayName.Replace("^N", ""));//"Tracked star"[Auto] : "
            }
            else
            {
                Events["ManualTracking"].guiActive = false;
            }

            // Update main status field visibility
            if (state == PanelState.Failure || state == PanelState.Unknown)
            {
                Fields["panelStatusEnergy"].guiActive = false;
                Fields["panelStatusSunAOA"].guiActive = false;
                if (ROFlag)
                {
                    Fields["panelStatusWear"].guiActive = true;
                    panelStatusWear = "100 %".ToString();
                }
            }
            Fields["panelStatusEnergy"].guiActive = false;
            Fields["panelStatusSunAOA"].guiActive = false;
            switch (exposureState)
            {
                case ExposureState.NotVisible:
                    panelStatus = "<color=#ff2222>" + SolarPanelFixer_notvisible + "</color>";//not visible
                    break;
                case ExposureState.OccludedPart:
                    panelStatus = BuildString("<color=#ff2222>", Localizer.Format(SolarPanelFixer_occludedby, mainOccludingPart), "</color>");//occluded by 
                    break;
                case ExposureState.BadOrientation:
                    panelStatus = "<color=#ff2222>" + SolarPanelFixer_badorientation + "</color>";//bad orientation
                    break;
                case ExposureState.Disabled:
                    switch (state)
                    {
                        case PanelState.Retracted: panelStatus = SolarPanelFixer_retracted; break;//"retracted"
                        case PanelState.Extending: panelStatus = SolarPanelFixer_extending; break;//"extending"
                        case PanelState.Retracting: panelStatus = SolarPanelFixer_retracting; break;//"retracting"
                        case PanelState.Broken: panelStatus = SolarPanelFixer_broken; break;//"broken"
                        case PanelState.Failure: panelStatus = SolarPanelFixer_failure; break;//"failure"
                        case PanelState.Unknown: panelStatus = SolarPanelFixer_invalidstate; break;//"invalid state"
                    }
                    break;
                case ExposureState.Exposed:
                    Fields["panelStatusEnergy"].guiActive = true;
                    Fields["panelStatusSunAOA"].guiActive = true;
                    panelStatus = "<color=#eaff56>" + SolarPanelFixer_sunDirect + "</color>";//"Sun Direct"
                    sb.Length = 0;
                    double num = 0;
                    if (ROFlag)
                    {
                        if (Double.Parse(currentOutput.ToString(rateFormat)) < 1.0)
                        {
                            num = currentOutput * 1000;
                            EcUIUnit = "W";
                        }
                        else
                        {
                            num = currentOutput;
                            EcUIUnit = "KW";
                        }
                    }
                    else
                        num = currentOutput;
                    if (resourceName == "ThermalPower")
                    {
                        EcUIUnit = "TP/s";
                    }
                    sb.Append(num.ToString(rateFormat));
                    sb.Append(" ");
                    sb.Append(EcUIUnit);
                    panelStatusEnergy = sb.ToString();

                    sb.Length = 0;
                    sb.Append(exposureFactor.ToString("P0"));
                    panelStatusSunAOA = sb.ToString();

                    if (wearFactor < 1.0 && ROFlag)
                    {
                        Fields["panelStatusWear"].guiActive = true;
                        sb.Length = 0;
                        sb.Append((1.0 - wearFactor).ToString("P0"));
                        panelStatusWear = sb.ToString();
                    }
                    break;
            }
        }

        public void FixedUpdate()
        {
            if (SolarPanel == null)
            {
                return;
            }

            if (HighLogic.LoadedSceneIsFlight && vessel != null && vessel.situation == Vessel.Situations.PRELAUNCH)
                launchUT = Planetarium.GetUniversalTime();

            // can't produce anything if not deployed, broken, etc
            PanelState newState = SolarPanel.GetState();
            if (state != newState)
            {
                state = newState;
            }
            if (!(state == PanelState.Extended || state == PanelState.ExtendedFixed || state == PanelState.Static))
            {
                exposureState = ExposureState.Disabled;
                currentOutput = 0.0;
                return;
            }

            // do nothing else in editor
            if (HighLogic.LoadedSceneIsEditor)
            {
                return;
            }

            Vessel vesselActive = FlightGlobals.ActiveVessel;
            Vector3d position = VesselPosition(vesselActive);
            List<KopernicusStar> starList=new List<KopernicusStar>();

            Vector3d direction;
            double distance;
            for (Int32 s = 0; s < KopernicusStar.Stars.Count; s++)
            {
                KopernicusStar starL=KopernicusStar.Stars[s];
                Vector3d dIRECTION=(starL.sun.position-position).normalized;
                /*double Block=SolarPanel.GetOccludedFactor(dIRECTION,out trackedPart);
                double Cosine=SolarPanel.GetCosineFactor(dIRECTION);*/
                double factor= IsBodyVisible(vessel, position, starL.sun, GetLargeBodies(position), out direction, out distance) ? 1.0 : 0.0;
                //if (Cosine * Block == 0)
                if (factor == 0.0)
                {
                    continue;
                }
                else
                {
                    starList.Add(starL);
                }
            }

            if (starList.Count > 0)
            {
                KopernicusStar brightestStar = KopernicusStar.GetBrightest(position, starList);
                if (!manualTracking && trackedSun != brightestStar.sun)
                {
                    trackedSunIndex = brightestStar.sun.flightGlobalsIndex;
                    trackedSun = brightestStar.sun;
                    SolarPanel.SetTrackedBody(trackedSun);
                }
            }
            else
            {
                KopernicusStar[] orderedStars = KopernicusStar.Stars
                    .OrderBy(s => Vector3.Distance(vessel.transform.position, s.sun.position)).ToArray();
                if (!manualTracking && trackedSun != orderedStars[0].sun)
                {
                    trackedSunIndex = orderedStars[0].sun.flightGlobalsIndex;
                    trackedSun = orderedStars[0].sun;
                    SolarPanel.SetTrackedBody(trackedSun);
                }
            }

            exposureFactor = 0.0;
            Double totalFlux = 0;
            Double totalFlow = 0;
            double totalSunExposure = 0.0;
            // iterate over all stars, compute the exposure factor
            for (Int32 s = 0; s < KopernicusStar.Stars.Count; s++)
            {
                KopernicusStar star = KopernicusStar.Stars[s];
                // Use this star
                star.shifter.ApplyPhysics();

                Vector3d sunDirection = (star.sun.position - position).normalized;

                // Add to TotalFlux and EC tally
                float panelEffectivness = 0;
                //Now for some fancy atmospheric math
                float atmoDensityMult = 1;
                float atmoAngleMult = 1;
                float tempMult = 1;
                if (vessel.atmDensity > 0)
                {
                    float horizonAngle = (float)Math.Acos(FlightGlobals.currentMainBody.Radius /
                        (FlightGlobals.currentMainBody.Radius +FlightGlobals.ship_altitude));

                    float sunZenithAngleDeg = Vector3.Angle(FlightGlobals.upAxis, star.sun.position);

                    Double gravAccelParameter = (vessel.mainBody.gravParameter /
                        Math.Pow(vessel.mainBody.Radius + FlightGlobals.ship_altitude, 2));

                    float massOfAirColumn =(float)(FlightGlobals.getStaticPressure() / gravAccelParameter);

                    tempMult = temperatureEfficCurve.Evaluate((float)vessel.atmosphericTemperature);
                    atmoDensityMult = AtmosphericAttenutationAirMassMultiplier.Evaluate(massOfAirColumn);
                    atmoAngleMult = AtmosphericAttenutationSolarAngleMultiplier.Evaluate(sunZenithAngleDeg);
                }

                double starFlux = 0;
                //Calculate flux
                double starFluxAtHome = 0;
                if (PhysicsGlobals.SolarLuminosityAtHome != 0)
                {
                    starFluxAtHome = 1360 / PhysicsGlobals.SolarLuminosityAtHome;
                }
                starFlux = star.CalculateFluxAt(vessel) * starFluxAtHome;

                totalFlux += starFlux;

                double sunCosineFactor = 0.0;
                double sunOccludedFactor = 0.0;
                string occludingPart = null;
                // Compute final aggregate exposure factor
                double sunExposureFactor = 0.0;


                CalExposureAndCosin(star, sunDirection, out sunCosineFactor, out sunOccludedFactor, out occludingPart, out exposureStatus);
                sunExposureFactor = sunCosineFactor * sunOccludedFactor;
                totalSunExposure += sunExposureFactor;
                if (star.sun.Equals(trackedSun))
                {
                    exposureFactor = sunExposureFactor;
                }

                if ((sunExposureFactor != 0) && (tempMult != 0) && (atmoAngleMult != 0) && (atmoDensityMult != 0))
                {
                    panelEffectivness = ((float)nominalRate / 24.3999996185303f) / 56.37091313591871f * (float)sunExposureFactor * tempMult *
                                            atmoAngleMult *
                                            atmoDensityMult; //56.blabla is a weird constant we use to turn flux into EC
                }
                if (starFluxAtHome > 0)
                {
                    totalFlow += ((starFlux) * panelEffectivness) /
                                 (1360 / PhysicsGlobals.SolarLuminosityAtHome);
                }

            }
            if ((exposureStatus != ExposureState.Exposed) && (totalSunExposure < 0.01))
            {
                exposureState = exposureStatus;
            }
            KopernicusStar.CelestialBodies[trackedSun].shifter.ApplyPhysics();
            vessel.solarFlux = totalFlux;

            // get wear factor (time based output degradation)
            wearFactor = 1.0;
            if (timeEfficCurve?.Curve.keys.Length > 1)
                wearFactor = Clamp(timeEfficCurve.Evaluate((float)((Planetarium.GetUniversalTime() - launchUT) / 3600.0)), 0.0, 1.0);

            // get final output rate in EC/s
            if (ROFlag)
                currentOutput = totalFlow * wearFactor;
            else
                currentOutput = totalFlow;

            double trackedSunVisiblefactor= IsBodyVisible(vessel, position, trackedSun, GetLargeBodies(position), out direction, out distance) ? 1.0 : 0.0;
            // ignore very small outputs
            if (currentOutput < 1e-10)
            {
                currentOutput = 0.0;
                if (exposureStatus == ExposureState.OccludedPart)
                {
                    if (trackedSunVisiblefactor == 0)
                        exposureStatus = ExposureState.NotVisible;
                    exposureState = exposureStatus;
                }
                else
                {
                    exposureState = ExposureState.NotVisible;
                }
                if (wearFactor == 0)
                {
                    exposureState = ExposureState.Disabled;
                    state = PanelState.Failure;
                    Fields["panelStatusEnergy"].guiActive = false;
                    Fields["panelStatusSunAOA"].guiActive = false;
                }
            }
            else
            {
                exposureState = ExposureState.Exposed;
                if (resourceName == null)
                {
                    resourceName = "ElectricCharge";
                }
                part.RequestResource(resourceName, (-currentOutput) * TimeWarp.fixedDeltaTime);
            }
        }

        #endregion

        #region Other methods
        public bool GetSolarPanelModule()
        {
            // handle the possibility of multiple solar panel and SolarPanelFixer modules on the part
            List<KopernicusSolarPanel> fixerModules = new List<KopernicusSolarPanel>();
            foreach (PartModule pm in part.Modules)
            {
                if (pm is KopernicusSolarPanel fixerModule)
                    fixerModules.Add(fixerModule);
            }
            foreach (PartModule pm in part.Modules)
            {
                if (fixerModules.Exists(p => p.SolarPanel != null && p.SolarPanel.TargetModule == pm))
                    continue;
                switch (pm.moduleName)
                {
                    case "ModuleROSolar": ROFlag = true; break;
                    case "ModuleROSolarPanel": ROFlag = true; break;
                }
            }

            // find the module based on explicitely supported modules
            foreach (PartModule pm in part.Modules)
            {
                if (fixerModules.Exists(p => p.SolarPanel != null && p.SolarPanel.TargetModule == pm))
                    continue;
                // mod supported modules
                switch (pm.moduleName)
                {
                    case "ModuleCurvedSolarPanel": SolarPanel = new NFSCurvedPanel(); break;
                    case "SSTUSolarPanelStatic": SolarPanel = new SSTUStaticPanel(); break;
                    case "SSTUSolarPanelDeployable": SolarPanel = new SSTUVeryComplexPanel(); break;
                    case "SSTUModularPart": SolarPanel = new SSTUVeryComplexPanel(); break;
                    case "ModuleROSolar": SolarPanel = new ROConfigurablePanel(); break;
                    case "ModuleROSolarPanel": SolarPanel = new ROConfigurablePanel(); break;
                    default:
                        if (pm is ModuleDeployableSolarPanel)
                            SolarPanel = new StockPanel(); break;
                }
                if (SolarPanel != null)
                {
                    SolarPanel.OnLoad(this, pm);
                    break;
                }
            }

            if (SolarPanel == null)
            {
                Debug.Log("Could not find a supported solar panel module, disabling SolarPanelFixer module...");
                enabled = isEnabled = moduleIsEnabled = false;
                return false;
            }

            return true;
        }

        public void ToggleState()
        {
            SolarPanel.ToggleState(state);
        }

        public void ReliabilityEvent(bool isBroken)
        {
            state = isBroken ? PanelState.Failure : SolarPanel.GetState();
            SolarPanel.Break(isBroken);
        }

        public static string BuildString(string a, string b, string c)
        {
            sb.Length = 0;
            sb.Append(a);
            sb.Append(b);
            sb.Append(c);
            return sb.ToString();
        }

        /// <summary>return true if 'body' is visible from 'vesselPos'. Very fast method.</summary>
		/// <param name="occludingBodies">the bodies that will be checked for occlusion</param>
		/// <param name="bodyDir">normalized vector from vessel to body</param>
		/// <param name="bodyDist">distance from vessel to body surface</param>
		/// <returns></returns>
        public static bool IsBodyVisible(Vessel vessel, Vector3d vesselPos, CelestialBody body, List<CelestialBody> occludingBodies, out Vector3d bodyDir, out double bodyDist)
        {
            // generate ray parameters
            bodyDir = body.position - vesselPos;
            bodyDist = bodyDir.magnitude;
            bodyDir /= bodyDist;
            bodyDist -= body.Radius;

            // for very small bodies the analytic method is very unreliable at high latitudes
            // we use a physic raycast (a lot slower)
            if (Landed(vessel) && vessel.mainBody.Radius < 100000.0 && (vessel.latitude < -45.0 || vessel.latitude > 45.0))
                return RaytracePhysic(vessel, vesselPos, body.position, body.Radius);

            // check if the ray intersect one of the provided bodies
            foreach (CelestialBody occludingBody in occludingBodies)
            {
                if (occludingBody == body) continue;
                if (!RayAvoidBody(vesselPos, bodyDir, bodyDist, occludingBody)) return false;
            }

            return true;
        }

        private static int planetaryLayerMask = int.MaxValue;
        public static bool RaytracePhysic(Vessel vessel, Vector3d vesselPos, Vector3d end, double endNegOffset = 0.0)
        {
            // for unloaded vessels, position in scaledSpace is 1 fixedUpdate frame desynchronized :
            if (!vessel.loaded)
                vesselPos += vessel.mainBody.position - vessel.mainBody.getTruePositionAtUT(Planetarium.GetUniversalTime() + TimeWarp.fixedDeltaTime);

            // convert vessel position to scaled space
            ScaledSpace.LocalToScaledSpace(ref vesselPos);
            ScaledSpace.LocalToScaledSpace(ref end);
            Vector3d dir = end - vesselPos;
            if (endNegOffset > 0) dir -= dir.normalized * (endNegOffset * ScaledSpace.InverseScaleFactor);

            return !Physics.Raycast(vesselPos, dir, (float)dir.magnitude, planetaryLayerMask);
        }

        public static bool RayAvoidBody(Vector3d start, Vector3d dir, double dist, CelestialBody body)
        {
            // ray from origin to body center
            Vector3d diff = body.position - start;

            // projection of origin->body center ray over the raytracing direction
            double k = Vector3d.Dot(diff, dir);

            // the ray doesn't hit body if its minimal analytical distance along the ray is less than its radius
            // simplified from 'start + dir * k - body.position'
            return k < 0.0 || k > dist || (dir * k - diff).magnitude > body.Radius;
        }

        public static bool Landed(Vessel v)
        {
            if (v.loaded) return v.Landed || v.Splashed;
            else return v.protoVessel.landed || v.protoVessel.splashed;
        }

        public static string EllipsisMiddle(string s, int len)
        {
            if (s.Length > len)
            {
                len = (len - 3) / 2;
                return BuildString(s.Substring(0, len), "...", s.Substring(s.Length - len));
            }
            return s;
        }

        /// <summary>return the list of bodies whose apparent diameter is greater than 10 arcmin from the 'position' POV</summary>
        public static List<CelestialBody> GetLargeBodies(Vector3d position)
        {
            List <CelestialBody> visibleBodies = new List<CelestialBody>();
            foreach (CelestialBody occludingBody in FlightGlobals.Bodies)
            {
                // if apparent diameter > ~10 arcmin (~0.003 radians), consider the body for occlusion checks
                // real apparent diameters at earth : sun/moon ~ 30 arcmin, Venus ~ 1 arcmin max
                double apparentSize = (occludingBody.Radius * 2.0) / (occludingBody.position - position).magnitude;
                if (apparentSize > 0.003) visibleBodies.Add(occludingBody);
            }
            return visibleBodies;
        }

        public static Vector3d VesselPosition(Vessel v)
        {
            // the issue
            //   - GetWorldPos3D() return mainBody position for a few ticks after scene changes
            //   - we can detect that, and fall back to evaluating position from the orbit
            //   - orbit is not valid if the vessel is landed, and for a tick on prelaunch/staging/decoupling
            //   - evaluating position from latitude/longitude work in all cases, but is probably the slowest method

            // get vessel position
            Vector3d pos = v.GetWorldPos3D();

            // during scene changes, it will return mainBody position
            if (Vector3d.SqrMagnitude(pos - v.mainBody.position) < 1.0)
            {
                // try to get it from orbit
                pos = v.orbit.getPositionAtUT(Planetarium.GetUniversalTime());

                // if the orbit is invalid (landed, or 1 tick after prelaunch/staging/decoupling)
                if (double.IsNaN(pos.x))
                {
                    // get it from lat/long (work even if it isn't landed)
                    pos = v.mainBody.GetWorldSurfacePosition(v.latitude, v.longitude, v.altitude);
                }
            }
            // victory
            return pos;
        }

        private static readonly BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        ///<summary> Returns the value of a private field via reflection </summary>
        public static T ReflectionValue<T>(object instance, string field_name)
        {
            return (T)instance.GetType().GetField(field_name, flags).GetValue(instance);
        }
        public static T ReflectionValue<T>(PartModule m, string value_name)
        {
            return (T)m.GetType().GetField(value_name, flags).GetValue(m);
        }
        public static void ReflectionCall(object m, string call_name)
        {
            m.GetType().GetMethod(call_name, flags).Invoke(m, null);
        }

        ///<summary>
        /// set a value from a module using reflection
        /// note: useful when the module is from another assembly, unknown at build time
        /// note: useful when the value isn't persistent
        /// note: this function break hard when external API change, by design
        ///</summary>
        public static void ReflectionValue<T>(PartModule m, string value_name, T value)
        {
            m.GetType().GetField(value_name, flags).SetValue(m, value);
        }

        ///<summary> Sets the value of a private field via reflection </summary>
        public static void ReflectionValue<T>(object instance, string value_name, T value)
        {
            instance.GetType().GetField(value_name, flags).SetValue(instance, value);
        }

        public static string GetString(ProtoPartModuleSnapshot m, string name, string def_value = "")
        {
            string s = m.moduleValues.GetValue( name );
            return s ?? def_value;
        }

        ///<summary>return true if a tutorial scenario or making history mission is active</summary>
        public static bool IsScenario()
        {
            return HighLogic.CurrentGame.Mode == Game.Modes.SCENARIO
                || HighLogic.CurrentGame.Mode == Game.Modes.SCENARIO_NON_RESUMABLE
                || HighLogic.CurrentGame.Mode == Game.Modes.MISSION_BUILDER
                || HighLogic.CurrentGame.Mode == Game.Modes.MISSION;
        }

        ///<summary>disable the module and return true if a tutorial scenario is active</summary>
        public static bool DisableScenario(PartModule m)
        {
            if (IsScenario())
            {
                m.enabled = false;
                m.isEnabled = false;
                return true;
            }
            return false;
        }

        ///<summary>clamp a value</summary>
        public static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        ///<summary>Calculate cosine factor and masking factor</summary>
        public void CalExposureAndCosin(KopernicusStar star, Vector3d sunDirection, out double sunCosineFactor, out double sunOccludedFactor, out string occludingPart, out ExposureState exposureStats)
        {
            sunCosineFactor = 0.0;
            sunOccludedFactor = 0.0;
            occludingPart = null;
            exposureStats = exposureStatus;
            // Get the cosine factor (alignement between the sun and the panel surface)
            sunCosineFactor = SolarPanel.GetCosineFactor(sunDirection);
            if (sunCosineFactor == 0.0)
            {
                // If this is the tracked sun and the panel is not oriented toward the sun, update the gui info string.
                if (star.sun == trackedSun)
                {
                    exposureStats = ExposureState.BadOrientation;
                }
            }
            else
            {
                // The panel is oriented toward the sun, do a physic raycast to check occlusion from parts, terrain, buildings...
                sunOccludedFactor = SolarPanel.GetOccludedFactor(sunDirection, out occludingPart);
                // If this is the tracked sun and the panel is occluded, update the gui info string. 
                if (star.sun == trackedSun)
                {
                    if (occludingPart != null)
                    {
                        exposureStats = ExposureState.OccludedPart;
                        mainOccludingPart = EllipsisMiddle(occludingPart, 15);
                    }
                    else
                    {
                        exposureStats = ExposureState.NotVisible;
                    }
                }
            }
        }
        #endregion

        #region Abstract class for common interaction with supported PartModules
        public abstract class SupportedPanel
        {
            /// <summary>Reference to the SolarPanelFixer, must be set from OnLoad</summary>
            protected KopernicusSolarPanel fixerModule;

            /// <summary>Reference to the target module</summary>
            public abstract PartModule TargetModule { get; }

            /// <summary>
            /// Will be called by the SolarPanelFixer OnLoad, must set the partmodule reference.
            /// GetState() must be able to return the correct state after this has been called
            /// </summary>
            public abstract void OnLoad(KopernicusSolarPanel fixerModule, PartModule targetModule);

            /// <summary> Main inititalization method called from OnStart, every hack we do must be done here (In particular the one preventing the target module from generating EC)</summary>
            /// <param name="initialized">will be true if the method has already been called for this module (OnStart can be called multiple times in the editor)</param>
            /// <param name="nominalRate">nominal rate at 1AU</param>
            /// <returns>must return false is something has gone wrong, will disable the whole module</returns>
            public abstract bool OnStart(bool initialized, ref double nominalRate);

            /// <summary>Must return a [0;1] scalar evaluating the local occlusion factor (usually with a physic raycast already done by the target module)</summary>
            /// <param name="occludingPart">if the occluding object is a part, name of the part. MUST return null in all other cases.</param>
            /// <param name="analytic">if true, the returned scalar must account for the given sunDir, so we can't rely on the target module own raycast</param>
            public abstract double GetOccludedFactor(Vector3d sunDir, out string occludingPart);

            /// <summary>Must return a [0;1] scalar evaluating the angle of the given sunDir on the panel surface (usually a dot product clamped to [0;1])</summary>
            /// <param name="analytic">if true and the panel is orientable, the returned scalar must be the best possible output (must use the rotation around the pivot)</param>
            public abstract double GetCosineFactor(Vector3d sunDir, bool analytic = false);

            /// <summary>must return the state of the panel, must be able to work before OnStart has been called</summary>
            public abstract PanelState GetState();

            /// <summary>Can be overridden if the target module implement a time efficiency curve. Keys are in hours, values are a scalar in the [0:1] range.</summary>
            public virtual FloatCurve GetTimeCurve() { return new FloatCurve(new Keyframe[] { new Keyframe(0f, 1f) }); }

            /// <summary>Called at Update(), can contain target module specific hacks</summary>
            public virtual void OnUpdate() { }

            /// <summary>Is the panel a sun-tracking panel</summary>
            public virtual bool IsTracking => false;

            /// <summary>Kopernicus stars support : must set the animation tracked body</summary>
            public virtual void SetTrackedBody(CelestialBody body) { }

            /// <summary>Reliability : specific hacks for the target module that must be applied when the panel is disabled by a failure</summary>
            public virtual void Break(bool isBroken) { }

            /// <summary>Automation : override this with "return false" if the module doesn't support automation when loaded</summary>
            public virtual bool SupportAutomation(PanelState state)
            {
                switch (state)
                {
                    case PanelState.Retracted:
                    case PanelState.Extending:
                    case PanelState.Extended:
                    case PanelState.Retracting:
                        return true;
                    default:
                        return false;
                }
            }

            /// <summary>Automation : override this with "return false" if the module doesn't support automation when unloaded</summary>
            public virtual bool SupportProtoAutomation(ProtoPartModuleSnapshot protoModule)
            {
                switch (GetString(protoModule, "state"))
                {
                    case "Retracted":
                    case "Extended":
                        return true;
                    default:
                        return false;
                }
            }

            /// <summary>Automation : this must work when called on the prefab module</summary>
            public virtual bool IsRetractable() { return false; }

            /// <summary>Automation : must be implemented if the panel is extendable</summary>
            public virtual void Extend() { }

            /// <summary>Automation : must be implemented if the panel is retractable</summary>
            public virtual void Retract() { }

            ///<summary>Automation : Called OnLoad, must set the target module persisted extended/retracted fields to reflect changes done trough automation while unloaded</summary>
            public virtual void SetDeployedStateOnLoad(PanelState state) { }

            ///<summary>Automation : convenience method</summary>
            public void ToggleState(PanelState state)
            {
                switch (state)
                {
                    case PanelState.Retracted: Extend(); return;
                    case PanelState.Extended: Retract(); return;
                }
            }
        }

        private abstract class SupportedPanel<T> : SupportedPanel where T : PartModule
        {
            public T panelModule;
            public override PartModule TargetModule => panelModule;
        }

        #endregion

        #region Stock module support (ModuleDeployableSolarPanel)
        // stock solar panel module support
        // - we don't support the temperatureEfficCurve
        // - we override the stock UI
        // - we still reuse most of the stock calculations
        // - we let the module fixedupdate/update handle animations/suncatching
        // - we prevent stock EC generation by reseting the reshandler rate
        // - we don't support cylindrical/spherical panel types
        private class StockPanel : SupportedPanel<ModuleDeployableSolarPanel>
        {
            private Transform sunCatcherPosition;   // middle point of the panel surface (usually). Use only position, panel surface direction depend on the pivot transform, even for static panels.
            private Transform sunCatcherPivot;      // If it's a tracking panel, "up" is the pivot axis and "position" is the pivot position. In any case "forward" is the panel surface normal.

            public override void OnLoad(KopernicusSolarPanel fixerModule, PartModule targetModule)
            {
                this.fixerModule = fixerModule;
                panelModule = (ModuleDeployableSolarPanel)targetModule;
            }

            public override bool OnStart(bool initialized, ref double nominalRate)
            {
                // hide stock ui
                panelModule.Fields["sunAOA"].guiActive = false;
                panelModule.Fields["flowRate"].guiActive = false;
                panelModule.Fields["status"].guiActive = false;

                resourceName = panelModule.resourceName;
                
                if (sunCatcherPivot == null)
                    sunCatcherPivot = panelModule.part.FindModelComponent<Transform>(panelModule.pivotName);
                if (sunCatcherPosition == null)
                    sunCatcherPosition = panelModule.part.FindModelTransform(panelModule.secondaryTransformName);

                if (sunCatcherPosition == null)
                {
                    Debug.Log("Could not find suncatcher transform `{0}` in part `{1}`");
                    return false;
                }

                // avoid rate lost due to OnStart being called multiple times in the editor
                if (panelModule.resHandler.outputResources[0].rate == 0.0)
                    return true;

                nominalRate = panelModule.resHandler.outputResources[0].rate;
                // reset target module rate
                // - This can break mods that evaluate solar panel output for a reason or another (eg: AmpYear, BonVoyage).
                //   We fix that by exploiting the fact that resHandler was introduced in KSP recently, and most of
                //   these mods weren't updated to reflect the changes or are not aware of them, and are still reading
                //   chargeRate. However the stock solar panel ignore chargeRate value during FixedUpdate.
                //   So we only reset resHandler rate.
                panelModule.resHandler.outputResources[0].rate = 0.0;

                return true;
            }

            // akwardness award : stock timeEfficCurve use 24 hours days (1/(24*60/60)) as unit for the curve keys, we convert that to hours
            public override FloatCurve GetTimeCurve()
            {

                if (panelModule.timeEfficCurve?.Curve.keys.Length > 1)
                {
                    FloatCurve timeCurve = new FloatCurve();
                    foreach (Keyframe key in panelModule.timeEfficCurve.Curve.keys)
                        timeCurve.Add(key.time * 24f, key.value, key.inTangent * (1f / 24f), key.outTangent * (1f / 24f));
                    return timeCurve;
                }
                return base.GetTimeCurve();
            }

            // detect occlusion from the scene colliders using the stock module physics raycast, or our own if analytic mode = true
            public override double GetOccludedFactor(Vector3d sunDir, out string occludingPart)
            {
                double occludingFactor = 1.0;
                occludingPart = null;
                RaycastHit raycastHit;
                if (sunCatcherPosition == null)
                    sunCatcherPosition = panelModule.part.FindModelTransform(panelModule.secondaryTransformName);

                Physics.Raycast(sunCatcherPosition.position + (sunDir * panelModule.raycastOffset), sunDir, out raycastHit, 10000f);

                if (raycastHit.collider != null)
                {
                    Part blockingPart = Part.GetComponentUpwards<Part>(raycastHit.collider.gameObject);
                    if (blockingPart != null)
                    {
                        // avoid panels from occluding themselves
                        if (blockingPart == panelModule.part)
                            return occludingFactor;

                        occludingPart = blockingPart.partInfo.title;
                    }
                    occludingFactor = 0.0;
                }
                return occludingFactor;
            }

            // we use the current panel orientation, only doing it ourself when analytic = true
            public override double GetCosineFactor(Vector3d sunDir, bool analytic = false)
            {
                switch (panelModule.panelType)
                {
                    case ModuleDeployableSolarPanel.PanelType.FLAT:
                        if (!analytic)
                            return Math.Max(Vector3d.Dot(sunDir, panelModule.trackingDotTransform.forward), 0.0);

                        if (panelModule.isTracking)
                            return Math.Cos(1.57079632679 - Math.Acos(Vector3d.Dot(sunDir, sunCatcherPivot.up)));
                        else
                            return Math.Max(Vector3d.Dot(sunDir, sunCatcherPivot.forward), 0.0);

                    case ModuleDeployableSolarPanel.PanelType.CYLINDRICAL:
                        return Math.Max((1.0 - Math.Abs(Vector3d.Dot(sunDir, panelModule.trackingDotTransform.forward))) * (1.0 / Math.PI), 0.0);
                    case ModuleDeployableSolarPanel.PanelType.SPHERICAL:
                        return 0.25;
                    default:
                        return 0.0;
                }
            }

            public override PanelState GetState()
            {
                // Detect modified TotalEnergyRate (B9PS switching of the stock module or ROSolar built-in switching)
                if (panelModule.resHandler.outputResources[0].rate != 0.0)
                {
                    OnStart(false, ref fixerModule.nominalRate);
                }

                if (!panelModule.useAnimation)
                {
                    if (panelModule.deployState == ModuleDeployablePart.DeployState.BROKEN)
                        return PanelState.Broken;

                    return PanelState.Static;
                }

                switch (panelModule.deployState)
                {
                    case ModuleDeployablePart.DeployState.EXTENDED:
                        if (!IsRetractable()) return PanelState.ExtendedFixed;
                        return PanelState.Extended;
                    case ModuleDeployablePart.DeployState.RETRACTED: return PanelState.Retracted;
                    case ModuleDeployablePart.DeployState.RETRACTING: return PanelState.Retracting;
                    case ModuleDeployablePart.DeployState.EXTENDING: return PanelState.Extending;
                    case ModuleDeployablePart.DeployState.BROKEN: return PanelState.Broken;
                }
                return PanelState.Unknown;
            }

            public override void SetDeployedStateOnLoad(PanelState state)
            {
                switch (state)
                {
                    case PanelState.Retracted:
                        panelModule.deployState = ModuleDeployablePart.DeployState.RETRACTED;
                        break;
                    case PanelState.Extended:
                    case PanelState.ExtendedFixed:
                        panelModule.deployState = ModuleDeployablePart.DeployState.EXTENDED;
                        break;
                }
            }

            public override void Extend() { panelModule.Extend(); }

            public override void Retract() { panelModule.Retract(); }

            public override bool IsRetractable() { return panelModule.retractable; }

            public override void Break(bool isBroken)
            {
                // reenable the target module
                panelModule.isEnabled = !isBroken;
                panelModule.enabled = !isBroken;
                if (isBroken) panelModule.part.FindModelComponents<Animation>().ForEach(k => k.Stop()); // stop the animations if we are disabling it
            }

            public override bool IsTracking => panelModule.isTracking;

            public override void SetTrackedBody(CelestialBody body)
            {
                panelModule.trackingBody = body;
                panelModule.GetTrackingBodyTransforms();
            }

            public override void OnUpdate()
            {
                panelModule.flowRate = (float)fixerModule.currentOutput;
            }
        }
        #endregion

        #region Near Future Solar support (ModuleCurvedSolarPanel)
        // Near future solar curved panel support
        // - We prevent the NFS module from running (disabled at MonoBehavior level)
        // - We replicate the behavior of its FixedUpdate()
        // - We call its Update() method but we disable the KSPFields UI visibility.
        private class NFSCurvedPanel : SupportedPanel<PartModule>
        {
            private Transform[] sunCatchers;    // model transforms named after the "PanelTransformName" field
            private bool deployable;            // "Deployable" field
            private Action panelModuleUpdate;   // delegate for the module Update() method

            public override void OnLoad(KopernicusSolarPanel fixerModule, PartModule targetModule)
            {
                this.fixerModule = fixerModule;
                panelModule = targetModule;
                deployable = ReflectionValue<bool>(panelModule, "Deployable");
            }

            public override bool OnStart(bool initialized, ref double nominalRate)
            {

                // get a delegate for Update() method (avoid performance penality of reflection)
                panelModuleUpdate = (Action)Delegate.CreateDelegate(typeof(Action), panelModule, "Update");
                // since we are disabling the MonoBehavior, ensure the module Start() has been called
                ReflectionCall(panelModule, "Start");

                // get transform name from module
                string transform_name = ReflectionValue<string>(panelModule, "PanelTransformName");

                // get panel components
                sunCatchers = panelModule.part.FindModelTransforms(transform_name);
                if (sunCatchers.Length == 0) return false;

                // disable the module at the Unity level, we will handle its updates manually
                panelModule.enabled = false;

                // return panel nominal rate
                nominalRate = ReflectionValue<float>(panelModule, "TotalEnergyRate");

                return true;

            }

            public override double GetOccludedFactor(Vector3d sunDir, out string occludingPart)
            {
                double occludedFactor = 1.0;
                occludingPart = null;

                RaycastHit raycastHit;
                foreach (Transform panel in sunCatchers)
                {
                    if (Physics.Raycast(panel.position + (sunDir * 0.25), sunDir, out raycastHit, 10000f))
                    {
                        if (occludingPart == null && raycastHit.collider != null)
                        {
                            Part blockingPart = Part.GetComponentUpwards<Part>(raycastHit.transform.gameObject);
                            if (blockingPart != null)
                            {
                                // avoid panels from occluding themselves
                                if (blockingPart == panelModule.part)
                                    continue;

                                occludingPart = blockingPart.partInfo.title;
                            }
                            //occludedFactor -= 1.0 / sunCatchers.Length;
                            occludedFactor = 0;
                        }
                    }
                }

                if (occludedFactor < 1E-5) occludedFactor = 0.0;
                return occludedFactor;
            }

            public override double GetCosineFactor(Vector3d sunDir, bool analytic = false)
            {
                double cosineFactor = 0.0;

                foreach (Transform panel in sunCatchers)
                {
                    cosineFactor += Math.Max(Vector3d.Dot(sunDir, panel.forward), 0.0);
                }

                return cosineFactor / sunCatchers.Length;
            }

            public override void OnUpdate()
            {
                // manually call the module Update() method since we have disabled the unity Monobehavior
                panelModuleUpdate();

                // hide ui fields
                foreach (BaseField field in panelModule.Fields)
                {
                    field.guiActive = false;
                }
            }

            public override PanelState GetState()
            {
                // Detect modified TotalEnergyRate (B9PS switching of the target module)
                double newrate = ReflectionValue<float>(panelModule, "TotalEnergyRate");
                if (newrate != fixerModule.nominalRate)
                {
                    OnStart(false, ref fixerModule.nominalRate);
                }

                string stateStr = ReflectionValue<string>(panelModule, "SavedState");
                Type enumtype = typeof(ModuleDeployablePart.DeployState);
                if (!Enum.IsDefined(enumtype, stateStr))
                {
                    if (!deployable) return PanelState.Static;
                    return PanelState.Unknown;
                }

                ModuleDeployablePart.DeployState state = (ModuleDeployablePart.DeployState)Enum.Parse(enumtype, stateStr);

                switch (state)
                {
                    case ModuleDeployablePart.DeployState.EXTENDED:
                        if (!deployable) return PanelState.Static;
                        return PanelState.Extended;
                    case ModuleDeployablePart.DeployState.RETRACTED: return PanelState.Retracted;
                    case ModuleDeployablePart.DeployState.RETRACTING: return PanelState.Retracting;
                    case ModuleDeployablePart.DeployState.EXTENDING: return PanelState.Extending;
                    case ModuleDeployablePart.DeployState.BROKEN: return PanelState.Broken;
                }
                return PanelState.Unknown;
            }

            public override void SetDeployedStateOnLoad(PanelState state)
            {
                switch (state)
                {
                    case PanelState.Retracted:
                        ReflectionValue(panelModule, "SavedState", "RETRACTED");
                        break;
                    case PanelState.Extended:
                        ReflectionValue(panelModule, "SavedState", "EXTENDED");
                        break;
                }
            }

            public override void Extend() { ReflectionCall(panelModule, "DeployPanels"); }

            public override void Retract() { ReflectionCall(panelModule, "RetractPanels"); }

            public override bool IsRetractable() { return true; }

            public override void Break(bool isBroken)
            {
                // in any case, the monobehavior stays disabled
                panelModule.enabled = false;
                if (isBroken)
                    panelModule.isEnabled = false; // hide the extend/retract UI
                else
                    panelModule.isEnabled = true; // show the extend/retract UI
            }
        }
        #endregion

        #region SSTU static multi-panel module support (SSTUSolarPanelStatic)
        // - We prevent the module from running (disabled at MonoBehavior level and KSP level)
        // - We replicate the behavior by ourselves
        private class SSTUStaticPanel : SupportedPanel<PartModule>
        {
            private Transform[] sunCatchers;    // model transforms named after the "PanelTransformName" field

            public override void OnLoad(KopernicusSolarPanel fixerModule, PartModule targetModule)
            { this.fixerModule = fixerModule; panelModule = targetModule; }

            public override bool OnStart(bool initialized, ref double nominalRate)
            {
                // disable it completely
                panelModule.enabled = panelModule.isEnabled = panelModule.moduleIsEnabled = false;

                // method that parse the suncatchers "suncatcherTransforms" config string into a List<string>
                ReflectionCall(panelModule, "parseTransformData");
                // method that get the transform list (panelData) from the List<string>
                ReflectionCall(panelModule, "findTransforms");
                // get the transforms
                sunCatchers = ReflectionValue<List<Transform>>(panelModule, "panelData").ToArray();
                // the nominal rate defined in SSTU is per transform
                nominalRate = ReflectionValue<float>(panelModule, "resourceAmount") * sunCatchers.Length;
                return true;
            }

            // exactly the same code as NFS curved panel
            public override double GetCosineFactor(Vector3d sunDir, bool analytic = false)
            {
                double cosineFactor = 0.0;

                foreach (Transform panel in sunCatchers)
                {
                    cosineFactor += Math.Max(Vector3d.Dot(sunDir, panel.forward), 0.0);
                }

                return cosineFactor / sunCatchers.Length;
            }

            // exactly the same code as NFS curved panel
            public override double GetOccludedFactor(Vector3d sunDir, out string occludingPart)
            {
                double occludedFactor = 1.0;
                occludingPart = null;

                RaycastHit raycastHit;
                foreach (Transform panel in sunCatchers)
                {
                    if (Physics.Raycast(panel.position + (sunDir * 0.25), sunDir, out raycastHit, 10000f))
                    {
                        if (occludingPart == null && raycastHit.collider != null)
                        {
                            Part blockingPart = Part.GetComponentUpwards<Part>(raycastHit.transform.gameObject);
                            if (blockingPart != null)
                            {
                                // avoid panels from occluding themselves
                                if (blockingPart == panelModule.part)
                                    continue;

                                occludingPart = blockingPart.partInfo.title;
                            }
                            //occludedFactor -= 1.0 / sunCatchers.Length;
                            occludedFactor = 0;
                        }
                    }
                }

                if (occludedFactor < 1E-5) occludedFactor = 0.0;
                return occludedFactor;
            }

            public override PanelState GetState() { return PanelState.Static; }

            public override bool SupportAutomation(PanelState state) { return false; }

            public override bool SupportProtoAutomation(ProtoPartModuleSnapshot protoModule) { return false; }

            public override void Break(bool isBroken)
            {
                // in any case, everything stays disabled
                panelModule.enabled = panelModule.isEnabled = panelModule.moduleIsEnabled = false;
            }
        }
        #endregion

        #region SSTU deployable/tracking multi-panel support (SSTUSolarPanelDeployable/SSTUModularPart)
        // SSTU common support for all solar panels that rely on the SolarModule/AnimationModule classes
        // - We prevent stock EC generation by setting to 0.0 the fields from where SSTU is getting the rates
        // - We use our own data structure that replicate the multiple panel per part possibilities, it store the transforms we need
        // - We use an aggregate of the nominal rate of each panel and assume all panels on the part are the same (not an issue currently, but the possibility exists in SSTU)
        // - Double-pivot panels that use multiple partmodules (I think there is only the "ST-MST-ISS solar truss" that does that) aren't supported
        // - Automation is currently not supported. Might be doable, but I don't have to mental strength to deal with it.
        // - Reliability is 100% untested and has a very barebones support. It should disable the EC output but not animations nor extend/retract ability.
        private class SSTUVeryComplexPanel : SupportedPanel<PartModule>
        {
            private object solarModuleSSTU; // instance of the "SolarModule" class
            private object animationModuleSSTU; // instance of the "AnimationModule" class
            private Func<string> getAnimationState; // delegate for the AnimationModule.persistentData property (string of the animState struct)
            private List<SSTUPanelData> panels;
            private TrackingType trackingType = TrackingType.Unknown;
            private enum TrackingType { Unknown = 0, Fixed, SinglePivot, DoublePivot }
            private string currentModularVariant;

            private class SSTUPanelData
            {
                public Transform pivot;
                public Axis pivotAxis;
                public SSTUSunCatcher[] suncatchers;

                public class SSTUSunCatcher
                {
                    public object objectRef; // reference to the "SuncatcherData" class instance, used to get the raycast hit (direct ref to the RaycastHit doesn't work)
                    public Transform transform;
                    public Axis axis;
                }

                public bool IsValid => suncatchers[0].transform != null;
                public Vector3 PivotAxisVector => GetDirection(pivot, pivotAxis);
                public int SuncatcherCount => suncatchers.Length;
                public Vector3 SuncatcherPosition(int index) => suncatchers[index].transform.position;
                public Vector3 SuncatcherAxisVector(int index) => GetDirection(suncatchers[index].transform, suncatchers[index].axis);
                public RaycastHit SuncatcherHit(int index) => ReflectionValue<RaycastHit>(suncatchers[index].objectRef, "hitData");

                public enum Axis { XPlus, XNeg, YPlus, YNeg, ZPlus, ZNeg }
                public static Axis ParseSSTUAxis(object sstuAxis) { return (Axis)Enum.Parse(typeof(Axis), sstuAxis.ToString()); }
                private Vector3 GetDirection(Transform transform, Axis axis)
                {
                    switch (axis) // I hope I got this right
                    {
                        case Axis.XPlus: return transform.right;
                        case Axis.XNeg: return transform.right * -1f;
                        case Axis.YPlus: return transform.up;
                        case Axis.YNeg: return transform.up * -1f;
                        case Axis.ZPlus: return transform.forward;
                        case Axis.ZNeg: return transform.forward * -1f;
                        default: return Vector3.zero;
                    }
                }
            }

            public override void OnLoad(KopernicusSolarPanel fixerModule, PartModule targetModule)
            { this.fixerModule = fixerModule; panelModule = targetModule; }

            public override bool OnStart(bool initialized, ref double nominalRate)
            {
                // get a reference to the "SolarModule" class instance, it has everything we need (transforms, rates, etc...)
                switch (panelModule.moduleName)
                {
                    case "SSTUModularPart":
                        solarModuleSSTU = ReflectionValue<object>(panelModule, "solarFunctionsModule");
                        currentModularVariant = ReflectionValue<string>(panelModule, "currentSolar");
                        break;
                    case "SSTUSolarPanelDeployable":
                        solarModuleSSTU = ReflectionValue<object>(panelModule, "solarModule");
                        break;
                    default:
                        return false;
                }

                // Get animation module
                animationModuleSSTU = ReflectionValue<object>(solarModuleSSTU, "animModule");
                // Get animation state property delegate
                PropertyInfo prop = animationModuleSSTU.GetType().GetProperty("persistentData");
                getAnimationState = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), animationModuleSSTU, prop.GetGetMethod());

                // SSTU stores the sum of the nominal output for all panels in the part, we retrieve it
                float newNominalrate = ReflectionValue<float>(solarModuleSSTU, "standardPotentialOutput");
                // OnStart can be called multiple times in the editor, but we might already have reset the rate
                // In the editor, if the "no panel" variant is selected, newNominalrate will be 0.0, so also check initialized
                if (newNominalrate > 0.0 || initialized == false)
                {
                    nominalRate = newNominalrate;
                    // reset the rate sum in the SSTU module. This won't prevent SSTU from generating EC, but this way we can keep track of what we did
                    // don't doit in the editor as it isn't needed and we need it in case of variant switching
                    if (HighLogic.LoadedSceneIsFlight) ReflectionValue(solarModuleSSTU, "standardPotentialOutput", 0f);
                }

                panels = new List<SSTUPanelData>();
                object[] panelDataArray = ReflectionValue<object[]>(solarModuleSSTU, "panelData"); // retrieve the PanelData class array that contain suncatchers and pivots data arrays
                foreach (object panel in panelDataArray)
                {
                    object[] suncatchers = ReflectionValue<object[]>(panel, "suncatchers"); // retrieve the SuncatcherData class array
                    object[] pivots = ReflectionValue<object[]>(panel, "pivots"); // retrieve the SolarPivotData class array

                    int suncatchersCount = suncatchers.Length;
                    if (suncatchers == null || pivots == null || suncatchersCount == 0) continue;

                    // instantiate our data class
                    SSTUPanelData panelData = new SSTUPanelData();

                    // get suncatcher transforms and the orientation of the panel surface normal
                    panelData.suncatchers = new SSTUPanelData.SSTUSunCatcher[suncatchersCount];
                    for (int i = 0; i < suncatchersCount; i++)
                    {
                        object suncatcher = suncatchers[i];
                        if (HighLogic.LoadedSceneIsFlight) ReflectionValue(suncatcher, "resourceRate", 0f); // actually prevent SSTU modules from generating EC, but not in the editor
                        panelData.suncatchers[i] = new SSTUPanelData.SSTUSunCatcher();
                        panelData.suncatchers[i].objectRef = suncatcher; // keep a reference to the original suncatcher instance, for raycast hit acquisition
                        panelData.suncatchers[i].transform = ReflectionValue<Transform>(suncatcher, "suncatcher"); // get suncatcher transform
                        panelData.suncatchers[i].axis = SSTUPanelData.ParseSSTUAxis(ReflectionValue<object>(suncatcher, "suncatcherAxis")); // get suncatcher axis
                    }

                    // get pivot transform and the pivot axis. Only needed for single-pivot tracking panels
                    // double axis panels can have 2 pivots. Its seems the suncatching one is always the second.
                    // For our purpose we can just assume always perfect alignement anyway.
                    // Note : some double-pivot panels seems to use a second SSTUSolarPanelDeployable instead, we don't support those.
                    switch (pivots.Length)
                    {
                        case 0:
                            trackingType = TrackingType.Fixed; break;
                        case 1:
                            trackingType = TrackingType.SinglePivot;
                            panelData.pivot = ReflectionValue<Transform>(pivots[0], "pivot");
                            panelData.pivotAxis = SSTUPanelData.ParseSSTUAxis(ReflectionValue<object>(pivots[0], "pivotRotationAxis"));
                            break;
                        case 2:
                            trackingType = TrackingType.DoublePivot; break;
                        default: continue;
                    }

                    panels.Add(panelData);
                }

                // disable ourselves if no panel was found
                if (panels.Count == 0) return false;

                // hide PAW status fields
                switch (panelModule.moduleName)
                {
                    case "SSTUModularPart": panelModule.Fields["solarPanelStatus"].guiActive = false; break;
                    case "SSTUSolarPanelDeployable": foreach (var field in panelModule.Fields) field.guiActive = false; break;
                }
                return true;
            }

            public override double GetCosineFactor(Vector3d sunDir, bool analytic = false)
            {
                double cosineFactor = 0.0;
                int suncatcherTotalCount = 0;
                foreach (SSTUPanelData panel in panels)
                {
                    if (!panel.IsValid) continue;
                    suncatcherTotalCount += panel.SuncatcherCount;
                    for (int i = 0; i < panel.SuncatcherCount; i++)
                    {
                        if (!analytic) { cosineFactor += Math.Max(Vector3d.Dot(sunDir, panel.SuncatcherAxisVector(i)), 0.0); continue; }

                        switch (trackingType)
                        {
                            case TrackingType.Fixed: cosineFactor += Math.Max(Vector3d.Dot(sunDir, panel.SuncatcherAxisVector(i)), 0.0); continue;
                            case TrackingType.SinglePivot: cosineFactor += Math.Cos(1.57079632679 - Math.Acos(Vector3d.Dot(sunDir, panel.PivotAxisVector))); continue;
                            case TrackingType.DoublePivot: cosineFactor += 1.0; continue;
                        }
                    }
                }
                return cosineFactor / suncatcherTotalCount;
            }

            public override double GetOccludedFactor(Vector3d sunDir, out string occludingPart)
            {
                double occludingFactor = 0.0;
                occludingPart = null;
                int suncatcherTotalCount = 0;
                foreach (SSTUPanelData panel in panels)
                {
                    if (!panel.IsValid) continue;
                    suncatcherTotalCount += panel.SuncatcherCount;
                    for (int i = 0; i < panel.SuncatcherCount; i++)
                    {
                        RaycastHit raycastHit;
                        Physics.Raycast(panel.SuncatcherPosition(i) + (sunDir * 0.25), sunDir, out raycastHit, 10000f);

                        if (raycastHit.collider != null)
                        {
                            occludingFactor += 1.0; // in case of multiple panels per part, it is perfectly valid for panels to occlude themselves so we don't do the usual check
                            Part blockingPart = Part.GetComponentUpwards<Part>(raycastHit.transform.gameObject);
                            if (occludingPart == null && blockingPart != null) // don't update if occlusion is from multiple parts
                                occludingPart = blockingPart.partInfo.title;
                        }
                    }
                }
                occludingFactor = 1.0 - (occludingFactor / suncatcherTotalCount);
                //if (occludingFactor < 0.01) occludingFactor = 0.0; // avoid precison issues
                if (occludingFactor > 0.0 && occludingFactor < 1.0) occludingFactor = 0.0; // avoid precison issues
                return occludingFactor;
            }

            public override PanelState GetState()
            {
                switch (trackingType)
                {
                    case TrackingType.Fixed: return PanelState.Static;
                    case TrackingType.Unknown: return PanelState.Unknown;
                }
                // handle solar panel variant switching in SSTUModularPart
                if (HighLogic.LoadedSceneIsEditor && panelModule.ClassName == "SSTUModularPart")
                {
                    string newVariant = ReflectionValue<string>(panelModule, "currentSolar");
                    if (newVariant != currentModularVariant)
                    {
                        currentModularVariant = newVariant;
                        OnStart(false, ref fixerModule.nominalRate);
                    }
                }
                // get animation state
                switch (getAnimationState())
                {
                    case "STOPPED_START": return PanelState.Retracted;
                    case "STOPPED_END": return PanelState.Extended;
                    case "PLAYING_FORWARD": return PanelState.Extending;
                    case "PLAYING_BACKWARD": return PanelState.Retracting;
                }
                return PanelState.Unknown;
            }

            public override bool IsTracking => trackingType == TrackingType.SinglePivot || trackingType == TrackingType.DoublePivot;

            public override void SetTrackedBody(CelestialBody body)
            {
                ReflectionValue(solarModuleSSTU, "trackedBodyIndex", body.flightGlobalsIndex);
            }

            public override bool SupportAutomation(PanelState state) { return false; }

            public override bool SupportProtoAutomation(ProtoPartModuleSnapshot protoModule) { return false; }
        }
        #endregion

        #region ROSolar switcheable/resizeable MDSP derivative (ModuleROSolar)
        // Made by Pap for RO. Implement in-editor model switching / resizing on top of the stock module.
        // TODO: Tracking panels implemented in v1.1 (May 2020).  Need further work here to get those working?
        // Plugin is here : https://github.com/KSP-RO/ROLibrary/blob/master/Source/ROLib/Modules/ModuleROSolar.cs
        // Configs are here : https://github.com/KSP-RO/ROSolar
        // Require the following MM patch to work :
        /*
        @PART:HAS[@MODULE[ModuleROSolar]]:AFTER[zKopernicus] { %MODULE[KopernicusSolarPanel]{} }
        */
        private class ROConfigurablePanel : StockPanel
        {
            // Note : this has been implemented in the base class (StockPanel) because
            // we have the same issue with NearFutureSolar B9PS-switching its MDSP modules.
        }

        #endregion
    }
}
