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
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Diagnostics.CodeAnalysis;
using Kopernicus.Components;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;
using Kopernicus.UI;
using Object = UnityEngine.Object;

namespace Kopernicus.Configuration
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SpaceCenterLoader : BaseLoader, IParserEventSubscriber, ITypeParser<KSC>
    {
        // The KSC Object we're editing
        public KSC Value { get; set; }

        // latitude
        [ParserTarget("latitude")]
        [KittopiaDescription("The latitude of the KSC buildings.")]
        public NumericParser<Double> Latitude
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
        public NumericParser<Double> Longitude
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
        public Vector3Parser RepositionRadial
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
        public NumericParser<Double> DecalLatitude
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
        public NumericParser<Double> DecalLongitude
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

        // lodvisibleRangeMultiplier
        [ParserTarget("lodvisibleRangeMultiplier")]
        public NumericParser<Double> LodvisibleRangeMultiplier
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
        public NumericParser<Single> ReorientFinalAngle
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
        public Vector3Parser ReorientInitialUp
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
        public NumericParser<Boolean> ReorientToSphere
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
        public NumericParser<Double> RepositionRadiusOffset
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
        public NumericParser<Boolean> RepositionToSphere
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
        public NumericParser<Boolean> RepositionToSphereSurface
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
        public NumericParser<Boolean> RepositionToSphereSurfaceAddHeight
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
        public Vector3Parser Position
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
        public NumericParser<Double> Radius
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
        public NumericParser<Double> HeightMapDeformity
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
        public NumericParser<Double> AbsoluteOffset
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
        public NumericParser<Boolean> Absolute
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
        [KittopiaDescription("The color of the KSC grass.")]
        public ColorParser GroundColorParser
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
        [KittopiaDescription("The texture of the KSC grass from up close.")]
        public Texture2DParser GroundTextureParser
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

        // Editor Ground Color
        [ParserTarget("editorGroundColor")]
        [KittopiaDescription("The color of the grass all around the KSC (editor only).")]
        public ColorParser EditorGroundColorParser
        {
            get { return Value.editorGroundColor; }
            set
            {
                Value.editorGroundColor = value;
                if (!Injector.IsInPrefab)
                {
                    Value.Start();
                }
            }
        }

        [ParserTarget("Material", AllowMerge = true)]
        [KittopiaUntouchable]
        public GrassMaterialLoader GrassMaterialLoader { get; set; }

        // Editor Ground Texture
        [ParserTarget("editorGroundTex")]
        [KittopiaDescription("The surface texture of the grass all around the KSC (editor only).")]
        public Texture2DParser EditorGroundTexParser
        {
            get { return Value.editorGroundTex; }
            set
            {
                Value.editorGroundTex = value;
                if (!Injector.IsInPrefab)
                {
                    Value.Start();
                }
            }
        }

        [ParserTarget("editorGroundTexScale")]
        [KittopiaDescription("The scale of the surface texture of the grass all around the KSC (editor only).")]
        public Vector2Parser EditorGroundTexScaleParser
        {
            get { return Value.editorGroundTexScale; }
            set
            {
                Value.editorGroundTexScale = value;
                if (!Injector.IsInPrefab)
                {
                    Value.Start();
                }
            }
        }

        [ParserTarget("editorGroundTexOffset")]
        [KittopiaDescription("The offset of the surface texture of the grass all around the KSC (editor only).")]
        public Vector2Parser EditorGroundTexOffsetParser
        {
            get { return Value.editorGroundTexOffset; }
            set
            {
                Value.editorGroundTexOffset = value;
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
            Object.DontDestroyOnLoad(Value);
        }

        /// <summary>
        /// Creates a new SpaceCenter Loader from a spawned CelestialBody.
        /// </summary>
        [KittopiaConstructor(KittopiaConstructor.ParameterType.CelestialBody)]
        public SpaceCenterLoader(CelestialBody body)
        {
            // Is this a spawned body?
            if (body.scaledBody == null || Injector.IsInPrefab)
            {
                throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
            }

            // Store values
            Value = body.GetComponent<KSC>();
            if (Value != null)
            {
                return;
            }
            Value = body.gameObject.AddComponent<KSC>();
            Value.Start();
            Object.DontDestroyOnLoad(Value);
        }
    }

    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class GrassMaterialLoader : BaseLoader, ITypeParser<KSC.GrassMaterial>
    {
        // The KSC.GrassMaterial Object we're editing
        public KSC.GrassMaterial Value { get; set; }

        // NearGrass
        [ParserTarget("nearGrassTexture")]
        [KittopiaDescription("The texture of the KSC grass from up close.")]
        public Texture2DParser NearGrassTexture
        {
            get { return Value.nearGrassTexture; }
            set
            {
                Value.nearGrassTexture = value;
            }
        }

        [ParserTarget("nearGrassTiling")]
        [KittopiaDescription("The tiling of the KSC grass from up close.")]
        public NumericParser<Single> NearGrassTiling
        {
            get { return Value.nearGrassTiling; }
            set
            {
                Value.nearGrassTiling = value;
            }
        }

        // FarGrass
        [ParserTarget("farGrassTexture")]
        [KittopiaDescription("The texture of the KSC grass from a distance.")]
        public Texture2DParser FarGrassTexture
        {
            get { return Value.farGrassTexture; }
            set
            {
                Value.farGrassTexture = value;
            }
        }

        [ParserTarget("farGrassTiling")]
        [KittopiaDescription("The tiling of the KSC grass from a distance.")]
        public NumericParser<Single> FarGrassTiling
        {
            get { return Value.farGrassTiling; }
            set
            {
                Value.farGrassTiling = value;
            }
        }

        [ParserTarget("farGrassBlendDistance")]
        [KittopiaDescription("The blend distance to the far KSC grass.")]
        public NumericParser<Single> FarGrassBlendDistance
        {
            get { return Value.farGrassBlendDistance; }
            set
            {
                Value.farGrassBlendDistance = value;
            }
        }

        // GrassColor
        [ParserTarget("grassColor")]
        [KittopiaDescription("The color of the KSC grass.")]
        public ColorParser GrassColor
        {
            get { return Value.grassColor; }
            set
            {
                Value.grassColor = value;
            }
        }

        // Tarmac
        [ParserTarget("tarmacTexture")]
        [KittopiaDescription("The texture of the KSC tarmac.")]
        public Texture2DParser TarmacTexture
        {
            get { return Value.tarmacTexture; }
            set
            {
                Value.tarmacTexture = value;
            }
        }

        [ParserTarget("tarmacTextureOffset")]
        [KittopiaDescription("The texture offset of the KSC tarmac.")]
        public Vector2Parser TarmacTextureOffset
        {
            get { return Value.tarmacTextureOffset; }
            set
            {
                Value.tarmacTextureOffset = value;
            }
        }

        [ParserTarget("tarmacTextureScale")]
        [KittopiaDescription("The texture Scale of the KSC tarmac.")]
        public Vector2Parser TarmacTextureScale
        {
            get { return Value.tarmacTextureScale; }
            set
            {
                Value.tarmacTextureScale = value;
            }
        }

        // Other
        [ParserTarget("opacity")]
        [KittopiaDescription("The opacity of the KSC grass material.")]
        public NumericParser<Single> Opacity
        {
            get { return Value.opacity; }
            set
            {
                Value.opacity = value;
            }
        }

        [ParserTarget("rimColor")]
        [KittopiaDescription("The rimColor of the KSC grass material.")]
        public ColorParser RimColor
        {
            get { return Value.rimColor; }
            set
            {
                Value.rimColor = value;
            }
        }

        [ParserTarget("rimFalloff")]
        [KittopiaDescription("The opacity of the KSC grass material.")]
        public NumericParser<Single> RimFalloff
        {
            get { return Value.rimFalloff; }
            set
            {
                Value.rimFalloff = value;
            }
        }

        [ParserTarget("underwaterFogFactor")]
        [KittopiaDescription("The underwaterFogFactor of the KSC grass material.")]
        public NumericParser<Single> UnderwaterFogFactor
        {
            get { return Value.underwaterFogFactor; }
            set
            {
                Value.underwaterFogFactor = value;
            }
        }

        /// <summary>
        /// Creates a new GrassMaterial Loader from the Injector context.
        /// </summary>
        public GrassMaterialLoader()
        {
            // Is this the parser context?
            if (!Injector.IsInPrefab)
            {
                throw new InvalidOperationException("Must be executed in Injector context.");
            }

            // Store values
            Value = generatedBody.celestialBody.gameObject.GetComponent<KSC>().Material = new KSC.GrassMaterial();
        }

        /// <summary>
        /// Creates a new GrassMaterial Loader from a spawned CelestialBody.
        /// </summary>
        [KittopiaConstructor(KittopiaConstructor.ParameterType.CelestialBody)]
        public GrassMaterialLoader(CelestialBody body)
        {
            // Is this a spawned body?
            if (body.scaledBody == null || Injector.IsInPrefab)
            {
                throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
            }

            // Store values
            KSC ksc = body.GetComponent<KSC>();
            if (ksc != null)
            {
                Value = ksc.Material;
                if (Value != null)
                {
                    return;
                }
                Value = ksc.Material = new KSC.GrassMaterial();
            }
        }
    }
}
