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
		
		// Dump PQS members
		public static void DumpPQS(PQS s)
		{
			Debug.Log ("------------ PQS Data -----------");
			Debug.Log ("useSharedMaterial: " + s.useSharedMaterial);
			Debug.Log ("radius: " + s.radius);
			Debug.Log ("radiusSquared: " + s.radiusSquared);
			Debug.Log ("radiusDelta: " + s.radiusDelta);
			Debug.Log ("radiusMax: " + s.radiusMax);
			Debug.Log ("radiusMin: " + s.radiusMin);
			Debug.Log ("circumference: " + s.circumference);
			Debug.Log ("maxFrameEnd: " + s.maxFrameEnd);
			//Debug.Log ("normalUpdateList: " + s.normalUpdateList.Count);
			Debug.Log ("quadAllowBuild: " + s.quadAllowBuild);
			Debug.Log ("isSubdivisionEnabled: " + s.isSubdivisionEnabled);
			Debug.Log ("visibleRadius: " + s.visibleRadius);
			Debug.Log ("visRadDelta: " + s.visRadDelta);
			Debug.Log ("visRad: " + s.visRad);
			Debug.Log ("horizonDistance: " + s.horizonDistance);
			Debug.Log ("minDetailDistance: " + s.minDetailDistance);
			Debug.Log ("maxDetailDistance: " + s.maxDetailDistance);
			Debug.Log ("transformRotation: " + s.transformRotation);
			Debug.Log ("transformPosition: " + s.transformPosition);
			Debug.Log ("reqUV2: " + s.reqUV2);
			Debug.Log ("reqAssignTangets: " + s.reqAssignTangents);
			Debug.Log ("reqUVQuad: " + s.reqUVQuad);
			Debug.Log ("reqSphereUV: " + s.reqSphereUV);
			Debug.Log ("reqBuildTangents: " + s.reqBuildTangents);
			Debug.Log ("reqGnomonicCoords: " + s.reqGnomonicCoords);
			Debug.Log ("reqVertexMapCoords: " + s.reqVertexMapCoods);
			Debug.Log ("reqColorChannel: " + s.reqColorChannel);
			Debug.Log ("reqCustomNormals: " + s.reqCustomNormals);
			Debug.Log ("cancelUpdate: " + s.cancelUpdate);
			Debug.Log ("subdivisionThreshold: " + s.subdivisionThreshold);
			Debug.Log ("subdivisionThresholds: " + s.subdivisionThresholds);
			Debug.Log ("quadCount: " + s.quadCount);
			Debug.Log ("quadCounts: " + s.quadCounts);
			Debug.Log ("quads: " + s.quads);
			Debug.Log ("collapseThreshold: " + s.collapseThreshold);
			Debug.Log ("collapseThresholds: " + s.collapseThresholds);
			Debug.Log ("collapseDelta: " + s.collapseDelta);
			Debug.Log ("sx: " + s.sx);
			Debug.Log ("sy: " + s.sy);
			Debug.Log ("isFakeBuild: " + s.isFakeBuild);
			Debug.Log ("maxLevelAtCurrentTgtSpeed: " + s.maxLevelAtCurrentTgtSpeed);
			Debug.Log ("collapseSeaLevelValue: " + s.collapseSeaLevelValue);
			Debug.Log ("minLevel: " + s.minLevel);
			Debug.Log ("maxLevel: " + s.maxLevel);
			Debug.Log ("seed: " + s.seed);
			Debug.Log ("maxFrametime: " + s.maxFrameTime);
			Debug.Log ("meshCastShadows: " + s.meshCastShadows);
			Debug.Log ("meshReceiveShadows: " + s.meshRecieveShadows);
			if (s.surfaceMaterial != null) 
			{
				Debug.Log ("surfaceMaterial: " + s.surfaceMaterial.name);
				Debug.Log ("surfaceMaterialShader: " + s.surfaceMaterial.shader.name);
			}
			if (s.fallbackMaterial != null) 
			{
				Debug.Log ("fallbackMaterial: " + s.fallbackMaterial.name);
				Debug.Log ("fallbackMaterialShader: " + s.fallbackMaterial.shader.name);
			}
			Debug.Log ("frameTimeDelta: " + s.frameTimeDelta);
			Debug.Log ("isalive: " + s.isAlive);
			Debug.Log ("isactive: " + s.isActive);
			Debug.Log ("isdisabled: " + s.isDisabled);
			Debug.Log ("isstarted: " + s.isStarted);
			Debug.Log ("isthinking: " + s.isThinking);
			Debug.Log ("surfaceRelativeQuads: " + s.surfaceRelativeQuads);
			Debug.Log ("buildTangents: " + s.buildTangents);
			Debug.Log ("isBuildingMaps: " + s.isBuildingMaps);
			if (s.target)
				Debug.Log ("target: " + s.target.name);
			else
				Debug.Log ("target: null");
			Debug.Log ("targetSpeed: " + s.targetSpeed);
			Debug.Log ("targetHeight: " + s.targetHeight);
			Debug.Log ("targetVelocity: " + s.targetVelocity);
			Debug.Log ("relativeTargetPosition: " + s.relativeTargetPosition);
			Debug.Log ("relativeTargetPositionNormalized: " + s.relativeTargetPositionNormalized);
			Debug.Log ("detailAltitudeQuads: " + s.detailAltitudeQuads);
			Debug.Log ("detailAltitudeMax: " + s.detailAltitudeMax);
			Debug.Log ("detailDelta: " + s.detailDelta);
			Debug.Log ("detailSeaLevelQuads: " + s.detailSeaLevelQuads);
			Debug.Log ("detailRad: " + s.detailRad);
			Debug.Log ("mapOcean: " + s.mapOcean);
			Debug.Log ("mapOceanHeight: " + s.mapOceanHeight);
			Debug.Log ("mapOceanColor: " + s.mapOceanColor);
			Debug.Log ("mapFilename: " + s.mapFilename);
			Debug.Log ("mapFilesize: " + s.mapFilesize);
			Debug.Log ("mapMaxHeight: " + s.mapMaxHeight);
			Debug.Log ("--------------------------------");
		}
	}
}

