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
            public class VertexVoronoi : ModLoader<PQSMod_VertexVoronoi>
            { 
                // Deformation of the Voronoi
                [ParserTarget("deformation")]
                public NumericParser<Double> deformation
                {
                    get { return mod.deformation; }
                    set { mod.deformation = value; }
                }

                // Displacement of the Voronoi
                [ParserTarget("displacement")]
                public NumericParser<Double> voronoiDisplacement
                {
                    get { return mod.voronoiDisplacement; }
                    set { mod.voronoiDisplacement = value; }
                }

                // Enabled distance of the Voronoi
                [ParserTarget("enableDistance")]
                public NumericParser<Boolean> voronoiEnableDistance
                {
                    get { return mod.voronoiEnableDistance; }
                    set { mod.voronoiEnableDistance = value; }
                }

                // Frequency of the Voronoi
                [ParserTarget("frequency")]
                public NumericParser<Double> frequency
                {
                    get { return mod.voronoiFrequency; }
                    set { mod.voronoiFrequency = value; }
                }

                // Seed of the Voronoi
                [ParserTarget("seed")]
                public NumericParser<Int32> seed
                {
                    get { return mod.voronoiSeed; }
                    set { mod.voronoiSeed = value; }
                }
            }
        }
    }
}

