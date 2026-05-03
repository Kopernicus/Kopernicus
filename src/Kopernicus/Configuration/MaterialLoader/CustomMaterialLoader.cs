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
using System.Collections.Generic;
using System.Linq;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.MaterialLoader.Parsing;
using Kopernicus.Configuration.Parsing;
using Kopernicus.OnDemand;
using KSPTextureLoader;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kopernicus.Configuration.MaterialLoader;

[RequireConfigType(ConfigType.Node)]
public class CustomMaterialLoader : BaseMaterialLoader
{
    /// <summary>
    /// The shader that will actually be loaded.
    /// </summary>
    [PreApply]
    [ParserTarget("shader")]
    public override ShaderParser ShaderParser { get; set; }

    /// <summary>
    /// Whether this specific material should use on-demand textures. Defaults
    /// to the global config but can be overridden if needed.
    /// </summary>
    [ParserTarget("onDemand")]
    public override NumericParser<bool> OnDemand { get; set; } = OnDemandStorage.UseOnDemand;

    public override void PostApply(ConfigNode node)
    {
        base.PostApply(node);

        foreach (var keyword in node.GetValues("keyword"))
            SetKeyword(keyword, true);
    }
}
