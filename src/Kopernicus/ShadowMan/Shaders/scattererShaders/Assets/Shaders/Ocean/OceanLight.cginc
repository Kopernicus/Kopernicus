//  Modified Light helpers for the ocean
//  unlike in autolight.cginc, object2world can't be used in the case of the ocean
//  the projection is instead done via other means
//  the world position is passed instead of doing a mul(_Object2World, v.vertex) which gives the wrong result here


#ifdef POINT
//#define LIGHTING_COORDS(idx1,idx2) unityShadowCoord3 _LightCoord : TEXCOORD##idx1; SHADOW_COORDS(idx2)
#define OCEAN_TRANSFER_VERTEX_TO_FRAGMENT(a) a._LightCoord = mul(unity_WorldToLight, worldPos).xyz; TRANSFER_SHADOW(a)
//#define LIGHT_ATTENUATION(a)	(tex2D(_LightTexture0, dot(a._LightCoord,a._LightCoord).rr).UNITY_ATTEN_CHANNEL * SHADOW_ATTENUATION(a))
#endif

#ifdef SPOT
//#define LIGHTING_COORDS(idx1,idx2) unityShadowCoord4 _LightCoord : TEXCOORD##idx1; SHADOW_COORDS(idx2)
#define OCEAN_TRANSFER_VERTEX_TO_FRAGMENT(a) a._LightCoord = mul(unity_WorldToLight, worldPos); TRANSFER_SHADOW(a)
//#define LIGHT_ATTENUATION(a)	( (a._LightCoord.z > 0) * UnitySpotCookie(a._LightCoord) * UnitySpotAttenuate(a._LightCoord.xyz) * SHADOW_ATTENUATION(a) )
#endif

#ifdef DIRECTIONAL
//	#define LIGHTING_COORDS(idx1,idx2) SHADOW_COORDS(idx1)
	#define OCEAN_TRANSFER_VERTEX_TO_FRAGMENT(a) TRANSFER_SHADOW(a)
//	#define LIGHT_ATTENUATION(a)	SHADOW_ATTENUATION(a)
#endif

#ifdef POINT_COOKIE
//#define LIGHTING_COORDS(idx1,idx2) unityShadowCoord3 _LightCoord : TEXCOORD##idx1; SHADOW_COORDS(idx2)
#define OCEAN_TRANSFER_VERTEX_TO_FRAGMENT(a) a._LightCoord = mul(unity_WorldToLight, worldPos).xyz; TRANSFER_SHADOW(a)
//#define LIGHT_ATTENUATION(a)	(tex2D(_LightTextureB0, dot(a._LightCoord,a._LightCoord).rr).UNITY_ATTEN_CHANNEL * texCUBE(_LightTexture0, a._LightCoord).w * SHADOW_ATTENUATION(a))
#endif

#ifdef DIRECTIONAL_COOKIE
//#define LIGHTING_COORDS(idx1,idx2) unityShadowCoord2 _LightCoord : TEXCOORD##idx1; SHADOW_COORDS(idx2)
#define OCEAN_TRANSFER_VERTEX_TO_FRAGMENT(a) a._LightCoord = mul(unity_WorldToLight, worldPos).xy; TRANSFER_SHADOW(a)
//#define LIGHT_ATTENUATION(a)	(tex2D(_LightTexture0, a._LightCoord).w * SHADOW_ATTENUATION(a))
#endif