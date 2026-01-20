/**
 * Kopernicus Planetary System Modifier
 * -------------------------------------------------------------
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 *
 * This library is intended to be used as a plugin for Kerbal Space Program
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 *
 * https://kerbalspaceprogram.com
 */

using System;
using System.Runtime.CompilerServices;
using Kopernicus.OnDemand;
using KSPTextureLoader;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

namespace Kopernicus.Components;

[BurstCompile]
public class KopernicusMapSO : MapSO
{
    new const float Byte2Float = 0.003921569f;
    new const float Float2Byte = 255f;
    const float Ushort2Float = 1f / 65535f;

    enum MemoryFormat : byte
    {
        None = 0,

        // Numbers are chosen here so that the lower 4 bits contain the stride in memory
        R8     = 0x01,
        A8     = 0x11,
        R16    = 0x02,
        RA16   = 0x12,
        RGB24  = 0x03,
        RGBA32 = 0x04,
    }

    // A non-allocating HeightAlpha for internal use.
    struct ValueHeightAlpha
    {
        public float height;
        public float alpha;

        public ValueHeightAlpha(float height, float alpha)
        {
            this.height = height;
            this.alpha = alpha;
        }

        public static implicit operator HeightAlpha(ValueHeightAlpha ha) => new HeightAlpha(ha.height, ha.alpha);
    }

    // Representation of the map
    protected TextureHandle<Texture2D> Handle { get; set; }
    private Texture2D texture;
    private NativeArray<byte> data;
    private MemoryFormat format;
    private bool loaded = false;
    private int Stride => (int)format & 0xF;

    public bool IsLoaded => loaded;
    public new MapDepth Depth { get; set; }

    #region CreateMap
    public override void CreateMap(MapDepth depth, string name, int width, int height) =>
        throw new NotImplementedException();

    /// <summary>
    /// Load the map from the provided texture handle. This version assumes
    /// that the texture handle is available forever, even if the MapSO doesn't
    /// end up holding onto it forever.
    /// </summary>
    /// <param name="depth"></param>
    /// <param name="tex"></param>
    public override void CreateMap(MapDepth depth, Texture2D tex)
    {
        Unload();

        try
        {
            CreateMapImpl(depth, texture);
        }
        catch (Exception e)
        {
            Debug.Log($"[Kopernicus] ERROR: Failed to load map {name} at path {Handle.Path}");
            Debug.LogException(e);
            Unload();
        }
    }

    /// <summary>
    /// Load the map from the provided texture handle.
    /// </summary>
    /// <param name="depth"></param>
    /// <param name="handle"></param>
    /// <returns></returns>
    /// 
    /// <remarks>
    /// This does not take ownership of the provided handle. You are responsible
    /// for disposing of it yourself.
    /// </remarks>
    public void CreateMap(MapDepth depth, TextureHandle<Texture2D> handle)
    {
        // Unload ourselves if we are loaded.
        Unload();

        try
        {
            this.Handle = handle.Acquire();
            var texture = handle.GetTexture();

            CreateMapImpl(depth, texture);
        }
        catch (Exception e)
        {
            Debug.Log($"[Kopernicus] ERROR: Failed to load map {name} at path {handle.Path}");
            Debug.LogException(e);
            Unload();
        }
    }

    private void CreateMapImpl(MapDepth depth, Texture2D tex)
    {
        if (OnDemandStorage.UseManualMemoryManagement)
            CreateCompiledMap(depth, tex);
        else
            CreateTextureMap(depth, tex);

        loaded = true;
    }

    private void CreateCompiledMap(MapDepth depth, Texture2D tex)
    {
        _width = tex.width;
        _height = tex.height;
        _bpp = (int)depth;

        switch (depth)
        {
            case MapDepth.Greyscale:
                switch (tex.format)
                {
                    case TextureFormat.R8:
                    case TextureFormat.R16:
                    case TextureFormat.Alpha8:
                        CreateDirectlyFromTexture(tex);
                        break;

                    default:
                        CreateGreyscaleFromRGB(tex);
                        break;
                }

                break;

            case MapDepth.HeightAlpha:
                switch (tex.format)
                {
                    case TextureFormat.R16:
                        // R16 gets treated as R8A8 for HeightAlpha maps for compat with
                        // the broader planet modding ecosystem.
                        CreateDirectlyFromTexture(tex);
                        format = MemoryFormat.RA16;
                        break;

                    case TextureFormat.R8:
                    case TextureFormat.Alpha8:
                        CreateDirectlyFromTexture(tex);
                        break;

                    default:
                        CreateHeightAlpha(tex);
                        break;
                }

                break;

            case MapDepth.RGB:
                switch (tex.format)
                {
                    case TextureFormat.R8:
                    case TextureFormat.R16:
                    case TextureFormat.Alpha8:
                    case TextureFormat.RGB24:
                        CreateDirectlyFromTexture(tex);
                        break;

                    default:
                        CreateRgb(tex);
                        break;
                }

                break;

            case MapDepth.RGBA:
                switch (tex.format)
                {
                    case TextureFormat.R8:
                    case TextureFormat.R16:
                    case TextureFormat.Alpha8:
                    case TextureFormat.RGB24:
                    case TextureFormat.RGBA32:
                        CreateDirectlyFromTexture(tex);
                        break;

                    default:
                        CreateRgba(tex);
                        break;
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(depth), depth, null);
        }

        _rowWidth = _width * Stride;
        _bpp = Stride;
        _isCompiled = true;

        // Clean up what we don't need anymore
        Handle?.Dispose();
        Handle = null;
    }

    private void CreateTextureMap(MapDepth depth, Texture2D tex)
    {
        texture = tex;
        Depth = depth;
        _name = tex.name;
        _width = tex.width;
        _height = tex.height;
        _bpp = (int)depth;

        switch (tex.format)
        {
            case TextureFormat.R8:
            case TextureFormat.Alpha8:
            case TextureFormat.RGB24:
            case TextureFormat.RGBA32:
                UseRawTextureData(tex);
                _rowWidth = _width * Stride;
                break;

            case TextureFormat.R16:
                UseRawTextureData(tex);
                if (depth == MapDepth.HeightAlpha)
                    format = MemoryFormat.RA16;
                _rowWidth = _width * Stride;
                break;

            default:
                format = MemoryFormat.None;
                _bpp = 4;
                _rowWidth = _width * _bpp;
                break;
        }

        // We're compiled
        _isCompiled = true;
    }

    private void UseRawTextureData(Texture2D tex)
    {
        format = tex.format switch
        {
            TextureFormat.R8 => MemoryFormat.R8,
            TextureFormat.Alpha8 => MemoryFormat.A8,
            TextureFormat.R16 => MemoryFormat.R16,
            TextureFormat.RGB24 => MemoryFormat.RGB24,
            TextureFormat.RGBA32 => MemoryFormat.RGBA32,
            _ => throw new InvalidOperationException($"texture format {tex.format} cannot be compiled directly to a memory texture"),
        };
        data = tex.GetRawTextureData<byte>();
    }

    private unsafe void CreateDirectlyFromTexture(Texture2D tex)
    {
        switch (tex.format)
        {
            case TextureFormat.R8:
                format = MemoryFormat.R8;
                break;

            case TextureFormat.Alpha8:
                format = MemoryFormat.A8;
                break;

            case TextureFormat.R16:
                format = MemoryFormat.R16;
                break;

            case TextureFormat.RGB24:
                format = MemoryFormat.RGB24;
                break;

            case TextureFormat.RGBA32:
                format = MemoryFormat.RGBA32;
                break;

            default:
                throw new InvalidOperationException($"texture format {tex.format} cannot be compiled directly to a memory texture");
        }

        this.data = new NativeArray<byte>(tex.width * tex.height * Stride, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        var data = tex.GetRawTextureData<byte>();
        if (data.Length < this.data.Length)
            throw new InvalidOperationException("image data was too small for destination array");

        UnsafeUtility.MemCpy(this.data.GetUnsafePtr(), data.GetUnsafePtr(), this.data.Length);
    }

    [BurstCompile]
    struct CreateGreyscaleJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Color32> pixels;

        [WriteOnly]
        public NativeArray<byte> image;

        public void Execute(int index)
        {
            image[index] = pixels[index].r;
        }
    }

    private new unsafe void CreateGreyscaleFromRGB(Texture2D tex)
    {
        Color32[] pixels32 = tex.GetPixels32();
        data = new NativeArray<byte>(pixels32.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        format = MemoryFormat.R8;

        fixed (Color32* ppixels = pixels32)
        {
            var pixels = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Color32>(ppixels, pixels32.Length, Allocator.Invalid);

            var job = new CreateGreyscaleJob
            {
                pixels = pixels,
                image = data
            };

            job.Schedule(pixels32.Length, 16384)
                .Complete();
        }
    }

    [BurstCompile]
    struct CreateHeightAlphaJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Color32> pixels;

        [WriteOnly]
        public NativeArray<byte> image;

        public void Execute(int index)
        {
            var pixel = pixels[index];

            image[index * 2 + 0] = pixel.r;
            image[index * 2 + 1] = pixel.a;
        }
    }

    private new unsafe void CreateHeightAlpha(Texture2D tex)
    {
        Color32[] pixels32 = tex.GetPixels32();
        data = new NativeArray<byte>(pixels32.Length * 2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        format = MemoryFormat.RA16;

        fixed (Color32* ppixels = pixels32)
        {
            var pixels = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Color32>(ppixels, pixels32.Length, Allocator.Invalid);

            var job = new CreateHeightAlphaJob
            {
                pixels = pixels,
                image = data
            };

            job.Schedule(pixels32.Length, 16384)
                .Complete();
        }
    }


    [BurstCompile]
    struct CreateRgbJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Color32> pixels;

        [WriteOnly]
        public NativeArray<byte> image;

        public void Execute(int index)
        {
            var pixel = pixels[index];

            image[index * 3 + 0] = pixel.r;
            image[index * 3 + 1] = pixel.g;
            image[index * 3 + 2] = pixel.b;
        }
    }

    private unsafe void CreateRgb(Texture2D tex)
    {
        Color32[] pixels32 = tex.GetPixels32();
        data = new NativeArray<byte>(pixels32.Length * 3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        format = MemoryFormat.RGB24;

        fixed (Color32* ppixels = pixels32)
        {
            var pixels = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Color32>(ppixels, pixels32.Length, Allocator.Invalid);

            var job = new CreateRgbJob
            {
                pixels = pixels,
                image = data
            };

            job.Schedule(pixels32.Length, 16384)
                .Complete();
        }
    }

    [BurstCompile]
    struct CreateRgbaJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Color32> pixels;

        [WriteOnly]
        public NativeArray<byte> image;

        public void Execute(int index)
        {
            var pixel = pixels[index];

            image[index * 4 + 0] = pixel.r;
            image[index * 4 + 1] = pixel.g;
            image[index * 4 + 2] = pixel.b;
            image[index * 4 + 3] = pixel.a;
        }
    }

    private unsafe void CreateRgba(Texture2D tex)
    {
        Color32[] pixels32 = tex.GetPixels32();
        data = new NativeArray<byte>(pixels32.Length * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        format = MemoryFormat.RGB24;

        fixed (Color32* ppixels = pixels32)
        {
            var pixels = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Color32>(ppixels, pixels32.Length, Allocator.Invalid);

            var job = new CreateRgbaJob
            {
                pixels = pixels,
                image = data
            };

            job.Schedule(pixels32.Length, 16384)
                .Complete();
        }
    }
    #endregion

    /// <summary>
    /// Explicitly unload the texture data for this MapSO.
    /// </summary>
    protected void Unload()
    {
        loaded = false;
        format = MemoryFormat.None;

        _width = 0;
        _height = 0;
        _bpp = 0;
        _rowWidth = 0;

        data.Dispose();
        data = default;
        texture = null;
        Handle?.Dispose();
        Handle = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WrapWidth(int x)
    {
        if (x < 0)
        {
            // TODO: This seems wrong?
            x = Width - x;
        }
        else if (x >= Width)
        {
            x -= Width;
        }

        return x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WrapHeight(int y)
    {
        if (y < 0)
        {
            y = Height - y;
        }
        else if (y >= Height)
        {
            y -= Height;
        }

        return y;
    }

    #region GetPixelByte
    public override byte GetPixelByte(int x, int y)
    {
        if (!IsLoaded)
            return 0;

        if (!data.IsCreated)
            return (byte)(texture.GetPixel(x, y).r * Float2Byte);

        x = WrapWidth(x);
        y = WrapHeight(y);
        var index = PixelIndex(x, y);

        switch (format)
        {
            case MemoryFormat.R16:
                // We use the high byte for R16 because this is equivalent to rescaling the
                // actual u16 value to a byte value.
                return data[index + 1];

            default:
                // Otherwise use the first byte.
                return data[index];
        }
    }
    #endregion

    #region GetPixelColor
    public override Color GetPixelColor(int x, int y)
    {
        if (!IsLoaded)
            return Color.black;

        if (!data.IsCreated)
            return texture.GetPixel(x, y);

        x = WrapWidth(x);
        y = WrapHeight(y);
        var index = PixelIndex(x, y);

        float r, a;
        switch (format)
        {
            case MemoryFormat.R8:
                r = Byte2Float * data[index];
                return new Color(r, r, r, 1f);

            case MemoryFormat.A8:
                a = Byte2Float * data[index];
                return new Color(0f, 0f, 0f, a);

            case MemoryFormat.R16:
                r = Ushort2Float * (data[index] + (data[index + 1] << 8));
                return new Color(r, r, r, 1f);

            case MemoryFormat.RA16:
                r = Byte2Float * data[index];
                a = Byte2Float * data[index + 1];
                return new Color(r, r, r, a);

            case MemoryFormat.RGB24:
                return new Color(
                    Byte2Float * data[index],
                    Byte2Float * data[index + 1],
                    Byte2Float * data[index + 2],
                    1f
                );

            case MemoryFormat.RGBA32:
                return new Color(
                    Byte2Float * data[index],
                    Byte2Float * data[index + 1],
                    Byte2Float * data[index + 2],
                    Byte2Float * data[index + 3]
                );

            default:
                return Color.black;
        }
    }

    public override Color GetPixelColor(double x, double y)
    {
        if (!IsLoaded)
            return Color.black;

        return base.GetPixelColor(x, y);
    }

    public override Color GetPixelColor(Single x, Single y)
    {
        if (!IsLoaded)
            return Color.black;

        return base.GetPixelColor(x, y);
    }
    #endregion

    #region GetPixelColor32
    public override Color32 GetPixelColor32(int x, int y)
    {
        if (!IsLoaded)
            return default;

        if (!data.IsCreated)
            return texture.GetPixel(x, y);

        x = WrapWidth(x);
        y = WrapHeight(y);
        var index = PixelIndex(x, y);

        byte r, a;
        switch (format)
        {
            case MemoryFormat.R8:
                r = data[index];
                return new Color32(r, r, r, 255);

            case MemoryFormat.A8:
                a = data[index];
                return new Color32(0, 0, 0, a);

            case MemoryFormat.R16:
                r = data[index + 1];
                return new Color32(r, r, r, 255);

            case MemoryFormat.RA16:
                r = data[index];
                a = data[index + 1];
                return new Color32(r, r, r, a);

            case MemoryFormat.RGB24:
                return new Color32(
                    data[index],
                    data[index + 1],
                    data[index + 2],
                    255
                );

            case MemoryFormat.RGBA32:
                return new Color32(
                    data[index],
                    data[index + 1],
                    data[index + 2],
                    data[index + 3]
                );

            default:
                return default;
        }
    }

    // Honestly Squad, why are they named GetPixelColor32, but return normal Colors instead of Color32?
    public override Color GetPixelColor32(Double x, Double y)
    {
        if (!IsLoaded)
            return default;

        return base.GetPixelColor32(x, y);
    }

    public override Color GetPixelColor32(Single x, Single y)
    {
        if (!IsLoaded)
            return default;

        return base.GetPixelColor32(x, y);
    }
    #endregion

    #region GetPixelFloat
    public override float GetPixelFloat(int x, int y)
    {
        if (!IsLoaded)
            return 0f;

        if (!data.IsCreated)
        {
            var pixel = texture.GetPixel(x, y);

            if (texture.format == TextureFormat.Alpha8)
                return pixel.a;

            switch (Depth)
            {
                case MapDepth.Greyscale:
                    return pixel.r;
                case MapDepth.HeightAlpha:
                    return 0.5f * (pixel.r + pixel.a);
                case MapDepth.RGB:
                    return (1f / 3f) * (pixel.r + pixel.g * pixel.b);
                case MapDepth.RGBA:
                    return 0.25f * (pixel.r + pixel.g + pixel.b + pixel.a);
                default:
                    return 0f;
            }
        }

        x = WrapWidth(x);
        y = WrapHeight(y);
        var index = PixelIndex(x, y);

        switch (format)
        {
            case MemoryFormat.R8:
            case MemoryFormat.A8:
                return Byte2Float * data[index];

            case MemoryFormat.R16:
                return Ushort2Float * (data[index] + (data[index + 1] << 8));

            case MemoryFormat.RA16:
                return (Byte2Float / 2f) * (data[index] + data[index + 1]);

            case MemoryFormat.RGB24:
                return (Byte2Float / 3f) * (data[index] + data[index + 1] + data[index + 2]);

            case MemoryFormat.RGBA32:
                return (Byte2Float / 4f) * (data[index] + data[index + 1] + data[index + 2]);

            default:
                return 0f;
        }
    }

    public override float GetPixelFloat(double x, double y)
    {
        if (!IsLoaded)
            return 0f;

        return base.GetPixelFloat(x, y);
    }

    public override float GetPixelFloat(float x, float y)
    {
        if (!IsLoaded)
            return 0f;

        return base.GetPixelFloat(x, y);
    }
    #endregion

    #region GetPixelHeightAlpha
    private ValueHeightAlpha GetPixelValueHeightAlpha(int x, int y)
    {
        if (!IsLoaded)
            return new ValueHeightAlpha(0f, 0f);

        if (!data.IsCreated)
        {
            var pixel = texture.GetPixel(x, y);

            switch (Depth)
            {
                case MapDepth.HeightAlpha:
                case MapDepth.RGBA:
                    return new ValueHeightAlpha(pixel.r, pixel.a);

                default:
                    return new ValueHeightAlpha(pixel.r, 1f);
            }
        }

        x = WrapWidth(x);
        y = WrapHeight(y);
        var index = PixelIndex(x, y);

        switch (format)
        {
            case MemoryFormat.R8:
            case MemoryFormat.RGB24:
                return new ValueHeightAlpha(Byte2Float * data[index], 1f);

            case MemoryFormat.A8:
                return new ValueHeightAlpha(0f, Byte2Float * data[index]);

            case MemoryFormat.R16:
                return new ValueHeightAlpha(Ushort2Float * (data[index] + (data[index + 1] << 8)), 1f);

            case MemoryFormat.RA16:
                return new ValueHeightAlpha(Byte2Float * data[index], Byte2Float * data[index + 1]);

            case MemoryFormat.RGBA32:
                return new ValueHeightAlpha(Byte2Float * data[index], Byte2Float * data[index + 3]);

            default:
                return new ValueHeightAlpha(0f, 0f);
        }
    }

    public override HeightAlpha GetPixelHeightAlpha(int x, int y) => GetPixelValueHeightAlpha(x, y);

    public override HeightAlpha GetPixelHeightAlpha(double x, double y)
    {
        if (!IsLoaded)
            return new HeightAlpha(0f, 0f);

        return base.GetPixelHeightAlpha(x, y);
    }

    public override HeightAlpha GetPixelHeightAlpha(float x, float y)
    {
        if (!IsLoaded)
            return new HeightAlpha(0f, 0f);

        return base.GetPixelHeightAlpha(x, y);
    }
    #endregion

    #region GreyByte
    public override byte GreyByte(int x, int y) => GetPixelByte(x, y);
    #endregion

    #region GreyFloat
    public override float GreyFloat(int x, int y)
    {

        if (!IsLoaded)
            return 0f;

        if (!data.IsCreated)
            return texture.GetPixel(x, y).grayscale;

        x = WrapWidth(x);
        y = WrapHeight(y);
        var index = PixelIndex(x, y);

        switch (format)
        {
            case MemoryFormat.R8:
            case MemoryFormat.A8:
            case MemoryFormat.RA16:
            case MemoryFormat.RGB24:
            case MemoryFormat.RGBA32:
                return Byte2Float * data[index];

            case MemoryFormat.R16:
                return Ushort2Float * (data[index] + (data[index + 1] << 8));

            default:
                return 0f;
        }
    }
    #endregion

    #region PixelByte
    public override byte[] PixelByte(int x, int y)
    {
        var c = GetPixelColor32(x, y);

        switch (Depth)
        {
            case MapDepth.Greyscale:
                return new[] { c.r };
            case MapDepth.HeightAlpha:
                return new[] { c.r, c.a };
            case MapDepth.RGB:
                return new[] { c.r, c.g, c.b };
            default:
                return new[] { c.r, c.g, c.b, c.a };
        }
    }
    #endregion

    #region CompileToTexture
    public override Texture2D CompileToTexture(byte filter)
    {
        if (!IsLoaded)
            return new Texture2D(1, 1);

        if (texture is null)
        {
            int stride = Stride;
            Color32[] color32 = new Color32[Size];
            for (Int32 i = 0; i < Size; i++)
            {
                val = (byte)((data[i * stride] & filter) == 0 ? 0 : 255);
                color32[i] = new Color32(val, val, val, 255);
            }

            Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            compiled.SetPixels32(color32);
            compiled.Apply(false, true);
            return compiled;
        }
        else
        {
            Texture2D compiled = UnityEngine.Object.Instantiate(texture);
            compiled.Apply(false, true);
            return compiled;
        }
    }

    // Generate a greyscale texture from the stored data
    public override Texture2D CompileGreyscale()
    {
        if (!IsLoaded)
            return new Texture2D(1, 1);

        if (texture is null)
        {
            var color32 = new Color32[Size];
            for (int i = 0, y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x)
                {
                    var v = GetPixelByte(x, y);
                    color32[i] = new Color32(v, v, v, byte.MaxValue);
                }
            }

            Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            compiled.SetPixels32(color32);
            compiled.Apply(false, true);
            return compiled;
        }
        else
        {
            Texture2D compiled = UnityEngine.Object.Instantiate(texture);
            compiled.Apply(false, true);
            return compiled;
        }
    }

    // Generate a height/alpha texture from the stored data
    public override Texture2D CompileHeightAlpha()
    {
        if (!IsLoaded)
            return new Texture2D(1, 1);

        if (texture is null)
        {
            var color32 = new Color32[Size];
            for (int i = 0, y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x)
                {
                    var ha = GetPixelValueHeightAlpha(x, y);
                    color32[i] = new Color(ha.height, ha.height, ha.height, ha.alpha);
                }
            }

            Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            compiled.SetPixels32(color32);
            compiled.Apply(false, true);
            return compiled;
        }
        else
        {
            Texture2D compiled = UnityEngine.Object.Instantiate(texture);
            compiled.Apply(false, true);
            return compiled;
        }
    }

    // Generate an RGB texture from the stored data
    public override Texture2D CompileRGB()
    {
        if (!IsLoaded)
            return new Texture2D(1, 1);

        if (texture is null)
        {
            var color32 = new Color32[Size];
            for (int i = 0, y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x)
                    color32[i] = GetPixelColor32(x, y);
            }

            Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            compiled.SetPixels32(color32);
            compiled.Apply(false, true);
            return compiled;
        }
        else
        {
            Texture2D compiled = UnityEngine.Object.Instantiate(texture);
            compiled.Apply(false, true);
            return compiled;
        }
    }

    // Generate an RGBA texture from the stored data
    public override Texture2D CompileRGBA()
    {
        if (!IsLoaded)
            return new Texture2D(1, 1);

        if (texture is null)
        {
            var color32 = new Color32[Size];
            for (int i = 0, y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x)
                    color32[i] = GetPixelColor32(x, y);
            }

            Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            compiled.SetPixels32(color32);
            compiled.Apply(false, true);
            return compiled;
        }
        else
        {
            Texture2D compiled = UnityEngine.Object.Instantiate(texture);
            compiled.Apply(false, true);
            return compiled;
        }
    }

    #endregion

}