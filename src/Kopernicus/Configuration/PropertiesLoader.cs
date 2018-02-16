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
using System.Linq;

namespace Kopernicus
{
    namespace Configuration
    {
        // Edit the CelestialBody component
        [RequireConfigType(ConfigType.Node)]
        public class PropertiesLoader : BaseLoader, IParserEventSubscriber, ITypeParser<CelestialBody>
        {
            // How visible should the planet be in the science archives
            public enum RDVisibility
            {
                VISIBLE,
                NOICON,
                HIDDEN,
                SKIP
            }
            
            // Celestial body to edit
            public CelestialBody Value { get; set; }

            // Body description
            [ParserTarget("description")]
            public String description
            {
                get { return Value.bodyDescription; }
                set { Value.bodyDescription = value; }
            }

            // Radius
            [ParserTarget("radius")]
            public NumericParser<Double> radius
            {
                get { return Value.Radius; }
                set { Value.Radius = value; }
            }

            // GeeASL
            [ParserTarget("geeASL")]
            public NumericParser<Double> geeASL
            {
                get { return Value.GeeASL; }
                set { Value.GeeASL = value; }
            }

            // Mass
            [ParserTarget("mass")]
            public NumericParser<Double> mass
            {
                get { return Value.Mass; }
                set { Value.Mass = value; _hasMass = true; }
            }
            private Boolean _hasMass = false;

            // Grav Param
            [ParserTarget("gravParameter")]
            public NumericParser<Double> gravParameter
            {
                get { return Value.gravParameter; }
                set { Value.gMagnitudeAtCenter = Value.gravParameter = value; _hasGravParam = true; }
            }
            private Boolean _hasGravParam = false;

            // Does the body rotate?
            [ParserTarget("rotates")]
            public NumericParser<Boolean> rotates
            {
                get { return Value.rotates; }
                set { Value.rotates = value; }
            }

            // Rotation period of the world
            [ParserTarget("rotationPeriod")]
            public NumericParser<Double> rotationPeriod
            {
                get { return Value.rotationPeriod; }
                set { Value.rotationPeriod = value; }
            }

            // Is the body tidally locked to its parent?
            [ParserTarget("tidallyLocked")]
            public NumericParser<Boolean> tidallyLocked
            {
                get { return Value.tidallyLocked; }
                set { Value.tidallyLocked = value; }
            }

            // Initial rotation of the world
            [ParserTarget("initialRotation")]
            public NumericParser<Double> initialRotation
            {
                get { return Value.initialRotation; }
                set { Value.initialRotation = value; }
            }

            // Altitude where the Game switches the reference frame
            [ParserTarget("inverseRotThresholdAltitude")]
            public NumericParser<Single> inverseRotThresholdAltitude
            {
                get { return Value.inverseRotThresholdAltitude; }
                set { Value.inverseRotThresholdAltitude = value; }
            }

            // albedo
            [ParserTarget("albedo")]
            public NumericParser<Double> albedo
            {
                get { return Value.albedo; }
                set { Value.albedo = value; }
            }

            // emissivity
            [ParserTarget("emissivity")]
            public NumericParser<Double> emissivity
            {
                get { return Value.emissivity; }
                set { Value.emissivity = value; }
            }

            // coreTemperatureOffset
            [ParserTarget("coreTemperatureOffset")]
            public NumericParser<Double> coreTemperatureOffset
            {
                get { return Value.coreTemperatureOffset; }
                set { Value.coreTemperatureOffset = value; }
            }

            // Time warp altitude limits
            [ParserTarget("timewarpAltitudeLimits")]
            public NumericCollectionParser<Single> timewarpAltitudeLimits
            {
                get { return Value.timeWarpAltitudeLimits != null ? Value.timeWarpAltitudeLimits.ToList() : new List<Single>(0); }
                set { Value.timeWarpAltitudeLimits = value.Value.ToArray(); }
            }

            // Sphere of Influence
            [ParserTarget("sphereOfInfluence")]
            public NumericParser<Double> sphereOfInfluence
            {
                get { return Value.Get("sphereOfInfluence", Value.sphereOfInfluence); }
                set { Value.sphereOfInfluence = value; Value.Set("sphereOfInfluence", value.Value); }
            }

            // Hill Sphere
            [ParserTarget("hillSphere")]
            public NumericParser<Double> hillSphere
            {
                get { return Value.Get("hillSphere", Value.hillSphere); }
                set { Value.hillSphere = value; Value.Set("hillSphere", value.Value); }
            }

            // solarRotationPeriod
            [ParserTarget("solarRotationPeriod")]
            public NumericParser<Boolean> solarRotationPeriod
            {
                get { return Value.solarRotationPeriod; }
                set { Value.solarRotationPeriod = value; }
            }

            // navballSwitchRadiusMult
            [ParserTarget("navballSwitchRadiusMult")]
            public NumericParser<Double> navballSwitchRadiusMult
            {
                get { return Value.navballSwitchRadiusMult; }
                set { Value.navballSwitchRadiusMult = value; }
            }
            // navballSwitchRadiusMult
            [ParserTarget("navballSwitchRadiusMultLow")]
            public NumericParser<Double> navballSwitchRadiusMultLow
            {
                get { return Value.navballSwitchRadiusMultLow; }
                set { Value.navballSwitchRadiusMultLow = value; }
            }

            // Science values of this body
            [ParserTarget("ScienceValues", AllowMerge = true)]
            public ScienceValuesLoader scienceValues { get; set; }

            // Biome definition via MapSO parser
            [ParserTarget("biomeMap")]
            public MapSOParser_RGB<CBAttributeMapSO> biomeMap
            {
                get { return Value.BiomeMap; }
                set
                {
                    if (((CBAttributeMapSO)value) != null)
                    {
                        Value.BiomeMap = value;
                        Value.BiomeMap.exactSearch = false;
                        Value.BiomeMap.nonExactThreshold = 0.05f;
                    }
                }
            }

            // Threshold for Biomes
            [ParserTarget("nonExactThreshold")]
            public NumericParser<Single> nonExactThreshold
            {
                get { return Value.BiomeMap != null ? Value.BiomeMap.nonExactThreshold : 0.05f; }
                set { Value.BiomeMap.nonExactThreshold = value; }
            }

            // If the biome threshold should get used
            [ParserTarget("exactSearch")]
            public NumericParser<Boolean> exactSearch
            {
                get { return Value.BiomeMap != null && Value.BiomeMap.exactSearch; }
                set { Value.BiomeMap.exactSearch = value; }
            }

            // Biomes of this body
            [ParserTargetCollection("Biomes", NameSignificance = NameSignificance.None, AllowMerge = true)]
            public CallbackList<BiomeLoader> biomes;

            // If the body name should be prefixed with "the" in some situations
            [ParserTarget("useTheInName")]
            public NumericParser<Boolean> useTheInName
            {
                get { return Value.bodyDisplayName.StartsWith("The", StringComparison.InvariantCultureIgnoreCase); }
                set { Value.bodyDisplayName = "The " + Value.bodyName; }
            }

            [ParserTarget("displayName")]
            public String displayName
            {
                get { return Value.bodyDisplayName; }
                set { Value.bodyDisplayName = value; }
            }

            // If the body should be unselectable
            [ParserTarget("selectable")]
            public NumericParser<Boolean> selectable
            {
                get { return Value.Get("selectable", true); }
                set { Value.Set("selectable", value.Value); }
            }

            // If the body should be hidden in RD
            [ParserTarget("RDVisibility")]
            public EnumParser<RDVisibility> hiddenRD
            {
                get { return Value.Get("hiddenRnD", RDVisibility.VISIBLE); }
                set { Value.Set("hiddenRnD", value.Value); }
            }

            // If the body should be hidden in RnD
            [ParserTarget("RnDVisibility")]
            public EnumParser<RDVisibility> hiddenRnD
            {
                get { return hiddenRD; }
                set { hiddenRD = value; }
            }

            // If the body should rotate in RnD
            [ParserTarget("RnDRotation")]
            public NumericParser<Boolean> RnDRotation
            {
                get
                {
                    return Value.Get("RnDRotation",
                        Value?.scaledBody?.GetComponentInChildren<SunCoronas>(true) != null);
                }
                set { Value.Set("RnDRotation", value.Value); }
            }

            // Max Zoom limit for TrackingStation and MapView
            // set the number of meters that can fit in the full height of the screen
            [ParserTarget("maxZoom")]
            public NumericParser<Single> minDistance
            {
                get { return Value.Get("maxZoom", 10 * 6000f); }
                set { Value.Set("maxZoom", value.Value / 6000f); }
            }

            // Apply Event
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                // Replace biomes
                if (Value.Get("removeBiomes", true) && node.HasNode("Biomes") && Value.BiomeMap != null)
                {
                    Value.BiomeMap = UnityEngine.Object.Instantiate(Value.BiomeMap);
                    Value.BiomeMap.Attributes = new CBAttributeMapSO.MapAttribute[] { };
                }

                // Event
                Events.OnPropertiesLoaderApply.Fire(this, node);
            }

            // PostApply Event
            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                // Converters
                if (_hasGravParam)
                    GravParamToOthers();
                else if (_hasMass)
                    MassToOthers();
                else
                    GeeASLToOthers();

                // Debug the fields (TODO - remove)
                Utility.DumpObjectFields(Value.scienceValues, " Science Values ");
                if (Value.BiomeMap != null)
                {
                    foreach (CBAttributeMapSO.MapAttribute biome in Value.BiomeMap.Attributes)
                    {
                        Logger.Active.Log("Found Biome: " + biome.name + " : " + biome.mapColor + " : " + biome.value);
                    }
                }

                // TODO - tentative fix, needs to be able to be configured (if it can be?)
                if (Value.progressTree == null)
                {
                    Value.progressTree = new KSPAchievements.CelestialBodySubtree(Value);
                    Logger.Active.Log("Added Progress Tree");
                }

                // Event
                Events.OnPropertiesLoaderPostApply.Fire(this, node);
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
                    Value.scienceValues = new CelestialBodyScienceParams();
                scienceValues = new ScienceValuesLoader(Value.scienceValues);
                
                // isHomeWorld Check
                Value.isHomeWorld = Value.transform.name == "Kerbin";
                
                // Biomes
                biomes = new CallbackList<BiomeLoader>(e =>
                {
                    // Check biome map
                    if (Value.BiomeMap == null)
                        throw new InvalidOperationException("The Biome Map cannot be null if you want to add biomes.");

                    // Replace the old biomes list with the new one
                    Value.BiomeMap.Attributes = biomes.Select(b => b.Value).ToArray();
                });
            }

            /// <summary>
            /// Creates a new Properties Loader from a spawned CelestialBody.
            /// </summary>
            public PropertiesLoader(CelestialBody body)
            {
                // Is this a spawned body?
                if (body?.scaledBody == null || Injector.IsInPrefab)
                {
                    throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
                }

                // Store values
                Value = body;
                
                // We require a science values object
                if (Value.scienceValues == null)
                    Value.scienceValues = new CelestialBodyScienceParams();
                scienceValues = new ScienceValuesLoader(Value.scienceValues);
                
                // isHomeWorld Check
                Value.isHomeWorld = Value.transform.name == "Kerbin";
                
                // Biomes
                biomes = new CallbackList<BiomeLoader>(e =>
                {
                    // Check biome map
                    if (Value.BiomeMap == null)
                        throw new InvalidOperationException("The Biome Map cannot be null if you want to add biomes.");

                    // Replace the old biomes list with the new one
                    Value.BiomeMap.Attributes = biomes.Select(b => b.Value).ToArray();
                });
            }

            // Mass converters
            public void GeeASLToOthers()
            {
                Double rsq = Value.Radius;
                rsq *= rsq;
                Value.gMagnitudeAtCenter = Value.GeeASL * 9.80665 * rsq;
                Value.gravParameter = Value.gMagnitudeAtCenter;
                Value.Mass = Value.gravParameter * (1 / 6.67408E-11);
                Logger.Active.Log("Via surface G, set gravParam to " + Value.gravParameter + ", mass to " + Value.Mass);
            }

            // Converts mass to Gee ASL using a body's radius.
            public void MassToOthers()
            {
                Double rsq = Value.Radius;
                rsq *= rsq;
                Value.GeeASL = Value.Mass * (6.67408E-11 / 9.80665) / rsq;
                Value.gMagnitudeAtCenter = Value.GeeASL * 9.80665 * rsq;
                Value.gravParameter = Value.gMagnitudeAtCenter;
                Logger.Active.Log("Via mass, set gravParam to " + Value.gravParameter + ", surface G to " + Value.GeeASL);
            }

            // Convert gravParam to mass and GeeASL
            public void GravParamToOthers()
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
}