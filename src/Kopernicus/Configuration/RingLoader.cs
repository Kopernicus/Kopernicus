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
using System.Collections.Generic;
using Kopernicus.Configuration.NoiseLoader;
using Kopernicus.UI;
using UniLinq;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class RingLoader : BaseLoader, IParserEventSubscriber, ITypeParser<Ring>
        {
            // Set-up our custom ring
            public Ring Value { get; set; }

            // Inner Radius of our ring
            [ParserTarget("innerRadius")]
            [KittopiaDescription("The lower bound of the ring (measured in meters from the center of the body).")]
            public NumericParser<Single> innerRadius
            {
                get { return Value.innerRadius; }
                set { Value.innerRadius = value; }
            }

            // Outer Radius of our ring
            [ParserTarget("outerRadius")]
            [KittopiaDescription("The upper bound of the ring (measured in meters from the center of the body).")]
            public NumericParser<Single> outerRadius
            {
                get { return Value.outerRadius; }
                set { Value.outerRadius = value; }
            }
            
            // Inner Radius multiplier of the ring
            [ParserTarget("InnerRadiusMultiplier")]
            [KittopiaDescription("A curve that defines a multiplier for the inner radius, using an angle (degree).")]
            public FloatCurveParser innerRadiusMultiplier
            {
                get { return Value.innerMultCurve; }
                set { Value.innerMultCurve = value; }
            }
            
            // Outer Radius multiplier of the ring
            [ParserTarget("OuterRadiusMultiplier")]
            [KittopiaDescription("A curve that defines a multiplier for the outer radius, using an angle (degree).")]
            public FloatCurveParser outerRadiusMultiplier
            {
                get { return Value.outerMultCurve; }
                set { Value.outerMultCurve = value; }
            }

            /// <summary>
            /// Distance between the top and bottom faces in milliradii
            /// </summary>
            [ParserTarget("thickness")]
            [KittopiaDescription("Distance between the top and bottom faces in milliradii.")]
            public NumericParser<Single> thickness 
            {
                get { return Value.thickness;  }
                set { Value.thickness = value; }
            }

            // Axis angle (inclination) of our ring
            [ParserTarget("angle")]
            [KittopiaDescription("Axis angle (inclination) of our ring.")]
            public NumericParser<Single> angle
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
            public NumericParser<Single> longitudeOfAscendingNode
            {
                get { return Value.longitudeOfAscendingNode;  }
                set { Value.longitudeOfAscendingNode = value; }
            }

            // Texture of our ring
            [ParserTarget("texture")]
            [KittopiaDescription("Texture of the ring")]
            public Texture2DParser texture
            {
                get { return Value.texture; }
                set { Value.texture = value; }
            }

            // Color of our ring
            [ParserTarget("color")]
            [KittopiaDescription("Color of the ring")]
            public ColorParser color
            {
                get { return Value.color; }
                set { Value.color = value; }
            }

            // Lock rotation of our ring?
            [ParserTarget("lockRotation")]
            [KittopiaDescription("Whether the rotation of the ring should always stay the same.")]
            public NumericParser<Boolean> lockRotation
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
            [KittopiaDescription("Number of seconds for the ring to complete one rotation. If zero, fall back to matching parent body if lockRotation=false, and standing perfectly still if it's true.")]
            public NumericParser<Single> rotationPeriod
            {
                get { return Value.rotationPeriod;  }
                set { Value.rotationPeriod = value; }
            }

            // Unlit our ring?
            [ParserTarget("unlit")]
            [KittopiaDescription("Apply an unlit shader to the ring?")]
            public NumericParser<Boolean> unlit
            {
                get { return Value.unlit; }
                set { Value.unlit = value; }
            }

            // Use new shader with mie scattering and planet shadow?
            [ParserTarget("useNewShader")]
            [KittopiaDescription("Use the new custom ring shader instead of the builtin Unity shaders.")]
            public NumericParser<Boolean> useNewShader
            {
                get { return Value.useNewShader; }
                set { Value.useNewShader = value; }
            }

            // Penumbra multiplier for new shader
            [ParserTarget("penumbraMultiplier")]
            public NumericParser<Single> penumbraMultiplier
            {
                get { return Value.penumbraMultiplier; }
                set { Value.penumbraMultiplier = value; }
            }

            // Amount of vertices arount the ring
            [ParserTarget("steps")]
            [KittopiaDescription("Amount of vertices arount the ring.")]
            public NumericParser<Int32> steps
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
            [KittopiaDescription("Number of times the texture should be tiled around the cylinder. If zero, use the old behavior of sampling a thin diagonal strip from (0,0) to (1,1).")]
            public NumericParser<Int32> tiles
            {
                get { return Value.tiles; }
                set { Value.tiles = value; }
            }

            /// <summary>
            /// This texture's opaque pixels cast shadows on our inner surface
            /// </summary>
            [ParserTarget("innerShadeTexture")]
            [KittopiaDescription("This texture's opaque pixels cast shadows on our inner surface.")]
            public Texture2DParser innerShadeTexture
            {
                get { return Value.innerShadeTexture;  }
                set { Value.innerShadeTexture = value; }
            }

            /// <summary>
            /// The inner shade texture repeats this many times over the inner surface
            /// </summary>
            [ParserTarget("innerShadeTiles")]
            [KittopiaDescription("The inner shade texture repeats this many times over the inner surface.")]
            public NumericParser<Int32> innerShadeTiles
            {
                get { return Value.innerShadeTiles;  }
                set { Value.innerShadeTiles = value; }
            }

            /// <summary>
            /// Number of seconds the inner shade texture takes to complete one rotation
            /// </summary>
            [ParserTarget("innerShadeRotationPeriod")]
            [KittopiaDescription("Number of seconds the inner shade texture takes to complete one rotation")]
            public NumericParser<Single> innerShadeRotationPeriod
            {
                get { return Value.innerShadeRotationPeriod;  }
                set { Value.innerShadeRotationPeriod = value; }
            }

            [ParserTargetCollection("Components", AllowMerge = true, NameSignificance = NameSignificance.Type)]
            public CallbackList<ComponentLoader<Ring>> Components { get; set; }

            [KittopiaAction("Rebuild Ring")]
            [KittopiaDescription("Updates the mesh of the planetary ring.")]
            public void RebuildRing()
            {
                UnityEngine.Object.Destroy(Value.GetComponent<MeshRenderer>());
                UnityEngine.Object.Destroy(Value.GetComponent<MeshFilter>());
                Value.BuildRing();
            }

            [KittopiaDestructor]
            public void Destroy()
            {
                UnityEngine.Object.Destroy(Value.gameObject);
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
                Value.planetRadius = (Single) generatedBody.celestialBody.Radius;

                // Need to check the parent body's rotation to orient the LAN properly
                Value.referenceBody = generatedBody.celestialBody;
                
                // Create the Component callback
                Components = new CallbackList<ComponentLoader<Ring>> (e =>
                {
                    Value.Components = Components.Select(c => c.Value).ToList();
                });
                if (Value.innerMultCurve == null)
                {
                    Value.innerMultCurve = new FloatCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) });
                }
                if (Value.outerMultCurve == null)
                {
                    Value.outerMultCurve = new FloatCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) });
                }
            }

            /// <summary>
            /// Creates a new Ring Loader from a spawned CelestialBody.
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.CelestialBody)]
            public RingLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }
                
                Value = new GameObject(body.transform.name + "Ring").AddComponent<Ring>();
                Value.transform.parent = body.scaledBody.transform;
                Value.planetRadius = (Single) body.Radius;

                // Need to check the parent body's rotation to orient the LAN properly
                Value.referenceBody = body;
                
                // Create the Component callback
                Components = new CallbackList<ComponentLoader<Ring>> (e =>
                {
                    Value.Components = Components.Select(c => c.Value).ToList();
                });
                
                // Load existing Modules
                foreach (IComponent<Ring> component in Value.Components)
                {
                    Type componentType = component.GetType();
                    foreach (Type loaderType in Parser.ModTypes)
                    {
                        if (loaderType.BaseType == null)
                            continue;
                        if (loaderType.BaseType.Namespace != "Kopernicus.Configuration")
                            continue;
                        if (!loaderType.BaseType.Name.StartsWith("ComponentParser"))
                            continue;
                        if (loaderType.BaseType.GetGenericArguments()[0] != Value.GetType())
                            continue;
                        if (loaderType.BaseType.GetGenericArguments()[1] != componentType)
                            continue;
                        
                        // We found our loader type
                        ComponentLoader<Ring> loader = (ComponentLoader<Ring>) Activator.CreateInstance(loaderType);
                        loader.Create(component);
                        Components.Add(loader);
                    }
                }
                if (Value.innerMultCurve == null)
                {
                    Value.innerMultCurve = new FloatCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) });
                }
                if (Value.outerMultCurve == null)
                {
                    Value.outerMultCurve = new FloatCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) });
                }
            }

            // Initialize the RingLoader
            public RingLoader(Ring value)
            {
                Value = value;
                
                // Create the Component callback
                Components = new CallbackList<ComponentLoader<Ring>> (e =>
                {
                    Value.Components = Components.Select(c => c.Value).ToList();
                });
                
                // Load existing Modules
                foreach (IComponent<Ring> component in Value.Components)
                {
                    Type componentType = component.GetType();
                    foreach (Type loaderType in Parser.ModTypes)
                    {
                        if (loaderType.BaseType == null)
                            continue;
                        if (loaderType.BaseType.Namespace != "Kopernicus.Configuration")
                            continue;
                        if (!loaderType.BaseType.Name.StartsWith("ComponentParser"))
                            continue;
                        if (loaderType.BaseType.GetGenericArguments()[0] != Value.GetType())
                            continue;
                        if (loaderType.BaseType.GetGenericArguments()[1] != componentType)
                            continue;
                        
                        // We found our loader type
                        ComponentLoader<Ring> loader = (ComponentLoader<Ring>) Activator.CreateInstance(loaderType);
                        loader.Create(component);
                        Components.Add(loader);
                    }
                }
                if (Value.innerMultCurve == null)
                {
                    Value.innerMultCurve = new FloatCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) });
                }
                if (Value.outerMultCurve == null)
                {
                    Value.outerMultCurve = new FloatCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) });
                }
            }
        }
    }
}
