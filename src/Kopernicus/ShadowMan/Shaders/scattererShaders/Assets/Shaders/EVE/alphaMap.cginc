#ifndef ALPHA_MAP_CG_INC
#define ALPHA_MAP_CG_INC

inline half vectorSum(half4 v) 
{
	return (v.x + v.y + v.z + v.w);
}

half4 alphaMask1;

#define ALPHA_VALUE_1(color) \
	vectorSum( color * alphaMask1 )

#ifdef ALPHAMAP_1
#define ALPHA_COLOR_1(color) half4(1, 1, 1, ALPHA_VALUE_1(color))
#else
#define ALPHA_COLOR_1(color) color
#endif

#endif