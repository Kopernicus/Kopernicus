// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Essentially a modified version of the unity screenSpace shadow collector shader
// 1) Always use INV_PROJECTION because it offers better precision than the ray method when far/near ratio is very high
// 2) Add a very small receiverPlaneDepthBias based on z value, to avoid shadow acne, jittering and other artifacts

UNITY_DECLARE_SHADOWMAP(_ShadowMapTexture);
float4 _ShadowMapTexture_TexelSize;
#define SHADOWMAPSAMPLER_AND_TEXELSIZE_DEFINED
sampler2D _ODSWorldTexture;

// ------------------------------------------------------------------
//  Helpers
// ------------------------------------------------------------------
UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

#include "../ShadowsCommon.cginc"

struct appdata {
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
#ifdef UNITY_STEREO_INSTANCING_ENABLED
    float3 ray0 : TEXCOORD1;
    float3 ray1 : TEXCOORD2;
#else
    float3 ray : TEXCOORD1;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {

    float4 pos : SV_POSITION;

    // xy uv / zw screenpos
    float4 uv : TEXCOORD0;
    // View space ray, for perspective case
    float3 ray : TEXCOORD1;
    // Orthographic view space positions (need xy as well for oblique matrices)
    float3 orthoPosNear : TEXCOORD2;
    float3 orthoPosFar  : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert (appdata v)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    float4 clipPos;
#if defined(STEREO_CUBEMAP_RENDER_ON)
    clipPos = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, v.vertex));
#else
    clipPos = UnityObjectToClipPos(v.vertex);
#endif
    o.pos = clipPos;
    o.uv.xy = v.texcoord;

    // unity_CameraInvProjection at the PS level.
    o.uv.zw = ComputeNonStereoScreenPos(clipPos);

    // Perspective case
#ifdef UNITY_STEREO_INSTANCING_ENABLED
    o.ray = unity_StereoEyeIndex == 0 ? v.ray0 : v.ray1;
#else
    o.ray = v.ray;
#endif

    // To compute view space position from Z buffer for orthographic case,
    // we need different code than for perspective case. We want to avoid
    // doing matrix multiply in the pixel shader: less operations, and less
    // constant registers used. Particularly with constant registers, having
    // unity_CameraInvProjection in the pixel shader would push the PS over SM2.0
    // limits.
    clipPos.y *= _ProjectionParams.x;
    float3 orthoPosNear = mul(unity_CameraInvProjection, float4(clipPos.x,clipPos.y,-1,1)).xyz;
    float3 orthoPosFar  = mul(unity_CameraInvProjection, float4(clipPos.x,clipPos.y, 1,1)).xyz;
    orthoPosNear.z *= -1;
    orthoPosFar.z *= -1;
    o.orthoPosNear = orthoPosNear;
    o.orthoPosFar = orthoPosFar;

    return o;
}

/**
* Get camera space coord from depth and inv projection matrices
*/
inline float3 computeCameraSpacePosFromDepthAndInvProjMat(v2f i)
{
    float zdepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);

    #if defined(UNITY_REVERSED_Z)
        zdepth = 1 - zdepth;
    #endif

    // View position calculation for oblique clipped projection case.
    // this will not be as precise nor as fast as the other method
    // (which computes it from interpolated ray & depth) but will work
    // with funky projections.
    float4 clipPos = float4(i.uv.zw, zdepth, 1.0);
    clipPos.xyz = 2.0f * clipPos.xyz - 1.0f;
    float4 camPos = mul(unity_CameraInvProjection, clipPos);
    camPos.xyz /= camPos.w;
    camPos.z *= -1;
    return camPos.xyz;
}

/**
* Get camera space coord from depth and info from VS
*/
inline float3 computeCameraSpacePosFromDepthAndVSInfo(v2f i)
{
    float zdepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);

    // 0..1 linear depth, 0 at camera, 1 at far plane.
    float depth = Linear01Depth(zdepth);

#if defined(UNITY_REVERSED_Z)
    zdepth = 1 - zdepth;
#endif
    float3 adjustedRay = i.ray;

    float3 vposPersp = adjustedRay * depth;

    float3 camPos = vposPersp;
    return camPos.xyz;
}

inline float3 computeCameraSpacePosFromDepth(v2f i);

/**
 *  Hard shadow
 */
fixed4 frag_hard (v2f i) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); // required for sampling the correct slice of the shadow map render texture array
    float4 wpos;
    float3 vpos;

#if defined(STEREO_CUBEMAP_RENDER_ON)
    wpos.xyz = tex2D(_ODSWorldTexture, i.uv.xy).xyz;
    wpos.w = 1.0f;
    vpos = mul(unity_WorldToCamera, wpos).xyz;
#else
    vpos = computeCameraSpacePosFromDepth(i);
    wpos = mul (unity_CameraToWorld, float4(vpos,1));
#endif
    fixed4 cascadeWeights = GET_CASCADE_WEIGHTS (wpos, vpos.z);
    float4 shadowCoord = GET_SHADOW_COORDINATES(wpos, cascadeWeights);

    //1 tap hard shadow
    fixed shadow = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, shadowCoord);
    shadow = lerp(_LightShadowData.r, 1.0, shadow);

    fixed4 res = shadow;
    return res;
}

/**
 *  Soft Shadow (SM 3.0)
 */
fixed4 frag_pcfSoft(v2f i) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); // required for sampling the correct slice of the shadow map render texture array
    float4 wpos;
    float3 vpos;

#if defined(STEREO_CUBEMAP_RENDER_ON)
    wpos.xyz = tex2D(_ODSWorldTexture, i.uv.xy).xyz;
    wpos.w = 1.0f;
    vpos = mul(unity_WorldToCamera, wpos).xyz;
#else
    vpos = computeCameraSpacePosFromDepth(i);

    // sample the cascade the pixel belongs to
    wpos = mul(unity_CameraToWorld, float4(vpos,1));
#endif
    fixed4 cascadeWeights = GET_CASCADE_WEIGHTS(wpos, vpos.z);
    float4 coord = GET_SHADOW_COORDINATES(wpos, cascadeWeights);
    	
    float zdepth2 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);


//    float3 receiverPlaneDepthBias = float3(0.0,0.0,0.5*Linear01Depth(zdepth2));	//my best or worst hack yet?
											//seems to fix all issues, no peter-panning either
											//perfect for 5000 shadowDistance
											//Holds up to 8000-9000 shadow distance and gets swimmy after

     float3 receiverPlaneDepthBias = float3(0.0,0.0,2.0 * Linear01Depth(zdepth2));	//With the new hybrid reconstruction method this isn't needed anymore but what Parallax renders to depth buffer is slightly offset so use agressive bias

#ifdef UNITY_USE_RECEIVER_PLANE_BIAS
    // Reveiver plane depth bias: need to calculate it based on shadow coordinate
    // as it would be in first cascade; otherwise derivatives
    // at cascade boundaries will be all wrong. So compute
    // it from cascade 0 UV, and scale based on which cascade we're in.
    float3 coordCascade0 = getShadowCoord_SingleCascade(wpos);
    float biasMultiply = dot(cascadeWeights,unity_ShadowCascadeScales);
    receiverPlaneDepthBias = UnityGetReceiverPlaneDepthBias(coordCascade0.xyz, biasMultiply);
#endif

#if defined(SHADER_API_MOBILE)
    half shadow = UnitySampleShadowmap_PCF5x5(coord, receiverPlaneDepthBias);
#else
    half shadow = UnitySampleShadowmap_PCF7x7(coord, receiverPlaneDepthBias);
#endif
    shadow = lerp(_LightShadowData.r, 1.0f, shadow);

    // Blend between shadow cascades if enabled
    //
    // Not working yet with split spheres, and no need when 1 cascade
#if UNITY_USE_CASCADE_BLENDING && !defined(SHADOWS_SPLIT_SPHERES) && !defined(SHADOWS_SINGLE_CASCADE)
    half4 z4 = (float4(vpos.z,vpos.z,vpos.z,vpos.z) - _LightSplitsNear) / (_LightSplitsFar - _LightSplitsNear);
    half alpha = dot(z4 * cascadeWeights, half4(1,1,1,1));

    UNITY_BRANCH
        if (alpha > 1 - UNITY_CASCADE_BLEND_DISTANCE)
        {
            // get alpha to 0..1 range over the blend distance
            alpha = (alpha - (1 - UNITY_CASCADE_BLEND_DISTANCE)) / UNITY_CASCADE_BLEND_DISTANCE;

            // sample next cascade
            cascadeWeights = fixed4(0, cascadeWeights.xyz);
            coord = GET_SHADOW_COORDINATES(wpos, cascadeWeights);

#ifdef UNITY_USE_RECEIVER_PLANE_BIAS
            biasMultiply = dot(cascadeWeights,unity_ShadowCascadeScales);
            receiverPlaneDepthBias = UnityGetReceiverPlaneDepthBias(coordCascade0.xyz, biasMultiply);
#endif

            half shadowNextCascade = UnitySampleShadowmap_PCF3x3(coord, receiverPlaneDepthBias);
            shadowNextCascade = lerp(_LightShadowData.r, 1.0f, shadowNextCascade);
            shadow = lerp(shadow, shadowNextCascade, alpha);
        }
#endif

    return shadow;
}