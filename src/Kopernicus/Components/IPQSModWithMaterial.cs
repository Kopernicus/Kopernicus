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

using UnityEngine;

namespace Kopernicus.Components
{
    /// <summary>
    /// Implement this to allow custom material loaders to set up your material
    /// when by calling <see cref="BaseMaterialLoader.OnParentApply" />.
    /// </summary>
    public interface IPQSModWithMaterial
    {
        /// <summary>
        /// The material that will be configured.
        /// </summary>
        Material Material { get; set; }

        /// <summary>
        /// The host <see cref="GameObject"/> that on-demand texture listener
        /// components will be attached to. Implicitly satisfied by any
        /// <see cref="MonoBehaviour"/>.
        /// </summary>
        GameObject gameObject { get; }
    }
}
