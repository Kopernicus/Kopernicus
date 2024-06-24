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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kopernicus.Components;
using Kopernicus.Components.ModularComponentSystem;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;
using Kopernicus.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kopernicus.Configuration
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class RingLoader : BaseLoader, IParserEventSubscriber, ITypeParser<Ring>
    {
        // Set-up our custom ring
        public Ring Value { get; set; }

        // Inner Radius of our ring
        [ParserTarget("innerRadius")]
        [KittopiaDescription("The lower bound of the ring (measured in meters from the center of the body).")]
        public NumericParser<Single> InnerRadius
        {
            get { return Value.innerRadius; }
            set { Value.innerRadius = value; }
        }

        // Outer Radius of our ring
        [ParserTarget("outerRadius")]
        [KittopiaDescription("The upper bound of the ring (measured in meters from the center of the body).")]
        public NumericParser<Single> OuterRadius
        {
            get { return Value.outerRadius; }
            set { Value.outerRadius = value; }
        }

        // Inner Radius multiplier of the ring
        [ParserTargetCollection("InnerRadiusMultiplier", Key = "key", NameSignificance = NameSignificance.Key)]
        [KittopiaDescription("A curve that defines a multiplier for the inner radius, using an angle (degree).")]
        public List<NumericCollectionParser<Single>> InnerRadiusMultiplier
        {
            get { return Utility.FloatCurveToList(Value.innerMultCurve); }
            set { Value.innerMultCurve = Utility.ListToFloatCurve(value); }
        }

        // Outer Radius multiplier of the ring
        [ParserTargetCollection("OuterRadiusMultiplier", Key = "key", NameSignificance = NameSignificance.Key)]
        [KittopiaDescription("A curve that defines a multiplier for the outer radius, using an angle (degree).")]
        public List<NumericCollectionParser<Single>> OuterRadiusMultiplier
        {
            get { return Utility.FloatCurveToList(Value.outerMultCurve); }
            set { Value.outerMultCurve = Utility.ListToFloatCurve(value); }
        }

        /// <summary>
        /// Distance between the top and bottom faces in milliradii
        /// </summary>
        [ParserTarget("thickness")]
        [KittopiaDescription("Distance between the top and bottom faces in milliradii.")]
        public NumericParser<Single> Thickness
        {
            get { return Value.thickness; }
            set { Value.thickness = value; }
        }

        // Axis angle (inclination) of our ring
        [ParserTarget("angle")]
        [KittopiaDescription("Axis angle (inclination) of our ring.")]
        public NumericParser<Single> Angle
        {
            get { return -Value.rotation.eulerAngles.x; }
            set { Value.rotation = Quaternion.Euler(-value, 0, 0); }
        }

        /// <summary>
        /// Angle between the absolute reference direction and the ascending node.
        /// Works just like the corresponding property on celestial bodies.
        /// </summary>
        [ParserTarget("longitudeOfAscendingNode")]
        [KittopiaDescription("Angle between the absolute reference direction and the ascending node.")]
        public NumericParser<Single> LongitudeOfAscendingNode
        {
            get { return Value.longitudeOfAscendingNode; }
            set { Value.longitudeOfAscendingNode = value; }
        }

        [ParserTarget("fadeoutStartDistance")]
        [KittopiaDescription("The maximum distance at which the rings are drawn without fading.")]
        public NumericParser<Single> FadeoutStartDistance
        {
            get { return Value.fadeoutStartDistance; }
            set { Value.fadeoutStartDistance = value; }
        }

        [ParserTarget("fadeoutStopDistance")]
        [KittopiaDescription("The minimum distance at which the rings are fully faded out.")]
        public NumericParser<Single> FadeoutStopDistance
        {
            get { return Value.fadeoutStopDistance; }
            set { Value.fadeoutStopDistance = value; }
        }

        [ParserTarget("fadeoutMinAlpha")]
        [KittopiaDescription("The opacity multiplier for the rings when fully faded out.")]
        public NumericParser<Single> FadeoutMinAlpha
        {
            get { return Value.fadeoutMinAlpha; }
            set { Value.fadeoutMinAlpha = value; }
        }

        // Texture of our ring
        [ParserTarget("texture")]
        [KittopiaDescription("Texture of the ring")]
        public Texture2DParser Texture
        {
            get { return Value.texture; }
            set { Value.texture = value; }
        }

        // Color of our ring
        [ParserTarget("color")]
        [KittopiaDescription("Color of the ring")]
        public ColorParser Color
        {
            get { return Value.color; }
            set { Value.color = value; }
        }

        // Lock rotation of our ring?
        [ParserTarget("lockRotation")]
        [KittopiaDescription("Whether the rotation of the ring should always stay the same.")]
        public NumericParser<Boolean> LockRotation
        {
            get { return Value.lockRotation; }
            set { Value.lockRotation = value; }
        }

        /// <summary>
        /// Number of seconds for the ring to complete one rotation.
        /// If zero, fall back to matching parent body if lockRotation=false,
        /// and standing perfectly still if it's true.
        /// </summary>
        [ParserTarget("rotationPeriod")]
        [KittopiaDescription(
            "Number of seconds for the ring to complete one rotation. If zero, fall back to matching parent body if lockRotation=false, and standing perfectly still if it's true.")]
        public NumericParser<Single> RotationPeriod
        {
            get { return Value.rotationPeriod; }
            set { Value.rotationPeriod = value; }
        }

        // Unlit our ring?
        [ParserTarget("unlit")]
        [KittopiaDescription("Apply an unlit shader to the ring?")]
        public NumericParser<Boolean> Unlit
        {
            get { return Value.unlit; }
            set { Value.unlit = value; }
        }

        // Use new shader with mie scattering and planet shadow?
        [ParserTarget("useNewShader")]
        [KittopiaDescription("Use the new custom ring shader instead of the builtin Unity shaders.")]
        public NumericParser<Boolean> UseNewShader
        {
            get { return Value.useNewShader; }
            set { Value.useNewShader = value; }
        }


        // Use front-lit half of shader?
        [ParserTarget("albedoStrength")]
        [KittopiaDescription("Intensity of the main ring texture as seen when looking at the lit side of the planet.")]
        public NumericParser<Single> AlbedoStrength
        {
            get { return Value.albedoStrength; }
            set { Value.albedoStrength = value; }
        }
        // Use backlit half of shader?
        [ParserTarget("scatteringStrength")]
        [KittopiaDescription("Intensity of the scattered light seen through the ring when looking towards the sun.")]
        public NumericParser<Single> ScatteringStrength
        {
            get { return Value.scatteringStrength; }
            set { Value.scatteringStrength = value; }
        }
        // MieG for backlighting
        [ParserTarget("anisotropy")]
        [KittopiaDescription("Mie phase function asymmetry for scattering effect; higher values result in a smaller spot.")]
        public NumericParser<Single> Anisotropy
        {
            get { return Value.anisotropy; }
            set { Value.anisotropy = value; }
        }
        // Texture of our ring
        [ParserTarget("backlitTexture")]
        [KittopiaDescription("Texture used for the backlit scattering effect")]
        public Texture2DParser BacklitTexture
        {
            get { return Value.backlitTexture; }
            set { Value.backlitTexture = value; }
        }

        // Penumbra multiplier for new shader
        [ParserTarget("penumbraMultiplier")]
        public NumericParser<Single> PenumbraMultiplier
        {
            get { return Value.penumbraMultiplier; }
            set { Value.penumbraMultiplier = value; }
        }

        // Amount of vertices around the ring
        [ParserTarget("steps")]
        [KittopiaDescription("Amount of vertices around the ring.")]
        public NumericParser<Int32> Steps
        {
            get { return Value.steps; }
            set { Value.steps = value; }
        }

        /// <summary>
        /// Number of times the texture should be tiled around the cylinder
        /// If zero, use the old behavior of sampling a thin diagonal strip
        /// from (0,0) to (1,1).
        /// </summary>
        [ParserTarget("tiles")]
        [KittopiaDescription(
            "Number of times the texture should be tiled around the cylinder. If zero, use the old behavior of sampling a thin diagonal strip from (0,0) to (1,1).")]
        public NumericParser<Int32> Tiles
        {
            get { return Value.tiles; }
            set { Value.tiles = value; }
        }

        /// <summary>
        /// This texture's opaque pixels cast shadows on our inner surface
        /// </summary>
        [ParserTarget("innerShadeTexture")]
        [KittopiaDescription("This texture's opaque pixels cast shadows on our inner surface.")]
        public Texture2DParser InnerShadeTexture
        {
            get { return Value.innerShadeTexture; }
            set { Value.innerShadeTexture = value; }
        }

        /// <summary>
        /// The inner shade texture repeats this many times over the inner surface
        /// </summary>
        [ParserTarget("innerShadeTiles")]
        [KittopiaDescription("The inner shade texture repeats this many times over the inner surface.")]
        public NumericParser<Int32> InnerShadeTiles
        {
            get { return Value.innerShadeTiles; }
            set { Value.innerShadeTiles = value; }
        }

        /// <summary>
        /// Number of seconds the inner shade texture takes to complete one rotation
        /// </summary>
        [ParserTarget("innerShadeRotationPeriod")]
        [KittopiaDescription("Number of seconds the inner shade texture takes to complete one rotation")]
        public NumericParser<Single> InnerShadeRotationPeriod
        {
            get { return Value.innerShadeRotationPeriod; }
            set { Value.innerShadeRotationPeriod = value; }
        }

        // Detail data
        [ParserTarget("Detail")]
        public DetailLoader Detail
        {
            get { return new DetailLoader(Value.detailSettings); }
            set { Value.detailSettings = value.Value; }
        }

        [RequireConfigType(ConfigType.Node)]
        public class DetailPassLoader : BaseLoader, ITypeParser<Ring.DetailPass>
        {
            public Ring.DetailPass Value { get; set; }

            public DetailPassLoader()
            {
                Value = new Ring.DetailPass();
            }

            public DetailPassLoader(Ring.DetailPass pass)
            {
                Value = pass;
            }

            [ParserTarget("texture")]
            [KittopiaDescription("The texture used for this layer of detail.")]
            public Texture2DParser Texture
            {
                get { return Value.texture; }
                set { Value.texture = value; }
            }

            [ParserTarget("alphaMin")]
            [KittopiaDescription("A per-channel override of the minimum opacity multiplier.")]
            public Vector4Parser AlphaMin
            {
                get { return Value.alphaMin; }
                set { Value.alphaMin = value; }
            }

            [ParserTarget("alphaMax")]
            [KittopiaDescription("A per-channel override of the maximum opacity multiplier.")]
            public Vector4Parser AlphaMax
            {
                get { return Value.alphaMax; }
                set { Value.alphaMax = value; }
            }

            [ParserTarget("tiling")]
            [KittopiaDescription("Texture tiling multiplier for this level of ring detail.")]
            public Vector2Parser Tiling
            {
                get { return Value.tiling; }
                set { Value.tiling = value; }
            }

            [ParserTarget("strength")]
            [KittopiaDescription("The strength of the detail overlay effect.")]
            public NumericParser<float> Strength
            {
                get { return Value.strength; }
                set { Value.strength = value; }
            }

            [ParserTarget("fadeInStart")]
            [KittopiaDescription("The distance from the camera that a ring pixel has to be for this detail level to start being blended in.")]
            public NumericParser<float> FadeInStart
            {
                get { return Value.fadeParams.x; }
                set { Value.fadeParams.x = value; }
            }

            [ParserTarget("fadeInEnd")]
            [KittopiaDescription("The distance from the camera that a ring pixel has to be for this detail level to fully be blended in.")]
            public NumericParser<float> FadeInEnd
            {
                get { return Value.fadeParams.y; }
                set { Value.fadeParams.y = value; }
            }

            [ParserTarget("fadeOutStart")]
            [KittopiaDescription("The distance from the camera at which this detail level will start being faded out again.")]
            public NumericParser<float> FadeOutStart
            {
                get { return Value.fadeParams.z; }
                set { Value.fadeParams.z = value; }
            }

            [ParserTarget("fadeOutEnd")]
            [KittopiaDescription("The distance from the camera at which this detail level is again fully ignored.")]
            public NumericParser<float> FadeOutEnd
            {
                get { return Value.fadeParams.w; }
                set { Value.fadeParams.w = value; }
            }

            [ParserTarget("detailMask")]
            [KittopiaDescription("A per-detail-pass per-texture-channel multiplier.")]
            public Vector4Parser DetailMask
            {
                get { return Value.detailMask; }
                set { Value.detailMask = value; }
            }
        }

        [RequireConfigType(ConfigType.Node)]
        public class DetailLoader : BaseLoader, ITypeParser<Ring.DetailSettings>
        {
            public Ring.DetailSettings Value { get; set; }

            public DetailLoader()
            {
                Value = new Ring.DetailSettings();
            }

            public DetailLoader(Ring.DetailSettings settings)
            {
                Value = settings;
            }

            [ParserTarget("detailRegionsMask")]
            [KittopiaDescription("A mask that is applied to the detail regions texture mutiplicatively.")]
            public Vector4Parser DetailRegionsMask
            {
                get { return Value.detailRegionsMask; }
                set { Value.detailRegionsMask = value; }
            }

            [ParserTarget("detailRegionsTexture")]
            [KittopiaDescription("A texture that defines per-location prominence of the detail noise texture channels.")]
            public Texture2DParser DetailRegionsTexture
            {
                get { return Value.detailRegionsTexture; }
                set { Value.detailRegionsTexture = value; }
            }

            [ParserTarget("Coarse")]
            public DetailPassLoader DetailCoarse
            {
                get { return new DetailPassLoader(Value.coarse); }
                set { Value.coarse = value.Value; }
            }

            [ParserTarget("Fine")]
            public DetailPassLoader DetailFine
            {
                get { return new DetailPassLoader(Value.fine); }
                set { Value.fine = value.Value; }
            }
        }

        [ParserTargetCollection("Components", AllowMerge = true, NameSignificance = NameSignificance.Type)]
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public CallbackList<ComponentLoader<Ring>> Components { get; set; }

        [KittopiaAction("Rebuild Ring")]
        [KittopiaDescription("Updates the mesh of the planetary ring.")]
        public void RebuildRing()
        {
            Object.DestroyImmediate(Value.GetComponent<MeshRenderer>());
            Object.DestroyImmediate(Value.GetComponent<MeshFilter>());
            Value.BuildRing();
        }

        [KittopiaDestructor]
        public void Destroy()
        {
            Object.Destroy(Value.gameObject);
        }

        // Apply event
        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            Events.OnRingLoaderApply.Fire(this, node);
        }

        // Post-Apply event
        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
            Events.OnRingLoaderPostApply.Fire(this, node);
        }

        // Initialize the RingLoader
        public RingLoader()
        {
            // Is this the parser context?
            if (!Injector.IsInPrefab)
            {
                throw new InvalidOperationException("Must be executed in Injector context.");
            }

            Value = new GameObject(generatedBody.name + "Ring").AddComponent<Ring>();
            Value.transform.parent = generatedBody.scaledVersion.transform;
            Value.planetRadius = (Single)generatedBody.celestialBody.Radius;

            // Need to check the parent body's rotation to orient the LAN properly
            Value.referenceBody = generatedBody.celestialBody;

            // Create the Component callback
            Components = new CallbackList<ComponentLoader<Ring>>(e =>
            {
                Value.Components = Components.Select(c => c.Value).ToList();
            });
            if (Value.innerMultCurve == null)
            {
                Value.innerMultCurve = new FloatCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) });
            }

            if (Value.outerMultCurve == null)
            {
                Value.outerMultCurve = new FloatCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) });
            }
        }

        /// <summary>
        /// Creates a new Ring Loader from a spawned CelestialBody.
        /// </summary>
        [KittopiaConstructor(KittopiaConstructor.ParameterType.CelestialBody)]
        public RingLoader(CelestialBody body)
        {
            // Is this a spawned body?
            if (body.scaledBody == null || Injector.IsInPrefab)
            {
                throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
            }

            Value = new GameObject(body.transform.name + "Ring").AddComponent<Ring>();
            Value.transform.parent = body.scaledBody.transform;
            Value.planetRadius = (Single)body.Radius;

            // Need to check the parent body's rotation to orient the LAN properly
            Value.referenceBody = body;

            // Create the Component callback
            Components = new CallbackList<ComponentLoader<Ring>>(e =>
            {
                Value.Components = Components.Select(c => c.Value).ToList();
            });

            // Load existing Modules
            foreach (IComponent<Ring> component in Value.Components)
            {
                Type componentType = component.GetType();
                Type componentLoaderType = typeof(ComponentLoader<,>).MakeGenericType(typeof(Ring), componentType);
                foreach (Type loaderType in Parser.ModTypes)
                {
                    if (!componentLoaderType.IsAssignableFrom(loaderType))
                    {
                        continue;
                    }

                    // We found our loader type
                    ComponentLoader<Ring> loader = (ComponentLoader<Ring>) Activator.CreateInstance(loaderType);
                    loader.Create(component);
                    Components.Add(loader);
                }
            }

            if (Value.innerMultCurve == null)
            {
                Value.innerMultCurve = new FloatCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) });
            }

            if (Value.outerMultCurve == null)
            {
                Value.outerMultCurve = new FloatCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) });
            }
        }

        // Initialize the RingLoader
        public RingLoader(Ring value)
        {
            Value = value;

            // Create the Component callback
            Components = new CallbackList<ComponentLoader<Ring>>(e =>
            {
                Value.Components = Components.Select(c => c.Value).ToList();
            });

            // Load existing Modules
            foreach (IComponent<Ring> component in Value.Components)
            {
                Type componentType = component.GetType();
                Type componentLoaderType = typeof(ComponentLoader<,>).MakeGenericType(typeof(Ring), componentType);
                foreach (Type loaderType in Parser.ModTypes)
                {
                    if (!componentLoaderType.IsAssignableFrom(loaderType))
                    {
                        continue;
                    }

                    // We found our loader type
                    ComponentLoader<Ring> loader = (ComponentLoader<Ring>) Activator.CreateInstance(loaderType);
                    loader.Create(component);
                    Components.Add(loader);
                }
            }

            if (Value.innerMultCurve == null)
            {
                Value.innerMultCurve = new FloatCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) });
            }

            if (Value.outerMultCurve == null)
            {
                Value.outerMultCurve = new FloatCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) });
            }
        }
    }
}
