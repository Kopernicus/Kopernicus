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
using UnityEngine;

namespace Kopernicus
{
	public class KopernicusUtility
	{
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
			// Is this child in our immediate children
			Transform t = transform.Find (name);

			// If we did not find a transform, search through the children
			if (t == null) 
			{
				foreach (Transform child in transform) 
				{
					// Recurse into the child
					t = FindInChildren (child, name);
					if(t != null)
					{
						break;
					}
				}
			}

			// Return the transform (will be null if it was not found)
			return t;
		}

		/**
		 * Recursively searches for a component in the game object heirarchy
		 **/
		public static T RecursivelyGetComponent<T> (Transform transform) where T : Component
		{
			// Do we have this component?
			T component = transform.GetComponent<T> ();

			// If we didn't find the component, loop through children
			if (component == null) 
			{
				foreach(Transform child in transform)
				{
					// Is it in the child?
					component = RecursivelyGetComponent<T>(child);

					// If we found it, break out
					if(component != null) 
						break;
				}
			}

			// Return the component we found
			return component;
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
			if (body.celestialBody.name == name)
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

		/**
		 * Returns the local space object
		 */
		public static GameObject GetLocalSpace ()
		{
			return GameObject.Find (PSystemManager.Instance.localSpaceName);
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
		public static void GameObjectWalk(GameObject o, String prefix = "")
		{
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

		// Dump an object by reflection
		public static void DumpObject(object o, string title = "---------")
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
	}
}

