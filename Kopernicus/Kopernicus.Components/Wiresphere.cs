/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// Class to visualize a sphere around a specific transform
        /// </summary>
        public class Wiresphere : MonoBehaviour
        {
            /// Variables
            public CelestialBody body { get; set; }

            /// <summary>
            /// Gets the Celestial Body we're attached to and reads the values
            /// </summary>
            void Start()
            {
                body = GetComponent<CelestialBody>();
            }

            /// <summary>
            /// Updates the wiresphere
            /// </summary>
            void OnGUI()
            {
                if (!MapView.MapIsEnabled)
                    return;

                // Draw
                GLTools.DrawOrbit(0, 0, body.sphereOfInfluence, 0, 0, 0, 0, body, body.orbitDriver.Renderer.orbitColor, GLTools.Style.DASHED);
                GLTools.DrawOrbit(90, 0, body.sphereOfInfluence, 0, 0, 0, 0, body, body.orbitDriver.Renderer.orbitColor, GLTools.Style.DASHED);
                GLTools.DrawOrbit(90, 0, body.sphereOfInfluence, 90, 0, 0, 0, body, body.orbitDriver.Renderer.orbitColor, GLTools.Style.DASHED);
            }
        }
    }
}