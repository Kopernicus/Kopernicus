using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kopernicus
{
    namespace OnDemand
    {
        public interface ILoadOnDemand
        {
            bool Load();
            bool Unload();
            bool IsLoaded();
            void SetPath(string path);
            void SetAutoLoad(bool doAutoLoad);
        }
    }
}
