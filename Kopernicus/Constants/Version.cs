/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;

namespace Kopernicus
{
    namespace Constants
    {    
        // Informations about the current version of Kopernicus
        public class Version
        {
            // Versioning information
            public const string versionNumber = "0.5.2"; 

            // Get a string for the logging
            public static string version
            {
                get
                {
                    #if DEBUG
                    bool developmentBuild = true;
                    #else
                    bool developmentBuild = false;
                    #endif
                    return "Kopernicus " + versionNumber + (developmentBuild ? " [Development Build]" : "") + " - (BuildDate: " + BuiltTime().ToString("dd.MM.yyyy HH:mm:ss") + "; AssemblyHash: " + AssemblyHandle() + ")";
                }
            }

            // Returns the SHA1 Hash of the assembly
            public static string AssemblyHandle()
            {
                string filePath = Assembly.GetCallingAssembly().Location;
                return Convert.ToBase64String(SHA1.Create().ComputeHash(File.ReadAllBytes(filePath)));
            }

            // Returns the time when the assembly was built
            public static DateTime BuiltTime()
            {
                string filePath = Assembly.GetCallingAssembly().Location;
                const int c_PeHeaderOffset = 60;
                const int c_LinkerTimestampOffset = 8;
                byte[] b = new byte[2048];
                Stream s = null;

                try
                {
                    s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    s.Read(b, 0, 2048);
                }
                finally
                {
                    if (s != null)
                    {
                        s.Close();
                    }
                }

                int i = BitConverter.ToInt32(b, c_PeHeaderOffset);
                int secondsSince1970 = BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                dt = dt.AddSeconds(secondsSince1970);
                dt = dt.ToLocalTime();
                return dt;
            }
        }
    }
}
