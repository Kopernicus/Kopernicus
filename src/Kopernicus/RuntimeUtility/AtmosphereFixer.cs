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
using Kopernicus.Configuration;
using Kopernicus.Constants;
using UnityEngine;

namespace Kopernicus.RuntimeUtility
{
    public class AtmosphereInfo
    {
        public static readonly Dictionary<String, AtmosphereInfo> Atmospheres = new Dictionary<String, AtmosphereInfo>();

        private readonly Boolean _debugAlwaysUpdateAll;
        private readonly Boolean _doScale;
        private readonly Single _outerRadius;
        private readonly Single _innerRadius;
        private readonly Single _eSun;
        private readonly Single _kr;
        private readonly Single _km;
        private readonly Vector3 _transformScale;
        private readonly Single _scaleDepth;
        private readonly Single _samples;
        private readonly Single _g;
        private readonly Color _waveLength;
        private readonly Color _invWaveLength;

        public static void StoreAfg(AtmosphereFromGround afg)
        {
            if (afg.planet == null)
            {
                Debug.Log("[Kopernicus] Trying to store AFG, but planet null!");
                return;
            }
            Atmospheres[afg.planet.transform.name] = new AtmosphereInfo(afg);
        }

        public static Boolean PatchAfg(AtmosphereFromGround afg)
        {
            if (!Atmospheres.TryGetValue(afg.planet.transform.name, out AtmosphereInfo info))
            {
                return false;
            }
            try
            {
                info.Apply(afg);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private AtmosphereInfo(AtmosphereFromGround afg)
        {
            _debugAlwaysUpdateAll = afg.DEBUG_alwaysUpdateAll;
            _doScale = afg.doScale;
            _eSun = afg.ESun;
            _kr = afg.Kr;
            _km = afg.Km;
            _transformScale = afg.transform.localScale;
            _scaleDepth = afg.scaleDepth;
            _samples = afg.samples;
            _g = afg.g;
            _waveLength = afg.waveLength;
            _invWaveLength = afg.invWaveLength;
            _outerRadius = afg.outerRadius;
            _innerRadius = afg.innerRadius;
        }

        private void Apply(AtmosphereFromGround afg)
        {
            Transform transform = afg.transform;
            afg.DEBUG_alwaysUpdateAll = _debugAlwaysUpdateAll;
            afg.doScale = _doScale;
            afg.ESun = _eSun;
            afg.Kr = _kr;
            afg.Km = _km;
            transform.localScale = _transformScale;
            afg.scaleDepth = _scaleDepth;
            afg.samples = _samples;
            afg.g = _g;
            afg.waveLength = _waveLength;
            afg.invWaveLength = _invWaveLength;
            afg.outerRadius = _outerRadius;
            afg.innerRadius = _innerRadius;
            transform.localPosition = Vector3.zero;

            AtmosphereFromGroundLoader.CalculatedMembers(afg);
            afg.SetMaterial(true);

            Events.OnRuntimeUtilityPatchAFG.Fire(afg);
        }
    }

    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class AtmosphereFixer : MonoBehaviour
    {
        private Double _timeCounter;

        private void Awake()
        {
            if (!CompatibilityChecker.IsCompatible())
            {
                Destroy(this);
                return;
            }

            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                return;
            }

            if ((FlightGlobals.GetHomeBody() != null)
                && (FlightGlobals.GetHomeBody().atmosphericAmbientColor != null))
            {
                RenderSettings.ambientLight = FlightGlobals.GetHomeBody().atmosphericAmbientColor;
            }
        }

        private void Start()
        {
            if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                return;
            }
            Destroy(this); // don't hang around.
        }

        private void Update()
        {
            if (_timeCounter < 0.5d)
            {
                _timeCounter += Time.deltaTime;
                return;
            }
            foreach (AtmosphereFromGround afg in Resources.FindObjectsOfTypeAll<AtmosphereFromGround>())
            {
                if (!afg.planet)
                {
                    continue;
                }

                if (AtmosphereInfo.PatchAfg(afg))
                {
                    Debug.Log("[Kopernicus] AtmosphereFixer => Patched AtmosphereFromGround for " +
                              afg.planet.bodyName);
                }
            }
            Destroy(this); // don't hang around.
        }
    }
}
