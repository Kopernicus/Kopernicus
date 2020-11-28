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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.OnDemand;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kopernicus.Configuration.Parsing
{
    // Alternative parser for Vector3
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public class PositionParser : BaseLoader
    {
        // Latitude
        [ParserTarget("latitude")]
        public NumericParser<Double> Latitude { get; set; }

        // Longitude
        [ParserTarget("longitude")]
        public NumericParser<Double> Longitude { get; set; }

        // Altitude
        [ParserTarget("altitude")]
        public NumericParser<Double> Altitude { get; set; }

        // Default Constructor
        public PositionParser()
        {
            Latitude = 0;
            Longitude = 0;
            Altitude = 0;
        }

        // Convert
        public static implicit operator Vector3(PositionParser parser)
        {
            PSystemBody generatedBody = Parser.GetState<Body>("Kopernicus:currentBody")?.GeneratedBody;
            if (generatedBody == null)
            {
                return Vector3.zero;
            }

            Double radius = generatedBody.celestialBody.Radius;
            return Utility.LLAtoECEF(parser.Latitude, parser.Longitude, parser.Altitude,
                Math.Max(radius, 0));
        }
    }

    // Parser for Texture2D
    [RequireConfigType(ConfigType.Value)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
                if (Value != null)
                {
                    return;
                }

                Debug.LogError("[Kopernicus] Could not find built-in texture " + textureName);
                Logger.Active.Log("Could not find built-in texture " + textureName);
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
                if (Value == null)
                {
                    return;
                }
                try
                {
                    Value.Apply(false, true);
                }
                catch
                {
                    Debug.LogError("[Kopernicus] Failed to upload texture " + Value.name + " to the GPU");
                    Logger.Active.Log("Failed to upload texture " + Value.name + " to the GPU");
                }

                return;
            }

            // Texture was not found
            Value = null;
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            if (Value == null)
            {
                return null;
            }

            if (GameDatabase.Instance.ExistsTexture(Value.name) || OnDemandStorage.TextureExists(Value.name))
            {
                return Value.name;
            }

            return "BUILTIN/" + Value.name;
        }

        /// <summary>
        /// Create a new Texture2DParser
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
    [RequireConfigType(ConfigType.Value)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class MapSOParserGreyScale<T> : BaseLoader, IParsable, ITypeParser<T> where T : MapSO
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
            Boolean useOnDemand = OnDemandStorage.UseOnDemand;
            Boolean useOnDemandBiomes = OnDemandStorage.UseOnDemandBiomes;

            if (s.StartsWith("BUILTIN/"))
            {
                s = s.Substring(8);
                Value = Utility.FindMapSO<T>(s);
            }
            else
            {
                // are we on-demand? Don't load now.
                if (useOnDemand && typeof(T) == typeof(MapSO) ||
                    useOnDemandBiomes && typeof(T) == typeof(CBAttributeMapSO))
                {
                    if (!Utility.TextureExists(s))
                    {
                        return;
                    }

                    if (typeof(T) == typeof(CBAttributeMapSO))
                    {
                        CBAttributeMapSODemand cbMap = ScriptableObject.CreateInstance<CBAttributeMapSODemand>();
                        cbMap.Path = s;
                        cbMap.Depth = MapSO.MapDepth.Greyscale;
                        cbMap.AutoLoad = OnDemandStorage.OnDemandLoadOnMissing;
                        OnDemandStorage.AddMap(generatedBody.name, cbMap);
                        Value = cbMap as T;
                    }
                    else
                    {
                        MapSODemand map = ScriptableObject.CreateInstance<MapSODemand>();
                        map.Path = s;
                        map.Depth = MapSO.MapDepth.Greyscale;
                        map.AutoLoad = OnDemandStorage.OnDemandLoadOnMissing;
                        OnDemandStorage.AddMap(generatedBody.name, map);
                        Value = map as T;
                    }
                }
                else // Load the texture
                {
                    Texture2D map = OnDemandStorage.LoadTexture(s, false, false, false);
                    if (map == null)
                    {
                        return;
                    }

                    // Create a new map script object
                    Value = ScriptableObject.CreateInstance<T>();
                    Value.CreateMap(MapSO.MapDepth.Greyscale, map);
                    Object.DestroyImmediate(map);
                }
            }

            if (Value != null)
            {
                Value.name = s;
            }
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            if (Value == null)
            {
                return null;
            }

            if (GameDatabase.Instance.ExistsTexture(Value.name) || OnDemandStorage.TextureExists(Value.name))
            {
                return Value.name;
            }

            return "BUILTIN/" + Value.name;
        }

        /// <summary>
        /// Create a new MapSOParser_GreyScale
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public MapSOParserGreyScale()
        {

        }

        /// <summary>
        /// Create a new MapSOParser_GreyScale from an already existing Texture
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public MapSOParserGreyScale(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator T(MapSOParserGreyScale<T> parser)
        {
            return parser.Value;
        }

        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator MapSOParserGreyScale<T>(T value)
        {
            return new MapSOParserGreyScale<T>(value);
        }
    }

    // Parser for a MapSO RGB
    [RequireConfigType(ConfigType.Value)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class MapSOParserRGB<T> : BaseLoader, IParsable, ITypeParser<T> where T : MapSO
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
            Boolean useOnDemand = OnDemandStorage.UseOnDemand;
            Boolean useOnDemandBiomes = OnDemandStorage.UseOnDemandBiomes;

            if (s.StartsWith("BUILTIN/"))
            {
                s = s.Substring(8);
                Value = Utility.FindMapSO<T>(s);
            }
            else
            {
                // check if OnDemand.
                if (useOnDemand && typeof(T) == typeof(MapSO) ||
                    useOnDemandBiomes && typeof(T) == typeof(CBAttributeMapSO))
                {
                    if (!Utility.TextureExists(s))
                    {
                        return;
                    }
                    if (typeof(T) == typeof(CBAttributeMapSO))
                    {
                        CBAttributeMapSODemand cbMap = ScriptableObject.CreateInstance<CBAttributeMapSODemand>();
                        cbMap.Path = s;
                        cbMap.Depth = MapSO.MapDepth.RGB;
                        cbMap.AutoLoad = OnDemandStorage.OnDemandLoadOnMissing;
                        OnDemandStorage.AddMap(generatedBody.name, cbMap);
                        Value = cbMap as T;
                    }
                    else
                    {
                        MapSODemand map = ScriptableObject.CreateInstance<MapSODemand>();
                        map.Path = s;
                        map.Depth = MapSO.MapDepth.RGB;
                        map.AutoLoad = OnDemandStorage.OnDemandLoadOnMissing;
                        OnDemandStorage.AddMap(generatedBody.name, map);
                        Value = map as T;
                    }
                }
                else
                {
                    // Load the texture
                    Texture2D map = Utility.LoadTexture(s, false, false, false);
                    if (map == null)
                    {
                        return;
                    }

                    // Create a new map script object
                    Value = ScriptableObject.CreateInstance<T>();
                    Value.CreateMap(MapSO.MapDepth.RGB, map);
                    Object.DestroyImmediate(map);
                }
            }

            if (Value != null)
            {
                Value.name = s;
            }
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            if (Value == null)
            {
                return null;
            }

            if (GameDatabase.Instance.ExistsTexture(Value.name) || OnDemandStorage.TextureExists(Value.name))
            {
                return Value.name;
            }

            return "BUILTIN/" + Value.name;
        }

        /// <summary>
        /// Create a new MapSOParser_RGB
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public MapSOParserRGB()
        {

        }

        /// <summary>
        /// Create a new MapSOParser_RGB from an already existing Texture
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public MapSOParserRGB(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator T(MapSOParserRGB<T> parser)
        {
            return parser.Value;
        }

        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator MapSOParserRGB<T>(T value)
        {
            return new MapSOParserRGB<T>(value);
        }
    }

    // Parser for a MapSO RGBA
    [RequireConfigType(ConfigType.Value)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class MapSOParserRGBA<T> : BaseLoader, IParsable, ITypeParser<T> where T : MapSO
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
            Boolean useOnDemand = OnDemandStorage.UseOnDemand;
            Boolean useOnDemandBiomes = OnDemandStorage.UseOnDemandBiomes;

            if (s.StartsWith("BUILTIN/"))
            {
                s = s.Substring(8);
                Value = Utility.FindMapSO<T>(s);
            }
            else
            {
                // check if OnDemand.
                if (useOnDemand && typeof(T) == typeof(MapSO) ||
                    useOnDemandBiomes && typeof(T) == typeof(CBAttributeMapSO))
                {
                    if (!Utility.TextureExists(s))
                    {
                        return;
                    }
                    if (typeof(T) == typeof(CBAttributeMapSO))
                    {
                        CBAttributeMapSODemand cbMap = ScriptableObject.CreateInstance<CBAttributeMapSODemand>();
                        cbMap.Path = s;
                        cbMap.Depth = MapSO.MapDepth.RGBA;
                        cbMap.AutoLoad = OnDemandStorage.OnDemandLoadOnMissing;
                        OnDemandStorage.AddMap(generatedBody.name, cbMap);
                        Value = cbMap as T;
                    }
                    else
                    {
                        MapSODemand map = ScriptableObject.CreateInstance<MapSODemand>();
                        map.Path = s;
                        map.Depth = MapSO.MapDepth.RGBA;
                        map.AutoLoad = OnDemandStorage.OnDemandLoadOnMissing;
                        OnDemandStorage.AddMap(generatedBody.name, map);
                        Value = map as T;
                    }
                }
                else
                {
                    // Load the texture
                    Texture2D map = Utility.LoadTexture(s, false, false, false);
                    if (map == null)
                    {
                        return;
                    }

                    // Create a new map script object
                    Value = ScriptableObject.CreateInstance<T>();
                    Value.CreateMap(MapSO.MapDepth.RGBA, map);
                    Object.DestroyImmediate(map);
                }
            }

            if (Value != null)
            {
                Value.name = s;
            }
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            if (Value == null)
            {
                return null;
            }

            if (GameDatabase.Instance.ExistsTexture(Value.name) || OnDemandStorage.TextureExists(Value.name))
            {
                return Value.name;
            }

            return "BUILTIN/" + Value.name;
        }

        /// <summary>
        /// Create a new MapSOParser_RGBA
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public MapSOParserRGBA()
        {

        }

        /// <summary>
        /// Create a new MapSOParser_RGB from an already existing Texture
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public MapSOParserRGBA(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator T(MapSOParserRGBA<T> parser)
        {
            return parser.Value;
        }

        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator MapSOParserRGBA<T>(T value)
        {
            return new MapSOParserRGBA<T>(value);
        }
    }

    // Parser for Physics Material
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class PhysicsMaterialParser : ITypeParser<PhysicMaterial>
    {
        // Physics material we are generating
        public PhysicMaterial Value { get; set; }

        // Physics material parameters
        [ParserTarget("bounceCombine")]
        public EnumParser<PhysicMaterialCombine> BounceCombine
        {
            get { return Value.bounceCombine; }
            set { Value.bounceCombine = value; }
        }

        [ParserTarget("frictionCombine")]
        public EnumParser<PhysicMaterialCombine> FrictionCombine
        {
            get { return Value.frictionCombine; }
            set { Value.frictionCombine = value; }
        }

        [ParserTarget("bounciness")]
        public NumericParser<Single> Bounciness
        {
            get { return Value.bounciness; }
            set { Value.bounciness = value; }
        }

        [ParserTarget("staticFriction")]
        public NumericParser<Single> StaticFriction
        {
            get { return Value.staticFriction; }
            set { Value.staticFriction = value; }
        }

        [ParserTarget("dynamicFriction")]
        public NumericParser<Single> DynamicFriction
        {
            get { return Value.dynamicFriction; }
            set { Value.dynamicFriction = value; }
        }

        /// <summary>
        /// Create a new PhysicsMaterialParser
        /// </summary>
        public PhysicsMaterialParser()
        {
            Value = new PhysicMaterial
            {
                name = "Ground",
                dynamicFriction = 0.6f,
                staticFriction = 0.8f,
                bounciness = 0.0f,
                frictionCombine = PhysicMaterialCombine.Maximum,
                bounceCombine = PhysicMaterialCombine.Average
            };
        }

        /// <summary>
        /// Create a new PhysicsMaterialParser from an already existing PhysicMaterial
        /// </summary>
        public PhysicsMaterialParser(PhysicMaterial material) : this()
        {
            if (material != null)
            {
                Value = material;
            }
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
                Value.name = s;
                return;
            }

            // Mesh was not found
            Value = null;
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            if (Value == null)
            {
                return null;
            }

            if (File.Exists(KSPUtil.ApplicationRootPath + "GameData/" + Value.name))
            {
                return Value.name;
            }

            return "BUILTIN/" + Value.name;
        }

        /// <summary>
        /// Create a new MeshParser
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public MeshParser()
        {

        }

        /// <summary>
        /// Create a new MeshParser from an already existing Mesh
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
    public class AssetParser<T> : IParsable, ITypeParser<T> where T : Object
    {
        /// <summary>
        /// The value that is being parsed
        /// </summary>
        public T Value { get; set; }

        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static readonly Dictionary<String, AssetBundle> Bundles = new Dictionary<String, AssetBundle>();

        /// <summary>
        /// Parse the Value from a string
        /// </summary>
        public void SetFromString(String s)
        {
            String[] split = s.Split(':');
            if (!File.Exists(KSPUtil.ApplicationRootPath + "GameData/" + split[0]))
            {
                Logger.Active.Log("Couldn't find asset file at path: " + KSPUtil.ApplicationRootPath + "GameData/" +
                                  split[0]);
                return;
            }

            AssetBundle bundle;
            if (!Bundles.ContainsKey(split[0]))
            {
                bundle = AssetBundle.LoadFromMemory(
                    File.ReadAllBytes(KSPUtil.ApplicationRootPath + "GameData/" + split[0]));
                Bundles.Add(split[0], bundle);
            }
            else
            {
                bundle = Bundles[split[0]];
            }

            Value = UnityEngine.Object.Instantiate(bundle.LoadAsset<T>(split[1]));
            Object.DontDestroyOnLoad(Value);
            Value.name = s;
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            return Value == null ? null : Value.name;
        }

        /// <summary>
        /// Create a new AssetParser
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public AssetParser()
        {

        }

        /// <summary>
        /// Create a new AssetParser from an already existing Value
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
            // Check if we are attempting to load a builtin game object
            if (s.StartsWith("BUILTIN/"))
            {
                String objName = Regex.Replace(s, "BUILTIN/", "");
                Value = UnityEngine.Object.Instantiate(Resources.FindObjectsOfTypeAll<GameObject>()
                    .First(obj => obj.name == objName));
                Value.name = objName;
                return;
            }

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
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            if (Value == null)
            {
                return null;
            }

            if (GameDatabase.Instance.ExistsModel(Value.name))
            {
                return Value.name;
            }

            return "BUILTIN/" + Value.name;
        }

        /// <summary>
        /// Create a new MuParser
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
            Value = Resources.FindObjectsOfTypeAll<Material>()
                .FirstOrDefault(material => material.name == materialName);
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            if (Value == null)
            {
                return null;
            }

            return "BUILTIN/" + Value.name;
        }

        /// <summary>
        /// Create a new StockMaterialParser
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public StockMaterialParser()
        {
        }

        /// <summary>
        /// Create a new StockMaterialParser from an already existing Material
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
            {
                return new StockMaterialParser();
            }

            Material m = new Material(material);
            return new StockMaterialParser {Value = m};
        }
    }
}
