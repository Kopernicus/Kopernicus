/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
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

namespace Kopernicus
{
    namespace Configuration
    {
        /**
         * Simple parser for numerics
         **/
        [RequireConfigType(ConfigType.Value)]
        public class NumericParser<T> : IParsable
        {
            public T value;
            private MethodInfo parserMethod;
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
        }

        /* Simple parser for numeric collections */
        [RequireConfigType(ConfigType.Value)]
        public class NumericCollectionParser<T> : IParsable
        {
            public List<T> value;
            private MethodInfo parserMethod;
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
        }

        /** Simple parser for string arrays */
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
        }

        /** Parser for color */
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
        }
        
        /** Parser for color32 */
        [RequireConfigType(ConfigType.Value)]
        public class Color32Parser : IParsable
        {
            public Color32 value;
            public void SetFromString(string s)
            {
                value = ConfigNode.ParseColor32(s);
            }
            public Color32Parser()
            {
                
            }
        }

        /** Parser for enum */
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
        }
        
        /** Parser for matrix 4x4 */
        [RequireConfigType(ConfigType.Value)]
        public class Matrix4x4Parser : IParsable 
        {
            public Matrix4x4 value;
            public void SetFromString(string s)
            {
                value = ConfigNode.ParseMatrix4x4(s);
            }
            public Matrix4x4Parser ()
            {
                
            }
        }
        
        /** Parser for quaternion */
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
        }
        
        /** Parser for dual quaternion */
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
        }
        
        /** Parser for vec2 **/
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
        }
        
        /** Parser for vec3 **/
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
        }
        
        /** Parser for vec3d **/
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
        }
        
        /** Parser for vec4 **/
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
        }
        
        /** Parser for Texture2D **/
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
                    value = UnityEngine.Resources.FindObjectsOfTypeAll<Texture2D> ().Where (tex => tex.name == textureName).First ();
                    return;
                }

                // Otherwise search the game database for one loaded from GameData/
                else if (GameDatabase.Instance.ExistsTexture (s)) 
                {
                    // Get the texture URL
                    value = GameDatabase.Instance.GetTexture(s, false);
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
        }

        /** Parser for a MapSO */
        public class MapSOParser_GreyScale<T> : IParsable where T : MapSO
        {
            public T value;
            public void SetFromString (string s)
            {
                bool useOnDemand = (typeof(T) == typeof(CBAttributeMapSO)) ? OnDemand.OnDemandStorage.useOnDemandBiomes : OnDemand.OnDemandStorage.useOnDemand;
                if (Templates.instance.mapsGray != null && Templates.instance.mapsGray.ContainsKey(s))
                {
                    value = (T)Templates.instance.mapsGray[s];
                    if (useOnDemand)
                    {
                        value.name += ", and " + OnDemand.OnDemandStorage.currentBody;
                        OnDemand.OnDemandStorage.AddMap(OnDemand.OnDemandStorage.currentBody, (OnDemand.ILoadOnDemand)value);
                    }
                }
                else
                {
                    T obj = null;
                    if (s.StartsWith("BUILTIN/"))
                    {
                        obj = Utility.FindMapSO(s) as T;
                    }
                    if (obj != null)
                    {
                        value = obj; // can't make built-in maps On-Demand
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
                                    OnDemand.CBAttributeMapSODemand valCB = ScriptableObject.CreateInstance<OnDemand.CBAttributeMapSODemand>();
                                    valCB.SetPath(s);
                                    valCB.Depth = MapSO.MapDepth.Greyscale;
                                    valCB.name = mapName + " (CBG) for " + OnDemand.OnDemandStorage.currentBody;
                                    valCB.SetAutoLoad(OnDemand.OnDemandStorage.onDemandLoadOnMissing);

                                    OnDemand.OnDemandStorage.AddMap(OnDemand.OnDemandStorage.currentBody, valCB);
                                    value = valCB as T;
                                }
                                else
                                {
                                    OnDemand.MapSODemand valMap = ScriptableObject.CreateInstance<OnDemand.MapSODemand>();
                                    valMap.SetPath(s);
                                    valMap.Depth = MapSO.MapDepth.Greyscale;
                                    valMap.name = mapName + " (G) for " + OnDemand.OnDemandStorage.currentBody;
                                    valMap.SetAutoLoad(OnDemand.OnDemandStorage.onDemandLoadOnMissing);

                                    OnDemand.OnDemandStorage.AddMap(OnDemand.OnDemandStorage.currentBody, valMap);
                                    value = valMap as T;
                                }
                                Templates.instance.mapsGray[s] = value;
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
                                Templates.instance.mapsGray[s] = value;
                            }
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
        }

        /** Parser for a MapSO */
        public class MapSOParser_RGB<T> : IParsable where T : MapSO
        {
            public T value;
            public void SetFromString(string s)
            {
                bool useOnDemand = (typeof(T) == typeof(CBAttributeMapSO)) ? OnDemand.OnDemandStorage.useOnDemandBiomes : OnDemand.OnDemandStorage.useOnDemand;
                if (Templates.instance.mapsRGB != null && Templates.instance.mapsRGB.ContainsKey(s))
                {
                    value = (T)Templates.instance.mapsRGB[s];

                    if (useOnDemand)
                    {
                        value.name += ", and " + OnDemand.OnDemandStorage.currentBody;
                        OnDemand.OnDemandStorage.AddMap(OnDemand.OnDemandStorage.currentBody, (OnDemand.ILoadOnDemand)value);
                    }
                }
                else
                {
                    T obj = null;
                    if (s.StartsWith("BUILTIN/"))
                    {
                        obj = Utility.FindMapSO(s) as T;
                    }
                    if (obj != null)
                    {
                        value = obj; // can't make built-in maps On-Demand
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
                                    OnDemand.CBAttributeMapSODemand valCB = ScriptableObject.CreateInstance<OnDemand.CBAttributeMapSODemand>();
                                    valCB.SetPath(s);
                                    valCB.Depth = MapSO.MapDepth.RGB;
                                    valCB.name = mapName + " (CBRGB) for " + OnDemand.OnDemandStorage.currentBody;
                                    valCB.SetAutoLoad(OnDemand.OnDemandStorage.onDemandLoadOnMissing);

                                    OnDemand.OnDemandStorage.AddMap(OnDemand.OnDemandStorage.currentBody, valCB);
                                    value = valCB as T;
                                }
                                else
                                {
                                    OnDemand.MapSODemand valMap = ScriptableObject.CreateInstance<OnDemand.MapSODemand>();
                                    valMap.SetPath(s);
                                    valMap.Depth = MapSO.MapDepth.RGB;
                                    valMap.name = mapName + " (RGB) for " + OnDemand.OnDemandStorage.currentBody;
                                    valMap.SetAutoLoad(OnDemand.OnDemandStorage.onDemandLoadOnMissing);

                                    OnDemand.OnDemandStorage.AddMap(OnDemand.OnDemandStorage.currentBody, valMap);
                                    value = valMap as T;
                                }
                                Templates.instance.mapsRGB[s] = value;
                                Debug.Log("OD: created map of path " + s);
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
                                Templates.instance.mapsRGB[s] = value;
                            }
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
        }

        /** Parser for a float curve **/
        [RequireConfigType(ConfigType.Node)]
        public class FloatCurveParser : IParserEventSubscriber
        {
            public FloatCurve curve { get; private set; }

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
        }

        /** Parser for Physics Material **/
        [RequireConfigType(ConfigType.Node)]
        public class PhysicsMaterialParser : IParserEventSubscriber
        {
            // Physics material we are generating
            public PhysicMaterial material { get; private set; }

            // Physics material parameters
            [ParserTarget("bounceCombine", optional = true)]
            private EnumParser<PhysicMaterialCombine> bounceCombine
            {
                set { material.bounceCombine = value.value; }
            }

            [ParserTarget("frictionCombine", optional = true)]
            private EnumParser<PhysicMaterialCombine> frictionCombine
            {
                set { material.frictionCombine = value.value; }
            }
            
            [ParserTarget("frictionDirection2", optional = true)]
            private Vector3Parser frictionDirection2
            {
                set { material.frictionDirection2 = value.value; }
            }

            [ParserTarget("bounciness", optional = true)]
            private NumericParser<float> bounciness
            {
                set { material.bounciness = value.value; }
            }
            
            [ParserTarget("staticFriction", optional = true)]
            private NumericParser<float> staticFriction
            {
                set { material.staticFriction = value.value; }
            }
            
            [ParserTarget("staticFriction2", optional = true)]
            private NumericParser<float> staticFriction2
            {
                set { material.staticFriction2 = value.value; }
            }
            
            [ParserTarget("dynamicFriction", optional = true)]
            private NumericParser<float> dynamicFriction
            {
                set { material.dynamicFriction = value.value; }
            }
            
            [ParserTarget("dynamicFriction2", optional = true)]
            private NumericParser<float> dynamicFriction2
            {
                set { material.dynamicFriction2 = value.value; }
            }

            void IParserEventSubscriber.Apply(ConfigNode node) { }
            void IParserEventSubscriber.PostApply(ConfigNode node) { }

            // Default constructor
            public PhysicsMaterialParser ()
            {
                this.material = null;
            }

            // Initializing constructor
            public PhysicsMaterialParser (PhysicMaterial material)
            {
                this.material = material;
            }
        }

        /** Parser for mesh */
        [RequireConfigType(ConfigType.Value)]
        public class MeshParser : IParsable
        {
            public Mesh mesh;
            public GameObject Object;
            public void SetFromString (string s)
            {
                // Check if we are attempting to load a builtin mesh
                if (s.StartsWith("BUILTIN/"))
                {
                    try
                    {
                        string objectName = Regex.Replace(s, "BUILTIN/", "");
                        Object = Resources.FindObjectsOfTypeAll<GameObject>().Where(o => o.name == objectName).First();
                        List<MeshFilter> filters = Object.GetComponentsInChildren<MeshFilter>(true).ToList();
                        List<CombineInstance> combines = new List<CombineInstance>();
                        filters.ForEach(f => combines.Add(new CombineInstance() { mesh = f.sharedMesh }));
                        mesh = new Mesh();
                        mesh.CombineMeshes(combines.ToArray());
                    }
                    catch { }

                    // Check if we've found a mesh, otherwise try to find it directly
                    if (mesh == null)
                    {
                        string meshName = Regex.Replace(s, "BUILTIN/", "");
                        mesh = Resources.FindObjectsOfTypeAll<Mesh>().Where(m => m.name == meshName).First();
                        Object = new GameObject(meshName);
                        Object.AddComponent<MeshFilter>().sharedMesh = mesh;
                        MonoBehaviour.DontDestroyOnLoad(Object);
                    }
                }
                else if (s.EndsWith(".obj")) // Check which Mesh-Type we're loading
                {
                    string path = KSPUtil.ApplicationRootPath + "GameData/" + s;
                    if (File.Exists(path))
                    {
                        mesh = ObjImporter.ImportFile(path);
                        mesh.name = Path.GetFileNameWithoutExtension(path);
                        Object = new GameObject(mesh.name);
                        UnityEngine.Object.DontDestroyOnLoad(Object);
                        Object.AddComponent<MeshFilter>().sharedMesh = mesh;
                        return;
                    }
                }
                else if (s.EndsWith(".mu"))
                {
                    // If there's a model, import it
                    string path = Regex.Replace(s, ".mu", "");
                    if (GameDatabase.Instance.ExistsModel(path))
                    {
                        Object = GameDatabase.Instance.GetModel(path);
                        List<MeshFilter> filters = Object.GetComponentsInChildren<MeshFilter>(true).ToList();
                        List<CombineInstance> combines = new List<CombineInstance>();
                        filters.ForEach(f => combines.Add(new CombineInstance() { mesh = f.sharedMesh }));
                        mesh = new Mesh();
                        mesh.CombineMeshes(combines.ToArray());
                        return;
                    }
                }

                // Mesh was not found
                mesh = null;
                Object = null;
            }
            public MeshParser ()
            {
                
            }
            public MeshParser (Mesh mesh)
            {
                this.mesh = mesh;
            }
            public MeshParser(GameObject Object)
            {
                this.Object = Object;
            }
            public MeshParser(GameObject Object, Mesh mesh)
            {
                this.Object = Object;
                this.mesh = mesh;
            }
        }
    }
}
