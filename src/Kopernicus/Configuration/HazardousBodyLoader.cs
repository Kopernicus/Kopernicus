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

        // The average heat on the body
        [ParserTarget("heat")]
        [KittopiaDescription("The average heat on the body.")]
        public NumericParser<Double> Heat
        {
            get { return Value.heatRate; }
            set { Value.heatRate = value; }
        }

        // How much time passes between applying the heat to a vessel
        [ParserTarget("interval")]
        [KittopiaDescription("How much time passes between applying the heat to a vessel.")]
        public NumericParser<Single> Interval
        {
            get { return Value.heatInterval; }
            set { Value.heatInterval = value; }
        }

        // Controls the how much of the average heat gets applied at a certain altitude
        [ParserTarget("AltitudeCurve")]
        [KittopiaDescription("Controls the how much of the average heat gets applied at a certain altitude.")]
        public FloatCurveParser AltitudeCurve
        {
            get { return Value.altitudeCurve; }
            set { Value.altitudeCurve = value; }
        }

        // Controls the how much of the average heat gets applied at a certain latitude
        [ParserTarget("LatitudeCurve")]
        [KittopiaDescription("Controls the how much of the average heat gets applied at a certain latitude.")]
        public FloatCurveParser LatitudeCurve
        {
            get { return Value.latitudeCurve; }
            set { Value.latitudeCurve = value; }
        }

        // Controls the how much of the average heat gets applied at a certain longitude
        [ParserTarget("LongitudeCurve")]
        [KittopiaDescription("Controls the how much of the average heat gets applied at a certain longitude.")]
        public FloatCurveParser LongitudeCurve
        {
            get { return Value.longitudeCurve; }
            set { Value.longitudeCurve = value; }
        }

        // Controls the how much of the average heat gets applied at a certain longitude
        [ParserTarget("HeatMap")]
        [KittopiaDescription("Greyscale map for fine control of the heat on a planet. black = 0, white = 1")]
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