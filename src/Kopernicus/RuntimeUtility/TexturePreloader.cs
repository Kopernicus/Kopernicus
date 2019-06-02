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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.Constants;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kopernicus.RuntimeUtility
{
    public class TexturePreloader : LoadingSystem
    {
        public static readonly Dictionary<String, Texture2D> Textures = new Dictionary<String, Texture2D>();
        
        private Boolean _ready;
        private String _progressTitle = "Kopernicus: Loading Textures";
        
        public override Boolean IsReady() => _ready;
        public override Single LoadWeight() => 0;
        public override Single ProgressFraction() => 1;
        public override String ProgressTitle() => _progressTitle;

        [ParserTargetCollection("Preload", Key = "textures", NameSignificance = NameSignificance.Key)]
        public List<String> paths;

        public override void StartLoad()
        {
            // Get the Kopernicus configuration node
            ConfigNode kopernicus = GameDatabase.Instance.GetConfigs(Injector.ROOT_NODE_NAME)[0].config;
            Parser.LoadObjectFromConfigurationNode(this, kopernicus);
            if (paths == null)
            {
                paths = new List<String>();
            }
            paths = paths.Distinct().ToList();
            
            _ready = false;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            foreach (String path in paths)
            {
                // Find all specified files inside of GameData
                String searchPath = KSPUtil.ApplicationRootPath + "GameData/" + path;
                String[] filesToLoad = Directory.GetFiles(
                    Path.GetDirectoryName(searchPath) ?? throw new InvalidOperationException(),
                    Path.GetFileName(searchPath),
                    SearchOption.AllDirectories);

                yield return null;

                // Loop through them and load all of them
                foreach (String file in filesToLoad)
                {
                    String cfgPath = file.Substring((KSPUtil.ApplicationRootPath + "GameData/").Length);
                    if (Textures.ContainsKey(cfgPath))
                    {
                        continue;
                    }
                    
                    _progressTitle = "Kopernicus: Loading " + cfgPath;
                    Textures.Add(cfgPath, Utility.LoadTexture(cfgPath, false, true, false));
                    yield return null;
                }
            }
            
            // Index builtin textures as well
            _progressTitle = "Kopernicus: Indexing BUILTIN textures";
            Texture[] builtin = Resources.FindObjectsOfTypeAll<Texture>();
            Int32 c = 0;
            foreach (Texture t in builtin)
            {
                String textureName = "BUILTIN/" + t.name;
                Int32 m = 2;
                while (Textures.ContainsKey(textureName))
                {
                    textureName = "BUILTIN/" + t.name + "-" + m++;
                }
                _progressTitle = "Kopernicus: Indexing " + textureName;
                Textures.Add(textureName, t as Texture2D);
                if (c++ % 100 == 0)
                {
                    yield return null;
                }
            }

            _ready = true;
        }
    }

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class TexturePreloaderInjector : MonoBehaviour
    {
        private void Start()
        {
            if (!CompatibilityChecker.IsCompatible())
            {
                Destroy(this);
                return;
            }
            DontDestroyOnLoad(gameObject);

            if (LoadingScreen.Instance == null)
            {
                Debug.Log("[Kopernicus] LoadingScreen instance not found! Aborting Texture preloading.");
                return;
            }

            List<LoadingSystem> list = LoadingScreen.Instance.loaders;
            list?.Insert(list.FindIndex(m => m is PartLoader), gameObject.AddComponent<TexturePreloader>());
        }
    }
}