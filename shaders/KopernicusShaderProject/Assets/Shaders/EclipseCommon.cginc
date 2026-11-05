uniform float4 sunPosAndRadius;   //xyz sun pos w radius
uniform float4x4 lightOccluders1; //array of light occluders
uniform float4x4 lightOccluders2; //for each float4 xyz pos w radius

//Source:   wikibooks.org/wiki/GLSL_Programming/Unity/Soft_Shadows_of_Spheres
//I believe space engine also uses the same approach because the eclipses look the same ;)
float getEclipseShadow(float3 worldPos, float3 worldLightPos,float3 occluderSpherePosition,
	float3 occluderSphereRadius, float3 lightSourceRadius)		
{											
	float3 lightDirection = float3(worldLightPos - worldPos);
	float3 lightDistance = length(lightDirection);
	lightDirection = lightDirection / lightDistance;

	// computation of level of shadowing w  
	float3 sphereDirection = float3(occluderSpherePosition - worldPos);  //occluder planet
	float sphereDistance = length(sphereDirection);
	sphereDirection = sphereDirection / sphereDistance;

	float dd = lightDistance * (asin(min(1.0, length(cross(lightDirection, sphereDirection)))) 
		- asin(min(1.0, occluderSphereRadius / sphereDistance)));

	float w = smoothstep(-1.0, 1.0, -dd / lightSourceRadius);
	w = w * smoothstep(0.0, 0.2, dot(lightDirection, sphereDirection));

	return (1-w);
}

inline float getEclipseShadows(float3 worldPos, float4x4 lightOccludersmatrix)
{
	float eclipseShadow = 1.0;
	for (int i=0; i<4; ++i)
	{
		UNITY_BRANCH
		if (lightOccludersmatrix[i].w > 0)
		{
			eclipseShadow*= getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccludersmatrix[i].xyz,lightOccludersmatrix[i].w, sunPosAndRadius.w);
		}
		else
		{
			break;
		}
	}
	return eclipseShadow;
}


inline float getEclipseShadows(float3 worldPos)
{
	return (getEclipseShadows(worldPos, lightOccluders1) * getEclipseShadows(worldPos, lightOccluders2));
}