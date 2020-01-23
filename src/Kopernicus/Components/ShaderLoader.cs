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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;

namespace Kopernicus.Components
{
    public static class ShaderLoader
    {
        /// <summary>
        /// A collection of all shaders
        /// </summary>
        private static readonly Dictionary<String, Shader> ShaderDictionary = new Dictionary<String, Shader>();

        /// <summary>
        /// Requests a Shader
        /// </summary>
        public static Shader GetShader(String shaderName)
        {
            Debug.Log("[Kopernicus] ShaderLoader: GetShader " + shaderName);
            return ShaderDictionary.ContainsKey(shaderName) ? ShaderDictionary[shaderName] : null;
        }

        /// <summary>
        /// Manually load Shader Asset bundles.
        /// </summary>
        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        public static void LoadAssetBundle(String path, String bundleName)
        {
            // GameData
            path = Path.Combine(KSPUtil.ApplicationRootPath + "GameData/", path);

            // Pick correct bundle for platform
            if (Application.platform == RuntimePlatform.WindowsPlayer &&
                SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL"))
            {
                path = Path.Combine(path, bundleName + "-linux.unity3d"); // fixes OpenGL on windows
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                path = Path.Combine(path, bundleName + "-windows.unity3d");
            }
            else if (Application.platform == RuntimePlatform.LinuxPlayer)
            {
                path = Path.Combine(path, bundleName + "-linux.unity3d");
            }
            else
            {
                path = Path.Combine(path, bundleName + "-macosx.unity3d");
            }

            Debug.Log("[Kopernicus] ShaderLoader: Loading asset bundle at path " + path);

            AssetBundle bundle = AssetBundle.LoadFromFile(path);
            Shader[] shaders = bundle.LoadAllAssets<Shader>();

            foreach (Shader shader in shaders)
            {
                Debug.Log("[Kopernicus] ShaderLoader: adding " + shader.name);
                ShaderDictionary.Add(shader.name, shader);
            }

            bundle.Unload(false); // unload the raw asset bundle
        }
    }
}