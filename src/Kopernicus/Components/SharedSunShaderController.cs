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
    /// A sun shader controller extension that uses the sharedMaterial
    /// </summary>
    public class SharedSunShaderController : SunShaderController
    {
        private Renderer _renderer;

        private Texture2D _rampMap;
        private static readonly Int32 Offset3 = Shader.PropertyToID("_Offset3");
        private static readonly Int32 Offset2 = Shader.PropertyToID("_Offset2");
        private static readonly Int32 Offset1 = Shader.PropertyToID("_Offset1");
        private static readonly Int32 Offset0 = Shader.PropertyToID("_Offset0");
        private static readonly Int32 RampMap = Shader.PropertyToID("_RampMap");

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            UpdateRampMap();
        }

        private void OnDestroy()
        {
            if (_rampMap != null)
            {
                Destroy(_rampMap);
            }
        }

        private void UpdateRampMap()
        {
            if (_rampMap != null)
            {
                Destroy(_rampMap);
            }

            _rampMap = new Texture2D(256, 3, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            for (Int32 i = 0; i < 256; i++)
            {
                Single r = curve0.Evaluate(i * 0.00390625f);
                Single g = curve1.Evaluate(i * 0.00390625f);
                Single b = curve2.Evaluate(i * 0.00390625f);
                Single a = curve3.Evaluate(i * 0.00390625f);
                for (Int32 j = 0; j < 5; j++)
                {
                    _rampMap.SetPixel(i, j, new Color(r, g, b, a));
                }
            }

            _rampMap.Apply(false, true);
            _renderer.sharedMaterial.SetTexture(RampMap, _rampMap);
        }

        private void Update()
        {
            Single time;
            if (usePlanetariumTime && Planetarium.fetch != null)
            {
                time = (Single)Planetarium.GetUniversalTime() * speedFactor;
            }
            else
            {
                time = Time.realtimeSinceStartup * speedFactor;
            }

            _renderer.sharedMaterial.SetFloat(Offset0, 1f / frequency0 * time);
            _renderer.sharedMaterial.SetFloat(Offset1, 1f / frequency1 * time);
            _renderer.sharedMaterial.SetFloat(Offset2, 1f / frequency2 * time);
            _renderer.sharedMaterial.SetFloat(Offset3, 1f / frequency3 * time);
        }
    }
}