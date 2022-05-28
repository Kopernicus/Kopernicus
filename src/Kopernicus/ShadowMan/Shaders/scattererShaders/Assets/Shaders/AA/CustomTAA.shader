// Modified TAA shader to not blur out the scatterer ocean

Shader "Scatterer/TemporalAntialiasing" {
	SubShader {
		Tags {"Queue" = "Transparent-499" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		Pass {
			Tags {"Queue" = "Transparent-499" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

			Cull Off
			ZTest Off
			ZWrite Off

			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			#pragma multi_compile CUSTOM_OCEAN_OFF CUSTOM_OCEAN_ON

			sampler2D _ScreenColor;
			float4 _ScreenColor_TexelSize;

			sampler2D _HistoryTex;

			sampler2D _CameraDepthTexture;
			float4 _CameraDepthTexture_TexelSize;

			sampler2D _CameraMotionVectorsTexture;

#if defined (CUSTOM_OCEAN_ON)
			uniform sampler2D ScattererDepthCopy;
#endif

			float2 _Jitter;
			float4 _FinalBlendParameters; // x: static, y: dynamic, z: motion amplification
			float _Sharpness;

			// Constants
			#define HALF_MAX        65504.0 // (2 - 2^-10) * 2^15
			#define HALF_MAX_MINUS1 65472.0 // (2 - 2^-9) * 2^15
			#define EPSILON         1.0e-4
			#define PI              3.14159265359
			#define TWO_PI          6.28318530718
			#define FOUR_PI         12.56637061436
			#define INV_PI          0.31830988618
			#define INV_TWO_PI      0.15915494309
			#define INV_FOUR_PI     0.07957747155
			#define HALF_PI         1.57079632679
			#define INV_HALF_PI     0.636619772367

			#define FLT_EPSILON     1.192092896e-07 // Smallest positive number, such that 1.0 + FLT_EPSILON != 1.0
			#define FLT_MIN         1.175494351e-38 // Minimum representable positive floating-point number
			#define FLT_MAX         3.402823466e+38 // Maximum representable floating-point number

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 screenPos : TEXCOORD0;

			};
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.pos);

				return o;
			}

			float2 GetClosestFragment(float2 uv)
			{
				float2 k = _CameraDepthTexture_TexelSize.xy;

				float4 neighborhood = float4(
					tex2D(_CameraDepthTexture, uv - k).r,
					tex2D(_CameraDepthTexture, uv + float2(k.x, -k.y)).r,
					tex2D(_CameraDepthTexture, uv + float2(-k.x, k.y)).r,
					tex2D(_CameraDepthTexture, uv + k).r
				);

				#if SHADER_API_D3D11
				#define COMPARE_DEPTH(a, b) step(b, a)
				#else
				#define COMPARE_DEPTH(a, b) step(a, b)
				#endif

				float3 result = float3(0.0, 0.0, SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
				result = lerp(result, float3(-1.0, -1.0, neighborhood.x), COMPARE_DEPTH(neighborhood.x, result.z));
				result = lerp(result, float3( 1.0, -1.0, neighborhood.y), COMPARE_DEPTH(neighborhood.y, result.z));
				result = lerp(result, float3(-1.0,  1.0, neighborhood.z), COMPARE_DEPTH(neighborhood.z, result.z));
				result = lerp(result, float3( 1.0,  1.0, neighborhood.w), COMPARE_DEPTH(neighborhood.w, result.z));

				return (uv + result.xy * k);
			}

			float3 Min3(float3 a, float3 b, float3 c)
			{
				return min(min(a, b), c);
			}

			float4 ClipToAABB(float4 color, float3 minimum, float3 maximum)
			{
				// Note: only clips towards aabb center (but fast!)
				float3 center = 0.5 * (maximum + minimum);
				float3 extents = 0.5 * (maximum - minimum);

				// This is actually `distance`, however the keyword is reserved
				float3 offset = color.rgb - center;

				float3 ts = abs(extents / (offset + 0.0001));
				float t = saturate(Min3(ts.x, ts.y, ts.z));
				color.rgb = center + offset * t;
				return color;
			}

			float4 Solve(float2 motion, float2 texcoord)
			{
				const float2 k = _ScreenColor_TexelSize.xy;
				float2 uv = texcoord - _Jitter;

				float4 color = tex2Dlod(_ScreenColor, float4(uv,0.0,0.0));

				float4 topLeft = tex2D(_ScreenColor, (uv - k * 0.5));
				float4 bottomRight = tex2D(_ScreenColor, (uv + k * 0.5));

				float4 corners = 4.0 * (topLeft + bottomRight) - 2.0 * color;

				// Sharpen output
				color += (color - (corners * 0.166667)) * 2.718282 * _Sharpness;
				color = clamp(color, 0.0, HALF_MAX_MINUS1);

				// Tonemap color and history samples
				float4 average = (corners + color) * 0.142857;

				float4 history = tex2D(_HistoryTex, texcoord - motion);

				float motionLength = length(motion);
				float2 luma = float2(Luminance(average), Luminance(color));
				//float nudge = 4.0 * abs(luma.x - luma.y);
				float nudge = lerp(4.0, 0.25, saturate(motionLength * 100.0)) * abs(luma.x - luma.y);

				float4 minimum = min(bottomRight, topLeft) - nudge;
				float4 maximum = max(topLeft, bottomRight) + nudge;

				// Clip history samples
				history = ClipToAABB(history, minimum.xyz, maximum.xyz);

				// Blend method
				float weight = clamp(
					lerp(_FinalBlendParameters.x, _FinalBlendParameters.y, motionLength * _FinalBlendParameters.z),
					_FinalBlendParameters.y, _FinalBlendParameters.x
				);

#if defined (CUSTOM_OCEAN_ON)
				float oceanDepth = tex2Dlod(ScattererDepthCopy, float4(uv,0,0));
				float zdepth = tex2Dlod(_CameraDepthTexture, float4(uv,0,0));

				if (oceanDepth != zdepth)
				{					
					weight*= 0.35; //seems to be the best of both worlds, still antialiases but doesn't blur the ocean much
				}
#endif

				color = lerp(color, history, weight);
				color = clamp(color, 0.0, HALF_MAX_MINUS1);

				return color;
			}

			float4 frag(v2f i) : SV_Target
			{
				float2 uv = i.screenPos.xy / i.screenPos.w;

#if SHADER_API_D3D11 || SHADER_API_D3D || SHADER_API_D3D12
				if (_ProjectionParams.x > 0) {uv.y = 1.0 - uv.y;}
#endif
				float2 closest = GetClosestFragment(uv);
				float2 motion = tex2Dlod(_CameraMotionVectorsTexture, float4(closest,0.0,0.0)).xy;

				return Solve(motion, uv);
			}
			ENDCG
		}
	}

}