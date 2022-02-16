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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using DDSHeaders;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kopernicus.OnDemand
{
    // Class to store OnDemand stuff
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class OnDemandStorage
    {
        // Lists
        private static readonly Dictionary<String, List<ILoadOnDemand>> Maps =
            new Dictionary<String, List<ILoadOnDemand>>();

        // Whole file buffer management
        private static Byte[] _wholeFileBuffer;
        private static Int32 _sizeWholeFile;
        private static Int32 _arrayLengthOffset;

        // OnDemand flags
        public static Boolean UseOnDemand = true;
        public static Boolean UseOnDemandBiomes = false;
        public static Boolean OnDemandLoadOnMissing = true;
        public static Boolean OnDemandLogOnMissing = true;
        public static Int32 OnDemandUnloadDelay = 10;

        public static Boolean UseManualMemoryManagement = true;

        // Add a map to the map-list
        public static void AddMap(String body, ILoadOnDemand map)
        {
            // If the map is null, abort
            if (map == null)
            {
                return;
            }

            // Create the sublist
            if (!Maps.ContainsKey(body))
            {
                Maps[body] = new List<ILoadOnDemand>();
            }

            // Add the map
            if (!Maps[body].Contains(map))
            {
                Maps[body].Add(map);

                // Log
                Debug.Log("[OD] Adding for body " + body + " map named " + map.Name + " of path = " + map.Path);
            }
            else
            {
                Debug.Log("[OD] WARNING: trying to add a map but is already tracked! Current body is " + body +
                          " and map name is " + map.Name + " and path is " + map.Path);
            }
        }

        // Remove a map from the list
        public static void RemoveMap(String body, ILoadOnDemand map)
        {
            // If the map is null, abort
            if (map == null)
            {
                return;
            }

            // If the sublist exists, remove the map
            if (Maps.ContainsKey(body))
            {
                if (Maps[body].Contains(map))
                {
                    Maps[body].Remove(map);
                }
                else
                {
                    Debug.Log(
                        "[OD] WARNING: Trying to remove a map from a body, but the map is not tracked for the body!");
                }
            }
            else
            {
                Debug.Log("[OD] WARNING: Trying to remove a map from a body, but the body is not known!");
            }

            // If all maps of the body are unloaded, remove the body completely
            if (Maps[body].Count == 0)
            {
                Maps.Remove(body);
            }
        }

        // Enable a list of maps
        public static void EnableMapList(List<ILoadOnDemand> maps, List<ILoadOnDemand> exclude = null)
        {
            // If the excludes are null, create an empty list
            if (exclude == null)
            {
                exclude = new List<ILoadOnDemand>();
            }

            // Go through all maps
            for (Int32 i = maps.Count - 1; i >= 0; --i)
            {
                // If excluded...
                if (exclude.Contains(maps[i]))
                {
                    continue;
                }

                // Load the map
                maps[i].Load();
            }
        }

        // Disable a list of maps
        public static void DisableMapList(List<ILoadOnDemand> maps, List<ILoadOnDemand> exclude = null)
        {
            // If the excludes are null, create an empty list
            if (exclude == null)
            {
                exclude = new List<ILoadOnDemand>();
            }

            // Go through all maps
            for (Int32 i = maps.Count - 1; i >= 0; --i)
            {
                // If excluded...
                if (exclude.Contains(maps[i]))
                {
                    continue;
                }

                // Load the map
                maps[i].Unload();
            }
        }

        // Enable all maps of a body
        public static void EnableBody(String body)
        {
            if (!Maps.ContainsKey(body))
            {
                return;
            }

            Debug.Log("[OD] --> OnDemandStorage.EnableBody loading " + body);
            EnableMapList(Maps[body]);
        }

        // Unload all maps of a body
        public static void DisableBody(String body)
        {
            if (!Maps.ContainsKey(body))
            {
                return;
            }

            Debug.Log("[OD] <--- OnDemandStorage.DisableBody destroying " + body);
            DisableMapList(Maps[body]);
        }

        public static Boolean EnableBodyPqs(String body)
        {
            if (!Maps.ContainsKey(body))
            {
                return false;
            }

            Debug.Log("[OD] --> OnDemandStorage.EnableBodyPQS loading " + body);
            EnableMapList(Maps[body].Where(m => m is MapSODemand).ToList());
            return true;
        }

        public static Boolean DisableBodyPqs(String body)
        {
            if (!Maps.ContainsKey(body))
            {
                return false;
            }

            Debug.Log("[OD] <--- OnDemandStorage.DisableBodyPQS destroying " + body);
            DisableMapList(Maps[body].Where(m => m is MapSODemand).ToList());
            return true;
        }

        public static void EnableBodyBiomeMaps(String body)
        {
            if (!Maps.ContainsKey(body))
            {
                return;
            }

            Debug.Log("[OD] --> OnDemandStorage.EnableBodyCBMaps loading " + body);
            EnableMapList(Maps[body].Where(m => m is CBAttributeMapSODemand).ToList());
        }

        public static void DisableBodyBiomeMaps(String body)
        {
            if (!Maps.ContainsKey(body))
            {
                return;
            }

            Debug.Log("[OD] <--- OnDemandStorage.DisableBodyCBMaps destroying " + body);
            DisableMapList(Maps[body].Where(m => m is CBAttributeMapSODemand).ToList());
        }

        public static Byte[] LoadWholeFile(String path)
        {
            // If we haven't worked out if we can patch array length then do it
            if (_arrayLengthOffset == 0)
            {
                CalculateArrayLengthOffset();
            }

            // If we can't patch array length then just use the normal function
            if (_arrayLengthOffset == 1)
            {
                return File.ReadAllBytes(path);
            }

            // Otherwise we do cunning stuff
            using (FileStream file = File.OpenRead(path))
            {
                if (file.Length > Int32.MaxValue)
                {
                    throw new Exception("File too large");
                }

                Int32 fileBytes = (Int32) file.Length;

                if (_wholeFileBuffer == null || fileBytes > _sizeWholeFile)
                {
                    // Round it up to a 1MB multiple
                    _sizeWholeFile = (fileBytes + 0xFFFFF) & ~0xFFFFF;
                    Debug.Log("[Kopernicus] LoadWholeFile reallocating buffer to " + _sizeWholeFile);
                    _wholeFileBuffer = new Byte[_sizeWholeFile];
                }
                else
                {
                    // Reset the length of the array to the full size
                    FudgeByteArrayLength(_wholeFileBuffer, _sizeWholeFile);
                }

                // Read all the data from the file
                Int32 i = 0;
                while (fileBytes > 0)
                {
                    Int32 read = file.Read(_wholeFileBuffer, i, fileBytes > 0x100000 ? 0x100000 : fileBytes);
                    if (read <= 0)
                    {
                        continue;
                    }

                    i += read;
                    fileBytes -= read;
                }

                // Fudge the length of the array
                FudgeByteArrayLength(_wholeFileBuffer, i);

                return _wholeFileBuffer;
            }
        }

        public static Byte[] LoadRestOfReader(BinaryReader reader)
        {
            // If we haven't worked out if we can patch array length then do it
            if (_arrayLengthOffset == 0)
            {
                CalculateArrayLengthOffset();
            }

            Int64 chunkBytes = reader.BaseStream.Length - reader.BaseStream.Position;
            if (chunkBytes > Int32.MaxValue)
            {
                throw new Exception("Chunk too large");
            }

            // If we can't patch array length then just use the normal function
            if (_arrayLengthOffset == 1)
            {
                return reader.ReadBytes((Int32)chunkBytes);
            }

            // Otherwise we do cunning stuff
            Int32 fileBytes = (Int32) chunkBytes;
            if (_wholeFileBuffer == null || fileBytes > _sizeWholeFile)
            {
                // Round it up to a 1MB multiple
                _sizeWholeFile = (fileBytes + 0xFFFFF) & ~0xFFFFF;
                Debug.Log("[Kopernicus] LoadRestOfReader reallocating buffer to " + _sizeWholeFile);
                _wholeFileBuffer = new Byte[_sizeWholeFile];
            }
            else
            {
                // Reset the length of the array to the full size
                FudgeByteArrayLength(_wholeFileBuffer, _sizeWholeFile);
            }

            // Read all the data from the file
            Int32 i = 0;
            while (fileBytes > 0)
            {
                Int32 read = reader.Read(_wholeFileBuffer, i, fileBytes > 0x100000 ? 0x100000 : fileBytes);
                if (read <= 0)
                {
                    continue;
                }

                i += read;
                fileBytes -= read;
            }

            // Fudge the length of the array
            FudgeByteArrayLength(_wholeFileBuffer, i);

            return _wholeFileBuffer;
        }

        private static unsafe void CalculateArrayLengthOffset()
        {
            // Work out the offset by allocating a small array and searching backwards until we find the correct value
            Int32[] temp = new Int32[3];
            Int32 offset = -4;
            fixed (Int32* ptr = &temp[0])
            {
                Int32* p = ptr - 1;
                while (*p != 3 && offset > -44)
                {
                    offset -= 4;
                    p--;
                }

                _arrayLengthOffset = *p == 3 ? offset : 1;
                Debug.Log("[Kopernicus] CalculateArrayLengthOffset using offset of " + _arrayLengthOffset);
            }
        }

        private static unsafe void FudgeByteArrayLength(Byte[] array, Int32 len)
        {
            fixed (Byte* ptr = &array[0])
            {
                Int32* pLen = (Int32*) (ptr + _arrayLengthOffset);
                *pLen = len;
            }
        }

        // Loads a texture
        public static Texture2D LoadTexture(String path, Boolean compress, Boolean upload, Boolean unreadable)
        {
            Texture2D map = null;
            path = KSPUtil.ApplicationRootPath + "GameData/" + path;
            if (File.Exists(path))
            {
                Boolean uncaught = true;
                Boolean error = false;
                try
                {
                    if (path.ToLower().EndsWith(".dds"))
                    {
                        // Borrowed from stock KSP 1.0 DDS loader (hi Mike!)
                        // Also borrowed the extra bits from Sarbian.
                        using (BinaryReader binaryReader = new BinaryReader(File.OpenRead(path)))
                        {
                            UInt32 num = binaryReader.ReadUInt32();
                            if (num == DDSValues.uintMagic)
                            {

                                DDSHeader ddsHeader = new DDSHeader(binaryReader);

                                if (ddsHeader.ddspf.dwFourCC == DDSValues.uintDX10)
                                {
                                    // ReSharper disable once ObjectCreationAsStatement
                                    new DDSHeaderDX10(binaryReader);
                                }

                                Boolean alpha = (ddsHeader.ddspf.dwFlags & 0x00000002) != 0;
                                Boolean fourcc = (ddsHeader.ddspf.dwFlags & 0x00000004) != 0;
                                Boolean rgb = (ddsHeader.ddspf.dwFlags & 0x00000040) != 0;
                                Boolean alphapixel = (ddsHeader.ddspf.dwFlags & 0x00000001) != 0;
                                Boolean luminance = (ddsHeader.ddspf.dwFlags & 0x00020000) != 0;
                                Boolean rgb888 = ddsHeader.ddspf.dwRBitMask == 0x000000ff &&
                                             ddsHeader.ddspf.dwGBitMask == 0x0000ff00 &&
                                             ddsHeader.ddspf.dwBBitMask == 0x00ff0000;
                                Boolean rgb565 = ddsHeader.ddspf.dwRBitMask == 0x0000F800 &&
                                             ddsHeader.ddspf.dwGBitMask == 0x000007E0 &&
                                             ddsHeader.ddspf.dwBBitMask == 0x0000001F;
                                Boolean argb4444 = ddsHeader.ddspf.dwABitMask == 0x0000f000 &&
                                               ddsHeader.ddspf.dwRBitMask == 0x00000f00 &&
                                               ddsHeader.ddspf.dwGBitMask == 0x000000f0 &&
                                               ddsHeader.ddspf.dwBBitMask == 0x0000000f;
                                Boolean rbga4444 = ddsHeader.ddspf.dwABitMask == 0x0000000f &&
                                               ddsHeader.ddspf.dwRBitMask == 0x0000f000 &&
                                               ddsHeader.ddspf.dwGBitMask == 0x000000f0 &&
                                               ddsHeader.ddspf.dwBBitMask == 0x00000f00;

                                Boolean mipmap = (ddsHeader.dwCaps & DDSPixelFormatCaps.MIPMAP) !=
                                             0u;
                                if (fourcc)
                                {
                                    if (ddsHeader.ddspf.dwFourCC == DDSValues.uintDXT1)
                                    {
                                        map = new Texture2D((Int32)ddsHeader.dwWidth, (Int32)ddsHeader.dwHeight,
                                            TextureFormat.DXT1, mipmap);
                                        map.LoadRawTextureData(LoadRestOfReader(binaryReader));
                                    }
                                    else if (ddsHeader.ddspf.dwFourCC == DDSValues.uintDXT3)
                                    {
                                        map = new Texture2D((Int32)ddsHeader.dwWidth, (Int32)ddsHeader.dwHeight,
                                            (TextureFormat)11, mipmap);
                                        map.LoadRawTextureData(LoadRestOfReader(binaryReader));
                                    }
                                    else if (ddsHeader.ddspf.dwFourCC == DDSValues.uintDXT5)
                                    {
                                        map = new Texture2D((Int32)ddsHeader.dwWidth, (Int32)ddsHeader.dwHeight,
                                            TextureFormat.DXT5, mipmap);
                                        map.LoadRawTextureData(LoadRestOfReader(binaryReader));
                                    }
                                    else if (ddsHeader.ddspf.dwFourCC == DDSValues.uintDXT2)
                                    {
                                        Debug.Log("[Kopernicus]: DXT2 not supported" + path);
                                    }
                                    else if (ddsHeader.ddspf.dwFourCC == DDSValues.uintDXT4)
                                    {
                                        Debug.Log("[Kopernicus]: DXT4 not supported: " + path);
                                    }
                                    else if (ddsHeader.ddspf.dwFourCC == DDSValues.uintDX10)
                                    {
                                        Debug.Log("[Kopernicus]: DX10 dds not supported: " + path);
                                    }
                                    else if (ddsHeader.ddspf.dwFourCC == DDSValues.uintMagic)
                                    {
                                        Debug.Log("[Kopernicus]: Magic dds not supported: " + path);
                                    }
                                    else
                                    {
                                        fourcc = false;
                                    }
                                }

                                if (!fourcc)
                                {
                                    TextureFormat textureFormat = TextureFormat.ARGB32;
                                    Boolean ok = true;
                                    if (rgb && rgb888)
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
                                    else if (!rgb && alpha != luminance && (ddsHeader.ddspf.dwRGBBitCount == 8 || ddsHeader.ddspf.dwRGBBitCount == 16))
                                    {
                                        if (ddsHeader.ddspf.dwRGBBitCount == 8)
                                        {
                                            // A8 format or Luminance 8
                                            if (alpha)
                                                textureFormat = TextureFormat.Alpha8;
                                            else
                                                textureFormat = TextureFormat.R8;
                                        }
                                        else if (ddsHeader.ddspf.dwRGBBitCount == 16)
                                        {
                                            // R16 format
                                            textureFormat = TextureFormat.R16;
                                        }
                                    }
                                    else if (ddsHeader.ddspf.dwRGBBitCount == 4 || ddsHeader.ddspf.dwRGBBitCount == 8)
                                    {
                                        try
                                        {
                                            Int32 bpp = (Int32) ddsHeader.ddspf.dwRGBBitCount;
                                            Int32 colors = (Int32) Math.Pow(2, bpp);
                                            Int32 width = (Int32) ddsHeader.dwWidth;
                                            Int32 height = (Int32) ddsHeader.dwHeight;
                                            Int64 length = new FileInfo(path).Length;
                                            Int32 pixels = width * height * bpp / 8 + 4 * colors;

                                            if (length - 128 >= pixels)
                                            {
                                                Byte[] data = binaryReader.ReadBytes(pixels);

                                                Color32[] palette = new Color32[colors];
                                                Color32[] image = new Color32[width * height];

                                                for (Int32 i = 0; i < 4 * colors; i += 4)
                                                {
                                                    palette[i / 4] = new Color32(data[i + 0], data[i + 1], data[i + 2],
                                                        data[i + 3]);
                                                }

                                                for (Int32 i = 4 * colors; i < data.Length; i++)
                                                {
                                                    image[(i - 4 * colors) * 8 / bpp] = palette[data[i] * colors / 256];
                                                    if (bpp == 4)
                                                    {
                                                        image[(i - 64) * 2 + 1] = palette[data[i] % 16];
                                                    }
                                                }

                                                map = new Texture2D(width, height, TextureFormat.ARGB32, false);
                                                map.SetPixels32(image);

                                                // We loaded the texture manually
                                                ok = false;
                                            }
                                            else
                                            {
                                                error = true;
                                            }
                                        }
                                        catch
                                        {
                                            error = true;
                                        }
                                    }
                                    else
                                    {
                                        ok = false;
                                        error = true;
                                    }

                                    if (error)
                                    {
                                        Debug.Log(
                                            "[Kopernicus]: Only DXT1, DXT5, A8, R8, R16, RGB24, RGBA32, RGB565, ARGB4444, RGBA4444, 4bpp palette and 8bpp palette are supported");
                                    }

                                    if (ok)
                                    {
                                        map = new Texture2D((Int32)ddsHeader.dwWidth, (Int32)ddsHeader.dwHeight,
                                            textureFormat, mipmap);
                                        map.LoadRawTextureData(LoadRestOfReader(binaryReader));
                                    }

                                }
                            }
                            else
                            {
                                Debug.Log("[Kopernicus]: Bad DDS header.");
                            }
                        }
                    }
                    else
                    {
                        map = new Texture2D(2, 2);
                        Byte[] data = LoadWholeFile(path);
                        if (data == null)
                        {
                            throw new Exception("LoadWholeFile failed");
                        }

                        map.LoadImage(data);
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
                else
                {
                    map.name = path.Remove(0, (KSPUtil.ApplicationRootPath + "GameData/").Length);

                    if (compress)
                    {
                        map.Compress(true);
                    }

                    if (upload)
                    {
                        map.Apply(false, unreadable);
                    }
                }
            }
            else
            {
                Debug.Log("[Kopernicus]: texture does not exist! " + path);
            }

            return map;
        }

        // Checks if a Texture exists
        public static Boolean TextureExists(String path)
        {
            path = KSPUtil.ApplicationRootPath + "GameData/" + path;
            return File.Exists(path);
        }
    }
}
