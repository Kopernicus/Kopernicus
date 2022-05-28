Shader "Scatterer/CausticsGodraysRaymarch"
{
	Properties
	{
		_CausticsTexture ("_CausticsTexture", 2D) = "" {}
		layer1Scale ("Layer 1 scale", Vector) = (1.0101,1.0101,0)
		layer1Speed ("Layer 1 speed", Vector) = (0.05123,0.05123,0)
		layer2Scale ("Layer 2 scale", Vector) = (1.235487,1.235487,0)
		layer2Speed ("Layer 2 speed", Vector) = (0.074872,0.074872,0)
	}

	SubShader
	{
		Pass
		{
			ZTest On
			ZWrite Off

			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#pragma multi_compile SPHERE_PLANET FLAT_PLANET

			#include "UnityCG.cginc"
			#include "Lighting.cginc"		
			#include "AutoLight.cginc"
			#include "../OceanShadows.cginc"
			#include "../../CommonAtmosphere.cginc"

			float4x4 CameraToWorld;
			float4x4 WorldToLight;

			float3 LightDir;
			float3 PlanetOrigin;
			float oceanRadius;
			float transparencyDepth;
			float lightRaysStrength;

			sampler2D _CausticsTexture;

			float2 layer1Scale;
			float2 layer1Speed;

			float2 layer2Scale;
			float2 layer2Speed;

			float causticsBlurDepth;

			float warpTime;

			sampler2D ScattererDownscaledDepth;
			float4 ScattererDownscaledDepth_TexelSize;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD1;

			};
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv  = ComputeNonStereoScreenPos(o.pos);
				o.screenPos = ComputeScreenPos(o.pos);

				return o;
			}

			sampler2D _MainTex;

			float4 frag (v2f IN) : SV_Target
			{
				// blur caustics the farther we are from surface
				float blurFactor = 0.0;
				float time = _Time.y;
				time = (warpTime == 0.0) ? time : warpTime;

				float totalCaustics = 0;
				float currentDistance = 0;

				float3 worldPosition = _WorldSpaceCameraPos;

				float2 uv = IN.screenPos.xy/IN.screenPos.w;
				float zdepth =  tex2Dlod(ScattererDownscaledDepth, float4(uv,0.0,0.0));
#ifdef SHADER_API_D3D11  //#if defined(UNITY_REVERSED_Z)
				zdepth = 1 - zdepth;
#endif
				float4 clipPos = float4(uv, zdepth, 1.0);
				clipPos.xyz = 2.0f * clipPos.xyz - 1.0f;
				float4 camPos = mul(unity_CameraInvProjection, clipPos);

				float4 fragWorldPos = mul(CameraToWorld,camPos);
				fragWorldPos/=fragWorldPos.w;

				float3 viewDir = normalize(fragWorldPos.xyz - worldPosition);

				float maxDistance = intersectSphereInside (worldPosition, viewDir, PlanetOrigin, oceanRadius);
				maxDistance = min(maxDistance, length(camPos.xyz / camPos.w));
				maxDistance = min(maxDistance, transparencyDepth * 2.0);

				float dotLight = dot(viewDir,-LightDir);
				dotLight=pow(abs(dotLight),40) * sign(dotLight);
				float boostFactor = (dotLight<0.0) ? -0.0085 : 0.005 ;

				float GRID_SIZE = 8;
				float GRID_SIZE_SQR_RCP = (1.0/(GRID_SIZE*GRID_SIZE));
				float stepSize = 0.2; //maybe change this to 0.007?
				// Calculate the offsets on the ray according to the interleaved sampling pattern
				float2 interleavedPos = fmod( float2(IN.pos.x, ScattererDownscaledDepth_TexelSize.w - IN.pos.y), GRID_SIZE );
				float rayStartOffset = ( interleavedPos.y * GRID_SIZE + interleavedPos.x ) * ( stepSize * GRID_SIZE_SQR_RCP ) ;

				worldPosition+=(0.1+rayStartOffset)*viewDir;

				float fadeOut = 1.0;
#ifdef SPHERE_PLANET
				float underwaterDepth = max(oceanRadius - length(worldPosition - PlanetOrigin), 0.0);
				blurFactor = lerp(0.0,5.0,underwaterDepth/causticsBlurDepth);

				fadeOut = smoothstep(0.0,0.4,1.0-(underwaterDepth/causticsBlurDepth)); //fade them out starting from last 40% of causticsBlurDepth
#else
				blurFactor = lerp(0.0,5.0,worldPosition.y/30.0);
#endif

				float2 layer1Offset = layer1Speed * float2(time,time);
				float2 layer2Offset = layer2Speed * float2(time,time);

				for (int i=0;i<150;i++)
				{	
					float2 uvCookie = mul(WorldToLight, float4(worldPosition, 1)).xy;

					float2 uvSample1 = layer1Scale * uvCookie + layer1Offset;
					float2 uvSample2 = layer2Scale * uvCookie + layer2Offset;

					float causticsSample1 = tex2Dlod(_CausticsTexture,float4(uvSample1,0.0,blurFactor)).r;
					float causticsSample2 = tex2Dlod(_CausticsTexture,float4(uvSample2,0.0,blurFactor)).r;

					float caustics = 0.01*lightRaysStrength*min(causticsSample1,causticsSample2);

					float4 clipPos = mul(UNITY_MATRIX_VP,float4(worldPosition,1.0));
					float4 screenPos = ComputeScreenPos(clipPos);
					float shadowTerm = getOceanHardShadow(float4(worldPosition,1.0),-screenPos.z);

					totalCaustics+=(1.0-totalCaustics)*caustics*shadowTerm;
					float increment = 0.2 + boostFactor*i*dotLight;   //variable step size, boost when looking towards light source or direction to make it look like it's properly coming from infinity
										
					currentDistance+=increment;
					worldPosition+=viewDir*increment;
//
					if (currentDistance > maxDistance)
					{
						break;
					}
				}

				totalCaustics*=fadeOut;

				return float4(totalCaustics,totalCaustics,totalCaustics,1.0);
			}
			ENDCG
		}
	}

}