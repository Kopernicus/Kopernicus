Shader "Scatterer/CompositeCausticsGodrays"
{
	Properties{
	}

	SubShader{
		Tags { "Queue"="Transparent+2" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Pass {
			Blend OneMinusDstColor One //softAdditive
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include"UnityCG.cginc"

			uniform sampler2D LightRaysTexture;

			uniform sampler2D _CameraDepthTexture;
			float4 _CameraDepthTexture_TexelSize;

			uniform sampler2D ScattererDownscaledDepth;
			float4 ScattererDownscaledDepth_TexelSize;

			uniform float3 _sunColor; //already calculated sun extinction or color
			uniform float3 _Underwater_Color;

			struct appdata_t {
				float4 vertex : POSITION;
				float2 uv: TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_t v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				return o;
			}


			float4 frag(v2f i) : COLOR
			{
				float color = tex2D(LightRaysTexture, i.uv).r;
				return float4(_Underwater_Color * _sunColor * color,1.0);
			}

			ENDCG
		}
	}
}