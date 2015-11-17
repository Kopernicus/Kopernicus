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

using Kopernicus.Components;
using UnityEngine;
using System.Linq;

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
            [ParserTarget("latitude", optional = true, allowMerge = false)]
            public NumericParser<double> latitude
            {
                get { return ksc.latitude; }
                set { ksc.latitude = value; }
            }

            // longitude
            [ParserTarget("longitude", optional = true, allowMerge = false)]
            public NumericParser<double> longitude
            {
                get { return ksc.longitude; }
                set { ksc.longitude = value; }
            }

            [ParserTarget("repositionRadial", optional = true, allowMerge = false)]
            public Vector3Parser repositionRadial
            {
                get { return ksc.repositionRadial; }
                set { ksc.repositionRadial = value; }
            }

            // decalLatitude
            [ParserTarget("decalLatitude", optional = true, allowMerge = false)]
            public NumericParser<double> decalLatitude
            {
                get { return ksc.decalLatitude; }
                set { ksc.decalLatitude = value; }
            }

            // decalLongitude
            [ParserTarget("decalLongitude", optional = true, allowMerge = false)]
            public NumericParser<double> decalLongitude
            {
                get { return ksc.decalLongitude; }
                set { ksc.decalLongitude = value; }
            }

            // lodvisibleRangeMultipler
            [ParserTarget("lodvisibleRangeMultipler", optional = true, allowMerge = false)]
            public NumericParser<double> lodvisibleRangeMultipler
            {
                get { return ksc.lodvisibleRangeMult; }
                set { ksc.lodvisibleRangeMult = value; }
            }

            // reorientFinalAngle
            [ParserTarget("reorientFinalAngle", optional = true, allowMerge = false)]
            public NumericParser<float> reorientFinalAngle
            {
                get { return ksc.reorientFinalAngle; }
                set { ksc.reorientFinalAngle = value; }
            }

            // reorientInitialUp
            [ParserTarget("reorientInitialUp", optional = true, allowMerge = false)]
            public Vector3Parser reorientInitialUp
            {
                get { return ksc.reorientInitialUp; }
                set { ksc.reorientInitialUp = value; }
            }

            // reorientToSphere
            [ParserTarget("reorientToSphere", optional = true, allowMerge = false)]
            public NumericParser<bool> reorientToSphere
            {
                get { return ksc.reorientToSphere; }
                set { ksc.reorientToSphere = value; }
            }

            // repositionRadiusOffset
            [ParserTarget("repositionRadiusOffset", optional = true, allowMerge = false)]
            public NumericParser<double> repositionRadiusOffset
            {
                get { return ksc.repositionRadiusOffset; }
                set { ksc.repositionRadiusOffset = value; }
            }

            // repositionToSphere
            [ParserTarget("repositionToSphere", optional = true, allowMerge = false)]
            public NumericParser<bool> repositionToSphere
            {
                get { return ksc.repositionToSphere; }
                set { ksc.repositionToSphere = value; }
            }

            // repositionToSphereSurface
            [ParserTarget("repositionToSphereSurface", optional = true, allowMerge = false)]
            public NumericParser<bool> repositionToSphereSurface
            {
                get { return ksc.repositionToSphereSurface; }
                set { ksc.repositionToSphereSurface = value; }
            }

            // repositionToSphereSurfaceAddHeight
            [ParserTarget("repositionToSphereSurfaceAddHeight", optional = true, allowMerge = false)]
            public NumericParser<bool> repositionToSphereSurfaceAddHeight
            {
                get { return ksc.repositionToSphereSurfaceAddHeight; }
                set { ksc.repositionToSphereSurfaceAddHeight = value; }
            }

            // position
            [ParserTarget("position", optional = true, allowMerge = false)]
            public Vector3Parser position
            {
                get { return ksc.position; }
                set { ksc.position = value; }
            }

            // radius
            [ParserTarget("radius", optional = true, allowMerge = false)]
            public NumericParser<double> radius
            {
                get { return ksc.radius; }
                set { ksc.radius = value; }
            }

            // heightMapDeformity
            [ParserTarget("heightMapDeformity", optional = true, allowMerge = false)]
            public NumericParser<double> heightMapDeformity
            {
                get { return ksc.heightMapDeformity; }
                set { ksc.heightMapDeformity = value; }
            }

            // absoluteOffset
            [ParserTarget("absoluteOffset", optional = true, allowMerge = false)]
            public NumericParser<double> absoluteOffset
            {
                get { return ksc.absoluteOffset; }
                set { ksc.absoluteOffset = value; }
            }

            // absolute
            [ParserTarget("absolute", optional = true, allowMerge = false)]
            public NumericParser<bool> absolute
            {
                get { return ksc.absolute; }
                set { ksc.absolute = value; }
            }

            // groundColor
            [ParserTarget("groundColor", optional = true, allowMerge = false)]
            public ColorParser groundColorParser
            {
                get { return ksc.color; }
                set { ksc.color = value; }
            }

            // Texture
            [ParserTarget("groundTexture", optional = true, allowMerge = false)]
            public Texture2DParser groundTextureParser
            {
                get { return ksc.mainTexture; }
                set { ksc.mainTexture = value; }
            }

            // Default constructor
            public SpaceCenterLoader()
            {
                ksc = new GameObject("SpaceCenter " + generatedBody.name).AddComponent<KSC>();
                Object.DontDestroyOnLoad(ksc);
            }

            // Runtime Constructor
            public SpaceCenterLoader(CelestialBody body)
            {
                ksc = Object.FindObjectsOfType<KSC>().First(k => k.name == "SpaceCenter " + body.name);
            }

            // Apply event
            void IParserEventSubscriber.Apply(ConfigNode node) { }

            // Post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node) { }
        }
    }
}
