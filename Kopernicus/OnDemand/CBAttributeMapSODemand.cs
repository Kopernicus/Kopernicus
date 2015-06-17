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
        public class CBAttributeMapSODemand : CBAttributeMapSO, ILoadOnDemand
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
                    Debug.Log("OD: CBmap " + name + " enabling self, time was " + OnDemand.OnDemandStorage.mapTimes[this] + ". Path = " + mapPath);
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
                        DestroyImmediate(map);
                        Debug.Log("OD: CBmap " + name + " enabling self, time was " + OnDemand.OnDemandStorage.mapTimes[this] + ". Path = " + mapPath);
                        OnDemand.OnDemandStorage.enabledMaps[this] = true;
                        OnDemand.OnDemandStorage.mapTimes[this] = 0f;
                        return true;
                    }
                    Debug.Log("OD: ERROR: Failed to load CBmap " + name + " at path " + mapPath);
                    return false;
                }
            }
            public bool Unload()
            {
                if (!isLoaded || mapPath == "")
                    return false;
                _data = null;
                isLoaded = false;
                Debug.Log("OD: CBmap " + name + " disabling self, time was " + OnDemand.OnDemandStorage.mapTimes[this] + ". Path = " + mapPath);
                OnDemand.OnDemandStorage.enabledMaps[this] = false;
                OnDemand.OnDemandStorage.mapTimes[this] = 0f;
                this._data = null;
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

            public CBAttributeMapSODemand()
                : base()
            {
                // register here or on creation by parser?
                // for now we'll do it on creation (i.e. elsewhere)
            }

            public override CBAttributeMapSO.MapAttribute GetAtt(double lat, double lon)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting attribute with unloaded CBmap " + name + " of path " + mapPath + ", autoload = " + autoLoad);
                    if (autoLoad)
                        Load();
                    else
                        return Attributes[0];
                }
                return base.GetAtt(lat, lon);
            }
            public override Texture2D CompileToTexture()
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: compiling with unloaded CBmap " + name + " of path " + mapPath + ", autoload = " + autoLoad);
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
