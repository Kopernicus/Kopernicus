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
using UnityEngine;

namespace Kopernicus.OnDemand;

public interface IOnDemandTextureListener
{
    /// <summary>
    /// The shader that this texture will be loaded for, if any. This will be
    /// used to choose the type of texture loaded. If null then the texture
    /// loader will load it as a generic <see cref="Texture" /> which may not
    /// necessarily match up with what the shader expects.
    /// </summary>
    Shader Shader { get; }

    /// <summary>
    /// Whether the texture associated with this listener needs to be loaded
    /// immediately.
    /// </summary>
    bool Immediate { get; }

    /// <summary>
    /// This gets called once the texture is loaded with the new texture.
    /// </summary>
    void OnTextureLoaded(string property, Texture texture);
}
