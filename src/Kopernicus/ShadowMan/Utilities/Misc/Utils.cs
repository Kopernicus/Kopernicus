using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime;
using KSP;
using KSP.IO;
using UnityEngine;

namespace Kopernicus.ShadowMan
{
    public static class Utils
    {
        private static string pluginPath;
        public static string PluginPath
        {
            get
            {
                if (ReferenceEquals(null, pluginPath))
                {
                    string codeBase = Assembly.GetExecutingAssembly ().CodeBase;
                    UriBuilder uri = new UriBuilder (codeBase);
                    pluginPath = Uri.UnescapeDataString(uri.Path);
                    pluginPath = Path.GetDirectoryName(pluginPath);
                }
                return pluginPath;
            }
        }

        private static string gameDataPath;
        public static string GameDataPath
        {
            get
            {
                if (ReferenceEquals(null, gameDataPath))
                {
                    gameDataPath = KSPUtil.ApplicationRootPath + "GameData/";
                }
                return gameDataPath;
            }
        }

        public static void LogDebug(string log)
        {
            Debug.Log("[ShadowMan][Debug] " + log);
        }

        public static void LogInfo(string log)
        {
            Debug.Log("[ShadowMan][Info] " + log);
        }

        public static void LogError(string log)
        {
            Debug.LogError("[ShadowMan][Error] " + log);
        }

        public static GameObject GetMainMenuObject(CelestialBody celestialBody)
        {
            string name = celestialBody.isHomeWorld ? "Kerbin" : celestialBody.name;

            GameObject mainMenuObject = GameObject.FindObjectsOfType<GameObject>().FirstOrDefault(b => ( (b.name == name) && b.transform.parent.name.Contains("Scene")));

            if (ReferenceEquals(mainMenuObject, null))
            {
                throw new Exception("No correct main menu object found for " + celestialBody.name);
            }

            return mainMenuObject;
        }

        public static Transform GetScaledTransform(string body)
        {
            return (ScaledSpace.Instance.transform.FindChild(body));
        }

        public static void FixKopernicusRingsRenderQueue()
        {
            foreach (CelestialBody _cb in FlightGlobals.Bodies)
            {
                if (!_cb.name.Equals("KopernicusWatchdog"))
                {
                    GameObject ringObject;
                    ringObject = GameObject.Find(_cb.name + "Ring");
                    if (ringObject)
                    {
                        ringObject.GetComponent<MeshRenderer>().material.renderQueue = 3005;
                        Utils.LogDebug("Found rings for " + _cb.name);
                    }
                }
            }
        }

        public static void FixSunsCoronaRenderQueue()
        {
            foreach (CelestialBody _scattererCB in FlightGlobals.Bodies)
            {
                if (!_scattererCB.name.Equals("KopernicusWatchdog"))
                {
                    Transform scaledSunTransform = Utils.GetScaledTransform(Kopernicus.Components.KopernicusStar.GetBrightest(_scattererCB).sun.name);
                    foreach (Transform child in scaledSunTransform)
                    {
                        MeshRenderer temp = child.gameObject.GetComponent<MeshRenderer> ();
                        if (temp != null)
                            temp.material.renderQueue = 2998;
                    }
                }
            }
        }

        public static RenderTexture CreateTexture(string name, int width, int height, int depth, RenderTextureFormat format, bool useMipmap, FilterMode filtermode, int antiAliasing)
        {

            RenderTexture renderTexture = new RenderTexture ( width,height,depth, format);
            renderTexture.name = name;
            renderTexture.useMipMap = useMipmap;
            renderTexture.filterMode = filtermode;
            renderTexture.antiAliasing = antiAliasing;
            renderTexture.Create();

            return renderTexture;
        }

        // As of 1.9.1 there are two rendering modes in KSP, unified localCamera (Directx 11) and dual local cameras (the old mode)
        // Sometimes we need to do some work on the first local camera to render, which can be either the unified camera or the far camera
        public static Camera getEarliestLocalCamera()
        {
            return ShadowMan.Instance.unifiedCameraMode ? ShadowMan.Instance.nearCamera : ShadowMan.Instance.farCamera;
        }

        // Will return true for zero, so be aware
        public static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        // If condition is true, enable keywordOn and disable keywordOff, else do the opposite
        public static void EnableOrDisableShaderKeywords(Material mat, string keywordOn, string keywordOff, bool enable)
        {
            if (!ReferenceEquals(mat, null))
            {
                if (enable)
                {
                    mat.EnableKeyword(keywordOn);
                    mat.DisableKeyword(keywordOff);
                }
                else
                {
                    mat.EnableKeyword(keywordOff);
                    mat.DisableKeyword(keywordOn);
                }
            }
        }

        //Thanks to linx for this snippet
        public static Texture2D LoadDDSTexture(byte[] data, string name)
        {
            Texture2D texture=null;

            byte ddsSizeCheck = data[4];
            if (ddsSizeCheck != 124)
            {
                LogError("This DDS texture is invalid - Unable to read the size check value from the header.");
                return texture;
            }


            int height = data[13] * 256 + data[12];
            int width = data[17] * 256 + data[16];

            int DDS_HEADER_SIZE = 128;
            byte[] dxtBytes = new byte[data.Length - DDS_HEADER_SIZE];
            Buffer.BlockCopy(data, DDS_HEADER_SIZE, dxtBytes, 0, data.Length - DDS_HEADER_SIZE);
            int mipMapCount = (data[28]) | (data[29] << 8) | (data[30] << 16) | (data[31] << 24);

            TextureFormat format = TextureFormat.YUY2; //just an invalid type
            if (data[84] == 'D')
            {
                if (data[87] == 49) //Also char '1'
                {
                    format = TextureFormat.DXT1;
                }
                else if (data[87] == 53)    //Also char '5'
                {
                    format = TextureFormat.DXT5;
                }
            }

            if (format == TextureFormat.YUY2)
            {
                LogError("Format of texture " + name + " unidentified");
                return texture;
            }

            if (mipMapCount == 1)
            {
                texture = new Texture2D(width, height, format, false);
            }
            else
            {
                texture = new Texture2D(width, height, format, true);
            }
            try
            {
                texture.LoadRawTextureData(dxtBytes);
            }
            catch
            {
                LogError("Texture " + name + " couldn't be loaded");
                return texture;
            }
            texture.Apply();

            LogInfo("Loaded texture " + name + " " + width.ToString() + "x" + height.ToString() + " mip count: " + mipMapCount.ToString());

            return texture;
        }
    }
}

