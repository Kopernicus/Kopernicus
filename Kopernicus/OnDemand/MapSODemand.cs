/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
* Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;
using System.IO;

namespace Kopernicus
{
    namespace OnDemand
    {
        public class MapSODemand : MapSO, ILoadOnDemand
        {
            protected string mapPath = "";
            protected bool isLoaded = false;
            protected bool autoLoad = false;

            public bool IsLoaded() { return isLoaded; }
            public void SetPath(string path) { mapPath = path; }
            public void SetAutoLoad(bool doAutoLoad) { autoLoad = doAutoLoad; }
            public string MapName() { return name; }
            public string MapPath() { return mapPath; }
            public bool Load()
            {
                if (isLoaded)
                    return false;
                if (mapPath.EndsWith(".tga"))
                {
                    CreateMap(Depth, mapPath);
                    isLoaded = true;
                    Debug.Log("OD: map " + name + " enabling self, time was " + OnDemand.OnDemandStorage.mapTimes[this] + ". Path = " + mapPath);
                    OnDemand.OnDemandStorage.enabledMaps[this] = true;
                    OnDemand.OnDemandStorage.mapTimes[this] = 0f;
                    return true;
                }
                else
                {
                    Texture2D map = Utility.LoadTexture(mapPath, false, false, false);
                    if (map != null)
                    {
                        CreateMap(Depth, map);
                        isLoaded = true;
                        Debug.Log("OD: map " + name + " enabling self, time was " + OnDemand.OnDemandStorage.mapTimes[this] + ". Path = " + mapPath);
                        OnDemand.OnDemandStorage.enabledMaps[this] = true;
                        OnDemand.OnDemandStorage.mapTimes[this] = 0f;
                        DestroyImmediate(map);
                        return true;
                    }

                    Debug.Log("OD: ERROR: Failed to load map " + name + " at path " + mapPath);
                    return false;
                }
            }
            public bool Unload()
            {
                if (!isLoaded || mapPath == "")
                    return false;
                _data = null;
                isLoaded = false;
                Debug.Log("OD: map " + name + " disabling self, time was " + OnDemand.OnDemandStorage.mapTimes[this] + ". Path = " + mapPath);
                OnDemand.OnDemandStorage.enabledMaps[this] = false;
                OnDemand.OnDemandStorage.mapTimes[this] = 0f;
                this._data = null;
                if(OnDemandStorage.onDemandForceCollect)
                    GC.Collect();
                return true;
            }

            new public MapSO.MapDepth Depth
            {
                get
                {
                    return (MapSO.MapDepth)_bpp;
                }
                set
                {
                    _bpp = (int)value;
                }
            }

            public override void CreateMap(MapSO.MapDepth depth, Texture2D tex)
            {
                base.CreateMap(depth, tex);
                isLoaded = true;
            }

            public void CreateMap(MapDepth depth, string path)
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("GameData", path));
                byte[] bytes = File.ReadAllBytes(fullPath);
                TGAHeader tex = new TGAHeader(bytes);

                this._name = Path.GetFileNameWithoutExtension(fullPath); ;
                this._width = tex.width;
                this._height = tex.height;
                this._bpp = (int)depth;
                this._rowWidth = this._width * this._bpp;

                switch (depth)
                {
                    case MapDepth.Greyscale:
                        GreyscaleFromRGB(tex, bytes);
                        break;

                    case MapDepth.HeightAlpha:
                        HeightAlpha(tex, bytes);
                        break;

                    case MapDepth.RGB:
                        RGB(tex, bytes);
                        break;

                    case MapDepth.RGBA:
                        RGBA(tex, bytes);
                        break;
                }

                this._isCompiled = true;
            }

            public void CreateMap(MapSO.MapDepth depth, string path, Texture2D tex)
            {
                mapPath = path;
                CreateMap(depth, tex);
            }

            public MapSODemand()
                : base()
            {
                // register here or on PQSMod creation?
                // for now we'll do it on creation (i.e. elsewhere)
            }
            public override byte GetPixelByte(int x, int y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting pixelbyte with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return (byte)0;
                }
                return base.GetPixelByte(x, y);
            }
            public override Color GetPixelColor(double x, double y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting pixelColD with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return Color.black;
                }
                return base.GetPixelColor(x, y);
            }
            public override Color GetPixelColor(float x, float y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting pixelColF with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return Color.black;
                }
                return base.GetPixelColor(x, y);
            }
            public override Color GetPixelColor(int x, int y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting pixelColI with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return Color.black;
                }
                return base.GetPixelColor(x, y);
            }
            public override Color GetPixelColor32(double x, double y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting pixelCol32D with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else return Color.black;
                }
                return base.GetPixelColor32(x, y);
            }
            public override Color GetPixelColor32(float x, float y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting pixelCol32F with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return Color.black;
                }
                return base.GetPixelColor32(x, y);
            }
            public override Color32 GetPixelColor32(int x, int y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting pixelCol32I with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return new Color32();
                }
                return base.GetPixelColor32(x, y);
            }
            public override float GetPixelFloat(double x, double y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting pixelFloatD with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return 0f;
                }
                return base.GetPixelFloat(x, y);
            }
            public override float GetPixelFloat(float x, float y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting pixelFloatF with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return 0f;
                }
                return base.GetPixelFloat(x, y);
            }
            public override float GetPixelFloat(int x, int y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting pixelFloatI with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return 0f;
                }
                return base.GetPixelFloat(x, y);
            }
            public override MapSO.HeightAlpha GetPixelHeightAlpha(double x, double y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting pixelHeightAlphaD with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return new MapSO.HeightAlpha(0f, 0f);
                }
                return base.GetPixelHeightAlpha(x, y);
            }
            public override MapSO.HeightAlpha GetPixelHeightAlpha(float x, float y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting pixelHeightAlphaF with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return new MapSO.HeightAlpha(0f, 0f);
                }
                return base.GetPixelHeightAlpha(x, y);
            }
            public override MapSO.HeightAlpha GetPixelHeightAlpha(int x, int y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting pixelHeightAlphaI with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return new MapSO.HeightAlpha(0f, 0f);
                }
                return base.GetPixelHeightAlpha(x, y);
            }
            public override byte GreyByte(int x, int y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting GreyByteI with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return (byte)0;
                }
                return base.GreyByte(x, y);
            }
            public override float GreyFloat(int x, int y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting GreyFloat with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return 0f;
                }
                return base.GreyFloat(x, y);
            }
            public override byte[] PixelByte(int x, int y)
            {
                if (!isLoaded)
                {
                    if (OnDemandStorage.onDemandLogOnMissing) Debug.Log("OD: ERROR: getting pixelByte with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return new byte[_bpp];
                }
                return base.PixelByte(x, y);
            }
            public override Texture2D CompileToTexture()
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: compiling with unloaded map " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return new Texture2D(_width, _height);
                }
                return base.CompileToTexture();
            }

            protected void GreyscaleFromRGB(TGAHeader tex, byte[] data)
            {
                Color32[] colorArray = TGAUtils.ReadImage(tex, data);
                int length = colorArray.Length;
                this._data = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    this._data[i] = colorArray[i].r;
                }
            }

            protected void HeightAlpha(TGAHeader tex, byte[] data)
            {
                Color32[] colorArray = TGAUtils.ReadImage(tex, data);
                int num = colorArray.Length * 2;
                this._data = new byte[num];
                int index = 0;
                int num3 = 0;
                while (index < colorArray.Length)
                {
                    this._data[num3++] = colorArray[index].r;
                    this._data[num3++] = colorArray[index].a;
                    index++;
                }
            }

            protected void RGB(TGAHeader tex, byte[] data)
            {
                Color32[] colorArray = TGAUtils.ReadImage(tex, data);
                int num = colorArray.Length * 3;
                this._data = new byte[num];
                int index = 0;
                int num3 = 0;
                while (index < colorArray.Length)
                {
                    this._data[num3++] = colorArray[index].r;
                    this._data[num3++] = colorArray[index].g;
                    this._data[num3++] = colorArray[index].b;
                    index++;
                }
            }

            protected void RGBA(TGAHeader tex, byte[] data)
            {
                Color32[] colorArray = TGAUtils.ReadImage(tex, data);
                int num = colorArray.Length * 4;
                this._data = new byte[num];
                int index = 0;
                int num3 = 0;
                while (index < colorArray.Length)
                {
                    this._data[num3++] = colorArray[index].r;
                    this._data[num3++] = colorArray[index].g;
                    this._data[num3++] = colorArray[index].b;
                    this._data[num3++] = colorArray[index].a;
                    index++;
                }
            }
        }
    }
}
