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

using Kopernicus.Components;
using System;
using System.Linq;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class SpaceCenterLoader : BaseLoader, IParserEventSubscriber, ITypeParser<KSC>
        {
            // The KSC Object we're editing
            public KSC Value { get; set; }

            // latitude
            [ParserTarget("latitude")]
            [KittopiaDescription("The latitude of the KSC buildings.")]
            public NumericParser<Double> latitude
            {
                get { return Value.latitude; }
                set
                {
                    Value.latitude = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // longitude
            [ParserTarget("longitude")]
            [KittopiaDescription("The longitude of the KSC buildings.")]
            public NumericParser<Double> longitude
            {
                get { return Value.longitude; }
                set
                {
                    Value.longitude = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            [ParserTarget("repositionRadial")]
            public Vector3Parser repositionRadial
            {
                get { return Value.repositionRadial; }
                set
                {
                    Value.repositionRadial = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // decalLatitude
            [ParserTarget("decalLatitude")]
            [KittopiaDescription("The latitude of the center of the flat area around the KSC.")]
            public NumericParser<Double> decalLatitude
            {
                get { return Value.decalLatitude; }
                set
                {
                    Value.decalLatitude = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // decalLongitude
            [ParserTarget("decalLongitude")]
            [KittopiaDescription("The longitude of the center of the flat area around the KSC.")]
            public NumericParser<Double> decalLongitude
            {
                get { return Value.decalLongitude; }
                set
                {
                    Value.decalLongitude = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // lodvisibleRangeMultipler
            [ParserTarget("lodvisibleRangeMultipler")]
            public NumericParser<Double> lodvisibleRangeMultipler
            {
                get { return Value.lodvisibleRangeMult; }
                set
                {
                    Value.lodvisibleRangeMult = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // reorientFinalAngle
            [ParserTarget("reorientFinalAngle")]
            public NumericParser<Single> reorientFinalAngle
            {
                get { return Value.reorientFinalAngle; }
                set
                {
                    Value.reorientFinalAngle = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // reorientInitialUp
            [ParserTarget("reorientInitialUp")]
            public Vector3Parser reorientInitialUp
            {
                get { return Value.reorientInitialUp; }
                set
                {
                    Value.reorientInitialUp = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // reorientToSphere
            [ParserTarget("reorientToSphere")]
            public NumericParser<Boolean> reorientToSphere
            {
                get { return Value.reorientToSphere; }
                set
                {
                    Value.reorientToSphere = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // repositionRadiusOffset
            [ParserTarget("repositionRadiusOffset")]
            public NumericParser<Double> repositionRadiusOffset
            {
                get { return Value.repositionRadiusOffset; }
                set
                {
                    Value.repositionRadiusOffset = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // repositionToSphere
            [ParserTarget("repositionToSphere")]
            public NumericParser<Boolean> repositionToSphere
            {
                get { return Value.repositionToSphere; }
                set
                {
                    Value.repositionToSphere = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // repositionToSphereSurface
            [ParserTarget("repositionToSphereSurface")]
            public NumericParser<Boolean> repositionToSphereSurface
            {
                get { return Value.repositionToSphereSurface; }
                set
                {
                    Value.repositionToSphereSurface = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // repositionToSphereSurfaceAddHeight
            [ParserTarget("repositionToSphereSurfaceAddHeight")]
            public NumericParser<Boolean> repositionToSphereSurfaceAddHeight
            {
                get { return Value.repositionToSphereSurfaceAddHeight; }
                set
                {
                    Value.repositionToSphereSurfaceAddHeight = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // position
            [ParserTarget("position")]
            [KittopiaDescription("The position of the KSC buildings represented as a Vector.")]
            public Vector3Parser position
            {
                get { return Value.position; }
                set
                {
                    Value.position = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // radius
            [ParserTarget("radius")]
            [KittopiaDescription("The altitude of the KSC.")]
            public NumericParser<Double> radius
            {
                get { return Value.radius; }
                set
                {
                    Value.radius = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // heightMapDeformity
            [ParserTarget("heightMapDeformity")]
            public NumericParser<Double> heightMapDeformity
            {
                get { return Value.heightMapDeformity; }
                set
                {
                    Value.heightMapDeformity = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // absoluteOffset
            [ParserTarget("absoluteOffset")]
            public NumericParser<Double> absoluteOffset
            {
                get { return Value.absoluteOffset; }
                set
                {
                    Value.absoluteOffset = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // absolute
            [ParserTarget("absolute")]
            public NumericParser<Boolean> absolute
            {
                get { return Value.absolute; }
                set
                {
                    Value.absolute = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // groundColor
            [ParserTarget("groundColor")]
            [KittopiaDescription("The color of the grass at the KSC.")]
            public ColorParser groundColorParser
            {
                get { return Value.color; }
                set
                {
                    Value.color = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // Texture
            [ParserTarget("groundTexture")]
            [KittopiaDescription("The surface texture of the grass spots at the KSC.")]
            public Texture2DParser groundTextureParser
            {
                get { return Value.mainTexture; }
                set
                {
                    Value.mainTexture = value;
                    if (!Injector.IsInPrefab)
                    {
                        Value.Start();
                    }
                }
            }

            // Apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                Events.OnSpaceCenterLoaderApply.Fire(this, node);
            }

            // Post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                Events.OnSpaceCenterLoaderPostApply.Fire(this, node);
            }

            /// <summary>
            /// Creates a new SpaceCenter Loader from the Injector context.
            /// </summary>
            public SpaceCenterLoader()
            {
                // Is this the parser context?
                if (!Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("Must be executed in Injector context.");
                }

                // Store values
                Value = generatedBody.celestialBody.gameObject.AddComponent<KSC>();
                UnityEngine.Object.DontDestroyOnLoad(Value);
            }

            /// <summary>
            /// Creates a new SpaceCenter Loader from a spawned CelestialBody.
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.CelestialBody)]
            public SpaceCenterLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }

                // Store values
                Value = body.GetComponent<KSC>();
                if (Value == null)
                {
                    Value = generatedBody.celestialBody.gameObject.AddComponent<KSC>();
                    Value.Start();
                    UnityEngine.Object.DontDestroyOnLoad(Value);
                }
            }
        }
    }
}
