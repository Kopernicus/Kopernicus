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

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class HazardousBodyLoader : BaseLoader, ITypeParser<HazardousBody>
        {
            // Set-up our parental objects
            public HazardousBody Value { get; set; }

            // The average heat on the body
            [ParserTarget("heat")]
            [KittopiaDescription("The average heat on the body.")]
            public NumericParser<Double> heat
            {
                get { return Value.HeatRate; }
                set { Value.HeatRate = value; }
            }

            // How much time passes between applying the heat to a vessel
            [ParserTarget("interval")]
            [KittopiaDescription("How much time passes between applying the heat to a vessel.")]
            public NumericParser<Single> interval
            {
                get { return Value.HeatInterval; }
                set { Value.HeatInterval = value; }
            }

            // Controls the how much of the average heat gets applied at a certain altitude
            [ParserTarget("AltitudeCurve")]
            [KittopiaDescription("Controls the how much of the average heat gets applied at a certain altitude.")]
            public FloatCurveParser altitudeCurve
            {
                get { return Value.AltitudeCurve; }
                set { Value.AltitudeCurve = value; }
            }

            // Controls the how much of the average heat gets applied at a certain latitude
            [ParserTarget("LatitudeCurve")]
            [KittopiaDescription("Controls the how much of the average heat gets applied at a certain latitude.")]
            public FloatCurveParser latitudeCurve
            {
                get { return Value.LatitudeCurve; }
                set { Value.LatitudeCurve = value; }
            }

            // Controls the how much of the average heat gets applied at a certain longitude
            [ParserTarget("LongitudeCurve")]
            [KittopiaDescription("Controls the how much of the average heat gets applied at a certain longitude.")]
            public FloatCurveParser longitudeCurve
            {
                get { return Value.LongitudeCurve; }
                set { Value.LongitudeCurve = value; }
            }

            [KittopiaDestructor]
            public void Destroy()
            {
                UnityEngine.Object.Destroy(Value);
            }

            /// <summary>
            /// Creates a new Particle Loader from the Injector context.
            /// </summary>
            public HazardousBodyLoader()
            {
                // Is this the parser context?
                if (!Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("Must be executed in Injector context.");
                }
                
                // Store values
                Value = generatedBody.celestialBody.gameObject.AddOrGetComponent<HazardousBody>();
            }

            /// <summary>
            /// Creates a new Particle Loader on a spawned CelestialBody.
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.CelestialBody)]
            public HazardousBodyLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }

                // Store values
                Value = body.gameObject.AddOrGetComponent<HazardousBody>();
            }
        }
    }
}
