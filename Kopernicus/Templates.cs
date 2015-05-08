using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

namespace Kopernicus
{
    public class Templates
    {
        static public Templates instance = null;
        
        // for loading only one each
        public Dictionary<string, MapSO> mapsGray;
        public Dictionary<string, MapSO> mapsRGB;
        
        public Templates()
        {
            instance = this;

            mapsGray = new Dictionary<string, MapSO>();
            mapsRGB = new Dictionary<string, MapSO>();
        }
    }
}
