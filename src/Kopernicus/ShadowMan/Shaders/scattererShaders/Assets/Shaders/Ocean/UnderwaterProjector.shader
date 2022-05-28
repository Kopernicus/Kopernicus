
Shader "Scatterer/UnderwaterScatterProjector" {
	SubShader {
		Tags {"Queue" = "Transparent-5" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		Pass {
			//Cull Front
			Cull Back
			ZTest LEqual
			ZWrite Off

			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "../CommonAtmosphere.cginc"

			#pragma multi_compile DITHERING_OFF DITHERING_ON
			
			uniform float3 _planetPos;

			uniform float3 _Underwater_Color;

			uniform float transparencyDepth;
			uniform float darknessDepth;
			
			struct v2f
			{
				float3 viewPos:TEXCOORD0;
				float3 viewDir:TEXCOORD1;
				float3 worldPos:TEXCOORD2;
			};

			v2f vert(appdata_base v, out float4 outpos: SV_POSITION)
			{
				v2f o;
				outpos =  UnityObjectToClipPos(v.vertex);
				o.viewPos =  UnityObjectToViewPos(v.vertex.xyz);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex).xyz;
				o.viewDir = o.worldPos - _WorldSpaceCameraPos;
				o.worldPos = o.worldPos - _planetPos;
				return o;
			}

			float3 oceanColor(float3 viewDir, float3 lightDir, float3 surfaceDir)
			{
				float angleToLightDir = (dot(viewDir, surfaceDir) + 1 )* 0.5;
				float3 waterColor = pow(_Underwater_Color, 4.0 *(-1.0 * angleToLightDir + 1.0));
				return waterColor;
			}

			half4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{
				float fragDistance = length(i.viewPos);                

				float3 rayDir=normalize(i.viewDir);

				float3 _camPos = _WorldSpaceCameraPos - _planetPos;
				float waterLightExtinction = length(getSkyExtinction(normalize(_camPos + 10.0) * Rg , SUN_DIR));

				float underwaterDepth = Rg - length(_camPos);

				underwaterDepth = lerp(1.0,0.0,underwaterDepth / darknessDepth);

				float3 waterColor= underwaterDepth * hdrNoExposure( waterLightExtinction * _sunColor * oceanColor(rayDir,SUN_DIR,normalize(_camPos)));
				float alpha = min(fragDistance/transparencyDepth,1.0);
				return float4(dither(waterColor, screenPos), alpha);
			}

			ENDCG
		}
	}
}