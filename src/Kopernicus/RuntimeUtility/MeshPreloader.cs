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
using Kopernicus.Constants;
using UnityEngine;

namespace Kopernicus.RuntimeUtility
{
    public class MeshPreloader : LoadingSystem
    {
        public static readonly Dictionary<String, Mesh> Meshes = new Dictionary<String, Mesh>();

        private Boolean _ready;
        private String _progressTitle = "Kopernicus: Loading ScaledSpace Meshes";

        public override Boolean IsReady() => _ready;
        public override Single LoadWeight() => 0;
        public override Single ProgressFraction() => 1;
        public override String ProgressTitle() => _progressTitle;

        public override void StartLoad()
        {
            _ready = false;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            // Find all .bin files inside of GameData
            String[] binFiles = Directory.GetFiles(KSPUtil.ApplicationRootPath + "GameData/", "*.bin",
                SearchOption.AllDirectories);

            yield return null;

            // Loop through them and load all of them
            for (Int32 i = 0; i < binFiles.Length; i++)
            {
                _progressTitle = "Kopernicus: Loading " + binFiles[i].Substring((KSPUtil.ApplicationRootPath + "GameData/").Length);

                try
                {
                    Meshes.Add(binFiles[i], Utility.DeserializeMesh(binFiles[i]));
                    Debug.Log("[Kopernicus] Loaded '" + binFiles[i] + "'");
                }
                catch
                {
                    Debug.Log("[Kopernicus] Could not load '" + binFiles[i] + "'");
                }

                yield return null;
            }

            _ready = true;
        }
    }

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class MeshPreloaderInjector : MonoBehaviour
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
                Debug.Log("[Kopernicus] LoadingScreen instance not found! Aborting Mesh preloading.");
                return;
            }

            List<LoadingSystem> list = LoadingScreen.Instance.loaders;
            list?.Insert(list.FindIndex(m => m is GameDatabase) + 1, gameObject.AddComponent<MeshPreloader>());
        }
    }
}