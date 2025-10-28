// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'

Shader "Scatterer/CausticsOcclusion"
{
    Properties
    {
    	_CausticsTexture ("_CausticsTexture", 2D) = "" {}
    	layer1Scale ("Layer 1 scale", Vector) = (1.0101,1.0101,0)
    	layer1Speed ("Layer 1 speed", Vector) = (0.05123,0.05123,0)
    	layer2Scale ("Layer 2 scale", Vector) = (1.235487,1.235487,0)
    	layer2Speed ("Layer 2 speed", Vector) = (0.074872,0.074872,0)
    	causticsMultiply ("causticsMultiply", Float) = 1
    	causticsMinBrightness ("causticsMinBrightness", Float) = 0.1
    }

	SubShader
	{
		Pass
		{
			Cull Back ZWrite Off ZTest Off
			BlendOp Min //take the minimum so existing shadows that are not completely at zero are respected

			CGPROGRAM
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "../../DepthCommon.cginc"

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile SPHERE_PLANET FLAT_PLANET

			float4x4 CameraToWorld;
			float4x4 WorldToLight;

			float3 PlanetOrigin;
			float oceanRadius;

			sampler2D _CausticsTexture;

			float2 layer1Scale;
			float2 layer1Speed;

			float2 layer2Scale;
			float2 layer2Speed;

			float causticsMultiply;
			float causticsMinBrightness;
			float causticsBlurDepth;

			float warpTime;

			struct v2f 
			{
    			float4  pos : SV_POSITION;
    			float2  uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
    			v2f OUT;
    			OUT.pos = UnityObjectToClipPos(v.vertex);
			OUT.uv = ComputeScreenPos(OUT.pos);

    			return OUT;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float zdepth = tex2Dlod(_CameraDepthTexture, float4(i.uv,0,0));

#if SHADER_API_D3D11 || SHADER_API_D3D || SHADER_API_D3D12
				if (zdepth == 0.0) {discard;}
#else
				if (zdepth == 1.0) {discard;}
#endif

				float3 worldPos = getPreciseWorldPosFromDepth(i.uv, zdepth, CameraToWorld);

				// blur caustics the farther we are from texture
				float blurFactor = 0.0;
				float time = _Time.y;

#ifdef SPHERE_PLANET
				float underwaterDepth = max(oceanRadius - length(worldPos - PlanetOrigin), 0.0);
				blurFactor = lerp(0.0,5.0,underwaterDepth/causticsBlurDepth);
				time = (warpTime == 0.0) ? time : warpTime;
#else
				blurFactor = lerp(0.0,5.0,-worldPos.y/30.0);
#endif

				float2 uvCookie = mul(WorldToLight, float4(worldPos.xyz, 1)).xy;

    				float2 uvSample1 = layer1Scale * uvCookie + layer1Speed * float2(time,time);
				float2 uvSample2 = layer2Scale * uvCookie + layer2Speed * float2(time,time);

				float causticsSample1 = tex2Dbias(_CausticsTexture,float4(uvSample1,0.0,blurFactor)).r;
				float causticsSample2 = tex2Dbias(_CausticsTexture,float4(uvSample2,0.0,blurFactor)).r;

				float caustics = causticsMultiply*min(causticsSample1,causticsSample2)+causticsMinBrightness;
#ifdef SPHERE_PLANET
				caustics = lerp (1.0, caustics, clamp(underwaterDepth/1.5f, 0.0, 1.0)); //fade caustics in over the first meter and half of depth
#endif
				return float4(caustics,caustics,caustics,1.0);
			}
			ENDCG
		}
	}
}