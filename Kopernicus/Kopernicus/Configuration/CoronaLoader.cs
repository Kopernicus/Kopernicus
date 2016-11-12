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
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class CoronaLoader : BaseLoader
        {
            // The generated corona
            public SunCoronas coronaComponent;
            public GameObject corona { get; set; }

            // Material definition for the Corona
            [ParserTarget("scaleSpeed", allowMerge = true)]
            public NumericParser<float> scaleSpeed
            {
                get { return coronaComponent.scaleSpeed; }
                set { coronaComponent.scaleSpeed = value; }
            }

            [ParserTarget("scaleLimitY", allowMerge = true)]
            public NumericParser<float> scaleLimitY 
            {
                get { return coronaComponent.scaleLimitY; }
                set { coronaComponent.scaleLimitY = value; }
            }

            [ParserTarget("scaleLimitX", allowMerge = true)]
            public NumericParser<float> scaleLimitX
            {
                get { return coronaComponent.scaleLimitX; }
                set { coronaComponent.scaleLimitX = value; }
            }

            [ParserTarget("updateInterval", allowMerge = true)]
            public NumericParser<float> updateInterval
            {
                get { return coronaComponent.updateInterval; }
                set { coronaComponent.updateInterval = value; }
            }

            [ParserTarget("speed", allowMerge = true)]
            public NumericParser<int> speed
            {
                get { return coronaComponent.Speed; }
                set { coronaComponent.Speed = value; }
            }

            [ParserTarget("rotation", allowMerge = true)]
            public NumericParser<float> rotation
            {
                get { return coronaComponent.Rotation; }
                set { coronaComponent.Rotation = value; }
            }

            [ParserTarget("Material", allowMerge = true, getChild = false)]
            public ParticleAddSmoothLoader material
            {
                get { return new ParticleAddSmoothLoader(coronaComponent.GetComponent<Renderer>().sharedMaterial); }
                set { coronaComponent.GetComponent<Renderer>().sharedMaterial = new Material(value); }
            }

            // Default constructor
            public CoronaLoader()
            {
                // We need to get the body for the Sun (to steal it's corona mesh)
                PSystemBody sun = Utility.FindBody (PSystemManager.Instance.systemPrefab.rootBody, "Sun");

                // Clone a default Corona
                corona = UnityEngine.Object.Instantiate(sun.scaledVersion.GetComponentsInChildren<SunCoronas>(true).First().gameObject) as GameObject;
                corona.transform.parent = Utility.Deactivator;
                coronaComponent = corona.GetComponent<SunCoronas> ();

                // Setup the material loader
                material = new ParticleAddSmoothLoader (corona.GetComponent<Renderer>().material);
                material.name = Guid.NewGuid().ToString();
            }

            // Runtime constructor
            public CoronaLoader(SunCoronas component)
            {
                coronaComponent = component;
                corona = component.gameObject;
            }
        }
    }
}

