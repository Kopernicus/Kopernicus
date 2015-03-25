/**
 * Kopernicus Planetary System Modifier
 * Copyright (C) 2014 Bryce C Schroeder (bryce.schroeder@gmail.com), Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * http://www.ferazelhosting.net/~bryce/contact.html
 * 
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
using System.Reflection;
using System.Collections.Generic;

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

		/** Parser for color */
		[RequireConfigType(ConfigType.Value)]
		public class ColorParser : IParsable
		{
			public Color value;
			public void SetFromString(string s)
			{
				value = ConfigNode.ParseColor(s);
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
				// Throw exception if this texture doesn't exist
				if (!GameDatabase.Instance.ExistsTexture (s)) 
				{
					throw new Exception("Texture \"" + s + "\" not found");
				}

				// Get the texture URL
				value = GameDatabase.Instance.GetTexture(s, false);
			}
			public Texture2DParser ()
			{
				
			}
			public Texture2DParser (Texture2D value)
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

		/** Parser for animation curve **/
		[RequireConfigType(ConfigType.Node)]
		public class AnimationCurveParser : IParserEventSubscriber
		{
			// Animation curve we are generating
			public AnimationCurve curve { get; private set; }

			// Build the curve from data found in the node
			void IParserEventSubscriber.Apply(ConfigNode node)
			{
				// List of keyframes
				SortedList<int, Keyframe> keyframes = new SortedList<int, Keyframe>();

				// Iterate through all the values in the node (all are keyframes)
				foreach(ConfigNode.Value frame in node.values)
				{
					// Convert the "name" (left side) into an int for sorting
					int key = int.Parse(frame.name);

					// Get an array of the frame data
					List<float> value = new List<float> ();
					foreach (string e in frame.value.Split(' ')) 
						value.Add(float.Parse(e));

					// Build the keyframe
					Keyframe keyframe;
					if(value.Count == 2)
						keyframe = new Keyframe(value[0], value[1]);
					else if(value.Count == 4)
						keyframe = new Keyframe(value[0], value[1], value[2], value[3]);
					else
						throw new Exception("Keyframe consists of either 2 or 4 floats");

					// Add the keyframe to the list
					keyframes.Add(key, keyframe);
				}

				// Create the final animation curve
				curve = new AnimationCurve();
				foreach(KeyValuePair<int, Keyframe> keyframe in keyframes)
					curve.AddKey(keyframe.Value);
			}

			// We don't use this
			void IParserEventSubscriber.PostApply(ConfigNode node) { }

			// Construct this fine object
			public AnimationCurveParser (AnimationCurve curve = null)
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

			// Initializing constructor
			public PhysicsMaterialParser (PhysicMaterial material = null)
			{
				this.material = material;
			}
		}
	}
}

