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

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus.Components
{
    // Wrapper for the OrbitRendererData class to make it a bit easier to use
    public class KopernicusOrbitRendererData : OrbitRendererData
    {
        // Accessor for the nodeColor field
        private static readonly FieldInfo NodeColor = typeof(OrbitRendererData)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault();

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public Color nodeColor
        {
            get { return (Color) NodeColor.GetValue(this); }
            set { NodeColor.SetValue(this, value); }
        }

        public KopernicusOrbitRendererData(CelestialBody body, OrbitRendererBase renderer) : base(body)
        {
            orbitColor = renderer.orbitColor;
            nodeColor = renderer.nodeColor;
            lowerCamVsSmaRatio = renderer.lowerCamVsSmaRatio;
            upperCamVsSmaRatio = renderer.upperCamVsSmaRatio;
        }

        public KopernicusOrbitRendererData(CelestialBody body, OrbitRendererData data) : base(body)
        {
            orbitColor = data.orbitColor;
            nodeColor = (Color) NodeColor.GetValue(data);
            lowerCamVsSmaRatio = data.lowerCamVsSmaRatio;
            upperCamVsSmaRatio = data.upperCamVsSmaRatio;
        }
    }
}