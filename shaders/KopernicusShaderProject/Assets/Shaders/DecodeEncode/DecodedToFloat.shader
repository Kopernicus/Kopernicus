Shader "EncodeFloat/DecodeToFloat" 
{
	SubShader 
	{
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform sampler2D _TexR, _TexG, _TexB, _TexA;
			uniform float _Max, _Min;

			struct v2f 
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.uv = v.texcoord.xy;
				return OUT;
			}

			//This is a built in function but I like to see the code
			float decodeFloatRGBA( float4 rgba ) 
			{
				return dot( rgba, float4(1.0, 1/255.0, 1/65025.0, 1/160581375.0) );
			}

			float4 frag(v2f IN) : COLOR
			{

				float R = decodeFloatRGBA(tex2D(_TexR, IN.uv.xy));
				float G = decodeFloatRGBA(tex2D(_TexG, IN.uv.xy));
				float B = decodeFloatRGBA(tex2D(_TexB, IN.uv.xy));
				float A = decodeFloatRGBA(tex2D(_TexA, IN.uv.xy));

				return (float4(R,G,B,A) * _Max) - _Min;

			}

			ENDCG

		}
	}
}