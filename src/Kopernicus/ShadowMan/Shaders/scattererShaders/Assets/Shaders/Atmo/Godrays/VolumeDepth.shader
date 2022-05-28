Shader "Scatterer/VolumeDepth"
{
	Properties
	{

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{

			Cull Off

			ZWrite Off
			ZTest Always
			Blend One One

//			Cull Off
//			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma hull hull
			#pragma domain domain

			#include "UnityCG.cginc"
			#include "UnityShadowLibrary.cginc"
			#include "ShadowVolumeUtils.cginc"
			#include "../../IntersectCommon.cginc"
			#include "GodraysCommon.cginc"

			//if no ocean -> use camera depth buffer: OCEAN_INTERSECT_OFF
			//if ocean and in depth buffer mode -> use ocean depth buffer: OCEAN_INTERSECT_DEPTH
			//if ocean and in projector mode -> use analytical ocean intersect: OCEAN_INTERSECT_ANALYTICAL
			#pragma multi_compile OCEAN_INTERSECT_OFF OCEAN_INTERSECT_ANALYTICAL 
			#pragma multi_compile CLOUDSMAP_OFF CLOUDSMAP_ON
			#pragma multi_compile DOWNSCALE_DEPTH_OFF DOWNSCALE_DEPTH_ON

			sampler2D _ShadowMapTextureCopyScatterer;

			sampler2D _CameraDepthTexture;
			float4 _CameraDepthTexture_TexelSize;

			float4x4 CameraToWorld;

			float3 lightDirection;

			float3 _planetPos;
			float Rg;
			float Rt;
			float _experimentalAtmoScale;

			sampler2D cloudShadowMap;

			StructuredBuffer<float4x4> inverseShadowMatricesBuffer;

			float4x4 lightToWorld;


			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2t //vertex 2 tesselation
			{
				float4 pos : INTERNALTESSPOS;
				float4 worldOriginPos : TEXCOORD0;
				float4 worldEndPos : TEXCOORD1;
			};

			struct d2f //domain 2 frag
			{
				float4 pos : SV_POSITION;
				float4 viewPos: TEXCOORD0;
				float4 projPos : TEXCOORD1;
				float4 finalWorldPos : TEXCOORD2;
				float isCloud : TEXCOORD3;
			};

			v2t vert (appdata v)
			{
				v2t o;

				//o.pos = float4(2.0*(v.uv-0.5),0.0,1.0); //try modifying this, clip space goes from -1 to 1, our UVs go from?to?
				//o.pos = float4(3.0*(v.uv-0.5),0.0,1.0); //HOLY FUCK THIS FIXES THE DISAPPEARING CHUNKS WHAT THE FUCK
				o.pos = float4(2.9*(v.uv-0.5),0.0,1.0);

				o.worldOriginPos = mul(lightToWorld,o.pos);
				o.worldEndPos = mul(lightToWorld,float4(o.pos.x,o.pos.y,1.0,1.0));

				return o;
			}

			struct OutputPatchConstant
			{
				float edge[3]: SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			OutputPatchConstant tesselationConstants(InputPatch<v2t,3> patch)
			{
				OutputPatchConstant o;

				bool isVisible = false;
				for (int i=0;i<3;i++)
				{
					float4 clipSpaceOrigin = mul(UNITY_MATRIX_VP, patch[i].worldOriginPos);
					float4 clipSpaceEnd    = mul(UNITY_MATRIX_VP, patch[i].worldEndPos);
					isVisible = isVisible || intersectsFrustum(clipSpaceOrigin.xyz/clipSpaceOrigin.w, clipSpaceEnd.xyz/clipSpaceEnd.w); //consider this to be working though I'm not sure, maybe pass a color?
				}

				if (!isVisible)
				{
					o.edge[0] = 0.0;
					o.edge[1] = 0.0;
					o.edge[2] = 0.0;
					o.inside = 0.0;
				}
				else
				{
					float maxTesselationFactor = 64.0;

					float distEdge0 = rayDistanceToPoint(0.5*(patch[1].worldOriginPos+patch[2].worldOriginPos), lightDirection, _WorldSpaceCameraPos);
					float distEdge1 = rayDistanceToPoint(0.5*(patch[2].worldOriginPos+patch[0].worldOriginPos), lightDirection, _WorldSpaceCameraPos);
					float distEdge2 = rayDistanceToPoint(0.5*(patch[0].worldOriginPos+patch[1].worldOriginPos), lightDirection, _WorldSpaceCameraPos);

					float factor0 = clamp(maxTesselationFactor * 225.0 / distEdge0,1.0,maxTesselationFactor);
					float factor1 = clamp(maxTesselationFactor * 225.0 / distEdge1,1.0,maxTesselationFactor);
					float factor2 = clamp(maxTesselationFactor * 225.0 / distEdge2,1.0,maxTesselationFactor);

//					float factor0 = clamp(maxTesselationFactor * 225.0 / distEdge0,4.0,maxTesselationFactor); // min of 4.0 is slooooow
//					float factor1 = clamp(maxTesselationFactor * 225.0 / distEdge1,4.0,maxTesselationFactor);
//					float factor2 = clamp(maxTesselationFactor * 225.0 / distEdge2,4.0,maxTesselationFactor);

					//float insideFactor = max(factor0, max(factor1,factor2)); //doesn't seem to be always correct, so try to think about it, think about what causes cracks in this case, it's normal, every triangle's max isn't necessarily the adjacent triangle's max

					o.edge[0] = factor0;
					o.edge[1] = factor1;
					o.edge[2] = factor2;
					o.inside  = (factor0 + factor1 + factor2) / 3.0;  //doesn't seem to be always correct, so try to think about it, think about what causes cracks in this case, it's normal, every triangle's max isn't necessarily the adjacent triangle's max
				}

				return o;
			}

			[domain("tri")]
			[partitioning("integer")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("tesselationConstants")]
			[outputcontrolpoints(3)]
			v2t hull(InputPatch<v2t,3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}
				
			[domain("tri")]
			d2f domain(OutputPatchConstant tessFactors,const OutputPatch<v2t,3> vs, float3 d:SV_DomainLocation)
			{
				d2f o;
				float3 worldPos = vs[0].worldOriginPos.xyz/vs[0].worldOriginPos.w * d.x + vs[1].worldOriginPos.xyz/vs[1].worldOriginPos.w * d.y + vs[2].worldOriginPos.xyz/vs[2].worldOriginPos.w * d.z;

				float4 finalWorldPos = 0;
				float4 shadowPos = 0;

				float isCloud = 0;
				//only support 4 cascades here
#if defined(CLOUDSMAP_ON)
				fixed cascadeIndex = pickMostDetailedCascadeCloud (float4(worldPos,1.0), shadowPos, _ShadowMapTextureCopyScatterer, cloudShadowMap, isCloud);
#else
				fixed cascadeIndex = pickMostDetailedCascade (float4(worldPos,1.0), shadowPos, _ShadowMapTextureCopyScatterer);		
#endif

				float4x4 shadowToWorld = inverseShadowMatricesBuffer[max(cascadeIndex,0)];

				finalWorldPos = mul( shadowToWorld, shadowPos);

				if (cascadeIndex < 0.0)
				{
					//finalWorldPos = float4(_WorldSpaceCameraPos.xyz + lightDirection * 700000,1.0); //this works well for stock but breaks with parallax, what gives?
					finalWorldPos = float4(_WorldSpaceCameraPos.xyz + lightDirection * 500000,1.0);
				}

				o.pos=UnityWorldToClipPos(finalWorldPos);
				o.viewPos = float4(UnityWorldToViewPos(finalWorldPos),1.0);
				o.projPos = ComputeScreenPos(o.pos);
				o.finalWorldPos = finalWorldPos;
				o.isCloud = isCloud;

				return o;
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

			//gets the min depth for surrounding 4 depth texels, in downscaled case
			float getMinDepth(float2 uv)
			{
				float2 texelSize = 0.5 * _CameraDepthTexture_TexelSize.xy;
				float2 taps[4] = { float2(uv + float2(-1,-1)*texelSize), float2(uv + float2(-1,1)*texelSize),
						   float2(uv + float2(1,-1)*texelSize),  float2(uv + float2(1,1)*texelSize)  };

				float depth1 = tex2D(_CameraDepthTexture, taps[0]).r;
				float depth2 = tex2D(_CameraDepthTexture, taps[1]).r;
				float depth3 = tex2D(_CameraDepthTexture, taps[2]).r;
				float depth4 = tex2D(_CameraDepthTexture, taps[3]).r;

				//Only return zero if all samples are zero, otherwise return the smallest which isn't zero
				float result = depth4;
				result = (result == 0.0) || (depth3 == 0.0) ? max(result, depth3) : min (result,depth3);
				result = (result == 0.0) || (depth2 == 0.0) ? max(result, depth2) : min (result,depth2);
				result = (result == 0.0) || (depth1 == 0.0) ? max(result, depth1) : min (result,depth1);

				return result;
			}

			float4 frag (d2f i, fixed facing : VFACE) : SV_Target
			{

				float2 depthUV = i.projPos.xy/i.projPos.w;

#if defined(DOWNSCALE_DEPTH_ON)
				float zdepth = getMinDepth(depthUV);
#else
				float zdepth = tex2Dlod(_CameraDepthTexture, float4(depthUV,0,0));
#endif

				float linearDepth = Linear01Depth(zdepth);

#ifdef SHADER_API_D3D11  //#if defined(UNITY_REVERSED_Z)
				zdepth = 1 - zdepth;
#endif

				float4 depthClipPos = float4(depthUV, zdepth, 1.0);
				depthClipPos.xyz = 2.0 * depthClipPos.xyz - 1.0;
				float4 depthViewPos = mul(unity_CameraInvProjection, depthClipPos);

				depthViewPos.xyz /= depthViewPos.w;

				float3 rayDirection = normalize(depthViewPos.xyz);

				float3 cameraForwardDir = float3(0,0,-1);
				float aa = dot(rayDirection, cameraForwardDir);

				depthViewPos.xyz = rayDirection * linearDepth/aa * _ProjectionParams.z;

				float depthLength = length(depthViewPos.xyz);

				float3 viewDir = normalize(i.finalWorldPos.xyz/i.finalWorldPos.w - _WorldSpaceCameraPos);
				float viewLength = length(i.viewPos.xyz/i.viewPos.w);

				if (zdepth != 1.0)	//cap by terrain distance
				{
					viewLength = min(depthLength, viewLength);
				}
//				else 			//ray going into the sky
//				{

//					if (i.isCloud > 0.0)
//					{
//						viewLength*= 0.3; //this looks nice against the sky but where terrain and shadow meet it looks like shite
//					}
//				}

#if defined(OCEAN_INTERSECT_ANALYTICAL)  //cap by boundary to ocean
				float oceanIntersectDistance = intersectSphereOutside(_WorldSpaceCameraPos, viewDir, _planetPos, Rg);

				if ((oceanIntersectDistance > 0.0) && (oceanIntersectDistance <= viewLength))
				{
					viewLength = oceanIntersectDistance;
				}
#endif
				float mu = dot(normalize(_WorldSpaceCameraPos-_planetPos), viewDir);
				viewLength = OpticalDepth(_experimentalAtmoScale * (Rt-Rg) * 0.5, length(_WorldSpaceCameraPos-_planetPos), mu, viewLength);

				viewLength = writeGodrayToTexture(viewLength);

				return facing > 0 ? viewLength : -viewLength;
			}
			ENDCG
		}
	}
}