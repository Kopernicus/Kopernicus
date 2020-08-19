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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Kopernicus.Components;
using Kopernicus.Configuration;
using Kopernicus.Configuration.Asteroids;
using Kopernicus.OnDemand;
using ModularFI;
using UnityEngine;

namespace Kopernicus
{
    /// <summary>
    /// Utility methods for creating custom game events
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public class Events : MonoBehaviour
    {
        [Description("Injector.PreLoad")]
        public static EventVoid OnPreLoad { get; private set; }
        [Description("Injector.PostLoad")]
        public static EventData<PSystem> OnPostLoad { get; private set; }
        [Description("Injector.PreFixing")]
        public static EventVoid OnPreFixing { get; private set; }
        [Description("Injector.PreBodyFixing")]
        public static EventData<CelestialBody> OnPreBodyFixing { get; private set; }
        [Description("Injector.PostBodyFixing")]
        public static EventData<CelestialBody> OnPostBodyFixing { get; private set; }
        [Description("Injector.PostFixing")]
        public static EventVoid OnPostFixing { get; private set; }

        [Description("AtmosphereFromGroundLoader.Apply")]
        public static EventData<AtmosphereFromGroundLoader, ConfigNode> OnAFGLoaderApply { get; private set; }
        [Description("AtmosphereFromGroundLoader.PostApply")]
        public static EventData<AtmosphereFromGroundLoader, ConfigNode> OnAFGLoaderPostApply { get; private set; }

        [Description("AtmosphereFromGroundLoader.Apply.NR")]
        private static EventData<ConfigNode> OnAFGLoaderApplyNR { get; set; }
        [Description("AtmosphereFromGroundLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnAFGLoaderPostApplyNR { get; set; }

        [Description("AtmosphereLoader.Apply")]
        public static EventData<AtmosphereLoader, ConfigNode> OnAtmosphereLoaderApply { get; private set; }
        [Description("AtmosphereLoader.PostApply")]
        public static EventData<AtmosphereLoader, ConfigNode> OnAtmosphereLoaderPostApply { get; private set; }

        [Description("AtmosphereLoader.Apply.NR")]
        private static EventData<ConfigNode> OnAtmosphereLoaderApplyNR { get; set; }
        [Description("AtmosphereLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnAtmosphereLoaderPostApplyNR { get; set; }

        [Description("BiomeLoader.Apply")]
        public static EventData<BiomeLoader, ConfigNode> OnBiomeLoaderApply { get; private set; }
        [Description("BiomeLoader.PostApply")]
        public static EventData<BiomeLoader, ConfigNode> OnBiomeLoaderPostApply { get; private set; }

        [Description("BiomeLoader.Apply.NR")]
        private static EventData<ConfigNode> OnBiomeLoaderApplyNR { get; set; }
        [Description("BiomeLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnBiomeLoaderPostApplyNR { get; set; }

        [Description("CoronaLoader.Apply")]
        public static EventData<CoronaLoader, ConfigNode> OnCoronaLoaderApply { get; private set; }
        [Description("CoronaLoader.PostApply")]
        public static EventData<CoronaLoader, ConfigNode> OnCoronaLoaderPostApply { get; private set; }

        [Description("CoronaLoader.Apply.NR")]
        private static EventData<ConfigNode> OnCoronaLoaderApplyNR { get; set; }
        [Description("CoronaLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnCoronaLoaderPostApplyNR { get; set; }

        [Description("DebugLoader.Apply")]
        public static EventData<DebugLoader, ConfigNode> OnDebugLoaderApply { get; private set; }
        [Description("DebugLoader.PostApply")]
        public static EventData<DebugLoader, ConfigNode> OnDebugLoaderPostApply { get; private set; }

        [Description("DebugLoader.Apply.NR")]
        private static EventData<ConfigNode> OnDebugLoaderApplyNR { get; set; }
        [Description("DebugLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnDebugLoaderPostApplyNR { get; set; }

        [Description("FogLoader.Apply")]
        public static EventData<FogLoader, ConfigNode> OnFogLoaderApply { get; private set; }
        [Description("FogLoader.PostApply")]
        public static EventData<FogLoader, ConfigNode> OnFogLoaderPostApply { get; private set; }

        [Description("FogLoader.Apply.NR")]
        private static EventData<ConfigNode> OnFogLoaderApplyNR { get; set; }
        [Description("FogLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnFogLoaderPostApplyNR { get; set; }

        [Description("LightShifterLoader.Apply")]
        public static EventData<LightShifterLoader, ConfigNode> OnLightShifterLoaderApply { get; private set; }
        [Description("LightShifterLoader.PostApply")]
        public static EventData<LightShifterLoader, ConfigNode> OnLightShifterLoaderPostApply { get; private set; }

        [Description("LightShifterLoader.Apply.NR")]
        private static EventData<ConfigNode> OnLightShifterLoaderApplyNR { get; set; }
        [Description("LightShifterLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnLightShifterLoaderPostApplyNR { get; set; }

        [Description("OceanLoader.Apply")]
        public static EventData<OceanLoader, ConfigNode> OnOceanLoaderApply { get; private set; }
        [Description("OceanLoader.PostApply")]
        public static EventData<OceanLoader, ConfigNode> OnOceanLoaderPostApply { get; private set; }

        [Description("OceanLoader.Apply.NR")]
        private static EventData<ConfigNode> OnOceanLoaderApplyNR { get; set; }
        [Description("OceanLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnOceanLoaderPostApplyNR { get; set; }

        [Description("OrbitLoader.Apply")]
        public static EventData<OrbitLoader, ConfigNode> OnOrbitLoaderApply { get; private set; }
        [Description("OrbitLoader.PostApply")]
        public static EventData<OrbitLoader, ConfigNode> OnOrbitLoaderPostApply { get; private set; }

        [Description("OrbitLoader.Apply.NR")]
        private static EventData<ConfigNode> OnOrbitLoaderApplyNR { get; set; }
        [Description("OrbitLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnOrbitLoaderPostApplyNR { get; set; }

        [Description("ParticleLoader.Apply")]
        public static EventData<ParticleLoader, ConfigNode> OnParticleLoaderApply { get; private set; }
        [Description("ParticleLoader.PostApply")]
        public static EventData<ParticleLoader, ConfigNode> OnParticleLoaderPostApply { get; private set; }

        [Description("ParticleLoader.Apply.NR")]
        private static EventData<ConfigNode> OnParticleLoaderApplyNR { get; set; }
        [Description("ParticleLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnParticleLoaderPostApplyNR { get; set; }

        [Description("PQSLoader.Apply")]
        public static EventData<PQSLoader, ConfigNode> OnPQSLoaderApply { get; private set; }
        [Description("PQSLoader.PostApply")]
        public static EventData<PQSLoader, ConfigNode> OnPQSLoaderPostApply { get; private set; }

        [Description("PQSLoader.Apply.NR")]
        private static EventData<ConfigNode> OnPQSLoaderApplyNR { get; set; }
        [Description("PQSLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnPQSLoaderPostApplyNR { get; set; }

        [Description("PropertiesLoader.Apply")]
        public static EventData<PropertiesLoader, ConfigNode> OnPropertiesLoaderApply { get; private set; }
        [Description("PropertiesLoader.PostApply")]
        public static EventData<PropertiesLoader, ConfigNode> OnPropertiesLoaderPostApply { get; private set; }

        [Description("PropertiesLoader.Apply.NR")]
        private static EventData<ConfigNode> OnPropertiesLoaderApplyNR { get; set; }
        [Description("PropertiesLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnPropertiesLoaderPostApplyNR { get; set; }

        [Description("RingLoader.Apply")]
        public static EventData<RingLoader, ConfigNode> OnRingLoaderApply { get; private set; }
        [Description("RingLoader.PostApply")]
        public static EventData<RingLoader, ConfigNode> OnRingLoaderPostApply { get; private set; }

        [Description("RingLoader.Apply.NR")]
        private static EventData<ConfigNode> OnRingLoaderApplyNR { get; set; }
        [Description("RingLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnRingLoaderPostApplyNR { get; set; }

        [Description("ScaledVersionLoader.Apply")]
        public static EventData<ScaledVersionLoader, ConfigNode> OnScaledVersionLoaderApply { get; private set; }
        [Description("ScaledVersionLoader.PostApply")]
        public static EventData<ScaledVersionLoader, ConfigNode> OnScaledVersionLoaderPostApply { get; private set; }

        [Description("ScaledVersionLoader.Apply.NR")]
        private static EventData<ConfigNode> OnScaledVersionLoaderApplyNR { get; set; }
        [Description("ScaledVersionLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnScaledVersionLoaderPostApplyNR { get; set; }

        [Description("ScienceValuesLoader.Apply")]
        public static EventData<ScienceValuesLoader, ConfigNode> OnScienceValuesLoaderApply { get; private set; }
        [Description("ScienceValuesLoader.PostApply")]
        public static EventData<ScienceValuesLoader, ConfigNode> OnScienceValuesLoaderPostApply { get; private set; }

        [Description("ScienceValuesLoader.Apply.NR")]
        private static EventData<ConfigNode> OnScienceValuesLoaderApplyNR { get; set; }
        [Description("ScienceValuesLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnScienceValuesLoaderPostApplyNR { get; set; }

        [Description("SpaceCenterLoader.Apply")]
        public static EventData<SpaceCenterLoader, ConfigNode> OnSpaceCenterLoaderApply { get; private set; }
        [Description("SpaceCenterLoader.PostApply")]
        public static EventData<SpaceCenterLoader, ConfigNode> OnSpaceCenterLoaderPostApply { get; private set; }

        [Description("SpaceCenterLoader.Apply.NR")]
        private static EventData<ConfigNode> OnSpaceCenterLoaderApplyNR { get; set; }
        [Description("SpaceCenterLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnSpaceCenterLoaderPostApplyNR { get; set; }

        [Description("TemplateLoader.Apply")]
        public static EventData<TemplateLoader, ConfigNode> OnTemplateLoaderApply { get; private set; }
        [Description("TemplateLoader.PostApply")]
        public static EventData<TemplateLoader, ConfigNode> OnTemplateLoaderPostApply { get; private set; }

        [Description("TemplateLoader.Apply.NR")]
        private static EventData<ConfigNode> OnTemplateLoaderApplyNR { get; set; }
        [Description("TemplateLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnTemplateLoaderPostApplyNR { get; set; }

        [Description("Loader.Apply")]
        public static EventData<Loader, ConfigNode> OnLoaderApply { get; private set; }
        [Description("Loader.PostApply")]
        public static EventData<Loader, ConfigNode> OnLoaderPostApply { get; private set; }
        [Description("Loader.LoadBody")]
        public static EventData<Body, ConfigNode> OnLoaderLoadBody { get; private set; }
        [Description("Loader.LoadedAllBodies")]
        public static EventData<Loader, ConfigNode> OnLoaderLoadedAllBodies { get; private set; }
        [Description("Loader.LoadAsteroid")]
        public static EventData<Asteroid, ConfigNode> OnLoaderLoadAsteroid { get; private set; }
        [Description("Loader.FinalizeBody")]
        public static EventData<Body> OnLoaderFinalizeBody { get; private set; }

        [Description("Loader.Apply.NR")]
        private static EventData<ConfigNode> OnLoaderApplyNR { get; set; }
        [Description("Loader.PostApply.NR")]
        private static EventData<ConfigNode> OnLoaderPostApplyNR { get; set; }
        [Description("Loader.LoadBody.NR")]
        private static EventData<ConfigNode> OnLoaderLoadBodyNR { get; set; }
        [Description("Loader.LoadedAllBodies.NR")]
        private static EventData<ConfigNode> OnLoaderLoadedAllBodiesNR { get; set; }
        [Description("Loader.LoadAsteroid.NR")]
        private static EventData<ConfigNode> OnLoaderLoadAsteroidNR { get; set; }
        [Description("Loader.FinalizeBody.NR")]
        private static EventVoid OnLoaderFinalizeBodyNR { get; set; }

        [Description("Body.Apply")]
        public static EventData<Body, ConfigNode> OnBodyApply { get; private set; }
        [Description("Body.PostApply")]
        public static EventData<Body, ConfigNode> OnBodyPostApply { get; private set; }
        [Description("Body.GenerateScaledSpace")]
        public static EventData<Body, ConfigNode> OnBodyGenerateScaledSpace { get; private set; }

        [Description("Body.Apply.NR")]
        private static EventData<ConfigNode> OnBodyApplyNR { get; set; }
        [Description("Body.PostApply.NR")]
        private static EventData<ConfigNode> OnBodyPostApplyNR { get; set; }
        [Description("Body.GenerateScaledSpace.NR")]
        private static EventData<ConfigNode> OnBodyGenerateScaledSpaceNR { get; set; }
        
        [Description("RuntimeUtility.PatchAFG")]
        public static EventData<AtmosphereFromGround> OnRuntimeUtilityPatchAFG { get; private set; }
        [Description("RuntimeUtility.SpawnAsteroid")]
        public static EventData<Asteroid, ProtoVessel> OnRuntimeUtilitySpawnAsteroid { get; private set; }
        [Description("RuntimeUtility.UpdateMenu")]
        public static EventVoid OnRuntimeUtilityUpdateMenu { get; private set; }
        [Description("RuntimeUtility.PatchFI")]
        public static EventVoid OnRuntimeUtilityPatchFI { get; private set; }
        [Description("RuntimeUtility.SwitchStar")]
        public static EventData<KopernicusStar> OnRuntimeUtilitySwitchStar { get; private set; }

        [Description("RuntimeUtility.SpawnAsteroid.NR")]
        private static EventData<ProtoVessel> OnRuntimeUtilitySpawnAsteroidNR { get; set; }
        [Description("RuntimeUtility.SwitchStar.NR")]
        private static EventVoid OnRuntimeUtilitySwitchStarNR { get; set; }
        
        [Description("Components.SwitchKSC")]
        public static EventData<KSC> OnSwitchKSC { get; private set; }
        [Description("Components.ApplyNameChange")]
        public static EventData<NameChanger, CelestialBody> OnApplyNameChange { get; private set; }
        [Description("Components.KopernicusHeatManager")]
        public static EventData<ModularFlightIntegrator> OnCalculateBackgroundRadiationTemperature { get; set; }

        [Description("Components.SwitchKSC.NR")]
        private static EventVoid OnSwitchKSCNR { get; set; }
        [Description("Components.ApplyNameChange.NR")]
        private static EventData<CelestialBody> OnApplyNameChangeNR { get; set; }
        
        [Description("OnDemand.MapSO.Load")]
        public static EventData<MapSODemand> OnMapSOLoad { get; private set; }
        [Description("OnDemand.MapSO.Unload")]
        public static EventData<MapSODemand> OnMapSOUnload { get; private set; }
            
        [Description("OnDemand.CBMapSO.Load")]
        public static EventData<CBAttributeMapSODemand> OnCBMapSOLoad { get; private set; }
        [Description("OnDemand.CBMapSO.Unload")]
        public static EventData<CBAttributeMapSODemand> OnCBMapSOUnload { get; private set; }

        [Description("OnDemand.ScaledSpace.Load")]
        public static EventData<ScaledSpaceOnDemand> OnScaledSpaceLoad { get; private set; }
        [Description("OnDemand.ScaledSpace.Unload")]
        public static EventData<ScaledSpaceOnDemand> OnScaledSpaceUnload { get; private set; }
            
        [Description("OnDemand.MapSO.Load.NR")]
        private static EventVoid OnMapSOLoadNR { get; set; }
        [Description("OnDemand.MapSO.Unload.NR")]
        private static EventVoid OnMapSOUnloadNR { get; set; }

        [Description("OnDemand.CBMapSO.Load.NR")]
        private static EventVoid OnCBMapSOLoadNR { get; set; }
        [Description("OnDemand.CBMapSO.Unload.NR")]
        private static EventVoid OnCBMapSOUnloadNR { get; set; }

        [Description("OnDemand.ScaledSpace.Load.NR")]
        private static EventVoid OnScaledSpaceLoadNR { get; set; }
        [Description("OnDemand.ScaledSpace.Unload.NR")]
        private static EventVoid OnScaledSpaceUnloadNR { get; set; }

        private void Awake()
        {
            PropertyInfo[] events = typeof(Events).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            for (Int32 i = 0; i < events.Length; i++)
            {
                PropertyInfo info = events[i];
                DescriptionAttribute description = ((DescriptionAttribute[]) info.GetCustomAttributes(typeof(DescriptionAttribute), false))[0];
                events[i].SetValue(null, Activator.CreateInstance(events[i].PropertyType, "Kopernicus." + description.Description), null);
            }
            RegisterNREvents();
            Destroy(this);
        }

        private static void RegisterNREvents()
        {            
            OnAFGLoaderApply.Add((a, c) => OnAFGLoaderApplyNR.Fire(c));
            OnAFGLoaderPostApply.Add((a, c) => OnAFGLoaderPostApplyNR.Fire(c));
            OnAtmosphereLoaderApply.Add((a, c) => OnAtmosphereLoaderApplyNR.Fire(c));
            OnAtmosphereLoaderPostApply.Add((a, c) => OnAtmosphereLoaderPostApplyNR.Fire(c));
            OnBiomeLoaderApply.Add((a, c) => OnBiomeLoaderApplyNR.Fire(c));
            OnBiomeLoaderPostApply.Add((a, c) => OnBiomeLoaderPostApplyNR.Fire(c));
            OnCoronaLoaderApply.Add((a, c) => OnCoronaLoaderApplyNR.Fire(c));
            OnCoronaLoaderPostApply.Add((a, c) => OnCoronaLoaderPostApplyNR.Fire(c));
            OnDebugLoaderApply.Add((a, c) => OnDebugLoaderApplyNR.Fire(c));
            OnDebugLoaderPostApply.Add((a, c) => OnDebugLoaderPostApplyNR.Fire(c));
            OnFogLoaderApply.Add((a, c) => OnFogLoaderApplyNR.Fire(c));
            OnFogLoaderPostApply.Add((a, c) => OnFogLoaderPostApplyNR.Fire(c));
            OnLightShifterLoaderApply.Add((a, c) => OnLightShifterLoaderApplyNR.Fire(c));
            OnLightShifterLoaderPostApply.Add((a, c) => OnLightShifterLoaderPostApplyNR.Fire(c));
            OnOceanLoaderApply.Add((a, c) => OnOceanLoaderApplyNR.Fire(c));
            OnOceanLoaderPostApply.Add((a, c) => OnOceanLoaderPostApplyNR.Fire(c));
            OnOrbitLoaderApply.Add((a, c) => OnOrbitLoaderApplyNR.Fire(c));
            OnOrbitLoaderPostApply.Add((a, c) => OnOrbitLoaderPostApplyNR.Fire(c));
            OnParticleLoaderApply.Add((a, c) => OnParticleLoaderApplyNR.Fire(c));
            OnParticleLoaderPostApply.Add((a, c) => OnParticleLoaderPostApplyNR.Fire(c));
            OnPQSLoaderApply.Add((a, c) => OnPQSLoaderApplyNR.Fire(c));
            OnPQSLoaderPostApply.Add((a, c) => OnPQSLoaderPostApplyNR.Fire(c));
            OnPropertiesLoaderApply.Add((a, c) => OnPropertiesLoaderApplyNR.Fire(c));
            OnPropertiesLoaderPostApply.Add((a, c) => OnPropertiesLoaderPostApplyNR.Fire(c));
            OnRingLoaderApply.Add((a, c) => OnRingLoaderApplyNR.Fire(c));
            OnRingLoaderPostApply.Add((a, c) => OnRingLoaderPostApplyNR.Fire(c));
            OnScaledVersionLoaderApply.Add((a, c) => OnScaledVersionLoaderApplyNR.Fire(c));
            OnScaledVersionLoaderPostApply.Add((a, c) => OnScaledVersionLoaderPostApplyNR.Fire(c));
            OnScienceValuesLoaderApply.Add((a, c) => OnScienceValuesLoaderApplyNR.Fire(c));
            OnScienceValuesLoaderPostApply.Add((a, c) => OnScienceValuesLoaderPostApplyNR.Fire(c));
            OnSpaceCenterLoaderApply.Add((a, c) => OnSpaceCenterLoaderApplyNR.Fire(c));
            OnSpaceCenterLoaderPostApply.Add((a, c) => OnSpaceCenterLoaderPostApplyNR.Fire(c));
            OnTemplateLoaderApply.Add((a, c) => OnTemplateLoaderApplyNR.Fire(c));
            OnTemplateLoaderPostApply.Add((a, c) => OnTemplateLoaderPostApplyNR.Fire(c));
            OnLoaderApply.Add((a, c) => OnLoaderApplyNR.Fire(c));
            OnLoaderPostApply.Add((a, c) => OnLoaderPostApplyNR.Fire(c));
            OnLoaderLoadBody.Add((a, c) => OnLoaderLoadBodyNR.Fire(c));
            OnLoaderLoadedAllBodies.Add((a, c) => OnLoaderLoadedAllBodiesNR.Fire(c));
            OnLoaderLoadAsteroid.Add((a, c) => OnLoaderLoadAsteroidNR.Fire(c));
            OnLoaderFinalizeBody.Add(a => OnLoaderFinalizeBodyNR.Fire());
            OnBodyApply.Add((a, c) => OnBodyApplyNR.Fire(c));
            OnBodyPostApply.Add((a, c) => OnBodyPostApplyNR.Fire(c));
            OnBodyGenerateScaledSpace.Add((a, c) => OnBodyGenerateScaledSpaceNR.Fire(c));
            OnRuntimeUtilitySpawnAsteroid.Add((a, c) => OnRuntimeUtilitySpawnAsteroidNR.Fire(c));
            OnRuntimeUtilitySwitchStar.Add(a => OnRuntimeUtilitySwitchStarNR.Fire());
            OnSwitchKSC.Add(a => OnSwitchKSCNR.Fire());
            OnApplyNameChange.Add((a, c) => OnApplyNameChangeNR.Fire(c));
            OnMapSOLoad.Add(a => OnMapSOLoadNR.Fire());
            OnMapSOUnload.Add(a => OnMapSOUnloadNR.Fire());
            OnCBMapSOLoad.Add(a => OnCBMapSOLoadNR.Fire());
            OnCBMapSOUnload.Add(a => OnCBMapSOUnloadNR.Fire());
            OnScaledSpaceLoad.Add(a => OnScaledSpaceLoadNR.Fire());
            OnScaledSpaceUnload.Add(a => OnScaledSpaceUnloadNR.Fire());
        }
    }
}