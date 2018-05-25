using System;
using UnityEngine;

namespace Kopernicus
{
	namespace Components
	{
		/// <summary>
		/// A scaled space fader extension that uses the sharedMaterial
		/// </summary>
		public class SharedScaledSpaceFader : ScaledSpaceFader
		{
			void Start()
			{
				celestialBody = PSystemManager.Instance.localBodies.Find(b => b.scaledBody == gameObject);
				r = GetComponent<Renderer>();
			}
			
			void Update()
			{
				if (FlightGlobals.currentMainBody == celestialBody && !MapView.MapIsEnabled &&
				    FlightGlobals.currentMainBody.pqsController != null)
				{
					Double alt = FlightGlobals.currentMainBody.pqsController.visibleAltitude;
					Single t;
					if (alt <= fadeStart)
					{
						r.enabled = false;
					}
					else
					{
						if (alt >= fadeEnd)
						{
							t = 1f;
						}
						else
						{
							t = (Single) ((alt - fadeStart) / (fadeEnd - fadeStart));
						}

						r.enabled = true;
						r.sharedMaterial.SetFloat(floatName, t);
					}
				}
				else if (GetAngularSize(transform, r, ScaledCamera.Instance.cam) > 1f)
				{
					r.sharedMaterial.SetFloat(floatName, 1f);
					r.enabled = true;
				}
				else
				{
					r.enabled = false;
				}

				if (celestialBody.ResourceMap != _resourceMap)
				{
					_resourceMap = celestialBody.ResourceMap;
					r.sharedMaterial.SetTexture("_ResourceMap", _resourceMap);
				}
			}

			void OnDestroy()
			{
				if (r != null && r.sharedMaterial != null)
				{
					r.sharedMaterial.SetFloat(floatName, 1f);
				}
			}
			
			private Texture2D _resourceMap;
		}
	}
}