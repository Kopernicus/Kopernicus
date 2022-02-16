/**
 * Kopernicus Planetary System Modifier
 * ------------------------------------------------------------- 
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
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using UnityEngine;

namespace Kopernicus.Components
{
    /// <summary>
    /// A scaled space fader extension that uses the sharedMaterial
    /// </summary>
    public class SharedScaledSpaceFader : ScaledSpaceFader
    {
        private Texture2D _resourceMap;
        private static readonly Int32 ResourceMap = Shader.PropertyToID("_ResourceMap");

        private void Start()
        {
            celestialBody = PSystemManager.Instance.localBodies.Find(b => b.scaledBody == gameObject);
            r = GetComponent<Renderer>();
        }

        private void Update()
        {
            if (FlightGlobals.currentMainBody == celestialBody && !MapView.MapIsEnabled &&
                FlightGlobals.currentMainBody.pqsController != null)
            {
                Double alt = FlightGlobals.currentMainBody.pqsController.visibleAltitude;
                if (alt <= fadeStart)
                {
                    r.enabled = false;
                }
                else
                {
                    Single t;
                    if (alt >= fadeEnd)
                    {
                        t = 1f;
                    }
                    else
                    {
                        t = (Single)((alt - fadeStart) / (fadeEnd - fadeStart));
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

            if (celestialBody.ResourceMap == _resourceMap)
            {
                return;
            }

            _resourceMap = celestialBody.ResourceMap;
            r.sharedMaterial.SetTexture(ResourceMap, _resourceMap);
        }

        private void OnDestroy()
        {
            if (r != null && r.sharedMaterial != null)
            {
                r.sharedMaterial.SetFloat(floatName, 1f);
            }
        }
    }
}
