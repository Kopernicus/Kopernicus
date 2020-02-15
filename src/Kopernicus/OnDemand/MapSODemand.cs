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
using Kopernicus.Components;
using UnityEngine;

namespace Kopernicus.OnDemand
{
    /// <summary>
    /// MapSO Replacement to support Texture streaming
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class MapSODemand : MapSO, ILoadOnDemand
    {
        // Representation of the map
        private Texture2D Data { get; set; }
        private NativeByteArray Image { get; set; }

        // States
        public Boolean IsLoaded { get; set; }
        public Boolean AutoLoad { get; set; }

        // Path of the Texture
        public String Path { get; set; }

        // MapDepth
        public new MapDepth Depth { get; set; }

        // Name
        String ILoadOnDemand.Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Load the Map
        /// </summary>
        public void Load()
        {
            // Check if the Map is already loaded
            if (IsLoaded)
            {
                return;
            }

            // Load the Map
            Texture2D map = OnDemandStorage.LoadTexture(Path, false, false, false);

            // If the map isn't null
            if (map != null)
            {
                CreateMap(Depth, map);
                IsLoaded = true;
                Events.OnMapSOLoad.Fire(this);
                Debug.Log("[OD] ---> Map " + name + " enabling self. Path = " + Path);
                return;
            }

            // Return nothing
            Debug.Log("[OD] ERROR: Failed to load map " + name + " at path " + Path);
        }

        /// <summary>
        /// Unload the map
        /// </summary>
        public void Unload()
        {
            // We can only destroy the map, if it is loaded
            if (!IsLoaded)
            {
                return;
            }

            // Nuke the map
            if (OnDemandStorage.UseManualMemoryManagement)
            {
                Image.Free();
            }
            else
            {
                DestroyImmediate(Data);
            }

            // Set flags
            IsLoaded = false;

            // Event
            Events.OnMapSOUnload.Fire(this);

            // Log
            Debug.Log("[OD] <--- Map " + name + " disabling self. Path = " + Path);
        }

        /// <summary>
        /// Create a map from a Texture2D
        /// </summary>
        public override void CreateMap(MapDepth depth, Texture2D tex)
        {
            // If the Texture is null, abort
            if (tex == null)
            {
                Debug.Log("[OD] ERROR: Failed to load map");
                return;
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                _name = tex.name;
                _width = tex.width;
                _height = tex.height;
                _bpp = (Int32) depth;
                _rowWidth = _width * _bpp;
                switch (depth)
                {
                    case MapDepth.Greyscale:
                    {
                        if (tex.format != TextureFormat.Alpha8)
                        {
                            CreateGreyscaleFromRgb(tex);
                        }
                        else
                        {
                            CreateGreyscaleFromAlpha(tex);
                        }

                        break;
                    }

                    case MapDepth.HeightAlpha:
                    {
                        CreateHeightAlpha(tex);
                        break;
                    }

                    case MapDepth.RGB:
                    {
                        CreateRgb(tex);
                        break;
                    }

                    case MapDepth.RGBA:
                    {
                        CreateRgba(tex);
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(depth), depth, null);
                }

                _isCompiled = true;

                // Clean up what we don't need anymore
                DestroyImmediate(tex);
            }
            else
            {
                // Set _data
                Data = tex;

                // Variables
                _width = tex.width;
                _height = tex.height;
                Depth = depth;
                _bpp = 4;
                _rowWidth = _width * _bpp;

                // We're compiled
                _isCompiled = true;
            }
        }

        private new void CreateGreyscaleFromAlpha(Texture2D tex)
        {
            Color32[] pixels32 = tex.GetPixels32();
            Image = new NativeByteArray(pixels32.Length);
            for (Int32 i = 0; i < pixels32.Length; i++)
            {
                Image[i] = pixels32[i].a;
            }
        }

        private void CreateGreyscaleFromRgb(Texture2D tex)
        {
            Color32[] pixels32 = tex.GetPixels32();
            Image = new NativeByteArray(pixels32.Length);
            for (Int32 i = 0; i < pixels32.Length; i++)
            {
                Image[i] = pixels32[i].r;
            }
        }

        private new void CreateHeightAlpha(Texture2D tex)
        {
            Color32[] pixels32 = tex.GetPixels32();
            Image = new NativeByteArray(pixels32.Length * 2);
            for (Int32 i = 0; i < pixels32.Length; i++)
            {
                Image[i * 2] = pixels32[i].r;
                Image[i * 2 + 1] = pixels32[i].a;
            }
        }

        private void CreateRgb(Texture2D tex)
        {
            Color32[] pixels32 = tex.GetPixels32();
            Image = new NativeByteArray(pixels32.Length * 3);
            for (Int32 i = 0; i < pixels32.Length; i++)
            {
                Image[i * 3] = pixels32[i].r;
                Image[i * 3 + 1] = pixels32[i].g;
                Image[i * 3 + 2] = pixels32[i].b;
            }
        }

        private void CreateRgba(Texture2D tex)
        {
            Color32[] pixels32 = tex.GetPixels32();
            Image = new NativeByteArray(pixels32.Length * 4);
            for (Int32 i = 0; i < pixels32.Length; i++)
            {
                Image[i * 3] = pixels32[i].r;
                Image[i * 3 + 1] = pixels32[i].g;
                Image[i * 3 + 2] = pixels32[i].b;
                Image[i * 3 + 3] = pixels32[i].a;
            }
        }

        // GetPixelByte
        public override Byte GetPixelByte(Int32 x, Int32 y)
        {
            // If we aren't loaded....
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting pixelbyte with unloaded map " + name + " of path " + Path +
                              ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return 0;
                }
            }

            if (!OnDemandStorage.UseManualMemoryManagement)
            {
                return (Byte) (Data.GetPixel(x, y).r * Float2Byte);
            }

            if (x < 0)
            {
                x = Width - x;
            }
            else if (x >= Width)
            {
                x -= Width;
            }

            if (y < 0)
            {
                y = Height - y;
            }
            else if (y >= Height)
            {
                y -= Height;
            }

            return Image[PixelIndex(x, y)];
        }

        // GetPixelColor - Double
        public override Color GetPixelColor(Double x, Double y)
        {
            if (IsLoaded)
            {
                return base.GetPixelColor(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelColD with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return Color.black;
            }

            return base.GetPixelColor(x, y);
        }

        // GetPixelColor - Float
        public override Color GetPixelColor(Single x, Single y)
        {
            if (IsLoaded)
            {
                return base.GetPixelColor(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelColF with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return Color.black;
            }

            return base.GetPixelColor(x, y);
        }

        // GetPixelColor - Int
        public override Color GetPixelColor(Int32 x, Int32 y)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting pixelColI with unloaded map " + name + " of path " + Path +
                              ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return Color.black;
                }
            }

            if (!OnDemandStorage.UseManualMemoryManagement)
            {
                return Data.GetPixel(x, y);
            }

            index = PixelIndex(x, y);
            switch (_bpp)
            {
                case 3:
                    return new Color(Byte2Float * Image[index], Byte2Float * Image[index + 1],
                        Byte2Float * Image[index + 2], 1f);
                case 4:
                    return new Color(Byte2Float * Image[index], Byte2Float * Image[index + 1],
                        Byte2Float * Image[index + 2], Byte2Float * Image[index + 3]);
            }

            if (_bpp != 2)
            {
                retVal = Byte2Float * Image[index];
                return new Color(retVal, retVal, retVal, 1f);
            }

            retVal = Byte2Float * Image[index];
            return new Color(retVal, retVal, retVal, Byte2Float * Image[index + 1]);
        }

        // GetPixelColor32 - Double
        public override Color GetPixelColor32(Double x, Double y)
        {
            if (IsLoaded)
            {
                return base.GetPixelColor32(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelCol32D with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return Color.black;
            }

            return base.GetPixelColor32(x, y);
        }

        // GetPixelColor32 - Float - Honestly Squad, why are they named GetPixelColor32, but return normal Colors instead of Color32?
        public override Color GetPixelColor32(Single x, Single y)
        {
            if (IsLoaded)
            {
                return base.GetPixelColor32(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelCol32F with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return Color.black;
            }

            return base.GetPixelColor32(x, y);
        }

        // GetPixelColor32 - Int
        public override Color32 GetPixelColor32(Int32 x, Int32 y)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting pixelCol32I with unloaded map " + name + " of path " + Path +
                              ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new Color32();
                }
            }

            if (!OnDemandStorage.UseManualMemoryManagement)
            {
                return Data.GetPixel(x, y);
            }

            index = PixelIndex(x, y);
            switch (_bpp)
            {
                case 3:
                    return new Color32(Image[index], Image[index + 1], Image[index + 2], 255);
                case 4:
                    return new Color32(Image[index], Image[index + 1], Image[index + 2], Image[index + 3]);
            }

            if (_bpp != 2)
            {
                val = Image[index];
                return new Color32(val, val, val, 255);
            }

            val = Image[index];
            return new Color32(val, val, val, Image[index + 1]);
        }

        // GetPixelFloat - Double
        public override Single GetPixelFloat(Double x, Double y)
        {
            if (IsLoaded)
            {
                return base.GetPixelFloat(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelFloatD with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return 0f;
            }

            return base.GetPixelFloat(x, y);
        }

        // GetPixelFloat - Float
        public override Single GetPixelFloat(Single x, Single y)
        {
            if (IsLoaded)
            {
                return base.GetPixelFloat(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelFloatF with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return 0f;
            }

            return base.GetPixelFloat(x, y);
        }

        // GetPixelFloat - Integer
        public override Single GetPixelFloat(Int32 x, Int32 y)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting pixelFloatI with unloaded map " + name + " of path " + Path +
                              ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return 0f;
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                retVal = 0f;
                index = PixelIndex(x, y);
                for (Int32 i = 0; i < _bpp; i++)
                {
                    retVal += Image[index + i];
                }

                retVal /= _bpp;
                retVal *= Byte2Float;
                return retVal;
            }

            Color pixel = Data.GetPixel(x, y);
            Single value = 0f;
            switch (Depth)
            {
                case MapDepth.Greyscale:
                    value = pixel.r;
                    break;
                case MapDepth.HeightAlpha:
                    value = pixel.r + pixel.a;
                    break;
                case MapDepth.RGB:
                    value = pixel.r + pixel.g + pixel.b;
                    break;
                case MapDepth.RGBA:
                    value = pixel.r + pixel.g + pixel.b + pixel.a;
                    break;
            }

            // Enhanced support for L8 .dds
            if (Data.format == TextureFormat.Alpha8)
            {
                value = pixel.a;
            }

            return value / (Int32) Depth;
        }

        // GetPixelHeightAlpha - Double
        public override HeightAlpha GetPixelHeightAlpha(Double x, Double y)
        {
            if (IsLoaded)
            {
                return base.GetPixelHeightAlpha(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelHeightAlphaD with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return new HeightAlpha(0f, 0f);
            }

            return base.GetPixelHeightAlpha(x, y);
        }

        // GetPixelHeightAlpha - Float
        public override HeightAlpha GetPixelHeightAlpha(Single x, Single y)
        {
            if (IsLoaded)
            {
                return base.GetPixelHeightAlpha(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelHeightAlphaF with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return new HeightAlpha(0f, 0f);
            }

            return base.GetPixelHeightAlpha(x, y);
        }

        // GetPixelHeightAlpha - Int
        public override HeightAlpha GetPixelHeightAlpha(Int32 x, Int32 y)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting pixelHeightAlphaI with unloaded map " + name + " of path " +
                              Path + ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new HeightAlpha(0f, 0f);
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                index = PixelIndex(x, y);
                if (_bpp == 2)
                {
                    return new HeightAlpha(Byte2Float * Image[index], Byte2Float * Image[index + 1]);
                }

                if (_bpp != 4)
                {
                    return new HeightAlpha(Byte2Float * Image[index], 1f);
                }

                val = Image[index];
                return new HeightAlpha(Byte2Float * Image[index], Byte2Float * Image[index + 3]);
            }

            Color pixel = Data.GetPixel(x, y);
            if (Depth == MapDepth.HeightAlpha || Depth == MapDepth.RGBA)
            {
                return new HeightAlpha(pixel.r, pixel.a);
            }

            return new HeightAlpha(pixel.r, 1f);
        }

        // GreyByte
        public override Byte GreyByte(Int32 x, Int32 y)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting GreyByteI with unloaded map " + name + " of path " + Path +
                              ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return 0;
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                return Image[PixelIndex(x, y)];
            }

            return (Byte) (Float2Byte * Data.GetPixel(x, y).r);
        }

        // GreyFloat
        public override Single GreyFloat(Int32 x, Int32 y)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting GreyFloat with unloaded map " + name + " of path " + Path +
                              ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return 0f;
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                return Byte2Float * Image[PixelIndex(x, y)];
            }

            return Data.GetPixel(x, y).grayscale;
        }

        // PixelByte
        public override Byte[] PixelByte(Int32 x, Int32 y)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting pixelByte with unloaded map " + name + " of path " + Path +
                              ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new Byte[_bpp];
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                Byte[] numArray = new Byte[_bpp];
                index = PixelIndex(x, y);
                for (Int32 i = 0; i < _bpp; i++)
                {
                    numArray[i] = Image[index + i];
                }

                return numArray;
            }

            Color c = Data.GetPixel(x, y);
            switch (Depth)
            {
                case MapDepth.Greyscale:
                    return new[] {(Byte) c.r};
                case MapDepth.HeightAlpha:
                    return new[] {(Byte) c.r, (Byte) c.a};
                case MapDepth.RGB:
                    return new[] {(Byte) c.r, (Byte) c.g, (Byte) c.b};
                default:
                    return new[] {(Byte) c.r, (Byte) c.g, (Byte) c.b, (Byte) c.a};
            }
        }

        // CompileToTexture
        public override Texture2D CompileToTexture(Byte filter)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: compiling with unloaded map " + name + " of path " + Path + ", autoload = " +
                              AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new Texture2D(_width, _height);
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                Color32[] color32 = new Color32[Size];
                for (Int32 i = 0; i < Size; i++)
                {
                    val = (Byte) ((Image[i] & filter) == 0 ? 0 : 255);
                    color32[i] = new Color32(val, val, val, 255);
                }

                Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                compiled.SetPixels32(color32);
                compiled.Apply(false, true);
                return compiled;
            }
            else
            {
                Texture2D compiled = UnityEngine.Object.Instantiate(Data);
                compiled.Apply(false, true);
                return compiled;
            }
        }

        // Generate a greyscale texture from the stored data
        public override Texture2D CompileGreyscale()
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: compiling with unloaded map " + name + " of path " + Path + ", autoload = " +
                              AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new Texture2D(_width, _height);
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                Color32[] color32 = new Color32[Size];
                for (Int32 i = 0; i < Size; i++)
                {
                    val = Image[i];
                    color32[i] = new Color32(val, val, val, 255);
                }

                Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                compiled.SetPixels32(color32);
                compiled.Apply(false, true);
                return compiled;
            }
            else
            {
                Texture2D compiled = UnityEngine.Object.Instantiate(Data);
                compiled.Apply(false, true);
                return compiled;
            }
        }

        // Generate a height/alpha texture from the stored data
        public override Texture2D CompileHeightAlpha()
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: compiling with unloaded map " + name + " of path " + Path + ", autoload = " +
                              AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new Texture2D(_width, _height);
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                Color32[] color32 = new Color32[Width * Height];
                for (Int32 i = 0; i < Width * Height; i++)
                {
                    val = Image[i * 2];
                    color32[i] = new Color32(val, val, val, Image[i * 2 + 1]);
                }

                Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                compiled.SetPixels32(color32);
                compiled.Apply(false, true);
                return compiled;
            }
            else
            {
                Texture2D compiled = UnityEngine.Object.Instantiate(Data);
                compiled.Apply(false, true);
                return compiled;
            }
        }

        // Generate an RGB texture from the stored data
        public override Texture2D CompileRGB()
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: compiling with unloaded map " + name + " of path " + Path + ", autoload = " +
                              AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new Texture2D(_width, _height);
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                Color32[] color32 = new Color32[Width * Height];
                for (Int32 i = 0; i < Width * Height; i++)
                {
                    color32[i] = new Color32(Image[i * 3], Image[i * 3 + 1], Image[i * 3 + 2], 255);
                }

                Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                compiled.SetPixels32(color32);
                compiled.Apply(false, true);
                return compiled;
            }
            else
            {
                Texture2D compiled = UnityEngine.Object.Instantiate(Data);
                compiled.Apply(false, true);
                return compiled;
            }
        }

        // Generate an RGBA texture from the stored data
        public override Texture2D CompileRGBA()
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: compiling with unloaded map " + name + " of path " + Path + ", autoload = " +
                              AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new Texture2D(_width, _height);
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                Color32[] color32 = new Color32[Width * Height];
                for (Int32 i = 0; i < Width * Height; i++)
                {
                    color32[i] = new Color32(Image[i * 3], Image[i * 3 + 1], Image[i * 3 + 2], Image[i * 3 + 3]);
                }

                Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                compiled.SetPixels32(color32);
                compiled.Apply(false, true);
                return compiled;
            }
            else
            {
                Texture2D compiled = UnityEngine.Object.Instantiate(Data);
                compiled.Apply(false, true);
                return compiled;
            }
        }
    }
}