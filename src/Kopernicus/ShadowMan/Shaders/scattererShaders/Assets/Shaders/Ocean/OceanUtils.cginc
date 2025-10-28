uniform float refractionIndex;
uniform float3 _Underwater_Color;
uniform float3 _Ocean_Color;
uniform float transparencyDepth;
uniform float skyReflectionStrength;

float3 ReflectedSky(float3 V, float3 N, float3 sunDir, float3 earthP)
{
	float3 result = float3(0,0,0);

	float3 reflectedAngle=reflect(-V,N);
	reflectedAngle.z=max(reflectedAngle.z,0.0);	//hack to avoid unsightly black pixels from downwards reflections
	result = SkyRadiance3(earthP,reflectedAngle, sunDir);

	return result;
}

//TODO: check if can optimize/simplify
float3 refractVector(float3 I, float3 N, float ior) 
{ 
	float cosi = dot(I, N);

	float3 n = N; 
	cosi = -cosi;

	float eta = 1.0 / ior; 
	float k = 1 - eta * eta * (1 - cosi * cosi); 
	return k < 0 ? 0 : eta * I + (eta * cosi - sqrt(k)) * n; 
}

float3 RefractedSky(float3 V, float3 N, float3 sunDir, float3 earthP) 
{
	float3 result = float3(0,0,0);

	float3 refractedAngle = refractVector(-V, -N, 1/refractionIndex);
	result = SkyRadiance3(earthP,refractedAngle, sunDir);

	return result;
}

//TODO: check if can optimize/simplify
float fresnel_dielectric(float3 I, float3 N, float eta)
{
	//compute fresnel reflectance without explicitly computing the refracted direction
	float c = abs(dot(I, N));
	float g = eta * eta - 1.0 + c * c;
	float result;

	float g2 =g;
	g = sqrt(g);
	float A =(g - c)/(g + c);
	float B =(c *(g + c)- 1.0)/(c *(g - c)+ 1.0);
	result = 0.5 * A * A *(1.0 + B * B);

	result = (g2>0) ? result : 1.0;  // TIR (no refracted component)

	return result;
}

//underwater ocean color
float3 oceanColor(float3 viewDir, float3 lightDir, float3 surfaceDir)
{
	float angleToLightDir = (dot(viewDir, surfaceDir) + 1 )* 0.5;
	float3 waterColor = pow(_Underwater_Color, 4.0 *(-1.0 * angleToLightDir + 1.0));
	return waterColor;
}

float getFresnel(float3 V, float3 N, float sigmaSq)
{
  #if defined (UNDERWATER_ON)
	float fresnel = 1.0 - fresnel_dielectric(V, N, 1/refractionIndex);
  #else
	float fresnel = MeanFresnel(V, N, sigmaSq);
  #endif

	fresnel = clamp(fresnel,0.0,1.0);
	return fresnel;
}

float3 getSkyColor(float fresnel, float3 V, float3 N, float3 L, float3 earthP, float3 skyE, float shadowTerm, float radius)
{
  #if defined (UNDERWATER_ON)
	return (fresnel * RefractedSky(V, N, L, earthP));
  #else
    #if defined (SKY_REFLECTIONS_ON)
	float3 skyColor = lerp(skyE / M_PI, ReflectedSky(V, N, L, earthP), skyReflectionStrength);			//mix accurate sky reflection and sky irradiance

	return (fresnel * (skyColor * lerp(0.5,1.0,shadowTerm) + (UNITY_LIGHTMODEL_AMBIENT.rgb*0.07)));
    #else
	return (fresnel * (skyE / M_PI * lerp(0.5,1.0,shadowTerm) + (UNITY_LIGHTMODEL_AMBIENT.rgb*0.07)));		//sky irradiance only
    #endif
  #endif
}

float3 getOceanColor(float fresnel, float3 V, float3 N, float3 L, float3 earthP, float3 skyE, float shadowTerm)
{
  #if defined (UNDERWATER_ON)
	float3 ocColor = _sunColor * oceanColor(reflect(-V,N),L,float3(0.0,0.0,1.0));		//reflected ocean color from underwater
	float waterLightExtinction = length(getSkyExtinction(earthP, L));
	return(hdrNoExposure(waterLightExtinction * ocColor) * lerp(0.8,1.0,shadowTerm));
  #else
	return(0.98 * (1.0 - fresnel) * _Ocean_Color * (skyE / M_PI) * lerp(0.3,1.0,shadowTerm));
  #endif
}

float2 getPerturbedUVsAndDepth(float2 depthUV, float3 N, float oceanDistance, out float fragDistance, out float depth)
{
	float2 uv;
  #if defined (UNDERWATER_ON)
	uv = depthUV.xy + (N.xy)*0.025 * float2(1.0,10.0);
  #else
	uv = depthUV.xy + N.xy*0.025;
  #endif

	fragDistance = getScattererFragDistance(uv);
	depth= fragDistance - oceanDistance;							//water depth, ie viewing ray distance in water

	uv = (depth < 0) ? depthUV.xy : uv;							//for refractions, use the normal fragment uv instead the perturbed one if the perturbed one is closer
	fragDistance = getScattererFragDistance(uv);
	depth= fragDistance - oceanDistance;

  #if !defined (UNDERWATER_ON)
	depth=lerp(depth,transparencyDepth,clamp((oceanDistance-1000.0)/5000.0,0.0,1.0)); 	//fade out refractions and transparency at distance, to hide swirly artifacts of low precision
  #endif

	return uv;
}

float getTransparencyAlpha(float depth)
{
	float transparencyAlpha=lerp(0.0,1.0,depth/transparencyDepth);
	transparencyAlpha = (depth < -0.5) ? 1.0 : transparencyAlpha;   //fix black edge around antialiased terrain in front of ocean
	return transparencyAlpha;
}

float adjustWhiteCapStrengthWithDepth(float _Ocean_WhiteCapStr, float shoreFoam, float depth)
{
	_Ocean_WhiteCapStr = lerp(shoreFoam,_Ocean_WhiteCapStr, clamp(depth*0.2,0.0,1.0));
	return ((depth <= 0.0) ? 0.0 : _Ocean_WhiteCapStr); //fixes white outline around objects in front of the ocean
}

float applyFarWhiteCapStrength(float oceanDistance, float alphaRadius, float _Ocean_WhiteCapStr, float farWhiteCapStr)
{
	float clampFactor= clamp(oceanDistance/alphaRadius,0.0,1.0); //factor to clamp whitecaps
	return(lerp(_Ocean_WhiteCapStr,farWhiteCapStr,clampFactor));
}

float3 getTotalWhiteCapRadiance(float outWhiteCapStr, float jmx, float jSigma2, float3 sunL, float3 N, float3 L, float3 skyE, float shadowTerm)
{
	// get coverage
	float W = WhitecapCoverage(outWhiteCapStr,jmx,jSigma2);

	// compute and add whitecap radiance
	float3 l = (sunL * (max(dot(N, L), 0.0)) + skyE + UNITY_LIGHTMODEL_AMBIENT.rgb * 30) / M_PI;
	return (float3(W * l * 0.4)* lerp(0.5,1.0,shadowTerm));
}



void getPlanetShineContribution(out float3 LsunTotal, out float3 R_ftotTotal, out float3 LseaTotal, out float3 LskyTotal)
{
//	for (int i=0; i<4; ++i)
//	{
//		if (planetShineRGB[i].w == 0) break;
//
//		L=normalize(planetShineSources[i].xyz);
//		SunRadianceAndSkyIrradiance(earthP, N, L, sunL, skyE);
//
//		#if defined (SKY_REFLECTIONS_ON)
//		Lsky = fresnel * ReflectedSky(V, N, L, earthP);   //planet, accurate sky reflections
//		#else
//		Lsky = fresnel * skyE / M_PI; 		   //planet, sky irradiance only
//		#endif
//
//		Lsun = ReflectedSunRadiance(L, V, N, sigmaSq) * sunL;
//		Lsea = RefractedSeaRadiance(V, N, sigmaSq) * _Ocean_Color * skyE / M_PI;
//		l = (sunL * (max(dot(N, L), 0.0)) + skyE) / M_PI;
//		R_ftot = float3(W * l * 0.4);
//
//		//if source is not a sun compute intensity of light from angle to light source
//		float intensity=1;  
//		if (planetShineSources[i].w != 1.0f)
//		{
//			intensity = 0.57f*max((0.75-dot(normalize(planetShineSources[i].xyz - earthP),_Ocean_SunDir)),0);
//		}
//
//		surfaceColor+= abs((Lsun + Lsky + Lsea + R_ftot)*planetShineRGB[i].xyz*planetShineRGB[i].w*intensity);
//		LsunTotal   += Lsun;
//		R_ftotTotal += R_ftot;
//		LseaTotal   += Lsea;
//		LskyTotal   += Lsky;
//	}
}