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
            public class VertexSimplexMultiChromatic : ModLoader<PQSMod_VertexSimplexMultiChromatic>
            {
                // The frequency of the alpha noise
                [ParserTarget("alphaFrequency")]
                public NumericParser<double> alphaFrequency
                {
                    get { return mod.alphaFrequency; }
                    set { mod.alphaFrequency = value; }
                }

                // Octaves of the alpha noise
                [ParserTarget("alphaOctaves")]
                public NumericParser<double> alphaOctaves
                {
                    get { return mod.alphaOctaves; }
                    set { mod.alphaOctaves = value; }
                }

                // Persistence of the alpha noise
                [ParserTarget("alphaPersistence")]
                public NumericParser<double> alphaPersistence
                {
                    get { return mod.alphaPersistence; }
                    set { mod.alphaPersistence = value; }
                }

                // The seed of the alpha noise
                [ParserTarget("alphaSeed")]
                public NumericParser<int> alphaSeed
                {
                    get { return mod.alphaSeed; }
                    set { mod.alphaSeed = value; }
                }

                // Amount of color that will be applied
                [ParserTarget("blend")]
                public NumericParser<float> blend
                {
                    get { return mod.blend; }
                    set { mod.blend = value; }
                }

                // The frequency of the blue noise
                [ParserTarget("blueFrequency")]
                public NumericParser<double> blueFrequency
                {
                    get { return mod.blueFrequency; }
                    set { mod.blueFrequency = value; }
                }

                // Octaves of the blue noise
                [ParserTarget("blueOctaves")]
                public NumericParser<double> blueOctaves
                {
                    get { return mod.blueOctaves; }
                    set { mod.blueOctaves = value; }
                }

                // Persistence of the blue noise
                [ParserTarget("bluePersistence")]
                public NumericParser<double> bluePersistence
                {
                    get { return mod.bluePersistence; }
                    set { mod.bluePersistence = value; }
                }

                // The seed of the blue noise
                [ParserTarget("blueSeed")]
                public NumericParser<int> blueSeed
                {
                    get { return mod.blueSeed; }
                    set { mod.blueSeed = value; }
                }

                // The frequency of the green noise
                [ParserTarget("greenFrequency")]
                public NumericParser<double> greenFrequency
                {
                    get { return mod.greenFrequency; }
                    set { mod.greenFrequency = value; }
                }

                // Octaves of the green noise
                [ParserTarget("greenOctaves")]
                public NumericParser<double> greenOctaves
                {
                    get { return mod.greenOctaves; }
                    set { mod.greenOctaves = value; }
                }

                // Persistence of the green noise
                [ParserTarget("greenPersistence")]
                public NumericParser<double> greenPersistence
                {
                    get { return mod.greenPersistence; }
                    set { mod.greenPersistence = value; }
                }

                // The seed of the green noise
                [ParserTarget("greenSeed")]
                public NumericParser<int> greenSeed
                {
                    get { return mod.greenSeed; }
                    set { mod.greenSeed = value; }
                }

                // The frequency of the red noise
                [ParserTarget("redFrequency")]
                public NumericParser<double> redFrequency
                {
                    get { return mod.redFrequency; }
                    set { mod.redFrequency = value; }
                }

                // Octaves of the red noise
                [ParserTarget("redOctaves")]
                public NumericParser<double> redOctaves
                {
                    get { return mod.redOctaves; }
                    set { mod.redOctaves = value; }
                }

                // Persistence of the red noise
                [ParserTarget("redPersistence")]
                public NumericParser<double> redPersistence
                {
                    get { return mod.redPersistence; }
                    set { mod.redPersistence = value; }
                }

                // The seed of the red noise
                [ParserTarget("redSeed")]
                public NumericParser<int> redSeed
                {
                    get { return mod.redSeed; }
                    set { mod.redSeed = value; }
                }
            }
        }
    }
}

