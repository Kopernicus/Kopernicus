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
        public class Texture2DParser : IParsable, ITypeParser<Texture2D>
        {
            /// <summary>
            /// The value that is being parsed
            /// </summary>
            public Texture2D Value { get; set; }
            
            /// <summary>
            /// Parse the Value from a string
            /// </summary>
            public void SetFromString(String s)
            {
                // Check if we are attempting to load a builtin texture
                if (s.StartsWith("BUILTIN/"))
                {
                    String textureName = Regex.Replace(s, "BUILTIN/", "");
                    Value = Resources.FindObjectsOfTypeAll<Texture>().FirstOrDefault(tex => tex.name == textureName) as Texture2D;
                    if (Value == null)
                    {
                        Debug.LogError("[Kopernicus] Could not find built-in texture " + textureName);
                        Logger.Active.Log("Could not find built-in texture " + textureName);
                    }
                    return;
                }

                // Otherwise search the game database for one loaded from GameData/
                if (GameDatabase.Instance.ExistsTexture(s))
                {
                    // Get the texture URL
                    Value = GameDatabase.Instance.GetTexture(s, false);
                    return;
                }

                // Or load the texture directly
                if (OnDemandStorage.TextureExists(s))
                {
                    Value = OnDemandStorage.LoadTexture(s, false, true, false);
                    
                    // Upload it to the GPU if the parser could load something
                    if (Value != null)
                    {
                        try
                        {
                            Value.Apply(false, true);
                        }
                        catch
                        {
                            Debug.LogError("[Kopernicus] Failed to upload texture " + Value.name + " to the GPU");
                            Logger.Active.Log("Failed to upload texture " + Value.name + " to the GPU");
                        }
                    }
                    return;
                }

                // Texture was not found
                Value = null;
            }
        
            /// <summary>
            /// Create a new Texture2DParser
            /// </summary>
            public Texture2DParser()
            {
            }
        
            /// <summary>
            /// Create a new Texture2DParser from an already existing Texture
            /// </summary>
            public Texture2DParser(Texture2D i)
            {
                Value = i;
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator Texture2D(Texture2DParser parser)
            {
                return parser.Value;
            }
        
            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator Texture2DParser(Texture2D value)
            {
                return new Texture2DParser(value);
            }
        }

        // Parser for a MapSO
        public class MapSOParser_GreyScale<T> : BaseLoader, IParsable, ITypeParser<T> where T : MapSO
        {
            /// <summary>
            /// The value that is being parsed
            /// </summary>
            public T Value { get; set; }

            /// <summary>
            /// Parse the Value from a string
            /// </summary>
            public void SetFromString(String s)
            {
                // Should we use OnDemand?
                Boolean useOnDemand = OnDemandStorage.useOnDemand;

                if (s.StartsWith("BUILTIN/"))
                {
                    s = s.Substring(8);
                    Value = Utility.FindMapSO<T>(s);
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
                                Value = valCB as T;
                            }
                            else
                            {
                                MapSODemand valMap = ScriptableObject.CreateInstance<MapSODemand>();
                                valMap.Path = s;
                                valMap.Depth = MapSO.MapDepth.Greyscale;
                                valMap.name = mapName + " (G) for " + generatedBody.name;
                                valMap.AutoLoad = OnDemandStorage.onDemandLoadOnMissing;
                                OnDemandStorage.AddMap(generatedBody.name, valMap);
                                Value = valMap as T;
                            }
                        }
                    }
                    else // Load the texture
                    {
                        Texture2D map = OnDemandStorage.LoadTexture(s, false, false, false);
                        if (map != null)
                        {
                            // Create a new map script object
                            Value = ScriptableObject.CreateInstance<T>();
                            Value.CreateMap(MapSO.MapDepth.Greyscale, map);
                            UnityEngine.Object.DestroyImmediate(map);
                        }
                    }
                }
            }
        
            /// <summary>
            /// Create a new MapSOParser_GreyScale
            /// </summary>
            public MapSOParser_GreyScale()
            {
                
            }
        
            /// <summary>
            /// Create a new MapSOParser_GreyScale from an already existing Texture
            /// </summary>
            public MapSOParser_GreyScale(T value)
            {
                Value = value;
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator T(MapSOParser_GreyScale<T> parser)
            {
                return parser.Value;
            }
        
            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator MapSOParser_GreyScale<T>(T value)
            {
                return new MapSOParser_GreyScale<T>(value);
            }
        }

        // Parser for a MapSO
        public class MapSOParser_RGB<T> : BaseLoader, IParsable, ITypeParser<T> where T : MapSO
        {
            /// <summary>
            /// The value that is being parsed
            /// </summary>
            public T Value { get; set; }

            /// <summary>
            /// Parse the Value from a string
            /// </summary>
            public void SetFromString(String s)
            {
                // Should we use OnDemand?
                Boolean useOnDemand = OnDemandStorage.useOnDemand;

                if (s.StartsWith("BUILTIN/"))
                {
                    s = s.Substring(8);
                    Value = Utility.FindMapSO<T>(s);
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
                                Value = valCB as T;
                            }
                            else
                            {
                                OnDemand.MapSODemand valMap = ScriptableObject.CreateInstance<MapSODemand>();
                                valMap.Path = s;
                                valMap.Depth = MapSO.MapDepth.RGB;
                                valMap.name = mapName + " (RGB) for " + generatedBody.name;
                                valMap.AutoLoad = OnDemandStorage.onDemandLoadOnMissing;
                                OnDemandStorage.AddMap(generatedBody.name, valMap);
                                Value = valMap as T;
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
                            Value = ScriptableObject.CreateInstance<T>();
                            Value.CreateMap(MapSO.MapDepth.RGB, map);
                            UnityEngine.Object.DestroyImmediate(map);
                        }
                    }
                }
            }
        
            /// <summary>
            /// Create a new MapSOParser_RGB
            /// </summary>
            public MapSOParser_RGB()
            {
                
            }
        
            /// <summary>
            /// Create a new MapSOParser_RGB from an already existing Texture
            /// </summary>
            public MapSOParser_RGB(T value)
            {
                Value = value;
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator T(MapSOParser_RGB<T> parser)
            {
                return parser.Value;
            }
        
            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator MapSOParser_RGB<T>(T value)
            {
                return new MapSOParser_RGB<T>(value);
            }
        }

        // Parser for Physics Material
        [RequireConfigType(ConfigType.Node)]
        public class PhysicsMaterialParser : ITypeParser<PhysicMaterial>
        {
            // Physics material we are generating
            public PhysicMaterial Value { get; set; }

            // Physics material parameters
            [ParserTarget("bounceCombine")]
            public EnumParser<PhysicMaterialCombine> bounceCombine
            {
                get { return Value.bounceCombine; }
                set { Value.bounceCombine = value; }
            }

            [ParserTarget("frictionCombine")]
            public EnumParser<PhysicMaterialCombine> frictionCombine
            {
                get { return Value.frictionCombine; }
                set { Value.frictionCombine = value; }
            }

            [ParserTarget("bounciness")]
            public NumericParser<Single> bounciness
            {
                get { return Value.bounciness; }
                set { Value.bounciness = value; }
            }

            [ParserTarget("staticFriction")]
            public NumericParser<Single> staticFriction
            {
                get { return Value.staticFriction; }
                set { Value.staticFriction = value; }
            }

            [ParserTarget("dynamicFriction")]
            public NumericParser<Single> dynamicFriction
            {
                get { return Value.dynamicFriction; }
                set { Value.dynamicFriction = value; }
            }
        
            /// <summary>
            /// Create a new PhysicsMaterialParser
            /// </summary>
            public PhysicsMaterialParser()
            {
                Value = new PhysicMaterial();
                Value.name = "Ground";
                Value.dynamicFriction = 0.6f;
                Value.staticFriction = 0.8f;
                Value.bounciness = 0.0f;
                Value.frictionCombine = PhysicMaterialCombine.Maximum;
                Value.bounceCombine = PhysicMaterialCombine.Average;;
            }
        
            /// <summary>
            /// Create a new PhysicsMaterialParser from an already existing PhysicMaterial
            /// </summary>
            public PhysicsMaterialParser(PhysicMaterial material)
            {
                Value = material;
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator PhysicMaterial(PhysicsMaterialParser parser)
            {
                return parser.Value;
            }
        
            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator PhysicsMaterialParser(PhysicMaterial value)
            {
                return new PhysicsMaterialParser(value);
            }
        }

        // Parser for mesh
        [RequireConfigType(ConfigType.Value)]
        public class MeshParser : IParsable, ITypeParser<Mesh>
        {
            /// <summary>
            /// The value that is being parsed
            /// </summary>
            public Mesh Value { get; set; }

            /// <summary>
            /// Parse the Value from a string
            /// </summary>
            public void SetFromString(String s)
            {
                // Check if we are attempting to load a builtin mesh
                if (s.StartsWith("BUILTIN/"))
                {
                    String meshName = Regex.Replace(s, "BUILTIN/", "");
                    Value = Resources.FindObjectsOfTypeAll<Mesh>().First(mesh => mesh.name == meshName);
                    return;
                }

                String path = KSPUtil.ApplicationRootPath + "GameData/" + s;
                if (File.Exists(path))
                {
                    Value = ObjImporter.ImportFile(path);
                    Value.name = Path.GetFileNameWithoutExtension(path);
                    return;
                }

                // Mesh was not found
                Value = null;
            }
        
            /// <summary>
            /// Create a new MeshParser
            /// </summary>
            public MeshParser()
            {

            }
        
            /// <summary>
            /// Create a new MeshParser from an already existing Mesh
            /// </summary>
            public MeshParser(Mesh value)
            {
                Value = value;
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator Mesh(MeshParser parser)
            {
                return parser.Value;
            }
        
            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator MeshParser(Mesh value)
            {
                return new MeshParser(value);
            }
        }

        [RequireConfigType(ConfigType.Value)]
        public class AssetParser<T> : IParsable, ITypeParser<T> where T : UnityEngine.Object
        {
            /// <summary>
            /// The value that is being parsed
            /// </summary>
            public T Value { get; set; }

            private static Dictionary<String, AssetBundle> bundles = new Dictionary<String, AssetBundle>();

            /// <summary>
            /// Parse the Value from a string
            /// </summary>
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
                Value = UnityEngine.Object.Instantiate(bundle.LoadAsset<T>(split[1]));
                UnityEngine.Object.DontDestroyOnLoad(Value);
            }
        
            /// <summary>
            /// Create a new AssetParser
            /// </summary>
            public AssetParser()
            {

            }
        
            /// <summary>
            /// Create a new AssetParser from an already existing Value
            /// </summary>
            public AssetParser(T value)
            {
                Value = value;
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator T(AssetParser<T> parser)
            {
                return parser.Value;
            }
        
            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator AssetParser<T>(T value)
            {
                return new AssetParser<T>(value);
            }
        }

        // parser for .mu
        [RequireConfigType(ConfigType.Value)]
        public class MuParser : IParsable, ITypeParser<GameObject>
        {
            /// <summary>
            /// The value that is being parsed
            /// </summary>
            public GameObject Value { get; set; }

            /// <summary>
            /// Parse the Value from a string
            /// </summary>
            public void SetFromString(String s)
            {
                // If there's a model, import it
                if (GameDatabase.Instance.ExistsModel(s))
                {
                    Value = GameDatabase.Instance.GetModel(s);
                    return;
                }

                // Otherwise, set the value to null
                Value = null;
            }
        
            /// <summary>
            /// Create a new MuParser
            /// </summary>
            public MuParser()
            {

            }
        
            /// <summary>
            /// Create a new MuParser from an already existing Object
            /// </summary>
            public MuParser(GameObject value)
            {
                Value = value;
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator GameObject(MuParser parser)
            {
                return parser.Value;
            }
        
            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator MuParser(GameObject value)
            {
                return new MuParser(value);
            }
        }
        
        // Stock scatter material parser
        [RequireConfigType(ConfigType.Value)]
        public class StockMaterialParser : IParsable, ITypeParser<Material>
        {
            /// <summary>
            /// The value that is being parsed
            /// </summary>
            public Material Value { get; set; }
        
            /// <summary>
            /// Parse the Value from a string
            /// </summary>
            public void SetFromString(String s)
            {
                String materialName = Regex.Replace(s, "BUILTIN/", "");
                Value = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(material => material.name == materialName);
            }
        
            /// <summary>
            /// Create a new StockMaterialParser
            /// </summary>
            public StockMaterialParser()
            {
            }
        
            /// <summary>
            /// Create a new StockMaterialParser from an already existing Material
            /// </summary>
            public StockMaterialParser(Material i)
            {
                Value = i;
            }

            /// <summary>
            /// Convert Parser to Value
            /// </summary>
            public static implicit operator Material(StockMaterialParser parser)
            {
                return parser?.Value;
            }
        
            /// <summary>
            /// Convert Value to Parser
            /// </summary>
            public static implicit operator StockMaterialParser(Material material)
            {
                if (material == null)
                    return new StockMaterialParser();
                Material m = new Material(material);
                m.name = "BUILTIN/" + m.name;
                return new StockMaterialParser { Value = m };
            }
        }
    }
}
