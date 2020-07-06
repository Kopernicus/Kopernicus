/**
 * Kopernicus Planetary System Modifier
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
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace Kopernicus.Constants
{
    // Information about the current version of Kopernicus
    public static class Version
    {
        // Version information
        private static String VersionNumber
        {
            get
            {
                return CompatibilityChecker.VERSION_MAJOR + "." + CompatibilityChecker.VERSION_MINOR + "." +
                       CompatibilityChecker.REVISION + "-" + CompatibilityChecker.KOPERNICUS;
            }
        }

        // Get a String for the logging
        // Get a String for the logging
        public static String VersionId
        {
            get
            {
#if DEBUG
                const String DEVELOPMENT_BUILD = " [Development Build]";
#else
                const String DEVELOPMENT_BUILD = "";
#endif
                return "RTB Kopernicus " + VersionNumber + DEVELOPMENT_BUILD + " - (BuildDate: " +
                       BuiltTime(Assembly.GetCallingAssembly()).ToString("dd.MM.yyyy HH:mm:ss") + "; AssemblyHash: " +
                       AssemblyHandle() + ")";
            }
        }

        // Returns the SHA1 Hash of the assembly
        private static String AssemblyHandle()
        {
            String filePath = Assembly.GetCallingAssembly().Location;
            return Convert.ToBase64String(SHA1.Create().ComputeHash(File.ReadAllBytes(filePath)));
        }

        // Returns the time when the assembly was built
        public static DateTime BuiltTime(Assembly assembly)
        {
            String filePath = assembly.Location;
            const Int32 PE_HEADER_OFFSET = 60;
            const Int32 LINKER_TIMESTAMP_OFFSET = 8;
            Byte[] b = new Byte[2048];
            Stream s = null;

            try
            {
                s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                s?.Close();
            }

            Int32 i = BitConverter.ToInt32(b, PE_HEADER_OFFSET);
            Int32 secondsSince1970 = BitConverter.ToInt32(b, i + LINKER_TIMESTAMP_OFFSET);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.ToUniversalTime();
            return dt;
        }
    }
}
