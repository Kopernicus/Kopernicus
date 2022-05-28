using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Kopernicus.ShadowMan
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class RenderTypeFixer : MonoBehaviour
    {
        static Dictionary<String, Shader> shaderDictionary = new Dictionary<String, Shader>();
        private void Awake()
        {
            if (HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                GameEvents.onGameSceneLoadRequested.Add(GameSceneLoaded);
            }
        }


        private void GameSceneLoaded(GameScenes scene)
        {
            if (scene == GameScenes.SPACECENTER || scene == GameScenes.FLIGHT)
            {
                Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
                foreach (Material mat in materials)
                {
                    fixRenderType(mat);
                }
            }
        }

        public static void fixRenderType(Material mat)
        {
            String name = mat.shader.name;
            if ((name.Contains("PQS") && name.Contains("Terrain"))
                || (name.Contains("PQS Main")))

            //no longer need custom depth buffer for refractions or scattering, only for distant shadows, so disable these
            //|| (name == "Legacy Shaders/Transparent/Specular")    //fixes kerbal visor leaking into water refraction
            //|| (name == "KSP/Emissive/Diffuse")))    			  //fixes new mk1-3 pod

            {
                mat.SetOverrideTag("RenderType", "Opaque");
            }

            //fixes trees and cutouts
            if ((name == "Legacy Shaders/Transparent/Cutout") || (name == "KSP/Alpha/Cutoff") || (name == "KSP/Specular (Cutoff)"))
            {
                mat.SetOverrideTag("RenderType", "TransparentCutout");
            }

            if (name.Contains("EVE/PlanetLight") || name.Contains("EVE/Cloud") || name.Contains("Translucent") || name.Contains("Legacy Shaders/Particles"))
            {
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetOverrideTag("IgnoreProjector", "True");
            }
        }
    }
}