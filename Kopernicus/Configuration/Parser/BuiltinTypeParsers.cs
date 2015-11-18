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

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using Kopernicus.OnDemand;

namespace Kopernicus
{
    namespace Configuration
    {
        // Simple parser for numerics
        [RequireConfigType(ConfigType.Value)]
        public class NumericParser<T> : IParsable
        {
            public T value;
            public MethodInfo parserMethod;
            public void SetFromString(string s)
            {
                value = (T) parserMethod.Invoke (null, new object[] {s});
            }
            public NumericParser()
            {
                // Get the parse method for this object
                parserMethod = (typeof(T)).GetMethod ("Parse", new Type[] {(typeof(string))});
            }
            public NumericParser(T i) : this()
            {
                value = i;
            }

            // Convert
            public static implicit operator T(NumericParser<T> parser)
            {
                return parser.value;
            }
            public static implicit operator NumericParser<T>(T value)
            {
                return new NumericParser<T>(value);
            }
        }

        // Simple parser for numeric collections 
        [RequireConfigType(ConfigType.Value)]
        public class NumericCollectionParser<T> : IParsable
        {
            public List<T> value;
            public MethodInfo parserMethod;
            public void SetFromString (string s)
            {
                // Need a new list
                value = new List<T> ();

                // Get the tokens of this string
                foreach (string e in s.Split(' ')) 
                {
                    value.Add((T) parserMethod.Invoke (null, new object[] {e}));
                }
            }
            public NumericCollectionParser()
            {
                // Get the parse method for this object
                parserMethod = (typeof(T)).GetMethod ("Parse", new Type[] {(typeof(string))});
            }
            public NumericCollectionParser(T[] i) : this()
            {
                value = new List<T>(i);
            }
            public NumericCollectionParser(List<T> i) : this()
            {
                value = i;
            }

            // Convert
            public static implicit operator T[](NumericCollectionParser<T> parser)
            {
                return parser.value.ToArray();
            }
            public static implicit operator NumericCollectionParser<T>(T[] value)
            {
                return new NumericCollectionParser<T>(value);
            }
        }

        // Simple parser for string arrays
        [RequireConfigType(ConfigType.Value)]
        public class StringCollectionParser : IParsable
        {
            public IList<string> value;
            public void SetFromString (string s)
            {
                // Need a new list
                value = new List<string> (Regex.Replace (s, "\\s+", "").Split (','));
            }
            public StringCollectionParser()
            {
                value = new List<string> ();
            }
            public StringCollectionParser(string[] i)
            {
                value = new List<string>(i);
            }
            public StringCollectionParser(IList<string> i)
            {
                value = i;
            }

            // Convert
            public static implicit operator string[](StringCollectionParser parser)
            {
                return parser.value.ToArray();
            }
            public static implicit operator StringCollectionParser(string[] value)
            {
                return new StringCollectionParser(value);
            }
        }

        // Parser for color
        [RequireConfigType(ConfigType.Value)]
        public class ColorParser : IParsable
        {
            public Color value;
            public void SetFromString(string s)
            {
                if (s.StartsWith("RGBA("))
                {
                    s = s.Replace("RGBA(", string.Empty);
                    s = s.Replace(")", string.Empty);
                    s = s.Replace(" ", string.Empty);
                    string[] colorArray = s.Split(',');

                    value = new Color(float.Parse(colorArray[0]) / 255, float.Parse(colorArray[1]) / 255, float.Parse(colorArray[2]) / 255, float.Parse(colorArray[3]) / 255);
                }
                else if (s.StartsWith("RGB("))
                {
                    s = s.Replace("RGB(", string.Empty);
                    s = s.Replace(")", string.Empty);
                    s = s.Replace(" ", string.Empty);
                    string[] colorArray = s.Split(',');

                    value = new Color(float.Parse(colorArray[0]) / 255, float.Parse(colorArray[1]) / 255, float.Parse(colorArray[2]) / 255, 1);
                }
                else if (s.StartsWith("XKCD."))
                {
                    PropertyInfo color = typeof(XKCDColors).GetProperty(s.Replace("XKCD.", ""), BindingFlags.Static | BindingFlags.Public);
                    value = (Color)color.GetValue(null, null);
                }
                else if (s.StartsWith("#"))
                {
                    value = XKCDColors.ColorTranslator.FromHtml(s);
                }
                else
                {
                    value = ConfigNode.ParseColor(s);
                }
            }
            public ColorParser()
            {
                value = Color.white;
            }
            public ColorParser(Color i)
            {
                value = i;
            }

            // Convert
            public static implicit operator Color(ColorParser parser)
            {
                return parser.value;
            }
            public static implicit operator ColorParser(Color value)
            {
                return new ColorParser(value);
            }
        }

        // Parser for enum
        [RequireConfigType(ConfigType.Value)]
        public class EnumParser<T> : IParsable where T : struct, IConvertible
        {
            public T value;
            public void SetFromString(string s)
            {
                value = (T) (object) ConfigNode.ParseEnum(typeof (T), s);
            }
            public EnumParser ()
            {
                
            }
            public EnumParser (T i)
            {
                value = i;
            }

            // Convert
            public static implicit operator T(EnumParser<T> parser)
            {
                return parser.value;
            }
            public static implicit operator EnumParser<T>(T value)
            {
                return new EnumParser<T>(value);
            }
        }

        // Parser for quaternion
        [RequireConfigType(ConfigType.Value)]
        public class QuaternionParser : IParsable
        {
            public Quaternion value;
            public void SetFromString(string s)
            {
                value = ConfigNode.ParseQuaternion(s);
            }
            public QuaternionParser()
            {
                
            }
            public QuaternionParser(Quaternion value)
            {
                this.value = value;
            }

            // Convert
            public static implicit operator Quaternion(QuaternionParser parser)
            {
                return parser.value;
            }
            public static implicit operator QuaternionParser(Quaternion value)
            {
                return new QuaternionParser(value);
            }
        }

        // Parser for dual quaternion
        [RequireConfigType(ConfigType.Value)]
        public class QuaternionDParser : IParsable
        {
            public QuaternionD value;
            public void SetFromString(string s)
            {
                value = ConfigNode.ParseQuaternion(s);
            }
            public QuaternionDParser()
            {
                
            }
            public QuaternionDParser(QuaternionD value)
            {
                this.value = value;
            }

            // Convert
            public static implicit operator QuaternionD(QuaternionDParser parser)
            {
                return parser.value;
            }
            public static implicit operator QuaternionDParser(QuaternionD value)
            {
                return new QuaternionDParser(value);
            }
        }

        // Parser for vec2 
        [RequireConfigType(ConfigType.Value)]
        public class Vector2Parser : IParsable
        {
            public Vector2 value;
            public void SetFromString(string s)
            {
                value = ConfigNode.ParseVector2(s);
            }
            public Vector2Parser()
            {
                
            }
            public Vector2Parser(Vector2 value)
            {
                this.value = value;
            }

            // Convert
            public static implicit operator Vector2(Vector2Parser parser)
            {
                return parser.value;
            }
            public static implicit operator Vector2Parser(Vector2 value)
            {
                return new Vector2Parser(value);
            }
        }

        // Parser for vec3
        [RequireConfigType(ConfigType.Value)]
        public class Vector3Parser : IParsable
        {
            public Vector3 value;
            public void SetFromString(string s)
            {
                value = ConfigNode.ParseVector3(s);
            }
            public Vector3Parser()
            {
                
            }
            public Vector3Parser(Vector3 value)
            {
                this.value = value;
            }

            // Convert
            public static implicit operator Vector3(Vector3Parser parser)
            {
                return parser.value;
            }
            public static implicit operator Vector3Parser(Vector3 value)
            {
                return new Vector3Parser(value);
            }
        }

        // Alternative parser for Vector3
        [RequireConfigType(ConfigType.Node)]
        public class PositionParser : BaseLoader
        {
            // Latitude
            [ParserTarget("latitude", optional = true)]
            public NumericParser<double> latitude { get; set; }

            // Longitude
            [ParserTarget("longitude", optional = true)]
            public NumericParser<double> longitude { get; set; }

            // Altitude
            [ParserTarget("altitude", optional = true)]
            public NumericParser<double> altitude { get; set; }

            // Default Constructor
            public PositionParser()
            {
                latitude = new NumericParser<double>(0);
                longitude = new NumericParser<double>(0);
                altitude = new NumericParser<double>(0);
            }

            // Convert
            public static implicit operator Vector3(PositionParser parser)
            {
                return Utility.LLAtoECEF(parser.latitude, parser.longitude, parser.altitude, generatedBody.celestialBody.Radius);
            }
        }

        // Parser for vec3d
        [RequireConfigType(ConfigType.Value)]
        public class Vector3DParser : IParsable
        {
            public Vector3d value;
            public void SetFromString(string s)
            {
                value = ConfigNode.ParseVector3D(s);
            }
            public Vector3DParser()
            {
                
            }
            public Vector3DParser(Vector3d value)
            {
                this.value = value;
            }

            // Convert
            public static implicit operator Vector3d(Vector3DParser parser)
            {
                return parser.value;
            }
            public static implicit operator Vector3DParser(Vector3d value)
            {
                return new Vector3DParser(value);
            }
        }

        // Parser for vec4
        [RequireConfigType(ConfigType.Value)]
        public class Vector4Parser : IParsable
        {
            public Vector4 value;
            public void SetFromString(string s)
            {
                value = ConfigNode.ParseVector4(s);
            }
            public Vector4Parser()
            {

            }
            public Vector4Parser(Vector4 value)
            {
                this.value = value;
            }

            // Convert
            public static implicit operator Vector4(Vector4Parser parser)
            {
                return parser.value;
            }
            public static implicit operator Vector4Parser(Vector4 value)
            {
                return new Vector4Parser(value);
            }
        }

        // Parser for Texture2D
        [RequireConfigType(ConfigType.Value)]
        public class Texture2DParser : IParsable
        {
            public Texture2D value;
            public void SetFromString (string s)
            {
                // Check if we are attempting to load a builtin texture
                if (s.StartsWith ("BUILTIN/")) 
                {
                    string textureName = Regex.Replace (s, "BUILTIN/", "");
                    value = Resources.FindObjectsOfTypeAll<Texture>().Where(tex => tex.name == textureName).First() as Texture2D;
                    return;
                }

                // Otherwise search the game database for one loaded from GameData/
                else if (GameDatabase.Instance.ExistsTexture (s)) 
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
            public Texture2DParser ()
            {
                
            }
            public Texture2DParser (Texture2D value)
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
            public void SetFromString(string s)
            {
                // Should we use OnDemand?
                bool useOnDemand = OnDemandStorage.useOnDemand;

                if (s.StartsWith("BUILTIN/"))
                {
                    value = Utility.FindMapSO(s, typeof(T) == typeof(CBAttributeMapSO)) as T; // can't make built-in maps On-Demand.....yet... >:D
                }
                else
                {
                    // are we on-demand? Don't load now.
                    if (useOnDemand)
                    {
                        if (Utility.TextureExists(s))
                        {
                            string mapName = s;
                            mapName = mapName.Substring(s.LastIndexOf('/') + 1);
                            int lastDot = mapName.LastIndexOf('.');
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
                value.name = s;
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
            public void SetFromString(string s)
            {
                // Should we use OnDemand?
                bool useOnDemand = OnDemandStorage.useOnDemand;

                if (s.StartsWith("BUILTIN/"))
                {
                    value = Utility.FindMapSO(s, typeof(T) == typeof(CBAttributeMapSO)) as T;
                }
                else
                {
                    // check if OnDemand.
                    if (useOnDemand)
                    {
                        if (Utility.TextureExists(s))
                        {
                            string mapName = s;
                            mapName = mapName.Substring(s.LastIndexOf('/') + 1);
                            int lastDot = mapName.LastIndexOf('.');
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
                value.name = s;
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

        // Parser for a float curve
        [RequireConfigType(ConfigType.Node)]
        public class FloatCurveParser : IParserEventSubscriber
        {
            public FloatCurve curve { get; set; }

            // Build the curve from the data found in the node
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                curve = new FloatCurve ();
                curve.Load (node);
            }

            // We don't use this
            void IParserEventSubscriber.PostApply(ConfigNode node) {  }

            // Default constructor
            public FloatCurveParser ()
            {
                this.curve = null;
            }

            // Default constructor
            public FloatCurveParser (FloatCurve curve)
            {
                this.curve = curve;
            }

            // Convert
            public static implicit operator FloatCurve(FloatCurveParser parser)
            {
                return parser.curve;
            }
            public static implicit operator FloatCurveParser(FloatCurve value)
            {
                return new FloatCurveParser(value);
            }
        }

        // Parser for Physics Material
        [RequireConfigType(ConfigType.Node)]
        public class PhysicsMaterialParser : IParserEventSubscriber
        {
            // Physics material we are generating
            public PhysicMaterial material { get; set; }

            // Physics material parameters
            [ParserTarget("bounceCombine", optional = true)]
            public EnumParser<PhysicMaterialCombine> bounceCombine
            {
                get { return material.bounceCombine; }
                set { material.bounceCombine = value; }
            }

            [ParserTarget("frictionCombine", optional = true)]
            public EnumParser<PhysicMaterialCombine> frictionCombine
            {
                get { return material.frictionCombine; }
                set { material.frictionCombine = value; }
            }
            
            [ParserTarget("frictionDirection2", optional = true)]
            public Vector3Parser frictionDirection2
            {
                get { return material.frictionDirection2; }
                set { material.frictionDirection2 = value; }
            }

            [ParserTarget("bounciness", optional = true)]
            public NumericParser<float> bounciness
            {
                get { return material.bounciness; }
                set { material.bounciness = value; }
            }           
            
            [ParserTarget("staticFriction", optional = true)]
            public NumericParser<float> staticFriction
            { 
                get { return material.staticFriction; }
                set { material.staticFriction = value; }
            }
            
            [ParserTarget("staticFriction2", optional = true)]
            public NumericParser<float> staticFriction2
            {
                get { return material.staticFriction2; }
                set { material.staticFriction2 = value.value; }
            }
            
            [ParserTarget("dynamicFriction", optional = true)]
            public NumericParser<float> dynamicFriction
            {
                get { return material.dynamicFriction; }
                set { material.dynamicFriction = value.value; }
            }
            
            [ParserTarget("dynamicFriction2", optional = true)]
            public NumericParser<float> dynamicFriction2
            {
                get { return material.dynamicFriction2; }
                set { material.dynamicFriction2 = value.value; }
            }

            void IParserEventSubscriber.Apply(ConfigNode node) { }
            void IParserEventSubscriber.PostApply(ConfigNode node) { }

            // Default constructor
            public PhysicsMaterialParser()
            {
                this.material = null;
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
            public void SetFromString (string s)
            {
                // Check if we are attempting to load a builtin mesh
                if (s.StartsWith ("BUILTIN/")) 
                {
                    string meshName = Regex.Replace (s, "BUILTIN/", "");
                    value = UnityEngine.Resources.FindObjectsOfTypeAll<Mesh> ().Where (mesh => mesh.name == meshName).First ();
                    return;
                }

                String path = KSPUtil.ApplicationRootPath + "GameData/" + s;
                if (System.IO.File.Exists(path))
                {
                    value = ObjImporter.ImportFile(path);
                    value.name = Path.GetFileNameWithoutExtension(path);
                    return;
                }

                // Mesh was not found
                value = null;
            }
            public MeshParser ()
            {
                
            }
            public MeshParser (Mesh value)
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

        // parser for .mu
		[RequireConfigType(ConfigType.Value)]
		public class MuParser : IParsable
		{
			public GameObject value;

			public void SetFromString (string s)
			{
				// If there's a model, import it
				if (GameDatabase.Instance.ExistsModel (s))
				{
					value = GameDatabase.Instance.GetModel (s);
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
