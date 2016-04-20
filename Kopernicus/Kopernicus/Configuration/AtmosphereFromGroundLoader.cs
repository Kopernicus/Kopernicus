using Kopernicus.Components;
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
            public const float INVSCALEFACTOR = (1f / 6000f);

            // AtmosphereFromGround we're modifying
            public AtmosphereFromGround afg;
            public CelestialBody body;

            // DEBUG_alwaysUpdateAll
            [ParserTarget("DEBUG_alwaysUpdateAll", optional = true)]
            public NumericParser<bool> DEBUG_alwaysUpdateAll
            {
                get { return afg.DEBUG_alwaysUpdateAll; }
                set { afg.DEBUG_alwaysUpdateAll = value; }
            }

            // doScale
            [ParserTarget("doScale", optional = true)]
            public NumericParser<bool> doScale
            {
                get { return afg.doScale; }
                set { afg.doScale = value; }
            }

            // ESun
            [ParserTarget("ESun", optional = true)]
            public NumericParser<float> ESun
            {
                get { return afg.ESun; }
                set { afg.ESun = value; }
            }

            // g
            [ParserTarget("g", optional = true)]
            public NumericParser<float> g
            {
                get { return afg.g; }
                set { afg.g = value; }
            }

            // innerRadius
            [ParserTarget("innerRadius", optional = true)]
            public NumericParser<float> innerRadius
            {
                get { return afg.innerRadius / INVSCALEFACTOR; }
                set { afg.innerRadius = value * INVSCALEFACTOR; }
            }

            // invWaveLength
            [ParserTarget("invWaveLength", optional = true)]
            public ColorParser invWaveLength
            {
                get { return afg.invWaveLength; }
                set
                {
                    afg.invWaveLength = value;
                    afg.waveLength = new Color((float)Math.Sqrt(Math.Sqrt(1d / afg.invWaveLength[0])), (float)Math.Sqrt(Math.Sqrt(1d / afg.invWaveLength[1])), (float)Math.Sqrt(Math.Sqrt(1d / afg.invWaveLength[2])), 0.5f);
                }
            }

            // Km
            [ParserTarget("Km", optional = true)]
            public NumericParser<float> Km
            {
                get { return afg.Km; }
                set { afg.Km = value; }
            }

            // Kr
            [ParserTarget("Kr", optional = true)]
            public NumericParser<float> Kr
            {
                get { return afg.Kr; }
                set { afg.Kr = value; }
            }

            // outerRadius
            [ParserTarget("outerRadius", optional = true)]
            public NumericParser<float> outerRadius
            {
                get { return afg.outerRadius / INVSCALEFACTOR; }
                set { afg.outerRadius = value * INVSCALEFACTOR; }
            }

            // samples
            [ParserTarget("samples", optional = true)]
            public NumericParser<float> samples
            {
                get { return afg.samples; }
                set { afg.samples = value; }
            }

            // scale
            [ParserTarget("scale", optional = true)]
            public NumericParser<float> scale
            {
                get { return afg.scale; }
                set { afg.scale = value; }
            }

            // scaleDepth
            [ParserTarget("scaleDepth", optional = true)]
            public NumericParser<float> scaleDepth
            {
                get { return afg.scaleDepth; }
                set { afg.scaleDepth = value; }
            }

            [ParserTarget("transformScale", optional = true)]
            public Vector3Parser transformScale
            {
                get { return afg.doScale ? Vector3.zero : afg.transform.localScale; }
                set { afg.transform.localScale = value; afg.doScale = false; }
            }

            // waveLength
            [ParserTarget("waveLength", optional = true)]
            public ColorParser waveLength
            {
                get { return afg.waveLength; }
                set
                {
                    afg.waveLength = value;
                    afg.invWaveLength = new Color((float)(1d / Math.Pow(afg.waveLength[0], 4)), (float)(1d / Math.Pow(afg.waveLength[1], 4)), (float)(1d / Math.Pow(afg.waveLength[2], 4)), 0.5f);
                }
            }

            // outerRadiusMult
            [ParserTarget("outerRadiusMult", optional = true)]
            public NumericParser<float> outerRadiusMult
            {
                get { return (afg.outerRadius / INVSCALEFACTOR) / (float)body.Radius; }
                set { afg.outerRadius = (((float)body.Radius) * value) * INVSCALEFACTOR; }
            }

            // innerRadiusMult
            [ParserTarget("innerRadiusMult", optional = true)]
            public NumericParser<float> innerRadiusMult
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
            void IParserEventSubscriber.Apply(ConfigNode node) { }

            // Parser post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                CalculatedMembers(afg); // with the new values.
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
                afg.outerRadius = (((float)body.Radius) * 1.025f) * INVSCALEFACTOR;
                afg.innerRadius = afg.outerRadius * 0.975f;
                afg.scaleDepth = -0.25f;
                afg.invWaveLength = new Color((float)(1d / Math.Pow(afg.waveLength[0], 4)), (float)(1d / Math.Pow(afg.waveLength[1], 4)), (float)(1d / Math.Pow(afg.waveLength[2], 4)), 0.5f);

                CalculatedMembers(afg);
            }

            // Runtime constructor
            public AtmosphereFromGroundLoader(CelestialBody body) : this()
            {
                this.body = body;
                afg = body.scaledBody.GetComponentInChildren<AtmosphereFromGround>();
            }

        }
    }
}