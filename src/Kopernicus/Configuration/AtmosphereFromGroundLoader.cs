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
using System.Linq;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;
using Kopernicus.Constants;
using Kopernicus.RuntimeUtility;
using Kopernicus.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kopernicus.Configuration
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class AtmosphereFromGroundLoader : BaseLoader, IParserEventSubscriber, ITypeParser<AtmosphereFromGround>
    {
        /// <summary>
        /// The scale factor between ScaledSpace and LocalSpace
        /// </summary>
        private const Single INVSCALEFACTOR = 1f / 6000f;

        /// <summary>
        /// AtmosphereFromGround we're modifying
        /// </summary>
        public AtmosphereFromGround Value { get; set; }

        // DEBUG_alwaysUpdateAll
        [ParserTarget("DEBUG_alwaysUpdateAll")]
        [KittopiaDescription("Whether all parameters should get recalculated and reapplied every frame.")]
        public NumericParser<Boolean> DebugAlwaysUpdateAll
        {
            get { return Value.DEBUG_alwaysUpdateAll; }
            set
            {
                Value.DEBUG_alwaysUpdateAll = value;
                if (Injector.IsInPrefab)
                {
                    return;
                }
                CalculatedMembers(Value);
                AtmosphereInfo.StoreAfg(Value);
                AtmosphereInfo.PatchAfg(Value);
            }
        }

        // doScale
        [ParserTarget("doScale")]
        [KittopiaDescription("Whether the atmosphere mesh should be scaled automatically.")]
        public NumericParser<Boolean> DoScale
        {
            get { return Value.doScale; }
            set
            {
                Value.doScale = value;
                if (Injector.IsInPrefab)
                {
                    return;
                }
                CalculatedMembers(Value);
                AtmosphereInfo.StoreAfg(Value);
                AtmosphereInfo.PatchAfg(Value);
            }
        }

        // innerRadius
        [ParserTarget("innerRadius")]
        [KittopiaDescription("The lower bound of the atmosphere effect.")]
        public NumericParser<Single> InnerRadius
        {
            get { return Value.innerRadius / INVSCALEFACTOR; }
            set
            {
                Value.innerRadius = value * INVSCALEFACTOR;
                if (Injector.IsInPrefab)
                {
                    return;
                }
                CalculatedMembers(Value);
                AtmosphereInfo.StoreAfg(Value);
                AtmosphereInfo.PatchAfg(Value);
            }
        }

        // invWaveLength
        [ParserTarget("invWaveLength")]
        public ColorParser InvWaveLength
        {
            get { return Value.invWaveLength; }
            set
            {
                Value.invWaveLength = value;
                Value.waveLength = new Color((Single) Math.Sqrt(Math.Sqrt(1d / Value.invWaveLength[0])),
                    (Single) Math.Sqrt(Math.Sqrt(1d / Value.invWaveLength[1])),
                    (Single) Math.Sqrt(Math.Sqrt(1d / Value.invWaveLength[2])), 0.5f);
                if (Injector.IsInPrefab)
                {
                    return;
                }
                CalculatedMembers(Value);
                AtmosphereInfo.StoreAfg(Value);
                AtmosphereInfo.PatchAfg(Value);
            }
        }

        // outerRadius
        [ParserTarget("outerRadius")]
        [KittopiaDescription("The upper bound of the atmosphere effect.")]
        public NumericParser<Single> OuterRadius
        {
            get { return Value.outerRadius / INVSCALEFACTOR; }
            set
            {
                Value.outerRadius = value * INVSCALEFACTOR;
                if (Injector.IsInPrefab)
                {
                    return;
                }
                CalculatedMembers(Value);
                AtmosphereInfo.StoreAfg(Value);
                AtmosphereInfo.PatchAfg(Value);
            }
        }

        // samples
        [ParserTarget("samples")]
        public NumericParser<Single> Samples
        {
            get { return Value.samples; }
            set
            {
                Value.samples = value;
                if (Injector.IsInPrefab)
                {
                    return;
                }
                CalculatedMembers(Value);
                AtmosphereInfo.StoreAfg(Value);
                AtmosphereInfo.PatchAfg(Value);
            }
        }

        [ParserTarget("transformScale")]
        [KittopiaDescription(
            "The scale of the atmosphere mesh in all three directions. Automatically set if doScale is enabled.")]
        public Vector3Parser TransformScale
        {
            get { return Value.transform.localScale; }
            set
            {
                Value.transform.localScale = value;
                Value.doScale = false;
                if (Injector.IsInPrefab)
                {
                    return;
                }
                CalculatedMembers(Value);
                AtmosphereInfo.StoreAfg(Value);
                AtmosphereInfo.PatchAfg(Value);
            }
        }

        // waveLength
        [ParserTarget("waveLength")]
        public ColorParser WaveLength
        {
            get { return Value.waveLength; }
            set
            {
                Value.waveLength = value;
                Value.invWaveLength = new Color((Single) (1d / Math.Pow(Value.waveLength[0], 4)),
                    (Single) (1d / Math.Pow(Value.waveLength[1], 4)),
                    (Single) (1d / Math.Pow(Value.waveLength[2], 4)), 0.5f);
                if (Injector.IsInPrefab)
                {
                    return;
                }
                CalculatedMembers(Value);
                AtmosphereInfo.StoreAfg(Value);
                AtmosphereInfo.PatchAfg(Value);
            }
        }

        // outerRadiusMult
        [ParserTarget("outerRadiusMult")]
        [KittopiaDescription("A multiplier that automatically sets outerRadius based on the planets radius.")]
        public NumericParser<Single> OuterRadiusMult
        {
            get { return Value.outerRadius / INVSCALEFACTOR / (Single) Value.planet.Radius; }
            set
            {
                Value.outerRadius = (Single) Value.planet.Radius * value * INVSCALEFACTOR;
                if (Injector.IsInPrefab)
                {
                    return;
                }
                CalculatedMembers(Value);
                AtmosphereInfo.StoreAfg(Value);
                AtmosphereInfo.PatchAfg(Value);
            }
        }

        // innerRadiusMult
        [ParserTarget("innerRadiusMult")]
        [KittopiaDescription("A multiplier that automatically sets innerRadius based on the planets radius.")]
        public NumericParser<Single> InnerRadiusMult
        {
            get { return Value.innerRadius / Value.outerRadius; }
            set
            {
                Value.innerRadius = Value.outerRadius * value;
                if (Injector.IsInPrefab)
                {
                    return;
                }
                CalculatedMembers(Value);
                AtmosphereInfo.StoreAfg(Value);
                AtmosphereInfo.PatchAfg(Value);
            }
        }

        /// <summary>
        /// Removes the atmosphere from ground
        /// </summary>
        [KittopiaDestructor]
        public void Destroy()
        {
            // Remove the Atmosphere from Ground
            Value.planet.afg = null;
            AtmosphereInfo.Atmospheres.Remove(Value.planet.transform.name);
            AtmosphereFromGround[] atmospheres = Value.transform.parent.GetComponentsInChildren<AtmosphereFromGround>();
            foreach (AtmosphereFromGround afg in atmospheres)
            {
                Object.Destroy(afg.gameObject);
            }

            // Disable the Light controller
            MaterialSetDirection[] materialSetDirections =
                Value.transform.parent.GetComponentsInChildren<MaterialSetDirection>();
            foreach (MaterialSetDirection msd in materialSetDirections)
            {
                Object.Destroy(msd);
            }
        }

        /// <summary>
        /// Set default values for the AtmosphereFromGround
        /// </summary>
        [KittopiaAction("Set Default Values")]
        [KittopiaDescription("Sets stock values for the AtmosphereFromGround")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public void SetDefaultValues()
        {
            // Set Defaults
            Value.ESun = 30f;
            Value.Kr = 0.00125f;
            Value.Km = 0.00015f;
            Value.samples = 4f;
            Value.g = -0.85f;
            if (Value.waveLength == new Color(0f, 0f, 0f, 0f))
            {
                Value.waveLength = new Color(0.65f, 0.57f, 0.475f, 0.5f);
            }

            Value.outerRadius = (Single) Value.planet.Radius * 1.025f * INVSCALEFACTOR;
            Value.innerRadius = Value.outerRadius * 0.975f;
            Value.scaleDepth = -0.25f;
            Value.invWaveLength = new Color((Single) (1d / Math.Pow(Value.waveLength[0], 4)),
                (Single) (1d / Math.Pow(Value.waveLength[1], 4)), (Single) (1d / Math.Pow(Value.waveLength[2], 4)),
                0.5f);
            CalculatedMembers(Value);
        }

        /// <summary>
        /// Calculates the default members for the AFG
        /// </summary>
        public static void CalculatedMembers(AtmosphereFromGround afg)
        {
            afg.g2 = afg.g * afg.g;
            afg.KrESun = afg.Kr * afg.ESun;
            afg.KmESun = afg.Km * afg.ESun;
            afg.Kr4PI = afg.Kr * 4f * Mathf.PI;
            afg.Km4PI = afg.Km * 4f * Mathf.PI;
            afg.outerRadius2 = afg.outerRadius * afg.outerRadius;
            afg.innerRadius2 = afg.innerRadius * afg.innerRadius;
            afg.scale = 1f / (afg.outerRadius - afg.innerRadius);
            afg.scaleOverScaleDepth = afg.scale / afg.scaleDepth;

            if (afg.doScale)
            {
                afg.transform.localScale = Vector3.one * 1.025f;
            }
        }

        // Parser apply event
        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            // Set defaults
            SetDefaultValues();

            // Fire event
            Events.OnAFGLoaderApply.Fire(this, node);
        }

        // Parser post apply event
        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
            // Recalculate with the new values and store
            CalculatedMembers(Value);
            AtmosphereInfo.StoreAfg(Value);

            // Fire event
            Events.OnAFGLoaderPostApply.Fire(this, node);
        }

        /// <summary>
        /// Creates a new AtmosphereFromGround Loader from the Injector context.
        /// </summary>
        public AtmosphereFromGroundLoader()
        {
            // Is this the parser context?
            if (generatedBody == null)
            {
                throw new InvalidOperationException("Must be executed in Injector context.");
            }

            // Store values
            Value = generatedBody.scaledVersion.GetComponentsInChildren<AtmosphereFromGround>(true)
                ?.FirstOrDefault();

            if (Value == null)
            {
                // Add the material light direction behavior
                MaterialSetDirection materialLightDirection =
                    generatedBody.scaledVersion.AddOrGetComponent<MaterialSetDirection>();
                materialLightDirection.valueName = "_localLightDirection";

                // Create the atmosphere shell game object
                GameObject scaledAtmosphere = new GameObject("Atmosphere");
                scaledAtmosphere.transform.parent = generatedBody.scaledVersion.transform;
                scaledAtmosphere.layer = GameLayers.SCALED_SPACE_ATMOSPHERE;
                MeshRenderer renderer = scaledAtmosphere.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = new Components.MaterialWrapper.AtmosphereFromGround();
                MeshFilter meshFilter = scaledAtmosphere.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = Templates.ReferenceGeosphere;
                Value = scaledAtmosphere.AddComponent<AtmosphereFromGround>();
            }

            Value.planet = generatedBody.celestialBody;
        }

        /// <summary>
        /// Creates a new AtmosphereFromGround Loader from a spawned CelestialBody.
        /// </summary>
        [KittopiaConstructor(KittopiaConstructor.ParameterType.CelestialBody)]
        public AtmosphereFromGroundLoader(CelestialBody body)
        {
            // Is this a spawned body?
            if (body.scaledBody == null)
            {
                throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
            }

            // Store values
            Value = body.afg;

            if (Value == null)
            {
                // Add the material light direction behavior
                MaterialSetDirection materialLightDirection = body.scaledBody.AddComponent<MaterialSetDirection>();
                materialLightDirection.valueName = "_localLightDirection";

                // Create the atmosphere shell game object
                GameObject scaledAtmosphere = new GameObject("Atmosphere");
                scaledAtmosphere.transform.parent = body.scaledBody.transform;
                scaledAtmosphere.layer = GameLayers.SCALED_SPACE_ATMOSPHERE;
                MeshRenderer renderer = scaledAtmosphere.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = new Components.MaterialWrapper.AtmosphereFromGround();
                MeshFilter meshFilter = scaledAtmosphere.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = Templates.ReferenceGeosphere;
                Value = body.afg = scaledAtmosphere.AddComponent<AtmosphereFromGround>();
                Value.planet = body;
                Value.sunLight = Sun.Instance.gameObject;
                Value.mainCamera = ScaledCamera.Instance.transform;
                AtmosphereInfo.StoreAfg(Value);
                AtmosphereInfo.PatchAfg(Value);

                // Set defaults
                SetDefaultValues();
            }

            Value.planet = body;
        }
    }
}