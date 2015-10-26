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

using System.Collections;
using UnityEngine;

namespace Kopernicus
{
    namespace OnDemand
    {
        // Class to load ScaledSpace Textures on Demand
        public class ScaledSpaceDemand : MonoBehaviour
        {
            // Path to the Texture
            public string texture;

            // Path to the normal map
            public string normals;

            // ScaledSpace MeshRenderer
            public MeshRenderer scaledRenderer;

            // State of the texture
            public bool isLoaded = true;

            // Start(), get the scaled Mesh renderer
            void Start()
            {
                scaledRenderer = GetComponent<MeshRenderer>();
                OnBecameInvisible();
            }

            // OnBecameVisible(), load the texture
            void OnBecameVisible()
            {
                StartCoroutine(HandleScaledSpace(true));
            }

            // OnBecameInvisible(), kill the texture
            void OnBecameInvisible()
            {
                StartCoroutine(HandleScaledSpace(false));
            }

            // Use a Coroutine to load the maps, to make everything smoother
            IEnumerator HandleScaledSpace(bool mode)
            {
                // Load
                if (mode)
                {
                    // If it is already loaded, return
                    if (isLoaded)
                        yield break;

                    // Load Diffuse
                    if (TextureExists(texture))
                        StartCoroutine(LoadTexture(texture, true));

                    // Yield
                    yield return null;

                    // Load Normals
                    if (TextureExists(normals))
                        StartCoroutine(LoadTexture(normals, false));

                    // Flags
                    isLoaded = true;

                    // Break
                    yield break;
                }

                // Unload
                else
                {
                    // If it is already loaded, return
                    if (!isLoaded)
                        yield break;

                    // Kill Diffuse
                    if (TextureExists(texture))
                        DestroyImmediate(scaledRenderer.material.GetTexture("_MainTex"));

                    // Yield
                    yield return null;

                    // Kill Normals
                    if (TextureExists(normals))
                        DestroyImmediate(scaledRenderer.material.GetTexture("_BumpMap"));

                    // Flags
                    isLoaded = false;

                    // Stop
                    yield break;
                }
            }

            // Checks if a Texture exists
            public static bool TextureExists(string path)
            {
                path = KSPUtil.ApplicationRootPath + "GameData/" + path;
                return System.IO.File.Exists(path);
            }

            // Loads a texture in a Coroutine
            IEnumerator LoadTexture(string path, bool diffuse)
            {
                Texture2D map = null;
                path = KSPUtil.ApplicationRootPath + "GameData/" + path;
                if (System.IO.File.Exists(path))
                {
                    if (path.ToLower().EndsWith(".dds"))
                    {
                        // Borrowed from stock KSP 1.0 DDS loader (hi Mike!)
                        // Also borrowed the extra bits from Sarbian.
                        byte[] buffer = System.IO.File.ReadAllBytes(path);
                        System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(new System.IO.MemoryStream(buffer));
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
                                    map.LoadRawTextureData(binaryReader.ReadBytes((int)(binaryReader.BaseStream.Length - binaryReader.BaseStream.Position)));
                                }
                                else if (dDSHeader.ddspf.dwFourCC == DDSHeaders.DDSValues.uintDXT3)
                                {
                                    map = new Texture2D((int)dDSHeader.dwWidth, (int)dDSHeader.dwHeight, (TextureFormat)11, mipmap);
                                    map.LoadRawTextureData(binaryReader.ReadBytes((int)(binaryReader.BaseStream.Length - binaryReader.BaseStream.Position)));
                                }
                                else if (dDSHeader.ddspf.dwFourCC == DDSHeaders.DDSValues.uintDXT5)
                                {
                                    map = new Texture2D((int)dDSHeader.dwWidth, (int)dDSHeader.dwHeight, TextureFormat.DXT5, mipmap);
                                    map.LoadRawTextureData(binaryReader.ReadBytes((int)(binaryReader.BaseStream.Length - binaryReader.BaseStream.Position)));
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
                                if (fourcc)
                                    yield return null;
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
                                    map.LoadRawTextureData(binaryReader.ReadBytes((int)(binaryReader.BaseStream.Length - binaryReader.BaseStream.Position)));
                                    yield return null;
                                }

                            }
                            if (map != null)
                                map.Apply(false, true);
                            yield return null;
                        }
                        else
                            Debug.Log("[Kopernicus]: Bad DDS header.");
                    }
                    else
                    {
                        map = new Texture2D(2, 2);
                        map.LoadImage(System.IO.File.ReadAllBytes(path));
                        yield return null;
                        map.Apply(false, true);
                        yield return null;
                    }
                    map.name = path.Remove(0, (KSPUtil.ApplicationRootPath + "GameData/").Length);
                }            
                else
                    Debug.Log("[Kopernicus]: texture does not exist! " + path);

                // Apply the map
                if (diffuse)
                    scaledRenderer.material.SetTexture("_MainTex", map);
                else
                    scaledRenderer.material.SetTexture("_BumpMap", map);
                yield break;
            }
        }
    }
}