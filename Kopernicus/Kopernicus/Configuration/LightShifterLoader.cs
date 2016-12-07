/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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
using Kopernicus.Components;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class LightShifterLoader : BaseLoader, IParserEventSubscriber
        {
            // The edited object
            public LightShifter lsc { get; set; }

            // The sunflare for the star
            [ParserTarget("sunFlare")]
            public AssetParser<Flare> sunFlare
            {
                get { return lsc.sunFlare; }
                set { lsc.sunFlare = value; }
            }

            // sunlightColor
            [ParserTarget("sunlightColor")]
            public ColorParser sunlightColor
            {
                get { return lsc.sunlightColor; }
                set { lsc.sunlightColor = value; }
            }

            // sunlightIntensity
            [ParserTarget("sunlightIntensity")]
            public NumericParser<float> sunlightIntensity
            {
                get { return lsc.sunlightIntensity; }
                set { lsc.sunlightIntensity = value; }
            }

            // sunlightShadowStrength
            [ParserTarget("sunlightShadowStrength")]
            public NumericParser<float> sunlightShadowStrength
            {
                get { return lsc.sunlightShadowStrength; }
                set { lsc.sunlightShadowStrength = value; }
            }

            // scaledSunlightColor
            [ParserTarget("scaledSunlightColor")]
            public ColorParser scaledSunlightColor
            {
                get { return lsc.scaledSunlightColor; }
                set { lsc.scaledSunlightColor = value; }
            }

            // scaledSunlightIntensity
            [ParserTarget("scaledSunlightIntensity")]
            public NumericParser<float> scaledSunlightIntensity
            {
                get { return lsc.scaledSunlightIntensity; }
                set { lsc.scaledSunlightIntensity = value; }
            }

            // IVASunColor
            [ParserTarget("IVASunColor")]
            public ColorParser IVASunColor
            {
                get { return lsc.IVASunColor; }
                set { lsc.IVASunColor = value; }
            }

            // IVASunIntensity
            [ParserTarget("IVASunIntensity")]
            public NumericParser<float> IVASunIntensity
            {
                get { return lsc.IVASunIntensity; }
                set { lsc.IVASunIntensity = value; }
            }

            // ambientLightColor
            [ParserTarget("ambientLightColor")]
            public ColorParser ambientLightColor
            {
                get { return lsc.ambientLightColor; }
                set { lsc.ambientLightColor = value; }
            }

            // Set the color that the star emits
            [ParserTarget("sunLensFlareColor")]
            public ColorParser sunLensFlareColor
            {
                get { return lsc.sunLensFlareColor; }
                set { lsc.sunLensFlareColor = value; }
            }

            // givesOffLight
            [ParserTarget("givesOffLight")]
            public NumericParser<bool> givesOffLight
            {
                get { return lsc.givesOffLight; }
                set { lsc.givesOffLight = value; }
            }

            // sunAU
            [ParserTarget("sunAU")]
            public NumericParser<double> sunAU
            {
                get { return lsc.AU; }
                set { lsc.AU = value; }
            }

            // brightnessCurve
            [ParserTarget("brightnessCurve")]
            public FloatCurveParser brightnessCurve
            {
                get { return lsc.brightnessCurve; }
                set { lsc.brightnessCurve = value; }
            }

            // sunAU
            [ParserTarget("luminosity")]
            public NumericParser<double> luminosity
            {
                get { return lsc.solarLuminosity; }
                set { lsc.solarLuminosity = value; }
            }

            // sunAU
            [ParserTarget("insolation")]
            public NumericParser<double> insolation
            {
                get { return lsc.solarInsolation; }
                set { lsc.solarInsolation = value; }
            }

            // sunAU
            [ParserTarget("radiationFactor")]
            public NumericParser<double> radiation
            {
                get { return lsc.radiationFactor; }
                set { lsc.radiationFactor = value; }
            }

            // Default constructor
            public LightShifterLoader()
            {
                lsc = Object.Instantiate(LightShifter.prefab) as LightShifter;
                lsc.transform.parent = generatedBody.scaledVersion.transform;
                lsc.name = generatedBody.name;
            }

            // Runtime Constructor, takes Celestial Body
            public LightShifterLoader(CelestialBody body)
            {
                lsc = body.GetComponentInChildren<LightShifter>();
            }

            // Apply event
            void IParserEventSubscriber.Apply(ConfigNode node) { }

            // Post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node) { }
        }

    }
}
