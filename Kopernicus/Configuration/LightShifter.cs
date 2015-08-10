/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class LightShifter : IParserEventSubscriber
        {
            public LightShifterComponent lsc;

            // sunlightColor
            [ParserTarget("sunlightColor", optional = true, allowMerge = false)]
            public ColorParser sunlightColor
            {
                set { lsc.sunlightColor = value.value; }
            }

            // sunlightIntensity
            [ParserTarget("sunlightIntensity", optional = true, allowMerge = false)]
            public NumericParser<float> sunlightIntensity
            {
                set { lsc.sunlightIntensity = value.value; }
            }

            // sunlightShadowStrength
            [ParserTarget("sunlightShadowStrength", optional = true, allowMerge = false)]
            public NumericParser<float> sunlightShadowStrength
            {
                set { lsc.sunlightShadowStrength = value.value; }
            }

            // scaledSunlightColor
            [ParserTarget("scaledSunlightColor", optional = true, allowMerge = false)]
            public ColorParser scaledSunlightColor
            {
                set { lsc.scaledSunlightColor = value.value; }
            }

            // scaledSunlightIntensity
            [ParserTarget("scaledSunlightIntensity", optional = true, allowMerge = false)]
            public NumericParser<float> scaledSunlightIntensity
            {
                set { lsc.scaledSunlightIntensity = value.value; }
            }

            // IVASunColor
            [ParserTarget("IVASunColor", optional = true, allowMerge = false)]
            public ColorParser IVASunColor
            {
                set { lsc.IVASunColor = value.value; }
            }

            // IVASunIntensity
            [ParserTarget("IVASunIntensity", optional = true, allowMerge = false)]
            public NumericParser<float> IVASunIntensity
            {
                set { lsc.IVASunIntensity = value.value; }
            }

            // ambientLightColor
            [ParserTarget("ambientLightColor", optional = true, allowMerge = false)]
            public ColorParser ambientLightColor
            {
                set { lsc.ambientLightColor = value.value; }
            }

            // Set the color that the star emits
            [ParserTarget("sunLensFlareColor", optional = true)]
            private ColorParser sunLensFlareColor
            {
                set { lsc.sunLensFlareColor = value.value; }
            }

            // givesOffLight
            [ParserTarget("givesOffLight", optional = true, allowMerge = false)]
            public NumericParser<bool> givesOffLight
            {
                set { lsc.givesOffLight = value.value; }
            }

            // sunAU
            [ParserTarget("sunAU", optional = true, allowMerge = false)]
            public NumericParser<double> sunAU
            {
                set { lsc.AU = value.value; }
            }

            // brightnessCurve
            [ParserTarget("brightnessCurve", optional = true, allowMerge = false)]
            public FloatCurveParser brightnessCurve
            {
                set { lsc.brightnessCurve = value.curve; }
            }

            public LightShifter()
            {
                lsc = LightShifterComponent.Instantiate(LightShifterComponent.LightSwitcherPrefab) as LightShifterComponent;
            }

            // Apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
            }

            // Post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
            }
        }

        public class LightShifterComponent : MonoBehaviour
        {
            public Color sunlightColor;
            public float sunlightIntensity;
            public float sunlightShadowStrength;
            public Color scaledSunlightColor;
            public float scaledSunlightIntensity;
            public Color IVASunColor;
            public float IVASunIntensity;
            public Color ambientLightColor;
            public Color sunLensFlareColor;
            public bool givesOffLight = true;
            public double AU;
            public FloatCurve brightnessCurve;

            private static LightShifterComponent prefab;

            public static LightShifterComponent LightSwitcherPrefab
            {
                get
                {
                    if (prefab == null)
                    {
                        // If the prefab is null, create it
                        GameObject prefabGOB = new GameObject("LightShifter");
                        prefabGOB.transform.parent = Utility.Deactivator;
                        prefab = prefabGOB.AddComponent<LightShifterComponent>();

                        // Fill it with default values
                        prefab.sunlightColor = Color.white;
                        prefab.sunlightIntensity = 0.45f;
                        prefab.sunlightShadowStrength = 0.7523364f;
                        prefab.scaledSunlightColor = Color.white;
                        prefab.scaledSunlightIntensity = 0.45f;
                        prefab.IVASunColor = new Color(1.0f, 0.977f, 0.896f, 1.0f);
                        prefab.IVASunIntensity = 0.34f;
                        prefab.sunLensFlareColor = Color.white;
                        prefab.ambientLightColor = new Color(0.06f, 0.06f, 0.06f, 1.0f);
                        prefab.AU = 13599840256;
                        prefab.brightnessCurve = new FloatCurve(new Keyframe[] 
                        { 
                            new Keyframe(-0.01573471f, 0.217353f, 1.706627f, 1.706627f),
                            new Keyframe(5.084181f, 3.997075f, -0.001802375f, -0.001802375f),
                            new Keyframe(38.56295f, 1.82142f, 0.0001713f, 0.0001713f)
                        });
                    }

                    // Return the prefab
                    return prefab;
                }
            }

            private bool isActive = false;

            public void SetStatus(bool status, GameScenes scene)
            {
                this.isActive = status;

                if (isActive)
                {
                    SetActive(scene);
                }
            }

            private void OnLevelWasLoaded(int loadedLevel)
            {
                if (isActive)
                {
                    SetActive((GameScenes)loadedLevel);
                }
            }

            private void SetActive(GameScenes scene)
            {
                GameObject sunLight = GameObject.Find("SunLight");
                GameObject scaledSunLight = GameObject.Find("Scaledspace SunLight");

                if (sunLight && scaledSunLight)
                {
                    if (sunlightColor != null)
                        sunLight.light.color = sunlightColor;

                    if (sunlightIntensity != float.NaN)
                        sunLight.light.intensity = sunlightIntensity;

                    if (sunlightShadowStrength != float.NaN)
                        sunLight.light.shadowStrength = sunlightShadowStrength;

                    if (scaledSunlightColor != null)
                        scaledSunLight.light.color = scaledSunlightColor;

                    if (scaledSunlightIntensity != float.NaN)
                        scaledSunLight.light.intensity = scaledSunlightIntensity;

                    if (scene == GameScenes.FLIGHT)
                    {
                        GameObject IVASun = GameObject.Find("IVASun");

                        if (IVASun)
                        {
                            if (IVASunColor != null)
                                IVASun.light.color = IVASunColor;

                            if (IVASunIntensity != float.NaN)
                                IVASun.light.intensity = IVASunIntensity;
                        }
                    }

                    DynamicAmbientLight ambientLight = FindObjectOfType(typeof(DynamicAmbientLight)) as DynamicAmbientLight;

                    if (ambientLightColor != null && ambientLight)
                        ambientLight.vacuumAmbientColor = ambientLightColor;

                }
            }
        }
    }
}
