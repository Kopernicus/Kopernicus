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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using KSP;
using KSP.IO;
using UnityEngine;

namespace Kopernicus.Configuration
{
    public class ConfigReader
    {
        [Persistent]
        public string HomeWorldName = "Kerbin";
        [Persistent]
        public bool EnforceShaders = false;
        [Persistent]
        public bool WarnShaders = false;
        [Persistent]
        public int EnforcedShaderLevel = 2;
        [Persistent]
        public string UseKopernicusAsteroidSystem = "True";
        [Persistent]
        public int SolarRefreshRate = 1;
        [Persistent]
        public bool EnableKopernicusShadowManager = true;
        [Persistent]
        public int ShadowRangeCap = 50000;
        [Persistent]
        public bool DisableMainMenuMunScene = true;
        [Persistent]
        public bool HandleHomeworldAtmosphericUnitDisplay = true;
        [Persistent]
        public bool UseIncorrectScatterDensityLogic = false;
        [Persistent]
        public bool DisableFarAwayColliders = true;
        [Persistent]
        public bool EnableAtmosphericExtinction = false;
        [Persistent]
        public bool UseStockMohoTemplate = true;
        [Persistent]
        public bool UseOnDemandLoader = false;
        [Persistent]
        public bool UseRealWorldDensity = false;
        [Persistent]
        public bool RecomputeSOIAndHillSpheres = false;
        [Persistent]
        public bool LimitRWDensityToStockBodies = true;
        [Persistent] 
        public bool UseOlderRWScalingLogic = false;
        [Persistent]
        public float RescaleFactor = 1.0f;
        [Persistent]
        public float RealWorldSizeFactor = 10.625f;
        [Persistent]
        public string SelectedPQSQuality = PQSCache.PresetList.preset;
        [Persistent]
        public float SettingsWindowXcoord = 0;
        [Persistent]
        public float SettingsWindowYcoord = 0;

        public UrlDir.UrlConfig[] baseConfigs;
        public void loadMainSettings()
        {
            baseConfigs = GameDatabase.Instance.GetConfigs("Kopernicus_config");
            if (baseConfigs.Length == 0)
            {
                Debug.LogWarning("No Kopernicus_Config file found, using defaults");
                return;
            }

            if (baseConfigs.Length > 1)
            {
                Debug.LogWarning("Multiple Kopernicus_Config files detected, check your install");
            }
            try
            {
                ConfigNode.LoadObjectFromConfig(this, baseConfigs[0].config);
                Debug.Log("[Kopernicus Configurations] Using: ");
                Debug.Log("HomeWorldName: " + HomeWorldName);
                Debug.Log("EnforceShaders: " + EnforceShaders);
                Debug.Log("WarnShaders: " + WarnShaders);
                Debug.Log("EnforcedShaderLevel: " + EnforcedShaderLevel);
                Debug.Log("UseKopernicusAsteroidSystem: " + UseKopernicusAsteroidSystem);
                Debug.Log("SolarRefreshRate: " + SolarRefreshRate);
                Debug.Log("EnableKopernicusShadowManager: " + EnableKopernicusShadowManager);
                Debug.Log("ShadowRangeCap: " + ShadowRangeCap);
                Debug.Log("DisableMainMenuMunScene: " + DisableMainMenuMunScene);
                Debug.Log("HandleHomeworldAtmosphericUnitDisplay: " + HandleHomeworldAtmosphericUnitDisplay);
                Debug.Log("UseIncorrectScatterDensityLogic: " + UseIncorrectScatterDensityLogic);
                Debug.Log("DisableFarAwayColliders: " + DisableFarAwayColliders);
                Debug.Log("EnableAtmosphericExtinction: " + EnableAtmosphericExtinction);
                Debug.Log("UseStockMohoTemplate: " + UseStockMohoTemplate);
                Debug.Log("UseOnDemandLoader: " + UseOnDemandLoader);
                Debug.Log("UseRealWorldDensity: " + UseRealWorldDensity);
                Debug.Log("RecomputeSOIAndHillSpheres: " + RecomputeSOIAndHillSpheres);
                Debug.Log("LimitRWDensityToStockBodies: " + LimitRWDensityToStockBodies);
                Debug.Log("UseOlderRWScalingLogic: " + UseOlderRWScalingLogic);
                Debug.Log("RescaleFactor: " + RescaleFactor);
                Debug.Log("RealWorldSizeFactor: " + RealWorldSizeFactor);
                Debug.Log("SettingsWindowXcoord: " + SettingsWindowXcoord);
                Debug.Log("SettingsWindowYcoord: " + SettingsWindowYcoord);
            }
            catch
            {
                Debug.LogWarning("Error loading config, using defaults");
            }
        }
    }
    public class ConfigLoader : MonoBehaviour
    {
        public static void ModuleManagerPostLoad()
        {
            //Load our settings
            RuntimeUtility.RuntimeUtility.KopernicusConfig.loadMainSettings();
        }
    }
}
