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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using ModularFI;
using UnityEngine;

namespace Kopernicus.Components
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
        /// The results of the latest flux calculation for each star
        /// </summary>
        public static Dictionary<String, Double> SolarFlux;

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
        /// The SunFlare component that controls the lensflare assigned to this star
        /// </summary>
        public KopernicusSunFlare lensFlare;

        /// <summary>
        /// Override for <see cref="FlightIntegrator.CalculateSunBodyFlux"/>
        /// </summary>
        public static void SunBodyFlux(ModularFlightIntegrator flightIntegrator)
        {
            KopernicusStar sun = Instance as KopernicusStar;

            Boolean directSunlight = false;
            Double solarFlux = 0;

            // Calculate the values for all bodies except the sun
            for (Int32 i = 0; i < Stars.Count; i++)
            {
                KopernicusStar star = Stars[i];
                if (star == sun)
                {
                    return;
                }

                // Apply physics variables
                star.shifter.ApplyPhysics();

                // Calculate flux
                flightIntegrator.BaseFICalculateSunBodyFlux();
                solarFlux += flightIntegrator.solarFlux;

                if (!SolarFlux.ContainsKey(star.name))
                {
                    SolarFlux.Add(star.name, flightIntegrator.solarFlux);
                }
                else
                {
                    SolarFlux[star.name] = flightIntegrator.solarFlux;
                }
                if (flightIntegrator.Vessel.directSunlight)
                {
                    directSunlight = true;
                }
            }

            // Set back to the values for sun
            sun.shifter.ApplyPhysics();

            // Get the values for sun
            flightIntegrator.BaseFICalculateSunBodyFlux();
            if (!SolarFlux.ContainsKey(sun.name))
            {
                SolarFlux.Add(sun.name, flightIntegrator.solarFlux);
            }
            else
            {
                SolarFlux[sun.name] = flightIntegrator.solarFlux;
            }

            // Apply the stored values for the other bodies
            flightIntegrator.solarFlux += solarFlux;
            flightIntegrator.Vessel.directSunlight |= directSunlight;
        }

        /// <summary>
        /// Returns the star the given body orbits
        /// </summary>
        public static KopernicusStar GetNearest(CelestialBody body)
        {
            return Stars.OrderBy(s => Vector3.Distance(body.position, s.sun.position)).First();
        }

        /// <summary>
        /// Starts up fi instance
        /// </summary>
        protected override void Awake()
        {
            if (Stars == null)
            {
                Stars = new List<KopernicusStar>();
            }

            if (SolarFlux == null)
            {
                SolarFlux = new Dictionary<String, Double>();
            }

            Stars.Add(this);
            DontDestroyOnLoad(this);
            light = gameObject.GetComponent<Light>();

            // Gah
            typeof(Sun).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Last(f => f.FieldType == typeof(Light)).SetValue(this, light);

            // sun flare
            Camera.onPreCull += cam =>
            {
                Vector3d scaledSpace = target.transform.position - ScaledSpace.LocalToScaledSpace(sun.position);
                sunDirection = scaledSpace.normalized;
                sunRotation = sunDirection;
                sunRotation.x = Math.Round(sunRotation.x, sunRotationPrecision);
                sunRotation.y = Math.Round(sunRotation.y, sunRotationPrecision);
                sunRotation.z = Math.Round(sunRotation.z, sunRotationPrecision);
                if (sunRotation != Vector3d.zero)
                {
                    transform.forward = sunRotation;
                }
            };
        }

        /// <summary>
        /// Create stuff when the object is live
        /// </summary>
        protected override void Start()
        {
            // Get the LightShifter
            shifter = sun.scaledBody.GetComponentsInChildren<LightShifter>(true)?[0];

            // Lensflare
            if (shifter != null)
            {
                lensFlare.sunFlare.flare = shifter.sunFlare ? shifter.sunFlare : lensFlare.sunFlare.flare;
            }

            // IVA Light
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                iva = UnityEngine.Object.Instantiate(Resources.FindObjectsOfTypeAll<IVASun>().Last(), transform, true);
                iva.sunT = transform;
            }

            // Scaled Space Light
            if (!useLocalSpaceSunLight)
            {
                return;
            }

            scaledSunLight = new GameObject("Scaledspace SunLight " + sun.name).AddComponent<Light>();
            scaledSunLight.type = LightType.Directional;
            scaledSunLight.intensity = light.intensity;
            scaledSunLight.color = light.color;
            Transform scaledLightTransform = scaledSunLight.transform;
            scaledLightTransform.parent = transform;
            scaledLightTransform.localPosition = Vector3.zero;
            scaledLightTransform.localRotation = Quaternion.identity;
            scaledSunLight.cullingMask = 1 << 10;
            GameEvents.onGameSceneLoadRequested.Add(SceneLoaded);
        }

        /// <summary>
        /// Updates the light values based on the current scene
        /// </summary>
        private void SceneLoaded(GameScenes scene)
        {
            light.shadowBias = scene != GameScenes.SPACECENTER ? shadowBiasFlight : shadowBiasSpaceCentre;
            if (gameObject.GetComponentInChildren<IVASun>() != null)
            {
                DestroyImmediate(gameObject.GetComponentInChildren<IVASun>().gameObject);
            }
        }

        /// <summary>
        /// Updates this instance
        /// </summary>
        [SuppressMessage("ReSharper", "ArrangeStaticMemberQualifier")]
        private void LateUpdate()
        {
            // Set precision
            sunRotationPrecision = MapView.MapIsEnabled ? sunRotationPrecisionMapView : sunRotationPrecisionDefault;

            // Apply light settings
            Vector3d localSpace = ScaledSpace.ScaledToLocalSpace(target.position);
            if (light)
            {
                light.color = shifter.sunlightColor;
                light.intensity =
                    shifter.intensityCurve.Evaluate((Single) Vector3d.Distance(sun.position, localSpace));
                light.shadowStrength = shifter.sunlightShadowStrength;
            }

            // Patch the ScaledSpace light
            if (scaledSunLight)
            {
                scaledSunLight.color = shifter.scaledSunlightColor;
                scaledSunLight.intensity = shifter.scaledIntensityCurve.Evaluate(
                    (Single) Vector3d.Distance(ScaledSpace.LocalToScaledSpace(sun.position), target.position));
            }

            if (HighLogic.LoadedSceneIsFlight && iva && iva.GetComponent<Light>())
            {
                iva.GetComponent<Light>().color = shifter.ivaSunColor;
                iva.GetComponent<Light>().intensity =
                    shifter.ivaIntensityCurve.Evaluate((Single) Vector3d.Distance(sun.position, localSpace));
            }

            // Set SunFlare color
            lensFlare.sunFlare.color = shifter.sunLensFlareColor;

            // Set other stuff
            lensFlare.AU = shifter.au;
            lensFlare.brightnessCurve = shifter.brightnessCurve.Curve;
            lensFlare.sun = sun;
            lensFlare.target = target;

            // States
            Boolean lightsOn = HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneHasPlanetarium ||
                               HighLogic.LoadedScene == GameScenes.SPACECENTER;
            light.enabled = shifter.givesOffLight && lightsOn;
            lensFlare.gameObject.SetActive(shifter.givesOffLight && lightsOn);
            sunFlare.enabled = false;
            if (useLocalSpaceSunLight && Instance.useLocalSpaceSunLight)
            {
                scaledSunLight.enabled = shifter.givesOffLight && lightsOn;
            }

            if (Current != null && Current.lensFlare != null)
            {
                SunFlare.Instance = Current.lensFlare;
            }

            // Update Scaled Space Light
            if (!useLocalSpaceSunLight)
            {
                return;
            }

            if (FlightGlobals.currentMainBody == null || FlightGlobals.currentMainBody == sun)
            {
                localTime = 1f;
            }
            else
            {
                Double targetAltitude = FlightGlobals.getAltitudeAtPos(localSpace, FlightGlobals.currentMainBody);
                if (targetAltitude < 0)
                {
                    targetAltitude = 0;
                }

                Double horizonAngle = Math.Acos(FlightGlobals.currentMainBody.Radius /
                                                (FlightGlobals.currentMainBody.Radius + targetAltitude));
                Single horizonScalar = -Mathf.Sin((Single) horizonAngle);
                Single dayNightRatio = 1f - Mathf.Abs(horizonScalar);
                Single fadeStartAtAlt = horizonScalar + fadeStart * dayNightRatio;
                Single fadeEndAtAlt = horizonScalar - fadeEnd * dayNightRatio;
                localTime = Vector3.Dot(-FlightGlobals.getUpAxis(localSpace), transform.forward);
                light.intensity = Mathf.Lerp(0f, light.intensity,
                    Mathf.InverseLerp(fadeEndAtAlt, fadeStartAtAlt, localTime));
            }
        }

        /// <summary>
        /// Override this function and use <see cref="Current"/> instead of Planetarium sun
        /// </summary>
        public override Double GetLocalTimeAtPosition(Vector3d wPos, CelestialBody cb)
        {
            Vector3d pos1 = Vector3d.Exclude(cb.angularVelocity, FlightGlobals.getUpAxis(cb, wPos));
            Vector3d pos2 = Vector3d.Exclude(cb.angularVelocity, Current.sun.position - cb.position);
            #pragma warning disable 618
            Double angle = (Vector3d.Dot(Vector3d.Cross(pos2, pos1), cb.angularVelocity) < 0 ? -1 : 1) *
                           Vector3d.AngleBetween(pos1, pos2) / 6.28318530717959 + 0.5;
            #pragma warning restore 618
            if (angle > Math.PI * 2)
            {
                angle -= Math.PI * 2;
            }

            return angle;
        }
    }
}
