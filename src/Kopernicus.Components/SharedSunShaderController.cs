using System;
using UnityEngine;

namespace Kopernicus
{
	namespace Components
	{
		/// <summary>
		/// A sun shader controller extension that uses the sharedMaterial
		/// </summary>
		public class SharedSunShaderController : SunShaderController
		{
			void Start()
			{
				_renderer = GetComponent<Renderer>();
				UpdateRampMap();
			}
			
			void OnDestroy()
			{
				if (rampMap != null)
				{
					Destroy(rampMap);
				}
			}
			
			private void UpdateRampMap()
			{
				if (rampMap != null)
				{
					Destroy(rampMap);
				}

				rampMap = new Texture2D(256, 3, TextureFormat.RGBA32, false) {filterMode = FilterMode.Bilinear};
				for (Int32 i = 0; i < 256; i++)
				{
					Single r = curve0.Evaluate(i * 0.00390625f);
					Single g = curve1.Evaluate(i * 0.00390625f);
					Single b = curve2.Evaluate(i * 0.00390625f);
					Single a = curve3.Evaluate(i * 0.00390625f);
					for (Int32 j = 0; j < 5; j++)
					{
						rampMap.SetPixel(i, j, new Color(r,g,b,a));
					}
				}

				rampMap.Apply(false, true);
				_renderer.sharedMaterial.SetTexture("_RampMap", rampMap);
			}
			
			private void Update()
			{
				Single time;
				if (usePlanetariumTime && Planetarium.fetch != null)
				{
					time = (Single) Planetarium.GetUniversalTime() * speedFactor;
				}
				else
				{
					time = Time.realtimeSinceStartup * speedFactor;
				}

				_renderer.sharedMaterial.SetFloat("_Offset0", 1f / frequency0 * time);
				_renderer.sharedMaterial.SetFloat("_Offset1", 1f / frequency1 * time);
				_renderer.sharedMaterial.SetFloat("_Offset2", 1f / frequency2 * time);
				_renderer.sharedMaterial.SetFloat("_Offset3", 1f / frequency3 * time);
			}
			
			private Renderer _renderer;
			
			private Texture2D rampMap;
		}
	}
}