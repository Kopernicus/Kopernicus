/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System.Collections.Generic;
using System.Linq;

namespace Kopernicus
{
    namespace Configuration
    {
        // Edit the CelestialBody component
        [RequireConfigType(ConfigType.Node)]
        public class PropertiesLoader : BaseLoader, IParserEventSubscriber
        {
            // Celestial body to edit
            public CelestialBody celestialBody { get; set; }

            // Body description
            [ParserTarget("description", optional = true)]
            public string description 
            {
                get { return celestialBody.bodyDescription; }
                set { celestialBody.bodyDescription = value; }
            }

            // Radius
            [ParserTarget("radius", optional = true)]
            public NumericParser<double> radius 
            {
                get { return celestialBody.Radius; }
                set { celestialBody.Radius = value; }
            }
            
            // GeeASL
            [ParserTarget("geeASL", optional = true)]
            public NumericParser<double> geeASL 
            {
                get { return celestialBody.GeeASL; }
                set { celestialBody.GeeASL = value; hasGeeASL = true; }
            }
            private bool hasGeeASL = false;
            
            // Mass
            [ParserTarget("mass", optional = true)]
            public NumericParser<double> mass
            {
                get { return celestialBody.Mass; }
                set { celestialBody.Mass = value; hasMass = true; }
            }
            private bool hasMass = false;

            // Grav Param
            [ParserTarget("gravParameter", optional = true)]
            public NumericParser<double> gravParameter
            {
                get { return celestialBody.gravParameter; }
                set { celestialBody.gMagnitudeAtCenter = celestialBody.gravParameter = value; hasGravParam = true; }
            }
            private bool hasGravParam = false;
            
            // Does the body rotate?
            [ParserTarget("rotates", optional = true)]
            public NumericParser<bool> rotates
            {
                get { return celestialBody.rotates; }
                set { celestialBody.rotates = value; }
            }
            
            // Rotation period of the world
            [ParserTarget("rotationPeriod", optional = true)]
            public NumericParser<double> rotationPeriod
            {
                get { return celestialBody.rotationPeriod; }
                set { celestialBody.rotationPeriod = value; }
            }
            
            // Is the body tidally locked to its parent?
            [ParserTarget("tidallyLocked", optional = true)]
            public NumericParser<bool> tidallyLocked
            {
                get { return celestialBody.tidallyLocked; }
                set { celestialBody.tidallyLocked = value; }
            }

            // Initial rotation of the world
            [ParserTarget("initialRotation", optional = true)]
            public NumericParser<double> initialRotation
            {
                get { return celestialBody.initialRotation; }
                set { celestialBody.initialRotation = value; }
            }

            // Altitude where the Game switches the reference frame
            [ParserTarget("inverseRotThresholdAltitude", optional = true)]
            public NumericParser<float> inverseRotThresholdAltitude
            {
                get { return celestialBody.inverseRotThresholdAltitude; }
                set { celestialBody.inverseRotThresholdAltitude = value; }
            }
            
            // albedo
            [ParserTarget("albedo", optional = true)]
            public NumericParser<double> albedo
            {
                get { return celestialBody.albedo; }
                set { celestialBody.albedo = value; }
            }

            // emissivity
            [ParserTarget("emissivity", optional = true)]
            public NumericParser<double> emissivity
            {
                get { return celestialBody.emissivity; }
                set { celestialBody.emissivity = value; }
            }

            // coreTemperatureOffset
            [ParserTarget("coreTemperatureOffset", optional = true)]
            public NumericParser<double> coreTemperatureOffset
            {
                get { return celestialBody.coreTemperatureOffset; }
                set { celestialBody.coreTemperatureOffset = value; }
            }
            
            // Is this the home world
            [ParserTarget("isHomeWorld", optional = true)]
            public NumericParser<bool> isHomeWorld
            {
                get { return celestialBody.isHomeWorld; }
                set { celestialBody.isHomeWorld = value; }
            }

            // Time warp altitude limits
            [ParserTarget("timewarpAltitudeLimits", optional = true)]
            public NumericCollectionParser<float> timewarpAltitudeLimits 
            {
                get { return celestialBody.timeWarpAltitudeLimits != null ? celestialBody.timeWarpAltitudeLimits : new float[0]; }
                set { celestialBody.timeWarpAltitudeLimits = value.value.ToArray(); }
            }

            // Sphere of Influence
            [ParserTarget("sphereOfInfluence", optional = true)]
            public NumericParser<double> sphereOfInfluence
            {
                get { return celestialBody.sphereOfInfluence; }
                set { Templates.sphereOfInfluence.Add(celestialBody.name, value); }
            }

            // Hill Sphere
            [ParserTarget("hillSphere", optional = true)]
            public NumericParser<double> hillSphere
            {
                get { return celestialBody.hillSphere; }
                set { Templates.hillSphere.Add(celestialBody.bodyTransform.name, value); }
            }

            // solarRotationPeriod
            [ParserTarget("solarRotationPeriod", optional = true)]
            public NumericParser<bool> solarRotationPeriod
            {
                get { return celestialBody.solarRotationPeriod; }
                set { celestialBody.solarRotationPeriod = value; }
            }

            // navballSwitchRadiusMult
            [ParserTarget("navballSwitchRadiusMult", optional = true)]
            public NumericParser<double> navballSwitchRadiusMult
            {
                get { return celestialBody.navballSwitchRadiusMult; }
                set { celestialBody.navballSwitchRadiusMult = value; }
            }

            // Science values of this body
            [ParserTarget("ScienceValues", optional = true, allowMerge = true)]
            public ScienceValuesLoader scienceValues { get; set; }

            // Biomes of this body
            [PreApply]
            [ParserTargetCollection("Biomes", optional = true, nameSignificance = NameSignificance.None)]
            public List<BiomeLoader> biomes = new List<BiomeLoader>();

            // Biome definition via MapSO parser
            [ParserTarget("biomeMap", optional = true)]
            public MapSOParser_RGB<CBAttributeMapSO> biomeMap
            {
                get { return celestialBody.BiomeMap; }
                set 
                {
                    if (((CBAttributeMapSO)value) != null) 
                    {
                        celestialBody.BiomeMap = value;
                        celestialBody.BiomeMap.exactSearch = false;
                        celestialBody.BiomeMap.nonExactThreshold = 0.05f;
                        celestialBody.BiomeMap.Attributes = biomes.Select (b => b.attribute).ToArray ();
                    }
                }
            }

            // Threshold for Biomes
            [ParserTarget("nonExactThreshold", optional = true)]
            public NumericParser<float> nonExactThreshold
            {
                get { return celestialBody.BiomeMap != null ? celestialBody.BiomeMap.nonExactThreshold : 0.05f; }
                set { celestialBody.BiomeMap.nonExactThreshold = value; }
            }

            // If the biome threshold should get used
            [ParserTarget("exactSearch", optional = true)]
            public NumericParser<bool> exactSearch
            {
                get { return celestialBody.BiomeMap != null ? celestialBody.BiomeMap.exactSearch : false; }
                set { celestialBody.BiomeMap.exactSearch = value; }
            }

            // If the body name should be prefixed with "the" in some situations
            [ParserTarget("useTheInName", optional = true)]
            public NumericParser<bool> useTheInName
            {
                get { return celestialBody.use_The_InName; }
                set { celestialBody.use_The_InName = value; }
            }

            // If the body should be unselectable
            [ParserTarget("selectable", optional = true)]
            public NumericParser<bool> selectable
            {
                get { return !Templates.notSelectable.Contains(celestialBody.transform.name); }
                set { if (!value.value) Templates.notSelectable.Add(celestialBody.transform.name); }
            }

            // If the body should be hidden in RnD
            [ParserTarget("hiddenRnD", optional = true)]
            public NumericParser<bool> hiddenRnD
            {
                get { return Templates.hiddenRnD.Contains(celestialBody.transform.name); }
                set { if (value.value) Templates.hiddenRnD.Add(celestialBody.transform.name); }
            }

            // Apply Event
            void IParserEventSubscriber.Apply (ConfigNode node)
            {
                // We require a science values object
                if (celestialBody.scienceValues == null)
                    celestialBody.scienceValues = new CelestialBodyScienceParams();

                // Create the science values cache
                scienceValues = new ScienceValuesLoader(celestialBody.scienceValues);
            }

            // PostApply Event
            void IParserEventSubscriber.PostApply (ConfigNode node)
            {
                // Converters
                if (hasGravParam)
                    GravParamToOthers();
                else if (hasMass)
                    MassToOthers();
                else
                    GeeASLToOthers();

                // Debug the fields (TODO - remove)
                Utility.DumpObjectFields (celestialBody.scienceValues, " Science Values ");
                if (celestialBody.BiomeMap != null) 
                {
                    foreach (CBAttributeMapSO.MapAttribute biome in celestialBody.BiomeMap.Attributes) 
                    {
                        Logger.Active.Log ("Found Biome: " + biome.name + " : " + biome.mapColor + " : " + biome.value);
                    }
                }

                // TODO - tentative fix, needs to be able to be configured (if it can be?)
                if (celestialBody.progressTree == null) 
                {
                    celestialBody.progressTree = new KSPAchievements.CelestialBodySubtree (celestialBody);
                    Logger.Active.Log ("Added Progress Tree");
                }
            }

            // Properties requires a celestial body referece, as this class is designed to edit the body
            public PropertiesLoader (CelestialBody celestialBody)
            {
                this.celestialBody = celestialBody;
            }

            // Properties requires a celestial body referece, as this class is designed to edit the body
            public PropertiesLoader()
            {
                celestialBody = generatedBody.celestialBody;
            }

            // Mass converters
            public void GeeASLToOthers()
            {
                double rsq = celestialBody.Radius;
                rsq *= rsq;
                celestialBody.gMagnitudeAtCenter = celestialBody.GeeASL * 9.81 * rsq;
                celestialBody.gravParameter = celestialBody.gMagnitudeAtCenter;
                celestialBody.Mass = celestialBody.gravParameter * (1 / 6.674E-11);
                Logger.Active.Log("Via surface G, set gravParam to " + celestialBody.gravParameter + ", mass to " + celestialBody.Mass);
            }

            // Converts mass to Gee ASL using a body's radius.
            public void MassToOthers()
            {
                double rsq = celestialBody.Radius;
                rsq *= rsq;
                celestialBody.GeeASL = celestialBody.Mass * (6.674E-11 / 9.81) / rsq;
                celestialBody.gMagnitudeAtCenter = celestialBody.GeeASL * 9.81 * rsq;
                celestialBody.gravParameter = celestialBody.gMagnitudeAtCenter;
                Logger.Active.Log("Via mass, set gravParam to " + celestialBody.gravParameter + ", surface G to " + celestialBody.GeeASL);
            }

            // Convert gravParam to mass and GeeASL
            public void GravParamToOthers()
            {
                double rsq = celestialBody.Radius;
                rsq *= rsq;
                celestialBody.Mass = celestialBody.gravParameter * (1 / 6.674E-11);
                celestialBody.GeeASL = celestialBody.gravParameter / 9.81 / rsq;
                celestialBody.gMagnitudeAtCenter = celestialBody.gravParameter;
                Logger.Active.Log("Via gravParam, set mass to " + celestialBody.Mass + ", surface G to " + celestialBody.GeeASL);
            }
        }
    }
}
