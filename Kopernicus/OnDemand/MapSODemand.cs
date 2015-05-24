using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;

namespace Kopernicus
{
    namespace OnDemand
    {
        public class MapSODemand : MapSO, ILoadOnDemand
        {
            protected string mapPath = "";
            protected bool isLoaded = false;

            public bool IsLoaded() { return isLoaded; }
            public void SetPath(string path) { mapPath = path; }
            public bool Load()
            {
                if (isLoaded)
                    return false;
                Texture2D map = Utility.LoadTexture(mapPath, false, false, false);
                if (map != null)
                {
                    CreateMap(Depth, map);
                    isLoaded = true;
                    DestroyImmediate(map);
                }
                return true;
            }
            public bool Unload()
            {
                if (!isLoaded || mapPath == "")
                    return false;
                _data = null;
                isLoaded = false;
                System.GC.Collect();
                return true;
            }

            public override void CreateMap(MapSO.MapDepth depth, Texture2D tex)
            {
                base.CreateMap(depth, tex);
                isLoaded = true;
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
                    Debug.Log("OD: ERROR: getting pixelbyte with unloaded map");
                    Load();
                }
                return base.GetPixelByte(x, y);
            }
            public override Color GetPixelColor(double x, double y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting pixelColD with unloaded map");
                    Load();
                }
                return base.GetPixelColor(x, y);
            }
            public override Color GetPixelColor(float x, float y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting pixelColF with unloaded map");
                    Load();
                }
                return base.GetPixelColor(x, y);
            }
            public override Color GetPixelColor(int x, int y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting pixelColI with unloaded map");
                    Load();
                }
                return base.GetPixelColor(x, y);
            }
            public override Color GetPixelColor32(double x, double y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting pixelCol32D with unloaded map");
                    Load();
                }
                return base.GetPixelColor32(x, y);
            }
            public override Color GetPixelColor32(float x, float y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting pixelCol32F with unloaded map");
                    Load();
                }
                return base.GetPixelColor32(x, y);
            }
            public override Color32 GetPixelColor32(int x, int y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting pixelCol32I with unloaded map");
                    Load();
                }
                return base.GetPixelColor32(x, y);
            }
            public override float GetPixelFloat(double x, double y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting pixelFloatD with unloaded map");
                    Load();
                }
                return base.GetPixelFloat(x, y);
            }
            public override float GetPixelFloat(float x, float y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting pixelFloatF with unloaded map");
                    Load();
                }
                return base.GetPixelFloat(x, y);
            }
            public override float GetPixelFloat(int x, int y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting pixelFloatI with unloaded map");
                    Load();
                }
                return base.GetPixelFloat(x, y);
            }
            public override MapSO.HeightAlpha GetPixelHeightAlpha(double x, double y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting pixelHeightAlphaD with unloaded map");
                    Load();
                }
                return base.GetPixelHeightAlpha(x, y);
            }
            public override MapSO.HeightAlpha GetPixelHeightAlpha(float x, float y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting pixelHeightAlphaF with unloaded map");
                    Load();
                }
                return base.GetPixelHeightAlpha(x, y);
            }
            public override MapSO.HeightAlpha GetPixelHeightAlpha(int x, int y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting pixelHeightAlphaI with unloaded map");
                    Load();
                }
                return base.GetPixelHeightAlpha(x, y);
            }
            public override byte GreyByte(int x, int y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting GreyByteI with unloaded map");
                    Load();
                }
                return base.GreyByte(x, y);
            }
            public override float GreyFloat(int x, int y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting GreyFloat with unloaded map");
                    Load();
                }
                return base.GreyFloat(x, y);
            }
            public override byte[] PixelByte(int x, int y)
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: getting pixelByte with unloaded map");
                    Load();
                }
                return base.PixelByte(x, y);
            }
            public override Texture2D CompileToTexture()
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: compiling with unloaded map");
                    Load();
                }
                return base.CompileToTexture();
            }
        }
    }
}
