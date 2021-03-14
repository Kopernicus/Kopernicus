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
#if (!KSP_VERSION_1_8)
using System.Diagnostics.CodeAnalysis;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Configuration.Parsing;
using UnityEngine;

namespace Kopernicus.Configuration.ModLoader
{
    [RequireConfigType(ConfigType.Node)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TextureAtlas : ModLoader<PQSMod_TextureAtlas>
    {
        // The map texture for the planet
        [ParserTarget("map")]
        public MapSOParserRGB<CBTextureAtlasSO> Map
        {
            get { return Mod.textureAtlasMap; }
            set { Mod.textureAtlasMap = value; }
        }

        public override void Create(PQS pqsVersion)
        {
            base.Create(pqsVersion);

            Shader blend1 = Shader.Find("Terrain/PQS/PQS Triplanar Zoom Rotation Texture Array - 1 Blend");
            Shader blend2 = Shader.Find("Terrain/PQS/PQS Triplanar Zoom Rotation Texture Array - 2 Blend");
            Shader blend3 = Shader.Find("Terrain/PQS/PQS Triplanar Zoom Rotation Texture Array - 3 Blend");
            Shader blend4 = Shader.Find("Terrain/PQS/PQS Triplanar Zoom Rotation Texture Array - 4 Blend");

            Mod.material1Blend = new Material(blend1);
            Mod.material2Blend = new Material(blend2);
            Mod.material3Blend = new Material(blend3);
            Mod.material4Blend = new Material(blend4);
        }

        public override void Create(PQSMod_TextureAtlas mod, PQS pqsVersion)
        {
            base.Create(mod, pqsVersion);

            Shader blend1 = Shader.Find("Terrain/PQS/PQS Triplanar Zoom Rotation Texture Array - 1 Blend");
            Shader blend2 = Shader.Find("Terrain/PQS/PQS Triplanar Zoom Rotation Texture Array - 2 Blend");
            Shader blend3 = Shader.Find("Terrain/PQS/PQS Triplanar Zoom Rotation Texture Array - 3 Blend");
            Shader blend4 = Shader.Find("Terrain/PQS/PQS Triplanar Zoom Rotation Texture Array - 4 Blend");

            if (Mod.material1Blend == null)
            {
                Mod.material1Blend = new Material(blend1);
            }

            if (Mod.material2Blend == null)
            {
                Mod.material2Blend = new Material(blend2);
            }

            if (Mod.material3Blend == null)
            {
                Mod.material3Blend = new Material(blend3);
            }

            if (Mod.material4Blend == null)
            {
                Mod.material4Blend = new Material(blend4);
            }
        }
    }
}
#endif