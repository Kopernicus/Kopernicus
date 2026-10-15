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
using BurstPQS.Map;
using Unity.Burst;
using UnityEngine;
using Kopernicus.Components;
using BurstPQS;

namespace Kopernicus.BurstPQS;

[BurstCompile]
[BatchPQSMod(typeof(PQSMod_ScatterMask))]
public class ScatterMask(PQSMod_ScatterMask mod)
    : BatchPQSMod<PQSMod_ScatterMask>(mod)
{
    private const double Sqrt2 = 1.4142135623730951;

    private BurstMapSO? mask;

    public override void OnSetup()
    {
        base.OnSetup();

        if (mod.mask is not null)
            mask = BurstMapSO.Create(mod.mask);
    }

    public override void Dispose()
    {
        base.Dispose();

        mask?.Dispose();
        mask = null;
    }

    public override void OnQuadPreBuild(PQ quad, BatchPQSJobSet jobSet)
    {
        base.OnQuadPreBuild(quad, jobSet);

        if (mod.debugShowColorMap || QuadOverlapsMask(quad))
            jobSet.Add(new BuildJob(mod, mask));
    }


    [BurstCompile(FloatMode = FloatMode.Fast)]
    struct BuildJob(PQSMod_ScatterMask mod, BurstMapSO? mask) : IBatchPQSVertexJob, IDisposable
    {
        public BurstMapSO? mask = mask;

        public Vector3d uAxis = mod.uAxis;
        public Vector3d vAxis = mod.vAxis;
        public double width = mod.width;
        public bool debugShowColorMap = mod.debugShowColorMap;

        public void BuildVertices(in BuildVerticesData data)
        {
            if (debugShowColorMap)
                data.vertColor.Fill(Color.black);

            var sphere = data.sphere;
            if (sphere.isBuildingMaps)
                return;

            if (this.mask is not BurstMapSO mask)
                return;

            var ratio = sphere.radius / (0.5 * width);

            for (int i = 0; i < data.VertexCount; ++i)
            {
                var dir = data.directionFromCenter[i];
                var u = (Vector3d.Dot(dir, uAxis) * ratio + 1.0) * 0.5;
                var v = (Vector3d.Dot(dir, vAxis) * ratio + 1.0) * 0.5;

                if (u < 0.0 || u > 1.0)
                    continue;
                if (v < 0.0 || v > 1.0)
                    continue;

                var value = mask.GetPixelFloat(u, v);
                var allowScatter = value > 0.01f;

                data.allowScatter[i] = allowScatter;
                if (debugShowColorMap)
                    data.vertColor[i] = allowScatter ? Color.green : Color.red;
            }
        }

        public void Dispose()
        {
            mask?.Dispose();
            mask = null;
        }
    }

    /// <summary>
    /// Could this quad potentially overlap with the mask?
    /// </summary>
    /// <param name="quad"></param>
    /// <returns></returns>
    private bool QuadOverlapsMask(PQ quad)
    {
        var quadPos = quad.positionPlanetRelative * mod.sphere.radius;
        return WithinRadius(
            mod.position,
            quadPos,
            (0.5 * mod.width + quad.scalePlanetRelative) * Sqrt2
        );
    }

    private static bool WithinRadius(Vector3d a, Vector3d b, double distance) =>
        (a - b).sqrMagnitude <= distance * distance;
}
