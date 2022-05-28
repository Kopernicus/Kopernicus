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
    public class ShaderReplacer : MonoBehaviour
    {
        private static ShaderReplacer instance;
        public Dictionary<string, Shader> LoadedShaders = new Dictionary<string, Shader>();
        public Dictionary<string, ComputeShader> LoadedComputeShaders = new Dictionary<string, ComputeShader>();
        public Dictionary<string, Texture> LoadedTextures = new Dictionary<string, Texture>();
        string path;

        Dictionary<string, Shader> EVEshaderDictionary;

        public Dictionary<string, string> gameShaders = new Dictionary<string, string>();

        private ShaderReplacer()
        {
            Init();
        }

        public static ShaderReplacer Instance
        {
            get
            {
                if (ReferenceEquals(instance, null))
                {
                    instance = new ShaderReplacer();
                    Utils.LogDebug("ShaderReplacer instance created");
                }
                return instance;
            }
        }


        private void Init()
        {
            path = KSPUtil.ApplicationRootPath + "GameData/";
            LoadAssetBundle();
        }

        public void LoadAssetBundle()
        {
            string shaderspath;

            if (Application.platform == RuntimePlatform.WindowsPlayer && SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL"))
                shaderspath = path + "Kopernicus/Shaders/scatterershaders-linux";   //fixes openGL on windows
            else
                if (Application.platform == RuntimePlatform.WindowsPlayer)
                shaderspath = path + "Kopernicus/Shaders/scatterershaders-windows";
            else if (Application.platform == RuntimePlatform.LinuxPlayer)
                shaderspath = path + "Kopernicus/Shaders/scatterershaders-linux";
            else
                shaderspath = path + "Kopernicus/Shaders/scatterershaders-macosx";

            LoadedShaders.Clear();
            LoadedComputeShaders.Clear();
            LoadedTextures.Clear();

            using (WWW www = new WWW("file://" + shaderspath))
            {
                AssetBundle bundle = www.assetBundle;
                Shader[] shaders = bundle.LoadAllAssets<Shader>();

                foreach (Shader shader in shaders)
                {
                    //Utils.Log (""+shader.name+" loaded. Supported?"+shader.isSupported.ToString());
                    LoadedShaders.Add(shader.name, shader);
                }

                ComputeShader[] computeShaders = bundle.LoadAllAssets<ComputeShader>();

                foreach (ComputeShader computeShader in computeShaders)
                {
                    //Utils.LogInfo ("Compute shader "+computeShader.name+" loaded.");
                    LoadedComputeShaders.Add(computeShader.name, computeShader);
                }

                Texture[] textures = bundle.LoadAllAssets<Texture>();

                foreach (Texture texture in textures)
                {
                    LoadedTextures.Add(texture.name, texture);
                }

                bundle.Unload(false); // unload the raw asset bundle
                www.Dispose();
            }
        }

        public void replaceEVEshaders()
        {
            //reflection get EVE shader dictionary
            Utils.LogDebug("Replacing EVE shaders");

            //find EVE shaderloader
            Type EVEshaderLoaderType = getType ("ShaderLoader.ShaderLoaderClass");

            if (EVEshaderLoaderType == null)
            {
                Utils.LogDebug("Eve shaderloader type not found");
            }
            else
            {
                Utils.LogDebug("Eve shaderloader type found");

                Utils.LogDebug("Eve shaderloader version: " + EVEshaderLoaderType.Assembly.GetName().ToString());

                const BindingFlags flags =  BindingFlags.FlattenHierarchy |  BindingFlags.NonPublic | BindingFlags.Public |
                    BindingFlags.Instance | BindingFlags.Static;

                try
                {
                    //				EVEinstance = EVEType.GetField("Instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                    EVEshaderDictionary = EVEshaderLoaderType.GetField("shaderDictionary", flags).GetValue(null) as Dictionary<string, Shader>;
                }
                catch (Exception)
                {
                    Utils.LogDebug("No EVE shader dictionary found");
                }

                if (EVEshaderDictionary == null)
                {
                    Utils.LogDebug("Failed grabbing EVE shader dictionary");
                }
                else
                {
                    Utils.LogDebug("Successfully grabbed EVE shader dictionary");


                    if (EVEshaderDictionary.ContainsKey("EVE/Cloud"))
                    {
                        EVEshaderDictionary["EVE/Cloud"] = LoadedShaders["ShadowMan-EVE/Cloud"];
                    }
                    else
                    {
                        List<Material> cloudsList = new List<Material>();
                        EVEshaderDictionary.Add("EVE/Cloud", LoadedShaders["ShadowMan-EVE/Cloud"]);
                    }

                    Utils.LogDebug("Replaced EVE/Cloud in EVE shader dictionary");

                    if (EVEshaderDictionary.ContainsKey("EVE/CloudVolumeParticle"))
                    {
                        EVEshaderDictionary["EVE/CloudVolumeParticle"] = LoadedShaders["ShadowMan-EVE/CloudVolumeParticle"];
                    }
                    else
                    {
                        List<Material> cloudsList = new List<Material>();
                        EVEshaderDictionary.Add("EVE/CloudVolumeParticle", LoadedShaders["ShadowMan-EVE/CloudVolumeParticle"]);
                    }

                    Utils.LogDebug("replaced EVE/CloudVolumeParticle in EVE shader dictionary");

                    if (EVEshaderDictionary.ContainsKey("EVE/GeometryCloudVolumeParticle"))
                    {
                        EVEshaderDictionary["EVE/GeometryCloudVolumeParticle"] = LoadedShaders["ShadowMan-EVE/GeometryCloudVolumeParticle"];
                    }
                    else
                    {
                        List<Material> cloudsList = new List<Material>();
                        EVEshaderDictionary.Add("EVE/GeometryCloudVolumeParticle", LoadedShaders["ShadowMan-EVE/GeometryCloudVolumeParticle"]);
                    }

                    Utils.LogDebug("replaced EVE/GeometryCloudVolumeParticle in EVE shader dictionary");

                    if (EVEshaderDictionary.ContainsKey("EVE/GeometryCloudVolumeParticleToTexture"))
                    {
                        EVEshaderDictionary["EVE/GeometryCloudVolumeParticleToTexture"] = LoadedShaders["ShadowMan-EVE/GeometryCloudVolumeParticleToTexture"];
                    }
                    else
                    {
                        List<Material> cloudsList = new List<Material>();
                        EVEshaderDictionary.Add("EVE/GeometryCloudVolumeParticleToTexture", LoadedShaders["ShadowMan-EVE/GeometryCloudVolumeParticleToTexture"]);
                    }

                    Utils.LogDebug("replaced EVE/GeometryCloudVolumeParticleToTexture in EVE shader dictionary");
                }
            }


            //replace shaders of already created materials
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>();

            //gameShaders.Clear ();

            foreach (Material mat in materials)
            {
                //ReplaceGameShader(mat);

                if (EVEshaderDictionary != null)
                {
                    ReplaceEVEShader(mat);
                }
            }

            //			foreach (string key in gameShaders.Keys)
            //			{
            //				Utils.Log(gameShaders[key]);
            //			}
        }

        //		private void ReplaceGameShader(Material mat)
        //		{
        //			String name = mat.shader.name;
        //			Shader replacementShader = null;
        //			
        //			if (!gameShaders.ContainsKey (name))
        //			{
        //				gameShaders [name] = "shader " + name + " materials: " + mat.name;
        //			}
        //			else
        //			{
        //				gameShaders [name] = gameShaders [name] + " " + mat.name;
        //			}
        //			
        //			switch (name)
        //			{
        //			case "Terrain/PQS/PQS Main - Optimised":
        //				Utils.Log("replacing PQS main optimised");
        //				replacementShader = LoadedShaders["Scatterer/PQS/PQS Main - Optimised"];
        //				Utils.Log("Shader replaced");
        //				break;
        //			default:
        //				return;
        //			}
        //			if (replacementShader != null)
        //			{
        //				mat.shader = replacementShader;
        //			}
        //		}

        private void ReplaceEVEShader(Material mat)
        {
            String name = mat.shader.name;
            Shader replacementShader = null;

            switch (name)
            {
                case "EVE/Cloud":
                    Utils.LogDebug("replacing EVE/Cloud");
                    replacementShader = LoadedShaders["ShadowMan-EVE/Cloud"];
                    Utils.LogDebug("Shader replaced");
                    break;
                case "EVE/CloudVolumeParticle":
                    Utils.LogDebug("replacing EVE/CloudVolumeParticle");
                    replacementShader = LoadedShaders["ShadowMan-EVE/CloudVolumeParticle"];
                    Utils.LogDebug("Shader replaced");
                    break;
                case "EVE/GeometryCloudVolumeParticle":
                    Utils.LogDebug("replacing EVE/GeometryCloudVolumeParticle");
                    replacementShader = LoadedShaders["ShadowMan-EVE/GeometryCloudVolumeParticle"];
                    Utils.LogDebug("Shader replaced");
                    break;
                case "EVE/GeometryCloudVolumeParticleToTexture":
                    Utils.LogDebug("replacing EVE/GeometryCloudVolumeParticleToTexture");
                    replacementShader = LoadedShaders["ShadowMan-EVE/GeometryCloudVolumeParticleToTexture"];
                    Utils.LogDebug("Shader replaced");
                    break;
                //			case "Terrain/PQS/PQS Main - Optimised":
                //				Utils.Log("replacing PQS main optimised");
                //				replacementShader = LoadedShaders["Scatterer/PQS/PQS Main - Optimised"];
                //				Utils.Log("Shader replaced");
                //				break;
                default:
                    return;
            }
            if (replacementShader != null)
            {
                mat.shader = replacementShader;
            }
        }

        internal static Type getType(string name)
        {
            Type type = null;
            AssemblyLoader.loadedAssemblies.TypeOperation(t =>
            {
                if (t.FullName == name)
                    type = t;
            }
            );

            if (type != null)
            {
                return type;
            }
            return null;
        }
    }
}
