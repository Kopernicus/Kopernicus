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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
        public class ShaderLoader
        {
            /// <summary>
            /// A collection of all shaders
            /// </summary>
            private static Dictionary<string, Shader> shaderDictionary = new Dictionary<string, Shader>();

            /// <summary>
            /// Requests a Shader
            /// </summary>
            /// <param name="shaderName"></param>
            /// <returns></returns>
            public static Shader GetShader(String shaderName)
            {
                Debug.Log("[Kopernicus] ShaderLoader: GetShader " + shaderName);

                if (shaderDictionary.Count == 0)
                {
                    LoadAssetBundle("Kopernicus/Shaders", "kopernicusshaders");
                }

                if (shaderDictionary.ContainsKey(shaderName))
                {
                    return shaderDictionary[shaderName];
                }

                return null;
            }

            /// <summary>
            /// Manually load Shader Asset bundles.
            /// </summary>
            public static void LoadAssetBundle(String path, String bundleName)
            {
                // GameData
                path = Path.Combine(KSPUtil.ApplicationRootPath + "GameData/", path);

                // Pick correct bundle for platform
                if (Application.platform == RuntimePlatform.WindowsPlayer && SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL"))
                    path = Path.Combine(path, bundleName + "-linux.unity3d");   // fixes OpenGL on windows
                else if (Application.platform == RuntimePlatform.WindowsPlayer)
                    path = Path.Combine(path, bundleName + "-windows.unity3d"); 
                else if (Application.platform == RuntimePlatform.LinuxPlayer)
                    path = Path.Combine(path, bundleName + "-linux.unity3d"); 
                else
                    path = Path.Combine(path, bundleName + "-macosx.unity3d");

                Debug.Log("[Kopernicus] ShaderLoader: Loading asset bundle at path " + path);

                using (WWW www = new WWW("file://" + path))
                {
                    AssetBundle bundle = www.assetBundle;
                    Shader[] shaders = bundle.LoadAllAssets<Shader>();

                    foreach (Shader shader in shaders)
                    {
                        Debug.Log("[Kopernicus] ShaderLoader: adding " + shader.name);
                        shaderDictionary.Add(shader.name, shader);
                    }

                    bundle.Unload(false); // unload the raw asset bundle
                }
            }
        }
    }
}