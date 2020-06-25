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
        /// A dictionary to get the <see cref="KopernicusStar"/> component using the <see cref="CelestialBody"/>.
        /// </summary>
        public static Dictionary<CelestialBody, KopernicusStar> CelestialBodies;

        /// <summary>
        /// A list of all <see cref="Sun"/><i>s</i> and their luminosity
        /// </summary>
        public static Dictionary<Sun, Double> StarsLuminosity
        {
            get
            {
                return Stars.ToDictionary(star => star as Sun, star => star.shifter.solarLuminosity);
            }
        }

        /// <summary>
        /// The currently active <see cref="KopernicusStar"/>, for stuff we can't patch
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
        /// The <see cref="SunFlare"/> component that controls the lensflare assigned to this star
        /// </summary>
        public KopernicusSunFlare lensFlare;

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
                    shifter.intensityCurve.Evaluate((Single)Vector3d.Distance(sun.position, localSpace));
                light.shadowStrength = shifter.sunlightShadowStrength;
            }

            // Patch the ScaledSpace light
            if (scaledSunLight)
            {
                scaledSunLight.color = shifter.scaledSunlightColor;
                scaledSunLight.intensity = shifter.scaledIntensityCurve.Evaluate(
                    (Single)Vector3d.Distance(ScaledSpace.LocalToScaledSpace(sun.position), target.position));
            }

            if (HighLogic.LoadedSceneIsFlight && iva && iva.GetComponent<Light>())
            {
                iva.GetComponent<Light>().color = shifter.ivaSunColor;
                iva.GetComponent<Light>().intensity =
                    shifter.ivaIntensityCurve.Evaluate((Single)Vector3d.Distance(sun.position, localSpace));
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
                Single horizonScalar = -Mathf.Sin((Single)horizonAngle);
                Single dayNightRatio = 1f - Mathf.Abs(horizonScalar);
                Single fadeStartAtAlt = horizonScalar + fadeStart * dayNightRatio;
                Single fadeEndAtAlt = horizonScalar - fadeEnd * dayNightRatio;
                localTime = Vector3.Dot(-FlightGlobals.getUpAxis(localSpace), transform.forward);
                light.intensity = Mathf.Lerp(0f, light.intensity,
                    Mathf.InverseLerp(fadeEndAtAlt, fadeStartAtAlt, localTime));
            }
        }

        /// <summary>
        /// Returns the <see cref="Vessel.solarFlux"/> at the given location.
        /// </summary>
        public Double CalculateFluxAt(Vessel vessel)
        {
            // Get sunVector
            Boolean directSunlight = false;
            Vector3 integratorPosition = vessel.transform.position;
            Vector3d scaledSpace = ScaledSpace.LocalToScaledSpace(integratorPosition);
            Vector3 position = sun.scaledBody.transform.position;
            Double scale = Math.Max((position - scaledSpace).magnitude, 1);
            Vector3 sunVector = (position - scaledSpace) / scale;
            Ray ray = new Ray(ScaledSpace.LocalToScaledSpace(integratorPosition), sunVector);

            // Get Thermal Stats
            if (vessel.mainBody.atmosphere && !vessel.mainBody.isStar)
            {
                if (sun == GetBodyReferencing(vessel.mainBody))
                {
                    FlightIntegrator FI = vessel.GetComponent<FlightIntegrator>();
                    vessel.mainBody.GetAtmoThermalStats(true, sun, sunVector, Vector3d.Dot(sunVector, vessel.upAxis), vessel.upAxis, vessel.altitude, out FI.atmosphereTemperatureOffset, out FI.bodyEmissiveFlux, out FI.bodyAlbedoFlux);
                }
            }

            // Get Solar Flux
            Double realDistanceToSun = 0;
            if (!Physics.Raycast(ray, out RaycastHit raycastHit, Single.MaxValue, ModularFlightIntegrator.SunLayerMask))
            {
                directSunlight = true;
                realDistanceToSun = scale * ScaledSpace.ScaleFactor - sun.Radius;
            }
            else if (raycastHit.transform.GetComponent<ScaledMovement>().celestialBody == sun)
            {
                realDistanceToSun = ScaledSpace.ScaleFactor * raycastHit.distance;
                directSunlight = true;
            }

            if (directSunlight)
            {
                Double output = PhysicsGlobals.SolarLuminosity / (12.5663706143592 * realDistanceToSun * realDistanceToSun);
                return output;
            }

            return 0;
        }

        /// <summary>
        /// Override for <see cref="FlightIntegrator.CalculateSunBodyFlux"/>
        /// </summary>
        public static void SunBodyFlux(ModularFlightIntegrator MFI)
        {
            // Nullchecks
            if (MFI.Vessel == null || MFI.Vessel.state == Vessel.State.DEAD || MFI.CurrentMainBody == null)
            {
                return;
            }

            Double solarFlux = 0;

            // Calculate the values for all bodies
            for (Int32 i = 0; i < KopernicusStar.Stars.Count; i++)
            {
                KopernicusStar star = KopernicusStar.Stars[i];

                if (star == KopernicusStar.Current)
                {
                    continue;
                }

                // Set Physics
                star.shifter.ApplyPhysics();

                // Calculate Flux
                solarFlux += star.CalculateFluxAt(MFI.Vessel) * PhysicsGlobals.SolarLuminosityAtHome / 1360;
            }

            // Set Physics to the Current Star
            KopernicusStar.Current.shifter.ApplyPhysics();

            // Calculate Flux
            solarFlux += Current.CalculateFluxAt(MFI.Vessel) * PhysicsGlobals.SolarLuminosityAtHome / 1360;

            // Reapply
            MFI.Vessel.directSunlight = solarFlux > 0;
            MFI.solarFlux = solarFlux;
        }

        /// <summary>
        /// Returns the <see cref="CelestialBody"/> directly orbiting the parent <see cref="KopernicusStar"/> for a given <see cref="CelestialBody"/>.
        /// </summary>
        public static CelestialBody GetBodyReferencing(CelestialBody body)
        {
            while (body?.orbit?.referenceBody != null && !body.orbit.referenceBody.isStar)
            {
                body = body.orbit.referenceBody;
            }

            return body;
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
