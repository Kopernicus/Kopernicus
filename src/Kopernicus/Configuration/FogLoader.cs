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

using System;
using Kopernicus.UI;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class FogLoader : BaseLoader, IParserEventSubscriber, ITypeParser<CelestialBody>
        {
            /// <summary>
            /// CelestialBody we're modifying
            /// </summary>
            public CelestialBody Value { get; set; }

            // afgAltMult
            [ParserTarget("afgAltMult")]
            public NumericParser<Single> oceanAFGAltMult
            {
                get { return Value.oceanAFGAltMult; }
                set { Value.oceanAFGAltMult = value; }
            }

            // afgBase
            [ParserTarget("afgBase")]
            public NumericParser<Single> oceanAFGBase
            {
                get { return Value.oceanAFGBase; }
                set { Value.oceanAFGBase = value; }
            }

            // afgLerp
            [ParserTarget("afgLerp")]
            public NumericParser<Boolean> oceanAFGLerp
            {
                get { return Value.oceanAFGLerp; }
                set { Value.oceanAFGLerp = value; }
            }

            // afgMin
            [ParserTarget("afgMin")]
            public NumericParser<Single> oceanAFGMin
            {
                get { return Value.oceanAFGMin; }
                set { Value.oceanAFGMin = value; }
            }

            // fogColorEnd
            [ParserTarget("fogColorEnd")]
            public ColorParser oceanFogColorEnd
            {
                get { return Value.oceanFogColorEnd; }
                set { Value.oceanFogColorEnd = value; }
            }

            // fogColorStart
            [ParserTarget("fogColorStart")]
            public ColorParser oceanFogColorStart
            {
                get { return Value.oceanFogColorStart; }
                set { Value.oceanFogColorStart = value; }
            }

            // fogDensityAltScalar
            [ParserTarget("fogDensityAltScalar")]
            public NumericParser<Single> oceanFogDensityAltScalar
            {
                get { return Value.oceanFogDensityAltScalar; }
                set { Value.oceanFogDensityAltScalar = value; }
            }

            // fogDensityEnd
            [ParserTarget("fogDensityEnd")]
            public NumericParser<Single> oceanFogDensityEnd
            {
                get { return Value.oceanFogDensityEnd; }
                set { Value.oceanFogDensityEnd = value; }
            }

            // fogDensityExponent
            [ParserTarget("fogDensityExponent")]
            public NumericParser<Single> oceanFogDensityExponent
            {
                get { return Value.oceanFogDensityExponent; }
                set { Value.oceanFogDensityExponent = value; }
            }

            // fogDensityPQSMult
            [ParserTarget("fogDensityPQSMult")]
            public NumericParser<Single> oceanFogDensityPQSMult
            {
                get { return Value.oceanFogDensityPQSMult; }
                set { Value.oceanFogDensityPQSMult = value; }
            }

            // fogDensityStart
            [ParserTarget("fogDensityStart")]
            public NumericParser<Single> oceanFogDensityStart
            {
                get { return Value.oceanFogDensityStart; }
                set { Value.oceanFogDensityStart = value; }
            }

            // skyColorMult
            [ParserTarget("skyColorMult")]
            public NumericParser<Single> oceanSkyColorMult
            {
                get { return Value.oceanSkyColorMult; }
                set { Value.oceanSkyColorMult = value; }
            }

            // skyColorOpacityAltMult
            [ParserTarget("skyColorOpacityAltMult")]
            public NumericParser<Single> oceanSkyColorOpacityAltMult
            {
                get { return Value.oceanSkyColorOpacityAltMult; }
                set { Value.oceanSkyColorOpacityAltMult = value; }
            }

            // skyColorOpacityBase
            [ParserTarget("skyColorOpacityBase")]
            public NumericParser<Single> oceanSkyColorOpacityBase
            {
                get { return Value.oceanSkyColorOpacityBase; }
                set { Value.oceanSkyColorOpacityBase = value; }
            }

            // sunAltMult
            [ParserTarget("sunAltMult")]
            public NumericParser<Single> oceanSunAltMult
            {
                get { return Value.oceanSunAltMult; }
                set { Value.oceanSunAltMult = value; }
            }

            // sunBase
            [ParserTarget("sunBase")]
            public NumericParser<Single> oceanSunBase
            {
                get { return Value.oceanSunBase; }
                set { Value.oceanSunBase = value; }
            }

            // sunMin
            [ParserTarget("sunMin")]
            public NumericParser<Single> oceanSunMin
            {
                get { return Value.oceanSunMin; }
                set { Value.oceanSunMin = value; }
            }

            // useFog
            [ParserTarget("useFog")]
            public NumericParser<Boolean> oceanUseFog
            {
                get { return Value.oceanUseFog; }
                set { Value.oceanUseFog = value; }
            }

            // Parser apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                Events.OnFogLoaderApply.Fire(this, node);
            }

            // Parser post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                Events.OnFogLoaderPostApply.Fire(this, node);
            }

            /// <summary>
            /// Creates a new Fog Loader from the Injector context.
            /// </summary>
            public FogLoader()
            {
                // Is this the parser context?
                if (!Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("Must be executed in Injector context.");
                }

                // Store values
                Value = generatedBody.celestialBody;
            }

            /// <summary>
            /// Creates a new Fog Loader from a spawned CelestialBody.
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.CelestialBody)]
            public FogLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }

                // Store values
                Value = body;
            }
        }
    }
}

