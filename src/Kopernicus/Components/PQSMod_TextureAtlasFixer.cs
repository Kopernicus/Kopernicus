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
* which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
* itself is governed by the terms of its EULA, not the license above.
*
* https://kerbalspaceprogram.com
*/

using System;
using System.Diagnostics.CodeAnalysis;

namespace Kopernicus.Components
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PQSMod_TextureAtlasFixer : PQSMod
    {
        private PQSMod_TextureAtlas[] _mods;

        public override void OnSetup()
        {
            _mods = sphere.GetComponentsInChildren<PQSMod_TextureAtlas>(true);
        }

        public override void OnQuadPreBuild(PQ quad)
        {
            for (Int32 i = 0; i < _mods.Length; i++)
            {
                PQSMod_TextureAtlas mod = _mods[i];

                mod.material1Blend.CopyPropertiesFromMaterial(sphere.surfaceMaterial);
                mod.material2Blend.CopyPropertiesFromMaterial(sphere.surfaceMaterial);
                mod.material3Blend.CopyPropertiesFromMaterial(sphere.surfaceMaterial);
                mod.material4Blend.CopyPropertiesFromMaterial(sphere.surfaceMaterial);
            }
        }
    }
}