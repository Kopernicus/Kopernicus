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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Kopernicus.Components;
using KSPTextureLoader;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Kopernicus.OnDemand
{
    /// <summary>
    /// MapSO Replacement to support Texture streaming
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [BurstCompile]
    public class MapSODemand : MapSO, ILoadOnDemand, IPreloadOnDemand
    {
        enum MapState : byte
        {
            Unloaded = 0,
            Loaded,
            Error
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
        private CPUTextureHandle Handle { get; set; }
        private CPUTexture2D Data { get; set; }

        private MapState State { get; set; }

        // States
        public bool IsLoaded
        {
            get => State == MapState.Loaded;

            // Setting this externally never worked, but we need to keep it for back-compat.
            [Obsolete("Setting IsLoaded is not supported and is now a no-op")]
            set
            {
                Debug.LogWarning($"[Kopernicus] Setting MapSODemand.IsLoaded externally is not supported");
            }
        }
        public bool AutoLoad { get; set; }

        // Path of the Texture
        public string Path { get; set; }

        // MapDepth
        public new MapDepth Depth { get; set; }

        // Name
        string ILoadOnDemand.Name
        {
            get { return name; }
            set { name = value; }
        }

        public void Preload()
        {
            if (IsLoaded)
                return;

            // We already have a handle, loading is already in progress or completed.
            if (Handle != null)
                return;

            var options = new TextureLoadOptions
            {
                Hint = TextureLoadHint.BatchSynchronous,
                Unreadable = false
            };
            Handle = TextureLoader.LoadCPUTexture(Path, options);
        }

        /// <summary>
        /// Load the Map
        /// </summary>
        public void Load()
        {
            // Check if the Map is already loaded
            if (IsLoaded)
                return;

            if (Handle is null)
            {
                var options = new TextureLoadOptions
                {
                    Hint = TextureLoadHint.Synchronous,
                    Unreadable = false
                };
                Handle = TextureLoader.LoadCPUTexture(Path, options);
            }

            // Load the Map
            CPUTexture2D map;
            try
            {
                map = Handle.GetTexture();
            }
            catch (Exception e)
            {
                Debug.Log($"[OD] ERROR: Failed to load map {name} at path {Path}");
                Debug.LogException(e);

                Handle?.Dispose();
                Handle = null;
                State = MapState.Error;
                return;
            }

            // CreateMap may unload the texture so we should get the asset bundle path first
            var assetBundle = Handle?.AssetBundle;

            // Make sure we're in an error state if CreateMap throws.
            State = MapState.Error;

            // If the map isn't null
            CreateMap(Depth, map);
            State = MapState.Loaded;
            Events.OnMapSOLoad.Fire(this);
            if (assetBundle is null)
                Debug.Log($"[OD] ---> Map {name} enabling self. Path = {Path}");
            else
                Debug.Log($"[OD] ---> Map {name} enabling self. Path = {Path}, Asset Bundle = {assetBundle}");
        }

        /// <summary>
        /// Unload the map
        /// </summary>
        public void Unload()
        {
            // Clear the texture handle regardless of whether we are loaded or not.
            Handle?.Dispose();
            Handle = null;
            Data = null;

            // We can only destroy the map, if it is loaded
            bool loaded = IsLoaded;
            State = MapState.Unloaded;

            if (!loaded)
                return;

            // Event
            Events.OnMapSOUnload.Fire(this);

            // Log
            Debug.Log("[OD] <--- Map " + name + " disabling self. Path = " + Path);
        }

        #region CreateMap
        void CreateMap(MapDepth depth, CPUTexture2D tex)
        {
            // If the Texture is null, abort
            if (tex == null)
            {
                Debug.Log("[OD] ERROR: Failed to load map");
                return;
            }

            _width = tex.Width;
            _height = tex.Height;
            _bpp = (int)depth;

            if (tex.Format == TextureFormat.R16 && depth == MapDepth.HeightAlpha)
                Data = new RA16Texture(tex);
            else
                Data = tex;

            _isCompiled = true;
        }

        /// <summary>
        /// Create a map from a Texture2D
        /// </summary>
        public override void CreateMap(MapDepth depth, Texture2D tex)
        {
            throw new NotSupportedException();
        }
        #endregion

        private bool EnsureLoaded()
        {
            // We failed to load once, prevent spamming the log and/or emitting NREs repeatedly.
            if (State == MapState.Error)
                return false;

            if (OnDemandStorage.OnDemandLogOnMissing)
                Debug.Log($"[OD] ERROR: read from unloaded map {name} with path {Path}, autoload = {AutoLoad}");

            if (AutoLoad)
                Load();

            return IsLoaded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WrapWidth(int x)
        {
            x %= Width;
            if (x < 0)
                x += Width;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WrapHeight(int y)
        {
            y %= Height;
            if (y < 0)
                y += Height;
            return y;
        }

        #region GetPixelByte
        public override byte GetPixelByte(int x, int y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return 0;
            }

            var pixel = Data.GetPixel32(x, y);
            if (Data.Format == TextureFormat.Alpha8)
                return pixel.a;
            else
                return pixel.r;
        }
        #endregion

        #region GetPixelColor
        public override Color GetPixelColor(int x, int y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return Color.black;
            }

            return Data.GetPixel(x, y);
        }

        public override Color GetPixelColor(double x, double y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return Color.black;
            }

            return base.GetPixelColor(x, y);
        }

        public override Color GetPixelColor(Single x, Single y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return Color.black;
            }

            return base.GetPixelColor(x, y);
        }
        #endregion

        #region GetPixelColor32
        public override Color32 GetPixelColor32(int x, int y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return default;
            }

            return Data.GetPixel32(x, y);
        }

        // Honestly Squad, why are they named GetPixelColor32, but return normal Colors instead of Color32?
        public override Color GetPixelColor32(Double x, Double y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return default;
            }

            return base.GetPixelColor32(x, y);
        }

        public override Color GetPixelColor32(Single x, Single y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return default;
            }

            return base.GetPixelColor32(x, y);
        }
        #endregion

        #region GetPixelFloat
        public override float GetPixelFloat(int x, int y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return 0f;
            }

            var pixel = Data.GetPixel(x, y);

            if (Data.Format == TextureFormat.Alpha8)
                return pixel.a;

            switch (Depth)
            {
                case MapDepth.Greyscale:
                    return pixel.r;
                case MapDepth.HeightAlpha:
                    return 0.5f * (pixel.r + pixel.a);
                case MapDepth.RGB:
                    return (1f / 3f) * (pixel.r + pixel.g + pixel.b);
                case MapDepth.RGBA:
                    return 0.25f * (pixel.r + pixel.g + pixel.b + pixel.a);
                default:
                    return 0f;
            }
        }

        public override float GetPixelFloat(double x, double y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return 0f;
            }

            return base.GetPixelFloat(x, y);
        }

        public override float GetPixelFloat(float x, float y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return 0f;
            }

            return base.GetPixelFloat(x, y);
        }
        #endregion

        #region GetPixelHeightAlpha
        private ValueHeightAlpha GetPixelValueHeightAlpha(int x, int y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new ValueHeightAlpha(0f, 0f);
            }

            var pixel = Data.GetPixel(x, y);
            switch (Depth)
            {
                case MapDepth.HeightAlpha:
                case MapDepth.RGBA:
                    return new ValueHeightAlpha(pixel.r, pixel.a);

                default:
                    return new ValueHeightAlpha(pixel.r, 1f);
            }
        }

        public override HeightAlpha GetPixelHeightAlpha(int x, int y) => GetPixelValueHeightAlpha(x, y);

        public override HeightAlpha GetPixelHeightAlpha(double x, double y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new HeightAlpha(0f, 0f);
            }

            return base.GetPixelHeightAlpha(x, y);
        }

        public override HeightAlpha GetPixelHeightAlpha(float x, float y)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new HeightAlpha(0f, 0f);
            }

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
            {
                if (!EnsureLoaded())
                    return 0f;
            }

            var pixel = Data.GetPixel(x, y);
            if (Data.Format == TextureFormat.Alpha8)
                return pixel.a;
            return pixel.r;
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

        // CompileToTexture
        public override Texture2D CompileToTexture(byte filter)
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new Texture2D(_width, _height);
            }

            var data = Data.GetRawTextureData<byte>();
            var texture = new Texture2D(Width, Height, Data.Format, Data.MipCount, false);
            texture.LoadRawTextureData(data);
            texture.Apply(false, true);
            return texture;
        }

        // Generate a greyscale texture from the stored data
        public override Texture2D CompileGreyscale()
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new Texture2D(_width, _height);
            }

            var color32 = new Color32[Size];
            for (int i = 0, y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x, ++i)
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

        // Generate a height/alpha texture from the stored data
        public override Texture2D CompileHeightAlpha()
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new Texture2D(_width, _height);
            }

            var color32 = new Color32[Size];
            for (int i = 0, y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x, ++i)
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

        // Generate an RGB texture from the stored data
        public override Texture2D CompileRGB()
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new Texture2D(_width, _height);
            }

            var color32 = new Color32[Size];
            for (int i = 0, y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x, ++i)
                    color32[i] = GetPixelColor32(x, y);
            }

            Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            compiled.SetPixels32(color32);
            compiled.Apply(false, true);
            return compiled;
        }

        // Generate an RGBA texture from the stored data
        public override Texture2D CompileRGBA()
        {
            if (!IsLoaded)
            {
                if (!EnsureLoaded())
                    return new Texture2D(_width, _height);
            }

            var color32 = new Color32[Size];
            for (int i = 0, y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x, ++i)
                    color32[i] = GetPixelColor32(x, y);
            }

            Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            compiled.SetPixels32(color32);
            compiled.Apply(false, true);
            return compiled;
        }

        sealed class RA16Texture(CPUTexture2D tex) : CPUTexture2D
        {
            readonly CPUTexture2D tex = tex;
            readonly CPUTexture2D.RG16 wrap = new(tex.GetRawTextureData<byte>(), tex.Width, tex.Height, tex.MipCount);

            public override int Width => tex.Width;

            public override int Height => tex.Height;

            public override int MipCount => tex.MipCount;

            public override TextureFormat Format => tex.Format;

            public override Color GetPixel(int x, int y, int mipLevel = 0)
            {
                var pixel = tex.GetPixel(x, y, mipLevel);
                return new(pixel.r, 1f, 1f, pixel.g);
            }
            public override Color32 GetPixel32(int x, int y, int mipLevel = 0)
            {
                var pixel = tex.GetPixel32(x, y, mipLevel);
                return new(pixel.r, 255, 255, pixel.g);
            }
            public override Color GetPixelBilinear(float u, float v, int mipLevel = 0)
            {
                var pixel = tex.GetPixelBilinear(u, v, mipLevel);
                return new(pixel.r, 1f, 1f, pixel.g);
            }

            public override NativeArray<byte> GetRawTextureData() => tex.GetRawTextureData();

            public override NativeArray<Color> GetPixels(int mipLevel = 0, Allocator allocator = Allocator.Temp)
            {
                var pixels = tex.GetPixels(mipLevel, allocator);
                for (int i = 0; i < pixels.Length; ++i)
                {
                    var p = pixels[i];
                    pixels[i] = new(p.r, p.a, p.b, p.g);
                }
                return pixels;
            }

            public override NativeArray<Color32> GetPixels32(int mipLevel = 0, Allocator allocator = Allocator.Temp)
            {
                var pixels = tex.GetPixels32(mipLevel, allocator);
                for (int i = 0; i < pixels.Length; ++i)
                {
                    var p = pixels[i];
                    pixels[i] = new(p.r, p.a, p.b, p.g);
                }
                return pixels;
            }
        }
    }
}
