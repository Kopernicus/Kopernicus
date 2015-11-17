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
            public float radius { get; set; }
            public Color color { get; set; }
            public CelestialBody body { get; set; }

            /// Vectrosity
            private VectorLine[] lines { get; set; }

            /// <summary>
            /// Gets the Celestial Body we're attached to and reads the values
            /// </summary>
            void Start()
            {
                body = GetComponent<CelestialBody>();
                color = body.orbitDriver != null ? body.orbitDriver.orbitColor : Color.white;
                radius = (float)body.sphereOfInfluence;
                lines = new VectorLine[] { CreateLine(0, 0), CreateLine(90, 0), CreateLine(90, 90) };
            }

            /// <summary>
            /// Updates the wiresphere
            /// </summary>
            void LateUpdate()
            {
                if (!MapView.MapIsEnabled)
                    return;

                // Draw
                Vector.DrawLine(CreateLine(0, 0, lines[0]));
                Vector.DrawLine(CreateLine(90, 0, lines[1]));
                Vector.DrawLine(CreateLine(90, 90, lines[2]));
            }

            /// <summary>
            /// Destroy the Lines
            /// </summary>
            void OnDestroy()
            {
                for (int i = 0; i < 3; i++)
                    Vector.DestroyLine(ref lines[i]);
            }

            /// <summary>
            /// Creates a Vectrosity Line using an orbit
            /// </summary>
            public VectorLine CreateLine(double inclination, double lan, VectorLine line = null)
            {
                if (line == null)
                {
                    line = new VectorLine(body.name + " SOI", new Vector3[360 / 15 * 8 * 2], color, MapView.OrbitLinesMaterial, 5f, LineType.Discrete);
                    line.vectorObject.transform.parent = ScaledSpace.Instance.transform;
                    line.vectorObject.renderer.castShadows = false;
                    line.vectorObject.renderer.receiveShadows = false;
                    line.layer = 31;
                }
                Vector3[] points = new Vector3[(int)Math.Floor(360 / 15d)];
                Orbit orbit = new Orbit(inclination, 0, radius, lan, 0, 0, 0, body);
                for (int i = 0; i < Math.Floor(360 / 15d); i++)
                {
                    points[i] = ScaledSpace.LocalToScaledSpace(orbit.getPositionFromEccAnomaly(i * 15 * Math.PI / 180));
                }
                Vector.MakeSplineInLine(line, points, true);
                Vector.SetColor(line, new Color(color.r, color.g, color.b, 0.1f));
                return line;
            }
        }
    }
}