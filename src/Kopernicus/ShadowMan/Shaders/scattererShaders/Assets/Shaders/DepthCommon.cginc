#include "UnityCG.cginc"

uniform sampler2D _CameraDepthTexture;
uniform sampler2D _customDepthTexture;

float3 getViewSpacePosFromDepth(float2 uv)
{
	uv.y = 1.0 - uv.y;

	float zdepth = tex2Dlod(_CameraDepthTexture, float4(uv,0,0));

	#ifdef SHADER_API_D3D11  //#if defined(UNITY_REVERSED_Z)
	zdepth = 1 - zdepth;
	#endif

	float4 clipPos = float4(uv, zdepth, 1.0);
	clipPos.xyz = 2.0f * clipPos.xyz - 1.0f;
	float4 camPos = mul(unity_CameraInvProjection, clipPos);
	camPos.xyz /= camPos.w;
	camPos.z *= -1;
	return camPos.xyz;
}

float3 getPreciseViewSpacePosFromDepth(float2 uv)
{
	uv.y = 1.0 - uv.y;

	float zdepth = tex2Dlod(_CameraDepthTexture, float4(uv,0,0));
	float depth = Linear01Depth(zdepth);

	#ifdef SHADER_API_D3D11  //#if defined(UNITY_REVERSED_Z)
	zdepth = 1 - zdepth;
	#endif

	float4 clipPos = float4(uv, zdepth, 1.0);
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

float getScattererFragDistance(float2 uv)
{
	#if defined (SCATTERER_MERGED_DEPTH_ON)
	return tex2Dlod(_customDepthTexture, float4(uv,0,0)).r* 750000;
	#else
	return length(getPreciseViewSpacePosFromDepth(uv).xyz);
	#endif
}

//checks if we have anything in the depth buffer or is empty at coordinate
//used by sunflare to check if we should block the sun
fixed checkDepthBufferEmpty(float2 uv)
{
	#if defined (SCATTERER_MERGED_DEPTH_ON)
	float depth =  tex2Dlod(_customDepthTexture,float4(uv,0.0,0.0));  //if there's something in the way don't render the flare	
	return (depth < 1.0) ? 0.0 : 1.0 ;
	#else
	float zdepth = tex2Dlod(_CameraDepthTexture, float4(uv,0,0));

	#ifdef SHADER_API_D3D11  //#if defined(UNITY_REVERSED_Z)
	zdepth = 1 - zdepth;
	#endif

	return (zdepth < 1.0) ? 0.0 : 1.0 ;
	#endif
}

//get WorldPos from depth using inaccurate invprojection method
float3 getWorldPosFromDepth(float2 uv, float zdepth, float4x4 CameraToWorld)
{
	#ifdef SHADER_API_D3D11  //#if defined(UNITY_REVERSED_Z)
	zdepth = 1 - zdepth;
	#endif

	float4 clipPos = float4(uv, zdepth, 1.0);
	clipPos.xyz = 2.0f * clipPos.xyz - 1.0f;

	float4 camPos = mul(unity_CameraInvProjection, clipPos);

	float4 worldPos = mul(CameraToWorld,camPos);
	return (worldPos.xyz/worldPos.w);
}

//hybrid method using the depth from the ray method and the direction from the invprojection method
//seems to work pretty well
//could be optimized further by moving the raydirection calculation to the vertex shader
float3 getPreciseWorldPosFromDepth(float2 uv, float zdepth, float4x4 CameraToWorld)
{
	float depth = Linear01Depth(zdepth);

#if defined(UNITY_REVERSED_Z)
	zdepth = 1 - zdepth;
#endif

	float4 clipPos = float4(uv, zdepth, 1.0);
	clipPos.xyz = 2.0f * clipPos.xyz - 1.0f;
	float4 camPos = mul(unity_CameraInvProjection, clipPos);
	camPos.xyz /= camPos.w;

	float3 rayDirection = normalize(camPos.xyz);

	float3 cameraForwardDir = float3(0,0,-1);
	float aa = dot(rayDirection, cameraForwardDir);

	camPos.xyz = rayDirection * depth/aa * _ProjectionParams.z;

	float4 worldPos = mul(CameraToWorld,float4(camPos.xyz,1.0));
	return (worldPos.xyz/worldPos.w);
}

//Refines the inaccurate worldPos from invprojection with a search algorithm
//While this gives good results, it is no longer needed, the hybrid ray/projmatrix method is faster and gives similar precision
float getRefinedDistanceFromDepth(float unrefinedDistance, float zdepth, float3 worldViewDir)
{
	//maybe scale these based on distance?
	const int maxIterations = 10; //seems about perfect
	int iteration = 0;

	//This should considerably reduce the search space
	float maxSearchDistance = unrefinedDistance * 1.30;
	float minSearchDistance = unrefinedDistance * 0.70;

	float mid = 0;
	float3 worldPos0 = float3(0.0,0.0,0.0);
	float4 clipPos = float4(0.0,0.0,0.0,1.0);
	float depth = -10.0;

	while ((iteration < maxIterations) && (depth != zdepth))
	{
		mid = 0.5 * (maxSearchDistance + minSearchDistance);

		worldPos0 = _WorldSpaceCameraPos + worldViewDir * mid;

		clipPos = mul(UNITY_MATRIX_VP, float4(worldPos0,1.0));
		depth = clipPos.z/clipPos.w;

		maxSearchDistance = (depth < zdepth) ? mid : maxSearchDistance;
		minSearchDistance = (depth > zdepth) ? mid : minSearchDistance;

		iteration++;
	}

	return mid;
}