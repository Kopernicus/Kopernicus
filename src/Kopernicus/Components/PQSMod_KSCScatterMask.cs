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

namespace Kopernicus.Components
{
    /// <summary>
    /// PQSMod that disables scatters per-vertex based on a mask map
    /// </summary>
    public class PQSMod_KSCScatterMask : PQSMod
    {
        public double radius;
        public Vector3 position = Vector3.zero;
        public float angle;
        public MapSO colorMap;
        public bool debugShowColorMap;
        private double inclusionAngle;
        private bool quadActive;
        public Vector3d normalisedPosition;
        private double quadAngle;
        private float maskValue;
        private Vector3d vertRot;
        public Quaternion rot;
        private float u;
        private float v;
        private void Reset()
        {
            radius = 100.0;
            position = Vector3.forward;
            angle = 0f;
            vertRot = Vector3.forward;
        }

        public override void OnSetup()
        {
            if (HighLogic.LoadedSceneIsGame)
            {
                if (!SpaceCenter.Instance.cb.displayName.Contains(sphere.name))
                {
                    UnityEngine.Object.Destroy(this);
                    return;
                }
                requirements = (PQS.ModiferRequirements.MeshColorChannel);
                position = SpaceCenter.Instance.SrfNVector;
                normalisedPosition = position.normalized;
                inclusionAngle = Math.Atan(radius / sphere.radius) * 4.0;
                rot = Quaternion.AngleAxis(angle, Vector3.up) * Quaternion.FromToRotation(normalisedPosition, Vector3.up);
                if (colorMap == null)
                {
                    Debug.LogWarning("PQSMod_KSCScatterMask: No color map specified, planet: " + sphere.name);
                    modEnabled = false;
                    return;
                }
            }
        }
        public override void OnQuadPreBuild(PQ quad)
        {
            base.OnQuadPreBuild(quad);
            if (!RuntimeUtility.RuntimeUtility.KopernicusConfig.CleanupKSCScatters)
            {
                modEnabled = false;
                return;
            }
            else
            {
                modEnabled = true;
            }
        }
        public override void OnVertexBuild(PQS.VertexBuildData vertexBuildData)
        {
            if (!quadActive)
            {
                if (debugShowColorMap)
                {
                    vertexBuildData.vertColor = Color.black;
                }
                return;
            }

            if (sphere.isBuildingMaps)
            {
                quadAngle = Math.Acos(Vector3d.Dot(vertexBuildData.directionFromCenter, normalisedPosition));
                if (quadAngle > inclusionAngle)
                {
                    return;
                }
            }
            vertRot = rot * vertexBuildData.directionFromCenter;
            u = (float)((vertRot.x * sphere.radius / radius + 1.0) * 0.5);
            v = (float)((vertRot.z * sphere.radius / radius + 1.0) * 0.5);

            if (u > 1 || v > 1 || u < 0 || v < 0)
            {
                return;
            }

            if (colorMap != null)
            {
                maskValue = colorMap.GetPixelColor(u, v).g;
                if (debugShowColorMap)
                {
                    vertexBuildData.vertColor = maskValue > 0.01f ? Color.green : Color.red;
                }
                vertexBuildData.allowScatter = maskValue > 0.01f ? true : false;
            }
        }

        public override void OnQuadBuilt(PQ quad)
        {
            quadActive = true;
        }
    }

    [RequireConfigType(ConfigType.Node)]
    public class KSCScatterMask : ModLoader<PQSMod_KSCScatterMask>
    {
        [ParserTarget("debugShowColorMap", Optional = true)]
        public NumericParser<Boolean> DebugShowColorMap
        {
            get { return Mod.debugShowColorMap; }
            set { Mod.debugShowColorMap = value; }
        }
        [ParserTarget("colorMap")]
        public MapSOParserRGB<MapSO> ColorMap
        {
            get { return Mod.colorMap; }
            set { Mod.colorMap = value; }
        }
        [ParserTarget("angle")]
        public NumericParser<Single> Angle
        {
            get { return Mod.angle; }
            set { Mod.angle = value; }
        }
        [ParserTarget("radius", Optional = false)]
        public NumericParser<double> Radius
        {
            get { return Mod.radius; }
            set { Mod.radius = value; }
        }
    }
}
