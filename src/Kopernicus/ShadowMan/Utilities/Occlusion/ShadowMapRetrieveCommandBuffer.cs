using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;


namespace Kopernicus.ShadowMan
{
    public class ShadowMapRetrieveCommandBuffer : MonoBehaviour
    {

        private CommandBuffer m_Buffer;
        private Light m_Light;

        public ShadowMapRetrieveCommandBuffer()
        {
            m_Buffer = new CommandBuffer();
            m_Buffer.name = "ShadowManShadowMapRetrieve";

            //instead of making a copy like shadowMapCopyCommandbuffer, here we just set the shadowmap as a global texture, for access in shader
            m_Buffer.SetGlobalTexture(ShaderProperties._ShadowMapTextureShadowMan_PROPERTY, new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive));

            m_Light = GetComponent<Light>();
            m_Light.AddCommandBuffer(LightEvent.AfterShadowMap, m_Buffer);
        }

        public void OnDestroy()
        {
            m_Light.RemoveCommandBuffer(LightEvent.AfterShadowMap, m_Buffer);
        }
    }
}

