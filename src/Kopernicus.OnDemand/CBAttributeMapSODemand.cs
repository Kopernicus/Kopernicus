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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using UnityEngine;

namespace Kopernicus
{
    namespace OnDemand
    {
        public class CBAttributeMapSODemand : CBAttributeMapSO, ILoadOnDemand
        {
            // Representation of the map
            protected new Texture2D _data { get; set; }
            protected NativeByteArray _image { get; set; }

            // States
            public Boolean IsLoaded { get; set; }
            public Boolean AutoLoad { get; set; }

            // Path of the Texture
            public String Path { get; set; }

            // MapDepth
            public new MapDepth Depth { get; set; }

            /// <summary>
            /// Load the Map
            /// </summary>
            public void Load()
            {
                // Check if the Map is already loaded
                if (IsLoaded)
                    return;

                // Load the Map
                Texture2D map = OnDemandStorage.LoadTexture(Path, false, false, false);

                // If the map isn't null
                if (map != null)
                {
                    CreateMap(Depth, map);
                    IsLoaded = true;
                    Events.OnCBMapSOLoad.Fire(this);
                    Debug.Log("[OD] ---> CBmap " + name + " enabling self. Path = " + Path);
                    return;
                }

                // Return nothing
                Debug.Log("[OD] ERROR: Failed to load CBmap " + name + " at path " + Path);
            }

            /// <summary>
            /// Unload the map
            /// </summary>
            public void Unload()
            {
                // We can only destroy the map, if it is loaded
                if (!IsLoaded)
                    return;

                // Nuke the map
                if (OnDemandStorage.useManualMemoryManagement)
                {
                    _image.Free();
                }
                else
                {
                    DestroyImmediate(_data);
                }

                // Set flags
                IsLoaded = false;

                // Event
                Events.OnCBMapSOUnload.Fire(this);

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
                    Debug.Log("[OD] ERROR: Failed to load CBmap");
                    return;
                }

                if (OnDemandStorage.useManualMemoryManagement)
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
                                CreateGreyscaleFromRGB(tex);
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
                            CreateRGB(tex);
                            break;
                        }
                        case MapDepth.RGBA:
                        {
                            CreateRGBA(tex);
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
                    _data = tex;

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

            protected new void CreateGreyscaleFromAlpha(Texture2D tex)
            {
                Color32[] pixels32 = tex.GetPixels32();
                _image = new NativeByteArray(pixels32.Length);
                for (Int32 i = 0; i < pixels32.Length; i++)
                {
                    _image[i] = pixels32[i].a;
                }
            }

            protected new void CreateGreyscaleFromRGB(Texture2D tex)
            {
                Color32[] pixels32 = tex.GetPixels32();
                _image = new NativeByteArray(pixels32.Length);
                for (Int32 i = 0; i < pixels32.Length; i++)
                {
                    _image[i] = pixels32[i].r;
                }
            }

            protected new void CreateHeightAlpha(Texture2D tex)
            {
                Color32[] pixels32 = tex.GetPixels32();
                _image = new NativeByteArray(pixels32.Length * 2);
                for (Int32 i = 0; i < pixels32.Length; i++)
                {
                    _image[i * 2] = pixels32[i].r;
                    _image[i * 2 + 1] = pixels32[i].a;
                }
            }

            protected new void CreateRGB(Texture2D tex)
            {
                Color32[] pixels32 = tex.GetPixels32();
                _image = new NativeByteArray(pixels32.Length * 3);
                for (Int32 i = 0; i < pixels32.Length; i++)
                {
                    _image[i * 3] = pixels32[i].r;
                    _image[i * 3 + 1] = pixels32[i].g;
                    _image[i * 3 + 2] = pixels32[i].b;
                }
            }

            protected new void CreateRGBA(Texture2D tex)
            {
                Color32[] pixels32 = tex.GetPixels32();
                _image = new NativeByteArray(pixels32.Length * 4);
                for (Int32 i = 0; i < pixels32.Length; i++)
                {
                    _image[i * 3] = pixels32[i].r;
                    _image[i * 3 + 1] = pixels32[i].g;
                    _image[i * 3 + 2] = pixels32[i].b;
                    _image[i * 3 + 3] = pixels32[i].a;
                }
            }

            public CBAttributeMapSODemand()
                : base()
            {
                // register here or on PQSMod creation?
                // for now we'll do it on creation (i.e. elsewhere)
            }

            // GetAtt
            public override MapAttribute GetAtt(Double lat, Double lon)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting attribute with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return Attributes[0];
                }

                return base.GetAtt(lat, lon);
            }

            // GetPixelByte
            public override Byte GetPixelByte(Int32 x, Int32 y)
            {
                // If we aren't loaded....
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting pixelbyte with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return 0;
                }

                if (OnDemandStorage.useManualMemoryManagement)
                {
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

                    return _image[PixelIndex(x, y)];
                }

                return (Byte) (_data.GetPixel(x, y).r * Float2Byte);
            }

            // GetPixelColor - Double
            public override Color GetPixelColor(Double x, Double y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting pixelColD with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return Color.black;
                }

                return base.GetPixelColor(x, y);
            }

            // GetPixelColor - Float
            public override Color GetPixelColor(Single x, Single y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting pixelColF with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return Color.black;
                }

                return base.GetPixelColor(x, y);
            }

            // GetPixelColor - Int
            public override Color GetPixelColor(Int32 x, Int32 y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting pixelColI with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return Color.black;
                }

                if (OnDemandStorage.useManualMemoryManagement)
                {
                    index = PixelIndex(x, y);
                    if (_bpp == 3)
                    {
                        return new Color(Byte2Float * _image[index], Byte2Float * _image[index + 1],
                            Byte2Float * _image[index + 2], 1f);
                    }

                    if (_bpp == 4)
                    {
                        return new Color(Byte2Float * _image[index], Byte2Float * _image[index + 1],
                            Byte2Float * _image[index + 2], Byte2Float * _image[index + 3]);
                    }

                    if (_bpp != 2)
                    {
                        retVal = Byte2Float * _image[index];
                        return new Color(retVal, retVal, retVal, 1f);
                    }

                    retVal = Byte2Float * _image[index];
                    return new Color(retVal, retVal, retVal, Byte2Float * _image[index + 1]);
                }

                return _data.GetPixel(x, y);
            }

            // GetPixelColor32 - Double
            public override Color GetPixelColor32(Double x, Double y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting pixelCol32D with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return Color.black;
                }

                return base.GetPixelColor32(x, y);
            }

            // GetPixelColor32 - Float - Honestly Squad, why are they named GetPixelColor32, but return normal Colors instead of Color32?
            public override Color GetPixelColor32(Single x, Single y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting pixelCol32F with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return Color.black;
                }

                return base.GetPixelColor32(x, y);
            }

            // GetPixelColor32 - Int
            public override Color32 GetPixelColor32(Int32 x, Int32 y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting pixelCol32I with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new Color32();
                }

                if (OnDemandStorage.useManualMemoryManagement)
                {
                    index = PixelIndex(x, y);
                    if (_bpp == 3)
                    {
                        return new Color32(_image[index], _image[index + 1], _image[index + 2], 255);
                    }

                    if (_bpp == 4)
                    {
                        return new Color32(_image[index], _image[index + 1], _image[index + 2], _image[index + 3]);
                    }

                    if (_bpp != 2)
                    {
                        val = _image[index];
                        return new Color32(val, val, val, 255);
                    }

                    val = _image[index];
                    return new Color32(val, val, val, _image[index + 1]);
                }

                return _data.GetPixel(x, y);
            }

            // GetPixelFloat - Double
            public override Single GetPixelFloat(Double x, Double y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting pixelFloatD with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return 0f;
                }

                return base.GetPixelFloat(x, y);
            }

            // GetPixelFloat - Float
            public override Single GetPixelFloat(Single x, Single y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting pixelFloatF with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return 0f;
                }

                return base.GetPixelFloat(x, y);
            }

            // GetPixelFloat - Integer
            public override Single GetPixelFloat(Int32 x, Int32 y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting pixelFloatI with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return 0f;
                }

                if (OnDemandStorage.useManualMemoryManagement)
                {
                    retVal = 0f;
                    index = PixelIndex(x, y);
                    for (Int32 i = 0; i < _bpp; i++)
                    {
                        retVal += _image[index + i];
                    }

                    retVal /= _bpp;
                    retVal *= Byte2Float;
                    return retVal;
                }

                Color pixel = _data.GetPixel(x, y);
                Single value = 0f;
                if (Depth == MapDepth.Greyscale)
                    value = pixel.r;
                else if (Depth == MapDepth.HeightAlpha)
                    value = pixel.r + pixel.a;
                else if (Depth == MapDepth.RGB)
                    value = pixel.r + pixel.g + pixel.b;
                else if (Depth == MapDepth.RGBA)
                    value = pixel.r + pixel.g + pixel.b + pixel.a;

                // Enhanced support for L8 .dds
                if (_data.format == TextureFormat.Alpha8)
                    value = pixel.a;

                return value / (Int32) Depth;
            }

            // GetPixelHeightAlpha - Double
            public override HeightAlpha GetPixelHeightAlpha(Double x, Double y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting pixelHeightAlphaD with unloaded CBmap " + name + " of path " +
                                  Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new HeightAlpha(0f, 0f);
                }

                return base.GetPixelHeightAlpha(x, y);
            }

            // GetPixelHeightAlpha - Float
            public override HeightAlpha GetPixelHeightAlpha(Single x, Single y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting pixelHeightAlphaF with unloaded CBmap " + name + " of path " +
                                  Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new HeightAlpha(0f, 0f);
                }

                return base.GetPixelHeightAlpha(x, y);
            }

            // GetPixelHeightAlpha - Int
            public override HeightAlpha GetPixelHeightAlpha(Int32 x, Int32 y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting pixelHeightAlphaI with unloaded CBmap " + name + " of path " +
                                  Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new HeightAlpha(0f, 0f);
                }

                if (OnDemandStorage.useManualMemoryManagement)
                {
                    index = PixelIndex(x, y);
                    if (_bpp == 2)
                    {
                        return new HeightAlpha(Byte2Float * _image[index], Byte2Float * _image[index + 1]);
                    }

                    if (_bpp != 4)
                    {
                        return new HeightAlpha(Byte2Float * _image[index], 1f);
                    }

                    val = _image[index];
                    return new HeightAlpha(Byte2Float * _image[index], Byte2Float * _image[index + 3]);
                }

                Color pixel = _data.GetPixel(x, y);
                if (Depth == MapDepth.HeightAlpha || Depth == MapDepth.RGBA)
                    return new HeightAlpha(pixel.r, pixel.a);
                return new HeightAlpha(pixel.r, 1f);
            }

            // GreyByte
            public override Byte GreyByte(Int32 x, Int32 y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting GreyByteI with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return 0;
                }

                if (OnDemandStorage.useManualMemoryManagement)
                {
                    return _image[PixelIndex(x, y)];
                }

                return (Byte) (Float2Byte * _data.GetPixel(x, y).r);
            }

            // GreyFloat
            public override Single GreyFloat(Int32 x, Int32 y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting GreyFloat with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return 0f;
                }

                if (OnDemandStorage.useManualMemoryManagement)
                {
                    return Byte2Float * _image[PixelIndex(x, y)];
                }

                return _data.GetPixel(x, y).grayscale;
            }

            // PixelByte
            public override Byte[] PixelByte(Int32 x, Int32 y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: getting pixelByte with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new Byte[_bpp];
                }

                if (OnDemandStorage.useManualMemoryManagement)
                {
                    Byte[] numArray = new Byte[this._bpp];
                    index = PixelIndex(x, y);
                    for (Int32 i = 0; i < _bpp; i++)
                    {
                        numArray[i] = _image[index + i];
                    }

                    return numArray;
                }

                Color c = _data.GetPixel(x, y);
                if (Depth == MapDepth.Greyscale)
                    return new Byte[] {(Byte) c.r};
                else if (Depth == MapDepth.HeightAlpha)
                    return new Byte[] {(Byte) c.r, (Byte) c.a};
                else if (Depth == MapDepth.RGB)
                    return new Byte[] {(Byte) c.r, (Byte) c.g, (Byte) c.b};
                else
                    return new Byte[] {(Byte) c.r, (Byte) c.g, (Byte) c.b, (Byte) c.a};
            }

            // CompileToTexture
            public override Texture2D CompileToTexture(Byte filter)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: compiling with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new Texture2D(_width, _height);
                }

                if (OnDemandStorage.useManualMemoryManagement)
                {
                    Color32[] color32 = new Color32[Size];
                    for (Int32 i = 0; i < Size; i++)
                    {
                        val = (Byte) ((_image[i] & filter) == 0 ? 0 : 255);
                        color32[i] = new Color32(val, val, val, 255);
                    }

                    Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                    compiled.SetPixels32(color32);
                    compiled.Apply(false, true);
                    return compiled;
                }
                else
                {
                    Texture2D compiled = Instantiate(_data);
                    compiled.Apply(false, true);
                    return compiled;
                }
            }

            // Generate a greyscale texture from the stored data
            public override Texture2D CompileGreyscale()
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: compiling with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new Texture2D(_width, _height);
                }

                if (OnDemandStorage.useManualMemoryManagement)
                {
                    Color32[] color32 = new Color32[Size];
                    for (Int32 i = 0; i < Size; i++)
                    {
                        val = _image[i];
                        color32[i] = new Color32(val, val, val, 255);
                    }

                    Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                    compiled.SetPixels32(color32);
                    compiled.Apply(false, true);
                    return compiled;
                }
                else
                {
                    Texture2D compiled = Instantiate(_data);
                    compiled.Apply(false, true);
                    return compiled;
                }
            }

            // Generate a height/alpha texture from the stored data
            public override Texture2D CompileHeightAlpha()
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: compiling with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new Texture2D(_width, _height);
                }

                if (OnDemandStorage.useManualMemoryManagement)
                {
                    Color32[] color32 = new Color32[Width * Height];
                    for (Int32 i = 0; i < Width * Height; i++)
                    {
                        val = _image[i * 2];
                        color32[i] = new Color32(val, val, val, _image[i * 2 + 1]);
                    }

                    Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                    compiled.SetPixels32(color32);
                    compiled.Apply(false, true);
                    return compiled;
                }
                else
                {
                    Texture2D compiled = Instantiate(_data);
                    compiled.Apply(false, true);
                    return compiled;
                }
            }

            // Generate an RGB texture from the stored data
            public override Texture2D CompileRGB()
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: compiling with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new Texture2D(_width, _height);
                }

                if (OnDemandStorage.useManualMemoryManagement)
                {
                    Color32[] color32 = new Color32[Width * Height];
                    for (Int32 i = 0; i < Width * Height; i++)
                    {
                        color32[i] = new Color32(_image[i * 3], _image[i * 3 + 1], _image[i * 3 + 2], 255);
                    }

                    Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                    compiled.SetPixels32(color32);
                    compiled.Apply(false, true);
                    return compiled;
                }
                else
                {
                    Texture2D compiled = Instantiate(_data);
                    compiled.Apply(false, true);
                    return compiled;
                }
            }

            // Generate an RGBA texture from the stored data
            public override Texture2D CompileRGBA()
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing)
                        Debug.Log("[OD] ERROR: compiling with unloaded CBmap " + name + " of path " + Path +
                                  ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new Texture2D(_width, _height);
                }

                if (OnDemandStorage.useManualMemoryManagement)
                {
                    Color32[] color32 = new Color32[Width * Height];
                    for (Int32 i = 0; i < Width * Height; i++)
                    {
                        color32[i] = new Color32(_image[i * 3], _image[i * 3 + 1], _image[i * 3 + 2],
                            _image[i * 3 + 3]);
                    }

                    Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                    compiled.SetPixels32(color32);
                    compiled.Apply(false, true);
                    return compiled;
                }
                else
                {
                    Texture2D compiled = Instantiate(_data);
                    compiled.Apply(false, true);
                    return compiled;
                }
            }
        }
    }
}
