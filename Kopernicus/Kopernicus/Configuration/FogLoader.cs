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

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class FogLoader : BaseLoader, IParserEventSubscriber
        {
            /// <summary>
            /// CelestialBody we're modifying
            /// </summary>
            public CelestialBody celestialBody { get; set; }

            // afgAltMult
            [ParserTarget("afgAltMult")]
            public NumericParser<Single> oceanAFGAltMult
            {
                get { return celestialBody.oceanAFGAltMult; }
                set { celestialBody.oceanAFGAltMult = value; }
            }

            // afgBase
            [ParserTarget("afgBase")]
            public NumericParser<Single> oceanAFGBase
            {
                get { return celestialBody.oceanAFGBase; }
                set { celestialBody.oceanAFGBase = value; }
            }

            // afgLerp
            [ParserTarget("afgLerp")]
            public NumericParser<Boolean> oceanAFGLerp
            {
                get { return celestialBody.oceanAFGLerp; }
                set { celestialBody.oceanAFGLerp = value; }
            }

            // afgMin
            [ParserTarget("afgMin")]
            public NumericParser<Single> oceanAFGMin
            {
                get { return celestialBody.oceanAFGMin; }
                set { celestialBody.oceanAFGMin = value; }
            }

            // fogColorEnd
            [ParserTarget("fogColorEnd")]
            public ColorParser oceanFogColorEnd
            {
                get { return celestialBody.oceanFogColorEnd; }
                set { celestialBody.oceanFogColorEnd = value; }
            }

            // fogColorStart
            [ParserTarget("fogColorStart")]
            public ColorParser oceanFogColorStart
            {
                get { return celestialBody.oceanFogColorStart; }
                set { celestialBody.oceanFogColorStart = value; }
            }

            // fogDensityAltScalar
            [ParserTarget("fogDensityAltScalar")]
            public NumericParser<Single> oceanFogDensityAltScalar
            {
                get { return celestialBody.oceanFogDensityAltScalar; }
                set { celestialBody.oceanFogDensityAltScalar = value; }
            }

            // fogDensityEnd
            [ParserTarget("fogDensityEnd")]
            public NumericParser<Single> oceanFogDensityEnd
            {
                get { return celestialBody.oceanFogDensityEnd; }
                set { celestialBody.oceanFogDensityEnd = value; }
            }

            // fogDensityExponent
            [ParserTarget("fogDensityExponent")]
            public NumericParser<Single> oceanFogDensityExponent
            {
                get { return celestialBody.oceanFogDensityExponent; }
                set { celestialBody.oceanFogDensityExponent = value; }
            }

            // fogDensityPQSMult
            [ParserTarget("fogDensityPQSMult")]
            public NumericParser<Single> oceanFogDensityPQSMult
            {
                get { return celestialBody.oceanFogDensityPQSMult; }
                set { celestialBody.oceanFogDensityPQSMult = value; }
            }

            // fogDensityStart
            [ParserTarget("fogDensityStart")]
            public NumericParser<Single> oceanFogDensityStart
            {
                get { return celestialBody.oceanFogDensityStart; }
                set { celestialBody.oceanFogDensityStart = value; }
            }

            // skyColorMult
            [ParserTarget("skyColorMult")]
            public NumericParser<Single> oceanSkyColorMult
            {
                get { return celestialBody.oceanSkyColorMult; }
                set { celestialBody.oceanSkyColorMult = value; }
            }

            // skyColorOpacityAltMult
            [ParserTarget("skyColorOpacityAltMult")]
            public NumericParser<Single> oceanSkyColorOpacityAltMult
            {
                get { return celestialBody.oceanSkyColorOpacityAltMult; }
                set { celestialBody.oceanSkyColorOpacityAltMult = value; }
            }

            // skyColorOpacityBase
            [ParserTarget("skyColorOpacityBase")]
            public NumericParser<Single> oceanSkyColorOpacityBase
            {
                get { return celestialBody.oceanSkyColorOpacityBase; }
                set { celestialBody.oceanSkyColorOpacityBase = value; }
            }

            // sunAltMult
            [ParserTarget("sunAltMult")]
            public NumericParser<Single> oceanSunAltMult
            {
                get { return celestialBody.oceanSunAltMult; }
                set { celestialBody.oceanSunAltMult = value; }
            }

            // sunBase
            [ParserTarget("sunBase")]
            public NumericParser<Single> oceanSunBase
            {
                get { return celestialBody.oceanSunBase; }
                set { celestialBody.oceanSunBase = value; }
            }

            // sunMin
            [ParserTarget("sunMin")]
            public NumericParser<Single> oceanSunMin
            {
                get { return celestialBody.oceanSunMin; }
                set { celestialBody.oceanSunMin = value; }
            }

            // useFog
            [ParserTarget("useFog")]
            public NumericParser<Boolean> oceanUseFog
            {
                get { return celestialBody.oceanUseFog; }
                set { celestialBody.oceanUseFog = value; }
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
                if (generatedBody == null)
                    throw new InvalidOperationException("Must be executed in Injector context.");

                // Store values
                celestialBody = generatedBody.celestialBody;
            }

            /// <summary>
            /// Creates a new Fog Loader from a spawned CelestialBody.
            /// </summary>
            public FogLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null)
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");

                // Store values
                celestialBody = body;
            }

            /// <summary>
            /// Creates a new Fog Loader from a custom PSystemBody.
            /// </summary>
            public FogLoader(PSystemBody body)
            {
                // Set generatedBody
                generatedBody = body ?? throw new InvalidOperationException("The body cannot be null.");

                // Store values
                celestialBody = generatedBody.celestialBody;
            }
        }
    }
}

