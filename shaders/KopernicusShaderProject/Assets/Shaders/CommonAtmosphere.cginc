//
//  Precomputed Atmospheric Scattering
//  Copyright (c) 2008 INRIA
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without
//  modification, are permitted provided that the following conditions
//  are met:
//  1. Redistributions of source code must retain the above copyright
//     notice, this list of conditions and the following disclaimer.
//  2. Redistributions in binary form must reproduce the above copyright
//     notice, this list of conditions and the following disclaimer in the
//     documentation and/or other materials provided with the distribution.
//  3. Neither the name of the copyright holders nor the names of its
//     contributors may be used to endorse or promote products derived from
//     this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
//  AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
//  ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
//  LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
//  CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
//  SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
//  INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
//  CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
//  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
//  THE POSSIBILITY OF SUCH DAMAGE.
//
//  Author: Eric Bruneton
//  Modified and ported to Unity by Justin Hawkins 2013
//


//            #define useHorizonHack
#define useAnalyticTransmittance

uniform sampler2D _Transmittance;
uniform sampler2D _Inscatter;
uniform sampler2D _Irradiance;
uniform float TRANSMITTANCE_W;
uniform float TRANSMITTANCE_H;
uniform float SKY_W;
uniform float SKY_H;
uniform float M_PI;
uniform float3 EARTH_POS;

// Rayleigh
uniform float HR;
uniform float3 betaR;

// Mie
uniform float HM;
uniform float3 betaMSca;
uniform float3 betaMEx;
uniform float mieG;

uniform float Rg;
uniform float Rt;
uniform float RL;
uniform float RES_R;
uniform float RES_MU;
uniform float RES_MU_S;
uniform float RES_NU;
uniform float3 SUN_DIR;

#define _Sun_Intensity 100.0;
uniform float3 _sunColor;

uniform float _experimentalAtmoScale;

uniform float _viewdirOffset;

uniform float cloudTerminatorSmooth;

#include "IntersectCommon.cginc"

/** returns the matrix {{0.0,3.0},{2.0,1.0}} */
float bayer2(const float2 xy)
{
	return fmod(3.0*xy.x+2.0*xy.y,4.0);
}

/** 
 * Non-normalized Bayer-Pattern for power-of-two sized matrices (see https://en.wikipedia.org/wiki/Ordered_dithering for the formula).
 * sizeExpOfTwo determines the dimension of the matrix to generate. For instance having sizeExpOfTwo == 3 yields an 8x8 matrix, sizeExpOfTwo == 4 yields 16x16.
 * The needed normalization factor is (2**(sizeExpOfTwo))**2, so for sizeExpOfTwo == 3 the normalization would be 64.
 * xy: Indices inside the matrix. For performance reasons they are not sanitized, so make sure you use valid indices.
 * Floating point variant. Known to be exact up to sizeExpOfTwo == 12 (might be better, just not tested)
 */
float bayerPattern(const int sizeExpOfTwo, float2 xy)
{
	float factor = 1.0;
	float summand = 0.0;
	//The code would get easier to read if a while-loop were used, but then the compiler would have a harder time unrolling it.
	for(int i=1;i<sizeExpOfTwo;++i)
	{
		float matrixSize = exp2(sizeExpOfTwo-i);
		float summ = bayer2(floor(xy/matrixSize));
		xy=fmod(xy,matrixSize);
		summand = summand+factor*summ;
		factor = factor*4.0;
	}
	return factor * bayer2(xy) + summand;
}

//test version without a loop and for fixed 8x8 matrix size - not really any faster than the generic function above. Only left in for reference.
uint bayer8inl(const uint2 xy)
{
	return 4*(4*bayer2(xy%2) + bayer2(xy%4/2)) + bayer2(xy/4);
}

float3 dither (float3 iColor, float2 iScreenPos)
{
#if defined (DITHERING_ON)
	float bayer=bayerPattern(3,fmod(int2(iScreenPos),8));

	const float rgbByteMax=255.;

	float3 rgb=iColor*rgbByteMax;
	float3 head=floor(rgb);
	float3 tail=frac(rgb);

	return (head + step(bayer,64.f*tail)) / rgbByteMax;
#else
	return iColor;
#endif
}

float4 dither (float4 iColor, float2 iScreenPos)
{
	return float4(dither(iColor.rgb, iScreenPos), iColor.a);
}


float3 hdr(float3 L, float exposure) {
	L = L * exposure;
	L.r = L.r < 1.413 ? pow(L.r * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.r);
	L.g = L.g < 1.413 ? pow(L.g * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.g);
	L.b = L.b < 1.413 ? pow(L.b * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.b);
	return L;
}

float3 hdrNoExposure(float3 L) {
	L.r = L.r < 1.413 ? pow(L.r * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.r);
	L.g = L.g < 1.413 ? pow(L.g * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.g);
	L.b = L.b < 1.413 ? pow(L.b * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.b);
	return L;
}

float4 Texture4D(sampler2D table, float r, float mu, float muS, float nu, float RtResized)
{
	float H = sqrt(RtResized * RtResized - Rg * Rg);
	float rho = sqrt(r * r - Rg * Rg);
	float rmu = r * mu;
	float delta = rmu * rmu - r * r + Rg * Rg;
	float4 cst = rmu < 0.0 && delta > 0.0 ? float4(1.0, 0.0, 0.0, 0.5 - 0.5 / RES_MU) : float4(-1.0, H * H, H, 0.5 + 0.5 / RES_MU);
	float uR = 0.5 / RES_R + rho / H * (1.0 - 1.0 / float(RES_R));
	float uMu = cst.w + (rmu * cst.x + sqrt(delta + cst.y)) / (rho + cst.z) * (0.5 - 1.0 / RES_MU);
	// paper formula
	//    float uMuS = 0.5 / float(RES_MU_S) + max((1.0 - exp(-3.0 * muS - 0.6)) / (1.0 - exp(-3.6)), 0.0) * (1.0 - 1.0 / float(RES_MU_S));
	// better formula
	float uMuS = 0.5 / RES_MU_S + (atan(max(muS, -0.1975) * tan(1.26 * 1.1)) / 1.1 + (1.0 - 0.26)) * 0.5 * (1.0 - 1.0 / RES_MU_S);
	float _lerp = (nu + 1.0) / 2.0 * (RES_NU - 1.0);
	float uNu = floor(_lerp);
	_lerp = _lerp - uNu;
	//original 3D lookup
	//return tex3Dlod(table, float4((uNu + uMuS) / RES_NU, uMu, uR, 0)) * (1.0 - _lerp) + tex3Dlod(table, float4((uNu + uMuS + 1.0) / RES_NU, uMu, uR, 0)) * _lerp;
	//new 2D lookup
	float u_0 = floor(uR*RES_R-1)/(RES_R);
	float u_1 = floor(uR*RES_R)/(RES_R);
	float u_frac = frac(uR*RES_R);
	float4 A = tex2Dlod(table, float4((uNu + uMuS) / RES_NU, uMu / RES_R + u_0,0.0,0.0)) * (1.0 - _lerp) + tex2Dlod(table, float4((uNu + uMuS + 1.0) / RES_NU, uMu / RES_R + u_0,0.0,0.0)) * _lerp;
	float4 B = tex2Dlod(table, float4((uNu + uMuS) / RES_NU, uMu / RES_R + u_1,0.0,0.0)) * (1.0 - _lerp) + tex2Dlod(table, float4((uNu + uMuS + 1.0) / RES_NU, uMu / RES_R + u_1,0.0,0.0)) * _lerp;
	return (A * (1.0-u_frac) + B * u_frac);
}

float3 GetMie(float4 rayMie)
{
	// approximated single Mie scattering (cf. approximate Cm in paragraph "Angular precision")
	// rayMie.rgb=C*, rayMie.w=Cm,r
	return rayMie.rgb * rayMie.w / max(rayMie.r, 1e-4) * (betaR.r / betaR);
}

float PhaseFunctionR(float mu)
{
	// Rayleigh phase function
	return (3.0 / (16.0 * M_PI)) * (1.0 + mu * mu);
}

float PhaseFunctionM(float mu)
{
	// Mie phase function
	return 1.5 * 1.0 / (4.0 * M_PI) * (1.0 - mieG*mieG) * pow(1.0 + (mieG*mieG) - 2.0*mieG*mu, -3.0/2.0) * (1.0 + mu * mu) / (2.0 + mieG*mieG);
}

float3 Transmittance(float r, float mu)
{
	// transmittance(=transparency) of atmosphere for infinite ray (r,mu)
	// (mu=cos(view zenith angle)), intersections with ground ignored
	float uR, uMu;
	uR = sqrt((r - Rg) / (Rt - Rg));
	uMu = atan((mu + 0.15) / (1.0 + 0.15) * tan(1.5)) / 1.5;
	//#if !defined(SHADER_API_OPENGL)
	return tex2Dlod (_Transmittance, float4(uMu, uR,0.0,0.0)).rgb;
	//#else
	//    return tex2D (_Transmittance, float2(uMu, uR)).rgb;
	//#endif
}


// optical depth for ray (r,mu) of length d, using analytic formula
// (mu=cos(view zenith angle)), intersections with ground ignored
// H=height scale of exponential density function
float OpticalDepth(float H, float r, float mu, float d)
{
	float a = sqrt((0.5/H)*r);
	float2 a01 = a*float2(mu, mu + d / r);
	float2 a01s = sign(a01);
	float2 a01sq = a01*a01;
	float x = a01s.y > a01s.x ? exp(a01sq.x) : 0.0;
	float2 y = a01s / (2.3193*abs(a01) + sqrt(1.52*a01sq + 4.0)) * float2(1.0, exp(-d/H*(d/(2.0*r)+mu)));
	return sqrt((6.2831*H)*r) * exp((Rg-r)/H) * (x + dot(y, float2(1.0, -1.0)));
}

// transmittance(=transparency) of atmosphere for ray (r,mu) of length d
// (mu=cos(view zenith angle)), intersections with ground ignored
// uses analytic formula instead of transmittance texture
float3 AnalyticTransmittance(float r, float mu, float d)
{
	return exp(- betaR * OpticalDepth(HR * _experimentalAtmoScale, r, mu, d) - betaMEx * OpticalDepth(HM * _experimentalAtmoScale, r, mu, d));
}

//search algorithm to find approx distance from optical depth
float DistanceFromOpticalDepth(float H, float r, float mu, float targetOpticalDepth, float maxLength)
{
	if (targetOpticalDepth == 0.0)
		return 0.0;
	
	int maxIterations = 12;
	int iteration = 0;

	float minDistance = 0; //maybe also init this with the targetOpticalDepth?
	float maxDistance = maxLength;

	float mid = 0.5 * (maxDistance + minDistance);
	float depth = 0;

	while ((iteration < maxIterations) && (depth != targetOpticalDepth))
	{
		depth = OpticalDepth(H, r, mu, mid);

		if (depth >= targetOpticalDepth)
		{
			maxDistance = mid;
		}
		else
		{
			minDistance = mid;
		}

		mid = 0.5 * (maxDistance + minDistance);


		iteration++;
	}

	return mid;
}

//the extinction part extracted from the inscattering function
//this is for objects in atmo, computed using analyticTransmittance (better precision and less artifacts) or the precomputed transmittance table
float3 getExtinction(float3 camera, float3 _point, float shaftWidth, float scaleCoeff, float irradianceFactor)
{
	float3 extinction = float3(1, 1, 1);
	float3 viewdir = _point - camera;
	float d = length(viewdir) * scaleCoeff;
	viewdir = viewdir / d;
	/////////////////////experimental block begin
	float RtResized = Rg + (Rt - Rg) * _experimentalAtmoScale;
	//                viewdir.x += _viewdirOffset;
	viewdir = normalize(viewdir);
	/////////////////////experimental block end
	float r = length(camera) * scaleCoeff;

	if (r < 0.9 * Rg) {
		camera.y += Rg;
		r = length(camera) * scaleCoeff;
	}

	float rMu = dot(camera, viewdir);
	float mu = rMu / r;

	float dSq = rMu * rMu - r * r + RtResized*RtResized;
	float deltaSq = sqrt(dSq);

	float din = max(-rMu - deltaSq, 0.0);

	if (din > 0.0 && din < d)
	{
		rMu += din;
		mu = rMu / RtResized;
		r = RtResized;
		d -= din;
	}
	if (r <= RtResized && dSq >= 0.0) 
	{ 
		//    	if (r < Rg + 1600.0)
		//    	{
		//    		// avoids imprecision problems in aerial perspective near ground
		//    		//Not sure if necessary with extinction
		//        	float f = (Rg + 1600.0) / r;
		//        	r = r * f;
		//    	}

		r = (r < Rg + 1600.0) ? (Rg + 1600.0) : r; //wtf is this?

		//set to analyticTransmittance only atm
		#if defined (useAnalyticTransmittance)
		extinction = min(AnalyticTransmittance(r, mu, d), 1.0);
		#endif
	}	
	else
	{	//if out of atmosphere
		extinction = float3(1,1,1);
	}

	return extinction;
}

//Extinction for a ray going all the way to the end of the atmosphere
//i.e an infinite ray
//for clouds so no analyticTransmittance required, may change this to use analytic transmittance so that everything is consistent
float3 getSkyExtinction(float3 camera, float3 viewdir) //instead of camera this is the cloud position
{
	float3 extinction = float3(1,1,1);

	float RtResized=Rg+(Rt-Rg)*_experimentalAtmoScale;

	float r = length(camera);
	float rMu = dot(camera, viewdir);
	float mu = rMu / r;

	float dSq = rMu * rMu - r * r + RtResized*RtResized;
	float deltaSq = sqrt(dSq);

	float din = max(-rMu - deltaSq, 0.0);
	if (din > 0.0)
	{
		camera += din * viewdir;
		rMu += din;
		mu = rMu / RtResized;
		r = RtResized;
	}

	extinction = Transmittance(r, mu);

	if (r > RtResized || dSq < 0.0) 
	{
		extinction = float3(1,1,1);
	} 	

	//    //return mu < -sqrt(1.0 - (Rg / r) * (Rg / r)) ? float3(0,0,0) : extinction; //why is this not needed?
	//
	//    float terminatorAngle = -sqrt(1.0 - (Rg / r) * (Rg / r));
	//    //return mu < terminatorAngle ? float3(0,0,0) : extinction;
	//
	//    float index= (mu - (terminatorAngle - cloudTerminatorSmooth)) / (cloudTerminatorSmooth);
	//
	//    if (index>1)
	//    	return extinction;
	//    else if (index < 0)
	//    	return float3(0,0,0);
	//    else
	//    	return lerp (float3(0,0,0),extinction,index);

	return extinction;
}

float3 sunsetExtinction(float3 camera)
{
	return(getSkyExtinction(camera,SUN_DIR));
}

//Extinction for a ray going all the way to the end of the atmosphere
//i.e an infinite ray
//with analyticTransmittance
//doesn't seem to be used
float3 getSkyExtinctionAnaLyticTransmittance(float3 camera, float3 viewdir) //instead of camera this is the cloud position
{
	float3 extinction = float3(1,1,1);

	float RtResized=Rg+(Rt-Rg)*_experimentalAtmoScale;

	float r = length(camera);
	float rMu = dot(camera, viewdir);
	float mu = rMu / r;

	float dSq = rMu * rMu - r * r + RtResized*RtResized;
	float deltaSq = sqrt(dSq);

	float din = max(-rMu - deltaSq, 0.0);
	if (din > 0.0)
	{
		camera += din * viewdir;
		rMu += din;
		mu = rMu / RtResized;
		r = RtResized;
	}

	//extinction = Transmittance(r, mu);

	float distInAtmo = abs(intersectSphereInside(camera,viewdir,float3(0,0,0),RtResized));
	extinction = AnalyticTransmittance(r, mu, distInAtmo);

	if (r > RtResized || dSq < 0.0) 
	{
		extinction = float3(1,1,1);
	} 				

	//return mu < -sqrt(1.0 - (Rg / r) * (Rg / r)) ? float3(0,0,0) : extinction; //why is this not needed?


	//    			float terminatorAngle = -sqrt(1.0 - (Rg / r) * (Rg / r));
	//    			//return mu < terminatorAngle ? float3(0,0,0) : extinction;
	//
	//    			float index= (mu - (terminatorAngle - cloudTerminatorSmooth)) / (cloudTerminatorSmooth);
	//
	//    			if (index>1)
	//    				return extinction;
	//    			else if (index < 0)
	//    				return float3(0,0,0);
	//    			else
	//    				return lerp (float3(0,0,0),extinction,index);

	return extinction;
}

float3 SkyRadiance2(float3 camera, float3 viewdir, float3 sundir, out float3 extinction)
{
	extinction = float3(1,1,1);
	float3 result = float3(0,0,0);

	float RtResized=Rg+(Rt-Rg)*_experimentalAtmoScale;


	//viewdir.x+=_viewdirOffset;
	viewdir=normalize(viewdir);

	//camera *= scale;
	//camera += viewdir * max(shaftWidth, 0.0);
	float r = length(camera);

	float rMu = dot(camera, viewdir);
	rMu+=_viewdirOffset * r;

	float mu = rMu / r;

	//	float r0 = r;
	//	float mu0 = mu;

	float dSq = rMu * rMu - r * r + RtResized*RtResized;
	float deltaSq = sqrt(dSq);

	float din = max(-rMu - deltaSq, 0.0);
	if (din > 0.0)
	{
		camera += din * viewdir;
		rMu += din;
		mu = rMu / RtResized;
		r = RtResized;
	}

	float nu = dot(viewdir, sundir);
	float muS = dot(camera, sundir) / r;

	//	float4 inScatter = Texture4D(_Sky_Inscatter, r, rMu / r, muS, nu);
	float4 inScatter = Texture4D(_Inscatter, r, rMu / r, muS, nu,RtResized);

	extinction = Transmittance(r, mu);

	if (r <= RtResized && dSq >= 0.0)
	{

		//        if (shaftWidth > 0.0) 
		//        {
		//            if (mu > 0.0) {
		//                inScatter *= min(Transmittance(r0, mu0) / Transmittance(r, mu), 1.0).rgbr;
		//            } else {
		//                inScatter *= min(Transmittance(r, -mu) / Transmittance(r0, -mu0), 1.0).rgbr;
		//            }
		//        }

		float3 inScatterM = GetMie(inScatter);
		float phase = PhaseFunctionR(nu);
		float phaseM = PhaseFunctionM(nu);
		result = inScatter.rgb * phase + inScatterM * phaseM;
	}    
	else
	{
		result = float3(0,0,0);
		extinction = float3(1,1,1);
	} 

	result*=_sunColor;
	return result * _Sun_Intensity;

}

//same as 2 but with no extinction
float3 SkyRadiance3(float3 camera, float3 viewdir, float3 sundir)//, out float3 extinction)//, float shaftWidth)
{
	float3 result = float3(0,0,0);

	float RtResized=Rg+(Rt-Rg)*_experimentalAtmoScale;


	//viewdir.x+=_viewdirOffset;
	viewdir=normalize(viewdir);

	//camera *= scale;
	//camera += viewdir * max(shaftWidth, 0.0);
	float r = max(length(camera),Rg+100.0); //fixes artifacts when camera is crossing water surface

	float rMu = dot(camera, viewdir);
	rMu+=_viewdirOffset * r;

	//float mu = rMu / r;

	float dSq = rMu * rMu - r * r + RtResized*RtResized;
	float deltaSq = sqrt(dSq);

	float din = max(-rMu - deltaSq, 0.0);
	if (din > 0.0)
	{
		camera += din * viewdir;
		rMu += din;
		//mu = rMu / RtResized;
		r = RtResized;
	}

	float nu = dot(viewdir, sundir);
	float muS = dot(camera, sundir) / r;

	float4 inScatter = Texture4D(_Inscatter, r, rMu / r, muS, nu,RtResized);

	//extinction = Transmittance(r, mu);

	if (r <= RtResized && dSq >= 0.0) 
	{

		//        if (shaftWidth > 0.0) 
		//        {
		//            if (mu > 0.0) {
		//                inScatter *= min(Transmittance(r0, mu0) / Transmittance(r, mu), 1.0).rgbr;
		//            } else {
		//                inScatter *= min(Transmittance(r, -mu) / Transmittance(r0, -mu0), 1.0).rgbr;
		//            }
		//        }

		float3 inScatterM = GetMie(inScatter);
		float phase = PhaseFunctionR(nu);
		float phaseM = PhaseFunctionM(nu);
		result = inScatter.rgb * phase + inScatterM * phaseM;
	}    
	else
	{
		result = float3(0,0,0);
		//extinction = float3(1,1,1);
	} 

	result*=_sunColor;
	return result * _Sun_Intensity;
}

float2 GetIrradianceUV(float r, float muS) 
{
	float uR = (r - Rg) / (Rt - Rg);
	float uMuS = (muS + 0.2) / (1.0 + 0.2);
	return float2(uMuS, uR);
}

float3 Irradiance(sampler2D samp, float r, float muS) 
{
	float2 uv = GetIrradianceUV(r, muS);  
	return tex2Dlod(samp,float4(uv,0.0,0.0)).rgb;    
}

// incident sky light at given position, integrated over the hemisphere (irradiance)
// r=length(x)
// muS=dot(x,s) / r
float3 SkyIrradiance(float r, float muS)
{	
	return Irradiance(_Irradiance, r, muS) * _sunColor * _Sun_Intensity;
}

// transmittance(=transparency) of atmosphere for infinite ray (r,mu)
// (mu=cos(view zenith angle)), or zero if ray intersects ground
// Change to analytic?
float3 TransmittanceWithShadow(float r, float mu) 
{
	return mu < -sqrt(1.0 - (Rg / r) * (Rg / r)) ? float3(0,0,0) : Transmittance(r, mu);
}

// incident sun light at given position (radiance)
// r=length(x)
// muS=dot(x,s) / r
float3 SunRadiance(float r, float muS)
{
	return TransmittanceWithShadow(r, muS) * _sunColor * _Sun_Intensity;
}

void SunRadianceAndSkyIrradiance(float3 worldP, float3 worldN, float3 worldS, out float3 sunL, out float3 skyE)
{
	//	worldP *= scale;
	float r = length(worldP);
	if (r < 0.9 * Rg) {
		worldP.z += Rg;
		r = length(worldP);
	}
	float3 worldV = worldP / r; // vertical vector
	float muS = dot(worldV, worldS);

	float sunOcclusion = 1.0;// - sunShadow;
	//sunL = SunRadiance(r, muS) * sunOcclusion;
	sunL = TransmittanceWithShadow(r, muS) * _sunColor * _Sun_Intensity;//removed _Sun_Intensity multiplier

	// ambient occlusion due only to slope, does not take self shadowing into account
	float skyOcclusion = (1.0 + dot(worldV, worldN)) * 0.5;
	// factor 2.0 : hack to increase sky contribution (numerical simulation of
	// "precompued atmospheric scattering" gives less luminance than in reality)
	skyE = 2.0 * SkyIrradiance(r, muS) * skyOcclusion;
}

float3 SimpleSkyirradiance(float3 worldP, float3 worldN, float3 worldS)
{
	float r = length(worldP);
	if (r < 0.9 * Rg) {
		worldP.z += Rg;
		r = length(worldP);
	}
	float3 worldV = worldP / r; // vertical vector
	float muS = dot(worldV, worldS);

	// ambient occlusion due only to slope, does not take self shadowing into account
	float skyOcclusion = (1.0 + dot(worldV, worldN)) * 0.5;
	// factor 2.0 : hack to increase sky contribution (numerical simulation of
	// "precompued atmospheric scattering" gives less luminance than in reality)
	float3 skyE = 2.0 * SkyIrradiance(r, muS) * skyOcclusion;

	return skyE;
}

//InScattering with modified atmo heights
float3 InScattering2(float3 camera, float3 _point, float3 sunDir, out float3 extinction) {
	// single scattered sunlight between two points
	// camera=observer
	// point=point on the ground
	// sundir=unit vector towards the sun
	// return scattered light and extinction coefficient
	float3 result = float3(0, 0, 0);
	extinction=1.0;
	float3 viewdir = _point - camera;
	float d = length(viewdir);
	viewdir = viewdir / d;

	float RtResized = Rg + (Rt - Rg) * _experimentalAtmoScale;
	viewdir = normalize(viewdir);

	float r = length(camera);
	if (r < 0.9 * Rg)
	{
	        camera.y += Rg;
	        _point.y += Rg;
	        r = length(camera);
	}

	float rMu = dot(camera, viewdir);
	float mu = rMu / r;
	float r0 = r;
	float mu0 = mu;
	float muExtinction=mu;
	_point -= viewdir * clamp(1.0, 0.0, d);
	float dSq = rMu * rMu - r * r + RtResized*RtResized;
	float deltaSq = sqrt(dSq);
	float din = max(-rMu - deltaSq, 0.0);
	if (din > 0.0 && din < d)
	{
		camera += din * viewdir;
		rMu += din;
		mu = rMu / RtResized;
		r = RtResized;
		d -= din;
	}

	if (r <= RtResized && dSq >= 0.0) 
	{ 
		float nu = dot(viewdir, sunDir);
		float muS = dot(camera, sunDir) / r;
		float4 inScatter;
		if (r < Rg + 1600.0) {
			// avoids imprecision problems in aerial perspective near ground
			float f = (Rg + 1600.0) / r;
			r = r * f;
			rMu = rMu * f;
			_point = _point * f;
		}
		float r1 = length(_point);
		float rMu1 = dot(_point, viewdir);
		float mu1 = rMu1 / r1;
		float muS1 = dot(_point, sunDir) / r1;

		//        #if defined (useAnalyticTransmittance)
		extinction = min(AnalyticTransmittance(r, mu, d), 1.0);
		//#else
		//                            if (mu > 0.0)
		//                            {
		//                                    extinction = min(Transmittance(r, mu, Rt) / Transmittance(r1, mu1, Rt), 1.0);
		//                            }
		//                                else
		//                            {
		//                                    extinction = min(Transmittance(r1, -mu1, Rt) / Transmittance(r, -mu, Rt), 1.0);
		//                            }

//		        #endif
//		#ifdef useHorizonHack
		//reduces it but doesn't fix it, have to try to get the other one
		        //const float EPS = 0.004;
			const float EPS = 0.01;
		        float lim = -sqrt(1.0 - (Rg / r) * (Rg / r));
		        if (abs(mu - lim) < EPS)
			{
				//float a = ((mu - lim) + EPS) / (2.0 * EPS);
				float a = saturate(2.0 * ((mu - lim) + EPS) / (2.0 * EPS));

				//these make inScatterA black, let's try without and increase EPS to 0.01
				//almost does it, still a faint outline of it visible, maybe debug if sqrt doesn't need to be 0 below?
			        //trying 0.04, nope, still a faint outline visible
				//back to 0.01 with sqrt fix, doesn't work either, weird, maybe extinction is too strong?

//				mu = lim - EPS;
//				r1 = r * r + d * d + 2.0 * r * d * mu;
//				r1 = r1 > 0.0 ? sqrt(r1) : 1e30;
//				mu1 = (r * mu + d) / r1;

				float4 inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu, RtResized);
				float4 inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu, RtResized);
				float4 inScatterA = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);

				mu = lim + EPS;
				r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
				mu1 = (r * mu + d) / r1;

				inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu, RtResized);
				inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu, RtResized);
				float4 inScatterB = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);

				inScatter = lerp(inScatterA, inScatterB, a);
		        }
		        else
//		#endif
			{
				float4 inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu,RtResized);
				float4 inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu,RtResized);
				inScatter = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
			}

		
		// avoids imprecision problems in Mie scattering when sun is below horizon
		inScatter.w *= smoothstep(0.00, 0.02, muS);
		float3 inScatterM = GetMie(inScatter);
		float phase = PhaseFunctionR(nu);
		float phaseM = PhaseFunctionM(nu);
		result = inScatter.rgb * phase + inScatterM * phaseM;
	}
	else
	{	//if out of atmosphere
		result = float3(0,0,0);
		//        extinction = float3(1,1,1);
	}

	return result * _sunColor * _Sun_Intensity;
}
