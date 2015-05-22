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
        public class CBAttributeMapSODemand : CBAttributeMapSO, ILoadOnDemand
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
                    Debug.Log("OD: ERROR: getting attribute with unloaded CBmap");
                    Load();
                }
                return base.GetAtt(lat, lon);
            }
            public override Texture2D CompileToTexture()
            {
                if (!isLoaded)
                {
                    Debug.Log("OD: ERROR: compiling with unloaded CBmap");
                    Load();
                }
                return base.CompileToTexture();
            }
        }
    }
}
