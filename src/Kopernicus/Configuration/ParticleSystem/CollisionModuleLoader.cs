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
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class CollisionModuleLoader : ITypeParser<ParticleSystem.CollisionModule>
        {
            // The module we are editing
            private ParticleSystem.CollisionModule _value;
            public ParticleSystem.CollisionModule Value
            {
                get { return _value; }
                set { _value = value; }
            }

            [ParserTarget("Bounce")]
            [KittopiaDescription("How much force is applied to each particle after a collision.")]
            public MinMaxCurveLoader bounce
            {
                get { return _value.bounce; }
                set { _value.bounce = value; }
            }

            [ParserTarget("collidesWith")]
            [KittopiaDescription("Control which layers this particle system collides with.")]
            public NumericCollectionParser<Int32> collidesWith
            {
                get
                {
                    List<Int32> layers = new List<Int32>();
                    for (Int32 i = 1; i < 32; i++)
                    {
                        Int32 mask = 1 << i;
                        if ((_value.collidesWith.value & mask) != 0)
                        {
                            layers.Add(i);
                        }
                    }
                    return new NumericCollectionParser<Int32>(layers);
                    ;
                }
                set
                {
                    _value.collidesWith = 0;
                    foreach (Int32 i in value.Value)
                    {
                        _value.collidesWith |= 1 << i;
                    }
                }
            }

            [ParserTarget("Dampen")]
            [KittopiaDescription("How much speed is lost from each particle after a collision.")]
            public MinMaxCurveLoader dampen
            {
                get { return _value.dampen; }
                set { _value.dampen = value; }
            }

            [ParserTarget("enabled")]
            [KittopiaDescription("Enable/disable the Collision module.")]
            public NumericParser<Boolean> enabled
            {
                get { return _value.enabled; }
                set { _value.enabled = value; }
            }

            [ParserTarget("enableDynamicColliders")]
            [KittopiaDescription("Allow particles to collide with dynamic colliders when using world collision mode.")]
            public NumericParser<Boolean> enableDynamicColliders
            {
                get { return _value.enableDynamicColliders; }
                set { _value.enableDynamicColliders = value; }
            }

            [ParserTarget("enableInteriorCollisions")]
            [KittopiaDescription("Allow particles to collide when inside colliders.")]
            public NumericParser<Boolean> enableInteriorCollisions
            {
                get { return _value.enableInteriorCollisions; }
                set { _value.enableInteriorCollisions = value; }
            }

            [ParserTarget("LifetimeLoss")]
            [KittopiaDescription("How much a particle's lifetime is reduced after a collision.")]
            public MinMaxCurveLoader lifetimeLoss
            {
                get { return _value.lifetimeLoss; }
                set { _value.lifetimeLoss = value; }
            }

            [ParserTarget("maxCollisionShapes")]
            [KittopiaDescription("The maximum number of collision shapes that will be considered for particle collisions.")]
            public NumericParser<Int32> maxCollisionShapes
            {
                get { return _value.maxCollisionShapes; }
                set { _value.maxCollisionShapes = value; }
            }

            [ParserTarget("maxKillSpeed")]
            [KittopiaDescription("Kill particles whose speed goes above this threshold, after a collision.")]
            public NumericParser<Single> maxKillSpeed
            {
                get { return _value.maxKillSpeed; }
                set { _value.maxKillSpeed = value; }
            }

            [ParserTarget("minKillSpeed")]
            [KittopiaDescription("Kill particles whose speed falls below this threshold, after a collision.")]
            public NumericParser<Single> minKillSpeed
            {
                get { return _value.minKillSpeed; }
                set { _value.minKillSpeed = value; }
            }

            [ParserTarget("quality")]
            [KittopiaDescription("Specifies the accuracy of particle collisions against colliders in the scene.")]
            public EnumParser<ParticleSystemCollisionQuality> quality
            {
                get { return _value.quality; }
                set { _value.quality = value; }
            }

            [ParserTarget("radiusScale")]
            [KittopiaDescription("A multiplier applied to the size of each particle before collisions are processed.")]
            public NumericParser<Single> radiusScale
            {
                get { return _value.radiusScale; }
                set { _value.radiusScale = value; }
            }

            [ParserTarget("sendCollisionMessages")]
            [KittopiaDescription("Send collision callback messages.")]
            public NumericParser<Boolean> sendCollisionMessages
            {
                get { return _value.sendCollisionMessages; }
                set { _value.sendCollisionMessages = value; }
            }

            [ParserTarget("voxelSize")]
            [KittopiaDescription("Size of voxels in the collision cache.")]
            public NumericParser<Single> voxelSize
            {
                get { return _value.voxelSize; }
                set { _value.voxelSize = value; }
            }

            // Create a new module
            [KittopiaConstructor(KittopiaConstructor.Parameter.Empty, purpose = KittopiaConstructor.Purpose.Create)]
            public CollisionModuleLoader()
            {
                _value = new ParticleSystem.CollisionModule
                {
                    mode = ParticleSystemCollisionMode.Collision3D,
                    type = ParticleSystemCollisionType.World
                };
            }

            // Edit an existing module
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public CollisionModuleLoader(ParticleSystem.CollisionModule module)
            {
                _value = module;
            }

            public static implicit operator ParticleSystem.CollisionModule(CollisionModuleLoader loader)
            {
                return loader.Value;
            }

            public static implicit operator CollisionModuleLoader(ParticleSystem.CollisionModule value)
            {
                return new CollisionModuleLoader(value);
            }
        }
    }
}