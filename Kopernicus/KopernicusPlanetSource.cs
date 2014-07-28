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
	// This class will add a planet. It will be supplanted by
	// versions which do more useful things, such as loading a planet from
	// configuration files, loading and modifying a stock planet from KSP's
	// resources (e.g. to allow modders to create modifications of the stock
	// Kerbol system, or to support backward compatibility with PlanetFactory),
	// or even procedurally generating a planet.
	// DOES NOT CURRENTLY HANDLE THE CASE OF GENERATING THE ROOT (Sun)
	// This code necessarily has side effects of execution, because of the way AddBody
	// works (we can't make a planet *not* in a system and return it.)
	public class KopernicusPlanetSource
	{
		public static PSystemBody GeneratePlanet (PSystem system) {
			return GenerateSystemBody (system, system.rootBody);
		}
		public static PSystemBody GenerateSystemBody(PSystem system, PSystemBody parent) {
			// AddBody makes the GameObject and stuff. It also attaches it to the system and
			// parent.
			PSystemBody body = system.AddBody (parent);

			// set up the various parameters
			body.name = "Kopernicus";
			body.orbitRenderer.orbitColor = Color.magenta;
			body.flightGlobalsIndex = 100;

			// Some parameters of the celestialBody, which represents the actual planet...
			// PSystemBody is more of a container that associates the planet with its orbit 
			// and position in the planetary system, etc.
			body.celestialBody.bodyName = "Kopernicus";
			body.celestialBody.Radius = 300000;
			body.celestialBody.GeeASL = 0.8; // This is g, not acceleration due to g, it turns out.
			body.celestialBody.gravParameter = 398600.0; // guessing this is the Standard gravitational parameter, i.e. mu
			// It appears that it calculates SOI for you if you give it this stuff.
			body.celestialBody.bodyDescription = "Merciful Kod, this thing just APPEARED! And unlike last time, it wasn't bird droppings on the telescope.";

			// Setup the orbit of "Kopernicus."  The "Orbit" class actually is built to support serialization straight
			// from Squad, so storing these to files (and loading them) will be pretty easy.
			body.orbitDriver.orbit = new Orbit (0.0, 0.0, 150000000000, 0, 0, 0, 0, system.rootBody.celestialBody);
			body.orbitDriver.celestialBody = body.celestialBody;
			body.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;
			body.orbitDriver.UpdateOrbit ();

			/** Relavent snippet from scaled version dump ok 
			 * [LOG 00:57:21.294] ---------- Scaled Version Dump -----------
			 * [LOG 00:57:21.294] Dres (UnityEngine.GameObject)
			 * [LOG 00:57:21.294]  >>> Components <<< 
			 * [LOG 00:57:21.294]  Dres (UnityEngine.Transform)
			 * [LOG 00:57:21.294]  Dres (UnityEngine.MeshFilter)
			 * [LOG 00:57:21.294]  Dres (UnityEngine.MeshRenderer)
			 * [LOG 00:57:21.294]  Dres (UnityEngine.SphereCollider)
			 * [LOG 00:57:21.294]  Dres (ScaledSpaceFader)
			 * [LOG 00:57:21.295]  >>> ---------- <<< 
			 * [LOG 00:57:21.295] -----------------------------------------
			 */

			// Temporarily clone the Dres scaled version for the structure
			// Find the dres prefab
			Debug.Log ("----- Scaled Verision -----");
			PSystemBody Dres = KopernicusUtility.FindBody (system.rootBody, "Eeloo");
			GameObject scaledVersion = (GameObject) UnityEngine.Object.Instantiate(Dres.scaledVersion);
			scaledVersion.name = "Kopernicus";
			body.scaledVersion = scaledVersion; 

			// Look at the transfor
			Debug.Log ("Body Radius: " + Dres.celestialBody.Radius);
			Debug.Log ("Scale: " + body.transform.localScale);

			// Look at the mesh renderer
			MeshRenderer renderer = scaledVersion.GetComponent<MeshRenderer> ();
			Debug.Log ("Material Name: " + renderer.material.name);

			// Look at the mesh filter
			MeshFilter filter = scaledVersion.GetComponent<MeshFilter> ();
			Debug.Log ("Mesh Name: " + filter.mesh.name);
			Debug.Log ("Mesh Vertex Count: " + filter.mesh.vertexCount);

			// Analyze the maximums and minimuns
			Vector3 maximum = new Vector3 (float.MinValue, float.MinValue, float.MinValue);
			Vector3 minimum = new Vector3 (float.MaxValue, float.MaxValue, float.MaxValue);
			foreach (Vector3 vertex in filter.mesh.vertices) 
			{
				minimum.x = (vertex.x < minimum.x) ? vertex.x : minimum.x;
				minimum.y = (vertex.y < minimum.y) ? vertex.y : minimum.y;
				minimum.z = (vertex.z < minimum.z) ? vertex.z : minimum.z;
				maximum.x = (vertex.x > maximum.x) ? vertex.x : maximum.x;
				maximum.y = (vertex.y > maximum.y) ? vertex.y : maximum.y;
				maximum.z = (vertex.z > maximum.z) ? vertex.z : maximum.z;
			}
			Debug.Log ("Mesh Vertex Minimums: " + minimum);
			Debug.Log ("Mesh Vertex Maximums: " + maximum);

			// Look at the sphere collider
			SphereCollider collider = scaledVersion.GetComponent<SphereCollider> ();
			Debug.Log ("Sphere Collider: Radius: " + collider.radius + ", {" + collider.center + "}");

			// Adjust the scaled space fader to our new celestial body
			ScaledSpaceFader fader = scaledVersion.GetComponent<ScaledSpaceFader> ();
			fader.celestialBody = body.celestialBody;

			// Print out other info
			Debug.Log ("FadeStart: " + fader.fadeStart + ", FadeEnd: " + fader.fadeEnd + ", FloatName: " + fader.floatName);
			Debug.Log ("------------------------------");

			// Return the new body
			return body;
		}


	}
}

