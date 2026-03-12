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

using System.Collections.Generic;
using KSPTextureLoader;
using UnityEngine;

namespace Kopernicus.OnDemand;

/// <summary>
/// A component attached to the __deactivator GameObject that keeps texture handles
/// alive without resorting to GCHandle leaks. Handles stored here are retained for
/// the lifetime of the game so that KSPTextureLoader's internal cache stays valid.
/// </summary>
internal class TextureHandleStorage : MonoBehaviour
{
    private static TextureHandleStorage _instance;

    private readonly List<TextureHandle<Texture2D>> textures = [];
    private readonly List<CPUTextureHandle> cpuTextures = [];

    public static TextureHandleStorage Instance
    {
        get
        {
            if (!_instance.IsNullOrDestroyed())
                return _instance;

            return _instance = Utility.Deactivator.gameObject.AddComponent<TextureHandleStorage>();
        }
    }

    public void Store(TextureHandle<Texture2D> handle)
    {
        textures.Add(handle);
    }

    public void Store(CPUTextureHandle handle)
    {
        cpuTextures.Add(handle);
    }
}
