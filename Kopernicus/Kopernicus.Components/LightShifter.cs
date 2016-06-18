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

namespace Kopernicus
{
    namespace Components
    {
        // Class that manages the light of stars
        public class LightShifter : MonoBehaviour
        {
            // Variables
            public Color sunlightColor;
            public float sunlightIntensity;
            public float sunlightShadowStrength;
            public Color scaledSunlightColor;
            public float scaledSunlightIntensity;
            public Color IVASunColor;
            public float IVASunIntensity;
            public Color ambientLightColor;
            public Color sunLensFlareColor;
            public bool givesOffLight;
            public double AU;
            public FloatCurve brightnessCurve;
            public double solarInsolation;
            public double solarLuminosity;
            public double radiationFactor;
            public Flare sunFlare;

            // Prefab that makes every star yellow by default
            public static LightShifter prefab
            {
                get
                {
                    // If the prefab is null, create it
                    GameObject prefabGOB = new GameObject("LightShifter");
                    LightShifter prefab = prefabGOB.AddComponent<LightShifter>();

                    // Fill it with default values
                    prefab.sunlightColor = Color.white;
                    prefab.sunlightIntensity = 0.9f;
                    prefab.sunlightShadowStrength = 0.7523364f;
                    prefab.scaledSunlightColor = Color.white;
                    prefab.scaledSunlightIntensity = 0.9f;
                    prefab.IVASunColor = new Color(1.0f, 0.977f, 0.896f, 1.0f);
                    prefab.IVASunIntensity = 0.8099999f;
                    prefab.sunLensFlareColor = Color.white;
                    prefab.ambientLightColor = new Color(0.06f, 0.06f, 0.06f, 1.0f);
                    prefab.AU = 13599840256;
                    prefab.brightnessCurve = new FloatCurve(new Keyframe[]
                    {
                        new Keyframe(-0.01573471f, 0.217353f, 1.706627f, 1.706627f),
                        new Keyframe(5.084181f, 3.997075f, -0.001802375f, -0.001802375f),
                        new Keyframe(38.56295f, 1.82142f, 0.0001713f, 0.0001713f)
                    });
                    prefab.givesOffLight = true;
                    prefab.solarInsolation = PhysicsGlobals.SolarInsolationAtHome;
                    prefab.solarLuminosity = PhysicsGlobals.SolarLuminosityAtHome;
                    prefab.radiationFactor = PhysicsGlobals.RadiationFactor;
                    prefab.sunFlare = null;

                    // Return the prefab
                    return prefab;
                }
            }

            // Patches all light sources
            public void Apply(Light light, Light scaledLight, Light ivaLight)
            {
                // Patch the local space light
                if (light)
                {
                    light.color = sunlightColor;
                    light.intensity = sunlightIntensity;
                    light.shadowStrength = sunlightShadowStrength;
                }

                // Patch the ScaledSpace light
                if (scaledLight)
                {
                    scaledLight.color = scaledSunlightColor;
                    scaledLight.intensity = scaledSunlightIntensity;
                }

                if (HighLogic.LoadedSceneIsFlight && ivaLight)
                {
                    ivaLight.color = IVASunColor;
                    ivaLight.intensity = IVASunIntensity;
                }

            }

            /// <summary>
            /// Applies the ambient color.
            /// </summary>
            public void ApplyAmbient()
            {
                DynamicAmbientLight ambientLight = FindObjectOfType(typeof (DynamicAmbientLight)) as DynamicAmbientLight;
                if (ambientLight)
                    ambientLight.vacuumAmbientColor = ambientLightColor;
            }
        }
    }
}