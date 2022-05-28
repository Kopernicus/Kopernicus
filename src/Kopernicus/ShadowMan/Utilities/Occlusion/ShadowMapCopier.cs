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
using UnityEngine.Rendering;


namespace Kopernicus.ShadowMan
{
    public class ShadowMapCopier : MonoBehaviour
    {
        private CommandBuffer copyCascadeCB0, copyCascadeCB1, copyCascadeCB2, copyCascadeCB3, textureCopyBuffer;
        private Light m_Light;
        bool commandBufferAdded = false;

        private static Dictionary<Light, ShadowMapCopier> instances = new Dictionary<Light, ShadowMapCopier>();

        public static void RequestShadowMapCopy(Light light)
        {
            if (instances.ContainsKey(light))
            {
                instances[light].RequestShadowMapCopy();
            }
            else
            {
                instances[light] = ScaledCamera.Instance.galaxyCamera.gameObject.AddComponent<ShadowMapCopier>();
                instances[light].Init(light);
                instances[light].RequestShadowMapCopy();
            }
        }

        public ShadowMapCopier()
        {
        }

        public void Init(Light light)
        {
            m_Light = light;
            CreateTextureCopyCB();
            GameEvents.onGameSceneLoadRequested.Add(RecreateForSceneChange);
        }

        public void RecreateForSceneChange(GameScenes scene)
        {
            StartCoroutine(DelayedRecreateForSceneChange());
        }

        //When scene changes, the resolution of the shadowMap can change so recreate the commandBuffers
        IEnumerator DelayedRecreateForSceneChange()
        {
            for (int i = 0; i < 3; i++)
                yield return new WaitForFixedUpdate();

            Disable();
            CreateTextureCopyCB();
        }

        //Adds one one commandbuffer which does an optimized pixel-by-pixel texture copy
        private void CreateTextureCopyCB()
        {
            textureCopyBuffer = new CommandBuffer();
            textureCopyBuffer.CopyTexture(BuiltinRenderTextureType.CurrentActive, ShadowMapCopy.RenderTexture);
        }

        //These are adapted for copying one cascade and then rendering other stuff on top, like clouds, however since I'm going to render them separately, better do a fast copytexture
        private void CreateCopyCascadeCBs()
        {
            copyCascadeCB0 = CreateCopyCascadeCB(ShadowMapCopy.RenderTexture, 0f, 0f, 0.5f, 0.5f);
            copyCascadeCB1 = CreateCopyCascadeCB(ShadowMapCopy.RenderTexture, 0.5f, 0f, 0.5f, 0.5f);
            copyCascadeCB2 = CreateCopyCascadeCB(ShadowMapCopy.RenderTexture, 0f, 0.5f, 0.5f, 0.5f);
            copyCascadeCB3 = CreateCopyCascadeCB(ShadowMapCopy.RenderTexture, 0.5f, 0.5f, 0.5f, 0.5f);
        }

        private CommandBuffer CreateCopyCascadeCB(RenderTexture targetRt, float startX, float startY, float width, float height)
        {
            CommandBuffer cascadeCopyCB = new CommandBuffer();
            Rect cascadeRect = new Rect ((int)(startX * targetRt.width), (int)(startY * targetRt.height), (int)(width * targetRt.width), (int)(height * targetRt.height));

            cascadeCopyCB.EnableScissorRect(cascadeRect);
            cascadeCopyCB.SetShadowSamplingMode(BuiltinRenderTextureType.CurrentActive, ShadowSamplingMode.RawDepth);
            cascadeCopyCB.Blit(BuiltinRenderTextureType.CurrentActive, targetRt);
            cascadeCopyCB.DisableScissorRect();

            return cascadeCopyCB;
        }

        public void Disable()
        {
            m_Light.RemoveCommandBuffer(LightEvent.AfterShadowMap, textureCopyBuffer);

            commandBufferAdded = false;
        }

        public void Enable()
        {
            if (!commandBufferAdded)
            {
                m_Light.AddCommandBuffer(LightEvent.AfterShadowMap, textureCopyBuffer);

                commandBufferAdded = true;
            }
        }

        public void OnPreRender()
        {
            if (commandBufferAdded)
                Disable();
        }

        private void RequestShadowMapCopy()
        {
            if (!commandBufferAdded)
                Enable();
        }

        public void OnDestroy()
        {
            Disable();
        }
    }

    public static class ShadowMapCopy
    {
        private static RenderTexture renderTexture;

        public static RenderTexture RenderTexture
        {
            get
            {
                if (ReferenceEquals(renderTexture, null))
                {
                    CreateTexture();
                }
                else
                {
                    //If the size of the shadowMap changed from the last time we created a copy
                    if (ShadowMan.Instance.sunLight.shadowCustomResolution != renderTexture.width)
                    {
                        renderTexture.Release();
                        CreateTexture();
                    }
                }

                return renderTexture;
            }
        }

        private static void CreateTexture()
        {
            //I think we can leave it because they all should have the same custom resolution anyway
            renderTexture = new RenderTexture((int)ShadowMan.Instance.sunLight.shadowCustomResolution, (int)ShadowMan.Instance.sunLight.shadowCustomResolution, 0, RenderTextureFormat.Shadowmap);
            renderTexture.useMipMap = false;
            renderTexture.antiAliasing = 1;
            renderTexture.filterMode = FilterMode.Point;
            renderTexture.Create();
        }
    }
}

