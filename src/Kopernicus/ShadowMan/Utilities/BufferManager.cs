using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kopernicus.ShadowMan
{
    //Pretty sure this class should be useless, change affected classes to static
    public class BufferManager : MonoBehaviour
    {
        public RenderTexture depthTexture;          //full-scene depth texture, from merged built-in depth textures of the two local cameras
                                                    //public RenderTexture occlusionTexture;	//for SSAO and eclipses, for now will just contain a copy of the screenspace shadowmask, probably not necessary
        Coroutine checkTexturesCoroutine;

        public bool depthTextureCleared = false;    //clear depth texture when away from PQS, for the sunflare shader

        public void Awake()
        {
            CreateTextures();
        }

        public void CreateTextures() //create simpler method createTexture with params, call it multiple times, make it static and move it to utils, reuse in skynode as well
        {
            if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
            {
                if (!ShadowMan.Instance.unifiedCameraMode)
                {
                    depthTexture = Utils.CreateTexture("ShadowManDepthTexture", Screen.width, Screen.height, 0, RenderTextureFormat.RFloat, false, FilterMode.Point, 1);
                }
            }
        }

        public void ClearDepthTexture()
        {
            if (!ShadowMan.Instance.unifiedCameraMode)
            {
                RenderTexture rt = RenderTexture.active;
                RenderTexture.active = depthTexture;
                GL.Clear(false, true, Color.white);
                RenderTexture.active = rt;
            }
            depthTextureCleared = true;
        }

        public void OnDestroy()
        {
            if (depthTexture)
            {
                depthTexture.Release();
                UnityEngine.Object.Destroy(depthTexture);
            }
            if (!ReferenceEquals(checkTexturesCoroutine, null))
            {
                StopCoroutine(checkTexturesCoroutine);
            }
        }

    }
}
