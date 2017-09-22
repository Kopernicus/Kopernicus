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
using System.Linq;

namespace Kopernicus
{
    namespace Configuration
    {
        // See: http://en.wikipedia.org/wiki/Argument_of_periapsis#mediaviewer/File:Orbit1.svg
        [RequireConfigType(ConfigType.Node)]
        public class OrbitLoader : BaseLoader, IParserEventSubscriber
        {
            /// <summary>
            /// KSP Orbit we're editing
            /// </summary>
            public Orbit orbit { get; set; }

            /// <summary>
            /// CelestialBody we're editing
            /// </summary>
            public CelestialBody celestialBody { get; set; }

            // Reference body to orbit
            [ParserTarget("referenceBody")]
            public String referenceBody { get; set; }

            // How inclined is the orbit
            [ParserTarget("inclination")]
            public NumericParser<Double> inclination 
            {
                get { return orbit.inclination; }
                set { orbit.inclination = value; }
            }
            
            // How excentric is the orbit
            [ParserTarget("eccentricity")]
            public NumericParser<Double> eccentricity
            {
                get { return orbit.eccentricity; }
                set { orbit.eccentricity = value; }
            }

            // Highest point of the orbit
            [ParserTarget("semiMajorAxis")]
            public NumericParser<Double> semiMajorAxis
            {
                get { return orbit.semiMajorAxis; }
                set { orbit.semiMajorAxis = value; }
            }

            // Position of the highest point on the orbit circle
            [ParserTarget("longitudeOfAscendingNode")]
            public NumericParser<Double> longitudeOfAscendingNode
            {
                get { return orbit.LAN; }
                set { orbit.LAN = value; }
            }

            // argumentOfPeriapsis
            [ParserTarget("argumentOfPeriapsis")]
            public NumericParser<Double> argumentOfPeriapsis
            {
                get { return orbit.argumentOfPeriapsis; }
                set { orbit.argumentOfPeriapsis = value; }
            }

            // meanAnomalyAtEpoch
            [ParserTarget("meanAnomalyAtEpoch")]
            public NumericParser<Double> meanAnomalyAtEpoch
            {
                get { return orbit.meanAnomalyAtEpoch; }
                set { orbit.meanAnomalyAtEpoch = value; }
            }

            // meanAnomalyAtEpochD
            [ParserTarget("meanAnomalyAtEpochD")]
            public NumericParser<Double> meanAnomalyAtEpochD
            {
                get { return orbit.meanAnomalyAtEpoch / Math.PI * 180d; }
                set { orbit.meanAnomalyAtEpoch = value.value * Math.PI / 180d; }
            }

            // epoch
            [ParserTarget("epoch")]
            public NumericParser<Double> epoch
            {
                get { return orbit.epoch; }
                set { orbit.epoch = value; }
            }
            
            // Orbit renderer color
            [ParserTarget("color")]
            public ColorParser color
            {
                get { return generatedBody != null ? generatedBody.orbitRenderer.nodeColor : (celestialBody.orbitDriver.orbitColor * 2).A(celestialBody.orbitDriver.orbitColor.a); }
                set { generatedBody.orbitRenderer.SetColor(value); }
            }

            // Orbit Icon color
            [ParserTarget("iconColor")]
            public ColorParser iconColor
            {
                // get { return generatedBody.orbitRenderer.nodeColor; }
                set { generatedBody.orbitRenderer.nodeColor = value.value; }
            }

            // Orbit Draw Mode
            [ParserTarget("mode")]
            public EnumParser<OrbitRenderer.DrawMode> mode
            {
                get { return celestialBody?.orbitDriver?.Renderer?.drawMode; }
                set { generatedBody.Set("drawMode", value.value); }
            }

            // Orbit Icon Mode
            [ParserTarget("icon")]
            public EnumParser<OrbitRenderer.DrawIcons> icon
            {
                get { return celestialBody?.orbitDriver?.Renderer?.drawIcons; }
                set { generatedBody.Set("drawIcons", value.value); }
            }

            // Orbit rendering bounds
            [ParserTarget("cameraSmaRatioBounds")]
            public NumericCollectionParser<Single> cameraSmaRatioBounds = new NumericCollectionParser<Single>(new Single[] { 0.3f, 25f });

            // Parser apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                // Event
                Events.OnOrbitLoaderApply.Fire(this, node);
            }

            // Parser post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                if (epoch != null)
                    orbit.epoch += Templates.epoch;

                if (generatedBody != null)
                {
                    generatedBody.orbitRenderer.lowerCamVsSmaRatio = cameraSmaRatioBounds.value[0];
                    generatedBody.orbitRenderer.upperCamVsSmaRatio = cameraSmaRatioBounds.value[1];
                }
                Events.OnOrbitLoaderPostApply.Fire(this, node);
            }

            /// <summary>
            /// Creates a new Orbit Loader from the Injector context.
            /// </summary>
            public OrbitLoader()
            {
                // Is this the parser context?
                if (generatedBody == null)
                    throw new InvalidOperationException("Must be executed in Injector context.");

                // If this body needs orbit controllers, create them
                if (generatedBody.orbitDriver == null)
                {
                    generatedBody.orbitDriver = generatedBody.celestialBody.gameObject.AddComponent<OrbitDriver>();
                    generatedBody.orbitRenderer = generatedBody.celestialBody.gameObject.AddComponent<OrbitRenderer>();
                }
                generatedBody.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;
                cameraSmaRatioBounds = new Single[] { generatedBody.orbitRenderer.lowerCamVsSmaRatio, generatedBody.orbitRenderer.upperCamVsSmaRatio };

                // Store values
                orbit = generatedBody.orbitDriver.orbit ?? new Orbit();
                celestialBody = generatedBody.celestialBody;
            }

            /// <summary>
            /// Creates a new Orbit Loader from a spawned CelestialBody.
            /// </summary>
            public OrbitLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null)
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");

                // Store values
                orbit = body.orbit ?? new Orbit();
                celestialBody = body;
            }

            /// <summary>
            /// Creates a new Orbit Loader from a custom PSystemBody.
            /// </summary>
            public OrbitLoader(PSystemBody body)
            {
                // Set generatedBody
                generatedBody = body ?? throw new InvalidOperationException("The body cannot be null.");

                // If this body needs orbit controllers, create them
                if (generatedBody.orbitDriver == null)
                {
                    generatedBody.orbitDriver = generatedBody.celestialBody.gameObject.AddComponent<OrbitDriver>();
                    generatedBody.orbitRenderer = generatedBody.celestialBody.gameObject.AddComponent<OrbitRenderer>();
                }
                generatedBody.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;
                cameraSmaRatioBounds = new Single[] { generatedBody.orbitRenderer.lowerCamVsSmaRatio, generatedBody.orbitRenderer.upperCamVsSmaRatio };

                // Store values
                orbit = generatedBody.orbitDriver.orbit ?? new Orbit();
                celestialBody = generatedBody.celestialBody;
            }

            // Finalize an Orbit
            public static void FinalizeOrbit(CelestialBody body)
            {
                if (body.orbitDriver != null)
                {
                    if (body.referenceBody != null)
                    {
                        // Only recalculate the SOI, if it's not forced
                        if (!body.Has("hillSphere"))
                            body.hillSphere = body.orbit.semiMajorAxis * (1.0 - body.orbit.eccentricity) * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 1.0 / 3.0);

                        if (!body.Has("sphereOfInfluence"))
                            body.sphereOfInfluence = Math.Max(
                                body.orbit.semiMajorAxis * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 0.4),
                                Math.Max(body.Radius * Templates.SOIMinRadiusMult, body.Radius + Templates.SOIMinAltitude));

                        // this is unlike stock KSP, where only the reference body's mass is used.
                        body.orbit.period = 2 * Math.PI * Math.Sqrt(Math.Pow(body.orbit.semiMajorAxis, 2) / 6.67408e-11 * body.orbit.semiMajorAxis / (body.referenceBody.Mass + body.Mass));
                        body.orbit.meanMotion = 2 * Math.PI / body.orbit.period;    // in theory this should work but I haven't tested it

                        if (body.orbit.eccentricity <= 1.0)
                        {
                            body.orbit.meanAnomaly = body.orbit.meanAnomalyAtEpoch;
                            body.orbit.orbitPercent = body.orbit.meanAnomalyAtEpoch / (Math.PI * 2);
                            body.orbit.ObTAtEpoch = body.orbit.orbitPercent * body.orbit.period;
                        }
                        else
                        {
                            // ignores this body's own mass for this one...
                            body.orbit.meanAnomaly = body.orbit.meanAnomalyAtEpoch;
                            body.orbit.ObT = Math.Pow(Math.Pow(Math.Abs(body.orbit.semiMajorAxis), 3.0) / body.orbit.referenceBody.gravParameter, 0.5) * body.orbit.meanAnomaly;
                            body.orbit.ObTAtEpoch = body.orbit.ObT;
                        }
                    }
                    else
                    {
                        body.sphereOfInfluence = Double.PositiveInfinity;
                        body.hillSphere = Double.PositiveInfinity;
                    }
                }
                try
                {
                    body.CBUpdate();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log("CBUpdate for " + body.name + " failed: " + e.Message);
                }
            }
        }
    }
}

