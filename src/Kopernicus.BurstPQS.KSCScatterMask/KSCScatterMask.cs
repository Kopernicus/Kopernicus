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

namespace Kopernicus.BurstPQS.KSCScatterMask;

[BurstCompile]
[BatchPQSMod(typeof(PQSMod_KSCScatterMask))]
public class KSCScatterMask(PQSMod_KSCScatterMask mod)
    : BatchPQSMod<PQSMod_KSCScatterMask>(mod)
{
    public override void OnQuadPreBuild(PQ quad, BatchPQSJobSet jobSet)
    {
        base.OnQuadPreBuild(quad, jobSet);
        jobSet.Add(new BuildJob(mod));
    }

    [BurstCompile]
    struct BuildJob(PQSMod_KSCScatterMask mod) : IBatchPQSVertexJob, IDisposable
    {
        public BurstMapSO? colorMap = mod.colorMap is not null
            ? BurstMapSO.Create(mod.colorMap)
            : null;

        public bool quadActive = mod.quadActive;
        public double inclusionAngle = mod.inclusionAngle;
        public Vector3d normalizedPosition = mod.normalisedPosition;
        public Quaternion rot = mod.rot;
        public double radius = mod.radius;
        public bool debugShowColorMap = mod.debugShowColorMap;

        public bool cleanScatters = RuntimeUtility.RuntimeUtility.KopernicusConfig.CleanupKSCScatters;

        public void BuildVertices(in BuildVerticesData data)
        {
            if (!quadActive)
            {
                data.vertColor.Clear();
                return;
            }
            if (cleanScatters)
            {
                var sphere = data.sphere;

                for (int i = 0; i < data.VertexCount; ++i)
                {
                    if (sphere.isBuildingMaps)
                    {
                        var quadAngle = Math.Acos(
                        Vector3d.Dot(data.directionFromCenter[i], normalizedPosition)
                    );
                        if (quadAngle > inclusionAngle)
                            continue;
                    }

                    var vertRot = rot * data.directionFromCenter[i];
                    var u = (float)((vertRot.x * sphere.radius / radius + 1.0) * 0.5);
                    var v = (float)((vertRot.z * sphere.radius / radius + 1.0) * 0.5);

                    if (u > 1 || v > 1 || u < 0 || v < 0)
                        continue;

                    if (this.colorMap is not BurstMapSO colorMap)
                        continue;

                    var maskValue = colorMap.GetPixelColor(u, v).g;

                    if (debugShowColorMap)
                    {
                        data.vertColor[i] = maskValue > 0.01f ? Color.green : Color.red;
                    }
                    data.allowScatter[i] = maskValue > 0.01f ? true : false;
                }
            }
        }

        public void Dispose()
        {
            colorMap?.Dispose();
            colorMap = null;
        }
    }
}
