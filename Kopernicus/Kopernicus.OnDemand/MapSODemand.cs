/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
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
        // MapSO Replacement to support Texture streaming
        public class MapSODemand : MapSO, ILoadOnDemand
        {
            // BitPerPixels is always 4
            protected new const int _bpp = 4;

            // Representation of the map
            protected new Texture2D _data { get; set; }

            // States
            public bool IsLoaded { get; set; }
            public bool AutoLoad { get; set; }

            // Path of the Texture
            public string Path { get; set; }

            // MapDepth
            public new MapDepth Depth { get; set; }

            // Load the Map
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
                    Debug.Log("[OD] ---> Map " + name + " enabling self. Path = " + Path);
                    return;
                }

                // Return nothing
                Debug.Log("[OD] ERROR: Failed to load map " + name + " at path " + Path);
            }

            // Unload the map
            public void Unload()
            {
                // We can only destroy the map, if it is loaded
                if (!IsLoaded)
                    return;

                // Nuke the map
                DestroyImmediate(_data);

                // Set flags
                IsLoaded = false;

                // Log
                Debug.Log("[OD] <--- Map " + name + " disabling self. Path = " + Path);
            }

            // Create a map from a Texture2D
            public override void CreateMap(MapDepth depth, Texture2D tex)
            {
                // If the Texture is null, abort
                if (tex == null)
                {
                    Debug.Log("[OD] ERROR: Failed to load map");
                    return;
                }

                // Set _data
                _data = tex;

                // Variables
                _width = tex.width;
                _height = tex.height;
                Depth = depth;
                _rowWidth = _width * _bpp;

                // We're compiled
                _isCompiled = true;
            }

            public MapSODemand()
                : base()
            {
                // register here or on PQSMod creation?
                // for now we'll do it on creation (i.e. elsewhere)
            }

            // GetPixelByte
            public override byte GetPixelByte(int x, int y)
            {
                // If we aren't loaded....
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelbyte with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return 0;
                }
                return (byte)(_data.GetPixel(x, y).r * Float2Byte);
            }

            // GetPixelColor - Double
            public override Color GetPixelColor(double x, double y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelColD with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return Color.black;
                }

                BilinearCoords coords = ConstructBilinearCoords(x, y);
                return Color.Lerp(
                    Color.Lerp(
                        this.GetPixelColor(coords.xFloor, coords.yFloor), 
                        this.GetPixelColor(coords.xCeiling, coords.yFloor), 
                        coords.u), 
                    Color.Lerp(
                        this.GetPixelColor(coords.xFloor, coords.yCeiling), 
                        this.GetPixelColor(coords.xCeiling, coords.yCeiling),
                        coords.u),
                    coords.v);
            }

            // GetPixelColor - Float
            public override Color GetPixelColor(float x, float y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelColF with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return Color.black;
                }

                BilinearCoords coords = ConstructBilinearCoords(x, y);
                return Color.Lerp(
                    Color.Lerp(
                        this.GetPixelColor(coords.xFloor, coords.yFloor),
                        this.GetPixelColor(coords.xCeiling, coords.yFloor),
                        coords.u),
                    Color.Lerp(
                        this.GetPixelColor(coords.xFloor, coords.yCeiling),
                        this.GetPixelColor(coords.xCeiling, coords.yCeiling),
                        coords.u),
                    coords.v);
            }

            // GetPixelColor - Int
            public override Color GetPixelColor(int x, int y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelColI with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return Color.black;
                }
                return _data.GetPixel(x, y);
            }

            // GetPixelColor32 - Double
            public override Color GetPixelColor32(double x, double y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelCol32D with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return Color.black;
                }

                BilinearCoords coords = ConstructBilinearCoords(x, y);
                return Color32.Lerp(
                    Color32.Lerp(
                        this.GetPixelColor32(coords.xFloor, coords.yFloor),
                        this.GetPixelColor32(coords.xCeiling, coords.yFloor),
                        coords.u),
                    Color32.Lerp(
                        this.GetPixelColor32(coords.xFloor, coords.yCeiling),
                        this.GetPixelColor32(coords.xCeiling, coords.yCeiling),
                        coords.u),
                    coords.v);
            }

            // GetPixelColor32 - Float - Honestly Squad, why are they named GetPixelColor32, but return normal Colors instead of Color32?
            public override Color GetPixelColor32(float x, float y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelCol32F with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return Color.black;
                }

                BilinearCoords coords = ConstructBilinearCoords(x, y);
                return Color32.Lerp(
                    Color32.Lerp(
                        this.GetPixelColor32(coords.xFloor, coords.yFloor),
                        this.GetPixelColor32(coords.xCeiling, coords.yFloor),
                        coords.u),
                    Color32.Lerp(
                        this.GetPixelColor32(coords.xFloor, coords.yCeiling),
                        this.GetPixelColor32(coords.xCeiling, coords.yCeiling),
                        coords.u),
                    coords.v);
            }

            // GetPixelColor32 - Int
            public override Color32 GetPixelColor32(int x, int y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelCol32I with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new Color32();
                }
                return _data.GetPixel(x, y);
            }

            // GetPixelFloat - Double
            public override float GetPixelFloat(double x, double y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelFloatD with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return 0f;
                }

                BilinearCoords coords = ConstructBilinearCoords(x, y);
                return Mathf.Lerp(
                    Mathf.Lerp(
                        GetPixelFloat(coords.xFloor, coords.yFloor), 
                        GetPixelFloat(coords.xCeiling, coords.yFloor), 
                        coords.u), 
                    Mathf.Lerp(
                        GetPixelFloat(coords.xFloor, coords.yCeiling), 
                        GetPixelFloat(coords.xCeiling, coords.yCeiling),
                        coords.u),
                    coords.v);
            }

            // GetPixelFloat - Float
            public override float GetPixelFloat(float x, float y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelFloatF with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return 0f;
                }

                BilinearCoords coords = ConstructBilinearCoords(x, y);
                return Mathf.Lerp(
                    Mathf.Lerp(
                        GetPixelFloat(coords.xFloor, coords.yFloor),
                        GetPixelFloat(coords.xCeiling, coords.yFloor),
                        coords.u),
                    Mathf.Lerp(
                        GetPixelFloat(coords.xFloor, coords.yCeiling),
                        GetPixelFloat(coords.xCeiling, coords.yCeiling),
                        coords.u),
                    coords.v);
            }

            // GetPixelFloat - Integer
            public override float GetPixelFloat(int x, int y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelFloatI with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return 0f;
                }

                Color pixel = _data.GetPixel(x, y);
                float value = 0f;
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

                return value / (int)Depth;
            }

            // GetPixelHeightAlpha - Double
            public override HeightAlpha GetPixelHeightAlpha(double x, double y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelHeightAlphaD with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new HeightAlpha(0f, 0f);
                }

                BilinearCoords coords = ConstructBilinearCoords(x, y);
                return HeightAlpha.Lerp(
                    HeightAlpha.Lerp(
                        GetPixelHeightAlpha(coords.xFloor, coords.yFloor), 
                        GetPixelHeightAlpha(coords.xCeiling, coords.yFloor), 
                        coords.u), 
                    HeightAlpha.Lerp(
                        GetPixelHeightAlpha(coords.xFloor, coords.yCeiling), 
                        GetPixelHeightAlpha(coords.xFloor, coords.yCeiling),
                        coords.u),
                    coords.v);
            }

            // GetPixelHeightAlpha - Float
            public override HeightAlpha GetPixelHeightAlpha(float x, float y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelHeightAlphaF with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new HeightAlpha(0f, 0f);
                }

                BilinearCoords coords = ConstructBilinearCoords(x, y);
                return HeightAlpha.Lerp(
                    HeightAlpha.Lerp(
                        GetPixelHeightAlpha(coords.xFloor, coords.yFloor),
                        GetPixelHeightAlpha(coords.xCeiling, coords.yFloor),
                        coords.u),
                    HeightAlpha.Lerp(
                        GetPixelHeightAlpha(coords.xFloor, coords.yCeiling),
                        GetPixelHeightAlpha(coords.xFloor, coords.yCeiling),
                        coords.u),
                    coords.v);
            }

            // GetPixelHeightAlpha - Int
            public override HeightAlpha GetPixelHeightAlpha(int x, int y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelHeightAlphaI with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new HeightAlpha(0f, 0f);
                }

                Color pixel = _data.GetPixel(x, y);
                if (Depth == (MapDepth.HeightAlpha | MapDepth.RGBA))
                    return new HeightAlpha(pixel.r, pixel.a);
                else
                    return new HeightAlpha(pixel.r, 1f);
            }

            // GreyByte
            public override byte GreyByte(int x, int y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting GreyByteI with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return 0;
                }
                return (byte)(Float2Byte * _data.GetPixel(x, y).r);
            }

            // GreyFloat
            public override float GreyFloat(int x, int y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting GreyFloat with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return 0f;
                }
                return _data.GetPixel(x, y).grayscale;
            }

            // PixelByte
            public override byte[] PixelByte(int x, int y)
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: getting pixelByte with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new byte[_bpp];
                }

                Color c = _data.GetPixel(x, y);
                if (Depth == MapDepth.Greyscale)
                    return new byte[] { (byte)c.r };
                else if (Depth == MapDepth.HeightAlpha)
                    return new byte[] { (byte)c.r, (byte)c.a };
                else if (Depth == MapDepth.RGB)
                    return new byte[] { (byte)c.r, (byte)c.g, (byte)c.b };
                else
                    return new byte[] { (byte)c.r, (byte)c.g, (byte)c.b, (byte)c.a };
            }

            // CompileToTexture
            public override Texture2D CompileToTexture()
            {
                if (!IsLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("[OD] ERROR: compiling with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                    if (AutoLoad) Load();
                    else return new Texture2D(_width, _height);
                }
                Texture2D compiled = Instantiate(_data) as Texture2D;
                compiled.Apply(false, true);
                return compiled;
            }

            // ConstructBilinearCoords from double
            protected new BilinearCoords ConstructBilinearCoords(double x, double y)
            {
                // Create the struct
                BilinearCoords coords = new BilinearCoords();

                // Floor
                x = Math.Abs(x - Math.Floor(x));
                y = Math.Abs(y - Math.Floor(y));

                // X to U
                coords.x = x * _width;
                coords.xFloor = (int)Math.Floor(coords.x);
                coords.xCeiling = (int)Math.Ceiling(coords.x);
                coords.u = (float)(coords.x - coords.xFloor);
                if (coords.xCeiling == _width) coords.xCeiling = 0;

                // Y to V
                coords.y = y * _height;
                coords.yFloor = (int)Math.Floor(coords.y);
                coords.yCeiling = (int)Math.Ceiling(coords.y);
                coords.v = (float)(coords.y - coords.yFloor);
                if (coords.yCeiling == this._height) coords.yCeiling = 0;

                // We're done
                return coords;
            }

            // ConstructBilinearCoords from float
            protected new BilinearCoords ConstructBilinearCoords(float x, float y)
            {
                return ConstructBilinearCoords((double)x, (double)y);
            }

            // BilinearCoords
            public struct BilinearCoords
            {
                public double x, y;
                public int xCeiling, xFloor, yCeiling, yFloor;
                public float u, v;
            }
        }
    }
}
