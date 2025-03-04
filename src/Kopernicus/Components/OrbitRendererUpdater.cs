﻿/**
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

using UnityEngine;

namespace Kopernicus.Components
{
    [RequireComponent(typeof(OrbitDriver))]
    public class OrbitRendererUpdater : MonoBehaviour
    {
        private OrbitDriver _driver;

        private void Start()
        {
            _driver = GetComponent<OrbitDriver>();
        }

        // Updates the parameters of OrbitRenderer every frame
        private void Update()
        {
            if (!_driver.celestialBody)
            {
                return;
            }

            if (!PSystemManager.OrbitRendererDataCache.ContainsKey(_driver.celestialBody))
            {
                return;
            }

            if (!_driver.Renderer)
            {
                return;
            }

            OrbitRendererData originalData = PSystemManager.OrbitRendererDataCache[_driver.celestialBody];
            if (!(originalData is KopernicusOrbitRendererData))
            {
                PSystemManager.OrbitRendererDataCache[_driver.celestialBody] =
                    new KopernicusOrbitRendererData(_driver.celestialBody, originalData);
            }

            KopernicusOrbitRendererData data =
                (KopernicusOrbitRendererData)PSystemManager.OrbitRendererDataCache[_driver.celestialBody];
            _driver.Renderer.orbitColor = data.orbitColor;
            _driver.Renderer.nodeColor = data.nodeColor;
            _driver.Renderer.upperCamVsSmaRatio = data.upperCamVsSmaRatio;
            _driver.Renderer.lowerCamVsSmaRatio = data.lowerCamVsSmaRatio;
        }
    }
}
