UNITY_DECLARE_SHADOWMAP(_ShadowMapTextureScatterer);
float4 _ShadowMapTextureScatterer_TexelSize;

#if SHADER_API_D3D11
	#define SHADOWS_SPLIT_SPHERES
#endif

#include "../ShadowsCommon.cginc"

/**
* PCF tent shadowmap filtering based on a 7x7 kernel (optimized with 16 taps)
* Same as unity's but modified to use a different shadowmap identifier
*/
half ScattererSampleShadowmap_PCF7x7(float4 coord, float3 receiverPlaneDepthBias)
{
    half shadow = 1;

    #ifndef SHADOWS_NATIVE
        // when we don't have hardware PCF sampling, fallback to a simple 3x3 sampling with averaged results.
        return UnitySampleShadowmap_PCF3x3NoHardwareSupport(coord, receiverPlaneDepthBias);
    #endif

    // tent base is 7x7 base thus covering from 49 to 64 texels, thus we need 16 bilinear PCF fetches
    float2 tentCenterInTexelSpace = coord.xy * _ShadowMapTextureScatterer_TexelSize.zw;
    float2 centerOfFetchesInTexelSpace = floor(tentCenterInTexelSpace + 0.5);
    float2 offsetFromTentCenterToCenterOfFetches = tentCenterInTexelSpace - centerOfFetchesInTexelSpace;

    // find the weight of each texel based on the area of a 45 degree slop tent above each of them.
    float4 texelsWeightsU_A, texelsWeightsU_B;
    float4 texelsWeightsV_A, texelsWeightsV_B;
    _UnityInternalGetWeightPerTexel_7TexelsWideTriangleFilter(offsetFromTentCenterToCenterOfFetches.x, texelsWeightsU_A, texelsWeightsU_B);
    _UnityInternalGetWeightPerTexel_7TexelsWideTriangleFilter(offsetFromTentCenterToCenterOfFetches.y, texelsWeightsV_A, texelsWeightsV_B);

    // each fetch will cover a group of 2x2 texels, the weight of each group is the sum of the weights of the texels
    float4 fetchesWeightsU = float4(texelsWeightsU_A.xz, texelsWeightsU_B.xz) + float4(texelsWeightsU_A.yw, texelsWeightsU_B.yw);
    float4 fetchesWeightsV = float4(texelsWeightsV_A.xz, texelsWeightsV_B.xz) + float4(texelsWeightsV_A.yw, texelsWeightsV_B.yw);

    // move the PCF bilinear fetches to respect texels weights
    float4 fetchesOffsetsU = float4(texelsWeightsU_A.yw, texelsWeightsU_B.yw) / fetchesWeightsU.xyzw + float4(-3.5,-1.5,0.5,2.5);
    float4 fetchesOffsetsV = float4(texelsWeightsV_A.yw, texelsWeightsV_B.yw) / fetchesWeightsV.xyzw + float4(-3.5,-1.5,0.5,2.5);
    fetchesOffsetsU *= _ShadowMapTextureScatterer_TexelSize.xxxx;
    fetchesOffsetsV *= _ShadowMapTextureScatterer_TexelSize.yyyy;

    // fetch !
    float2 bilinearFetchOrigin = centerOfFetchesInTexelSpace * _ShadowMapTextureScatterer_TexelSize.xy;
    shadow  = fetchesWeightsU.x * fetchesWeightsV.x * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.x, fetchesOffsetsV.x), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.y * fetchesWeightsV.x * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.y, fetchesOffsetsV.x), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.z * fetchesWeightsV.x * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.z, fetchesOffsetsV.x), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.w * fetchesWeightsV.x * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.w, fetchesOffsetsV.x), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.x * fetchesWeightsV.y * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.x, fetchesOffsetsV.y), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.y * fetchesWeightsV.y * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.y, fetchesOffsetsV.y), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.z * fetchesWeightsV.y * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.z, fetchesOffsetsV.y), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.w * fetchesWeightsV.y * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.w, fetchesOffsetsV.y), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.x * fetchesWeightsV.z * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.x, fetchesOffsetsV.z), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.y * fetchesWeightsV.z * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.y, fetchesOffsetsV.z), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.z * fetchesWeightsV.z * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.z, fetchesOffsetsV.z), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.w * fetchesWeightsV.z * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.w, fetchesOffsetsV.z), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.x * fetchesWeightsV.w * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.x, fetchesOffsetsV.w), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.y * fetchesWeightsV.w * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.y, fetchesOffsetsV.w), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.z * fetchesWeightsV.w * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.z, fetchesOffsetsV.w), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.w * fetchesWeightsV.w * UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.w, fetchesOffsetsV.w), coord.z, receiverPlaneDepthBias));

    return shadow;
}

/**
 *  Hard shadow
 *  Same as unity's, with the min strength/intensity removed
 */
half getOceanHardShadow(float4 worldPos, float viewPosZ)
{
    fixed4 cascadeWeights = GET_CASCADE_WEIGHTS (worldPos, viewPosZ);
    float4 shadowCoord = GET_SHADOW_COORDINATES(worldPos, cascadeWeights);

    //1 tap hard shadow
    fixed shadow = UNITY_SAMPLE_SHADOW(_ShadowMapTextureScatterer, shadowCoord);
    //shadow = lerp(_LightShadowData.r, 1.0, shadow); //min strength/intensity, not used for ocean

    fixed4 res = shadow;
    return res;
}


/**
 *  Soft Shadow (SM 3.0)
 *  Same as unity's, but using a different shadowMap identifier
 *  And with the min strength/intensity removed
 */
half getOceanSoftShadow(float4 worldPos, float viewPosZ)
{

    fixed4 cascadeWeights = GET_CASCADE_WEIGHTS (worldPos, viewPosZ);
    float4 coord = GET_SHADOW_COORDINATES(worldPos, cascadeWeights);


#if SHADER_API_D3D11
    float3 receiverPlaneDepthBias = float3(0.001,0.001,0.001); //custom bias to fix issue with jittering shadows on d3d11 in 1.9, probably not necessary for ocean though
#else
	float3 receiverPlaneDepthBias = 0.0;
#endif

	//why is this slower than the ragular shadow? maybe change this to 3x3?
    half shadow = ScattererSampleShadowmap_PCF7x7(coord, receiverPlaneDepthBias);
    //shadow = lerp(_LightShadowData.r, 1.0f, shadow); //min strength/intensity, not used for ocean

    // Blend between shadow cascades if enabled
    //
    // Not working yet with split spheres, and no need when 1 cascade
#if UNITY_USE_CASCADE_BLENDING && !defined(SHADOWS_SPLIT_SPHERES) && !defined(SHADOWS_SINGLE_CASCADE)
    half4 z4 = (float4(viewPosZ,viewPosZ,viewPosZ,viewPosZ) - _LightSplitsNear) / (_LightSplitsFar - _LightSplitsNear);
    half alpha = dot(z4 * cascadeWeights, half4(1,1,1,1));

	//why is it slow? maybe remove this?
    UNITY_BRANCH
        if (alpha > 1 - UNITY_CASCADE_BLEND_DISTANCE)
        {
            // get alpha to 0..1 range over the blend distance
            alpha = (alpha - (1 - UNITY_CASCADE_BLEND_DISTANCE)) / UNITY_CASCADE_BLEND_DISTANCE;

            // sample next cascade
            cascadeWeights = fixed4(0, cascadeWeights.xyz);
            coord = GET_SHADOW_COORDINATES(worldPos, cascadeWeights);

            half shadowNextCascade = UnitySampleShadowmap_PCF3x3(coord, receiverPlaneDepthBias);
            //shadowNextCascade = lerp(_LightShadowData.r, 1.0f, shadowNextCascade); //min strength/intensity, not used for ocean
            shadow = lerp(shadow, shadowNextCascade, alpha);
        }
#endif
    return shadow;
}

half getOceanShadow(float4 worldPos, float viewPosZ)
{
	//fade value
	float zDist = dot(_WorldSpaceCameraPos - worldPos, UNITY_MATRIX_V[2].xyz);
	float fadeDist = UnityComputeShadowFadeDistance(worldPos, zDist);
	half  shadowFade = UnityComputeShadowFade(fadeDist);

#if defined (OCEAN_SHADOWS_SOFT)
	return lerp(getOceanSoftShadow(worldPos, viewPosZ),1.0,shadowFade);
#elif defined (OCEAN_SHADOWS_HARD)
	return lerp(getOceanHardShadow(worldPos, viewPosZ),1.0,shadowFade);
#endif
}