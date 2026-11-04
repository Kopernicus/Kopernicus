using System;
using System.Runtime.CompilerServices;
using BurstPQS.Map;
using BurstPQS.Util;
using Kopernicus.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace BurstPQS.Kopernicus.Map;

/// <summary>
/// An <see cref="IMapSO"/> implementation for <see cref="KopernicusCBAttributeMapSO"/>.
/// Stores biome index data and a biome color palette, implementing the same
/// nearest-biome bilinear sampling that Kopernicus uses for color lookups.
/// </summary>
[BurstCompile]
public struct BurstCBAttributeMapSO : IMapSO, IDisposable
{
    const float Byte2Float = 0.003921569f;

    NativeArray<byte> data;
    NativeArray<Color> biomeColors;
    int rowWidth;
    ulong gchandle;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public MapSO.MapDepth Depth => MapSO.MapDepth.Greyscale;

    public unsafe BurstCBAttributeMapSO(KopernicusCBAttributeMapSO mapSO)
    {
        Width = mapSO.Width;
        Height = mapSO.Height;
        rowWidth = mapSO.RowWidth;

        var dataptr = UnsafeUtility.PinGCArrayAndGetDataAddress(mapSO._data, out gchandle);
        data = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
            dataptr,
            mapSO._data.Length,
            Allocator.Invalid
        );

        var attrs = mapSO.Attributes;
        biomeColors = new NativeArray<Color>(attrs.Length, Allocator.Persistent);
        for (int i = 0; i < attrs.Length; i++)
            biomeColors[i] = attrs[i].mapColor;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    readonly int PixelIndex(int x, int y)
    {
        x = MathUtil.Clamp(x, 0, Width - 1);
        y = MathUtil.Clamp(y, 0, Height - 1);
        return x + y * rowWidth;
    }

    // --- Int coordinate getters ---

    public float GetPixelFloat(int x, int y) => data[PixelIndex(x, y)] * Byte2Float;

    public Color GetPixelColor(int x, int y)
    {
        int idx = data[PixelIndex(x, y)];
        return idx < biomeColors.Length ? biomeColors[idx] : Color.black;
    }

    public Color32 GetPixelColor32(int x, int y) => (Color32)GetPixelColor(x, y);

    public HeightAlpha GetPixelHeightAlpha(int x, int y) =>
        new(data[PixelIndex(x, y)] * Byte2Float, 1f);

    // --- Float/Double Float and HeightAlpha: standard bilinear (stock MapSO behavior) ---

    public float GetPixelFloat(float x, float y) => MapSODefaults.GetPixelFloat(ref this, x, y);

    public float GetPixelFloat(double x, double y) => MapSODefaults.GetPixelFloat(ref this, x, y);

    public HeightAlpha GetPixelHeightAlpha(float x, float y) =>
        MapSODefaults.GetPixelHeightAlpha(ref this, x, y);

    public HeightAlpha GetPixelHeightAlpha(double x, double y) =>
        MapSODefaults.GetPixelHeightAlpha(ref this, x, y);

    // --- Float/Double Color: nearest-biome bilinear (Kopernicus behavior) ---

    public Color GetPixelColor(float x, float y) => GetPixelColorBilinear(x, y);

    public Color GetPixelColor(double x, double y) => GetPixelColorBilinear(x, y);

    public Color32 GetPixelColor32(float x, float y) => (Color32)GetPixelColorBilinear(x, y);

    public Color32 GetPixelColor32(double x, double y) => (Color32)GetPixelColorBilinear(x, y);

    /// <summary>
    /// Matches Kopernicus's <c>GetPixelBiome</c>: performs bilinear interpolation of
    /// biome indices, then selects the corner biome closest to the interpolated value.
    /// </summary>
    Color GetPixelColorBilinear(double x, double y)
    {
        GetBilinearCoordinates(
            x,
            y,
            Width,
            Height,
            out int minX,
            out int maxX,
            out int minY,
            out int maxY,
            out double midX,
            out double midY
        );

        byte b00 = data[minX + minY * rowWidth];
        byte b10 = data[maxX + minY * rowWidth];
        byte b01 = data[minX + maxY * rowWidth];
        byte b11 = data[maxX + maxY * rowWidth];

        byte biome;
        if (b00 == b10 && b00 == b01 && b00 == b11)
        {
            biome = b00;
        }
        else
        {
            double i00 = b00;
            double i10 = b10;
            double i01 = b01;
            double i11 = b11;

            double top = i00 + (i10 - i00) * midX;
            double bot = i01 + (i11 - i01) * midX;
            double interp = top + (bot - top) * midY;

            biome = b00;
            double bestDist = DiffMagnitude(i00, interp);

            double dist = DiffMagnitude(i10, interp);
            if (dist < bestDist)
            {
                bestDist = dist;
                biome = b10;
            }

            dist = DiffMagnitude(i01, interp);
            if (dist < bestDist)
            {
                bestDist = dist;
                biome = b01;
            }

            dist = DiffMagnitude(i11, interp);
            if (dist < bestDist)
            {
                biome = b11;
            }
        }

        if (biome >= biomeColors.Length)
            return Color.black;

        return biomeColors[biome];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static double DiffMagnitude(double a, double b)
    {
        double d = a - b;
        return d * d;
    }

    /// <summary>
    /// Matches Kopernicus's <c>GetBilinearCoordinates</c>: wraps X, clamps Y to [0, 1).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static unsafe void GetBilinearCoordinates(
        double x,
        double y,
        int width,
        int height,
        out int minX,
        out int maxX,
        out int minY,
        out int maxY,
        out double midX,
        out double midY
    )
    {
        x = Math.Abs(x - Math.Floor(x));
        double cx = x * width;
        minX = (int)cx;
        maxX = minX + 1;
        midX = cx - minX;
        if (maxX == width)
            maxX = 0;

        if (y >= 1.0)
        {
            // BitDecrement(1.0): largest double less than 1.0
            // Matches Kopernicus's lessThanOneDouble = Utility.BitDecrement(1.0)
            long bits = 0x3FEFFFFFFFFFFFFF;
            y = *(double*)&bits;
        }
        else if (y < 0.0)
        {
            y = 0.0;
        }

        double cy = y * height;
        minY = (int)cy;
        maxY = minY + 1;
        midY = cy - minY;
        if (maxY == height)
            maxY = height - 1;
    }

    public void Dispose()
    {
        if (gchandle != 0)
            UnsafeUtility.ReleaseGCObject(gchandle);

        biomeColors.Dispose();
        this = default;
    }
}
