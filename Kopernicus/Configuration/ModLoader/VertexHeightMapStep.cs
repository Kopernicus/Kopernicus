/**
 * Kopernicus Planetary System Modifier
 * Copyright (C) 2014 Bryce C Schroeder (bryce.schroeder@gmail.com), Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * http://www.ferazelhosting.net/~bryce/contact.html
 * 
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.IO;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class VertexHeightMapStep : ModLoader<PQSMod_VertexHeightMapStep>
            {
                // The map texture for the planet
                [ParserTarget("map", optional = false)]
                public Texture2DParser heightMap
                {
                    get { return mod.heightMap; }
                    set { mod.heightMap = value; }
                }

                // Height map offset
                [ParserTarget("offset", optional = true)]
                public NumericParser<double> heightMapOffset 
                {
                    get { return mod.heightMapOffset; }
                    set { mod.heightMapOffset = value; }
                }

                // Height map offset
                [ParserTarget("deformity", optional = true)]
                public NumericParser<double> heightMapDeformity
                {
                    get { return mod.heightMapDeformity; }
                    set { mod.heightMapDeformity = value; }
                }

                // Height map offset
                [ParserTarget("scaleDeformityByRadius", optional = true)]
                public NumericParser<bool> scaleDeformityByRadius
                {
                    get { return mod.scaleDeformityByRadius; }
                    set { mod.scaleDeformityByRadius = value; }
                }

                [ParserTarget("coastHeight", optional = true)]
                public NumericParser<double> coastHeight
                {
                    get { return mod.coastHeight; }
                    set { mod.coastHeight = value; }
                }
            }
        }
    }
}

