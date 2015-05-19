using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kopernicus
{
    namespace OnDemand
    {
        public class Handler
        {
            public static Handler Instance
            {
                get
                {
                    if (_instance == null)
                        _instance = new Handler();
                    return _instance;
                }
            }
            private static Handler _instance;

            public static Dictionary<MapSODemand, bool> enabledMaps;
            public static Dictionary<string, List<MapSODemand>> perBody;

            public Handler()
            {
                enabledMaps = new Dictionary<MapSODemand, bool>();
                perBody = new Dictionary<string, List<MapSODemand>>();
            }
        }
    }
}
