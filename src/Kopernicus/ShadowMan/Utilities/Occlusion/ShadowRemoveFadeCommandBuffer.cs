//"fixes" the shadows fading near far clip plane

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

namespace Kopernicus.ShadowMan
{
    public class ShadowRemoveFadeCommandBuffer : MonoBehaviour
    {

        private CommandBuffer m_Buffer;
        private Camera m_Camera;

        private void Awake()
        {
            m_Buffer = new CommandBuffer();
            m_Buffer.name = "ShadowManShadowRemoveFade";

            //works for the fade but doesn't fix breaks in squares/axis-aligned lines near farclipPlane of nearCamera, limitation of what? idk
            //could still be ok for SSAO, maybe passable for eclipses but not sure
            m_Buffer.SetGlobalVector(ShaderProperties.unity_ShadowFadeCenterAndType_PROPERTY, new Vector4(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, -1f));

            m_Camera = GetComponent<Camera>();
            m_Camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, m_Buffer);
        }

        public void OnDestroy()
        {
            m_Camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, m_Buffer);
        }
    }
}

