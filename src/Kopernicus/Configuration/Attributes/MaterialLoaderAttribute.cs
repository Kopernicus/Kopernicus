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

namespace Kopernicus.Configuration.Attributes;

/// <summary>
/// Use this attribute to indicate that your class is a material loader for
/// <paramref name="shader"/>.
/// </summary>
/// <param name="shader">the name of a shader that this loader can be used for.</param>
/// 
/// <remarks>
/// Any type using this attribute must have either a constructor that takes a
/// <see cref="Material" />, or a two-argument constructor that takes both a
/// <see cref="Material" /> and a <see cref="string" />. The string will contain
/// the name of the shader that is being loaded.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class MaterialLoaderAttribute(string shader) : Attribute
{
    public string Shader { get; } = shader;
}
