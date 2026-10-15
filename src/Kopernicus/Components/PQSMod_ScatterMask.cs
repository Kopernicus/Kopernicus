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

using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.ModLoader;
using Kopernicus.Configuration.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kopernicus.Components;

public class PQSMod_ScatterMask : PQSMod
{
    #region Config
    // These define the center of scatter mask on the surface
    public double lat;
    public double lon;

    // The width, in meters, of one side of the mask.
    public double width;

    // Rotation of the texture, in degrees, about the mask center. At 0 the
    // texture's +V axis points north and its +U axis points east.
    public float angle;

    // Turn this on to show the color map for debugging reasons.
    public bool debugShowColorMap;

    // The mask texture. 
    public MapSO mask;
    #endregion

    #region Derived State
    // The position of the mask center on the sphere surface, in
    // planet-local coordinates.
    private Vector3d position;

    // Surface tangent directions, in planet-local coordinates, that the
    // texture's U and V axes are mapped along. These are unit vectors.
    private Vector3d uAxis;
    private Vector3d vAxis;
    #endregion

    public override void OnSetup()
    {
        if (debugShowColorMap)
            requirements |= PQS.ModiferRequirements.MeshColorChannel;

        var cb = sphere.GetCelestialBody();

        var normal = cb.GetRelSurfaceNVector(lat, lon);
        position = normal * sphere.radius;

        // Build an east/north tangent basis at the mask center. The planet's
        // local +Y axis is its north pole.
        var east = Vector3d.Cross(normal, Vector3d.up);
        if (east.sqrMagnitude < 1e-12)
            east = Vector3d.right; // At a pole every direction is south; pick one.
        east.Normalize();
        var north = Vector3d.Cross(east, normal);

        // Fold the texture rotation into the basis so that each vertex only
        // needs two dot products.
        var cos = Math.Cos(angle * Math.PI / 180.0);
        var sin = Math.Sin(angle * Math.PI / 180.0);
        uAxis = east * cos + north * sin;
        vAxis = north * cos - east * sin;

        if (mask == null)
        {
            Debug.LogWarning("PQSMod_KSCScatterMask: No mask specified, planet: " + sphere.name);
            modEnabled = false;
        }
    }

    public override void OnVertexBuild(PQS.VertexBuildData data)
    {
        if (debugShowColorMap)
            data.vertColor = Color.black;

        if (sphere.isBuildingMaps)
            return;

        var ratio = sphere.radius / (0.5 * width);
        var dir = data.directionFromCenter;
        var u = (Vector3d.Dot(dir, uAxis) * ratio + 1.0) * 0.5;
        var v = (Vector3d.Dot(dir, vAxis) * ratio + 1.0) * 0.5;

        if (u < 0.0 || u > 1.0)
            return;
        if (v < 0.0 || v > 1.0)
            return;

        if (mask is null)
            return;

        var value = mask.GetPixelFloat(u, v);
        data.allowScatter = value > 0.01f;
        if (debugShowColorMap)
            data.vertColor = data.allowScatter ? Color.green : Color.red;
    }


    private static bool WithinRadius(Vector3d a, Vector3d b, double distance) =>
        (a - b).sqrMagnitude <= distance * distance;
}
