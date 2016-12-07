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
            public class VertexSimplexColorRGB : ModLoader<PQSMod_VertexSimplexColorRGB>
            {
                // blendColor
                [ParserTarget("blendColor")]
                public ColorParser blendColor
                {
                    get { return new Color(mod.rBlend, mod.gBlend, mod.bBlend, 1); }
                    set 
                    {
                        mod.bBlend = value.value.b;
                        mod.rBlend = value.value.r;
                        mod.gBlend = value.value.g;
                    }
                }

                // blend
                [ParserTarget("blend")]
                public NumericParser<float> blend
                {
                    get { return mod.blend; }
                    set { mod.blend = value; }
                }

                // frequency
                [ParserTarget("frequency")]
                public NumericParser<double> frequency
                {
                    get { return mod.frequency; }
                    set { mod.frequency = value; }
                }

                // octaves
                [ParserTarget("octaves")]
                public NumericParser<double> octaves
                {
                    get { return mod.octaves; }
                    set { mod.octaves = value; }
                }

                // persistence
                [ParserTarget("persistence")]
                public NumericParser<double> persistence
                {
                    get { return mod.persistence; }
                    set { mod.persistence = value; }
                }

                // seed
                [ParserTarget("seed")]
                public NumericParser<int> seed
                {
                    get { return mod.seed; }
                    set { mod.seed = value; }
                }
            }
        }
    }
}

