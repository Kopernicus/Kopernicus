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

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class FogLoader : BaseLoader
        {
            // Body we are going to modify
            public CelestialBody body { get; set; }

            // afgAltMult
            [ParserTarget("afgAltMult")]
            public NumericParser<float> oceanAFGAltMult
            {
                get { return body.oceanAFGAltMult; }
                set { body.oceanAFGAltMult = value; }
            }

            // afgBase
            [ParserTarget("afgBase")]
            public NumericParser<float> oceanAFGBase
            {
                get { return body.oceanAFGBase; }
                set { body.oceanAFGBase = value; }
            }

            // afgLerp
            [ParserTarget("afgLerp")]
            public NumericParser<bool> oceanAFGLerp
            {
                get { return body.oceanAFGLerp; }
                set { body.oceanAFGLerp = value; }
            }

            // afgMin
            [ParserTarget("afgMin")]
            public NumericParser<float> oceanAFGMin
            {
                get { return body.oceanAFGMin; }
                set { body.oceanAFGMin = value; }
            }

            // fogColorEnd
            [ParserTarget("fogColorEnd")]
            public ColorParser oceanFogColorEnd
            {
                get { return body.oceanFogColorEnd; }
                set { body.oceanFogColorEnd = value; }
            }

            // fogColorStart
            [ParserTarget("fogColorStart")]
            public ColorParser oceanFogColorStart
            {
                get { return body.oceanFogColorStart; }
                set { body.oceanFogColorStart = value; }
            }

            // fogDensityAltScalar
            [ParserTarget("fogDensityAltScalar")]
            public NumericParser<float> oceanFogDensityAltScalar
            {
                get { return body.oceanFogDensityAltScalar; }
                set { body.oceanFogDensityAltScalar = value; }
            }

            // fogDensityEnd
            [ParserTarget("fogDensityEnd")]
            public NumericParser<float> oceanFogDensityEnd
            {
                get { return body.oceanFogDensityEnd; }
                set { body.oceanFogDensityEnd = value; }
            }

            // fogDensityExponent
            [ParserTarget("fogDensityExponent")]
            public NumericParser<float> oceanFogDensityExponent
            {
                get { return body.oceanFogDensityExponent; }
                set { body.oceanFogDensityExponent = value; }
            }

            // fogDensityPQSMult
            [ParserTarget("fogDensityPQSMult")]
            public NumericParser<float> oceanFogDensityPQSMult
            {
                get { return body.oceanFogDensityPQSMult; }
                set { body.oceanFogDensityPQSMult = value; }
            }

            // fogDensityStart
            [ParserTarget("fogDensityStart")]
            public NumericParser<float> oceanFogDensityStart
            {
                get { return body.oceanFogDensityStart; }
                set { body.oceanFogDensityStart = value; }
            }

            // skyColorMult
            [ParserTarget("skyColorMult")]
            public NumericParser<float> oceanSkyColorMult
            {
                get { return body.oceanSkyColorMult; }
                set { body.oceanSkyColorMult = value; }
            }

            // skyColorOpacityAltMult
            [ParserTarget("skyColorOpacityAltMult")]
            public NumericParser<float> oceanSkyColorOpacityAltMult
            {
                get { return body.oceanSkyColorOpacityAltMult; }
                set { body.oceanSkyColorOpacityAltMult = value; }
            }

            // skyColorOpacityBase
            [ParserTarget("skyColorOpacityBase")]
            public NumericParser<float> oceanSkyColorOpacityBase
            {
                get { return body.oceanSkyColorOpacityBase; }
                set { body.oceanSkyColorOpacityBase = value; }
            }

            // sunAltMult
            [ParserTarget("sunAltMult")]
            public NumericParser<float> oceanSunAltMult
            {
                get { return body.oceanSunAltMult; }
                set { body.oceanSunAltMult = value; }
            }

            // sunBase
            [ParserTarget("sunBase")]
            public NumericParser<float> oceanSunBase
            {
                get { return body.oceanSunBase; }
                set { body.oceanSunBase = value; }
            }

            // sunMin
            [ParserTarget("sunMin")]
            public NumericParser<float> oceanSunMin
            {
                get { return body.oceanSunMin; }
                set { body.oceanSunMin = value; }
            }

            // useFog
            [ParserTarget("useFog")]
            public NumericParser<bool> oceanUseFog
            {
                get { return body.oceanUseFog; }
                set { body.oceanUseFog = value; }
            }

            // Default constructor
            public FogLoader()
            {
                body = generatedBody.celestialBody;
            }

            // Standard constructor takes a science parameters object
            public FogLoader(CelestialBody body)
            {
                this.body = body;
            }
        }
    }
}

