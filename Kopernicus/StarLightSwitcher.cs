/**
 * Kopernicus Planetary System Modifier
 * 
 * Copyright (C) 2015 Bryce C Schroeder (bryce.schroeder@gmail.com)
 *                    Nathaniel R. Lewis (linux.robotdude@gmail.com)
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
 * 
 * Original code from Star Systems by OvenProofMars, Fastwings, medsouz
 * Modified by Thomas P., Nathaniel R. Lewis (Teknoman117) for Kopernicus
 * 
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kopernicus
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class StarLightSwitcher : MonoBehaviour
    {
		// List of celestial bodies that are stars
		private List<CelestialBody> stars;

        // Custom powerCurves
        private Dictionary<string, ConfigNode> powerNodes = new Dictionary<string,ConfigNode>();

        // StarTransforms
        private Dictionary<string, Transform> starTransform = new Dictionary<string,Transform>();

        // Store StarColors 
        private Dictionary<string, Dictionary<string, Vector4>> starColor = new Dictionary<string, Dictionary<string, Vector4>>();

        // StarTexture
        private Dictionary<string, Texture2D> mainTex = new Dictionary<string,Texture2D>();

		// MonoBehavior.Awake()
		void Awake()
		{
			// Don't kill us
			Debug.Log ("[Kopernicus]: StarLightSwitcher Started");
			DontDestroyOnLoad (this);
			stars = PSystemManager.Instance.localBodies.Where (body => body.scaledBody.GetComponentsInChildren<SunShaderController> (true).Length > 0).ToList ();

            // Load custom powerCurves
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("Kopernicus")[0].GetNodes("Body"))
            {                                                                               
                if (node.HasNode("powerCurve"))
                    powerNodes.Add(node.GetValue("name"), node.GetNode("powerCurve")); // Add custom powerCurve to our Dictionary
                else
                    powerNodes.Add(node.GetValue("name"), powerNodes["Sun"]);
            }

            // Get StarTransforms
            foreach (Transform TransformStar in ScaledSpace.Instance.scaledSpaceTransforms)
            {
                starTransform[TransformStar.name] = TransformStar;
            }

            // Load StarColors
            ConfigNode root = GameDatabase.Instance.GetConfigs("Kopernicus")[0].config;
            foreach (ConfigNode body in root.GetNodes("Body"))
            {
                if (body.HasNode("StarColor"))
                {
                    ConfigNode color = body.GetNode("StarColor");
                    Dictionary<string, Vector4> colorValues = new Dictionary<string, Vector4>();
                    string bodyName = body.GetValue("name");
                    colorValues.Add("lightColor", ConfigNode.ParseVector4(color.GetValue("lightColor") ?? "0,0,0,0"));
                    colorValues.Add("emitColor0", ConfigNode.ParseVector4(color.GetValue("emitColor0") ?? "0,0,0,0"));
                    colorValues.Add("emitColor1", ConfigNode.ParseVector4(color.GetValue("emitColor1") ?? "0,0,0,0"));
                    colorValues.Add("sunspotColor", ConfigNode.ParseVector4(color.GetValue("sunspotColor") ?? "0,0,0,0"));
                    colorValues.Add("rimColor", ConfigNode.ParseVector4(color.GetValue("rimColor") ?? "0,0,0,0"));

                    mainTex.Add(bodyName, GameDatabase.Instance.GetTexture(color.GetValue("coronaTexture") ?? "", false));
                    starColor.Add(bodyName, colorValues);
                }
            }

            // Apply StarColor to Star
            foreach (CelestialBody star in stars)
            {
                if (starColor.Keys.Contains(star.bodyName))
                {
                    starTransform[star.bodyName].renderer.material.SetColor("_EmitColor0", ColorConvert(starColor[star.bodyName]["emitColor0"]));
                    starTransform[star.bodyName].renderer.material.SetColor("_EmitColor1", ColorConvert(starColor[star.bodyName]["emitColor1"]));
                    starTransform[star.bodyName].renderer.material.SetColor("_SunspotColor", ColorConvert(starColor[star.bodyName]["sunspotColor"]));
                    starTransform[star.bodyName].renderer.material.SetColor("_RimColor", ColorConvert(starColor[star.bodyName]["rimColor"]));
                    
                }
            }
		}

        void Update()
        {
            // Get the current position of the active vessel
			Vector3 position = Vector3.zero;
			if (PlanetariumCamera.fetch.enabled == true) 
			{
				position = ScaledSpace.ScaledToLocalSpace (PlanetariumCamera.fetch.GetCameraTransform ().position);
			} 
			else if (FlightGlobals.ActiveVessel != null) 
			{
				position = FlightGlobals.ActiveVessel.GetTransform ().position;
			}

			// Get the closest star
			CelestialBody closestStar = stars.OrderBy (star => FlightGlobals.getAltitudeAtPos (position, star)).First ();
			if (closestStar != Sun.Instance.sun) 
			{
				SetSun (closestStar);
			}
        }

		// Set the active sun object
        public void SetSun(CelestialBody CB)
        {
            //Set star as active star
            Sun.Instance.sun = CB;
            Planetarium.fetch.Sun = CB;
            Debug.Log("Active sun set to: " + CB.name);

            //Set sunflare color
            Sun.Instance.sunFlare.color = (starColor.Keys.Contains(CB.bodyName)) ? ColorConvert(starColor[CB.bodyName]["lightColor"]) : new Color(1.0f, 1.0f, 1.0f, 1.0f);

            //Reset solar panels (Credit to Kcreator)
            foreach (ModuleDeployableSolarPanel panel in FindObjectsOfType(typeof(ModuleDeployableSolarPanel)))
            {
                panel.OnStart(PartModule.StartState.Orbital);
            }

            // Set custom powerCurve for solar panels
            if (FlightGlobals.ActiveVessel != null)
            {
                foreach (ModuleDeployableSolarPanel sp in FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleDeployableSolarPanel>())
                {
                    if (powerNodes[CB.bodyName].GetType() == typeof(ConfigNode)) // Sure is sure
                    {
                        sp.powerCurve = new FloatCurve();
                        sp.powerCurve.Load(powerNodes[CB.bodyName]); // Apply our custom powerCurve
                    }
                }
            }

            // Update Corona texture
            if (starTransform.Keys.Contains(CB.bodyName))
            {
                foreach (SunCoronas Corona in starTransform[CB.bodyName].GetComponentsInChildren<SunCoronas>())
                {
                    if (mainTex.Keys.Contains(CB.bodyName))
                        Corona.renderer.material.mainTexture = mainTex[CB.bodyName];
                }
            }
        }

        public Color ColorConvert(Vector4 c) {
            return new Color(c.x, c.y, c.z, c.w);
        }

    }
}
