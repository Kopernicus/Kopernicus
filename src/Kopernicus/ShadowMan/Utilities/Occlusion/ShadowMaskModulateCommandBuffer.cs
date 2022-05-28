using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;


namespace Kopernicus.ShadowMan
{
    public class ShadowMaskModulateCommandBuffer : MonoBehaviour
    {

        private CommandBuffer m_Buffer;
        private Light m_Light;
        private Material m_ShadowMaskModulateMaterial;

        public ShadowMaskModulateCommandBuffer()
        {
            // after light's screenspace shadow mask is computed, copy it
            m_Buffer = new CommandBuffer();
            m_Buffer.name = "ShadowManScreenspaceShadowMaskmodulate";

            m_ShadowMaskModulateMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/ModulateShadowMaskWithOcclusion")]);
            //			m_ShadowMaskModulateMaterial.SetTexture ("OcclusionTexture", Core.Instance.bufferRenderingManager.occlusionTexture);

            //after light's screenspace shadow mask is computed, modulate it by occlusion texture
            //			m_Buffer.Blit (Core.Instance.bufferRenderingManager.occlusionTexture, BuiltinRenderTextureType.CurrentActive, m_ShadowMaskModulateMaterial);

            m_Light = GetComponent<Light>();
            m_Light.AddCommandBuffer(LightEvent.AfterScreenspaceMask, m_Buffer);
        }

        public void OnDestroy()
        {
            m_Light.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, m_Buffer);
        }
    }
}

