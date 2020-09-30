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
using System.Reflection;
using Kopernicus.Components.MaterialWrapper;
using UnityEngine;

namespace Kopernicus.Components
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PQSMod_TextureAtlasFixer : PQSMod
    {
        private PQSMod_TextureAtlas[] _mods;

        private static readonly Int32 NormalTex = Shader.PropertyToID("_NormalTex");
        private static readonly Int32 AtlasTex = Shader.PropertyToID("_AtlasTex");

        public override void OnSetup()
        {
            _mods = sphere.GetComponentsInChildren<PQSMod_TextureAtlas>(true);

            if (PQSTriplanarZoomRotationTextureArray.UsesSameShader(sphere.surfaceMaterial))
            {
                return;
            }
            // Make the TextureAtlas mods useless by removing the atlas map
            for (Int32 i = 0; i < _mods.Length; i++)
            {
                _mods[i].textureAtlasMap = null;
            }
        }

        public override void OnQuadPreBuild(PQ quad)
        {
            if (!PQSTriplanarZoomRotationTextureArray.UsesSameShader(sphere.surfaceMaterial))
            {
                return;
            }

            for (Int32 i = 0; i < _mods.Length; i++)
            {
                PQSMod_TextureAtlas mod = _mods[i];

                Texture2DArray atlas = (Texture2DArray) sphere.surfaceMaterial.GetTexture(AtlasTex);

                if (atlas == null)
                {
                    atlas = (Texture2DArray) mod.material1Blend.GetTexture(AtlasTex);
                }

                Texture2DArray normal = (Texture2DArray) sphere.surfaceMaterial.GetTexture(NormalTex);

                if (normal == null)
                {
                    normal = (Texture2DArray) mod.material1Blend.GetTexture(NormalTex);
                }

                mod.material1Blend.CopyPropertiesFromMaterial(sphere.surfaceMaterial);
                mod.material2Blend.CopyPropertiesFromMaterial(sphere.surfaceMaterial);
                mod.material3Blend.CopyPropertiesFromMaterial(sphere.surfaceMaterial);
                mod.material4Blend.CopyPropertiesFromMaterial(sphere.surfaceMaterial);

                sphere.surfaceMaterial.SetTexture(AtlasTex, atlas);
                mod.material1Blend.SetTexture(AtlasTex, atlas);
                mod.material2Blend.SetTexture(AtlasTex, atlas);
                mod.material3Blend.SetTexture(AtlasTex, atlas);
                mod.material4Blend.SetTexture(AtlasTex, atlas);

                sphere.surfaceMaterial.SetTexture(NormalTex, normal);
                mod.material1Blend.SetTexture(NormalTex, normal);
                mod.material2Blend.SetTexture(NormalTex, normal);
                mod.material3Blend.SetTexture(NormalTex, normal);
                mod.material4Blend.SetTexture(NormalTex, normal);
            }
        }
    }
}
