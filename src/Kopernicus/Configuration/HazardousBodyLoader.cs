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
using System.Diagnostics.CodeAnalysis;
using Kopernicus.Components;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;
using Kopernicus.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kopernicus.Configuration
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class HazardousBodyLoader : BaseLoader, ITypeParser<HazardousBody>
    {
        // Set-up our parental objects
        public HazardousBody Value { get; set; }

        // The maximum temperature that will eventually be reached.
        [ParserTarget("maxTemp")]
        [KittopiaDescription("The maximum temperature that will eventually be reached.")]
        public NumericParser<Double> MaxTemp
        {
            get { return Value.maxTemp; }
            set { Value.maxTemp = value; }
        }

        // How many seconds it'll take to get halfway to maxTemp.
        [ParserTarget("lambda")]
        [KittopiaDescription("How many seconds it'll take to get halfway to maxTemp.")]
        public NumericParser<Single> Lambda
        {
            get { return Value.lambda; }
            set { Value.lambda = value; }
        }

        // Multiplier curve to change maxTemp with altitude
        [ParserTarget("AltitudeCurve")]
        [KittopiaDescription("Multiplier curve to change maxTemp with altitude.")]
        public FloatCurveParser AltitudeCurve
        {
            get { return Value.altitudeCurve; }
            set { Value.altitudeCurve = value; }
        }

        // Multiplier curve to change maxTemp with latitude
        [ParserTarget("LatitudeCurve")]
        [KittopiaDescription("Multiplier curve to change maxTemp with latitude.")]
        public FloatCurveParser LatitudeCurve
        {
            get { return Value.latitudeCurve; }
            set { Value.latitudeCurve = value; }
        }

        // Multiplier curve to change maxTemp with longitude
        [ParserTarget("LongitudeCurve")]
        [KittopiaDescription("Multiplier curve to change maxTemp with longitude.")]
        public FloatCurveParser LongitudeCurve
        {
            get { return Value.longitudeCurve; }
            set { Value.longitudeCurve = value; }
        }

        // Multiplier map for maxTemp
        [ParserTarget("HeatMap")]
        [KittopiaDescription("Greyscale map for fine control of the maxTemp on a planet. black = 0, white = 1")]
        public MapSOParserGreyScale<MapSO> HeatMap
        {
            get { return Value.heatMap; }
            set { Value.heatMap = value; }
        }


        [KittopiaDestructor]
        public void Destroy()
        {
            Object.Destroy(Value);
        }

        /// <summary>
        /// Creates a new HazardousBody Loader from the Injector context.
        /// </summary>
        public HazardousBodyLoader()
        {
            // Is this the parser context?
            if (!Injector.IsInPrefab)
            {
                throw new InvalidOperationException("Must be executed in Injector context.");
            }

            // Store values
            Value = generatedBody.celestialBody.gameObject.AddComponent<HazardousBody>();
            Value.altitudeCurve = new FloatCurve(new[] {new Keyframe(0, 1)});
            Value.latitudeCurve = new FloatCurve(new[] {new Keyframe(0, 1)});
            Value.longitudeCurve = new FloatCurve(new[] {new Keyframe(0, 1)});
        }

        /// <summary>
        /// Creates a new HazardousBody Loader on a spawned CelestialBody.
        /// </summary>
        [KittopiaConstructor(KittopiaConstructor.ParameterType.CelestialBody)]
        public HazardousBodyLoader(CelestialBody body)
        {
            // Is this a spawned body?
            if (body.scaledBody == null || Injector.IsInPrefab)
            {
                throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
            }

            // Store values
            Value = body.gameObject.AddComponent<HazardousBody>();
            Value.altitudeCurve = new FloatCurve(new[] {new Keyframe(0, 1)});
            Value.latitudeCurve = new FloatCurve(new[] {new Keyframe(0, 1)});
            Value.longitudeCurve = new FloatCurve(new[] {new Keyframe(0, 1)});
        }

        /// <summary>
        /// Creates a new HazardousBody Loader from an already existing component
        /// </summary>
        public HazardousBodyLoader(HazardousBody value)
        {
            // Store values
            Value = value;

            // Null safe
            if (Value.altitudeCurve == null)
            {
                Value.altitudeCurve = new FloatCurve(new[] {new Keyframe(0, 1)});
            }

            if (Value.latitudeCurve == null)
            {
                Value.latitudeCurve = new FloatCurve(new[] {new Keyframe(0, 1)});
            }

            if (Value.longitudeCurve == null)
            {
                Value.longitudeCurve = new FloatCurve(new[] {new Keyframe(0, 1)});
            }
        }
    }
}