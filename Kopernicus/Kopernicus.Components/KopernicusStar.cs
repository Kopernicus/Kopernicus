/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// Implementation of the <see cref="Sun"/> API.
        /// </summary>
        public class KopernicusStar : Sun
        {
            /// <summary>
            /// A list of all stars
            /// </summary>
            public static List<KopernicusStar> Stars;

            /// <summary>
            /// The currently active star, for stuff we cant patch
            /// </summary>
            public static KopernicusStar Current;

            /// <summary>
            /// The sunlight
            /// </summary>
            public Light light;

            /// <summary>
            /// The light when we are in IVA
            /// </summary>
            public IVASun iva;

            /// <summary>
            /// The light settings for this star
            /// </summary>
            public LightShifter shifter;

            /// <summary>
            /// Override for <see cref="FlightIntegrator.CalculateSunBodyFlux"/>
            /// </summary>
            public static void SunBodyFlux(ModularFI.ModularFlightIntegrator flightIntegrator)
            {
                // Set Physics
                PhysicsGlobals.SolarLuminosityAtHome = Current.shifter.solarLuminosity;
                PhysicsGlobals.SolarInsolationAtHome = Current.shifter.solarInsolation;

                // Get "Correct" values
                CelestialBody sunBody = FlightIntegrator.sunBody;
                flightIntegrator.BaseFICalculateSunBodyFlux();

                // FI Values
                bool directSunlight = flightIntegrator.Vessel.directSunlight;
                Vector3 sunVector = flightIntegrator.sunVector;
                double solarFlux = flightIntegrator.solarFlux;
                float sunAxialDot = flightIntegrator.sunAxialDot;
                double sunDotCorrected = flightIntegrator.sunDotCorrected;
                double bodySunFlux = flightIntegrator.bodySunFlux;
                double atmosphereTemperatureOffset = flightIntegrator.atmosphereTemperatureOffset;
                double bodyEmissiveFlux = flightIntegrator.bodyEmissiveFlux;
                double bodyTemperature = flightIntegrator.bodyTemperature;
                double bodyAlbedoFlux = flightIntegrator.bodyAlbedoFlux;
                double bodyPolarAngle = flightIntegrator.bodyPolarAngle;
                double sunPolarAngle = flightIntegrator.sunPolarAngle;
                double sunBodyMaxDot = flightIntegrator.sunBodyMaxDot;
                double sunBodyMinDot = flightIntegrator.sunBodyMinDot;
                double bodyDayFraction = flightIntegrator.bodyDayFraction;
                double sunDotNormalized = flightIntegrator.sunDotNormalized;
                double realDistanceToSun = flightIntegrator.realDistanceToSun;

                // Calculate the values for all bodies
                foreach (KopernicusStar star in Stars.Where(s => s.sun != sunBody))
                {
                    // Set Physics
                    PhysicsGlobals.SolarLuminosityAtHome = star.shifter.solarLuminosity;
                    PhysicsGlobals.SolarInsolationAtHome = star.shifter.solarInsolation;

                    FlightIntegrator.sunBody = star.sun;
                    flightIntegrator.BaseFICalculateSunBodyFlux();

                    // And save them
                    if (flightIntegrator.Vessel.directSunlight)
                        directSunlight = true;
                    solarFlux += flightIntegrator.solarFlux;
                    bodySunFlux += flightIntegrator.bodySunFlux;
                    atmosphereTemperatureOffset += flightIntegrator.atmosphereTemperatureOffset;
                    bodyEmissiveFlux += flightIntegrator.bodyEmissiveFlux;
                    bodyTemperature += flightIntegrator.bodyTemperature;
                    bodyAlbedoFlux += flightIntegrator.bodyAlbedoFlux;
                }

                // Reapply
                flightIntegrator.Vessel.directSunlight = directSunlight;
                flightIntegrator.sunVector = sunVector;
                flightIntegrator.solarFlux = solarFlux;
                flightIntegrator.sunAxialDot = sunAxialDot;
                flightIntegrator.sunDotCorrected = sunDotCorrected;
                flightIntegrator.bodySunFlux = bodySunFlux;
                flightIntegrator.atmosphereTemperatureOffset = atmosphereTemperatureOffset;
                flightIntegrator.bodyEmissiveFlux = bodyEmissiveFlux;
                flightIntegrator.bodyTemperature = bodyTemperature;
                flightIntegrator.bodyAlbedoFlux = bodyAlbedoFlux;
                flightIntegrator.bodyPolarAngle = bodyPolarAngle;
                flightIntegrator.sunPolarAngle = sunPolarAngle;
                flightIntegrator.sunBodyMaxDot = sunBodyMaxDot;
                flightIntegrator.sunBodyMinDot = sunBodyMinDot;
                flightIntegrator.bodyDayFraction = bodyDayFraction;
                flightIntegrator.sunDotNormalized = sunDotNormalized;
                flightIntegrator.realDistanceToSun = realDistanceToSun;
                FlightIntegrator.sunBody = sunBody;
            }

            /// <summary>
            /// Starts up this instance
            /// </summary>
            protected override void Awake()
            {
                if (Stars == null)
                    Stars = new List<KopernicusStar>();
                Stars.Add(this);
                DontDestroyOnLoad(this);
                light = gameObject.GetComponent<Light>();
                Camera.onPreCull += cam =>
                {
                    Vector3d scaledSpace = target.transform.position - ScaledSpace.LocalToScaledSpace(sun.position);
                    sunDirection = scaledSpace.normalized;
                    if (sunDirection != Vector3d.zero)
                        transform.forward = sunDirection;
                };
            }

            /// <summary>
            /// Create stuff when the object is live
            /// </summary>
            protected override void Start()
            {
                // Get the LightShifter
                shifter = sun.scaledBody.GetComponentsInChildren<LightShifter>(true)?[0];

                // Gah
                typeof(Sun).GetField("lgt", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(this, light);

                // Scaled Space Light
                if (!useLocalSpaceSunLight) return;
                scaledSunLight = (new GameObject("Scaledspace SunLight " + sun.name)).AddComponent<Light>();
                scaledSunLight.type = LightType.Directional;
                scaledSunLight.intensity = light.intensity;
                scaledSunLight.color = light.color;
                scaledSunLight.transform.parent = transform;
                scaledSunLight.transform.localPosition = Vector3.zero;
                scaledSunLight.transform.localRotation = Quaternion.identity;
                scaledSunLight.cullingMask = 1024;
                light.cullingMask = light.cullingMask ^ 1024;
                GameEvents.onGameSceneLoadRequested.Add(SceneLoaded);
            }

            /// <summary>
            /// Updates the light values based on the current scene
            /// </summary>
            /// <param name="scene"></param>
            void SceneLoaded(GameScenes scene)
            {
                light.shadowBias = scene != GameScenes.SPACECENTER ? 0.125f : 1f;

                // IVA Sun
                if (HighLogic.LoadedSceneIsFlight)
                {
                    iva = Instantiate(Resources.FindObjectsOfTypeAll<IVASun>().Last());
                    iva.transform.parent = transform;
                    iva.sunT = transform;
                }
            }

            /// <summary>
            /// Updates this instance
            /// </summary>
            void LateUpdate()
            {
                // Update the lensflare orientation and scale
                sunFlare.brightness = brightnessMultiplier * brightnessCurve.Evaluate((float)(1 / (Vector3d.Distance(Camera.current.transform.position, ScaledSpace.LocalToScaledSpace(sun.position)) / (AU * ScaledSpace.InverseScaleFactor))));

                // Apply light settings
                shifter.Apply(light, scaledSunLight, iva?.GetComponent<Light>());

                // Set SunFlare color
                sunFlare.color = shifter.sunLensFlareColor;

                // Set other stuff
                AU = shifter.AU;
                brightnessCurve = shifter.brightnessCurve.Curve;

                // States
                bool lightsOn = (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneHasPlanetarium || HighLogic.LoadedScene == GameScenes.SPACECENTER);
                light.enabled = shifter.givesOffLight && lightsOn && Current == this;
                sunFlare.enabled = shifter.givesOffLight && lightsOn;
                if (useLocalSpaceSunLight && Sun.Instance.useLocalSpaceSunLight)
                    scaledSunLight.enabled = shifter.givesOffLight && lightsOn && Current == this;

                // Update Scaled Space Light
                if (!useLocalSpaceSunLight) return;
                Vector3d localSpace = ScaledSpace.ScaledToLocalSpace(target.position);
                if (FlightGlobals.currentMainBody == null || FlightGlobals.currentMainBody == sun)
                {
                    localTime = 1f;
                    light.intensity = scaledSunLight.intensity;
                }
                else
                {
                    double targetAltitude = FlightGlobals.getAltitudeAtPos(localSpace, FlightGlobals.currentMainBody);
                    if (targetAltitude < 0)
                        targetAltitude = 0;
                    double horizonAngle = Math.Acos(FlightGlobals.currentMainBody.Radius / (FlightGlobals.currentMainBody.Radius + targetAltitude));
                    float horizonScalar = -Mathf.Sin((float)horizonAngle);
                    float dayNightRatio = 1f - Mathf.Abs(horizonScalar);
                    float fadeStartAtAlt = horizonScalar + fadeStart * dayNightRatio;
                    float fadeEndAtAlt = horizonScalar - fadeEnd * dayNightRatio;
                    localTime = Vector3.Dot(-FlightGlobals.getUpAxis(localSpace), transform.forward);
                    light.intensity = Mathf.Lerp(0f, scaledSunLight.intensity, Mathf.InverseLerp(fadeEndAtAlt, fadeStartAtAlt, localTime));
                }
            }

            /// <summary>
            /// Override this function and use <see cref="Current"/> instead of Planetarium sun
            /// </summary>
            public override double GetLocalTimeAtPosition(Vector3d wPos, CelestialBody cb)
            {
                Vector3d pos1 = Vector3d.Exclude(cb.angularVelocity, FlightGlobals.getUpAxis(cb, wPos));
                Vector3d pos2 = Vector3d.Exclude(cb.angularVelocity, Current.sun.position - cb.position);
                double angle = (Vector3d.Dot(Vector3d.Cross(pos2, pos1), cb.angularVelocity) < 0 ? -1 : 1 * Vector3d.AngleBetween(pos1, pos2)) / 6.28318530717959 + 0.5;
                if (angle > Math.PI * 2)
                    angle -= Math.PI * 2;
                return angle;
            }
        }
    }
}