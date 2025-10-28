using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

public class ModulateShadowMask : MonoBehaviour
{

	private CommandBuffer m_Buffer;
	private Light m_Light;
	private Camera fakeCamera;

	[SerializeField]
	private Material m_ShadowMaskModulateMaterial;

	public ModulateShadowMask ()
	{

	}

	public void Awake()
	{
		m_ShadowMaskModulateMaterial.DisableKeyword ("SPHERE_PLANET");
		m_ShadowMaskModulateMaterial.EnableKeyword ("FLAT_PLANET");

		m_Buffer = new CommandBuffer();
		m_Buffer.name = "ScreenspaceShadowMaskmodulate";

		m_Buffer.Blit (null, BuiltinRenderTextureType.CurrentActive, m_ShadowMaskModulateMaterial);

		m_Light = GetComponent<Light>();
		m_Light.AddCommandBuffer (LightEvent.AfterScreenspaceMask, m_Buffer);

	}
		
	public void Update()
	{
		m_ShadowMaskModulateMaterial.SetMatrix ("CameraToWorld", Camera.main.cameraToWorldMatrix);
		m_ShadowMaskModulateMaterial.SetMatrix ("WorldToLight", m_Light.gameObject.transform.worldToLocalMatrix); //may look like it doesn't work in the editor, ie using the editor Camera
	}

	public void OnDestroy ()
	{
		m_Light.RemoveCommandBuffer (LightEvent.AfterScreenspaceMask, m_Buffer);
	}
}


