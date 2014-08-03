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
	// Logging functions for certain shaders
	namespace MaterialWrapper
	{
		// Wrapper for the "Terrain/PQS/Ocean Surface Quad" shader and material
		public class PQSOceanSurfaceQuad : Material
		{
			// String constants for shader properties
			private const string shaderName         = "Terrain/PQS/Ocean Surface Quad";
			private const string colorFromSpaceKey  = "_ColorFromSpace"; 
			private const string specularColorKey   = "_SpecColor";
			private const string shininessKey       = "_Shininess";
			private const string glossKey           = "_Gloss";
			private const string tilingKey          = "_tiling";
			private const string normalTilingKey    = "_bTiling";
			private const string normalMapKey       = "_BumpMap";
			private const string waterTexture0Key   = "_WaterTex";
			private const string waterTexture1Key   = "_WaterTex1";
			private const string mixKey             = "_Mix";
			private const string falloffPowerKey    = "_falloffPower";
			private const string falloffExponentKey = "_falloffExp";
			private const string fogColorKey  	    = "_fogColor";
			private const string heightFalloffKey   = "_heightFallOff";
			private const string globalDensityKey   = "_globalDensity";
			private const string atmosphereDepthKey = "_atmosphereDepth";
			private const string fogColorRampKey    = "_fogColorRamp";
			private const string fadeStartKey       = "_fadeStart";
			private const string fadeEndKey         = "_fadeEnd";
			private const string oceanOpacityKey    = "_oceanOpacity";
			private const string planetOpacityKey   = "_PlanetOpacity";
			private const string displacementKey    = "_displacement";
			private const string displacementFrequencyKey = "_dispFreq";

			// Private Propertoes
			private static Shader shaderForMaterial 
			{
				get { return Shader.Find (shaderName); }
			}

			// Property wrappers
			public Color colorFromSpace 
			{
				get { return GetColor(colorFromSpaceKey); }
				set { SetColor(colorFromSpaceKey, value); }
			}
			public Color specularColor 
			{
				get { return GetColor (specularColorKey); }
				set { SetColor (specularColorKey, value); }
			}
			public float shininess
			{
				get { return GetFloat (shininessKey); }
				set { SetFloat (shininessKey, Mathf.Clamp (value, 0.01f, 1.0f)); }
			}
			public float gloss
			{
				get { return GetFloat (glossKey); }
				set { SetFloat (glossKey, Mathf.Clamp (value, 0.01f, 1.0f)); }
			}
			public float tiling 
			{
				get { return GetFloat (tilingKey); }
				set { SetFloat (tilingKey, value); }
			}
			public float normalTiling 
			{
				get { return GetFloat (normalTilingKey); }
				set { SetFloat (normalTilingKey, value); }
			}
			public Texture2D normalMap
			{
				get { return GetTexture (normalMapKey) as Texture2D; }
				set { SetTexture (normalMapKey, value); }
			}
			public Texture2D waterTexture0 
			{
				get { return GetTexture (waterTexture0Key) as Texture2D; }
				set { SetTexture (waterTexture0Key, value); }
			}
			public Texture2D waterTexture1 
			{
				get { return GetTexture (waterTexture1Key) as Texture2D; }
				set { SetTexture (waterTexture1Key, value); }
			}
			public Color fogColor 
			{
				get { return GetColor (fogColorKey); }
				set { SetColor (fogColorKey, value); }
			}
			public Texture2D fogColorRamp 
			{
				get { return GetTexture (fogColorRampKey) as Texture2D; }
				set { SetTexture (fogColorRampKey, value); }
			}
			public float mix 
			{
				get { return GetFloat (mixKey); }
				set { SetFloat (mixKey, value); }
			}
			public float falloffPower 
			{
				get { return GetFloat (falloffPowerKey); }
				set { SetFloat (falloffPowerKey, value); }
			}
			public float falloffExponent 
			{
				get { return GetFloat (falloffExponentKey); }
				set { SetFloat (falloffExponentKey, value); }
			}
			public float heightFalloff 
			{
				get { return GetFloat (heightFalloffKey); }
				set { SetFloat (heightFalloffKey, value); }
			}
			public float globalDensity
			{
				get { return GetFloat (globalDensityKey); }
				set { SetFloat (globalDensityKey, value); }
			}
			public float atmosphereDepth
			{
				get { return GetFloat (atmosphereDepthKey); }
				set { SetFloat (atmosphereDepthKey, value); }
			}
			public float fadeStart
			{
				get { return GetFloat (fadeStartKey); }
				set { SetFloat (fadeStartKey, value); }
			}
			public float fadeEnd
			{
				get { return GetFloat (fadeEndKey); }
				set { SetFloat (fadeEndKey, value); }
			}
			public float oceanOpacity
			{
				get { return GetFloat (oceanOpacityKey); }
				set { SetFloat (oceanOpacityKey, value); }
			}
			public float planetOpacity
			{
				get { return GetFloat (planetOpacityKey); }
				set { SetFloat (planetOpacityKey, value); }
			}
			public float displacement 
			{
				get { return GetFloat (displacementKey); }
				set { SetFloat (displacementKey, value); }
			}
			public float displacementFrequency 
			{
				get { return GetFloat (displacementFrequencyKey); }
				set { SetFloat (displacementFrequencyKey, value); }
			}

			#region Debug Utilities

			public void Log()
			{
				// Iterate through all of the properties
				Debug.Log("--------- " + ToString() + " ------------");
				foreach (PropertyInfo property in GetType().GetProperties()) 
				{
					Debug.Log(property.Name + " = " + property.GetValue(this, null));
				}
				Debug.Log("-----------------------------------------");
			}

			#endregion

			#region Constructors
			public PQSOceanSurfaceQuad() : base(shaderForMaterial)
			{

			}
			
			public PQSOceanSurfaceQuad(string contents) : base(contents)
			{
				// Throw exception if this material was not the proper material
				base.shader = shaderForMaterial;
			}

			public PQSOceanSurfaceQuad(Material material) : base(material)
			{
				// Throw exception if this material was not the proper material
				if (material.shader.name != shaderName)
					throw new InvalidOperationException("PQSOceanSurfaceQuad material requires the \"" + shaderName + "\" shader");
			}
			#endregion
		}
	}
}

