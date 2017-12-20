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
using Kopernicus.MaterialWrapper;
using Kopernicus.UI;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class CoronaLoader : BaseLoader, IParserEventSubscriber
        {
            // The generated corona
            public SunCoronas coronaComponent;
            public GameObject corona { get; set; }

            // Material definition for the Corona
            [ParserTarget("scaleSpeed", allowMerge = true)]
            public NumericParser<Single> scaleSpeed
            {
                get { return coronaComponent.scaleSpeed; }
                set { coronaComponent.scaleSpeed = value; }
            }

            [ParserTarget("scaleLimitY", allowMerge = true)]
            public NumericParser<Single> scaleLimitY 
            {
                get { return coronaComponent.scaleLimitY; }
                set { coronaComponent.scaleLimitY = value; }
            }

            [ParserTarget("scaleLimitX", allowMerge = true)]
            public NumericParser<Single> scaleLimitX
            {
                get { return coronaComponent.scaleLimitX; }
                set { coronaComponent.scaleLimitX = value; }
            }

            [ParserTarget("updateInterval", allowMerge = true)]
            public NumericParser<Single> updateInterval
            {
                get { return coronaComponent.updateInterval; }
                set { coronaComponent.updateInterval = value; }
            }

            [ParserTarget("speed", allowMerge = true)]
            public NumericParser<Int32> speed
            {
                get { return coronaComponent.Speed; }
                set { coronaComponent.Speed = value; }
            }

            [ParserTarget("rotation", allowMerge = true)]
            public NumericParser<Single> rotation
            {
                get { return coronaComponent.Rotation; }
                set { coronaComponent.Rotation = value; }
            }

            [ParserTarget("Material", allowMerge = true, getChild = false)]
            public ParticleAddSmoothLoader material
            {
                get { return (ParticleAddSmoothLoader)coronaComponent.GetComponent<Renderer>().sharedMaterial; }
                set { coronaComponent.GetComponent<Renderer>().sharedMaterial = value; }
            }

            [KittopiaDestructor]
            public void Destroy()
            {
                UnityEngine.Object.Destroy(corona);
            }

            // Parser apply event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                Events.OnCoronaLoaderApply.Fire(this, node);
            }

            // Parser post apply event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                Events.OnCoronaLoaderPostApply.Fire(this, node);
            }

            /// <summary>
            /// Creates a new Corona Loader from the Injector context.
            /// </summary>
            public CoronaLoader()
            {
                // Is this the parser context?
                if (!Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("Must be executed in Injector context.");
                }
                
                // We need to get the body for the Sun (to steal it's corona mesh)
                PSystemBody sun = Utility.FindBody (Injector.StockSystemPrefab.rootBody, "Sun");

                // Clone a default Corona
                corona = UnityEngine.Object.Instantiate(sun.scaledVersion.GetComponentsInChildren<SunCoronas>(true).First().gameObject) as GameObject;
                
                // Backup local transform parameters 
                Vector3 position = corona.transform.localPosition;
                Vector3 scale = corona.transform.localScale;
                Quaternion rotation = corona.transform.rotation;

                // Parent the new corona
                corona.transform.parent = generatedBody.scaledVersion.transform;

                // Restore the local transform settings
                corona.transform.localPosition = position;
                corona.transform.localScale = scale;
                corona.transform.localRotation = rotation;
                
                coronaComponent = corona.GetComponent<SunCoronas> ();

                // Setup the material loader
                material = new ParticleAddSmoothLoader (corona.GetComponent<Renderer>().material);
                material.name = Guid.NewGuid().ToString();
            }

            /// <summary>
            /// Creates a new Corona Loader for an already existing body.
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.CelestialBody, purpose = KittopiaConstructor.Purpose.Create)]
            public CoronaLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }
                
                // We need to get the body for the Sun (to steal it's corona mesh)
                PSystemBody sun = Utility.FindBody (Injector.StockSystemPrefab.rootBody, "Sun");

                // Clone a default Corona
                corona = UnityEngine.Object.Instantiate(sun.scaledVersion.GetComponentsInChildren<SunCoronas>(true).First().gameObject) as GameObject;
                
                // Backup local transform parameters 
                Vector3 position = corona.transform.localPosition;
                Vector3 scale = corona.transform.localScale;
                Quaternion rotation = corona.transform.rotation;

                // Parent the new corona
                corona.transform.parent = body.scaledBody.transform;

                // Restore the local transform settings
                corona.transform.localPosition = position;
                corona.transform.localScale = scale;
                corona.transform.localRotation = rotation;
                
                coronaComponent = corona.GetComponent<SunCoronas> ();

                // Setup the material loader
                material = new ParticleAddSmoothLoader (corona.GetComponent<Renderer>().material);
                material.name = Guid.NewGuid().ToString();
            }

            /// <summary>
            /// Creates a new Corona Loader from an already existing corona
            /// </summary>
            [KittopiaConstructor(KittopiaConstructor.Parameter.Element, purpose = KittopiaConstructor.Purpose.Edit)]
            public CoronaLoader(SunCoronas component)
            {
                coronaComponent = component;
                corona = component.gameObject;
                if (!(corona.GetComponent<Renderer>().material is ParticleAddSmoothLoader))
                {
                    material = new ParticleAddSmoothLoader(corona.GetComponent<Renderer>().material);
                }
            }
        }
    }
}

