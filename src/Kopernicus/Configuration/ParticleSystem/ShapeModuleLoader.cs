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
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class ShapeModuleLoader : ITypeParser<ParticleSystem.ShapeModule>
        {
            // The module we are editing
            private ParticleSystem.ShapeModule _value;
            public ParticleSystem.ShapeModule Value
            {
                get { return _value; }
                set { _value = value; }
            }

            [ParserTarget("angle")]
            [KittopiaDescription("Angle of the cone.")]
            public NumericParser<Single> angle
            {
                get { return _value.angle; }
                set { _value.angle = value; }
            }

            [ParserTarget("arc")]
            [KittopiaDescription("Circle arc angle.")]
            public NumericParser<Single> arc
            {
                get { return _value.arc; }
                set { _value.arc = value; }
            }

            [ParserTarget("box")]
            [KittopiaDescription("Scale of the box.")]
            public Vector3Parser box
            {
                get { return _value.box; }
                set { _value.box = value; }
            }

            [ParserTarget("length")]
            [KittopiaDescription("Length of the cone.")]
            public NumericParser<Single> length
            {
                get { return _value.length; }
                set { _value.length = value; }
            }

            [ParserTarget("mesh")]
            [KittopiaDescription("Mesh to emit particles from.")]
            public MeshParser mesh
            {
                get { return _value.mesh; }
                set { _value.mesh = value; }
            }

            [ParserTarget("meshMaterialIndex")]
            [KittopiaDescription("Emit particles from a single material of a mesh.")]
            public NumericParser<Int32> meshMaterialIndex
            {
                get { return _value.meshMaterialIndex; }
                set { _value.meshMaterialIndex = value; }
            }
            
            [ParserTarget("enabled")]
            [KittopiaDescription("Enable/disable the Shape module.")]
            public NumericParser<Boolean> enabled
            {
                get { return _value.enabled; }
                set { _value.enabled = value; }
            }

            // Create a new module
            [KittopiaConstructor(KittopiaConstructor.Parameter.Empty, purpose = KittopiaConstructor.Purpose.Create)]
            public ShapeModuleLoader()
            {
                _value = new ParticleSystem.ShapeModule();
            }

            // Edit an existing module
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public ShapeModuleLoader(ParticleSystem.ShapeModule module)
            {
                _value = module;
            }

            public static implicit operator ParticleSystem.ShapeModule(ShapeModuleLoader loader)
            {
                return loader.Value;
            }

            public static implicit operator ShapeModuleLoader(ParticleSystem.ShapeModule value)
            {
                return new ShapeModuleLoader(value);
            }
        }
    }
}