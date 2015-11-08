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
 
namespace Kopernicus
{
    namespace Configuration
    {
        namespace ModLoader
        {
            [RequireConfigType(ConfigType.Node)]
            public class AerialPerspectiveMaterial : ModLoader<PQSMod_AerialPerspectiveMaterial>
            {
                // atmosphereDepth
                [ParserTarget("atmosphereDepth", optional = true)]
                public NumericParser<float> deformity
                {
                    get { return mod.atmosphereDepth; }
                    set { mod.atmosphereDepth = value; }
                }

                // The altitude of the camera
                [ParserTarget("cameraAlt", optional = true)]
                public NumericParser<double> cameraAlt
                {
                    get { return mod.cameraAlt; }
                    set { mod.cameraAlt = value; }
                }

                // Athmospheric altitude of the camera.
                [ParserTarget("cameraAtmosAlt", optional = true)]
                public NumericParser<float> cameraAtmosAlt
                {
                    get { return mod.cameraAtmosAlt; }
                    set { mod.cameraAtmosAlt = value; }
                }

                // DEBUG_SetEveryFrame
                [ParserTarget("DEBUG_SetEveryFrame", optional = true)]
                public NumericParser<bool> DEBUG_SetEveryFrame
                {
                    get { return mod.DEBUG_SetEveryFrame; }
                    set { mod.DEBUG_SetEveryFrame = value; }
                }

                // Global density of the material
                [ParserTarget("globalDensity", optional = true)]
                public NumericParser<float> globalDensity
                {
                    get { return mod.globalDensity; }
                    set { mod.globalDensity = value; }
                }

                // heightDensAtViewer
                [ParserTarget("heightDensAtViewer", optional = true)]
                public NumericParser<float> heightDensAtViewer
                {
                    get { return mod.heightDensAtViewer; }
                    set { mod.heightDensAtViewer = value; }
                }

                // heightFalloff
                [ParserTarget("heightFalloff", optional = true)]
                public NumericParser<float> heightFalloff
                {
                    get { return mod.heightFalloff; }
                    set { mod.heightFalloff = value; }
                }
            }
        }
    }
}

