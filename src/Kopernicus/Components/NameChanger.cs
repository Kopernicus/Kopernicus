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
using System.Linq;
using UnityEngine;

namespace Kopernicus.Components
{
    /// <summary>
    /// Component to change the displayed name of a body
    /// </summary>
    public class NameChanger : MonoBehaviour
    {
        // Variables
        public String oldName;
        public String newName;

        /// <summary>
        /// Apply the name changes
        /// </summary>
        public void Start()
        {
            foreach (CelestialBody b in FlightGlobals.Bodies.Where(b => b.bodyName == oldName))
            {
                OrbitRendererData data = PSystemManager.OrbitRendererDataCache[b];
                PSystemManager.OrbitRendererDataCache.Remove(b);
                // Before we make the change, we need to make sure FlightGlobals's bodyNames cache
                // has the old name (for back-compat with older mods)
                FlightGlobals.GetBodyByName(b.bodyName);
                // Now we can change the name safely.
                b.bodyName = newName;
                PlanetariumCamera.fetch.targets.Find(t => t.name == oldName).name = newName;
                data.cb = b;
                PSystemManager.OrbitRendererDataCache.Add(b, data);
                Events.OnApplyNameChange.Fire(this, b);
            }
        }
    }
}
