using System;
using System.Runtime.CompilerServices;
using BurstPQS.CompilerServices;
using BurstPQS.Map;
using Kopernicus.Components;
using Kopernicus.OnDemand;
using KSPTextureLoader;
using Unity.Burst;
using UnityEngine;

namespace BurstPQS.Kopernicus.Map;

/// <summary>
/// Adapter structs that wrap KSPTextureLoader's <see cref="CPUTexture2D"/> format types
/// and implement <see cref="IMapSO"/> for use with <see cref="BurstMapSO"/>.
/// </summary>
[BurstCompile(FloatMode = FloatMode.Fast)]
static partial class BurstKopernicusMapSO
{
    struct FormatMapSO<T>(T texture, MapSO.MapDepth depth) : IMapSO
        where T : ICPUTexture2D
    {
        readonly T texture = texture;
        readonly MapSO.MapDepth depth = depth;

        public readonly int Width => texture.Width;
        public readonly int Height => texture.Height;
        public readonly MapSO.MapDepth Depth => depth;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetPixelFloat(int x, int y)
        {
            Color c = texture.GetPixel(x, y);
            // Alpha8 stores its value in the alpha channel (r=g=b=1)
            if (texture.Format == TextureFormat.Alpha8)
                return c.a;
            return depth switch
            {
                MapSO.MapDepth.Greyscale => c.r,
                MapSO.MapDepth.HeightAlpha => (c.r + c.a) * 0.5f,
                MapSO.MapDepth.RGB => (c.r + c.g + c.b) * (1f / 3f),
                MapSO.MapDepth.RGBA => (c.r + c.g + c.b + c.a) * 0.25f,
                _ => 0f,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color GetPixelColor(int x, int y)
        {
            Color c = texture.GetPixel(x, y);
            return depth switch
            {
                MapSO.MapDepth.Greyscale => texture.Format == TextureFormat.Alpha8
                    ? new Color(c.a, c.a, c.a, 1f)
                    : new Color(c.r, c.r, c.r, 1f),
                MapSO.MapDepth.HeightAlpha => new Color(c.r, c.r, c.r, c.a),
                _ => c,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color32 GetPixelColor32(int x, int y)
        {
            Color32 c = texture.GetPixel32(x, y);
            return depth switch
            {
                MapSO.MapDepth.Greyscale => texture.Format == TextureFormat.Alpha8
                    ? new Color32(c.a, c.a, c.a, 255)
                    : new Color32(c.r, c.r, c.r, 255),
                MapSO.MapDepth.HeightAlpha => new Color32(c.r, c.r, c.r, c.a),
                _ => c,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HeightAlpha GetPixelHeightAlpha(int x, int y)
        {
            Color c = texture.GetPixel(x, y);
            float height = texture.Format == TextureFormat.Alpha8 ? c.a : c.r;

            return depth switch
            {
                MapSO.MapDepth.HeightAlpha or MapSO.MapDepth.RGBA => new(height, c.a),
                _ => new(height, 1f),
            };
        }

        public float GetPixelFloat(float x, float y) => MapSODefaults.GetPixelFloat(ref this, x, y);

        public float GetPixelFloat(double x, double y) =>
            MapSODefaults.GetPixelFloat(ref this, x, y);

        public Color GetPixelColor(float x, float y) => MapSODefaults.GetPixelColor(ref this, x, y);

        public Color GetPixelColor(double x, double y) =>
            MapSODefaults.GetPixelColor(ref this, x, y);

        public Color32 GetPixelColor32(float x, float y) =>
            MapSODefaults.GetPixelColor32(ref this, x, y);

        public Color32 GetPixelColor32(double x, double y) =>
            MapSODefaults.GetPixelColor32(ref this, x, y);

        public HeightAlpha GetPixelHeightAlpha(float x, float y) =>
            MapSODefaults.GetPixelHeightAlpha(ref this, x, y);

        public HeightAlpha GetPixelHeightAlpha(double x, double y) =>
            MapSODefaults.GetPixelHeightAlpha(ref this, x, y);
    }

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.Alpha8>), Name = "mapSO")]
    [BurstCompile]
    public partial struct Alpha8 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.ARGB32>), Name = "mapSO")]
    [BurstCompile]
    public partial struct ARGB32 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.ARGB4444>), Name = "mapSO")]
    [BurstCompile]
    public partial struct ARGB4444 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.BC4>), Name = "mapSO")]
    [BurstCompile]
    public partial struct BC4 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.BC5>), Name = "mapSO")]
    [BurstCompile]
    public partial struct BC5 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.BC6H>), Name = "mapSO")]
    [BurstCompile]
    public partial struct BC6H : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.BC7>), Name = "mapSO")]
    [BurstCompile]
    public partial struct BC7 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.BGRA32>), Name = "mapSO")]
    [BurstCompile]
    public partial struct BGRA32 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.DXT1>), Name = "mapSO")]
    [BurstCompile]
    public partial struct DXT1 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.DXT5>), Name = "mapSO")]
    [BurstCompile]
    public partial struct DXT5 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.R8>), Name = "mapSO")]
    [BurstCompile]
    public partial struct R8 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.R16>), Name = "mapSO")]
    [BurstCompile]
    public partial struct R16 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.RA16>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RA16 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.RFloat>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RFloat : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.RG16>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RG16 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.RGB24>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RGB24 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.RGB565>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RGB565 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.RGBA32>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RGBA32 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.RGBA4444>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RGBA4444 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.RGBAFloat>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RGBAFloat : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.RGBAHalf>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RGBAHalf : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.RGFloat>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RGFloat : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.RGHalf>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RGHalf : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.RHalf>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RHalf : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.KopernicusPalette4>), Name = "mapSO")]
    [BurstCompile]
    public partial struct KopernicusPalette4 : IMapSO;

    [StructInherit(typeof(FormatMapSO<CPUTexture2D.KopernicusPalette8>), Name = "mapSO")]
    [BurstCompile]
    public partial struct KopernicusPalette8 : IMapSO;

    // HeightAlpha maps backed by two-channel textures are wrapped by
    // KopernicusMapSO so that the second channel reads as alpha.
    [StructInherit(typeof(FormatMapSO<KopernicusMapSO.RACPUTexture<CPUTexture2D.RG16>>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RG16AsRA : IMapSO;

    [StructInherit(typeof(FormatMapSO<KopernicusMapSO.RACPUTexture<CPUTexture2D.RGHalf>>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RGHalfAsRA : IMapSO;

    [StructInherit(typeof(FormatMapSO<KopernicusMapSO.RACPUTexture<CPUTexture2D.RGFloat>>), Name = "mapSO")]
    [BurstCompile]
    public partial struct RGFloatAsRA : IMapSO;

    [StructInherit(typeof(FormatMapSO<KopernicusMapSO.RACPUTexture<CPUTexture2D.BC5>>), Name = "mapSO")]
    [BurstCompile]
    public partial struct BC5AsRA : IMapSO;

    public static BurstMapSO Create(MapSODemand mapSO)
    {
        if (!mapSO.IsLoaded)
            mapSO.Load();

        if (!mapSO.IsLoaded)
            return BurstMapSO.Create(new InvalidMapSO());

        return Create((KopernicusMapSO)mapSO);
    }

    public static BurstMapSO Create(KopernicusMapSO mapSO)
    {
        var texture = mapSO.Texture;
        if (texture == null)
            return BurstMapSO.Create(new InvalidMapSO());

        return Create(texture, mapSO.Depth);
    }

    public static BurstMapSO Create(CPUTexture2D texture, MapSO.MapDepth depth)
    {
        var width = texture.Width;
        var height = texture.Height;
        var mipCount = texture.MipCount;
        var data = texture.GetRawTextureData();

        return texture switch
        {
            CPUTexture2D<CPUTexture2D.Alpha8> => BurstMapSO.Create(
                new Alpha8(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.ARGB32> => BurstMapSO.Create(
                new ARGB32(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.ARGB4444> => BurstMapSO.Create(
                new ARGB4444(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.BC4> => BurstMapSO.Create(
                new BC4(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.BC5> => BurstMapSO.Create(
                new BC5(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.BC6H> => BurstMapSO.Create(
                new BC6H(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.BC7> => BurstMapSO.Create(
                new BC7(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.BGRA32> => BurstMapSO.Create(
                new BGRA32(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.DXT1> => BurstMapSO.Create(
                new DXT1(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.DXT5> => BurstMapSO.Create(
                new DXT5(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.R8> => BurstMapSO.Create(
                new R8(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.R16> => BurstMapSO.Create(
                new R16(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.RA16> => BurstMapSO.Create(
                new RA16(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.RFloat> => BurstMapSO.Create(
                new RFloat(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.RG16> => BurstMapSO.Create(
                new RG16(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.RGB24> => BurstMapSO.Create(
                new RGB24(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.RGB565> => BurstMapSO.Create(
                new RGB565(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.RGBA32> => BurstMapSO.Create(
                new RGBA32(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.RGBA4444> => BurstMapSO.Create(
                new RGBA4444(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.RGBAFloat> => BurstMapSO.Create(
                new RGBAFloat(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.RGBAHalf> => BurstMapSO.Create(
                new RGBAHalf(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.RGFloat> => BurstMapSO.Create(
                new RGFloat(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.RGHalf> => BurstMapSO.Create(
                new RGHalf(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.RHalf> => BurstMapSO.Create(
                new RHalf(new(data, width, height, mipCount), depth)
            ),
            CPUTexture2D<CPUTexture2D.KopernicusPalette4> => BurstMapSO.Create(
                new KopernicusPalette4(new(data, width, height), depth)
            ),
            CPUTexture2D<CPUTexture2D.KopernicusPalette8> => BurstMapSO.Create(
                new KopernicusPalette8(new(data, width, height), depth)
            ),
            CPUTexture2D<KopernicusMapSO.RACPUTexture<CPUTexture2D.RG16>> => BurstMapSO.Create(
                new RG16AsRA(new(new(data, width, height, mipCount)), depth)
            ),
            CPUTexture2D<KopernicusMapSO.RACPUTexture<CPUTexture2D.RGHalf>> => BurstMapSO.Create(
                new RGHalfAsRA(new(new(data, width, height, mipCount)), depth)
            ),
            CPUTexture2D<KopernicusMapSO.RACPUTexture<CPUTexture2D.RGFloat>> => BurstMapSO.Create(
                new RGFloatAsRA(new(new(data, width, height, mipCount)), depth)
            ),
            CPUTexture2D<KopernicusMapSO.RACPUTexture<CPUTexture2D.BC5>> => BurstMapSO.Create(
                new BC5AsRA(new(new(data, width, height, mipCount)), depth)
            ),
            _ => throw new NotSupportedException(
                $"CPU texture of type {texture.GetType().FullName} is not supported by BurstKopernicusMapSO"
            ),
        };
    }
}
