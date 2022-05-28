#ifndef CUBE_MAP_CG_INC
#define CUBE_MAP_CG_INC

#ifdef MAP_TYPE_CUBE_1
#define GET_CUBE_MAP_1(name, vect) GetCubeMap(cube ## name, vect)
#define GET_CUBE_MAP_P(name, vect, n, nsc, nst, na) GetCubeMapPerturbed(cube ## name, vect, n, nsc, nst, na)
#define GET_NO_LOD_CUBE_MAP_1(name, vect) GetCubeMapNoLOD(cube ## name, vect)
#define FRAG_GET_NO_LOD_CUBE_MAP_1(name, vect) half4(1,1,1,1)
#define VERT_GET_NO_LOD_CUBE_MAP_1(name, vect) GetCubeMapNoLOD(cube ## name, vect)
#define CUBEMAP_DEF_1(name) \
	uniform samplerCUBE cube ## name;
#elif defined (MAP_TYPE_CUBE6_1)
#define GET_CUBE_MAP_1(name, vect) GetCubeMap(cube ## name ## xn, cube ## name ## xp, \
											cube ## name ## yn, cube ## name ## yp, \
											cube ## name ## zn, cube ## name ## zp, vect)
#define GET_CUBE_MAP_P(name, vect, n, nsc, nst, na) GetCubeMapPerturbed(cube ## name ## xn, cube ## name ## xp, \
											cube ## name ## yn, cube ## name ## yp, \
											cube ## name ## zn, cube ## name ## zp, vect, n, nsc, nst, na)
#define GET_NO_LOD_CUBE_MAP_1(name, vect) GetCubeMapNoLOD(cube ## name ## xn, cube ## name ## xp, \
														cube ## name ## yn, cube ## name ## yp, \
														cube ## name ## zn, cube ## name ## zp, vect)
#define FRAG_GET_NO_LOD_CUBE_MAP_1(name, vect)  GetCubeMap(cube ## name ## xn, cube ## name ## xp, \
											cube ## name ## yn, cube ## name ## yp, \
											cube ## name ## zn, cube ## name ## zp, vect)
#define VERT_GET_NO_LOD_CUBE_MAP_1(name, vect) half4(1,1,1,1)
#define CUBEMAP_DEF_1(name) \
	sampler2D cube ## name ## xn, cube ## name ## xp; \
	sampler2D cube ## name ## yn, cube ## name ## yp; \
	sampler2D cube ## name ## zn, cube ## name ## zp;

#elif defined (MAP_TYPE_CUBE2_1)
#define GET_CUBE_MAP_1(name, vect) GetCubeMap(cube ## name ## POS, cube ## name ## NEG, vect)
#define GET_CUBE_MAP_P(name, vect, n, nsc, nst, na) GetCubeMapPerturbed(cube ## name ## POS, cube ## name ## NEG, vect, n, nsc, nst, na)
#define GET_NO_LOD_CUBE_MAP_1(name, vect) GetCubeMapNoLOD(cube ## name ## POS, cube ## name ## NEG, vect)
#define FRAG_GET_NO_LOD_CUBE_MAP_1(name, vect) half4(1,1,1,1)
#define VERT_GET_NO_LOD_CUBE_MAP_1(name, vect) GetCubeMapNoLOD(cube ## name ## POS, cube ## name ## NEG, vect)
#define CUBEMAP_DEF_1(name) \
	sampler2D cube ## name ## POS; \
	sampler2D cube ## name ## NEG;

#else
#define GET_CUBE_MAP_1(name, vect) GetCubeMap(name, vect)
#define GET_CUBE_MAP_P(name, vect, n, nsc, nst, na) GetCubeMapPerturbed(name, vect, n, nsc, nst, na)
#define GET_NO_LOD_CUBE_MAP_1(name, vect) GetCubeMapNoLOD(name, vect)
#define FRAG_GET_NO_LOD_CUBE_MAP_1(name, vect)  half4(1,1,1,1)
#define VERT_GET_NO_LOD_CUBE_MAP_1(name, vect) GetCubeMapNoLOD(name, vect)
#define CUBEMAP_DEF_1(name) \
	sampler2D name;
#endif



inline float4 CubeDerivatives(float2 uv, float scale)
{
	
	//Make the UV continuous. 
	float2 uvS = abs(uv - (.5*scale));

	float2 uvCont;
	uvCont.x = max(uvS.x, uvS.y);
	uvCont.y = min(uvS.x, uvS.y);

	return float4(ddx(uvCont), ddy(uvCont));
}


inline float4 Derivatives(float2 uv)
{
	float2 uvCont = uv;
	//Make the UV continuous. 
	uvCont.x = abs(uvCont.x - .5);
	return float4(ddx(uvCont), ddy(uvCont));
}

inline float2 GetCubeUV(float3 cubeVect, float2 uvOffset)
{
	float2 uv;
	uv.x = .5 + (INV_2PI*atan2(cubeVect.x, cubeVect.z));
	uv.y = INV_PI*acos(cubeVect.y);
	uv += uvOffset;
	return uv;
}

#define GetCubeCubeUV(cubeVect) \
	float3 cubeVectNorm = normalize(cubeVect); \
	float3 cubeVectNormAbs = abs(cubeVectNorm);\
	half zxlerp = step(cubeVectNormAbs.x, cubeVectNormAbs.z);\
	half nylerp = step(cubeVectNormAbs.y, max(cubeVectNormAbs.x, cubeVectNormAbs.z));\
	half s = lerp(cubeVectNorm.x, cubeVectNorm.z, zxlerp);\
	s = sign(lerp(cubeVectNorm.y, s, nylerp));\
	half3 detailCoords = lerp(half3(1, -s, -1)*cubeVectNorm.xzy, half3(1, s, -1)*cubeVectNorm.zxy, zxlerp);\
	detailCoords = lerp(half3(1, 1, s)*cubeVectNorm.yxz, detailCoords, nylerp);\
	half2 uv = ((.5*detailCoords.yz) / abs(detailCoords.x)) + .5;


inline half4 GetCubeMapNoLOD(sampler2D texSampler, float3 cubeVect)
{
	float4 uv;
	float3 cubeVectNorm = normalize(cubeVect);
	uv.xy = GetCubeUV(cubeVectNorm, float2(0, 0));
	uv.zw = float2(0, 0);
	half4 tex = tex2Dlod(texSampler, uv);
	return tex;
}

inline half4 GetCubeMap(sampler2D texSampler, float3 cubeVect)
{
	float3 cubeVectNorm = normalize(cubeVect);
	float2 uv = GetCubeUV(cubeVectNorm, float2(0, 0));
	float4 uvdd = Derivatives(uv);
	half4 tex = tex2D(texSampler, uv, uvdd.xy, uvdd.zw);
	return tex;
}

inline half4 GetCubeMapPerturbed(sampler2D texSampler, float3 cubeVect, sampler2D uvNoiseSampler, float uvNoiseScale, float uvNoiseStrength, float2 uvNoiseAnimation)
{
	float3 cubeVectNorm = normalize(cubeVect);
	float2 uv = GetCubeUV(cubeVectNorm, float2(0, 0));
	float2 uvd = fmod(uv, uvNoiseScale) / uvNoiseScale + uvNoiseAnimation*float2(_UniversalTime.x, _UniversalTime.x);
	uv += (tex2D(uvNoiseSampler, uvd) - float2(0.5, 0.5))*uvNoiseStrength;
	float4 uvdd = Derivatives(uv);
	half4 tex = tex2D(texSampler, uv, uvdd.xy, uvdd.zw);
	return tex;
}

inline half4 GetCubeMapNoLOD(samplerCUBE texSampler, float3 cubeVect)
{
	half4 uv;
	uv.xyz = normalize(cubeVect);
	uv.w = 0;
	half4 tex = texCUBElod(texSampler, uv);
	return tex;
}

inline half4 GetCubeMap(samplerCUBE texSampler, float3 cubeVect)
{
	half4 tex = texCUBE(texSampler, normalize(cubeVect));
	return tex;
}

inline half4 GetCubeMapPerturbed(samplerCUBE texSampler, float3 cubeVect, sampler2D uvNoiseSampler, float uvNoiseScale, float uvNoiseStrength, float2 uvNoiseAnimation)
{
	cubeVect = normalize(cubeVect);
	float2 uv;
	uv.x = .5 + (INV_2PI*atan2(cubeVect.x, cubeVect.z));
	uv.y = INV_PI*acos(cubeVect.y);
	float2 uvd = fmod(cubeVect, uvNoiseScale) / uvNoiseScale + uvNoiseAnimation*float2(_UniversalTime.x, _UniversalTime.x);
	cubeVect.xy += (tex2D(uvNoiseSampler, uvd) - float2(0.5, 0.5))*uvNoiseStrength;
	half4 tex = texCUBE(texSampler, cubeVect);
	return tex;
}

inline half4 GetCubeMapNoLOD(sampler2D texXn, sampler2D texXp, sampler2D texYn, sampler2D texYp, sampler2D texZn, sampler2D texZp, float3 cubeVect)
{
	float4 uv4;
	uv4.zw = float2(0, 0);

	GetCubeCubeUV(cubeVect);
	uv4.xy = uv;

	half4 sampxn = tex2Dlod(texXn, uv4);
	half4 sampxp = tex2Dlod(texXp, uv4);
	half4 sampyn = tex2Dlod(texYn, uv4);
	half4 sampyp = tex2Dlod(texYp, uv4);
	half4 sampzn = tex2Dlod(texZn, uv4);
	half4 sampzp = tex2Dlod(texZp, uv4);

	half4 sampx = lerp(sampxn, sampxp, step(0, s));
	half4 sampy = lerp(sampyn, sampyp, step(0, s));
	half4 sampz = lerp(sampzn, sampzp, step(0, s));
	
	half4 samp = lerp(sampx, sampz, zxlerp);
		  samp = lerp(sampy, samp, nylerp);
	

	return samp;
	
}

inline half4 GetCubeMap(sampler2D texXn, sampler2D texXp, sampler2D texYn, sampler2D texYp, sampler2D texZn, sampler2D texZp, float3 cubeVect)
{
	GetCubeCubeUV(cubeVect);

	//this fixes UV discontinuity on Y-X seam by swapping uv coords in derivative calcs when in the X quadrants.
	float4 uvdd = CubeDerivatives(uv, 1);


	half4 sampxn = tex2D(texXn, uv, uvdd.xy, uvdd.zw);
	half4 sampxp = tex2D(texXp, uv, uvdd.xy, uvdd.zw);
	half4 sampyn = tex2D(texYn, uv, uvdd.xy, uvdd.zw);
	half4 sampyp = tex2D(texYp, uv, uvdd.xy, uvdd.zw);
	half4 sampzn = tex2D(texZn, uv, uvdd.xy, uvdd.zw);
	half4 sampzp = tex2D(texZp, uv, uvdd.xy, uvdd.zw);

	half4 sampx = lerp(sampxn, sampxp, step(0, s));
	half4 sampy = lerp(sampyn, sampyp, step(0, s));
	half4 sampz = lerp(sampzn, sampzp, step(0, s));

	half4 samp = lerp(sampx, sampz, zxlerp);
	samp = lerp(sampy, samp, nylerp);
	return samp;
}

inline half4 GetCubeMapPerturbed(sampler2D texXn, sampler2D texXp, sampler2D texYn, sampler2D texYp, sampler2D texZn, sampler2D texZp, float3 cubeVect, sampler2D uvNoiseSampler, float uvNoiseScale, float uvNoiseStrength, float2 uvNoiseAnimation)
{
	GetCubeCubeUV(cubeVect);

	float2 uvd = fmod(uv, uvNoiseScale) / uvNoiseScale + uvNoiseAnimation*float2(_UniversalTime.x, _UniversalTime.x);
	uv += (tex2D(uvNoiseSampler, uvd) - float2(0.5, 0.5))*uvNoiseStrength;

	//this fixes UV discontinuity on Y-X seam by swapping uv coords in derivative calcs when in the X quadrants.
	float4 uvdd = CubeDerivatives(uv, 1);


	half4 sampxn = tex2D(texXn, uv, uvdd.xy, uvdd.zw);
	half4 sampxp = tex2D(texXp, uv, uvdd.xy, uvdd.zw);
	half4 sampyn = tex2D(texYn, uv, uvdd.xy, uvdd.zw);
	half4 sampyp = tex2D(texYp, uv, uvdd.xy, uvdd.zw);
	half4 sampzn = tex2D(texZn, uv, uvdd.xy, uvdd.zw);
	half4 sampzp = tex2D(texZp, uv, uvdd.xy, uvdd.zw);

	half4 sampx = lerp(sampxn, sampxp, step(0, s));
	half4 sampy = lerp(sampyn, sampyp, step(0, s));
	half4 sampz = lerp(sampzn, sampzp, step(0, s));

	half4 samp = lerp(sampx, sampz, zxlerp);
	samp = lerp(sampy, samp, nylerp);
	return samp;
}

inline half4 GetCubeMapNoLOD(sampler2D texSamplerPos, sampler2D texSamplerNeg, float3 cubeVect)
{

	float4 uv4;
	uv4.zw = float2(0, 0);

	GetCubeCubeUV(cubeVect);
	uv4.xy = uv;

	half4 texPos = tex2Dlod(texSamplerPos, uv4);
	half4 texNeg = tex2Dlod(texSamplerNeg, uv4);

	half4 tex = lerp(texNeg, texPos, step(0, s));

	half alpha = lerp(tex.x, tex.z, zxlerp);
	alpha = lerp(tex.y, alpha, nylerp);
	return half4(tex.a, tex.a, tex.a, alpha);

}

inline half4 GetCubeMap(sampler2D texSamplerPos, sampler2D texSamplerNeg, float3 cubeVect)
{
	GetCubeCubeUV(cubeVect);

	float4 uvdd = CubeDerivatives(uv, 1);

	half4 texPos = tex2D(texSamplerPos, uv, uvdd.xy, uvdd.zw);
	half4 texNeg = tex2D(texSamplerNeg, uv, uvdd.xy, uvdd.zw);

	half4 tex = lerp(texNeg, texPos, step(0, s));

	half alpha = lerp(tex.x, tex.z, zxlerp);
	alpha = lerp(tex.y, alpha, nylerp);
	return half4(tex.a, tex.a, tex.a, alpha);

}

inline half4 GetCubeMapPerturbed(sampler2D texSamplerPos, sampler2D texSamplerNeg, float3 cubeVect, sampler2D uvNoiseSampler, float uvNoiseScale, float uvNoiseStrength, float2 uvNoiseAnimation)
{
	GetCubeCubeUV(cubeVect);

	float2 uvd = fmod(uv, uvNoiseScale) / uvNoiseScale + uvNoiseAnimation*float2(_UniversalTime.x, _UniversalTime.x);
	uv += (tex2D(uvNoiseSampler, uvd) - float2(0.5, 0.5))*uvNoiseStrength;

	float4 uvdd = CubeDerivatives(uv, 1);

	half4 texPos = tex2D(texSamplerPos, uv, uvdd.xy, uvdd.zw);
	half4 texNeg = tex2D(texSamplerNeg, uv, uvdd.xy, uvdd.zw);

	half4 tex = lerp(texNeg, texPos, step(0, s));

	half alpha = lerp(tex.x, tex.z, zxlerp);
	alpha = lerp(tex.y, alpha, nylerp);
	return half4(tex.a, tex.a, tex.a, alpha);
}



inline half4 GetCubeDetailMapNoLOD(sampler2D texSampler, float3 cubeVect, float detailScale)
{
	float4 uv4;
	uv4.zw = float2(0, 0);

	GetCubeCubeUV(cubeVect);
	uv4.xy = uv;
	half4 tex = tex2Dlod(texSampler, uv4);
	return tex;
}

inline half4 GetCubeDetailMap(sampler2D texSampler, float3 cubeVect, float detailScale)
{
	GetCubeCubeUV(cubeVect);
	uv*=detailScale;
	float4 uvdd = CubeDerivatives(uv, detailScale);
	half4 tex = tex2D(texSampler, uv, uvdd.xy, uvdd.zw);
	return 	tex;
}


#endif