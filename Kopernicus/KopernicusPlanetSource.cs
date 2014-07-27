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
		public static PSystemBody GeneratePlanet (PSystem system, Orbit orbit = null) {
			return GenerateSystemBody (system, system.rootBody, orbit);
		}
		public static PSystemBody GenerateSystemBody(PSystem system, PSystemBody parent, Orbit orbit = null) {
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
			if (orbit == null)
				body.orbitDriver.orbit = new Orbit (0.0, 0.0, 150000000000, 0, 0, 0, 0, system.rootBody.celestialBody);
			else
				body.orbitDriver.orbit = orbit;

			body.orbitDriver.celestialBody = body.celestialBody;
			body.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;
			body.orbitDriver.UpdateOrbit ();

			// Temporarily clone the Dres scaled version for the structure
			// Find the dres prefab
			PSystemBody Dres = KopernicusUtility.FindBody (system.rootBody, "Dres");
			GameObject scaledVersion = (GameObject) UnityEngine.Object.Instantiate(Dres.scaledVersion);
			scaledVersion.name = "Kopernicus";
			body.scaledVersion = scaledVersion;

			// Adjust the scaled space fader to our new celestial body
			ScaledSpaceFader fader = scaledVersion.GetComponent<ScaledSpaceFader> ();
			fader.celestialBody = body.celestialBody;


			return body;

		}


	}

	// Add a clone of a stock planet (in a different orbit)
	// This is a kind of KopernicusPlanetSource
	public class StockPlanetSource : KopernicusPlanetSource {
		public static PSystemBody GeneratePlanet (PSystem system, string stockPlanetName, string newName, Orbit orbit = null) {
			return GenerateSystemBody (system, system.rootBody, stockPlanetName, newName, orbit);
		}

		public static PSystemBody GenerateSystemBody(PSystem system, PSystemBody parent, string stockPlanetName, string newName, Orbit orbit = null) {
			// AddBody makes the GameObject and stuff. It also attaches it to the system and
			// parent.
			PSystemBody body = system.AddBody (parent);
			PSystemBody prototype = KopernicusUtility.FindBody (system.rootBody, stockPlanetName);

			if (prototype == null) {
				Debug.Log ("Kopernicus:StockPlanetSource can't find a stock planet named " + stockPlanetName);
				return null;
			}

			// set up the various parameters
			body.name = newName;
			body.orbitRenderer.orbitColor = prototype.orbitRenderer.orbitColor;
			body.flightGlobalsIndex = prototype.flightGlobalsIndex;

			// Some parameters of the celestialBody, which represents the actual planet...
			// PSystemBody is more of a container that associates the planet with its orbit 
			// and position in the planetary system, etc.
			body.celestialBody.bodyName = newName;
			body.celestialBody.Radius = prototype.celestialBody.Radius;

			// This is g, not acceleration due to g, it turns out.
			body.celestialBody.GeeASL = prototype.celestialBody.GeeASL; 
			// This is the Standard gravitational parameter, i.e. mu
			body.celestialBody.gravParameter = prototype.celestialBody.gravParameter; 
			// It appears that it calculates SOI for you if you give it this stuff.
			body.celestialBody.bodyDescription = prototype.celestialBody.bodyDescription;
			// at the moment, this value is always "Generic" but I guess that might change.
			body.celestialBody.bodyType = prototype.celestialBody.bodyType;

			// Setup the orbit of "Kopernicus."  The "Orbit" class actually is built to support serialization straight
			// from Squad, so storing these to files (and loading them) will be pretty easy.

			//Debug.Log ("..About to assign orbit.");

			// Note that we may have to adjust the celestialBody target here, because odds are
			// we're putting a planet found in the systemPrefab into a system that is not the
			// systemPrefab and has different celestialbodies. FIXME by having it look up the 
			// body by name perhaps? That will break of course if you have, say, two Jools.

			if (orbit == null)
				body.orbitDriver.orbit = prototype.orbitDriver.orbit; // probably won't work if not going into a cloned systemprefab Sun
			else
				body.orbitDriver.orbit = orbit;

 

			body.orbitDriver.celestialBody = body.celestialBody;
			body.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;
			body.orbitDriver.UpdateOrbit ();

			//Debug.Log ("..About to clone the scaledversion.");
			// Temporarily clone the Dres scaled version for the structure
			// Find the dres prefab
			GameObject scaledVersion = (GameObject)UnityEngine.Object.Instantiate(prototype.scaledVersion);
			/*if (scaledVersion == null)
				Debug.Log ("ScaledVersion is null");
			else
				Debug.Log ("ScaledVersion is not null.");*/
			body.scaledVersion = scaledVersion;

			//Debug.Log ("..About to assign fader.");
			// Adjust the scaled space fader to our new celestial body
			ScaledSpaceFader fader = scaledVersion.GetComponent<ScaledSpaceFader> ();
			//Debug.Log ("fader: " + fader + " sv:", scaledVersion);
			fader.celestialBody = body.celestialBody;

			//Debug.Log ("..done.");
			return body;

		}
	}
}

