// This shader is now deprecated as the long distance version is now better in every way without adding a performance hit

// Essentially a modified version of the unity screenSpace shadow collector shader
// 1) Always use INV_PROJECTION because it offers better precision than the ray method when far/near ratio is very high
// 2) Add a very small receiverPlaneDepthBias based on z value, to avoid shadow acne, jittering and other artifacts
Shader "Scatterer/fixedScreenSpaceShadows" {
Properties {
    _ShadowMapTexture ("", any) = "" {}
    _ODSWorldTexture("", 2D) = "" {}
}

CGINCLUDE
#include "FixedScreenSpaceShadows.cginc"
ENDCG

//// ----------------------------------------------------------------------------------------
//// Subshader for hard shadows:
//// Just collect shadows into the buffer. Used on pre-SM3 GPUs and when hard shadows are picked.
//
//SubShader {
//    Tags{ "ShadowmapFilter" = "HardShadow" }
//    Pass {
//        ZWrite Off ZTest Always Cull Off
//
//        CGPROGRAM
//        #pragma vertex vert
//        #pragma fragment frag_hard
//        #pragma multi_compile_shadowcollector
//
//        inline float3 computeCameraSpacePosFromDepth(v2f i)
//        {
//            return computeCameraSpacePosFromDepthAndVSInfo(i);
//        }
//        ENDCG
//    }
//}

// ----------------------------------------------------------------------------------------
// Subshader for hard shadows:
// Just collect shadows into the buffer. Used on pre-SM3 GPUs and when hard shadows are picked.
// This version does inv projection at the PS level, slower and less precise however more general.

SubShader {
//    Tags{ "ShadowmapFilter" = "HardShadow_FORCE_INV_PROJECTION_IN_PS" }
      Tags{ "ShadowmapFilter" = "HardShadow" }
    Pass{
        ZWrite Off ZTest Always Cull Off

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag_hard
        #pragma multi_compile_shadowcollector

        inline float3 computeCameraSpacePosFromDepth(v2f i)
        {
            return computeCameraSpacePosFromDepthAndInvProjMat(i);
        }
        ENDCG
    }
}

//// ----------------------------------------------------------------------------------------
//// Subshader that does soft PCF filtering while collecting shadows.
//// Requires SM3 GPU.
//
//Subshader {
//    Tags {"ShadowmapFilter" = "PCF_SOFT"}
//    Pass {
//        ZWrite Off ZTest Always Cull Off
//
//        CGPROGRAM
//        #pragma vertex vert
//        #pragma fragment frag_pcfSoft
//        #pragma multi_compile_shadowcollector
//        #pragma target 3.0
//
//        inline float3 computeCameraSpacePosFromDepth(v2f i)
//        {
//            return computeCameraSpacePosFromDepthAndVSInfo(i);
//        }
//        ENDCG
//    }
//}

// ----------------------------------------------------------------------------------------
// Subshader that does soft PCF filtering while collecting shadows.
// Requires SM3 GPU.
// This version does inv projection at the PS level, slower and less precise however more general.
// 

Subshader{
    //Tags{ "ShadowmapFilter" = "PCF_SOFT_FORCE_INV_PROJECTION_IN_PS" }
    Tags {"ShadowmapFilter" = "PCF_SOFT"}
    Pass{
        ZWrite Off ZTest Always Cull Off

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag_pcfSoft
        #pragma multi_compile_shadowcollector
        #pragma target 3.0

        inline float3 computeCameraSpacePosFromDepth(v2f i)
        {
            return computeCameraSpacePosFromDepthAndInvProjMat(i);
        }
        ENDCG
    }
}

Fallback Off
}