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
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Kopernicus
{
	public class Utility
	{
		/**
		 * @return LocalSpace game object
		 */
		public static GameObject LocalSpace
		{
			get { return GameObject.Find (PSystemManager.Instance.localSpaceName); }
		}

		// Static object representing the deactivator
		private static GameObject deactivator;

		/**
		 * Get an object which is deactivated, essentially, and children are prefabs
		 * @return shared deactivated object for making prefabs
		 */
		public static Transform Deactivator
		{
			get
			{
				if(deactivator == null)
				{
					deactivator = new GameObject ("__deactivator");
					deactivator.SetActive (false);
					UnityEngine.Object.DontDestroyOnLoad (deactivator);
				}
				return deactivator.transform;
			}
		}
		
		/**
		 * Copy one objects fields to another object via reflection
		 * @param source Object to copy fields from
		 * @param destination Object to copy fields to
		 **/
		public static void CopyObjectFields<T> (T source, T destination)
		{
			// Reflection based copy
			foreach (FieldInfo field in (typeof (T)).GetFields()) 
			{
				// Only copy non static fields
				if (!field.IsStatic)
				{
					field.SetValue(destination, field.GetValue(source));
				}
			}
		}

		/**
		 * Recursively searches for a named transform in the Transform heirarchy.  The requirement of
		 * such a function is sad.  This should really be in the Unity3D API.  Transform.Find() only
		 * searches in the immediate children.
		 *
		 * @param transform Transform in which is search for named child
		 * @param name Name of child to find
		 * 
		 * @return Desired transform or null if it could not be found
		 */
		public static Transform FindInChildren (Transform transform, string name)
		{
			// Is this null?
			if (transform == null) 
			{
				return null;
			}

			// Are the names equivalent
			if (transform.name == name) 
			{
				return transform;
			}

			// If we did not find a transform, search through the children
			foreach (Transform child in transform) 
			{
				// Recurse into the child
				Transform t = FindInChildren (child, name);
				if(t != null)
				{
					return t;
				}
			}

			// Return the transform (will be null if it was not found)
			return null;
		}
		
		// Dump an object by reflection
		public static void DumpObjectFields(object o, string title = "---------")
		{
			// Dump the raw PQS of Dres (by reflection)
			Debug.Log("---------" + title + "------------");
			foreach (FieldInfo field in o.GetType().GetFields()) 
			{
				if (!field.IsStatic)
				{
					Debug.Log (field.Name + " = " + field.GetValue (o));
				}
			}
			Debug.Log("--------------------------------------");
		}

		public static void DumpObjectProperties(object o, string title = "---------")
		{
			// Iterate through all of the properties
			Debug.Log("--------- " + title + " ------------");
			foreach (PropertyInfo property in o.GetType().GetProperties()) 
			{
				if(property.CanRead)
					Debug.Log (property.Name + " = " + property.GetValue(o, null));
			}
			Debug.Log("--------------------------------------");
		}

		/**
		 * Recursively searches for a named PSystemBody
		 *
		 * @param body Parent body to begin search in
		 * @param name Name of body to find
		 * 
		 * @return Desired body or null if not found
		 */
		public static PSystemBody FindBody (PSystemBody body, string name)
		{
			// Is this the body wer are looking for?
			if (body.celestialBody.bodyName == name)
				return body;

			// Otherwise search children
			foreach (PSystemBody child in body.children) 
			{
				PSystemBody b = FindBody(child, name);
				if(b != null) 
					return b;
			}

			// Return null because we didn't find shit
			return null;
		}

		// Print out a tree containing all the objects in the game
		public static void PerformObjectDump()
		{
			Debug.Log ("--------- Object Dump -----------");
			foreach (GameObject b in GameObject.FindObjectsOfType(typeof (GameObject))) 
			{
				// Essentially, we iterate through all game objects currently alive and search for 
				// the ones without a parent.  Extrememly inefficient and terrible, but its just for
				// exploratory purposes
				if(b.transform.parent == null)
				{
					// Print out the tree of child objects
					GameObjectWalk(b, "");
				}
			}
			Debug.Log ("---------------------------------");
		}

		// Print out the tree of components 
		public static void GameObjectWalk (GameObject o, String prefix = "")
		{
			// If null, don't do anything
			if (o == null) 
				return;

			// Print this object
			Debug.Log (prefix + o);
			Debug.Log (prefix + " >>> Components <<< ");
			foreach (Component c in o.GetComponents(typeof(Component))) 
			{
				Debug.Log(prefix + " " + c);
			}
			Debug.Log (prefix + " >>> ---------- <<< ");
			
			// Game objects are related to each other via transforms in Unity3D.
			foreach (Transform b in o.transform) 
			{
				if(b != null)
					GameObjectWalk(b.gameObject, "    " + prefix);
			}
		}

		// Print out the celestial bodies
		public static void PSystemBodyWalk(PSystemBody b, String prefix = "")
		{
			Debug.Log (prefix + b.celestialBody.bodyName + ":" + b.flightGlobalsIndex);
			foreach (PSystemBody c in b.children) 
			{
				PSystemBodyWalk(c, prefix + "    ");
			}
		}

		public static void CopyMesh(Mesh source, Mesh dest)
		{
			//ProfileTimer.Push("CopyMesh");
			Vector3[] verts = new Vector3[source.vertexCount];
			source.vertices.CopyTo(verts, 0);
			dest.vertices = verts;
			
			int[] tris = new int[source.triangles.Length];
			source.triangles.CopyTo(tris, 0);
			dest.triangles = tris;
			
			Vector2[] uvs = new Vector2[source.uv.Length];
			source.uv.CopyTo(uvs, 0);
			dest.uv = uvs;
			
			Vector2[] uv2s = new Vector2[source.uv2.Length];
			source.uv2.CopyTo(uv2s, 0);
			dest.uv2 = uv2s;
			
			Vector3[] normals = new Vector3[source.normals.Length];
			source.normals.CopyTo(normals, 0);
			dest.normals = normals;
			
			Vector4[] tangents = new Vector4[source.tangents.Length];
			source.tangents.CopyTo(tangents, 0);
			dest.tangents = tangents;
			
			Color[] colors = new Color[source.colors.Length];
			source.colors.CopyTo(colors, 0);
			dest.colors = colors;
			
			Color32[] colors32 = new Color32[source.colors32.Length];
			source.colors32.CopyTo(colors32, 0);
			dest.colors32 = colors32;
			
			//ProfileTimer.Pop("CopyMesh");
		}

		public static Mesh DuplicateMesh(Mesh source)
		{
			// Create new mesh object
			Mesh dest = new Mesh();

			//ProfileTimer.Push("CopyMesh");
			Vector3[] verts = new Vector3[source.vertexCount];
			source.vertices.CopyTo(verts, 0);
			dest.vertices = verts;
			
			int[] tris = new int[source.triangles.Length];
			source.triangles.CopyTo(tris, 0);
			dest.triangles = tris;
			
			Vector2[] uvs = new Vector2[source.uv.Length];
			source.uv.CopyTo(uvs, 0);
			dest.uv = uvs;
			
			Vector2[] uv2s = new Vector2[source.uv2.Length];
			source.uv2.CopyTo(uv2s, 0);
			dest.uv2 = uv2s;
			
			Vector3[] normals = new Vector3[source.normals.Length];
			source.normals.CopyTo(normals, 0);
			dest.normals = normals;
			
			Vector4[] tangents = new Vector4[source.tangents.Length];
			source.tangents.CopyTo(tangents, 0);
			dest.tangents = tangents;
			
			Color[] colors = new Color[source.colors.Length];
			source.colors.CopyTo(colors, 0);
			dest.colors = colors;
			
			Color32[] colors32 = new Color32[source.colors32.Length];
			source.colors32.CopyTo(colors32, 0);
			dest.colors32 = colors32;
			
			//ProfileTimer.Pop("CopyMesh");
			return dest;
		}

		// Taken from Nathankell's RSS Utils.cs; uniformly scaled vertices
		public static void ScaleVerts(Mesh mesh, float scaleFactor)
		{
			//ProfileTimer.Push("ScaleVerts");
			Vector3[] vertices = new Vector3[mesh.vertexCount];
			for (int i = 0; i < mesh.vertexCount; i++)
			{
				Vector3 v = mesh.vertices[i];
				v *= scaleFactor;
				vertices[i] = v;
			}
			mesh.vertices = vertices;
			//ProfileTimer.Pop("ScaleVerts");
		}

		// Serialize a mesh to disk
		public static void SerializeMesh(Mesh mesh, string path)
		{
			// Open an output filestream
			System.IO.FileStream outputStream = new System.IO.FileStream (path, System.IO.FileMode.Create, System.IO.FileAccess.Write);
			System.IO.BinaryWriter writer = new System.IO.BinaryWriter (outputStream);

			// Write the vertex count of the mesh
			writer.Write (mesh.vertices.Length);
			foreach(Vector3 vertex in mesh.vertices) 
			{
				writer.Write (vertex.x);
				writer.Write (vertex.y);
				writer.Write (vertex.z);
			}
			writer.Write (mesh.uv.Length);
			foreach(Vector2 uv in mesh.uv) 
			{
				writer.Write (uv.x);
				writer.Write (uv.y);
			}
			writer.Write (mesh.triangles.Length);
			foreach(int triangle in mesh.triangles) 
			{
				writer.Write(triangle);
			}

			// Finish writing
			writer.Close ();
			outputStream.Close ();
		}

		// Deserialize a mesh from disk
		public static Mesh DeserializeMesh(string path)
		{
			System.IO.FileStream inputStream = new System.IO.FileStream (path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			System.IO.BinaryReader reader = new System.IO.BinaryReader (inputStream);

			// Get the vertices
			int count = reader.ReadInt32 ();
			Vector3[] vertices = new Vector3[count];
			for (int i = 0; i < count; i++) 
			{
				Vector3 vertex;
				vertex.x = reader.ReadSingle ();
				vertex.y = reader.ReadSingle ();
				vertex.z = reader.ReadSingle ();
				vertices [i] = vertex;
			}

			// Get the uvs
			int uv_count = reader.ReadInt32 ();
			Vector2[] uvs = new Vector2[uv_count];
			for (int i = 0; i < uv_count; i++) 
			{
				Vector2 uv;
				uv.x = reader.ReadSingle();
				uv.y = reader.ReadSingle();
				uvs[i] = uv;
			}

			// Get the triangles
			int tris_count = reader.ReadInt32 ();
			int[] triangles = new int[tris_count];
			for (int i = 0; i < tris_count; i++)
				triangles [i] = reader.ReadInt32 ();

			// Close
			reader.Close ();
			inputStream.Close ();

			// Create the mesh
			Mesh m = new Mesh ();
			m.vertices = vertices;
			m.triangles = triangles;
			m.uv = uvs;
			m.RecalculateNormals ();
			m.RecalculateBounds ();
			return m;
		}

		/** 
		 * Enumerable class to iterate over parents.  Defined to allow us to use Linq
		 * and predicates. 
		 *
		 * See examples: http://msdn.microsoft.com/en-us/library/78dfe2yb(v=vs.110).aspx
		 **/
		public class ParentEnumerator : IEnumerable<GameObject>
		{
			// The game object who and whose parents are going to be enumerated
			private GameObject initial;

			// Enumerator class
			public class Enumerator : IEnumerator<GameObject>
			{
				public GameObject original;
				public GameObject current;

				public Enumerator (GameObject initial)
				{
					this.original = initial;
					this.current = this.original;
				}

				public bool MoveNext ()
				{
					if (current.transform.parent != null && current.transform.parent == current.transform) 
					{
						current = current.transform.parent.gameObject;
						return true;
					} 
					else 
					{
						return false;
					}
				}

				public void Reset ()
				{	
					current = original;
				}

				void IDisposable.Dispose () { }

				public GameObject Current 
				{
					get { return current; }
				}

				object IEnumerator.Current 
				{
					get { return Current; }
				}
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(initial);
			}

		    IEnumerator IEnumerable.GetEnumerator()
			{
				return (IEnumerator) GetEnumerator();
			}
			
			IEnumerator<GameObject> IEnumerable<GameObject>.GetEnumerator()
			{
				return (IEnumerator<GameObject>) GetEnumerator();
			}
			
			public ParentEnumerator (GameObject initial)
			{
				this.initial = initial;
			}
		}

		/** 
		 * Enumerable class to iterate over parents.  Defined to allow us to use Linq
		 * and predicates.  Allows this fun operation to find a sun closest to us under 
		 * the tree
		 * 
		 *     Utility.ReferenceBodyEnumerator e = new Utility.ReferenceBodyEnumerator(FlightGlobals.currentMainBody);
		 *     CelestialBody sun = e.First(p => p.GetComponentsInChildren(typeof (ScaledSun), true).Length > 0);
		 *
		 * See examples: http://msdn.microsoft.com/en-us/library/78dfe2yb(v=vs.110).aspx
		 **/
		public class ReferenceBodyEnumerator : IEnumerable<CelestialBody>
		{
			// The game object who and whose parents are going to be enumerated
			private CelestialBody initial;
			
			// Enumerator class
			public class Enumerator : IEnumerator<CelestialBody>
			{
				public CelestialBody original;
				public CelestialBody current;
				
				public Enumerator (CelestialBody initial)
				{
					this.original = initial;
					this.current = this.original;
				}
				
				public bool MoveNext ()
				{
					if (current.referenceBody != null) 
					{
						current = current.referenceBody;
						return true;
					} 
				
					return false;
				}
				
				public void Reset ()
				{	
					current = original;
				}
				
				void IDisposable.Dispose () { }
				
				public CelestialBody Current 
				{
					get { return current; }
				}
				
				object IEnumerator.Current 
				{
					get { return Current; }
				}
			}
			
			public Enumerator GetEnumerator()
			{
				return new Enumerator(initial);
			}
			
			IEnumerator IEnumerable.GetEnumerator()
			{
				return (IEnumerator) GetEnumerator();
			}
			
			IEnumerator<CelestialBody> IEnumerable<CelestialBody>.GetEnumerator()
			{
				return (IEnumerator<CelestialBody>) GetEnumerator();
			}
			
			public ReferenceBodyEnumerator (CelestialBody initial)
			{
				this.initial = initial;
			}
		}
	}
}

