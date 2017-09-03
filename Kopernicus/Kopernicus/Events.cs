/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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

using Kopernicus.Components;
using Kopernicus.Configuration;
using Kopernicus.Configuration.Asteroids;
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    /// <summary>
    /// Utility methods for creating custom game events
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Events : MonoBehaviour
    {
        [Description("Injector.PreLoad")]
        public static EventVoid OnPreLoad { get; }
        [Description("Injector.PostLoad")]
        public static EventData<PSystem> OnPostLoad { get; }
        [Description("Injector.PreFixing")]
        public static EventVoid OnPreFixing { get; }
        [Description("Injector.PreBodyFixing")]
        public static EventData<CelestialBody> OnPreBodyFixing { get; }
        [Description("Injector.PostBodyFixing")]
        public static EventData<CelestialBody> OnPostBodyFixing { get; }
        [Description("Injector.PostFixing")]
        public static EventVoid OnPostFixing { get; }

        [Description("AtmosphereFromGroundLoader.Apply")]
        public static EventData<AtmosphereFromGroundLoader, ConfigNode> OnAFGLoaderApply { get; }
        [Description("AtmosphereFromGroundLoader.PostApply")]
        public static EventData<AtmosphereFromGroundLoader, ConfigNode> OnAFGLoaderPostApply { get; }

        [Description("AtmosphereFromGroundLoader.Apply.NR")]
        private static EventData<ConfigNode> OnAFGLoaderApplyNR { get; }
        [Description("AtmosphereFromGroundLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnAFGLoaderPostApplyNR { get; }

        [Description("AtmosphereLoader.Apply")]
        public static EventData<AtmosphereLoader, ConfigNode> OnAtmosphereLoaderApply { get; }
        [Description("AtmosphereLoader.PostApply")]
        public static EventData<AtmosphereLoader, ConfigNode> OnAtmosphereLoaderPostApply { get; }

        [Description("AtmosphereLoader.Apply.NR")]
        private static EventData<ConfigNode> OnAtmosphereLoaderApplyNR { get; }
        [Description("AtmosphereLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnAtmosphereLoaderPostApplyNR { get; }

        [Description("BiomeLoader.Apply")]
        public static EventData<BiomeLoader, ConfigNode> OnBiomeLoaderApply { get; }
        [Description("BiomeLoader.PostApply")]
        public static EventData<BiomeLoader, ConfigNode> OnBiomeLoaderPostApply { get; }

        [Description("BiomeLoader.Apply.NR")]
        private static EventData<ConfigNode> OnBiomeLoaderApplyNR { get; }
        [Description("BiomeLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnBiomeLoaderPostApplyNR { get; }

        [Description("CoronaLoader.Apply")]
        public static EventData<CoronaLoader, ConfigNode> OnCoronaLoaderApply { get; }
        [Description("CoronaLoader.PostApply")]
        public static EventData<CoronaLoader, ConfigNode> OnCoronaLoaderPostApply { get; }

        [Description("CoronaLoader.Apply.NR")]
        private static EventData<ConfigNode> OnCoronaLoaderApplyNR { get; }
        [Description("CoronaLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnCoronaLoaderPostApplyNR { get; }

        [Description("DebugLoader.Apply")]
        public static EventData<DebugLoader, ConfigNode> OnDebugLoaderApply { get; }
        [Description("DebugLoader.PostApply")]
        public static EventData<DebugLoader, ConfigNode> OnDebugLoaderPostApply { get; }

        [Description("DebugLoader.Apply.NR")]
        private static EventData<ConfigNode> OnDebugLoaderApplyNR { get; }
        [Description("DebugLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnDebugLoaderPostApplyNR { get; }

        [Description("FogLoader.Apply")]
        public static EventData<FogLoader, ConfigNode> OnFogLoaderApply { get; }
        [Description("FogLoader.PostApply")]
        public static EventData<FogLoader, ConfigNode> OnFogLoaderPostApply { get; }

        [Description("FogLoader.Apply.NR")]
        private static EventData<ConfigNode> OnFogLoaderApplyNR { get; }
        [Description("FogLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnFogLoaderPostApplyNR { get; }

        [Description("LightShifterLoader.Apply")]
        public static EventData<LightShifterLoader, ConfigNode> OnLightShifterLoaderApply { get; }
        [Description("LightShifterLoader.PostApply")]
        public static EventData<LightShifterLoader, ConfigNode> OnLightShifterLoaderPostApply { get; }

        [Description("LightShifterLoader.Apply.NR")]
        private static EventData<ConfigNode> OnLightShifterLoaderApplyNR { get; }
        [Description("LightShifterLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnLightShifterLoaderPostApplyNR { get; }

        [Description("OceanLoader.Apply")]
        public static EventData<OceanLoader, ConfigNode> OnOceanLoaderApply { get; }
        [Description("OceanLoader.PostApply")]
        public static EventData<OceanLoader, ConfigNode> OnOceanLoaderPostApply { get; }

        [Description("OceanLoader.Apply.NR")]
        private static EventData<ConfigNode> OnOceanLoaderApplyNR { get; }
        [Description("OceanLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnOceanLoaderPostApplyNR { get; }

        [Description("OrbitLoader.Apply")]
        public static EventData<OrbitLoader, ConfigNode> OnOrbitLoaderApply { get; }
        [Description("OrbitLoader.PostApply")]
        public static EventData<OrbitLoader, ConfigNode> OnOrbitLoaderPostApply { get; }

        [Description("OrbitLoader.Apply.NR")]
        private static EventData<ConfigNode> OnOrbitLoaderApplyNR { get; }
        [Description("OrbitLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnOrbitLoaderPostApplyNR { get; }

        [Description("ParticleLoader.Apply")]
        public static EventData<ParticleLoader, ConfigNode> OnParticleLoaderApply { get; }
        [Description("ParticleLoader.PostApply")]
        public static EventData<ParticleLoader, ConfigNode> OnParticleLoaderPostApply { get; }

        [Description("ParticleLoader.Apply.NR")]
        private static EventData<ConfigNode> OnParticleLoaderApplyNR { get; }
        [Description("ParticleLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnParticleLoaderPostApplyNR { get; }

        [Description("PQSLoader.Apply")]
        public static EventData<PQSLoader, ConfigNode> OnPQSLoaderApply { get; }
        [Description("PQSLoader.PostApply")]
        public static EventData<PQSLoader, ConfigNode> OnPQSLoaderPostApply { get; }

        [Description("PQSLoader.Apply.NR")]
        private static EventData<ConfigNode> OnPQSLoaderApplyNR { get; }
        [Description("PQSLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnPQSLoaderPostApplyNR { get; }

        [Description("PropertiesLoader.Apply")]
        public static EventData<PropertiesLoader, ConfigNode> OnPropertiesLoaderApply { get; }
        [Description("PropertiesLoader.PostApply")]
        public static EventData<PropertiesLoader, ConfigNode> OnPropertiesLoaderPostApply { get; }

        [Description("PropertiesLoader.Apply.NR")]
        private static EventData<ConfigNode> OnPropertiesLoaderApplyNR { get; }
        [Description("PropertiesLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnPropertiesLoaderPostApplyNR { get; }

        [Description("RingLoader.Apply")]
        public static EventData<RingLoader, ConfigNode> OnRingLoaderApply { get; }
        [Description("RingLoader.PostApply")]
        public static EventData<RingLoader, ConfigNode> OnRingLoaderPostApply { get; }

        [Description("RingLoader.Apply.NR")]
        private static EventData<ConfigNode> OnRingLoaderApplyNR { get; }
        [Description("RingLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnRingLoaderPostApplyNR { get; }

        [Description("ScaledVersionLoader.Apply")]
        public static EventData<ScaledVersionLoader, ConfigNode> OnScaledVersionLoaderApply { get; }
        [Description("ScaledVersionLoader.PostApply")]
        public static EventData<ScaledVersionLoader, ConfigNode> OnScaledVersionLoaderPostApply { get; }

        [Description("ScaledVersionLoader.Apply.NR")]
        private static EventData<ConfigNode> OnScaledVersionLoaderApplyNR { get; }
        [Description("ScaledVersionLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnScaledVersionLoaderPostApplyNR { get; }

        [Description("ScienceValuesLoader.Apply")]
        public static EventData<ScienceValuesLoader, ConfigNode> OnScienceValuesLoaderApply { get; }
        [Description("ScienceValuesLoader.PostApply")]
        public static EventData<ScienceValuesLoader, ConfigNode> OnScienceValuesLoaderPostApply { get; }

        [Description("ScienceValuesLoader.Apply.NR")]
        private static EventData<ConfigNode> OnScienceValuesLoaderApplyNR { get; }
        [Description("ScienceValuesLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnScienceValuesLoaderPostApplyNR { get; }

        [Description("SpaceCenterLoader.Apply")]
        public static EventData<SpaceCenterLoader, ConfigNode> OnSpaceCenterLoaderApply { get; }
        [Description("SpaceCenterLoader.PostApply")]
        public static EventData<SpaceCenterLoader, ConfigNode> OnSpaceCenterLoaderPostApply { get; }

        [Description("SpaceCenterLoader.Apply.NR")]
        private static EventData<ConfigNode> OnSpaceCenterLoaderApplyNR { get; }
        [Description("SpaceCenterLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnSpaceCenterLoaderPostApplyNR { get; }

        [Description("TemplateLoader.Apply")]
        public static EventData<TemplateLoader, ConfigNode> OnTemplateLoaderApply { get; }
        [Description("TemplateLoader.PostApply")]
        public static EventData<TemplateLoader, ConfigNode> OnTemplateLoaderPostApply { get; }

        [Description("TemplateLoader.Apply.NR")]
        private static EventData<ConfigNode> OnTemplateLoaderApplyNR { get; }
        [Description("TemplateLoader.PostApply.NR")]
        private static EventData<ConfigNode> OnTemplateLoaderPostApplyNR { get; }

        [Description("Loader.Apply")]
        public static EventData<Loader, ConfigNode> OnLoaderApply { get; }
        [Description("Loader.PostApply")]
        public static EventData<Loader, ConfigNode> OnLoaderPostApply { get; }
        [Description("Loader.LoadBody")]
        public static EventData<Body, ConfigNode> OnLoaderLoadBody { get; }
        [Description("Loader.LoadAsteroid")]
        public static EventData<Asteroid, ConfigNode> OnLoaderLoadAsteroid { get; }
        [Description("Loader.FinalizeBody")]
        public static EventData<Body> OnLoaderFinalizeBody { get; }

        [Description("Loader.Apply.NR")]
        private static EventData<ConfigNode> OnLoaderApplyNR { get; }
        [Description("Loader.PostApply.NR")]
        private static EventData<ConfigNode> OnLoaderPostApplyNR { get; }
        [Description("Loader.LoadBody.NR")]
        private static EventData<ConfigNode> OnLoaderLoadBodyNR { get; }
        [Description("Loader.LoadAsteroid.NR")]
        private static EventData<ConfigNode> OnLoaderLoadAsteroidNR { get; }
        [Description("Loader.FinalizeBody.NR")]
        private static EventVoid OnLoaderFinalizeBodyNR { get; }

        [Description("Body.Apply")]
        public static EventData<Body, ConfigNode> OnBodyApply { get; }
        [Description("Body.PostApply")]
        public static EventData<Body, ConfigNode> OnBodyPostApply { get; }
        [Description("Body.GenerateScaledSpace")]
        public static EventData<Body, ConfigNode> OnBodyGenerateScaledSpace { get; }

        [Description("Body.Apply.NR")]
        private static EventData<ConfigNode> OnBodyApplyNR { get; }
        [Description("Body.PostApply.NR")]
        private static EventData<ConfigNode> OnBodyPostApplyNR { get; }
        [Description("Body.GenerateScaledSpace.NR")]
        private static EventData<ConfigNode> OnBodyGenerateScaledSpaceNR { get; }
        
        [Description("RuntimeUtility.PatchAFG")]
        public static EventData<AtmosphereFromGround> OnRuntimeUtilityPatchAFG { get; }
        [Description("RuntimeUtility.SpawnAsteroid")]
        public static EventData<Asteroid, ProtoVessel> OnRuntimeUtilitySpawnAsteroid { get; }
        [Description("RuntimeUtility.UpdateMenu")]
        public static EventVoid OnRuntimeUtilityUpdateMenu { get; }
        [Description("RuntimeUtility.PatchFI")]
        public static EventVoid OnRuntimeUtilityPatchFI { get; }
        [Description("RuntimeUtility.SwitchStar")]
        public static EventData<KopernicusStar> OnRuntimeUtilitySwitchStar { get; }

        [Description("RuntimeUtility.SpawnAsteroid.NR")]
        private static EventData<ProtoVessel> OnRuntimeUtilitySpawnAsteroidNR { get; }
        [Description("RuntimeUtility.SwitchStar.NR")]
        private static EventVoid OnRuntimeUtilitySwitchStarNR { get; }

        void Awake()
        {
            PropertyInfo[] events = typeof(Events).GetProperties(BindingFlags.Static | BindingFlags.Public);
            for (Int32 i = 0; i < events.Length; i++)
            {
                PropertyInfo info = events[i];
                DescriptionAttribute description = (info.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[])[0];
                events[i].SetValue(null, Activator.CreateInstance(events[i].PropertyType, new[] { "Kopernicus." + description.Description }), null);
            }
            RegisterNREvents();
            Destroy(this);
        }

        void RegisterNREvents()
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
            OnLoaderLoadAsteroid.Add((a, c) => OnLoaderLoadAsteroidNR.Fire(c));
            OnLoaderFinalizeBody.Add((a) => OnLoaderFinalizeBodyNR.Fire());
            OnBodyApply.Add((a, c) => OnBodyApplyNR.Fire(c));
            OnBodyPostApply.Add((a, c) => OnBodyPostApplyNR.Fire(c));
            OnBodyGenerateScaledSpace.Add((a, c) => OnBodyGenerateScaledSpaceNR.Fire(c));
            OnRuntimeUtilitySpawnAsteroid.Add((a, c) => OnRuntimeUtilitySpawnAsteroidNR.Fire(c));
            OnRuntimeUtilitySwitchStar.Add((a) => OnRuntimeUtilitySwitchStarNR.Fire());
        }
    }
}