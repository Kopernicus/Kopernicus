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

using Kopernicus.OnDemand;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        // Alternative parser for Vector3
        [RequireConfigType(ConfigType.Node)]
        public class PositionParser : BaseLoader
        {
            // Latitude
            [ParserTarget("latitude")]
            public NumericParser<Double> latitude { get; set; }

            // Longitude
            [ParserTarget("longitude")]
            public NumericParser<Double> longitude { get; set; }

            // Altitude
            [ParserTarget("altitude")]
            public NumericParser<Double> altitude { get; set; }

            // Default Constructor
            public PositionParser()
            {
                latitude = new NumericParser<Double>(0);
                longitude = new NumericParser<Double>(0);
                altitude = new NumericParser<Double>(0);
            }

            // Convert
            public static implicit operator Vector3(PositionParser parser)
            {
                Double? radius = Loader.currentBody?.generatedBody?.celestialBody?.Radius;
                return Utility.LLAtoECEF(parser.latitude, parser.longitude, parser.altitude, radius > 0 ? (Double)radius : 0);
            }
        }

        // Parser for Texture2D
        [RequireConfigType(ConfigType.Value)]
        public class Texture2DParser : IParsable
        {
            public Texture2D value;
            public void SetFromString(String s)
            {
                // Check if we are attempting to load a builtin texture
                if (s.StartsWith("BUILTIN/"))
                {
                    String textureName = Regex.Replace(s, "BUILTIN/", "");
                    value = Resources.FindObjectsOfTypeAll<Texture>().FirstOrDefault(tex => tex.name == textureName) as Texture2D;
                    if (value == null)
                    {
                        Debug.LogError("[Kopernicus] Could not find built-in texture " + textureName);
                        Logger.Active.Log("Could not find built-in texture " + textureName);
                    }
                    return;
                }

                // Otherwise search the game database for one loaded from GameData/
                else if (GameDatabase.Instance.ExistsTexture(s))
                {
                    // Get the texture URL
                    value = GameDatabase.Instance.GetTexture(s, false);
                    return;
                }

                // Or load the texture directly
                else if (Utility.TextureExists(s))
                {
                    value = Utility.LoadTexture(s, false, false, false);
                    return;
                }

                // Texture was not found
                value = null;
            }
            public Texture2DParser()
            {

            }
            public Texture2DParser(Texture2D value)
            {
                this.value = value;
            }

            // Convert
            public static implicit operator Texture2D(Texture2DParser parser)
            {
                return parser.value;
            }
            public static implicit operator Texture2DParser(Texture2D value)
            {
                return new Texture2DParser(value);
            }
        }

        // Parser for a MapSO
        public class MapSOParser_GreyScale<T> : BaseLoader, IParsable where T : MapSO
        {
            // Value
            public T value;

            // Load the MapSO
            public void SetFromString(String s)
            {
                // Should we use OnDemand?
                Boolean useOnDemand = OnDemandStorage.useOnDemand;

                if (s.StartsWith("BUILTIN/"))
                {
                    s = s.Substring(8);
                    value = Utility.FindMapSO<T>(s);
                }
                else
                {
                    // are we on-demand? Don't load now.
                    if (useOnDemand)
                    {
                        if (Utility.TextureExists(s))
                        {
                            String mapName = s;
                            mapName = mapName.Substring(s.LastIndexOf('/') + 1);
                            Int32 lastDot = mapName.LastIndexOf('.');
                            if (lastDot > 0)
                                mapName = mapName.Substring(0, lastDot);
                            if (typeof(T) == typeof(CBAttributeMapSO))
                            {
                                CBAttributeMapSODemand valCB = ScriptableObject.CreateInstance<CBAttributeMapSODemand>();
                                valCB.Path = s;
                                valCB.Depth = MapSO.MapDepth.Greyscale;
                                valCB.name = mapName + " (CBG) for " + generatedBody.name;
                                valCB.AutoLoad = OnDemandStorage.onDemandLoadOnMissing;
                                OnDemandStorage.AddMap(generatedBody.name, valCB);
                                value = valCB as T;
                            }
                            else
                            {
                                MapSODemand valMap = ScriptableObject.CreateInstance<MapSODemand>();
                                valMap.Path = s;
                                valMap.Depth = MapSO.MapDepth.Greyscale;
                                valMap.name = mapName + " (G) for " + generatedBody.name;
                                valMap.AutoLoad = OnDemandStorage.onDemandLoadOnMissing;
                                OnDemandStorage.AddMap(generatedBody.name, valMap);
                                value = valMap as T;
                            }
                        }
                    }
                    else // Load the texture
                    {
                        Texture2D map = Utility.LoadTexture(s, false, false, false);
                        if (map != null)
                        {
                            // Create a new map script object
                            value = ScriptableObject.CreateInstance<T>();
                            value.CreateMap(MapSO.MapDepth.Greyscale, map);
                            UnityEngine.Object.DestroyImmediate(map);
                        }
                    }
                }
            }
            public MapSOParser_GreyScale()
            {
                this.value = null;
            }
            public MapSOParser_GreyScale(T value)
            {
                this.value = value;
            }

            // Convert
            public static implicit operator T(MapSOParser_GreyScale<T> parser)
            {
                return parser.value;
            }
            public static implicit operator MapSOParser_GreyScale<T>(T value)
            {
                return new MapSOParser_GreyScale<T>(value);
            }
        }

        // Parser for a MapSO
        public class MapSOParser_RGB<T> : BaseLoader, IParsable where T : MapSO
        {
            // Value
            public T value;

            // Load the MapSO
            public void SetFromString(String s)
            {
                // Should we use OnDemand?
                Boolean useOnDemand = OnDemandStorage.useOnDemand;

                if (s.StartsWith("BUILTIN/"))
                {
                    s = s.Substring(8);
                    value = Utility.FindMapSO<T>(s);
                }
                else
                {
                    // check if OnDemand.
                    if (useOnDemand)
                    {
                        if (Utility.TextureExists(s))
                        {
                            String mapName = s;
                            mapName = mapName.Substring(s.LastIndexOf('/') + 1);
                            Int32 lastDot = mapName.LastIndexOf('.');
                            if (lastDot > 0)
                                mapName = mapName.Substring(0, lastDot);
                            if (typeof(T) == typeof(CBAttributeMapSO))
                            {
                                CBAttributeMapSODemand valCB = ScriptableObject.CreateInstance<CBAttributeMapSODemand>();
                                valCB.Path = s;
                                valCB.Depth = MapSO.MapDepth.RGB;
                                valCB.name = mapName + " (CBRGB) for " + generatedBody.name;
                                valCB.AutoLoad = OnDemandStorage.onDemandLoadOnMissing;
                                OnDemandStorage.AddMap(generatedBody.name, valCB);
                                value = valCB as T;
                            }
                            else
                            {
                                OnDemand.MapSODemand valMap = ScriptableObject.CreateInstance<MapSODemand>();
                                valMap.Path = s;
                                valMap.Depth = MapSO.MapDepth.RGB;
                                valMap.name = mapName + " (RGB) for " + generatedBody.name;
                                valMap.AutoLoad = OnDemandStorage.onDemandLoadOnMissing;
                                OnDemandStorage.AddMap(generatedBody.name, valMap);
                                value = valMap as T;
                            }
                        }
                    }
                    else
                    {
                        // Load the texture
                        Texture2D map = Utility.LoadTexture(s, false, false, false);
                        if (map != null)
                        {
                            // Create a new map script object
                            value = ScriptableObject.CreateInstance<T>();
                            value.CreateMap(MapSO.MapDepth.RGB, map);
                            UnityEngine.Object.DestroyImmediate(map);
                        }
                    }
                }
            }
            public MapSOParser_RGB()
            {
                this.value = null;
            }
            public MapSOParser_RGB(T value)
            {
                this.value = value;
            }

            // Convert
            public static implicit operator T(MapSOParser_RGB<T> parser)
            {
                return parser.value;
            }
            public static implicit operator MapSOParser_RGB<T>(T value)
            {
                return new MapSOParser_RGB<T>(value);
            }
        }

        // Parser for Physics Material
        [RequireConfigType(ConfigType.Node)]
        public class PhysicsMaterialParser : IParserEventSubscriber
        {
            // Physics material we are generating
            public PhysicMaterial material { get; set; }

            // Physics material parameters
            [ParserTarget("bounceCombine")]
            public EnumParser<PhysicMaterialCombine> bounceCombine
            {
                get { return material.bounceCombine; }
                set { material.bounceCombine = value; }
            }

            [ParserTarget("frictionCombine")]
            public EnumParser<PhysicMaterialCombine> frictionCombine
            {
                get { return material.frictionCombine; }
                set { material.frictionCombine = value; }
            }

            [ParserTarget("bounciness")]
            public NumericParser<Single> bounciness
            {
                get { return material.bounciness; }
                set { material.bounciness = value; }
            }

            [ParserTarget("staticFriction")]
            public NumericParser<Single> staticFriction
            {
                get { return material.staticFriction; }
                set { material.staticFriction = value; }
            }

            [ParserTarget("dynamicFriction")]
            public NumericParser<Single> dynamicFriction
            {
                get { return material.dynamicFriction; }
                set { material.dynamicFriction = value.value; }
            }

            void IParserEventSubscriber.Apply(ConfigNode node) { }
            void IParserEventSubscriber.PostApply(ConfigNode node) { }

            // Default constructor
            public PhysicsMaterialParser()
            {
                material = new PhysicMaterial();
                material.name = "Ground";
                material.dynamicFriction = 0.6f;
                material.staticFriction = 0.8f;
                material.bounciness = 0.0f;
                material.frictionCombine = PhysicMaterialCombine.Maximum;
                material.bounceCombine = PhysicMaterialCombine.Average;;
            }

            // Initializing constructor
            public PhysicsMaterialParser(PhysicMaterial material)
            {
                this.material = material;
            }

            // Convert
            public static implicit operator PhysicMaterial(PhysicsMaterialParser parser)
            {
                return parser.material;
            }
            public static implicit operator PhysicsMaterialParser(PhysicMaterial material)
            {
                return new PhysicsMaterialParser(material);
            }
        }

        // Parser for mesh
        [RequireConfigType(ConfigType.Value)]
        public class MeshParser : IParsable
        {
            public Mesh value;
            public void SetFromString(String s)
            {
                // Check if we are attempting to load a builtin mesh
                if (s.StartsWith("BUILTIN/"))
                {
                    String meshName = Regex.Replace(s, "BUILTIN/", "");
                    value = Resources.FindObjectsOfTypeAll<Mesh>().First(mesh => mesh.name == meshName);
                    return;
                }

                String path = KSPUtil.ApplicationRootPath + "GameData/" + s;
                if (File.Exists(path))
                {
                    value = ObjImporter.ImportFile(path);
                    value.name = Path.GetFileNameWithoutExtension(path);
                    return;
                }

                // Mesh was not found
                value = null;
            }
            public MeshParser()
            {

            }
            public MeshParser(Mesh value)
            {
                this.value = value;
            }

            // Convert
            public static implicit operator Mesh(MeshParser parser)
            {
                return parser.value;
            }
            public static implicit operator MeshParser(Mesh mesh)
            {
                return new MeshParser(mesh);
            }
        }

        [RequireConfigType(ConfigType.Value)]
        public class AssetParser<T> : IParsable where T : UnityEngine.Object
        {
            // The loaded value
            public T value;

            private static Dictionary<String, AssetBundle> bundles = new Dictionary<String, AssetBundle>();

            // Load the AssetBundle with the object
            public void SetFromString(String s)
            {
                String[] split = s.Split(':');
                if (!File.Exists(KSPUtil.ApplicationRootPath + "GameData/" + split[0]))
                {
                    Logger.Active.Log("Couldn't find asset file at path: " + KSPUtil.ApplicationRootPath + "GameData/" + split[0]);
                    return;
                }
                AssetBundle bundle = null;
                if (!bundles.ContainsKey(split[0]))
                {
                    bundle = AssetBundle.LoadFromMemory(File.ReadAllBytes(KSPUtil.ApplicationRootPath + "GameData/" + split[0]));
                    bundles.Add(split[0], bundle);
                }
                else
                {
                    bundle = bundles[split[0]];
                }
                value = UnityEngine.Object.Instantiate(bundle.LoadAsset<T>(split[1]));
                UnityEngine.Object.DontDestroyOnLoad(value);
                //bundle.Unload(false);
            }
            public AssetParser()
            {

            }
            public AssetParser(T value)
            {
                this.value = value;
            }

            // Convert
            public static implicit operator T(AssetParser<T> parser)
            {
                return parser.value;
            }
            public static implicit operator AssetParser<T>(T value)
            {
                return new AssetParser<T>(value);
            }
        }

        // parser for .mu
        [RequireConfigType(ConfigType.Value)]
        public class MuParser : IParsable
        {
            public GameObject value;

            public void SetFromString(String s)
            {
                // If there's a model, import it
                if (GameDatabase.Instance.ExistsModel(s))
                {
                    value = GameDatabase.Instance.GetModel(s);
                    return;
                }

                // Otherwise, set the value to null
                value = null;
            }

            // Default constructor
            public MuParser()
            {
            }

            // Initializing constructor
            public MuParser(GameObject value)
            {
                this.value = value;
            }
        }
    }
}
