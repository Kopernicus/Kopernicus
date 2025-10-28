Shader "Scatterer/invisible" 
{
	SubShader 
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="InvOcean"}

		Pass 
		{
			ZWrite Off
			ZTest on

			Blend SrcAlpha OneMinusSrcAlpha


			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl

			struct v2f 
			{
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.pos = float4(2.0, 2.0, 2.0, 1.0); //outside clip space => cull vertex
				return OUT;
			}


			float4 frag(v2f IN) : COLOR
			{
				return float4(0.0,0.0,0.0,0.0);			
			}

			ENDCG
		}
	}
}