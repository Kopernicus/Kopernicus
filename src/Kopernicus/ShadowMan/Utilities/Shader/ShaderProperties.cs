//here we get the int for each shader property beforehand
//that way we don't do a string compare for lookup every frame (what Shader.SetFloat/Color/Matrix does)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kopernicus.ShadowMan
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class ShaderProperties : MonoBehaviour
    {
        //sorry about this huge wall
        //I wrote a script to take care of the whole thing
        //And by script I mean a regex

        public static int _experimentalAtmoScale_PROPERTY { get { return _experimentalAtmoScale; } }
        private static int _experimentalAtmoScale;
        public static int _viewdirOffset_PROPERTY { get { return _viewdirOffset; } }
        private static int _viewdirOffset;
        public static int atmosphereGlobalScale_PROPERTY { get { return atmosphereGlobalScale; } }
        private static int atmosphereGlobalScale;
        public static int Rg_PROPERTY { get { return Rg; } }
        private static int Rg;
        public static int Rt_PROPERTY { get { return Rt; } }
        private static int Rt;
        public static int RL_PROPERTY { get { return RL; } }
        private static int RL;
        public static int mieG_PROPERTY { get { return mieG; } }
        private static int mieG;
        public static int _Sun_Intensity_PROPERTY { get { return _Sun_Intensity; } }
        private static int _Sun_Intensity;
        public static int _Sun_WorldSunDir_PROPERTY { get { return _Sun_WorldSunDir; } }
        private static int _Sun_WorldSunDir;
        public static int _Godray_WorldSunDir_PROPERTY { get { return _Godray_WorldSunDir; } }
        private static int _Godray_WorldSunDir;
        public static int SUN_DIR_PROPERTY { get { return SUN_DIR; } }
        private static int SUN_DIR;
        public static int SUN_INTENSITY_PROPERTY { get { return SUN_INTENSITY; } }
        private static int SUN_INTENSITY;
        public static int _Globals_WorldToCamera_PROPERTY { get { return _Globals_WorldToCamera; } }
        private static int _Globals_WorldToCamera;
        public static int _Globals_CameraToWorld_PROPERTY { get { return _Globals_CameraToWorld; } }
        private static int _Globals_CameraToWorld;
        public static int _Globals_CameraToScreen_PROPERTY { get { return _Globals_CameraToScreen; } }
        private static int _Globals_CameraToScreen;
        public static int _Globals_ScreenToCamera_PROPERTY { get { return _Globals_ScreenToCamera; } }
        private static int _Globals_ScreenToCamera;
        public static int _Globals_WorldCameraPos_PROPERTY { get { return _Globals_WorldCameraPos; } }
        private static int _Globals_WorldCameraPos;
        public static int _ShadowMan_Origin_PROPERTY { get { return _ShadowMan_Origin; } }
        private static int _ShadowMan_Origin;
        public static int _Exposure_PROPERTY { get { return _Exposure; } }
        private static int _Exposure;
        public static int _ScatteringExposure_PROPERTY { get { return _ScatteringExposure; } }
        private static int _ScatteringExposure;
        public static int _RimExposure_PROPERTY { get { return _RimExposure; } }
        private static int _RimExposure;
        public static int lightOccluders1_PROPERTY { get { return lightOccluders1; } }
        private static int lightOccluders1;
        public static int lightOccluders2_PROPERTY { get { return lightOccluders2; } }
        private static int lightOccluders2;
        public static int sunPosAndRadius_PROPERTY { get { return sunPosAndRadius; } }
        private static int sunPosAndRadius;
        public static int extinctionGroundFade_PROPERTY { get { return extinctionGroundFade; } }
        private static int extinctionGroundFade;
        public static int _Alpha_Global_PROPERTY { get { return _Alpha_Global; } }
        private static int _Alpha_Global;
        public static int _Extinction_Tint_PROPERTY { get { return _Extinction_Tint; } }
        private static int _Extinction_Tint;
        public static int extinctionMultiplier_PROPERTY { get { return extinctionMultiplier; } }
        private static int extinctionMultiplier;
        public static int extinctionRimFade_PROPERTY { get { return extinctionRimFade; } }
        private static int extinctionRimFade;
        public static int _extinctionScatterIntensity_PROPERTY { get { return _extinctionScatterIntensity; } }
        private static int _extinctionScatterIntensity;
        public static int _global_alpha_PROPERTY { get { return _global_alpha; } }
        private static int _global_alpha;
        public static int _global_depth_PROPERTY { get { return _global_depth; } }
        private static int _global_depth;
        public static int _Post_Extinction_Tint_PROPERTY { get { return _Post_Extinction_Tint; } }
        private static int _Post_Extinction_Tint;
        public static int postExtinctionMultiplier_PROPERTY { get { return postExtinctionMultiplier; } }
        private static int postExtinctionMultiplier;
        public static int _openglThreshold_PROPERTY { get { return _openglThreshold; } }
        private static int _openglThreshold;
        public static int _ViewProjInv_PROPERTY { get { return _ViewProjInv; } }
        private static int _ViewProjInv;
        public static int _camPos_PROPERTY { get { return _camPos; } }
        private static int _camPos;
        public static int _Ocean_SunDir_PROPERTY { get { return _Ocean_SunDir; } }
        private static int _Ocean_SunDir;
        public static int _Ocean_CameraToOcean_PROPERTY { get { return _Ocean_CameraToOcean; } }
        private static int _Ocean_CameraToOcean;
        public static int _Ocean_OceanToCamera_PROPERTY { get { return _Ocean_OceanToCamera; } }
        private static int _Ocean_OceanToCamera;
        public static int _Globals_WorldToOcean_PROPERTY { get { return _Globals_WorldToOcean; } }
        private static int _Globals_WorldToOcean;
        public static int _Globals_OceanToWorld_PROPERTY { get { return _Globals_OceanToWorld; } }
        private static int _Globals_OceanToWorld;
        public static int _Ocean_CameraPos_PROPERTY { get { return _Ocean_CameraPos; } }
        private static int _Ocean_CameraPos;
        public static int _Ocean_Color_PROPERTY { get { return _Ocean_Color; } }
        private static int _Ocean_Color;
        public static int _Ocean_ScreenGridSize_PROPERTY { get { return _Ocean_ScreenGridSize; } }
        private static int _Ocean_ScreenGridSize;
        public static int _Ocean_Radius_PROPERTY { get { return _Ocean_Radius; } }
        private static int _Ocean_Radius;
        public static int _OceanAlpha_PROPERTY { get { return _OceanAlpha; } }
        private static int _OceanAlpha;
        public static int alphaRadius_PROPERTY { get { return alphaRadius; } }
        private static int alphaRadius;
        public static int sphereDir_PROPERTY { get { return sphereDir; } }
        private static int sphereDir;
        public static int cosTheta_PROPERTY { get { return cosTheta; } }
        private static int cosTheta;
        public static int sinTheta_PROPERTY { get { return sinTheta; } }
        private static int sinTheta;
        public static int _Ocean_MapSize_PROPERTY { get { return _Ocean_MapSize; } }
        private static int _Ocean_MapSize;
        public static int _Ocean_Choppyness_PROPERTY { get { return _Ocean_Choppyness; } }
        private static int _Ocean_Choppyness;
        public static int _Ocean_GridSizes_PROPERTY { get { return _Ocean_GridSizes; } }
        private static int _Ocean_GridSizes;
        public static int _Ocean_HeightOffset_PROPERTY { get { return _Ocean_HeightOffset; } }
        private static int _Ocean_HeightOffset;
        public static int _Ocean_Variance_PROPERTY { get { return _Ocean_Variance; } }
        private static int _Ocean_Variance;
        public static int _Ocean_Map0_PROPERTY { get { return _Ocean_Map0; } }
        private static int _Ocean_Map0;
        public static int _Ocean_Map1_PROPERTY { get { return _Ocean_Map1; } }
        private static int _Ocean_Map1;
        public static int _Ocean_Map2_PROPERTY { get { return _Ocean_Map2; } }
        private static int _Ocean_Map2;
        public static int _Ocean_Map3_PROPERTY { get { return _Ocean_Map3; } }
        private static int _Ocean_Map3;
        public static int _Ocean_Map4_PROPERTY { get { return _Ocean_Map4; } }
        private static int _Ocean_Map4;
        public static int _VarianceMax_PROPERTY { get { return _VarianceMax; } }
        private static int _VarianceMax;
        public static int _Map5_PROPERTY { get { return _Map5; } }
        private static int _Map5;
        public static int _Map6_PROPERTY { get { return _Map6; } }
        private static int _Map6;
        public static int _Map7_PROPERTY { get { return _Map7; } }
        private static int _Map7;
        public static int _Choppyness_PROPERTY { get { return _Choppyness; } }
        private static int _Choppyness;
        public static int _Ocean_WhiteCapStr_PROPERTY { get { return _Ocean_WhiteCapStr; } }
        private static int _Ocean_WhiteCapStr;
        public static int farWhiteCapStr_PROPERTY { get { return farWhiteCapStr; } }
        private static int farWhiteCapStr;
        public static int _Ocean_Foam0_PROPERTY { get { return _Ocean_Foam0; } }
        private static int _Ocean_Foam0;
        public static int _Ocean_Foam1_PROPERTY { get { return _Ocean_Foam1; } }
        private static int _Ocean_Foam1;
        public static int _Transmittance_PROPERTY { get { return _Transmittance; } }
        private static int _Transmittance;
        public static int _Inscatter_PROPERTY { get { return _Inscatter; } }
        private static int _Inscatter;
        public static int _Irradiance_PROPERTY { get { return _Irradiance; } }
        private static int _Irradiance;
        public static int _customDepthTexture_PROPERTY { get { return _customDepthTexture; } }
        private static int _customDepthTexture;
        public static int _godrayDepthTexture_PROPERTY { get { return _godrayDepthTexture; } }
        private static int _godrayDepthTexture;
        public static int M_PI_PROPERTY { get { return M_PI; } }
        private static int M_PI;
        public static int Rl_PROPERTY { get { return Rl; } }
        private static int Rl;
        public static int RES_R_PROPERTY { get { return RES_R; } }
        private static int RES_R;
        public static int RES_MU_PROPERTY { get { return RES_MU; } }
        private static int RES_MU;
        public static int RES_MU_S_PROPERTY { get { return RES_MU_S; } }
        private static int RES_MU_S;
        public static int RES_NU_PROPERTY { get { return RES_NU; } }
        private static int RES_NU;
        public static int SKY_W_PROPERTY { get { return SKY_W; } }
        private static int SKY_W;
        public static int SKY_H_PROPERTY { get { return SKY_H; } }
        private static int SKY_H;
        public static int betaMSca_PROPERTY { get { return betaMSca; } }
        private static int betaMSca;
        public static int betaMEx_PROPERTY { get { return betaMEx; } }
        private static int betaMEx;
        public static int HR_PROPERTY { get { return HR; } }
        private static int HR;
        public static int HM_PROPERTY { get { return HM; } }
        private static int HM;
        public static int betaR_PROPERTY { get { return betaR; } }
        private static int betaR;

        public static int TRANSMITTANCE_W_PROPERTY { get { return TRANSMITTANCE_W; } }
        private static int TRANSMITTANCE_W;

        public static int TRANSMITTANCE_H_PROPERTY { get { return TRANSMITTANCE_H; } }
        private static int TRANSMITTANCE_H;

        public static int AVERAGE_GROUND_REFLECTANCE_PROPERTY { get { return AVERAGE_GROUND_REFLECTANCE; } }
        private static int AVERAGE_GROUND_REFLECTANCE;

        public static int _Spectrum01_PROPERTY { get { return _Spectrum01; } }
        private static int _Spectrum01;
        public static int _Spectrum23_PROPERTY { get { return _Spectrum23; } }
        private static int _Spectrum23;
        public static int _WTable_PROPERTY { get { return _WTable; } }
        private static int _WTable;
        public static int _Offset_PROPERTY { get { return _Offset; } }
        private static int _Offset;
        public static int _InverseGridSizes_PROPERTY { get { return _InverseGridSizes; } }
        private static int _InverseGridSizes;

        public static int _T_PROPERTY { get { return _T; } }
        private static int _T;
        public static int _Buffer1_PROPERTY { get { return _Buffer1; } }
        private static int _Buffer1;
        public static int _Buffer2_PROPERTY { get { return _Buffer2; } }
        private static int _Buffer2;

        public static int _PlanetOpacity_PROPERTY { get { return _PlanetOpacity; } }
        private static int _PlanetOpacity;

        public static int extinctionThickness_PROPERTY { get { return extinctionThickness; } }
        private static int extinctionThickness;

        public static int _planetPos_PROPERTY { get { return _planetPos; } }
        private static int _planetPos;

        public static int planetShineSources_PROPERTY { get { return planetShineSources; } }
        private static int planetShineSources;

        public static int planetShineRGB_PROPERTY { get { return planetShineRGB; } }
        private static int planetShineRGB;

        public static int _Sky_Transmittance_PROPERTY { get { return _Sky_Transmittance; } }
        private static int _Sky_Transmittance;

        public static int ringInnerRadius_PROPERTY { get { return ringInnerRadius; } }
        private static int ringInnerRadius;

        public static int ringOuterRadius_PROPERTY { get { return ringOuterRadius; } }
        private static int ringOuterRadius;

        public static int ringNormal_PROPERTY { get { return ringNormal; } }
        private static int ringNormal;

        public static int ringTexture_PROPERTY { get { return ringTexture; } }
        private static int ringTexture;

        public static int cloudColorMultiplier_PROPERTY { get { return cloudColorMultiplier; } }
        private static int cloudColorMultiplier;

        public static int cloudScatteringMultiplier_PROPERTY { get { return cloudScatteringMultiplier; } }
        private static int cloudScatteringMultiplier;

        public static int cloudSkyIrradianceMultiplier_PROPERTY { get { return cloudSkyIrradianceMultiplier; } }
        private static int cloudSkyIrradianceMultiplier;

        public static int _PlanetWorldPos_PROPERTY { get { return _PlanetWorldPos; } }
        private static int _PlanetWorldPos;

        public static int extinctionTint_PROPERTY { get { return extinctionTint; } }
        private static int extinctionTint;

        public static int _SkyExposure_PROPERTY { get { return _SkyExposure; } }
        private static int _SkyExposure;

        public static int _camForward_PROPERTY { get { return _camForward; } }
        private static int _camForward;

        public static int _sunColor_PROPERTY { get { return _sunColor; } }
        private static int _sunColor;

        public static int _ShadowManCameraOverlap_PROPERTY { get { return _ShadowManCameraOverlap; } }
        private static int _ShadowManCameraOverlap;

        public static int flatScaledSpaceModel_PROPERTY { get { return flatScaledSpaceModel; } }
        private static int flatScaledSpaceModel;

        public static int renderSunFlare_PROPERTY { get { return renderSunFlare; } }
        private static int renderSunFlare;

        public static int sunViewPortPos_PROPERTY { get { return sunViewPortPos; } }
        private static int sunViewPortPos;

        public static int aspectRatio_PROPERTY { get { return aspectRatio; } }
        private static int aspectRatio;

        public static int sunGlareScale_PROPERTY { get { return sunGlareScale; } }
        private static int sunGlareScale;

        public static int sunGlareFade_PROPERTY { get { return sunGlareFade; } }
        private static int sunGlareFade;

        public static int ghost1Fade_PROPERTY { get { return ghost1Fade; } }
        private static int ghost1Fade;

        public static int ghost2Fade_PROPERTY { get { return ghost2Fade; } }
        private static int ghost2Fade;

        public static int ghost3Fade_PROPERTY { get { return ghost3Fade; } }
        private static int ghost3Fade;

        public static int _ButterFlyLookUp_PROPERTY { get { return _ButterFlyLookUp; } }
        private static int _ButterFlyLookUp;

        public static int _ReadBuffer0_PROPERTY { get { return _ReadBuffer0; } }
        private static int _ReadBuffer0;

        public static int _ReadBuffer1_PROPERTY { get { return _ReadBuffer1; } }
        private static int _ReadBuffer1;

        public static int _MainTex_PROPERTY { get { return _MainTex; } }
        private static int _MainTex;

        public static int _ZwriteVariable_PROPERTY { get { return _ZwriteVariable; } }
        private static int _ZwriteVariable;

        public static int warpTime_PROPERTY { get { return warpTime; } }
        private static int warpTime;

        public static int renderOnCurrentCamera_PROPERTY { get { return renderOnCurrentCamera; } }
        private static int renderOnCurrentCamera;

        public static int useDbufferOnCamera_PROPERTY { get { return useDbufferOnCamera; } }
        private static int useDbufferOnCamera;

        public static int shoreFoam_PROPERTY { get { return shoreFoam; } }
        private static int shoreFoam;

        public static int CameraToWorld_PROPERTY { get { return CameraToWorld; } }
        private static int CameraToWorld;

        public static int WorldToLight_PROPERTY { get { return WorldToLight; } }
        private static int WorldToLight;

        public static int LightDir_PROPERTY { get { return LightDir; } }
        private static int LightDir;

        public static int PlanetOrigin_PROPERTY { get { return PlanetOrigin; } }
        private static int PlanetOrigin ;

        public static int refractionIndex_PROPERTY { get { return refractionIndex; } }
        private static int refractionIndex;

        public static int scattererOrigDirectionalColor_PROPERTY { get { return scattererOrigDirectionalColor; } }
        private static int scattererOrigDirectionalColor;

        public static int unity_ShadowFadeCenterAndType_PROPERTY { get { return unity_ShadowFadeCenterAndType; } }
        private static int unity_ShadowFadeCenterAndType;

        public static int _ShadowMapTextureShadowMan_PROPERTY { get { return _ShadowMapTextureShadowMan; } }
        private static int _ShadowMapTextureShadowMan;

        public static int _ShadowMapTextureCopyShadowMan_PROPERTY { get { return _ShadowMapTextureCopyShadowMan; } }
        private static int _ShadowMapTextureCopyShadowMan;

        public static int ShadowManAdditionalInvProjection_PROPERTY { get { return ShadowManAdditionalInvProjection; } }
        private static int ShadowManAdditionalInvProjection;

        public static int AdditionalDepthBuffer_PROPERTY { get { return AdditionalDepthBuffer; } }
        private static int AdditionalDepthBuffer;

        public static int lightDirection_PROPERTY { get { return lightDirection; } }
        private static int lightDirection;

        public static int _godrayCloudThreshold_PROPERTY { get { return _godrayCloudThreshold; } }
        private static int _godrayCloudThreshold;

        public static int lightToWorld_PROPERTY { get { return lightToWorld; } }
        private static int lightToWorld;

        public static int _godrayStrength_PROPERTY { get { return _godrayStrength; } }
        private static int _godrayStrength;

        private void Awake()
        {
            _experimentalAtmoScale = Shader.PropertyToID("_experimentalAtmoScale");
            _viewdirOffset = Shader.PropertyToID("_viewdirOffset");
            atmosphereGlobalScale = Shader.PropertyToID("atmosphereGlobalScale");
            Rg = Shader.PropertyToID("Rg");
            Rt = Shader.PropertyToID("Rt");
            RL = Shader.PropertyToID("RL");
            mieG = Shader.PropertyToID("mieG");
            _Sun_Intensity = Shader.PropertyToID("_Sun_Intensity");
            _Sun_WorldSunDir = Shader.PropertyToID("_Sun_WorldSunDir");
            _Godray_WorldSunDir = Shader.PropertyToID("_Godray_WorldSunDir");
            SUN_DIR = Shader.PropertyToID("SUN_DIR");
            SUN_INTENSITY = Shader.PropertyToID("SUN_INTENSITY");
            _Globals_WorldToCamera = Shader.PropertyToID("_Globals_WorldToCamera");
            _Globals_CameraToWorld = Shader.PropertyToID("_Globals_CameraToWorld");
            _Globals_CameraToScreen = Shader.PropertyToID("_Globals_CameraToScreen");
            _Globals_ScreenToCamera = Shader.PropertyToID("_Globals_ScreenToCamera");
            _Globals_WorldCameraPos = Shader.PropertyToID("_Globals_WorldCameraPos");
            _ShadowMan_Origin = Shader.PropertyToID("_ShadowMan_Origin");
            _Exposure = Shader.PropertyToID("_Exposure");
            _ScatteringExposure = Shader.PropertyToID("_ScatteringExposure");
            _RimExposure = Shader.PropertyToID("_RimExposure");
            lightOccluders1 = Shader.PropertyToID("lightOccluders1");
            lightOccluders2 = Shader.PropertyToID("lightOccluders2");
            sunPosAndRadius = Shader.PropertyToID("sunPosAndRadius");
            extinctionGroundFade = Shader.PropertyToID("extinctionGroundFade");
            _Alpha_Global = Shader.PropertyToID("_Alpha_Global");
            _Extinction_Tint = Shader.PropertyToID("_Extinction_Tint");
            extinctionMultiplier = Shader.PropertyToID("extinctionMultiplier");
            extinctionRimFade = Shader.PropertyToID("extinctionRimFade");
            _extinctionScatterIntensity = Shader.PropertyToID("_extinctionScatterIntensity");
            _global_alpha = Shader.PropertyToID("_global_alpha");
            _global_depth = Shader.PropertyToID("_global_depth");
            _Post_Extinction_Tint = Shader.PropertyToID("_Post_Extinction_Tint");
            postExtinctionMultiplier = Shader.PropertyToID("postExtinctionMultiplier");
            _openglThreshold = Shader.PropertyToID("_openglThreshold");
            _ViewProjInv = Shader.PropertyToID("_ViewProjInv");
            _camPos = Shader.PropertyToID("_camPos");
            _Ocean_SunDir = Shader.PropertyToID("_Ocean_SunDir");
            _Ocean_CameraToOcean = Shader.PropertyToID("_Ocean_CameraToOcean");
            _Ocean_OceanToCamera = Shader.PropertyToID("_Ocean_OceanToCamera");
            _Globals_WorldToOcean = Shader.PropertyToID("_Globals_WorldToOcean");
            _Globals_OceanToWorld = Shader.PropertyToID("_Globals_OceanToWorld");
            _Ocean_CameraPos = Shader.PropertyToID("_Ocean_CameraPos");
            _Ocean_Color = Shader.PropertyToID("_Ocean_Color");
            _Ocean_ScreenGridSize = Shader.PropertyToID("_Ocean_ScreenGridSize");
            _Ocean_Radius = Shader.PropertyToID("_Ocean_Radius");
            _OceanAlpha = Shader.PropertyToID("_OceanAlpha");
            alphaRadius = Shader.PropertyToID("alphaRadius");
            sphereDir = Shader.PropertyToID("sphereDir");
            cosTheta = Shader.PropertyToID("cosTheta");
            sinTheta = Shader.PropertyToID("sinTheta");
            _Ocean_MapSize = Shader.PropertyToID("_Ocean_MapSize");
            _Ocean_Choppyness = Shader.PropertyToID("_Ocean_Choppyness");
            _Ocean_GridSizes = Shader.PropertyToID("_Ocean_GridSizes");
            _Ocean_HeightOffset = Shader.PropertyToID("_Ocean_HeightOffset");
            _Ocean_Variance = Shader.PropertyToID("_Ocean_Variance");
            _Ocean_Map0 = Shader.PropertyToID("_Ocean_Map0");
            _Ocean_Map1 = Shader.PropertyToID("_Ocean_Map1");
            _Ocean_Map2 = Shader.PropertyToID("_Ocean_Map2");
            _Ocean_Map3 = Shader.PropertyToID("_Ocean_Map3");
            _Ocean_Map4 = Shader.PropertyToID("_Ocean_Map4");
            _VarianceMax = Shader.PropertyToID("_VarianceMax");
            _Map5 = Shader.PropertyToID("_Map5");
            _Map6 = Shader.PropertyToID("_Map6");
            _Map7 = Shader.PropertyToID("_Map7");
            _Choppyness = Shader.PropertyToID("_Choppyness");
            _Ocean_WhiteCapStr = Shader.PropertyToID("_Ocean_WhiteCapStr");
            farWhiteCapStr = Shader.PropertyToID("farWhiteCapStr");
            _Ocean_Foam0 = Shader.PropertyToID("_Ocean_Foam0");
            _Ocean_Foam1 = Shader.PropertyToID("_Ocean_Foam1");

            _Transmittance = Shader.PropertyToID("_Transmittance");
            _Inscatter = Shader.PropertyToID("_Inscatter");
            _Irradiance = Shader.PropertyToID("_Irradiance");
            _customDepthTexture = Shader.PropertyToID("_customDepthTexture");
            _godrayDepthTexture = Shader.PropertyToID("_godrayDepthTexture");

            M_PI = Shader.PropertyToID("M_PI");
            Rl = Shader.PropertyToID("Rl");
            RES_R = Shader.PropertyToID("RES_R");
            RES_MU = Shader.PropertyToID("RES_MU");
            RES_MU_S = Shader.PropertyToID("RES_MU_S");
            RES_NU = Shader.PropertyToID("RES_NU");
            SKY_W = Shader.PropertyToID("SKY_W");
            SKY_H = Shader.PropertyToID("SKY_H");
            betaMSca = Shader.PropertyToID("betaMSca");
            betaMEx = Shader.PropertyToID("betaMEx");
            HR = Shader.PropertyToID("HR");
            HM = Shader.PropertyToID("HM");
            betaR = Shader.PropertyToID("betaR");

            TRANSMITTANCE_W = Shader.PropertyToID("TRANSMITTANCE_W");
            TRANSMITTANCE_H = Shader.PropertyToID("TRANSMITTANCE_H");
            AVERAGE_GROUND_REFLECTANCE = Shader.PropertyToID("AVERAGE_GROUND_REFLECTANCE");

            _Spectrum01 = Shader.PropertyToID("_Spectrum01");
            _Spectrum23 = Shader.PropertyToID("_Spectrum23");
            _WTable = Shader.PropertyToID("_WTable");
            _Offset = Shader.PropertyToID("_Offset");
            _InverseGridSizes = Shader.PropertyToID("_InverseGridSizes");
            _InverseGridSizes = Shader.PropertyToID("_InverseGridSizes");

            _T = Shader.PropertyToID("_T");
            _Buffer1 = Shader.PropertyToID("_Buffer1");
            _Buffer2 = Shader.PropertyToID("_Buffer2");

            _PlanetOpacity = Shader.PropertyToID("_PlanetOpacity");
            extinctionThickness = Shader.PropertyToID("extinctionThickness");
            _planetPos = Shader.PropertyToID("_planetPos");
            planetShineSources = Shader.PropertyToID("planetShineSources");
            planetShineRGB = Shader.PropertyToID("planetShineRGB");
            _Sky_Transmittance = Shader.PropertyToID("_Sky_Transmittance");
            ringInnerRadius = Shader.PropertyToID("ringInnerRadius");
            ringOuterRadius = Shader.PropertyToID("ringOuterRadius");
            ringNormal = Shader.PropertyToID("ringNormal");
            ringTexture = Shader.PropertyToID("ringTexture");
            cloudColorMultiplier = Shader.PropertyToID("cloudColorMultiplier");
            cloudScatteringMultiplier = Shader.PropertyToID("cloudScatteringMultiplier");
            cloudSkyIrradianceMultiplier = Shader.PropertyToID("cloudSkyIrradianceMultiplier");
            _PlanetWorldPos = Shader.PropertyToID("_PlanetWorldPos");
            extinctionTint = Shader.PropertyToID("extinctionTint");
            _SkyExposure = Shader.PropertyToID("_SkyExposure");
            _camForward = Shader.PropertyToID("_camForward");
            _sunColor = Shader.PropertyToID("_sunColor");
            _ShadowManCameraOverlap = Shader.PropertyToID("_ShadowManCameraOverlap");
            flatScaledSpaceModel = Shader.PropertyToID("flatScaledSpaceModel");

            renderSunFlare = Shader.PropertyToID("renderSunFlare");
            sunViewPortPos = Shader.PropertyToID("sunViewPortPos");
            aspectRatio = Shader.PropertyToID("aspectRatio");
            sunGlareScale = Shader.PropertyToID("sunGlareScale");
            sunGlareFade = Shader.PropertyToID("sunGlareFade");
            ghost1Fade = Shader.PropertyToID("ghost1Fade");
            ghost2Fade = Shader.PropertyToID("ghost2Fade");
            ghost3Fade = Shader.PropertyToID("ghost3Fade");
            _ButterFlyLookUp = Shader.PropertyToID("_ButterFlyLookUp");
            _ReadBuffer0 = Shader.PropertyToID("_ReadBuffer0");
            _ReadBuffer1 = Shader.PropertyToID("_ReadBuffer1");
            _MainTex = Shader.PropertyToID("_MainTex");
            _ZwriteVariable = Shader.PropertyToID("_ZwriteVariable");
            warpTime = Shader.PropertyToID("warpTime");
            renderOnCurrentCamera = Shader.PropertyToID("renderOnCurrentCamera");
            useDbufferOnCamera = Shader.PropertyToID("useDbufferOnCamera");
            shoreFoam = Shader.PropertyToID("shoreFoam");

            CameraToWorld = Shader.PropertyToID("CameraToWorld");
            WorldToLight = Shader.PropertyToID("WorldToLight");
            LightDir = Shader.PropertyToID("LightDir");
            PlanetOrigin = Shader.PropertyToID("PlanetOrigin");
            refractionIndex = Shader.PropertyToID("refractionIndex");

            scattererOrigDirectionalColor = Shader.PropertyToID("scattererOrigDirectionalColor");
            unity_ShadowFadeCenterAndType = Shader.PropertyToID("unity_ShadowFadeCenterAndType");
            _ShadowMapTextureShadowMan = Shader.PropertyToID("_ShadowMapTextureShadowMan");

            _ShadowMapTextureCopyShadowMan = Shader.PropertyToID("_ShadowMapTextureCopyShadowMan");

            ShadowManAdditionalInvProjection = Shader.PropertyToID("ShadowManAdditionalInvProjection");
            AdditionalDepthBuffer = Shader.PropertyToID("AdditionalDepthBuffer");

            lightDirection = Shader.PropertyToID("lightDirection");
            _godrayCloudThreshold = Shader.PropertyToID("_godrayCloudThreshold");

            lightToWorld = Shader.PropertyToID("lightToWorld");

            _godrayStrength = Shader.PropertyToID("_godrayStrength");
        }
    }
}