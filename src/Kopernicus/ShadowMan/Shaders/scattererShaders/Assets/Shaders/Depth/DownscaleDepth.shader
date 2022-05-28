Shader "Scatterer/DownscaleDepth"
{
	SubShader
	{
		Pass //Pass 0, downscale default buffer to 1/4
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _CameraDepthTexture;
			float4 _CameraDepthTexture_TexelSize; // (1.0/width, 1.0/height, width, height)

			v2f vert( appdata_img v )
			{
				v2f o = (v2f)0;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;

				return o;
			}

			float4 frag(v2f input) : SV_Target
			{
				float2 texelSize = 0.5 * _CameraDepthTexture_TexelSize.xy;
				float2 taps[4] = {     float2(input.uv + float2(-1,-1)*texelSize),
					float2(input.uv + float2(-1,1)*texelSize),
					float2(input.uv + float2(1,-1)*texelSize),
					float2(input.uv + float2(1,1)*texelSize) };

				float depth1 = tex2D(_CameraDepthTexture, taps[0]).r;
				float depth2 = tex2D(_CameraDepthTexture, taps[1]).r;
				float depth3 = tex2D(_CameraDepthTexture, taps[2]).r;
				float depth4 = tex2D(_CameraDepthTexture, taps[3]).r;

				//float result = min(depth1, min(depth2, min(depth3, depth4))); //takes min depth, for reverse Z equivalent to taking farthest, may or may not be better for depth discontinuities, test both
				//good but should eliminate samples with depth 0.0

//				//Only return zero if all samples are zero, otherwise return the smallest which isn't zero
				float result = depth4;
				result = (result == 0.0) || (depth3 == 0.0) ? max(result, depth3) : min (result,depth3);
				result = (result == 0.0) || (depth2 == 0.0) ? max(result, depth2) : min (result,depth2);
				result = (result == 0.0) || (depth1 == 0.0) ? max(result, depth1) : min (result,depth1);

				return result;
			}

			ENDCG
		}

		Pass //Pass 1, downscale other buffer to a further 1/4 (so 1/16 final)
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D ScattererDownscaledDepthIntermediate;
			float4 ScattererDownscaledDepthIntermediate_TexelSize; // (1.0/width, 1.0/height, width, height)

			v2f vert( appdata_img v )
			{
				v2f o = (v2f)0;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;

				return o;
			}

			float4 frag(v2f input) : SV_Target
			{
				float2 texelSize = 0.5 * ScattererDownscaledDepthIntermediate_TexelSize.xy;
				float2 taps[4] = {     float2(input.uv + float2(-1,-1)*texelSize),
					float2(input.uv + float2(-1,1)*texelSize),
					float2(input.uv + float2(1,-1)*texelSize),
					float2(input.uv + float2(1,1)*texelSize) };

				float depth1 = tex2D(ScattererDownscaledDepthIntermediate, taps[0]).r;
				float depth2 = tex2D(ScattererDownscaledDepthIntermediate, taps[1]).r;
				float depth3 = tex2D(ScattererDownscaledDepthIntermediate, taps[2]).r;
				float depth4 = tex2D(ScattererDownscaledDepthIntermediate, taps[3]).r;

				//Only return zero if all samples are zero, otherwise return the smalles which isn't zero
				float result = depth4;
				result = (result == 0.0) || (depth3 == 0.0) ? max(result, depth3) : min (result,depth3);
				result = (result == 0.0) || (depth2 == 0.0) ? max(result, depth2) : min (result,depth2);
				result = (result == 0.0) || (depth1 == 0.0) ? max(result, depth1) : min (result,depth1);

				return result;
			}

			ENDCG
		}

		Pass //Pass 2, perform bilateral blurring
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D ScattererDownscaledDepth;
			float4 ScattererDownscaledDepth_TexelSize; // (1.0/width, 1.0/height, width, height)

			float2 BlurDir;

			sampler2D TextureToBlur;
			float4 TextureToBlur_TexelSize;

			v2f vert( appdata_img v )
			{
				v2f o = (v2f)0;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;

				return o;
			}

			float4 frag(v2f input) : SV_Target
			{
				const float offset[4] = { 0, 1, 2, 3 };
				const float weight[4] = { 0.266, 0.213, 0.1, 0.036 };

				//linearise depth [0-1]
				float centralDepth = Linear01Depth(tex2D(ScattererDownscaledDepth, input.uv));

				float result = tex2D(TextureToBlur, input.uv).r * weight[0];
				float totalWeight = weight[0];


				//float BlurDepthFalloff = 0.01; //probably change this
				//float BlurDepthFalloff = 1.0; //guess this is too much, try 0.1
				float BlurDepthFalloff = 0.1; //seems ok but doesn't fix my issue though

				[unroll]
				for (int i = 1; i < 4; i++)
				{
					float depth = Linear01Depth(tex2D(ScattererDownscaledDepth, (input.uv + BlurDir * offset[i] * ScattererDownscaledDepth_TexelSize.xy )));

					float w = abs(depth-centralDepth)* BlurDepthFalloff;
					w = exp(-w*w);

					result += tex2D(TextureToBlur, ( input.uv + BlurDir * offset[i] * TextureToBlur_TexelSize.xy )).r * w * weight[i];

					totalWeight += w * weight[i];

					depth = Linear01Depth(tex2D(ScattererDownscaledDepth, (input.uv - BlurDir * offset[i] * ScattererDownscaledDepth_TexelSize.xy )));

					w = abs(depth-centralDepth)* BlurDepthFalloff;
					w = exp(-w*w);

					result += tex2D(TextureToBlur, ( input.uv - BlurDir * offset[i] * TextureToBlur_TexelSize.xy )).r * w* weight[i];

					totalWeight += w * weight[i];
				}

				return float4(result,result,result,1.0);
			}

			ENDCG
		}

		Pass //Pass 3, downscale custom depth buffer to 1/4
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D ScattererDepthCopy;
			float4 ScattererDepthCopy_TexelSize; // (1.0/width, 1.0/height, width, height)

			v2f vert( appdata_img v )
			{
				v2f o = (v2f)0;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;

				return o;
			}

			float4 frag(v2f input) : SV_Target
			{
				float2 texelSize = 0.5 * ScattererDepthCopy_TexelSize.xy;
				float2 taps[4] = {     float2(input.uv + float2(-1,-1)*texelSize),
					float2(input.uv + float2(-1,1)*texelSize),
					float2(input.uv + float2(1,-1)*texelSize),
					float2(input.uv + float2(1,1)*texelSize) };

				float depth1 = tex2D(ScattererDepthCopy, taps[0]).r;
				float depth2 = tex2D(ScattererDepthCopy, taps[1]).r;
				float depth3 = tex2D(ScattererDepthCopy, taps[2]).r;
				float depth4 = tex2D(ScattererDepthCopy, taps[3]).r;

				//Only return zero if all samples are zero, otherwise return the smallest which isn't zero
				float result = depth4;
				result = (result == 0.0) || (depth3 == 0.0) ? max(result, depth3) : min (result,depth3);
				result = (result == 0.0) || (depth2 == 0.0) ? max(result, depth2) : min (result,depth2);
				result = (result == 0.0) || (depth1 == 0.0) ? max(result, depth1) : min (result,depth1);

				return result;
			}

			ENDCG
		}
	}
	Fallback off
}