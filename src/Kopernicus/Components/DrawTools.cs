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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Kopernicus.Components
{
    /// <summary>
    /// Utility for drawing GL lines
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class DrawTools
    {
        /// <summary>
        /// Possibilities for styling a GL line
        /// </summary>
        public enum Style
        {
            Solid = 1,
            Dashed = 2
        }

        /// <summary>
        /// The resolution of the gl line
        /// </summary>
        private const Double RESOLUTION = 1d;

        /// <summary>
        /// The internal representation of the GL material
        /// </summary>
        private static Material _material;

        /// <summary>
        /// The screen points
        /// </summary>
        private static Vector3 _screenPoint1, _screenPoint2;

        /// <summary>
        /// A list with vertices for the GL Line
        /// </summary>
        private static readonly List<Vector3d> Points = new List<Vector3d>();

        /// <summary>
        /// Gets or sets the used GL material.
        /// </summary>
        /// <value>
        /// The material.
        /// </value>
        public static Material Material
        {
            get
            {
                if (_material == null)
                {
                    _material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
                }

                return _material;
            }
            set { _material = value; }
        }

        /// <summary>
        /// Draws the orbit with the given arguments.
        /// </summary>
        public static void DrawOrbit(Double inc, Double e, Double sma, Double lan, Double w, Double mEp, Double t,
            CelestialBody body)
        {
            DrawOrbit(new Orbit(inc, e, sma, lan, w, mEp, t, body));
        }

        /// <summary>
        /// Draws the orbit with the given arguments.
        /// </summary>
        public static void DrawOrbit(Double inc, Double e, Double sma, Double lan, Double w, Double mEp, Double t,
            CelestialBody body, Color color)
        {
            DrawOrbit(new Orbit(inc, e, sma, lan, w, mEp, t, body), color);
        }

        /// <summary>
        /// Draws the orbit with the given arguments.
        /// </summary>
        public static void DrawOrbit(Double inc, Double e, Double sma, Double lan, Double w, Double mEp, Double t,
            CelestialBody body, Color color, Style style)
        {
            DrawOrbit(new Orbit(inc, e, sma, lan, w, mEp, t, body), color, style);
        }

        /// <summary>
        /// Draws the orbit.
        /// </summary>
        /// <param name="orbit">The orbit.</param>
        public static void DrawOrbit(Orbit orbit)
        {
            DrawOrbit(orbit, Color.white);
        }

        /// <summary>
        /// Draws the orbit.
        /// </summary>
        /// <param name="orbit">The orbit.</param>
        /// <param name="color">The color of the created line.</param>
        /// <param name="style">The style of the created line.</param>
        public static void DrawOrbit(Orbit orbit, Color color, Style style = Style.Solid)
        {
            // Only render visible stuff
            if (!orbit.referenceBody.scaledBody.GetComponent<Renderer>().isVisible)
            {
                return;
            }

            // Clear points
            Points.Clear();

            // Calculations for elliptical orbits
            if (orbit.eccentricity < 1)
            {
                for (Int32 i = 0; i < Math.Floor(360.0 / RESOLUTION); i++)
                {
                    Points.Add(ScaledSpace.LocalToScaledSpace(
                        orbit.getPositionFromEccAnomaly(i * RESOLUTION * Math.PI / 180)));
                }

                Points.Add(Points[0]); // close the loop
            }
            // Calculations for hyperbolic orbits
            else
            {
                for (Int32 i = -1000; i <= 1000; i += 5)
                {
                    Points.Add(ScaledSpace.LocalToScaledSpace(
                        orbit.getPositionFromEccAnomaly(i * RESOLUTION * Math.PI / 180)));
                }
            }

            // Draw the path
            DrawPath(Points, color, style);
        }

        /// <summary>
        /// Draws a path defined by multiple vertices in points.
        /// </summary>
        public static void DrawPath(List<Vector3d> points, Color color, Style style)
        {
            // Start the GL drawing
            GL.PushMatrix();
            Material.SetPass(0);
            GL.LoadPixelMatrix();
            GL.Begin(GL.LINES);
            GL.Color(color);

            // Evaluate the needed amount of steps
            Int32 step = (Int32) style;

            // Draw every point
            for (Int32 i = 0; i < points.Count - 1; i += step)
            {
                // Occlusion check
                Vector3 cameraPos = PlanetariumCamera.Camera.transform.position;
                if (Physics.Raycast(cameraPos, (points[i] - cameraPos).normalized) ||
                    Physics.Raycast(cameraPos, (points[i + 1] - cameraPos).normalized))
                {
                    continue;
                }

                // Map world coordinates to screen coordinates
                _screenPoint1 = PlanetariumCamera.Camera.WorldToScreenPoint(points[i]);
                _screenPoint2 = PlanetariumCamera.Camera.WorldToScreenPoint(points[i + 1]);

                // Draw the GL vertices
                if (!(_screenPoint1.z > 0) || !(_screenPoint2.z > 0))
                {
                    continue;
                }

                GL.Vertex3(_screenPoint1.x, _screenPoint1.y, 0);
                GL.Vertex3(_screenPoint2.x, _screenPoint2.y, 0);
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
        public static Boolean IsOccluded(Vector3d worldPosition, CelestialBody byBody)
        {
            Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);
            Vector3d vc = (byBody.position - camPos) / (byBody.Radius - 100);
            Vector3d vt = (worldPosition - camPos) / (byBody.Radius - 100);

            Double vtVc = Vector3d.Dot(vt, vc);

            // In front of the horizon plane
            if (vtVc < vc.sqrMagnitude - 1)
            {
                return false;
            }

            return vtVc * vtVc / vt.sqrMagnitude > vc.sqrMagnitude - 1;
        }

    }
}
