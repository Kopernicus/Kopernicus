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
        public class CoronaLoader : BaseLoader, IParserEventSubscriber, ITypeParser<SunCoronas>
        {
            // The generated corona
            public SunCoronas Value { get; set; }

            // Material definition for the Corona
            [ParserTarget("scaleSpeed", AllowMerge = true)]
            public NumericParser<Single> scaleSpeed
            {
                get { return Value.scaleSpeed; }
                set { Value.scaleSpeed = value; }
            }

            [ParserTarget("scaleLimitY", AllowMerge = true)]
            public NumericParser<Single> scaleLimitY 
            {
                get { return Value.scaleLimitY; }
                set { Value.scaleLimitY = value; }
            }

            [ParserTarget("scaleLimitX", AllowMerge = true)]
            public NumericParser<Single> scaleLimitX
            {
                get { return Value.scaleLimitX; }
                set { Value.scaleLimitX = value; }
            }

            [ParserTarget("updateInterval", AllowMerge = true)]
            public NumericParser<Single> updateInterval
            {
                get { return Value.updateInterval; }
                set { Value.updateInterval = value; }
            }

            [ParserTarget("speed", AllowMerge = true)]
            public NumericParser<Int32> speed
            {
                get { return Value.Speed; }
                set { Value.Speed = value; }
            }

            [ParserTarget("rotation", AllowMerge = true)]
            public NumericParser<Single> rotation
            {
                get { return Value.Rotation; }
                set { Value.Rotation = value; }
            }

            [ParserTarget("Material", AllowMerge = true, GetChild = false)]
            public ParticleAddSmoothLoader material
            {
                get { return (ParticleAddSmoothLoader)Value.GetComponent<Renderer>().sharedMaterial; }
                set { Value.GetComponent<Renderer>().sharedMaterial = value; }
            }

            [KittopiaDestructor]
            public void Destroy()
            {
                UnityEngine.Object.Destroy(Value.gameObject);
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
                GameObject corona = UnityEngine.Object.Instantiate(sun.scaledVersion.GetComponentsInChildren<SunCoronas>(true).First().gameObject) as GameObject;
                
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
                
                Value = corona.GetComponent<SunCoronas> ();

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
                GameObject corona = UnityEngine.Object.Instantiate(sun.scaledVersion.GetComponentsInChildren<SunCoronas>(true).First().gameObject) as GameObject;
                
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
                
                Value = corona.GetComponent<SunCoronas> ();

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
                Value = component;
                if (!(Value.GetComponent<Renderer>().material is ParticleAddSmoothLoader))
                {
                    material = new ParticleAddSmoothLoader(Value.GetComponent<Renderer>().material);
                }
            }
        }
    }
}

