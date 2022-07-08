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

using Kopernicus.Components.ModularComponentSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Kopernicus.Components.ModularScatter
{
    /// <summary>
    /// Moves the scatter objects to the sea level
    /// </summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class SeaLevelScatterComponent : IComponent<ModularScatter>
    {
        /// <summary>
        /// Scatters will be moved up/down by a random value from this range
        /// </summary>
        public List<Single> AltitudeVariance = new List<Single> {0f, 0f};

        public void Apply(ModularScatter system) => throw new NotImplementedException();

        public void PostApply(ModularScatter system) => throw new NotImplementedException();

        public void Update(ModularScatter system) => throw new NotImplementedException();
    }
}
