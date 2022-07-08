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
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class ShadowMan : MonoBehaviour
    {
        private static ShadowMan instance;
        public static ShadowMan Instance { get { return instance; } }
        public BufferManager bufferManager;
        public EVEReflectionHandler eveReflectionHandler;

        public ShadowRemoveFadeCommandBuffer shadowFadeRemover;
        public TweakShadowCascades shadowCascadeTweaker;

        public Light sunLight,scaledSpaceSunLight, mainMenuLight;
        public Light[] lights;
        public Camera farCamera, scaledSpaceCamera, nearCamera;
        static float originalShadowDistance = 0f;
        public bool isActive = false;
        public bool unifiedCameraMode = false;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(this);
            }

            if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                isActive = true;
            }

            if (isActive)
            {
                StartCoroutine(DelayedInit());
            }
        }

        //wait for 4 frames (1 less than scatterer) for EVE and the game to finish setting up
        IEnumerator DelayedInit()
        {
            int delayFrames = (HighLogic.LoadedScene == GameScenes.MAINMENU) ? 4 : 1;
            for (int i = 0; i < delayFrames; i++)
                yield return new WaitForFixedUpdate();
            if (RuntimeUtility.RuntimeUtility.KopernicusConfig.EnableKopernicusShadowManager)
            {
                Init();
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(this);
            }
        }

        void Init()
        {
            SetupMainCameras();

            FindSunlights();

            SetShadows();

            Utils.FixKopernicusRingsRenderQueue();
            Utils.FixSunsCoronaRenderQueue();

            if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
            {
                // Note: Stock KSP dragCubes make a copy of components and removes them rom far/near cameras when rendering
                // This can cause issues with renderTextures and commandBuffers, to keep in mind for when implementing godrays
                bufferManager = (BufferManager)scaledSpaceCamera.gameObject.AddComponent(typeof(BufferManager));    // This doesn't need to be added to any camera anymore
                                                                                                                    // TODO: move to appropriate gameObjec
            }

            if (!unifiedCameraMode)
                shadowFadeRemover = (ShadowRemoveFadeCommandBuffer)nearCamera.gameObject.AddComponent(typeof(ShadowRemoveFadeCommandBuffer));

            //magically fix stupid issues when reverting to space center from map view
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                MapView.MapIsEnabled = false;
            }

            Utils.LogDebug("Kopernicus.ShadowMan setup done");
        }

        void OnDestroy()
        {
            if (isActive)
            {
                if (nearCamera)
                {
                    if (nearCamera.gameObject.GetComponent(typeof(Wireframe)))
                        Component.DestroyImmediate(nearCamera.gameObject.GetComponent(typeof(Wireframe)));


                    if (farCamera && farCamera.gameObject.GetComponent(typeof(Wireframe)))
                        Component.DestroyImmediate(farCamera.gameObject.GetComponent(typeof(Wireframe)));


                    if (scaledSpaceCamera.gameObject.GetComponent(typeof(Wireframe)))
                        Component.DestroyImmediate(scaledSpaceCamera.gameObject.GetComponent(typeof(Wireframe)));
                }

                if (shadowFadeRemover)
                {
                    shadowFadeRemover.OnDestroy();
                    Component.DestroyImmediate(shadowFadeRemover);
                }

                if (shadowCascadeTweaker)
                {
                    Component.DestroyImmediate(shadowCascadeTweaker);
                }

                if (bufferManager)
                {
                    bufferManager.OnDestroy();
                    Component.DestroyImmediate(bufferManager);
                }
            }
        }

        void SetupMainCameras()
        {
            scaledSpaceCamera = Camera.allCameras.FirstOrDefault(_cam => _cam.name == "Camera ScaledSpace");
            farCamera = Camera.allCameras.FirstOrDefault(_cam => _cam.name == "Camera 01");
            nearCamera = Camera.allCameras.FirstOrDefault(_cam => _cam.name == "Camera 00");

            if (nearCamera && !farCamera)
            {
                Utils.LogInfo("Running in unified camera mode");
                unifiedCameraMode = true;
            }

            if (scaledSpaceCamera && nearCamera)
            {
                if (!unifiedCameraMode)
                {
                    shadowCascadeTweaker = (TweakShadowCascades)Utils.getEarliestLocalCamera().gameObject.AddComponent(typeof(TweakShadowCascades));
                    shadowCascadeTweaker.Init(new Vector3(0.005f, 0.025f, 0.125f));
                }
                else
                {
                    shadowCascadeTweaker = (TweakShadowCascades)Utils.getEarliestLocalCamera().gameObject.AddComponent(typeof(TweakShadowCascades));
                    shadowCascadeTweaker.Init(new Vector3(0.0015f, 0.015f, 0.15f));
                }
            }
            else if (HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                // If are in main menu, where there is only 1 camera, affect all cameras to Landscape camera
                scaledSpaceCamera = Camera.allCameras.Single(_cam => _cam.name == "Landscape Camera");
                farCamera = scaledSpaceCamera;
                nearCamera = scaledSpaceCamera;
            }
            else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                // If in trackstation, just to get rid of some nullrefs
                farCamera = scaledSpaceCamera;
                nearCamera = scaledSpaceCamera;
            }
        }

        void SetShadows()
        {
            if (HighLogic.LoadedScene != GameScenes.MAINMENU)
            {
                if (unifiedCameraMode)
                {
                    QualitySettings.shadowProjection = ShadowProjection.StableFit; //way more resistant to jittering
                    GraphicsSettings.SetShaderMode(BuiltinShaderType.ScreenSpaceShadows, BuiltinShaderMode.UseCustom);

                    GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, ShaderReplacer.Instance.LoadedShaders[("Scatterer/customScreenSpaceShadows")]);
                }
                else
                {
                    QualitySettings.shadowProjection = ShadowProjection.CloseFit;
                }
                if (originalShadowDistance == 0f)
                {
                    originalShadowDistance = QualitySettings.shadowDistance;
                }

                QualitySettings.shadowDistance = RuntimeUtility.RuntimeUtility.KopernicusConfig.ShadowDistanceLimit;

                SetShadowsForLight(sunLight);

                // And finally force shadow Casting and receiving on celestial bodies if not already set
                foreach (CelestialBody _sc in FlightGlobals.Bodies)
                {
                    if (_sc.pqsController && (!_sc.name.Equals("KopernicusWatchdog")))
                    {
                        _sc.pqsController.meshCastShadows = true;
                        _sc.pqsController.meshRecieveShadows = true;
                    }
                }
            }
        }

        public void SetShadowsForLight(Light light)
        {
            if (light && (HighLogic.LoadedScene != GameScenes.MAINMENU))
            {
                //fixes checkerboard artifacts aka shadow acne
                float bias = unifiedCameraMode ? 0f : 0.72f;
                float normalBias = unifiedCameraMode ? 0f : 0.5f;
                if (bias != 0f)
                    light.shadowBias = bias;
                if (normalBias != 0f)
                    light.shadowNormalBias = normalBias;
                int customRes = unifiedCameraMode ? 8192 : 0;
                if (customRes != 0)
                {
                    if (Utils.IsPowerOfTwo(customRes))
                    {
                        Utils.LogDebug("Setting shadowmap resolution to: " + customRes.ToString() + " on " + light.name);
                        light.shadowCustomResolution = customRes;
                    }
                    else
                    {
                        Utils.LogError("Selected shadowmap resolution not a power of 2: " + customRes.ToString());
                    }
                }
                else
                    light.shadowCustomResolution = 0;
            }
        }

        void FindSunlights()
        {
            lights = (Light[])Light.FindObjectsOfType(typeof(Light));
            foreach (Light _light in lights)
            {
                if (_light.gameObject.name == "SunLight")
                {
                    sunLight = _light;
                }
                if (_light.gameObject.name == "Scaledspace SunLight")
                {
                    scaledSpaceSunLight = _light;
                }
                if (_light.gameObject.name.Contains("PlanetLight") || _light.gameObject.name.Contains("Directional light"))
                {
                    mainMenuLight = _light;
                }
            }
        }
    }
}
