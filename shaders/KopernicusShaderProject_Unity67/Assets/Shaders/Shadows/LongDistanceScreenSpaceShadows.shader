// Same as fixedScreenSpaceShadows shader but uses the enhanced ray/projection method to calculate accurate position
// Some other experimentes are kept in this file but no longer used: Including dual-depth method, binary search method, double precision method
Shader "Scatterer/customScreenSpaceShadows" {
Properties {
    _ShadowMapTexture ("", any) = "" {}
    _ODSWorldTexture("", 2D) = "" {}
}

CGINCLUDE
#include "FixedScreenSpaceShadows.cginc"
#include "DoublePrecisionEmulation.cginc"

UNITY_DECLARE_DEPTH_TEXTURE(AdditionalDepthBuffer);
float4x4  ScattererAdditionalInvProjection;

/**
* Get camera space coord from depth and inv projection matrices
* No longer needed, deprecated in favor of hybrid ray/matrix method below
*/
inline float3 computeCameraSpacePosFromDualDepthAndInvProjMat(v2f i)
{
    float zdepth  = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);
    float zdepth2 = SAMPLE_DEPTH_TEXTURE(AdditionalDepthBuffer, i.uv.xy);

    #if defined(UNITY_REVERSED_Z)
        zdepth  = 1 - zdepth;
        zdepth2 = 1 - zdepth2;
    #endif

    float4 clipPos = float4(i.uv.zw, zdepth, 1.0);
    clipPos.xyz = 2.0f * clipPos.xyz - 1.0f;
    float4 camPos = mul(unity_CameraInvProjection, clipPos);
    camPos.xyz /= camPos.w;
    camPos.z *= -1;

    float4 clipPos2 = float4(i.uv.zw, zdepth2, 1.0);
    clipPos2.xyz = 2.0f * clipPos2.xyz - 1.0f;
    float4 camPos2 = mul(ScattererAdditionalInvProjection, clipPos2);
    camPos2.xyz /= camPos2.w;
    camPos2.z *= -1;

    return length(camPos.xyz) < 8000 ? camPos.xyz : camPos2.xyz ;
}

//Refines the inaccurate worldPos from invprojection with a search algorithm
//Apparently overshoots here even though it worked well for scattering shader
float getRefinedDistanceFromDepth(float unrefinedDistance, float zdepth, float3 viewDir)
{
	const int maxIterations = 30; //seems about perfect
	int iteration = 0;

	float maxSearchDistance = unrefinedDistance * 1.30;
	float minSearchDistance = unrefinedDistance * 0.70;

	float mid = 0;
	float3 camPos0 = float3(0.0,0.0,0.0);
	float4 clipPos = float4(0.0,0.0,0.0,1.0);
	float depth = -10.0;

	while ((iteration < maxIterations) && (depth != zdepth))
	{
		mid = 0.5 * (maxSearchDistance + minSearchDistance);

		camPos0 =  viewDir * mid;

		clipPos = mul(UNITY_MATRIX_P, float4(camPos0,1.0));
		depth = clipPos.z/clipPos.w;

		maxSearchDistance = (depth < zdepth) ? mid : maxSearchDistance;
		minSearchDistance = (depth > zdepth) ? mid : minSearchDistance;

		iteration++;
	}

	return mid;
}

// Binary search method, worked well for the scattering shader but seems to overshoot here
inline float3 computeEnhancedCameraSpacePosFromDepthAndInvProjMat(v2f i)
{
	float textureZdepth  = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);

	float zdepth = textureZdepth;
	
	#if defined(UNITY_REVERSED_Z)
	zdepth  = 1 - zdepth;
	#endif

	float4 clipPos = float4(i.uv.zw, zdepth, 1.0);
	clipPos.xyz = 2.0f * clipPos.xyz - 1.0f;
	float4 camPos = mul(unity_CameraInvProjection, clipPos);
	camPos.xyz /= camPos.w;
	camPos.z *= -1;

	float invDepthLength = length(camPos.xyz);
	float3 viewDir = camPos.xyz / invDepthLength;

	//now refine the inaccurate distance
	float distance = invDepthLength;

//	if (distance > 8000.0)
	{
		distance = getRefinedDistanceFromDepth(distance, textureZdepth, viewDir); //seems like my method is overshooting actually, or something else is wrong with these params, to investigate, maybe the projection matrix, maybe the direction is flipped
	}
			
	camPos.xyz = distance * viewDir;

	return camPos.xyz;
}

//after implementing this it seems like this isn't even the source of the issue, but rather the ray direction that unity passes, good job unity wtf
inline float2 Linear01DepthDoublePrecision( float z )
{
	//return 1.0 / (_ZBufferParams.x * z + _ZBufferParams.y);

	float2 doubleZBufferParamsY = ds_sub(ds_set(1.0), ds_div(ds_set(_ProjectionParams.y), ds_set(_ProjectionParams.z)));
	float2 doubleWhateverElse   = ds_mul(ds_div(ds_set(_ProjectionParams.z), ds_set(_ProjectionParams.y)), ds_set(z));

	float2 doubleResultLower = ds_add(doubleZBufferParamsY, doubleWhateverElse);
	float2 doubleResult = ds_div(ds_set(1.0), doubleResultLower);
	return doubleResult;
}

//This is the same ray method but calculations are done in emulated double precision
//This ended up being useless, the source of the problem is the ray direction passed by unity
inline float3 computeCameraSpacePosFromDepthAndVSInfoDoublePrecision(v2f i)
{
	float zdepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);

	// 0..1 linear depth, 0 at camera, 1 at far plane.
	float2 doublePrecisionDepth = Linear01DepthDoublePrecision(zdepth);

	float3 adjustedRay = i.ray;
	float3 rayDirection = normalize(i.ray);

	float3 vposPersp = adjustedRay * doublePrecisionDepth.x;

	return vposPersp;
}

/**
* Use the ray method to find position, except use the ray direction calculated with the invProj method because it's more accurate
* Kind of a hybrid of the two methods
*/
inline float3 computeCameraSpacePosFromLinearDepthAndInvProjMat(v2f i)
{
	float zdepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);

#if SHADER_API_D3D11 || SHADER_API_D3D || SHADER_API_D3D12
	if (zdepth == 0.0) {discard;}
#else
	if (zdepth == 1.0) {discard;}
#endif

	float depth = Linear01Depth(zdepth);

	//float3 rayDirection = normalize(i.ray); //this ray is imprecise and can't be trusted
						  //calculate our own ray direction using the inverse projection matrix method

#if defined(UNITY_REVERSED_Z)
	zdepth = 1 - zdepth;
#endif

	float4 clipPos = float4(i.uv.zw, zdepth, 1.0);
	clipPos.xyz = 2.0f * clipPos.xyz - 1.0f;
	float4 camPos = mul(unity_CameraInvProjection, clipPos);
	camPos.xyz /= camPos.w;
	camPos.z *= -1;

	float3 rayDirection = normalize(camPos.xyz);

	float3 cameraForwardDir = float3(0,0,1);
	float aa = dot(rayDirection, cameraForwardDir);

	float3 vposPersp = rayDirection * depth/aa * _ProjectionParams.z;
	return vposPersp;
}

ENDCG


// ----------------------------------------------------------------------------------------
// Subshader for hard shadows:
// Just collect shadows into the buffer. Used on pre-SM3 GPUs and when hard shadows are picked.
// This version does inv projection at the PS level, slower and less precise however more general.

SubShader {
      Tags{ "ShadowmapFilter" = "HardShadow" }
    Pass{
        ZWrite Off ZTest Always Cull Off

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag_hard
        #pragma multi_compile_shadowcollector

        inline float3 computeCameraSpacePosFromDepth(v2f i)
        {
		return computeCameraSpacePosFromLinearDepthAndInvProjMat(i);
        }
        ENDCG
    }
}

// ----------------------------------------------------------------------------------------
// Subshader that does soft PCF filtering while collecting shadows.
// Requires SM3 GPU.
// This version does inv projection at the PS level, slower and less precise however more general.
// 

Subshader{
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
		return computeCameraSpacePosFromLinearDepthAndInvProjMat(i);
        }

        ENDCG
    }
}

Fallback Off
}