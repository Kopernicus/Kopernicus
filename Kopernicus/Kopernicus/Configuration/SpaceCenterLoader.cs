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
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class SpaceCenterLoader : BaseLoader, IParserEventSubscriber
        {
            // The KSC Object we're editing
            public KSC ksc;

            // latitude
            [ParserTarget("latitude")]
            public NumericParser<Double> latitude
            {
                get { return ksc.latitude; }
                set { ksc.latitude = value; }
            }

            // longitude
            [ParserTarget("longitude")]
            public NumericParser<Double> longitude
            {
                get { return ksc.longitude; }
                set { ksc.longitude = value; }
            }

            [ParserTarget("repositionRadial")]
            public Vector3Parser repositionRadial
            {
                get { return ksc.repositionRadial; }
                set { ksc.repositionRadial = value; }
            }

            // decalLatitude
            [ParserTarget("decalLatitude")]
            public NumericParser<Double> decalLatitude
            {
                get { return ksc.decalLatitude; }
                set { ksc.decalLatitude = value; }
            }

            // decalLongitude
            [ParserTarget("decalLongitude")]
            public NumericParser<Double> decalLongitude
            {
                get { return ksc.decalLongitude; }
                set { ksc.decalLongitude = value; }
            }

            // lodvisibleRangeMultipler
            [ParserTarget("lodvisibleRangeMultipler")]
            public NumericParser<Double> lodvisibleRangeMultipler
            {
                get { return ksc.lodvisibleRangeMult; }
                set { ksc.lodvisibleRangeMult = value; }
            }

            // reorientFinalAngle
            [ParserTarget("reorientFinalAngle")]
            public NumericParser<Single> reorientFinalAngle
            {
                get { return ksc.reorientFinalAngle; }
                set { ksc.reorientFinalAngle = value; }
            }

            // reorientInitialUp
            [ParserTarget("reorientInitialUp")]
            public Vector3Parser reorientInitialUp
            {
                get { return ksc.reorientInitialUp; }
                set { ksc.reorientInitialUp = value; }
            }

            // reorientToSphere
            [ParserTarget("reorientToSphere")]
            public NumericParser<Boolean> reorientToSphere
            {
                get { return ksc.reorientToSphere; }
                set { ksc.reorientToSphere = value; }
            }

            // repositionRadiusOffset
            [ParserTarget("repositionRadiusOffset")]
            public NumericParser<Double> repositionRadiusOffset
            {
                get { return ksc.repositionRadiusOffset; }
                set { ksc.repositionRadiusOffset = value; }
            }

            // repositionToSphere
            [ParserTarget("repositionToSphere")]
            public NumericParser<Boolean> repositionToSphere
            {
                get { return ksc.repositionToSphere; }
                set { ksc.repositionToSphere = value; }
            }

            // repositionToSphereSurface
            [ParserTarget("repositionToSphereSurface")]
            public NumericParser<Boolean> repositionToSphereSurface
            {
                get { return ksc.repositionToSphereSurface; }
                set { ksc.repositionToSphereSurface = value; }
            }

            // repositionToSphereSurfaceAddHeight
            [ParserTarget("repositionToSphereSurfaceAddHeight")]
            public NumericParser<Boolean> repositionToSphereSurfaceAddHeight
            {
                get { return ksc.repositionToSphereSurfaceAddHeight; }
                set { ksc.repositionToSphereSurfaceAddHeight = value; }
            }

            // position
            [ParserTarget("position")]
            public Vector3Parser position
            {
                get { return ksc.position; }
                set { ksc.position = value; }
            }

            // radius
            [ParserTarget("radius")]
            public NumericParser<Double> radius
            {
                get { return ksc.radius; }
                set { ksc.radius = value; }
            }

            // heightMapDeformity
            [ParserTarget("heightMapDeformity")]
            public NumericParser<Double> heightMapDeformity
            {
                get { return ksc.heightMapDeformity; }
                set { ksc.heightMapDeformity = value; }
            }

            // absoluteOffset
            [ParserTarget("absoluteOffset")]
            public NumericParser<Double> absoluteOffset
            {
                get { return ksc.absoluteOffset; }
                set { ksc.absoluteOffset = value; }
            }

            // absolute
            [ParserTarget("absolute")]
            public NumericParser<Boolean> absolute
            {
                get { return ksc.absolute; }
                set { ksc.absolute = value; }
            }

            // groundColor
            [ParserTarget("groundColor")]
            public ColorParser groundColorParser
            {
                get { return ksc.color; }
                set { ksc.color = value; }
            }

            // Texture
            [ParserTarget("groundTexture")]
            public Texture2DParser groundTextureParser
            {
                get { return ksc.mainTexture; }
                set { ksc.mainTexture = value; }
            }

            /// <summary>
            /// Creates a new SpaceCenter Loader from the Injector context.
            /// </summary>
            public SpaceCenterLoader()
            {
                // Is this the parser context?
                if (generatedBody == null)
                    throw new InvalidOperationException("Must be executed in Injector context.");

                // Store values
                ksc = new GameObject("SpaceCenter " + generatedBody.name).AddComponent<KSC>();
                UnityEngine.Object.DontDestroyOnLoad(ksc);
            }

            /// <summary>
            /// Creates a new SpaceCenter Loader from a spawned CelestialBody.
            /// </summary>
            public SpaceCenterLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null)
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");

                // Store values
                ksc = UnityEngine.Object.FindObjectsOfType<KSC>().First(k => k.name == "SpaceCenter " + body.name);
            }

            /// <summary>
            /// Creates a new SpaceCenter Loader from a custom PSystemBody.
            /// </summary>
            public SpaceCenterLoader(PSystemBody body)
            {
                // Set generatedBody
                generatedBody = body ?? throw new InvalidOperationException("The body cannot be null.");

                // Store values
                ksc = new GameObject("SpaceCenter " + generatedBody.name).AddComponent<KSC>();
                UnityEngine.Object.DontDestroyOnLoad(ksc);
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
        }
    }
}
