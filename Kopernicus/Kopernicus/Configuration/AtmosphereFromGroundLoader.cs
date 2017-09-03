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
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class AtmosphereFromGroundLoader : BaseLoader, IParserEventSubscriber
        {
            // since ScaledSpace doesn't exist to query.
            public const Single INVSCALEFACTOR = (1f / 6000f);

            // AtmosphereFromGround we're modifying
            public AtmosphereFromGround afg;
            public CelestialBody body;

            // DEBUG_alwaysUpdateAll
            [ParserTarget("DEBUG_alwaysUpdateAll")]
            public NumericParser<Boolean> DEBUG_alwaysUpdateAll
            {
                get { return afg.DEBUG_alwaysUpdateAll; }
                set { afg.DEBUG_alwaysUpdateAll = value; }
            }

            // doScale
            [ParserTarget("doScale")]
            public NumericParser<Boolean> doScale
            {
                get { return afg.doScale; }
                set { afg.doScale = value; }
            }

            // ESun
            [ParserTarget("ESun")]
            public NumericParser<Single> ESun
            {
                get { return afg.ESun; }
                set { afg.ESun = value; }
            }

            // g
            [ParserTarget("g")]
            public NumericParser<Single> g
            {
                get { return afg.g; }
                set { afg.g = value; }
            }

            // innerRadius
            [ParserTarget("innerRadius")]
            public NumericParser<Single> innerRadius
            {
                get { return afg.innerRadius / INVSCALEFACTOR; }
                set { afg.innerRadius = value * INVSCALEFACTOR; }
            }

            // invWaveLength
            [ParserTarget("invWaveLength")]
            public ColorParser invWaveLength
            {
                get { return afg.invWaveLength; }
                set
                {
                    afg.invWaveLength = value;
                    afg.waveLength = new Color((Single)Math.Sqrt(Math.Sqrt(1d / afg.invWaveLength[0])), (Single)Math.Sqrt(Math.Sqrt(1d / afg.invWaveLength[1])), (Single)Math.Sqrt(Math.Sqrt(1d / afg.invWaveLength[2])), 0.5f);
                }
            }

            // Km
            [ParserTarget("Km")]
            public NumericParser<Single> Km
            {
                get { return afg.Km; }
                set { afg.Km = value; }
            }

            // Kr
            [ParserTarget("Kr")]
            public NumericParser<Single> Kr
            {
                get { return afg.Kr; }
                set { afg.Kr = value; }
            }

            // outerRadius
            [ParserTarget("outerRadius")]
            public NumericParser<Single> outerRadius
            {
                get { return afg.outerRadius / INVSCALEFACTOR; }
                set { afg.outerRadius = value * INVSCALEFACTOR; }
            }

            // samples
            [ParserTarget("samples")]
            public NumericParser<Single> samples
            {
                get { return afg.samples; }
                set { afg.samples = value; }
            }

            // scale
            [ParserTarget("scale")]
            public NumericParser<Single> scale
            {
                get { return afg.scale; }
                set { afg.scale = value; }
            }

            // scaleDepth
            [ParserTarget("scaleDepth")]
            public NumericParser<Single> scaleDepth
            {
                get { return afg.scaleDepth; }
                set { afg.scaleDepth = value; }
            }

            [ParserTarget("transformScale")]
            public Vector3Parser transformScale
            {
                get { return afg.doScale ? Vector3.zero : afg.transform.localScale; }
                set { afg.transform.localScale = value; afg.doScale = false; }
            }

            // waveLength
            [ParserTarget("waveLength")]
            public ColorParser waveLength
            {
                get { return afg.waveLength; }
                set
                {
                    afg.waveLength = value;
                    afg.invWaveLength = new Color((Single)(1d / Math.Pow(afg.waveLength[0], 4)), (Single)(1d / Math.Pow(afg.waveLength[1], 4)), (Single)(1d / Math.Pow(afg.waveLength[2], 4)), 0.5f);
                }
            }

            // outerRadiusMult
            [ParserTarget("outerRadiusMult")]
            public NumericParser<Single> outerRadiusMult
            {
                get { return (afg.outerRadius / INVSCALEFACTOR) / (Single)body.Radius; }
                set { afg.outerRadius = (((Single)body.Radius) * value) * INVSCALEFACTOR; }
            }

            // innerRadiusMult
            [ParserTarget("innerRadiusMult")]
            public NumericParser<Single> innerRadiusMult
            {
                get { return afg.innerRadius / afg.outerRadius; }
                set { afg.innerRadius = afg.outerRadius * value; }
            }

            // Calculates the default members for the AFG
            public static void CalculatedMembers(AtmosphereFromGround atmo)
            {
                atmo.g2 = atmo.g * atmo.g;
                atmo.KrESun = atmo.Kr * atmo.ESun;
                atmo.KmESun = atmo.Km * atmo.ESun;
                atmo.Kr4PI = atmo.Kr * 4f * Mathf.PI;
                atmo.Km4PI = atmo.Km * 4f * Mathf.PI;
                atmo.outerRadius2 = atmo.outerRadius * atmo.outerRadius;
                atmo.innerRadius2 = atmo.innerRadius * atmo.innerRadius;
                atmo.scale = 1f / (atmo.outerRadius - atmo.innerRadius);
                atmo.scaleOverScaleDepth = atmo.scale / atmo.scaleDepth;

                if (atmo.doScale)
                    atmo.transform.localScale = Vector3.one * 1.025f;
            }

            // Parser apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                Events.OnAFGLoaderApply.Fire(this, node);
            }

            // Parser post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                CalculatedMembers(afg); // with the new values.
                Events.OnAFGLoaderPostApply.Fire(this, node);
            }

            // Default Constructor
            public AtmosphereFromGroundLoader()
            {
                body = generatedBody.celestialBody;
                afg = generatedBody.scaledVersion.GetComponentsInChildren<AtmosphereFromGround>(true)[0];
                
                // Set Defaults
                afg.planet = body;
                afg.ESun = 30f;
                afg.Kr = 0.00125f;
                afg.Km = 0.00015f;

                afg.samples = 4f;
                afg.g = -0.85f;
                if (afg.waveLength == new Color(0f, 0f, 0f, 0f))
                {
                    afg.waveLength = new Color(0.65f, 0.57f, 0.475f, 0.5f);
                }
                afg.outerRadius = (((Single)body.Radius) * 1.025f) * INVSCALEFACTOR;
                afg.innerRadius = afg.outerRadius * 0.975f;
                afg.scaleDepth = -0.25f;
                afg.invWaveLength = new Color((Single)(1d / Math.Pow(afg.waveLength[0], 4)), (Single)(1d / Math.Pow(afg.waveLength[1], 4)), (Single)(1d / Math.Pow(afg.waveLength[2], 4)), 0.5f);

                CalculatedMembers(afg);
            }

            // Runtime constructor
            public AtmosphereFromGroundLoader(CelestialBody body)
            {
                this.body = body;
                afg = body.afg;

                // Set Defaults
                afg.planet = body;
                afg.ESun = 30f;
                afg.Kr = 0.00125f;
                afg.Km = 0.00015f;

                afg.samples = 4f;
                afg.g = -0.85f;
                if (afg.waveLength == new Color(0f, 0f, 0f, 0f))
                {
                    afg.waveLength = new Color(0.65f, 0.57f, 0.475f, 0.5f);
                }
                afg.outerRadius = (((Single)body.Radius) * 1.025f) * INVSCALEFACTOR;
                afg.innerRadius = afg.outerRadius * 0.975f;
                afg.scaleDepth = -0.25f;
                afg.invWaveLength = new Color((Single)(1d / Math.Pow(afg.waveLength[0], 4)), (Single)(1d / Math.Pow(afg.waveLength[1], 4)), (Single)(1d / Math.Pow(afg.waveLength[2], 4)), 0.5f);

                CalculatedMembers(afg);
            }

        }
    }
}