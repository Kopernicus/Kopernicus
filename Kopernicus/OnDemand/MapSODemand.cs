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
        public class MapSODemand : MapSO
        {
            protected string mapPath;
            protected bool isLoaded = false;

            public override void CreateMap(MapSO.MapDepth depth, Texture2D tex)
            {
                base.CreateMap(depth, tex);
                isLoaded = true;
            }
            public virtual void Unload()
            {
                if (!isLoaded)
                    return;

            }
        }
    }
}
