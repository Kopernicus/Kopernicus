/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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
            // KSP orbit objects we are editing
            public Orbit orbit { get; set; }
            public CelestialBody body { get; set; }

            // Reference body to orbit
            [ParserTarget("referenceBody")]
            public string referenceBody { get; set; }

            // How inclined is the orbit
            [ParserTarget("inclination")]
            public NumericParser<double> inclination 
            {
                get { return orbit.inclination; }
                set { orbit.inclination = value; }
            }
            
            // How excentric is the orbit
            [ParserTarget("eccentricity")]
            public NumericParser<double> eccentricity
            {
                get { return orbit.eccentricity; }
                set { orbit.eccentricity = value; }
            }

            // Highest point of the orbit
            [ParserTarget("semiMajorAxis")]
            public NumericParser<double> semiMajorAxis
            {
                get { return orbit.semiMajorAxis; }
                set { orbit.semiMajorAxis = value; }
            }

            // Position of the highest point on the orbit circle
            [ParserTarget("longitudeOfAscendingNode")]
            public NumericParser<double> longitudeOfAscendingNode
            {
                get { return orbit.LAN; }
                set { orbit.LAN = value; }
            }

            // argumentOfPeriapsis
            [ParserTarget("argumentOfPeriapsis")]
            public NumericParser<double> argumentOfPeriapsis
            {
                get { return orbit.argumentOfPeriapsis; }
                set { orbit.argumentOfPeriapsis = value; }
            }

            // meanAnomalyAtEpoch
            [ParserTarget("meanAnomalyAtEpoch")]
            public NumericParser<double> meanAnomalyAtEpoch
            {
                get { return orbit.meanAnomalyAtEpoch; }
                set { orbit.meanAnomalyAtEpoch = value; }
            }

            // meanAnomalyAtEpochD
            [ParserTarget("meanAnomalyAtEpochD")]
            public NumericParser<double> meanAnomalyAtEpochD
            {
                get { return orbit.meanAnomalyAtEpoch / Math.PI * 180d; }
                set { orbit.meanAnomalyAtEpoch = value.value * Math.PI / 180d; }
            }

            // epoch
            [ParserTarget("epoch")]
            public NumericParser<double> epoch
            {
                get { return orbit.epoch; }
                set { orbit.epoch = value; }
            }
            
            // Orbit renderer color
            [ParserTarget("color")]
            public ColorParser color
            {
                get { return generatedBody != null ? generatedBody.orbitRenderer.nodeColor : (body.orbitDriver.orbitColor * 2).A(body.orbitDriver.orbitColor.a); }
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
                get { return body?.orbitDriver?.Renderer?.drawMode; }
                set { generatedBody.Set("drawMode", value.value); }
            }

            // Orbit Icon Mode
            [ParserTarget("icon")]
            public EnumParser<OrbitRenderer.DrawIcons> icon
            {
                get { return body?.orbitDriver?.Renderer?.drawIcons; }
                set { generatedBody.Set("drawIcons", value.value); }
            }

            // Orbit rendering bounds
            [ParserTarget("cameraSmaRatioBounds")]
            public NumericCollectionParser<float> cameraSmaRatioBounds = new NumericCollectionParser<float>(new float[] { 0.3f, 25f });

            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                if (generatedBody == null) return;

                // If this body needs orbit controllers, create them
                if (generatedBody.orbitDriver == null)
                {
                    generatedBody.orbitDriver = generatedBody.celestialBody.gameObject.AddComponent<OrbitDriver>();
                    generatedBody.orbitRenderer = generatedBody.celestialBody.gameObject.AddComponent<OrbitRenderer>();
                }

                // Setup orbit
                generatedBody.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;
                orbit = generatedBody.orbitDriver.orbit;
                referenceBody = orbit?.referenceBody?.name;
                float[] bounds = new float[] { generatedBody.orbitRenderer.lowerCamVsSmaRatio, generatedBody.orbitRenderer.upperCamVsSmaRatio };
                cameraSmaRatioBounds = bounds;

                // Remove null
                if (orbit == null) orbit = new Orbit();
            }

            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                if (generatedBody == null) return;
                if (epoch != null)
                    orbit.epoch += Templates.epoch;
                generatedBody.orbitDriver.orbit = orbit;
                generatedBody.orbitRenderer.lowerCamVsSmaRatio = cameraSmaRatioBounds.value[0];
                generatedBody.orbitRenderer.upperCamVsSmaRatio = cameraSmaRatioBounds.value[1];
            }

            // Construct an empty orbit
            public OrbitLoader() { }

            // Copy orbit provided
            public OrbitLoader(CelestialBody body)
            {
                this.body = body;
                orbit = body.orbitDriver.orbit;
                referenceBody = body.orbit.referenceBody.name;
                float[] bounds = new float[] { body.orbitDriver.lowerCamVsSmaRatio, body.orbitDriver.upperCamVsSmaRatio };
                cameraSmaRatioBounds.value = bounds.ToList();
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
                        body.orbit.period = 2 * Math.PI * Math.Sqrt(Math.Pow(body.orbit.semiMajorAxis, 2) / 6.674E-11 * body.orbit.semiMajorAxis / (body.referenceBody.Mass + body.Mass));
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

