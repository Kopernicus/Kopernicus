using System;
namespace Kopernicus.ShadowMan
{
    public static class ReflectionUtils
    {
        internal static Type getType(string name)
        {
            Type type = null;
            AssemblyLoader.loadedAssemblies.TypeOperation(t => { if (t.FullName == name) type = t; });

            if (type != null)
            {
                return type;
            }
            return null;
        }
    }
}

