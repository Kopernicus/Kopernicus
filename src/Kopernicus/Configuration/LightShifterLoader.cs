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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using Kopernicus.Components;
using System;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class LightShifterLoader : BaseLoader, IParserEventSubscriber, ITypeParser<LightShifter>
        {
            /// <summary>
            /// LightShifter we're modifying
            /// </summary>
            public LightShifter Value { get; set; }

            // The sunflare for the star
            [ParserTarget("sunFlare")]
            [KittopiaDescription("The asset bundle containing a Unity LensFlare object that should be applied to the star.")]
            public AssetParser<Flare> sunFlare
            {
                get { return Value.sunFlare; }
                set { Value.sunFlare = value; }
            }

            // sunlightColor
            [ParserTarget("sunlightColor")]
            [KittopiaDescription("The color of the LocalSpace starlight. Influences vessels and PQS terrain.")]
            public ColorParser sunlightColor
            {
                get { return Value.sunlightColor; }
                set { Value.sunlightColor = value; }
            }

            // sunlightIntensity
            [ParserTarget("sunlightIntensity")]
            [KittopiaDescription("The intensity of the LocalSpace starlight. Usage not recommended, because of a lacking distance limit. Use IntensityCurve instead.")]
            public NumericParser<Single> sunlightIntensity
            {
                set
                {
                    Value.intensityCurve = new FloatCurve(new Keyframe[]
                    {
                        new Keyframe(0, value),
                        new Keyframe(1, value)
                    });
                }
            }

            // sunlightShadowStrength
            [ParserTarget("sunlightShadowStrength")]
            [KittopiaDescription("The strength of the shadows caused by LocalSpace starlight.")]
            public NumericParser<Single> sunlightShadowStrength
            {
                get { return Value.sunlightShadowStrength; }
                set { Value.sunlightShadowStrength = value; }
            }

            // scaledSunlightColor
            [ParserTarget("scaledSunlightColor")]
            [KittopiaDescription("The color of the ScaledSpace starlight. Influences the ScaledSpace representation of the bodies.")]
            public ColorParser scaledSunlightColor
            {
                get { return Value.scaledSunlightColor; }
                set { Value.scaledSunlightColor = value; }
            }

            // scaledSunlightIntensity
            [ParserTarget("scaledSunlightIntensity")]
            [KittopiaDescription("The intensity of the ScaledSpace starlight. Usage not recommended, because of a lacking distance limit. Use ScaledIntensityCurve instead.")]
            public NumericParser<Single> scaledSunlightIntensity
            {
                set
                {
                    Value.scaledIntensityCurve = new FloatCurve(new Keyframe[]
                    {
                        new Keyframe(0, value),
                        new Keyframe(1, value)
                    });
                }
            }

            // IVASunColor
            [ParserTarget("IVASunColor")]
            [KittopiaDescription("The color of the starlight in IVA view.")]
            public ColorParser IVASunColor
            {
                get { return Value.IVASunColor; }
                set { Value.IVASunColor = value; }
            }

            // IVASunIntensity
            [ParserTarget("IVASunIntensity")]
            [KittopiaDescription("The intensity of the IVA starlight. Usage not recommended, because of a lacking distance limit. Use IVAIntensityCurve instead.")]
            public NumericParser<Single> IVASunIntensity
            {
                set
                {
                    Value.ivaIntensityCurve = new FloatCurve(new Keyframe[]
                    {
                        new Keyframe(0, value),
                        new Keyframe(1, value)
                    });
                }
            }

            // ambientLightColor
            [ParserTarget("ambientLightColor")]
            [KittopiaDescription("The color of ambient lighting when orbiting near the star.")]
            public ColorParser ambientLightColor
            {
                get { return Value.ambientLightColor; }
                set { Value.ambientLightColor = value; }
            }

            // Set the color that the star emits
            [ParserTarget("sunLensFlareColor")]
            [KittopiaDescription("The color of the stars LensFlare effect. Gets multiplied with the color of the base texture (yellow-ish for stock flare).")]
            public ColorParser sunLensFlareColor
            {
                get { return Value.sunLensFlareColor; }
                set { Value.sunLensFlareColor = value; }
            }

            // givesOffLight
            [ParserTarget("givesOffLight")]
            [KittopiaDescription("Whether the star should emit light and have a LensFlare effect.")]
            public NumericParser<Boolean> givesOffLight
            {
                get { return Value.givesOffLight; }
                set { Value.givesOffLight = value; }
            }

            // sunAU
            [ParserTarget("sunAU")]
            [KittopiaDescription("TODO")]
            public NumericParser<Double> sunAU
            {
                get { return Value.AU; }
                set { Value.AU = value; }
            }

            // brightnessCurve
            [ParserTarget("brightnessCurve")]
            [KittopiaDescription("Associates a distance value with a multiplier for the brightness of the LensFlare effect.")]
            public FloatCurveParser brightnessCurve
            {
                get { return Value.brightnessCurve; }
                set { Value.brightnessCurve = value; }
            }

            // sunAU
            [ParserTarget("luminosity")]
            [KittopiaDescription("TODO")]
            public NumericParser<Double> luminosity
            {
                get { return Value.solarLuminosity; }
                set { Value.solarLuminosity = value; }
            }

            // sunAU
            [ParserTarget("insolation")]
            [KittopiaDescription("TODO")]
            public NumericParser<Double> insolation
            {
                get { return Value.solarInsolation; }
                set { Value.solarInsolation = value; }
            }

            // sunAU
            [ParserTarget("radiationFactor")]
            [KittopiaDescription("TODO")]
            public NumericParser<Double> radiation
            {
                get { return Value.radiationFactor; }
                set { Value.radiationFactor = value; }
            }

            // intensityCurve
            [ParserTarget("IntensityCurve")]
            [KittopiaDescription("Associates a distance value (in meters) with a value that describes the intensity of the LocalSpace starlight at that point.")]
            public FloatCurveParser intensityCurve
            {
                get { return Value.intensityCurve; }
                set { Value.intensityCurve = value; }
            }

            // scaledIntensityCurve
            [ParserTarget("ScaledIntensityCurve")]
            [KittopiaDescription("Associates a distance value (in meters / 6000) with a value that describes the intensity of the ScaledSpace starlight at that point.")]
            public FloatCurveParser scaledIntensityCurve
            {
                get { return Value.scaledIntensityCurve; }
                set { Value.scaledIntensityCurve = value; }
            }

            // intensityCurve
            [ParserTarget("IVAIntensityCurve")]
            [KittopiaDescription("Associates a distance value (in meters) with a value that describes the intensity of the IVA starlight at that point.")]
            public FloatCurveParser ivaIntensityCurve
            {
                get { return Value.ivaIntensityCurve; }
                set { Value.ivaIntensityCurve = value; }
            }

            // Parser apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                Events.OnLightShifterLoaderApply.Fire(this, node);
            }

            // Parser post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                Events.OnLightShifterLoaderPostApply.Fire(this, node);
            }

            /// <summary>
            /// Creates a new LightShifter Loader from the Injector context.
            /// </summary>
            public LightShifterLoader()
            {
                // Is this the parser context?
                if (!Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("Must be executed in Injector context.");
                }

                // Store values
                Value = LightShifter.prefab;
                Value.transform.parent = generatedBody.scaledVersion.transform;
                Value.name = generatedBody.name;
            }

            /// <summary>
            /// Creates a new LightShifter Loader from an already existing body
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.CelestialBody)]
            public LightShifterLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }

                // Store values or create a new light shifter
                Value = body.scaledBody.GetComponentInChildren<LightShifter>();
                if (Value == null)
                {
                    Value = LightShifter.prefab;
                    Value.transform.parent = body.scaledBody.transform;
                    Value.name = body.transform.name;
                }
            }
        }

    }
}
