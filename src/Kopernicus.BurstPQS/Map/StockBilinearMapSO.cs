using System;
using BurstPQS.Map;
using Unity.Burst;
using UnityEngine;

namespace BurstPQS.Kopernicus.Map;

/// <summary>
/// A wrapper around <see cref="StockBurstMapSO"/> that uses the stock KSP bilinear
/// filtering behavior (Y-axis wrapping at poles) instead of the Kopernicus-patched
/// behavior (Y-axis clamping). Used for MapSOs that were authored assuming stock
/// wrapping, such as the stock Moho heightmap.
/// </summary>
[BurstCompile]
public struct StockBilinearBurstMapSO(MapSO mapSO) : IMapSO, IDisposable
{
    StockBurstMapSO inner = new(mapSO);

    public readonly int Width => inner.Width;
    public readonly int Height => inner.Height;
    public readonly MapSO.MapDepth Depth => inner.Depth;

    public float GetPixelFloat(int x, int y) => inner.GetPixelFloat(x, y);

    public Color GetPixelColor(int x, int y) => inner.GetPixelColor(x, y);

    public Color32 GetPixelColor32(int x, int y) => inner.GetPixelColor32(x, y);

    public HeightAlpha GetPixelHeightAlpha(int x, int y) => inner.GetPixelHeightAlpha(x, y);

    public float GetPixelFloat(float x, float y)
    {
        ConstructBilinearCoords(
            x,
            y,
            out int minX,
            out int maxX,
            out int minY,
            out int maxY,
            out float midX,
            out float midY
        );
        return Mathf.Lerp(
            Mathf.Lerp(GetPixelFloat(minX, minY), GetPixelFloat(maxX, minY), midX),
            Mathf.Lerp(GetPixelFloat(minX, maxY), GetPixelFloat(maxX, maxY), midX),
            midY
        );
    }

    public float GetPixelFloat(double x, double y) => GetPixelFloat((float)x, (float)y);

    public Color GetPixelColor(float x, float y)
    {
        ConstructBilinearCoords(
            x,
            y,
            out int minX,
            out int maxX,
            out int minY,
            out int maxY,
            out float midX,
            out float midY
        );
        return Color.Lerp(
            Color.Lerp(GetPixelColor(minX, minY), GetPixelColor(maxX, minY), midX),
            Color.Lerp(GetPixelColor(minX, maxY), GetPixelColor(maxX, maxY), midX),
            midY
        );
    }

    public Color GetPixelColor(double x, double y) => GetPixelColor((float)x, (float)y);

    public Color32 GetPixelColor32(float x, float y)
    {
        ConstructBilinearCoords(
            x,
            y,
            out int minX,
            out int maxX,
            out int minY,
            out int maxY,
            out float midX,
            out float midY
        );
        return Color32.Lerp(
            Color32.Lerp(GetPixelColor32(minX, minY), GetPixelColor32(maxX, minY), midX),
            Color32.Lerp(GetPixelColor32(minX, maxY), GetPixelColor32(maxX, maxY), midX),
            midY
        );
    }

    public Color32 GetPixelColor32(double x, double y) => GetPixelColor32((float)x, (float)y);

    public HeightAlpha GetPixelHeightAlpha(float x, float y)
    {
        ConstructBilinearCoords(
            x,
            y,
            out int minX,
            out int maxX,
            out int minY,
            out int maxY,
            out float midX,
            out float midY
        );
        return HeightAlpha.Lerp(
            HeightAlpha.Lerp(
                GetPixelHeightAlpha(minX, minY),
                GetPixelHeightAlpha(maxX, minY),
                midX
            ),
            HeightAlpha.Lerp(
                GetPixelHeightAlpha(minX, maxY),
                GetPixelHeightAlpha(maxX, maxY),
                midX
            ),
            midY
        );
    }

    public HeightAlpha GetPixelHeightAlpha(double x, double y) =>
        GetPixelHeightAlpha((float)x, (float)y);

    public void Dispose() => inner.Dispose();

    /// <summary>
    /// Stock bilinear coordinate construction: wraps both X and Y axes (matching
    /// <see cref="MapSO.ConstructBilinearCoords(float, float)"/>).
    /// </summary>
    readonly void ConstructBilinearCoords(
        float x,
        float y,
        out int minX,
        out int maxX,
        out int minY,
        out int maxY,
        out float midX,
        out float midY
    )
    {
        x = Mathf.Abs(x - Mathf.Floor(x));
        y = Mathf.Abs(y - Mathf.Floor(y));

        float centerX = x * Width;
        float centerY = y * Height;

        minX = Mathf.FloorToInt(centerX);
        maxX = Mathf.CeilToInt(centerX);
        minY = Mathf.FloorToInt(centerY);
        maxY = Mathf.CeilToInt(centerY);

        midX = centerX - minX;
        midY = centerY - minY;

        if (maxX == Width)
            maxX = 0;
        if (maxY == Height)
            maxY = 0;
    }
}
