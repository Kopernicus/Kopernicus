using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;


namespace Kopernicus.ShadowMan
{
    public class ShadowMaskCopyCommandBuffer : MonoBehaviour
    {

        private CommandBuffer m_Buffer;
        private Light m_Light;

        public ShadowMaskCopyCommandBuffer()
        {
            // after light's screenspace shadow mask is computed, copy it
            m_Buffer = new CommandBuffer();
            m_Buffer.name = "ShadowManScreenspaceShadowMaskCopy";

            //			m_Buffer.Blit (BuiltinRenderTextureType.CurrentActive, Core.Instance.bufferRenderingManager.occlusionTexture);

            m_Light = GetComponent<Light>();
            m_Light.AddCommandBuffer(LightEvent.AfterScreenspaceMask, m_Buffer);
        }

        public void OnDestroy()
        {
            m_Light.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, m_Buffer);
        }
    }
}

