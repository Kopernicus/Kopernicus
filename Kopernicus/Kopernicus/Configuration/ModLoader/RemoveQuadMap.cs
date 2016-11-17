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
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class RemoveQuadMap : ModLoader<PQSMod_RemoveQuadMap>
            {
                // The map texture for the Quad Remover (?)
                [ParserTarget("map")]
                public MapSOParser_GreyScale<MapSO> map
                {
                    get { return mod.map; }
                    set { mod.map = value; }
                }

                // The deformity of the map for the Quad Remover (?)
                [ParserTarget("deformity")]
                public NumericParser<float> mapDeformity
                {
                    get { return mod.mapDeformity; }
                    set { mod.mapDeformity = value; }
                }

                // The max. height for the Quad Remover (?)
                [ParserTarget("maxHeight")]
                public NumericParser<float> maxHeight
                {
                    get { return mod.maxHeight; }
                    set { mod.maxHeight = value; }
                }

                // The min texture for the Quad Remover (?)
                [ParserTarget("minHeight")]
                public NumericParser<float> minHeight 
                {
                    get { return mod.minHeight; }
                    set { mod.minHeight = value; }
                }
            }
        }
    }
}

