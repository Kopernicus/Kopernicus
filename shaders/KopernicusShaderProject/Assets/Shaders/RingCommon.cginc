uniform sampler2D ringTexture;
uniform float ringInnerRadius;
uniform float ringOuterRadius;
uniform float3 ringNormal;

//only supporting Kopernicus linear rings for now (no support for tiled rings or rings with thickness)
//rgb channels are returned modulated by alpha value (used for sunflare)
//alpha value returned is actually 1-alpha (used for ring shadow on atmosphere/planet)
inline float4 getLinearRingColor(float3 worldPos, float3 sunDir, float3 planetAndRingOrigin)
{
	float parallel = 0.0;
	float3 ringIntersectPt = LinePlaneIntersection(worldPos, sunDir, ringNormal, planetAndRingOrigin, parallel);

	//calculate ring texture position on intersect
	float distance = length (ringIntersectPt - planetAndRingOrigin);
	float ringTexturePosition = (distance - ringInnerRadius) / (ringOuterRadius - ringInnerRadius); //inner and outer radiuses are converted to local space coords on plugin side
	ringTexturePosition = 1 - ringTexturePosition; //flip to match UVs

	float4 ringColor = tex2D(ringTexture, float2(ringTexturePosition,ringTexturePosition));
	ringColor.a = 1-ringColor.a;

	ringColor.xyz*=ringColor.a;

	//don't apply any shadows if intersect point is not between inner and outer radius or line and plane are parallel
	ringColor = (ringTexturePosition > 1 || ringTexturePosition < 0 || parallel == 1.0 ) ? float4(1.0,1.0,1.0,1.0) : ringColor;
	return ringColor;
}