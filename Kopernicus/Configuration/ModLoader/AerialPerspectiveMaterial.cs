/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
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
            public class AerialPerspectiveMaterial : ModLoader, IParserEventSubscriber
            {
                // Actual PQS mod we are loading
                private PQSMod_AerialPerspectiveMaterial _mod;

                // atmosphereDepth
                [ParserTarget("atmosphereDepth", optional = true)]
                private NumericParser<float> deformity
                {
                    set { _mod.atmosphereDepth = value.value; }
                }

                // The altitude of the camera
                [ParserTarget("cameraAlt", optional = true)]
                private NumericParser<double> cameraAlt
                {
                    set { _mod.cameraAlt = value.value; }
                }

                // Athmospheric altitude of the camera.
                [ParserTarget("cameraAtmosAlt", optional = true)]
                private NumericParser<float> cameraAtmosAlt
                {
                    set { _mod.cameraAtmosAlt = value.value; }
                }

                // DEBUG_SetEveryFrame
                [ParserTarget("DEBUG_SetEveryFrame", optional = true)]
                private NumericParser<bool> DEBUG_SetEveryFrame
                {
                    set { _mod.DEBUG_SetEveryFrame = value.value; }
                }

                // Global density of the material
                [ParserTarget("globalDensity", optional = true)]
                private NumericParser<float> globalDensity
                {
                    set { _mod.globalDensity = value.value; }
                }

                // heightDensAtViewer
                [ParserTarget("heightDensAtViewer", optional = true)]
                private NumericParser<float> heightDensAtViewer
                {
                    set { _mod.heightDensAtViewer = value.value; }
                }

                // heightFalloff
                [ParserTarget("heightFalloff", optional = true)]
                private NumericParser<float> heightFalloff
                {
                    set { _mod.heightFalloff = value.value; }
                }

                void IParserEventSubscriber.Apply(ConfigNode node)
                {

                }

                void IParserEventSubscriber.PostApply(ConfigNode node)
                {

                }

                public AerialPerspectiveMaterial()
                {
                    // Create the base mod
                    GameObject modObject = new GameObject("AerialPerspectiveMaterial");
                    modObject.transform.parent = Utility.Deactivator;
                    _mod = modObject.AddComponent<PQSMod_AerialPerspectiveMaterial>();
                    base.mod = _mod;
                }

                public AerialPerspectiveMaterial(PQSMod template)
                {
                    _mod = template as PQSMod_AerialPerspectiveMaterial;
                    _mod.transform.parent = Utility.Deactivator;
                    base.mod = _mod;
                }
            }
        }
    }
}

