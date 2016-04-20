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

using System.Collections;
using UnityEngine;

namespace Kopernicus
{
    namespace OnDemand
    {
        // Class to load ScaledSpace Textures on Demand
        public class ScaledSpaceDemand : MonoBehaviour
        {
            // Path to the Texture
            public string texture;

            // Path to the normal map
            public string normals;

            // ScaledSpace MeshRenderer
            public MeshRenderer scaledRenderer;

            // State of the texture
            public bool isLoaded = true;

            // Start(), get the scaled Mesh renderer
            void Start()
            {
                scaledRenderer = GetComponent<MeshRenderer>();
                OnBecameInvisible();
            }

            // OnBecameVisible(), load the texture
            void OnBecameVisible()
            {
                // If it is already loaded, return
                if (isLoaded)
                    return;

                // Load Diffuse
                if (OnDemandStorage.TextureExists(texture))
                    scaledRenderer.material.SetTexture("_MainTex", OnDemandStorage.LoadTexture(texture, false, true, true));

                // Load Normals
                if (OnDemandStorage.TextureExists(normals))
                    scaledRenderer.material.SetTexture("_BumpMap", OnDemandStorage.LoadTexture(normals, false, true, false));

                // Flags
                isLoaded = true;
            }

            // OnBecameInvisible(), kill the texture
            void OnBecameInvisible()
            {
                // If it is already loaded, return
                if (!isLoaded)
                    return;

                // Kill Diffuse
                if (OnDemandStorage.TextureExists(texture))
                    DestroyImmediate(scaledRenderer.material.GetTexture("_MainTex"));

                // Kill Normals
                if (OnDemandStorage.TextureExists(normals))
                    DestroyImmediate(scaledRenderer.material.GetTexture("_BumpMap"));

                // Flags
                isLoaded = false;
            }
        }
    }
}