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

using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Kopernicus.Components
{
    /// <summary>
    /// Class to visualize a sphere around a specific transform
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Wiresphere : MonoBehaviour
    {
        // Variables
        public CelestialBody Body { get; set; }

        /// <summary>
        /// Gets the Celestial Body we're attached to and reads the values
        /// </summary>
        private void Start()
        {
            Body = GetComponent<CelestialBody>();
        }

        /// <summary>
        /// Updates the wiresphere
        /// </summary>
        private void OnGUI()
        {
            if (!MapView.MapIsEnabled)
            {
                return;
            }

            // Draw
            DrawTools.DrawOrbit(0, 0, Body.sphereOfInfluence, 0, 0, 0, 0, Body, Body.orbitDriver.Renderer.orbitColor,
                DrawTools.Style.Dashed);
            DrawTools.DrawOrbit(90, 0, Body.sphereOfInfluence, 0, 0, 0, 0, Body, Body.orbitDriver.Renderer.orbitColor,
                DrawTools.Style.Dashed);
            DrawTools.DrawOrbit(90, 0, Body.sphereOfInfluence, 90, 0, 0, 0, Body, Body.orbitDriver.Renderer.orbitColor,
                DrawTools.Style.Dashed);
        }
    }
}
