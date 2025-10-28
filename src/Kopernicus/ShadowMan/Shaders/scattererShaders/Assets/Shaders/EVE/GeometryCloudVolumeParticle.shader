Shader "Scatterer-EVE/GeometryCloudVolumeParticle" {
	Properties {
		_Tex("Particle Texture", 2D) = "white" {}
		_MainTex("Main (RGB)", 2D) = "white" {}
		_PerlinTex("Perlin (RGB)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_DetailTex("Detail (RGB)", 2D) = "white" {}
		_DetailScale("Detail Scale", Range(0,1000)) = 100
		_DistFade("Distance Fade Near", Float) = 1.0
		_DistFadeVert("Distance Fade Vertical", Float) = 0.0004
		_MinScatter("Min Scatter", Float) = 1.05
		_Opacity("Opacity", Float) = 1.05
		_Color("Color Tint", Color) = (1,1,1,1)
		_InvFade("Soft Particles Factor", Range(0,1.0)) = .008
		_Rotation("Rotation", Float) = 0
		_MaxScale("Max Scale", Float) = 1
		_MaxTrans("Max Translation", Vector) = (0,0,0)
		_NoiseScale("Noise Scale", Vector) = (1,2,.0005,100)
		_SunPos("_SunPos", Vector) = (0,0,0)
		_SunRadius("_SunRadius", Float) = 1
	}

	Category {

		Tags { "Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching"="True" }
		Blend SrcAlpha OneMinusSrcAlpha
		Fog { Mode Global}
		AlphaTest Greater 0
		ColorMask RGB
		//Cull Back
		Cull Off
		Lighting On
		ZWrite Off

		SubShader {
			Pass {

				Lighting On
				Tags { "LightMode"="ForwardBase"}

				CGPROGRAM
				#include "EVEUtils.cginc"
				#pragma vertex vert
				#pragma fragment frag
				#pragma geometry geom
				#define MAG_ONE 1.4142135623730950488016887242097
				#pragma fragmentoption ARB_precision_hint_fastest
				//#pragma multi_compile_fwdbase
				#pragma multi_compile SOFT_DEPTH_OFF SOFT_DEPTH_ON
#pragma multi_compile MAP_TYPE_1 MAP_TYPE_CUBE_1 MAP_TYPE_CUBE2_1 MAP_TYPE_CUBE6_1
#ifndef MAP_TYPE_CUBE2_1
#pragma multi_compile ALPHAMAP_N_1 ALPHAMAP_1
#endif

				#include "noiseSimplex.cginc"
				#include "alphaMap.cginc"
				#include "cubeMap.cginc"

#pragma multi_compile SCATTERER_OFF SCATTERER_ON
#pragma multi_compile SCATTERER_USE_ORIG_DIR_COLOR_OFF SCATTERER_USE_ORIG_DIR_COLOR_ON

#ifdef SCATTERER_ON
				#include "../CommonAtmosphere.cginc"
#endif

				CUBEMAP_DEF_1(_MainTex)

				sampler2D _Tex;
				sampler2D _DetailTex;
				sampler2D _BumpMap;

				float4x4 _PosRotation;

				float _DetailScale;
				fixed4 _Color;
				float _DistFade;
				float _DistFadeVert;
				float _MinScatter;
				float _Opacity;
				float _InvFade;
				float _Rotation;
				float _QuadSize;
				float _MaxScale;
				float4 _NoiseScale;
				float3 _MaxTrans;

				sampler2D _CameraDepthTexture;

#ifdef SCATTERER_ON
				uniform float cloudColorMultiplier;
				uniform float cloudScatteringMultiplier;
				uniform float cloudSkyIrradianceMultiplier;
				uniform float3 _Sun_WorldSunDir;
				uniform float3 _PlanetWorldPos;
				uniform float3 scattererOrigDirectionalColor;
#endif

				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float3 normal : NORMAL;
					float4 tangent : TANGENT;
					float2 texcoord : TEXCOORD0;
				};

				struct v2g {
					float4 pos : SV_POSITION;
					fixed4 color : COLOR;
					float3 hashVect : TEXCOORD0;
					float4 localOrigin : TEXCOORD1;
					float4 origin : TEXCOORD2;
					float viewDirFade : TEXCOORD3;
					float3 planetPos : TEXCOORD4;
					float particleFade: TEXCOORD5;
				};

				//vertex function, called for every point on our hexSeg, each vertex corresponding to the origin of a particle/quad
				v2g vert (appdata_t v)
				{
					v2g o;
					UNITY_INITIALIZE_OUTPUT(v2g, o);

					v.vertex.xyz/=v.vertex.w;
					float4 origin = mul(unity_ObjectToWorld, v.vertex); //origin of the quad in worldSpace

					float4 planet_pos = mul(_PosRotation, origin);

					float3 normalized = _NoiseScale.z*(planet_pos.xyz);
					float3 hashVect =  .5*(float3(snoise(normalized), snoise(_NoiseScale.x*normalized), snoise(_NoiseScale.y*normalized))+1);  //unique hash of the particle based on planet pos
					o.hashVect = hashVect;

					float4 localOrigin;
					localOrigin.xyz = (2*hashVect-1)*_MaxTrans;   		//offset to localOrigin based on hash above
					localOrigin.w = 1;

					//localOrigin.xyz+=v.vertex.xyz;			//here this is wrong, in the original shader, local origin is added to the quad in it's space and gets transformed with it'w own M matrix with no issues
																	//here as we add this to the space of the hex, transforming later with M matrix can cause additional rotation, we can add this in worldSpace instead

					origin.xyz+=localOrigin;				//the particle transforms are originally oriented same as in world space, therefore we can do this offset directly in worldSpace in our case
					o.origin = origin;

					localOrigin = mul (unity_WorldToObject, origin);	//transform back to find the new localOrigin
					o.localOrigin = localOrigin;

					planet_pos = mul(_MainRotation, origin);  		//new planet pos based on offset origin
					o.planetPos = planet_pos.xyz;
																											
					float3 detail_pos = mul(_DetailRotation, planet_pos).xyz;
					o.color = VERT_GET_NO_LOD_CUBE_MAP_1(_MainTex, planet_pos.xyz);
					o.color.rgba *= GetCubeDetailMapNoLOD(_DetailTex, detail_pos, _DetailScale);

					o.viewDirFade = GetDistanceFade(distance(origin, _WorldSpaceCameraPos), _DistFade, _DistFadeVert);
					o.color.a *= o.viewDirFade;

					float4 mvCenter = mul(UNITY_MATRIX_MV, float4(localOrigin.xyz,1.0));  //offset quad origin in viewspace
					o.pos=mvCenter;
					o.pos = o.color.a > (1.0/255.0) ? o.pos : float4(2.0, 2.0, 2.0, 1.0); //cull vertex if low alpha, pos outside clipspace

					float fadeOut = (-mvCenter.z/mvCenter.w) * 0.004;
					o.particleFade = smoothstep(0.0,1.0,fadeOut);
#ifdef SCATTERER_ON

					float3 worldPos = origin;

					float3 extinction = float3(0, 0, 0);

					float3 WCP = _WorldSpaceCameraPos; //unity supplied, in local Space

//					float3 worigin = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
					float3 worigin = _PlanetWorldPos;
                	
					float3 relWorldPos=worldPos-worigin;
					float3 relCameraPos=WCP-worigin;
					//relWorldPos=lerp(Rg*normalize(relWorldPos),relWorldPos,cloudExtinctionHeightMultiplier);					

					//extinction of light from sun to cloud
					float3 sunExtinction = getSkyExtinction(relWorldPos,_Sun_WorldSunDir);

					//extinction from cloud to observer
					extinction = getExtinction(relCameraPos, relWorldPos, 1.0, 1.0, 1.0);

					o.color.rgb *= cloudColorMultiplier * extinction * sunExtinction;

//					//skyLight
//					float3 skyE = SimpleSkyirradiance(relWorldPos, viewDir.xyz, _Sun_WorldSunDir);
//
//					o.color.rgb *= cloudColorMultiplier * extinction * sunExtinction + skyE*cloudSkyIrradianceMultiplier;

//					o.relWorldPos = relWorldPos;
//					//inScattering from cloud to observer
//					float3 inscatter = InScattering2(relCameraPos, relWorldPos, extinction, _Sun_WorldSunDir, 1.0, 1.0, 1.0);
//					o.inScattering = inscatter * cloudScatteringMultiplier;
//
//					//extinction from cloud to observer
//					extinction = getExtinction(relCameraPos, relWorldPos, 1.0, 1.0, 1.0);
//
//					o.color.rgb *= cloudColorMultiplier * extinction;					 
#endif	


					return o;
				}


				struct g2f
				{
					float4 pos : SV_POSITION;
					fixed4 color : COLOR;
					half4 viewDir : TEXCOORD0;
					float2 texcoordZY : TEXCOORD1;
					float2 texcoordXZ : TEXCOORD2;
					float2 texcoordXY : TEXCOORD3;
					float3 uv : TEXCOORD4;		//x and y UVs, z is particleFade
					float4 projPos : TEXCOORD5;
					float3 planetPos : TEXCOORD6;
					float3 viewDirT : TEXCOORD7;
					float3 lightDirT : TEXCOORD8;
					float4 localOrigin : TEXCOORD9;
				};

				//this function builds a quad corner vertex from the data passed to it, it will be called by the geometry shader
				g2f buildQuadVertex(v2g originPoint, float3 vertexPosition, float2 vertexUV, float3 viewDir, float4x4 mvMatrix, float localScale)
				{
					g2f tri;
					UNITY_INITIALIZE_OUTPUT(g2f, tri);

					float3 mvCenter = originPoint.pos.xyz/originPoint.pos.w;
					tri.pos = float4(mvCenter + _QuadSize * localScale * vertexPosition,1.0);    //position in view space of quad corner

#ifdef SOFT_DEPTH_ON
					float eyedepth = -tri.pos.z;
#endif

					tri.pos = mul (UNITY_MATRIX_P, tri.pos);

#ifdef SOFT_DEPTH_ON
					tri.projPos = ComputeScreenPos (tri.pos);
					//COMPUTE_EYEDEPTH(tri.projPos.z);

					//we replace COMPUTE_EYEDEPTH with it's definition as it's only designed for the vertex shader input
					//tri.projPos.z = -UnityObjectToViewPos( v.vertex ).z
					tri.projPos.z = eyedepth;
#endif

    				

					//pass these values we need for the fragment shader
					tri.viewDir.xyz = abs(viewDir).xyz;
					tri.viewDir.w = originPoint.viewDirFade;
					tri.planetPos = originPoint.planetPos;
					tri.color = originPoint.color;

					float2 texcoodOffsetxy = ((2*vertexUV)- 1);
					float4 texcoordOffset = float4(texcoodOffsetxy.x, texcoodOffsetxy.y, 0, 1.0);   //would 1.0 work here???? let's find out

					float4 ZYv = texcoordOffset.zyxw;
					float4 XZv = texcoordOffset.xzyw;
					float4 XYv = texcoordOffset.xyzw;

					ZYv.z*=sign(-viewDir.x);
					XZv.x*=sign(-viewDir.y);
					XYv.x*=sign(viewDir.z);
					
					ZYv.x += sign(-viewDir.x)*sign(ZYv.z)*(viewDir.z);
					XZv.y += sign(-viewDir.y)*sign(XZv.x)*(viewDir.x);
					XYv.z += sign(-viewDir.z)*sign(XYv.x)*(viewDir.x);

					ZYv.x += sign(-viewDir.x)*sign(ZYv.y)*(viewDir.y);
					XZv.y += sign(-viewDir.y)*sign(XZv.z)*(viewDir.z);
					XYv.z += sign(-viewDir.z)*sign(XYv.y)*(viewDir.y);

					float2 ZY = mul(mvMatrix, ZYv).xy - mvCenter.xy;
					float2 XZ = mul(mvMatrix, XZv).xy - mvCenter.xy;
					float2 XY = mul(mvMatrix, XYv).xy - mvCenter.xy;

					tri.texcoordZY = half2(.5 ,.5) + .6*(ZY);
					tri.texcoordXZ = half2(.5 ,.5) + .6*(XZ);
					tri.texcoordXY = half2(.5 ,.5) + .6*(XY);

					viewDir = normalize(originPoint.origin - _WorldSpaceCameraPos); 	//worldSpaceView dir to center of quad not to fragment, why? maybe to frag would be better?

					half3 normal = normalize(-viewDir);
					float3 tangent = UNITY_MATRIX_V[0].xyz;
					float3 binormal = -cross(normal, normalize(tangent));
					float3x3 rotation = float3x3(tangent.xyz, binormal, normal);

					tri.lightDirT = normalize(mul(rotation, _WorldSpaceLightPos0.xyz));
					tri.viewDirT = normalize(mul(rotation, viewDir));

					tri.uv = float3(vertexUV, originPoint.particleFade);   			//x and y quad UV, z particleFade
					tri.localOrigin = originPoint.localOrigin;

    				return tri;
				}

				//geometry shader
				//for every vertex from the vertex shader it will create a particle quad of 4 vertexes and 2 triangles
				[maxvertexcount(4)]
    			void geom(point v2g input[1], inout TriangleStream<g2f> outStream)
    			{
					g2f tri;

					//common values for all the quad
					float localScale = (input[0].hashVect.x*(_MaxScale - 1)) + 1;

					float4x4 M = rand_rotation(
						(float3(frac(_Rotation),0,0))+input[0].hashVect,
						localScale,
						input[0].localOrigin.xyz/input[0].localOrigin.w);

					float4x4 mvMatrix = mul(mul(UNITY_MATRIX_V, unity_ObjectToWorld), M);
					float3 viewDir = normalize(mvMatrix[2].xyz); //cameraSpace viewDir I think

					//build our quad
					tri = buildQuadVertex(input[0],float3(-0.5,-0.5,0.0),float2(0.0,0.0),viewDir,mvMatrix,localScale);
					outStream.Append(tri);

					tri = buildQuadVertex(input[0],float3(-0.5,0.5,0.0),float2(0.0,1.0),viewDir,mvMatrix,localScale);
					outStream.Append(tri);

					tri = buildQuadVertex(input[0],float3(0.5,-0.5,0.0),float2(1.0,0.0),viewDir,mvMatrix,localScale);
					outStream.Append(tri);

					tri = buildQuadVertex(input[0],float3(0.5,0.5,0.0),float2(1.0,1.0),viewDir,mvMatrix,localScale);
					outStream.Append(tri);

					outStream.RestartStrip();
    			}

				float4 frag (g2f IN) : COLOR
				{

					half4 tex;
					tex.r = tex2D(_Tex, IN.texcoordZY).r;
					tex.g = tex2D(_Tex, IN.texcoordXZ).g;
					tex.b = tex2D(_Tex, IN.texcoordXY).b;

					tex.a = 0;
									
					tex.rgb *= IN.viewDir.rgb;
					half4 vect = half4( IN.viewDir.rgb, 0);
					tex /= vectorSum(vect);

					tex = half4(1, 1, 1, vectorSum(tex));

					half4 color = FRAG_GET_NO_LOD_CUBE_MAP_1(_MainTex, IN.planetPos);
					color = ALPHA_COLOR_1(color);

					color *= _Color * IN.color;

					
					//half3 normT = UnpackNormal(tex2D(_BumpMap, IN.uv));
					half3 normT;
					normT.xy = ((2*IN.uv.xy)-1);
					normT.z = sqrt(1 - saturate(dot(normT.xy, normT.xy)));
					//normT.xy = 2 * INV_PI*asin((2 * IN.uv) - 1) ;
					//normT.xy = sin(PI*(IN.uv-.5));
					//normT.z = 1;
					//color.rg = IN.uv;


					color.a *= tex.a;
					tex.a = IN.viewDir.w*tex.a;

#if (defined(SCATTERER_ON) && defined(SCATTERER_USE_ORIG_DIR_COLOR_ON))
					_LightColor0.rgb = scattererOrigDirectionalColor;
#endif

					color.rgb *= ScatterColorLight(IN.lightDirT, IN.viewDirT, normT, tex, _MinScatter, _Opacity, 1).rgb;

#ifdef SOFT_DEPTH_ON
					float depth = UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.projPos)));
					depth = LinearEyeDepth (depth);
					float partZ = IN.projPos.z;
					float fade = depth >= (0.99 * _ProjectionParams.z) ? 1.0 : saturate (_InvFade * (depth-partZ));	//fade near objects but don't fade on far plane (max depth value)
					color.a *= fade;
#endif

					color.a *= IN.uv.z;		//particle fade as they approach camera
					
					return color;
				}
				ENDCG
			}

		}

	}
}