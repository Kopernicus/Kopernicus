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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class VertexSimplexHeightMap : ModLoader<PQSMod_VertexSimplexHeightMap>
            {
                // The deformity of the simplex terrain
                [ParserTarget("deformity")]
                public NumericParser<Double> deformity
                {
                    get { return mod.deformity; }
                    set { mod.deformity = value; }
                }

                // The frequency of the simplex terrain
                [ParserTarget("frequency")]
                public NumericParser<Double> frequency
                {
                    get { return mod.frequency; }
                    set { mod.frequency = value; }
                }

                // Height end
                [ParserTarget("heightEnd")]
                public NumericParser<Single> heightEnd
                {
                    get { return mod.heightEnd; }
                    set { mod.heightEnd = value; }
                }

                // Height start
                [ParserTarget("heightStart")]
                public NumericParser<Single> heightStart
                {
                    get { return mod.heightStart; }
                    set { mod.heightStart = value; }
                }

                // The greyscale map texture used
                [ParserTarget("map")]
                public MapSOParser_GreyScale<MapSO> heightMap
                {
                    get { return mod.heightMap; }
                    set { mod.heightMap = value; }
                }

                // Octaves of the simplex terrain
                [ParserTarget("octaves")]
                public NumericParser<Double> octaves
                {
                    get { return mod.octaves; }
                    set { mod.octaves = value; }
                }

                // Persistence of the simplex terrain
                [ParserTarget("persistence")]
                public NumericParser<Double> persistence
                {
                    get { return mod.persistence; }
                    set { mod.persistence = value; }
                }

                // The seed of the simplex terrain
                [ParserTarget("seed")]
                public NumericParser<Int32> seed
                {
                    get { return mod.seed; }
                    set { mod.seed = value; }
                }
            }
        }
    }
}

