//check if ray defined by origin and end intersects frustum
//for it to intersect frustum origin and end must be on different ends of one of the axes
bool intersectsFrustum(float3 origin, float3 end)
{
	return !(origin.x > 1.0 && end.x > 1.0 || origin.x < -1.0 && end.x < -1.0)
		|| !(origin.y > 1.0 && end.y > 1.0 || origin.y < -1.0 && end.y < -1.0)
		|| !(origin.z < 0.0 && end.z < 0.0);
}

float rayDistanceToPoint(float3 rayOrigin, float3 rayDirection, float3 targetPoint)
{
	return  length(cross(rayDirection, targetPoint - rayOrigin));
}

inline bool between(float a, float b, float x)
{
	return (x > a) && (x < b);
}

//cascadeWeights -> 0,1,2,3 -> zero is the most detailed -> 3 is the least detailed
//0 is in lower left corner, 1 in lower right corner, 2 in upper left corner, 3 in upper right corner, in the case of regular cascades no split spheres, not sure about splitSpheres
//Still need to check the shadowMap for each cascade though to make sure we have a depth value at that coordinate
inline fixed pickMostDetailedCascade(float4 wpos, out float4 shadowPos, sampler2D shadowMap)
{
	float zdepth = 0;
	shadowPos = 0;

	for (int i=0; i<4;i++)
	{
		float3 coords = mul (unity_WorldToShadow[i], wpos).xyz;
		zdepth = tex2Dlod(shadowMap, float4(coords.xy,0.0,0.0)).r;
			
		float startX = fmod(i,2) * 0.5;
		float startY = (i/2) * 0.5;

		if (between(startX, startX+0.5, coords.x) && between(startY, startY+0.5, coords.y) && (zdepth > 0.0) && ((zdepth < 0.98) || (i==3) ))
		{
			shadowPos = float4(coords.xy, zdepth, 1.0);
			return i;
		}
	}

	return -1;
}

//same but pick between regular shadowMap and cloud shadowMap
inline fixed pickMostDetailedCascadeCloud(float4 wpos, out float4 shadowPos, sampler2D shadowMap, sampler2D cloudShadowMap, out float isCloud)
{
	shadowPos = 0;

	for (int i=0; i<4;i++)
	{
		float3 coords = mul (unity_WorldToShadow[i], wpos).xyz;

		float zdepth = tex2Dlod(shadowMap, float4(coords.xy,0.0,0.0)).r;
		float zdepthCloud = tex2Dlod(cloudShadowMap, float4(coords.xy,0.0,0.0)).r;

		float maxDepth = max(zdepth,zdepthCloud);

		float startX = fmod(i,2) * 0.5;
		float startY = (i/2) * 0.5;

		if (between(startX, startX+0.5, coords.x) && between(startY, startY+0.5, coords.y) && (maxDepth > 0.0) && ((maxDepth < 0.98) || (i==3) ))
		{
			isCloud = (zdepthCloud > zdepth) ? 1.0 : 0.0;
			shadowPos = float4(coords.xy, maxDepth, 1.0);
			return i;
		}
	}

	return -1;
}