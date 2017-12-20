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
using System.ComponentModel;
using System.Linq;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class AtmosphereFromGroundLoader : BaseLoader, IParserEventSubscriber
        {
            /// <summary>
            /// The scale factor between ScaledSpace and LocalSpace
            /// </summary>
            public const Single INVSCALEFACTOR = (1f / 6000f);

            /// <summary>
            /// AtmosphereFromGround we're modifying
            /// </summary>
            public AtmosphereFromGround atmosphereFromGround;

            /// <summary>
            /// CelestialBody we're modifying
            /// </summary>
            private CelestialBody celestialBody;

            /// <summary>
            /// ScaledSpace object of the body we're modifying
            /// </summary>
            private GameObject scaledVersion;

            // DEBUG_alwaysUpdateAll
            [ParserTarget("DEBUG_alwaysUpdateAll")]
            [KittopiaDescription("Whether all parameters should get recalculated and reapplied every frame.")]
            public NumericParser<Boolean> DEBUG_alwaysUpdateAll
            {
                get { return atmosphereFromGround.DEBUG_alwaysUpdateAll; }
                set { atmosphereFromGround.DEBUG_alwaysUpdateAll = value; }
            }

            // doScale
            [ParserTarget("doScale")]
            [KittopiaDescription("Whether the atmosphere mesh should be scaled automatically.")]
            public NumericParser<Boolean> doScale
            {
                get { return atmosphereFromGround.doScale; }
                set { atmosphereFromGround.doScale = value; }
            }

            // ESun
            [ParserTarget("ESun")]
            public NumericParser<Single> ESun
            {
                get { return atmosphereFromGround.ESun; }
                set { atmosphereFromGround.ESun = value; }
            }

            // g
            [ParserTarget("g")]
            public NumericParser<Single> g
            {
                get { return atmosphereFromGround.g; }
                set { atmosphereFromGround.g = value; }
            }

            // innerRadius
            [ParserTarget("innerRadius")]
            [KittopiaDescription("The lower bound of the atmosphere effect.")]
            public NumericParser<Single> innerRadius
            {
                get { return atmosphereFromGround.innerRadius / INVSCALEFACTOR; }
                set { atmosphereFromGround.innerRadius = value * INVSCALEFACTOR; }
            }

            // invWaveLength
            [ParserTarget("invWaveLength")]
            public ColorParser invWaveLength
            {
                get { return atmosphereFromGround.invWaveLength; }
                set
                {
                    atmosphereFromGround.invWaveLength = value;
                    atmosphereFromGround.waveLength = new Color((Single)Math.Sqrt(Math.Sqrt(1d / atmosphereFromGround.invWaveLength[0])), (Single)Math.Sqrt(Math.Sqrt(1d / atmosphereFromGround.invWaveLength[1])), (Single)Math.Sqrt(Math.Sqrt(1d / atmosphereFromGround.invWaveLength[2])), 0.5f);
                }
            }

            // Km
            [ParserTarget("Km")]
            public NumericParser<Single> Km
            {
                get { return atmosphereFromGround.Km; }
                set { atmosphereFromGround.Km = value; }
            }

            // Kr
            [ParserTarget("Kr")]
            public NumericParser<Single> Kr
            {
                get { return atmosphereFromGround.Kr; }
                set { atmosphereFromGround.Kr = value; }
            }

            // outerRadius
            [ParserTarget("outerRadius")]
            [KittopiaDescription("The upper bound of the atmosphere effect.")]
            public NumericParser<Single> outerRadius
            {
                get { return atmosphereFromGround.outerRadius / INVSCALEFACTOR; }
                set { atmosphereFromGround.outerRadius = value * INVSCALEFACTOR; }
            }

            // samples
            [ParserTarget("samples")]
            public NumericParser<Single> samples
            {
                get { return atmosphereFromGround.samples; }
                set { atmosphereFromGround.samples = value; }
            }

            // scale
            [ParserTarget("scale")]
            public NumericParser<Single> scale
            {
                get { return atmosphereFromGround.scale; }
                set { atmosphereFromGround.scale = value; }
            }

            // scaleDepth
            [ParserTarget("scaleDepth")]
            public NumericParser<Single> scaleDepth
            {
                get { return atmosphereFromGround.scaleDepth; }
                set { atmosphereFromGround.scaleDepth = value; }
            }

            [ParserTarget("transformScale")]
            [KittopiaDescription("The scale of the atmosphere mesh in all three directions. Automatically set if doScale is enabled.")]
            public Vector3Parser transformScale
            {
                get { return atmosphereFromGround.doScale ? Vector3.zero : atmosphereFromGround.transform.localScale; }
                set { atmosphereFromGround.transform.localScale = value; atmosphereFromGround.doScale = false; }
            }

            // waveLength
            [ParserTarget("waveLength")]
            public ColorParser waveLength
            {
                get { return atmosphereFromGround.waveLength; }
                set
                {
                    atmosphereFromGround.waveLength = value;
                    atmosphereFromGround.invWaveLength = new Color((Single)(1d / Math.Pow(atmosphereFromGround.waveLength[0], 4)), (Single)(1d / Math.Pow(atmosphereFromGround.waveLength[1], 4)), (Single)(1d / Math.Pow(atmosphereFromGround.waveLength[2], 4)), 0.5f);
                }
            }

            // outerRadiusMult
            [ParserTarget("outerRadiusMult")]
            [KittopiaDescription("A multiplier that automatically sets outerRadius based on the planets radius.")]
            public NumericParser<Single> outerRadiusMult
            {
                get { return (atmosphereFromGround.outerRadius / INVSCALEFACTOR) / (Single)atmosphereFromGround.planet.Radius; }
                set { atmosphereFromGround.outerRadius = (((Single)atmosphereFromGround.planet.Radius) * value) * INVSCALEFACTOR; }
            }

            // innerRadiusMult
            [ParserTarget("innerRadiusMult")]
            [KittopiaDescription("A multiplier that automatically sets innerRadius based on the planets radius.")]
            public NumericParser<Single> innerRadiusMult
            {
                get { return atmosphereFromGround.innerRadius / atmosphereFromGround.outerRadius; }
                set { atmosphereFromGround.innerRadius = atmosphereFromGround.outerRadius * value; }
            }

            /// <summary>
            /// Set default values for the AtmosphereFromGround
            /// </summary>
            [KittopiaAction("Set Default Values")]
            [KittopiaDescription("Sets stock values for the AtmosphereFromGround")]
            public void SetDefaultValues()
            {
                // Set Defaults
                atmosphereFromGround.planet = celestialBody;
                atmosphereFromGround.ESun = 30f;
                atmosphereFromGround.Kr = 0.00125f;
                atmosphereFromGround.Km = 0.00015f;
                atmosphereFromGround.samples = 4f;
                atmosphereFromGround.g = -0.85f;
                if (atmosphereFromGround.waveLength == new Color(0f, 0f, 0f, 0f))
                {
                    atmosphereFromGround.waveLength = new Color(0.65f, 0.57f, 0.475f, 0.5f);
                }
                atmosphereFromGround.outerRadius = (((Single)celestialBody.Radius) * 1.025f) * INVSCALEFACTOR;
                atmosphereFromGround.innerRadius = atmosphereFromGround.outerRadius * 0.975f;
                atmosphereFromGround.scaleDepth = -0.25f;
                atmosphereFromGround.invWaveLength = new Color((Single)(1d / Math.Pow(atmosphereFromGround.waveLength[0], 4)), (Single)(1d / Math.Pow(atmosphereFromGround.waveLength[1], 4)), (Single)(1d / Math.Pow(atmosphereFromGround.waveLength[2], 4)), 0.5f);
            }

            /// <summary>
            /// Calculates the default members for the AFG
            /// </summary>
            [KittopiaAction("Calculate Members")]
            [KittopiaDescription("Calculates values that are based on changeable values.")]
            public void CalculateMembers()
            {
                CalculatedMembers(atmosphereFromGround);
            }

            /// <summary>
            /// Stores the members for the AFG
            /// </summary>
            [KittopiaAction("Store AFG")]
            [KittopiaDescription("Stores the parameters of the AtmosphereFromGround so they are applied on top of the default ones.")]
            public void Store()
            {
                CalculateMembers();
                AFGInfo.StoreAFG(atmosphereFromGround);
            }

            /// <summary>
            /// Calculates the default members for the AFG
            /// </summary>
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
                // Set defaults
                SetDefaultValues();
                
                // Calculate the remaining values
                CalculateMembers();

                // Fire event
                Events.OnAFGLoaderApply.Fire(this, node);
            }

            // Parser post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                // Recalculate with the new values and store
                Store();

                // Fire event
                Events.OnAFGLoaderPostApply.Fire(this, node);
            }

            /// <summary>
            /// Creates a new AtmosphereFromGround Loader from the Injector context.
            /// </summary>
            public AtmosphereFromGroundLoader()
            {
                // Is this the parser context?
                if (generatedBody == null)
                    throw new InvalidOperationException("Must be executed in Injector context.");

                // Store values
                celestialBody = generatedBody.celestialBody;
                atmosphereFromGround = generatedBody.scaledVersion.GetComponentsInChildren<AtmosphereFromGround>(true)?.FirstOrDefault();
                scaledVersion = generatedBody.scaledVersion;

                if (atmosphereFromGround == null)
                {
                    // Add the material light direction behavior
                    MaterialSetDirection materialLightDirection = scaledVersion.AddComponent<MaterialSetDirection>();
                    materialLightDirection.valueName = "_localLightDirection";

                    // Create the atmosphere shell game object
                    GameObject scaledAtmosphere = new GameObject("Atmosphere");
                    scaledAtmosphere.transform.parent = scaledVersion.transform;
                    scaledAtmosphere.layer = Constants.GameLayers.ScaledSpaceAtmosphere;
                    MeshRenderer renderer = scaledAtmosphere.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = new MaterialWrapper.AtmosphereFromGround();
                    MeshFilter meshFilter = scaledAtmosphere.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = Templates.ReferenceGeosphere;
                    atmosphereFromGround = scaledAtmosphere.AddComponent<AtmosphereFromGround>();
                }
            }

            /// <summary>
            /// Creates a new AtmosphereFromGround Loader from a spawned CelestialBody.
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.CelestialBody)]
            public AtmosphereFromGroundLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null)
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");

                // Store values
                celestialBody = body;
                atmosphereFromGround = body.afg;
                scaledVersion = body.scaledBody;

                if (atmosphereFromGround == null)
                {
                    // Add the material light direction behavior
                    MaterialSetDirection materialLightDirection = scaledVersion.AddComponent<MaterialSetDirection>();
                    materialLightDirection.valueName = "_localLightDirection";

                    // Create the atmosphere shell game object
                    GameObject scaledAtmosphere = new GameObject("Atmosphere");
                    scaledAtmosphere.transform.parent = scaledVersion.transform;
                    scaledAtmosphere.layer = Constants.GameLayers.ScaledSpaceAtmosphere;
                    MeshRenderer renderer = scaledAtmosphere.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = new MaterialWrapper.AtmosphereFromGround();
                    MeshFilter meshFilter = scaledAtmosphere.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = Templates.ReferenceGeosphere;
                    atmosphereFromGround = scaledAtmosphere.AddComponent<AtmosphereFromGround>();
                }
            }
        }
    }
}