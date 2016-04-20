/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
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
            public class VertexVoronoi : ModLoader<PQSMod_VertexVoronoi>
            { 
                // Deformation of the Voronoi
                [ParserTarget("deformation", optional = true)]
                public NumericParser<double> deformation
                {
                    get { return mod.deformation; }
                    set { mod.deformation = value; }
                }

                // Displacement of the Voronoi
                [ParserTarget("displacement", optional = true)]
                public NumericParser<double> voronoiDisplacement
                {
                    get { return mod.voronoiDisplacement; }
                    set { mod.voronoiDisplacement = value; }
                }

                // Enabled distance of the Voronoi
                [ParserTarget("enableDistance", optional = true)]
                public NumericParser<bool> voronoiEnableDistance
                {
                    get { return mod.voronoiEnableDistance; }
                    set { mod.voronoiEnableDistance = value; }
                }

                // Frequency of the Voronoi
                [ParserTarget("frequency", optional = true)]
                public NumericParser<double> frequency
                {
                    get { return mod.voronoiFrequency; }
                    set { mod.voronoiFrequency = value; }
                }

                // Seed of the Voronoi
                [ParserTarget("seed", optional = true)]
                public NumericParser<int> seed
                {
                    get { return mod.voronoiSeed; }
                    set { mod.voronoiSeed = value; }
                }
            }
        }
    }
}

