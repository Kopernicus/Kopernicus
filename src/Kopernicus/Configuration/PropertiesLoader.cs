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

using Kopernicus.Components;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;
using Kopernicus.OnDemand;
using Kopernicus.UI;
using KSPAchievements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Kopernicus.Configuration
{
    // Edit the CelestialBody component
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class PropertiesLoader : BaseLoader, IParserEventSubscriber, ITypeParser<CelestialBody>
    {
        // How visible should the planet be in the science archives
        public enum RnDVisibility
        {
            Visible,
            Noicon,
            Hidden,
            Skip
        }

        // Celestial body to edit
        public CelestialBody Value { get; set; }

        // Body description
        [ParserTarget("description")]
        public String Description
        {
            get { return Value.bodyDescription; }
            set { Value.bodyDescription = value; }
        }

        // Radius
        [ParserTarget("radius")]
        public NumericParser<Double> Radius
        {
            get { return Value.Radius; }
            set { Value.Radius = value; }
        }

        // GeeASL
        [ParserTarget("geeASL")]
        public NumericParser<Double> GeeAsl
        {
            get { return Value.GeeASL; }
            set
            {
                Value.GeeASL = value;
                GeeAslToOthers();
            }
        }

        // Mass
        [ParserTarget("mass")]
        public NumericParser<Double> Mass
        {
            get { return Value.Mass; }
            set
            {
                Value.Mass = value;
                MassToOthers();
            }
        }

        // Grav Param
        [ParserTarget("gravParameter")]
        public NumericParser<Double> GravParameter
        {
            get { return Value.gravParameter; }
            set
            {
                Value.gMagnitudeAtCenter = Value.gravParameter = value;
                GravParamToOthers();
            }
        }

        // Does the body rotate?
        [ParserTarget("rotates")]
        public NumericParser<Boolean> Rotates
        {
            get { return Value.rotates; }
            set { Value.rotates = value; }
        }

        // Rotation period of the world
        [ParserTarget("rotationPeriod")]
        public NumericParser<Double> RotationPeriod
        {
            get { return Value.rotationPeriod; }
            set { Value.rotationPeriod = value; }
        }

        // Is the body tidally locked to its parent?
        [ParserTarget("tidallyLocked")]
        public NumericParser<Boolean> TidallyLocked
        {
            get { return Value.tidallyLocked; }
            set { Value.tidallyLocked = value; }
        }

        // Initial rotation of the world
        [ParserTarget("initialRotation")]
        public NumericParser<Double> InitialRotation
        {
            get { return Value.initialRotation; }
            set { Value.initialRotation = value; }
        }

        // Altitude where the Game switches the reference frame
        [ParserTarget("inverseRotThresholdAltitude")]
        public NumericParser<Single> InverseRotThresholdAltitude
        {
            get { return Value.inverseRotThresholdAltitude; }
            set { Value.inverseRotThresholdAltitude = value; }
        }

        // albedo
        [ParserTarget("albedo")]
        public NumericParser<Double> Albedo
        {
            get { return Value.albedo; }
            set { Value.albedo = value; }
        }

        // emissivity
        [ParserTarget("emissivity")]
        public NumericParser<Double> Emissivity
        {
            get { return Value.emissivity; }
            set { Value.emissivity = value; }
        }

        // coreTemperatureOffset
        [ParserTarget("coreTemperatureOffset")]
        public NumericParser<Double> CoreTemperatureOffset
        {
            get { return Value.coreTemperatureOffset; }
            set { Value.coreTemperatureOffset = value; }
        }

        // Time warp altitude limits
        [ParserTarget("timewarpAltitudeLimits")]
        public NumericCollectionParser<Single> TimewarpAltitudeLimits
        {
            get
            {
                return Value.timeWarpAltitudeLimits != null
                    ? Value.timeWarpAltitudeLimits.ToList()
                    : new List<Single>(0);
            }
            set { Value.timeWarpAltitudeLimits = value.Value.ToArray(); }
        }

        // Sphere of Influence
        [ParserTarget("sphereOfInfluence")]
        public NumericParser<Double> SphereOfInfluence
        {
            get { return Value.Get("sphereOfInfluence", Value.sphereOfInfluence); }
            set
            {
                Value.sphereOfInfluence = value;
                Value.Set("sphereOfInfluence", value.Value);
            }
        }

        // Hill Sphere
        // [ParserTarget("hillSphere")]
        // public NumericParser<Double> hillSphere
        // {
        //     get { return Value.Get("hillSphere", Value.hillSphere); }
        //     set { Value.hillSphere = value; Value.Set("hillSphere", value.Value); }
        // }

        // solarRotationPeriod
        [ParserTarget("solarRotationPeriod")]
        public NumericParser<Boolean> SolarRotationPeriod
        {
            get { return Value.solarRotationPeriod; }
            set { Value.solarRotationPeriod = value; }
        }

        // navballSwitchRadiusMult
        [ParserTarget("navballSwitchRadiusMult")]
        public NumericParser<Double> NavballSwitchRadiusMult
        {
            get { return Value.navballSwitchRadiusMult; }
            set { Value.navballSwitchRadiusMult = value; }
        }

        // navballSwitchRadiusMult
        [ParserTarget("navballSwitchRadiusMultLow")]
        public NumericParser<Double> NavballSwitchRadiusMultLow
        {
            get { return Value.navballSwitchRadiusMultLow; }
            set { Value.navballSwitchRadiusMultLow = value; }
        }

        // Science values of this body
        [ParserTarget("ScienceValues", AllowMerge = true)]
        [KittopiaUntouchable]
        public ScienceValuesLoader ScienceValues { get; set; }

        // Biomes of this body
        [ParserTargetCollection("Biomes", NameSignificance = NameSignificance.None, AllowMerge = true)]
        public List<BiomeLoader> Biomes
        {
            get
            {
                // runtime stuff for kittopia
                if (!Injector.IsInPrefab && _biomes == null)
                {
                    if (Value.BiomeMap == null)
                        return null;

                    List<BiomeLoader> runtimeBiomes = new List<BiomeLoader>(Value.BiomeMap.Attributes.Length);
                    for (int i = 0; i < Value.BiomeMap.Attributes.Length; i++)
                        runtimeBiomes.Add(new BiomeLoader(Value.BiomeMap.Attributes[i]));
                    return runtimeBiomes;
                }

                // real value from config
                return _biomes;
            }

            set => _biomes = value;
        }

        private List<BiomeLoader> _biomes;

        // Biome texture path, or "BUILTIN/stock_map" for stock bodies biome maps
        [ParserTarget("biomeMap")]
        public string BiomeMap
        {
            get
            {
                // runtime stuff for kittopia
                if (!Injector.IsInPrefab && _biomeMap == null)
                {
                    string runtimeName = Value.BiomeMap?.MapName;
                    if (runtimeName == null)
                        return null;

                    if (GameDatabase.Instance.ExistsTexture(runtimeName) || OnDemandStorage.TextureExists(runtimeName))
                        return runtimeName;

                    return "BUILTIN/" + runtimeName;
                }

                // real value from config
                return _biomeMap;
            }

            set => _biomeMap = value;
        }

        private string _biomeMap;

        // If the body name should be prefixed with "the" in some situations
        [ParserTarget("useTheInName")]
        public NumericParser<Boolean> UseTheInName
        {
            get { return Value.bodyDisplayName.StartsWith("The", StringComparison.InvariantCultureIgnoreCase); }
            set
            {
                if (value)
                {
                    Value.bodyDisplayName = "The " + Value.bodyName;
                }
                else
                {
                    Value.bodyDisplayName = Value.bodyName;
                }
            }
        }

        [ParserTarget("displayName")]
        public String DisplayName
        {
            get { return Value.bodyDisplayName; }
            set { Value.bodyDisplayName = Value.bodyAdjectiveDisplayName = value; }
        }

        // If the body should be unselectable
        [ParserTarget("selectable")]
        public NumericParser<Boolean> Selectable
        {
            get { return Value.Get("selectable", true); }
            set
            {
                Value.Set("selectable", value.Value);
                if (!Injector.IsInPrefab)
                {
                    PlanetariumCamera.fetch.targets = Templates.MapTargets.Where(m =>
                        m.celestialBody != null && m.celestialBody.Get("selectable", true)).ToList();
                }
            }
        }

        // If the body should be hidden in RnD
        [ParserTarget("RnDVisibility")]
        [ParserTarget("RDVisibility")]
        public EnumParser<RnDVisibility> HiddenRnD
        {
            get { return Value.Get("hiddenRnD", RnDVisibility.Visible); }
            set { Value.Set("hiddenRnD", value.Value); }
        }

        // If the body should rotate in RnD
        [ParserTarget("RnDRotation")]
        public NumericParser<Boolean> RnDRotation
        {
            get
            {
                return Value.Get("RnDRotation",
                    Value.scaledBody.GetComponentInChildren<SunCoronas>(true) != null);
            }
            set { Value.Set("RnDRotation", value.Value); }
        }

        // Max Zoom limit for TrackingStation and MapView
        // set the number of meters that can fit in the full height of the screen
        [ParserTarget("maxZoom")]
        public NumericParser<Single> MinDistance
        {
            get { return Value.Get("maxZoom", 10 * 6000f); }
            set { Value.Set("maxZoom", value.Value / 6000f); }
        }

        // Apply Event
        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            // Event
            Events.OnPropertiesLoaderApply.Fire(this, node);
        }

        // PostApply Event
        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
            Stopwatch watch = Stopwatch.StartNew();
            Value.BiomeMap = ParseBiomeMapAndBiomeDefinitions();
            if (Value.BiomeMap != null)
                Logger.Active.Log($"Processed '{Value.BiomeMap.MapName}' in {watch.Elapsed.TotalMilliseconds:F3}ms");

                // Debug the fields (TODO - remove)
            Utility.DumpObjectFields(Value.scienceValues, " Science Values ");

            // TODO - tentative fix, needs to be able to be configured (if it can be?)
            if (Value.progressTree == null)
            {
                Value.progressTree = new CelestialBodySubtree(Value);
                Logger.Active.Log("Added Progress Tree");
            }

            // Event
            Events.OnPropertiesLoaderPostApply.Fire(this, node);
        }

        /// <summary>
        /// Create the fast lookup biome map for this body. Returns null if the body doesn't have
        /// a biome map.
        /// </summary>
        /// <remarks>
        /// This is done in post-apply because we need the list of biome definitions to be able
        /// to parse the texture (which is converted to a lookup table of biome indices), and
        /// I've no idea how to have the parser access another, plus we want to convert the stock
        /// biome maps anyway.
        /// </remarks>
        private CBAttributeMapSO ParseBiomeMapAndBiomeDefinitions()
        {
            CBAttributeMapSO.MapAttribute[] mapAttributes;
            if (_biomes != null && _biomes.Count > 0)
            {
                mapAttributes = new CBAttributeMapSO.MapAttribute[_biomes.Count];
                for (int i = 0; i < _biomes.Count; i++)
                {
                    CBAttributeMapSO.MapAttribute biome = _biomes[i].Value;
                    Logger.Active.Log("Configured biome: " + biome.name + " : " + biome.mapColor + " : " + biome.value);
                    mapAttributes[i] = biome;
                }
            }
            else
            {
                mapAttributes = null;
            }

            KopernicusCBAttributeMapSO kopernicusBiomeMap = ScriptableObject.CreateInstance<KopernicusCBAttributeMapSO>(); ;

            if (string.IsNullOrEmpty(_biomeMap))
            {
                if (Value.BiomeMap == null)
                    return null;

                // convert stock biome maps to our fast lookup derivative
                if (Value.BiomeMap.GetType() == typeof(CBAttributeMapSO) && !kopernicusBiomeMap.ConvertFromStockMap(Value.BiomeMap, mapAttributes))
                {
                    Logger.Active.Log($"Failed to optimize stock biome map '{_biomeMap}'");
                    return Value.BiomeMap;
                }
            }
            else if (_biomeMap.StartsWith("BUILTIN/"))
            {
                CBAttributeMapSO templateMap = Utility.FindMapSO<CBAttributeMapSO>(BiomeMap.Substring("BUILTIN/".Length));
                if (templateMap == null)
                {
                    Logger.Active.Log($"The BUILTIN biome map '{_biomeMap}' doesn't exists");
                    return null;
                }

                if (!kopernicusBiomeMap.ConvertFromStockMap(templateMap, mapAttributes))
                {
                    Logger.Active.Log($"Failed to convert BUILTIN biome map '{_biomeMap}'");
                    return null;
                }
            }
            else
            {
                if (mapAttributes == null)
                {
                    Logger.Active.Log($"Failed to create biomeMap '{_biomeMap}', no biomes defined");
                    return null;
                }

                Texture2D texture = Utility.LoadTexture(_biomeMap, false, false, false);
                if (texture == null)
                {
                    Logger.Active.Log($"Failed to load biomeMap '{_biomeMap}'");
                    return null;
                }

                kopernicusBiomeMap.CreateMapWithAttributes(texture, mapAttributes);
                UnityEngine.Object.DestroyImmediate(texture);
            }

            return kopernicusBiomeMap;
        }

        /// <summary>
        /// Creates a new Properties Loader from the Injector context.
        /// </summary>
        public PropertiesLoader()
        {
            // Is this the parser context?
            if (!Injector.IsInPrefab)
            {
                throw new InvalidOperationException("Must be executed in Injector context.");
            }

            // Store values
            Value = generatedBody.celestialBody;

            // We require a science values object
            if (Value.scienceValues == null)
            {
                Value.scienceValues = new CelestialBodyScienceParams();
            }
            ScienceValues = new ScienceValuesLoader(Value.scienceValues);
            // isHomeWorld Check
            Value.isHomeWorld = Value.transform.name == RuntimeUtility.RuntimeUtility.KopernicusConfig.HomeWorldName;
        }

        /// <summary>
        /// Creates a new Properties Loader from a spawned CelestialBody.
        /// </summary>
        public PropertiesLoader(CelestialBody body)
        {
            // Is this a spawned body?
            if (body.scaledBody == null || Injector.IsInPrefab)
            {
                throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
            }

            // Store values
            Value = body;

            // We require a science values object
            if (Value.scienceValues == null)
            {
                Value.scienceValues = new CelestialBodyScienceParams();
            }
            ScienceValues = new ScienceValuesLoader(Value.scienceValues);

            // isHomeWorld Check
            Value.isHomeWorld = Value.transform.name == RuntimeUtility.RuntimeUtility.KopernicusConfig.HomeWorldName;
        }

        // Mass converters
        private void GeeAslToOthers()
        {
            Double rsq = Value.Radius;
            rsq *= rsq;
            Value.gMagnitudeAtCenter = Value.GeeASL * 9.80665 * rsq;
            Value.gravParameter = Value.gMagnitudeAtCenter;
            Value.Mass = Value.gravParameter * (1 / 6.67408E-11);
            Logger.Active.Log("Via surface G, set gravParam to " + Value.gravParameter + ", mass to " + Value.Mass);
        }

        // Converts mass to Gee ASL using a body's radius.
        private void MassToOthers()
        {
            Double rsq = Value.Radius;
            rsq *= rsq;
            Value.GeeASL = Value.Mass * (6.67408E-11 / 9.80665) / rsq;
            Value.gMagnitudeAtCenter = Value.GeeASL * 9.80665 * rsq;
            Value.gravParameter = Value.gMagnitudeAtCenter;
            Logger.Active.Log("Via mass, set gravParam to " + Value.gravParameter + ", surface G to " + Value.GeeASL);
        }

        // Convert gravParam to mass and GeeASL
        private void GravParamToOthers()
        {
            Double rsq = Value.Radius;
            rsq *= rsq;
            Value.Mass = Value.gravParameter * (1 / 6.67408E-11);
            Value.GeeASL = Value.gravParameter / 9.80665 / rsq;
            Value.gMagnitudeAtCenter = Value.gravParameter;
            Logger.Active.Log("Via gravParam, set mass to " + Value.Mass + ", surface G to " + Value.GeeASL);
        }
    }
}
