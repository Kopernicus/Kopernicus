/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class Corona
        {
            // The generated corona
            private SunCoronas coronaComponent;
            public GameObject corona { get; private set; }

            // Material definition for the Corona
            [ParserTarget("scaleSpeed", optional = true, allowMerge = true)]
            private NumericParser<float> scaleSpeed
            {
                set { coronaComponent.scaleSpeed = value.value; }
            }

            [ParserTarget("scaleLimitY", optional = true, allowMerge = true)]
            private NumericParser<float> scaleLimitY 
            {
                set { coronaComponent.scaleLimitY = value.value; }
            }

            [ParserTarget("scaleLimitX", optional = true, allowMerge = true)]
            private NumericParser<float> scaleLimitX
            {
                set { coronaComponent.scaleLimitX = value.value; }
            }

            [ParserTarget("updateInterval", optional = true, allowMerge = true)]
            private NumericParser<float> updateInterval
            {
                set { coronaComponent.updateInterval = value.value; }
            }

            [ParserTarget("speed", optional = true, allowMerge = true)]
            private NumericParser<int> speed
            {
                set { coronaComponent.Speed = value.value; }
            }

            [ParserTarget("rotation", optional = true, allowMerge = true)]
            private NumericParser<float> rotation
            {
                set { coronaComponent.Rotation = value.value; }
            }

            [ParserTarget("Material", optional = true, allowMerge = true)]
            private ParticleAddSmoothLoader material;

            public Corona()
            {
                // We need to get the body for the Sun (to steal it's corona mesh)
                PSystemBody sun = Utility.FindBody (PSystemManager.Instance.systemPrefab.rootBody, "Sun");

                // Clone a default Corona
                corona = GameObject.Instantiate (sun.scaledVersion.GetComponentsInChildren<SunCoronas> (true).First ().gameObject) as GameObject;
                corona.transform.parent = Utility.Deactivator;
                coronaComponent = corona.GetComponent<SunCoronas> ();

                // Setup the material loader
                material = new ParticleAddSmoothLoader (corona.renderer.material);
                material.name = Guid.NewGuid().ToString();
                corona.renderer.sharedMaterial = material;
            }
        }
    }
}

