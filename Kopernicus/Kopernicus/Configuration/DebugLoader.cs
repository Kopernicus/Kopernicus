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
 
namespace Kopernicus
{
    namespace Configuration
    {
        // Loads Debugging properties for a Body
        [RequireConfigType(ConfigType.Node)]
        public class DebugLoader
        {
            // If this is set to false, Kopernicus wont save a .bin file with the scaledSpace mesh 
            [ParserTarget("exportMesh")]
            public NumericParser<bool> exportMesh = new NumericParser<bool>(true);

            // If this is set to true, Kopernicus will update the ScaledSpace mesh, even if the original conditions aren't matched
            [ParserTarget("update")]
            public NumericParser<bool> update = new NumericParser<bool>(false);

            // If this is set to true, a wireframe will appear to visualize the SOI
            [ParserTarget("showSOI")]
            public NumericParser<bool> showSOI = new NumericParser<bool>(false);
        }
    }
}

