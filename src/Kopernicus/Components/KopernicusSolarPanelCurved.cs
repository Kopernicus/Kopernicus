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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;
using System.Diagnostics.CodeAnalysis;
using Kopernicus.Components;
using static ModuleDeployablePart;
using static UIPartActionResourceTransfer;
using Kopernicus.Configuration;

namespace Kopernicus
{
    public class KopernicusSolarPanelCurved : PartModule
    {
        //Strings for Localization
        private static readonly string SP_status_DirectSunlight = Localizer.Format("#Kopernicus_UI_DirectSunlight");  // "Direct Sunlight"
        private static readonly string SP_status_Underwater = Localizer.Format("#Kopernicus_UI_Underwater");          // "Underwater"

        //panel power cached value
        private double _cachedFlowRate = 0;
        private float cachedFlowRate = 0;

        //timer value
        private int frameTimer = 0;

        //declare internal float curves
        private static readonly FloatCurve AtmosphericAttenutationAirMassMultiplier = new FloatCurve();
        private static readonly FloatCurve AtmosphericAttenutationSolarAngleMultiplier = new FloatCurve();
        private static readonly FloatCurve TemperatureEfficCurve = new FloatCurve();

        [KSPField(isPersistant = false)]
        public string PanelTransformName;

        [KSPField(isPersistant = false)]
        public string DeployAnimation;

        [KSPField(isPersistant = false)]
        public bool Deployable = false;

        [KSPField(isPersistant = false)]
        public float TotalEnergyRate = 5.0f;

        [KSPField(isPersistant = false)]
        public FloatCurve powerCurve = new FloatCurve();

        [KSPField(isPersistant = false)]
        public string ResourceName;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Sun Exposure")]
        public string SunExposure;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Energy Flow")]
        public string EnergyFlow;

        [KSPField(isPersistant = false)]
        public float energyFlow;

        [KSPField(isPersistant = false)]
        public double _energyFlow;

        public float sunExposure;

        public double homeAltAboveSun;

        // ACTIONS
        // -----------------
        // Deploy Panels
        [KSPEvent(guiActive = true, guiName = "Deploy Panel", active = true, guiActiveEditor = true)]
        public void DeployPanels()
        {
            Deploy();
            if (HighLogic.LoadedSceneIsEditor)
                foreach (Part p in part.symmetryCounterparts)
                {
                    KopernicusSolarPanelCurved m = p.GetComponent<KopernicusSolarPanelCurved>();
                    m.Deploy();
                }

        }
        // Retract Panels
        [KSPEvent(guiActive = true, guiName = "Retract Panel", active = false, guiActiveEditor = true)]
        public void RetractPanels()
        {
            Retract();
            if (HighLogic.LoadedSceneIsEditor)
                foreach (Part p in part.symmetryCounterparts)
                {
                    KopernicusSolarPanelCurved m = p.GetComponent<KopernicusSolarPanelCurved>();
                    m.Retract();
                }

        }
        // Toggle Panels
        [KSPEvent(guiActive = false, guiName = "Toggle Panel", active = false)]
        public void TogglePanels()
        {

            Toggle();
            if (HighLogic.LoadedSceneIsEditor)
                foreach (Part p in part.symmetryCounterparts)
                {
                    KopernicusSolarPanelCurved m = p.GetComponent<KopernicusSolarPanelCurved>();
                    m.Toggle();
                }

        }

        [KSPAction("Deploy Panels")]
        public void DeployPanelsAction(KSPActionParam param)
        {
            DeployPanels();
        }

        [KSPAction("Retract Panels")]
        public void RetractPanelsAction(KSPActionParam param)
        {
            RetractPanels();
        }

        [KSPAction("Toggle Panels")]
        public void TogglePanelsAction(KSPActionParam param)
        {
            TogglePanels();
        }

        // Deploy Panels
        public void Deploy()
        {

            if (!Deployable)
                return;

            for (int i = 0; i < deployStates.Length; i++)
            {
                if (HighLogic.LoadedSceneIsEditor)
                    deployStates[i].speed = 10;
                else
                    deployStates[i].speed = 1;
            }
            State = ModuleDeployablePart.DeployState.EXTENDING;

        }

        // Retract Panels
        public void Retract()
        {

            if (!Deployable)
                return;

            for (int i = 0; i < deployStates.Length; i++)
            {
                if (HighLogic.LoadedSceneIsEditor)
                    deployStates[i].speed = -10;
                else
                    deployStates[i].speed = -1;
            }
            State = ModuleDeployablePart.DeployState.RETRACTING;
        }
        // Toggle Panels
        public void Toggle()
        {

            if (State == ModuleDeployablePart.DeployState.EXTENDED)
                Retract();
            else if (State == ModuleDeployablePart.DeployState.RETRACTED)
                Deploy();
            else
                return;
        }

        // Get the state
        public ModuleDeployablePart.DeployState State
        {
            get
            {
                try
                {
                    return (ModuleDeployablePart.DeployState)Enum.Parse(typeof(ModuleDeployablePart.DeployState), SavedState);
                }
                catch
                {
                    State = ModuleDeployablePart.DeployState.RETRACTED;
                    return State;
                }
            }
            set
            {
                SavedState = value.ToString();
            }
        }

        // Current panel state
        [KSPField(isPersistant = true)]
        public string SavedState;


        private AnimationState[] deployStates;

        private Transform[] panelTransforms;
        private int panelCount= 0;
        private float chargePerTransform;

        private bool flight = false;

        private Transform sunTransform;

        // Info for ui
        public override string GetInfo()
        {
            return Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_PartInfo", TotalEnergyRate.ToString("F2"));
        }

        public string GetModuleTitle()
        {
            return "CurvedSolarPanel";
        }
        public override string GetModuleDisplayName()
        {
            return Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_ModuleName");
        }
        public void Start()
        {

            panelTransforms = part.FindModelTransforms(PanelTransformName);
            panelCount = panelTransforms.Length;

            // Get homeworld's distance from Sun.
            // Dealing with systems that could have either have homeworlds in very eccentric
            // orbits or as moons of another body would need something more sophisticated.
            homeAltAboveSun = FlightGlobals.getAltitudeAtPos(FlightGlobals.GetHomeBody().position, FlightGlobals.Bodies[0]);

            Actions["DeployPanelsAction"].guiName = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Action_DeployPanelsAction");
            Actions["RetractPanelsAction"].guiName = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Action_RetractPanelsAction");
            Actions["TogglePanelsAction"].guiName = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Action_TogglePanelsAction");

            Events["DeployPanels"].guiName = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Event_DeployPanels");
            Events["RetractPanels"].guiName = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Event_RetractPanels");
            Events["TogglePanels"].guiName = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Event_TogglePanels");

            Fields["EnergyFlow"].guiName = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Field_EnergyFlow");
            Fields["SunExposure"].guiName = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Field_SunExposure");

            if (Deployable)
            {
                deployStates = Utility.SetUpAnimation(DeployAnimation, this.part);


                if (State == ModuleDeployablePart.DeployState.EXTENDED || State == ModuleDeployablePart.DeployState.EXTENDING)
                {
                    for (int i = 0; i < deployStates.Length; i++)
                    {
                        deployStates[i].normalizedTime = 1f;
                    }
                }
                else if (State == ModuleDeployablePart.DeployState.RETRACTED || State == ModuleDeployablePart.DeployState.RETRACTING)
                {
                    for (int i = 0; i < deployStates.Length; i++)
                    {
                        deployStates[i].normalizedTime = 0f;
                    }
                }
                else
                {
                    // broken! none for you!
                }
            }
            else
            {
                Events["DeployPanels"].active = false;
                Events["RetractPanels"].active = false;
                Events["TogglePanels"].active = false;
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                sunTransform = FlightGlobals.Bodies[0].bodyTransform;
                flight = true;
            }
            else
            {
                flight = false;
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (KopernicusStar.UseMultiStarLogic)
            {
                //Setup Floatcurves
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
                TemperatureEfficCurve.Add(4f, 1.2f, 0f, -0.0006f);
                TemperatureEfficCurve.Add(300f, 1f, -0.0008f, -0.0008f);
                TemperatureEfficCurve.Add(1200f, 0.134f, -0.00035f, -0.00035f);
                TemperatureEfficCurve.Add(1900f, 0.02f, -3.72E-05f, -3.72E-05f);
                TemperatureEfficCurve.Add(2500f, 0.01f, 0f, 0f);
            }
        }

        public void FixedUpdate()
        {
            if (KopernicusStar.UseMultiStarLogic)
            {
                int blockedPartCount = 0;
                int blockedBodyCount = 0;
                string body = "";
                string obscuringPart = "";
                frameTimer++;
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (!Deployable || (Deployable && (State == ModuleDeployablePart.DeployState.EXTENDED)))
                    {
                        if (frameTimer >
                            (50 * Kopernicus.RuntimeUtility.RuntimeUtility.KopernicusConfig.SolarRefreshRate))
                        {
                            CelestialBody trackingStar = KopernicusStar.Current.sun;
                            frameTimer = 0;
                            Double totalFlux = 0;
                            Double totalFlow = 0;
                            energyFlow = 0;
                            _energyFlow = 0;
                            sunExposure = 0;
                            Double bestFlux = 0;
                            for (Int32 s = 0; s < KopernicusStar.Stars.Count; s++)
                            {
                                KopernicusStar star = KopernicusStar.Stars[s];
                                // Use this star
                                star.shifter.ApplyPhysics();

                                //Calculate flux
                                double starFluxAtHome = 0;
                                if (PhysicsGlobals.SolarLuminosityAtHome != 0)
                                {
                                    starFluxAtHome = 1360 / PhysicsGlobals.SolarLuminosityAtHome;
                                }

                                double starFlux = 0;
                                starFlux = star.CalculateFluxAt(vessel) * starFluxAtHome;

                                //Check if star has better flux
                                if (bestFlux < starFlux)
                                {
                                    bestFlux = starFlux;
                                }
                                // Add to TotalFlux and EC tally
                                totalFlux += starFlux;
                                float panelEffectivness = 0;
                                //Now for some fancy atmospheric math
                                float atmoDensityMult = 1;
                                float atmoAngleMult = 1;
                                float tempMult = 1;
                                Vector3d localSpace = ScaledSpace.ScaledToLocalSpace(star.target.position);
                                if (this.vessel.atmDensity > 0)
                                {
                                    float horizonAngle = (float)Math.Acos(FlightGlobals.currentMainBody.Radius /
                                                                          (FlightGlobals.currentMainBody.Radius +
                                                                           FlightGlobals.ship_altitude));
                                    Vector3 horizonVector = new Vector3(0, Mathf.Sin(Mathf.Deg2Rad * horizonAngle),
                                        Mathf.Cos(Mathf.Deg2Rad * horizonAngle));
                                    float sunZenithAngleDeg = Vector3.Angle(FlightGlobals.upAxis, star.sun.position);

                                    Double gravAccelParameter = (vessel.mainBody.gravParameter /
                                                                 Math.Pow(
                                                                     vessel.mainBody.Radius +
                                                                     FlightGlobals.ship_altitude, 2));
                                    float massOfAirColumn =
                                        (float)(FlightGlobals.getStaticPressure() / gravAccelParameter);

                                    tempMult = TemperatureEfficCurve.Evaluate(
                                        (float)this.vessel.atmosphericTemperature);
                                    atmoDensityMult =
                                        AtmosphericAttenutationAirMassMultiplier.Evaluate(massOfAirColumn);
                                    atmoAngleMult =
                                        AtmosphericAttenutationSolarAngleMultiplier.Evaluate(sunZenithAngleDeg);
                                }

                                chargePerTransform = TotalEnergyRate / panelCount;

                                for (int i = 0; i < panelTransforms.Length; i++)
                                {
                                    float angle = 0f;

                                    if (SolarLOS(panelTransforms[i], out angle, out body, star.sun))
                                    {
                                        if (PartLOS(panelTransforms[i], out obscuringPart, star.sun))
                                        {
                                            sunExposure += Mathf.Clamp01(Mathf.Cos(angle * Mathf.Deg2Rad)) / panelCount;
                                        }
                                        else
                                        {
                                            sunExposure += 0f;
                                            blockedPartCount++;
                                        }
                                    }
                                    else
                                    {
                                        sunExposure += 0f;
                                        blockedBodyCount++;

                                    }

                                }

                                if ((sunExposure != 0) && (tempMult != 0) && (atmoAngleMult != 0) && (atmoDensityMult != 0))
                                {
                                    panelEffectivness = (TotalEnergyRate / 24.4f) / 56.37091313591871f * sunExposure * tempMult *
                                                        atmoAngleMult *
                                                        atmoDensityMult; //56.blabla is a weird constant we use to turn flux into EC
                                }
                                if (starFluxAtHome > 0)
                                {
                                    totalFlow += (starFlux * panelEffectivness) /
                                                 (1360 / PhysicsGlobals.SolarLuminosityAtHome);
                                }
                            }
                            vessel.solarFlux = totalFlux;
                            //Add to new output
                            energyFlow = (float)totalFlow;
                            _energyFlow = totalFlow / TotalEnergyRate;
                            EnergyFlow = String.Format("{0:F2}", energyFlow);
                            SunExposure = String.Format("{0:F2}", sunExposure);

                            if (blockedPartCount >= panelCount)
                            {
                                SunExposure = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Field_SunExposure_Blocked", obscuringPart.ToString());
                            }
                            if (blockedBodyCount >= panelCount)
                            {
                                SunExposure = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Field_SunExposure_Blocked", body.ToString());
                            }

                            part.RequestResource(ResourceName, (-energyFlow) * TimeWarp.fixedDeltaTime);
                            //caching logic
                            cachedFlowRate = energyFlow;
                            _cachedFlowRate = _energyFlow;
                        }
                        else
                        {
                            //inbetween timings logic
                            energyFlow = cachedFlowRate;
                            _energyFlow = _cachedFlowRate;
                            EnergyFlow = String.Format("{0:F2}", energyFlow);
                            SunExposure = String.Format("{0:F2}", sunExposure);

                            if (blockedPartCount >= panelCount)
                            {
                                SunExposure = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Field_SunExposure_Blocked", obscuringPart.ToString());
                            }
                            if (blockedBodyCount >= panelCount)
                            {
                                SunExposure = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Field_SunExposure_Blocked", body.ToString());
                            }

                            part.RequestResource(ResourceName, (-energyFlow) * TimeWarp.fixedDeltaTime);
                        }

                        //see if tracked star is blocked or not
                        if (sunExposure > 0)
                        {
                            //this ensures the "blocked" GUI option is set right, if we're exposed to you we're not blocked
                            vessel.directSunlight = true;
                        }

                        // Restore The Current Star
                        KopernicusStar.Current.shifter.ApplyPhysics();
                    }
                }
                else
                {
                    //Packed logic
                    energyFlow = cachedFlowRate;
                    _energyFlow = _cachedFlowRate;
                    EnergyFlow = String.Format("{0:F2}", energyFlow);
                    SunExposure = String.Format("{0:F2}", sunExposure);

                    if (blockedPartCount >= panelCount)
                    {
                        SunExposure = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Field_SunExposure_Blocked", obscuringPart.ToString());
                    }
                    if (blockedBodyCount >= panelCount)
                    {
                        SunExposure = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Field_SunExposure_Blocked", body.ToString());
                    }

                    part.RequestResource(ResourceName, (-energyFlow) * TimeWarp.fixedDeltaTime);
                }
            }
            else
            {
                if (flight)
                {
                    if (!Deployable || (Deployable && (State == ModuleDeployablePart.DeployState.EXTENDED)))
                    {
                        sunExposure = 0f;
                        energyFlow = 0f;

                        int blockedPartCount = 0;
                        int blockedBodyCount = 0;
                        string body = "";
                        string obscuringPart = "";

                        chargePerTransform = TotalEnergyRate / panelCount;

                        for (int i = 0; i < panelTransforms.Length; i++)
                        {
                            float angle = 0f;

                            if (SolarLOS(panelTransforms[i], out angle, out body,FlightGlobals.Bodies[0]))
                            {
                                if (PartLOS(panelTransforms[i], out obscuringPart,FlightGlobals.Bodies[0]))
                                {
                                    sunExposure += Mathf.Clamp01(Mathf.Cos(angle * Mathf.Deg2Rad)) / panelCount;
                                    energyFlow += Mathf.Clamp01(Mathf.Cos(angle * Mathf.Deg2Rad)) * chargePerTransform;
                                }
                                else
                                {
                                    energyFlow += 0f;
                                    sunExposure += 0f;
                                    blockedPartCount++;
                                }
                            }
                            else
                            {
                                sunExposure += 0f;
                                energyFlow += 0f;
                                blockedBodyCount++;

                            }

                        }

                        double altAboveSun = FlightGlobals.getAltitudeAtPos(vessel.GetWorldPos3D(), FlightGlobals.Bodies[0]);


                        float realFlow = energyFlow * (float)( (homeAltAboveSun*homeAltAboveSun) / (altAboveSun*altAboveSun));

                        //Debug.Log(altAboveSun.ToString() + ", gives " + realFlow);

                        EnergyFlow = String.Format("{0:F2}", realFlow);
                        SunExposure = String.Format("{0:F2}", sunExposure);

                        if (blockedPartCount >= panelCount)
                        {
                            SunExposure = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Field_SunExposure_Blocked", obscuringPart.ToString());
                        }
                        if (blockedBodyCount >= panelCount)
                        {
                            SunExposure = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Field_SunExposure_Blocked", body.ToString());
                        }

                        part.RequestResource(ResourceName, (-realFlow) * TimeWarp.fixedDeltaTime);
                    }
                    else if (Deployable && (State == ModuleDeployablePart.DeployState.BROKEN))
                    {
                        SunExposure = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Field_SunExposure_Broken");
                        EnergyFlow = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Field_EnergyFlow_Broken");
                    }
                    else if (Deployable && (State == ModuleDeployablePart.DeployState.RETRACTED))
                    {
                        SunExposure = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Field_SunExposure_Retracted");
                        EnergyFlow = Localizer.Format("#LOC_NFSolar_ModuleCurvedSolarPanel_Field_EnergyFlow_Retracted");
                    }
                }
            }
        }
        public void Update()
        {

            if (Deployable)
            {
                for (int i = 0; i < deployStates.Length; i++)
                {
                    deployStates[i].normalizedTime = Mathf.Clamp01(deployStates[i].normalizedTime);
                }
                if (State == ModuleDeployablePart.DeployState.RETRACTING)
                {
                    if (EvalAnimationCompletionReversed(deployStates) == 0f)
                        State = ModuleDeployablePart.DeployState.RETRACTED;
                }

                if (State == ModuleDeployablePart.DeployState.EXTENDING)
                {
                    if (EvalAnimationCompletion(deployStates) == 1f)
                        State = ModuleDeployablePart.DeployState.EXTENDED;
                }

                if ((State == ModuleDeployablePart.DeployState.EXTENDED && Events["DeployPanels"].active) || (State == ModuleDeployablePart.DeployState.RETRACTED && Events["RetractPanels"].active))
                {
                    Events["DeployPanels"].active = !Events["DeployPanels"].active;
                    Events["RetractPanels"].active = !Events["RetractPanels"].active;
                }
            }
        }

        private bool PartLOS(Transform refXForm, out string obscuringPart, CelestialBody sun)
        {
            bool sunVisible = true;
            obscuringPart = "nil";

            RaycastHit hit;
            if (Physics.Raycast(refXForm.position, refXForm.position - sun.transform.position, out hit, 2500f))
            {

                Transform hitObj = hit.transform;
                Part pt = hitObj.GetComponent<Part>();
                if (pt != null && pt != part)
                {
                    sunVisible = false;
                    obscuringPart = pt.partInfo.name;
                }
            }

            return sunVisible;
        }

        private bool SolarLOS(Transform refXForm, out float angle, out string obscuringBody, CelestialBody sun)
        {
            bool sunVisible = true;
            angle = 0f;
            obscuringBody = "nil";

            CelestialBody currentBody = FlightGlobals.currentMainBody;

            angle = Vector3.Angle(refXForm.forward, sun.transform.position - refXForm.position);

            if (currentBody != sun)
            {

                Vector3 vT = sun.position - part.vessel.GetWorldPos3D();
                Vector3 vC = currentBody.position - part.vessel.GetWorldPos3D();
                // if true, behind horizon plane
                if (Vector3.Dot(vT, vC) > (vC.sqrMagnitude - currentBody.Radius * currentBody.Radius))
                {
                    // if true, obscured
                    if ((Mathf.Pow(Vector3.Dot(vT, vC), 2) / vT.sqrMagnitude) > (vC.sqrMagnitude - currentBody.Radius * currentBody.Radius))
                    {
                        sunVisible = false;
                        obscuringBody = currentBody.name;
                    }
                }
            }

            return sunVisible;

            // discard this for now; stock panels don't check against other than main body

            //foreach (CelestialBody planet in FlightGlobals.Bodies)
            //{
            //    if (planet == sun)
            //    {
            //        angle = Vector3.Angle(refXForm.forward, refXForm.position - planet.transform.position);
            //    }
            //    else
            //    {
            //        Vector3d vT = sun.position - part.vessel.GetWorldPos3D();
            //        Vector3d vC = planet.position - part.vessel.GetWorldPos3D();
            //        // if true, behind horizon plane
            //        if (Vector3d.Dot(vT, vC) > (vC.sqrMagnitude - planet.Radius*planet.Radius))
            //        {
            //            // if true, obsucred
            //            if ((Mathf.Pow(Vector3.Dot(vT, vC), 2) / vT.sqrMagnitude) > (vC.sqrMagnitude - planet.Radius * planet.Radius))
            //            {
            //                sunVisible = false;
            //                obscuringBody = planet.name;
            //            }
            //        }
            //    }

            //}


        }


        private float EvalAnimationCompletion(AnimationState[] states)
        {
            float checker = 0f;
            for (int i = 0; i < states.Length; i++)
            {
                checker = Mathf.Max(states[i].normalizedTime, checker);
            }
            return checker;
        }
        private float EvalAnimationCompletionReversed(AnimationState[] states)
        {
            float checker = 1f;
            for (int i = 0; i < states.Length; i++)
            {
                checker = Mathf.Min(states[i].normalizedTime, checker);
            }
            return checker;
        }
    }
}
