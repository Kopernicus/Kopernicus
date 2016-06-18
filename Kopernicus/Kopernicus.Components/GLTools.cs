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
        /// Utility for drawing GL lines
        /// </summary>
        public static class GLTools
        {
            /// <summary>
            /// Possibilities for styling a GL line
            /// </summary>
            public enum Style
            {
                SOLID = 1,
                DASHED = 2
            }

            /// <summary>
            /// The resolution of the gl line
            /// </summary>
            public const double resolution = 1d;

            /// <summary>
            /// The internal representation of the GL material
            /// </summary>
            private static Material _material;

            /// <summary>
            /// The screen points
            /// </summary>
            private static Vector3 screenPoint1, screenPoint2;

            /// <summary>
            /// A list with vertices for the GL Line
            /// </summary>
            private static List<Vector3d> points = new List<Vector3d>();

            /// <summary>
            /// Gets or sets the used GL material.
            /// </summary>
            /// <value>
            /// The material.
            /// </value>
            public static Material material
            {
                get
                {
                    if (_material == null)
                        _material = new Material(Shader.Find("Particles/Additive"));
                    return _material;
                }
                set { _material = material; }
            }

            /// <summary>
            /// Draws the orbit with the given arguments.
            /// </summary>
            public static void DrawOrbit(double inc, double e, double sma, double lan, double w, double mEp, double t, CelestialBody body)
            {
                DrawOrbit(new Orbit(inc, e, sma, lan, w, mEp, t, body));
            }

            /// <summary>
            /// Draws the orbit with the given arguments.
            /// </summary>
            public static void DrawOrbit(double inc, double e, double sma, double lan, double w, double mEp, double t, CelestialBody body, Color color)
            {
                DrawOrbit(new Orbit(inc, e, sma, lan, w, mEp, t, body), color);
            }

            /// <summary>
            /// Draws the orbit with the given arguments.
            /// </summary>
            public static void DrawOrbit(double inc, double e, double sma, double lan, double w, double mEp, double t, CelestialBody body, Color color, Style style)
            {
                DrawOrbit(new Orbit(inc, e, sma, lan, w, mEp, t, body), color, style);
            }

            /// <summary>
            /// Draws the orbit.
            /// </summary>
            /// <param name="orbit">The orbit.</param>
            public static void DrawOrbit(Orbit orbit)
            {
                DrawOrbit(orbit, Color.white, Style.SOLID);
            }

            /// <summary>
            /// Draws the orbit.
            /// </summary>
            /// <param name="orbit">The orbit.</param>
            /// <param name="color">The color of the created line.</param>
            public static void DrawOrbit(Orbit orbit, Color color)
            {
                DrawOrbit(orbit, color, Style.SOLID); 
            }

            /// <summary>
            /// Draws the orbit.
            /// </summary>
            /// <param name="orbit">The orbit.</param>
            /// <param name="color">The color of the created line.</param>
            /// <param name="style">The style of the created line.</param>
            public static void DrawOrbit(Orbit orbit, Color color, Style style)
            {
                // Only render visible stuff
                if (!orbit.referenceBody.scaledBody.GetComponent<Renderer>().isVisible)
                    return;

                // Clear points
                points.Clear();

                // Calculations for elliptical orbits
                if (orbit.eccentricity < 1)
                {
                    for (int i = 0; i < Math.Floor(360.0 / resolution); i++)
                        points.Add(ScaledSpace.LocalToScaledSpace(orbit.getPositionFromEccAnomaly(i * resolution * Math.PI / 180)));
                    points.Add(points[0]); // close the loop
                }
                // Calculations for hyperbolic orbits
                else
                {
                    for (int i = -1000; i <= 1000; i += 5)
                        points.Add(ScaledSpace.LocalToScaledSpace(orbit.getPositionFromEccAnomaly(i * resolution * Math.PI / 180)));
                }

                // Draw the path
                DrawPath(orbit.referenceBody, points, color, style);
            }

            /// <summary>
            /// Draws a path defined by multiple vertices in points.
            /// </summary>
            /// <param name="points">The vertices of the path.</param>
            /// <param name="color">The color of the created path.</param>
            /// <param name="style">Whether the path is dashed or solid</param>
            public static void DrawPath(CelestialBody body, List<Vector3d> points, Color color, Style style)
            {
                // Start the GL drawing
                GL.PushMatrix();
                material.SetPass(0);
                GL.LoadPixelMatrix();
                GL.Begin(GL.LINES);
                GL.Color(color);

                // Evaluate the needed amount of steps
                int step = (int)style;

                // Draw every point
                for (int i = 0; i < points.Count - 1; i += step)
                {
                    // Occlusion check
                    Vector3 cameraPos = PlanetariumCamera.Camera.transform.position;
                    if (Physics.Raycast(cameraPos, (points[i] - cameraPos).normalized) || Physics.Raycast(cameraPos, (points[i + 1] - cameraPos).normalized))
                        continue;

                    // Map world coordinates to screen coordinates
                    screenPoint1 = PlanetariumCamera.Camera.WorldToScreenPoint(points[i]);
                    screenPoint2 = PlanetariumCamera.Camera.WorldToScreenPoint(points[i + 1]);

                    // Draw the GL vertices
                    if (screenPoint1.z > 0 && screenPoint2.z > 0)
                    {
                        GL.Vertex3(screenPoint1.x, screenPoint1.y, 0);
                        GL.Vertex3(screenPoint2.x, screenPoint2.y, 0);
                    }
                }
                GL.End();
                GL.PopMatrix();
            }

            /// <summary>
            ///  Determines whether the specified position is occluded by a celestial body.
            ///  Borrowed from Sarbian
            /// </summary>
            /// <param name="worldPosition">The world position.</param>
            /// <param name="byBody">The by body.</param>
            /// <returns></returns>
            public static bool IsOccluded(Vector3d worldPosition, CelestialBody byBody)
            {
                Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);
                Vector3d VC = (byBody.position - camPos) / (byBody.Radius - 100);
                Vector3d VT = (worldPosition - camPos) / (byBody.Radius - 100);

                double VT_VC = Vector3d.Dot(VT, VC);

                // In front of the horizon plane
                if (VT_VC < VC.sqrMagnitude - 1) return false;

                return VT_VC * VT_VC / VT.sqrMagnitude > VC.sqrMagnitude - 1;
            }

        }
    }
}
