Shader "Scatterer/CompositeDownscaledScattering" {
	SubShader {
		Tags {"Queue" = "Transparent-499" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		Pass {
			Tags {"Queue" = "Transparent-499" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

			Cull Off
			ZTest Off
			ZWrite [_ZwriteVariable]

			Blend SrcAlpha OneMinusSrcAlpha //traditional alpha-blending

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "../CommonAtmosphere.cginc"

			#pragma multi_compile CUSTOM_OCEAN_OFF CUSTOM_OCEAN_ON

			uniform sampler2D DownscaledScattering0;	//Scattering RGB, extinction.R
			uniform sampler2D DownscaledScattering1;	//Extinction.GB

			uniform sampler2D ScattererDownscaledScatteringDepth;
			float4 ScattererDownscaledScatteringDepth_TexelSize;


#if defined (CUSTOM_OCEAN_ON)
			uniform sampler2D ScattererScreenCopy;
			uniform sampler2D ScattererDepthCopy;
			float4 ScattererDepthCopy_TexelSize;

			#define fullresDepthTexture ScattererDepthCopy;
			#define fullresDepthTexture_TexelSize ScattererDepthCopy_TexelSize;
#else
			uniform sampler2D _CameraDepthTexture;
			float4 _CameraDepthTexture_TexelSize;

			#define fullresDepthTexture _CameraDepthTexture;
			#define fullresDepthTexture_TexelSize _CameraDepthTexture_TexelSize;

			uniform sampler2D ScattererScreenCopyBeforeOcean;
#endif

			struct v2f
			{
				float4 screenPos : TEXCOORD0;
			};

			v2f vert(appdata_base v, out float4 outpos: SV_POSITION)
			{
				v2f o;

#if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)
				outpos = float4(2.0 * v.vertex.x, 2.0 * v.vertex.y *_ProjectionParams.x, -1.0 , 1.0);
#else
				outpos = float4(2.0 * v.vertex.x, 2.0 * v.vertex.y, 0.0 , 1.0);
#endif
				o.screenPos = ComputeScreenPos(outpos);

				return o;
			}
				
			struct fout
			{
				float4 color : COLOR;
#if defined (CUSTOM_OCEAN_ON)
				float depth  : DEPTH;
#endif
			};

			void UpdateNearestSample(	inout float MinDist,
				inout float2 NearestUV,
				float Z,
				float2 UV,
				float ZFull
			)
			{
				float Dist = abs(Z - ZFull);
#if SHADER_API_D3D11 || SHADER_API_D3D || SHADER_API_D3D12
				if ((Dist < MinDist) && (Z > 0.0))
#else
				if ((Dist < MinDist) && (Z < 1.0))
#endif
				{
					MinDist = Dist;
					NearestUV = UV;
				}
			}

			fout frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS)
			{
				float2 uv = i.screenPos.xy / i.screenPos.w;

#if defined (CUSTOM_OCEAN_ON)
				float zdepth = tex2Dlod(ScattererDepthCopy, float4(uv,0,0));
#else
				float zdepth = tex2Dlod(_CameraDepthTexture, float4(uv,0,0));
#endif

				//read full resolution depth
				float ZFull = Linear01Depth(zdepth);

#if SHADER_API_D3D11 || SHADER_API_D3D || SHADER_API_D3D12
				if (_ProjectionParams.x > 0) {uv.y = 1.0 - uv.y;}
				if (zdepth == 0.0) {discard;}
#else
				if (zdepth == 1.0) {discard;}
#endif

				//find low res depth texture texel size
				float2 lowResTexelSize = ScattererDownscaledScatteringDepth_TexelSize.xy;

				float2 lowResUV = uv; 

				float MinDist = 1.0;

				float2 UV00 = lowResUV - 0.5 * lowResTexelSize;
				float2 NearestUV = UV00;
				float Z00 = Linear01Depth( SAMPLE_DEPTH_TEXTURE( ScattererDownscaledScatteringDepth, UV00) );   
				UpdateNearestSample(MinDist, NearestUV, Z00, UV00, ZFull);

				float2 UV10 = float2(UV00.x+lowResTexelSize.x, UV00.y);
				float Z10 = Linear01Depth( SAMPLE_DEPTH_TEXTURE( ScattererDownscaledScatteringDepth, UV10) );  
				UpdateNearestSample(MinDist, NearestUV, Z10, UV10, ZFull);

				float2 UV01 = float2(UV00.x, UV00.y+lowResTexelSize.y);
				float Z01 = Linear01Depth( SAMPLE_DEPTH_TEXTURE( ScattererDownscaledScatteringDepth, UV01) );  
				UpdateNearestSample(MinDist, NearestUV, Z01, UV01, ZFull);

				float2 UV11 = UV00 + lowResTexelSize;
				float Z11 = Linear01Depth( SAMPLE_DEPTH_TEXTURE( ScattererDownscaledScatteringDepth, UV11) );  
				UpdateNearestSample(MinDist, NearestUV, Z11, UV11, ZFull);

				float4 col0 = tex2Dlod(DownscaledScattering0, float4(NearestUV,0,0));
				float2 col1 = tex2Dlod(DownscaledScattering1, float4(NearestUV,0,0)).rg;

				float3 inscatter = col0.rgb;
				float3 extinction = float3(col0.a,col1.rg);

#if defined (CUSTOM_OCEAN_ON)
				float3 backGrnd = tex2Dlod(ScattererScreenCopy, float4(uv.x, uv.y,0.0,0.0));
#else
				float3 backGrnd = tex2Dlod(ScattererScreenCopyBeforeOcean, float4(uv.x, uv.y,0.0,0.0));
#endif

				//composite backGround by extinction
				backGrnd*=extinction;

				//composite background with inscatter, soft-blend it
				backGrnd+= (1.0 - backGrnd) * dither(inscatter, screenPos);

				fout output;
				output.color = float4(backGrnd,1.0);
#if defined (CUSTOM_OCEAN_ON)
				output.depth = zdepth;			//this needs to only be done if rendering with an ocean
#endif
				return output;
			}
			ENDCG
		}
	}

}