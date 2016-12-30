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
            static Dictionary<string, Shader> shaderDictionary = null;

            public static Shader GetShader(String shaderName)
            {
                Debug.Log("[Kopernicus] shader loader GetShader " + shaderName);

                if (shaderDictionary == null)
                {
                    LoadAssetBundle();
                }

                if (shaderDictionary.ContainsKey(shaderName))
                {
                    return shaderDictionary[shaderName];
                }

                return null;
            }

            public static void LoadAssetBundle()
            {
                //not the best way to do it but the KSP assetLoader is so messed up I'd rather do like this

                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                path = Path.GetDirectoryName(path);

                //remove "Plugins" from path
                int index = path.LastIndexOf("/Plugins");
                path = (index < 0) ? path : path.Remove(index, path.Length-index);

                //pick correct bundle for platform
                if (Application.platform == RuntimePlatform.WindowsPlayer && SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL"))
                    path = path + "/Shaders/kopernicusshaders-linux";   //fixes OpenGL on windows
                else if (Application.platform == RuntimePlatform.WindowsPlayer)
                    path = path + "/Shaders/kopernicusshaders-windows";
                else if (Application.platform == RuntimePlatform.LinuxPlayer)
                    path = path + "/Shaders/kopernicusshaders-linux";
                else
                    path = path + "/Shaders/kopernicusshaders-macosx";

                Debug.Log("[Kopernicus] shader loader path " + path);

                shaderDictionary = new Dictionary<string, Shader>();

                using (WWW www = new WWW("file://" + path))
                {
                    AssetBundle bundle = www.assetBundle;
                    Shader[] shaders = bundle.LoadAllAssets<Shader>();

                    foreach (Shader shader in shaders)
                    {
                        Debug.Log("[Kopernicus] shader loader adding " + shader.name);
                        shaderDictionary.Add(shader.name, shader);
                    }

                    bundle.Unload(false); // unload the raw asset bundle
                    www.Dispose();
                }
            }
        }
    }
}