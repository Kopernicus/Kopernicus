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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
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
    namespace Components
    {
        /// <summary>
        /// Utility methods for creating custom game events
        /// </summary>
        [KSPAddon(KSPAddon.Startup.Instantly, true)]
        public class Events : MonoBehaviour
        {
            [Description("Components.SwitchKSC")]
            public static EventData<KSC> OnSwitchKSC { get; private set; }
            [Description("Components.ApplyNameChange")]
            public static EventData<NameChanger, CelestialBody> OnApplyNameChange { get; private set; }
            [Description("Components.ParticleCollision")]
            public static EventData<PlanetParticleEmitter, GameObject> OnParticleCollision { get; private set; }

            [Description("Components.SwitchKSC.NR")]
            private static EventVoid OnSwitchKSCNR { get; set; }
            [Description("Components.ApplyNameChange.NR")]
            private static EventData<CelestialBody> OnApplyNameChangeNR { get; set; }
            [Description("Components.ParticleCollision.NR")]
            private static EventData<GameObject> OnParticleCollisionNR { get; set; }

            void Awake()
            {
                PropertyInfo[] events = typeof(Events).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
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
                OnSwitchKSC.Add((a) => OnSwitchKSCNR.Fire());
                OnApplyNameChange.Add((a, c) => OnApplyNameChangeNR.Fire(c));
                OnParticleCollision.Add((a, c) => OnParticleCollisionNR.Fire(c));
            }
        }
    }
}