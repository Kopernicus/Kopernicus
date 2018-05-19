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
using System.Collections.Generic;
using Kopernicus.Components;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        // See: http://en.wikipedia.org/wiki/Argument_of_periapsis#mediaviewer/File:Orbit1.svg
        [RequireConfigType(ConfigType.Node)]
        public class OrbitLoader : BaseLoader, IParserEventSubscriber, ITypeParser<CelestialBody>
        {
            /// <summary>
            /// CelestialBody we're editing
            /// </summary>
            public CelestialBody Value { get; set; }

            // Reference body to orbit
            [ParserTarget("referenceBody")]
            [KittopiaHideOption]
            [KittopiaDescription("The body that this body is orbiting around.")]
            public String referenceBody
            {
                get
                {
                    if (Injector.IsInPrefab)
                    {
                        return _referenceBody;
                    }
                    return Value.orbitDriver.referenceBody.transform.name;
                }
                set
                {
                    if (Injector.IsInPrefab)
                    {
                        _referenceBody = value;
                        return;
                    }
                    Value.orbitDriver.referenceBody = Value.orbit.referenceBody =
                        PSystemManager.Instance.localBodies.Find(b => b.transform.name == value);
                }
            }

            private String _referenceBody;

            // How inclined is the orbit
            [ParserTarget("inclination")]
            public NumericParser<Double> inclination 
            {
                get { return Value.orbit.inclination; }
                set { Value.orbit.inclination = value; }
            }
            
            // How excentric is the orbit
            [ParserTarget("eccentricity")]
            public NumericParser<Double> eccentricity
            {
                get { return Value.orbit.eccentricity; }
                set { Value.orbit.eccentricity = value; }
            }

            // Highest point of the orbit
            [ParserTarget("semiMajorAxis")]
            [KittopiaDescription("The altitude of the highest point in the orbit")]
            public NumericParser<Double> semiMajorAxis
            {
                get { return Value.orbit.semiMajorAxis; }
                set { Value.orbit.semiMajorAxis = value; }
            }

            // Position of the highest point on the orbit circle
            [ParserTarget("longitudeOfAscendingNode")]
            [KittopiaDescription("The position of the highest point on the orbit circle")]
            public NumericParser<Double> longitudeOfAscendingNode
            {
                get { return Value.orbit.LAN; }
                set { Value.orbit.LAN = value; }
            }

            // argumentOfPeriapsis
            [ParserTarget("argumentOfPeriapsis")]
            public NumericParser<Double> argumentOfPeriapsis
            {
                get { return Value.orbit.argumentOfPeriapsis; }
                set { Value.orbit.argumentOfPeriapsis = value; }
            }

            // meanAnomalyAtEpoch
            [ParserTarget("meanAnomalyAtEpoch")]
            public NumericParser<Double> meanAnomalyAtEpoch
            {
                get { return Value.orbit.meanAnomalyAtEpoch; }
                set { Value.orbit.meanAnomalyAtEpoch = value; }
            }

            // meanAnomalyAtEpochD
            [ParserTarget("meanAnomalyAtEpochD")]
            public NumericParser<Double> meanAnomalyAtEpochD
            {
                get { return Value.orbit.meanAnomalyAtEpoch / Math.PI * 180d; }
                set { Value.orbit.meanAnomalyAtEpoch = value.Value * Math.PI / 180d; }
            }

            // epoch
            [ParserTarget("epoch")]
            public NumericParser<Double> epoch
            {
                get { return Value.orbit.epoch; }
                set { Value.orbit.epoch = value; }
            }
            
            // Orbit renderer color
            [ParserTarget("color")]
            [KittopiaDescription("The color of the orbit line in the Tracking Station")]
            public ColorParser color
            {
                get
                {
                    if (Injector.IsInPrefab)
                    {
                        return generatedBody.orbitRenderer.nodeColor;
                    }
                    KopernicusOrbitRendererData data =
                        (KopernicusOrbitRendererData) PSystemManager.OrbitRendererDataCache[Value];
                    return data.orbitColor;
                }
                set
                {
                    if (Injector.IsInPrefab)
                    {
                        generatedBody.orbitRenderer.SetColor(value);
                    }
                    else
                    {
                        KopernicusOrbitRendererData data =
                            (KopernicusOrbitRendererData) PSystemManager.OrbitRendererDataCache[Value];
                        data.nodeColor = value;
                        data.orbitColor = (value.Value * 0.5f).A(data.nodeColor.a);
                        PSystemManager.OrbitRendererDataCache[Value] = data;
                    }
                }
            }

            // Orbit Icon color
            [ParserTarget("nodeColor")]
            [ParserTarget("iconColor")]
            [KittopiaDescription("The color of the circle that marks the planets current position on the orbit")]
            public ColorParser iconColor
            {
                get
                {
                    if (Injector.IsInPrefab)
                    {
                        return generatedBody.orbitRenderer.nodeColor;
                    }
                    KopernicusOrbitRendererData data =
                        (KopernicusOrbitRendererData) PSystemManager.OrbitRendererDataCache[Value];
                    return data.nodeColor;
                }
                set
                {
                    if (Injector.IsInPrefab)
                    {
                        generatedBody.orbitRenderer.nodeColor = value;
                    }
                    else
                    {
                        KopernicusOrbitRendererData data =
                            (KopernicusOrbitRendererData) PSystemManager.OrbitRendererDataCache[Value];
                        data.nodeColor = value;
                        PSystemManager.OrbitRendererDataCache[Value] = data;
                    }
                }
            }

            // Orbit Draw Mode
            [ParserTarget("mode")]
            #if !KSP131
            public EnumParser<OrbitRendererBase.DrawMode> mode
            #else
            public EnumParser<OrbitRenderer.DrawMode> mode
            #endif
            {
                #if !KSP131
                get { return Value.Get("drawMode", OrbitRendererBase.DrawMode.REDRAW_AND_RECALCULATE); }
                #else
                get { return Value.Get("drawMode", OrbitRenderer.DrawMode.REDRAW_AND_RECALCULATE); }
                #endif
                set { Value.Set("drawMode", value.Value); }
            }

            // Orbit Icon Mode
            [ParserTarget("icon")]
            #if !KSP131
            public EnumParser<OrbitRendererBase.DrawIcons> icon
            #else
            public EnumParser<OrbitRenderer.DrawIcons> icon
            #endif
            {
                #if !KSP131
                get { return Value.Get("drawIcons", OrbitRendererBase.DrawIcons.ALL); }
                #else
                get { return Value.Get("drawIcons", OrbitRenderer.DrawIcons.ALL); }
                #endif
                set { Value.Set("drawIcons", value.Value); }
            }

            // Orbit rendering bounds
            [ParserTarget("cameraSmaRatioBounds")]
            public NumericCollectionParser<Single> cameraSmaRatioBounds
            {
                get
                {
                    if (Injector.IsInPrefab)
                    {
                        return new List<Single> { generatedBody.orbitRenderer.lowerCamVsSmaRatio,
                            generatedBody.orbitRenderer.upperCamVsSmaRatio };
                    }
                    OrbitRendererData data = PSystemManager.OrbitRendererDataCache[Value];
                    return new List<Single> { data.lowerCamVsSmaRatio, data.upperCamVsSmaRatio };
                }
                set
                {
                    if (Injector.IsInPrefab)
                    {
                        generatedBody.orbitRenderer.lowerCamVsSmaRatio = value.Value[0];
                        generatedBody.orbitRenderer.upperCamVsSmaRatio = value.Value[1];
                    }
                    else
                    {
                        OrbitRendererData data = PSystemManager.OrbitRendererDataCache[Value];
                        data.lowerCamVsSmaRatio = value.Value[0];
                        data.upperCamVsSmaRatio = value.Value[1];
                        PSystemManager.OrbitRendererDataCache[Value] = data;
                    }
                }
            }

            /// <summary>
            /// Recalculates some of the orbital parameters to be more realistic
            /// </summary>
            [KittopiaAction("Finalize Orbit")]
            [KittopiaDescription("Recalculates some of the orbital parameters to be more realistic")]
            public void FinalizeOrbit()
            {
                FinalizeOrbit(Value);
            }

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
                    Value.orbit.epoch += Templates.epoch;
                
                Events.OnOrbitLoaderPostApply.Fire(this, node);
            }

            /// <summary>
            /// Creates a new Orbit Loader from the Injector context.
            /// </summary>
            public OrbitLoader()
            {
                // Is this the parser context?
                if (!Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("Must be executed in Injector context.");
                }

                // If this body needs orbit controllers, create them
                if (generatedBody.orbitDriver == null)
                {
                    generatedBody.orbitDriver = generatedBody.celestialBody.gameObject.AddComponent<OrbitDriver>();
                    generatedBody.orbitRenderer = generatedBody.celestialBody.gameObject.AddComponent<OrbitRenderer>();
                }
                generatedBody.celestialBody.gameObject.AddOrGetComponent<OrbitRendererUpdater>();
                generatedBody.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;

                // Store values
                Value = generatedBody.celestialBody;
                Value.orbitDriver = generatedBody.orbitDriver;
                Value.orbitDriver.orbit = generatedBody.orbitDriver.orbit ?? new Orbit();
            }

            /// <summary>
            /// Creates a new Orbit Loader from a spawned CelestialBody.
            /// </summary>
            public OrbitLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }

                // Does this body have an orbit?
                if (body.orbitDriver == null)
                {
                    throw new InvalidOperationException("The body must have an Orbit to be editabled.");
                }

                // Add the rendering updater to the celestial body
                body.gameObject.AddOrGetComponent<OrbitRendererUpdater>();
                
                // Update the OrbitRenderer data
                KopernicusOrbitRendererData data = body.orbitDriver.Renderer == null
                    ? new KopernicusOrbitRendererData(body, PSystemManager.OrbitRendererDataCache[body])
                    : new KopernicusOrbitRendererData(body, body.orbitDriver.Renderer);
                if (PSystemManager.OrbitRendererDataCache.ContainsKey(body))
                {
                    PSystemManager.OrbitRendererDataCache[body] = data;
                }
                else
                {
                    PSystemManager.OrbitRendererDataCache.Add(body, data);
                }

                // Store values
                Value = body;
                if (Value.orbitDriver.orbit == null)
                {
                    Value.orbitDriver.orbit = new Orbit();
                }
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

