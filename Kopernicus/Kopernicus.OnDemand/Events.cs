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
 
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace OnDemand
    {
        /// <summary>
        /// Utility methods for creating custom game events
        /// </summary>
        [KSPAddon(KSPAddon.Startup.Instantly, true)]
        public class Events : MonoBehaviour
        {
            [Description("OnDemand.MapSO.Load")]
            public static EventData<MapSODemand> OnMapSOLoad { get; }
            [Description("OnDemand.MapSO.Unload")]
            public static EventData<MapSODemand> OnMapSOUnload { get; }
            
            [Description("OnDemand.CBMapSO.Load")]
            public static EventData<CBAttributeMapSODemand> OnCBMapSOLoad { get; }
            [Description("OnDemand.CBMapSO.Unload")]
            public static EventData<CBAttributeMapSODemand> OnCBMapSOUnload { get; }

            [Description("OnDemand.ScaledSpace.Load")]
            public static EventData<ScaledSpaceDemand> OnScaledSpaceLoad { get; }
            [Description("OnDemand.ScaledSpace.Unload")]
            public static EventData<ScaledSpaceDemand> OnScaledSpaceUnload { get; }

            [Description("OnDemand.Body.Load")]
            public static EventData<CelestialBody> OnBodyLoad { get; }
            [Description("OnDemand.Body.Unload")]
            public static EventData<CelestialBody> OnBodyUnload { get; }
            
            [Description("OnDemand.MapSO.Load.NR")]
            private static EventVoid OnMapSOLoadNR { get; }
            [Description("OnDemand.MapSO.Unload.NR")]
            private static EventVoid OnMapSOUnloadNR { get; }

            [Description("OnDemand.CBMapSO.Load.NR")]
            private static EventVoid OnCBMapSOLoadNR { get; }
            [Description("OnDemand.CBMapSO.Unload.NR")]
            private static EventVoid OnCBMapSOUnloadNR { get; }

            [Description("OnDemand.ScaledSpace.Load.NR")]
            private static EventVoid OnScaledSpaceLoadNR { get; }
            [Description("OnDemand.ScaledSpace.Unload.NR")]
            private static EventVoid OnScaledSpaceUnloadNR { get; }

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
                OnMapSOLoad.Add((a) => OnMapSOLoadNR.Fire());
                OnMapSOUnload.Add((a) => OnMapSOUnloadNR.Fire());
                OnCBMapSOLoad.Add((a) => OnCBMapSOLoadNR.Fire());
                OnCBMapSOUnload.Add((a) => OnCBMapSOUnloadNR.Fire());
                OnScaledSpaceLoad.Add((a) => OnScaledSpaceLoadNR.Fire());
                OnScaledSpaceUnload.Add((a) => OnScaledSpaceUnloadNR.Fire());
            }
        }
    }
}