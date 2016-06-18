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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Threading;
using System.IO;
using System.ComponentModel;

namespace Kopernicus
{
    namespace OnDemand
    {
        // Class to store OnDemand stuff
        public static class OnDemandStorage
        {
            // Lists
            public static Dictionary<string, List<ILoadOnDemand>> maps = new Dictionary<string, List<ILoadOnDemand>>();
            public static Dictionary<PQS, PQSMod_OnDemandHandler> handlers = new Dictionary<PQS, PQSMod_OnDemandHandler>();
            public static string currentBody = "";

            // Whole file buffer management
            private static byte[] wholeFileBuffer = null;
            private static int sizeWholeFile = 0;
            private static int arrayLengthOffset = 0;

            // OnDemand flags
            public static bool useOnDemand = true;
            public static bool useOnDemandBiomes = true;
            public static bool onDemandLoadOnMissing = true;
            public static bool onDemandLogOnMissing = true;
            public static int onDemandUnloadDelay = 10;

            // Add the management handler to the PQS
            public static void AddHandler(PQS pqsVersion)
            {
                PQSMod_OnDemandHandler handler = new GameObject("OnDemandHandler").AddComponent<PQSMod_OnDemandHandler>();
                handler.transform.parent = pqsVersion.transform;
                UnityEngine.Object.DontDestroyOnLoad(handler);
                handler.sphere = pqsVersion;
                handler.order = 1;
                handlers[pqsVersion] = handler;
            }

            // Add a map to the map-list
            public static void AddMap(string body, ILoadOnDemand map)
            {
                // If the map is null, abort
                if (map == null)
                    return;

                // Create the sublist
                if (!maps.ContainsKey(body)) maps[body] = new List<ILoadOnDemand>();

                // Add the map
                if (!maps[body].Contains(map))
                {
                    maps[body].Add(map);

                    // Log
                    Debug.Log("[OD] Adding for body " + body + " map named " + map.name + " of path = " + map.Path);
                }
                else
                {
                    Debug.Log("[OD] WARNING: trying to add a map but is already tracked! Current body is " + body + " and map name is " + map.name + " and path is " + map.Path);
                }
            }

            // Remove a map from the list
            public static void RemoveMap(string body, ILoadOnDemand map)
            {
                // If the map is null, abort
                if (map == null)
                    return;

                // If the sublist exists, remove the map
                if (maps.ContainsKey(body))
                {
                    if (maps[body].Contains(map))
                    {
                        maps[body].Remove(map);
                    }
                    else
                    {
                        Debug.Log("[OD] WARNING: Trying to remove a map from a body, but the map is not tracked for the body!");
                    }
                }
                else
                {
                    Debug.Log("[OD] WARNING: Trying to remove a map from a body, but the body is not known!");
                }

                // If all maps of the body are unloaded, remove the body completely
                if (maps[body].Count == 0)
                    maps.Remove(body);
            }

            // Enable a list of maps
            public static void EnableMapList(List<ILoadOnDemand> maps, List<ILoadOnDemand> exclude = null)
            {
                // If the excludes are null, create an empty list
                if (exclude == null)
                    exclude = new List<ILoadOnDemand>();

                // Go through all maps
                for (int i = maps.Count - 1; i >= 0; --i)
                {
                    // If excluded...
                    if (exclude.Contains(maps[i])) continue;

                    // Load the map
                    maps[i].Load();
                }
            }

            // Disable a list of maps
            public static void DisableMapList(List<ILoadOnDemand> maps, List<ILoadOnDemand> exclude = null)
            {
                // If the excludes are null, create an empty list
                if (exclude == null)
                    exclude = new List<ILoadOnDemand>();

                // Go through all maps
                for (int i = maps.Count - 1; i >= 0; --i)
                {
                    // If excluded...
                    if (exclude.Contains(maps[i])) continue;

                    // Load the map
                    maps[i].Unload();
                }
            }

            // Enable all maps of a body
            public static bool EnableBody(string body)
            {
                if (maps.ContainsKey(body))
                {
                    EnableMapList(maps[body]);
                    return true;
                }
                return false;
                
            }

            // Unload all maps of a body
            public static bool DisableBody(string body)
            {
                if (maps.ContainsKey(body))
                {
                    DisableMapList(maps[body]);
                    return true;
                }
                return false;
            }

            public static byte[] LoadWholeFile(string path)
            {
                // If we haven't worked out if we can patch array length then do it
                if (arrayLengthOffset == 0)
                    CalculateArrayLengthOffset();

                // If we can't patch array length then just use the normal function
                if (arrayLengthOffset == 1)
                    return File.ReadAllBytes(path);

                // Otherwise we do cunning stuff
                FileStream file = File.OpenRead(path);
                if (file.Length > int.MaxValue)
                    throw new Exception("File too large");

                int fileBytes = (int)file.Length;

                if (wholeFileBuffer == null || fileBytes > sizeWholeFile)
                {
                    // Round it up to a 1MB multiple
                    sizeWholeFile = (fileBytes + 0xFFFFF) & ~0xFFFFF;
                    Debug.Log("[OD] LoadWholeFile reallocating buffer to " + sizeWholeFile);
                    wholeFileBuffer = new byte[sizeWholeFile];
                }
                else
                {
                    // Reset the length of the array to the full size
                    FudgeByteArrayLength(wholeFileBuffer, sizeWholeFile);
                }

                // Read all the data from the file
                int i = 0;
                while (fileBytes > 0)
                {
                    int read = file.Read(wholeFileBuffer, i, (fileBytes > 0x100000) ? 0x100000 : fileBytes);
                    if (read > 0)
                    {
                        i += read;
                        fileBytes -= read;
                    }
                }

                // Fudge the length of the array
                FudgeByteArrayLength(wholeFileBuffer, i);

                return wholeFileBuffer;
            }

            public static byte[] LoadRestOfReader(BinaryReader reader)
            {
                // If we haven't worked out if we can patch array length then do it
                if (arrayLengthOffset == 0)
                    CalculateArrayLengthOffset();

                long chunkBytes = reader.BaseStream.Length - reader.BaseStream.Position;
                if (chunkBytes > int.MaxValue)
                    throw new Exception("Chunk too large");

                // If we can't patch array length then just use the normal function
                if (arrayLengthOffset == 1)
                    return reader.ReadBytes((int)chunkBytes);

                // Otherwise we do cunning stuff
                int fileBytes = (int)chunkBytes;
                if (wholeFileBuffer == null || fileBytes > sizeWholeFile)
                {
                    // Round it up to a 1MB multiple
                    sizeWholeFile = (fileBytes + 0xFFFFF) & ~0xFFFFF;
                    Debug.Log("[OD] LoadRestOfReader reallocating buffer to " + sizeWholeFile);
                    wholeFileBuffer = new byte[sizeWholeFile];
                }
                else
                {
                    // Reset the length of the array to the full size
                    FudgeByteArrayLength(wholeFileBuffer, sizeWholeFile);
                }

                // Read all the data from the file
                int i = 0;
                while (fileBytes > 0)
                {
                    int read = reader.Read(wholeFileBuffer, i, (fileBytes > 0x100000) ? 0x100000 : fileBytes);
                    if (read > 0)
                    {
                        i += read;
                        fileBytes -= read;
                    }
                }

                // Fudge the length of the array
                FudgeByteArrayLength(wholeFileBuffer, i);

                return wholeFileBuffer;
            }

            unsafe static void CalculateArrayLengthOffset()
            {
                // Work out the offset by allocating a small array and searching backwards until we find the correct value
                int[] temp = new int[3];
                int offset = -4;
                fixed (int* ptr = &temp[0])
                {
                    int* p = ptr - 1;
                    while (*p != 3 && offset > -44)
                    {
                        offset -= 4;
                        p--;
                    }

                    arrayLengthOffset = (*p == 3) ? offset : 1;
                    Debug.Log("[OD] CalculateArrayLengthOffset using offset of " + arrayLengthOffset);
                }
            }

            unsafe static void FudgeByteArrayLength(byte[] array, int len)
            {
                fixed (byte* ptr = &array[0])
                {
                    int* pLen = (int*)(ptr + arrayLengthOffset);
                    *pLen = len;
                }
            }

            // Loads a texture
            public static Texture2D LoadTexture(string path, bool compress, bool upload, bool unreadable)
            {
                Texture2D map = null;
                path = Directory.GetCurrentDirectory() + "/GameData/" + path;
                if (File.Exists(path))
                {
                    bool uncaught = true;
                    try
                    {
                        if (path.ToLower().EndsWith(".dds"))
                        {
                            // Borrowed from stock KSP 1.0 DDS loader (hi Mike!)
                            // Also borrowed the extra bits from Sarbian.
                            BinaryReader binaryReader = new BinaryReader(File.OpenRead(path));
                            uint num = binaryReader.ReadUInt32();
                            if (num == DDSHeaders.DDSValues.uintMagic)
                            {

                                DDSHeaders.DDSHeader dDSHeader = new DDSHeaders.DDSHeader(binaryReader);

                                if (dDSHeader.ddspf.dwFourCC == DDSHeaders.DDSValues.uintDX10)
                                {
                                    new DDSHeaders.DDSHeaderDX10(binaryReader);
                                }

                                bool alpha = (dDSHeader.dwFlags & 0x00000002) != 0;
                                bool fourcc = (dDSHeader.dwFlags & 0x00000004) != 0;
                                bool rgb = (dDSHeader.dwFlags & 0x00000040) != 0;
                                bool alphapixel = (dDSHeader.dwFlags & 0x00000001) != 0;
                                bool luminance = (dDSHeader.dwFlags & 0x00020000) != 0;
                                bool rgb888 = dDSHeader.ddspf.dwRBitMask == 0x000000ff && dDSHeader.ddspf.dwGBitMask == 0x0000ff00 && dDSHeader.ddspf.dwBBitMask == 0x00ff0000;
                                //bool bgr888 = dDSHeader.ddspf.dwRBitMask == 0x00ff0000 && dDSHeader.ddspf.dwGBitMask == 0x0000ff00 && dDSHeader.ddspf.dwBBitMask == 0x000000ff;
                                bool rgb565 = dDSHeader.ddspf.dwRBitMask == 0x0000F800 && dDSHeader.ddspf.dwGBitMask == 0x000007E0 && dDSHeader.ddspf.dwBBitMask == 0x0000001F;
                                bool argb4444 = dDSHeader.ddspf.dwABitMask == 0x0000f000 && dDSHeader.ddspf.dwRBitMask == 0x00000f00 && dDSHeader.ddspf.dwGBitMask == 0x000000f0 && dDSHeader.ddspf.dwBBitMask == 0x0000000f;
                                bool rbga4444 = dDSHeader.ddspf.dwABitMask == 0x0000000f && dDSHeader.ddspf.dwRBitMask == 0x0000f000 && dDSHeader.ddspf.dwGBitMask == 0x000000f0 && dDSHeader.ddspf.dwBBitMask == 0x00000f00;

                                bool mipmap = (dDSHeader.dwCaps & DDSHeaders.DDSPixelFormatCaps.MIPMAP) != (DDSHeaders.DDSPixelFormatCaps)0u;
                                bool isNormalMap = ((dDSHeader.ddspf.dwFlags & 524288u) != 0u || (dDSHeader.ddspf.dwFlags & 2147483648u) != 0u);
                                if (fourcc)
                                {
                                    if (dDSHeader.ddspf.dwFourCC == DDSHeaders.DDSValues.uintDXT1)
                                    {
                                        map = new Texture2D((int)dDSHeader.dwWidth, (int)dDSHeader.dwHeight, TextureFormat.DXT1, mipmap);
                                        map.LoadRawTextureData(LoadRestOfReader(binaryReader));
                                    }
                                    else if (dDSHeader.ddspf.dwFourCC == DDSHeaders.DDSValues.uintDXT3)
                                    {
                                        map = new Texture2D((int)dDSHeader.dwWidth, (int)dDSHeader.dwHeight, (TextureFormat)11, mipmap);
                                        map.LoadRawTextureData(LoadRestOfReader(binaryReader));
                                    }
                                    else if (dDSHeader.ddspf.dwFourCC == DDSHeaders.DDSValues.uintDXT5)
                                    {
                                        map = new Texture2D((int)dDSHeader.dwWidth, (int)dDSHeader.dwHeight, TextureFormat.DXT5, mipmap);
                                        map.LoadRawTextureData(LoadRestOfReader(binaryReader));
                                    }
                                    else if (dDSHeader.ddspf.dwFourCC == DDSHeaders.DDSValues.uintDXT2)
                                    {
                                        Debug.Log("[Kopernicus]: DXT2 not supported" + path);
                                    }
                                    else if (dDSHeader.ddspf.dwFourCC == DDSHeaders.DDSValues.uintDXT4)
                                    {
                                        Debug.Log("[Kopernicus]: DXT4 not supported: " + path);
                                    }
                                    else if (dDSHeader.ddspf.dwFourCC == DDSHeaders.DDSValues.uintDX10)
                                    {
                                        Debug.Log("[Kopernicus]: DX10 dds not supported: " + path);
                                    }
                                    else
                                        fourcc = false;
                                }
                                if (!fourcc)
                                {
                                    TextureFormat textureFormat = TextureFormat.ARGB32;
                                    bool ok = true;
                                    if (rgb && (rgb888 /*|| bgr888*/))
                                    {
                                        // RGB or RGBA format
                                        textureFormat = alphapixel
                                        ? TextureFormat.RGBA32
                                        : TextureFormat.RGB24;
                                    }
                                    else if (rgb && rgb565)
                                    {
                                        // Nvidia texconv B5G6R5_UNORM
                                        textureFormat = TextureFormat.RGB565;
                                    }
                                    else if (rgb && alphapixel && argb4444)
                                    {
                                        // Nvidia texconv B4G4R4A4_UNORM
                                        textureFormat = TextureFormat.ARGB4444;
                                    }
                                    else if (rgb && alphapixel && rbga4444)
                                    {
                                        textureFormat = TextureFormat.RGBA4444;
                                    }
                                    else if (!rgb && alpha != luminance)
                                    {
                                        // A8 format or Luminance 8
                                        textureFormat = TextureFormat.Alpha8;
                                    }
                                    else
                                    {
                                        ok = false;
                                        Debug.Log("[Kopernicus]: Only DXT1, DXT5, A8, RGB24, RGBA32, RGB565, ARGB4444 and RGBA4444 are supported");
                                    }
                                    if (ok)
                                    {
                                        map = new Texture2D((int)dDSHeader.dwWidth, (int)dDSHeader.dwHeight, textureFormat, mipmap);
                                        map.LoadRawTextureData(LoadRestOfReader(binaryReader));
                                    }

                                }
                                if (map != null)
                                    if (upload)
                                        map.Apply(false, unreadable);
                            }
                            else
                                Debug.Log("[Kopernicus]: Bad DDS header.");
                        }
                        else
                        {
                            map = new Texture2D(2, 2);
                            byte[] data = LoadWholeFile(path);
                            if (data == null)
                                throw new Exception("LoadWholeFile failed");

                            map.LoadImage(data);
                            if (compress)
                                map.Compress(true);
                            if (upload)
                                map.Apply(false, unreadable);
                        }
                    }
                    catch (Exception ex)
                    {
                        uncaught = false;
                        Debug.Log("[Kopernicus]: failed to load " + path + " with exception " + ex.Message);
                    }
                    if (map == null && uncaught)
                    {
                        Debug.Log("[Kopernicus]: failed to load " + path);
                    }
                    map.name = path.Remove(0, (KSPUtil.ApplicationRootPath + "GameData/").Length);
                }
                else
                    Debug.Log("[Kopernicus]: texture does not exist! " + path);

                return map;
            }

            // Checks if a Texture exists
            public static bool TextureExists(string path)
            {
                path = Directory.GetCurrentDirectory() + "/GameData/" + path;
                return File.Exists(path);
            }
        }
    }
}
